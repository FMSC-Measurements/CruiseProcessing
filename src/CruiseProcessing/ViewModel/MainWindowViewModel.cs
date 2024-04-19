using CruiseDAL;
using CruiseDAL.DataObjects;
using CruiseProcessing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

#nullable enable

namespace CruiseProcessing.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private ICommand _showVolumeEquationsCommand;
        private ICommand _showValueEquationsCommand;
        private ICommand _showR8VolumeEquationsCommand;
        private ICommand _showR9VolumeEquationsCommand;
        private ICommand _showModifyMerchRulesCommand;
        private ICommand _showModifyWeightFactorsCommand;
        private RelayCommand _addStandardReportsCommand;
        private ICommand _addGraphicalReportsCommand;
        private RelayCommand _createTextOutputFileCommand;
        private RelayCommand _createHtmlFileCommand;
        private RelayCommand _createPdfFileCommand;
        private RelayCommand _createCsvFileCommand;
        private ICommand _showPrintPreviewCommand;
        private RelayCommand _addLocalVolumeCommand;
        private ICommand _openFileCommand;
        private ICommand _processCommand;
        private string _title;
        private bool _isOutFileCreated;
        private ICommand _showAbout;

        public MainWindowViewModel(IServiceProvider services, DialogService dialogService, DataLayerContext dataserviceProvider, ILogger<MainWindowViewModel> logger)
        {
            Services = services;
            DataserviceProvider = dataserviceProvider;
            Logger = logger;
            DialogService = dialogService;

            
        }

        public CPbusinessLayer DataLayer => DataserviceProvider.DataLayer;

        public IServiceProvider Services { get; }
        public DataLayerContext DataserviceProvider { get; }
        public ILogger Logger { get; }
        public DialogService DialogService { get; }

        public string AppVerson { get; }




        public bool IsFileOpen => DataserviceProvider.DataLayer != null;
        public bool IsFileTemplate => DataserviceProvider?.DataLayer.IsTemplateFile ?? false;

        public bool IsCruiseFileOpen => IsFileOpen && !IsFileTemplate;

        public bool IsFileProcessed => IsCruiseFileOpen && DataLayer.IsProcessed;

        public bool IsOutFileCreated
        {
            get => _isOutFileCreated;
            set => SetProperty(ref _isOutFileCreated, value);
        }


        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ICommand OpenFileCommand => _openFileCommand ??= new RelayCommand(OpenFile);

        public ICommand ShowVolumeEquationsCommand => _showVolumeEquationsCommand ??= new RelayCommand(() => DialogService.ShowVolumeEquations(IsFileTemplate));
        public ICommand ShowValueEquationsCommand => _showValueEquationsCommand ??= new RelayCommand(DialogService.ShowValueEquations);
        public ICommand ShowR8VolumeEquationsCommand => _showR8VolumeEquationsCommand ??= new RelayCommand(DialogService.ShowR8VolumeEquations);
        public ICommand ShowR9VolumeEquationsCommand => _showR9VolumeEquationsCommand ??= new RelayCommand(DialogService.ShowR9VolumeEquations);
        public ICommand ShowModifyMerchRulesCommand => _showModifyMerchRulesCommand ??= new RelayCommand(DialogService.ShowModifyMerchRules);
        public ICommand ShowModifyWeightFactorsCommand => _showModifyWeightFactorsCommand ??= new RelayCommand(DialogService.ShowModifyWeightFactors);

        public ICommand AddStandardReportsCommand => _addStandardReportsCommand ??= new RelayCommand(AddStandardReports);
        public ICommand AddGraphicalReportsCommand => _addGraphicalReportsCommand ??= new RelayCommand(DialogService.ShowGraphicalReports);

        public ICommand ProcessCommand => _processCommand ??= new RelayCommand(ProcessCruise);

        public ICommand CreateTextOutputFileCommand => _createTextOutputFileCommand ??= new RelayCommand(CreateOuputFile);
        public ICommand CreateHtmlFileCommand => _createHtmlFileCommand ??= new RelayCommand(CreateHtmlFile);
        public ICommand CreatePdfFileCommand => _createPdfFileCommand ??= new RelayCommand(DialogService.ShowCreatePdf);
        public ICommand CreateCsvFileCommand => _createCsvFileCommand ??= new RelayCommand(DialogService.ShowCreateCsv);
        public ICommand ShowPrintPreviewCommand => _showPrintPreviewCommand ??= new RelayCommand(DialogService.ShowPrintPreview);
        public ICommand AddLocalVolumeCommand => _addLocalVolumeCommand ??= new RelayCommand(DialogService.ShowAddLocalVolumes);
        public ICommand ShowAbout => _showAbout ??= new RelayCommand(DialogService.ShowAbout);


        private void CreateHtmlFile()
        {

            HTMLoutput ho = new HTMLoutput(DataLayer, DialogService);
            ho.CreateHTMLfile();
        }

        public void RefreshIsProcessed()
        {
            //  See if volume has been calculated (sum expansion factor since those are calculated before volume)
            //  July 2014 -- However it looks like expansion factors could be present but volume is not
            //  need to pull calculated values as well and sum net volumes
            var tList = DataLayer.getTrees();
            double summedEF = tList.Sum(t => t.ExpansionFactor);
            var tcvList = DataLayer.getTreeCalculatedValues();
            double summedNetBDFT = tcvList.Sum(tc => tc.NetBDFTPP);
            double summedNetCUFT = tcvList.Sum(tc => tc.NetCUFTPP);

            DataLayer.IsProcessed = (summedEF > 0 || summedNetBDFT > 0 || summedNetCUFT > 0);
        }

        private void CreateOuputFile()
        {
            //  Pull reports selected


            RefreshIsProcessed();
            if (!IsFileProcessed)
            {
                DialogService.ShowInformation("Looks like volume has not been calculated.\nReports cannot be produced without calculated volume.\nPlease calculate volume before continuing.");
                return;
            }


            List<ReportsDO> selectedReports = DataLayer.GetSelectedReports();
            //  no reports?  let user know to go back and select reports
            if (selectedReports.Count == 0)
            {
                DialogService.ShowError("No reports selected.\nReturn to Reports section and select reports.");
                return;
            }

            //  Show dialog creating text file

            string? outFile = DialogService.ShowOutput(selectedReports);
            if(outFile != null)
            {
                DialogService.ShowInformation("Text output file is complete and can be found at:\n" + outFile);
                IsOutFileCreated = true;
            }
        }

        private void AddStandardReports()
        {
            //  calls routine to add standard and regional reports
            List<ReportsDO> currentReports = DataLayer.GetReports();
            //  and get the all reports array
            //  then check for various conditions to know what to do with the reports list
            if (currentReports.Count == 0)
            {
                currentReports = ReportsDataservice.GetDefaultReports();
                DataLayer.SaveReports(currentReports);


            }//end if
            else if (currentReports.Count < ReportsDataservice.reportsArray.GetLength(0))
            {
                //  old or new list?  Check title
                if (currentReports[0].Title == "" || currentReports[0].Title == null)
                {
                    //  old reports -- update list
                    currentReports = ReportMethods.updateReportsList(currentReports, ReportsDataservice.reportsArray);
                    DataLayer.SaveReports(currentReports);
                }
                else
                {
                    //  new reports -- just add
                    currentReports = ReportMethods.addReports(currentReports, ReportsDataservice.reportsArray);
                    DataLayer.SaveReports(currentReports);
                }   //  endif

            }   //  endif
                //  now get reports selected


            ReportMethods.deleteCSVReports(currentReports, DataLayer);
            currentReports = DataLayer.GetSelectedReports();
            //  Get selected reports 
            DialogService.ShowStandardReports(currentReports, IsFileTemplate);
        }




        private void ProcessCruise()
        {
            DialogService.ShowProcess();
            OnPropertyChanged(nameof(IsFileProcessed));
        }

        private void OpenFile()
        {
            var filePath = DialogService.AskOpenCruise();
            if(filePath != null)
            {
                OpenFile(filePath);
            }
        }

        public void OpenFile(string filePath)
        {
            if (!EnsurePathValid(filePath, Logger, DialogService)
                && !EnsurePathExistsAndCanWrite(filePath, Logger, DialogService))
            {
                return;
            }

            var fileName = Path.GetFileName(filePath);
            var extention = Path.GetExtension(filePath).ToLowerInvariant();

            var isTemplate = false;
            string cruiseID = null;
            DAL dal = null;
            CruiseDatastore_V3 dal_v3 = null;

            if (extention == ".cruise")
            {
                dal = new DAL(filePath);
            }
            else if (extention == ".crz3")
            {
                var processFilePath = Path.ChangeExtension(filePath, ".process");

                var v3db = new CruiseDatastore_V3(filePath);
                cruiseID = v3db.ExecuteScalar<string>("SELECT CruiseID FROM Cruise LIMIT 1");


                var downConverter = new DownMigrator();
                if (!downConverter.EnsureCanMigrate(cruiseID, v3db, out var error_message))
                {
                    var message = "Unable to open V3 Cruise File due to Design Checks \r\nMessages: " + error_message;
                    Logger.LogWarning(message);
                    DialogService.ShowError(message);
                    return;
                }

                try
                {
                    var v2db = new DAL(processFilePath, true);

                    downConverter.MigrateFromV3ToV2(cruiseID, v3db, v2db, "Cruise Processing");

                    dal = v2db;
                    dal_v3 = v3db;

                }
                catch (Exception ex)
                {
                    if (File.Exists(processFilePath))
                    { File.Delete(processFilePath); }

                    Logger.LogError(ex, "Error Migrating Cruise: {FileName}", fileName);
                    DialogService.ShowError("Error Translating V3 Cruise Data \r\nError: " + ex.ToString());
                    return;
                }

            }
            else if (extention == ".cut")
            {
                dal = new DAL(filePath);
                isTemplate = true;
            }
            else if (extention == ".crz3t")
            {
                var tempV2 = Path.ChangeExtension(filePath, ".cut.process");
                try
                {
                    var v3db = new CruiseDatastore_V3(filePath);
                    cruiseID = v3db.ExecuteScalar<string>("SELECT CruiseID FROM Cruise LIMIT 1");
                    var tempV2Db = new DAL(tempV2, true);
                    var downConverter = new DownMigrator();
                    downConverter.MigrateFromV3ToV2(cruiseID, v3db, tempV2Db, "Cruise Processing");

                    dal = tempV2Db;
                    dal_v3 = v3db;
                    isTemplate = true;

                }
                catch (Exception ex)
                {
                    if (File.Exists(tempV2))
                    { File.Delete(tempV2); }

                    Logger.LogError(ex, "Error Migrating Template: {FileName}", fileName);

                    return;
                }
            }
            else
            {
                Logger.LogWarning("Invalid File Type: {FileName}", fileName);
                DialogService.ShowError("Invalid File Type Selected");
                return;
            }

            //open connection forces the connection to remain open not to close and open.  Might be good to re-work the process button click?
            dal.OpenConnection();

            var datalayer = new CPbusinessLayer(dal, dal_v3, cruiseID);

            if (!isTemplate)
            {
                if (datalayer.saleWithNullSpecies())
                {
                    //One or more records contain incomplete data which affect processing.\n

                    Logger.LogWarning("File Has Trees With Null Species: {FileName}", fileName);
                    DialogService.ShowError("One or more records contain incomplete data which affect processing..\nPlease correct before using cruise processing.");
                    return;
                }
            }

            DataserviceProvider.DataLayer = datalayer;
            //  add file name to title line at top
            Title = (fileName.Length > 35) ? "..." + fileName.Substring(fileName.Length - 35, 35)
                    : fileName;

            OnPropertyChanged(nameof(DataLayer));
            OnPropertyChanged(nameof(IsCruiseFileOpen));
            OnPropertyChanged(nameof(IsFileOpen));
            OnPropertyChanged(nameof(IsFileTemplate));
        }

        public static bool EnsurePathValid(string path, ILogger logger, IDialogService dialogService)
        {
            try
            {
                path = Path.GetFullPath(path);

                // in net6.2 and later long paths are supported by default.
                // however it can still cause issue. So we need to manual check the
                // directory length
                // 
                var dirName = Path.GetDirectoryName(path);
                if (dirName.Length >= 248 || path.Length >= 260)
                {
                    throw new PathTooLongException("The supplied path is too long");
                }
            }
            catch (PathTooLongException ex)
            {
                var message = "File Path Too Long";
                logger.LogError(ex, message);
                dialogService.ShowError(message);
                return false;
            }
            catch (SecurityException ex)
            {
                var message = "Can Not Open File Due To File Permissions";
                logger.LogError(ex, message);
                dialogService.ShowError(message);
                return false;
            }
            catch (ArgumentException ex)
            {
                var message = (!string.IsNullOrEmpty(path) && path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                    ? "Path Contains Invalid Characters" : "Invalid File Path";
                logger.LogError(ex, message);
                dialogService.ShowError(message);
                return false;
            }

            return true;
        }

        public static bool EnsurePathExistsAndCanWrite(string path, ILogger logger, IDialogService dialogService)
        {
            if (!File.Exists(path))
            {
                var message = "Selected File Does Not Exist";
                logger.LogWarning(message);
                dialogService.ShowMessage(message, "Warning");
                return false;
            }

            if (File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly))
            {
                var message = "Selected File Is Read Only.\r\nIf opening file from non-local location, please copy file to a location on your PC before opening.";
                logger.LogWarning(message);
                dialogService.ShowMessage(message, "Warning");
                return false;
            }
            return true;
        }

        protected bool EnsureFileOpen()
        {
            if (IsFileOpen) { return true; }

            DialogService.ShowError("No file selected.  Cannot continue.\nPlease select a file.");
            return false;
        }

    }
}
