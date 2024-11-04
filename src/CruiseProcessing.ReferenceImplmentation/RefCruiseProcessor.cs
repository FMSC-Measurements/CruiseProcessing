using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.ReferenceImplmentation
{
    public class RefCruiseProcessor : ICruiseProcessor
    {
        protected ILogger<RefCalculateTreeValues> _ctvLogger;
        public CpDataLayer DataLayer { get; }
        public ILogger Logger { get; }
        

        public IServiceProvider Services { get; }

        public RefCruiseProcessor(CpDataLayer dataLayer, ILogger<RefCruiseProcessor> logger, ILogger<RefCalculateTreeValues> ctvLogger)
        {
            DataLayer = dataLayer;
            Logger = logger;
            _ctvLogger = ctvLogger;
        }

        public Task ProcessCruiseAsync(IProgress<string> progress)
        {
            return Task.Run(() => ProcessCruise(progress));
        }

        public void ProcessCruise(IProgress<string> progress)
        {
            //  before making IDs, need to check for blank or null secondary products in sample groups
            //  if blank, default to 02 for every region but 6 where it will be 08 instead
            //  put a warning message in the error log table indicating the secondary product was set to a default
            //  June 2013
            DefaultSecondaryProduct();

            //  next show preparation of data
            progress?.Report("Preparing data for processing.");
            var dal = DataLayer.DAL;
            dal.BeginTransaction();
            try
            {
                var calcVal = new RefCalculatedValues(DataLayer);
                calcVal.CalcValues();

                //  now need some other tables to start summing values
                List<LCDDO> lcdList = DataLayer.getLCD();
                List<POPDO> popList = DataLayer.getPOP();
                List<PRODO> proList = DataLayer.getPRO();

                List<CountTreeDO> ctList = DataLayer.getCountTrees();
                List<PlotDO> pList = DataLayer.getPlots();
                List<TreeDO> tList = DataLayer.getTrees();

                //  show preparation of data is complete
                progress?.Report("Preparation of data complete");
                //  now loop through strata and show status message updating for each stratum
                List<StratumDO> sList = DataLayer.GetStrata();
                foreach (StratumDO sdo in sList)
                {

                    //  update status message for next stratum
                    progress?.Report("Calculating stratum " + sdo.Code);

                    var calcTreeVal = new RefCalculateTreeValues(DataLayer, _ctvLogger);

                    ProcessStratum(sdo, calcTreeVal, calcVal, lcdList, popList, proList, ctList, pList, tList);

                }   //  end foreach stratum

                dal.CommitTransaction();

                DataLayer.IsProcessed = true;
                //  show volume calculation is finished
                progress?.Report("Processing is DONE");

            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Logger.LogError(ex, "Processing Error: {FilePath}", dal.Path);
                DataLayer.IsProcessed = false;

                //  show volume calculation is finished
                progress.Report("Processing Error");
                throw;
            }


        }


        private void ProcessStratum(
            StratumDO sdo,
            ICalculateTreeValues calcTreeVal,
            RefCalculatedValues calcVal,
            List<LCDDO> lcdList,
            List<POPDO> popList,
            List<PRODO> proList,
            List<CountTreeDO> ctList,
            List<PlotDO> pList,
            List<TreeDO> tList)
        {
            //  Sum counts and KPI for LCD table
            List<PlotDO> justPlots = PlotMethods.GetStrata(pList, sdo.Code);
            //  need cut and leave trees for this
            List<LCDDO> justCurrentLCD = LCDmethods.GetStratum(lcdList, sdo.Code);
            calcVal.SumTreeCountsLCD(sdo.Code, ctList, justPlots, justCurrentLCD, sdo.Method, lcdList);

            //  Sum counts and KPI for POP table
            List<POPDO> justCurrentPOP = POPmethods.GetStratumData(popList, sdo.Code, "");
            calcVal.SumTreeCountsPOP(sdo.Code, ctList, justPlots, justCurrentPOP, sdo.Method, popList);

            //  Sum counts and KPI for PRO table
            List<PRODO> justCurrentPRO = PROmethods.GetCutTrees(proList, sdo.Code, "", "", 0);
            calcVal.SumTreeCountsPRO(sdo.Code, ctList, justPlots, justCurrentPRO, sdo.Method, proList);

            //  Calculate expansion factor
            calcVal.CalcExpFac(sdo, justPlots, justCurrentPOP);

            //  Calculate volumes
            calcTreeVal.ProcessTrees(sdo.Code, sdo.Method, (long)sdo.Stratum_CN);

            //  Update 3P tally
            if (sdo.Method == "3P")
            {
                List<LCDDO> LCDstratum = GetStratumGroupedBy(sdo.Code, DataLayer);

                Update3Ptally(ctList, justCurrentLCD, tList, LCDstratum);
                //  Save 
                DataLayer.SaveLCD(justCurrentLCD);
            }   //  endif method is 3P


            //  Update expansion factors for methods 3PPNT, F3P, and P3P
            if (sdo.Method == "3PPNT" || sdo.Method == "F3P" || sdo.Method == "P3P")
            {
                List<TreeDO> justCurrentStratum = tList.FindAll(
                    delegate (TreeDO td)
                    {
                        return td.Stratum.Code == sdo.Code;
                    });
                List<TreeCalculatedValuesDO> tcvList = DataLayer.getTreeCalculatedValues((int)sdo.Stratum_CN);
                UpdateExpansionFactors(justCurrentStratum, tcvList);
                //  Save update
                DataLayer.SaveTrees(justCurrentStratum);
            }   //  endif on method

            //  Sum data for the LCD, POP and PRO table
            SumAll.SumAllValues(DataLayer, sdo, justPlots, justCurrentLCD, justCurrentPOP, justCurrentPRO);

            //  Update STR tally after expansion factors are summed
            if (sdo.Method == "STR")
            {

                UpdateSTRtally(sdo.Code, justCurrentLCD, ctList, lcdList);
                //  save
                DataLayer.SaveLCD(lcdList);
            }   //  endif method is STR
        }

        private void Update3Ptally(List<CountTreeDO> ctList, List<LCDDO> justCurrentLCD,
                                    List<TreeDO> tList, List<LCDDO> LCDstratum)
        {
            double TotalEF = 0;
            double TotalMeas = 0;
            double TotalCount = 0;

            //  Need LCD for 3p stratum ordered by sample group, species (and STM)
            //List<LCDDO> LCDstratum = LCDmethods.GetStratumGroupedBy(fileName, currST, bslyr);
            foreach (LCDDO l in LCDstratum)
            {
                //  find each group in just current LCD
                List<LCDDO> currentGroup = justCurrentLCD.FindAll(
                    delegate (LCDDO ld)
                    {
                        return l.SampleGroup == ld.SampleGroup && l.Species == ld.Species && l.STM == ld.STM;
                    });
                TotalEF = currentGroup.Sum(ldo => ldo.SumExpanFactor);

                if (l.STM == "N")
                {
                    //  Find counts
                    List<CountTreeDO> justCounts = ctList.FindAll(
                        delegate (CountTreeDO ctdo)
                        {
                            if (ctdo.TreeDefaultValue != null)
                                return ctdo.SampleGroup.Code == l.SampleGroup &&
                                     ctdo.TreeDefaultValue.Species == l.Species;
                            else return false;
                        });
                    if (justCounts.Count > 0)
                    {
                        TotalCount = justCounts.Sum(ct => ct.TreeCount);
                        TotalMeas = currentGroup.Sum(lcddo => lcddo.MeasuredTrees);
                        TotalCount += TotalMeas;
                    }   //  endif justCounts is empty
                }
                else if (l.STM == "Y") TotalCount = currentGroup.Sum(lcdo => lcdo.MeasuredTrees);

                //  Add any insurance trees to count
                List<TreeDO> insurTrees = tList.FindAll(
                    delegate (TreeDO td)
                    {
                        return td.Stratum.Code == l.Stratum && td.SampleGroup.Code == l.SampleGroup &&
                                td.Species == l.Species && td.STM == l.STM;
                    });
                if (l.STM == "N")
                    TotalCount += insurTrees.Sum(t => t.TreeCount);

                //  Recalc tallied trees for this group
                foreach (LCDDO ldo in currentGroup)
                    if (TotalEF > 0)
                        ldo.TalliedTrees = ldo.SumExpanFactor / TotalEF * TotalCount;
            }   //  end foreach loop

            return;
        }   //  end Update3Ptally


        //  August 2016 -- need to replace tally for STR method when
        //  multiple species in a sample group were tallied by sample group and not
        //  by species.  Replace with sum of expansion factor
        private void UpdateSTRtally(string currST, List<LCDDO> justCurrentLCD, List<CountTreeDO> ctList,
                                    List<LCDDO> lcdList)
        {
            //Possibly update this from Sample group instead of strata.  See templeton.
            //  pull count records for STR stratum
            List<CountTreeDO> currentGroups = ctList.FindAll(
                delegate (CountTreeDO cg)
                {
                    return cg.SampleGroup.Stratum.Code == currST;
                });


            string sampleGroupCN = "";
            //  see if TreeDefaultValue_CN is zero indicating BySampleGroup
            int totalTDV = 0;
            foreach (CountTreeDO cg in currentGroups)
            {
                //if treeDefaultValue_cn is not null tally by species
                //else tally by sample group.   
                //if (cg.TreeDefaultValue_CN != null)
                //{
                //regardless of sample group tally or species tally correct the tally.
                string strataCode = "";

                string sampleGroupCode = "";

                if (cg.SampleGroup != null)
                {
                    sampleGroupCode = cg.SampleGroup.Code;
                    if (cg.SampleGroup.Stratum != null)
                    {
                        strataCode = cg.SampleGroup.Stratum.Code;
                    }//end if
                }//end if


                //  replace tally with sum of expansion factor
                foreach (LCDDO jg in justCurrentLCD)
                {
                    if (jg.SampleGroup == sampleGroupCode && jg.Stratum == strataCode)
                    {
                        //  find in original to update
                        int nthRow = lcdList.FindIndex(
                        delegate (LCDDO ll)
                        {
                            return ll.LCD_CN == jg.LCD_CN;
                        });

                        if (nthRow >= 0)
                        {
                            lcdList[nthRow].TalliedTrees = lcdList[nthRow].SumExpanFactor;
                        }//end if
                    }//end if
                }   //  end foreach loop

                totalTDV += 0;


                //}//end if
                //else
                //{
                //    //Don't apply if count tree has a CN (meaning its selected coby species)
                //    //totalTDV++;
                //}//end else

            }//  end foreach loop

            ////  what was sampling method
            //if (totalTDV == 0.0)
            //{

            //}   //  endif

            return;
        }  //  end UpdateSTRtally



        private void UpdateExpansionFactors(List<TreeDO> justCurrST, List<TreeCalculatedValuesDO> tcvList)
        {
            //  find tree calculated volume to see if expansion factor needs to be reset
            foreach (TreeDO t in justCurrST)
            {
                int nthRow = tcvList.FindIndex(
                    delegate (TreeCalculatedValuesDO tcv)
                    {
                        return tcv.Tree_CN == t.Tree_CN && tcv.GrossBDFTPP == 0 && tcv.GrossCUFTPP == 0;
                    });
                if (nthRow >= 0)
                    t.ExpansionFactor = 0;
            }   //  end foreach loop

            return;
        }   //  end UpdateExpansionFactors

        private void DefaultSecondaryProduct()
        {
            ErrorLogCollection errors = new ErrorLogCollection();
            var currRegion = DataLayer.getRegion();

            List<SampleGroupDO> sgList = DataLayer.getSampleGroups();

            foreach (SampleGroupDO sgd in sgList)
            {
                if (String.IsNullOrWhiteSpace(sgd.SecondaryProduct))
                {
                    switch (currRegion)
                    {
                        case "06":
                        case "6":
                            sgd.SecondaryProduct = "08";
                            errors.AddError("SampleGroup", "W", "19", (long)sgd.SampleGroup_CN, "SecondaryProduct");
                            break;
                        case "05":
                        case "5":
                            sgd.SecondaryProduct = "20";
                            errors.AddError("SampleGroup", "W", "19", (long)sgd.SampleGroup_CN, "SecondaryProduct");
                            break;
                        default:
                            sgd.SecondaryProduct = "02";
                            errors.AddError("SampleGroup", "W", "19", (long)sgd.SampleGroup_CN, "SecondaryProduct");
                            break;
                    }   //  end switch
                }   //  endif
            }   //  end foreach loop
            DataLayer.SaveSampleGroups(sgList);
            if (errors.Any())
            {
                DataLayer.SaveErrorMessages(errors);
            }
        }

        public static List<LCDDO> GetStratumGroupedBy(string currentST, CpDataLayer dataLayer)
        {
            List<LCDDO> justStratum = new List<LCDDO>();
            justStratum = dataLayer.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2  GROUP BY ", "SampleGroup,Species,STM", "C", currentST);
            return justStratum;
        }   //  end GetStratumGroupedBy
    }
}
