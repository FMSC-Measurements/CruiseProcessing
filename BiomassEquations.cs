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
    public partial class BiomassEquations : Form
    {
        #region
        List<BiomassEquationDO> bioList = new List<BiomassEquationDO>();
        public string fileName;
        private CPbusinessLayer bslyr = new CPbusinessLayer();
        private string currSP;
        private string currPR;
        private string currLD;
        private string currEQ;
        private string currWF;
        #endregion

        public BiomassEquations()
        {
            InitializeComponent();
        }


        public void setupDialog()
        {
            //  if there are biomass euqations, show in grid
            //  if not, just initialize the grid
            bslyr.fileName = fileName;
            bioList = bslyr.getBiomassEquations();
            biomassEquationDOBindingSource.DataSource = bioList;
            biomassEquationList.DataSource = biomassEquationDOBindingSource;

            //  cannot complete species list or equation list without the biomass library
            //  pull species/product and live/dead combinations from tree table to populate
            //  species list
            fillSpeciesList();

            //  initially equation and weight factor and insert button are disabled.
            //  if user selects an individual component then those become enabled.
            equationList.Enabled = false;
            wgt_factors.Enabled = false;
            insertButton.Enabled = false;

            return;
        }   //  end setupDialog


        private void onInsertClick(object sender, EventArgs e)
        {
            BiomassEquationDO biodo = new BiomassEquationDO();
            biodo.Species = speciesList.SelectedItem.ToString();
            biodo.Component = componentList.SelectedItem.ToString();
            biodo.Equation = equationList.SelectedItem.ToString();
            //biodo.Product = Convert.ToInt16(productList.SelectedItem.ToString());
            bioList.Add(biodo);
            biomassEquationDOBindingSource.ResetBindings(false);

        }   //  end onInsertClick


        private void onFinished(object sender, EventArgs e)
        {
            //  Make sure user wants to save all entries
            DialogResult nResult = MessageBox.Show("Do you want to save changes?", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                bslyr.fileName = fileName;
                bslyr.SaveBiomassEquations(bioList);
                Cursor.Current = this.Cursor;
            }   //  endif
            return;
        }   //  end onFinished


        private void componentSelectIndexChanged(object sender, EventArgs e)
        {
            string selectedComponent = equationList.SelectedItem.ToString();
            if (selectedComponent == "Use defaults for all components")
            {
                //  call NBEL to get default values
            }
            else
            {
                //  need region, forest and district for these calls to NBEL
                bslyr.fileName = fileName;
                string currRegion = bslyr.getRegion();
                string currForest = bslyr.getForest();
                string currDistrict = bslyr.getDistrict();
                switch (selectedComponent)
                {
                    case "Total Tree":
                        //  call to NBEL for equations
                        break;
                    case "Live Branches":
                        break;
                    case "Dead Branches":
                        break;
                    case "Foliage":
                        break;
                    case "Tip":
                        break;
                    case "Mainstem Sawtimber":
                        break;
                    case "Mainstem Topwood":
                        break;
                }   //  end switch
            }   //  endif
        }   //  end componentSelectIndexChanged


        private void equationSelectIndexChanged(object sender, EventArgs e)
        {
            currEQ = equationList.SelectedItem.ToString();
        }   //  end equationSelectIndexChanged

        private void wgtFactorSelectIndexChanged(object sender, EventArgs e)
        {
            currWF = wgt_factors.SelectedItem.ToString();
        }   //  end wgtFactorSelectIndexChanged


        private void onSpeciesSelectIndexChanged(object sender, EventArgs e)
        {
            string speciesSelection = speciesList.SelectedItem.ToString();
            //  break apart into species, product and live/dead for NBEL call
            currSP = speciesSelection.Substring(0, 6);
            currPR = speciesSelection.Substring(11, 2);
            currLD = speciesSelection.Substring(17, 4);
            if (currLD == "Live")
                currLD = "L";
            else if (currLD == "Dead")
                currLD = "D";            
        }   //  end onSpeciesSelectIndexChanged


        private void fillSpeciesList()
        {
            //  combination of species, product and live/dead comes from tree table
            bslyr.fileName = fileName;
            List<TreeDefaultValueDO> tdvList = bslyr.GetUniqueSpeciesProductLiveDead();
            StringBuilder sb = new StringBuilder();
            foreach (TreeDefaultValueDO tdv in tdvList)
            {
                sb.Remove(0, sb.Length);
                sb.Append(tdv.Species.PadRight(6,' '));
                sb.Append(" -- ");
                sb.Append(tdv.PrimaryProduct);
                sb.Append(" -- ");
                if (tdv.LiveDead == "L")
                    sb.Append("Live");
                else sb.Append("Dead");
                speciesList.Items.Add(sb.ToString());
            }   //  end foreach loop
        }   //  end fillSpeciesList


        private void onCancel(object sender, EventArgs e)
        {
            DialogResult nResult = MessageBox.Show("Are you sure you really want to cancel", "CONFIRMATINO", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                Close();
                return;
            }   //  endif nResult
        }   //  end onCancel

    }
}
