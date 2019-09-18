using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CruiseProcessing
{
    public partial class PDFwatermarkDlg : Form
    {
        #region
        public int H2OmarkSelection = 0;
        public int includeDate = 0;
        #endregion
        public PDFwatermarkDlg()
        {
            InitializeComponent();
            WatermarkNone.Checked = true;
        }

        private void onFinished(object sender, EventArgs e)
        {
            Close();
            return;
        }

        private void onNoWatermark(object sender, EventArgs e)
        {
            H2OmarkSelection = 1;
        }

        private void onDraft(object sender, EventArgs e)
        {
            H2OmarkSelection = 2;
        }

        private void onContractOfRecord(object sender, EventArgs e)
        {
            H2OmarkSelection = 3;
        }

        private void onCruiseOfRecord(object sender, EventArgs e)
        {
            H2OmarkSelection = 4;
        }

        private void onIncludeDate(object sender, EventArgs e)
        {
            includeDate = 1;
        }
    }
}
