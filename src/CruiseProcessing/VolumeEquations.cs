﻿using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CruiseProcessing
{
    public partial class VolumeEquations : Form
    {
        private const int CRZSPDFTCS_STRINGLENGTH = 256;
        public static readonly ReadOnlyCollection<string> BIOMASS_COMPONENTS = Array.AsReadOnly(new[] { "TotalTreeAboveGround", "LiveBranches", "DeadBranches", "Foliage", "PrimaryProd", "SecondaryProd", "StemTip" });

        //  list class for volumes
        public List<VolEqList> volList = new List<VolEqList>();

        public List<VolumeEquationDO> equationList = new List<VolumeEquationDO>();
        private string selectedRegion;
        private string selectedForest;
        public int templateFlag;
        private int trackRow = -1;

        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CRZSPDFTCS(ref int regn, StringBuilder forst, out int spcd, float[] wf, StringBuilder agteq, StringBuilder lbreq,
            StringBuilder dbreq, StringBuilder foleq, StringBuilder tipeq, StringBuilder wf1ref, StringBuilder wf2ref, StringBuilder mcref,
            StringBuilder agtref, StringBuilder lbrref, StringBuilder dbrref, StringBuilder folref, StringBuilder tipref,
            int i1, int i2, int i3, int i4, int i5, int i6, int i7, int i8, int i9, int i10, int i11, int i12, int i13, int i14);

        public CpDataLayer DataLayer { get; }
        public IServiceProvider Services { get; }
        public IDialogService DialogService { get; }

        protected VolumeEquations()
        {
            InitializeComponent();
        }

        public VolumeEquations(CpDataLayer dataLayer, IDialogService dialogService, IServiceProvider services)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        public int setupDialog()
        {
            //  fill species list from tree table
            //  show message box if this is BLM
            string currRegion = DataLayer.getRegion();
            if (currRegion == "7" || currRegion == "07")
            {
                DialogService.ShowInformation("The Cruise Region is BLM (07).  BLM Volume Equations cannot be entered here.");
                Close();
                return -1;
            }   //  endif BLM

            //  if there are volume equations, show in grid
            //  if not, the grid is just initialized
            equationList = DataLayer.getVolumeEquations();

            //  Check for missing common name and model name
            VolumeEqMethods.SetSpeciesAndModelValues(equationList, currRegion);

            var speciesProduct = DataLayer.GetUniqueSpeciesProductFromTrees();

            // this removes any species that are not in the tree table
            // however, if a cruiser is processing the cruise before it is complete
            // the species may not be in the tree table yet
            // so we may not want to automatically remove those equations
            // equationList = updateEquationList(speciesProduct);

            //  If there are no species/products in tree default values, it's wrong
            //  tell user to check the file design in CSM --  June 2013
            if (!speciesProduct.Any())
            {
                DialogService.ShowError("No species/product combinations found in Tree records.\nPlease enter tree records before continuing.");
                Close();
                return -1;
            }   //  endif

            foreach (var spProd in speciesProduct)
            {

                if (!equationList.Any(x => x.Species == spProd.SpeciesCode && x.PrimaryProduct == spProd.ProductCode))
                {
                    var ved = new VolumeEquationDO
                    {
                        Species = spProd.SpeciesCode,
                        PrimaryProduct = spProd.ProductCode
                    };
                    equationList.Add(ved);
                }
            }


            volumeEquationDOBindingSource.DataSource = equationList;
            volumeEquationList.DataSource = volumeEquationDOBindingSource;

            //  also add species and product to combo boxes at bottom
            var justSpecies = DataLayer.GetAllSpeciesCodes();
            foreach(var sp in justSpecies)
            {
                speciesList.Items.Add(sp);
            }

            var justProduct = DataLayer.GetDistincePrimaryProductCodes();
            foreach(var prod in  justProduct)
            {
                productList.Items.Add(prod);
            }

            volRegion.Enabled = false;
            volForest.Enabled = false;
            volEquation.Enabled = false;
            speciesList.Enabled = false;
            productList.Enabled = false;
            return 1;
        }   //  end setupDialog

        public int setupTemplateDialog()
        {
            equationList = DataLayer.getVolumeEquations();

            if(!equationList.Any())
            {
                //  Regions 8 and 9, BLM and Region 6 (?) cannot edit equations
                //  inform user and return to main menu == March 2017
                DialogService.ShowInformation("Regional template files for Regions 6, 8, 9 and BLM cannot be edited here.\nThese equations are created or entered when running a cruise file in CruiseProcessing.");
            }

            volumeEquationDOBindingSource.DataSource = equationList;
            volumeEquationList.DataSource = volumeEquationDOBindingSource;
            //  can't pull species/product combinations from tree default values
            //  user may want to enter a species or product not available in the default codes
            //  so text boxes are enabled when a template file is edited
            //  combo boxes are hidden and disabled -- March 2017
            //  also add species and product to combo boxes at bottom
            /*                ArrayList justSpecies = bslyr.GetJustSpecies("TreeDefaultValue");
                            for (int n = 0; n < justSpecies.Count; n++)
                                speciesList.Items.Add(justSpecies[n].ToString());

                            ArrayList justProduct = bslyr.GetJustPrimaryProduct();
                            for (int n = 0; n < justProduct.Count; n++)
                                productList.Items.Add(justProduct[n].ToString());
            */
            volRegion.Enabled = false;
            volForest.Enabled = false;
            volEquation.Enabled = false;
            speciesList.Enabled = false;

            // hide combo boxes and use text boxes for species and product
            speciesList.Hide();
            templateSpecies.BringToFront();
            templateSpecies.Enabled = false;
            productList.Enabled = false;
            productList.Hide();
            templateProduct.BringToFront();
            templateProduct.Enabled = false;

            return 1;
        }   //  end setupTempalteDialog

        private void onRegionSelected(object sender, EventArgs e)
        {
            selectedRegion = volRegion.SelectedItem.ToString();
            if (selectedRegion == "DOD")
                selectedRegion = "11";
            //if(selectedRegion == "05")
            //    MessageBox.Show("USE \"ALL\" FOR THE FOREST SELECTION.\nThe specific forest number should only be used for special reports\nwhen requestd by the Northern Spotted Owl planning team.","WARNING",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            fillForests(selectedRegion);
        }

        private void fillForests(string selectedRegion)
        {
            volList = volumeLists.GetVolumeEquationsByRegion(selectedRegion);

            //  find unique forests to generate list
            var distinctForests = volList.Select(x => x.vForest).Distinct().ToArray();

            volForest.Items.Clear();
            volForest.Items.AddRange(distinctForests);

            volForest.Enabled = true;
        }

        private void onForestIndexChanged(object sender, EventArgs e)
        {
            selectedForest = volForest.SelectedItem.ToString();
            fillEquations(selectedForest);
        }

        private void fillEquations(string selectedForest)
        {
            //  find equations for the selected forest
            volEquation.Items.Clear();
            for (int k = 0; k < volList.Count; k++)
            {
                if (selectedForest == volList[k].vForest)
                    volEquation.Items.Add(volList[k].vEquation.ToString());
            }   //  end for k loop

            volEquation.Enabled = true;
            if (templateFlag == 1)
            {
                templateSpecies.Enabled = true;
                templateProduct.Enabled = true;
            }
            else if (templateFlag == 0)
            {
                speciesList.Enabled = true;
                productList.Enabled = true;
            }   //  endif templateFlag
        }

        private void onCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (volumeEquationList.CurrentRow.IsNewRow == true)
                trackRow = volumeEquationList.CurrentCell.RowIndex;
            else
                trackRow = volumeEquationList.CurrentCell.RowIndex;

            if (volumeEquationList.Rows[e.RowIndex].Cells[2].Selected == true)
            {
                volRegion.Enabled = true;
            }   //  endif
        }   //  end onCellClick

        private void onInsertClick(object sender, EventArgs e)
        {
            //  if any of the five fields at the bottom are blank, warn users before continuing
            if (templateFlag == 0)
            {
                if (selectedRegion == null || selectedForest == null || volEquation.SelectedItem == null ||
                    speciesList.SelectedItem == null || productList.SelectedItem == null)
                {
                    DialogService.ShowError("One or more items to insert cannot be blank.\nPlease correct.");
                    return;
                }
            }
            else if (templateFlag == 1)
            {
                if (selectedRegion == null || selectedForest == null || volEquation.SelectedItem == null ||
                    templateSpecies.Text == null || templateProduct.Text == null)
                {
                    DialogService.ShowError("One or more items to insert cannot be blank.\nPlease correct.");
                    return;
                }
            }
            if (trackRow >= 0)
            {
                volumeEquationList.CurrentCell = volumeEquationList.Rows[trackRow].Cells[2];
                //bool isNew = volumeEquationList.CurrentRow.IsNewRow;      //  debug statement
                volumeEquationList.EditMode = DataGridViewEditMode.EditOnEnter;
            }
            else if (trackRow == -1)
            {
                //  means user didn't click a cell which could be the first row is being edited
                //  so set trackRow to zero for first row
                trackRow = 0;
            }   //  endif trackRow

            if (trackRow < equationList.Count)
            {
                //  must be a change
                equationList[trackRow].VolumeEquationNumber = volEquation.SelectedItem.ToString();
                if (templateFlag == 0)
                {
                    equationList[trackRow].Species = speciesList.SelectedItem.ToString();
                    equationList[trackRow].PrimaryProduct = productList.SelectedItem.ToString();
                }
                else if (templateFlag == 1)
                {
                    equationList[trackRow].Species = templateSpecies.Text.ToString();
                    equationList[trackRow].PrimaryProduct = templateProduct.Text.ToString();
                }   //  endif templateFalg
                //  common species name and model
                int nthRow = volList.FindIndex(
                    delegate (VolEqList vel)
                    {
                        return vel.vEquation == volEquation.SelectedItem.ToString() &&
                                vel.vForest == selectedForest;
                    });
                if (nthRow >= 0)
                {
                    equationList[trackRow].CommonSpeciesName = volList[nthRow].vCommonName;
                    equationList[trackRow].Model = volList[nthRow].vModelName;
                }   //  endif nthRow
            }
            else if (trackRow >= equationList.Count)
            {
                //  it's a new record
                VolumeEquationDO ved = new VolumeEquationDO();
                ved.VolumeEquationNumber = volEquation.SelectedItem.ToString();
                int nthRow = volList.FindIndex(
                    delegate (VolEqList vel)
                    {
                        return vel.vEquation == ved.VolumeEquationNumber;
                    });
                if (nthRow >= 0)
                {
                    ved.CommonSpeciesName = volList[nthRow].vCommonName;
                    ved.Model = volList[nthRow].vModelName;
                }   //  endif nthRow
                if (templateFlag == 0)
                {
                    ved.Species = speciesList.SelectedItem.ToString();
                    ved.PrimaryProduct = productList.SelectedItem.ToString();
                }
                else if (templateFlag == 1)
                {
                    ved.Species = templateSpecies.Text.ToString();
                    ved.PrimaryProduct = templateProduct.Text.ToString();
                }   //  endif templateFlag
                equationList.Add(ved);
            }   //  endif trackRow

            volumeEquationDOBindingSource.ResetBindings(false);

            volumeEquationList.ClearSelection();
            volumeEquationList.CurrentCell = volumeEquationList.Rows[trackRow].Cells[7];

            volForest.Text = "";
            volEquation.Text = "";
            volRegion.Text = "";
            volRegion.Enabled = false;
            volForest.Enabled = false;
            volEquation.Enabled = false;
            if (templateFlag == 0)
            {
                speciesList.Text = "";
                productList.Text = "";
                speciesList.Enabled = false;
                productList.Enabled = false;
            }
            else if (templateFlag == 1)
            {
                templateSpecies.Text = "";
                templateProduct.Text = "";
                templateSpecies.Enabled = false;
                templateProduct.Enabled = false;
            }   //  endif templateflag
        }   //  end onInsertClick

        private void onFinished(object sender, EventArgs e)
        {
            //  make sure volume equation is not blank for any species/product and make sure at least one volume flag is checked

            List<string> errors = new List<string>();
            if (equationList.Any(x => string.IsNullOrEmpty(x.VolumeEquationNumber)))
            {
                errors.Add("One or more records have no volume equation.");
            }
            if (equationList.Any(x => string.IsNullOrEmpty(x.Species)))
            {
                errors.Add("One or more records has no species");
            }
            if (equationList.Any(x => x.CalcBoard == 0 && x.CalcCubic == 0 && x.CalcCord == 0))
            {
                errors.Add("One or more records has no volume selected (Calc Board, Calc Cubic, Calc Cord)");
            }
            if ( equationList.Any(x => string.IsNullOrEmpty(x.PrimaryProduct)))
            {
                errors.Add("One or more records has no Primary Product");
            }
            if(errors.Any())
            {
                DialogService.ShowError(string.Join("\r\n", errors));
                return;
            }

            //  Otherwise, make sure user wants to save all entries

            var dResult = DialogService.AskYesNo("Do you want to save changes?", DialogServiceResult.Yes);
            if (dResult == DialogServiceResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                DataLayer.SaveVolumeEquations(equationList);

                if (equationList.Any(x => x.CalcBiomass > 0))
                {
                    //  Load biomass information if biomass flag was checked
                    //  But if it's an edited template then capture region and forest
                    //  before updating biomass.  March 2017
                    if (templateFlag == 1)
                    {
                        TemplateRegionForest trf = Services.GetRequiredService<TemplateRegionForest>();
                        trf.ShowDialog();
                        UpdateBiomassTemplate(equationList, trf.currentRegion, trf.currentForest);
                    }
                    else
                    {
                        UpdateBiomassCruise(equationList);
                    }
                }
                else
                {
                    //  remove all biomass equations
                    DataLayer.ClearBiomassEquations();
                }
                Cursor.Current = this.Cursor;
            }   //  endif
            

            Close();
        }

        private void onCancel(object sender, EventArgs e)
        {
            Close();
        }

        public void UpdateBiomassTemplate(List<VolumeEquationDO> equationList, string currRegion, string currForest)
        {
            //  capture percent removed
            var prList = DialogService.ShowPercentRemovedDialog(equationList);

            UpdateBiomassTemplate(DataLayer, equationList, currRegion, currForest, prList);
        }

        public static void UpdateBiomassTemplate(CpDataLayer dataLayer, IEnumerable<VolumeEquationDO> equationList, string currRegion, string currForest, IReadOnlyCollection<PercentRemoved> prList)
        {
            int REGN = Convert.ToInt32(currRegion);

            List<TreeDefaultValueDO> treeDef = dataLayer.getTreeDefaults();
            List<BiomassEquationDO> biomassEquations = new List<BiomassEquationDO>();

            foreach (VolumeEquationDO volEq in equationList)
            {
                if (volEq.CalcBiomass == 1)
                {
                    // find species/product in tree default values for FIA code
                    var percentRemoved = prList.FirstOrDefault(pr => pr.bioSpecies == volEq.Species && pr.bioProduct == volEq.PrimaryProduct);
                    float percentRemovedValue = (percentRemoved != null && float.TryParse(percentRemoved.bioPCremoved, out var pct))
                        ? pct : 0.0f;

                    var treeDefaults = treeDef.Where(td => td.Species == volEq.Species && td.PrimaryProduct == volEq.PrimaryProduct);

                    foreach(var tdv in treeDefaults)
                    {
                        var bioEqs = MakeBiomassEquations(volEq, REGN, currForest, (int)tdv.FIAcode, tdv.LiveDead, percentRemovedValue);

                        biomassEquations.AddRange(bioEqs);
                    }
                }   //  endif
            }   //  end foreach loop

            //  save list
            dataLayer.ClearBiomassEquations();
            dataLayer.SaveBiomassEquations(biomassEquations);
        }

        public void UpdateBiomassCruise(List<VolumeEquationDO> equationList)
        {
            //  if just one volume equation has biomass flag checked, need to capture percent removed for any or all
            var prList = DialogService.ShowPercentRemovedDialog(equationList);
            UpdateBiomassCruise(DataLayer, equationList, prList);
        }

        public static void UpdateBiomassCruise(CpDataLayer dataLayer, IEnumerable<VolumeEquationDO> equationList, IReadOnlyCollection<PercentRemoved> prList)
        {
            //  new variables for biomass call
            string currRegion = dataLayer.getRegion();
            string currForest = dataLayer.getForest();
            int REGN = Convert.ToInt32(currRegion);

            var biomassEquations = new List<BiomassEquationDO>();

            //  Region 8 does things so differently -- multiple species could be
            //  in the equation list so need to check for duplicates.  Otherwise, the biomass equations
            //  won't save.  February 2014

            //  update biomass equations
            foreach (VolumeEquationDO volEq in equationList.GroupBy(x => x.Species + ";" + x.PrimaryProduct).Select(x => x.First()))
            {
                if (volEq.CalcBiomass == 1)
                {
                    var percentRemoved = prList.FirstOrDefault(pr => pr.bioSpecies == volEq.Species && pr.bioProduct == volEq.PrimaryProduct);
                    float percentRemovedValue = (percentRemoved != null && float.TryParse(percentRemoved.bioPCremoved, out var pct))
                        ? pct : 0.0f;

                    var spProdLiveDeads = dataLayer.GetUniqueSpeciesProductLiveDeadFromTrees(volEq.Species, volEq.PrimaryProduct);
                    foreach (var spProdLiveDead in spProdLiveDeads)
                    {
                        var bioEqs = MakeBiomassEquations(volEq, REGN, currForest, spProdLiveDead.FiaCode, spProdLiveDead.LiveDead, percentRemovedValue);

                        biomassEquations.AddRange(bioEqs);
                    }

                }   //  endif biomass checked
            }   //  end foreach loop
            //  save list

            dataLayer.ClearBiomassEquations();
            dataLayer.SaveBiomassEquations(biomassEquations);
        }

        public static IEnumerable<BiomassEquationDO> MakeBiomassEquations(VolumeEquationDO volEq, int region, string forest, int fiaCode, string liveDead, float percentRemovedValue)
        {
            

            var biomassEquations = new List<BiomassEquationDO>();

            float[] WF = new float[3];
            var FORST = new StringBuilder(CRZSPDFTCS_STRINGLENGTH).Append(forest);
            var AGTEQ = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var LBREQ = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var DBREQ = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var FOLEQ = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var TIPEQ = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var WF1REF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var WF2REF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var MCREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var AGTREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var LBRREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var DBRREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var FOLREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var TIPREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            int REGN = region;
            int SPCD = fiaCode;
            CRZSPDFTCS(ref REGN,
                       FORST,
                       out fiaCode,
                       WF,
                       AGTEQ,
                       LBREQ,
                       DBREQ,
                       FOLEQ,
                       TIPEQ,
                       WF1REF,
                       WF2REF,
                       MCREF,
                       AGTREF,
                       LBRREF,
                       DBRREF,
                       FOLREF,
                       TIPREF,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH);
            // note: fiaCode is both an input and an output variable

            
            

            foreach(var comp in BIOMASS_COMPONENTS)
            {
                BiomassEquationDO bedo = new BiomassEquationDO();
                bedo.FIAcode = fiaCode;
                bedo.LiveDead = liveDead; // note: although biomass Eqs have live dead. Since during calculating tree value BioEq's arn't selected by LiveDead it doesn't matter what the LD is
                bedo.Product = volEq.PrimaryProduct;
                bedo.Species = volEq.Species;
                bedo.PercentMoisture = WF[2];
                bedo.PercentRemoved = percentRemovedValue;

                switch (comp)
                {
                    case "TotalTreeAboveGround":
                        {
                            bedo.Component = comp;
                            bedo.Equation = AGTEQ.ToString();
                            bedo.MetaData = AGTREF.ToString();
                            break;
                        }
                    case "LiveBranches":
                        {
                            bedo.Component = comp;
                            bedo.Equation = LBREQ.ToString();
                            bedo.MetaData = LBRREF.ToString();
                            break;
                        }
                    case "DeadBranches":
                        {
                            bedo.Component = comp;
                            bedo.Equation = DBREQ.ToString();
                            bedo.MetaData = DBRREF.ToString();
                            break;
                        }
                    case "Foliage":
                        {
                            bedo.Component = comp;
                            bedo.Equation = FOLEQ.ToString();
                            bedo.MetaData = FOLREF.ToString();
                            break;
                        }
                    case "PrimaryProd":
                        {
                            bedo.Component = comp;
                            bedo.Equation = "";
                            bedo.MetaData = WF1REF.ToString();

                            if (region == 5)
                            {
                                int[] R5_Prod20_WF_FIAcodes = new int[] { 122, 116, 117, 015, 020, 202, 081, 108 };
                                if (volEq.PrimaryProduct == "20"
                                    && R5_Prod20_WF_FIAcodes.Any(x => x == fiaCode))
                                {
                                    bedo.WeightFactorPrimary = WF[1];
                                }
                                else bedo.WeightFactorPrimary = WF[0];
                            }
                            else if (REGN == 1 && volEq.PrimaryProduct != "01")
                            {
                                bedo.WeightFactorPrimary = WF[1];
                            }
                            else
                            {
                                bedo.WeightFactorPrimary = WF[0];
                            }

                            
                            break;
                        }
                    case "SecondaryProd":
                        {
                            bedo.Component = comp;
                            bedo.Equation = "";
                            bedo.WeightFactorSecondary = WF[1];
                            bedo.MetaData = WF2REF.ToString();
                            break;
                        }
                    case "StemTip":
                        {
                            bedo.Component = comp;
                            bedo.Equation = TIPEQ.ToString();
                            bedo.MetaData = TIPREF.ToString();
                            break;
                        }
                }

                biomassEquations.Add( bedo );
            }

            return biomassEquations;
        }

        private List<VolumeEquationDO> updateEquationList(string[,] speciesProduct)
        {
            List<VolumeEquationDO> updatedList = new List<VolumeEquationDO>();
            for (int k = 0; k < speciesProduct.GetLength(0); k++)
            {
                int nthRow = equationList.FindIndex(
                    delegate (VolumeEquationDO v)
                    {
                        return v.Species == speciesProduct[k, 0] && v.PrimaryProduct == speciesProduct[k, 1];
                    });
                if (nthRow >= 0)
                    updatedList.Add(equationList[nthRow]);
            }   //  end for k loop

            return updatedList;
        }   //  end updateEquationList


        private void onDelete(object sender, EventArgs e)
        {
            DataGridViewRow row = volumeEquationList.SelectedRows[0];
            volumeEquationList.Rows.Remove(row);
        }   //  end onDelete

 



        private void getTemplateSpecies(object sender, EventArgs e)
        {
        }

        private void getTemplateProduct(object sender, EventArgs e)
        {
        }   //  end updateBiomass
    }
}