using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using System.Runtime.InteropServices;
using CruiseProcessing.Data;
using System.Collections;
using Microsoft.Extensions.Logging;

namespace CruiseProcessing
{
    public interface ICalculateTreeValues
    {
        void ProcessTrees(string currST, string currMethod, long currST_CN);
    }

    public partial class CalculateTreeValues2 : ICalculateTreeValues
    {
        #region

        private string Region { get; }
        private string Forest { get; }
        private string District { get; }
        private string CruiseNumber { get; }

        protected CpDataLayer DataLayer { get; }
        public ILogger Log { get; }
        public List<VolumeEquationDO> VolumeEquations { get; }

        public CalculateTreeValues2(CpDataLayer dataLayer, ILogger<CalculateTreeValues2> log)
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            Log = log;

            //  Retrieve region, forest and district
            Region = dataLayer.getRegion();
            Forest = dataLayer.getForest();
            District = dataLayer.getDistrict();
            CruiseNumber = dataLayer.getCruiseNumber();

            VolumeEquations = dataLayer.getVolumeEquations();
        }



        #endregion

        public void ProcessTrees(string currST, string currMethod, long currST_CN)
        {
            //  Calculate volumes by stratum

            CalculateVolumes(currMethod, currST_CN);

            //  Check for value equations
            List<ValueEquationDO> valList = DataLayer.getValueEquations();
            if (valList.Count > 0)
            {
                List<TreeDO> strataTrees = DataLayer.JustMeasuredTrees(currST_CN);
                List<TreeCalculatedValuesDO> tcList = DataLayer.getTreeCalculatedValues();
                CalculateValue(strataTrees, currST, currMethod, Region, valList, tcList);
            }   //  endif valList
        }   //  end ProcessTrees


        public void CalculateVolumes(string currMethod,
                                            long stratum_CN)
        {
            //  need to check for method -- FIXCNT only does biomass when requested.  Does not calculate volume. June 2016
            if (currMethod == "FIXCNT")
            {
                List<TreeDO> treeList = DataLayer.JustFIXCNTtrees(stratum_CN);

                var tcvList = CalculateVolumesFixCNT(treeList);
                //  Save calculated values
                DataLayer.SaveTreeCalculatedValues(tcvList);
            }
            else
            {
                List<TreeDO> strataTrees = DataLayer.JustMeasuredTrees(stratum_CN);
                var tcvList = CalculateVolumes(strataTrees);

                //  Save calculated values
                DataLayer.SaveTreeCalculatedValues(tcvList);
            }   //  endif on method

        }   //  end CalculateVolumes

        public List<TreeCalculatedValuesDO> CalculateVolumes(IEnumerable<TreeDO> strataTrees)
        {
            List<TreeCalculatedValuesDO> tcList = new List<TreeCalculatedValuesDO>();


            //  Will recovered volume be calculated?  Sum recoverable defect primary product to set flag
            var hasRecoverablePrimary = strataTrees.Any(t => t.RecoverablePrimary > 0);


            // EditChecks will catch values that shouldn't parse
            int REGN = int.Parse(Region);
            int IDIST = int.Parse(District);
            StringBuilder FORST = new StringBuilder(STRING_BUFFER_SIZE).Append(Forest);

            StringBuilder CTYPE = new StringBuilder(256);

            // Varable LogLength hasn't been used since V1 so this behavior might be stale
            string vllType = DataLayer.getVLL();
            if (vllType == "false")
                CTYPE.Append("C");
            else CTYPE.Append(vllType);


            //  loop through individual trees and calculate volume for all equations requested by species/product
            foreach (TreeDO td in strataTrees)
            {
                //  Is this a FBS tree?  process separately and skip volume calculation
                if (td.IsFallBuckScale == 1)
                {
                    var treeCN = td.Tree_CN.Value;
                    var tcv = CalculateVoume_FBS(treeCN);
                    tcList.Add(tcv);
                }
                else
                {
                    if ((td.DBH == 0 && td.DRC == 0)
                        || (td.TotalHeight == 0 && td.MerchHeightPrimary == 0 && td.MerchHeightSecondary == 0 && td.UpperStemHeight == 0))
                    { continue; } // if tree missing diameters or heights skip

                    var tcv = CalculateTreeVolume(td, hasRecoverablePrimary, REGN, IDIST, FORST, CTYPE);
                    if (tcv != null) { tcList.Add(tcv); }
                }   //  endif
            }   //  end foreach loop

            return tcList;
        }

