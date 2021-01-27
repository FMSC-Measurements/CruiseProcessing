using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    class OutputLiveDead : CreateTextFile
    {
        #region
        public string currentReport;
        private int[] fieldLengths;
        private List<string> prtFields = new List<string>();
        private List<ReportSubtotal> productSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> unitMethodSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> grandTotal = new List<ReportSubtotal>();
        private string[] completeHeader = new string[8];
        private List<StratumDO> sList = new List<StratumDO>();
        private List<CuttingUnitDO> cList = new List<CuttingUnitDO>();
        private List<PRODO> proList = new List<PRODO>();
        private string volType;
        private string currType;
        private List<LiveDeadRecords> ldList = new List<LiveDeadRecords>();
        private double estTrees = 0.0;
        private double totalGrsP = 0.0;
        private double totalNetP = 0.0;
        private double totalGrsS = 0.0;
        private double totalNetS = 0.0;
        #endregion
        
        public void CreateLiveDead(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  generates LD1-LD8 live/dead reports
            string currentTitle = fillReportTitle(currentReport);
            sList = bslyr.getStratum();
            cList = bslyr.getCuttingUnits();
            proList = bslyr.getPRO();
            List<LCDDO> lcdList = bslyr.getLCD();

            //  clear total values
            productSubtotal.Clear();
            unitMethodSubtotal.Clear();
            grandTotal.Clear();
            ldList.Clear();

            numOlines = 0;
            //  set volume type based on report
            switch(currentReport)
            {
                case "LD1":
                case "LD3":
                case "LD5":
                case "LD7":
                    volType = "CUFT";
                    //  any data for report?
                    if (lcdList.Sum(l => l.SumGCUFT) == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No cubic foot volume for report");
                        return;
                    }   //  endif testVolume
                    break;
                case "LD2":
                case "LD4":
                case "LD6":
                case "LD8":
                    volType = "BDFT";
                    //  any data for report?
                    if (lcdList.Sum(l => l.SumGBDFT) == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No board foot volume for report");
                        return;
                    }   //  endif no volume
                    break;
            }   //  end switch
            //  create report title and headers
            rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FTS, reportConstants.FCTO);
            //  pull LCD groups
            List<LCDDO> justGroups = LCDmethods.GetCutGroupedBy("", "", 9, bslyr);
            switch (currentReport)
            {
                case "LD1":
                case "LD2":
                    ldList.Clear();
                    finishColumnHeaders(rh.LD1LD2left, rh.LiveDeadRight);
                    fieldLengths = new int[] { 1, 3, 7, 9, 10, 12, 9, 10, 12, 9, 10, 12, 9, 10, 8 };
                    AccumulateAllData(justGroups, lcdList);
                    OutputData(strWriteOut, rh, ref pageNumb, "");
                    //  output last product subtotal and then grand total
                    OutputSubtotal("P", rh, productSubtotal, strWriteOut, ref pageNumb, "");
                    OutputSubtotal("G", rh, grandTotal, strWriteOut, ref pageNumb, "");
                    break;
                case "LD3":
                case "LD4":
                    finishColumnHeaders(rh.LD3LD4left, rh.LiveDeadRight);
                    fieldLengths = new int[] { 1, 4, 3, 7, 9, 10, 11, 9, 10, 11, 9, 10, 11, 9, 10, 8 };
                    AccumulateAllData("CUT", justGroups);
                    OutputData(strWriteOut, rh, ref pageNumb, "CUT");
                    //  output last product subtotal
                    OutputSubtotal("P", rh, productSubtotal, strWriteOut, ref pageNumb, "CUT");
                    OutputSubtotal("U", rh, unitMethodSubtotal, strWriteOut, ref pageNumb, "CUT");
                    OutputSubtotal("G", rh, grandTotal, strWriteOut, ref pageNumb, "CUT");
                    break;
                case "LD5":
                case "LD6":
                    finishColumnHeaders(rh.LD5LD6left, rh.LiveDeadRight);
                    fieldLengths = new int[] { 1, 4, 3, 7, 9, 10, 11, 9, 10, 11, 9, 10, 11, 9, 10, 8 };
                    AccumulateAllData("PAY", justGroups);
                    OutputData(strWriteOut, rh, ref pageNumb, "PAY");
                    //  output last product subtotal
                    OutputSubtotal("P", rh, productSubtotal, strWriteOut, ref pageNumb, "PAY");
                    OutputSubtotal("U", rh, unitMethodSubtotal, strWriteOut, ref pageNumb, "PAY");
                    OutputSubtotal("G", rh, grandTotal, strWriteOut, ref pageNumb, "PAY");
                    break;
                case "LD7":
                case "LD8":
                    //  June 2017 -- these reports are by logging method so if blank or null
                    // report cannot be generated
                    List<CuttingUnitDO> cutList = bslyr.getCuttingUnits();
                    int noMethod = 0;
                    foreach(CuttingUnitDO ct in cutList)
                    {
                        if(ct.LoggingMethod == "" || ct.LoggingMethod == " " || ct.LoggingMethod == null)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> One or more logging methods are missing.  Cannot generate report.");
                            noMethod = -1;
                        }   //  endif
                    }   //  end foreach loop
                    if (noMethod != -1)
                    {
                        finishColumnHeaders(rh.LD7LD8left, rh.LiveDeadRight);
                        fieldLengths = new int[] { 1, 4, 3, 7, 9, 10, 11, 9, 10, 11, 9, 10, 11, 9, 10, 8 };
                        AccumulateAllData("LOG", justGroups);
                        OutputData(strWriteOut, rh, ref pageNumb, "LOG");
                        //  output last product subtotal
                        OutputSubtotal("P", rh, productSubtotal, strWriteOut, ref pageNumb, "LOG");
                        OutputSubtotal("U", rh, unitMethodSubtotal, strWriteOut, ref pageNumb, "LOG");
                        OutputSubtotal("G", rh, grandTotal, strWriteOut, ref pageNumb, "LOG");
                    }   //  endif
                    break;
            }   //  end switch on current report
            //  output footer
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine(rh.LiveDeadFooter[0]);
            strWriteOut.WriteLine(rh.LiveDeadFooter[1]);
            strWriteOut.WriteLine(rh.LiveDeadFooter[2]);

            return;
        }   // end CreateLiveDead


        private void AccumulateAllData(List<LCDDO> justGroups, List<LCDDO> lcdList)
        {
            //  Works for LD1-LD2
            //  Calculate values and store
            //  Load group information into list
            LoadGroup("", justGroups);

            foreach (LCDDO jg in justGroups)
            {
                estTrees = 0.0;
                totalGrsP = 0.0;
                totalGrsS = 0.0;
                totalNetP = 0.0;
                totalNetS = 0.0;
                //  Find all live, dead and other to sum
                for (int j = 0; j < 3; j++)
                {
                    switch (j)
                    {
                        case 0:
                            currType = "L";
                            break;
                        case 1:
                            currType = "D";
                            break;
                        case 2:
                            currType = "O";
                            break;
                    }   //  end switch

                    List<LCDDO> justLiveDead = lcdList.FindAll(
                        delegate(LCDDO l)
                        {
                            return l.CutLeave == jg.CutLeave &&
                                   l.PrimaryProduct == jg.PrimaryProduct &&
                                   l.SecondaryProduct == jg.SecondaryProduct &&
                                   l.Species == jg.Species && l.LiveDead == currType;
                        });
                    SumUp(justLiveDead);
                    //  load primary
                    LoadList(estTrees, totalGrsP, totalNetP, jg.PrimaryProduct, jg.Species, currType, "P");
                    //  load secondary
                    LoadList(estTrees, totalGrsS, totalNetS, jg.SecondaryProduct, jg.Species, currType, "S");
                    estTrees = 0.0;
                    totalGrsP = 0.0;
                    totalGrsS = 0.0;
                    totalNetP = 0.0;
                    totalNetS = 0.0;
                }   //  end for loop through live/dead type
            }   //  end foreach loop
            return;
        }   //  end AccumulateData


        private void AccumulateAllData(string reportType, List<LCDDO> justGroups)
        {
            //  Works for LD3-LD8
            //  total by stratum to prorate volume
            foreach (StratumDO s in sList)
            {
                s.CuttingUnits.Populate();
                //  load groups into LiveDeadRecords list
                LoadGroupInfo(reportType, justGroups, s);
                estTrees = 0.0;
                totalGrsP = 0.0;
                totalNetP = 0.0;
                totalGrsS = 0.0;
                totalNetS = 0.0;
                //  Pull LCD for current stratum
                List<LCDDO> listToProrate = bslyr.GetLCDdata(s.Code, "WHERE Stratum = @p1 AND CutLeave = @p2", "PrimaryProduct,SecondaryProduct,Species");
                for(int j=0;j<3;j++)
                {
                    switch (j)
                    {
                        case 0:
                            currType = "L";
                            break;
                        case 1:
                            currType = "D";
                            break;
                        case 2:
                            currType = "O";
                            break;
                    }   //  end switch
                    foreach (LCDDO jg in justGroups)
                    {
                        //  find live for current group
                        List<LCDDO> justLiveDead = listToProrate.FindAll(
                            delegate(LCDDO l)
                            {
                                return l.Stratum == s.Code && l.CutLeave == jg.CutLeave && 
                                    l.PrimaryProduct == jg.PrimaryProduct &&
                                    l.SecondaryProduct == jg.SecondaryProduct && l.Species == jg.Species &&
                                    l.LiveDead == currType;
                            });
                        SumUp(justLiveDead);
                        //  load primary
                        LoadList(s, reportType, estTrees, totalGrsP, totalNetP, jg.PrimaryProduct, jg.Species, jg.SampleGroup, currType, "P");
                        //  load secondary
                        LoadList(s, reportType, estTrees, totalGrsS, totalNetS, jg.SecondaryProduct, jg.Species, jg.SampleGroup, currType, "S");
                        estTrees = 0.0;
                        totalGrsP = 0.0;
                        totalNetP = 0.0;
                        totalGrsS = 0.0;
                        totalNetS = 0.0;
                    }   //  end for loop on live/dead codes
                }   //  end foreach loop on justGroups
            }   //  end foreach loop on stratum
            return;
        }   //  end AccumulateAllData


        private void LoadGroupInfo(string reportType, List<LCDDO> justGroups, StratumDO sdo)
        {
            //  Works for LD3-LD8
            //  find current report type and load groups for each
            switch (reportType)
            {
                case "CUT":
                    foreach (CuttingUnitDO cuttingUnit in sdo.CuttingUnits)
                        LoadGroup(cuttingUnit.Code, justGroups);
                    break;
                case "PAY":
                    foreach (CuttingUnitDO cuttingUnit in sdo.CuttingUnits)
                        LoadGroup(cuttingUnit.PaymentUnit, justGroups);
                    break;
                case "LOG":
                    foreach (CuttingUnitDO cuttingUnit in sdo.CuttingUnits)
                        LoadGroup(cuttingUnit.LoggingMethod, justGroups);
                    break;
            }   //  end switch
            return;
        }   //  end LoadGroupInfo

        private void LoadGroup(string currType, List<LCDDO> justGroups)
        {
            //  Loads group for current type
            //  LD3-LD8
            foreach (LCDDO l in justGroups)
            {
                // load primary product groups
                int nthRow = ldList.FindIndex(
                    delegate(LiveDeadRecords rdl)
                    {
                        return rdl.value1 == l.PrimaryProduct && rdl.value2 == l.Species &&
                                rdl.value3 == currType;
                    });
                if (nthRow == -1)
                {
                    LiveDeadRecords ld = new LiveDeadRecords();
                    ld.value1 = l.PrimaryProduct;
                    ld.value2 = l.Species;
                    if (currType == null)
                        ld.value3 = " ";
                    else ld.value3 = currType;
                    ld.priOrsec = "P";
                    ldList.Add(ld);
                }   // endif
            }   //  end foreach loop
            //  secondary product
            //  is there secondary product to report?
            if ((justGroups.Sum(j => j.SumGBDFTtop) > 0) || (justGroups.Sum(j => j.SumGCUFTtop) > 0))
            {
                foreach (LCDDO l in justGroups)
                {
                    int nthRow = ldList.FindIndex(
                        delegate(LiveDeadRecords rdl)
                        {
                            return rdl.value1 == l.SecondaryProduct && rdl.value2 == l.Species &&
                                    rdl.value3 == currType;
                        });
                    if (nthRow == -1)
                    {
                        LiveDeadRecords ld = new LiveDeadRecords();
                        ld.value1 = l.SecondaryProduct;
                        ld.value2 = l.Species;
                        if (currType == null)
                            ld.value3 = " ";
                        else ld.value3 = currType;
                        ld.priOrsec = "S";
                        ldList.Add(ld);
                    }   // endif

                }   //  end foreach loop
            }   //  endif secondary volume available
            return;
        }   //  end LoadGroup


        private void SumUp(List<LCDDO> justList)
        {
            double strAcres = 0.0;
            foreach (LCDDO jl in justList)
            {
                //  need correct acres for each stratum
                int nthRow = sList.FindIndex(
                    delegate(StratumDO s)
                    {
                        return s.Code == jl.Stratum;
                    });
                if(nthRow >= 0)
                    strAcres = Utilities.ReturnCorrectAcres(jl.Stratum,bslyr,(long)sList[nthRow].Stratum_CN);

                //  modify strata acres based on report number.  prorated reports should not be expanded by acres
                if (currentReport != "LD1" && currentReport != "LD2")
                    strAcres = 1.0;
                
                switch (volType)
                { 
                    case "BDFT":
                        totalGrsP += jl.SumGBDFT * strAcres;
                        totalNetP += jl.SumNBDFT * strAcres;
                        totalGrsS += jl.SumGBDFTtop * strAcres;
                        totalNetS += jl.SumNBDFTtop * strAcres;
                        //  Add recovered to net primary
                        totalNetP += jl.SumBDFTrecv * strAcres;
                        break;
                    case "CUFT":
                        totalGrsP += jl.SumGCUFT * strAcres;
                        totalNetP += jl.SumNCUFT * strAcres;
                        totalGrsS += jl.SumGCUFTtop * strAcres;
                        totalNetS += jl.SumNCUFTtop * strAcres;
                        //  Add recovered to net primary
                        totalNetP += jl.SumCUFTrecv * strAcres;
                        break;
                } //  end switch
                if (jl.STM == "Y")
                    estTrees += jl.SumExpanFactor;
                else if (Utilities.MethodLookup(jl.Stratum, bslyr) == "3P" ||
                        Utilities.MethodLookup(jl.Stratum, bslyr) == "S3P")
                    estTrees += jl.TalliedTrees;
                else estTrees += jl.SumExpanFactor * strAcres;
            }   //  end foreach loop
            
            return;
        }   //  end SumUp


        private void LoadList(double ETS, double TGV, double TNV, string currPrd, 
                                string currSpec, string whichSection, string whichProd)
        {
            //  Load into LiveDeadRecords list
            //  find record and add values
            //  this works for LD1/LD2
            //  find primary product and secondary product
            int nthRow = ldList.FindIndex(
                delegate(LiveDeadRecords ld)
                {
                    return ld.value1 == currPrd && ld.value2 == currSpec;
                });
            if (nthRow >= 0)
            {
                switch (whichSection)
                {
                    case "L":
                        if (whichProd == "P")
                            ldList[nthRow].value4 += ETS;
                        else if(whichProd == "S")
                            ldList[nthRow].value4 += 0;
                        ldList[nthRow].value5 += TGV;
                        ldList[nthRow].value6 += TNV;
                        break;
                    case "D":
                        if (whichProd == "P")
                            ldList[nthRow].value7 += ETS;
                        else if(whichProd == "S")
                            ldList[nthRow].value7 += 0;
                        ldList[nthRow].value8 += TGV;
                        ldList[nthRow].value9 += TNV;
                        break;
                    case "O":
                        if (whichProd == "P")
                            ldList[nthRow].value10 += ETS;
                        else if(whichProd == "S")
                            ldList[nthRow].value10 += 0.0;
                        ldList[nthRow].value11 += TGV;
                        ldList[nthRow].value12 += TNV; 
                        break;
                }   //  end switch
                //  update total column
                if (whichProd == "P")
                    ldList[nthRow].value13 += ETS;
                else if(whichProd == "S")
                    ldList[nthRow].value13 += 0.0;
                ldList[nthRow].value14 += TGV;
                ldList[nthRow].value15 += TNV;
            }   //  endif nthRow

            return;
        }   //  end LoadList


        private void LoadList(StratumDO sdo, string reportType, double ETS, double TGV, double TNV, 
                                string currPrd, string currSpec, string currSG, string whichSection,
                                string whichProd)
        {
            //  find group and load into LiveDeadRecords list
            //  works for LD3-LD8
            string currType = "";
            double proratFactor = 0;
            foreach (CuttingUnitDO cuttingUnit in sdo.CuttingUnits)
            {
                switch (reportType)
                {
                    case "CUT":
                        currType = cuttingUnit.Code;
                        break;
                    case "PAY":
                        currType = cuttingUnit.PaymentUnit;
                        if (currType == null) currType = " ";
                        break;
                    case "LOG":
                        currType = cuttingUnit.LoggingMethod;
                        if (currType == null) currType = " ";
                        break;
                }   //  end switch

            int nthRow = ldList.FindIndex(
                delegate(LiveDeadRecords ld)
                {
                    return ld.value1 == currPrd && ld.value2 == currSpec && ld.value3 == currType;
                });
            if (nthRow >= 0)
            {
                //  need proration factor for this unit
                int ithRow = proList.FindIndex(
                    delegate(PRODO p)
                    {
                        return p.Stratum == sdo.Code && p.CuttingUnit == cuttingUnit.Code &&
                                p.SampleGroup == currSG;
                    });
                if (ithRow >= 0)
                    proratFactor = proList[ithRow].ProrationFactor;

                switch (whichSection)
                {
                    case "L":
                        if (whichProd == "P")
                            ldList[nthRow].value4 += ETS * proratFactor;
                        else if(whichProd == "S")
                            ldList[nthRow].value4 += 0;
                        ldList[nthRow].value5 += TGV * proratFactor;
                        ldList[nthRow].value6 += TNV * proratFactor;
                        break;
                    case "D":
                        if (whichProd == "P")
                            ldList[nthRow].value7 += ETS * proratFactor;
                        else if(whichProd == "S")
                            ldList[nthRow].value7 += 0;
                        ldList[nthRow].value8 += TGV * proratFactor;
                        ldList[nthRow].value9 += TNV * proratFactor;
                        break;
                    case "O":
                        if (whichProd == "P")
                            ldList[nthRow].value10 += ETS * proratFactor;
                        else if(whichProd == "S")
                            ldList[nthRow].value10 += 0;
                        ldList[nthRow].value11 += TGV * proratFactor;
                        ldList[nthRow].value12 += TNV * proratFactor;
                        break;
                }   //  end switch
                //  update total column
                if (whichProd == "P")
                    ldList[nthRow].value13 += ETS * proratFactor;
                else if(whichProd == "S")
                    ldList[nthRow].value13 += 0;
                ldList[nthRow].value14 += TGV * proratFactor;
                ldList[nthRow].value15 += TNV * proratFactor;
            }   //  endif nthRow
            }   //  end for loop
            return;
        }   //  end LoadList


        private void OutputData(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, string reportType)
        {
            //  now write body of report with appropriate subtotals
            string currProd = "*";
            string currUnit = "*";
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                completeHeader, 16, ref pageNumb, "");
            foreach (LiveDeadRecords ldr in ldList)
            {
                if (ldr.value13 > 0)
                {
                    prtFields.Clear();
                    prtFields.Add("");
                    switch (reportType)
                    {
                        case "":
                            if (currProd == "*")
                            {
                                currProd = ldr.value1;
                                prtFields.Clear();
                                prtFields.Add("");
                                prtFields.Add(currProd);
                            }
                            else if (currProd == ldr.value1)
                            {
                                prtFields.Clear();
                                prtFields.Add("");
                                prtFields.Add("  ");
                            }
                            else if (currProd != ldr.value1)
                            {
                                OutputSubtotal("P", rh, productSubtotal, strWriteOut, ref pageNumb, "");
                                productSubtotal.Clear();
                                currProd = ldr.value1;
                                prtFields.Clear();
                                prtFields.Add("");
                                prtFields.Add(currProd);
                            }   //  endif
                            break;
                        case "CUT":
                        case "PAY":
                        case "LOG":
                            if (currProd == "*" && currUnit == "*")
                            {
                                currProd = ldr.value1;
                                currUnit = ldr.value3;
                                prtFields.Add(currUnit.Trim());
                                prtFields.Add(currProd);
                            }
                            else if (currProd == ldr.value1 && currUnit == ldr.value3)
                            {
                                //  new species
                                prtFields.Clear();
                                prtFields.Add("");
                                prtFields.Add("   ");
                                prtFields.Add("  ");
                            }
                            else if (currProd != ldr.value1 && currUnit == ldr.value3)
                            {
                                //  new product but unit is the same -- output product subtotal
                                OutputSubtotal("P", rh, productSubtotal, strWriteOut, ref pageNumb, reportType);
                                productSubtotal.Clear();
                                currProd = ldr.value1;
                                prtFields.Clear();
                                prtFields.Add("");
                                prtFields.Add("    ");
                                prtFields.Add(currProd);
                            }
                            else if (currUnit != ldr.value3)
                            {
                                //  output product subtotal
                                OutputSubtotal("P", rh, productSubtotal, strWriteOut, ref pageNumb, reportType);
                                //  output unit subtotal
                                prtFields.Clear();
                                OutputSubtotal("U", rh, unitMethodSubtotal, strWriteOut, ref pageNumb, reportType);
                                productSubtotal.Clear();
                                unitMethodSubtotal.Clear();
                                currProd = ldr.value1;
                                currUnit = ldr.value3;
                                prtFields.Clear();
                                prtFields.Add("");
                                prtFields.Add(currUnit.Trim());
                                prtFields.Add(currProd);
                            }   //  endif
                            break;
                    }   //  end switch

                    //  load species
                    prtFields.Add(ldr.value2);
                    //  finish loading record into print fields
                    //  live section
                    prtFields.Add(String.Format("{0,7:F0}", ldr.value4).PadLeft(7, ' '));
                    prtFields.Add(String.Format("{0,8:F0}", ldr.value5).PadLeft(8, ' '));
                    prtFields.Add(String.Format("{0,8:F0}", ldr.value6).PadLeft(8, ' '));
                    UpdateSubtotal(ldr.value4, ldr.value5, ldr.value6, currProd, "", "L", productSubtotal);
                    UpdateSubtotal(ldr.value4, ldr.value5, ldr.value6, currProd, currUnit, "L", unitMethodSubtotal);
                    UpdateSubtotal(ldr.value4, ldr.value5, ldr.value6, currProd, "", "L", grandTotal);
                    // dead section
                    prtFields.Add(String.Format("{0,7:F0}", ldr.value7).PadLeft(7, ' '));
                    prtFields.Add(String.Format("{0,8:F0}", ldr.value8).PadLeft(8, ' '));
                    prtFields.Add(String.Format("{0,8:F0}", ldr.value9).PadLeft(8, ' '));
                    UpdateSubtotal(ldr.value7, ldr.value8, ldr.value9, currProd, "", "D", productSubtotal);
                    UpdateSubtotal(ldr.value7, ldr.value8, ldr.value9, currProd, currUnit, "D", unitMethodSubtotal);
                    UpdateSubtotal(ldr.value7, ldr.value8, ldr.value9, currProd, "", "D", grandTotal);
                    //  other section
                    prtFields.Add(String.Format("{0,7:F0}", ldr.value10).PadLeft(7, ' '));
                    prtFields.Add(String.Format("{0,8:F0}", ldr.value11).PadLeft(8, ' '));
                    prtFields.Add(String.Format("{0,8:F0}", ldr.value12).PadLeft(8, ' '));
                    UpdateSubtotal(ldr.value10, ldr.value11, ldr.value12, currProd, "", "O", productSubtotal);
                    UpdateSubtotal(ldr.value10, ldr.value11, ldr.value12, currProd, currUnit, "O", unitMethodSubtotal);
                    UpdateSubtotal(ldr.value10, ldr.value11, ldr.value12, currProd, "", "O", grandTotal);
                    //  total section
                    prtFields.Add(String.Format("{0,7:F0}", ldr.value13).PadLeft(7, ' '));
                    prtFields.Add(String.Format("{0,8:F0}", ldr.value14).PadLeft(8, ' '));
                    prtFields.Add(String.Format("{0,8:F0}", ldr.value15).PadLeft(8, ' '));
                    UpdateSubtotal(ldr.value13, ldr.value14, ldr.value15, currProd, "", "T", productSubtotal);
                    UpdateSubtotal(ldr.value13, ldr.value14, ldr.value15, currProd, currUnit, "T", unitMethodSubtotal);
                    UpdateSubtotal(ldr.value13, ldr.value14, ldr.value15, currProd, "", "T", grandTotal);

                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  endif total is greater than zero
            }   //  end foreach loop
            return;
        }   //  end OutputData
        

        private void UpdateSubtotal(double ETS, double GTS, double NTS, string currProd, string currUnit,
                                string LDtype, List<ReportSubtotal> totalToUpdate)
        {
            //  updates subtotal
            if (totalToUpdate.Count == 0)
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value1 = currProd;
                rs.Value2 = currUnit;
                switch (LDtype)
                {
                    case "L":
                        rs.Value3 = ETS;
                        rs.Value4 = GTS;
                        rs.Value5 = NTS;
                        break;
                    case "D":
                        rs.Value6 = ETS;
                        rs.Value7 = GTS;
                        rs.Value8 = NTS;
                        break;
                    case "O":
                        rs.Value9 = ETS;
                        rs.Value10 = GTS;
                        rs.Value11 = NTS;
                        break;
                    case "T":
                        rs.Value12 = ETS;
                        rs.Value13 = GTS;
                        rs.Value14 = NTS;
                        break;
                }   //  end switch
                totalToUpdate.Add(rs);
            }
            else if(totalToUpdate.Count > 0)
            {
                //  record already exists -- update appropriately
                switch (LDtype)
                {
                    case "L":
                        totalToUpdate[0].Value3 += ETS;
                        totalToUpdate[0].Value4 += GTS;
                        totalToUpdate[0].Value5 += NTS;
                        break;
                    case "D":
                        totalToUpdate[0].Value6 += ETS;
                        totalToUpdate[0].Value7 += GTS;
                        totalToUpdate[0].Value8 += NTS;
                        break;
                    case "O":
                        totalToUpdate[0].Value9 += ETS;
                        totalToUpdate[0].Value10 += GTS;
                        totalToUpdate[0].Value11 += NTS;
                        break;
                    case "T":
                        totalToUpdate[0].Value12 += ETS;
                        totalToUpdate[0].Value13 += GTS;
                        totalToUpdate[0].Value14 += NTS;
                        break;
                }   // end switch
            }   //  endif 
            return;
        }   //  end UpdateSubtotal


        private void OutputSubtotal(string whichTotal, reportHeaders rh, List<ReportSubtotal> subtotalToPrint,
                                    StreamWriter strWriteOut, ref int pageNumb, string reportType)
        {
            prtFields.Clear();
            //  header if needed
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                completeHeader, 16, ref pageNumb, "");
            switch (whichTotal)
            {
                case "P":       //  product subtotal front load
                    prtFields.Add("");
                    if (reportType == "CUT" || reportType == "PAY" || reportType == "LOG")
                        prtFields.Add("    ");
                    prtFields.Add(productSubtotal[0].Value1.PadLeft(2, ' '));
                    prtFields.Add("TOTAL  ");
                    break;
                case "U":
                    //  unit subtotal front load
                    prtFields.Clear();
                    prtFields.Add("");
                    prtFields.Add(unitMethodSubtotal[0].Value2.Trim().PadRight(3, ' '));
                    prtFields.Add(" ");
                    prtFields.Add("TOTAL");
                    break;
                case "G":       //  grand total front load
                    prtFields.Clear();
                    prtFields.Add("");
                    if (reportType == "CUT" || reportType == "PAY" || reportType == "LOG")
                        prtFields.Add("    ");
                    prtFields.Add("  ");
                    prtFields.Add("TOTALS");
                    break;
            }   //  end switch

            //  Live section
            prtFields.Add(String.Format("{0,7:F0}", subtotalToPrint[0].Value3).PadLeft(7, ' '));
            prtFields.Add(String.Format("{0,8:F0}", subtotalToPrint[0].Value4).PadLeft(8, ' '));
            prtFields.Add(String.Format("{0,8:F0}", subtotalToPrint[0].Value5).PadLeft(8, ' '));
            //  dead section
            prtFields.Add(String.Format("{0,7:F0}", subtotalToPrint[0].Value6).PadLeft(7, ' '));
            prtFields.Add(String.Format("{0,8:F0}", subtotalToPrint[0].Value7).PadLeft(8, ' '));
            prtFields.Add(String.Format("{0,8:F0}", subtotalToPrint[0].Value8).PadLeft(8, ' '));
            //  other section
            prtFields.Add(String.Format("{0,7:F0}", subtotalToPrint[0].Value9).PadLeft(7, ' '));
            prtFields.Add(String.Format("{0,8:F0}", subtotalToPrint[0].Value10).PadLeft(8, ' '));
            prtFields.Add(String.Format("{0,8:F0}", subtotalToPrint[0].Value11).PadLeft(8, ' '));
            //  total section
            prtFields.Add(String.Format("{0,7:F0}", subtotalToPrint[0].Value12).PadLeft(7, ' '));
            prtFields.Add(String.Format("{0,8:F0}", subtotalToPrint[0].Value13).PadLeft(8, ' '));
            prtFields.Add(String.Format("{0,8:F0}", subtotalToPrint[0].Value14).PadLeft(8, ' '));

            switch (whichTotal)
            {
                case "P":           //  write product subtotal
                    strWriteOut.Write("             ");
                    strWriteOut.WriteLine(reportConstants.subtotalLine1);
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                    strWriteOut.WriteLine(reportConstants.longLine);
                    break;
                case "U":           // write unit subtotal
                    switch (reportType)
                    {
                        case "CUT":
                            strWriteOut.WriteLine(" CUTTING UNIT");
                            break;
                        case "PAY":
                            strWriteOut.WriteLine(" PAYMENT UNIT");
                            break;
                        case "LOG":
                            strWriteOut.WriteLine(" LOGGING METHOD");
                            break;
                    }   //  end switch
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                    strWriteOut.WriteLine(reportConstants.longLine);
                    strWriteOut.WriteLine(reportConstants.longLine);
                    break;
                case "G":       //  write grand total
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                    break;
            }   //  end switch
            prtFields.Clear();
            return;
        }   //  end OutputSubtotal


        private void finishColumnHeaders(string[] leftSide, string[] rightSide)
        {
            //  clear completeHeader before loading again
            for (int k = 0; k < 7; k++)
                completeHeader[k] = null;
            //  load up complete header
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < leftSide.Count(); k++)
            {
                sb.Clear();
                sb.Append(leftSide[k]);
                sb.Append(rightSide[k]);
                completeHeader[k] = sb.ToString();
            }   //  end for loop
            //  set volume type in the header
            completeHeader[7] = completeHeader[7].Replace("XXXX", volType);
            return;
        }   //  end finishColumnHeaders

        public class LiveDeadRecords
        {
            //  support the accumulation of data for the body of the reports
            public string value1 { get; set; }
            public string value2 { get; set; }
            public string value3 { get; set; }
            public string priOrsec { get; set; }
            public double value4 { get; set; }
            public double value5 { get; set; }
            public double value6 { get; set; }
            public double value7 { get; set; }
            public double value8 { get; set; }
            public double value9 { get; set; }
            public double value10 { get; set; }
            public double value11 { get; set; }
            public double value12 { get; set; }
            public double value13 { get; set; }
            public double value14 { get; set; }
            public double value15 { get; set; }

        }   //  end LiveDeadRecords
    }
}
