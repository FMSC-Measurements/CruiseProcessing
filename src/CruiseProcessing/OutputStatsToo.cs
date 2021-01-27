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
    class OutputStatsToo : CreateTextFile
    {
        #region
        public string currentReport;
        private int[] fieldLengths;
        private ArrayList prtFields = new ArrayList();
        private string[] completeHeader = new string[7];
        private List<POPDO> popList = new List<POPDO>();
        private List<LCDDO> lcdList = new List<LCDDO>();
        private int[] pagesToPrint = new int[3];
        private List<ReportSubtotal> aggProduct = new List<ReportSubtotal>();
        private List<ReportSubtotal> aggUOM = new List<ReportSubtotal>();
        private List<ReportSubtotal> aggStrata = new List<ReportSubtotal>();
        private List<ReportSubtotal> totalProduct = new List<ReportSubtotal>();
        private List<ReportSubtotal> totalUOM = new List<ReportSubtotal>();
        private List<ReportSubtotal> totalStrata = new List<ReportSubtotal>();
        private List<StatSums> groupSums = new List<StatSums>();
        #endregion

        public void OutputStatReports(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  ST3 (DS1) and ST4 (DS2)
            string currentTitle = fillReportTitle(currentReport);
            popList = bslyr.getPOP();
            lcdList = bslyr.getLCD();

            //  check for no data
            List<POPDO> popCutOnly = POPmethods.GetCutTrees(popList);
            List<LCDDO> lcdCutOnly = LCDmethods.GetCutOrLeave(lcdList, "C", "", "", "");
            if (popCutOnly.Count == 0 && lcdCutOnly.Count == 0 && currentReport == "ST3")
            {
                noDataForReport(strWriteOut, currentReport, " >> No data available to generate this report.");
                return;
            }
            else if(lcdCutOnly.Sum(l => l.SumValue) == 0 && currentReport == "ST4")
            {
                noDataForReport(strWriteOut, currentReport, " >> No data available to generate this report.");
                return;
            }   //  endif no data

            //  need report header and initialize pages to print
            for (int j = 0; j < 3; j++)
                pagesToPrint[j] = 0;
            switch (currentReport)
            {
                case "ST3":
                    if (lcdList.Sum(l => l.SumGBDFT) > 0 || lcdList.Sum(l => l.SumGCUFT) > 0 || lcdList.Sum(l => l.SumCords) > 0)
                        pagesToPrint[0] = 1;
                    if (lcdList.Sum(l => l.SumGBDFTtop) > 0 || lcdList.Sum(l => l.SumGCUFTtop) > 0 || 
                        lcdList.Sum(l => l.SumCordsTop) > 0)
                        pagesToPrint[1] = 1;
                    if (lcdList.Sum(l => l.SumBDFTrecv) > 0 || lcdList.Sum(l => l.SumCUFTrecv) > 0 || 
                        lcdList.Sum(l => l.SumCordsRecv) > 0)
                        pagesToPrint[2] = 1;
                    fieldLengths = new int[] { 1, 5, 4, 4, 13, 11, 15, 19, 12, 10, 14, 10 };
                    break;
                case "ST4":
                    if (popList.Sum(p => p.Stg1ValueXPP) > 0)
                        pagesToPrint[0] = 1;
                    if (popList.Sum(p => p.Stg1ValueXSP) > 0)
                        pagesToPrint[1] = 1;
                    if (popList.Sum(p => p.Stg1ValueXRP) > 0)
                        pagesToPrint[2] = 1;
                    fieldLengths = new int[] { 33, 5, 4, 4, 13, 12, 16, 12 };
                    break;
            }   //  end switch on report
            rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");

            //  process by product pages
            if (pagesToPrint[0] == 1)
            {
                //  pull strata, product and UOM groups from LCD
                List<LCDDO> justGroups = bslyr.GetLCDgroup(fileName,"Stratum,PrimaryProduct,UOM");
                // primary product pages
                if (currentReport == "ST3")
                    finishColumnHeaders(rh.ST3columns, "PRIMARY PRODUCT");
                else if (currentReport == "ST4")
                    finishColumnHeaders(rh.ST4columns, "");
                numOlines = 0;
                aggProduct.Clear();
                aggUOM.Clear();
                aggStrata.Clear();
                groupSums.Clear();
                ProcessData(strWriteOut, rh, ref pageNumb, justGroups, "PP");
                //  output subtotals here
                switch (currentReport)
                {
                    case "ST3":
                        OutputSubtotal(strWriteOut, rh, ref pageNumb, 1, aggProduct, "PRIMARY PRODUCT VOLUME");
                        OutputSubtotal(strWriteOut, rh, ref pageNumb, 2, aggUOM, "PRIMARY PRODUCT VOLUME");
                        OutputSubtotal(strWriteOut, rh, ref pageNumb, 3, aggStrata, "PRIMARY PRODUCT VOLUME");
                        break;
                    case "ST4":
                        OutputSubtotal(strWriteOut, rh, ref pageNumb, 1, aggProduct, "PRIMARY PRODUCT $ VALUE");
                        OutputSubtotal(strWriteOut, rh, ref pageNumb, 2, aggUOM, "PRIMARY PRODUCT $ VALUE");
                        OutputSubtotal(strWriteOut, rh, ref pageNumb, 3, aggStrata, "PRIMARY PRODUCT $ VALUE");
                        break;
                }   //  end switch on current report
                // output footer statement
                strWriteOut.WriteLine("");
                strWriteOut.WriteLine(rh.STfooters[2]);
                numOlines++;
            }   //  endif
            if (pagesToPrint[1] == 1)
            {
                //  pull strata, product and UOM groups from LCD
                List<LCDDO> justGroups = bslyr.GetLCDgroup(fileName, "Stratum,SecondaryProduct,UOM");
                //  reset field lengths for body of report
                if(currentReport == "ST3")
                {
                    fieldLengths = new int[] { 1, 5, 4, 4, 13, 11, 15, 19, 12, 10, 14, 10 };
                    finishColumnHeaders(rh.ST3columns, "SECONDARY PRODUCT");
                }
                else if (currentReport == "ST4")
                {
                    fieldLengths = new int[] { 33, 5, 4, 4, 13, 12, 16, 12 };
                    finishColumnHeaders(rh.ST4columns, "");
                }   //  endif on report

                // secondary product pages
                numOlines = 0;
                aggProduct.Clear();
                aggUOM.Clear();
                aggStrata.Clear();
                ProcessData(strWriteOut, rh, ref pageNumb, justGroups, "SP");
                //  output subtotals here
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 1, aggProduct, "SECONDARY PRODUCT VOLUME");
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 2, aggUOM, "SECONDARY PRODUCT VOLUME");
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 3, aggStrata, "SECONDARY PRODUCT VOLUME");
                // output footer statement
                strWriteOut.WriteLine("");
                strWriteOut.WriteLine(rh.STfooters[2]);
                numOlines++;
            }   //  endif
            if (pagesToPrint[2] == 1)
            {
                //  pull strata, product and UOM groups from LCD
                List<LCDDO> justGroups = bslyr.GetLCDgroup(fileName, "Stratum,SecondaryProduct,UOM");
                //  reset field lengths for body of report
                if (currentReport == "ST3")
                {
                    fieldLengths = new int[] { 1, 5, 4, 4, 13, 11, 15, 19, 12, 10, 14, 10 };
                    finishColumnHeaders(rh.ST3columns, "RECOVERED PRODUCT");
                }
                else if (currentReport == "ST4")
                {
                    fieldLengths = new int[] { 33, 5, 4, 4, 13, 12, 16, 12 };
                    finishColumnHeaders(rh.ST4columns, "");
                }   //  endif on report

                //  recovered product pages
                numOlines = 0;
                aggProduct.Clear();
                aggUOM.Clear();
                aggStrata.Clear();
                ProcessData(strWriteOut, rh, ref pageNumb, justGroups, "RP");
                //  output subtotals here
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 1, aggProduct, "RECOVERED PRODUCT VOLUME");
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 2, aggUOM, "RECOVERED PRODUCT VOLUME");
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 3, aggStrata, "RECOVERED PRODUCT VOLUME");
                // output footer statement
                strWriteOut.WriteLine("");
                strWriteOut.WriteLine(rh.STfooters[2]);
                numOlines++;
            }   //  endif

            //  output totals
            numOlines = 0;
            if (currentReport == "ST3")
            {
                finishColumnHeaders(rh.ST3columns, "******* TOTAL");
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 1, totalProduct, "TOTAL VOLUME");
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 2, totalUOM, "TOTAL VOLUME");
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 3, totalStrata, "TOTAL VOLUME"); 
            }
            else if (currentReport == "ST4")
            {
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 1, totalProduct, "TOTAL $ VALUE");
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 2, totalUOM, "TOTAL $ VALUE");
                OutputSubtotal(strWriteOut, rh, ref pageNumb, 3, totalStrata, "TOTAL $ VALUE");
            }   //  endif on report

            strWriteOut.WriteLine("");
            strWriteOut.WriteLine(rh.STfooters[2]);

            return;
        }   //  end OutputStatReports


        private void ProcessData(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, List<LCDDO> justGroups, 
                                        string prodType)
        {
            double strataAcres = 0.0;
            string currMeth;
            List<StratumDO> sList = bslyr.getStratum();
            //  process by groups
            foreach (LCDDO js in justGroups)
            {
                //  find acres and methods
                currMeth = Utilities.MethodLookup(js.Stratum, bslyr);
                strataAcres = Utilities.ReturnCorrectAcres(js.Stratum, bslyr, 
                                StratumMethods.GetStratumCN(js.Stratum, sList));
                //  Pull current group from LCD list
                List<LCDDO> currentGroup = new List<LCDDO>();
                if (prodType == "PP")
                    currentGroup = LCDmethods.GetCutOrLeave(lcdList, "C", js.Stratum, js.PrimaryProduct, 1);
                else if (prodType == "SP" || prodType == "RP")
                    currentGroup = LCDmethods.GetCutOrLeave(lcdList, "C", js.Stratum, js.SecondaryProduct, 2);

                //  Sum volumes and stat sums as needed
                SumGroups(strataAcres, currentGroup, prodType, js.UOM, currMeth);

                //  Calculate combined error before printing groups
                DetermineCombinedError(currMeth);

                //  Write groups
                switch (currentReport)
                {
                    case "ST3":
                        switch (prodType)
                        {
                            case "PP":
                                WriteCurrentGroup(strWriteOut, rh, ref pageNumb, "PRIMARY PRODUCT VOLUME");
                                break;
                            case "SP":
                                WriteCurrentGroup(strWriteOut, rh, ref pageNumb, "SECONDARY PRODUCT VOLUME");
                                break;
                            case "RP":
                                WriteCurrentGroup(strWriteOut, rh, ref pageNumb, "RECOVERED PRODUCT VOLUME");
                                break;
                        }   //  end switch on product
                        break;
                    case "ST4":
                        switch (prodType)
                        {
                            case "PP":
                                WriteCurrentGroup("PRIMARY PRODUCT $ VALUE", rh, strWriteOut, ref pageNumb);
                                break;
                            case "SP":
                                WriteCurrentGroup("SECONDARY PRODUCT $ VALUE", rh, strWriteOut, ref pageNumb);
                                break;
                            case "RP":
                                WriteCurrentGroup("RECOVERED PRODUCT $ VALUE", rh, strWriteOut, ref pageNumb);
                                break;
                        }   //  end switch on product
                        break;
                }   //  end switch on report
                groupSums.Clear();

            }   //  end foreach loop
            return;
        }   //  end ProcessData


        private void SumGroups(double STacres, List<LCDDO> currGrp, string prodType, string volType, string currMeth)
        {
            int nthRow = 0;
            foreach (LCDDO cg in currGrp)
            {
                if (groupSums.Count == 0)
                {
                    StatSums ss = new StatSums();
                    //  Add group
                    ss.ST = cg.Stratum;
                    ss.PP = cg.PrimaryProduct;
                    ss.SP = cg.SecondaryProduct;
                    ss.UOM = cg.UOM;
                    ss.SG = cg.SampleGroup;
                    groupSums.Add(ss);
                }
                else
                {
                    nthRow = groupSums.FindIndex(
                        delegate(StatSums s)
                        {
                            return s.ST == cg.Stratum && s.PP == cg.PrimaryProduct && s.SP == cg.SecondaryProduct &&
                                        s.UOM == cg.UOM && s.SG == cg.SampleGroup;
                        });
                    if (nthRow < 0)
                    {
                        //  Add group
                        StatSums ss = new StatSums();
                        ss.ST = cg.Stratum;
                        ss.PP = cg.PrimaryProduct;
                        ss.SP = cg.SecondaryProduct;
                        ss.UOM = cg.UOM;
                        ss.SG = cg.SampleGroup;
                        groupSums.Add(ss);
                        nthRow = groupSums.Count - 1;
                    }   //  endif
                }   //  endif 
                if (currentReport == "ST3")
                {
                    //  Sum up volumes
                    AddUpVolumes(prodType, volType, cg, STacres, nthRow);
                }
                else if (currentReport == "ST4")
                {
                    //  Sum up dollar value
                    AddUpValue(prodType, cg, STacres, nthRow);
                }   //  endif on report
            }   //  end foreach loop on lcd groups
            //  now find all POP records for the groups in groupSums
            foreach(StatSums gs in groupSums)
            {          
                List<POPDO> justPOP = POPmethods.GetStratumData(popList, gs.ST, gs.PP, gs.UOM, gs.SP, gs.SG);

                //  need samples
                switch (currMeth)
                {
                    case "STR":
                        gs.ST1Samps = justPOP.Sum(p => p.StageOneSamples);
                        gs.ST2Samps = justPOP.Sum(p => p.StageTwoSamples);
                        gs.STRtrees = justPOP.Sum(p => p.TalliedTrees);
                        break;
                    default:
                        gs.ST1Samps = justPOP.Sum(p => p.StageOneSamples);
                        gs.ST2Samps = justPOP.Sum(p => p.StageTwoSamples);
                        break;
                }   //  end switch on method

                //  now add up X and X sqrd
                AddUpXandXsqrd(justPOP, prodType, gs);
            }   //  end foreach loop
            return;
        }   //  end SumGroups


        private void AddUpVolumes(string prodType, string volType, LCDDO currGrp, double STacres, int currRow)
        {
            switch (prodType)
            {
                case "PP":
                    switch (volType)
                    {
                        case "01":
                            groupSums[currRow].GrossVol += currGrp.SumGBDFT * STacres;
                            groupSums[currRow].NetVol += currGrp.SumNBDFT * STacres;
                            break;
                        case "03":
                        case "05":
                            groupSums[currRow].GrossVol += currGrp.SumGCUFT * STacres;
                            groupSums[currRow].NetVol += currGrp.SumNCUFT * STacres;
                            break;
                        case "02":
                            groupSums[currRow].GrossVol += currGrp.SumCords * STacres;
                            groupSums[currRow].NetVol = 0.0;
                            break;
                        case "04":
                            groupSums[currRow].GrossVol += currGrp.SumExpanFactor * STacres;
                            groupSums[currRow].NetVol += currGrp.SumExpanFactor * STacres;
                            break;
                    }   //  end switch on volume type
                    break;
                case "SP":
                    switch (volType)
                    {
                        case "01":
                            groupSums[currRow].GrossVol += currGrp.SumGBDFTtop * STacres;
                            groupSums[currRow].NetVol += currGrp.SumNBDFTtop * STacres;
                            break;
                        case "03":
                        case "05":
                            groupSums[currRow].GrossVol += currGrp.SumGCUFTtop * STacres;
                            groupSums[currRow].NetVol += currGrp.SumNCUFTtop * STacres;
                            break;
                        case "02":
                            groupSums[currRow].GrossVol += currGrp.SumCordsTop * STacres;
                            groupSums[currRow].NetVol += 0.0;
                            break;
                    }   //  end switch on volume type
                    break;
                case "RP":
                    switch (volType)
                    {
                        case "01":
                            groupSums[currRow].GrossVol += 0.0;
                            groupSums[currRow].NetVol += currGrp.SumBDFTrecv * STacres;
                            break;
                        case "03":
                            groupSums[currRow].GrossVol += 0.0;
                            groupSums[currRow].NetVol += currGrp.SumCUFTrecv * STacres;
                            break;
                        case "02":
                            groupSums[currRow].GrossVol += 0.0;
                            groupSums[currRow].NetVol += currGrp.SumCordsRecv * STacres;
                            break;
                    }   //  end switch on volume type
                    break;
            }   //  end switch
            return;
        }   //  end AddUpVolumes


        private void AddUpValue(string prodType, LCDDO currGrp, double STacres, int currRow)
        {
            switch (prodType)
            {
                case "PP":
                    groupSums[currRow].GrossVol += currGrp.SumValue * STacres;
                    break;
                case "SP":
                    groupSums[currRow].GrossVol += currGrp.SumTopValue * STacres;
                    break;
                case "RP":
                    groupSums[currRow].GrossVol += currGrp.SumValueRecv * STacres;
                    break;
            }   //  end switch on product
            return;
        }   //  end AddUpValue


        private void AddUpXandXsqrd(List<POPDO> justPOP, string prodType, StatSums currGS)
        {
                switch (prodType)
                {
                    case "PP":
                        if (currentReport == "ST3")
                        {
                            currGS.ST1Xgross = justPOP.Sum(p => p.Stg1GrossXPP);
                            currGS.ST1Xnet = justPOP.Sum(p => p.Stg1NetXPP);
                            currGS.ST1X2gross = justPOP.Sum(p => p.Stg1GrossXsqrdPP);
                            currGS.ST1X2net = justPOP.Sum(p => p.Stg1NetXsqrdPP);
                            currGS.ST2Xgross = justPOP.Sum(p => p.Stg2GrossXPP);
                            currGS.ST2Xnet = justPOP.Sum(p => p.Stg2NetXPP);
                            currGS.ST2X2gross = justPOP.Sum(p => p.Stg2GrossXsqrdPP);
                            currGS.ST2X2net = justPOP.Sum(p => p.Stg2NetXsqrdPP);
                        }
                        else if (currentReport == "ST4")
                        {
                            currGS.ST1Xgross = justPOP.Sum(p => p.Stg1ValueXPP);
                            currGS.ST1Xnet = 0.0;
                            currGS.ST1X2gross = justPOP.Sum(p => p.Stg1ValueXsqrdPP);
                            currGS.ST1X2net = 0.0;
                        } //  endif on report
                        break;
                    case "SP":
                        if (currentReport == "ST3")
                        {
                            currGS.ST1Xgross = justPOP.Sum(p => p.Stg1GrossXSP);
                            currGS.ST1Xnet = justPOP.Sum(p => p.Stg1NetXSP);
                            currGS.ST1X2gross = justPOP.Sum(p => p.Stg1GrossXsqrdSP);
                            currGS.ST1X2net = justPOP.Sum(p => p.Stg1NetXsqrdSP);
                            currGS.ST2Xgross = justPOP.Sum(p => p.Stg2GrossXSP);
                            currGS.ST2Xnet = justPOP.Sum(p => p.Stg2NetXSP);
                            currGS.ST2X2gross = justPOP.Sum(p => p.Stg2GrossXsqrdSP);
                            currGS.ST2X2net = justPOP.Sum(p => p.Stg2NetXsqrdSP);
                        }
                        else if (currentReport == "ST4")
                        {
                            currGS.ST1Xgross = justPOP.Sum(p => p.Stg1ValueXSP);
                            currGS.ST1Xnet = 0.0;
                            currGS.ST1X2gross = justPOP.Sum(p => p.Stg1ValueXsqrdSP);
                            currGS.ST1X2net = 0.0;
                        } //  endif on report
                        break;
                    case "RP":
                        if (currentReport == "ST3")
                        {
                            currGS.ST1Xgross = justPOP.Sum(p => p.Stg1GrossXRP);
                            currGS.ST1Xnet = justPOP.Sum(p => p.Stg1NetXRP);
                            currGS.ST1X2gross = justPOP.Sum(p => p.Stg1GrossXsqrdRP);
                            currGS.ST1X2net = justPOP.Sum(p => p.Stg1NetXRsqrdRP);
                            currGS.ST2Xgross = justPOP.Sum(p => p.Stg2GrossXRP);
                            currGS.ST2Xnet = justPOP.Sum(p => p.Stg2NetXRP);
                            currGS.ST2X2gross = justPOP.Sum(p => p.Stg2GrossXsqrdRP);
                            currGS.ST2X2net = justPOP.Sum(p => p.Stg2NetXsqrdRP);
                        }
                        else if (currentReport == "ST4")
                        {
                            currGS.ST1Xgross = justPOP.Sum(p => p.Stg1ValueXRP);
                            currGS.ST1Xnet = 0.0;
                            currGS.ST1X2gross = justPOP.Sum(p => p.Stg1ValueXsqrdRP);
                            currGS.ST1X2net = 0.0;
                        } //  endif on report
                        break;
                }   //  end switch on product type
            return;
        }   //  end AddUpXandXsqrd


        private void DetermineCombinedError(string currMeth)
        {
            double CombinedGross = 0.0;
            double CombinedNet = 0.0;
            double theTvalue;
            double SampleErr1gross = 0.0;
            double SampleErr1net = 0.0;
            double SampleErr2gross = 0.0;
            double SampleErr2net = 0.0;
            double theMean = 0.0;
            double theSE = 0.0;

            foreach (StatSums gs in groupSums)
            {
                //  Determine combined error for each group
                if (currMeth == "100")
                {
                    CombinedGross = 0.0;
                    CombinedNet = 0.0;
                }
                else
                {
                    if (gs.ST1Samps > 1)
                    {
                        theTvalue = CommonStatistics.LookUpT((int)gs.ST1Samps - 1);
                        //  STAGE 1
                        //  Calculate gross
                        //  mean, standard error and sample error 1
                        theMean = gs.ST1Xgross / gs.ST1Samps;
                        theSE = (gs.ST1X2gross - (Math.Pow(gs.ST1Xgross, 2) / gs.ST1Samps)) / ((gs.ST1Samps - 1) * gs.ST1Samps);
                        if (theSE <= 0)
                            theSE = 0.0;
                        else
                        {
                            if (currMeth == "STR" && gs.STRtrees > 0)
                            {
                                theSE = theSE * (1 - (gs.ST1Samps / gs.STRtrees));
                                if (theSE < 0) theSE = 0.0;
                            }   //  endif on current method
                            theSE = Math.Sqrt(theSE);
                        }   //  endif on standard error
                        if (theMean > 0)
                            SampleErr1gross = (theSE / theMean) * 100 * theTvalue;
                        else SampleErr1gross = 0.0;

                        //  Calculate net
                        theMean = gs.ST1Xnet / gs.ST1Samps;
                        theSE = (gs.ST1X2net - (Math.Pow(gs.ST1Xnet, 2) / gs.ST1Samps)) / ((gs.ST1Samps - 1) * gs.ST1Samps);
                        if (theSE <= 0)
                            theSE = 0.0;
                        else
                        {
                            if (currMeth == "STR" && gs.STRtrees > 0)
                            {
                                theSE = theSE * (1 - (gs.ST1Samps / gs.STRtrees));
                                if (theSE < 0) theSE = 0.0;
                            }
                            theSE = Math.Sqrt(theSE);
                        }   //  endif on standard error
                        if (theMean > 0)
                            SampleErr1net = (theSE / theMean) * 100 * theTvalue;
                        else SampleErr1net = 0.0;
                    }   //  endif stage 1 samples greater than 1

                    //  STAGE 2
                    if (gs.ST2Samps > 1)
                    {
                        theTvalue = CommonStatistics.LookUpT((int)gs.ST2Samps - 1);
                        //  Calculate gross
                        //  mean, standard error and sample error 2
                        theMean = gs.ST2Xgross / gs.ST2Samps;
                        theSE = (gs.ST2X2gross - (Math.Pow(gs.ST2Xgross, 2) / gs.ST2Samps)) / ((gs.ST2Samps - 1) * gs.ST2Samps);
                        if (theSE <= 0)
                            theSE = 0.0;
                        else
                        {
                            if (currMeth == "STR" && gs.STRtrees > 0)
                            {
                                theSE = theSE * (1 - (gs.ST2Samps / gs.STRtrees));
                                if (theSE < 0) theSE = 0.0;
                            }   //  endif on current method
                            theSE = Math.Sqrt(theSE);
                        }   //  endif on standard error
                        if (theMean > 0)
                            SampleErr2gross = (theSE / theMean) * 100 * theTvalue;
                        else SampleErr2gross = 0.0;

                        //  Calculate net
                        theMean = gs.ST2Xnet / gs.ST2Samps;
                        theSE = (gs.ST2X2net - (Math.Pow(gs.ST2Xnet, 2) / gs.ST2Samps)) / ((gs.ST2Samps - 1) * gs.ST2Samps);
                        if (theSE <= 0)
                            theSE = 0.0;
                        else
                        {
                            if (currMeth == "STR" && gs.STRtrees > 0)
                            {
                                theSE = theSE * (1 - (gs.ST2Samps / gs.STRtrees));
                                if (theSE < 0) theSE = 0.0;
                            }
                            theSE = Math.Sqrt(theSE);
                        }   //  endif on standard error
                        if (theMean > 0)
                            SampleErr2net = (theSE / theMean) * 100 * theTvalue;
                        else SampleErr2net = 0.0;

                    }   //  endif stage 2
                }   //  endif currMeth

                //  Calculate combined error
                switch (currMeth)
                {
                    case "STR":
                    case "3P":
                    case "FIX":
                    case "PNT":
                        CombinedGross = SampleErr1gross;
                        CombinedNet = SampleErr1net;
                        break;
                    default:
                        CombinedGross = Math.Sqrt(Math.Pow(SampleErr1gross, 2) + Math.Pow(SampleErr2gross, 2));
                        CombinedNet = Math.Sqrt(Math.Pow(SampleErr1net, 2) + Math.Pow(SampleErr2net, 2));
                        break;
                }   //  end switch on method

                gs.GrossErrSq = Math.Pow((gs.GrossVol * CombinedGross), 2);
                gs.NetErrSq = Math.Pow((gs.NetVol * CombinedNet), 2);
            }   //  end foreach loop
            return;
        }   //  end DetermineCombinedError


        private void WriteCurrentGroup(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, string prodType)
        {
            double FinalGrossErr = 0;
            double FinalNetErr = 0;
            double FinalGrossErr2 = 0;
            double FinalNetErr2 = 0;
            double FinalGrossVol = 0;
            double FinalNetVol = 0;
            string fieldFormat1 = "{0,10:F1}";
            string fieldFormat2 = "{0,7:F2}";
            string fieldFormat3 = "{0,10:F0}";

            FinalGrossVol = groupSums.Sum(gs => gs.GrossVol);
            FinalNetVol = groupSums.Sum(gs => gs.NetVol);
            FinalGrossErr2 = groupSums.Sum(gs => gs.GrossErrSq);
            FinalNetErr2 = groupSums.Sum(gs => gs.NetErrSq);

            WriteReportHeading(strWriteOut, rh.reportTitles[0], prodType, rh.reportTitles[1], 
                                    completeHeader, 7, ref pageNumb, "");
            prtFields.Add("");
            prtFields.Add(groupSums[0].ST.PadLeft(2, ' '));
            switch (prodType)
            {
                case "PRIMARY PRODUCT VOLUME":
                    prtFields.Add(groupSums[0].PP.PadLeft(2, '0'));
                    break;
                case "SECONDARY PRODUCT VOLUME":
                case "RECOVERED PRODUCT VOLUME":
                    prtFields.Add(groupSums[0].SP.PadLeft(2, '0'));
                    break;
            }   //  end switch on product
            prtFields.Add(groupSums[0].UOM.PadLeft(2, ' '));

            //  Gross numbers
            prtFields.Add(Utilities.Format(fieldFormat1, FinalGrossVol));
            if(FinalGrossVol > 0) FinalGrossErr = Math.Sqrt(FinalGrossErr2) / FinalGrossVol;
            prtFields.Add(Utilities.Format(fieldFormat2, FinalGrossErr));
            //  Calculate confidence intervals
            prtFields.Add(Utilities.Format(fieldFormat3, CommonStatistics.CalculateConfidence(FinalGrossVol, FinalGrossErr, "-")));
            prtFields.Add(Utilities.Format(fieldFormat3, CommonStatistics.CalculateConfidence(FinalGrossVol, FinalGrossErr, "+")));

            //  Net numbers
            prtFields.Add(Utilities.Format(fieldFormat1, FinalNetVol));
            if(FinalNetVol > 0) FinalNetErr = Math.Sqrt(FinalNetErr2) / FinalNetVol;
            prtFields.Add(Utilities.Format(fieldFormat2, FinalNetErr));
            //  Calculate confidence intervals
            prtFields.Add(Utilities.Format(fieldFormat3, CommonStatistics.CalculateConfidence(FinalNetVol, FinalNetErr, "-")));
            prtFields.Add(Utilities.Format(fieldFormat3, CommonStatistics.CalculateConfidence(FinalNetVol, FinalNetErr, "+")));

            switch (prodType)
            {
                case "PRIMARY PRODUCT VOLUME":
                case "SECONDARY PRODUCT VOLUME":
                    if(FinalGrossVol > 0 && FinalNetVol > 0)
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                    break;
                case "RECOVERED PRODUCT VOLUME":
                    if(FinalNetVol > 0)
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                    break;
            }   //  end switch

            prtFields.Clear();
            //  Update subtotals
            UpdateSubtotals(FinalGrossVol, FinalNetVol, FinalGrossErr2, FinalNetErr2, groupSums[0].ST, 
                            groupSums[0].PP, groupSums[0].SP, groupSums[0].UOM, prodType);
            UpdateTotals(FinalGrossVol, FinalNetVol, FinalGrossErr2, FinalNetErr2, groupSums[0].ST, groupSums[0].PP, 
                                groupSums[0].SP, groupSums[0].UOM, prodType);
            return;
        }   //  end WriteCurrentGroup


        private void WriteCurrentGroup(string prodType, reportHeaders rh, StreamWriter strWriteOut, ref int pageNumb)
        {
            //  overloaded for ST4 report
            double FinalGrossErr = 0.0;
            double FinalGrossErr2 = 0.0;
            double FinalGrossVol = 0.0;
            string fieldFormat1 = "{0,10:F2}";
            string fieldFormat2 = "{0,9:F2}";
            string fieldFormat3 = "{0,12:F2}";

            FinalGrossVol = groupSums.Sum(gs => gs.GrossVol);
            FinalGrossErr2 = groupSums.Sum(gs => gs.GrossErrSq);

            WriteReportHeading(strWriteOut, rh.reportTitles[0], prodType, rh.reportTitles[1],
                                    completeHeader, 7, ref pageNumb, "");
            prtFields.Add("");
            prtFields.Add(groupSums[0].ST.PadLeft(2, ' '));
            switch (prodType)
            {
                case "PRIMARY PRODUCT $ VALUE":
                    prtFields.Add(groupSums[0].PP.PadLeft(2, ' '));
                    break;
                case "SECONDARY PRODUCT $ VALUE":
                case "RECOVERED PRODUCT $ VALUE":
                    prtFields.Add(groupSums[0].SP.PadLeft(2, ' '));
                    break;
            }   //  end switch on product
            prtFields.Add(groupSums[0].UOM.PadLeft(2, ' '));

            //  Gross numbers (only one set of numbers for dollar value)
            prtFields.Add(Utilities.Format(fieldFormat1, FinalGrossVol));
            if (FinalGrossVol > 0) FinalGrossErr = Math.Sqrt(FinalGrossErr2) / FinalGrossVol;
            prtFields.Add(Utilities.Format(fieldFormat2, FinalGrossErr));
            //  Calculate confidence intervals
            prtFields.Add(Utilities.Format(fieldFormat3, CommonStatistics.CalculateConfidence(FinalGrossVol, FinalGrossErr, "-")));
            prtFields.Add(Utilities.Format(fieldFormat3, CommonStatistics.CalculateConfidence(FinalGrossVol, FinalGrossErr, "+")));

            if (FinalGrossVol > 0)
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            prtFields.Clear();

            //  Update subtotals and totals
            UpdateSubtotals(FinalGrossVol, 0.0, FinalGrossErr2, 0.0, groupSums[0].ST, groupSums[0].PP, groupSums[0].SP, 
                                groupSums[0].UOM, prodType);
            UpdateTotals(FinalGrossVol, 0.0, FinalGrossErr2, 0.0, groupSums[0].ST, groupSums[0].PP, groupSums[0].SP,
                                groupSums[0].UOM, prodType);
            return;
        }   //  end WriteCurrentGroup


        private void UpdateSubtotals(double finalGrossVol, double finalNetVol, double finalGrossErr2, double finalNetErr2, 
                                        string currST, string currPP, string currSP, string currUOM, string prodType)
        {
            //  Aggregate by product
            string prodCode = "";
            int nthRow = 0;
            switch (prodType)
            {
                case "PRIMARY PRODUCT VOLUME":
                case "PRIMARY PRODUCT $ VALUE":
                    prodCode = currPP;
                    break;
                case "SECONDARY PRODUCT VOLUME":
                case "RECOVERED PRODUCT VOLUME":
                case "SECONDARY PRODUCT $ VALUE":
                case "RECOVERED PRODUCT $ VALUE":
                    prodCode = currSP;
                    break;
            }   //  end switch on product

            nthRow = aggProduct.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == prodCode && rs.Value2 == currUOM;
                });
            if (nthRow < 0)
            {
                //  Add new group
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = prodCode;
                r.Value2 = currUOM;
                r.Value3 = finalGrossVol;
                r.Value4 = finalNetVol;
                r.Value5 = finalGrossErr2;
                r.Value6 = finalNetErr2;
                aggProduct.Add(r);
            }
            else
            {
                //  update
                aggProduct[nthRow].Value3 += finalGrossVol;
                aggProduct[nthRow].Value4 += finalNetVol;
                aggProduct[nthRow].Value5 += finalGrossErr2;
                aggProduct[nthRow].Value6 += finalNetErr2;
            }   //  endif

            //  Aggregate by unit of measure
            nthRow = aggUOM.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == currUOM;
                });
            if (nthRow < 0)
            {
                //  add new group
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = currUOM;
                r.Value3 = finalGrossVol;
                r.Value4 = finalNetVol;
                r.Value5 = finalGrossErr2;
                r.Value6 = finalNetErr2;
                aggUOM.Add(r);
            }
            else
            {
                //  update
                aggUOM[nthRow].Value3 += finalGrossVol;
                aggUOM[nthRow].Value4 += finalNetVol;
                aggUOM[nthRow].Value5 += finalGrossErr2;
                aggUOM[nthRow].Value6 += finalNetErr2;
            }   //  endif

            //  Aggregate by strata
            nthRow = aggStrata.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == currST && rs.Value2 == currUOM;
                });
            if (nthRow < 0)
            {
                //  Add new group
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = currST;
                r.Value2 = currUOM;
                r.Value3 = finalGrossVol;
                r.Value4 = finalNetVol;
                r.Value5 = finalGrossErr2;
                r.Value6 = finalNetErr2;
                aggStrata.Add(r);
            }
            else
            {
                //  update
                aggStrata[nthRow].Value3 += finalGrossVol;
                aggStrata[nthRow].Value4 += finalNetVol;
                aggStrata[nthRow].Value5 += finalGrossErr2;
                aggStrata[nthRow].Value6 += finalNetErr2;
            }   //  endif
            return;
        }   //  end UpdateSubtotals


        private void UpdateTotals(double finalGrossVol, double finalNetVol, double finalGrossErr2, double finalNetErr2, 
                                string currST, string currPP, string currSP, string currUOM, string prodType)
        {
            //  Aggregate by product
            string prodCode = "";
            int nthRow = 0;
            switch (prodType)
            {
                case "PRIMARY PRODUCT VOLUME":
                case "PRIMARU PRODUCT $ VALUE":
                    prodCode = currPP;
                    break;
                case "SECONDARY PRODUCT VOLUME":
                case "RECOVERED PRODUCT VOLUME":
                case "SECONDARY PRODUCT $ VALUE":
                case "RECOVERED PRODUCT $ VALUE":
                    prodCode = currSP;
                    break;
            }   //  end switch on product

            nthRow = totalProduct.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == prodCode && rs.Value2 == currUOM;
                });
            if (nthRow < 0)
            {
                //  Add new group
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = prodCode;
                r.Value2 = currUOM;
                r.Value3 = finalGrossVol;
                r.Value4 = finalNetVol;
                r.Value5 = finalGrossErr2;
                r.Value6 = finalNetErr2;
                totalProduct.Add(r);
            }
            else
            {
                //  update
                totalProduct[nthRow].Value3 += finalGrossVol;
                totalProduct[nthRow].Value4 += finalNetVol;
                totalProduct[nthRow].Value5 += finalGrossErr2;
                totalProduct[nthRow].Value6 += finalNetErr2;
            }   //  endif

            //  Aggregate by unit of measure
            nthRow = totalUOM.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == currUOM;
                });
            if (nthRow < 0)
            {
                //  add new group
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = currUOM;
                r.Value3 = finalGrossVol;
                r.Value4 = finalNetVol;
                r.Value5 = finalGrossErr2;
                r.Value6 = finalNetErr2;
                totalUOM.Add(r);
            }
            else
            {
                //  update
                totalUOM[nthRow].Value3 += finalGrossVol;
                totalUOM[nthRow].Value4 += finalNetVol;
                totalUOM[nthRow].Value5 += finalGrossErr2;
                totalUOM[nthRow].Value6 += finalNetErr2;
            }   //  endif

            //  Aggregate by strata
            nthRow = totalStrata.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == currST && rs.Value2 == currUOM;
                });
            if (nthRow < 0)
            {
                //  Add new group
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = currST;
                r.Value2 = currUOM;
                r.Value3 = finalGrossVol;
                r.Value4 = finalNetVol;
                r.Value5 = finalGrossErr2;
                r.Value6 = finalNetErr2;
                totalStrata.Add(r);
            }
            else
            {
                //  update
                totalStrata[nthRow].Value3 += finalGrossVol;
                totalStrata[nthRow].Value4 += finalNetVol;
                totalStrata[nthRow].Value5 += finalGrossErr2;
                totalStrata[nthRow].Value6 += finalNetErr2;
            }   //  endif
            return;
        }   //  end UpdateTotals


        private void OutputSubtotal(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, int whichSubtotal,
                                        List<ReportSubtotal> subtotalToPrint, string prodType)
        {
            string fieldFormat1 = "{0,10:F1}";
            string fieldFormat2 = "{0,7:F2}";
            string fieldFormat3 = "{0,10:F0}";
            double finalErr = 0;
            //  write headers if needed
            WriteReportHeading(strWriteOut, rh.reportTitles[0], prodType, rh.reportTitles[1],
                                 completeHeader, 7, ref pageNumb, "");
            switch (whichSubtotal)
            {
                case 1:     //  product
                    if (currentReport == "ST3")
                        fieldLengths = new int[] { 6, 4, 4, 13, 11, 15, 19, 12, 10, 14, 10 };
                    else if (currentReport == "ST4")
                        fieldLengths = new int[] { 38, 4, 4, 13, 12, 16, 12 };
                    strWriteOut.WriteLine("");
                    strWriteOut.WriteLine(rh.STsubtotals[0]);
                    numOlines += 2;
                    break;
                case 2:     //  UOM
                    if (currentReport == "ST3")
                        fieldLengths = new int[] { 10, 4, 13, 11, 15, 19, 12, 10, 14, 10 };
                    else if (currentReport == "ST4")
                        fieldLengths = new int[] { 42, 4, 13, 12, 16, 12 };
                    strWriteOut.WriteLine("");
                    strWriteOut.WriteLine(rh.STsubtotals[1]);
                    numOlines += 2;
                    break;
                case 3: //  strata
                    if (currentReport == "ST3")
                        fieldLengths = new int[] { 1, 9, 4, 13, 11, 15, 19, 12, 10, 14, 10 };
                    else if (currentReport == "ST4")
                        fieldLengths = new int[] { 33, 9, 4, 13, 12, 16, 12 };
                    strWriteOut.WriteLine("");
                    strWriteOut.WriteLine(rh.STsubtotals[2]);
                    numOlines += 2;
                    break;
            }   //  end switch
            foreach (ReportSubtotal rs in subtotalToPrint)
            {
                finalErr = 0;
                //  write headers if needed
                WriteReportHeading(strWriteOut, rh.reportTitles[0], prodType, rh.reportTitles[1], 
                                     completeHeader, 7, ref pageNumb, "");
                switch (whichSubtotal)
                {
                    case 1:
                        //  product
                        prtFields.Add("");
                        prtFields.Add(rs.Value1.PadLeft(2, ' '));
                        prtFields.Add(rs.Value2.PadLeft(2, ' '));
                        break;
                    case 2:
                        //  UOM
                        prtFields.Add("");
                        prtFields.Add(rs.Value1.PadLeft(2, ' '));
                        break;
                    case 3:
                        //  strata
                        prtFields.Add("");
                        prtFields.Add(rs.Value1.PadLeft(2, ' '));
                        prtFields.Add(rs.Value2.PadLeft(2, ' '));
                        break;
                }   //  end switch 


                //  Gross values
                prtFields.Add(Utilities.Format(fieldFormat1, rs.Value3));
                //  Calculate final error
                if(rs.Value3 > 0) finalErr = Math.Sqrt(rs.Value5) / rs.Value3;
                prtFields.Add(Utilities.Format(fieldFormat2, finalErr));
                //  Calculate confidence intervals
                prtFields.Add(Utilities.Format(fieldFormat3, CommonStatistics.CalculateConfidence(rs.Value3, finalErr, "-")));
                prtFields.Add(Utilities.Format(fieldFormat3, CommonStatistics.CalculateConfidence(rs.Value3, finalErr, "+")));

                //  net is only for ST3
                if (currentReport == "ST3")
                {
                    //  net values
                    prtFields.Add(Utilities.Format(fieldFormat1, rs.Value4));
                    //  Calculate final error
                    if (rs.Value4 > 0) finalErr = Math.Sqrt(rs.Value6) / rs.Value4;
                    prtFields.Add(Utilities.Format(fieldFormat2, finalErr));
                    //  Calculate confidence intervals
                    prtFields.Add(Utilities.Format(fieldFormat3, CommonStatistics.CalculateConfidence(rs.Value4, finalErr, "-")));
                    prtFields.Add(Utilities.Format(fieldFormat3, CommonStatistics.CalculateConfidence(rs.Value4, finalErr, "+")));
                }   //  endif on current report

                //  output record
                if(rs.Value3 > 0 || rs.Value4 > 0) printOneRecord(fieldLengths, prtFields, strWriteOut);
                //  clear print fields for next group
                prtFields.Clear();
            }   //  end foreach loop
            return;
        }   //  end OutputSubtotals


        private void finishColumnHeaders(string[] headerToUse, string prodType)
        {
            //  clear out completeHeader first
            for (int j = 0; j < 7; j++)
                completeHeader[j] = null;

            for (int j = 0; j < headerToUse.Count(); j++)
                completeHeader[j] = headerToUse[j];

            completeHeader[0] = completeHeader[0].Replace("ZZZZZZZZZZZZZZZZZ", prodType);

            return;
        }   //  end finishColumnHeaders


        public class StatSums
        {
            public string ST { get; set; }
            public string PP { get; set; }
            public string SP { get; set; }
            public string UOM { get; set; }
            public string SG { get; set; }
            public double ST1Xgross { get; set; }
            public double ST1Xnet { get; set; }
            public double ST1X2gross { get; set; }
            public double ST1X2net { get; set; }
            public double ST2Xgross { get; set; }
            public double ST2Xnet { get; set; }
            public double ST2X2gross { get; set; }
            public double ST2X2net { get; set; }
            public double GrossVol { get; set; }
            public double NetVol { get; set; }
            public double ST1Samps { get; set; }
            public double ST2Samps { get; set; }
            public double STRtrees { get; set; }
            public double GrossErrSq { get; set; }
            public double NetErrSq { get; set; }
        }   //  end class StatSums
    }
}