        private TreeCalculatedValuesDO CalculateTreeVolume(TreeDO tree, bool hasRecoverablePrimary, int REGN, int IDIST, StringBuilder FORST, StringBuilder CTYPE)
        {
            if(tree.TreeDefaultValue_CN is null || tree.TreeDefaultValue_CN.Value == 0)
            { throw new InvalidOperationException("Tree Missing Tree Default Value"); }
            if(tree.SampleGroup_CN is null || tree.SampleGroup_CN == 0)
            { throw new InvalidOperationException("Tree Missing Sample Group"); };

            // TODO make calculate Nev volume methods static so we don't need to instantiate a calculator class
            CalculateNetVolume netVolumeCalculator = new CalculateNetVolume();

            TreeCalculatedValuesDO tcv = null;

            //  Outputs
            int BA = 0;
            int SI = 0;
            int INDEB = 0;
            float NOLOGP = 0; // Number of Logs Primary Product
            float NOLOGS = 0; // Number of Logs Secondary Product
            int TLOGS = 0; // Total Logs (NoLogP + NoLogS)
            float[] VOL = new float[I15];
            float[] LOGLEN = new float[I20];
            float[] BOLHT = new float[I21];
            float[,] LOGVOL = new float[I20, I7];
            float[,] LOGDIA = new float[I3, I21];

            // Inputs
            float DBHOB = tree.DBH;
            float DRCOB = tree.DRC;

            StringBuilder CONSPEC = new StringBuilder(STRING_BUFFER_SIZE).Append(tree.TreeDefaultValue.ContractSpecies);
            StringBuilder PROD = new StringBuilder(STRING_BUFFER_SIZE).Append(tree.SampleGroup.PrimaryProduct);
            StringBuilder LIVE = new StringBuilder(STRING_BUFFER_SIZE).Append(tree.LiveDead);
            StringBuilder HTTYPE = new StringBuilder(STRING_BUFFER_SIZE).Append(tree.TreeDefaultValue.MerchHeightType);

            float HTTOT = tree.TotalHeight;
            int HTLOG = (int)tree.TreeDefaultValue.MerchHeightLogLength;
            float HT1PRD = tree.MerchHeightPrimary;
            float HT2PRD = tree.MerchHeightSecondary;
            float UPSHT1 = tree.UpperStemHeight;
            float UPSHT2 = 0.0f;
            int HTTFLL = (int)tree.HeightToFirstLiveLimb;

            float UPSD1 = tree.UpperStemDiameter;
            //float UPSD1 = td.UpperStemDOB;
            float UPSD2 = 0.0f;

            float AVGZ1 = 0.0f;
            float AVGZ2 = 0.0f;
            int HTREF = 0;

            int FCLASS = (int)tree.FormClass;
            if (FCLASS == 0)
            {
                FCLASS = (int)tree.TreeDefaultValue.FormClass;
                AVGZ1 = tree.TreeDefaultValue.AverageZ;
                HTREF = (int)tree.TreeDefaultValue.ReferenceHeightPercent;
            }

            float DBTBH = tree.DBHDoubleBarkThickness;
            float BTR = tree.TreeDefaultValue.BarkThicknessRatio;

            List<LogDO> treeLogs = DataLayer.getTreeLogs(tree.Tree_CN.Value);
            var numLogs = treeLogs.Count;

            // CType of V is for Varable Log Length. This hasn't been used since V1
            // and at the time isn't settable by the user
            if (CTYPE.ToString() == "V" && numLogs > 0)
            {
                //  load LOGLEN with values or zeros
                for (int n = 0; n < numLogs; n++)
                    LOGLEN[n] = (float)treeLogs[n].Length;

                for (int n = numLogs; n < I20; n++)
                    LOGLEN[n] = 0;

                TLOGS = numLogs;
            }

            // log stock list gets regenerated for each volEq,
            // but we only need one set of log stocks.
            // create a variable to hold the list to save at the end of the tree loop
            List<LogStockDO> logStockList = null;

            //  Get volume equations
            var currEquations = VolumeEquations.Where(ved => ved.Species == tree.Species && ved.PrimaryProduct == tree.SampleGroup.PrimaryProduct);
            foreach (VolumeEquationDO ved in currEquations)
            {
                tcv = new TreeCalculatedValuesDO { Tree_CN = tree.Tree_CN };

                StringBuilder VOLEQ = new StringBuilder(STRING_BUFFER_SIZE).Append(ved.VolumeEquationNumber);

                //  Get top DIBs based on comparison of DIB on volume equation versus DIB on tree.  
                //  If tree DIB is zero, use volume equation DIB.  Else use tree DIB
                float MTOPP = (tree.TopDIBPrimary <= 0) ? ved.TopDIBPrimary : tree.TopDIBPrimary;
                float MTOPS = (tree.TopDIBSecondary <= 0) ? ved.TopDIBSecondary : tree.TopDIBSecondary;

                float STUMP = ved.StumpHeight;

                //  volume flags
                int CUTFLG = (int)ved.CalcTotal;
                int BFPFLG = (int)ved.CalcBoard;
                int CUPFLG = (int)ved.CalcCubic;
                int CDPFLG = (int)ved.CalcCord;
                int SPFLG = (int)ved.CalcTopwood;

                var merchModeFlag = ved.MerchModFlag;
                int PMTFLG = (merchModeFlag == 2) ? (int)ved.MerchModFlag
                    : 0;

                //  if merch rules have changed, pull those into mRules
                MRules mRules = (merchModeFlag == 2) ? new MRules(ved)
                    : new MRules(evod: 2, op: 11);

                int fiaspcd = int.Parse(ved.VolumeEquationNumber.Substring(7, 3));

                int ERRFLAG = 0;

                //  volume library call
                //VOLLIBCS(ref REGN, FORST, VOLEQ, ref MTOPP, ref MTOPS, ref STUMP,
                //         ref DBHOB, ref DRCOB, HTTYPE, ref HTTOT, ref HTLOG,
                //         ref HT1PRD, ref HT2PRD, ref UPSHT1, ref UPSHT2, ref UPSD1, ref UPSD2,
                //         ref HTREF, ref AVGZ1, ref AVGZ2, ref FCLASS, ref DBTBH, ref BTR,
                //         ref I3, ref I7, ref I15, ref I20, ref I21, VOL, LOGVOL, LOGDIA, LOGLEN, BOLHT,
                //         ref TLOGS, ref NOLOGP, ref NOLOGS, ref CUTFLG, ref BFPFLG,
                //         ref CUPFLG, ref CDPFLG, ref SPFLG, CONSPEC, PROD,
                //         ref HTTFLL, LIVE, ref BA, ref SI, CTYPE, ref ERRFLAG,
                //         ref INDEB, ref PMTFLG, ref mRules, ref IDIST, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE,
                //         STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, CHARLEN);

                //VOLLIBCS(ref REGN, FORST, VOLEQ, ref MTOPP, ref MTOPS, ref STUMP,
                //         ref DBHOB, ref DRCOB, HTTYPE, ref HTTOT, ref HTLOG,
                //         ref HT1PRD, ref HT2PRD, ref UPSHT1, ref UPSHT2, ref UPSD1, ref UPSD2,
                //         ref HTREF, ref AVGZ1, ref AVGZ2, ref FCLASS, ref DBTBH, ref BTR,
                //         ref I3, ref I7, ref I15, ref I20, ref I21, VOL, LOGVOL, LOGDIA, LOGLEN, BOLHT,
                //         ref TLOGS, ref NOLOGP, ref NOLOGS, ref CUTFLG, ref BFPFLG,
                //         ref CUPFLG, ref CDPFLG, ref SPFLG, CONSPEC, PROD,
                //         ref HTTFLL, LIVE, ref BA, ref SI, CTYPE, ref ERRFLAG,
                //         ref INDEB, ref PMTFLG, ref mRules, ref IDIST, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE,
                //         STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, CHARLEN);


                // unused VOLLIBCSNVB variables
                float brkht = 0.0f;
                float brkhtd = 0.0f;
                float cr = 0.0f;
                float cull = 0.0f;
                int decaycd = 0;
                float[] drybio = new float[DRYBIO_ARRAY_SIZE];
                float[] grnbio = new float[GRNBIO_ARRAY_SIZE];
                

                VOLLIBCSNVB(ref REGN, FORST, VOLEQ, ref MTOPP, ref MTOPS, 
                    ref STUMP, ref DBHOB, ref DRCOB, HTTYPE, ref HTTOT, 
                    ref HTLOG, ref HT1PRD, ref HT2PRD, ref UPSHT1, ref UPSHT2, 
                    ref UPSD1, ref UPSD2, ref HTREF, ref AVGZ1, ref AVGZ2, 
                    ref FCLASS, ref DBTBH, ref BTR, VOL, LOGVOL, 
                    LOGDIA, LOGLEN, BOLHT, ref TLOGS, ref NOLOGP, 
                    ref NOLOGS, ref CUTFLG, ref BFPFLG, ref CUPFLG, ref CDPFLG, 
                    ref SPFLG, CONSPEC, PROD, ref HTTFLL, LIVE, 
                    ref BA, ref SI, CTYPE, ref ERRFLAG, ref PMTFLG, 
                    ref mRules, ref IDIST,
                    ref brkht, ref brkhtd, ref fiaspcd, drybio, grnbio,
                    ref cr, ref cull, ref decaycd, 
                    STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE,
                         STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE, CHARLEN);


                if (ERRFLAG > 0)
                    DataLayer.LogError("Tree", (int)tree.Tree_CN, "W", ERRFLAG.ToString());

                //  Update log stock table with calculated values
                logStockList = GetLogStockList(treeLogs, (int)tree.Tree_CN, LOGVOL, LOGDIA, LOGLEN, TLOGS).ToList();

                //  Next, calculate net volumes
                netVolumeCalculator.calcNetVol(CTYPE.ToString(), VOL, LOGVOL, logStockList, tree, Region,
                                              NOLOGP, NOLOGS, TLOGS, MTOPP, tree.SampleGroup.PrimaryProduct);

                //  Update number of logs
                if (Region != "10" && Region != "06" && Region != "6")
                {
                    TLOGS = (int)NOLOGP;
                    if (NOLOGP - TLOGS > 0) TLOGS++;
                }   //  endif

                //  Update log defects -- except for BLM
                if (Region != "07")
                {
                    for (int n = 0; n < TLOGS; n++)
                    {
                        if (LOGVOL[n, 3] > 0)
                            logStockList[n].SeenDefect = (float)Math.Round(((LOGVOL[n, 3] - LOGVOL[n, 5]) / LOGVOL[n, 3] * 100));
                    }   //  end for n loop
                }   //  endif


                //  Need to see if biomass calculation is needed here since it needs VOL[13] and VOL[14] for the call
                float[] treeBiomassCalulations = CalculateBiomass(tree, VOL, Region, Forest);


                //  Store volumes in tree calculated values
                //  because of the change made to how these calculations work, the TreeCalculatedValues table is emptied
                //  prior to processing and rebuilt.
                //  Discovered when multiple volume equations are found for an individual tree, the save into the
                //  database fails.  The program needs to check for duplciate tree_CN in the list and update
                //  the record instead of adding another.  Otherwise, the tree_CN causes a constraint violations
                //  when it trys to save through the DAL.
                //  March 2014
                float hiddenDefect = (tree.HiddenPrimary > 0) ? tree.HiddenPrimary : Math.Max(tree.TreeDefaultValue.HiddenPrimary, 0);

                SetTreeCalculatedValues(tcv, VOL, LOGVOL, CUTFLG, BFPFLG, CUPFLG, CDPFLG,
                                 SPFLG, hasRecoverablePrimary, tree.TreeDefaultValue.CullPrimary, hiddenDefect, tree.SeenDefectPrimary, tree.RecoverablePrimary,
                                 treeBiomassCalulations, TLOGS, NOLOGP, NOLOGS, Region, DataLayer);

                //  update volume calcs in logstock if log volume calculated
                for (int k = 0; k < TLOGS; k++)
                {
                    var logNumber = (k + 1).ToString();
                    var ls = logStockList.FirstOrDefault(lsd => lsd.Tree_CN == tree.Tree_CN && lsd.LogNumber == logNumber);
                    if (ls != null)
                    {
                        if (BFPFLG == 1)
                        {
                            ls.GrossBoardFoot = LOGVOL[k, 0];
                            ls.BoardFootRemoved = LOGVOL[k, 1];
                            ls.NetBoardFoot = LOGVOL[k, 2];
                        }   //  endif board foot
                        if (CUPFLG == 1)
                        {
                            ls.GrossCubicFoot = LOGVOL[k, 3];
                            ls.CubicFootRemoved = LOGVOL[k, 4];
                            ls.NetCubicFoot = LOGVOL[k, 5];
                        }   //  endif cubic foot
                    }
                }   //  end for k loop

            }   //  end for each loop of volume equations

            //  and make sure modified log stock list is saved
            if (TLOGS != 0 && logStockList.Count != 0)
            { DataLayer.SaveLogStock(logStockList, TLOGS); }

            return tcv;
        }

