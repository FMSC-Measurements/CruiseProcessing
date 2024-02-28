using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing
{
    public class SumAll
    {
        public class TempPOPvalues
        {
            public string tpLeave { get; set; }
            public string tpStratum { get; set; }
            public string tpPlot { get; set; }
            public string tpSampGrp { get; set; }
            public string tpPrimProd { get; set; }
            public string tpSecondProd { get; set; }
            public string tpUOM { get; set; }
            public double tpGrossPP { get; set; }
            public double tpGrossSqPP { get; set; }
            public double tpNetPP { get; set; }
            public double tpNetSqPP { get; set; }
            public double tpValPP { get; set; }
            public double tpValSqPP { get; set; }
            public double tpGrossSP { get; set; }
            public double tpGrossSqSP { get; set; }
            public double tpNetSP { get; set; }
            public double tpNetSqSP { get; set; }
            public double tpValSP { get; set; }
            public double tpValSqSP { get; set; }
            public double tpGrossRP { get; set; }
            public double tpGrossSqRP { get; set; }
            public double tpNetRP { get; set; }
            public double tpNetSqRP { get; set; }
            public double tpValRP { get; set; }
            public double tpValSqRP { get; set; }
        }   //  end class TempPOPvalues

        private double theGrossNumer;
        private double theNetNumer;
        private double theValNumer;
        private double theDenom;
        private double theExpanFac;
        private List<TempPOPvalues> tpopList = new List<TempPOPvalues>();

        protected CPbusinessLayer DataLayer { get; }

        public SumAll(CPbusinessLayer dataLayer)
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        public void SumAllValues(string currST, string currMeth, int currST_CN, List<StratumDO> sList, List<PlotDO> pList,
                                        List<LCDDO> justCurrentLCD, List<POPDO> justCurrentPOP, List<PRODO> justCurrentPRO)
        {
            string[] listOfields = new string[36] {"GBDFTP","NBDFTP","GBDFTS","NBDFTS","GCUFTP","NCUFTP",
                                                    "GCUFTS","NCUFTS","TIP","CORDP","CORDS","BDFTR","CUFTR",
                                                    "CORDR","BDFTREMP","CUFTREMP","TOTCUFT",
                                                    "BIOMSP","BIOMSS","BIOTS","BIOF","BIOLB","BIODB","BIOTIP",
                                                    "VALP","VALS","VALR","DBHSUM","DBHSQD","LOGMS",
                                                    "EXPFAC","TOTHTSUM","MHPSUM","MHSSUM","HUSDSUM","LOGSTOP"};
            //  loops through current LCD for groups to sum
            //  need all tree calculated values too for this stratum
            List<TreeCalculatedValuesDO> tcvList = DataLayer.getTreeCalculatedValues(currST_CN);
            List<TreeCalculatedValuesDO> justThisGroup = new List<TreeCalculatedValuesDO>();
            foreach (LCDDO ldo in justCurrentLCD)
            {
                //  Well, stupid, you need just the current ldo record from the calculated values list
                //  Plus if the method is FIXCNT, there aren't any records in calculated values because no volume is calculated
                if (currMeth == "FIXCNT")
                {
                    SumUpJustFixcnt(currST_CN, ldo, tcvList);
                }
                else
                {
                    //  get just trees for current LCD group
                    justThisGroup = DataLayer.GetLCDtrees(currST, ldo, "M");
                    //  loop through list of fields to sum
                    for (int k = 0; k < 35; k++)
                    {
                        SumUpValues(ldo, listOfields[k], justThisGroup);
                    }   //  end for k loop
                }   //  endif
            }   //  end foreach loop

            DataLayer.SaveLCD(justCurrentLCD);

            //  Calculate ratio, plot values, etc. for statistics in POP table
            foreach (POPDO pdo in justCurrentPOP)
            {
                //  Get all trees for this group
                List<TreeCalculatedValuesDO> currPOPtrees = DataLayer.GetPOPtrees(pdo, currST, "M");
                //  Determine if there is recoverable product to accumulate
                double sumRecover = currPOPtrees.Sum(tcv => tcv.Tree.RecoverablePrimary);
                //  Also for POP make sure there is secondary to accumulate
                double sumSecondary = justCurrentLCD.Sum(ld => ld.SumNCUFTtop);

                if (currMeth == "3P" || currMeth == "S3P" || currMeth == "F3P" ||
                    currMeth == "P3P" || currMeth == "PCM" || currMeth == "PCMTRE")
                {
                    CalculateRatios(currPOPtrees, currMeth, pdo, sumRecover);
                    //  accumulate stage 2 here
                    if (currMeth == "S3P" || currMeth == "F3P" || currMeth == "P3P" ||
                        currMeth == "PCM" || currMeth == "PCMTRE")
                        AccumulateStage2(currPOPtrees, currMeth, pdo, sumRecover);
                }   //  endif for stage 2 values

                if (currMeth != "3P")
                    tpopList.Clear();
                if (currMeth == "F3P" || currMeth == "P3P" || currMeth == "PCM" || currMeth == "PCMTRE" ||
                    currMeth == "PNT" || currMeth == "FIX" || currMeth == "FIXCNT" ||
                    currMeth == "FCM" || currMeth == "3PPNT")
                {
                    //  get fixed plot size for current stratum if FIX or F3P
                    if (currMeth == "FIX" || currMeth == "F3P")
                        theExpanFac = StratumMethods.CheckMethod(sList, currST);
                    List<PlotDO> justPlots = pList.FindAll(
                        delegate (PlotDO plt)
                        {
                            return plt.Stratum_CN == currST_CN;
                        });
                    CalculatePlotValues(currMeth, pdo, sumRecover, currPOPtrees, justPlots);
                    if (currMeth == "FCM")
                        //  accumulate stage2
                        AccumulateStage2(currPOPtrees, currMeth, pdo, sumRecover);
                }   //  endif

                //  accumulate stage 1 here
                if (currMeth == "S3P" || currMeth == "100" || currMeth == "STR")
                {
                    //  pull current sample group to process
                    AccumulateStage1(currPOPtrees, currMeth, pdo, sumRecover);
                }
                else if (currMeth == "3PPNT")
                {
                    //  stage 1 is sum of all plot KPIs so pull stratum from plot table
                    //  and the method will need a separate function since table to sum will be a PlotDO
                    List<PlotDO> currentPlots = DataLayer.GetStrataPlots(pdo.Stratum);
                    Accumulate3PPNTstage1(currentPlots, pdo, sumRecover, sumSecondary);
                    //  stage 2 also needs to be summed for this method
                    AccumulateStage2(currPOPtrees, currMeth, pdo, sumRecover);
                }
                else AccumulateStage1(pdo, sumRecover);

                //  clear pop list for next group
                tpopList.Clear();
            }   //  end foreach loop

            DataLayer.SavePOP(justCurrentPOP);

            //  Calculate proration factors for this stratum
            double unitAcres = 0;
            foreach (PRODO prdo in justCurrentPRO)
            {
                //  get unit acres
                List<CuttingUnitDO> cutList = DataLayer.getCuttingUnits();
                unitAcres = CuttingUnitMethods.GetUnitAcres(cutList, prdo.CuttingUnit);
                CalculateProration(prdo, justCurrentLCD, justCurrentPOP, unitAcres, currMeth);
            }   //  end foreach loop

            DataLayer.SavePRO(justCurrentPRO);

            return;
        }   //  end SumAllValues

        private void SumUpValues(LCDDO ldo, string fieldToSum, List<TreeCalculatedValuesDO> tcvList)
        {
            //  sums up field to sum and stores in current object
            //  for merch height primary and secondary, need to find by height type
            List<TreeCalculatedValuesDO> justFeet = tcvList.FindAll(
                delegate (TreeCalculatedValuesDO tv)
                {
                    return tv.Tree.TreeDefaultValue.MerchHeightType == "F" ||
                        tv.Tree.TreeDefaultValue.MerchHeightType == "f" ||
                        tv.Tree.TreeDefaultValue.MerchHeightType == "" ||
                        tv.Tree.TreeDefaultValue.MerchHeightType == null;
                });
            List<TreeCalculatedValuesDO> justLogs = new List<TreeCalculatedValuesDO>();
            if (justFeet.Count == 0)
            {
                //  look for height type of "L"
                justLogs = tcvList.FindAll(
                    delegate (TreeCalculatedValuesDO tdo)
                    {
                        return tdo.Tree.TreeDefaultValue.MerchHeightType == "L" ||
                            tdo.Tree.TreeDefaultValue.MerchHeightType == "l";
                    });
            }   //  endif
            switch (fieldToSum)
            {
                case "GBDFTP":      //  gross BDFT primary product volume
                    ldo.SumGBDFT = tcvList.Sum(tcv => tcv.GrossBDFTPP * tcv.Tree.ExpansionFactor);
                    break;

                case "NBDFTP":      //  net BDFT primary product volume
                    ldo.SumNBDFT = tcvList.Sum(tcv => tcv.NetBDFTPP * tcv.Tree.ExpansionFactor);
                    break;

                case "GBDFTS":      //  gross BDFT secondary product volume
                    ldo.SumGBDFTtop = tcvList.Sum(tcv => tcv.GrossBDFTSP * tcv.Tree.ExpansionFactor); ;
                    break;

                case "NBDFTS":      //  net BDFT secondary product volume
                    ldo.SumNBDFTtop = tcvList.Sum(tcv => tcv.NetBDFTSP * tcv.Tree.ExpansionFactor);
                    break;

                case "GCUFTP":      //  gross CUFT primary product volume
                    ldo.SumGCUFT = tcvList.Sum(tcv => tcv.GrossCUFTPP * tcv.Tree.ExpansionFactor);
                    break;

                case "NCUFTP":      //  net CUFT primary product volume
                    ldo.SumNCUFT = tcvList.Sum(tcv => tcv.NetCUFTPP * tcv.Tree.ExpansionFactor);
                    break;

                case "GCUFTS":      //  gross CUFT secondary product volume
                    ldo.SumGCUFTtop = tcvList.Sum(tcv => tcv.GrossCUFTSP * tcv.Tree.ExpansionFactor);
                    break;

                case "NCUFTS":      //  net CUFT secondary product volume
                    ldo.SumNCUFTtop = tcvList.Sum(tcv => tcv.NetCUFTSP * tcv.Tree.ExpansionFactor);
                    break;

                case "TIP":
                    ldo.SumTipwood = tcvList.Sum(tcv => tcv.TipwoodVolume * tcv.Tree.ExpansionFactor);
                    break;

                case "CORDP":       //  primary product cords
                    ldo.SumCords = tcvList.Sum(tcv => tcv.CordsPP * tcv.Tree.ExpansionFactor);
                    break;

                case "CORDS":       //  secondary product cords
                    ldo.SumCordsTop = tcvList.Sum(tcv => tcv.CordsSP * tcv.Tree.ExpansionFactor);
                    break;

                case "CORDR":       //  recovered product cords
                    ldo.SumCordsRecv = tcvList.Sum(tcv => tcv.CordsRP * tcv.Tree.ExpansionFactor);
                    break;

                case "BDFTR":       //  BDFT recovered product
                    ldo.SumBDFTrecv = tcvList.Sum(tcv => tcv.GrossBDFTRP * tcv.Tree.ExpansionFactor);
                    break;

                case "CUFTR":       //  CUFT recovered product
                    ldo.SumCUFTrecv = tcvList.Sum(tcv => tcv.GrossCUFTRP * tcv.Tree.ExpansionFactor);
                    break;

                case "BDFTREMP":    //  BDFT removed primary product
                    ldo.SumGBDFTremv = tcvList.Sum(tcv => tcv.GrossBDFTRemvPP * tcv.Tree.ExpansionFactor);
                    break;

                case "CUFTREMP":    //  CUFT removed primary product
                    ldo.SumGCUFTremv = tcvList.Sum(tcv => tcv.GrossCUFTRemvPP * tcv.Tree.ExpansionFactor);
                    break;

                case "TOTCUFT":     //  Total cubic
                    ldo.SumTotCubic = tcvList.Sum(tcv => tcv.TotalCubicVolume * tcv.Tree.ExpansionFactor);
                    break;

                case "BIOMSP":      //  Biomass mainstem primary product
                    ldo.SumWgtMSP = tcvList.Sum(tcv => tcv.BiomassMainStemPrimary * tcv.Tree.ExpansionFactor);
                    break;

                case "BIOMSS":      //  Biomass mainstem secondary product
                    ldo.SumWgtMSS = tcvList.Sum(tcv => tcv.BiomassMainStemSecondary * tcv.Tree.ExpansionFactor);
                    break;

                case "BIOTS":       //  Biomass total stem
                    ldo.SumWgtBAT = tcvList.Sum(tcv => tcv.Biomasstotalstem * tcv.Tree.ExpansionFactor);
                    break;

                case "BIOF":        //  Biomass foliage
                    ldo.SumWgtBFT = tcvList.Sum(tcv => tcv.Biomassfoliage * tcv.Tree.ExpansionFactor);
                    break;

                case "BIOLB":       //  Biomass live branches
                    ldo.SumWgtBBL = tcvList.Sum(tcv => tcv.Biomasslivebranches * tcv.Tree.ExpansionFactor);
                    break;

                case "BIODB":       //  Biomass dead branches
                    ldo.SumWgtBBD = tcvList.Sum(tcv => tcv.Biomassdeadbranches * tcv.Tree.ExpansionFactor);
                    break;

                case "BIOTIP":      //  Biomass tip
                    ldo.SumWgtTip = tcvList.Sum(tcv => tcv.BiomassTip * tcv.Tree.ExpansionFactor);
                    break;

                case "VALP":        //  Value primary product
                    ldo.SumValue = tcvList.Sum(tcv => tcv.ValuePP * tcv.Tree.ExpansionFactor);
                    break;

                case "VALS":        //  Value secondary product
                    ldo.SumTopValue = tcvList.Sum(tcv => tcv.ValueSP * tcv.Tree.ExpansionFactor);
                    break;

                case "VALR":        //  Value recovered product
                    ldo.SumValueRecv = tcvList.Sum(tcv => tcv.ValueRP * tcv.Tree.ExpansionFactor);
                    break;

                case "DBHSUM":      //  DBH sum
                    ldo.SumDBHOB = tcvList.Sum(tcv => tcv.Tree.DBH * tcv.Tree.ExpansionFactor);
                    break;

                case "DBHSQD":      //  DBH squared
                    ldo.SumDBHOBsqrd = tcvList.Sum(tcv => (Math.Pow(tcv.Tree.DBH, 2)) * tcv.Tree.ExpansionFactor);
                    break;

                case "LOGMS":       //  number of logs mainstem
                    ldo.SumLogsMS = tcvList.Sum(tcv => tcv.NumberlogsMS * tcv.Tree.ExpansionFactor);
                    break;

                case "EXPFAC":      //  expansion factor
                    ldo.SumExpanFactor = tcvList.Sum(tcv => tcv.Tree.ExpansionFactor);
                    break;

                case "TOTHTSUM":    //  total height
                    ldo.SumTotHgt = tcvList.Sum(tcv => tcv.Tree.TotalHeight * tcv.Tree.ExpansionFactor);
                    break;

                case "MHPSUM":      //  merch height primary
                    if (justFeet.Count > 0)
                        ldo.SumMerchHgtPrim += justFeet.Sum(tcv => tcv.Tree.MerchHeightPrimary * tcv.Tree.ExpansionFactor);
                    else if (justFeet.Count == 0)
                        ldo.SumMerchHgtPrim += justLogs.Sum(tcv => (tcv.Tree.MerchHeightPrimary / 10) * tcv.Tree.ExpansionFactor);
                    break;

                case "MHSSUM":      //  merch height secondary
                    if (justFeet.Count > 0)
                        ldo.SumMerchHgtSecond += justFeet.Sum(tcv => tcv.Tree.MerchHeightSecondary * tcv.Tree.ExpansionFactor);
                    else if (justFeet.Count == 0)
                        ldo.SumMerchHgtSecond += justLogs.Sum(tcv => (tcv.Tree.MerchHeightSecondary / 10) * tcv.Tree.ExpansionFactor);
                    break;

                case "HUSDSUM":     //  height to upper stem diameter
                    ldo.SumHgtUpStem = tcvList.Sum(tcv => tcv.Tree.UpperStemHeight * tcv.Tree.ExpansionFactor);
                    break;

                case "LOGSTOP":     //  number of logs topwood
                    ldo.SumLogsTop = tcvList.Sum(tcv => tcv.NumberlogsTPW * tcv.Tree.ExpansionFactor);
                    break;
            }   //  end switch on field to sum
            return;
        }   //  end SumUpValues

        private void SumUpJustFixcnt(long currST_CN, LCDDO ldo, List<TreeCalculatedValuesDO> stratumTrees)
        {
            //  FIXCNT has no volume calculated so it doesn't show up in the TreeCalculatedValues table
            //  And basically the summed value is the expansion factor, DBH, DBH squared and maybe heights
            //  So need just the count trees for this stratum
            //  This is just for the LCD table
            //  June 2016 -- added the ability to calculatebiomass for this method so need to sum up the biomass fields
            //List<TreeDO> stratumTrees = bslyr.JustFIXCNTtrees(currST_CN);
            //  find current group
            List<TreeCalculatedValuesDO> justCurrentGroup = stratumTrees.FindAll(
                delegate (TreeCalculatedValuesDO tcv)
                {
                    return tcv.Tree.Species == ldo.Species && tcv.Tree.SampleGroup.CutLeave == ldo.CutLeave &&
                        tcv.Tree.SampleGroup.Code == ldo.SampleGroup &&
                        tcv.Tree.SampleGroup.PrimaryProduct == ldo.PrimaryProduct &&
                        tcv.Tree.SampleGroup.SecondaryProduct == ldo.SecondaryProduct &&
                        tcv.Tree.SampleGroup.UOM == ldo.UOM && tcv.Tree.LiveDead == ldo.LiveDead &&
                        //  September 2016 -- dropping contract species from LCD identifier
                        //tcv.Tree.Grade == ldo.TreeGrade && tcv.Tree.TreeDefaultValue.ContractSpecies == ldo.ContractSpecies &&
                        tcv.Tree.Grade == ldo.TreeGrade && tcv.Tree.Stratum.YieldComponent == ldo.Yield;
                });
            ldo.SumDBHOB = justCurrentGroup.Sum(t => t.Tree.DBH * t.Tree.ExpansionFactor);
            ldo.SumDBHOBsqrd = justCurrentGroup.Sum(t => Math.Pow(t.Tree.DBH, 2) * t.Tree.ExpansionFactor);
            ldo.SumTotHgt = justCurrentGroup.Sum(t => t.Tree.TotalHeight * t.Tree.ExpansionFactor);
            ldo.SumMerchHgtPrim = justCurrentGroup.Sum(t => t.Tree.MerchHeightPrimary * t.Tree.ExpansionFactor);
            ldo.SumMerchHgtSecond = justCurrentGroup.Sum(t => t.Tree.MerchHeightSecondary * t.Tree.ExpansionFactor);
            ldo.SumHgtUpStem = justCurrentGroup.Sum(t => t.Tree.UpperStemHeight * t.Tree.ExpansionFactor);
            ldo.SumExpanFactor = justCurrentGroup.Sum(t => t.Tree.ExpansionFactor);
            //  Add BiomassEqMethods fields
            ldo.SumWgtBAT = justCurrentGroup.Sum(t => t.Biomasstotalstem);
            ldo.SumWgtBBD = justCurrentGroup.Sum(t => t.Biomassdeadbranches);
            ldo.SumWgtBBL = justCurrentGroup.Sum(t => t.Biomasslivebranches);
            ldo.SumWgtBFT = justCurrentGroup.Sum(t => t.Biomassfoliage);
            ldo.SumWgtMSP = justCurrentGroup.Sum(t => t.BiomassMainStemPrimary);
            ldo.SumWgtMSS = justCurrentGroup.Sum(t => t.BiomassMainStemSecondary);
            ldo.SumWgtTip = justCurrentGroup.Sum(t => t.BiomassTip);

            return;
        }   //  end SumUpJustFixcnt

        private void CalculateRatios(List<TreeCalculatedValuesDO> tcvList, string currMethod,
                                                POPDO pdo, double recvSum)
        {
            //  need to capture plot number as methods such as 3P will have a plot_cn of zero instead a blank plot number
            //  like in the current CP --  so need a variable for plot number
            int currPlot;

            //  calculate ratio based on method and UOM
            //  for current sample group
            foreach (TreeCalculatedValuesDO tcv in tcvList)
            {
                //  check plot_cn here
                if (tcv.Tree.Plot_CN == 0 || tcv.Tree.Plot == null)
                    currPlot = 0;
                else currPlot = (int)tcv.Tree.Plot.PlotNumber;

                //  what is denominator?
                switch (currMethod)
                {
                    case "3P":
                    case "S3P":
                    case "F3P":
                        theDenom = (double)tcv.Tree.KPI;
                        break;

                    case "P3P":
                        theDenom = Math.Pow(tcv.Tree.DBH, 2) * tcv.Tree.KPI;
                        break;

                    case "PCM":
                    case "PCMTRE":
                        theDenom = Math.Pow(tcv.Tree.DBH, 2) * 0.005454;
                        break;

                    default:
                        theDenom = 0;
                        break;
                }   //  end switch

                //  what volume to use?
                switch (tcv.Tree.SampleGroup.UOM)
                {
                    case "01":      //  BDFT
                        //  primary product
                        theGrossNumer = tcv.GrossBDFTPP;
                        theNetNumer = tcv.NetBDFTPP;
                        theValNumer = tcv.ValuePP;
                        CalcAndStorePrimary(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        //  Secondary product
                        theGrossNumer = tcv.GrossBDFTSP;
                        theNetNumer = tcv.NetBDFTSP;
                        theValNumer = tcv.ValueSP;
                        CalcAndStoreSecondary(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        //  Recovered product
                        if (recvSum > 0)
                        {
                            theGrossNumer = tcv.GrossBDFTRP;
                            theNetNumer = theGrossNumer;
                            theValNumer = tcv.ValueRP;
                            CalcAndStoreRecovered(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        }   //  endif recovered
                        break;

                    case "02":      //  Cords
                        //  primary product
                        theGrossNumer = tcv.CordsPP;
                        theNetNumer = theGrossNumer;
                        theValNumer = tcv.ValuePP;
                        CalcAndStorePrimary(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        //  secondary product
                        theGrossNumer = tcv.CordsSP;
                        theNetNumer = theGrossNumer;
                        theValNumer = tcv.ValueSP;
                        CalcAndStoreSecondary(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        //  recovered product
                        if (recvSum > 0)
                        {
                            theGrossNumer = tcv.CordsRP;
                            theNetNumer = theGrossNumer;
                            theValNumer = tcv.ValueRP;
                            CalcAndStoreRecovered(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        }   //  endif recovered
                        break;

                    case "03":      //  CUFT
                        //  primary product
                        theGrossNumer = tcv.GrossCUFTPP;
                        theNetNumer = tcv.NetCUFTPP;
                        theValNumer = tcv.ValuePP;
                        CalcAndStorePrimary(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        //  secondary product
                        theGrossNumer = tcv.GrossCUFTSP;
                        theNetNumer = tcv.NetCUFTSP;
                        theValNumer = tcv.ValueSP;
                        CalcAndStoreSecondary(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        //  recovered product
                        if (recvSum > 0)
                        {
                            theGrossNumer = tcv.GrossCUFTRP;
                            theNetNumer = theGrossNumer;
                            theValNumer = tcv.ValueRP;
                            CalcAndStoreRecovered(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        }   //  endif recovered
                        break;

                    case "05":      //  weight
                        //  primary product
                        theGrossNumer = tcv.BiomassMainStemPrimary;
                        theNetNumer = theGrossNumer;
                        theValNumer = 0;
                        CalcAndStorePrimary(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        //  secondary product
                        theGrossNumer = tcv.BiomassMainStemSecondary;
                        theNetNumer = theGrossNumer;
                        theValNumer = 0;
                        CalcAndStoreSecondary(theGrossNumer, theNetNumer, theValNumer, theDenom, pdo,
                                                currPlot);
                        //  no recovered for weight
                        //  log a warning if theGrossNumer is zero--means flag not checked
                        if (theGrossNumer == 0)
                            DataLayer.LogError("TreeCalculatedValues", (int)tcv.Tree_CN, "W", "21");
                        break;
                }   //  end switch
            }   //  end foreach loop
            return;
        }   //  end CalculateRatios

        private void CalcAndStorePrimary(double theGrossNumer, double theNetNumer, double theValNumer,
                                                double theDenom, POPDO pdo, long currPlot)
        {
            TempPOPvalues tpop = new TempPOPvalues();
            if (theDenom > 0 && theGrossNumer > 0)
            {
                tpop.tpGrossPP = theGrossNumer / theDenom;
                tpop.tpGrossSqPP = Math.Pow((theGrossNumer / theDenom), 2);
                tpop.tpNetPP = theNetNumer / theDenom;
                tpop.tpNetSqPP = Math.Pow((theNetNumer / theDenom), 2);
                if (theValNumer > 0)
                {
                    tpop.tpValPP = theValNumer / theDenom;
                    tpop.tpValSqPP = Math.Pow((theValNumer / theDenom), 2);
                }
                else
                {
                    tpop.tpValPP = 0;
                    tpop.tpValSqPP = 0;
                }   //  endif theValNumer greater than zero
            }   //  endif
            //  store it

            tpop.tpLeave = pdo.CutLeave;
            tpop.tpStratum = pdo.Stratum;
            tpop.tpPlot = currPlot.ToString();
            tpop.tpSampGrp = pdo.SampleGroup;
            tpop.tpPrimProd = pdo.PrimaryProduct;
            tpop.tpSecondProd = pdo.SecondaryProduct;
            tpop.tpUOM = pdo.UOM;

            tpopList.Add(tpop);
            return;
        }   //  end CalcAndStorePrimary

        private void CalcAndStoreSecondary(double theGrossNumer, double theNetNumer, double theValNumer,
                                                    double theDenom, POPDO pdo, long currPlot)
        {
            TempPOPvalues tpop = new TempPOPvalues();
            if (theDenom > 0 && theGrossNumer > 0)
            {
                tpop.tpGrossSP = theGrossNumer / theDenom;
                tpop.tpGrossSqSP = Math.Pow((theGrossNumer / theDenom), 2);
                tpop.tpNetSP = theNetNumer / theDenom;
                tpop.tpNetSqSP = Math.Pow((theNetNumer / theDenom), 2);
                if (theValNumer > 0)
                {
                    tpop.tpValSP = theValNumer / theDenom;
                    tpop.tpValSqSP = Math.Pow((theValNumer / theDenom), 2);
                }
                else
                {
                    tpop.tpValSP = 0;
                    tpop.tpValSqSP = 0;
                }   //  endif theValNumer greater than zero
            }   //  endif
            //  store it
            tpop.tpLeave = pdo.CutLeave;
            tpop.tpStratum = pdo.Stratum;
            tpop.tpPlot = currPlot.ToString();
            tpop.tpSampGrp = pdo.SampleGroup;
            tpop.tpPrimProd = pdo.PrimaryProduct;
            tpop.tpSecondProd = pdo.SecondaryProduct;
            tpop.tpUOM = pdo.UOM;

            tpopList.Add(tpop);
            return;
        }   //  end CalcAndStoreSecondary

        private void CalcAndStoreRecovered(double theGrossNumer, double theNetNumer, double theValNumer, double theDenom,
                                                    POPDO pdo, long currPlot)
        {
            TempPOPvalues tpop = new TempPOPvalues();
            if (theDenom > 0 && theGrossNumer > 0)
            {
                tpop.tpGrossRP = theGrossNumer / theDenom;
                tpop.tpGrossSqRP = Math.Pow((theGrossNumer / theDenom), 2);
                tpop.tpNetRP = theNetNumer / theDenom;
                tpop.tpNetSqRP = Math.Pow((theNetNumer / theDenom), 2);
                if (theValNumer > 0)
                {
                    tpop.tpValRP = theValNumer / theDenom;
                    tpop.tpValSqRP = Math.Pow((theValNumer / theDenom), 2);
                }
                else
                {
                    tpop.tpValRP = 0;
                    tpop.tpValSqRP = 0;
                }   //  endif theValNumer greater than zero
            }   //  endif
            //  store it
            tpop.tpLeave = pdo.CutLeave;
            tpop.tpStratum = pdo.Stratum;
            tpop.tpPlot = currPlot.ToString();
            tpop.tpSampGrp = pdo.SampleGroup;
            tpop.tpPrimProd = pdo.PrimaryProduct;
            tpop.tpSecondProd = pdo.SecondaryProduct;
            tpop.tpUOM = pdo.UOM;

            tpopList.Add(tpop);
            return;
        }   //  end CalcAndStoreRecovered

        private void CalculatePlotValues(string currMethod, POPDO pdo, double recvSum,
                                            List<TreeCalculatedValuesDO> tcvList, List<PlotDO> justPlots)
        {
            double grossSumPP = 0;
            double grossSumSP = 0;
            double grossSumRP = 0;
            double netSumPP = 0;
            double netSumSP = 0;
            double netSumRP = 0;
            double valSumPP = 0;
            double valSumSP = 0;
            double valSumRP = 0;
            string currPlot;

            //  process by plots
            foreach (PlotDO plt in justPlots)
            {
                //  find plot data in current group
                // Save plot number
                currPlot = plt.PlotNumber.ToString();
                //  method dependent
                if (currMethod == "FCM" || currMethod == "FIXCNT" || currMethod == "PCM" || currMethod == "PCMTRE" ||
                    currMethod == "F3P" || currMethod == "P3P")
                {
                    //  since this totals plot values using tree count, find all trees in Tree for this plot
                    string[] valuesArray = new string[3] { plt.Plot_CN.ToString(), plt.Stratum_CN.ToString(), pdo.SampleGroup };
                    List<TreeDO> justTrees = DataLayer.getTreesOrdered("WHERE Plot_CN = @p1 AND Stratum_CN = @p2 ORDER BY ",
                                                                "TreeNumber", valuesArray);
                    List<TreeDO> currentSampleGroup = justTrees.FindAll(
                        delegate (TreeDO t)
                        {
                            return t.SampleGroup.Code == pdo.SampleGroup;
                        });
                    if (currMethod == "P3P")
                    {
                        theExpanFac = 1.0;
                        foreach (TreeDO tdo in currentSampleGroup)
                        {
                            //  may require modification if STM flag added back to
                            //  the commented line may need to be used based on outcome of meeting next week on STM
                            //  if(currMethod == "P3P" and non-sure-to-measure)
                            //  these need to be separated because P3P is expanded but F3P is not
                            //  STM added to pop table so the next two lines are now valid -- April 2013
                            if (pdo.STM == "Y") theExpanFac = tdo.PointFactor;
                            grossSumPP += tdo.KPI * theExpanFac;
                            netSumPP += tdo.KPI * theExpanFac;
                            valSumPP += tdo.KPI * theExpanFac;
                            grossSumSP += tdo.KPI * theExpanFac;
                            netSumSP += tdo.KPI * theExpanFac;
                            valSumSP += tdo.KPI * theExpanFac;
                            if (recvSum > 0)
                            {
                                grossSumRP += tdo.KPI * theExpanFac;
                                netSumRP += tdo.KPI * theExpanFac;
                                valSumRP += tdo.KPI * theExpanFac;
                            }   //  endif
                        }   //  end foreach loop
                    }
                    else if (currMethod == "F3P")
                    {
                        foreach (TreeDO tdo in currentSampleGroup)
                        {
                            grossSumPP += tdo.KPI;
                            netSumPP += tdo.KPI;
                            valSumPP += tdo.KPI;
                            //  make sure secondary was calculated before summing up KPI
                            int nthRow = tcvList.FindIndex(
                                delegate (TreeCalculatedValuesDO tv)
                                {
                                    return tv.Tree_CN == tdo.Tree_CN;
                                });
                            if (nthRow >= 0 && tcvList[nthRow].NetCUFTSP > 0)
                            {
                                grossSumSP += tdo.KPI;
                                netSumSP += tdo.KPI;
                                valSumSP += tdo.KPI;
                            }   //  endif nthRow

                            if (recvSum > 0)
                            {
                                grossSumRP += tdo.KPI;
                                netSumRP += tdo.KPI;
                                valSumRP += tdo.KPI;
                            }   //  endif
                        }   //  end foreach loop
                    }
                    else
                    {
                        foreach (TreeDO tdo in currentSampleGroup)
                        {
                            if (tdo.TreeCount > 0)
                            {
                                grossSumPP += tdo.TreeCount;
                                netSumPP += tdo.TreeCount;
                                valSumPP += tdo.TreeCount;
                                grossSumSP += tdo.TreeCount;
                                netSumSP += tdo.TreeCount;
                                valSumSP += tdo.TreeCount;
                                if (recvSum > 0)
                                {
                                    grossSumRP += tdo.TreeCount;
                                    netSumRP += tdo.TreeCount;
                                    valSumRP += tdo.TreeCount;
                                }   //  endif
                            }
                            else if (tdo.CountOrMeasure == "M")
                            {
                                grossSumPP++;
                                netSumPP++;
                                valSumPP++;
                                grossSumSP++;
                                netSumSP++;
                                valSumSP++;
                                if (recvSum > 0)
                                {
                                    grossSumRP++;
                                    netSumRP++;
                                    valSumRP++;
                                }   //  endif
                            }   //  endif tree count greater than zero
                        }   //  end foreach loop
                    }   //  endif currMethod
                }
                else
                {
                    List<TreeCalculatedValuesDO> plotTrees = tcvList.FindAll(
                        delegate (TreeCalculatedValuesDO tcv)
                        {
                            return plt.Plot_CN == tcv.Tree.Plot_CN && tcv.Tree.SampleGroup.Code == pdo.SampleGroup;
                        });

                    //  sum up plot tree values
                    foreach (TreeCalculatedValuesDO pt in plotTrees)
                    {
                        currPlot = pt.Tree.Plot.PlotNumber.ToString();
                        //  find expansion factor based on method
                        //  FIX and F3P are already set to FP size
                        switch (currMethod)
                        {
                            case "PNT":
                                theExpanFac = pt.Tree.TreeFactor;
                                break;

                            case "3PPNT":        //  ratio calculated for expansion factor
                                if (pt.Tree.Plot.KPI > 0)
                                    theExpanFac = pt.Tree.TreeFactor / pt.Tree.Plot.KPI;
                                else theExpanFac = 1.0;
                                break;
                        }   //  end switch for expansion factor

                        //  sum values based on method (base value used varies by method)
                        if (currMethod == "FIX" || currMethod == "PNT" || currMethod == "3PPNT")
                        {
                            //  calculate based on UOM for base value
                            switch (pt.Tree.SampleGroup.UOM)
                            {
                                case "01":      //  BDFT
                                    //  Primary product
                                    grossSumPP += pt.GrossBDFTPP * theExpanFac;
                                    netSumPP += pt.NetBDFTPP * theExpanFac;
                                    valSumPP += pt.ValuePP * theExpanFac;
                                    //  Secondary product
                                    grossSumSP += pt.GrossBDFTSP * theExpanFac;
                                    netSumSP += pt.NetBDFTSP * theExpanFac;
                                    valSumSP += pt.ValueSP * theExpanFac;
                                    //  Recovered product
                                    if (recvSum > 0)
                                    {
                                        grossSumRP += pt.GrossBDFTRP * theExpanFac;
                                        netSumRP += pt.GrossBDFTRP * theExpanFac;
                                        valSumRP += pt.ValueRP * theExpanFac;
                                    }   //  endif
                                    break;

                                case "02":      //  Cords
                                    //  Primary product
                                    grossSumPP += pt.CordsPP * theExpanFac;
                                    netSumPP += pt.CordsPP * theExpanFac;
                                    valSumPP += pt.ValuePP * theExpanFac;
                                    //  Secondary product
                                    grossSumSP += pt.CordsSP * theExpanFac;
                                    netSumSP += pt.CordsSP * theExpanFac;
                                    valSumSP += pt.ValueSP * theExpanFac;
                                    //  Recovered product
                                    if (recvSum > 0)
                                    {
                                        grossSumRP += pt.CordsRP * theExpanFac;
                                        netSumRP += pt.CordsRP * theExpanFac;
                                        valSumRP += pt.ValueRP * theExpanFac;
                                    }   //  endif
                                    break;

                                case "03":      //  CUFT
                                    //  Primary product
                                    grossSumPP += pt.GrossCUFTPP * theExpanFac;
                                    netSumPP += pt.NetCUFTPP * theExpanFac;
                                    valSumPP += pt.ValuePP * theExpanFac;
                                    //  secondary product
                                    grossSumSP += pt.GrossCUFTSP * theExpanFac;
                                    netSumSP += pt.NetCUFTSP * theExpanFac;
                                    valSumSP += pt.ValueSP * theExpanFac;
                                    //  recovered product
                                    if (recvSum > 0)
                                    {
                                        grossSumRP += pt.GrossCUFTRP * theExpanFac;
                                        netSumRP += pt.GrossCUFTRP * theExpanFac;
                                        valSumRP += pt.ValueRP * theExpanFac;
                                    }   //  endif
                                    break;

                                case "05":      //  Weight
                                    //  Primary product
                                    grossSumPP += pt.BiomassMainStemPrimary * theExpanFac;
                                    netSumPP += pt.BiomassMainStemPrimary * theExpanFac;
                                    //  secondary product
                                    grossSumSP += pt.BiomassMainStemSecondary * theExpanFac;
                                    netSumSP += pt.BiomassMainStemSecondary * theExpanFac;
                                    //  no recovered product for weight
                                    //  log warning messages if values equal zero - means biomass flag not checked
                                    if (grossSumPP == 0 && netSumPP == 0)
                                        DataLayer.LogError("TreeCalculatedValues", (int)pt.Tree_CN, "W", "21");
                                    break;
                            }   //  end switch on unit of measure
                        }   //  endif on method
                    }   //  end foreach loop
                }   //  endif on currMethod

                //  store plot sums
                TempPOPvalues tpop = new TempPOPvalues();
                tpop.tpLeave = pdo.CutLeave;
                tpop.tpStratum = pdo.Stratum;
                tpop.tpPlot = currPlot;
                tpop.tpSampGrp = pdo.SampleGroup;
                tpop.tpPrimProd = pdo.PrimaryProduct;
                tpop.tpSecondProd = pdo.SecondaryProduct;
                tpop.tpUOM = pdo.UOM;

                //  primary product
                tpop.tpGrossPP = grossSumPP;
                tpop.tpGrossSqPP = Math.Pow(grossSumPP, 2);
                tpop.tpNetPP = netSumPP;
                tpop.tpNetSqPP = Math.Pow(netSumPP, 2);
                tpop.tpValPP = valSumPP;
                tpop.tpValSqPP = Math.Pow(valSumPP, 2);
                //  secondary product
                tpop.tpGrossSP = grossSumSP;
                tpop.tpGrossSqSP = Math.Pow(grossSumSP, 2);
                tpop.tpNetSP = netSumSP;
                tpop.tpNetSqSP = Math.Pow(netSumSP, 2);
                tpop.tpValSP = valSumSP;
                tpop.tpValSqSP = Math.Pow(valSumSP, 2);
                //  recovered product
                tpop.tpGrossRP = grossSumRP;
                tpop.tpGrossSqRP = Math.Pow(grossSumRP, 2);
                tpop.tpNetRP = netSumRP;
                tpop.tpNetSqRP = Math.Pow(netSumRP, 2);
                tpop.tpValRP = valSumRP;
                tpop.tpValSqRP = Math.Pow(valSumRP, 2);

                tpopList.Add(tpop);

                //  reset sum variables
                grossSumPP = 0;
                netSumPP = 0;
                valSumPP = 0;
                grossSumSP = 0;
                netSumSP = 0;
                valSumSP = 0;
                grossSumRP = 0;
                netSumRP = 0;
                valSumRP = 0;
            }   //  end foreach loop
            return;
        }   //  end CalculatePlotValues

        private void AccumulateStage1(List<TreeCalculatedValuesDO> tcvList, string currMeth,
                                            POPDO pdo, double recvSum)
        {
            //  uses current trees to accumulate
            //  for methods S3P, 100 and STR
            double sumValue = 0;
            double sqrdValue = 0;

            //  Sum and store based on current method
            switch (currMeth)
            {
                case "S3P":
                    sumValue = tcvList.Sum(tcv => tcv.Tree.KPI);
                    sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.Tree.KPI, 2));
                    UpdateStage1(pdo, sumValue, sqrdValue, "All");
                    break;

                case "100":
                case "STR":
                    switch (pdo.UOM)
                    {
                        case "01":          // BDFT
                            //  Primary product
                            sumValue = tcvList.Sum(tcv => tcv.GrossBDFTPP);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.GrossBDFTPP, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXPP");
                            sumValue = tcvList.Sum(tcv => tcv.NetBDFTPP);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.NetBDFTPP, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "NetXPP");
                            //  secondary product
                            sumValue = tcvList.Sum(tcv => tcv.GrossBDFTSP);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.GrossBDFTSP, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXSP");
                            sumValue = tcvList.Sum(tcv => tcv.NetBDFTSP);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.NetBDFTSP, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "NetXSP");
                            //  recoverd product
                            if (recvSum > 0)
                            {
                                sumValue = tcvList.Sum(tcv => tcv.GrossBDFTRP);
                                sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.GrossBDFTRP, 2));
                                UpdateStage1(pdo, sumValue, sqrdValue, "GrossXRP");
                                sumValue = tcvList.Sum(tcv => tcv.GrossBDFTRP);
                                sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.GrossBDFTRP, 2));
                                UpdateStage1(pdo, sumValue, sqrdValue, "NetXRP");
                            }   //  endif recvSum
                            break;

                        case "02":          //  Cords
                            //  Primary product
                            sumValue = tcvList.Sum(tcv => tcv.CordsPP);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.CordsPP, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXPP");
                            UpdateStage1(pdo, sumValue, sqrdValue, "NetXPP");
                            //  Secondary product
                            sumValue = tcvList.Sum(tcv => tcv.CordsSP);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.CordsSP, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXSP");
                            UpdateStage1(pdo, sumValue, sqrdValue, "NetXSP");
                            //  recovered product
                            if (recvSum > 0)
                            {
                                sumValue = tcvList.Sum(tcv => tcv.CordsRP);
                                sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.CordsRP, 2));
                                UpdateStage1(pdo, sumValue, sqrdValue, "GrossXRP");
                                UpdateStage1(pdo, sumValue, sqrdValue, "NetXRP");
                            }   //  endif recvSum
                            break;

                        case "03":          //  CUFT
                            //  Primary product
                            sumValue = tcvList.Sum(tcv => tcv.GrossCUFTPP);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.GrossCUFTPP, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXPP");
                            sumValue = tcvList.Sum(tcv => tcv.NetCUFTPP);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.NetCUFTPP, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "NetXPP");
                            //  secondary product
                            sumValue = tcvList.Sum(tcv => tcv.GrossCUFTSP);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.GrossCUFTSP, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXSP");
                            sumValue = tcvList.Sum(tcv => tcv.NetCUFTSP);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.NetCUFTSP, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "NetXSP");
                            //  recovered product
                            if (recvSum > 0)
                            {
                                sumValue = tcvList.Sum(tcv => tcv.GrossCUFTRP);
                                sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.GrossCUFTRP, 2));
                                UpdateStage1(pdo, sumValue, sqrdValue, "GrossXRP");
                                // no real gross for recovered but stored anyway
                                // recovered all goes into net
                                UpdateStage1(pdo, sumValue, sqrdValue, "NetXRP");
                            }   //  endif recvSum
                            break;

                        case "05":          //  Weight
                            //  Primary product
                            sumValue = tcvList.Sum(tcv => tcv.BiomassMainStemPrimary);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.BiomassMainStemPrimary, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXPP");
                            UpdateStage1(pdo, sumValue, sqrdValue, "NetXPP");
                            //  Secondary product
                            sumValue = tcvList.Sum(tcv => tcv.BiomassMainStemSecondary);
                            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.BiomassMainStemSecondary, 2));
                            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXSP");
                            UpdateStage1(pdo, sumValue, sqrdValue, "NetXSP");
                            //  no recovered for 0
                            break;
                    }   //  end switch on unit of measure
                    break;
            }   //  end switch

            //  Value primary product
            sumValue = tcvList.Sum(tcv => tcv.ValuePP);
            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.ValuePP, 2));
            UpdateStage1(pdo, sumValue, sqrdValue, "ValXPP");
            //  Secondary product
            sumValue = tcvList.Sum(tcv => tcv.ValueSP);
            sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.ValueSP, 2));
            UpdateStage1(pdo, sumValue, sqrdValue, "ValXSP");
            //  Recovered product
            if (recvSum > 0)
            {
                sumValue = tcvList.Sum(tcv => tcv.ValueRP);
                sqrdValue = tcvList.Sum(tcv => Math.Pow(tcv.ValueRP, 22));
                UpdateStage1(pdo, sumValue, sqrdValue, "ValXRP");
            }   //  endif recvSum

            return;
        }   //  end AccumulateStage1

        private void AccumulateStage1(POPDO pdo, double recvSum)
        {
            //  overloaded method to store temporary list in POP object
            //  for methods 3P FIX PCM PNT F3P P3P FCM AND FIXCNT
            double sumValue = 0;
            double sqrdValue = 0;

            //  Primary product
            sumValue = tpopList.Sum(tpo => tpo.tpGrossPP);
            sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpGrossPP, 2));
            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXPP");
            sumValue = tpopList.Sum(tpo => tpo.tpNetPP);
            sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpNetPP, 2));
            UpdateStage1(pdo, sumValue, sqrdValue, "NetXPP");

            //  Secondary product
            sumValue = tpopList.Sum(tpo => tpo.tpGrossSP);
            sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpGrossSP, 2));
            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXSP");
            sumValue = tpopList.Sum(tpo => tpo.tpNetSP);
            sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpNetSP, 2));
            UpdateStage1(pdo, sumValue, sqrdValue, "NetXSP");

            //  Recovered product
            if (recvSum > 0)
            {
                sumValue = tpopList.Sum(tpo => tpo.tpGrossRP);
                sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpGrossRP, 2));
                UpdateStage1(pdo, sumValue, sqrdValue, "GrossXRP");
                sumValue = tpopList.Sum(tpo => tpo.tpNetRP);
                sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpNetRP, 2));
                UpdateStage1(pdo, sumValue, sqrdValue, "NetXRP");
            }   //  endif recvSum
            return;
        }   //  end AccumulateStage1

        public void Accumulate3PPNTstage1(List<PlotDO> currPlots, POPDO pdo, double recvSum, double secSum)
        {
            //  function sums plot KPIs for 3PPNT stage 1 values
            double sumValue = 0;
            double sqrdValue = 0;

            //  Primary product
            sumValue = currPlots.Sum(plt => plt.KPI);
            sqrdValue = currPlots.Sum(plt => Math.Pow(plt.KPI, 2));
            UpdateStage1(pdo, sumValue, sqrdValue, "GrossXPP");
            sumValue = currPlots.Sum(plt => plt.KPI);
            sqrdValue = currPlots.Sum(plt => Math.Pow(plt.KPI, 2));
            UpdateStage1(pdo, sumValue, sqrdValue, "NetXPP");

            //  Secondary product
            if (secSum > 0)
            {
                sumValue = currPlots.Sum(plt => plt.KPI);
                sqrdValue = currPlots.Sum(plt => Math.Pow(plt.KPI, 2));
                UpdateStage1(pdo, sumValue, sqrdValue, "GrossXSP");
                sumValue = currPlots.Sum(plt => plt.KPI);
                sqrdValue = currPlots.Sum(plt => Math.Pow(plt.KPI, 2));
                UpdateStage1(pdo, sumValue, sqrdValue, "NetXSP");
            }   //  endif secondary sum

            //  Recovered product
            if (recvSum > 0)
            {
                sumValue = currPlots.Sum(plt => plt.KPI);
                sqrdValue = currPlots.Sum(plt => Math.Pow(plt.KPI, 2));
                UpdateStage1(pdo, sumValue, sqrdValue, "GrossXRP");
                sumValue = currPlots.Sum(plt => plt.KPI);
                sqrdValue = currPlots.Sum(plt => Math.Pow(plt.KPI, 2));
                UpdateStage1(pdo, sumValue, sqrdValue, "NetXRP");
            }   //  endif recvSum

            //  Value primary product
            sumValue = currPlots.Sum(plt => plt.KPI);
            sqrdValue = currPlots.Sum(plt => Math.Pow(plt.KPI, 2));
            UpdateStage1(pdo, sumValue, sqrdValue, "ValXPP");
            //  Secondary product
            sumValue = currPlots.Sum(plt => plt.KPI);
            sqrdValue = currPlots.Sum(plt => Math.Pow(plt.KPI, 2));
            UpdateStage1(pdo, sumValue, sqrdValue, "ValXSP");
            //  Recovered product
            if (recvSum > 0)
            {
                sumValue = currPlots.Sum(plt => plt.KPI);
                sqrdValue = currPlots.Sum(plt => Math.Pow(plt.KPI, 22));
            }   //  endif recovered product
            return;
        }   //  end Accumulate3PPNTstage1

        private void UpdateStage1(POPDO pdo, double sValue, double sqValue, string fieldToUpdate)
        {
            switch (fieldToUpdate)
            {
                case "All":     //  means the same values are stored in all fields
                    pdo.Stg1GrossXPP = sValue;
                    pdo.Stg1GrossXsqrdPP = sqValue;
                    pdo.Stg1NetXPP = sValue;
                    pdo.Stg1NetXsqrdPP = sqValue;
                    pdo.Stg1ValueXPP = sValue;
                    pdo.Stg1ValueXsqrdPP = sqValue;
                    pdo.Stg1GrossXSP = sValue;
                    pdo.Stg1GrossXsqrdSP = sqValue;
                    pdo.Stg1NetXSP = sValue;
                    pdo.Stg1NetXsqrdSP = sqValue;
                    pdo.Stg1ValueXSP = sValue;
                    pdo.Stg1ValueXsqrdSP = sqValue;
                    pdo.Stg1GrossXRP = sValue;
                    pdo.Stg1GrossXsqrdRP = sqValue;
                    pdo.Stg1NetXRP = sValue;
                    pdo.Stg1NetXRsqrdRP = sqValue;
                    pdo.Stg1ValueXRP = sValue;
                    pdo.Stg1ValueXsqrdRP = sqValue;
                    return;

                case "GrossXPP":
                    pdo.Stg1GrossXPP = sValue;
                    pdo.Stg1GrossXsqrdPP = sqValue;
                    break;

                case "NetXPP":
                    pdo.Stg1NetXPP = sValue;
                    pdo.Stg1NetXsqrdPP = sqValue;
                    break;

                case "ValXPP":
                    pdo.Stg1ValueXPP = sValue;
                    pdo.Stg1ValueXsqrdPP = sqValue;
                    break;

                case "GrossXSP":
                    pdo.Stg1GrossXSP = sValue;
                    pdo.Stg1GrossXsqrdSP = sqValue;
                    break;

                case "NetXSP":
                    pdo.Stg1NetXSP = sValue;
                    pdo.Stg1NetXsqrdSP = sqValue;
                    break;

                case "ValXSP":
                    pdo.Stg1ValueXSP = sValue;
                    pdo.Stg1ValueXsqrdSP = sqValue;
                    break;

                case "GrossXRP":
                    pdo.Stg1GrossXRP = sValue;
                    pdo.Stg1GrossXsqrdRP = sqValue;
                    break;

                case "NetXRP":
                    pdo.Stg1NetXRP = sValue;
                    pdo.Stg1NetXRsqrdRP = sqValue;
                    break;

                case "ValXRP":
                    pdo.Stg1ValueXRP = sValue;
                    pdo.Stg1ValueXsqrdRP = sqValue;
                    break;
            }   //  end switch

            return;
        }   //  end UpdateStage1

        private void AccumulateStage2(List<TreeCalculatedValuesDO> justThisGroup, string currMeth,
                                                    POPDO pdo, double recvSum)
        {
            //  accumulates values for stage 2 and the following methods
            //  FCM PCM S3P F3P P3P
            double sumValue = 0;
            double sqrdValue = 0;

            switch (currMeth)
            {
                case "FCM":     //  this method sums the current trees based on UOM
                    switch (pdo.UOM)
                    {
                        case "01":      //  BDFT
                            //  primary product
                            sumValue = justThisGroup.Sum(jtg => jtg.GrossBDFTPP);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.GrossBDFTPP, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "GrossXPP");
                            sumValue = justThisGroup.Sum(jtg => jtg.NetBDFTPP);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.NetBDFTPP, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "NetXPP");
                            //  secondary product
                            sumValue = justThisGroup.Sum(jtg => jtg.GrossBDFTSP);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.GrossBDFTSP, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "GrossXSP");
                            sumValue = justThisGroup.Sum(jtg => jtg.NetBDFTSP);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.NetBDFTSP, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "NetXSP");
                            //  Recovered product
                            if (recvSum > 0)
                            {
                                sumValue = justThisGroup.Sum(jtg => jtg.GrossBDFTRP);
                                sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.GrossBDFTRP, 2));
                                UpdateStage2(pdo, sumValue, sqrdValue, "GrossXRP");
                                UpdateStage2(pdo, sumValue, sqrdValue, "NetXRP");
                            }   //  endif recvSum
                            break;

                        case "02":      //  Cords
                            //  Primary product
                            sumValue = justThisGroup.Sum(jtg => jtg.CordsPP);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.CordsPP, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "GrossXPP");
                            UpdateStage2(pdo, sumValue, sqrdValue, "NetXPP");
                            //  Secondary product
                            sumValue = justThisGroup.Sum(jtg => jtg.CordsSP);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.CordsSP, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "GrossXSP");
                            UpdateStage2(pdo, sumValue, sqrdValue, "NetXSP");
                            //  recovered product
                            if (recvSum > 0)
                            {
                                sumValue = justThisGroup.Sum(jtg => jtg.CordsRP);
                                sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.CordsRP, 2));
                                UpdateStage2(pdo, sumValue, sqrdValue, "GrossXRP");
                                UpdateStage2(pdo, sumValue, sqrdValue, "NetXRP");
                            }   //  endif recvSum
                            break;

                        case "03":      //  CUFT
                            //  Primary product
                            sumValue = justThisGroup.Sum(jtg => jtg.GrossCUFTPP);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.GrossCUFTPP, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "GrossXPP");
                            sumValue = justThisGroup.Sum(jtg => jtg.NetCUFTPP);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.NetCUFTPP, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "NetXPP");
                            //  secondary product
                            sumValue = justThisGroup.Sum(jtg => jtg.GrossCUFTSP);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.GrossCUFTSP, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "GrossXSP");
                            sumValue = justThisGroup.Sum(jtg => jtg.NetCUFTSP);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.NetCUFTSP, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "NetXSP");
                            //  recovered product
                            if (recvSum > 0)
                            {
                                sumValue = justThisGroup.Sum(jtg => jtg.GrossCUFTRP);
                                sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.GrossCUFTRP, 2));
                                UpdateStage2(pdo, sumValue, sqrdValue, "GrossXRP");
                                UpdateStage2(pdo, sumValue, sqrdValue, "NetXRP");
                            }   //  endif recvSum
                            break;

                        case "05":      //  Weight
                            //  Primary product
                            sumValue = justThisGroup.Sum(jtg => jtg.BiomassMainStemPrimary);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.BiomassMainStemPrimary, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "GrossXPP");
                            UpdateStage2(pdo, sumValue, sqrdValue, "NetXPP");
                            //  Secondary product
                            sumValue = justThisGroup.Sum(jtg => jtg.BiomassMainStemSecondary);
                            sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.BiomassMainStemSecondary, 2));
                            UpdateStage2(pdo, sumValue, sqrdValue, "GrossXSP");
                            UpdateStage2(pdo, sumValue, sqrdValue, "NetXSP");
                            //  no recovered product for weight
                            break;
                    }   //  end switch on unit of measure

                    //  Value primary secondary and recovered products
                    sumValue = justThisGroup.Sum(jtg => jtg.ValuePP);
                    sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.ValuePP, 2));
                    UpdateStage2(pdo, sumValue, sqrdValue, "ValXPP");
                    sumValue = justThisGroup.Sum(jtg => jtg.ValueSP);
                    sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.ValueSP, 2));
                    UpdateStage2(pdo, sumValue, sqrdValue, "ValXSP");
                    if (recvSum > 0)
                    {
                        sumValue = justThisGroup.Sum(jtg => jtg.ValueRP);
                        sqrdValue = justThisGroup.Sum(jtg => Math.Pow(jtg.ValueRP, 2));
                        UpdateStage2(pdo, sumValue, sqrdValue, "ValXRP");
                    }   //  endif recvSum
                    break;

                case "PCM":
                case "PCMTRE":
                case "S3P":
                case "F3P":
                case "P3P":
                case "3PPNT":
                    //  these methods use the tpopList to sum values
                    //  Primary product
                    sumValue = tpopList.Sum(tpo => tpo.tpGrossPP);
                    sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpGrossPP, 2));
                    UpdateStage2(pdo, sumValue, sqrdValue, "GrossXPP");
                    sumValue = tpopList.Sum(tpo => tpo.tpNetPP);
                    sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpNetPP, 2));
                    UpdateStage2(pdo, sumValue, sqrdValue, "NetXPP");
                    //  secondary product
                    sumValue = tpopList.Sum(tpo => tpo.tpGrossSP);
                    sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpGrossSP, 2));
                    UpdateStage2(pdo, sumValue, sqrdValue, "GrossXSP");
                    sumValue = tpopList.Sum(tpo => tpo.tpNetSP);
                    sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpNetSP, 2));
                    UpdateStage2(pdo, sumValue, sqrdValue, "NetXSP");
                    //  recovered product
                    if (recvSum > 0)
                    {
                        sumValue = tpopList.Sum(tpo => tpo.tpGrossRP);
                        sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpGrossRP, 2));
                        UpdateStage2(pdo, sumValue, sqrdValue, "GrossXRP");
                        sumValue = tpopList.Sum(tpo => tpo.tpNetRP);
                        sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpNetRP, 2));
                        UpdateStage2(pdo, sumValue, sqrdValue, "NetXRP");
                    }   //  endif recvSum
                    //  value primary, secondary and recoverd
                    sumValue = tpopList.Sum(tpo => tpo.tpValPP);
                    sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpValPP, 2));
                    UpdateStage2(pdo, sumValue, sqrdValue, "ValXPP");
                    sumValue = tpopList.Sum(tpo => tpo.tpValSP);
                    sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpValSP, 2));
                    UpdateStage2(pdo, sumValue, sqrdValue, "ValXSP");
                    if (recvSum > 0)
                    {
                        sumValue = tpopList.Sum(tpo => tpo.tpValRP);
                        sqrdValue = tpopList.Sum(tpo => Math.Pow(tpo.tpValRP, 2));
                        UpdateStage2(pdo, sumValue, sqrdValue, "ValXRP");
                    }   //  endif recvSum
                    break;
            }   //  end switch
            return;
        }   //  end AccumulateStage2

        private void UpdateStage2(POPDO pdo, double sValue, double sqValue, string fieldToUpdate)
        {
            switch (fieldToUpdate)
            {
                case "All":     //  means the same values are stored in all fields
                    pdo.Stg2GrossXPP = sValue;
                    pdo.Stg2GrossXsqrdPP = sqValue;
                    pdo.Stg2NetXPP = sValue;
                    pdo.Stg2NetXsqrdPP = sqValue;
                    pdo.Stg2ValueXPP = sValue;
                    pdo.Stg2ValueXsqrdPP = sqValue;
                    pdo.Stg2GrossXSP = sValue;
                    pdo.Stg2GrossXsqrdSP = sqValue;
                    pdo.Stg2NetXSP = sValue;
                    pdo.Stg2NetXsqrdSP = sqValue;
                    pdo.Stg2ValueXSP = sValue;
                    pdo.Stg2ValueXsqrdSP = sqValue;
                    pdo.Stg2GrossXRP = sValue;
                    pdo.Stg2GrossXsqrdRP = sqValue;
                    pdo.Stg2NetXRP = sValue;
                    pdo.Stg2NetXsqrdRP = sqValue;
                    pdo.Stg2ValueXRP = sValue;
                    pdo.Stg2ValueXsqrdRP = sqValue;
                    return;

                case "GrossXPP":
                    pdo.Stg2GrossXPP = sValue;
                    pdo.Stg2GrossXsqrdPP = sqValue;
                    break;

                case "NetXPP":
                    pdo.Stg2NetXPP = sValue;
                    pdo.Stg2NetXsqrdPP = sqValue;
                    break;

                case "ValXPP":
                    pdo.Stg2ValueXPP = sValue;
                    pdo.Stg2ValueXsqrdPP = sqValue;
                    break;

                case "GrossXSP":
                    pdo.Stg2GrossXSP = sValue;
                    pdo.Stg2GrossXsqrdSP = sqValue;
                    break;

                case "NetXSP":
                    pdo.Stg2NetXSP = sValue;
                    pdo.Stg2NetXsqrdSP = sqValue;
                    break;

                case "ValXSP":
                    pdo.Stg2ValueXSP = sValue;
                    pdo.Stg2ValueXsqrdSP = sqValue;
                    break;

                case "GrossXRP":
                    pdo.Stg2GrossXRP = sValue;
                    pdo.Stg2GrossXsqrdRP = sqValue;
                    break;

                case "NetXRP":
                    pdo.Stg2NetXRP = sValue;
                    pdo.Stg2NetXsqrdRP = sqValue;
                    break;

                case "ValXRP":
                    pdo.Stg2ValueXRP = sValue;
                    pdo.Stg2ValueXsqrdRP = sqValue;
                    break;
            }   //  end switch

            return;
        }   //  end UpdateStage2

        private void CalculateProration(PRODO prdo, List<LCDDO> currentLCD, List<POPDO> currentPOP,
                                                double unitAcres, string currMethod)
        {
            double proFac = 0;
            double proEst = 0;

            //  Find row for current PRO record in the current LCD and POP
            //  Will need to add STM flag when it's added back to structure -- done
            int LCDrow = currentLCD.FindIndex(
                delegate (LCDDO ldo)
                {
                    return ldo.CutLeave == prdo.CutLeave && ldo.Stratum == prdo.Stratum &&
                           ldo.SampleGroup == prdo.SampleGroup && ldo.PrimaryProduct == prdo.PrimaryProduct &&
                           ldo.SecondaryProduct == prdo.SecondaryProduct && ldo.UOM == prdo.UOM && ldo.STM == prdo.STM;
                });
            int POProw = currentPOP.FindIndex(
                delegate (POPDO pdo)
                {
                    return pdo.CutLeave == prdo.CutLeave && pdo.Stratum == prdo.Stratum &&
                           pdo.SampleGroup == prdo.SampleGroup && pdo.PrimaryProduct == prdo.PrimaryProduct &&
                           pdo.SecondaryProduct == prdo.SecondaryProduct && pdo.UOM == prdo.UOM && pdo.STM == prdo.STM;
                });

            //  Calculate proration factor and proratio estimate based on method
            switch (currMethod)
            {
                case "3P":
                    if (POProw >= 0)
                    {
                        if (currentPOP[POProw].SumKPI > 0)
                            proFac = prdo.SumKPI / currentPOP[POProw].SumKPI;
                    }   //  endif on POProw
                    if (LCDrow >= 0)
                        proEst = currentLCD[LCDrow].SumExpanFactor;
                    break;

                case "100":
                    proFac = 1.0;
                    proEst = 1.0;
                    break;

                case "S3P":
                case "STR":
                    if (POProw >= 0)
                    {
                        if (currentPOP[POProw].TalliedTrees > 0)
                            proFac = prdo.TalliedTrees / currentPOP[POProw].TalliedTrees;
                    }   //  endif on POProw
                    if (LCDrow >= 0)
                        proEst = currentLCD[LCDrow].SumExpanFactor;
                    break;

                default:
                    proFac = unitAcres;
                    proEst = 0;
                    break;
            }   //  end switch on method

            //  eventually this will be needed
            //  DONE -- April 2013
            if (prdo.STM == "Y") proFac = 1.0;

            prdo.ProrationFactor = proFac;
            prdo.ProratedEstimatedTrees = proEst;
            return;
        }   //  end CalculateProration
    }   //  end class SumAll
}