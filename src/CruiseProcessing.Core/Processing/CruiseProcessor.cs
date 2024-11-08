using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CruiseProcessing.Processing
{
    public class CruiseProcessor : ICruiseProcessor
    {
        public CpDataLayer DataLayer { get; protected set; }
        public IDialogService DialogService { get; protected set; }
        public ILogger Logger { get; protected set; }
        public ICalculateTreeValues TreeValCalculator { get; protected set; }

        protected CruiseProcessor()
        {
        }

        public CruiseProcessor(CpDataLayer dataLayer, IDialogService dialogService, [FromKeyedServices(nameof(CalculateTreeValues2))] ICalculateTreeValues calculateTreeValues, ILogger<CruiseProcessor> logger)
        {
            DataLayer = dataLayer;
            DialogService = dialogService;
            Logger = logger;
            TreeValCalculator = calculateTreeValues;
        }


        public Task ProcessCruiseAsync(IProgress<string> progress)
        {
            return Task.Run(() => ProcessCruise(progress));
        }

        public void ProcessCruise(IProgress<string> progress)
        {
            DataLayer.ResetBioMassEquationCache();

            //  before making IDs, need to check for blank or null secondary products in sample groups
            //  if blank, default to 02 for every region but 6 where it will be 08 instead
            //  put a warning message in the error log table indicating the secondary product was set to a default
            //  June 2013
            DefaultSecondaryProduct();

            //  next show preparation of data
            progress?.Report("Preparing data for processing.");
            Logger?.LogInformation("Start Processing");
            var dal = DataLayer.DAL;
            dal.BeginTransaction();
            try
            {
                CalculatedValues calcVal = new CalculatedValues(DataLayer);
                calcVal.CalcValues();

                //  show preparation of data is complete
                progress?.Report("Preparation of data complete");
                //  now loop through strata and show status message updating for each stratum
                List<StratumDO> sList = DataLayer.GetStrata();

                foreach (StratumDO sdo in sList)
                {
                    //  update status message for next stratum
                    progress?.Report("Processing stratum " + sdo.Code);
                    ProcessStratum(sdo, TreeValCalculator, calcVal);
                }

                //DataLayer.WriteGlobalValue(CpDataLayer.GLOBAL_KEY_TREEVALUECALCULATOR_TYPE, TreeValCalculator.GetType().Name);
                //DataLayer.WriteGlobalValue(CpDataLayer.GLOBAL_KEY_VOLUMELIBRARY_TYPE, TreeValCalculator.VolLib.GetType().Name);
                //DataLayer.WriteGlobalValue(CpDataLayer.GLOBAL_KEY_VOLUMELIBRARY_VERSION, TreeValCalculator.VolLib.GetVersionNumber().ToString());
                DataLayer.VolLibVersion = TreeValCalculator.GetVersion();


                dal.CommitTransaction();

                DataLayer.IsProcessed = true;
                //  show volume calculation is finished
                progress?.Report("Processing is DONE");
                Logger?.LogInformation("Processing is DONE");

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
            StratumDO stratum,
            ICalculateTreeValues calcTreeVal,
            CalculatedValues calcVal)
        {
            Logger?.LogInformation("Processing stratum {StratumCode}", stratum.Code);

            List<TreeDO> stratumTrees = DataLayer.GetTreesByStratum(stratum.Code).ToList();
            List<PlotDO> stratumPlots = DataLayer.GetPlotsByStratum(stratum.Code);
            //  need cut and leave trees for this
            List<LCDDO> stratumLcds = DataLayer.GetLcds(stratum.Code);
            List<POPDO> stratumPops = DataLayer.GetPopsByStratum(stratum.Code);
            List<PRODO> stratumPros = DataLayer.GetProsByStratum(stratum.Code);
            List<CountTreeDO> stratumCountTrees = DataLayer.GetCountTreesByStratum(stratum.Code);

            calcVal.SumTreeCounts(stratum, stratumPlots, stratumCountTrees, stratumLcds, stratumPops, stratumPros);

            DataLayer.SavePRO(stratumPros);
            DataLayer.SaveLCD(stratumLcds);
            DataLayer.SavePOP(stratumPops);

            //  Calculate expansion factor
            calcVal.CalcExpFac(stratum, stratumPlots, stratumPops);

            //  Calculate volumes
            calcTreeVal.ProcessTrees(stratum.Code, stratum.Method, (long)stratum.Stratum_CN);


            if (stratum.Method == "3P")
            {
                Update3Ptally(stratumCountTrees, stratumLcds, stratumTrees);
                DataLayer.SaveLCD(stratumLcds);
            }


            //  Update expansion factors for methods 3PPNT, F3P, and P3P
            if (stratum.Method == "3PPNT" || stratum.Method == "F3P" || stratum.Method == "P3P")
            {
                List<TreeCalculatedValuesDO> tcvList = DataLayer.getTreeCalculatedValues((int)stratum.Stratum_CN);
                UpdateExpansionFactors(stratumTrees, tcvList);
                DataLayer.SaveTrees(stratumTrees);
            }

            //  Sum data for the LCD, POP and PRO table
            SumAll.SumAllValues(DataLayer, stratum, stratumPlots, stratumLcds, stratumPops, stratumPros);

            //  Update STR tally after expansion factors are summed
            if (stratum.Method == "STR")
            {
                UpdateStrTalliedTrees(stratumLcds);
                DataLayer.SaveLCD(stratumLcds);
            }

            Logger?.LogInformation("Processing Done: {StratumCode}", stratum.Code);
        }

        private static void Update3Ptally(List<CountTreeDO> stratumCountTrees, List<LCDDO> stratumLcds,
                            List<TreeDO> stratumTrees)
        {
            //  Need LCD for 3p stratum ordered by sample group, species (and STM)
            //List<LCDDO> LCDstratum = LCDmethods.GetStratumGroupedBy(fileName, currST, bslyr);
            foreach (var lcdGroup in stratumLcds.GroupBy(x => (x.SampleGroup, x.Species, x.STM)).ToArray())
            {
                var stm = lcdGroup.Key.STM;
                var sampleGroup = lcdGroup.Key.SampleGroup;
                var species = lcdGroup.Key.Species;

                //  get LCDs by sg, species, and STM
                //List<LCDDO> lcdGroup = stratumLcds.FindAll(
                //    delegate (LCDDO ld)
                //    {
                //        return l.SampleGroup == ld.SampleGroup && l.Species == ld.Species && l.STM == ld.STM;
                //    });
                double totalExpansionFactor = lcdGroup.Sum(ldo => ldo.SumExpanFactor);
                var totalCount = lcdGroup.Sum(lcdo => lcdo.MeasuredTrees);
                if (stm == "N")
                {
                    totalCount += stratumCountTrees.Where(ct => ct.SampleGroup.Code == sampleGroup && ct.TreeDefaultValue?.Species == species)
                        .Sum(ct => ct.TreeCount);
                }

                // TODO : this is probably a bug, the variable implies trees should be just insurance tree but
                //  the selector doesn't check countMeasure, however since TreeCount is always 0 this bug isn't seen in the output
                // also since insurance tree counts are stored in the tree count table this is probably not needed
                if (stm == "N")
                {
                    //  Add any insurance trees to count
                    List<TreeDO> insurTrees = stratumTrees.FindAll(
                        delegate (TreeDO td)
                        {
                            return td.SampleGroup.Code == sampleGroup &&
                                    td.Species == species && td.STM == stm;
                        });

                    totalCount += insurTrees.Sum(t => t.TreeCount);
                }

                //  Recalc tallied trees for this group
                foreach (LCDDO ldo in lcdGroup)
                    if (totalExpansionFactor > 0)
                        ldo.TalliedTrees = ldo.SumExpanFactor / totalExpansionFactor * totalCount;
            }   //  end foreach loop

        }

        //  August 2016 -- need to replace tally for STR method when
        //  multiple species in a sample group were tallied by sample group and not
        //  by species.  Replace with sum of expansion factor
        //
        // Aug 2024 -- it appears this method was trying to do what was stated above at one point
        // i.e. using count tree records to enumerate sample groups in the stratum and then
        // updating all lcds in that sample group so that lcd.TalliedTrees = lcd.SumExpanFactor
        // if it was tallied by sample group.  However, the code was incredibly condoluded and
        // had been changed to update all lcds in the stratum so that lcd.TalliedTrees = lcd.SumExpanFactor
        // so I got rid of all the jumbed code.
        //
        // Note : the original versioon of this method used the count tree records to enumerate sample groups
        // so if there were no count tree records in the cruise, it wouldn't do anything. This shoudn't be possible
        // but it could cause different results if all counts are stored on trees. 
        private static void UpdateStrTalliedTrees(List<LCDDO> stratumLcds)
        {
            foreach (var lcd in stratumLcds)
            {
                lcd.TalliedTrees = lcd.SumExpanFactor;
            }
        }

        private static void UpdateExpansionFactors(List<TreeDO> justCurrST, List<TreeCalculatedValuesDO> tcvList)
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
            }
        }

        private void DefaultSecondaryProduct()
        {
            ErrorLogCollection errors = new ErrorLogCollection();
            var currRegion = DataLayer.getRegion();

            List<SampleGroupDO> sgList = DataLayer.getSampleGroups();

            foreach (SampleGroupDO sgd in sgList)
            {
                if (string.IsNullOrWhiteSpace(sgd.SecondaryProduct))
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
    }
}
