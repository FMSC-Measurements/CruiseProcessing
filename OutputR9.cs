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
    class OutputR9 : CreateTextFile
    {
        #region
        public string currentReport;
        private int[] fieldLengths;
  
        private regionalReportHeaders rRH = new regionalReportHeaders();
        private List<RegionalReports> listToOutput = new List<RegionalReports>();
        private double sumEstTrees, sumTipwood;

        public string currCL = "C";
        private ArrayList prtFields = new ArrayList();
        //private List<StratumDO> sList = new List<StratumDO>();
        private List<TreeCalculatedValuesDO> tcvList = new List<TreeCalculatedValuesDO>();
        //private List<CuttingUnitDO> cList = new List<CuttingUnitDO>();
        private List<LCDDO> lcdList = new List<LCDDO>();
        //private List<PRODO> proList = new List<PRODO>();
        private List<ReportSubtotal> unitSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> strataSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> grandTotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> summaryList = new List<ReportSubtotal>();
        private string[] completeHeader = new string[11];
        private double numTrees = 0.0;
        private double estTrees = 0.0;
        private double currTipwood = 0.0;
        private double sumExpanFactor = 0.0;
        private double proratFac = 0.0;

        private long currSTcn;
        private long currCUcn;

        #endregion


         /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void OutputTipwoodReport(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            string currentTitle = fillReportTitle(currentReport);
            List<StratumDO> sList = Global.BL.getStratum().ToList();
            List<PRODO> proList = Global.BL.getPRO().ToList();
            // string orderBy = "";
            // reset summation variables
           
            fieldLengths = new[] { 6, 8, 10, 12, 13 };
            numTrees = 0.0;
            estTrees = 0.0;
            currTipwood = 0.0;
            unitSubtotal.Clear();
            strataSubtotal.Clear();

            numOlines = 0;
 
            summaryList.Clear();
                    // needs to run by cutting unit
            foreach (CuttingUnitDO c in Global.BL.getCuttingUnits())
            {
                c.Strata.Populate();
                currCUcn = (long)c.CuttingUnit_CN;
                        //  Create report title
                rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
  
                //  Load and print data for current cutting unit
                LoadAndPrintProrated(strWriteOut, c, rh, ref pageNumb, summaryList, proList);
                if (unitSubtotal.Count > 0)
                     OutputUnitSubtotal(strWriteOut, ref pageNumb, rh, currentReport);
                unitSubtotal.Clear();
            }   //  end foreach loop
                    //  output Subtotal Summary and grand total for the report
            OutputSubtotalSummary(strWriteOut, rh, ref pageNumb, summaryList);
            OutputGrandTotal(strWriteOut, rh, currentReport, ref pageNumb);

            return;
        }   //  end OutputUnitReports


        private StratumDO findMethods(IEnumerable<StratumDO> sList)
        {
            //int nthRow = -1;
            return sList.FirstOrDefault(
                s => s.Method == "S3P" || s.Method == "F3P" || s.Method == "3P");

            //return nthRow;
        }   //  end findMethods


           private void OutputUnitSubtotal(StreamWriter strWriteOut, ref int pageNumb,
                                        reportHeaders rh, string currRPT)
        {
               //  WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
               //                     completeHeader, 13, ref pageNumb, "");
                 WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    rRH.R902columns, 10, ref pageNumb, "");
                    strWriteOut.Write("                  ");
                    strWriteOut.WriteLine(reportConstants.subtotalLine1);
                    strWriteOut.Write("  UNIT ");
                    strWriteOut.Write(unitSubtotal[0].Value1.PadLeft(3, ' '));
                    strWriteOut.Write(" TOTAL       ");
                    strWriteOut.Write(" {0,7:F0}", unitSubtotal[0].Value3);
                   // strWriteOut.Write("  {0,7:F0}", unitSubtotal[0].Value13);
                    strWriteOut.WriteLine("  {0,12:F0}", unitSubtotal[0].Value4);
                  strWriteOut.WriteLine(" ");
                    numOlines += 3;
             return;
        }   //  end OutputUnitSubtotal

         private void OutputGrandTotal(StreamWriter strWriteOut, reportHeaders rh, string currRPT, ref int pageNumb)
        {
            //WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
            //                        completeHeader, 13, ref pageNumb, "");
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                rRH.R902columns, 10, ref pageNumb, "");
            //  works for UC reports
            strWriteOut.WriteLine("");
                    strWriteOut.WriteLine("");
                    strWriteOut.Write(" OVERALL TOTALS-- ");
                    if (grandTotal.Count > 0)
                    {
                        strWriteOut.Write("{0,7:F0}", grandTotal[0].Value3);
                        strWriteOut.WriteLine("  {0,9:F0}", grandTotal[0].Value4);
                        numOlines += 3;
                    }   //  endif
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine("");

            grandTotal.Clear();
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                    rRH.R902columns, 10, ref pageNumb, "");
            //WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
            //                        completeHeader, 13, ref pageNumb, "");
            //  also output the footer
           // for (int k = 0; k < 5; k++)
           //     strWriteOut.WriteLine(rh.UCfooter[k]);
            return;
        }   //  end OutputGrandTotal


        private void OutputSubtotalSummary(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb,
                                            IEnumerable<ReportSubtotal> summaryList)
        {
            //  output summary headers
          //  WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
          //                      completeHeader, 13, ref pageNumb, "");
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                rRH.R902columns, 10, ref pageNumb, "");
            strWriteOut.WriteLine("                  _________________________________________________________________________________________________________________");
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine(" S U B T O T A L S    ");
            strWriteOut.WriteLine("                   TOT EST ");
            strWriteOut.WriteLine("                    # OF ");
            strWriteOut.WriteLine("          SPECIES   TREES      TIPWOOD");
            numOlines += 7;

            //  write out summary list
            foreach (ReportSubtotal rs in summaryList)
            {
                strWriteOut.Write("          ");
                strWriteOut.Write(rs.Value1.PadRight(6, ' '));
                strWriteOut.Write("  {0,7:F0}", rs.Value3);
                strWriteOut.WriteLine("  {0,9:F0}", rs.Value4);
                numOlines++;
            }   //  end foreach loop
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.WriteLine(reportConstants.longLine);
            numOlines += 2;
            return;
        }   //  end OutputSubtotalSummary



        private void finishColumnHeaders(string[] leftHandSide, string[] rightHandSide)
        {
            //  clean up completeHeader before loading again
            for (int j = 0; j < 11; j++)
                completeHeader[j] = null;
            //  load up complete header
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < rightHandSide.Count(); k++)
            {
                sb.Remove(0, sb.Length);
                sb.Append(leftHandSide[k]);
                sb.Append(rightHandSide[k]);
                completeHeader[k] = sb.ToString();
            }   //  end for loop
            return;
        }   //  end finishColumnHeaders


