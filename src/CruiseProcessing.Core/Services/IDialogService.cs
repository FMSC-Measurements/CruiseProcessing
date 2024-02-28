using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Services
{
    public interface IDialogService
    {
        void ShowMessage(string message, string caption = null);

        void ShowInformation(string message);

        void ShowError(string message);

        bool ShowWarningAskYesNo(string message);

        void ShowGraphOutputDialog(IEnumerable<string> graphReports);

        void ShowPrintPreview();

        IEnumerable<StewProductCosts> GetStewardshipProductCosts();
    }
}
