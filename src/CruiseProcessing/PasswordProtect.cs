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
    public partial class PasswordProtect : Form
    {
        #region
        public string passwordResponse;
        #endregion

        public PasswordProtect()
        {
            InitializeComponent();
        }

        private void onOK(object sender, EventArgs e)
        {
            if (passwordEntered.Text != "DENALI")
            {
                MessageBox.Show("WRONG!  Please try again.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                passwordEntered.Clear();
                passwordEntered.Focus();
            }
            else
            {
                passwordResponse = "OK";
                Close();
                return;
            }   //  endif

        }   //  end onOK
    }
}
