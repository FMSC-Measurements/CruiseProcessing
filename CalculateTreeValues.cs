using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using System.Runtime.InteropServices;

namespace CruiseProcessing
{
    public class CalculateTreeValues
    {
        #region
        public string fileName;
        private string currRegion;
        private  string currForest;
        private  string currDist;
        private  string currCruise;
        private  float MTOPP, MTOPS, NOLOGP, NOLOGS, BTR, DBTBH;
        private  int REGN, HTLOG, HTREF, FCLASS, HTTFLL, ERRFLAG, TLOGS, BA, SI, INDEB, PMTFLG;
        private  int CUTFLG, BFPFLG, CUPFLG, CDPFLG, SPFLG, IDIST;
        private  float DBHOB, DRCOB, HTTOT, HT1PRD, HT2PRD, STUMP;
        private  float UPSHT1, UPSHT2, UPSD1, UPSD2, AVGZ1, AVGZ2;
        private  StringBuilder FORST = new StringBuilder(12);
        private  StringBuilder PROD = new StringBuilder(12);
        private  StringBuilder HTTYPE = new StringBuilder(12);
        private  StringBuilder VOLEQ = new StringBuilder(12);
        private  StringBuilder LIVE = new StringBuilder(12);
        private  StringBuilder CONSPEC = new StringBuilder(12);
        private  MRules mRules;

