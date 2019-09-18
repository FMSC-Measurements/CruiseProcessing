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
    public partial class TemplateRegionForest : Form
    {
        #region
        public string currentRegion;
        public string currentForest;
        #endregion
        public TemplateRegionForest()
        {
            InitializeComponent();
        }

        private void onDone(object sender, EventArgs e)
        {
            Close();
            return;
        }

        private void onSelectedRegion(object sender, EventArgs e)
        {
            currentRegion = templateRegion.SelectedItem.ToString();
        }       //  end onSelectedRegion

        private void onEnteredForest(object sender, EventArgs e)
        {
            currentForest = templateForest.Text;
            if (currentForest == "All")
            {
                MessageBox.Show("Forest Number cannot be the word All,\nPlease try again.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                templateForest.Clear();
                templateForest.Focus();
            }
            else if (currentForest.Length == 1)
            {
                //  single digit forest number is not handled by biomass library
                //  add a leading zero
                currentForest.Insert(0, "0");
            }   //  endif
        }       //  end onEnteredForest
    }
}
