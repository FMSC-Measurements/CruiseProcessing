﻿using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Services
{
    public interface IDialogService
    {
        string? AskOpenCruise();

        void ShowAbout();

        void ShowMessage(string message, string caption = null);

        void ShowInformation(string message);

        void ShowError(string message);

        void ShowWarning(string message);

        bool ShowWarningAskYesNo(string message);

        void ShowGraphOutputDialog(IEnumerable<string> graphReports);

        void ShowPrintPreview();

        string? ShowOutput(List<ReportsDO> reports);

        bool ShowProcess();

        void ShowStandardReports(List<ReportsDO> reports, bool isTemplate);

        void ShowGraphicalReports();

        void ShowR8VolumeEquations();

        void ShowR9VolumeEquations();

        void ShowVolumeEquations(bool isTemplateFile);

        void ShowValueEquations();

        void ShowCreatePdf();

        void ShowCreateCsv();

        void ShowAddLocalVolumes();

        IEnumerable<StewProductCosts> GetStewardshipProductCosts();

        void ShowModifyMerchRules();

        void ShowModifyWeightFactors();
    }
}