        private TreeCalculatedValuesDO CalculateVoume_FBS(long treeCN)
        {
            var tcv = new TreeCalculatedValuesDO { Tree_CN = treeCN };

            List<LogDO> justTreeLogs = DataLayer.getTreeLogs(treeCN);

            CalcFallBuckScale(tcv, justTreeLogs);

            var logStockList = MakeLogStockList(treeCN, justTreeLogs).ToArray();
            if (logStockList.Any())
            { DataLayer.SaveLogStock(logStockList); }

            return tcv;
        }

        private List<TreeCalculatedValuesDO> CalculateVolumesFixCNT(List<TreeDO> treeList)
        {
            List<TreeCalculatedValuesDO> tcList = new List<TreeCalculatedValuesDO>();
            float[] VOL = new float[I15];

            //  pull all trees
            foreach (TreeDO td in treeList)
            {
                var calculatedBiomass = CalculateBiomass(td, VOL, Region, Forest);
                //Fnd this tree in calculated values and store appropriately
                int nthRow = tcList.FindIndex(
                    delegate (TreeCalculatedValuesDO t)
                    {
                        return t.Tree_CN == td.Tree_CN;
                    });
                if (nthRow >= 0)
                {
                    tcList[nthRow].BiomassMainStemPrimary = calculatedBiomass[4];
                    tcList[nthRow].BiomassMainStemSecondary = calculatedBiomass[5];
                    tcList[nthRow].Biomasstotalstem = calculatedBiomass[0];
                    tcList[nthRow].Biomasslivebranches = calculatedBiomass[1];
                    tcList[nthRow].Biomassdeadbranches = calculatedBiomass[2];
                    tcList[nthRow].Biomassfoliage = calculatedBiomass[3];
                    tcList[nthRow].BiomassTip = calculatedBiomass[6];
                }
                else if (nthRow < 0)
                {
                    TreeCalculatedValuesDO tcvdo = new TreeCalculatedValuesDO();
                    tcvdo.Tree_CN = td.Tree_CN;
                    tcvdo.BiomassMainStemPrimary = calculatedBiomass[4];
                    tcvdo.BiomassMainStemSecondary = calculatedBiomass[5];
                    tcvdo.Biomasstotalstem = calculatedBiomass[0];
                    tcvdo.Biomasslivebranches = calculatedBiomass[1];
                    tcvdo.Biomassdeadbranches = calculatedBiomass[2];
                    tcvdo.Biomassfoliage = calculatedBiomass[3];
                    tcvdo.BiomassTip = calculatedBiomass[6];
                    tcList.Add(tcvdo);
                }   //  endif on nthRow

            }   //  end foreach loop

            return tcList;
        }


