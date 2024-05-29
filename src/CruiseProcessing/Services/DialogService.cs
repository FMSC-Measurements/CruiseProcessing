using CruiseDAL.DataObjects;
using CruiseProcessing.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;


//using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
//using MessageBox = System.Windows.MessageBox;
//using MessageBoxButton = System.Windows.MessageBoxButton;
//using MessageBoxImage = System.Windows.MessageBoxImage;

using DialogResult = System.Windows.Forms.DialogResult;
using Form = System.Windows.Forms.Form;

#nullable enable

namespace CruiseProcessing.Services
{
    public class DialogService : IDialogService
    {
        private WindowInteropHelper _windowInteropHelper;

        public DialogService(IServiceProvider services, App app)
        {
            // = mainMenu;
            App = app;
            Services = services;
        }

        public App App { get; }

        public MainWindow? Window => App.MainWindow as MainWindow;

        protected WindowInteropHelper WindowInteropHelper => _windowInteropHelper ??= new WindowInteropHelper(Window);

        //private MainMenu MainMenu { get; }
        public IServiceProvider Services { get; }

        public string? AskOpenCruise()
        {
            //  Create an instance of the open file dialog
            OpenFileDialog browseDialog = new OpenFileDialog();

            //  Set filter options and filter index
            browseDialog.Filter = "Cruise Files|*.cruise;*.crz3|Template Files|*.cut;*.crz3t|All Files|*.*";
            browseDialog.FilterIndex = 1;
            browseDialog.Multiselect = false;

            var dialogResult = browseDialog.ShowDialog(Window) ?? false;

            return (dialogResult) ? browseDialog.FileName : null;
        }

        public void ShowError(string message)
        {
            MessageBox.Show(Window, message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowWarning(string message)
        {
            MessageBox.Show(Window, message, "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public bool ShowWarningAskYesNo(string message)
        {
            var result = MessageBox.Show(Window, message, "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == System.Windows.MessageBoxResult.Yes;
        }

        public void ShowGraphOutputDialog(IEnumerable<string> graphReports)
        {
            var dialog = Services.GetRequiredService<graphOutputDialog>();
            dialog.ShowDialog();
        }

        public void ShowInformation(string message)
        {
            MessageBox.Show(Window, message, "INFORMATION", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowMessage(string message, string caption = null)
        {
            MessageBox.Show(Window, message, caption);
        }


        protected void SetWindowInteropHelper(Form form)
        {
            WindowInteropHelper helper = new WindowInteropHelper(Window);
            helper.Owner = form.Handle;
        }

        public void ShowWinForm(Form form)
        {
            WindowInteropHelper helper = new WindowInteropHelper(Window);
            helper.Owner = form.Handle;
            form.ShowDialog();

        }

        public void ShowAbout()
        {
            ShowInformation("CruiseProcessing Version " + App.AppVersion.ToString(3) 
                + "\r\nForest Management Service Center\nFort Collins, Colorado");
        }

        public IEnumerable<StewProductCosts> GetStewardshipProductCosts()
        {
            var dialog = Services.GetRequiredService<StewardshipProductCosts>();
            dialog.ShowDialog();
            return dialog.StewList;
        }

        public void ShowPrintPreview()
        {
            var dialog = Services.GetRequiredService<PrintPreview>();
            dialog.setupDialog();
            dialog.ShowDialog();
        }

        public void ShowStandardReports(List<ReportsDO> reports, bool isTemplate)
        {
            ReportsDialog rd = Services.GetRequiredService<ReportsDialog>();


            rd.reportList = reports;
            rd.templateFlag = (isTemplate) ? 1 : 0;
            rd.setupDialog();
            rd.ShowDialog();
        }


        public void ShowGraphicalReports()
        {
            GraphReportsDialog grd = Services.GetRequiredService<GraphReportsDialog>();
            grd.setupDialog();
            grd.ShowDialog();
        }

        public string? ShowOutput(List<ReportsDO> reports)
        {
            TextFileOutput tfo = Services.GetRequiredService<TextFileOutput>();
            tfo.selectedReports = reports;
            tfo.setupDialog();
            tfo.ShowDialog();

            if(tfo.retrnState == 0)
            {
                return tfo.outFile;
            }
            return null;
        }

        public bool ShowProcess()
        {
            ProcessStatus statusDlg = Services.GetRequiredService<ProcessStatus>();
            statusDlg.ShowDialog();

            return statusDlg.DialogResult == DialogResult.OK;
        }

        public void ShowVolumeEquations(bool isTemplateFile)
        {
            VolumeEquations volEqObj = Services.GetRequiredService<VolumeEquations>();

            if (isTemplateFile)
            {
                int nResult = volEqObj.setupTemplateDialog();
                if (nResult == 1)
                {
                    volEqObj.templateFlag = 1;
                    volEqObj.ShowDialog();
                }   //  endif
            }
            else
            {
                int nResult = volEqObj.setupDialog();
                if (nResult != -1)
                {
                    volEqObj.ShowDialog();
                }
            }
        }

        public void ShowR8VolumeEquations()
        {
            R8VolEquation r8vol = Services.GetRequiredService<R8VolEquation>();
            r8vol.ShowDialog();
        }

        public void ShowR9VolumeEquations()
        {
            R9VolEquation r9vol = Services.GetRequiredService<R9VolEquation>();
            r9vol.setupDialog();
            r9vol.ShowDialog();
        }

        public void ShowValueEquations()
        {
            ValueEquations valEqObj = Services.GetRequiredService<ValueEquations>();

            int nResult = valEqObj.setupDialog();
            if (nResult == 1)
                valEqObj.ShowDialog();
        }

        public void ShowCreatePdf()
        {
            PDFfileOutput pfo = Services.GetRequiredService<PDFfileOutput>();
            int nResult = pfo.setupDialog();
            if (nResult == 0)
                pfo.ShowDialog();
        }

        public void ShowCreateCsv()
        {
            SelectCSV sc = Services.GetRequiredService<SelectCSV>();
            sc.setupDialog();
            sc.ShowDialog();
        }

        public void ShowAddLocalVolumes()
        {
            LocalVolume lv = Services.GetRequiredService<LocalVolume>();
            lv.setupDialog();
            lv.ShowDialog();
        }

        public void ShowModifyMerchRules()
        {
            ModifyMerchRules mmr = Services.GetRequiredService<ModifyMerchRules>();
            mmr.setupDialog();
            mmr.ShowDialog();
        }

        public void ShowModifyWeightFactors()
        {
            ModifyWeightFactors mwf = Services.GetRequiredService<ModifyWeightFactors>();
            var mResult = mwf.setupDialog();
            if (mResult == 1) mwf.ShowDialog();
        }
    }
}