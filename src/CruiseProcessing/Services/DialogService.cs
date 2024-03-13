using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CruiseProcessing.Services
{
    public class DialogService : IDialogService
    {
        public DialogService(IServiceProvider services)
        {
            // = mainMenu;
            Services = services;
        }

        //private MainMenu MainMenu { get; }
        public IServiceProvider Services { get; }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowWarning(string message)
        {
            MessageBox.Show(message, "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public bool ShowWarningAskYesNo(string message)
        {
            var result = MessageBox.Show(message, "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return result == DialogResult.Yes;
        }

        public void ShowGraphOutputDialog(IEnumerable<string> graphReports)
        {
            var dialog = Services.GetRequiredService<graphOutputDialog>();
            dialog.ShowDialog();
        }

        public void ShowInformation(string message)
        {
            MessageBox.Show(message, "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowMessage(string message, string caption = null)
        {
            MessageBox.Show(message, caption);
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
    }
}