        public float[] CalculateBiomass(TreeDO tree, float[] VOL, string region, string forest)
        {
            var calculatedBiomass = new float[CRZBIOMASSCS_BMS_SIZE];
            var species = tree.Species;
            var product = tree.SampleGroup.PrimaryProduct;
            var liveDead = tree.LiveDead;
            var biomassEqs = DataLayer.GetBiomassEquations(species, product); //  bioEqs.Where(bdo => bdo.Species == species && bdo.Product == product);

            if (!biomassEqs.Any())
            {
                Log.LogWarning("No Biomass Equations Found");
                return calculatedBiomass;
            }

            var currFIA = tree.TreeDefaultValue.FIAcode;
            var currDBH = tree.DBH;
            var currHGT = tree.TotalHeight;
            var currFC = (int)tree.FormClass;
            var currDRC = tree.DRC;


            StringBuilder FORST = new StringBuilder(STRING_BUFFER_SIZE).Append(forest);
            int SPCD = (int)currFIA;
            int iRegn = int.Parse(region);
            int ERRFLAG = 0;
            float[] WF = new float[3];

            // in older versions Cruise Processing ignored the liveDead value
            // to maintian some backwards compatibility we will try to use the
            // liveDead specific biomass equations first, then fall back to the
            // to whichever biomass equation is available

            var pProdEq = biomassEqs.FirstOrDefault(beq => beq.Component == "PrimaryProd" && beq.LiveDead == liveDead)
                ?? biomassEqs.FirstOrDefault(beq => beq.Component == "PrimaryProd");
            if (pProdEq != null)
            {
                WF[0] = pProdEq.WeightFactorPrimary;
                WF[2] = pProdEq.PercentMoisture;
            }

            var sProdEq = biomassEqs.FirstOrDefault(beq => beq.Component == "SecondaryProd" && beq.LiveDead == liveDead)
                ?? biomassEqs.FirstOrDefault(beq => beq.Component == "SecondaryProd");
            if (sProdEq != null)
            {
                WF[1] = sProdEq.WeightFactorSecondary;
            }

            var prod = new StringBuilder(STRING_BUFFER_SIZE).Append(product);

            //  WHY?                            //if(currRegion == "9" || currRegion == "09")
            //    CRZBIOMASSCS(ref REGN,FORST,ref SPCD,ref DBHOB,ref HTTOT,VOL,WF,calculatedBiomass,ref ERRFLAG,strlen);
            //else

            try
            {
                CRZBIOMASSCS(ref iRegn, FORST, ref SPCD, ref currDBH, ref currDRC, ref currHGT, ref currFC, VOL, WF,
                                    calculatedBiomass, ref ERRFLAG, prod, STRING_BUFFER_SIZE, STRING_BUFFER_SIZE);
            }
            catch(System.AccessViolationException e)
            {
                Log.LogCritical(e, "Tree_CN{TreeCN} fiaCode{fiaCode} prod{prod}", tree.Tree_CN, currFIA, product);
                throw;
            }

            // Apply percent removed if greater than zero
            // seems like each BioEq for a given species/prod has the same PercentRemoved value
            var pctRemv = biomassEqs.FirstOrDefault(x => x.PercentRemoved > 0);
            if (pctRemv != null)
            {
                for (int j = 0; j < CRZBIOMASSCS_BMS_SIZE; j++)
                { calculatedBiomass[j] = calculatedBiomass[j] * pctRemv.PercentRemoved / 100.0f; }
            }

            return calculatedBiomass;
        }


        //private void UpdateLogStock(List<LogDO> justTreeLogs, List<LogStockDO> logStockList, int currTreeCN,
        //                                    float[,] LOGVOL, float[,] LOGDIA, float[] LOGLEN, int TLOGS)
        //{
        //    for (int n = 0; n < TLOGS; n++)
        //    {
        //        var logNumber = n + 1;
        //        LogStockDO lsdo = new LogStockDO();

