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
using CruiseProcessing.Data;

namespace CruiseProcessing
{
    public partial class StewardshipProductCosts : Form
    {
        public List<StewProductCosts> StewList { get; } = new List<StewProductCosts>();
        public CpDataLayer DataLayer { get; }

        protected StewardshipProductCosts()
        {
            InitializeComponent();
        }

        public StewardshipProductCosts(CpDataLayer dataLayer)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //  pull unique cutting unit, species and primary product to put in stewList
            List<TreeDO> justSpecies = DataLayer.getUniqueStewardGroups();
            foreach (TreeDO js in justSpecies)
            {
                StewProductCosts spc = new StewProductCosts();
                spc.costUnit = js.CuttingUnit.Code;
                spc.costSpecies = js.Species;
                spc.costProduct = js.SampleGroup.PrimaryProduct;
                StewList.Add(spc);
            }   //  end foreach loop
            stewProductCostsBindingSource.DataSource = StewList;
            StewardCosts.DataSource = stewProductCostsBindingSource;
        }


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
            List<StewProductCosts> groupsIncluded = StewList.FindAll(
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
                DataLayer.SaveStewCosts(StewList);
                Close();
                return;
            }   //  endif no groups included
        }   //  end onFinished

    }
}
