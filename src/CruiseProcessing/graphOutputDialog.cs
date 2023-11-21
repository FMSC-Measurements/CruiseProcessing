using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CruiseProcessing
{
    public partial class graphOutputDialog : Form
    {
        public ArrayList graphReports = new ArrayList();
        protected CPbusinessLayer DataLayer { get; }

        protected graphOutputDialog()
        {
            InitializeComponent();
        }

        public graphOutputDialog(CPbusinessLayer dataLayer)
            :this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
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
            for (int k = 0; k < graphReports.Count; k++)
            {
                OutputGraphs og = new OutputGraphs(DataLayer);
                og.currentReport = graphReports[k].ToString();
                og.createGraphs();
            }   //  end for k loop
            Close();
            return;
        }   //  end onContinue

    }
}
