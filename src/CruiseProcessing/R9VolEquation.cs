using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using Microsoft.Extensions.DependencyInjection;


namespace CruiseProcessing
{
    public partial class R9VolEquation : Form 
    {
        public CPbusinessLayer DataLayer { get; }
        public IServiceProvider Services { get; }

        List<VolumeEquationDO> volList = new List<VolumeEquationDO>();
        ArrayList topwoodSpecies = new ArrayList();
        ArrayList topwoodFlags = new ArrayList();
        List<JustDIBs> jstDIBs = new List<JustDIBs>();
        double defaultConiferTop = 7.6;
        double defaultHardwoodTop = 9.6;
        double defaultNonSaw = 4.0;

        protected R9VolEquation()
        {
            InitializeComponent();
        }

        public R9VolEquation(CPbusinessLayer dataLayer, IServiceProvider services)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }


        public void setupDialog()
        {
            //  Initially, set default equations to whatever is in the volume equation table
            //  if empty, set to Clark
            volList = DataLayer.getVolumeEquations();
            if (volList.Count > 0)
            {
                foreach (VolumeEquationDO vel in volList)
                {
                    string firstEquation = vel.VolumeEquationNumber.ToString();
                    if (firstEquation.Substring(3, 3) == "DVE")
                    {
                        taperEquations.Checked = false;
                        topDIB.Enabled = false;
                        oldEquations.Checked = true;
                        break;
                    }   //  endif
                }   //  end foreach
            }
            else if (volList.Count == 0)
            {
                //  Need to save equations before user can specify topwood
                //  Remove all equations as they are rebuilt with either of these calls
                DataLayer.SaveVolumeEquations(volList);
                if (taperEquations.Checked == true)
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
            }   //  end foreach loop
            return;
        }   //  end setupDialog


        private void onTopwoodCalculation(object sender, EventArgs e)
        {
            R9Topwood r9top = Services.GetRequiredService<R9Topwood>();
            r9top.speciesList = topwoodSpecies;
            r9top.flagList = topwoodFlags;
            r9top.setupDialog();
            r9top.ShowDialog();
            topwoodSpecies = r9top.speciesList;
            topwoodFlags = r9top.flagList;
        }   //  end onTopwoodCalculation


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

        private void onFinished(object sender, EventArgs e)
        {
            //  Remove all equations as they are rebuilt with either of these calls
            volList.Clear();
            DataLayer.SaveVolumeEquations(volList);
            if (taperEquations.Checked == true)
                CreateEquations("900CLKE");
            else if (oldEquations.Checked == true)
                CreateEquations("900DVEE");

            //  Save equations
            DataLayer.SaveVolumeEquations(volList);
            if(calcBiomass.Checked == true)
            {
                VolumeEquations ve = Services.GetRequiredService<VolumeEquations>();
                ve.updateBiomass(volList);
            }   //  endif calculate biomass
            Close();
            return;
        }   //  end onFinished


        private void CreateEquations(string eqnPrefix)
        {

            string currentProduct;
            string currentSpecies;

            //  Need to capture DIBs before creating equations
            List<JustDIBs> oldDIBs = DataLayer.GetJustDIBs();

            //  Grab tree data grouped by unique species and products
            string[,] speciesProduct;
            speciesProduct = DataLayer.GetUniqueSpeciesProduct();

            StringBuilder sb = new StringBuilder();
            double updatedPrimaryDIB = 0.0;
            double updatedSecondaryDIB = 0.0;
            for(int k=0;k<speciesProduct.GetLength(0);k++)
            {
                if (speciesProduct[k, 0] != null)
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

                    currentProduct = speciesProduct[k, 1];
                    currentSpecies = speciesProduct[k, 0];

                    //  build equation
                    sb.Clear();
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
                        UpdateDIBs(oldDIBs, currentSpecies, currentProduct, updatedPrimaryDIB, updatedSecondaryDIB);
                        vel.TopDIBPrimary = Convert.ToSingle(updatedPrimaryDIB);
                        vel.TopDIBSecondary = Convert.ToSingle(updatedSecondaryDIB);
                    }
                    else if (eqnPrefix == "900CLKE")
                    {
                        //  See if DIB exists in the old DIB list for this species
                        int nthRow = oldDIBs.FindIndex(
                            delegate(JustDIBs jds)
                            {
                                return jds.speciesDIB == currentSpecies;
                            });
                        if (nthRow >= 0)
                        {
                            vel.TopDIBPrimary = Convert.ToSingle(oldDIBs[nthRow].primaryDIB);
                            vel.TopDIBSecondary = Convert.ToSingle(oldDIBs[nthRow].secondaryDIB);
                        }
                        else if (nthRow < 0)
                        {
                            //  Was it updated by the user?
                            int mthRow = jstDIBs.FindIndex(
                                delegate(JustDIBs jds)
                                {
                                    //  based on conversation with M.VanDyck, Sept 2014, any DIB change applies to
                                    // any primary product value, sawtimber or non-sawtimber.  Removed check for product.
                                    return jds.speciesDIB == currentSpecies;
                                });
                            if (mthRow >= 0)
                            {
                                //  Sawtimber
                                if (jstDIBs[mthRow].primaryDIB == 0.0)
                                {
                                    //  set to default
                                    if (Convert.ToInt32(currentSpecies) < 300)
                                        vel.TopDIBPrimary = Convert.ToSingle(defaultConiferTop);
                                    else vel.TopDIBPrimary = Convert.ToSingle(defaultHardwoodTop);
                                }
                                else vel.TopDIBPrimary = Convert.ToSingle(jstDIBs[mthRow].primaryDIB);

                                //  Non-saw
                                if (jstDIBs[mthRow].secondaryDIB == 0.0)
                                    vel.TopDIBSecondary = Convert.ToSingle(defaultNonSaw);
                                else vel.TopDIBSecondary = Convert.ToSingle(jstDIBs[mthRow].secondaryDIB);
                            }
                            else if (mthRow < 0)
                            {
                                //  set to defaults
                                if (Convert.ToInt32(currentSpecies) < 300)
                                    vel.TopDIBPrimary = Convert.ToSingle(defaultConiferTop);
                                else vel.TopDIBPrimary = Convert.ToSingle(defaultHardwoodTop);

                                vel.TopDIBSecondary = Convert.ToSingle(defaultNonSaw);
                            }   //  endif mthRow
                        }   //  endif nthRow
                    }   //  endif eqnPrefix
                    vel.Species = currentSpecies;
                    vel.PrimaryProduct = currentProduct;
                    if (calcBiomass.Checked == true)
                        vel.CalcBiomass = 1;
                    else vel.CalcBiomass = 0;
                    volList.Add(vel);
                }   //  endif not blank or null
            }   //  end for k loop

            return;
        }   //  end CreateEquations



        private void UpdateDIBs(List<JustDIBs> oldDIBs,string currSpec, string currProd,
                                double updatedPrimaryDIB, double updatedSecondaryDIB)
        {
            //  looks for current species and product in saved DIB list to replace as needed
            int nthRow = oldDIBs.FindIndex(
                delegate(JustDIBs odib)
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
