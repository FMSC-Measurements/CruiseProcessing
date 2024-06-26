﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL;
using CruiseProcessing.Services;
using iTextSharp.text;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CruiseProcessing.Data;

namespace CruiseProcessing
{
    public partial class ProcessStatus : Form
    {
        public CpDataLayer DataLayer { get; }
        public IDialogService DialogService { get; }

        protected IProgress<string> ProcessProgress { get; }

        protected ILogger Logger { get; }
        public IServiceProvider Services { get; }

        private void ProcessProgress_OnProgressChanged(string obj)
        {
            if (processingStatus.InvokeRequired)
            {
                processingStatus.Invoke(new Action(() => ProcessProgress_OnProgressChanged(obj)));
            }
            else
            {
                processingStatus.Text = obj;
            }

        }

        protected ProcessStatus()
        {
            InitializeComponent();

            ProcessProgress = new Progress<string>(ProcessProgress_OnProgressChanged);
        }

        public ProcessStatus(CpDataLayer dataLayer, IDialogService dialogService, ILogger<ProcessStatus> logger, IServiceProvider services)
    : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        private async void on_GO(object sender, EventArgs e)
        {

            if (!DoPreProcessChecks())
            {
                Close();
                return;
            }

            prepareCheck.Enabled = true;
            Cursor.Current = Cursors.WaitCursor;
            editCheck.Enabled = true;
            goButton.Enabled = false;

            try
            {
                await ProcessCoreAsync(ProcessProgress);

                volumeCheck.Enabled = true;
                //DialogResult = DialogResult.OK; // setting DialogResult will automaticly close form, also we don't really need to use dialogResult. 
            }
            catch (Exception ex) 
            {
                DialogService.ShowError("Processing Error: " + ex.GetType().Name);
                //DialogResult = DialogResult.Abort;
            }
            finally
            {
                Cursor.Current = this.Cursor;
            }
        }   //  end on_GO

        public bool DoPreProcessChecks()
        {
            //  check for errors from FScruiser before running edit checks
            //  generate an error report
            //  June 2013
            List<ErrorLogDO> fscList = DataLayer.getErrorMessages("E", "FScruiser");
            if (fscList.Any())
            {
                var messagesAndCounts = fscList.GroupBy(x => x.Message)
                    .Select(x => (Message: x.Key, Count: x.Count()));
                foreach (var i in messagesAndCounts)
                {
                    Logger.LogInformation("FScruiser Errors Found: {Message} Count:{Count}", i.Message, i.Count);
                }


                ErrorReport eRpt = new ErrorReport(DataLayer, DataLayer.GetReportHeaderData());
                var outputFileName = eRpt.PrintErrorReport(fscList, "FScruiser");
                string outputMessage = "ERRORS FROM FSCRUISER FOUND!\nCorrect data and rerun\nOutput file is:" + outputFileName;
                DialogService.ShowError(outputMessage);
                //  request made to open error report in preview -- May 2015
                DialogService.ShowPrintPreview();

                return false;
            }   //  endif report needed

            var allMeasureTrees = DataLayer.JustMeasuredTrees();

            //  March 2016 -- if the entire cruisde has no measured trees, that uis a critical erro
            //  and should stop the program.  Since no report can be generated, a message box appears to warn the user
            //  of the condition.
            if (allMeasureTrees.Count == 0)
            {
                Logger.LogInformation("No Measure Trees In Cruise");

                DialogService.ShowError("NO MEASURED TREES IN THIS CRUISE.\r\nCannot continue and cannot produce any reports.");
                return false;
            }

            List<StratumDO> sList = DataLayer.GetStrata();
            foreach (var st in sList)
            {
                var stTrees = DataLayer.GetTreesByStratum(st.Code);

                //  warn user if the stratum has no trees at all
                if (stTrees.Any() == false)
                {
                    Logger.LogInformation("Stratum Contains No Data: Code {StratumCode}", st.Code);
                    string warnMsg = "WARNING!  Stratum ";
                    warnMsg += st.Code;
                    warnMsg += " has no trees recorded.  Some reports may not be complete.\nContinue?";

                    if (!DialogService.ShowWarningAskYesNo(warnMsg))
                    { return false; }

                    Logger.LogInformation("Stratum Contains No Data, Continuing");
                }   //  endif no trees
            }


            processingStatus.Text = "READY TO BEGIN?  Click GO.";
            //Cursor.Current = Cursors.WaitCursor;
            //  perform edit checks -- 
            processingStatus.Text = "Edit checking the data.  Please wait.";
            processingStatus.Refresh();


            var errors = EditChecks.CheckErrors(DataLayer);
            if (errors.Any())
            {
                var messagesAndCounts = errors.GroupBy(x => x.Message)
                    .Select(x => (Message: x.Key, Count: x.Count()));
                foreach (var i in messagesAndCounts)
                {
                    Logger.LogInformation("EditCheck Errors Found: {Message} Count:{Count}", i.Message, i.Count);
                }

                DataLayer.SaveErrorMessages(errors);


                //  just check the ErrorLog table for entries
                if (errors.Any(x => x.Level == "E"))
                {
                    ErrorReport er = new ErrorReport(DataLayer, DataLayer.GetReportHeaderData());
                    var outputFileName = er.PrintErrorReport(errors, "CruiseProcessing");
                    string outputMessage = "ERRORS FOUND!\nCorrect data and rerun\nOutput file is:" + outputFileName;
                    DialogService.ShowError(outputMessage);
                    //  request made to open error report in preview -- May 2015
                    DialogService.ShowPrintPreview();

                    return false;
                }   //  endif report needed
            }

            return true;
        }


        protected Task ProcessCoreAsync(IProgress<string> progress)
        {
            return Task.Run(() => ProcessCore(progress));
        }

        public void ProcessCore(IProgress<string> progress)
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
                CalculatedValues calcVal = new CalculatedValues(DataLayer);
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

                    var calcTreeVal = Services.GetRequiredService<ICalculateTreeValues>();

                    ProcessStratum(sdo, calcTreeVal, calcVal, lcdList, popList, proList, ctList, pList, tList);

                }   //  end foreach stratum

                dal.CommitTransaction();

                DataLayer.IsProcessed = true;
                //  show volume calculation is finished
                progress?.Report("Processing is DONE");

            }
            catch(Exception ex)
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
            CalculatedValues calcVal,
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
                List<LCDDO> LCDstratum = LCDmethods.GetStratumGroupedBy(sdo.Code, DataLayer);

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
            SumAll.SumAllValues(DataLayer, sdo, pList, justCurrentLCD, justCurrentPOP, justCurrentPRO);

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
    }
}
