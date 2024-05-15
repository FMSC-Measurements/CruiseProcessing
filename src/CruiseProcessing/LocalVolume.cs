using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using System.Runtime.InteropServices;
using CruiseProcessing.Services;
using CruiseProcessing.Data;

namespace CruiseProcessing
{
    public partial class LocalVolume : Form
    {
        [DllImport("LocalVolume.dll", EntryPoint = "?GetLocalTable@CMakeLocalVolumeTable@@QAEXQAMQANHHPAN22PAM3PAH@Z", CallingConvention = CallingConvention.StdCall)]
        extern static void GetLocalTable(float[] DBHarray, double[] VolArray, int nItem, int TitleCode, ref double coef1, ref double coef2,
                            ref double coef3, ref float meanSqEr, ref float rSquared, ref int ModelCode);


        private string UOMtoUse = "";
        private string volType = "";
        private string volumeToUse = "None";
        private int totalTrees = 0;
        private bool topwoodRegress = false;
        private List<RegressGroups> rgList = new List<RegressGroups>();
        private List<RegressionDO> resultsList = new List<RegressionDO>();
        private StringBuilder concatSpecies = new StringBuilder();
        private string concatProduct;
        private string concatLiveDead;
        private double coef1 = 0;
        private double coef2 = 0;
        private double coef3 = 0;
        private float meanSqEr = 0;
        private float rSquared = 0;
        private int TitleCode = 0;
        private int ModelCode = 0;
        private static StringBuilder MainTitle = new StringBuilder(256);
        private static StringBuilder MainTitle_TW = new StringBuilder(256);
        

        protected CpDataLayer DataLayer {get;}
        public IDialogService DialogService { get; }

        protected LocalVolume()
        {
            InitializeComponent();
        }

        public LocalVolume(CpDataLayer dataLayer, IDialogService dialogService)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        public void setupDialog()
        {

            //  set useNet to default selection
            useNet.Checked = true;
            volumeToUse = "Net";
            //  then we need the unique species/product groups plus live/dead for the grid
            rgList = DataLayer.GetUniqueSpeciesGroups();
            SpeciesGroups.DataSource = rgList;

            //  remove existing regressions in table
            DataLayer.DeleteRegressions();
        }   //  end setupDialog


        private void grossClicked(object sender, EventArgs e)
        {
            volumeToUse = "Gross";
        }   //  end grossClicked

        private void netClicked(object sender, EventArgs e)
        {
            volumeToUse = "Net ";
        }   //  end netClicked

        private void regressTopwoodClicked(object sender, EventArgs e)
        {
            topwoodRegress = regressTopwood.Checked;
        }   //  end regressTopwodClicked