        //        lsdo.Tree_CN = currTreeCN;
        //        lsdo.LogNumber = logNumber.ToString();
        //        lsdo.SmallEndDiameter = LOGDIA[1, n + 1];
        //        lsdo.LargeEndDiameter = LOGDIA[1, n];
        //        lsdo.Length = (long)LOGLEN[n];
        //        lsdo.GrossBoardFoot = LOGVOL[n, 0];
        //        lsdo.NetBoardFoot = LOGVOL[n, 2];
        //        lsdo.BoardFootRemoved = LOGVOL[n, 1];
        //        lsdo.GrossCubicFoot = LOGVOL[n, 3];
        //        lsdo.NetCubicFoot = LOGVOL[n, 5];
        //        lsdo.CubicFootRemoved = LOGVOL[n, 4];
        //        lsdo.SeenDefect = 0;
        //        lsdo.PercentRecoverable = 0;
        //        lsdo.DIBClass = LOGDIA[0, n + 1];
        //        logStockList.Add(lsdo);
        //    }   //  end for n loop

        //    //  Find defect values for each log in original logs or set to zero
        //    foreach (LogStockDO lsdo in logStockList)
        //    {
        //        var log = justTreeLogs.FirstOrDefault(ld => lsdo.Tree_CN == ld.Tree_CN && lsdo.LogNumber == ld.LogNumber);
        //        if (log != null)
        //        {
        //            lsdo.SeenDefect = log.SeenDefect;
        //            lsdo.PercentRecoverable = log.PercentRecoverable;
        //            lsdo.Grade = log.Grade;
        //        }
        //    }   //  end foreach loop

        //    return;
        //}   //  end UpdateLogStock

        protected static IEnumerable<LogStockDO> GetLogStockList(IEnumerable<LogDO> justTreeLogs, int currTreeCN,
                                            float[,] LOGVOL, float[,] LOGDIA, float[] LOGLEN, int TLOGS)
        {
            for (int n = 0; n < TLOGS; n++)
            {
                var logNumber = n + 1;
                LogStockDO lsdo = new LogStockDO();

                lsdo.Tree_CN = currTreeCN;
                lsdo.LogNumber = logNumber.ToString();
                lsdo.SmallEndDiameter = LOGDIA[1, n + 1];
                lsdo.LargeEndDiameter = LOGDIA[1, n];
                lsdo.Length = (long)LOGLEN[n];
                lsdo.GrossBoardFoot = LOGVOL[n, 0];
                lsdo.NetBoardFoot = LOGVOL[n, 2];
                lsdo.BoardFootRemoved = LOGVOL[n, 1];
                lsdo.GrossCubicFoot = LOGVOL[n, 3];
                lsdo.NetCubicFoot = LOGVOL[n, 5];
                lsdo.CubicFootRemoved = LOGVOL[n, 4];
                lsdo.DIBClass = LOGDIA[0, n + 1];

                //  Find defect values for each log in original logs or set to zero
                var log = justTreeLogs.FirstOrDefault(ld => lsdo.Tree_CN == ld.Tree_CN && lsdo.LogNumber == ld.LogNumber);
                if (log != null)
                {
                    lsdo.SeenDefect = log.SeenDefect;
                    lsdo.PercentRecoverable = log.PercentRecoverable;
                    lsdo.Grade = log.Grade;
                }
                else
                {
                    lsdo.SeenDefect = 0;
                    lsdo.PercentRecoverable = 0;
                }

                yield return lsdo;
            }   //  end for n loop
        }


        //private void StoreCalculations(int currTreeCN, float[] VOL, float[,] LOGVOL, int TCUFTflag, int GBDFTflag,
        //                                    int GCUFTflag, int CORDflag, int SecondVolFlag, bool hasRecoverable,
        //                                    float cullDef, float hidDef, float seenDef, float recvDef,
        //                                    float[] biomassCalcs, List<TreeCalculatedValuesDO> tcvList,
        //                                    List<LogStockDO> logStockList, int TLOGS, float NOLOGP, float NOLOGS, IErrorLogDataService errorLogDataService)
        //{
        //    //  Store volumes in tree calculated values
        //    //  because of the change made to how these calculations work, the TreeCalculatedValues table is emptied
        //    //  prior to processing and rebuilt.
        //    //  Discovered when multiple volume equations are found for an individual tree, the save into the
        //    //  database fails.  The program needs to check for duplciate tree_CN in the list and update
        //    //  the record instead of adding another.  Otherwise, the tree_CN causes a constraint violations
        //    //  when it trys to save through the DAL.
        //    //  March 2014
        //    var tcv = tcvList.FirstOrDefault(x => x.Tree_CN == currTreeCN) 
        //        ?? new TreeCalculatedValuesDO { Tree_CN = currTreeCN };

        //    SetTreeCalculatedValues(tcv, VOL,
        //        LOGVOL, TCUFTflag, GBDFTflag, GCUFTflag, CORDflag,
        //                     SecondVolFlag, hasRecoverable, cullDef, hidDef, seenDef, recvDef,
        //                     biomassCalcs, TLOGS, NOLOGP, NOLOGS, logStockList, currRegion, errorLogDataService);

        //    return;
        //}   //  end StoreCalculations


