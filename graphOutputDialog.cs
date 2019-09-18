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
        #region
        public ArrayList graphReports = new ArrayList();
        public string fileName;
        #endregion

        public graphOutputDialog()
        {
            InitializeComponent();
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
                OutputGraphs og = new OutputGraphs();
                og.currentReport = graphReports[k].ToString();
                og.fileName = fileName;
                og.createGraphs();
            }   //  end for k loop
            Close();
            return;
        }   //  end onContinue

    }
}
