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
    public class OutputR1 : CreateTextFile
    {
        #region
        public string currentReport;
        private int[] fieldLengths;
        private ArrayList prtFields = new ArrayList();
        private List<RegionalReports> listToOutput = new List<RegionalReports>();
        private List<RegionalReports> listTotalOutput = new List<RegionalReports>();
        private List<ReportSubtotal> firstSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> secondSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> thirdSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> totalToOutput = new List<ReportSubtotal>();
        private regionalReportHeaders rRH = new regionalReportHeaders();
        private string[] completeHeader;
        private double currGRS;
        private double currNET;
        private double currEF;
        private double currDBH;
        private double currHGT;
        private double currLOGS;
        #endregion

        public void CreateR1reports(StreamWriter strWriteOut, ref int pageNum, reportHeaders rh)
        {
            //  fill report title array
            string currentTitle = fillReportTitle(currentReport);
            List<LCDDO> lcdList = bslyr.getLCD();;

            //  is there any data for the report
            switch (currentReport)
            {
                case "R101":
                    if (lcdList.Sum(l => l.SumGBDFT) == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No board foot volume for report");
                        return;
                    }   //  endif on board foot
                    if (lcdList.Sum(l => l.SumGCUFT) == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No cubic foot volume for report");
                        return;
                    }   //  endif on cubic foot
                    break;
                case "R102":
                    if (lcdList.Sum(l => l.SumGBDFT) == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No board foot volume for report");
                        return;
                    }   //  endif on board foot
                    break;
                case "R103":
                    if (lcdList.Sum(l => l.SumGCUFT) == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No cubic foot volume for report");
                        return;
                    }   //  endif on cubic foot
                    break;
                case "R104":
                    if (lcdList.Count == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No data for report");
                        return;
                    }   //  endif on no data
                    break;
                case "R105":
                    if (lcdList.Count == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, ">>>>> No data for report");
                        return;
                    }   //  end if no data
                    break;
            }   //  end switch on report

            //  process base on report
            switch (currentReport)
            {
                case "R101":
                    numOlines = 0;
                    fieldLengths = new int[] { 2, 6, 8, 4, 4, 7, 8, 11, 12, 6, 12, 11, 11, 14, 10, 5 };
                    rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FCTO, "");
                    //  pull groups from LCD
                    List<LCDDO> speciesList = bslyr.getLCDOrdered("WHERE CutLeave = ?", "GROUP BY UOM,PrimaryProduct,Species,ContractSpecies", "C", "");
                    createR101(speciesList, strWriteOut, ref pageNum, rh, lcdList);
                    break;
                case "R102":
                case "R103":
                    numOlines = 0;
                    string volTitle = "BOARD";
                    fieldLengths = new[] { 1, 4, 3, 4, 13, 6, 11, 9, 11, 11, 9, 7, 8, 9, 9, 7, 8 };
                    if (currentReport == "R103")
                        volTitle = reportConstants.volumeType.Replace("XXXXX", "CUBIC");
                    rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FCTO, volTitle);
                    List<CuttingUnitDO> justMethods = bslyr.getLoggingMethods();
                    //  June 2017 -- these reports are by logging method so if blank or null
                    //  cannot generate the reports
                    int noMethod = 0;
                    foreach (CuttingUnitDO jm in justMethods)
                    {
                        if(jm.LoggingMethod == " " || jm.LoggingMethod == null)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> One or more logging methods are missing.  Cannot generate this report.");
                            noMethod = -1;
                            break;
                        }   // endif
                    }   //  end foreach loop
                    if (noMethod != -1)
                    {
                        //  determine which heights to use for mean height calculation
                        int hgtOne = 0;
                        int hgtTwo = 0;
                        List<TreeDO> tList = bslyr.getTrees();
                        whichHeightFields(ref hgtOne, ref hgtTwo, tList);

                        //  accumulate data by logging method
                        completeHeader = createCompleteHeader();
                        AccumulateLogMethods(justMethods, hgtOne);
                        WriteCurrentMethods(strWriteOut, ref pageNum, rh);

                        //  output overall total
                        outputTotal(strWriteOut, ref pageNum, rh, 3, "", totalToOutput);
                    }   //  endif noMethod
                    break;
                case "R104":
                    numOlines = 0;
                    fieldLengths = new[] { 3, 8, 12, 12, 11, 11, 8, 9, 5 };
                    rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FCLT, "");
                    //  This report will be by stratum and cutting unit so get all stratum first
                    List<StratumDO> orderedStrata = bslyr.getStratum();
                    createR104(orderedStrata, strWriteOut, ref pageNum, rh);
                    break;
                case "R105":
                    numOlines = 0;
                    // section one -- volume summary by ubit
                    fieldLengths = new[] { 4, 7, 10, 10, 11, 10, 11, 11, 9, 11 };
                    rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FCTO, " ");
                    List<CuttingUnitDO> cList = bslyr.getCuttingUnits();
                    List<LCDDO> ldList = bslyr.getLCD();
                    List<TreeCalculatedValuesDO> tcvList = bslyr.getTreeCalculatedValues();
                    AccumulateValuesAndPrint(strWriteOut,cList, ldList);
                    WriteSectionOne(strWriteOut, ref pageNum, rh, rRH);

                    //  section two -- subtitaks by species only.
                    fieldLengths = new[] { 10, 7, 10, 12, 11, 11, 11, 11, 11, 11};
