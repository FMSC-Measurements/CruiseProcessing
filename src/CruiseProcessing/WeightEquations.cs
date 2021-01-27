using System;
using System.Collections;
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
    public partial class WeightEquations : Form
    {
        #region
        List<WeightEquationDO> wgtList = new List<WeightEquationDO>();
        public string fileName;
        private int trackRow = -1;
        #endregion

        public WeightEquations()
        {
            InitializeComponent();
        }


        public void setupDialog()
        {
            //  if there are weight equations, show in grid
            //  if not, just initialize the grid
            CPbusinessLayer bslyr = new CPbusinessLayer();
            bslyr.fileName = fileName;
            wgtList = bslyr.getWeightEquations();
            weightEquationDOBindingSource.DataSource = wgtList;
            weightEquationList.DataSource = weightEquationDOBindingSource;
            //  need unique species, livedead and primary product from trees    
            List<TreeDefaultValueDO> tList = bslyr.GetUniqueSpeciesProductLiveDead();

            if (wgtList.Count == 0)
            {
                //  Fill species, livedead and primary product with unique items from trees
                foreach (TreeDefaultValueDO tdo in tList)
                {
                    WeightEquationDO wedo = new WeightEquationDO();
                    wedo.Species = tdo.Species;
                    wedo.LiveDead = tdo.LiveDead;
                    wedo.PrimaryProduct = tdo.PrimaryProduct;
                    wgtList.Add(wedo);
                }   //  end foreach loop
                weightEquationDOBindingSource.ResetBindings(false);
            }   //  endif wgtList is empty

            // Fill lists at bottom with unique species and primary products
            ArrayList justSpecies = bslyr.GetJustSpecies();
            for (int n = 0; n < justSpecies.Count; n++)
                speciesList.Items.Add(justSpecies[n].ToString());

            ArrayList justProduct = bslyr.GetJustPrimaryProduct();
            for (int n = 0; n < justProduct.Count; n++)
                primaryProdList.Items.Add(justProduct[n].ToString());

            speciesList.Enabled = true;
            liveDeadList.Enabled = true;
            primaryProdList.Enabled = true;
            return;
        }   //  end setupDialog


        private void onCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (weightEquationList.CurrentRow.IsNewRow == true)
                trackRow = weightEquationList.CurrentCell.RowIndex;
            else
                trackRow = weightEquationList.CurrentCell.RowIndex;

            if (weightEquationList.Rows[e.RowIndex].Cells[2].Selected == true)
            {
                speciesList.Enabled = true;
                liveDeadList.Enabled = true;
                primaryProdList.Enabled = true;
            }   //  endif
        }   //  end onCellClick


        private void onInsertClick(object sender, EventArgs e)
        {
            if (trackRow >= 0)
            {
                weightEquationList.CurrentCell = weightEquationList.Rows[trackRow].Cells[2];
                //bool isNew = weightEquationList.CurrentRow.IsNewRow;      //  debug statement
                weightEquationList.EditMode = DataGridViewEditMode.EditOnEnter;
            }
            else if (trackRow == -1)
            {
                //  means used did click a cell which could be the first row is being edited.
                //  so set trackRow to zero for first row
                trackRow = 0;
            }   //  endif trackRow

            if (trackRow < wgtList.Count)
            {
                //  must be a change
                wgtList[trackRow].Species = speciesList.SelectedItem.ToString();
                wgtList[trackRow].LiveDead = liveDeadList.SelectedItem.ToString();
                wgtList[trackRow].PrimaryProduct = primaryProdList.SelectedItem.ToString();
            }
            else if (trackRow >= wgtList.Count)
            {
                //  it's a new record
                WeightEquationDO wedo = new WeightEquationDO();
                wedo.Species = speciesList.SelectedItem.ToString();
                wedo.LiveDead = liveDeadList.SelectedItem.ToString();
                wedo.PrimaryProduct = primaryProdList.SelectedItem.ToString();
                wgtList.Add(wedo);
            }   //  endif trackRow

            weightEquationDOBindingSource.ResetBindings(false);

            weightEquationList.ClearSelection();
            weightEquationList.CurrentCell = weightEquationList.Rows[trackRow].Cells[5];

            speciesList.Enabled = false;
            liveDeadList.Enabled = false;
            primaryProdList.Enabled = false;
        }   //  end onInsertClick
        

        private void onFinished(object sender, EventArgs e)
        {
            //  Make sure user wants to save all entries
            DialogResult nResult = MessageBox.Show("Do you want to save changes?", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                CPbusinessLayer bslyr = new CPbusinessLayer();
                bslyr.fileName = fileName;
                bslyr.SaveWeightEquations(wgtList);
                Cursor.Current = this.Cursor;
            }   //  endif
            Close();
            return;
        }   //  end onFinished

        private void onCancel(object sender, EventArgs e)
        {
            DialogResult nResult = MessageBox.Show("Are you sure you really want to cancel?", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                Close();
                return;
            }   //  endif nResult
        }   //  end onCancel


    }
}
