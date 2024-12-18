using System;
using System.Collections.Generic;
using System.Collections;
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
    public partial class ValueEquations : Form
    {
        private string[] BasicEquations = new string[8] {"01","02","03","04","06","07","10","12"};
        private string[] R9Equations = new string[14] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14" };
        private string[] R4Equations = new string[4] { "01", "02", "03", "04" };
        private string equationPrefix = "VLPP";
        private List<ValueEquationDO> valList = new List<ValueEquationDO>();
        private int trackRow = -1;
        protected CpDataLayer DataLayer { get; }

        protected ValueEquations()
        {
            InitializeComponent();
        }

        public ValueEquations(CpDataLayer dataLayer)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }


        public int setupDialog()
        {
            //  if there are value equations, show in grid
            //  if not, just initialize the grid
            valList = DataLayer.getValueEquations();
            valueEquationDOBindingSource.DataSource = valList;
            valueEquationList.DataSource = valueEquationDOBindingSource;

            //  need unique species and product
            List<TreeDefaultValueDO> tdvList = DataLayer.GetUniqueSpeciesProductLiveDead();
            
            if (valList.Count == 0)
            {
                foreach(TreeDefaultValueDO tdv in tdvList)
                {
                    ValueEquationDO ved = new ValueEquationDO();
                    ved.Species = tdv.Species;
                    ved.PrimaryProduct = tdv.PrimaryProduct;
                    valList.Add(ved);
                }   //  end foreach loop
                valueEquationDOBindingSource.ResetBindings(false);
            }   //  endif list is empty

            //  Fill lists at bottom with unique species and primary products
            var justSpecies = DataLayer.GetDistinctTreeSpeciesCodes();
            foreach(var sp in justSpecies)
            {
                speciesList.Items.Add(sp);
            }

            //  If there are no species/products in tree default values, it's wrong
            //  tell user to check the file design in CSM --  June 2013
            if (!justSpecies.Any())
            {
                MessageBox.Show("No species/product combinations found in Tree records.\nPlease enter tree records before continuing.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return -1;
            }   //  endif

            var justProducts = DataLayer.GetDistinctPrimaryProductCodes();
            foreach(var prod in  justProducts)
            {
                primaryProdList.Items.Add(prod);
            }

            regionNum.Enabled = false;
            equationNumber.Enabled = false;
            speciesList.Enabled = false;
            primaryProdList.Enabled = false;
            return 1;
        }   //  end setupDialog


        private void onRegionSelected(object sender, EventArgs e)
        {
            //  based on region build and populate equation list
            int indexNum = 8;
            StringBuilder sb = new StringBuilder();
            if (equationNumber.Items.Count > 0)
                equationNumber.Items.Clear();
            if (regionNum.Text == "04")
            {
                for (int k = 0; k < 4; k++)
                {
                    sb.Clear();
                    sb.Append(equationPrefix);
                    sb.Append("04");
                    sb.Append(R4Equations[k]);
                    equationNumber.Items.Add(sb.ToString());
                }   //  end for k loop
            }
            else if (regionNum.Text == "09")
            {
                for(int k = 0; k < 14; k++)
                {
                    sb.Clear();
                    sb.Append(equationPrefix);
                    sb.Append("09");
                    sb.Append(R9Equations[k]);
                    equationNumber.Items.Add(sb.ToString());
                }   //  end for k loop
            }
            else
            {
                for (int k = 0; k < indexNum; k++)
                {
                    sb.Clear();
                    sb.Append(equationPrefix);
                    sb.Append(regionNum.Text);
                    sb.Append(BasicEquations[k]);
                    equationNumber.Items.Add(sb.ToString());
                }   //  end for k loop
            }   //  endif

            return;
        }   //  end onRegionSelected

        private void onCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (valueEquationList.CurrentRow.IsNewRow == true)
                trackRow = valueEquationList.CurrentCell.RowIndex;
            else 
                trackRow = valueEquationList.CurrentCell.RowIndex;

            if (valueEquationList.Rows[e.RowIndex].Cells[2].Selected == true)
            {
                regionNum.Enabled = true;
                equationNumber.Enabled = true;
                speciesList.Enabled = true;
                primaryProdList.Enabled = true;
            }   //  endif
        }   //  end onCellClick


        private void onInsertEquation(object sender, EventArgs e)
        {
            if (trackRow >= 0)
            {
                valueEquationList.CurrentCell = valueEquationList.Rows[trackRow].Cells[2];
                //bool isNew = valueEquationList.CurrentRow.IsNewRow;
                valueEquationList.EditMode = DataGridViewEditMode.EditOnEnter;
            }
            else if(trackRow == -1)
            {
                //  means user didn't click a cell which could be the first row is being edited.
                //  so set trackRow to zero for first row
                trackRow = 0;
            }   //  endif trackRow
            if (trackRow < valList.Count)
            {
                //  must be a change 
                valList[trackRow].ValueEquationNumber = equationNumber.SelectedItem.ToString();
                valList[trackRow].Species = speciesList.SelectedItem.ToString();
                valList[trackRow].PrimaryProduct = primaryProdList.SelectedItem.ToString();
            }
            else if(trackRow >= valList.Count)
            {
                //  it's a new record
                ValueEquationDO ved = new ValueEquationDO();
                ved.ValueEquationNumber = equationNumber.SelectedItem.ToString();
                ved.Species = speciesList.SelectedItem.ToString();
                ved.PrimaryProduct = primaryProdList.SelectedItem.ToString();
                valList.Add(ved);
            }
            valueEquationDOBindingSource.ResetBindings(false);
            
            valueEquationList.ClearSelection();
            valueEquationList.CurrentCell = valueEquationList.Rows[trackRow].Cells[5];

            regionNum.Enabled = false;
            equationNumber.Enabled = false;
            speciesList.Enabled = false;
            primaryProdList.Enabled = false;
        }   //  end onInsertEquation

        
        private void onFinished(object sender, EventArgs e)
        {
            //  Make sure user wants to save all entries
            DialogResult nResult = MessageBox.Show("Do you want to save changes?", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                //  make sure the equations have been entered
                foreach (ValueEquationDO ved in valList)
                {
                    if (ved.ValueEquationNumber == null || ved.ValueEquationNumber == "" ||
                        ved.ValueEquationNumber == " ")
                    {
                        nResult = MessageBox.Show("One or more equation numbers are blank.\nCannot continue without equation numbers.\nNo records saved.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;
                    }   //  endif
                }   //  end foreach loop
                //  make sure coefficients are zero or a value but not null
                foreach (ValueEquationDO ved in valList)
                {
                    if (ved.Coefficient1 <= 0) ved.Coefficient1 = 0;
                    if (ved.Coefficient2 <= 0) ved.Coefficient2 = 0;
                    if (ved.Coefficient3 <= 0) ved.Coefficient3 = 0;
                    if (ved.Coefficient4 <= 0) ved.Coefficient4 = 0;
                    if (ved.Coefficient5 <= 0) ved.Coefficient5 = 0;
                    if (ved.Coefficient6 <= 0) ved.Coefficient6 = 0;
                }   //  end foreach loop
                DataLayer.SaveValueEquations(valList);
                Cursor.Current = this.Cursor;
            }   //  endif

            if (DataLayer.DAL_V3 != null)
            {
                DataLayer.syncValueEquationToV3();
            }//end if

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
        }

        private void onDelete(object sender, EventArgs e)
        {
            DataGridViewRow row = valueEquationList.SelectedRows[0];
            valueEquationList.Rows.Remove(row);
        }   //  end onCancel
    }
}