        public static void SetTreeCalculatedValues(TreeCalculatedValuesDO tcv, float[] VOL, float[,] LOGVOL, int TCUFTflag, int GBDFTflag,
                                            int GCUFTflag, int CORDflag, int SecondVolFlag, bool hasRecoverable,
                                            float cullDef, float hidDef, float seenDef, float recvDef,
                                            float[] biomassCalcs, int TLOGS, float numberOfLogsPrimary, float numberOfLogsSecondary, string currRegion, IErrorLogDataService errorLogDataService)
        {
            //  updates tree record in tree calculated values list
            if (TCUFTflag == 1) tcv.TotalCubicVolume = VOL[0];
            if (GBDFTflag == 1)         //  board foot volume
            {
                tcv.GrossBDFTPP = VOL[1];
                tcv.NetBDFTPP = VOL[2];
            }   //  endif GBDFTflag

            if (GCUFTflag == 1)         //  cubic foot volume
            {
                tcv.GrossCUFTPP = VOL[3];
                tcv.NetCUFTPP = VOL[4];
            }   //  endif GCUFTflag

            if (CORDflag == 1) tcv.CordsPP = VOL[5];

            if (SecondVolFlag == 1)          //  secondary product was calculated
            {
                tcv.GrossCUFTSP = VOL[6];
                tcv.NetCUFTSP = VOL[7];
                tcv.CordsSP = VOL[8];
                tcv.GrossBDFTSP = VOL[11];
                tcv.NetBDFTSP = VOL[12];
            }
            else if (SecondVolFlag == 0)
            {
                //  reset secondary buckets to zero
                tcv.GrossCUFTSP = 0;
                tcv.NetCUFTSP = 0;
                tcv.CordsSP = 0;
                tcv.GrossBDFTSP = 0;
                tcv.NetBDFTSP = 0;
            }   //  endif SecondVolFlag

            // save tipwood volume regardless of secondary flag status
            tcv.TipwoodVolume = VOL[14];

            //  save international calcs and number of logs
            tcv.GrossBDFTIntl = VOL[9];
            tcv.NetBDFTIntl = VOL[10];
            tcv.NumberlogsMS = numberOfLogsPrimary;

            //  Make sure secondary was not calculated to get correct number of logs for secondary
            if (VOL[6] != 0 || VOL[7] != 0 || VOL[8] != 0 || VOL[11] != 0 || VOL[12] != 0)
                tcv.NumberlogsTPW = numberOfLogsSecondary;

            //  Sum removed volume into tree removed
            for (int n = 0; n < TLOGS; n++)
            {
                tcv.GrossBDFTRemvPP += LOGVOL[n, 1];
                tcv.GrossCUFTRemvPP += LOGVOL[n, 4];
            }   //  end for n loop

            //  Calculate recovered if needed
            if (hasRecoverable)
            {
                SetTcvRecoveredVolume(tcv, cullDef, hidDef, seenDef, recvDef, currRegion, errorLogDataService);
            }

            //  Store biomass if there is any
            tcv.BiomassMainStemPrimary = biomassCalcs[4];
            tcv.BiomassMainStemSecondary = biomassCalcs[5];
            tcv.Biomasstotalstem = biomassCalcs[0];
            tcv.Biomasslivebranches = biomassCalcs[1];
            tcv.Biomassdeadbranches = biomassCalcs[2];
            tcv.Biomassfoliage = biomassCalcs[3];
            tcv.BiomassTip = biomassCalcs[6];




            static void SetTcvRecoveredVolume(TreeCalculatedValuesDO tcv, float cullDef, float hidDef,
                                            float seenDef, float recvDef, string currRegion, IErrorLogDataService errorLogDataService)
            {
                //  Check if recoverable defect is greater than the sum
                //  of the defects.  Is so, use the sum of the defects in place
                //  of the entered recoverable defect.  Issue a warning for this tree.
                //  This is for every region except 10
                float checkRecv = recvDef;
                if (recvDef > (cullDef + hidDef + seenDef) && currRegion != "10")
                {
                    checkRecv = cullDef + hidDef + seenDef;
                    errorLogDataService.LogError("TREE", (int)tcv.Tree_CN, "W", "18", "Volume");
                }   //  endif

                //  calculate recovered volume based on region
                if (currRegion == "9" || currRegion == "09")
                {
                    //  No board foot recovered calculated for Region 9
                    tcv.GrossBDFTRP = 0;
                    tcv.GrossCUFTRP = tcv.GrossCUFTPP * (checkRecv / 100);
                }
                else if (currRegion == "10")
                {
                    //  they use net instead of gross for the calc
                    tcv.GrossBDFTRP = tcv.NetBDFTPP * (checkRecv / 100);
                    tcv.GrossCUFTRP = tcv.NetCUFTPP * (checkRecv / 100);
                }
                else
                {
                    tcv.GrossBDFTRP = tcv.GrossBDFTPP * (checkRecv / 100);
                    tcv.GrossCUFTRP = tcv.GrossCUFTPP * (checkRecv / 100);
                }   //  endif on region

                if (cullDef > 0 || hidDef > 0 || seenDef > 0)
                    tcv.CordsRP = tcv.CordsPP / (cullDef + hidDef + seenDef) * checkRecv;
                tcv.ValueRP = tcv.ValuePP * tcv.GrossCUFTRP;

            }   //  end RecoveredVolume
        }


        protected void CalcFallBuckScale(TreeCalculatedValuesDO tcv, IEnumerable<LogDO> justTreeLogs)
        {
            tcv.GrossBDFTPP = justTreeLogs.Sum(j => j.GrossBoardFoot);
            tcv.NetBDFTPP = justTreeLogs.Sum(j => j.NetBoardFoot);
            tcv.GrossCUFTPP = justTreeLogs.Sum(j => j.GrossCubicFoot);
            tcv.NetCUFTPP = justTreeLogs.Sum(j => j.NetCubicFoot);
            tcv.GrossBDFTRemvPP = justTreeLogs.Sum(j => j.BoardFootRemoved);
            tcv.GrossCUFTRemvPP = justTreeLogs.Sum(j => j.CubicFootRemoved);
            tcv.NumberlogsMS = justTreeLogs.Count(); //  number of logs mainstem
        }

