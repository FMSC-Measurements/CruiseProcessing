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
    public partial class EnterNewFilename : Form
    {
        #region
        public string templateFilename;
        #endregion

        public EnterNewFilename()
        {
            InitializeComponent();
        }

        private void onNewFilename(object sender, EventArgs e)
        {
            templateFilename = newFileName.Text;
            templateFilename += ".cut";
        }   //  end inNewFukename

        private void onDone(object sender, EventArgs e)
        {
            Close();
            return;
        }   //  end onDone
    }
}
