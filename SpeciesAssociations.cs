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
    public partial class SpeciesAssociations : Form
    {
        #region
        public string fileName;
        public List<Biomass> bioList = new List<Biomass>();
        public bool tableExists;
        #endregion


        public SpeciesAssociations()
        {
            InitializeComponent();
        }


        public void setupDialog()
        {
            StringBuilder sb = new StringBuilder();
            //  Get just species from tree list
            CPbusinessLayer bslyr = new CPbusinessLayer();
            bslyr.fileName = fileName;
            ArrayList justSpecies = bslyr.GetJustSpecies();

            //  if there are rows in the bioList, show the data in the dialog
            if (bioList.Count() > 0)
            {
                //  Load values as needed
                foreach (Biomass bms in bioList)
                {
                    //  Is species from tree table already associated?  Suppress from user species list if so.
                    for (int k = 0; k < justSpecies.Count; k++)
                    {
                        if (justSpecies[k].ToString() != bms.userSpecies)
                            userSpecies.Items.Add(justSpecies[k].ToString());
                        else
                        {
                            sb.Remove(0, sb.Length);
                            sb.Append(bms.userSpecies);
                            sb.Append(" -- ");
                            sb.Append(bms.bioSpecies);
                            associations.Items.Add(sb.ToString());
                        }   //  endif justSpecies
                        //  add the other fields
                    }   //  end for k loop
                    fractionLeftInWoods.Items.Add(bms.FLIW);
                    damSmallTreesIncluded.Items.Add(bms.DSTincluded);
                }   //  end foreach loop
            }
            else if(bioList.Count() <= 0)
            {
                //  Add just species
                for (int k = 0; k < justSpecies.Count; k++)
                {
                    userSpecies.Items.Add(justSpecies[k].ToString());
                }   //  end for k loop
            }   //  endif bioList has rows

            return;
        }   //  end setupDialog

        private void onAssociate(object sender, EventArgs e)
        {
            //  get user species selection
            StringBuilder sb = new StringBuilder();
            if (userSpecies.SelectedIndex < 0 || availableSpecies.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a User Species and\nAssociation before continuing.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                userSpecies.Focus();
            }   //  endif

            sb.Append(userSpecies.SelectedItem);
            sb.Append(" -- ");
            sb.Append(availableSpecies.SelectedItem);
            associations.Items.Add(sb.ToString());
            //  Remove this species from user species list
            int nthRow = userSpecies.SelectedIndex;
            userSpecies.Items.RemoveAt(nthRow);

            //  Need dialog to capture fraction left in woods and percent damaged small trees to include for this species association
            FLIWdlg fdlg = new FLIWdlg();
            fdlg.speciesAssoc.Text = sb.ToString();
            fdlg.ShowDialog();

            //  then put into list box for FLIW and DST included
            fractionLeftInWoods.Items.Add(fdlg.fractionEntered);
            damSmallTreesIncluded.Items.Add(fdlg.dstIncludeEntered);

            return;
        }   //  end onAssociate

        private void onRemove(object sender, EventArgs e)
        {
            //  remove selected item from associations and put user species back in user species list box
            int nthPosition = associations.SelectedItem.ToString().IndexOf("-");
            userSpecies.Items.Add(associations.SelectedItem.ToString().Substring(0,nthPosition-1));
            int nthRow = associations.SelectedIndex;
            associations.Items.RemoveAt(nthRow);
            fractionLeftInWoods.Items.RemoveAt(nthRow);
            damSmallTreesIncluded.Items.RemoveAt(nthRow);
            return;
        }   //  end onRemove

        private void onCancel(object sender, EventArgs e)
        {
            // ask user if they are sure sure sure they want to cancel
            DialogResult dnr = MessageBox.Show("Are you sure you want to cancel?\nChanges made will not be saved.", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dnr == DialogResult.Yes)
            {
                Close();
                return;
            }   //  endif
        }   //  end onCancel

        private void onFinished(object sender, EventArgs e)
        {
            //  Store data in biomass table
            //  Update bioList before saving to database
            updateBiomassList();

            CPbusinessLayer bslyr = new CPbusinessLayer();
            bslyr.fileName = fileName;
            bslyr.SaveBiomassData(bioList, tableExists);

            DialogResult dnr = MessageBox.Show("All edits are saved.\nOK to exit?", "INFORMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dnr == DialogResult.Yes)
            {
                Close();
                return;
            }   //  endif
        }   //  end onFinished


        private void updateBiomassList()
        {
            string currentUserSpecies;
            string currentBioSpecies;
            CPbusinessLayer bslyr = new CPbusinessLayer();
            bslyr.fileName = fileName;
            int nLength = 0;
            int nthPosition;
            string currentAssn = "";

            //  updates bioList before it is saved in the database
            //  remove all rows first
            bioList.Clear();

            //  rows must be added for all strata where this species is located
            for (int k = 0; k < associations.Items.Count; k++)
            {
                currentAssn = associations.Items[k].ToString();
                //  split out user and bio species
                nLength = currentAssn.Length;
                nthPosition = currentAssn.IndexOf("-");
                currentUserSpecies = currentAssn.Substring(0, nthPosition - 1);
                currentAssn = currentAssn.Remove(0, nthPosition + 3);
                currentBioSpecies = currentAssn;


                //  How many strata have the user species?
                //List<TreeDO> speciesList = bslyr.GetStratumForBiomassReport(currentUserSpecies);
                string[,] stratumSG = bslyr.GetStratumForBiomassReport(currentUserSpecies);
                for (int j = 0; j < stratumSG.GetLength(0); j++)
                {
                    //  setup fields to load into bioList
                    Biomass bms = new Biomass();
                    bms.userSpecies = currentUserSpecies;
                    bms.bioSpecies = currentBioSpecies;
                    bms.FLIW = fractionLeftInWoods.Items[k].ToString();
                    bms.DSTincluded = damSmallTreesIncluded.Items[k].ToString();

                    bms.userStratum = stratumSG[j, 0];
                    bms.userSG = stratumSG[j, 1];

                    bioList.Add(bms);
                }
                
                /*foreach (TreeDO tree in speciesList)
                {
                    bms.userStratum = tree.Stratum.Code;
                    bms.userSG = tree.SampleGroup.Code;

                    bioList.Add(bms);
                }   //  end foreach loop
                 */
            }   //  end for k loop
                
            return;
        }   //  end updateBiomassList
    }
}