        protected IEnumerable<LogStockDO> MakeLogStockList(long currTree_CN, List<LogDO> justTreeLogs)
        {
            //  load logs into log stock table
            foreach (LogDO jtl in justTreeLogs)
            {
                LogStockDO ls = new LogStockDO();
                ls.Tree_CN = currTree_CN;
                ls.LogNumber = jtl.LogNumber;
                ls.Grade = jtl.Grade;
                ls.SeenDefect = jtl.SeenDefect;
                ls.PercentRecoverable = jtl.PercentRecoverable;
                ls.Length = jtl.Length;
                ls.ExportGrade = jtl.ExportGrade;
                ls.SmallEndDiameter = jtl.SmallEndDiameter;
                ls.LargeEndDiameter = jtl.LargeEndDiameter;
                //  Check for BLM to override removed based on log grade
                if (Region == "07" || Region == "7")
                {
                    switch (jtl.Grade)
                    {
                        case "9":
                            ls.BoardFootRemoved = 0;
                            ls.CubicFootRemoved = 0;
                            ls.NetBoardFoot = 0;
                            ls.NetCubicFoot = 0;
                            break;
                        case "7":
                        case "8":
                            ls.BoardFootRemoved = jtl.GrossBoardFoot;
                            ls.CubicFootRemoved = jtl.GrossCubicFoot;
                            ls.NetBoardFoot = 0;
                            ls.NetCubicFoot = 0;
                            break;
                        case "":
                        case " ":
                        case null:
                            ls.BoardFootRemoved = jtl.GrossBoardFoot;
                            ls.CubicFootRemoved = jtl.GrossCubicFoot;
                            // need a warning message about  blank log grade
                            DataLayer.LogError("TreeCalculatedValues", (int)currTree_CN, "W", "This tree has a blank log grade or no product assigned.");
                            break;
                        default:
                            ls.BoardFootRemoved = jtl.GrossBoardFoot;
                            ls.CubicFootRemoved = jtl.GrossCubicFoot;
                            break;
                    }   //  end switch on grade

                    ls.GrossBoardFoot = jtl.GrossBoardFoot;
                    ls.NetBoardFoot = jtl.NetBoardFoot;
                    ls.GrossCubicFoot = jtl.GrossCubicFoot;
                    ls.NetCubicFoot = jtl.NetCubicFoot;

                }
                else
                {
                    ls.GrossBoardFoot = jtl.GrossBoardFoot;
                    ls.NetBoardFoot = jtl.NetBoardFoot;
                    ls.GrossCubicFoot = jtl.GrossCubicFoot;
                    ls.NetCubicFoot = jtl.NetCubicFoot;
                    ls.BoardFootRemoved = jtl.BoardFootRemoved;
                    ls.CubicFootRemoved = jtl.CubicFootRemoved;
                }   //  endif on region
                ls.DIBClass = jtl.DIBClass;
                ls.BarkThickness = jtl.BarkThickness;

                yield return ls;
            }   //  end foreach loop
        }


        private void CalculateValue(List<TreeDO> strataTrees, string currST, string currMethod,
                                            string currRegion, List<ValueEquationDO> valList,
                                            List<TreeCalculatedValuesDO> tcvList)
        {
            //  process each tree
            foreach (TreeDO tdo in strataTrees)
            {
                //  find calculated values for current tree
                int mthRow = tcvList.FindIndex(
                    delegate (TreeCalculatedValuesDO tcvdo)
                    {
                        return tdo.Tree_CN == tcvdo.Tree_CN;
                    });

                //  Process according to equation number
                int nthRow = valList.FindIndex(
                    delegate (ValueEquationDO vedo)
                    {
                        return vedo.Species == tdo.Species && vedo.PrimaryProduct == tdo.SampleGroup.PrimaryProduct;
                    });
                float sawValue = 0;
                float topValue = 0;
                if (nthRow >= 0)
                {
                    string equationNumber = valList[nthRow].ValueEquationNumber.Substring(6, 2);
                    if (currRegion == "04")
                    {
                        //  convert clearface to integer for use in calculation
                        int clearFace = int.Parse(tdo.ClearFace);
                        if (clearFace < 0 || clearFace > 9) clearFace = 0;

                        //  Calculate ratio
                        float bdftRatio = 0;
                        if (tcvList[0].GrossBDFTPP > 0)
                            bdftRatio = tcvList[0].NetBDFTPP / tcvList[0].GrossBDFTPP;

                        switch (equationNumber)
                        {
                            case "01":
                            case "02":
                                clearFace++;
                                sawValue = (float)(valList[nthRow].Coefficient1 *
                                    Math.Pow(tdo.DBH, valList[nthRow].Coefficient2) *
                                    Math.Pow(tdo.TotalHeight, valList[nthRow].Coefficient3) *
                                    Math.Pow(clearFace, valList[nthRow].Coefficient4) * bdftRatio);
                                break;
                            case "03":
                            case "04":
                                clearFace++;
                                sawValue = (float)(valList[nthRow].Coefficient1 *
                                    Math.Pow(tdo.DBH, valList[nthRow].Coefficient2) *
                                    Math.Pow(tdo.TotalHeight, valList[nthRow].Coefficient3) * bdftRatio);
                                break;
                        }   //  end switch                    
                    }
                    else
                    {
                        switch (equationNumber)
                        {
                            //  sawtimber check
                            case "01":
                                sawValue = (tcvList[mthRow].GrossBDFTPP * valList[nthRow].Coefficient1) / 1000;
                                break;
                            case "02":
                                sawValue = (tcvList[mthRow].NetBDFTPP * valList[nthRow].Coefficient1) / 1000;
                                break;
                            case "03":
                                sawValue = (tcvList[mthRow].GrossCUFTPP * valList[nthRow].Coefficient1) / 100;
                                break;
                            case "04":
                                sawValue = (tcvList[mthRow].NetCUFTPP * valList[nthRow].Coefficient1) / 100;
                                break;
                            case "05":
                                sawValue = tcvList[mthRow].CordsPP * valList[nthRow].Coefficient1;
                                break;
                            case "06":
                                sawValue = (tcvList[mthRow].GrossBDFTPP * valList[nthRow].Coefficient1) / 1000;
                                topValue = (tcvList[mthRow].GrossCUFTSP * valList[nthRow].Coefficient2) / 100;
                                break;
                            case "07":
                                sawValue = (tcvList[mthRow].NetBDFTPP * valList[nthRow].Coefficient1) / 1000;
                                topValue = (tcvList[mthRow].NetCUFTSP * valList[nthRow].Coefficient2) / 100;
                                break;
                            case "08":
                                sawValue = (tcvList[mthRow].GrossBDFTPP * valList[nthRow].Coefficient1) / 1000;
                                topValue = tcvList[mthRow].CordsSP * valList[nthRow].Coefficient2;
                                break;
                            case "09":
                                sawValue = (tcvList[mthRow].NetBDFTPP * valList[nthRow].Coefficient1) / 1000;
                                topValue = tcvList[mthRow].CordsSP * valList[nthRow].Coefficient2;
                                break;
                            case "10":
                                sawValue = (tcvList[mthRow].GrossCUFTPP * valList[nthRow].Coefficient1) / 100;
                                topValue = (tcvList[mthRow].GrossCUFTSP * valList[nthRow].Coefficient2) / 100;
                                break;
                            case "11":
                                sawValue = (tcvList[mthRow].GrossCUFTPP * valList[nthRow].Coefficient1) / 100;
                                topValue = tcvList[mthRow].CordsSP * valList[nthRow].Coefficient2;
                                break;
                            case "12":
                                sawValue = (tcvList[mthRow].NetCUFTPP * valList[nthRow].Coefficient1) / 100;
                                topValue = (tcvList[mthRow].NetCUFTSP * valList[nthRow].Coefficient2) / 100;
                                break;
                            case "13":
                                sawValue = (tcvList[mthRow].NetCUFTPP * valList[nthRow].Coefficient1) / 100;
                                topValue = tcvList[mthRow].CordsSP * valList[nthRow].Coefficient2;
                                break;
                            case "14":
                                sawValue = tcvList[mthRow].CordsPP * valList[nthRow].Coefficient1;
                                topValue = tcvList[mthRow].CordsSP * valList[nthRow].Coefficient2;
                                break;
                            case "50":
                                sawValue = valList[nthRow].Coefficient1 + (valList[nthRow].Coefficient2 * tdo.DBH) +
                                    (valList[nthRow].Coefficient3 * tdo.TotalHeight) +
                                    (valList[nthRow].Coefficient4 * tdo.DBH * tdo.DBH) +
                                    (valList[nthRow].Coefficient5 * tdo.DBH * tdo.DBH * tdo.TotalHeight) +
                                    (valList[nthRow].Coefficient6 * tdo.DBH * tdo.DBH * tdo.DBH * tdo.TotalHeight);
                                break;
                        }   //  end switch
                    }   //  endif currRegion

                    //  save value
                    tcvList[mthRow].ValuePP = sawValue;
                    tcvList[mthRow].ValueSP = topValue;
                }   //  endif nthRow
            }   //  end foreach loop

            //bslyr.fileName = fileName;
            DataLayer.SaveTreeCalculatedValues(tcvList);
            return;
        }   //  end CalculateValue

