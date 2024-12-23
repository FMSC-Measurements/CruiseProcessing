﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using System.Runtime.InteropServices;
using CruiseProcessing.Data;
using System.Collections;
using Microsoft.Extensions.Logging;
using CruiseProcessing.Interop;
using CruiseProcessing.Services.Logging;

namespace CruiseProcessing.Processing
{
    public partial class CalculateTreeValues2 : ICalculateTreeValues
    {

        private string Region { get; }
        private string Forest { get; }
        private string District { get; }
        private string CruiseNumber { get; }

        protected CalculateNetVolume NetVolumeCalculator { get; }
        protected CpDataLayer DataLayer { get; }
        public IVolumeLibrary VolLib { get; }
        public ILogger Log { get; }
        public List<VolumeEquationDO> VolumeEquations { get; }

        public CalculateTreeValues2(CpDataLayer dataLayer)
            : this(dataLayer, new VolumeLibrary_20241118(), LoggerProvider.CreateLogger<CalculateTreeValues2>())
        { }

        public CalculateTreeValues2(CpDataLayer dataLayer, IVolumeLibrary volumeLibrary, ILogger<CalculateTreeValues2> log)
        {
            NetVolumeCalculator = new CalculateNetVolume();
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            VolLib = volumeLibrary ?? throw new ArgumentNullException(nameof(volumeLibrary));
            Log = log;

            //  Retrieve region, forest and district
            Region = dataLayer.getRegion();
            Forest = dataLayer.getForest();
            District = dataLayer.getDistrict();
            CruiseNumber = dataLayer.getCruiseNumber();

            VolumeEquations = dataLayer.getVolumeEquations();
        }


        public string GetVersion()
        {
            return VolLib.GetVersionNumberString() + ":CRZBIOMASS";
        }

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

            string CTYPE;

            // Variable LogLength hasn't been used since V1 so this behavior might be stale
            string vllType = DataLayer.getVLL();
            if (vllType == "false")
            { CTYPE = "C"; }
            else
            { CTYPE = vllType; }

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
                    if (td.DBH == 0 && td.DRC == 0
                        || td.TotalHeight == 0 && td.MerchHeightPrimary == 0 && td.MerchHeightSecondary == 0 && td.UpperStemHeight == 0)
                    { continue; } // if tree missing diameters or heights skip

