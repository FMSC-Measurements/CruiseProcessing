using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
using FMSC.ORM.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.ViewModel
{
    public partial class ProcessCruiseViewModel : ObservableObject
    {
        private bool? _isPrepareSuccess;
        private bool? _isProcessSuccess;
        private bool? _isEditCheckSuccess;
        private bool _isBusy;

        protected CpDataLayer DataLayer { get; }
        protected IDialogService DialogService { get; }
        protected IServiceProvider Services { get; }
        protected ILogger Log { get; }

        public IReadOnlyCollection<string> ProcessorNames { get; }

        public IProgress<string> ProcessProgress { get; }

        private string _processStatus;

        public string ProcessStatus
        {
            get => _processStatus;
            set => SetProperty(ref _processStatus, value);
        }

        public bool? IsEditCheckSuccess
        {
            get => _isEditCheckSuccess;
            protected set => SetProperty(ref _isEditCheckSuccess, value);
        }

        public bool? IsPrepareSuccess
        {
            get => _isPrepareSuccess;
            protected set => SetProperty(ref _isPrepareSuccess, value);
        }

        public bool? IsProcessSuccess
        {
            get => _isProcessSuccess;
            protected set => SetProperty(ref _isProcessSuccess, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            protected set => SetProperty(ref _isBusy, value);
        }

        public ProcessCruiseViewModel(CpDataLayer dataLayer, IDialogService dialogService, IServiceProvider service, ILogger<ProcessStatus> logger)
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            Services = service ?? throw new ArgumentNullException(nameof(service));
            Log = logger ?? throw new ArgumentNullException(nameof(logger));

            ProcessProgress = new Progress<string>(ProcessProgress_OnProgressChanged);
            ProcessorNames = ReferenceImplmentation.ReferenceImplimentationRegistry.CruiseProcessorImplementations.Select(x => x.Key).ToList();

            ProcessStatus = "Ready to begin? Click GO.";
        }

        private void ProcessProgress_OnProgressChanged(string obj)
        {
            ProcessStatus = obj;
        }

        [RelayCommand]
        public Task ProcessCruiseAsync()
        {
            Log?.LogInformation("Processing with Default Processor");
            var processor = Services.GetRequiredService<ICruiseProcessor>();
            return ProcessCruiseInternal(processor);
        }

        [RelayCommand]
        public Task ProcessCruiseWithAsync(string processorName)
        {
            if (ProcessorNames.Contains(processorName) == false)
            {
                throw new ArgumentException($"No processor with name {processorName}");
            }

            if(processorName == nameof(ReferenceImplmentation.RefCruiseProcessor))
            {
                DialogService.ShowMessage("Processing with Reference Processor. ");
            }

            Log?.LogInformation("Processing with {ProcessorName}", processorName);
            var processor = Services.GetRequiredKeyedService<ICruiseProcessor>(processorName);
            return ProcessCruiseInternal(processor);
        }

        protected async Task ProcessCruiseInternal(ICruiseProcessor processor)
        {
            IsEditCheckSuccess = null;
            IsPrepareSuccess = null;
            IsProcessSuccess = null;
            IsBusy = true;

            if (!DoPreProcessChecks())
            {
                IsEditCheckSuccess = false;
                return;
            }
            IsPrepareSuccess = true;
            IsEditCheckSuccess = true;

            try
            {
                await processor.ProcessCruiseAsync(ProcessProgress);
                IsProcessSuccess = true;
            }
            catch (Exception e)
            {
                IsProcessSuccess = false;
                var message = "Processing Error: " + e.GetType().Name;
                Log?.LogError(e, message);
                DialogService?.ShowError(message);
            }
            IsBusy = false;
        }

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
                    Log?.LogInformation("FScruiser Errors Found: {Message} Count:{Count}", i.Message, i.Count);
                }

                ErrorReport eRpt = new ErrorReport(DataLayer, DataLayer.GetReportHeaderData());
                var outputFileName = eRpt.PrintErrorReport(fscList, "FScruiser");
                string outputMessage = "ERRORS FROM FSCRUISER FOUND!\nCorrect data and rerun\nOutput file is:" + outputFileName;
                DialogService?.ShowError(outputMessage);
                //  request made to open error report in preview -- May 2015
                DialogService?.ShowPrintPreview();

                return false;
            }   //  endif report needed

            var allMeasureTrees = DataLayer.JustMeasuredTrees();

            //  March 2016 -- if the entire cruisde has no measured trees, that uis a critical erro
            //  and should stop the program.  Since no report can be generated, a message box appears to warn the user
            //  of the condition.
            if (allMeasureTrees.Count == 0)
            {
                Log?.LogInformation("No Measure Trees In Cruise");

                DialogService?.ShowError("NO MEASURED TREES IN THIS CRUISE.\r\nCannot continue and cannot produce any reports.");
                return false;
            }

            List<StratumDO> sList = DataLayer.GetStrata();
            foreach (var st in sList)
            {
                var stTrees = DataLayer.GetTreesByStratum(st.Code);

                //  warn user if the stratum has no trees at all
                if (stTrees.Any() == false)
                {
                    Log?.LogInformation("Stratum Contains No Data: Code {StratumCode}", st.Code);
                    string warnMsg = "WARNING!  Stratum ";
                    warnMsg += st.Code;
                    warnMsg += " has no trees recorded.  Some reports may not be complete.\nContinue?";

                    if (!(DialogService?.ShowWarningAskYesNo(warnMsg) ?? true))
                    { return false; }

                    Log?.LogInformation("Stratum Contains No Data, Continuing");
                }   //  endif no trees
            }

            //Cursor.Current = Cursors.WaitCursor;
            //  perform edit checks --
            ProcessStatus = "Edit checking the data.  Please wait.";

            var errors = EditChecks.CheckErrors(DataLayer);
            if (errors.Any())
            {
                var messagesAndCounts = errors.GroupBy(x => x.Message)
                    .Select(x => (Message: x.Key, Count: x.Count()));
                foreach (var i in messagesAndCounts)
                {
                    Log?.LogInformation("EditCheck Errors Found: {Message} Count:{Count}", i.Message, i.Count);
                }

                DataLayer.SaveErrorMessages(errors);

                //  just check the ErrorLog table for entries
                if (errors.Any(x => x.Level == "E"))
                {
                    ErrorReport er = new ErrorReport(DataLayer, DataLayer.GetReportHeaderData());
                    var outputFileName = er.PrintErrorReport(errors, "CruiseProcessing");
                    string outputMessage = "ERRORS FOUND!\nCorrect data and rerun\nOutput file is:" + outputFileName;
                    DialogService?.ShowError(outputMessage);
                    //  request made to open error report in preview -- May 2015
                    DialogService?.ShowPrintPreview();

                    return false;
                }   //  endif report needed
            }

            return true;
        }
    }
}