        //  this is used for the volume library call
        public struct MRules
        {
            public int evod;
            public int opt;

            public float maxlen;
            public float minlen;
            public float minlent;
            public float merchl;
            public float mtopp;
            public float mtops;
            public float stump;
            public float trim;
            public float btr;
            public float dbhtbh;
            public float minbfd;

            public char cor;

            public MRules(int evod, int op)
            {
                this.evod = evod;
                opt = op;
                maxlen = 0;
                minlen = 0;
                merchl = 0;
                mtopp = 0;
                mtops = 0;
                stump = 0;
                trim = 0;

                cor = 'Y';
                btr = default;
                dbhtbh = default;
                minbfd = default;
                minlent = default;
            }

            public MRules(VolumeEquationDO ved)
            {
                evod = (int)ved.EvenOddSegment;
                opt = (int)ved.SegmentationLogic;
                maxlen = ved.MaxLogLengthPrimary;
                minlen = ved.MinLogLengthPrimary;
                merchl = ved.MinMerchLength;
                mtopp = ved.TopDIBPrimary;
                mtops = ved.TopDIBSecondary;
                stump = ved.StumpHeight;
                trim = ved.Trim;

                cor = 'Y';
                btr = default;
                dbhtbh = default;
                minbfd = default;
                minlent = default;
            }

            public MRules(int ev, float mxln, float mnln, float merln, float mtpp, float mtps, int op, float stmp, float trm)
            {
                evod = ev;
                maxlen = mxln;
                minlen = mnln;
                merchl = merln;
                mtopp = mtpp;
                mtops = mtps;
                opt = op;
                stump = stmp;
                trim = trm;

                cor = 'Y';
                minlent = 0;
                btr = 0;
                dbhtbh = 0;
                minbfd = 0;
            }   //  end MRules

            //  MERCHANDIZING VARIABLES
            //***************************
            //  REGION - INTEGER - Region number used to set Regional Merchandizing Rules
            //  COR - CHARACTER - Flag to indicate Scribner table or Scribner factor volumes.
            //                  "Y" = table volumes, "N" = factor volumes
            //  EVOD - INTEGER - allow even or oadd segment lengths
            //                  segment options 11-14 allow odd lengths by definition
            //                  1 = odd segment lengths allowed
            //                  2 = only even segment lengths will be allowed
            //  MAXLEN - REAL - Maximum segment length
            //  MINLEN - REAL - Minimum segment length
            //  MERCHL - REAL - Minimum length of primary product a tree must have
            //                  must be merchantable
            //  ** TOP DIB TO USE**
            //  MTOPP - REAL - BDFT, CUFT and Cord Wood merch top for primary product
            //  MTOPS - REAL - CUFT and Cord Wood merch top for secondary product

            //  OPT - INTEGER - Specifies whcih segmentation option to use for
            //          merchandizing tree bole.  Option codes are as follows:
            //          11 = 16 ft log scale, presented as tree length log.  (FSH 2409.11)
            //          12 = 20 ft log scale, presented as tree length log.  (FSH 2409.11)
            //          13 = 32 ft log scale, presented as tree length log.
            //          14 = 40 ft log scale, presented as tree length log.
            //          21 = Nominal log length (NLL).  If top log is less than half
            //              of the NLL then it is combined with the next lowest log and
            //              this combined piece is then resegmented according to the
            //              entered merchandizing parameters giving two approximately
            //              equal log lengths.  If the segment length is greater than
            //              or equal to half the NLL then the segment stands on its' own.
            //          22 = Nominal log length (NLL).  top log is combined with the next
            //              lowest log and this combined piece is then resegmented
            //              according to the entered merchandizing parameters giving
            //              two approximately equal log lengths.
            //          23 = Nominal log length.  top segment stands on its' own.
            //          24 = Nominal log length.  if the top segment is less than 1/4 of
            //              NLL then the segment is dropped.  If the segment is 1/4 to
            //              3/4 of NLL then the segment length is set to 1/2 of NLL.
            //              if the segment is greater than 3/4 of NLL then the segment
            //              length is set to NLL.

            //  STUMP - REAL - height of stump in feet or fractions thereof
            //  TRIM - REAL - trim length for each segment in feet or fractions thereof.
        }   //  end MRules


    }
}
