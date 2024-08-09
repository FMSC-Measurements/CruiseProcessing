using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
using FMSC.ORM.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace CruiseProcessing
{
    public partial class R9VolEquation : Form
    {
        public const double DEFAULT_CONIFER_TOPDIB = 7.6;
        public const double DEFATULT_HARDWOOD_TOPDIB = 9.6;
        public const double DEFAULT_NONSAW_TOPDIB = 4.0;


        public CpDataLayer DataLayer { get; }
        public IServiceProvider Services { get; }
        public IDialogService DialogService { get; }

        List<VolumeEquationDO> volList = new List<VolumeEquationDO>();
        ArrayList topwoodSpecies = new ArrayList();
        public ArrayList topwoodFlags = new ArrayList();
        List<JustDIBs> jstDIBs = new List<JustDIBs>();


        protected R9VolEquation()
        {
            InitializeComponent();
        }

        public R9VolEquation(CpDataLayer dataLayer, IDialogService dialogService, IServiceProvider services)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }


        public void setupDialog()
        {
            //  Initially, set default equations to whatever is in the volume equation table
            //  if empty, set to Clark
            volList = DataLayer.getVolumeEquations();
            if (volList.Count > 0)
            {
                if (volList.Any(x => x.VolumeEquationNumber.Substring(3, 3) == "DVE"))
                {
                    taperEquations.Checked = false;
                    topDIB.Enabled = false;
                    oldEquations.Checked = true;
                }
            }
            else
            {
                //  Need to save equations before user can specify topwood
                //  Remove all equations as they are rebuilt with either of these calls
                DataLayer.SaveVolumeEquations(volList);
                if (taperEquations.Checked == true) // defaults to ture when form is created
                    CreateEquations("900CLKE");
                else if (oldEquations.Checked == true)
                    CreateEquations("900DVEE");

                //  Save equations
                DataLayer.SaveVolumeEquations(volList);
            }   //  endif equations exist

            //  load lists for topwood portion
            foreach (VolumeEquationDO vel in volList)
            {
                if (vel.PrimaryProduct == "01")
                {
                    topwoodSpecies.Add(vel.Species);
                    topwoodFlags.Add(vel.CalcTopwood);
                }   //  endif sawtimber
            }
        }


        private void onTopwoodCalculation(object sender, EventArgs e)
        {
            R9Topwood r9top = Services.GetRequiredService<R9Topwood>();
            r9top.speciesList = topwoodSpecies;
            r9top.flagList = topwoodFlags;
            r9top.setupDialog();
            r9top.ShowDialog();
            topwoodSpecies = r9top.speciesList;
            topwoodFlags = r9top.flagList;
        }


        private void onTopDIB(object sender, EventArgs e)
        {
            //  Need to save equations before user can change DIBs
            //  Remove all equations as they are rebuilt with either of these calls
            //volList.Clear();
            //bslyr.fileName = fileName;
            //bslyr.SaveVolumeEquations(volList);
            //if (taperEquations.Checked == true)
            //    CreateEquations("900CLKE");
            //else if (oldEquations.Checked == true)
            //    CreateEquations("900DVEE");

            //  Save equations
            DataLayer.SaveVolumeEquations(volList);
            R9TopDIB r9DIB = Services.GetRequiredService<R9TopDIB>();
            r9DIB.setupDialog();
            r9DIB.Show();
            jstDIBs = r9DIB.jstDIB;
        }   //  end onTopDIB


        private void OnClarkChecked(object sender, EventArgs e)
        {
            oldEquations.Checked = false;
            topDIB.Enabled = true;
        }

        private void onGevorkCheck(object sender, EventArgs e)
        {
            taperEquations.Checked = false;
            topDIB.Enabled = false;
        }

        public void onFinished(object sender, EventArgs e)
        {
            if (volList.Any()
                && DialogService.AskYesNo("Cruise Already Contains Volume Equations\r\rWould you like to reset Volume Equations", DialogServiceResult.No) == DialogServiceResult.Yes)
            {
                Finish(calcBiomass.Checked, true);
            }
            else
            {
                Finish(calcBiomass.Checked, false);
            }

            Close();
        }


        public void Finish(bool calculateBiomass, bool clearExistingEquations)
        {
            if (clearExistingEquations)
            {
                DataLayer.deleteVolumeEquations();
                volList.Clear();
            }

            if (taperEquations.Checked == true)
                CreateEquations("900CLKE");
            else if (oldEquations.Checked == true)
                CreateEquations("900DVEE");

            //  Save equations
            DataLayer.SaveVolumeEquations(volList);
            if (calculateBiomass == true)
            {
                VolumeEquations ve = Services.GetRequiredService<VolumeEquations>();
                ve.UpdateBiomassCruise(volList);
            }   //  endif calculate biomass
        }


        private void CreateEquations(string eqnPrefix)
        {
            //  Need to capture DIBs before creating equations
            List<JustDIBs> existingVolEqDIBs = DataLayer.GetJustDIBs();

            //  Grab tree data grouped by unique species and products
            var speciesProds = DataLayer.GetUniqueSpeciesProductFromTrees();
            foreach(var spProd in speciesProds)
            {
                var species = spProd.SpeciesCode;
                var product = spProd.ProductCode;
                if (species == null) { continue; }

                // Check to see if this species/product combo already exists
                if (volList.Any(x => x.Species == species && x.PrimaryProduct == product)) { continue; }

                VolumeEquationDO vel = BuildVolumeEquation(eqnPrefix, existingVolEqDIBs, product, species);
                if (vel != null) { volList.Add(vel); }
            }
        }

        private VolumeEquationDO BuildVolumeEquation(string eqnPrefix, IReadOnlyCollection<JustDIBs> dibList, string currentProduct, string currentSpecies)
        {
            VolumeEquationDO vel = new VolumeEquationDO();
            //  Per Mike VanDyck --  cords flag is on for everything including sawtimber
            vel.CalcCord = 1;

            //  Set constant values
            vel.CalcTotal = 0;
            vel.StumpHeight = 0;
            vel.TopDIBPrimary = 0;
            vel.TopDIBSecondary = 0;
            vel.Trim = 0;
            vel.SegmentationLogic = 0;
            vel.MaxLogLengthPrimary = 0;
            vel.MinLogLengthPrimary = 0;
            vel.MinMerchLength = 0;

            //  build equation
            StringBuilder sb = new StringBuilder();
            sb.Append(eqnPrefix);
            //  Fix species code as needed
            if (currentSpecies.Length == 2)
                sb.Append("0");
            else if (currentSpecies.Length == 1)
                sb.Append("00");
            sb.Append(currentSpecies);
            vel.VolumeEquationNumber = sb.ToString();

            //  Set calculation flags as needed
            vel.CalcCubic = 1;
            vel.CalcTopwood = 0;
            if (currentProduct == "01")
            {
                vel.CalcBoard = 1;
                //  Is topwood status changed for this species
                if (topwoodSpecies.IndexOf(currentSpecies) >= 0)
                    vel.CalcTopwood = Convert.ToInt32(topwoodFlags[topwoodSpecies.IndexOf(currentSpecies)]);
            }
            else
                vel.CalcBoard = 0;

            if (eqnPrefix == "900DVEE")
            {
                //  Need to update DIB values here
                //  looks for current species and product in saved DIB list to replace as needed
                var dib = dibList.FirstOrDefault(x => x.speciesDIB == currentSpecies && x.productDIB == currentProduct);
                if (dib != null)
                {
                    vel.TopDIBPrimary = dib.primaryDIB;
                    vel.TopDIBSecondary = dib.secondaryDIB;
                }
            }
            else if (eqnPrefix == "900CLKE")
            {
                //  See if DIB exists in the old DIB list for this species
                var dib = dibList.FirstOrDefault(x => x.speciesDIB == currentSpecies);
                if (dib != null)
                {
                    vel.TopDIBPrimary = dib.primaryDIB;
                    vel.TopDIBSecondary = dib.secondaryDIB;
                }
                else
                {
                    var dib2 = jstDIBs.FirstOrDefault(x => x.speciesDIB == currentSpecies);

                    var topDIBPrimary = dib2?.primaryDIB ?? 0.0f;

                    if (topDIBPrimary == 0.0f!)
                    {
                        if (int.TryParse(currentSpecies, out var fiaCode))
                        {
                            topDIBPrimary = (fiaCode < 300) ? (float)DEFAULT_CONIFER_TOPDIB : (float)DEFATULT_HARDWOOD_TOPDIB;
                        }
                        else
                        {
                            var errorMessage = "Expected species to be FIA code but found " + currentSpecies + ". Unable to get default top DIB from species";
                            DialogService.ShowError(errorMessage);
                            Services.GetService<ILogger<R9VolEquation>>()?.LogWarning(errorMessage);
                            return null;
                        }
                    }

                    vel.TopDIBPrimary = topDIBPrimary;
                    vel.TopDIBSecondary = (dib2 != null && dib2.secondaryDIB > 0.0f) ? dib2.secondaryDIB : (float)DEFAULT_NONSAW_TOPDIB;
                }
            }   //  endif eqnPrefix


            vel.Species = currentSpecies;
            vel.PrimaryProduct = currentProduct;
            if (calcBiomass.Checked == true)
                vel.CalcBiomass = 1;
            else vel.CalcBiomass = 0;
            return vel;
        }

        private void UpdateDIBs(List<JustDIBs> oldDIBs, string currSpec, string currProd,
                                double updatedPrimaryDIB, double updatedSecondaryDIB)
        {
            //  looks for current species and product in saved DIB list to replace as needed
            int nthRow = oldDIBs.FindIndex(
                delegate (JustDIBs odib)
                {
                    return odib.speciesDIB == currSpec && odib.productDIB == currProd;
                });
            if (nthRow >= 0)
            {
                if (oldDIBs[nthRow].primaryDIB != 0.0)
                    updatedPrimaryDIB = oldDIBs[nthRow].primaryDIB;

                if (oldDIBs[nthRow].secondaryDIB != 0.0)
                    updatedSecondaryDIB = oldDIBs[nthRow].secondaryDIB;
            }   //  endif nthRow
            return;
        }   //  end UpdateDIBs
    }
}