        //  declarations for external methods from vollib.dll
        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)]//EntryPoint = "VERNUM2",
        public static extern void VERNUM2(ref int a);

        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)]// CallingConvention = CallingConvention.StdCall)]
         static extern void VOLLIBCS(ref int regn, StringBuilder forst, StringBuilder voleq, ref float mtopp, ref float mtops,
            ref float stump, ref float dbhob, ref float drcob, StringBuilder httype, ref float httot, ref int htlog, ref float ht1prd,
            ref float ht2prd, ref float upsht1, ref float upsht2, ref float upsd1, ref float upsd2, ref int htref, ref float avgz1,
            ref float avgz2, ref int fclass, ref float dbtbh, ref float btr, ref int i3, ref int i7, ref int i15, ref int i20,
            ref int i21, float[] vol, float[,] logvol, float[,] logdia, float[] loglen, float[] bohlt, ref int tlogs, ref float nologp,
            ref float nologs, ref int cutflg, ref int bfpflg, ref int cupflg, ref int cdpflg, ref int spflg, StringBuilder conspec,
            StringBuilder prod, ref int httfll, StringBuilder live, ref int ba, ref int si, StringBuilder ctype, ref int errflg,
            ref int indeb, ref int pmtflg, ref MRules mRules, ref int dist, int ll1, int ll2, int ll3, int ll4, int ll5, int ll6, int ll7, int charLen);

        [DllImportAttribute("vollib.dll", CallingConvention = CallingConvention.Cdecl)]
         static extern void CRZBIOMASSCS(ref int regn, StringBuilder forst, ref int spcd, ref float dbhob, ref float drcob, ref float httot, 
                                        ref int fclass, float[] vol, float[] wf, float[] bms, ref int errflg, int i1);

        #endregion

        public  void ProcessTrees(IEnumerable<SaleDO> saleList, IEnumerable<TreeDO> tList, IEnumerable<VolumeEquationDO> vqList, string currST, string currMethod, long currST_CN)
        {
            //  Retrieve region, forest and district
            SaleDO sale = saleList.First();
            currRegion = sale.Region;
            currForest = sale.Forest;
            currDist = sale.District;
            currCruise = sale.SaleNumber;

            //  convert district  to integer for library call
            IDIST = Convert.ToInt16(currDist);

            //  Calculate volumes by stratum
           // List<TreeDO> justStratum = Global.BL.JustMeasuredTrees(currST_CN);

            CalculateVolumes(tList, vqList, currST, currMethod, Global.BL.getBiomassEquations(), currST_CN);

            //  Check for value equations
            if (Global.BL.getValueEquations().Count() > 0)
            {
                CalculateValue(tList, currST, currMethod, currRegion, Global.BL.getValueEquations(), Global.BL.getTreeCalculatedValues());
            }   //  endif valList

            return;
        }   //  end ProcessTrees


        private  void CalculateVolumes(IEnumerable<TreeDO> tList, IEnumerable<VolumeEquationDO> vqList,
            string currST, string currMethod, IEnumerable<BiomassEquationDO> bioList, long currST_CN)
        {        
            //  Also need tree calculated values by stratum
            List<TreeCalculatedValuesDO> tcList = new List<TreeCalculatedValuesDO>();

            //  Definitions for the Fortran DLL call
            int I3 = 3;
            int I7 = 7;
            int I15 = 15;
            int I20 = 20;
            int I21 = 21;

            int BA = 0;
            int SI = 0;
            float[] VOL = new float[I15];
            float[] LOGLEN = new float[I20];
            float[] BOLHT = new float[I21];
            float[,] LOGVOL = new float[I20,I7];
            float[,] LOGDIA = new float[I3,I21];

            //  character length parameters
            const int strlen = 12;
            const int charLen = 1;

            //strings for passing to/from Fortran.  pass fixed length strings
            StringBuilder CTYPE = new StringBuilder(12);
            string vllType = Global.BL.getVLL();
            if (vllType == "false")
                CTYPE.Append("C");
            else CTYPE.Append(vllType);

            //  return array for biomass calculations
            float[] calculatedBiomass = new float[8];

            //  need to check for method -- FIXCNT only does biomass when requested.  Does not calculate volume. June 2016
            if (currMethod == "FIXCNT")
            {
                //  pull all trees
              
                foreach (TreeDO td in Global.BL.JustFIXCNTtrees(currST_CN))
                {
                    CalculateBiomass(ref calculatedBiomass, bioList, td.Species, td.SampleGroup.PrimaryProduct, td.TreeDefaultValue.FIAcode, 
                        currRegion, currForest, VOL, td.DBH, td.DRC, td.TotalHeight,(int) td.FormClass, strlen);
                    //Fnd this tree in calculated values and store appropriately
                    int nthRow = tcList.FindIndex(
                        delegate(TreeCalculatedValuesDO t)
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
            }
            else
            {
                //  Will recovered volume be calculated?  Sum recoverable defect primary product to set flag
                int RecvFlag = 0;
                //double justRecover = strataTrees.Sum(tdo => tdo.RecoverablePrimary);
                //if (justRecover > 0) RecvFlag = 1;

                //  pull all volume equations
                List<LogDO> justTreeLogs;
                List<LogStockDO> logStockList;
                //  loop through individual trees and calculate volume for all equations requested by species/product
                foreach (TreeDO td in tList.Where(t => t.Stratum_CN == currST_CN && t.CountOrMeasure == "M"))
                {
                    //  Clear out calculated fields
                    BA = 0;
                    SI = 0;
                    MTOPP = 0;
                    MTOPS = 0;
                    NOLOGP = 0;
                    NOLOGS = 0;
                    TLOGS = 0;
                    Array.Clear(VOL, 0, VOL.Length);
                    Array.Clear(BOLHT, 0, BOLHT.Length);
                    Array.Clear(LOGDIA, 0, LOGDIA.Length);
                    Array.Clear(LOGLEN, 0, LOGLEN.Length);
                    Array.Clear(LOGVOL, 0, LOGVOL.Length);

                    //  find logs for this tree
                    justTreeLogs = Global.BL.getTreeLogs((long)td.Tree_CN).ToList();
                    logStockList = new List<LogStockDO>();
                    if (CTYPE.ToString() == "V" && justTreeLogs.Count() > 0)
                    {
                        //  load LOGLEN with values or zeros
                        for (int n = 0; n < justTreeLogs.Count(); n++)
                            LOGLEN[n] = (float)justTreeLogs[n].Length;

                        for (int n = justTreeLogs.Count(); n < 20; n++)
                            LOGLEN[n] = 0;

                        TLOGS = justTreeLogs.Count();
                    }   //  endif


                    //  Is this a FBS tree?  process separately and skip volume calculation
                    if (td.IsFallBuckScale == 1)
                    {
                        CalcFallBuckScale((int)td.Tree_CN, justTreeLogs, logStockList, tcList);
                        if (logStockList.Count != 0)
                            Global.BL.SaveLogStock(logStockList, logStockList.Count);
                    }
                    else if ((td.DBH > 0 || td.DRC > 0) &&
                            (td.TotalHeight > 0 || td.MerchHeightPrimary > 0 ||
                             td.MerchHeightSecondary > 0 || td.UpperStemHeight > 0))
                    {
                        //  Get volume equations
                        //List<VolumeEquationDO> currEquations = Global.BL.GetAllEquationsToCalc(td.Species, td.SampleGroup.PrimaryProduct);
                        foreach (VolumeEquationDO ved in vqList.Where(vq => vq.Species == td.Species && vq.PrimaryProduct == td.SampleGroup.PrimaryProduct))
                        {
                            //  get values for library call
                            getLibraryCallValues(td, ved);

                            //  volume library call
                            VOLLIBCS(ref REGN, FORST, VOLEQ, ref MTOPP, ref MTOPS, ref STUMP,
                                     ref DBHOB, ref DRCOB, HTTYPE, ref HTTOT, ref HTLOG,
                                     ref HT1PRD, ref HT2PRD, ref UPSHT1, ref UPSHT2, ref UPSD1, ref UPSD2,
                                     ref HTREF, ref AVGZ1, ref AVGZ2, ref FCLASS, ref DBTBH, ref BTR,
                                     ref I3, ref I7, ref I15, ref I20, ref I21, VOL, LOGVOL, LOGDIA, LOGLEN, BOLHT,
                                     ref TLOGS, ref NOLOGP, ref NOLOGS, ref CUTFLG, ref BFPFLG,
                                     ref CUPFLG, ref CDPFLG, ref SPFLG, CONSPEC, PROD,
                                     ref HTTFLL, LIVE, ref BA, ref SI, CTYPE, ref ERRFLAG,
                                     ref INDEB, ref PMTFLG, ref mRules, ref IDIST, strlen, strlen, strlen, strlen,
                                     strlen, strlen, strlen, charLen);
                            if (ERRFLAG > 0)
                                Utilities.LogError("Tree", (int)td.Tree_CN, "W", ERRFLAG.ToString());

                            //  Update log stock table with calculated values
                            UpdateLogStock(justTreeLogs, logStockList, (long)td.Tree_CN, LOGVOL, LOGDIA, LOGLEN, TLOGS);

                            //  Next, calculate net volumes
                            CalculateNetVolume.calcNetVol(CTYPE.ToString(), VOL, LOGVOL, logStockList, td, currRegion,
                                                          NOLOGP, NOLOGS, TLOGS, MTOPP, td.SampleGroup.PrimaryProduct);

                            //  Update number of logs
                            if (currRegion != "10" && currRegion != "06" && currRegion != "6")
                            {
                                TLOGS = (int)NOLOGP;
                                if (NOLOGP - TLOGS > 0) TLOGS++;
                            }   //  endif

                            //  Update log defects -- except for BLM
                            if (currRegion != "07")
                            {
                                for (int n = 0; n < TLOGS; n++)
                                {
                                    if (LOGVOL[n, 3] > 0)
                                        logStockList[n].SeenDefect = (float)Math.Round(((LOGVOL[n, 3] - LOGVOL[n, 5]) / LOGVOL[n, 3] * 100));
                                }   //  end for n loop
                            }   //  endif

                            //  Need to see if biomass calculation is needed here since it needs VOL[13] and VOL[14] for the call
                            if (bioList.Count() > 0)
                            {
                                CalculateBiomass(ref calculatedBiomass, bioList, td.Species, td.SampleGroup.PrimaryProduct,
                                                td.TreeDefaultValue.FIAcode, currRegion, currForest, VOL, td.DBH, td.DRC, td.TotalHeight,
                                                (int)td.FormClass, strlen);
                            }   //  endif bioList

                            //  Store returned values in tree calculated values
                            //  March 2015 -- same check on hidden defect
                            float tempHidden = 0;
                            if (td.HiddenPrimary <= 0)
                            {
                                if (td.TreeDefaultValue.HiddenPrimary > 0)
                                    tempHidden = td.TreeDefaultValue.HiddenPrimary;
                                else tempHidden = 0;
                            }
                            else tempHidden = td.HiddenPrimary;
                            StoreCalculations((int)td.Tree_CN, VOL, LOGVOL, CUTFLG, BFPFLG, CUPFLG, CDPFLG, SPFLG, RecvFlag,
                                        td.TreeDefaultValue.CullPrimary, tempHidden,
                                        td.SeenDefectPrimary, td.RecoverablePrimary, calculatedBiomass, tcList,
                                        logStockList);
                        }   //  end for each loop of volume equations
                    }   //  endif

                    //  and make sure modified log stock list is saved
                    if(TLOGS != 0 && logStockList.Count != 0)
                        Global.BL.SaveLogStock(logStockList, TLOGS);

                }   //  end foreach loop
            }   //  endif on method

            //  Save calculated values
            //Global.BL.fileName = fileName;
            Global.BL.SaveTreeCalculatedValues(tcList);
           

        }   //  end CalculateVolumes


        private  void getLibraryCallValues(TreeDO td, VolumeEquationDO ved)
        {
            //  convert values needed for libary call to volume library
            //  first, clear string builder fields
            FORST.Remove(0, FORST.Length);
            VOLEQ.Remove(0, VOLEQ.Length);
            HTTYPE.Remove(0, HTTYPE.Length);
            CONSPEC.Remove(0, CONSPEC.Length);
            PROD.Remove(0, PROD.Length);
            LIVE.Remove(0, LIVE.Length);

            REGN = Convert.ToInt16(currRegion);
            FORST.Append(currForest);
            VOLEQ.Append(ved.VolumeEquationNumber);

            //  Get top DIBs based on comparison of DIB on volume equation versus DIB on tree.  
            //  If tree DIB is zero, use volume equation DIB.  Else use tree DIB
            if(td.TopDIBPrimary <= 0)
                MTOPP = ved.TopDIBPrimary;
            else MTOPP = td.TopDIBPrimary;

            if(td.TopDIBSecondary <= 0)
                MTOPS = ved.TopDIBSecondary;
            else MTOPS = td.TopDIBSecondary;

            STUMP = ved.StumpHeight;
            DBHOB = td.DBH;
            DRCOB = td.DRC;

            HTTYPE.Append(td.TreeDefaultValue.MerchHeightType);
            HTTOT = td.TotalHeight;
            HTLOG = (int)td.TreeDefaultValue.MerchHeightLogLength;
            HT1PRD = td.MerchHeightPrimary;
            HT2PRD = td.MerchHeightSecondary;
            UPSHT1 = td.UpperStemHeight;
            UPSHT2 = 0;
            UPSD1 = td.UpperStemDiameter;
            //UPSD1 = td.UpperStemDOB;
            UPSD2 = 0;
            AVGZ1 = 0;
            AVGZ2 = 0;
            FCLASS = (int)td.FormClass;
            if (FCLASS == 0)
            {
                FCLASS = (int) td.TreeDefaultValue.FormClass;
                AVGZ1 = td.TreeDefaultValue.AverageZ;
                HTREF = (int)td.TreeDefaultValue.ReferenceHeightPercent;
            }   //  endif
            
            DBTBH = td.DBHDoubleBarkThickness;
            BTR = td.TreeDefaultValue.BarkThicknessRatio;

            //  volume flags
            CUTFLG = (int)ved.CalcTotal;
            BFPFLG = (int)ved.CalcBoard;
            CUPFLG = (int)ved.CalcCubic;
            CDPFLG = (int)ved.CalcCord;
            SPFLG = (int)ved.CalcTopwood;
            CONSPEC.Append(td.TreeDefaultValue.ContractSpecies);
            PROD.Append(td.SampleGroup.PrimaryProduct);
            HTTFLL = (int)td.HeightToFirstLiveLimb;
            LIVE.Append(td.LiveDead);
            BA = 0;
            SI = 0;
            INDEB = 0;
            //  if merch rules have changed, pull those into mRules
            if (ved.MerchModFlag == 2)
            {
                PMTFLG = (int) ved.MerchModFlag;
                mRules.evod = (int) ved.EvenOddSegment;
                mRules.opt = (int) ved.SegmentationLogic;
                mRules.cor = 'Y';
                mRules.maxlen = ved.MaxLogLengthPrimary;
                mRules.minlen = ved.MinLogLengthPrimary;
                mRules.merchl = ved.MinMerchLength;
                mRules.mtopp = ved.TopDIBPrimary;
                mRules.mtops = ved.TopDIBSecondary;
                mRules.stump = ved.StumpHeight;
                mRules.trim = ved.Trim;
                //  don't need to set the following
                //mRules.btr = 0;
                //mRules.dbhtbh = 0;
                //mRules.minbfd = 0;
            }
            else
            {
                PMTFLG = 0;
                mRules.evod = 2;
                mRules.opt = 11;
                mRules.cor = 'Y';
                mRules.maxlen = 0;
                mRules.minlen = 0;
                mRules.merchl = 0;
                mRules.mtopp = 0;
                mRules.mtops = 0;
                mRules.stump = 0;
                mRules.trim = 0;
                //  don't need to set the following
                //mRules.btr = 0;
                //mRules.dbhtbh = 0;
                //mRules.minbfd = 0;
            }

            return;
        }   //  end getLibraryCallValues


        private  void CalculateBiomass(ref float[] calculatedBiomass, IEnumerable<BiomassEquationDO> bioList, 
                                            string currSP, string currPP, long currFIA, string currRegion, 
                                            string currForest, float[] VOL, float currDBH, float currDRC,
                                            float currHGT, int currFC, int strlen)
        {
            //  separate function to calculate biomass
            float[] WF = new float[3];
            //  reset calculated biomass array
            Array.Clear(calculatedBiomass, 0, calculatedBiomass.Length);

            //  was biomass flag set for current species/product?
            //List<BiomassEquationDO> justSpecies = bioList.FindAll(
            //    delegate(BiomassEquationDO bdo)
            //    {
            //        return bdo.Species == currSP && bdo.Product == currPP;
            //    });

            //int nthRow = justSpecies.FindIndex(
            //    delegate(BiomassEquationDO b)
            //    {
            //        return b.Component == "PrimaryProd";
            //    });

            BiomassEquationDO primaryProduct = bioList.FirstOrDefault(bdo =>
                bdo.Species == currSP && bdo.Product == currPP && bdo.Component == "PrimaryProd");

            if (primaryProduct != null)
            {
                WF[0] = primaryProduct.WeightFactorPrimary;
                WF[2] = primaryProduct.PercentMoisture;
            }

            //if (nthRow >= 0)
            //{
            //    WF[0] = justSpecies[nthRow].WeightFactorPrimary;
            //    WF[2] = justSpecies[nthRow].PercentMoisture;
            //}

            //nthRow = justSpecies.FindIndex(
            //    delegate(BiomassEquationDO b)
            //    {
            //        return b.Component == "SecondaryProd";
            //    });

            BiomassEquationDO secondaryProduct = bioList.FirstOrDefault(bdo =>
                bdo.Species == currSP && bdo.Product == currPP && bdo.Component == "SecondaryProd");
           
            if (secondaryProduct != null)
                WF[1] = secondaryProduct.WeightFactorSecondary;

            FORST.Append(currForest);
            int SPCD = Convert.ToInt16(currFIA);
            int iRegn = Convert.ToInt16(currRegion);
            StringBuilder sForest = new StringBuilder(256);
            sForest.Append(currForest);
            //  WHY?                            //if(currRegion == "9" || currRegion == "09")
            //    CRZBIOMASSCS(ref REGN,FORST,ref SPCD,ref DBHOB,ref HTTOT,VOL,WF,calculatedBiomass,ref ERRFLAG,strlen);
            //else
            CRZBIOMASSCS(ref iRegn, sForest, ref SPCD, ref currDBH, ref currDRC, ref currHGT, ref currFC, VOL, WF, 
                                calculatedBiomass, ref ERRFLAG, strlen);


            // Apply percent removed if greater than zero
            if (secondaryProduct != null)
            {
                if (secondaryProduct.PercentRemoved > 0)
                {
                    for (int j = 0; j < 8; j++)
                        calculatedBiomass[j] = calculatedBiomass[j] * (float)(secondaryProduct.PercentRemoved / 100.0);

                }   //  endif percent removed greater than zero
            }  //  endif oon nthrow
            return;
        }   //  end CaclculateBiomass


        private  void UpdateLogStock(List<LogDO> justTreeLogs, List<LogStockDO> logStockList, long currTreeCN,
                                            float[,] LOGVOL, float[,] LOGDIA, float[] LOGLEN, int TLOGS)
        {
            int nthLog = 0;
            LogStockDO lsdo;
            for (int n = 0; n < TLOGS; n++)
            {
                nthLog = n + 1;
                lsdo = new LogStockDO();

                lsdo.Tree_CN = currTreeCN;
                lsdo.LogNumber = Convert.ToString(nthLog);
                lsdo.SmallEndDiameter = LOGDIA[1, nthLog];
                lsdo.LargeEndDiameter = LOGDIA[1, n];
                lsdo.Length = (long)LOGLEN[n];
                lsdo.GrossBoardFoot = LOGVOL[n, 0];
                lsdo.NetBoardFoot = LOGVOL[n, 2];
                lsdo.BoardFootRemoved = LOGVOL[n, 1];
                lsdo.GrossCubicFoot = LOGVOL[n, 3];
                lsdo.NetCubicFoot = LOGVOL[n, 5];
                lsdo.CubicFootRemoved = LOGVOL[n, 4];
                lsdo.SeenDefect = 0;
                lsdo.PercentRecoverable = 0;
                lsdo.DIBClass = LOGDIA[0, nthLog];
                var ld = justTreeLogs.Find(l => l.Tree_CN == currTreeCN && l.LogNumber == lsdo.LogNumber);
                if (ld != null)
                {
                    lsdo.SeenDefect = ld.SeenDefect;
                    lsdo.PercentRecoverable = ld.PercentRecoverable;
                    lsdo.Grade = ld.Grade;
                }

                logStockList.Add(lsdo);
            }   //  end for n loop

            //  Find defect values for each log in original logs or set to zero
//            foreach (LogStockDO ls in logStockList)
//            {
//                //int nthRow = justTreeLogs.FindIndex(
//                var ld = justTreeLogs.Find(l => l.Tree_CN == ls.Tree_CN && l.LogNumber == ls.LogNumber);
////                    delegate(LogDO ld)
////                    {
////                        return ls.Tree_CN == ld.Tree_CN && lsdo.LogNumber == ld.LogNumber;
////                    });
//                if (ld != null)
//                {
//                    ls.SeenDefect = ld.SeenDefect;
//                    ls.PercentRecoverable = ld.PercentRecoverable;
//                    ls.Grade = ld.Grade;
//                }
//            }   //  end foreach loop

            return;
        }   //  end UpdateLogStock


        private  void StoreCalculations(int currTreeCN, float[] VOL, float[,] LOGVOL, int TCUFTflag, int GBDFTflag, 
                                            int GCUFTflag, int CORDflag, int SecondVolFlag, int RecvFlag, 
                                            float cullDef, float hidDef, float seenDef, float recvDef,
                                            float[] biomassCalcs, List<TreeCalculatedValuesDO> tcvList,
                                            List<LogStockDO> logStockList)
        {
            //  Store volumes in tree calculated values
            //  because of the change made to how these calculations work, the TreeCalculatedValues table is emptied
            //  prior to processing and rebuilt.
            //  Discovered when multiple volume equations are found for an individual tree, the save into the
            //  database fails.  The program needs to check for duplciate tree_CN in the list and update
            //  the record instead of adding another.  Otherwise, the tree_CN causes a constraint violations
            //  when it trys to save through the DAL.
            //  March 2014
            TreeCalculatedValuesDO t = Global.BL.getTreeCalcRow(currTreeCN);

 //           int nthRow = tcvList.FindIndex(
 //               delegate(TreeCalculatedValuesDO t)
 //               {
 //                   return t.Tree_CN == currTreeCN;
 //               });
            if (t != null)
                loadSameTree(t, VOL, LOGVOL, TCUFTflag, GBDFTflag, GCUFTflag, CORDflag,
                             SecondVolFlag, RecvFlag, cullDef, hidDef, seenDef, recvDef,
                             biomassCalcs, tcvList, logStockList);
            else
                loadNewTree(currTreeCN, VOL, LOGVOL, TCUFTflag, GBDFTflag, GCUFTflag, CORDflag,
                            SecondVolFlag, RecvFlag, cullDef, hidDef, seenDef, recvDef,
                            biomassCalcs, tcvList, logStockList);
            return;
        }   //  end StoreCalculations


        private  void loadSameTree(TreeCalculatedValuesDO tcv, float[] VOL, float[,] LOGVOL, int TCUFTflag, int GBDFTflag,
                                            int GCUFTflag, int CORDflag, int SecondVolFlag, int RecvFlag,
                                            float cullDef, float hidDef, float seenDef, float recvDef,
                                            float[] biomassCalcs, List<TreeCalculatedValuesDO> tcvList,
                                            List<LogStockDO> logStockList)
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
                tcv.TipwoodVolume = VOL[14];
            }
            else if(SecondVolFlag == 0)
            {
                //  reset secondary buckets to zero
                tcv.GrossCUFTSP = 0;
                tcv.NetCUFTSP = 0;
                tcv.CordsSP = 0;
                tcv.GrossBDFTSP = 0;
                tcv.NetBDFTSP = 0;
                tcv.TipwoodVolume = 0;
            }   //  endif SecondVolFlag

            //  save international calcs and number of logs
            tcv.GrossBDFTIntl = VOL[9];
            tcv.NetBDFTIntl = VOL[10];
            tcv.NumberlogsMS = NOLOGP;

            //  Make sure secondary was not calculated to get correct number of logs for secondary
            if (VOL[6] != 0 || VOL[7] != 0 || VOL[8] != 0 || VOL[11] != 0 || VOL[12] != 0)
                tcv.NumberlogsTPW = NOLOGS;

            //  Sum removed volume into tree removed
            for (int n = 0; n < TLOGS; n++)
            {
                tcv.GrossBDFTRemvPP += LOGVOL[n, 1];
                tcv.GrossCUFTRemvPP += LOGVOL[n, 4];
            }   //  end for n loop

            //  Calculate recovered if needed
            if (RecvFlag == 1)
                RecoveredVolume(tcv, cullDef, hidDef, seenDef, recvDef);

            //  Store biomass if there is any
            tcv.BiomassMainStemPrimary = biomassCalcs[4];
            tcv.BiomassMainStemSecondary = biomassCalcs[5];
            tcv.Biomasstotalstem = biomassCalcs[0];
            tcv.Biomasslivebranches = biomassCalcs[1];
            tcv.Biomassdeadbranches = biomassCalcs[2];
            tcv.Biomassfoliage = biomassCalcs[3];
            tcv.BiomassTip = biomassCalcs[6];

            //  update volume calcs in logstock if log volume calculated
            for (int k = 0; k < TLOGS; k++)
            {
                //  find log to update
                int ithRow = logStockList.FindIndex(
                    delegate(LogStockDO lsd)
                    {
                        return lsd.Tree_CN == tcv.Tree_CN && lsd.LogNumber == (k + 1).ToString();
                    });
                if (ithRow >= 0)
                {
                    if (GBDFTflag == 1)
                    {
                        logStockList[ithRow].GrossBoardFoot = LOGVOL[k, 0];
                        logStockList[ithRow].BoardFootRemoved = LOGVOL[k, 1];
                        logStockList[ithRow].NetBoardFoot = LOGVOL[k, 2];
                    }   //  endif board foot
                    if (GCUFTflag == 1)
                    {
                        logStockList[ithRow].GrossCubicFoot = LOGVOL[k, 3];
                        logStockList[ithRow].CubicFootRemoved = LOGVOL[k, 4];
                        logStockList[ithRow].NetCubicFoot = LOGVOL[k, 5];
                    }   //  endif cubic foot
                }   //  endif ithRow
            }   //  end for k loop

            //  DONE!!!!!
            return;
        }   //  end loadSameTree


        private  void loadNewTree(int currTreeCN, float[] VOL, float[,] LOGVOL, int TCUFTflag, int GBDFTflag,
                                            int GCUFTflag, int CORDflag, int SecondVolFlag, int RecvFlag,
                                            float cullDef, float hidDef, float seenDef, float recvDef,
                                            float[] biomassCalcs, List<TreeCalculatedValuesDO> tcvList,
                                            List<LogStockDO> logStockList)
        {
            //  loads a new tree record into tree calculated values list
            TreeCalculatedValuesDO tcvdo = new TreeCalculatedValuesDO();

            tcvdo.Tree_CN = currTreeCN;
            if (TCUFTflag == 1) tcvdo.TotalCubicVolume = VOL[0];
            if (GBDFTflag == 1)         //  board foot volume
            {
                tcvdo.GrossBDFTPP = VOL[1];
                tcvdo.NetBDFTPP = VOL[2];
            }   //  endif GBDFTflag

            if(GCUFTflag == 1)          //  cubic foot volume
            {
                tcvdo.GrossCUFTPP = VOL[3];
                tcvdo.NetCUFTPP = VOL[4];
            }   //  end GCUFTflag

            if (CORDflag == 1) tcvdo.CordsPP = VOL[5];

            if (SecondVolFlag == 1)          //  secondary product has been calculated
            {
                tcvdo.GrossCUFTSP = VOL[6];
                tcvdo.NetCUFTSP = VOL[7];
                tcvdo.CordsSP = VOL[8];
                tcvdo.GrossBDFTSP = VOL[11];
                tcvdo.NetBDFTSP = VOL[12];
                tcvdo.TipwoodVolume = VOL[14];

            }
            else if(SecondVolFlag == 0)
            {
                //  reset all secondary buckets to zero
                tcvdo.GrossCUFTSP = 0;
                tcvdo.NetCUFTSP = 0;
                tcvdo.CordsSP = 0;
                tcvdo.GrossBDFTSP = 0;
                tcvdo.NetBDFTSP = 0;
                tcvdo.TipwoodVolume = 0;
            }   //  endif SecondVolFlag

            //  save international calcs and number of logs
            tcvdo.GrossBDFTIntl = VOL[9];
            tcvdo.NetBDFTIntl = VOL[10];
            tcvdo.NumberlogsMS = NOLOGP;

            //  Make sure secondary was not calculated to get correct number of logs for secondary
            if (VOL[6] != 0 || VOL[7] != 0 || VOL[8] != 0 || VOL[11] != 0 || VOL[12] != 0)
                tcvdo.NumberlogsTPW = NOLOGS;

            //  SUm removed volume into tree removed
            for (int n = 0; n < TLOGS; n++)
            {
                tcvdo.GrossBDFTRemvPP += LOGVOL[n, 1];
                tcvdo.GrossCUFTRemvPP += LOGVOL[n, 4];
            }   //  end for n loop

            //  Calculate recovered if needed
            if (RecvFlag == 1)
                RecoveredVolume(tcvdo, cullDef, hidDef, seenDef, recvDef);

            //  Storee biomass if there is any
            tcvdo.BiomassMainStemPrimary = biomassCalcs[4];
            tcvdo.BiomassMainStemSecondary = biomassCalcs[5];
            tcvdo.Biomasstotalstem = biomassCalcs[0];
            tcvdo.Biomasslivebranches = biomassCalcs[1];
            tcvdo.Biomassdeadbranches = biomassCalcs[2];
            tcvdo.Biomassfoliage = biomassCalcs[3];
            tcvdo.BiomassTip = biomassCalcs[6];

            //  update volume calcs in logstock
            for (int k = 0; k < TLOGS; k++)
            {
                //  find log to update
                int ithRow = logStockList.FindIndex(
                    delegate(LogStockDO lsd)
                    {
                        return lsd.Tree_CN == currTreeCN && lsd.LogNumber == (k + 1).ToString();
                    });
                if (ithRow >= 0)
                {
                    if (GBDFTflag == 1)
                    {
                        logStockList[ithRow].GrossBoardFoot = LOGVOL[k, 0];
                        logStockList[ithRow].BoardFootRemoved = LOGVOL[k, 1];
                        logStockList[ithRow].NetBoardFoot = LOGVOL[k, 2];
                    }   //  endif GBDFTflag
                    if (GCUFTflag == 1)
                    {
                        logStockList[ithRow].GrossCubicFoot = LOGVOL[k, 3];
                        logStockList[ithRow].CubicFootRemoved = LOGVOL[k, 4];
                        logStockList[ithRow].NetCubicFoot = LOGVOL[k, 5];
                    }   //  endif GCUFTflag
                }   //  endif ithRow
            }   //  end for k loop

            //  add row to list
            tcvList.Add(tcvdo);
            return;
        }   //  end loadNewTree


        private  void RecoveredVolume(TreeCalculatedValuesDO tcvdo, float cullDef, float hidDef, 
                                            float seenDef, float recvDef)
        {
            //  Check if recoverable defect is greater than the sum
            //  of the defects.  Is so, use the sum of the defects in place
            //  of the entered recoverable defect.  Issue a warning for this tree.
            //  This is for every region except 10
            float checkRecv = recvDef;
            if (recvDef > (cullDef + hidDef + seenDef) && currRegion != "10")
            {
                checkRecv = cullDef + hidDef + seenDef;
                Utilities.LogError("TREE", (int)tcvdo.Tree_CN, "W", "18");
            }   //  endif

            //  calculate recovered volume based on region
            if (currRegion == "9" || currRegion == "09")
            {
                //  No board foot recovered calculated for Region 9
                tcvdo.GrossBDFTRP = 0;
                tcvdo.GrossCUFTRP = tcvdo.GrossCUFTPP * (checkRecv / 100);
            }
            else if (currRegion == "10")
            {
                //  they use net instead of gross for the calc
                tcvdo.GrossBDFTRP = tcvdo.NetBDFTPP * (checkRecv / 100);
                tcvdo.GrossCUFTRP = tcvdo.NetCUFTPP * (checkRecv / 100);
            }   
            else
            {
                tcvdo.GrossBDFTRP = tcvdo.GrossBDFTPP * (checkRecv / 100);
                tcvdo.GrossCUFTRP = tcvdo.GrossCUFTPP * (checkRecv / 100);
            }   //  endif on region

            if (cullDef > 0 || hidDef > 0 || seenDef > 0)
                    tcvdo.CordsRP = tcvdo.CordsPP / (cullDef + hidDef + seenDef) * checkRecv;
            tcvdo.ValueRP = tcvdo.ValuePP * tcvdo.GrossCUFTRP;
            
            return;
        }   //  end RecoveredVolume


        private  void CalcFallBuckScale(int currTree_CN, List<LogDO> justTreeLogs,
                                                List<LogStockDO> logStockList,
                                                List<TreeCalculatedValuesDO> treeCalcList)
        {
            //  find tree_CN in calculated values table and store totals
            var tcl = treeCalcList.First(t => t.Tree_CN == currTree_CN);
            if (tcl != null)
            {
                //  Gross BDFT
                tcl.GrossBDFTPP = justTreeLogs.Sum(j => j.GrossBoardFoot);
                //  Net BDFT
                tcl.NetBDFTPP = justTreeLogs.Sum(j => j.NetBoardFoot);
                //  Gross CUFT
                tcl.GrossCUFTPP = justTreeLogs.Sum(j => j.GrossCubicFoot);
                //  Net CUFT
                tcl.NetCUFTPP = justTreeLogs.Sum(j => j.NetCubicFoot);
                //  BDFT removed
                tcl.GrossBDFTRemvPP = justTreeLogs.Sum(j => j.BoardFootRemoved);
                //  CUFT removed
                tcl.GrossCUFTRemvPP = justTreeLogs.Sum(j => j.CubicFootRemoved);
                //  number of logs mainstem
                tcl.NumberlogsMS = justTreeLogs.Count();
            }
            else 
            {
                TreeCalculatedValuesDO tcv = new TreeCalculatedValuesDO();
                tcv.Tree_CN = currTree_CN;
                tcv.GrossBDFTPP = justTreeLogs.Sum(j => j.GrossBoardFoot);
                tcv.NetBDFTPP = justTreeLogs.Sum(j => j.NetBoardFoot);
                tcv.GrossCUFTPP = justTreeLogs.Sum(j => j.GrossCubicFoot);
                tcv.NetCUFTPP = justTreeLogs.Sum(j => j.NetCubicFoot);
                tcv.GrossBDFTRemvPP = justTreeLogs.Sum(j => j.BoardFootRemoved);
                tcv.GrossCUFTRemvPP = justTreeLogs.Sum(j => j.CubicFootRemoved);
                tcv.NumberlogsMS = justTreeLogs.Count();
                treeCalcList.Add(tcv);
            }   //  endif on nthRow

            LogStockDO ls;
            //  load logs into log stock table
            foreach (LogDO jtl in justTreeLogs)
            {
                ls = new LogStockDO();
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
                if (currRegion == "07" || currRegion == "7")
                {
                    switch (jtl.Grade)
                    {
                        case "9":
                            ls.BoardFootRemoved = 0;
                            ls.CubicFootRemoved = 0;
                            ls.NetBoardFoot = 0;
                            ls.NetCubicFoot = 0;
                            break;
                        case "7":       case "8":
                            ls.BoardFootRemoved = jtl.GrossBoardFoot;
                            ls.CubicFootRemoved = jtl.GrossCubicFoot;
                            ls.NetBoardFoot = 0;
                            ls.NetCubicFoot = 0;
                            break;
                        case "":        case " ":       case null:
                            ls.BoardFootRemoved = jtl.GrossBoardFoot;
                            ls.CubicFootRemoved = jtl.GrossCubicFoot;
                            // need a warning message about  blank log grade
                            Utilities.LogError("TreeCalculatedValues", (int) currTree_CN, "W", "This tree has a blank log grade or no product assigned.");
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
                logStockList.Add(ls);
            }   //  end foreach loop
            return;
        }   //  end CalcFallBuckScale


        private  void CalculateValue(IEnumerable<TreeDO> strataTrees, string currST, string currMethod, 
                                            string currRegion, IEnumerable<ValueEquationDO> valList,
                                            IEnumerable<TreeCalculatedValuesDO> tcvList)
        {
            //  process each tree
            foreach (TreeDO tdo in strataTrees)
            {
                //  find calculated values for current tree
                TreeCalculatedValuesDO tcv = tcvList.FirstOrDefault(tcvdo => tdo.Tree_CN == tcvdo.Tree_CN);

                //  Process according to equation number
                ValueEquationDO ve = valList.FirstOrDefault(vedo => vedo.Species == tdo.Species && vedo.PrimaryProduct == tdo.SampleGroup.PrimaryProduct);
                float sawValue = 0;
                float topValue = 0;
                if (ve != null)
                {
                    string equationNumber = ve.ValueEquationNumber.Substring(6,2);
                    if (currRegion == "04")
                    {
                        //  convert clearface to integer for use in calculation
                        int clearFace = Convert.ToInt16(tdo.ClearFace);
                        if (clearFace < 0 || clearFace > 9) clearFace = 0;

                        //  Calculate ratio
                        float bdftRatio = 0;
                        if (tcv.GrossBDFTPP > 0)
                            bdftRatio = tcv.NetBDFTPP / tcv.GrossBDFTPP;

                        switch (equationNumber)
                        {
                            case "01":
                            case "02":
                                clearFace++;
                                sawValue = (float)(ve.Coefficient1 *
                                    Math.Pow(tdo.DBH, ve.Coefficient2) *
                                    Math.Pow(tdo.TotalHeight, ve.Coefficient3) *
                                    Math.Pow(clearFace, ve.Coefficient4) * bdftRatio);
                                break;
                            case "03":
                            case "04":
                                clearFace++;
                                sawValue = (float)(ve.Coefficient1 *
                                    Math.Pow(tdo.DBH, ve.Coefficient2) *
                                    Math.Pow(tdo.TotalHeight, ve.Coefficient3) * bdftRatio);
                                break;
                        }   //  end switch                    
                    }
                    else
                    {
                        switch (equationNumber)
                        {
                            //  sawtimber check
                            case "01":
                                sawValue = (tcv.GrossBDFTPP * ve.Coefficient1) / 1000;
                                break;
                            case "02":
                                sawValue = (tcv.NetBDFTPP * ve.Coefficient1) / 1000;
                                break;
                            case "03":
                                sawValue = (tcv.GrossCUFTPP * ve.Coefficient1) / 100;
                                break;
                            case "04":
                                sawValue = (tcv.NetCUFTPP * ve.Coefficient1) / 100;
                                break;
                            case "05":
                                sawValue = tcv.CordsPP * ve.Coefficient1;
                                break;
                            case "06":
                                sawValue = (tcv.GrossBDFTPP * ve.Coefficient1) / 1000;
                                topValue = (tcv.GrossCUFTSP * ve.Coefficient2) / 100;
                                break;
                            case "07":
                                sawValue = (tcv.NetBDFTPP * ve.Coefficient1) / 1000;
                                topValue = (tcv.NetCUFTSP * ve.Coefficient2) / 100;
                                break;
                            case "08":
                                sawValue = (tcv.GrossBDFTPP * ve.Coefficient1) / 1000;
                                topValue = tcv.CordsSP * ve.Coefficient2;
                                break;
                            case "09":
                                sawValue = (tcv.NetBDFTPP * ve.Coefficient1) / 1000;
                                topValue = tcv.CordsSP * ve.Coefficient2;
                                break;
                            case "10":
                                sawValue = (tcv.GrossCUFTPP * ve.Coefficient1) / 100;
                                topValue = (tcv.GrossCUFTSP * ve.Coefficient2) / 100;
                                break;
                            case "11":
                                sawValue = (tcv.GrossCUFTPP * ve.Coefficient1) / 100;
                                topValue = tcv.CordsSP * ve.Coefficient2;
                                break;
                            case "12":
                                sawValue = (tcv.NetCUFTPP * ve.Coefficient1) / 100;
                                topValue = (tcv.NetCUFTSP * ve.Coefficient2) / 100;
                                break;
                            case "13":
                                sawValue = (tcv.NetCUFTPP * ve.Coefficient1) / 100;
                                topValue = tcv.CordsSP * ve.Coefficient2;
                                break;
                            case "14":
                                sawValue = tcv.CordsPP * ve.Coefficient1;
                                topValue = tcv.CordsSP * ve.Coefficient2;
                                break;
                            case "50":
                                sawValue = ve.Coefficient1 + (ve.Coefficient2 * tdo.DBH) +
                                    (ve.Coefficient3 * tdo.TotalHeight) +
                                    (ve.Coefficient4 * tdo.DBH * tdo.DBH) +
                                    (ve.Coefficient5 * tdo.DBH * tdo.DBH * tdo.TotalHeight) +
                                    (ve.Coefficient6 * tdo.DBH * tdo.DBH * tdo.DBH * tdo.TotalHeight);
                                break;
                        }   //  end switch
                    }   //  endif currRegion

                    //  save value
                    tcv.ValuePP = sawValue;
                    tcv.ValueSP = topValue;
                }   //  endif nthRow
            }   //  end foreach loop

            //Global.BL.fileName = fileName;
            Global.BL.SaveTreeCalculatedValues(tcvList);
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

            public MRules(int ev, float mxln, float mnln,float merln,float mtpp,float mtps,int op, float stmp,float trm)
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