                    var tcv = CalculateTreeVolume(td, hasRecoverablePrimary, REGN, IDIST, CTYPE);
                    if (tcv != null) { tcList.Add(tcv); }
                }   //  endif
            }   //  end foreach loop

            return tcList;
        }

        private TreeCalculatedValuesDO CalculateTreeVolume(TreeDO tree, bool hasRecoverablePrimary, int REGN, int IDIST, string CTYPE)
        {
            if (tree.TreeDefaultValue_CN is null || tree.TreeDefaultValue_CN.Value == 0)
            { throw new InvalidOperationException("Tree Missing Tree Default Value"); }
            if (tree.SampleGroup_CN is null || tree.SampleGroup_CN == 0)
            { throw new InvalidOperationException("Tree Missing Sample Group"); };

            // log stock list gets regenerated for each volEq,
            // but we only need one set of log stocks.
            // create a variable to hold the list to save at the end of the tree loop
            List<LogStockDO> logStockList = new List<LogStockDO>();

            // Regions 8 and 9 used to use different volume equations for calculating board ft with Lasher
            // and cubic ft volumes with Clark and could have multiple volume equations for a single tree.
            // Now days they only use one equation.But in the case of multiple equations, the calculated volumes will
            // be saved on the same tree calculated values record.
            TreeCalculatedValuesDO tcv = null;
            int TLOGS = 0; // Total Logs (NoLogP + NoLogS)

            //  Get volume equations
            var currEquations = VolumeEquations.Where(ved => ved.Species == tree.Species && ved.PrimaryProduct == tree.SampleGroup.PrimaryProduct);
            foreach (VolumeEquationDO ved in currEquations)
            {
                tcv = CalculateTreeVolume(ved, tcv, logStockList, tree, hasRecoverablePrimary, REGN, IDIST, CTYPE, out TLOGS);
            }   //  end for each loop of volume equations

            //  and make sure modified log stock list is saved
            if (TLOGS != 0 && logStockList.Count != 0)
            { DataLayer.SaveLogStock(logStockList, TLOGS); }

            return tcv;
        }

        protected TreeCalculatedValuesDO CalculateTreeVolume(VolumeEquationDO volEq, TreeCalculatedValuesDO tcv, List<LogStockDO> logStockList, TreeDO tree, bool hasRecoverablePrimary, int REGN, int IDIST, string CTYPE, out int TLOGS)
        {
            

            // Inputs
            int BA = 0;
            int SI = 0;
            int INDEB = 0;
            float DBHOB = tree.DBH;
            float DRCOB = tree.DRC;

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
                ////  load LOGLEN with values or zeros
                //for (int n = 0; n < numLogs; n++)
                //    LOGLEN[n] = treeLogs[n].Length;

                //for (int n = numLogs; n < VolumeLibraryInterop.I20; n++)
                //    LOGLEN[n] = 0;

                TLOGS = numLogs;
            }

            tcv ??= new TreeCalculatedValuesDO { Tree_CN = tree.Tree_CN };

            //  Get top DIBs based on comparison of DIB on volume equation versus DIB on tree.
            //  If tree DIB is zero, use volume equation DIB.  Else use tree DIB
            float MTOPP = tree.TopDIBPrimary <= 0 ? volEq.TopDIBPrimary : tree.TopDIBPrimary;
            float MTOPS = tree.TopDIBSecondary <= 0 ? volEq.TopDIBSecondary : tree.TopDIBSecondary;

            float STUMP = volEq.StumpHeight;

            //  volume flags
            int CUTFLG = (int)volEq.CalcTotal;
            int BFPFLG = (int)volEq.CalcBoard;
            int CUPFLG = (int)volEq.CalcCubic;
            int CDPFLG = (int)volEq.CalcCord;
            int SPFLG = (int)volEq.CalcTopwood;

            var merchModeFlag = volEq.MerchModFlag;
            int PMTFLG = merchModeFlag == 2 ? (int)volEq.MerchModFlag
                : 0;

            //  if merch rules have changed, pull those into mRules
            MRules mRules = merchModeFlag == 2 ? new MRules(volEq)
                : new MRules(evod: 2, op: 11);

            int fiaspcd = int.Parse(volEq.VolumeEquationNumber.Substring(7, 3));

            // these variables are passed to VOLLIBCSNVB
            // but currently unused.
            float brkht = 0.0f;
            float brkhtd = 0.0f;
            float cr = 0.0f;
            float cull = 0.0f;
            int decaycd = 0;

            Log?.LogDebug("Calculating Volume VolEq: {VolumeEquationNumber}", volEq.VolumeEquationNumber);
            Log?.LogTrace("VolLib Prams " +
                            "{REGN} {FORST} {VOLEQ} {MTOPP} {MTOPS} " +
                            "{STUMP} {DBHOB} {DRCOB} {HTTYPE} {HTTOT} " +
                            "{HTLOG} {HT1PRD} {HT2PRD} {UPSHT1} {UPSHT2} " +
                            "{UPSD1} {UPSD2} {HTREF} {AVGZ1} {AVGZ2} " +
                            "{FCLASS} {DBTBH} {BTR} ",
                            REGN, Forest, volEq.VolumeEquationNumber, MTOPP, MTOPS,
                            STUMP, DBHOB, DRCOB, tree.TreeDefaultValue.MerchHeightType, HTTOT,
                            HTLOG, HT1PRD, HT2PRD, UPSHT1, UPSHT2,
                            UPSD1, UPSD2, HTREF, AVGZ1, AVGZ2,
                            FCLASS, DBTBH, BTR);

            //  volume library call
            VolLib.CalculateVolumeNVB(REGN, Forest, volEq.VolumeEquationNumber, MTOPP, MTOPS,
                STUMP, DBHOB, DRCOB, tree.TreeDefaultValue.MerchHeightType, HTTOT,
                HTLOG, HT1PRD, HT2PRD, UPSHT1, UPSHT2,
                UPSD1, UPSD2, HTREF, AVGZ1, AVGZ2,
                FCLASS, DBTBH, BTR, out var VOL, out var LOGVOL,
                out var LOGDIA, out var LOGLEN, out var BOLHT, out TLOGS, out var NOLOGP,
                out var NOLOGS, CUTFLG, BFPFLG, CUPFLG, CDPFLG,
                SPFLG, tree.TreeDefaultValue.ContractSpecies, tree.SampleGroup.PrimaryProduct, HTTFLL, tree.LiveDead,
                BA, SI, CTYPE, out var ERRFLAG, PMTFLG,
                mRules, IDIST,
                brkht, brkhtd, fiaspcd, out var drybio, out var grnbio,
                cr, cull, decaycd);

            if (ERRFLAG > 0)
            {
                Log?.LogInformation($"VOLLIBCSNVB Error Flag {ERRFLAG} - " + ErrorReport.GetWarningMessage(ERRFLAG.ToString()));
                DataLayer.LogError("Tree", (int)tree.Tree_CN, "W", ERRFLAG.ToString());
            }

            Log?.LogTrace($"Tree_CN {tree.Tree_CN} Vol Array" + string.Join(", ", VOL));

            //  Update log stock table with calculated values
            logStockList.AddRange(GetLogStockList(treeLogs, (int)tree.Tree_CN, LOGVOL, LOGDIA, LOGLEN, TLOGS));

            //  Next, calculate net volumes
            NetVolumeCalculator.calcNetVol(CTYPE.ToString(), VOL, LOGVOL, logStockList, tree, Region,
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
                        logStockList[n].SeenDefect = (float)Math.Round((LOGVOL[n, 3] - LOGVOL[n, 5]) / LOGVOL[n, 3] * 100);
                }   //  end for n loop
            }   //  endif

            //  Need to see if biomass calculation is needed here since it needs VOL[13] and VOL[14] for the call
            if(volEq.CalcBiomass == 1)
            {
                var calculatedBiomass = CalculateBiomass(tree, VOL, Region, Forest);
                calculatedBiomass.CopyValuesToTreeCalculatedValues(tcv);
            }

            //  Store volumes in tree calculated values
            //  because of the change made to how these calculations work, the TreeCalculatedValues table is emptied
            //  prior to processing and rebuilt.
            //  Discovered when multiple volume equations are found for an individual tree, the save into the
            //  database fails.  The program needs to check for duplciate tree_CN in the list and update
            //  the record instead of adding another.  Otherwise, the tree_CN causes a constraint violations
            //  when it trys to save through the DAL.
            //  March 2014
            float hiddenDefect = tree.HiddenPrimary > 0 ? tree.HiddenPrimary : Math.Max(tree.TreeDefaultValue.HiddenPrimary, 0);

            SetTreeCalculatedValues(tcv, VOL, LOGVOL, CUTFLG, BFPFLG, CUPFLG, CDPFLG,
                             SPFLG, hasRecoverablePrimary, tree.TreeDefaultValue.CullPrimary, hiddenDefect, tree.SeenDefectPrimary, tree.RecoverablePrimary,
                             TLOGS, NOLOGP, NOLOGS, Region, DataLayer);

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
            float[] VOL = new float[VolumeLibraryInterop.I15];

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
                    calculatedBiomass.CopyValuesToTreeCalculatedValues(tcList[nthRow]);
                }
                else if (nthRow < 0)
                {
                    TreeCalculatedValuesDO tcvdo = new TreeCalculatedValuesDO();
                    tcvdo.Tree_CN = td.Tree_CN;
                    calculatedBiomass.CopyValuesToTreeCalculatedValues(tcvdo);
                    tcList.Add(tcvdo);
                }   //  endif on nthRow
            }   //  end foreach loop

            return tcList;
        }

        public CrzBiomassResult CalculateBiomass(TreeDO tree, float[] VOL, string region, string forest)
        {
            
            var species = tree.Species;
            var product = tree.SampleGroup.PrimaryProduct;
            var liveDead = tree.LiveDead;

            var currFIA = tree.TreeDefaultValue.FIAcode;
            var currDBH = tree.DBH;
            var currHGT = tree.TotalHeight;
            var currFC = (int)tree.FormClass;
            var currDRC = tree.DRC;

            int SPCD = (int)currFIA;
            int iRegn = int.Parse(region);
            int ERRFLAG = 0;
            var WF = DataLayer.GetWeightFactorArray(species, product, liveDead, VolLib);

            
            CrzBiomassResult calculatedBiomass;
            try
            {
                calculatedBiomass = VolLib.CalculateBiomass(iRegn, forest, SPCD, currDBH, currDRC,
                    currHGT, currFC, VOL, WF,
                    out ERRFLAG, product);
            }
            catch (AccessViolationException e)
            {
                Log?.LogCritical(e, "Tree_CN{TreeCN} fiaCode{fiaCode} prod{prod}", tree.Tree_CN, currFIA, product);
                throw;
            }

            // Apply percent removed if greater than zero
            // seems like each BioEq for a given species/prod has the same PercentRemoved value
            var pctRemv = DataLayer.GetPercentRemoved(species, product);
            calculatedBiomass.AdjustForPercentRemoved(pctRemv);

            return calculatedBiomass;
        }

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

        public static void SetTreeCalculatedValues(TreeCalculatedValuesDO tcv,
            float[] VOL,
            float[,] LOGVOL,
            int calcTotal,
            int calcBoard,
            int calcCubic,
            int calcCord,
            int calcTopwood,
            bool hasRecoverablePrimary,
            float cullDef,
            float hidDef,
            float seenDef,
            float recvDef,
            int TLOGS,
            float numberOfLogsPrimary,
            float numberOfLogsSecondary,
            string currRegion,
            IErrorLogDataService errorLogDataService)
        {
            //  updates tree record in tree calculated values list
            if (calcTotal == 1) tcv.TotalCubicVolume = VOL[0];
            if (calcBoard == 1)         //  board foot volume
            {
                tcv.GrossBDFTPP = VOL[1];
                tcv.NetBDFTPP = VOL[2];
            }   //  endif GBDFTflag

            if (calcCubic == 1)         //  cubic foot volume
            {
                tcv.GrossCUFTPP = VOL[3];
                tcv.NetCUFTPP = VOL[4];
            }   //  endif GCUFTflag

            if (calcCord == 1) tcv.CordsPP = VOL[5];

            if (calcTopwood == 1)          //  secondary product was calculated
            {
                tcv.GrossCUFTSP = VOL[6];
                tcv.NetCUFTSP = VOL[7];
                tcv.CordsSP = VOL[8];
                tcv.GrossBDFTSP = VOL[11];
                tcv.NetBDFTSP = VOL[12];
            }
            else if (calcTopwood == 0)
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
            if (hasRecoverablePrimary)
            {
                SetTcvRecoveredVolume(tcv, cullDef, hidDef, seenDef, recvDef, currRegion, errorLogDataService);
            }

            static void SetTcvRecoveredVolume(TreeCalculatedValuesDO tcv, float cullDef, float hidDef,
                                            float seenDef, float recvDef, string currRegion, IErrorLogDataService errorLogDataService)
            {
                //  Check if recoverable defect is greater than the sum
                //  of the defects.  Is so, use the sum of the defects in place
                //  of the entered recoverable defect.  Issue a warning for this tree.
                //  This is for every region except 10
                float checkRecv = recvDef;
                if (recvDef > cullDef + hidDef + seenDef && currRegion != "10")
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
                                sawValue = tcvList[mthRow].GrossBDFTPP * valList[nthRow].Coefficient1 / 1000;
                                break;

                            case "02":
                                sawValue = tcvList[mthRow].NetBDFTPP * valList[nthRow].Coefficient1 / 1000;
                                break;

                            case "03":
                                sawValue = tcvList[mthRow].GrossCUFTPP * valList[nthRow].Coefficient1 / 100;
                                break;

                            case "04":
                                sawValue = tcvList[mthRow].NetCUFTPP * valList[nthRow].Coefficient1 / 100;
                                break;

                            case "05":
                                sawValue = tcvList[mthRow].CordsPP * valList[nthRow].Coefficient1;
                                break;

                            case "06":
                                sawValue = tcvList[mthRow].GrossBDFTPP * valList[nthRow].Coefficient1 / 1000;
                                topValue = tcvList[mthRow].GrossCUFTSP * valList[nthRow].Coefficient2 / 100;
                                break;

                            case "07":
                                sawValue = tcvList[mthRow].NetBDFTPP * valList[nthRow].Coefficient1 / 1000;
                                topValue = tcvList[mthRow].NetCUFTSP * valList[nthRow].Coefficient2 / 100;
                                break;

                            case "08":
                                sawValue = tcvList[mthRow].GrossBDFTPP * valList[nthRow].Coefficient1 / 1000;
                                topValue = tcvList[mthRow].CordsSP * valList[nthRow].Coefficient2;
                                break;

                            case "09":
                                sawValue = tcvList[mthRow].NetBDFTPP * valList[nthRow].Coefficient1 / 1000;
                                topValue = tcvList[mthRow].CordsSP * valList[nthRow].Coefficient2;
                                break;

                            case "10":
                                sawValue = tcvList[mthRow].GrossCUFTPP * valList[nthRow].Coefficient1 / 100;
                                topValue = tcvList[mthRow].GrossCUFTSP * valList[nthRow].Coefficient2 / 100;
                                break;

                            case "11":
                                sawValue = tcvList[mthRow].GrossCUFTPP * valList[nthRow].Coefficient1 / 100;
                                topValue = tcvList[mthRow].CordsSP * valList[nthRow].Coefficient2;
                                break;

                            case "12":
                                sawValue = tcvList[mthRow].NetCUFTPP * valList[nthRow].Coefficient1 / 100;
                                topValue = tcvList[mthRow].NetCUFTSP * valList[nthRow].Coefficient2 / 100;
                                break;

                            case "13":
                                sawValue = tcvList[mthRow].NetCUFTPP * valList[nthRow].Coefficient1 / 100;
                                topValue = tcvList[mthRow].CordsSP * valList[nthRow].Coefficient2;
                                break;

                            case "14":
                                sawValue = tcvList[mthRow].CordsPP * valList[nthRow].Coefficient1;
                                topValue = tcvList[mthRow].CordsSP * valList[nthRow].Coefficient2;
                                break;

                            case "50":
                                sawValue = valList[nthRow].Coefficient1 + valList[nthRow].Coefficient2 * tdo.DBH +
                                    valList[nthRow].Coefficient3 * tdo.TotalHeight +
                                    valList[nthRow].Coefficient4 * tdo.DBH * tdo.DBH +
                                    valList[nthRow].Coefficient5 * tdo.DBH * tdo.DBH * tdo.TotalHeight +
                                    valList[nthRow].Coefficient6 * tdo.DBH * tdo.DBH * tdo.DBH * tdo.TotalHeight;
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
    }
}