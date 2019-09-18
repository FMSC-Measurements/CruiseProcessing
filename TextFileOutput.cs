using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public partial class TextFileOutput : Form
    {
        #region
        public List<ReportsDO> selectedReports = new List<ReportsDO>();
        public string fileName;
        public string outFile;
        public int retrnState;
        public string currRegion;

        #endregion


        public TextFileOutput()
        {
            InitializeComponent();
        }



        public void setupDialog()
        {
            StringBuilder listOreports = new StringBuilder();
            foreach (ReportsDO rdo in selectedReports)
            {
                listOreports.Append(rdo.ReportID);
                listOreports.Append(" ");
            }   //  end foreach loop
            reportsList.Text = listOreports.ToString();
            finished_Button.Enabled = false;
            fileStatus.Enabled = false;
        }   //  end setupDialog


        private void click_Finished(object sender, EventArgs e)
        {
            Close();
            retrnState = 0;
            return;
        }

        private void click_Go(object sender, EventArgs e)
        {
            //  need to check for critical errors first
            Cursor.Current = Cursors.WaitCursor;
            reportsList.Refresh();
            go_Button.Refresh();
            finished_Button.Refresh();
            fileStatus.Enabled = true;
            fileStatus.Refresh();
            //  calls routine to create text output file
            CreateTextFile ctf = new CreateTextFile();
            ctf.fileName = fileName;
            ctf.currentRegion = currRegion;
            ctf.selectedReports = selectedReports;
            ctf.createTextFile();
            outFile = ctf.textFile;
            //  reset cursor
            Cursor.Current = Cursors.Default;
            finished_Button.Enabled = true;
            fileStatus.Enabled = false;
            go_Button.Enabled = false;
            return;
        }

        private void onClose(object sender, FormClosingEventArgs e)
        {
            retrnState = 1;
            return;
        }

    }
}
