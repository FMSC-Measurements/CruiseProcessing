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
    public partial class FLIWdlg : Form
    {
        #region
        double convertedValue;
        public string fractionEntered;
        public string dstIncludeEntered;
        #endregion

        public FLIWdlg()
        {
            InitializeComponent();
        }


        private void onFinished(object sender, EventArgs e)
        {
            fractionEntered = fractionText.Text;
            dstIncludeEntered = DSTinclude.Text;
            Close();
            return;
        }   //  end onFinished


        private void onFractionLeave(object sender, EventArgs e)
        {
            if (fractionText.Text != "")
                convertedValue = Convert.ToDouble(fractionText.Text);
            if (convertedValue >= 1.1)
            {
                MessageBox.Show("Fraction left in woods must be decimal.\nFor example, 55% is entered as 0.55", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                fractionText.Clear();
                fractionText.Focus();
                return;
            }   //  endif
            return;

        }   //  end onFractionLeave


        private void onIncludeLeave(object sender, EventArgs e)
        {
            if (DSTinclude.Text != "")
                convertedValue = Convert.ToDouble(DSTinclude.Text);
            if (convertedValue >= 1.1)
            {
                MessageBox.Show("Percent small trees must be decimal.\nFor example, 55% is entered as 0.55", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DSTinclude.Clear();
                DSTinclude.Focus();
                return;
            }   //  endif

        }   //  end onIncludeLeave
    }
}
