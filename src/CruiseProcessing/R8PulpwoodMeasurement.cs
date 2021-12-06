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
    public partial class R8PulpwoodMeasurement : Form
    {
        #region
        public int pulpHeight = -1;
        #endregion

        public R8PulpwoodMeasurement()
        {
            InitializeComponent();
        }

        private void onFinished(object sender, EventArgs e)
        {
            //  Check list box for a selection before continuing
            if (pulpwoodHeight.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a measurement type for pulpwood trees", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else pulpHeight = pulpwoodHeight.SelectedIndex;
            Close();
            return;
        }   //  end onFinished


        private void onCancel(object sender, EventArgs e)
        {
            if(pulpwoodHeight.SelectedIndex < 0)
            {
                DialogResult dr = MessageBox.Show("No pulpwood height measurement selected.\nAre you sure you want to cancel?\nEquations cannot be created without this information.", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if(dr == DialogResult.No)
                    return;
                else if(dr == DialogResult.Yes)
                {
                    Close();
                    return;
                }   //  endif
            }   //  endif
        }   //  onCancel
    }
}
