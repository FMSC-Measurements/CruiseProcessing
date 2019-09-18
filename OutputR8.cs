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
    public class OutputR8 : CreateTextFile
    {
        #region
        public string currentReport;
        private int[] fieldLengths;
        private ArrayList prtFields = new ArrayList();
        List<RegionalReports> listToOutput = new List<RegionalReports>();
        List<RegionalReports> totalToOutput = new List<RegionalReports>();
        List<RegionalReports> reportSummary = new List<RegionalReports>();
        private regionalReportHeaders rRH = new regionalReportHeaders();
        private string[] completeHeader;
        private double ccfFactor = 100.0;
        private double mbfFactor = 1000.0;
        private double currGRSbdft = 0;
        private double currGRScuft = 0;
        private double pineTop = 0;
        private double hardwoodTop = 0;
        #endregion

        public void CreateR8Reports(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  fill report title array
            string currentTitle = fillReportTitle(currentReport);

            // grad LCD list to see if there's volume for the report
            List<LCDDO> lcdList = Global.BL.getLCD().ToList();
            currGRSbdft = lcdList.Sum(l => l.SumGBDFT);
            currGRScuft = lcdList.Sum(l => l.SumGCUFT);
            if (currGRSbdft == 0 && currGRScuft == 0)
            {
                 noDataForReport(strWriteOut, currentReport, " >>>>> No volume for this report.");
                 return;
            }   // endif

            //  process reports
            switch (currentReport)
            {
                case "R801":
                    //  total sale acres
                    double totalAcres = Global.BL.getCuttingUnits().Sum(c => c.Area);
                    //  create complete header
                    completeHeader = createCompleteHeader(totalAcres);
                    rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.B2DC, reportConstants.FCTO);
                    fieldLengths = new int[] { 1, 6, 8, 9, 9, 7, 9, 10, 8, 9, 8, 13, 7, 11, 7, 7 };
                    //  Load DIB classes
                    LoadDIBclasses();
                    //  Load summary at bottom with row lines
                    for (int j = 0; j < 7; j++)
                    {
                        RegionalReports r = new RegionalReports();
                        r.value1 = rRH.R801lines[j];
                        reportSummary.Add(r);
                    }   //  end for j loop

                    //  Uses cut and measured trees only
                    AccumulateVolume(Global.BL.getTreeCalculatedValues().Where(
                        tcv => tcv.Tree.SampleGroup.CutLeave == "C" && tcv.Tree.CountOrMeasure == "M"));
                    WriteReport(strWriteOut, ref pageNumb, rh, totalAcres);
                    break;
                case "R802":
                    rh.createReportTitle(currentTitle, 6, 0, 0, "BY SAMPLE GROUP, SPECIES, AND PRODUCT", reportConstants.FCTO);
                    numOlines = 0;
                    //  processed by group
                    IEnumerable<SampleGroupDO> justGroups = Global.BL.getSampleGroups("WHERE CutLeave = ? GROUP BY Code");
                    OutputSawtimber(strWriteOut, ref pageNumb, rh, justGroups, lcdList);
                    numOlines = 0;
                    OutputProduct8(strWriteOut, ref pageNumb, rh, justGroups, lcdList);
                    numOlines = 0;
                    OutputPulpwood(strWriteOut, ref pageNumb, rh, justGroups, lcdList);
                    break;
            }   //  end switch on currentReport
            return;
        }   //  end CreateR8Reports


        private void AccumulateVolume(IEnumerable<TreeCalculatedValuesDO> justTrees)
        {
            //  R801
            string prevST = "*";
            double currAC = 0;
            //  process trees into the correct bucket
            foreach (TreeCalculatedValuesDO jt in justTrees)
            {
                //  get acres for stratum change
                if (prevST != jt.Tree.Stratum.Code)
                {
                    currAC = Utilities.ReturnCorrectAcres(jt.Tree.Stratum.Code, (long)jt.Tree.Stratum_CN);
                    prevST = jt.Tree.Stratum.Code;
                }   //  endif previous stratum

                //  find row to load using dbh
                int rowToLoad = findDBHindex(jt.Tree.DBH);

                //  need to convert species code to a number for finding less than and greater than
                int speciesCode = Convert.ToInt16(jt.Tree.Species);
                //  load volumes
                if (speciesCode < 300)
                {
                    //  softwood
                    switch (jt.Tree.SampleGroup.PrimaryProduct)
                    {
                        case "01":      // sawtimber
                            listToOutput[rowToLoad].value7 += jt.Tree.ExpansionFactor * currAC;
                            listToOutput[rowToLoad].value8 += jt.NetBDFTPP * jt.Tree.ExpansionFactor * currAC;
                            listToOutput[rowToLoad].value9 += jt.NetCUFTPP * jt.Tree.ExpansionFactor * currAC;
                            if (jt.NetBDFTPP > 0 || jt.NetCUFTPP > 0)
                            {
                                //  load summary values for the bottom of the report
                                reportSummary[0].value9 += jt.Tree.DBH * jt.Tree.ExpansionFactor * currAC;
                                reportSummary[0].value10 += jt.Tree.MerchHeightPrimary * jt.Tree.ExpansionFactor * currAC;
                                reportSummary[0].value11 += jt.Tree.UpperStemHeight * jt.Tree.ExpansionFactor * currAC;
                                //  subtotal sums
                                pineTop += jt.NetCUFTSP * jt.Tree.ExpansionFactor * currAC;
                            }   //  endif
                            break;
                        case "02":      //  pulpwood
                            listToOutput[rowToLoad].value17 += jt.Tree.ExpansionFactor * currAC;
                            listToOutput[rowToLoad].value18 += jt.NetCUFTPP * jt.Tree.ExpansionFactor * currAC;
                            double testCalc = jt.NetCUFTPP * jt.Tree.ExpansionFactor * currAC;
                            //  load summary values
                            reportSummary[4].value9 += jt.Tree.DBH * jt.Tree.ExpansionFactor * currAC;
                            reportSummary[4].value10 += jt.Tree.MerchHeightPrimary * jt.Tree.ExpansionFactor * currAC;
                            reportSummary[4].value11 += jt.Tree.UpperStemHeight * jt.Tree.ExpansionFactor * currAC;
                            break;
                        case "08":      //  product 08 only
                            listToOutput[rowToLoad].value13 += jt.Tree.ExpansionFactor * currAC;
                            listToOutput[rowToLoad].value14 += jt.NetCUFTPP * jt.Tree.ExpansionFactor * currAC;
                            //  load summary values
                            reportSummary[2].value9 += jt.Tree.DBH * jt.Tree.ExpansionFactor * currAC;
                            reportSummary[2].value10 += jt.Tree.MerchHeightPrimary * jt.Tree.ExpansionFactor * currAC;
                            reportSummary[2].value11 += jt.Tree.UpperStemHeight * jt.Tree.ExpansionFactor * currAC;
                            //  subtotal sums
                            pineTop += jt.NetCUFTSP * jt.Tree.ExpansionFactor * currAC;
                            break;
                    }   // end switch on primary product
                }
                else if(speciesCode >= 300)
                {
                    //  hardwood
                    switch (jt.Tree.SampleGroup.PrimaryProduct)
                    {
                        case "01":
                            listToOutput[rowToLoad].value10 += jt.Tree.ExpansionFactor * currAC;
                            listToOutput[rowToLoad].value11 += jt.NetBDFTPP * jt.Tree.ExpansionFactor * currAC;
                            listToOutput[rowToLoad].value12 += jt.NetCUFTPP * jt.Tree.ExpansionFactor * currAC;
                            //  load summary values for the bottom of the report
                            if (jt.NetBDFTPP > 0 || jt.NetCUFTPP > 0)
                            {
                                reportSummary[1].value9 += jt.Tree.DBH * jt.Tree.ExpansionFactor * currAC;
                                reportSummary[1].value10 += jt.Tree.MerchHeightPrimary * jt.Tree.ExpansionFactor * currAC;
                                reportSummary[1].value11 += jt.Tree.UpperStemHeight * jt.Tree.ExpansionFactor * currAC;
                                //  subtotal sums
                                hardwoodTop += jt.NetCUFTSP * jt.Tree.ExpansionFactor * currAC;
                            }   //  endif
                            break;
                        case "02":
                            listToOutput[rowToLoad].value19 += jt.Tree.ExpansionFactor * currAC;
                            listToOutput[rowToLoad].value20 += jt.NetCUFTPP * jt.Tree.ExpansionFactor * currAC;
                            //  load summary values
                            reportSummary[5].value9 += jt.Tree.DBH * jt.Tree.ExpansionFactor * currAC;
                            reportSummary[5].value10 += jt.Tree.MerchHeightPrimary * jt.Tree.ExpansionFactor * currAC;
                            reportSummary[5].value11 += jt.Tree.UpperStemHeight * jt.Tree.ExpansionFactor * currAC;
                            break;
                        case "08":
                            listToOutput[rowToLoad].value15 += jt.Tree.ExpansionFactor * currAC;
                            listToOutput[rowToLoad].value16 += jt.NetCUFTPP * jt.Tree.ExpansionFactor * currAC;
                            //  load summary values
                            reportSummary[3].value9 += jt.Tree.DBH * jt.Tree.ExpansionFactor * currAC;
                            reportSummary[3].value10 += jt.Tree.MerchHeightPrimary * jt.Tree.ExpansionFactor * currAC;
                            reportSummary[3].value11 += jt.Tree.UpperStemHeight * jt.Tree.ExpansionFactor * currAC;
                            //  subtotal sums
                            hardwoodTop += jt.NetCUFTSP * jt.Tree.ExpansionFactor * currAC;
                            break;
                    }   //  end switch on primary product
                }   //  endif on species
            }   //  end foreach loop on justTrees
            return;
        }   //  end AccumulateVolume


        private void WriteReport(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh, double totalAcres)
        {
            //  R801
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                        completeHeader, 14, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(lto.value1.PadLeft(2, ' '));
                prtFields.Add(Utilities.FormatField(lto.value7, "{0,6:F0}").ToString().PadLeft(6, ' '));
                prtFields.Add(Utilities.FormatField(lto.value8 / mbfFactor, "{0,8:F3}").ToString().PadLeft(8, ' '));
                prtFields.Add(Utilities.FormatField(lto.value9 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));
                prtFields.Add(Utilities.FormatField(lto.value10, "{0,6:F0}").ToString().PadLeft(6, ' '));
                prtFields.Add(Utilities.FormatField(lto.value11 / mbfFactor, "{0,8:F3}").ToString().PadLeft(8, ' '));
                prtFields.Add(Utilities.FormatField(lto.value12 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));
                prtFields.Add(Utilities.FormatField(lto.value13, "{0,6:F0}").ToString().PadLeft(6, ' '));
                prtFields.Add(Utilities.FormatField(lto.value14 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));
                prtFields.Add(Utilities.FormatField(lto.value15, "{0,6:F0}").ToString().PadLeft(6, ' '));
                prtFields.Add(Utilities.FormatField(lto.value16 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));
                prtFields.Add(Utilities.FormatField(lto.value17, "{0,6:F0}").ToString().PadLeft(6, ' '));
                prtFields.Add(Utilities.FormatField(lto.value18 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));
                prtFields.Add(Utilities.FormatField(lto.value19, "{0,6:F0}").ToString().PadLeft(6, ' '));
                prtFields.Add(Utilities.FormatField(lto.value20 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));
                //  print record
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach on listToOutput
            //  print total line
            strWriteOut.Write("TOTAL: ");
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value7), "{0,6:F0}").ToString().PadLeft(6, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value8) / mbfFactor, "{0,8:F3}").ToString().PadLeft(10, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value9) / ccfFactor, "{0,7:F2}").ToString().PadLeft(8, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value10), "{0,6:F0}").ToString().PadLeft(8, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value11) / mbfFactor, "{0,8:F3}").ToString().PadLeft(9, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value12) / ccfFactor, "{0,7:F2}").ToString().PadLeft(8, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value13), "{0,6:F0}").ToString().PadLeft(9, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value14) / ccfFactor, "{0,7:F2}").ToString().PadLeft(9, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value15), "{0,6:F0}").ToString().PadLeft(8, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value16) / ccfFactor, "{0,7:F2}").ToString().PadLeft(9, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value17), "{0,6:F0}").ToString().PadLeft(12, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value18) / ccfFactor, "{0,7:F2}").ToString().PadLeft(8, ' '));
            strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value19), "{0,6:F0}").ToString().PadLeft(10, ' '));
            strWriteOut.WriteLine(Utilities.FormatField(listToOutput.Sum(l => l.value20) / ccfFactor, "{0,7:F2}").ToString().PadLeft(8, ' '));

            //  output subtotal lines
            strWriteOut.Write(rRH.R801subtotal[0]);
            strWriteOut.Write(Utilities.FormatField(pineTop / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));
            strWriteOut.WriteLine(Utilities.FormatField(hardwoodTop / ccfFactor, "{0,7:F2}").ToString().PadLeft(18, ' '));

            strWriteOut.Write(rRH.R801subtotal[1]);
            double calcValue = 0;
            calcValue = listToOutput.Sum(l => l.value18);
            calcValue += pineTop;
            strWriteOut.Write(Utilities.FormatField(calcValue / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));
            calcValue = listToOutput.Sum(l => l.value20);
            calcValue += hardwoodTop;
            strWriteOut.WriteLine(Utilities.FormatField(calcValue / ccfFactor, "{0,7:F2}").ToString().PadLeft(18, ' '));

            //  output summary table
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                completeHeader, 14, ref pageNumb, "");
            strWriteOut.WriteLine("");
            for (int h = 0; h < 3; h++)
            {
                strWriteOut.WriteLine(rRH.R801summary[h]);
            }   //  end for loop
            double calcTrees = 0;
            double boardFoot = 0;
            double cubicFoot = 0;
            double totCalcTrees = 0;
            double totBoardFoot = 0;
            double totCubicFoot = 0;
            double totDBH = 0;
            double totHt1 = 0;
            double totHt2 = 0;
            int rowToPrint = 0;
            foreach (RegionalReports rs in reportSummary)
            {
                switch (rowToPrint)
                {
                    case 0:         //  pine sawtimber
                        calcTrees = listToOutput.Sum(l => l.value7);
                        boardFoot = listToOutput.Sum(l => l.value8);
                        cubicFoot = listToOutput.Sum(l => l.value9);
                        totCalcTrees += calcTrees;
                        totBoardFoot += boardFoot;
                        totCubicFoot += cubicFoot;
                        break;
                    case 1:         //  hardwood sawtimber
                        calcTrees = listToOutput.Sum(l => l.value10);
                        boardFoot = listToOutput.Sum(l => l.value11);
                        cubicFoot = listToOutput.Sum(l => l.value12);
                        totCalcTrees += calcTrees;
                        totBoardFoot += boardFoot;
                        totCubicFoot += cubicFoot;
                        break;
                    case 2:         //  pine products
                        calcTrees = listToOutput.Sum(l => l.value13);
                        cubicFoot = listToOutput.Sum(l => l.value14);
                        totCalcTrees += calcTrees;
                        totCubicFoot += cubicFoot;
                        break;
                    case 3:         //  hardwood products
                        calcTrees = listToOutput.Sum(l => l.value15);
                        cubicFoot = listToOutput.Sum(l => l.value16);
                        totCalcTrees += calcTrees;
                        totCubicFoot += cubicFoot;
                        break;
                    case 4:         //  pine pulpwood
                        calcTrees = listToOutput.Sum(l => l.value17);
                        cubicFoot = listToOutput.Sum(l => l.value18);
                        totCalcTrees += calcTrees;
                        totCubicFoot += cubicFoot;
                        break;
                    case 5:         //  hardwood products
                        calcTrees = listToOutput.Sum(l => l.value19);
                        cubicFoot = listToOutput.Sum(l => l.value20);
                        totCalcTrees += calcTrees;
                        totCubicFoot += cubicFoot;
                        break;
                }   //  end switch on rowToPrint

                strWriteOut.Write(rs.value1);
                switch (rowToPrint)
                {
                    case 0:         case 1:
                        calcValue = 0.0;
                        if (calcTrees > 0)
                            calcValue = boardFoot / calcTrees;
                        if (calcValue < 0) calcValue = 0.0;
                        strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));
                        calcValue = 0.0;
                        if (calcTrees > 0)
                            calcValue = cubicFoot / calcTrees;
                        if (calcValue < 0) calcValue = 0.0;
                        strWriteOut.Write(Utilities.FormatField(calcValue, "{0,7:F1}").ToString().PadLeft(10, ' '));
                        calcValue = 0.0;
                        if (calcTrees > 0)
                        {
                            strWriteOut.Write(Utilities.FormatField(rs.value9 / calcTrees, "{0,6:F1}").ToString().PadLeft(6, ' '));
                            strWriteOut.Write(Utilities.FormatField(rs.value10 / calcTrees, "{0,5:F1}").ToString().PadLeft(8, ' '));
                            strWriteOut.Write(Utilities.FormatField(rs.value11 / calcTrees, "{0,5:f1}").ToString().PadLeft(11, ' '));
                            totDBH += rs.value9;
                            totHt1 += rs.value10;
                            totHt2 += rs.value11;
                        }   //  endif calcTrees
                        //  per acre values
                        strWriteOut.Write(Utilities.FormatField(calcTrees / totalAcres, "{0,6:F1}").ToString().PadLeft(16, ' '));
                        strWriteOut.Write(Utilities.FormatField((boardFoot / mbfFactor) / totalAcres, "{0,6:F2}").ToString().PadLeft(8, ' '));
                        strWriteOut.WriteLine(Utilities.FormatField((cubicFoot / ccfFactor) / totalAcres, "{0,6:F2}").ToString().PadLeft(9, ' '));
                        break;
                    case 2:     case 3:     case 4:     case 5:
                        strWriteOut.Write("           ");
                        if (calcTrees > 0)
                            calcValue = cubicFoot / calcTrees;
                        if (calcValue < 0.0) calcValue = 0.0;
                        strWriteOut.Write(Utilities.FormatField(calcValue, "{0,7:F1}").ToString().PadLeft(7, ' '));
                        if (calcTrees > 0)
                        {
                            strWriteOut.Write(Utilities.FormatField(rs.value9 / calcTrees, "{0,5:F1}").ToString().PadLeft(6, ' '));
                            strWriteOut.Write("              ");
                            strWriteOut.Write(Utilities.FormatField(rs.value11 / calcTrees, "{0,5:f1}").ToString().PadLeft(5, ' '));
                            totDBH += rs.value9;
                            totHt1 += rs.value10;
                            totHt2 += rs.value11;
                        }
                        else if (calcTrees == 0.0)
                        {
                            strWriteOut.Write("   0.0");
                            strWriteOut.Write("             ");
                            strWriteOut.Write("   0.0");

                        }   //  endif calcTrees
                        //  per acre values
                        strWriteOut.Write(Utilities.FormatField(calcTrees / totalAcres, "{0,6:F1}").ToString().PadLeft(16, ' '));
                        strWriteOut.Write("           ");
                        if (rowToPrint == 4 || rowToPrint == 5)
                        {
                            strWriteOut.Write(Utilities.FormatField((cubicFoot / ccfFactor) / totalAcres, "{0,6:F2}").ToString().PadLeft(6, ' '));
                            strWriteOut.WriteLine(" (W/TOP)");
                        }
                        else
                        {
                            strWriteOut.WriteLine(Utilities.FormatField((cubicFoot / ccfFactor) / totalAcres, "{0,6:F2}").ToString().PadLeft(6, ' '));
                        }   // endif
                        break;
                    case 6:             //  total line
                        strWriteOut.Write(Utilities.FormatField(totBoardFoot / totCalcTrees, "{0,8:F0}").ToString().PadLeft(8, ' '));
                        strWriteOut.Write(Utilities.FormatField(totCubicFoot / totCalcTrees, "{0,7:F1}").ToString().PadLeft(10, ' '));
                        strWriteOut.Write(Utilities.FormatField(totDBH / totCalcTrees, "{0,6:F1}").ToString().PadLeft(6, ' '));
                        strWriteOut.Write(Utilities.FormatField(totHt1 / totCalcTrees, "{0,5:F1}").ToString().PadLeft(8, ' '));
                        strWriteOut.Write(Utilities.FormatField(totHt2 / totCalcTrees, "{0,5:F1}").ToString().PadLeft(11, ' '));
                        strWriteOut.Write(Utilities.FormatField(totCalcTrees / totalAcres, "{0,6:F1}").ToString().PadLeft(16, ' '));
                        strWriteOut.Write(Utilities.FormatField((totBoardFoot / mbfFactor) / totalAcres, "{0,6:F2}").ToString().PadLeft(8, ' '));
                        strWriteOut.WriteLine(Utilities.FormatField((totCubicFoot / ccfFactor) / totalAcres, "{0,6:F2}").ToString().PadLeft(9, ' '));
                        break;
                }   //  end switch on rowToPrint
                rowToPrint++;
            }   //  end foreach loop on reportSummary
            return;
        }   //  end WriteReport


        private void OutputSawtimber(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh, 
                                        IEnumerable<SampleGroupDO> justGroups, IEnumerable<LCDDO> lcdList)
        {
            //  R802    --  sawtimber page
            RegionalReports r1 = new RegionalReports();
            r1.value1 = "                                SAWTIMBER TOTAL:   ";
            totalToOutput.Add(r1);

            //  process by sample group
            foreach (SampleGroupDO jg in justGroups)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                           rRH.R802sawtimber, 14, ref pageNumb, "");

                //  accumulate data by species
                string prevST = "*";
                double currAC = 0;
                List<StratumDO> sList = Global.BL.getStratum().ToList();
                foreach (LCDDO js in lcdList.Where(
                    l => l.CutLeave == "C" && l.PrimaryProduct == "01" && l.SampleGroup == jg.Code))
                {
                    //  find species in listToOutput or add it
                    int nthRow = listToOutput.FindIndex(
                        delegate(RegionalReports l)
                        {
                            return l.value1 == js.Species;
                        });
                    //  find strata acres as needed
                    if (prevST != js.Stratum)
                    {
                        int mthRow = sList.FindIndex(
                            delegate(StratumDO s)
                            {
                                return s.Code == js.Stratum;
                            });
                        currAC = Utilities.ReturnCorrectAcres(js.Stratum, (long)sList[mthRow].Stratum_CN);
                        prevST = js.Stratum;
                    }   //  endif

                    if (nthRow == -1)
                    {
                        // add species
                        RegionalReports r = new RegionalReports();
                        r.value1 = js.Species;
                        r.value7 = js.SumExpanFactor * currAC;
                        r.value8 = js.SumNBDFT * currAC;
                        r.value9 = js.SumNCUFT * currAC;
                        r.value10 = js.SumNCUFTtop * currAC;
                        listToOutput.Add(r);
                    }
                    else if (nthRow >= 0)
                    {
                        //  update listToOutput
                        listToOutput[nthRow].value7 += js.SumExpanFactor * currAC;
                        listToOutput[nthRow].value8 += js.SumNBDFT * currAC;
                        listToOutput[nthRow].value9 += js.SumNCUFT * currAC;
                        listToOutput[nthRow].value10 += js.SumNCUFTtop * currAC;
                    }   //  endif nthRow
                }   //  end foreach on justSpecies

                // output sample group/species and total line if there is data to print
                if(listToOutput.Sum(l=>l.value7) > 0)
                {
                    fieldLengths = new int[] { 41, 10, 9, 11, 10, 7 };
                    strWriteOut.Write("                                    ");
                    strWriteOut.WriteLine(jg.Code.PadLeft(2, ' '));
                    foreach (RegionalReports lto in listToOutput)
                    {
                        WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                                            rRH.R802sawtimber, 14, ref pageNumb, "");
                        prtFields.Clear();
                        prtFields.Add("");
                        prtFields.Add(lto.value1.PadRight(6, ' '));
                        prtFields.Add(Utilities.FormatField(lto.value7, "{0,6:F0}").ToString().PadLeft(6, ' '));
                        prtFields.Add(Utilities.FormatField(lto.value8 / mbfFactor, "{0,8:F3}").ToString().PadLeft(8, ' '));
                        prtFields.Add(Utilities.FormatField(lto.value9 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));
                        prtFields.Add(Utilities.FormatField(lto.value10 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));

                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                    }   // end foreach loop on listToOutput
                    // output total for the sample group
                    strWriteOut.Write("                                    ");
                    strWriteOut.Write(jg.Code.PadLeft(2, ' '));
                    strWriteOut.Write(" TOTAL:      ");
                    strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value7), "{0,6:F0}").ToString().PadLeft(6, ' '));
                    strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value8) / mbfFactor, "{0,8:F3}").ToString().PadLeft(11, ' '));
                    strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value9) / ccfFactor, "{0,7:F2}").ToString().PadLeft(10, ' '));
                    strWriteOut.WriteLine(Utilities.FormatField(listToOutput.Sum(l => l.value10) / ccfFactor, "{0,7:F2}").ToString().PadLeft(10, ' '));
                    strWriteOut.WriteLine("");
                    //  update total list
                    totalToOutput[0].value7 += listToOutput.Sum(l => l.value7);
                    totalToOutput[0].value8 += listToOutput.Sum(l => l.value8);
                    totalToOutput[0].value9 += listToOutput.Sum(l => l.value9);
                    totalToOutput[0].value10 += listToOutput.Sum(l => l.value10);
                    listToOutput.Clear();
                }
            }   //  end foreach loop on sample group
            if(totalToOutput.Sum(t=>t.value7) > 0)
            {
                //  output sawtimber total line
                strWriteOut.Write(totalToOutput[0].value1);
                strWriteOut.Write(Utilities.FormatField(totalToOutput[0].value7, "{0,6:F0}").ToString().PadLeft(6, ' '));
                strWriteOut.Write(Utilities.FormatField(totalToOutput[0].value8 / mbfFactor, "{0,8:F3}").ToString().PadLeft(11, ' '));
                strWriteOut.Write(Utilities.FormatField(totalToOutput[0].value9 / ccfFactor, "{0,7:F2}").ToString().PadLeft(10, ' '));
                strWriteOut.WriteLine(Utilities.FormatField(totalToOutput[0].value10 / ccfFactor, "{0,7:F2}").ToString().PadLeft(10, ' '));
                totalToOutput.Clear();
            }   //  endif
            return;
        }   //  end OutputSawtimber



        private void OutputProduct8(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh, 
                                        IEnumerable<SampleGroupDO> justGroups, IEnumerable<LCDDO> lcdList)
        {
            //  R802    --  product 8 
            RegionalReports r2 = new RegionalReports();
            r2.value1 = "                                PRODUCT 8 TOTAL:   ";
            totalToOutput.Add(r2);

                //  process by sample group
                foreach (SampleGroupDO jg in justGroups)
                {
                        List<LCDDO> justSpecies = lcdList.Where(
                            l => l.CutLeave == "C" && l.PrimaryProduct == "08" && l.SampleGroup == jg.Code).ToList();

                            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                                    rRH.R802product08, 14, ref pageNumb, "");
                           
                    //  accumulate data by species
                    string prevST = "*";
                    double currAC = 0;
                    List<StratumDO> sList = Global.BL.getStratum().ToList();
                    foreach (LCDDO js in justSpecies)
                    {
                        //  find species in listToOutput or add it
                        int nthRow = listToOutput.FindIndex(
                            delegate(RegionalReports l)
                            {
                                return l.value1 == js.Species;
                            });
                        //  find strata acres as needed
                        if (prevST != js.Stratum)
                        {
                            StratumDO stratum = sList.First(s => s.Code == js.Stratum);
                            currAC = Utilities.ReturnCorrectAcres(js.Stratum, (long)stratum.Stratum_CN);
                            prevST = js.Stratum;
                        }   //  endif

                        if (nthRow == -1)
                        {
                            // add species
                            RegionalReports r = new RegionalReports();
                            r.value1 = js.Species;
                            r.value7 = js.SumExpanFactor * currAC;
                            r.value8 = js.SumNBDFT * currAC;
                            r.value9 = js.SumNCUFT * currAC;
                            r.value10 = js.SumNCUFTtop * currAC;
                            listToOutput.Add(r);
                        }
                        else if (nthRow >= 0)
                        {
                            //  update listToOutput
                            listToOutput[nthRow].value7 += js.SumExpanFactor * currAC;
                            listToOutput[nthRow].value8 += js.SumNBDFT * currAC;
                            listToOutput[nthRow].value9 += js.SumNCUFT * currAC;
                            listToOutput[nthRow].value10 += js.SumNCUFTtop * currAC;
                        }   //  endif nthRow
                    }   //  end foreach on justSpecies

                    // output sample group/species and total line 
                    if (listToOutput.Sum(l => l.value7) > 0)
                    {
                        fieldLengths = new int[] { 44, 9, 9, 10, 7 };
                        strWriteOut.Write("                                    ");
                        strWriteOut.WriteLine(jg.Code.PadLeft(2, ' '));
                        foreach (RegionalReports lto in listToOutput)
                        {
                            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                                    rRH.R802product08, 14, ref pageNumb, "");
                            prtFields.Clear();
                            prtFields.Add("");
                            prtFields.Add(lto.value1.PadRight(6, ' '));
                            prtFields.Add(Utilities.FormatField(lto.value7, "{0,6:F0}").ToString().PadLeft(6, ' '));
                            prtFields.Add(Utilities.FormatField(lto.value9 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));
                            prtFields.Add(Utilities.FormatField(lto.value10 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));

                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                        }   // end foreach loop on listToOutput
                        // output total for the sample group
                        strWriteOut.Write("                                    ");
                        strWriteOut.Write(jg.Code.PadLeft(2, ' '));
                        strWriteOut.Write(" TOTAL:      ");
                        strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value7), "{0,6:F0}").ToString().PadLeft(6, ' '));
                        strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value9) / ccfFactor, "{0,7:F2}").ToString().PadLeft(10, ' '));
                        strWriteOut.WriteLine(Utilities.FormatField(listToOutput.Sum(l => l.value10) / ccfFactor, "{0,7:F2}").ToString().PadLeft(10, ' '));
                        strWriteOut.WriteLine("");
                        //  update total list
                        totalToOutput[0].value7 += listToOutput.Sum(l => l.value7);
                        totalToOutput[0].value9 += listToOutput.Sum(l => l.value9);
                        totalToOutput[0].value10 += listToOutput.Sum(l => l.value10);
                        listToOutput.Clear();
                    }   //  endif something to print
                }   //  end foreach loop on sample group
                //  output total line for product8
                if (totalToOutput.Sum(t => t.value7) > 0)
                {
                    strWriteOut.Write(totalToOutput[0].value1);
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].value7, "{0,6:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].value9 / ccfFactor, "{0,7:F2}").ToString().PadLeft(10, ' '));
                    strWriteOut.WriteLine(Utilities.FormatField(totalToOutput[0].value10 / ccfFactor, "{0,7:F2}").ToString().PadLeft(10, ' '));
                }
                else
                {
                    strWriteOut.Write(totalToOutput[0].value1);
                    strWriteOut.Write("     0");
                    strWriteOut.Write("      0.00");
                    strWriteOut.WriteLine("       0.0");
                }   //  endif
                totalToOutput.Clear();
            return;
        }   //  end OutputProduct8



        private void OutputPulpwood(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh, 
                                        IEnumerable<SampleGroupDO> justGroups, IEnumerable<LCDDO> lcdList)
        {
            //  R802    --  pulpwood page
            RegionalReports r3 = new RegionalReports();
            r3.value1 = "                                            PULPWOOD TOTAL:   ";
            totalToOutput.Add(r3);

                //  process by sample group
                foreach (SampleGroupDO jg in justGroups)
                {
                        List<LCDDO> justSpecies = lcdList.Where(
                            l => l.CutLeave == "C" && l.SampleGroup == jg.Code &&
                                        l.PrimaryProduct != "01" && l.PrimaryProduct != "08").ToList();
                            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                            rRH.R802pulpwood, 14, ref pageNumb, "");

                    //  accumulate data by species
                    string prevST = "*";
                    double currAC = 0;
                    List<StratumDO> sList = Global.BL.getStratum().ToList();
                    foreach (LCDDO js in justSpecies)
                    {
                        //  find species in listToOutput or add it
                        RegionalReports rr = listToOutput.FirstOrDefault(l => l.value1 == js.Species);
                        //  find strata acres as needed
                        if (prevST != js.Stratum)
                        {
                            int mthRow = sList.FindIndex(
                                delegate(StratumDO s)
                                {
                                    return s.Code == js.Stratum;
                                });
                            currAC = Utilities.ReturnCorrectAcres(js.Stratum, (long)sList[mthRow].Stratum_CN);
                            prevST = js.Stratum;
                        }   //  endif

                        if (rr == null)
                        {
                            // add species
                            RegionalReports r = new RegionalReports();
                            r.value1 = js.Species;
                            r.value7 = js.SumExpanFactor * currAC;
                            r.value8 = js.SumNBDFT * currAC;
                            r.value9 = js.SumNCUFT * currAC;
                            r.value10 = js.SumNCUFTtop * currAC;
                            listToOutput.Add(r);
                        }
                        else
                        {
                            //  update listToOutput
                            rr.value7 += js.SumExpanFactor * currAC;
                            rr.value8 += js.SumNBDFT * currAC;
                            rr.value9 += js.SumNCUFT * currAC;
                            rr.value10 += js.SumNCUFTtop * currAC;
                        }   //  endif nthRow
                    }   //  end foreach on justSpecies

                    // output sample group/species and total line 
                    if (listToOutput.Sum(l => l.value7) > 0)
                    {
                        fieldLengths = new int[] { 52, 10, 9, 7 };
                        strWriteOut.Write("                                            ");
                        strWriteOut.WriteLine(jg.Code.PadLeft(2, ' '));
                        foreach (RegionalReports lto in listToOutput)
                        {
                            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                                    rRH.R802pulpwood, 14, ref pageNumb, "");
                            prtFields.Clear();
                            prtFields.Add("");
                            prtFields.Add(lto.value1.PadRight(6, ' '));
                            prtFields.Add(Utilities.FormatField(lto.value7, "{0,6:F0}").ToString().PadLeft(6, ' '));
                            prtFields.Add(Utilities.FormatField(lto.value9 / ccfFactor, "{0,7:F2}").ToString().PadLeft(7, ' '));

                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                        }   // end foreach loop on listToOutput
                        // output total for the sample group
                        strWriteOut.Write("                                            ");
                        strWriteOut.Write(jg.Code.PadLeft(2, ' '));
                        strWriteOut.Write(" TOTAL:         ");
                        strWriteOut.Write(Utilities.FormatField(listToOutput.Sum(l => l.value7), "{0,6:F0}").ToString().PadLeft(6, ' '));
                        strWriteOut.WriteLine(Utilities.FormatField(listToOutput.Sum(l => l.value9) / ccfFactor, "{0,7:F2}").ToString().PadLeft(10, ' '));
                        strWriteOut.WriteLine(" ");
                        //  update total list
                        totalToOutput[0].value7 += listToOutput.Sum(l => l.value7);
                        totalToOutput[0].value9 += listToOutput.Sum(l => l.value9);
                        listToOutput.Clear();
                    }   //  endif something to print
                }   //  end foreach loop on sample group
                if (totalToOutput.Sum(t => t.value7) > 0)
                {
                    //  output pulpwood total line
                    strWriteOut.Write(totalToOutput[0].value1);
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].value7, "{0,6:F0}").ToString().PadLeft(6, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].value9 / ccfFactor, "{0,7:F2}").ToString().PadLeft(10, ' '));
                }
                else
                {
                    strWriteOut.Write(totalToOutput[0].value1);
                    strWriteOut.Write("        0");
                    strWriteOut.WriteLine("       0.0");
                }   //  endif
                totalToOutput.Clear();
            return;
        }   //  end OutputPulpwood


        private void LoadDIBclasses()
        {
            //  Loads DIB classes for R801 -- a static load
            int nextDIB = 4;
            for (int j = 0; j < 29; j++)
            {
                RegionalReports rr = new RegionalReports();
                rr.value1 = (nextDIB).ToString();
                listToOutput.Add(rr);
                nextDIB += 2;
            }   //  end for j loop
            return;
        }   //  end LoadDIBclasses


        private int findDBHindex(float currDBH)
        {
            //  R801
            int DIBtoFind = -1;
            if (currDBH > 0 && currDBH < 5)
                DIBtoFind = 4;
            else if (currDBH > 60)
                DIBtoFind = 60;
            else DIBtoFind = (int)Math.Floor(currDBH);

            //if((currDBH % 2) > 0) DIBtoFind++;
            int remainder;
            Math.DivRem((int)currDBH, 2, out remainder);
            if (remainder > 0) DIBtoFind++;

            int nthRow = listToOutput.FindIndex(
                delegate(RegionalReports r)
                {
                    return r.value1 == DIBtoFind.ToString();
                });
            if (nthRow >= 0)
                return nthRow;
            else return 0;
        }   //  end findDBHindex

        private string[] createCompleteHeader(double saleAcres)
        {
            string[] finnishHeader = rRH.R801columns;
            finnishHeader[0] = finnishHeader[0].Replace("XXXXX", Utilities.FormatField(saleAcres, "{0,6:F2}").ToString().PadLeft(6, ' '));
            return finnishHeader;
        }   //  end createCompleteHeader


    }
}