//////////////////////////////////////////////////////////////////////////
        private void LoadAndPrintProrated(StreamWriter strWriteOut, CuttingUnitDO cdo, reportHeaders rh, ref int pageNumb,
                                            List<ReportSubtotal> summaryList, IEnumerable<PRODO> proList)
        {
            //  overloaded to properly print UC5-UC6
            //  pull distinct species from measured trees in Tree to get species groups for each unit for UC5
            //  or sample groups for UC6
            ArrayList groupsToProcess = new ArrayList();
            groupsToProcess = Global.BL.GetJustSpecies("Tree");
            foreach (string gtp in groupsToProcess)
            {
                //  pull data based on strata method and species
                foreach (StratumDO stratum in cdo.Strata)
                {
                    currSTcn = (long)stratum.Stratum_CN;
                    switch (stratum.Method)
                    {
                        case "100":
                            //  find cut trees and species to process
                            IEnumerable<TreeCalculatedValuesDO> currentGroup = Global.BL.getTreeCalculatedValues((int)stratum.Stratum_CN, (int)cdo.CuttingUnit_CN).Where(
                                tdo => tdo.Tree.Species == gtp && tdo.Tree.SampleGroup.CutLeave == currCL);
                            if (currentGroup.Any()) SumUpGroups(currentGroup, proList);
                            break;
                        default:
                            //  otherwise data comes from LCD and is NOT expanded
                            IEnumerable<LCDDO> currGroup = Global.BL.getLCDOrdered("WHERE CutLeave = ? AND Stratum = ? ORDER BY ", "Species", currCL, stratum.Code)
                                .Where(delegate (LCDDO l) { return l.Species == gtp; });
                            if (currGroup.Any()) SumUpGroups(currGroup, cdo.Code, proList);
                            break;
                    }   //  end switch on method
                }   //  end for loop on strata
                if (numTrees > 0)
                {
                    //  Ready to print line
                    prtFields.Add("");
                    prtFields.Add(cdo.Code.PadLeft(3, ' '));
                    prtFields.Add(gtp.PadRight(6, ' '));

                    WriteCurrentGroup(strWriteOut, rh, ref pageNumb);
                    UpdateSubtotalSummary(gtp, summaryList);
                    UpdateUnitTotal(currentReport);
                    unitSubtotal[0].Value1 = cdo.Code;
                }   //  endif something to print -- numTrees is not zero
                prtFields.Clear();
                // reset summation variables
                numTrees = 0.0;
                estTrees = 0.0;
                currTipwood = 0.0;
                //  end foreach loop on species
            }
            return;
        }   //  end LoadAndPrintProrated


        private void UpdateUnitTotal(string currRPT)
        {
            //  Works for UC reports
            //  subtotals are not prorated as the values are prorated when each line is printed
            if (unitSubtotal.Count > 0)
            {
                unitSubtotal[0].Value3 += numTrees;
                unitSubtotal[0].Value4 += currTipwood;
               // unitSubtotal[0].Value13 += estTrees;
            }
            else
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value3 += numTrees;
                rs.Value4 += currTipwood;
               // rs.Value13 += estTrees;
                unitSubtotal.Add(rs);
            }   //  endif
            return;
        }   //  end UpdateUnitTotal

        private void UpdateStrataTotal(string currRPT)
        {
            //  Works for UC reports
            //  subtotals not prorated as the values are prorated when each line is printed
            if (strataSubtotal.Count > 0)
            {
                strataSubtotal[0].Value3 += numTrees;
                strataSubtotal[0].Value4 += currTipwood;
            }
            else
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value3 += numTrees;
                rs.Value4 += currTipwood;
                strataSubtotal.Add(rs);
            }   //  endif

            //  updates grand total as well
            if (grandTotal.Count > 0)
            {
                grandTotal[0].Value3 += numTrees;
                grandTotal[0].Value4 += currTipwood;
            }
            else
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value3 += numTrees;
                rs.Value4 += currTipwood;

                grandTotal.Add(rs);
            }   //  endif

            return;
        }   //  end UpdateStrataTotal

