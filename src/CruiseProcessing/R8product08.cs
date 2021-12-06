using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CruiseProcessing
{
    public partial class R8product08 : Form
    {
        #region
        public string[,] speciesDIB;
        private double DIBnumeric;
        int numRows = 0;
        public CPbusinessLayer bslyr = new CPbusinessLayer();
        #endregion


        public R8product08()
        {
            InitializeComponent();
        }


        public int setupDialog()
        {
            //  are there any product 08 species to show in the grid?
            //  changes made to Region 8 equations --  July 2017 --  slight changes to this code
            //ArrayList justSpecies = bslyr.GetProduct08Species();
            ArrayList justSpecies = bslyr.GetJustSpecies("Tree");
            if (justSpecies.Count == 0)
            {
                MessageBox.Show("There are no measured trees in this sale.", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                return 0;
            }   //  endif

            //  Otherwise show default value and put species with default in the grid
            defaultDIB.Text = "6.0";
            speciesDIB = new string[justSpecies.Count, 2];
            //  set number of rows to the species list count
            for (int k = 0; k < justSpecies.Count; k++)
            {
                speciesDIB[k, 0] = justSpecies[k].ToString();
                speciesDIB[k, 1] = "6.0";
            }   //  end for k loop

            //  place just species in the list
            int nthRow = 0;
            speciesDIBlist.RowCount = justSpecies.Count;
            for (int k = 0; k < justSpecies.Count; k++)
            {
                speciesDIBlist.Rows[nthRow].Cells[0].Value = speciesDIB[k, 0];
                speciesDIBlist.Rows[nthRow].Cells[1].Value = speciesDIB[k, 1];
                nthRow++;
            }   //  end for loop on speciesDIB
            
            numRows = justSpecies.Count;
            return 1;
        }   //  end setupDialog


        private void onApply(object sender, EventArgs e)
        {
            //  make sure default dib fits in allowable values (4.0 to 12.0)
            DIBnumeric = Convert.ToDouble(defaultDIB.Text);
            if (DIBnumeric < 4.0 || DIBnumeric > 12.0)
            {
                MessageBox.Show("DIB value not within specified range.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                defaultDIB.Clear();
                defaultDIB.Focus();
                return;
            }   //  endif

            //  Otherwise, apply the default to the values to speciesDIB which is used to update volume equations
            for (int k = 0; k < numRows; k++)
            {
                speciesDIB[k, 1] = defaultDIB.Text;
                //  update grid with DIB defaults
                speciesDIBlist.Rows[k].Cells[1].Value = defaultDIB.Text;
            }   //  end for k loop
            

        }   //  end onApply

        private void onOK(object sender, EventArgs e)
        {
            //  update DIB list before leaving window
            for (int k = 0; k < numRows; k++)
            {
                DIBnumeric = Convert.ToDouble(speciesDIBlist.Rows[k].Cells[1].Value.ToString());
                if (DIBnumeric < 4.0 || DIBnumeric > 12.0)
                {
                    MessageBox.Show("DIB value not within specified range.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    speciesDIBlist.Rows[k].Cells[1].Value = "";
                    return;
                }
                else
                    speciesDIB[k, 1] = speciesDIBlist.Rows[k].Cells[1].Value.ToString();
            }   //  end for loop
            Close();
            return;
        }   //  end onOK

        private void onCancel(object sender, EventArgs e)
        {
            //  Make sure user wants to cancel
            DialogResult dr = MessageBox.Show("Are you sure you really want to cancel?\nAny changes made will not be saved.", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                Close();
                return;
            }   //  endif
        }   //  end onCancel

    }
}
