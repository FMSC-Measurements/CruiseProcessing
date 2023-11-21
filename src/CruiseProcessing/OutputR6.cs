using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CruiseProcessing
{
    public class OutputR6 : CreateTextFile
    {
        public string currentReport;
        private int[] fieldLengths;
        private List<string> prtFields = new List<string>();
        private List<RegionalReports> listToOutput = new List<RegionalReports>();
        private List<RegionalReports> unitSubtotal = new List<RegionalReports>();
        private List<ReportSubtotal> totalToOutput = new List<ReportSubtotal>();
        private regionalReportHeaders rRH = new regionalReportHeaders();
        private double currGRS = 0;
        private double currNET = 0;
        private double convFactor = 100.0;
        private double currAC = 0;
        private string currUOM;
        private string[] completeHeader;
        private string volumeType = "***** CCF *****";
        private int tableNumber = 1;

        public OutputR6(CPbusinessLayer businessLayer) : base(businessLayer)
        {
        }

        public void CreateR6reports(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  fill report title array
            string currentTitle = fillReportTitle(currentReport);

            //  process by report
            switch (currentReport)
            {
                case "R602":
                    //  June 2017 -- this report uses logging method so if it is blank or null
                    //  cannot generate the report
                    List<CuttingUnitDO> cutList = DataLayer.getCuttingUnits();
                    int noMethod = 0;
                    foreach (CuttingUnitDO ct in cutList)
                    {
                        if (ct.LoggingMethod == "" || ct.LoggingMethod == " " || ct.LoggingMethod == null)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> One or more logging methods are missing.  Cannot generate report.");
                            noMethod = -1;
                        }  //  endif
                    }  //  end foreach loop
                    if (noMethod != -1)
                    {
                        //  payment unit report
                        numOlines = 0;
                        fieldLengths = new int[] { 1, 6, 6, 12, 8, 10, 8, 6, 21, 10 };
                        rh.createReportTitle(currentTitle, 7, 41, 0, reportConstants.FCTO, "");
                        List<CuttingUnitDO> justPaymentUnits = DataLayer.getPaymentUnits();
                        // because of the way this report works, each group is written to the output file
                        // in the following function
                        AccumulatePaymentUnits(justPaymentUnits, strWriteOut, ref pageNumb, rh);
                    }  //  endif noMethod
                    break;

                case "R604":
                case "R605":
                    //  need species list as each table will be a species
                    List<LCDDO> speciesList = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 ", "GROUP BY Species", "C", "");
                    //  and log records
                    List<LogStockDO> logList = DataLayer.getCutLogs();

                    fieldLengths = new int[] { 1, 7, 13, 10, 13, 12, 10, 13, 10, 16, 14, 9 };
                    if (currentReport == "R605")
                    {
                        convFactor = 1000.0;
                        volumeType = "***** MBF *****";
                    }  //  endif currentReport
                    rh.createReportTitle(currentTitle, 6, 0, 0,
                            "LOG STOCK TABLE  FOR CUT TREES ONLY", volumeType);
                    //  loop through species list to calculate table values
                    foreach (LCDDO sl in speciesList)
                    {
                        numOlines = 0;
                        //  pull species from log list
                        List<LogStockDO> justSpecies = logList.FindAll(
                            delegate (LogStockDO js)
                            {
                                return js.Tree.Species == sl.Species;
                            });
                        //  are there log records for the current species?
                        if (justSpecies.Count > 0)
                        {
                            //  calculate average DBH for header
                            double averageDBH = CalculateAverageDBH(sl.Species, sl.CutLeave);
                            completeHeader = createCompleteHeader(sl.Species, averageDBH);
                            //  fill listToOutput with dib classes for this species
                            int maxDIB = (int)justSpecies.Max(j => j.SmallEndDiameter + 0.5);
                            fillOneInchClass(maxDIB);
                            //  accumulate table values
                            AccumulateTableValues(justSpecies);
                            //  write current table
                            WriteCurrentTable(strWriteOut, ref pageNumb, rh);
                            strWriteOut.WriteLine(reportConstants.longLine);
                            //  print total line
                            PrintTotal(strWriteOut, ref pageNumb, rh);
                            //  output footer
                            OutputFooter(strWriteOut, ref pageNumb, rh);
                            //  clear lists for next species and increment table number
                            listToOutput.Clear();
                            totalToOutput.Clear();
                            tableNumber++;
                        }  //  endif log records exist
                    }  //  end foreach loop
                    break;
            }  //  end switch
            return;
        }  //  end CreateR6reports

        private void AccumulatePaymentUnits(List<CuttingUnitDO> justPaymentUnits, StreamWriter strWriteOut,
                                               ref int pageNumb, reportHeaders rh)
        {
            //  R602
            double currGRS01 = 0;
            double currNET01 = 0;
            double currGRS08 = 0;
            double currNET08 = 0;
            double totalGRS01 = 0;
            double totalNET01 = 0;
            double totalGRS08 = 0;
            double totalNET08 = 0;
            double payUnitAcres = 0;

            List<CuttingUnitDO> cutList = DataLayer.getCuttingUnits();
            List<PRODO> proList = DataLayer.getPRO();
            List<LCDDO> lcdList = DataLayer.getLCD();
            List<LogStockDO> allLogs = DataLayer.getCutLogs();
            foreach (CuttingUnitDO jpu in justPaymentUnits)
            {
                //  find all cutting units for the current payment unit
                List<CuttingUnitDO> justUnits = cutList.FindAll(
                    delegate (CuttingUnitDO c)
                    {
                        return c.PaymentUnit == jpu.PaymentUnit;
                    });
                payUnitAcres = justUnits.Sum(j => j.Area);
                double proratFac = 0.0;
                string prevUOM = "*";
                //  then need strata volume to prorate
                foreach (CuttingUnitDO ju in justUnits)
                {
                    ju.Strata.Populate();
                    foreach (StratumDO stratum in ju.Strata)
                    {
                        //  need species for the strata
                        List<LCDDO> justSpecies = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2 ",
                                                   "ORDER BY Species", "C", stratum.Code);
                        //  process by species
                        string prevSP = "*";
                        string prevPP = "*";
                        string prevSProd = "*";
                        totalGRS01 = 0;
                        totalNET01 = 0;
                        totalGRS08 = 0;
                        totalNET08 = 0;
                        //  July 2014 -- need to know the cruise  method to get correct logs/trees
                        string currMeth = stratum.Method;
                        foreach (LCDDO js in justSpecies)
                        {
                            //  Store accumulated values for current species as needed
                            if (prevSP == "*")
                            {
                                prevSP = js.Species;
                                prevPP = js.PrimaryProduct;
                                prevSProd = js.SecondaryProduct;
                                prevUOM = js.UOM;
                            }
                            else if (prevSP != js.Species || (prevSP == js.Species && prevPP != js.PrimaryProduct))
                            {
                                if (totalNET01 > 0.0)
                                {
                                    RegionalReports r01 = new RegionalReports();
                                    r01.value1 = ju.PaymentUnit;
                                    r01.value2 = ju.Code;
                                    r01.value10 = ju.Area;
                                    r01.value4 = ju.LoggingMethod;
                                    r01.value5 = prevSP;
                                    //  load primary product
                                    r01.value6 = prevPP;
                                    r01.value7 = totalGRS01;
                                    r01.value8 = totalNET01;
                                    listToOutput.Add(r01);
                                    UpdateSubtotal(r01.value1, r01.value10, r01.value6, r01.value5,
                                                   js.UOM, r01.value7, r01.value8);
                                }  //  endif

                                //  load other product
                                if (totalNET08 > 0)
                                {
                                    RegionalReports r08 = new RegionalReports();
                                    r08.value6 = prevSProd;
                                    r08.value1 = ju.PaymentUnit;
                                    r08.value2 = ju.Code;
                                    r08.value10 = ju.Area;
                                    r08.value4 = ju.LoggingMethod;
                                    r08.value5 = prevSP;
                                    r08.value7 = totalGRS08;
                                    r08.value8 = totalNET08;
                                    listToOutput.Add(r08);
                                    UpdateSubtotal(r08.value1, r08.value10, r08.value6, r08.value5,
                                                           js.UOM, r08.value7, r08.value8);
                                }
                                else if (totalNET08 == 0)
                                {
                                    //  need to update unit acres in subtotal
                                    UpdateSubtotal(ju.PaymentUnit, ju.Area, js.SecondaryProduct, js.Species,
                                                       js.UOM, 0.0, 0.0);
                                }  //  endif
                                totalGRS01 = 0;
                                totalNET01 = 0;
                                totalGRS08 = 0;
                                totalNET08 = 0;
                                currGRS01 = 0;
                                currNET01 = 0;
                                currGRS08 = 0;
                                currNET08 = 0;
                                currUOM = js.UOM;
                                prevSP = js.Species;
                                prevPP = js.PrimaryProduct;
                                prevSProd = js.SecondaryProduct;
                                prevUOM = js.UOM;
                            }  //  endif no species match

                            //  find proration factor for current group
                            int nthRow = proList.FindIndex(
                                delegate (PRODO p)
                                {
                                    return p.Stratum == js.Stratum && p.CuttingUnit == ju.Code &&
                                        p.SampleGroup == js.SampleGroup && p.CutLeave == "C" &&
                                        p.PrimaryProduct == js.PrimaryProduct && p.STM == js.STM;
                                });
                            if (nthRow >= 0)
                                proratFac = proList[nthRow].ProrationFactor;
                            else if (nthRow < 0)
                                proratFac = 0.0;

                            //  pull logs and sum
                            List<LogStockDO> justLogs = new List<LogStockDO>();
                            if (currMeth == "100" || js.STM == "Y")
                            {
                                // need logs from the cutting unit only and not the entire stratum as they aren't truly prorated
                                justLogs = allLogs.FindAll(
                                    delegate (LogStockDO ls)
                                    {
                                        return ls.Tree.Stratum.Code == js.Stratum && ls.Tree.CuttingUnit.Code == ju.Code &&
                                            ls.Tree.Species == js.Species && ls.Tree.SampleGroup.Code == js.SampleGroup &&
                                            ls.Tree.SampleGroup.PrimaryProduct == js.PrimaryProduct &&
                                            ls.Tree.STM == js.STM;
                                    });
                            }
                            else
                            {
                                justLogs = allLogs.FindAll(
                                   delegate (LogStockDO ls)
                                    {
                                        return ls.Tree.Stratum.Code == js.Stratum && ls.Tree.Species == js.Species &&
                                               ls.Tree.SampleGroup.Code == js.SampleGroup &&
                                               ls.Tree.SampleGroup.PrimaryProduct == js.PrimaryProduct &&
                                               ls.Tree.STM == js.STM;
                                    });
                            }  //  endif on method
                               //  sum by log grade
                            foreach (LogStockDO jl in justLogs)
                            {
                                jl.Grade = jl.Grade.TrimEnd(' ');
                                switch (jl.Grade)
                                {
                                    case "1":
                                    case "2":
                                    case "3":
                                    case "4":
                                    case "5":
                                    case "6":
                                        currGRS01 += jl.GrossCubicFoot * jl.Tree.ExpansionFactor;
                                        currNET01 += jl.NetCubicFoot * jl.Tree.ExpansionFactor;
                                        break;

                                    case "7":
                                    case "8":
                                        currGRS08 += jl.GrossCubicFoot * jl.Tree.ExpansionFactor;
                                        currNET08 += jl.NetCubicFoot * jl.Tree.ExpansionFactor;
                                        break;
                                }  //  end switch on log grade
                            }  //  end foreach loop
                               //  prorate volume before loading
                            totalGRS01 += currGRS01 * proratFac;
                            totalNET01 += currNET01 * proratFac;
                            totalGRS08 += currGRS08 * proratFac;
                            totalNET08 += currNET08 * proratFac;
                            currGRS01 = 0;
                            currNET01 = 0;
                            currGRS08 = 0;
                            currNET08 = 0;
                        }  //  end foreach on species
                           //  load last group
                        if (totalNET01 > 0.0)
                        {
                            RegionalReports r01 = new RegionalReports();
                            r01.value1 = ju.PaymentUnit;
                            r01.value2 = ju.Code;
                            r01.value10 = ju.Area;
                            r01.value4 = ju.LoggingMethod;
                            r01.value5 = prevSP;
                            //  load primary product
                            r01.value6 = prevPP;
                            r01.value7 = totalGRS01;
                            r01.value8 = totalNET01;
                            listToOutput.Add(r01);
                            UpdateSubtotal(r01.value1, r01.value10, r01.value6, r01.value5,
                                           prevUOM, r01.value7, r01.value8);
                        }  //  endif

                        //  load other product
                        if (totalNET08 > 0)
                        {
                            RegionalReports r08 = new RegionalReports();
                            if (prevPP == "01")
                                r08.value6 = prevSProd;
                            else
                                r08.value6 = prevPP;
                            r08.value1 = ju.PaymentUnit;
                            r08.value2 = ju.Code;
                            r08.value10 = ju.Area;
                            r08.value4 = ju.LoggingMethod;
                            r08.value5 = prevSP;
                            r08.value7 = totalGRS08;
                            r08.value8 = totalNET08;
                            listToOutput.Add(r08);
                            UpdateSubtotal(r08.value1, r08.value10, r08.value6, r08.value5,
                                                   prevUOM, r08.value7, r08.value8);
                        }
                        else if (totalNET08 == 0)
                        {
                            //  need to update unit acres in subtotal
                            UpdateSubtotal(ju.PaymentUnit, ju.Area, prevSProd, prevSP,
                                               prevUOM, 0.0, 0.0);
                        }  //  endif
                    }  //  end for loop on strata
                }  //  end foreach loop on just units
                   //  output current payment unit
                WriteCurrentUnit(strWriteOut, ref pageNumb, rh, prevUOM);
                //  add payment unit acres to unit subtotal
                foreach (RegionalReports us in unitSubtotal)
                    us.value10 = payUnitAcres;
                OutputUnitSubtotal(strWriteOut, ref pageNumb, rh);
                listToOutput.Clear();
                UpdateOverallTotal();
                unitSubtotal.Clear();
                payUnitAcres = 0;
            }  //  end foreach loop on just payment units
            outputOverallTotal(strWriteOut, ref pageNumb, rh);
            return;
        }  //  end AccumulatePaymentUnits

        private void AccumulateTableValues(List<LogStockDO> justSpecies)
        {
            // R604/R605
            string currST = "**";
            //  loop through log list to get volumes
            foreach (LogStockDO js in justSpecies)
            {
                //  get strata acres if change in strata
                if (currST != js.Tree.Stratum.Code)
                {
                    currAC = Utilities.ReturnCorrectAcres(js.Tree.Stratum.Code, DataLayer, (long)js.Tree.Stratum_CN);
                    currST = js.Tree.Stratum.Code;
                }  //  endif on stratum

                //  Accumulate volume based on current report
                switch (currentReport)
                {
                    case "R604":
                        currGRS = js.GrossCubicFoot;
                        currNET = js.NetCubicFoot;
                        break;

                    case "R605":
                        currGRS = js.GrossBoardFoot;
                        currNET = js.NetBoardFoot;
                        break;
                }  //  end switch

                //  find appropriate row for loading into listToOutput
                int nthRow = findTableRow(js.SmallEndDiameter);
                if (nthRow < 0) nthRow = 0;
                switch (js.Grade)
                {
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                        listToOutput[nthRow].value7 += currGRS * js.Tree.ExpansionFactor * currAC;
                        listToOutput[nthRow].value8 += currNET * js.Tree.ExpansionFactor * currAC;
                        //  totals
                        listToOutput[nthRow].value14 += currGRS * js.Tree.ExpansionFactor * currAC;
                        listToOutput[nthRow].value15 += currNET * js.Tree.ExpansionFactor * currAC;
                        break;

                    case "7":
                    case "8":
                        listToOutput[nthRow].value10 += currGRS * js.Tree.ExpansionFactor * currAC;
                        listToOutput[nthRow].value11 += currNET * js.Tree.ExpansionFactor * currAC;
                        //  totals
                        listToOutput[nthRow].value14 += currGRS * js.Tree.ExpansionFactor * currAC;
                        listToOutput[nthRow].value15 += currNET * js.Tree.ExpansionFactor * currAC;
                        break;

                    case "9":
                        listToOutput[nthRow].value13 += currGRS * js.Tree.ExpansionFactor * currAC;
                        //  total
                        listToOutput[nthRow].value14 += currGRS * js.Tree.ExpansionFactor * currAC;
                        break;
                }  //  end switch
            }  //  end foreach loop
            return;
        }  //  end AccumulateTableValues

        private void UpdateSubtotal(string currPU, double currUnitAcres, string currProd,
                                   string currSP, string thisUOM, double currGross, double currNet)
        {
            //  R602
            //Is group already in subtotal list?  Update if so or add if not.
            int nthRow = unitSubtotal.FindIndex(
                delegate (RegionalReports r)
                {
                    return r.value1 == currPU && r.value5 == currSP && r.value6 == currProd;
                });

            if (nthRow >= 0)
            {
                unitSubtotal[nthRow].value7 += currGross;
                unitSubtotal[nthRow].value8 += currNet;
            }
            else if (nthRow < 0)
            {
                RegionalReports rr = new RegionalReports();
                rr.value1 = currPU;
                rr.value3 = thisUOM;
                rr.value5 = currSP;
                rr.value6 = currProd;
                rr.value7 = currGross;
                rr.value8 = currNet;
                unitSubtotal.Add(rr);
            }  //  endif

            return;
        }  //  end UpdateSubtotal

        private void OutputUnitSubtotal(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  R602
            double calcValue = 0;
            strWriteOut.WriteLine(" ");
            numOlines++;
            strWriteOut.Write("  PAYMENT UNIT ");
            if (unitSubtotal[0].value1 == null)
                strWriteOut.Write("   ");
            else strWriteOut.Write(unitSubtotal[0].value1.PadLeft(3, ' '));
            strWriteOut.WriteLine(" TOTALS --------------------------------------------------------------------------------");
            numOlines++;

            foreach (RegionalReports us in unitSubtotal)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                       rRH.R602columns, 13, ref pageNumb, "");
                if (us.value1 == null)
                    strWriteOut.Write("   ");
                else strWriteOut.Write(us.value1.PadLeft(3, ' '));
                strWriteOut.Write(String.Format("{0,7:F1}", us.value10).PadLeft(17, ' '));
                strWriteOut.Write(us.value5.PadLeft(15, ' '));
                strWriteOut.Write(us.value6.PadLeft(10, ' '));
                strWriteOut.Write(us.value3.PadLeft(8, ' '));
                // % defect
                if (us.value7 > 0)
                    calcValue = Math.Floor(((us.value7 - us.value8) / us.value7) * 100);
                else calcValue = 0.0;
                if (calcValue < 0) calcValue = 0;
                strWriteOut.Write(String.Format("{0,3:F0}", calcValue).PadLeft(7, ' '));
                strWriteOut.WriteLine(String.Format("{0,10:F2}", us.value8 / convFactor).PadLeft(28, ' '));
                numOlines++;
            }  //  end foreach loop on unit subtotals
            strWriteOut.WriteLine(reportConstants.longLine);
            numOlines++;
            return;
        }  //  end OutputUnitSubtotal

        private void UpdateOverallTotal()
        {
            //  R602
            foreach (RegionalReports us in unitSubtotal)
            {
                //  does product already exist in totalToOutput
                int nthRow = totalToOutput.FindIndex(
                    delegate (ReportSubtotal rs)
                    {
                        return rs.Value1 == us.value6;
                    });
                if (nthRow >= 0)
                {
                    //  update total net
                    totalToOutput[nthRow].Value3 += us.value8;
                }
                else if (nthRow < 0)
                {
                    //  add a new row
                    ReportSubtotal rs = new ReportSubtotal();
                    rs.Value1 = us.value6;
                    rs.Value3 = us.value8;
                    totalToOutput.Add(rs);
                }  //  endif
            }  //  end foreach on unitSubtotal
            return;
        }  //  end UpdateOverallTotal

        private void outputOverallTotal(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  R602
            //  per Jeff Penman, he wants the products sorted like 01, 20 etc.
            //  Since I can't get a List to sort properly, wrote my own.
            //  So the toatlToOutput needs to be sorted first.
            sortGrandTotal(totalToOutput);
            double grandTotal = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                   rRH.R602columns, 13, ref pageNumb, "");
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.WriteLine("         PRODUCT TOTALS");
            numOlines = numOlines + 2;
            foreach (ReportSubtotal tto in totalToOutput)
            {
                strWriteOut.Write(tto.Value1.PadLeft(46, ' '));
                strWriteOut.WriteLine(String.Format("{0,10:F2}", tto.Value3 / convFactor).PadLeft(42, ' '));
                numOlines++;
                grandTotal += tto.Value3;
            }  //  end foreach in totalToOutput
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.Write("                                            GRAND TOTAL                       ");
            strWriteOut.WriteLine(String.Format("{0,10:F2}", grandTotal / convFactor).PadLeft(10, ' '));
            numOlines = numOlines + 2;
            //  write footer lines
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                   rRH.R602columns, 12, ref pageNumb, "");
            strWriteOut.WriteLine(" ");
            strWriteOut.WriteLine(" ");
            strWriteOut.WriteLine(" ");
            strWriteOut.WriteLine("VOLUME BY SPECIES AND PRODUCT IS BASED ON GRADING ALL LOGS (100%).");
            strWriteOut.WriteLine("THE DEFAULT TREE GRADE WILL BE THE TREE GRADE DEFINED IN THE SUBPOPULATION TABLE.");
            strWriteOut.WriteLine("LOG GRADE AT THE LOG RECORD LEVEL WILL OVERRIDE THE DEFAULT TREE GRADE.");
            strWriteOut.WriteLine(" ");
            strWriteOut.WriteLine("PRODUCT 01 (SAW LOG) ARE GRADES 1-6");
            strWriteOut.WriteLine("PRODUCT 08 & 20 (NON-SAW) ARE GRADE 8");
            strWriteOut.WriteLine("GRADES 9 WILL CULL (100% DEFECT) WHEN USED AT THE LOG RECORD LEVEL.");
            strWriteOut.WriteLine(" ");
            strWriteOut.WriteLine("PRODUCT 08 & 20=FS, PRODUCT 02=BLM");
        }  //  end outputOverallTotal

        private void WriteCurrentUnit(StreamWriter strWriteout, ref int pageNumb, reportHeaders rh, string prevUOM)
        {
            //  R602
            double calcValue = 0;
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteout, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                   rRH.R602columns, 13, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                if (lto.value1 == null)
                    prtFields.Add("  ");
                else prtFields.Add(lto.value1.PadLeft(2, ' '));
                prtFields.Add(lto.value2.PadLeft(3, ' '));
                prtFields.Add(String.Format("{0,7:F1}", lto.value10).PadLeft(7, ' '));
                if (lto.value4 == null)
                    prtFields.Add("   ");
                else prtFields.Add(lto.value4.PadLeft(3, ' '));
                prtFields.Add(lto.value5.PadRight(6, ' '));
                prtFields.Add(lto.value6.PadLeft(2, '0'));
                prtFields.Add(prevUOM.PadLeft(2, '0'));
                //  calculate and print percent defect
                if (lto.value7 > 0.0)
                    calcValue = Math.Floor(((lto.value7 - lto.value8) / lto.value7) * 100);
                else calcValue = 0.0;
                if (calcValue < 0) calcValue = 0;
                prtFields.Add(String.Format("{0,3:F0}", calcValue).PadLeft(3, ' '));
                //  and net volume
                prtFields.Add(String.Format("{0,10:F2}", lto.value8 / convFactor).PadLeft(10, ' '));

                printOneRecord(fieldLengths, prtFields, strWriteout);
            }  //  end foreach loop on listToOutput
            return;
        }  //  edn WriteCurrentUnit

        private void WriteCurrentTable(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            // R604/R605
            double calcValue = 0;
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                    completeHeader, 16, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(" ");
                prtFields.Add(lto.value1.PadLeft(2, ' '));
                //  sawtimber columns
                prtFields.Add(String.Format("{0,8:F2}", lto.value7 / convFactor).PadLeft(8, ' '));
                prtFields.Add(String.Format("{0,8:F2}", lto.value8 / convFactor).PadLeft(8, ' '));
                if (lto.value7 > 0)
                    calcValue = ((lto.value7 - lto.value8) / lto.value7) * 100;
                else calcValue = 0.0;
                prtFields.Add(String.Format("{0,5:F1}", calcValue).PadLeft(5, ' '));
                //  non-sawtimber columns
                prtFields.Add(String.Format("{0,8:F2}", lto.value10 / convFactor).PadLeft(8, ' '));
                prtFields.Add(String.Format("{0,8:F2}", lto.value11 / convFactor).PadLeft(8, ' '));
                if (lto.value10 > 0)
                    calcValue = ((lto.value10 - lto.value11) / lto.value10) * 100;
                else calcValue = 0.0;
                prtFields.Add(String.Format("{0,5:F1}", calcValue).PadLeft(5, ' '));
                //  cull logs
                prtFields.Add(String.Format("{0,9:F2}", lto.value13 / convFactor).PadLeft(9, ' '));
                //  total gross volume
                prtFields.Add(String.Format("{0,9:F2}", lto.value14 / convFactor).PadLeft(9, ' '));
                //  total net volume
                prtFields.Add(String.Format("{0,9:F2}", lto.value15 / convFactor).PadLeft(9, ' '));
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }  //  end foreach loop
            return;
        }  //  end WriteCurrentTable

        private void PrintTotal(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  R604/R605
            double totalGRS = 0;
            double totalNET = 0;
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                   completeHeader, 16, ref pageNumb, "");
            strWriteOut.Write(" SPECIES TOTALS:     ");
            //  sawtimber columns
            totalGRS = listToOutput.Sum(lto => lto.value7);
            totalNET = listToOutput.Sum(lto => lto.value8);
            strWriteOut.Write(String.Format("{0,8:F2}", totalGRS / convFactor).PadLeft(8, ' '));
            strWriteOut.Write(String.Format("{0,8:F2}", totalNET / convFactor).PadLeft(10, ' '));
            if (totalGRS > 0)
                calcValue = ((totalGRS - totalNET) / totalGRS) * 100;
            else calcValue = 0.0;
            strWriteOut.Write(String.Format("{0,5:F1}", calcValue).PadLeft(10, ' '));
            strWriteOut.Write("%");
            // non-sawtimber columns
            totalGRS = listToOutput.Sum(lto => lto.value10);
            totalNET = listToOutput.Sum(lto => lto.value11);
            strWriteOut.Write(String.Format("{0,8:F2}", totalGRS / convFactor).PadLeft(14, ' '));
            strWriteOut.Write(String.Format("{0,8:F2}", totalNET / convFactor).PadLeft(10, ' '));
            if (totalGRS > 0)
                calcValue = ((totalGRS - totalNET) / totalGRS) * 100;
            else calcValue = 0.0;
            strWriteOut.Write(String.Format("{0,5:F1}", calcValue).PadLeft(9, ' '));
            strWriteOut.Write("%");
            //  cull log
            calcValue = listToOutput.Sum(lto => lto.value13);
            strWriteOut.Write(String.Format("{0,9:F2}", calcValue / convFactor).PadLeft(14, ' '));
            //  total gross
            calcValue = listToOutput.Sum(lto => lto.value14);
            strWriteOut.Write(String.Format("{0,9:F2}", calcValue / convFactor).PadLeft(16, ' '));
            //  total net
            calcValue = listToOutput.Sum(lto => lto.value15);
            strWriteOut.WriteLine(String.Format("{0,9:F2}", calcValue / convFactor).PadLeft(14, ' '));
            return;
        }  //  PrintTotal

        private void OutputFooter(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                               completeHeader, 16, ref pageNumb, "");
            strWriteOut.WriteLine(" VOLUME BY SPECIES AND PRODUCT IS BASED ON GRADING ALL LOGS.");
            strWriteOut.WriteLine(" THE DEFAULT TREE GRADE WILL BE THE TREE GRADE DEFINED IN THE SUBPOPULATION TABLE.");
            strWriteOut.WriteLine(" LOG GRADE AT THE LOG RECORD LEVEL WILL OVERRIDE THE DEFAULT TREE GRADE.");
            strWriteOut.WriteLine(" GRADES WILL BE ASSIGNED BASED ON THE PRIMARY AND SECONDARY TOP");
            strWriteOut.WriteLine(" LISTED IN THE VOLUME EQUATIONS.  LOGS WITH TOP DIB LESS THAN THE PRIMARY TOP DIB");
            strWriteOut.WriteLine(" WILL AUTOMATICALLY BE ASSIGNED A NON-SAW TIMBER GRADE 8.");
            return;
        }  //  end OutputFooter

        private int findTableRow(float currSED)
        {
            int currDiam = (int)Math.Floor(currSED + 0.5);
            int currRow = listToOutput.FindIndex(
                delegate (RegionalReports r)
                {
                    return r.value1 == currDiam.ToString();
                });
            return currRow;
        }  //  end findTableRow

        private string[] createCompleteHeader(string currSP, double avgDBH)
        {
            //  R604/R605
            string[] finnishHeader = new string[7];
            finnishHeader[0] = rRH.R604R605columns[0].Replace("XX", tableNumber.ToString().PadLeft(2, ' '));
            finnishHeader[1] = rRH.R604R605columns[1].Replace("ZZZZZZ", currSP.PadRight(6, ' '));
            string printAvgDBH = String.Format("{0,6:F1}", avgDBH);
            finnishHeader[2] = rRH.R604R605columns[2].Replace("TTTTTT", printAvgDBH.PadLeft(6, ' '));
            // load rest of header
            for (int k = 3; k < 7; k++)
                finnishHeader[k] = rRH.R604R605columns[k];

            return finnishHeader;
        }  //  end createCompleteHeader

        private double CalculateAverageDBH(string currSP, string currCL)
        {
            //  need all current species from LCD
            List<LCDDO> lcdList = DataLayer.getLCD();
            List<LCDDO> justSpecies = lcdList.FindAll(
                delegate (LCDDO l)
                {
                    return l.Species == currSP && l.CutLeave == currCL;
                });
            double DBHsum = justSpecies.Sum(j => j.SumDBHOB);
            double EFsum = justSpecies.Sum(j => j.SumExpanFactor);
            if (EFsum > 0)
                return DBHsum / EFsum;
            else return 0;
        }  //  end CalculateAverageDBH

        private void fillOneInchClass(int maxDIB)
        {
            for (int j = 2; j <= maxDIB; j++)
            {
                RegionalReports rr = new RegionalReports();
                rr.value1 = j.ToString();
                listToOutput.Add(rr);
            }  //  end for j loop
            return;
        }  // end fillOneInchClass

        private void sortGrandTotal(List<ReportSubtotal> totalToOutput)
        {
            //  R602
            List<ReportSubtotal> sortedList = new List<ReportSubtotal>();
            //  Product 01 first
            foreach (ReportSubtotal tto in totalToOutput)
            {
                if (tto.Value1 == "01")
                {
                    ReportSubtotal r = new ReportSubtotal();
                    r.Value1 = tto.Value1;
                    r.Value3 = tto.Value3;
                    sortedList.Add(r);
                }  //  endif
            }  //  end foreach loop
               //  Product 08
            foreach (ReportSubtotal tto in totalToOutput)
            {
                if (tto.Value1 == "08")
                {
                    ReportSubtotal r = new ReportSubtotal();
                    r.Value1 = tto.Value1;
                    r.Value3 = tto.Value3;
                    sortedList.Add(r);
                }  //  endif
            }  //  end foreach loop
               //  and Product 20
            foreach (ReportSubtotal tto in totalToOutput)
            {
                if (tto.Value1 == "20")
                {
                    ReportSubtotal r = new ReportSubtotal();
                    r.Value1 = tto.Value1;
                    r.Value3 = tto.Value3;
                    sortedList.Add(r);
                }  //  endif
            }  //  end foreach loop

            //  reset totalToOutput
            totalToOutput.Clear();
            foreach (ReportSubtotal sl in sortedList)
            {
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = sl.Value1;
                r.Value3 = sl.Value3;
                totalToOutput.Add(r);
            }  //  end foreach loop
            return;
        }  //  end sortGrandTotal
    }
}