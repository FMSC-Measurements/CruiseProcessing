﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public class OutputR4 : CreateTextFile
    {
        #region
        public string currentReport;
        private int[] fieldLengths;
        private ArrayList prtFields = new ArrayList();
        private List<RegionalReports> listToOutput = new List<RegionalReports>();
        private List<ReportSubtotal> totalToOutput = new List<ReportSubtotal>();
        private regionalReportHeaders rRH = new regionalReportHeaders();
        private double currGRS = 0;
        private double currNET = 0;
        private double currAC = 0;
        private double currDBH2 = 0;
        private double currHGT = 0;
        private double currLOGS = 0;
        private double currVAL = 0;
        private double currEF = 0;
        private double convFactor = 100.0;
        private string[] completeHeader;
        #endregion

        public void CreateR4Reports(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  fill report title array
            string currentTitle = fillReportTitle(currentReport);

            //  grab LCD list
            List<LCDDO> lcdList = bslyr.getLCD();
            //  Any data for current report?
            switch (currentReport)
            {
                case "R401":
                case "R403":
                    currGRS = lcdList.Sum(l => l.SumGBDFT);
                    if (currGRS == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No board foot volume for report ");
                        return;
                    }   //  endif
                    break;
                case "R402":
                case "R404":
                    currGRS = lcdList.Sum(l => l.SumGCUFT);
                    if (currGRS == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No cubic foot volume for report ");
                        break;
                    }   //  endif
                    break;
            }   //  end switch

            //  get cutting unit list to total sale acres
            List<CuttingUnitDO> cList = bslyr.getCuttingUnits();
            double totalSaleAcres = cList.Sum(c => c.Area);
            //  and stratum list to get expansion acres later
            List<StratumDO> sList = bslyr.getStratum();

            //  determine which heights to use for mean height calculation
            int hgtOne = 0;
            int hgtTwo = 0;
            List<TreeDO> tList = bslyr.getTrees();
            whichHeightFields(ref hgtOne, ref hgtTwo, tList);

            //  set volume type for headers
            string volTitle = "";
            switch (currentReport)
            {
                case"R401":     case "R403":
                    volTitle = reportConstants.volumeType.Replace("XXXXX","BOARD");
                    convFactor = 1000.0;
                    break;
                case "R402":    case "R404":
                    volTitle = reportConstants.volumeType.Replace("XXXXX", "CUBIC");
                    break;
            }   //  end switch on current report for volume type
            //  process by report
            switch(currentReport)
            {
                case "R401":
                case "R402":
                    numOlines = 0;
                    fieldLengths = new int[] { 1, 8, 3, 13, 7, 14, 11, 10, 12, 9, 9, 6, 10, 8, 8 };
                    completeHeader = createCompleteHeader();
                    rh.createReportTitle(currentTitle, 6, 0, 0, volTitle, reportConstants.FCTO_PPO);
                    //  group LCD by species as the first two reports run by species
                    List<LCDDO> speciesList = bslyr.getLCDOrdered("WHERE CutLeave = @p1 ", "GROUP BY Species,PrimaryProduct", "C", "");
                    foreach(LCDDO sl in speciesList)
                    {
                        AccumulateSpeciesVolume(lcdList,sl.Species, hgtOne, sl.PrimaryProduct);
                        WriteCurrentGroup(strWriteOut, ref pageNumb, rh, totalSaleAcres);
                        //  update total line
                        updateTotal();
                        listToOutput.Clear();
                    }   //  end foreach loop
                    //  output total line
                    writeTotalLine(strWriteOut, ref pageNumb, rh, totalSaleAcres);
                    break;
                case "R403":
                case "R404":
                    //  June 2017 -- These reports are by logging method so if it is blank or null
                    //  cannot generate the report
                    List<CuttingUnitDO> cutList = bslyr.getCuttingUnits();
                      int noMethod = 0;
                    foreach (CuttingUnitDO ct in cutList)
                    {
                        if (ct.LoggingMethod == "" || ct.LoggingMethod == " " || ct.LoggingMethod == null)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> One or more logging methods are missing.  Cannot generate this report.");
                            noMethod = -1;
                        }   //  endif
                    }   //  end foreach loop
                    if  (noMethod != -1)
                    {
                        numOlines = 0;
                        fieldLengths = new int[] { 1, 6, 7, 12, 9, 11, 10, 11, 12, 10, 9, 6, 9, 7, 8 };
                        completeHeader = createCompleteHeader();
                        rh.createReportTitle(currentTitle, 6, 0, 0, volTitle, reportConstants.FCTO_PPO);
                        //  these reports are prorated and grouped by logging method
                        List<CuttingUnitDO> justMethods = bslyr.getLoggingMethods();
                        AccumulateByLogMethod(justMethods, hgtOne);
                        WriteCurrentGroup(rh, strWriteOut, ref pageNumb, totalSaleAcres);
                        //  update total
                        updateTotal();
                        //  output total
                        writeTotalLine(strWriteOut, ref pageNumb, rh);
                    }   //  endif noMethod
                    break;
            }   //  end switch on currentReport
            return;
        }   //  end CreateR4Reports

        private void AccumulateSpeciesVolume(List<LCDDO> lcdList, string currSP, int hgtOne, string currPP)
        {
            //  pull all species from lcd list
            List<LCDDO> justSpecies = lcdList.FindAll(
                delegate(LCDDO l)
                {
                    return l.CutLeave == "C" && l.Species == currSP && l.PrimaryProduct == currPP;
                });

            //  need stratum table to get acres
            List<StratumDO> sList = bslyr.getStratum();
            currAC = 0;
            currDBH2 = 0;
            currHGT = 0;
            currLOGS = 0;
            currVAL = 0;
            currEF = 0;
            currGRS = 0;
            currNET = 0;
            int nthRow = 0;
            string prevST = "*";
            foreach (LCDDO js in justSpecies)
            {
                //  strata acres
                if (js.Stratum != prevST)
                {
                    nthRow = sList.FindIndex(
                        delegate(StratumDO s)
                        {
                            return s.Code == js.Stratum;
                        });
                    currAC = Utilities.ReturnCorrectAcres(js.Stratum, bslyr, (long)sList[nthRow].Stratum_CN);
                    prevST = js.Stratum;
                }   //  endif
                switch (currentReport)
                {
                    case "R401":        //  board foot
                        currGRS += js.SumGBDFT * currAC;
                        currNET += js.SumNBDFT * currAC;
                        break;
                    case "R402":        //  cubic foot
                        currGRS += js.SumGCUFT * currAC;
                        currNET += js.SumNCUFT * currAC;
                        break;
                }   //  end switch on current report

                //  sum other variables
                currDBH2 += js.SumDBHOBsqrd * currAC;
                switch (hgtOne)
                {
                    case 1:
                        currHGT += js.SumTotHgt * currAC;
                        break;
                    case 2:
                        currHGT += js.SumMerchHgtPrim * currAC;
                        break;
                    case 3:
                        currHGT += js.SumMerchHgtSecond * currAC;
                        break;
                    case 4:
                        currHGT += js.SumHgtUpStem * currAC;
                        break;
                }   //  end switch on height to use
                currVAL += js.SumValue * currAC;
                currLOGS += js.SumLogsMS * currAC;
                //  expansion factor
                if (sList[nthRow].Method == "S3P" || sList[nthRow].Method == "3P")
                {
                    if (js.SumExpanFactor > 0 && js.STM == "N")
                        currEF += js.SumExpanFactor * js.TalliedTrees / js.SumExpanFactor;
                    else if (js.STM == "Y")
                        currEF += js.SumExpanFactor * currAC;
                    else if(js.SumExpanFactor == 0)
                        currEF += js.TalliedTrees;
                }
                else currEF += js.SumExpanFactor * currAC;
            }   //  end foreach loop
            //  load into listToOutput
            RegionalReports rr = new RegionalReports();
            rr.value1 = currSP;
            rr.value2 = justSpecies[0].PrimaryProduct;
            rr.value7 = currGRS;
            rr.value8 = currNET;
            rr.value9 = currDBH2;
            rr.value10 = currHGT;
            rr.value11 = currLOGS;
            rr.value12 = currVAL;
            rr.value13 = currEF;
            listToOutput.Add(rr);
            return;
        }   //  end AccumulateSpeciesVolume


        private void AccumulateByLogMethod(List<CuttingUnitDO> justMethods, int hgtOne)
        {
            //R403/R404
            double unitAC = 0;
            currGRS = 0;
            currNET = 0;
            currEF = 0;
            currDBH2 = 0;
            currLOGS = 0;
            currHGT = 0;

            List<CuttingUnitDO> cutList = bslyr.getCuttingUnits();
            List<PRODO> proList = bslyr.getPRO();
            //  accumulate sums for each logging method
            foreach (CuttingUnitDO jm in justMethods)
            {
                // find all unit for current method
                List<CuttingUnitDO> justUnits = cutList.FindAll(
                    delegate(CuttingUnitDO c)
                    {
                        return c.LoggingMethod == jm.LoggingMethod;
                    });

                foreach (CuttingUnitDO ju in justUnits)
                {
                    unitAC += ju.Area;
                    ju.Strata.Populate();
                    //  sum up each stratum
                    foreach (StratumDO stratum in ju.Strata)
                    {
                        //  pull strata from LCD table
                        List<LCDDO> justStrata = bslyr.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2 ", "", 
                                                                        "C", stratum.Code, "");
                        foreach (LCDDO js in justStrata)
                        {
                            //  find proration factor for the group
                            int nthRow = proList.FindIndex(
                                delegate(PRODO p)
                                {
                                    return p.CutLeave == "C" && p.Stratum == js.Stratum && p.CuttingUnit == ju.Code
                                            && p.SampleGroup == js.SampleGroup && p.STM == js.STM && p.PrimaryProduct == js.PrimaryProduct;
                                });
                            double proratFactor;
                            if (nthRow >= 0)
                                proratFactor = proList[nthRow].ProrationFactor;
                            else proratFactor = 0;

                            switch (currentReport)
                            {
                                case "R403":
                                    currGRS += js.SumGBDFT * proratFactor;
                                    currNET += js.SumNBDFT * proratFactor;
                                    break;
                                case "R404":
                                    currGRS += js.SumGCUFT * proratFactor;
                                    currNET += js.SumNCUFT * proratFactor;
                                    break;
                            }   //  end switch on current report
                            //  sum up rest of values
                            currDBH2 += js.SumDBHOBsqrd * proratFactor;
                            switch (hgtOne)
                            {
                                case 1:
                                    currHGT += js.SumTotHgt * proratFactor;
                                    break;
                                case 2:
                                    currHGT += js.SumMerchHgtPrim * proratFactor;
                                    break;
                                case 3:
                                    currHGT += js.SumMerchHgtSecond * proratFactor;
                                    break;
                                case 4:
                                    currHGT += js.SumHgtUpStem * proratFactor;
                                    break;
                            }   //  end switch on height to use
                            currLOGS += js.SumLogsMS * proratFactor;
                            //  expansion factor
                            if (stratum.Method == "S3P" || stratum.Method == "3P")
                            {
                                if (js.SumExpanFactor > 0 && js.STM == "N")
                                    currEF += js.SumExpanFactor * js.TalliedTrees / js.SumExpanFactor;
                                else if (js.STM == "Y")
                                    currEF += js.SumExpanFactor * proratFactor;
                                else if (js.SumExpanFactor == 0)
                                    currEF += js.TalliedTrees;
                            }
                            else currEF += js.SumExpanFactor * proratFactor;

                            //  load listToOutput
                            loadListToOutput(jm.LoggingMethod, js.PrimaryProduct, currGRS, currNET, 
                                                currDBH2, currHGT, currLOGS, unitAC, currEF);
                            unitAC = 0;
                            currGRS = 0;
                            currNET = 0;
                            currEF = 0;
                            currDBH2 = 0;
                            currLOGS = 0;
                            currHGT = 0;
                        }   //  end foreach loop on justStrata
                    }   //  end for loop on strata list
                }   //  end foreach loop on justUnits
            }   //  end foreach loop on justMethods
            return;
        }   // end AccumulateByLogMethod


        private void loadListToOutput(string currLM, string currPP, double currGRS, double currNET, 
                    double currDBH2, double currHGT, double currLOGS, double currAC, double currEF)
        {
            //  loads ListToOutput for R403/R404
            //  see if group is already in ListToOutput
            int nthRow = listToOutput.FindIndex(
                delegate(RegionalReports rr)
                {
                    return rr.value1 == currLM && rr.value2 == currPP;
                });
            if (nthRow >= 0)
            {
                //  update numbers
                listToOutput[nthRow].value7 += currGRS;
                listToOutput[nthRow].value8 += currNET;
                listToOutput[nthRow].value9 += currDBH2;
                listToOutput[nthRow].value10 += currHGT;
                listToOutput[nthRow].value11 += currLOGS;
                listToOutput[nthRow].value12 += currAC;
                listToOutput[nthRow].value13 += currEF;
            }
            else if(nthRow < 0)
            {
                //  add group to ListToOutput
                RegionalReports rr = new RegionalReports();
                rr.value1 = currLM;
                rr.value2 = currPP;
                rr.value7 = currGRS;
                rr.value8 = currNET;
                rr.value9 = currDBH2;
                rr.value10 = currHGT;
                rr.value11 = currLOGS;
                rr.value12 = currAC;
                rr.value13 = currEF;
                listToOutput.Add(rr);
            }   //  endif nthRow
            return;
        }   //  end loadListToOutput



        private void WriteCurrentGroup(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh, double totAC)
        {
            //  R401/R402
            double calcValue = 0;
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                    completeHeader, 17, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(lto.value1.PadRight(6, ' '));
                prtFields.Add(lto.value2.PadLeft(2, '0'));
                //  gross volume
                prtFields.Add(Utilities.FormatField(lto.value7, "{0,10:F0}").ToString().PadLeft(10, ' '));
                //  defect percent
                if (lto.value7 > 0)
                    calcValue = ((lto.value7 - lto.value8) / lto.value7) * 100;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                //  net volume and number of trees
                prtFields.Add(Utilities.FormatField(lto.value8, "{0,10:F0}").ToString().PadLeft(10, ' '));
                prtFields.Add(Utilities.FormatField(lto.value13, "{0,8:F0}").ToString().PadLeft(8, ' '));
                //  gross volume per acre
                if (totAC > 0)
                    calcValue = lto.value7 / totAC;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));
                //  net volume per acre
                if (totAC > 0)
                    calcValue = lto.value8 / totAC;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));
                //  trees per acre
                if (totAC > 0)
                    calcValue = lto.value13 / totAC;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(6, ' '));
                //  quad mean DBH
                if (lto.value13 > 0)
                    calcValue = Math.Sqrt(lto.value9 / lto.value13);
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                //  mean height
                if (lto.value13 > 0)
                    calcValue = lto.value10 / lto.value13;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                //  net per tree
                if (lto.value13 > 0)
                    calcValue = lto.value8 / lto.value13;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));
                //  logs per gross volume
                if(lto.value7 > 0)
                    calcValue = lto.value11 / (lto.value7 / convFactor);
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue,"{0,6:F1}").ToString().PadLeft(6,' '));
                //  net log scale value
                if (lto.value12 > 0)
                {
                    if (lto.value8 > 0)
                    {
                        calcValue = lto.value12 / (lto.value8 / convFactor);
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F2}").ToString().PadLeft(8, ' '));
                    }
                    else prtFields.Add("        ");
                }   //  endif
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            return;
        }   //  end WriteCurrentGroup


        private void WriteCurrentGroup(reportHeaders rh, StreamWriter strWriteOut, ref int pageNumb, double totAC)
        {
            //  R403/R404
            double calcValue = 0;
            string currLogMethod = "";
            int methodFlag = 0;
            foreach (RegionalReports lto in listToOutput)
            {
                if (currLogMethod == lto.value1)
                    methodFlag = 1;
                else if (currLogMethod != lto.value1)
                {
                    currLogMethod = lto.value1;
                    methodFlag = 0;
                }   //  endif

                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                    completeHeader, 17, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                if (methodFlag == 0)
                    prtFields.Add(lto.value1.PadRight(3, ' '));
                else if (methodFlag == 1)
                    prtFields.Add("   ");
                prtFields.Add(lto.value2.PadLeft(2, '0'));
                // gross volume
                prtFields.Add(Utilities.FormatField(lto.value7, "{0,10:F0}").ToString().PadLeft(10, ' '));
                //  defect percent
                if (lto.value7 > 0)
                    calcValue = ((lto.value7 - lto.value8) / lto.value7) * 100;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                //  net volume and number of trees
                prtFields.Add(Utilities.FormatField(lto.value8, "{0,10:F0}").ToString().PadLeft(10, ' '));
                prtFields.Add(Utilities.FormatField(lto.value13, "{0,8:F0}").ToString().PadLeft(8, ' '));
                //  gross volume per acre
                if (totAC > 0)
                    calcValue = lto.value7 / totAC;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));
                //  net volume per acre
                if (totAC > 0)
                    calcValue = lto.value8 / totAC;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));
                //  trees per acre
                if (totAC > 0)
                    calcValue = lto.value13 / totAC;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(6, ' '));
                //  quad mean DBH
                if (lto.value13 > 0)
                    calcValue = Math.Sqrt(lto.value9 / lto.value13);
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                //  mean height
                if (lto.value13 > 0)
                    calcValue = lto.value10 / lto.value13;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                //  net per tree
                if (lto.value13 > 0)
                    calcValue = lto.value8 / lto.value13;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));
                //  logs per gross volume
                if (lto.value7 > 0)
                    calcValue = lto.value11 / (lto.value7 / convFactor);
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(6, ' '));
                //  total logging method acres
                prtFields.Add(Utilities.FormatField(lto.value12, "{0,8:F1}").ToString().PadLeft(8, ' '));

                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   // end foreach loop
            return;
        }   // end WriteCurrentGroup


        private void updateTotal()
        {
            if (totalToOutput.Count > 0)
            {
                foreach (RegionalReports lto in listToOutput)
                {
                    totalToOutput[0].Value7 += lto.value7;
                    totalToOutput[0].Value8 += lto.value8;
                    totalToOutput[0].Value9 += lto.value9;
                    totalToOutput[0].Value10 += lto.value10;
                    totalToOutput[0].Value11 += lto.value11;
                    totalToOutput[0].Value12 += lto.value12;
                    totalToOutput[0].Value13 += lto.value13;
                }   //  end foreach loop
            }
            else if(totalToOutput.Count == 0)
            {
                foreach (RegionalReports lto in listToOutput)
                {
                    ReportSubtotal r = new ReportSubtotal();
                    r.Value7 = lto.value7;
                    r.Value8 = lto.value8;
                    r.Value9 = lto.value9;
                    r.Value10 = lto.value10;
                    r.Value11 = lto.value11;
                    r.Value12 = lto.value12;
                    r.Value13 = lto.value13;
                    totalToOutput.Add(r);
                }   //  end foreach loop
            }   //  endif
            return;
        }   //  end updateTotal


        private void writeTotalLine(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh, double totAC)
        {
            //  R401/R402
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                completeHeader, 17, ref pageNumb, "");
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.Write(" TOTALS/AVE ");
            //  gross volume
            strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value7, "{0,10:F0}").ToString().PadLeft(10, ' '));
            //  defect percent
            if (totalToOutput[0].Value7 > 0)
                calcValue = ((totalToOutput[0].Value7 - totalToOutput[0].Value8) / totalToOutput[0].Value7) * 100;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(8, ' '));
            //  net volume and expansion factor
            strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value8, "{0,10:F0}").ToString().PadLeft(12, ' '));
            strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value13, "{0,8:F0}").ToString().PadLeft(12, ' '));
            //  gross volume per acre
            if (totAC > 0)
                calcValue = totalToOutput[0].Value7 / totAC;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(11, ' '));
            //  net volume per acre
            if (totAC > 0)
                calcValue = totalToOutput[0].Value8 / totAC;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(10, ' '));
            //  trees per acre
            if (totAC > 0)
                calcValue = totalToOutput[0].Value13 / totAC;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(10, ' '));
            //  quad mean dbh
            if (totalToOutput[0].Value13 > 0)
                calcValue = Math.Sqrt(totalToOutput[0].Value9 / totalToOutput[0].Value13);
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(8, ' '));
            //  mean height
            if (totalToOutput[0].Value13 > 0)
                calcValue = totalToOutput[0].Value10 / totalToOutput[0].Value13;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(9, ' '));
            //  net volume per tree
            if (totalToOutput[0].Value13 > 0)
                calcValue = totalToOutput[0].Value8 / totalToOutput[0].Value13;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(9, ' '));
            //  logs per volume
            if(totalToOutput[0].Value7 > 0)
                calcValue = totalToOutput[0].Value11 /(totalToOutput[0].Value7/convFactor);
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue,"{0,6:F1}").ToString().PadLeft(8,' '));
            //  net log scale value
            if(totalToOutput[0].Value12 > 0)
            {
                if(totalToOutput[0].Value8 > 0)
                {
                    calcValue = totalToOutput[0].Value12 / (totalToOutput[0].Value8 / convFactor);
                    strWriteOut.WriteLine(Utilities.FormatField(calcValue,"{0,8:F2}").ToString().PadLeft(10,' '));
                }
                else strWriteOut.WriteLine("          ");
            }
            else strWriteOut.WriteLine("     N/A  ");
            return;
        }   //  end writeTotalLine



        private void writeTotalLine(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  R403/R404
            //  May 2016 -- total line not picking up all groups -- fixed by summing up totalToOutput
            //  before printing the line
            double Total7 = totalToOutput.Sum(tto=>tto.Value7);
            double Total8 = totalToOutput.Sum(tto=>tto.Value8);
            double Total13 = totalToOutput.Sum(tto=>tto.Value13);
            double Total12 = totalToOutput.Sum(tto=>tto.Value12);
            double Total9 = totalToOutput.Sum(tto => tto.Value9);
            double Total10 = totalToOutput.Sum(tto => tto.Value10);
            double Total11 = totalToOutput.Sum(tto=>tto.Value11);
            
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                completeHeader, 17, ref pageNumb, "");
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.Write(" TOTALS/AVE   ");
            //  gross volume
            strWriteOut.Write(Utilities.FormatField(Total7, "{0,10:F0}").ToString().PadLeft(10, ' '));
            //  defect percent
            if (Total7 > 0)
                calcValue = ((Total7) - Total8) / Total7 * 100;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(7, ' '));
            //  net volume and expansion factor
            strWriteOut.Write(Utilities.FormatField(Total8, "{0,10:F0}").ToString().PadLeft(14, ' '));
            strWriteOut.Write(Utilities.FormatField(Total13, "{0,8:F0}").ToString().PadLeft(9, ' '));
            //  gross volume per acre
            if (Total12 > 0)
                calcValue = Total7 / Total12;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(10, ' '));
            //  net volume per acre
            if (Total12> 0)
                calcValue = Total8 / Total12;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(11, ' '));
            //  trees per acre
            if (Total12 > 0)
                calcValue = Total13 / Total12;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(10, ' '));
            //  quad mean dbh
            if (Total11 > 0)
                calcValue = Math.Sqrt(Total9 / Total13);
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(9, ' '));
            //  mean height
            if (Total13 > 0)
                calcValue = Total10 / Total13;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(9, ' '));
            //  net volume per tree
            if (Total13 > 0)
                calcValue = Total8 / Total13;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(9, ' '));
            //  logs per volume
            if (Total7 > 0)
                calcValue = Total11 / (Total7 / convFactor);
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(7, ' '));
            //  unit acres
            strWriteOut.WriteLine(Utilities.FormatField(Total12, "{0,8:F1}").ToString().PadLeft(9, ' '));
            return;
        }   //  end writeTotalLine


        private string[] createCompleteHeader()
        {
            string[] finnishHeader = new string[7];
            if (currentReport == "R401" || currentReport == "R402")
                finnishHeader = rRH.R401R402columns;
            else if (currentReport == "R403" || currentReport == "R404")
                finnishHeader = rRH.R403R404columns;
            switch (currentReport)
            {
                case "R401":        case "R403":
                    finnishHeader[5] = finnishHeader[5].Replace("XX", "BF");
                    finnishHeader[6] = finnishHeader[6].Replace("XX", "BF");
                    finnishHeader[6] = finnishHeader[6].Replace("ZZZ", "MBF");
                    break;
                case "R402":        case "R404":
                    finnishHeader[5] = finnishHeader[5].Replace("XX", "CF");
                    finnishHeader[6] = finnishHeader[6].Replace("XX", "CF");
                    finnishHeader[6] = finnishHeader[6].Replace("ZZZ", "CCF");
                    break;
            }   //  end switch on current report
            return finnishHeader;
        }   //  end createCompleteHeader

    }
}
