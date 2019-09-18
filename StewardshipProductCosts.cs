using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public partial class StewardshipProductCosts : Form
    {
        #region
        public List<StewProductCosts> stewList = new List<StewProductCosts>();
        #endregion

        public StewardshipProductCosts()
        {
            InitializeComponent();
        }

        public void setupDialog()
        {
            //  pull unique cutting unit, species and primary product to put in stewList
            foreach (TreeDO js in Global.BL.getUniqueStewardGroups())
            {
                StewProductCosts spc = new StewProductCosts();
                spc.costUnit = js.CuttingUnit.Code;
                spc.costSpecies = js.Species;
                spc.costProduct = js.SampleGroup.PrimaryProduct;
                stewList.Add(spc);
            }   //  end foreach loop
            stewProductCostsBindingSource.DataSource = stewList;
            StewardCosts.DataSource = stewProductCostsBindingSource;

            return;
        }   //  end setupDialog


        private void onCancel(object sender, EventArgs e)
        {
            DialogResult nResult = MessageBox.Show("Are you sure you want to cancel?", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                Close();
                return;
            }
        }   //  end onCancel


        private void onFinished(object sender, EventArgs e)
        {
            //  make the includeInReport has some groups selected
            List<StewProductCosts> groupsIncluded = stewList.FindAll(
                delegate(StewProductCosts sp)
                {
                    return sp.includeInReport == "True";
                });
            if (groupsIncluded.Count == 0)
            {
                MessageBox.Show("No groups selected for inclusion in the report.\nPlease select groups to include by checking the box under Include.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                Global.BL.SaveStewCosts(stewList);
                Close();
                return;
            }   //  endif no groups included
        }   //  end onFinished

    }
}
