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
    public partial class R9Topwood : Form
    {
        #region
        public string fileName;
        public ArrayList speciesList = new ArrayList();
        public ArrayList flagList = new ArrayList();
        #endregion


        public R9Topwood()
        {
            InitializeComponent();
        }

        public void setupDialog()
        {
            //  load lists as needed
            for (int k = 0; k < flagList.Count; k++)
            {
                if (flagList[k].ToString() == "0")
                    speciesToExclude.Items.Add(speciesList[k]);
                else if (flagList[k].ToString() == "1")
                    speciesToInclude.Items.Add(speciesList[k]);
            }   //  end for k loop
            return;
        }   //  end setupDialog


        private void onFinished(object sender, EventArgs e)
        {
            //  Store species in array list for use in creating equations
            if (speciesToInclude.Items.Count == 0)
            {
                DialogResult dr = MessageBox.Show("There are no species listed for inclusion.\nIs this correct?", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    speciesList.Clear();
                }
                else if (dr == DialogResult.No)
                    return;
            }
            else if(speciesToInclude.Items.Count > 0)
            {
                speciesList.Clear();
                flagList.Clear();
                //  add species to include
                for (int k = 0; k < speciesToInclude.Items.Count; k++)
                {
                    speciesList.Add(speciesToInclude.Items[k]);
                    flagList.Add("1");
                }   //  end for k loop
                //  and the species to exclude
                if (speciesToExclude.Items.Count > 0)
                {
                    for (int k = 0; k < speciesToExclude.Items.Count; k++)
                    {
                        speciesList.Add(speciesToExclude.Items[k]);
                        flagList.Add("0");
                    }   //  end for k loop
                }   //  endif
            }     //  endif species included
            Close();
            return;
        }   //  end onFinished


        private void onExcludeAll(object sender, EventArgs e)
        {
            //  moves all species from include list to exclude list
            for (int k = 0; k < speciesToInclude.Items.Count; k++)
                speciesToExclude.Items.Add(speciesToInclude.Items[k]);
            
            //  remove all from speciesToInclude
            speciesToInclude.Items.Clear();
            return;
        }   //  end onExcludeAll


        private void onExcludeOne(object sender, EventArgs e)
        {
            //  button is right facing arrow -- include to exclude
            //  moves one item from include to exclude
            object speciesSelected = speciesToInclude.SelectedItem;
            if (speciesSelected == null)
            {
                MessageBox.Show("Please select a species to exclude.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                speciesToExclude.Items.Add(speciesSelected);

                //  remove item from speciesToInclude
                speciesToInclude.Items.Remove(speciesSelected);
            }   //  endif species selected
        }   //  end onExcludeOne


        private void onIncludeOne(object sender, EventArgs e)
        {
            //  button is left facing arrow -- exclude to include
            //  moves one item from exclude to include
            object speciesSelected = speciesToExclude.SelectedItem;
            if (speciesSelected == null)
            {
                MessageBox.Show("Please select a species to include.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                speciesToInclude.Items.Add(speciesSelected);

                //  remove item from speciesToExclude
                speciesToExclude.Items.Remove(speciesSelected);
            }   //  endif species selected
        }   //  end onIncludeOne

        private void onIncludeAll(object sender, EventArgs e)
        {
            //  moves all species from exclude list to include list
            for (int k = 0; k < speciesToExclude.Items.Count; k++)
                speciesToInclude.Items.Add(speciesToExclude.Items[k]);

            //  remove all from speciesToExclude
            speciesToExclude.Items.Clear();
        }   //  end onIncludeAll
    }
}