//      /////////////////////////////////////////////////////////////////////////////////
        private void UpdateSubtotalSummary(string currSP, List<ReportSubtotal> summaryList)
        {
            //  used by UC5-UC6 only for summary at end of report
            //  see if current species is in the list
            ReportSubtotal rs = summaryList.FirstOrDefault(r => r.Value1 == currSP);
            if (rs != null)
            {
                rs.Value3 += numTrees;
                rs.Value4 += currTipwood;
                //summaryList[nthRow].Value13 += estTrees;
            }
            else            //  species not in list so add it
            {
                rs = new ReportSubtotal();
                rs.Value1 = currSP;
                rs.Value3 += numTrees;
                rs.Value4 += currTipwood;
                //rs.Value13 += estTrees;
                summaryList.Add(rs);

            }   //  endif


            //  updates grand total as well
            if (grandTotal.Count > 0)
            {
                grandTotal[0].Value3 += numTrees;
                grandTotal[0].Value4 += currTipwood;
                //grandTotal[0].Value13 += estTrees;
            }
            else
            {
                rs = new ReportSubtotal();
                rs.Value3 += numTrees;
                rs.Value4 += currTipwood;
                //rs.Value13 += estTrees;
                grandTotal.Add(rs);
            }   //  endif
            return;
        }   //  end UpdateSubtotalSummary


        private void WriteCurrentGroup(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  overloaded for UC reports
            string fieldFormat3 = "{0,9:F0}";
            string fieldFormat5 = "{0,7:F0}";

            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                rRH.R902columns, 10, ref pageNumb, "");


            //  Expansion factor is first
            prtFields.Add(Utilities.FormatField(numTrees, fieldFormat5));
             //  This is estimated number of trees -- all methods except 3P
             //if (currTipwood == 0.0)
             //       prtFields.Add("      0");
            // else prtFields.Add(Utilities.FormatField(estTrees, fieldFormat5));
            //  load volumes

            prtFields.Add(Utilities.FormatField(currTipwood, fieldFormat3));

            printOneRecord(fieldLengths, prtFields, strWriteOut);
            return;
        }   //  end WriteCurrentGroup


        private void SumUpGroups(IEnumerable<TreeCalculatedValuesDO> tList, IEnumerable<PRODO> proList)
        {
            //  overloaded for 100% method
            foreach (TreeCalculatedValuesDO t in tList)
            {
                //  separate sawtimber from non-saw
                if (t.Tree.SampleGroup.UOM != "05")
                {
                    numTrees += t.Tree.ExpansionFactor;
                    currTipwood += t.TipwoodVolume * t.Tree.ExpansionFactor;
 
                 //  lookup proration factor for this group
                    PRODO prodo = proList.FirstOrDefault(
                        p => p.CutLeave == "C" && p.Stratum == t.Tree.Stratum.Code &&
                                p.CuttingUnit == t.Tree.CuttingUnit.Code &&
                                p.SampleGroup == t.Tree.SampleGroup.Code &&
                                p.PrimaryProduct == t.Tree.SampleGroup.PrimaryProduct &&
                                p.UOM == t.Tree.SampleGroup.UOM);
                    if (prodo != null) proratFac = prodo.ProrationFactor;
                }   // endif on unit of measure
            }   //  end foreach loop on trees

            return;
        }   //  end SumUpGroups for 100% method


        private void SumUpGroups(IEnumerable<LCDDO> lcdList, string currCU, IEnumerable<PRODO> proList)
        {
            //  overloaded for all other methods using LCD
            foreach (LCDDO l in lcdList)
            {
                //  lookup proration factor for this group
                PRODO prodo = proList.FirstOrDefault(
                    p => p.CutLeave == currCL && p.Stratum == l.Stratum &&
                            p.CuttingUnit == currCU &&
                            p.SampleGroup == l.SampleGroup &&
                            p.PrimaryProduct == l.PrimaryProduct &&
                            p.UOM == l.UOM && p.STM == l.STM);
                if (prodo != null)
                    proratFac = prodo.ProrationFactor;

                // separate sawtimber from nonsaw
                if (l.UOM != "05")
                {
                    //  check on method since S3P and 3P use tallied trees instead of expansion factor
                    if (Utilities.MethodLookup(l.Stratum) == "3P" ||
                        Utilities.MethodLookup(l.Stratum) == "S3P")
                    {
                        numTrees += pull3PtallyTrees(proList, lcdList, l.SampleGroup,
                                                l.Species, l.Stratum, l.PrimaryProduct,
                                                l.LiveDead, l.STM, currCU);
                        estTrees = 0.0;
                    }
                    else if (Utilities.MethodLookup(l.Stratum) == "3PPNT")
                    {
                        numTrees += l.SumExpanFactor * proratFac;
                        estTrees = 0.0;
                    }
                    else
                    {
                        numTrees += l.SumExpanFactor * proratFac;
                        if (l.PrimaryProduct == "01" && (l.SumNBDFT > 0 || l.SumNCUFT > 0))
                            estTrees += l.SumExpanFactor * proratFac;
                    }   //  endif
                        //  Sum up STM trees separately as what's in the current cutting unit stays in that unit
                    if (l.STM == "Y")
                        //  calls capture method to sum expanded volume for each STM tree in the cutting unit
                        getSTMtrees(currSTcn, currCUcn, l.SampleGroup, l.STM, ref currTipwood, proratFac);
                    else
                    {
                        currTipwood += l.SumTipwood * proratFac;

                    }   //  endif sure to measure
                }   //  endif on unit of measure
            }   //  end foreach loop on LCD
            return;
        }   //  end SumUpGroups



        private double pull3PtallyTrees(IEnumerable<PRODO> proList, IEnumerable<LCDDO> lcdList, string currSG,
                                        string currSP, string currST, string currPP, string currLD,
                                        string currSTM, string currCU)
        {
            double talliedTrees = 0;
            //  Are there multiple species in the current sample group
            //  for just non-STM first
            //  need entire LCD list for UC5-6
           //lcdList = Global.BL.getLCD();
           
            if (lcdList.Where(
                l => l.Stratum == currST && l.SampleGroup == currSG && l.STM == "N").Count() > 1)
            {
                //  find number of tallied trees in LCD for current species
                LCDDO lcddo = lcdList.FirstOrDefault(
                    l => l.Stratum == currST && l.Species == currSP &&
                            l.SampleGroup == currSG && l.PrimaryProduct == currPP &&
                            l.LiveDead == currLD && l.STM == currSTM);
                if (lcddo != null)
                    talliedTrees += lcddo.TalliedTrees;
            }
            else
            {
                //  that means only one species per sample group
                //  so return number of tallied trees from the PRO table
                PRODO prodo = proList.FirstOrDefault(
                    p => p.Stratum == currST && p.SampleGroup == currSG &&
                            p.CuttingUnit == currCU && p.PrimaryProduct == currPP &&
                            p.STM == currSTM);
                if (prodo != null)
                    talliedTrees += prodo.TalliedTrees;
            }   //  endif

            return talliedTrees;
        }   //  end pull3PtallyTrees

        public void getSTMtrees(long currSTcn, long currCUcn, string currSG, string currSTM, ref double Tipwoodsum,
                                     double currProFac)
        {
            //  retrieve calc values for current stratum
            //  Find all STM trees in the current cutting unit
            //  expand and sum up
            foreach (TreeCalculatedValuesDO js in Global.BL.getTreeCalculatedValues((int)currSTcn, (int)currCUcn).Where(
                tcv => tcv.Tree.SampleGroup.Code == currSG && tcv.Tree.STM == currSTM))
            {
                Tipwoodsum += js.TipwoodVolume * js.Tree.ExpansionFactor * currProFac;
            }   //  end foreach loop

            //return;
        }   //  end getSTMtrees
            
        
        /*      public void OutputTipwoodReport(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
           {
               string currentTitle = fillReportTitle(currentReport);
               fieldLengths = new [] {6, 8, 10, 12, 13};
               List<TreeDO> tList = Global.BL.getTrees();
               List<TreeCalculatedValuesDO> tcvList = Global.BL.getTreeCalculatedValues();
               List<CuttingUnitDO> cuList = Global.BL.getCuttingUnits();

               foreach (CuttingUnitDO cul in cuList)
               {
                   double estTrees = 0;
                   double tipWoodVol = 0;
                   sumEstTrees = 0;
                   sumTipwood = 0;
                   string prevSP = "**";
                   cul.Strata.Populate();
                  //pull each stratum from LCD table
                   foreach (StratumDO sd in cul.Strata)
                   //for (int k = 0; k < cul.Strata.Count; k++)
                   {
                       List<LCDDO> justStrata = Global.BL.getLCDOrdered("WHERE CutLeave = ? AND Stratum = ? ORDER BY SPECIES",
                                                                           "", "C", sd.Code);
                       double STacres = Utilities.ReturnCorrectAcres(sd.Code, (long)sd.Stratum_CN);
                       //  pull tree data and sum
                       foreach (LCDDO ju in justStrata)
                       {
                           if (prevSP != ju.Species)
                           {

                               List<TreeDO> currentTrees = tList.FindAll(
                                   delegate (TreeDO t)
                                       {
                                           return t.Species == ju.Species && t.CountOrMeasure == "M";
                                       });
                               estTrees = currentTrees.Sum(tdo => tdo.ExpansionFactor);
                               foreach (TreeDO ct in currentTrees)
                               {
                                   int nthRow = tcvList.FindIndex(
                                       delegate (TreeCalculatedValuesDO tcv)
                                       {
                                           return tcv.Tree_CN == ct.Tree_CN;

                                       });
                                   //
                                   tipWoodVol += tcvList[nthRow].TipwoodVolume * ct.ExpansionFactor * STacres;
                                   //
                               }   //  end foreach  loopl
                                prevSP = ju.Species;
                              // end foreach loop
                               RegionalReports rr = new RegionalReports();
                               rr.value1 = cul.Code;
                               rr.value2 = ju.Species;
                               rr.value7 = estTrees;
                               rr.value8 = tipWoodVol;
                               listToOutput.Add(rr);

                               sumEstTrees += estTrees;
                               sumTipwood += tipWoodVol;
                               tipWoodVol = 0;
                               estTrees = 0;
                           }
                       }   //  end for k loop
                   }   //  end foreach loop
                   WriteCurrentGroup(strWriteOut, ref pageNumb, rh);
                   WriteSummaryGroup(strWriteOut, ref pageNumb, rh);
               }

               return;
               //  end OutputTipwoodReport

           }   //  end OutputR9

           private void WriteCurrentGroup(StreamWriter strWriteOut, ref int pageNum, reportHeaders rh)
           {
               //  works for R902
               foreach (RegionalReports lto in listToOutput)
               {
                   WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                       rRH.R902columns, 10, ref pageNum, "");

                   prtFields.Clear();
                   prtFields.Add("");
                   prtFields.Add(lto.value1.PadRight(4, ' '));
                   prtFields.Add(lto.value2.PadRight(6, ' '));

                   //  est trees and tipwood volume
                   prtFields.Add(Utilities.FormatField(lto.value7, "{0,10:F0}").ToString().PadLeft(10, ' '));
                   prtFields.Add(Utilities.FormatField(lto.value8, "{0,10:F0}").ToString().PadLeft(10, ' '));
                   printOneRecord(fieldLengths, prtFields, strWriteOut);
               }   //  end foreach loop
               return;
           }   //  end WriteCurrentGroup

           private void WriteSummaryGroup(StreamWriter strWriteOut, ref int pageNum, reportHeaders rh)
           {
               WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                   rRH.R902columns, 10, ref pageNum, "");
               // print solid line
               // print group summary
               // print blank line
               //  est trees and tipwood volume
               strWriteOut.WriteLine("_________________________________________________________________________________________________________");
               prtFields.Clear();
               prtFields.Add(Utilities.FormatField(sumEstTrees, "{0,10:F0}").ToString().PadLeft(34, ' '));
               prtFields.Add(Utilities.FormatField(sumTipwood, "{0,10:F0}").ToString().PadLeft(12, ' '));
               printOneRecord(fieldLengths, prtFields, strWriteOut);

           }
           */

    }


}
