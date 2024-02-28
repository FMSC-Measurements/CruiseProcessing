using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CruiseProcessing
{
    public partial class graphOutputDialog : Form
    {
        public IEnumerable<string> GraphReports { get; }
        protected CPbusinessLayer DataLayer { get; }

        protected graphOutputDialog()
        {
            InitializeComponent();
        }

        public graphOutputDialog(CPbusinessLayer dataLayer, IEnumerable<string> graphReports)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            GraphReports = graphReports ?? throw new ArgumentNullException(nameof(graphReports));
        }

        private void onCancel(object sender, EventArgs e)
        {
            Close();
            return;
        }   //  onCancel

        private void onContinue(object sender, EventArgs e)
        {
            OK_button.Enabled = false;
            Cancel_button.Enabled = false;

            //  loop through list and display graphs
            foreach (var report in GraphReports)
            {
                OutputGraphs og = new OutputGraphs(DataLayer);
                og.currentReport = report;
                og.createGraphs();
            }
            Close();
            return;
        }   //  end onContinue
    }
}