        private void displayHelp(object sender, EventArgs e)
        {
            //  display message box with information on topwood regression
            StringBuilder helpMessage = new StringBuilder();
            helpMessage.Append("Topwood is not regressed directly because there ");
            helpMessage.Append("is little correlation between DBH and topwood.  ");
            helpMessage.Append("Instead, topwood is added to the mainstem volume ");
            helpMessage.Append("and the regression is made on the total.");
            helpMessage.Append("\n\n");
            helpMessage.Append("To determine the amount of topwood, the mainstem ");
            helpMessage.Append("regression result is subtracted from the topwood ");
            helpMessage.Append("regression result.");
            MessageBox.Show(helpMessage.ToString(), "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }   //  end displayHelp

        private void onRegression(object sender, EventArgs e)
        {
            //  because of reset selection button, need to clear the concatenated fields nefore doing regression
            concatSpecies.Remove(0,concatSpecies.Length);
            concatLiveDead = "";
            concatProduct = "";

            //  make sure a UOM has been selected
            if (volType == "")
            {
                MessageBox.Show("Please select a valid UOM.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                UnitOfMeasure.Focus();
                return;
            }   //  endif

            //  make sure at least one speciesGroup was selected
            int anySelected = rgList.Sum(r => r.rgSelected);
            if (anySelected == 0)
            {
                MessageBox.Show("No species groups selected.\nPlease select at least one group to process.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }   //  endif

            MainTitle_TW.Remove(0, MainTitle_TW.Length);
            MainTitle.Remove(0, MainTitle.Length);
            //  setup title for graph
            if (topwoodRegress)
            {
                MainTitle_TW.Append(volumeToUse);
                MainTitle_TW.Append(volType);
                MainTitle.Append(volumeToUse);
                MainTitle.Append(volType);
                MainTitle_TW.Append(" (Primary + Secondary)");
                MainTitle.Append(" Primary");
                
            }
            else if (!topwoodRegress)
            {
                MainTitle.Append(volumeToUse);
                MainTitle.Append(volType);
                MainTitle.Append(" Primary");
            }   //  endif topwood

            //  pull data for species selected
            List<float> justDBH = new List<float>();
            List<double> justVol = new List<double>();
            List<double> justVol_TW = new List<double>();
            concatLiveDead = "";
            concatProduct = "";
            concatSpecies.Remove(0, concatSpecies.Length);
            foreach (RegressGroups rl in rgList)
            {
                if (rl.rgSelected == 1)
                {
                    //  pull trees
                    List<TreeCalculatedValuesDO> justTrees = DataLayer.getRegressTrees(rl.rgSpecies, rl.rgProduct, rl.rgLiveDead, "M");

                    //  load up arrays
                    foreach (TreeCalculatedValuesDO jt in justTrees)
                    {
                        // load DBH or DRC
                        float jd = new float();
                        if (jt.Tree.DBH > 0)
                            jd = jt.Tree.DBH;
                        else if (jt.Tree.DRC > 0)
                            jd = jt.Tree.DRC;
                        justDBH.Add(jd);

                        //  load volume  for primary regression
                        double jv = new double();
                        //  need primary whether topwood checked or not
                        switch (volType)
                        {
                            case "BDFT":
                                if (volumeToUse == "Gross")
                                    jv = jt.GrossBDFTPP;
                                else if (volumeToUse == "Net")
                                    jv = jt.NetBDFTPP;
                                break;
                            case "CUFT":
                                if (volumeToUse == "Gross")
                                    jv = jt.GrossCUFTPP;
                                else if (volumeToUse == "Net")
                                    jv = jt.NetCUFTPP;
                                break;
                            case "CORDS":
                                jv = jt.CordsPP;
                                break;
                        }   //  end switch
                        justVol.Add(jv);
                        
                        if(topwoodRegress)
                        {
                            switch(volType)
                            {
                                case "BDFT":
                                    if(volumeToUse == "Gross")
                                        jv = jt.GrossBDFTSP;
                                    else if(volumeToUse == "Net")
                                        jv = jt.NetBDFTSP;
                                    break;
                                case "CUFT":
                                    if(volumeToUse == "Gross")
                                        jv = jt.GrossCUFTSP;
                                    else if(volumeToUse == "Net")
                                        jv = jt.NetCUFTSP;
                                    break;
                                case "CORDS":
                                    jv = jt.CordsSP;
                                    break;
                            }   //  end switch
                            justVol_TW.Add(jv);
                        }   //  endif topwood
                    }   //  end foreach loop on trees
                    //  update species/product/livedead for current group
                    updateHeaderInfo(rl);
                }   //  endif selected
            }   //  end foreach on groups

            //  load arrays to pass to regression
            float[] DBHarray = new float[justDBH.Count];
            double[] VolArray = new double[justVol.Count];
            int nthItem = 0;
            foreach (float jd in justDBH)
            {
                DBHarray[nthItem] = jd;
                nthItem++;
            }   //  end foreach loop
            nthItem = 0;
            foreach (double jv in justVol)
            {
                VolArray[nthItem] = jv;
                nthItem++;
            }   //  end foreach loop

            //  call regression
            //  need to create model code to send for graph title
            switch (MainTitle.ToString())
            {
                case "GrossCUFT Primary":
                    TitleCode = 1;
                    break;
                case "NetCUFT Primary":
                    TitleCode = 2;
                    break;
                case "GrossBDFT Primary":
                    TitleCode = 3;
                    break;
                case "NetBDFT Primary":
                    TitleCode = 4;
                    break;
                case "Cords Primary":
                    TitleCode = 5;
                    break;
            }   //  end switch on MainTitle

            //  Call local volume library for primary regression
            if (nthItem < 3)
            {
                MessageBox.Show("Too few trees.  Need more than two trees\nto run regression analysis.", "WARING", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            GetLocalTable(DBHarray, VolArray, nthItem, TitleCode, ref coef1, ref coef2, ref coef3, ref meanSqEr, ref rSquared, ref ModelCode);

                //  create DBH class list
                float minDBH = DBHarray.Min();
                float maxDBH = DBHarray.Max();

            if (ModelCode != 0)
            {
                //  update regression results
                updateResults(volType, "Primary", concatSpecies, concatProduct, concatLiveDead, minDBH, maxDBH);
            }   //  endif cancel was clicked


            // then regress secondary
            if (topwoodRegress)
            {
                //  add TW to primary
                for (int j = 0; j < justVol_TW.Count; j++)
                    justVol[j] += justVol_TW[j];

                //  then dump into array to pass to regression
                Array.Clear(VolArray, 0, VolArray.Length);
                nthItem = 0;
                foreach (double jv in justVol)
                {
                    VolArray[nthItem] = jv;
                    nthItem++;
                }   //  end foreach loop

                //  change main title for secondary
                switch (MainTitle_TW.ToString())
                {
                    case "GrossCUFT (Primary + Secondary)":
                        TitleCode = 11;
                        break;
                    case "NetCUFT (Primary + Secondary)":
                        TitleCode = 21;
                        break;
                    case "GrossBDFT (Primary + Secondary)":
                        TitleCode = 31;
                        break;
                    case "NetBDFT (Primary + Secondary)":
                        TitleCode = 41;
                        break;
                    case "Cords (Primary + Secondary)":
                        TitleCode = 51;
                        break;
                }   //  end switch on MainTitle
                
                //  call regression for secondary
                GetLocalTable(DBHarray, VolArray, nthItem, TitleCode, ref coef1, ref coef2, ref coef3, ref meanSqEr, ref rSquared, ref ModelCode);

                if (ModelCode != 0)
                {
                    //  min and max DBH will be the same
                    //  update results list
                    updateResults(volType, "Secondary", concatSpecies, concatProduct, concatLiveDead, minDBH, maxDBH);
                }   //  endif cancel was clicked
            }   //  endif topwood

            //  Save results
            DataLayer.SaveRegress(resultsList);

            return;
        }   //  end onRegression


        private void onFinished(object sender, EventArgs e)
        {
            //  call class to generate reports
            LocalVolumeReports lvr = new LocalVolumeReports(DataLayer, DialogService);

            int nResult = lvr.OutputLocalVolume(DataLayer.FilePath);
            if (nResult == 0)
            {
                MessageBox.Show("Regression results have been added to the text output file.", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                return;
            }
            else
            {
                Close();
                return;
            }   //  endif
        }   //  end onFinished


        private void onIndexChanged(object sender, EventArgs e)
        {
            UOMtoUse = UnitOfMeasure.SelectedItem.ToString();

            //  May 2015 -- changed UOM to a selection by user, meaning they can choose boards, cords or cubic
            //  Per K.Cormier, drop weight since weight would be a scaled sale instead of tree measurement and
            //  they wouldn't regress weight.  Some regions (like 6) have different rules but would still not
            //  regress directly on weight but would select probably CUFT for regression.
            double totalVolume = 0;
            List<TreeCalculatedValuesDO> checkVolume = DataLayer.getTreeCalculatedValues();
            switch (UOMtoUse)
            {
                case "03 -- Cubic foot":
                    totalVolume = checkVolume.Sum(c => c.GrossCUFTPP);
                    volType = "CUFT";
                    break;
                case "01 -- Board foot":
                    totalVolume = checkVolume.Sum(c => c.GrossBDFTPP);
                    volType = "BDFT";
                    break;
                case "02 -- Cords":
                    totalVolume = checkVolume.Sum(c => c.CordsPP);
                    volType = "CORDS";
                    break;
                default:
                    volType = "";
                    break;
            }   //  end switch
            //  test volume to ensure it has been calculated before continuing
            if (totalVolume == 0)
            {
                MessageBox.Show("Selected UOM has no calculated volume.\nCannot continue with regression analysis.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }   //  endif
            //  per K.COrmier, lock down the UOM once it has been selected
            UnitOfMeasure.Enabled = false;
        }   //  end onIndexChanged


        private void groupSelected(object sender, DataGridViewCellEventArgs e)
        {
            List<TreeDO> tList = DataLayer.getTrees();
            List<TreeDO> justGroups = new List<TreeDO>();
            //  pull group from tree data and update number of trees selected
            int nthRow = SpeciesGroups.CurrentCell.RowIndex;
            string currSP = SpeciesGroups.Rows[nthRow].Cells[1].Value.ToString();
            string currPP = SpeciesGroups.Rows[nthRow].Cells[2].Value.ToString();
            string currLD = SpeciesGroups.Rows[nthRow].Cells[3].Value.ToString();
            //  pull volume for group selected
            justGroups = tList.FindAll(
                delegate(TreeDO t)
                {
                    return t.CountOrMeasure == "M" && t.Species == currSP &&
                        t.SampleGroup.PrimaryProduct == currPP &&
                        t.LiveDead == currLD;
                });
            object chk = SpeciesGroups.Rows[nthRow].Cells[0].GetEditedFormattedValue(nthRow, DataGridViewDataErrorContexts.CurrentCellChange);
            if((bool)chk == true)
            {
                //  set selected in rgList
                rgList[nthRow].rgSelected = 1;

                //  increment number of trees
                totalTrees += justGroups.Count();
                numOtrees.Text = totalTrees.ToString();
            }
            //else if (chk.Selected == false)
            else if((bool)chk == false)
            {
                //  decrement tree counts
                totalTrees -= justGroups.Count();
                rgList[nthRow].rgSelected = 0;
                if (totalTrees < 0)
                {
                    MessageBox.Show("Cannot have zero or less than zero number of trees.\nSelect one or more groups.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numOtrees.Text = "0";
                    return;
                }
                else numOtrees.Text = totalTrees.ToString();
            }   //  endif
            return;
        }   //  end groupSelected


        private void updateHeaderInfo(RegressGroups currGRP)
        {
            //  any species recorded yet?
            if(concatSpecies.Length == 0)
                concatSpecies.Append(currGRP.rgSpecies);
            else
            {
                //  convert concatSpecies to a string to find duplicate species
                string tempSpecies = concatSpecies.ToString();
                bool nthResult = tempSpecies.Contains(currGRP.rgSpecies);
                if (!nthResult)
                {
                    concatSpecies.Append("/");
                    concatSpecies.Append(currGRP.rgSpecies);
                }
            }   //  endif

            if (concatProduct == null || concatProduct == "")
                concatProduct += currGRP.rgProduct;
            else if(!concatProduct.Contains(currGRP.rgProduct) )
            {
                concatProduct += "/";
                concatProduct += currGRP.rgProduct;
            }   //  endif

            if (concatLiveDead == null || concatLiveDead == "")
                concatLiveDead += currGRP.rgLiveDead;
            else if(!concatLiveDead.Contains(currGRP.rgLiveDead))
            {
                concatLiveDead += "/";
                concatLiveDead += currGRP.rgLiveDead;
            }   //  endif
            return;
        }   //  end updateHeaderInfo


        private void updateResults(string volType, string POT, StringBuilder currSP, string currPP, string currLD, float currMin, float currMax)
        {
            RegressionDO rr = new RegressionDO();
            rr.rVolume = volumeToUse + volType;
            rr.rVolType = POT;
            rr.rSpeices = currSP.ToString();
            rr.rProduct = currPP;
            rr.rLiveDead = currLD;
            rr.CoefficientA = (float) coef1;
            rr.CoefficientB = (float) coef2;
            rr.CoefficientC = (float) coef3;
            rr.TotalTrees = totalTrees;
            rr.MeanSE = meanSqEr;
            rr.Rsquared = rSquared;
            rr.rMinDbh = currMin;
            rr.rMaxDbh = currMax;
            //  translate model code for model name
            switch(ModelCode)
            {
                case 1:
                    rr.RegressModel = "Linear";
                    break;
                case 2:
                    rr.RegressModel = "Quadratic";
                    break;
                case 3:
                    rr.RegressModel = "Log";
                    break;
                case 4:
                    rr.RegressModel = "Power";
                    break;
            }   //  end switch
            resultsList.Add(rr);
            return;
        }   //  end updateResults


        private void onResetSelection(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in SpeciesGroups.Rows)
            {
                DataGridViewCheckBoxCell chkBox = (DataGridViewCheckBoxCell)row.Cells[0];
                chkBox.Value = chkBox.FalseValue;
            }   //  end foreach loop
            totalTrees = 0;
            numOtrees.Text = "";
            //  also need to clera selected from rgList
            foreach (RegressGroups r in rgList)
                r.rgSelected = 0;
            return;
        }   //  end onResetSelection
    }
}
