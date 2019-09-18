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
    public partial class selectLength : Form
    {
        #region
        public int lengthSelected = 0;
        #endregion

        public selectLength()
        {
            InitializeComponent();
        }

        private void onFinished(object sender, EventArgs e)
        {
            Close();
            return;
        }

        private void onThirtyTwo(object sender, EventArgs e)
        {
            lengthSelected = 32;
        }

        private void onSixteen(object sender, EventArgs e)
        {
            lengthSelected = 16;
        }
    }
}
