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
    public partial class SecurityQuestion : Form
    {
        #region
        public string securityResponse;
        #endregion

        public SecurityQuestion()
        {
            InitializeComponent();
        }

        private void onOK(object sender, EventArgs e)
        {
            if (securityAnswer.Text != "DR. EDWIN SMITH")
            {
                MessageBox.Show("WRONG!  Please try again.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                securityAnswer.Clear();
                securityAnswer.Focus();
            }
            else
            {
                securityResponse = "OK";
                Close();
                return;
            }   //  endif
        }
    }
}