//                    List<LCDDO> lList = bslyr.getLCD();
                    AccumulateSubtotalAndPrint(strWriteOut, cList, ldList);
                    WriteSectionTwo(strWriteOut, ref pageNum, rh, rRH);

                    //section three -- Totals by product
                    //AccumulateTotalsAndPrint(strWriteOut,ldList,tcvList);
                    WriteSectionThree(strWriteOut, ref pageNum, rh, rRH);
                    
                    break;
            }   //  end switch
            return;
        }   //  end CreateR1reports


        private void createR101(List<LCDDO> speciesList, StreamWriter strWriteOut, ref int pageNum, 
                                reportHeaders rh, List<LCDDO> lcdList)
        {
            //  generates R101 report
            string currCS = "*";
            string currPP = "*";
            foreach (LCDDO sl in speciesList)
            {
                if (currPP == "*")
                    currPP = sl.PrimaryProduct;
                if (currCS == "*")
                    currCS = sl.ContractSpecies;
                if (currPP != sl.PrimaryProduct)
                {
                    //  Output product subtotal
                    outputTotal(1, currPP, firstSubtotal, strWriteOut, ref pageNum, rh);
                    firstSubtotal.Clear();
                    currPP = sl.PrimaryProduct;
                }
                if (currCS != sl.ContractSpecies)
                {
                    //  update overall total
                    updateTotal();
                    //  Output contract species total
                    outputTotal(2, currCS, secondSubtotal, strWriteOut, ref pageNum, rh);
                    secondSubtotal.Clear();
                    currCS = sl.ContractSpecies;
                }   //  endif

                AccumulateVolumes(lcdList, sl.ContractSpecies, sl.Species, sl.PrimaryProduct);
                //  Update product subtotal
                updateSubtotal(firstSubtotal);
                //  update contract species subtotal
                updateSubtotal(secondSubtotal);
                WriteCurrentGroup(strWriteOut, ref pageNum, rh);
                listToOutput.Clear();
            }   //  end foreach loop
            //  output last product and contract species total
            outputTotal(1, currPP, firstSubtotal, strWriteOut, ref pageNum, rh);
            outputTotal(2, currCS, secondSubtotal, strWriteOut, ref pageNum, rh);
            //  update overall total
            updateTotal();
            //  output overall contract species total
            outputTotal(3, "", totalToOutput, strWriteOut, ref pageNum, rh);
            return;
        }   //  end createR101


        private void createR104(List<StratumDO> orderedStrata, StreamWriter strWriteOut, ref int pageNum,
                                reportHeaders rh)
        {
            //  then process by stratum and cutting unit
            foreach (StratumDO os in orderedStrata)
            {
                os.CuttingUnits.Populate();
                ReportSubtotal r1 = new ReportSubtotal();
                r1.Value1 = "C";
                totalToOutput.Add(r1);
                ReportSubtotal r2 = new ReportSubtotal();
                r2.Value1 = "L";
                totalToOutput.Add(r2);
                foreach (CuttingUnitDO cu in os.CuttingUnits)
                {
                    // set two lines in totalToOutput for cut and leave
                    //  process cut trees first
                    AccumulateBasalArea(os.Code, cu.Code, cu.Area, "C");
                    WriteCurrentUnit(strWriteOut, ref pageNum, rh);
                    //  update and output cut/leave subtotal
                    if (listToOutput.Count > 0)
                    {
                        updateUnitOrStrata(3, "C");
                        outputCutLeaveSubtotal("C", strWriteOut, ref pageNum, rh);
                        firstSubtotal.Clear();
//                    }   //  endif
                        //  update unit subtotal
                        updateUnitOrStrata(1, "C");
                        //  update strata total
                        updateUnitOrStrata(2, "C");
                    }
                    else if (listToOutput.Count == 0)
                    {
                        StringBuilder msg = new StringBuilder();
                        msg.Append("CUTTING UNIT ");
                        msg.Append(cu.Code);
                        msg.Append(" has no cut tree data and is not included in this report. ");
                        msg.Append("Report is not complete.");
                        strWriteOut.WriteLine(msg);
                    }   //  endif

                    //  process leave trees
                    listToOutput.Clear();
                    AccumulateBasalArea(os.Code, cu.Code, cu.Area, "L");
                    if (listToOutput.Count > 0)
                    {
                        updateUnitOrStrata(3,"L");
                        outputCutLeaveSubtotal("L", strWriteOut, ref pageNum, rh);
                        firstSubtotal.Clear();
                    }   //  endif
                    //  update and output unit subtotal
                    updateUnitOrStrata(1,"L");
                    //  update strata total
                    updateUnitOrStrata(2,"L");
                    outputUnitOrStrata(strWriteOut, ref pageNum, rh, cu.Code, 1, secondSubtotal);
                    secondSubtotal.Clear();
                    listToOutput.Clear();
                }   //  end foreach loop on cutting unit
                //  output strata total
                outputUnitOrStrata(strWriteOut, ref pageNum, rh, os.Code, 2, totalToOutput);
                totalToOutput.Clear();
            }   //  end foreach loop on stratum
            return;
        }   //  end createR104


        private void AccumulateVolumes(List<LCDDO> lcdList, string currCS, string currSP, string currPP)
        {
            //  R101
            double currGB = 0;
            double currNB = 0;
            double currGC = 0;
            double currNC = 0;
            string currMeth = "";
            string currUOM = "";
            currLOGS = 0;
            currDBH = 0;
            currEF = 0;
            List<StratumDO> sList = bslyr.getStratum();
            //  pull current group from LCD
            List<LCDDO> justCurrentGroup = lcdList.FindAll(
                delegate(LCDDO l)
                {
                    return l.ContractSpecies == currCS && l.Species == currSP && l.PrimaryProduct == currPP;
                });

            //  going to need proration factor for sample group and unit on each LCD record
            List<PRODO> proList = bslyr.getPRO();
            foreach (LCDDO jcg in justCurrentGroup)
            {
                currUOM = jcg.UOM;
                //  get all units for stratum and sample group
                List<PRODO> justUnits = proList.FindAll(
                    delegate(PRODO p)
                    {
                        return p.CutLeave == "C" && p.Stratum == jcg.Stratum && p.SampleGroup == jcg.SampleGroup;
                    });
                foreach (PRODO ju in justUnits)
                {
                    //  sum DBH
                    currDBH += jcg.SumDBHOB * ju.ProrationFactor;
                    //  sum board foot
                    currGB += jcg.SumGBDFT * ju.ProrationFactor;
                    currNB += jcg.SumNBDFT * ju.ProrationFactor;
                    //  sum cubic foot
                    currGC += jcg.SumGCUFT * ju.ProrationFactor;
                    currNC += jcg.SumNCUFT * ju.ProrationFactor;
                    //  total logs
                    currLOGS += jcg.SumLogsMS * ju.ProrationFactor;
                    //  total EF
                    //  need method to get appropriate EF total
                    int nthRow = sList.FindIndex(
                        delegate(StratumDO s)
                        {
                            return s.Code == jcg.Stratum;
                        });
                    if (nthRow >= 0)
                        currMeth = sList[nthRow].Method;
                    switch (currMeth)
                    {
                        case "S3P":
                        case "3P":
                            if (jcg.STM == "Y")
                                currEF += jcg.SumExpanFactor;
                            else if (jcg.STM == "N")
                                currEF += jcg.TalliedTrees;
                            break;
                        default:
                            currEF += jcg.SumExpanFactor * ju.ProrationFactor;
                            break;
                    }   //  end switch
                }   //  end foreach loop
            }   //  end foreach loop

            //  load into output list
            RegionalReports r = new RegionalReports();
            if (currCS == null)
                r.value1 = " ";
            else r.value1 = currCS;
            r.value2 = currSP;
            r.value3 = currPP;
            r.value4 = currUOM;
            r.value7 = currDBH;
            r.value8 = currGB;
            r.value9 = currGC;
            r.value10 = currNB;
            r.value11 = currNC;
            r.value12 = currLOGS;
            r.value13 = currEF;
            listToOutput.Add(r);

            return;
        }   //  end AccumulateVolumes


        private void AccumulateLogMethods(List<CuttingUnitDO> justMethods, int hgtOne)
        {
            //  R102/R103
            currGRS = 0;
            currNET = 0;
            currEF = 0;
            currDBH = 0;
            currHGT = 0;
            currLOGS = 0;
            double currGTOP = 0;
            double currNTOP = 0;
            double currRECV = 0;
            double currSLP = 0;
            double slopeCnt = 0;
            double unitAC = 0;
            string[] prodList = new string[6] { "01", "02", "06", "07", "08", "14" };
            //  need unit list and PRO list
            List<CuttingUnitDO> cutList = bslyr.getCuttingUnits();
            List<PRODO> proList = bslyr.getPRO();
            //  accumulate sums by logging method
            foreach (CuttingUnitDO jm in justMethods)
            {
                //  find all units for current method
                List<CuttingUnitDO> justUnits = cutList.FindAll(
                    delegate(CuttingUnitDO c)
                    {
                        return c.LoggingMethod == jm.LoggingMethod;
                    });

                // process by product code
                for (int j = 0; j < 5; j++)
                {
                foreach (CuttingUnitDO ju in justUnits)
                {
                    unitAC += ju.Area;
                    ju.Strata.Populate();
                        //  sum up each stratum
                        foreach (StratumDO stratum in ju.Strata)
                        {
                            //  pull strata from lcdList
                            List<LCDDO> justStrata = bslyr.getLCDOrdered("WHERE CutLeave = ? AND Stratum = ? AND PrimaryProduct = ?",
                                                                        "", "C", stratum.Code, prodList[j]);
                            foreach (LCDDO js in justStrata)
                            {
                                //  find proration factor for the group
                                int nthRow = proList.FindIndex(
                                    delegate(PRODO p)
                                    {
                                        return p.CutLeave == "C" && p.Stratum == js.Stratum && p.CuttingUnit == ju.Code &&
                                            p.SampleGroup == js.SampleGroup && p.STM == js.STM && 
                                            p.PrimaryProduct == prodList[j];
                                    });
                                double proratFactor;
                                if (nthRow >= 0)
                                    proratFactor = proList[nthRow].ProrationFactor;
                                else proratFactor = 0;

                                switch (currentReport)
                                {
                                    case "R102":        //  board foot
                                        currGRS += js.SumGBDFT * proratFactor;
                                        currNET += js.SumNBDFT * proratFactor;
                                        currGTOP += js.SumGBDFTtop * proratFactor;
                                        currNTOP += js.SumNBDFTtop * proratFactor;
                                        currRECV += js.SumBDFTrecv * proratFactor;
                                        break;
                                    case "R103":        //  cubic foot
                                        currGRS += js.SumGCUFT * proratFactor;
                                        currNET += js.SumNCUFT * proratFactor;
                                        currGTOP += js.SumGCUFTtop * proratFactor;
                                        currNTOP += js.SumNCUFTtop * proratFactor;
                                        currRECV += js.SumCUFTrecv * proratFactor;
                                        break;
                                }   //  end switch on report
                                //  sum rest of values needed
                                currDBH += js.SumDBHOB * proratFactor;
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
                                }   //  end switch on height
                                currLOGS += js.SumLogsMS * proratFactor;
                                //  expansion factor is dependent on method
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
                            }   //  end foreach loop on justStrata
                            //  Sum slope percent for this stratum
                            List<PlotDO> justSlope = bslyr.GetStrataPlots(stratum.Code);
                            currSLP += justSlope.Sum(s => s.Slope);
                            slopeCnt += justSlope.Count();

                        }   //  end for loop on strata list
                    }   //  end foreach loop on stratum
                        //  load listToOutput with sums by product
                        //  primary
                        if(currGRS > 0)
                        {
                                RegionalReports rr = new RegionalReports();
                                rr.value1 = jm.LoggingMethod;
                                rr.value2 = prodList[j];
                                rr.value3 = "P";
                                rr.value7 = currGRS;
                                rr.value8 = currNET;
                                rr.value9 = currEF;
                                rr.value10 = currDBH;
                                rr.value11 = currHGT;
                                rr.value12 = currSLP;
                                rr.value13 = currLOGS;
                                rr.value14 = unitAC;
                                rr.value15 = slopeCnt;
                                listToOutput.Add(rr);
                        }   //  endif currGRS
                        //  secondary
                        if (currGTOP > 0)       
                        {
                            RegionalReports rr = new RegionalReports();
                            rr.value1 = jm.LoggingMethod;
                            if (prodList[j] == "01")
                                rr.value2 = "02";
                            else rr.value2 = prodList[j];
                            rr.value3 = "S";
                            rr.value7 = currGTOP;
                            rr.value8 = currNTOP;
                            rr.value9 = 0;
                            rr.value10 = 0;
                            rr.value11 = 0;
                            rr.value12 = 0;
                            rr.value13 = 0;
                            rr.value14 = unitAC;
                            rr.value15 = 0;
                            listToOutput.Add(rr);
                         }   //  endif
                        //  recovered
                        if (currRECV > 0)
                        {
                            RegionalReports rr = new RegionalReports();
                            rr.value1 = jm.LoggingMethod;
                            if (prodList[j] == "01")
                                rr.value2 = "02";
                            else rr.value2 = prodList[j];
                            rr.value3 = "R";
                            rr.value7 = 0;
                            rr.value8 = currRECV;
                            rr.value9 = 0;
                            rr.value10 = 0;
                            rr.value11 = 0;
                            rr.value12 = 0;
                            rr.value13 = 0;
                            rr.value14 = unitAC;
                            rr.value15 = 0;
                            listToOutput.Add(rr);
                        }   //  endif
                        currGRS = 0;
                        currNET = 0;
                        currGTOP = 0;
                        currNTOP = 0;
                        currRECV = 0;
                        currEF = 0;
                        currDBH = 0;
                        currHGT = 0;
                        currSLP = 0;
                        currLOGS = 0;
                        slopeCnt = 0;
                        unitAC = 0;
                }   //  end for loop on product list
            }   //  end foreach loop on justMethods

            return;
        }   //  end AccumulateLogMethods


        private void AccumulateBasalArea(string currST, string currCU, float currAC, string currCL)
        {
            //  R104
            List<TreeDO> tList = bslyr.getTrees();
            List<PRODO> proList = bslyr.getPRO();
            double currBA = 0;

            //  get groups from LCD
            List<LCDDO> justGroups = bslyr.GetLCDdata("WHERE Stratum = ? GROUP BY SampleGroup,Species,PrimaryProduct,CutLeave", currST);
            foreach (LCDDO jg in justGroups)
            {
                //  pull tree data for each cutting unit and group for average DBH
                //  then need all trees in the strata for the species/SG group for summed BA
                List<TreeDO> justTrees = tList.FindAll(
                      delegate(TreeDO t)
                      {
                          return t.Stratum.Code == currST && t.CuttingUnit.Code == currCU &&
                              t.Species == jg.Species && t.SampleGroup.Code == jg.SampleGroup && 
                              t.SampleGroup.PrimaryProduct == jg.PrimaryProduct && 
                              t.SampleGroup.CutLeave == currCL;
                      });
                List<TreeDO> allTrees = tList.FindAll(
                    delegate(TreeDO t)
                    {
                        return t.Stratum.Code == currST && t.Species == jg.Species &&
                            t.SampleGroup.Code == jg.SampleGroup &&
                            t.SampleGroup.PrimaryProduct == jg.PrimaryProduct &&
                            t.SampleGroup.CutLeave == currCL;
                    });
                //  are there trees in this strata and cutting unit to process?
                if (justTrees.Count > 0)
                {
                    //  sum up dbh and ef for average dbh
                    currDBH = justTrees.Sum(j => j.DBH * j.ExpansionFactor);
                    //  calculate and sum basal area 
                    currBA = allTrees.Sum(j => (0.005454 * Math.Pow(j.DBH, 2.0)) * j.ExpansionFactor);
                    //  also need sum of EF
                    currEF = justTrees.Sum(j => j.ExpansionFactor);

                    //  what's the proration factor for this group?
                    int nthRow = proList.FindIndex(
                        delegate(PRODO p)
                        {
                            return p.Stratum == currST && p.CuttingUnit == currCU &&
                                        p.SampleGroup == jg.SampleGroup && 
                                        p.PrimaryProduct == jg.PrimaryProduct &&
                                        p.CutLeave == jg.CutLeave;
                        });
                    //  store this group for printing
                    RegionalReports r = new RegionalReports();
                    r.value1 = currST;
                    r.value2 = currCU;
                    r.value3 = jg.Species;
                    r.value4 = jg.SampleGroup;
                    r.value5 = jg.PrimaryProduct;
                    r.value6 = currCL;
                    r.value7 = currAC;
                    if (nthRow >= 0)
                        r.value8 = proList[nthRow].ProrationFactor;
                    else r.value8 = 1.0;
                    r.value9 = currBA;
                    r.value10 = currDBH;
                    r.value11 = currEF;
                    listToOutput.Add(r);
                    currBA = 0;
                    currDBH = 0;
                }   //  endif trees in this strata and cutting unit
            }   //  end foreach loop on groups

            return;
        }   //  end AccumulateBasalArea


        private void WriteCurrentGroup(StreamWriter strWriteOut, ref int pageNum, reportHeaders rh)
        {
            //  works for R101
            double calcValue = 0;
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                    rRH.R101columns, 13, ref pageNum, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(lto.value1.PadRight(4, ' '));
                prtFields.Add(lto.value2.PadRight(6, ' '));
                prtFields.Add(lto.value3.PadLeft(2, '0'));
                prtFields.Add(lto.value4.PadLeft(2, '0'));
                //  average defect
                if (lto.value8 > 0.0)
                    calcValue = ((lto.value8 - lto.value10) / lto.value8) * 100;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(4, ' '));
                if (lto.value9 > 0)
                    calcValue = ((lto.value9 - lto.value11) / lto.value9) * 100;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(4, ' '));
                //  Gross and net ratios
                if (lto.value9 > 0)
                    calcValue = lto.value8 / lto.value9;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,7:F4}").ToString().PadLeft(7, ' '));
                if (lto.value11 > 0)
                    calcValue = lto.value10 / lto.value11;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,7:F4}").ToString().PadLeft(7, ' '));
                //  Average DBH
                if (lto.value13 > 0)
                    calcValue = lto.value7 / lto.value13;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                //  gross volume
                prtFields.Add(Utilities.FormatField(lto.value8, "{0,10:F0}").ToString().PadLeft(10, ' '));
                prtFields.Add(Utilities.FormatField(lto.value9, "{0,10:F0}").ToString().PadLeft(10, ' '));
                //  net volume
                prtFields.Add(Utilities.FormatField(lto.value10, "{0,10:F0}").ToString().PadLeft(10, ' '));
                prtFields.Add(Utilities.FormatField(lto.value11, "{0,10:F0}").ToString().PadLeft(10, ' '));
                //  16 foot log volume
                if (lto.value9 > 0)
                    calcValue = lto.value12 / (lto.value9 / 100);
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                if (lto.value8 > 0)
                    calcValue = lto.value12 / (lto.value8 / 1000);
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            return;
        }   //  end WriteCurrentGroup


        private void WriteCurrentMethods(StreamWriter strWriteOut, ref int pageNum, reportHeaders rh)
        {
            //  R102/R103
            //  print by method and product
            string prevPP = "*";
            string prevLM = "*";
            double calcValue = 0;
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                    completeHeader, 15, ref pageNum, "");
                if (prevPP == "*") prevPP = lto.value2;
                if (prevLM == "*") prevLM = lto.value1;
                if (prevPP != lto.value2)
                {
                    //  output product subtotal
                    outputTotal(strWriteOut, ref pageNum, rh, 1, prevPP, firstSubtotal);
                    firstSubtotal.Clear();
                    prevPP = lto.value2;
                }   //  endif
                if (prevLM != lto.value1)
                {
                    //  update overall total
                    updateTotalTotal();
                    //  output logging method subtotal
                    outputTotal(strWriteOut, ref pageNum, rh, 2, prevLM, thirdSubtotal);
                    thirdSubtotal.Clear();
                    prevLM = lto.value1;
                }   //  endif
                prtFields.Clear();
                prtFields.Add("");
                switch (lto.value3)
                {
                    case "P":
                        prtFields.Add(lto.value1.PadLeft(3, ' '));
                        prtFields.Add(lto.value2.PadLeft(2,' '));
                        prtFields.Add(lto.value3);
                        //  gross volume
                        prtFields.Add(Utilities.FormatField(lto.value7, "{0,10:F0}").ToString().PadLeft(10, ' '));
                        //  defect percent
                        if (lto.value7 > 0)
                            calcValue = ((lto.value7 - lto.value8) / lto.value7) * 100;
                        else calcValue = 0;
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                        //  net volume
                        prtFields.Add(Utilities.FormatField(lto.value8, "{0,10:F0}").ToString().PadLeft(10, ' '));
                        //  estimated trees
                        prtFields.Add(Utilities.FormatField(lto.value9, "{0,8:F0}").ToString().PadLeft(8, ' '));
                        //  gross and net volume per acre and trees per acre
                        calcValue = lto.value7 / lto.value14;
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,10:F0}").ToString().PadLeft(10, ' '));
                        calcValue = lto.value8 / lto.value14;
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,10:F0}").ToString().PadLeft(10, ' '));
                        calcValue = lto.value9 / lto.value14;
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(6, ' '));
                        //  mean DBH and mean height
                        if (lto.value9 > 0)
                            calcValue = lto.value10 / lto.value9;
                        else calcValue = 0;
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                        if (lto.value9 > 0)
                            calcValue = lto.value11 / lto.value9;
                        else calcValue = 0;
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,7:F1}").ToString().PadLeft(7, ' '));
                        //  net per tree
                        if(lto.value9 > 0)
                            calcValue = lto.value8/lto.value9;
                        else calcValue = 0;
                        prtFields.Add(Utilities.FormatField(calcValue,"{0,8:F0}").ToString().PadLeft(8,' '));
                        //  logs per volume
                        if(lto.value7 > 0)
                        {
                            if(currentReport == "R102")
                                calcValue = lto.value13 / (lto.value7 / 1000);
                            else if(currentReport == "R103")
                                calcValue = lto.value13 / (lto.value7 / 100);
                        }   
                        else calcValue = 0;
                        prtFields.Add(Utilities.FormatField(calcValue,"{0,6:F1}").ToString().PadLeft(6,' '));
                        //  average slope
                        if(lto.value15 > 0)
                            calcValue = lto.value12 / lto.value15;
                        else calcValue = 0;
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                        //  output acres
                        prtFields.Add(Utilities.FormatField(lto.value14, "{0,8:F1}").ToString().PadLeft(8, ' '));
                        break;
                    case "S":
                        //  gross volume
                        prtFields.Add(Utilities.FormatField(lto.value7, "{0,10:F0}").ToString().PadLeft(10, ' '));
                        //  no total defect
                        prtFields.Add("  0.0");
                        //  net volume
                        prtFields.Add(Utilities.FormatField(lto.value8, "{0,10:F0}").ToString().PadLeft(10, ' '));
                        //  no estimated trees
                        prtFields.Add("       0");
                        //  gross and net volume per acre
                        if (lto.value14 > 0)
                        {
                            calcValue = lto.value7 / lto.value14;
                            prtFields.Add(Utilities.FormatField(calcValue, "{0,10:F0}").ToString().PadLeft(10, ' '));
                            calcValue = lto.value8 / lto.value14;
                            prtFields.Add(Utilities.FormatField(calcValue, "{0,10:f)}").ToString().PadLeft(10, ' '));
                        }   //  endif
                        // next fields are zero
                        prtFields.Add("   0.0");
                        prtFields.Add("  0.0");
                        prtFields.Add("    0.0");
                        prtFields.Add("       0");
                        //  logs per gross volume
                        if (lto.value7 > 0)
                        {
                            if (currentReport == "R102")
                                calcValue = lto.value13 / (lto.value7 / 1000);
                            else if (currentReport == "R103")
                                calcValue = lto.value13 / (lto.value7 / 100);
                        }   //  endif
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(6, ' '));
                        prtFields.Add("  0.0");
                        prtFields.Add("     N/A");
                        break;
                    case "R":        //  calculated values not shown
                        prtFields.Add(lto.value1.PadLeft(3, ' '));
                        prtFields.Add(lto.value2.PadLeft(2, ' '));
                        prtFields.Add(lto.value3);
                        //  gross volume
                        prtFields.Add(Utilities.FormatField(lto.value7, "{0,10:F0}").ToString().PadLeft(10, ' '));
                        prtFields.Add("  0.0");
                        //  net volume
                        prtFields.Add(Utilities.FormatField(lto.value8, "{0,10:F0}").ToString().PadLeft(10, ' '));
                        //  no estimated trees
                        prtFields.Add("       0");
                        //  gross and net volume per acre
                        prtFields.Add("         0");
                        calcValue = lto.value8 / lto.value14;
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,10:F0}").ToString().PadLeft(10, ' '));
                        //  skip next fields
                        prtFields.Add("   0.0");
                        prtFields.Add("  0.0");
                        prtFields.Add("    0.0");
                        prtFields.Add("       0");
                        //  logs per gross volume
                        if (lto.value7 > 0)
                        {
                            if (currentReport == "R102")
                                calcValue = lto.value13 / (lto.value7 / 1000);
                            else if (currentReport == "R103")
                                calcValue = lto.value13 / (lto.value7 / 100);
                        }   //  endif
                        prtFields.Add(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(6, ' '));
                        prtFields.Add("  0.0");
                        prtFields.Add("     N/A");
                        break;
                }   //  end switch on product
                printOneRecord(fieldLengths, prtFields, strWriteOut);
                //  update product subtotal and logging method subtotal
                updateSubtotal(lto, 1);
                updateSubtotal(lto, 2);

            }   //  end foreach loop
            //  output last product and logging method subtotal
            outputTotal(strWriteOut, ref pageNum, rh, 1, prevPP, firstSubtotal);
            outputTotal(strWriteOut, ref pageNum, rh, 2, prevLM, thirdSubtotal);
            updateTotalTotal();
            return;
        }   //  end WriteCurrentMethod


        private void WriteCurrentUnit(StreamWriter strWriteOut, ref int pageNum, reportHeaders rh)
        {
            //  R104
            double calcValue = 0;
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                    rRH.R104columns, 10, ref pageNum, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(lto.value1.PadLeft(2, ' '));
                prtFields.Add(lto.value2.PadLeft(3, ' '));
                prtFields.Add(lto.value3.PadLeft(6, ' '));
                prtFields.Add(lto.value4.PadLeft(2, ' '));
                prtFields.Add(lto.value5.PadLeft(2, ' '));
                prtFields.Add(lto.value6);
                //  average DBH
                if (lto.value11 > 0)
                    calcValue = lto.value10 / lto.value11;
                else calcValue = 0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,4:F1}").ToString().PadLeft(4, ' '));
                //  basal area
                if (lto.value7 > 0)
                    calcValue = (lto.value9 * lto.value8) / lto.value7;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));

                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            return;
        }   //  end WriteCurrentUnit


        private void updateSubtotal(List<ReportSubtotal> totalToUpdate)
        {
            //  R101
            if (totalToUpdate.Count > 0)
            {
                foreach (RegionalReports lto in listToOutput)
                {
                    totalToUpdate[0].Value7 += lto.value7;
                    totalToUpdate[0].Value8 += lto.value8;
                    totalToUpdate[0].Value9 += lto.value9;
                    totalToUpdate[0].Value10 += lto.value10;
                    totalToUpdate[0].Value11 += lto.value11;
                    totalToUpdate[0].Value12 += lto.value12;
                    totalToUpdate[0].Value13 += lto.value13;
                }   //  end foreach loop
            }
            else if (totalToUpdate.Count == 0)
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
                    totalToUpdate.Add(r);
                }   //  end foreach loop
            }   //  endif
            return;
        }   //  end updateSubtotal


        private void updateTotal()
        {
            //  R101
            if (totalToOutput.Count > 0)
            {
                totalToOutput[0].Value7 += secondSubtotal[0].Value7;
                totalToOutput[0].Value8 += secondSubtotal[0].Value8;
                totalToOutput[0].Value9 += secondSubtotal[0].Value9;
                totalToOutput[0].Value10 += secondSubtotal[0].Value10;
                totalToOutput[0].Value11 += secondSubtotal[0].Value11;
                totalToOutput[0].Value12 += secondSubtotal[0].Value12;
                totalToOutput[0].Value13 += secondSubtotal[0].Value13;
            }
            else if(totalToOutput.Count == 0)
            {
                ReportSubtotal r = new ReportSubtotal();
                r.Value7 = secondSubtotal[0].Value7;
                r.Value8 = secondSubtotal[0].Value8;
                r.Value9 = secondSubtotal[0].Value9;
                r.Value10 = secondSubtotal[0].Value10;
                r.Value11 = secondSubtotal[0].Value11;
                r.Value12 = secondSubtotal[0].Value12;
                r.Value13 = secondSubtotal[0].Value13;
                totalToOutput.Add(r);
            }   //  endif
            return;
        }   //  end updateTotal


        private void updateSubtotal(RegionalReports oneObject, int whichSubtotal)
        {
            //  R102/R103
            switch (whichSubtotal)
            {
                case 1:         //  product subtotal
                    if (firstSubtotal.Count > 0)
                    {
                        firstSubtotal[0].Value7 += oneObject.value7;
                        firstSubtotal[0].Value8 += oneObject.value8;
                        firstSubtotal[0].Value9 += oneObject.value9;
                        firstSubtotal[0].Value10 += oneObject.value10;
                        firstSubtotal[0].Value11 += oneObject.value11;
                        firstSubtotal[0].Value12 += oneObject.value12;
                        firstSubtotal[0].Value13 += oneObject.value13;
                        firstSubtotal[0].Value14 = oneObject.value14;
                        firstSubtotal[0].Value15 += oneObject.value15;
                    }
                    else
                    {
                        ReportSubtotal r = new ReportSubtotal();
                        r.Value7 = oneObject.value7;
                        r.Value8 = oneObject.value8;
                        r.Value9 = oneObject.value9;
                        r.Value10 = oneObject.value10;
                        r.Value11 = oneObject.value11;
                        r.Value12 = oneObject.value12;
                        r.Value13 = oneObject.value13;
                        r.Value14 = oneObject.value14;
                        r.Value15 = oneObject.value15;
                        firstSubtotal.Add(r);
                    }   //  endif
                    break;
                case 2:         //  logging method subtotal
                    if (thirdSubtotal.Count > 0)
                    {
                        thirdSubtotal[0].Value7 += oneObject.value7;
                        thirdSubtotal[0].Value8 += oneObject.value8;
                        thirdSubtotal[0].Value9 += oneObject.value9;
                        thirdSubtotal[0].Value10 += oneObject.value10;
                        thirdSubtotal[0].Value11 += oneObject.value11;
                        thirdSubtotal[0].Value12 += oneObject.value12;
                        thirdSubtotal[0].Value13 += oneObject.value13;
                        thirdSubtotal[0].Value14 = oneObject.value14;
                        thirdSubtotal[0].Value15 += oneObject.value15;
                    }
                    else
                    {
                        ReportSubtotal r = new ReportSubtotal();
                        r.Value7 = oneObject.value7;
                        r.Value8 = oneObject.value8;
                        r.Value9 = oneObject.value9;
                        r.Value10 = oneObject.value10;
                        r.Value11 = oneObject.value11;
                        r.Value12 = oneObject.value12;
                        r.Value13 = oneObject.value13;
                        r.Value14 = oneObject.value14;
                        r.Value15 = oneObject.value15;
                        thirdSubtotal.Add(r);
                    }   //  endif
                    break;
            }   //  end switch
            return;
        }   //  end updateSubtotal


        private void updateTotalTotal()
        {
            //  R102/R103
            if (totalToOutput.Count > 0)
            {
                totalToOutput[0].Value7 += thirdSubtotal[0].Value7;
                totalToOutput[0].Value8 += thirdSubtotal[0].Value8;
                totalToOutput[0].Value9 += thirdSubtotal[0].Value9;
                totalToOutput[0].Value10 += thirdSubtotal[0].Value10;
                totalToOutput[0].Value11 += thirdSubtotal[0].Value11;
                totalToOutput[0].Value12 += thirdSubtotal[0].Value12;
                totalToOutput[0].Value13 += thirdSubtotal[0].Value13;
                totalToOutput[0].Value14 += thirdSubtotal[0].Value14;
                totalToOutput[0].Value15 += thirdSubtotal[0].Value15;
            }
            else
            {
                ReportSubtotal r = new ReportSubtotal();
                r.Value7 = thirdSubtotal[0].Value7;
                r.Value8 = thirdSubtotal[0].Value8;
                r.Value9 = thirdSubtotal[0].Value9;
                r.Value10 = thirdSubtotal[0].Value10;
                r.Value11 = thirdSubtotal[0].Value11;
                r.Value12 = thirdSubtotal[0].Value12;
                r.Value13 = thirdSubtotal[0].Value13;
                r.Value14 = thirdSubtotal[0].Value14;
                r.Value15 = thirdSubtotal[0].Value15;
                totalToOutput.Add(r);
            }   //  endif
            return;
        }   //  end updateTotalTotal


        private void updateUnitOrStrata(int UnitOrStrata, string cutLeave)
        {
            //  R104
            switch (UnitOrStrata)
            {
                case 1:         //  update unit subtotal
                    if (secondSubtotal.Count > 0)
                    {
                        secondSubtotal[0].Value7 += listToOutput.Sum(l => l.value7);
                        secondSubtotal[0].Value8 += listToOutput.Sum(l => l.value8);
                        secondSubtotal[0].Value9 += listToOutput.Sum(l => l.value9);
                        secondSubtotal[0].Value10 += listToOutput.Sum(l => l.value10);
                        secondSubtotal[0].Value11 += listToOutput.Sum(l => l.value11);
                    }
                    else 
                    {
                        ReportSubtotal r = new ReportSubtotal();
                        r.Value7 = listToOutput.Sum(l => l.value7);
                        r.Value8 = listToOutput.Sum(l => l.value8);
                        r.Value9 = listToOutput.Sum(l => l.value9);
                        r.Value10 = listToOutput.Sum(l => l.value10);
                        r.Value11 = listToOutput.Sum(l => l.value11);
                        secondSubtotal.Add(r);
                    }   //  endif
                    break;
                case 2:         //  update strata subtotal
                    if (totalToOutput.Count > 0)
                    {
                        if (totalToOutput[0].Value1 == cutLeave)
                        {
                            totalToOutput[0].Value7 += listToOutput[0].value7;
                            totalToOutput[0].Value9 += listToOutput.Sum(l => (l.value9 * l.value8));
                            totalToOutput[0].Value10 += listToOutput.Sum(l => l.value10);
                            totalToOutput[0].Value11 += listToOutput.Sum(l => l.value11);
                        }
                        else if (totalToOutput[1].Value1 == cutLeave)
                        {
                            if (listToOutput.Count > 0)
                            {
                                totalToOutput[1].Value7 += listToOutput[0].value7;
                                totalToOutput[1].Value9 += listToOutput.Sum(l => (l.value9 * l.value8));
                                totalToOutput[1].Value10 += listToOutput.Sum(l => l.value10);
                                totalToOutput[1].Value11 += listToOutput.Sum(l => l.value11);
                            }   //  endif
                        }   //  endif
                    }   //  endif
                    break;
                case 3:         //  update cut or leave subtotal
                    if (firstSubtotal.Count > 0)
                    {
                        firstSubtotal[0].Value7 += listToOutput.Sum(l => l.value7);
                        firstSubtotal[0].Value8 += listToOutput.Sum(l => l.value8);
                        firstSubtotal[0].Value9 += listToOutput.Sum(l => l.value9);
                        firstSubtotal[0].Value10 += listToOutput.Sum(l => l.value10);
                        firstSubtotal[0].Value11 += listToOutput.Sum(l => l.value11);
                    }
                    else
                    {
                        ReportSubtotal r = new ReportSubtotal();
                        r.Value7 = listToOutput.Sum(l => l.value7);
                        r.Value8 = listToOutput.Sum(l => l.value8);
                        r.Value9 = listToOutput.Sum(l => l.value9);
                        r.Value10 = listToOutput.Sum(l => l.value10);
                        r.Value11 = listToOutput.Sum(l => l.value11);
                        firstSubtotal.Add(r);
                    }   //  endif
                    break;
            }   //  end switch
            return;
        }   //  end updateUnitOrStrata


        private void outputTotal(int lineType, string currTotal, List<ReportSubtotal> totalsLine,
                                StreamWriter strWriteOut, ref int pageNum, reportHeaders rh)
        {
            //  writes subtotal line for any subtotal in R101
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                    rRH.R101columns, 13, ref pageNum, "");
            switch (lineType)
            {
                case 1:         //  product subtotal

                    strWriteOut.WriteLine("                         __________________________________________________________________________________________________________");
                    strWriteOut.Write("        PRODUCT ");
                    strWriteOut.Write(currTotal.PadLeft(2, ' '));
                    strWriteOut.Write(" TOTAL");
                    break;
                case 2:         //  contract species subtotal
                    strWriteOut.WriteLine("                         __________________________________________________________________________________________________________");
                    strWriteOut.Write("   CONTR SPEC ");
                    strWriteOut.Write(currTotal.PadLeft(4, ' '));
                    strWriteOut.Write(" TOTAL");
                    break;
                case 3:         //  overall contract species total
                    strWriteOut.WriteLine(reportConstants.longLine);
                    strWriteOut.WriteLine(reportConstants.longLine);
                    strWriteOut.Write("CONTRACT SPECIES TOTALS ");
                    break;
            }   //  end switch on line type

            //  rest of fields are the same for all three total lines
            //  average defect
            if (totalsLine[0].Value8 > 0.0)
                calcValue = ((totalsLine[0].Value8 - totalsLine[0].Value10) / totalsLine[0].Value8) * 100;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(4, ' '));
            if (totalsLine[0].Value9 > 0.0)
                calcValue = ((totalsLine[0].Value9 - totalsLine[0].Value11) / totalsLine[0].Value9) * 100;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(7, ' '));
            //  Gross and net ratios
            if (totalsLine[0].Value9 > 0.0)
                calcValue = totalsLine[0].Value8 / totalsLine[0].Value9;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,7:F4}").ToString().PadLeft(11, ' '));
            if (totalsLine[0].Value11 > 0.0)
                calcValue = totalsLine[0].Value10 / totalsLine[0].Value11;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,7:F4}").ToString().PadLeft(11));
            //  Average DBH
            if (totalsLine[0].Value13 > 0)
                calcValue = totalsLine[0].Value7 / totalsLine[0].Value13;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(10, ' '));
            //  gross volume
            strWriteOut.Write(Utilities.FormatField(totalsLine[0].Value8, "{0,10:F0}").ToString().PadLeft(11, ' '));
            strWriteOut.Write(Utilities.FormatField(totalsLine[0].Value9, "{0,10:F0}").ToString().PadLeft(12, ' '));
            //  net volume
            strWriteOut.Write(Utilities.FormatField(totalsLine[0].Value10, "{0,10:F0}").ToString().PadLeft(11, ' '));
            strWriteOut.Write(Utilities.FormatField(totalsLine[0].Value11, "{0,10:F0}").ToString().PadLeft(11, ' '));
            //  16 foot log volume
            if (totalsLine[0].Value9 > 0.0)
                calcValue = totalsLine[0].Value12 / (totalsLine[0].Value9 / 100);
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(9, ' '));
            if (totalsLine[0].Value8 > 0.0)
                calcValue = totalsLine[0].Value12 / (totalsLine[0].Value8 / 1000);
            strWriteOut.WriteLine(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(10, ' '));
            strWriteOut.WriteLine("");
            return;
        }   //  end outputTotal


        private void outputTotal(StreamWriter strWriteOut, ref int pageNum, reportHeaders rh,
                                int lineType, string currTotal, List<ReportSubtotal> totalsLine)
        {
            //  write subtotal or total line for any subtotal in R102/R103
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                completeHeader, 13, ref pageNum, "");
            switch (lineType)
            {
                case 1:     //  product subtotal
                    strWriteOut.WriteLine("            _________________________________________________________________________________________________________________________");
                    strWriteOut.Write(currTotal.PadLeft(4,' '));
                    strWriteOut.Write(" TOTAL  ");
                    break;
                case 2:     //  logging method subtotal
                   strWriteOut.WriteLine("            _________________________________________________________________________________________________________________________");
                   strWriteOut.Write(currTotal.PadLeft(4, ' '));
                   strWriteOut.Write(" TOTAL  ");
                   break;
                case 3:     //  overall total
                   strWriteOut.WriteLine(reportConstants.longLine);
                   strWriteOut.Write(" TOTALS/AVE ");
                   break;
            }   //  end switch on lineType

            //  rest of data is the same for all three subtotals and total
            //  gross volume
            strWriteOut.Write(Utilities.FormatField(totalsLine[0].Value7, "{0,10:F0}").ToString().PadLeft(10, ' '));
            //  defect percent
            if (totalsLine[0].Value7 > 0)
                calcValue = ((totalsLine[0].Value7 - totalsLine[0].Value8) / totalsLine[0].Value7) * 100;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(8, ' '));
            //  net volume
            strWriteOut.Write(Utilities.FormatField(totalsLine[0].Value8, "{0,10:F0}").ToString().PadLeft(11, ' '));
            //  estimated trees
            strWriteOut.Write(Utilities.FormatField(totalsLine[0].Value9, "{0,8:F0}").ToString().PadLeft(9, ' '));
            //  gross and net volume per acre and trees per acre
            calcValue = totalsLine[0].Value7 / totalsLine[0].Value14;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,10:F0}").ToString().PadLeft(11, ' '));
            calcValue = totalsLine[0].Value8 / totalsLine[0].Value14;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,10:F0}").ToString().PadLeft(11, ' '));
            calcValue = totalsLine[0].Value9 / totalsLine[0].Value14;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(7, ' '));
            //  mean DBH and mean height
            if (totalsLine[0].Value9 > 0)
                calcValue = totalsLine[0].Value10 / totalsLine[0].Value9;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(8, ' '));
            if (totalsLine[0].Value9 > 0)
                calcValue = totalsLine[0].Value11 / totalsLine[0].Value9;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,7:F1}").ToString().PadLeft(9, ' '));
            //  net per tree
            if (totalsLine[0].Value9 > 0)
                calcValue = totalsLine[0].Value8 / totalsLine[0].Value9;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(9, ' '));
            //  logs per volume
            if (totalsLine[0].Value7 > 0)
            {
                if (currentReport == "R102")
                    calcValue = totalsLine[0].Value13 / (totalsLine[0].Value7 / 1000);
                else if (currentReport == "R103")
                    calcValue = totalsLine[0].Value13 / (totalsLine[0].Value7 / 100);
            }
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,6:F1}").ToString().PadLeft(7, ' '));
            //  average slope
            if (totalsLine[0].Value15 > 0)
                calcValue = totalsLine[0].Value12 / totalsLine[0].Value15;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(8, ' '));
            //  output acres
            strWriteOut.WriteLine(Utilities.FormatField(totalsLine[0].Value14, "{0,8:F1}").ToString().PadLeft(10, ' '));
            strWriteOut.WriteLine("");
            if (lineType == 2)
            {
                strWriteOut.WriteLine(reportConstants.longLine);
                strWriteOut.WriteLine(reportConstants.longLine);
            }   //  endif
            return;
        }   //  end outputTotal

        private void AccumulateValuesAndPrint(StreamWriter strWriteOut,
                                        List<CuttingUnitDO> cList, 
                                        List<LCDDO> lcdList)
        {
            // R105
            List<PRODO> pList = bslyr.getPRO();
            foreach (CuttingUnitDO cd in cList)
            {
                cd.Strata.Populate();
                string currCU = cd.Code;
                //  find  proration factors for each cutting unit
                int nthRow = pList.FindIndex(
                    delegate(PRODO pl)
                    {
                        return pl.CuttingUnit == currCU;
                    });

                //Accumulate value for each cutting unit within each stratum
                foreach (StratumDO stratum in cd.Strata)
                {
                    //  find all stratum in LCD for sawtimber
                    List<LCDDO> currSTR = lcdList.FindAll(
                        delegate(LCDDO ld)
                        {
                            return ld.Stratum == stratum.Code &&
                                   ld.PrimaryProduct == "01" &&
                                    ld.CutLeave == "C";
                        });

                    foreach (LCDDO cs in currSTR)
                    {
                        //  Is unit already in the list?
                        //  Accumulate  prorated values for current unit
                        int kRow = listToOutput.FindIndex(
                            delegate(RegionalReports lto)
                            {
                                return lto.value1 == currCU;
                            });

                        if (kRow >= 0)
                        {
                            listToOutput[kRow].value9 += cs.SumGBDFT * pList[nthRow].ProrationFactor;
                            listToOutput[kRow].value10 += cs.SumGCUFT * pList[nthRow].ProrationFactor;
                            listToOutput[kRow].value11 += cs.SumNBDFT * pList[nthRow].ProrationFactor;
                            listToOutput[kRow].value12 += cs.SumNCUFT * pList[nthRow].ProrationFactor;
                        }
                        else if (kRow < 0)
                        {
                            RegionalReports rr = new RegionalReports();
                            rr.value1 = cd.Code;
                            rr.value8 = cd.Area;
                            rr.value9 += cs.SumGBDFT * pList[nthRow].ProrationFactor;
                            rr.value10 += cs.SumGCUFT * pList[nthRow].ProrationFactor;
                            rr.value11 += cs.SumNBDFT * pList[nthRow].ProrationFactor;
                            rr.value12 += cs.SumNCUFT * pList[nthRow].ProrationFactor;
                            listToOutput.Add(rr);
                        }   //  ednif
                    }  //  end foreach
                }   //  end for j loop
                //  now need everything that's not sawtimber
                foreach (StratumDO stratum in cd.Strata)
                {
                    //  find all stratum in LCD for non-sawtimber
                    List<LCDDO> currSTR = lcdList.FindAll(
                        delegate(LCDDO ld)
                        {
                            return ld.Stratum == stratum.Code &&
                                   ld.PrimaryProduct !="01" &&
                                    ld.CutLeave == "C";
                        });

                    foreach (LCDDO cs in currSTR)
                    {
                        //  Is unit already in the list?
                        //  Accumulate  prorated values for current unit
                        int kRow = listToOutput.FindIndex(
                            delegate(RegionalReports lto)
                            {
                                return lto.value1 == currCU;
                            });

                        if (kRow >= 0)
                        {
                            listToOutput[kRow].value13 += cs.SumGBDFT * pList[nthRow].ProrationFactor;
                            listToOutput[kRow].value14 += cs.SumGCUFT * pList[nthRow].ProrationFactor;
                            listToOutput[kRow].value15 += cs.SumNBDFT * pList[nthRow].ProrationFactor;
                            listToOutput[kRow].value16 += cs.SumNCUFT * pList[nthRow].ProrationFactor; 
                            listToOutput[kRow].value13 += cs.SumGBDFTtop * pList[nthRow].ProrationFactor;
                            listToOutput[kRow].value14 += cs.SumGCUFTtop * pList[nthRow].ProrationFactor;
                            listToOutput[kRow].value15 += cs.SumNBDFTtop * pList[nthRow].ProrationFactor;
                            listToOutput[kRow].value16 += cs.SumNCUFTtop * pList[nthRow].ProrationFactor;
                        
                        }
                        else if (kRow < 0)
                        {
                            RegionalReports rr = new RegionalReports();
                            rr.value1 = cd.Code;
                            rr.value8 = cd.Area;
                            rr.value13 += cs.SumGBDFT * pList[nthRow].ProrationFactor;
                            rr.value14 += cs.SumGCUFT * pList[nthRow].ProrationFactor;
                            rr.value15 += cs.SumNBDFT * pList[nthRow].ProrationFactor;
                            rr.value16 += cs.SumNCUFT * pList[nthRow].ProrationFactor;
                            rr.value13 += cs.SumGBDFTtop * pList[nthRow].ProrationFactor;
                            rr.value14 += cs.SumGCUFTtop * pList[nthRow].ProrationFactor;
                            rr.value15 += cs.SumNBDFTtop * pList[nthRow].ProrationFactor;
                            rr.value16 += cs.SumNCUFTtop * pList[nthRow].ProrationFactor;
                            listToOutput.Add(rr);
                        }   //  ednif
                    }  //  end foreach
                }  //  end for j loop
            }   //  end foreach

            return;
        }   //end AccumulateValueAndPRint

        private void AccumulateSubtotalAndPrint(StreamWriter strWriteOut, List<CuttingUnitDO> cList,
                                                    List<LCDDO> lList)
        {
            //  R105
            listToOutput.Clear();
            //  Get unique species from LCD
            string[,] justSpecies = bslyr.GetUniqueSpeciesProduct();
            for (int k = 0; k < justSpecies.GetLength(0); k++)
            {
                List<LCDDO> currentUnit = new List<LCDDO>();
                if (justSpecies[k, 0] != null && justSpecies[k, 1] == "01")
                {
                    //  find all species in lcd list
                    currentUnit = lList.FindAll(
                        delegate(LCDDO t)
                        {
                            return t.Species == justSpecies[k, 0] &&
                                t.PrimaryProduct == "01" &&
                                t.CutLeave == "C";
                        });
                }   //endif
                RegionalReports rr = new RegionalReports();
                RegionalReports rrt = new RegionalReports();
                if (currentUnit.Count != 0)
                {
                    foreach (LCDDO cu in currentUnit)
                    {
                        //  What method is the stratum on this tree?
                        //  And what is the strata acres for plot based methods
                        List<StratumDO> sList = bslyr.getStratum();
                        int mthRow = sList.FindIndex(
                            delegate (StratumDO s)
                            {
                                return s.Code == cu.Stratum;
                            });
                        double strAcres = Utilities.ReturnCorrectAcres(cu.Stratum, bslyr,
                                    (long)sList[mthRow].Stratum_CN);

                        //  is species already in the list?
                        int kRow = listToOutput.FindIndex(
                            delegate(RegionalReports lto)
                            {
                                return lto.value1 == justSpecies[k, 0];
                            });
                        if (kRow >= 0)
                        {
                            listToOutput[kRow].value9 += cu.SumGBDFT * strAcres;
                            listToOutput[kRow].value10 += cu.SumGCUFT * strAcres;
                            listToOutput[kRow].value11 += cu.SumNBDFT * strAcres;
                            listToOutput[kRow].value12 += cu.SumNCUFT * strAcres;
                            listToOutput[kRow].value13 += cu.SumGBDFTtop * strAcres;
                            listToOutput[kRow].value14 += cu.SumGCUFTtop * strAcres;
                            listToOutput[kRow].value15 += cu.SumNBDFTtop * strAcres;
                            listToOutput[kRow].value16 += cu.SumNCUFTtop * strAcres;
                        }
                        else if (kRow < 0)
                        {
                            rr.value1 = justSpecies[k, 0];
                            rr.value9 += cu.SumGBDFT * strAcres;
                            rr.value10 += cu.SumGCUFT * strAcres;
                            rr.value11 += cu.SumNBDFT * strAcres;
                            rr.value12 += cu.SumNCUFT * strAcres;
                            rr.value13 += cu.SumGBDFTtop * strAcres;
                            rr.value14 += cu.SumGCUFTtop * strAcres;
                            rr.value15 += cu.SumNBDFTtop * strAcres;
                            rr.value16 += cu.SumNCUFTtop * strAcres;
                            listToOutput.Add(rr);
                        }   //  endif
                            // add logic to total the volumes
                        int nthRow = listTotalOutput.FindIndex(
                        delegate (RegionalReports lto)
                        {
                            return lto.value1 == cu.PrimaryProduct;
                        });
                        if (nthRow >= 0)
                        {
                            listTotalOutput[nthRow].value9 += cu.SumGBDFT * strAcres;
                            listTotalOutput[nthRow].value10 += cu.SumGCUFT * strAcres;
                            listTotalOutput[nthRow].value11 += cu.SumNBDFT * strAcres;
                            listTotalOutput[nthRow].value12 += cu.SumNCUFT * strAcres;
                            //  also add any secondaary volume
                            listTotalOutput[nthRow].value13 += cu.SumGBDFTtop * strAcres;
                            listTotalOutput[nthRow].value14 += cu.SumGCUFTtop * strAcres;
                            listTotalOutput[nthRow].value15 += cu.SumNBDFTtop * strAcres;
                            listTotalOutput[nthRow].value16 += cu.SumNCUFTtop * strAcres;
                        }
                        else if (nthRow < 0)
                        {
                            rrt.value1 = cu.PrimaryProduct;
                            rrt.value2 = "P";
                            rrt.value3 = cu.SecondaryProduct;
                            rrt.value4 = "S";
                            rrt.value9 = cu.SumGBDFT * strAcres;
                            rrt.value10 = cu.SumGCUFT * strAcres;
                            rrt.value11 = cu.SumNBDFT * strAcres;
                            rrt.value12 = cu.SumNCUFT * strAcres;
                            rrt.value13 = cu.SumGBDFTtop * strAcres;
                            rrt.value14 = cu.SumGCUFTtop * strAcres;
                            rrt.value15 = cu.SumNBDFTtop * strAcres;
                            rrt.value16 = cu.SumNCUFTtop * strAcres;
                            listTotalOutput.Add(rrt);
                        }   //  endif
                    }   //  end foreach loop
                }   //  endif no records
            }   //  end for k loop

            //  accumulate non-sawtimber
            for (int k = 0; k < justSpecies.GetLength(0); k++)
            {
                List<LCDDO> currentUnit = new List<LCDDO>();
                if (justSpecies[k, 0] != null && justSpecies[k,1] != "01")
                {
                    //  find all species in lcd list
                    currentUnit = lList.FindAll(
                        delegate(LCDDO t)
                        {
                            return t.Species == justSpecies[k, 0] &&
                                t.PrimaryProduct != "01" &&
                                t.CutLeave == "C";
                        });
                }   //endif
                RegionalReports rr = new RegionalReports();
                RegionalReports rrt = new RegionalReports();
                if (currentUnit.Count != 0)
                {
                    foreach (LCDDO cu in currentUnit)
                    {  
                        //  What method is the stratum on this tree?
                        //  And what is the strata acres for plot based methods
                      List<StratumDO> sList = bslyr.getStratum();
                        int mthRow = sList.FindIndex(
                            delegate (StratumDO s)
                            {
                                return s.Code == cu.Stratum;
                            });
                        double strAcres = Utilities.ReturnCorrectAcres(cu.Stratum, bslyr,
                                    (long)sList[mthRow].Stratum_CN);
                        //  is species already in the list?
                        int kRow = listToOutput.FindIndex(
                            delegate(RegionalReports lto)
                            {
                                return lto.value1 == justSpecies[k, 0];
                            });
                        if (kRow >= 0)
                        {
                            listToOutput[kRow].value13 += cu.SumGBDFT * strAcres;
                            listToOutput[kRow].value14 += cu.SumGCUFT * strAcres;
                            listToOutput[kRow].value15 += cu.SumNBDFT * strAcres;
                            listToOutput[kRow].value16 += cu.SumNCUFT * strAcres;
                        }
                        else if (kRow < 0)
                        {
                            rr.value1 = justSpecies[k, 0];
                            rr.value13 += cu.SumGBDFT * strAcres;
                            rr.value14 += cu.SumGCUFT * strAcres;
                            rr.value15 += cu.SumNBDFT * strAcres;
                            rr.value16 += cu.SumNCUFT * strAcres;
                            listToOutput.Add(rr);
                        }   //  enduf
                        int nthRow = listTotalOutput.FindIndex(
                        delegate (RegionalReports lto)
                        {
                            return lto.value1 == cu.PrimaryProduct;
                        });
                        if (nthRow >= 0)
                        {
                            listTotalOutput[nthRow].value9 += cu.SumGBDFT * strAcres;
                            listTotalOutput[nthRow].value10 += cu.SumGCUFT * strAcres;
                            listTotalOutput[nthRow].value11 += cu.SumNBDFT * strAcres;
                            listTotalOutput[nthRow].value12 += cu.SumNCUFT * strAcres;
                            //  also add any secondaary volume
                            listTotalOutput[nthRow].value13 += cu.SumGBDFTtop * strAcres;
                            listTotalOutput[nthRow].value14 += cu.SumGCUFTtop * strAcres;
                            listTotalOutput[nthRow].value15 += cu.SumNBDFTtop * strAcres;
                            listTotalOutput[nthRow].value16 += cu.SumNCUFTtop * strAcres;
                        }
                        else if (nthRow < 0)
                        {
                            rrt.value1 = cu.PrimaryProduct;
                            rrt.value2 = "P";
                            rrt.value3 = cu.SecondaryProduct;
                            rrt.value4 = "S";
                            rrt.value9 = cu.SumGBDFT * strAcres;
                            rrt.value10 = cu.SumGCUFT * strAcres;
                            rrt.value11 = cu.SumNBDFT * strAcres;
                            rrt.value12 = cu.SumNCUFT * strAcres;
                            rrt.value13 = cu.SumGBDFTtop * strAcres;
                            rrt.value14 = cu.SumGCUFTtop * strAcres;
                            rrt.value15 = cu.SumNBDFTtop * strAcres;
                            rrt.value16 = cu.SumNCUFTtop * strAcres;
                            listTotalOutput.Add(rrt);
                        }   //  endif

                    }   //  end foreach loop
                }  //  endif no records
            }   //  end for k loop
            return;
        }   // end  AccuulateSubtotals
                                
        private void AccumulateTotalsAndPrint(StreamWriter strWriteOut, List<LCDDO> lcdList,
                                              List<TreeCalculatedValuesDO> tcvList)
        {
            listToOutput.Clear();
            //List<LCDDO> productValues = LCDmethods.GetCutGroupedBy("","",9,bslyr);

            foreach (LCDDO pv in lcdList)
            {
                //  Is the product in the list to output
                if (listToOutput.Count >= 0)
                {
                    int nthRow = listToOutput.FindIndex(
                        delegate (RegionalReports lto)
                        {
                           return lto.value1 == pv.PrimaryProduct;
                        });
                    if (nthRow >= 0)
                    {
                        listToOutput[nthRow].value7 += pv.SumGBDFT; ;
                        listToOutput[nthRow].value8 += pv.SumGCUFT;
                        listToOutput[nthRow].value9 += pv.SumNBDFT;
                        listToOutput[nthRow].value10 += pv.SumNCUFT;
                        //  also add any secondaary volume
                        listToOutput[nthRow].value11 += pv.SumGBDFTtop;
                        listToOutput[nthRow].value12 += pv.SumGCUFTtop;
                        listToOutput[nthRow].value13 += pv.SumNBDFTtop;
                        listToOutput[nthRow].value14 += pv.SumNCUFTtop;
                   }
                    else if (nthRow < 0)
                    {
                        RegionalReports rr = new RegionalReports();
                        rr.value1 = pv.PrimaryProduct;
                        rr.value2 = "P";
                        rr.value3 = pv.SecondaryProduct;
                        rr.value4 = "S";
                        rr.value7 = pv.SumGBDFT;
                        rr.value8 = pv.SumGCUFT;
                        rr.value9 = pv.SumNBDFT;
                        rr.value10 = pv.SumNCUFT;
                        rr.value11 = pv.SumGBDFTtop;
                        rr.value12 = pv.SumGCUFTtop;
                        rr.value13 = pv.SumNBDFTtop;
                        rr.value14 = pv.SumNCUFTtop;
                        listToOutput.Add(rr);
                    }   //  endif
                }   //  endif
           
            }       //  end foreaach
            return;
        }   // end Accumulate Totals

        private void WriteSectionOne(StreamWriter strWriteOut,ref int pagenumber,
            reportHeaders rh, regionalReportHeaders rRH)
        {
            //  R105
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], 
                            rh.reportTitles[2], rRH.R105sectionOne, 7, ref pagenumber, "");
            strWriteOut.WriteLine("_________________________________________________________________________________________________________");
            prtFields.Clear();
            foreach (RegionalReports lto in listToOutput)
            {
                prtFields.Add(lto.value1.PadRight(4, ' '));
                prtFields.Add(Utilities.FormatField(lto.value8, "{0,7:F0}").ToString().PadLeft(7, ' '));
                prtFields.Add(Utilities.FormatField(lto.value9, "{0,8:F0}").ToString().PadLeft(16, ' '));
                prtFields.Add(Utilities.FormatField(lto.value10, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value11, "{0,8:F0}").ToString().PadLeft(12, ' '));
                prtFields.Add(Utilities.FormatField(lto.value12, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value13, "{0,8:F0}").ToString().PadLeft(13, ' '));
                prtFields.Add(Utilities.FormatField(lto.value14, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value15, "{0,8:F0}").ToString().PadLeft(10, ' '));
                prtFields.Add(Utilities.FormatField(lto.value16, "{0,8:F0}").ToString().PadLeft(11, ' '));
                printOneRecord(fieldLengths, prtFields, strWriteOut);
                prtFields.Clear();
            }   //  end foreach loop
            strWriteOut.WriteLine("                  ______________________________________________________________________________________");

            return;
        }   //  end WriteSectionOne

        private void WriteSectionTwo(StreamWriter strWriteOut, ref int pagenumber, 
            reportHeaders rh, regionalReportHeaders rRH)
        {
            //  R105
            //  write section two headings only
            for (int j = 0; j < 5; j++)
            {
                strWriteOut.WriteLine(rRH.R105sectionTwo[j]);
                numOlines++;
            }   //  end for j loop
            strWriteOut.WriteLine("_________________________________________________________________________________________________________");
            prtFields.Clear();
            foreach (RegionalReports lto in listToOutput)
            {
                prtFields.Add("");
                prtFields.Add(lto.value1);
                prtFields.Add(Utilities.FormatField(lto.value9, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value10, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value11, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value12, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value13, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value14, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value15, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value16, "{0,8:F0}").ToString().PadLeft(11, ' '));
                printOneRecord(fieldLengths, prtFields, strWriteOut);
                prtFields.Clear();
            }   //  end foreach loop
            strWriteOut.WriteLine("                  ______________________________________________________________________________________");
            listToOutput.Clear();
            return;
        }   //  end WRite Section two

        private void WriteSectionThree(StreamWriter strWriteOut, ref int pagenumber, 
            reportHeaders rh, regionalReportHeaders rRH)
        {
            //  R105
            double calcValue = 0;
            double GBDFTtotal = 0;
            double GCUFTtotal = 0;
            double NBDFTtotal = 0;
            double NCUFTtotal = 0;
//            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1],
 //                           rh.reportTitles[2], rRH.R105sectionThree, 4, ref pagenumber, "");
            //  write section two headings only
            for (int j = 0; j < 5; j++)
            {
                strWriteOut.WriteLine(rRH.R105sectionThree[j]);
                numOlines++;
            }   //  end for j loop
            strWriteOut.WriteLine("_________________________________________________________________________________________________________");
            prtFields.Clear();
            foreach (RegionalReports lto in listTotalOutput)
            {
                fieldLengths = new[] { 5, 2, 8, 15, 11, 13, 11, 11, 10 };
                prtFields.Add("");
                //  Primary product
                prtFields.Add(lto.value1.PadRight(6, ' '));
                prtFields.Add(lto.value2.PadRight(3, ' '));
                //  ratio
                if(lto.value10 > 0)
                {
                    calcValue = lto.value9 / lto.value10;
                    prtFields.Add(Utilities.FormatField(calcValue,"{0,8:F4}").ToString());
                }
                else prtFields.Add("        ");
                if(lto.value12 > 0)
                {
                    calcValue = lto.value11 / lto.value12;
                    prtFields.Add(Utilities.FormatField(calcValue,"{0,8:F4}").ToString());
                }
                else prtFields.Add("        ");
                prtFields.Add(Utilities.FormatField(lto.value9, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value10, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value11, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value12, "{0,8:F0}").ToString().PadLeft(11, ' '));
                printOneRecord(fieldLengths, prtFields, strWriteOut);
                prtFields.Clear();
                GBDFTtotal += lto.value9;
                GCUFTtotal += lto.value10;
                NBDFTtotal += lto.value11;
                NCUFTtotal += lto.value12;

                //  Secondary product
                fieldLengths = new[] { 5, 6, 34, 13, 9, 11, 8};
                prtFields.Add(" ");
                prtFields.Add(lto.value3.PadRight(4,' '));
                prtFields.Add(lto.value4);
                prtFields.Add(Utilities.FormatField(lto.value13, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value14, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value15, "{0,8:F0}").ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.FormatField(lto.value16, "{0,8:F0}").ToString().PadLeft(11, ' '));
                printOneRecord(fieldLengths, prtFields, strWriteOut);
                prtFields.Clear(); 
                GBDFTtotal += lto.value13;
                GCUFTtotal += lto.value14;
                NBDFTtotal += lto.value15;
                NCUFTtotal += lto.value16;
            }   //  end foreach loop
            strWriteOut.WriteLine("                  ______________________________________________________________________________________");
            
            //  write overall totals
            //  ratio
            strWriteOut.Write("                   ");
            if(GCUFTtotal > 0)
            {
                calcValue = GBDFTtotal / GCUFTtotal;
                strWriteOut.Write(Utilities.FormatField(calcValue,"{0,8:F4}").ToString().PadRight(15,' '));
            }
            else strWriteOut.Write("        ");
            if(NCUFTtotal > 0)
            {
                calcValue = NBDFTtotal / NCUFTtotal;
                strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F4}").ToString().PadRight(14, ' '));
            }
            else strWriteOut.Write("        ");
            strWriteOut.Write(Utilities.FormatField(GBDFTtotal, "{0,8:F0}").ToString().PadRight(13, ' '));
            strWriteOut.Write(Utilities.FormatField(GCUFTtotal, "{0,8:F0}").ToString().PadRight(11, ' '));
            strWriteOut.Write(Utilities.FormatField(NBDFTtotal, "{0,8:F0}").ToString().PadRight(11, ' '));
            strWriteOut.WriteLine(Utilities.FormatField(NCUFTtotal, "{0,8:F0}").ToString().PadRight(11, ' '));
            

            return;
        }   //  end Write Section Three


        private void outputCutLeaveSubtotal(string currCL, StreamWriter strWriteOut, ref int pageNum, 
                                                reportHeaders rh)
        {
            //  R104
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                rRH.R104columns, 10, ref pageNum, "");
            strWriteOut.WriteLine("            _________________________________________________________________________________________________________________________");
            strWriteOut.Write("  SUBTOTAL                                               ");
            strWriteOut.Write(currCL);
            //  average DBH
            if (firstSubtotal[0].Value11 > 0)
                calcValue = firstSubtotal[0].Value10 / firstSubtotal[0].Value11;
            else calcValue = 0.0;
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F1}").ToString().PadLeft(11, ' '));
            //  basal area
            if (firstSubtotal[0].Value7 > 0)
                calcValue = (firstSubtotal[0].Value8 * firstSubtotal[0].Value9) / firstSubtotal[0].Value7;
            else calcValue = 0.0;
            strWriteOut.WriteLine(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(10, ' '));
            return;
        }   //  end outputCutLeaveSubtotal


        private void outputUnitOrStrata(StreamWriter strWriteOut, ref int pageNum, reportHeaders rh, 
                                        string currCode, int whichSubtotal, List<ReportSubtotal> subtotalList)
        {
            //  R104
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                rRH.R104columns, 10, ref pageNum, "");
            strWriteOut.WriteLine("            _________________________________________________________________________________________________________________________");

            switch (whichSubtotal)
            {
                case 1:         //  unit subtotal
                    strWriteOut.Write("   UNIT    ");
                    strWriteOut.Write(currCode.PadLeft(3, ' '));
                    strWriteOut.Write("  SUBTOTAL                                 ");
                    break;
                case 2:         //  strata subtotal
                    strWriteOut.Write("   STRATA  ");
                    strWriteOut.Write(currCode.PadLeft(3, ' '));
                    strWriteOut.Write("  SUBTOTAL                                 ");
                    strWriteOut.Write(subtotalList[0].Value1);
                    break;
            }   //  end switch
            foreach (ReportSubtotal sl in subtotalList)
            {
                //  output average DBH
                if (whichSubtotal == 1)
                {
                    if (sl.Value11 > 0)
                        calcValue = sl.Value10 / sl.Value11;
                    else calcValue = 0.0;
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F1}").ToString().PadLeft(12, ' '));
                    //  basal area
                    if(sl.Value7 > 0)
                        calcValue = (sl.Value8 * sl.Value9) / sl.Value7;
                    else calcValue = 0.0;
                    strWriteOut.WriteLine(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(10, ' '));
                    strWriteOut.WriteLine("");
                }
                else if (whichSubtotal == 2)
                {
                    if(sl.Value1 == "C")
                        strWriteOut.Write("            ");
                    else if (sl.Value1 == "L")
                    {
                        strWriteOut.Write("                                                         ");
                        strWriteOut.Write(sl.Value1);
                        strWriteOut.Write("            ");
                    }   //  endif
                    //  basal area
                    if (sl.Value7 > 0)
                        calcValue = sl.Value9 / sl.Value7;
                    else calcValue = 0.0;
                    strWriteOut.WriteLine(Utilities.FormatField(calcValue,"{0,5:F1}").ToString().PadLeft(9,' '));
                }   //  endif whichSubtotal
            }   //  end foreach loop
            strWriteOut.WriteLine(reportConstants.longLine);
            return;
        }   //  end outputUnitOrStrata


            //  writes subtotal line for any subtotal in R101
        private string[] createCompleteHeader()
        {
            string[] finnishHeader = new string[7];
            finnishHeader = rRH.R102R103columns;
            switch (currentReport)
            {
                case "R102":
                    finnishHeader[5] = finnishHeader[5].Replace("XX", "BF");
                    finnishHeader[6] = finnishHeader[6].Replace("XXX", "MBF");
                    finnishHeader[6] = finnishHeader[6].Replace("XX", "BF");
                    break;
                case "R103":
                    finnishHeader[5] = finnishHeader[5].Replace("XX", "CF");
                    finnishHeader[6] = finnishHeader[6].Replace("XXX", "CCF");
                    finnishHeader[6] = finnishHeader[6].Replace("XX", "CF");
                    break;
            }   //  end switch on current report
            return finnishHeader;
        }   //  end createCompleteHeader


    }
}
