using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CruiseProcessing
{
    public partial class R8VolEquation : Form
    {
        public class ForestGeoCode
        {
            public string District { get; set; }
            public string GeoCode { get; set; }
            public string GroupCode { get; set; } // group code doesn't appear to be used
        }

        List<VolumeEquationDO> volList = new List<VolumeEquationDO>();
        string[] topwoodStatus = new string[30];
        int pulpwoodHeight = -1;

        //string[,] DIBbySpecies;
        List<JustDIBs> DIBbySpecies = new List<JustDIBs>();

        static readonly IReadOnlyDictionary<string, IReadOnlyCollection<ForestGeoCode>> ForestGeoCodeLookup = new Dictionary<string, IReadOnlyCollection<ForestGeoCode>>()
            {
                { "01", new[]
                    {
                        new ForestGeoCode { District = "01", GeoCode = "4", GroupCode = "13" },
                        new ForestGeoCode { District = "03", GeoCode = "1", GroupCode = "15" },
                        new ForestGeoCode { District = "04", GeoCode = "4", GroupCode = "16" },
                        new ForestGeoCode { District = "05", GeoCode = "4", GroupCode = "14" },
                        new ForestGeoCode { District = "06", GeoCode = "4", GroupCode = "14" },
                        new ForestGeoCode { District = "07", GeoCode = "4", GroupCode = "17" },
                    }
                },
                { "02", new[] { new ForestGeoCode { GeoCode = "3", GroupCode = "10" } } },
                {"03", new[]
                    {
                        new ForestGeoCode { GeoCode = "3", GroupCode = "01" },
                        new ForestGeoCode { District = "08", GeoCode = "2", GroupCode = "06" },
                    }
                },
                { "04", new[] { new ForestGeoCode { GeoCode = "3", GroupCode = "01" } } },
                { "05", new[] { new ForestGeoCode { GeoCode = "1", GroupCode = "03" } } },
                { "06", new[]
                    {
                        new ForestGeoCode { GeoCode = "5", GroupCode = "02" },
                        new ForestGeoCode { District = "06", GeoCode = "5", GroupCode = "09" },
                    }
                },
                { "07",
                    new[]
                    {
                        new ForestGeoCode { District = "01", GeoCode = "5", GroupCode = "19" },
                        new ForestGeoCode { District = "02", GeoCode = "5", GroupCode = "20" },
                        new ForestGeoCode { District = "04", GeoCode = "5", GroupCode = "21" },
                        new ForestGeoCode { District = "05", GeoCode = "5", GroupCode = "22" },
                        new ForestGeoCode { District = "06", GeoCode = "7", GroupCode = "18" },
                        new ForestGeoCode { District = "07", GeoCode = "4", GroupCode = "23" },
                        new ForestGeoCode { District = "17", GeoCode = "4", GroupCode = "23" },
                    }
                },
                { "08", new[]
                    {
                        new ForestGeoCode { GeoCode = "3", GroupCode = "11" },
                        new ForestGeoCode { District = "11", GeoCode = "3", GroupCode = "12" },
                        new ForestGeoCode { District = "12", GeoCode = "3", GroupCode = "12" },
                        new ForestGeoCode { District = "13", GeoCode = "3", GroupCode = "12" },
                        new ForestGeoCode { District = "14", GeoCode = "3", GroupCode = "12" },
                        new ForestGeoCode { District = "15", GeoCode = "3", GroupCode = "12" },
                        new ForestGeoCode { District = "16", GeoCode = "3", GroupCode = "12" },
                    }
                },
                { "09", new[]
                    {
                        new ForestGeoCode { GeoCode = "6", GroupCode = "31" },
                        new ForestGeoCode { District = "01", GeoCode = "6", GroupCode = "30" },
                        new ForestGeoCode { District = "06", GeoCode = "6", GroupCode = "30" },
                        new ForestGeoCode { District = "12", GeoCode = "6", GroupCode = "32" },
                    }
                },
                {
                    "10", new[]
                    {
                        new ForestGeoCode { GeoCode = "6", GroupCode = "04" },
                        new ForestGeoCode { District = "07", GeoCode = "7", GroupCode = "05" },
                    }
                },
                {
                    "11", new[]
                    {
                        new ForestGeoCode { GeoCode = "3", GroupCode = "01" },
                        new ForestGeoCode { District = "03", GeoCode = "1", GroupCode = "07" },
                        new ForestGeoCode { District = "10", GeoCode = "2", GroupCode = "08" },
                    }
                },
                {
                    "12", new[]
                    {
                        new ForestGeoCode { GeoCode = "2", GroupCode = "24" },
                        new ForestGeoCode { District = "02", GeoCode = "3", GroupCode = "01" },
                        new ForestGeoCode { District = "05", GeoCode = "1", GroupCode = "25" },
                    }
                },
                {
                    "13", new[]
                    {
                        new ForestGeoCode { District = "01", GeoCode = "5", GroupCode = "26" },
                        new ForestGeoCode { District = "03", GeoCode = "5", GroupCode = "27" },
                        new ForestGeoCode { District = "04", GeoCode = "5", GroupCode = "29" },
                        new ForestGeoCode { District = "07", GeoCode = "5", GroupCode = "28" },
                    }
                },
                { "60", new[] { new ForestGeoCode { GeoCode = "3", GroupCode = "01" } } },
                { "36", new[] { new ForestGeoCode { GeoCode = "1", GroupCode = "25" } } },
            };

        public static ForestGeoCode GetR8ForestGeoCode(string forest, string district)
        {
            if (!ForestGeoCodeLookup.TryGetValue(forest, out var geoCodes)) return null;
            return geoCodes.FirstOrDefault(x => x.District == district) ?? geoCodes.First(x => x.District == null);
        }

        protected CpDataLayer DataLayer { get; }
        public IServiceProvider Services { get; }
        public IDialogService DialogService { get; }

        protected R8VolEquation()
        {
            InitializeComponent();
        }

        public R8VolEquation(CpDataLayer dataLayer, IServiceProvider services, IDialogService dialogService)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }


        private void onTopwoodClick(object sender, EventArgs e)
        {
            //  setup dialog and get checked species in return
            R8Topwood r8top = Services.GetRequiredService<R8Topwood>();
            r8top.setupDialog();
            r8top.ShowDialog();
            topwoodStatus = r8top.checkStatus;
        }   //  end onTopwoodClick


        private void onOK(object sender, EventArgs e)
        {
            if(volList.Any()
                && DialogService.AskYesNo("Cruise Already Contains Volume Equations\r\rWould you like to reset Volume Equations", DialogServiceResult.No) == DialogServiceResult.Yes)
            {
                Finish(calcBiomass.Checked,  true);
            }
            else
            {
                Finish(calcBiomass.Checked, false);
            }
        }

        public void Finish(bool calculateBiomass, bool clearExistingEquations)
        {
            if(clearExistingEquations)
            {
                DataLayer.deleteVolumeEquations();
                volList.Clear();
            }

            if (oldClarkCheckBox.Checked == true && pulpwoodHeight < 0)
            {
                DialogService.ShowError("Pulpwood Height not selected\r\n Please click Old Clark Equations to select Pulpwood Height ");
                return;
            }

            var sale = DataLayer.GetSale();
            string currentForest = sale.Forest;
            string currentDistrict = sale.District ?? "";

            //  Look up geo code and group code for this forest and district (if any)
            //  First look in defaults
            var forestGeoCode = GetR8ForestGeoCode(currentForest, currentDistrict);

            //  if geocode and group code are null, means forest or district are incorrect
            if (forestGeoCode == null)
            {
                DialogService.ShowError("Could not find Forest and/or District number.\r\nCannot complete equations.");
                Close();
                return;
            }

            //  get unique species/product combinations
            var speciesProduct = DataLayer.GetUniqueSpeciesProductFromTrees();
            foreach (var spProd in speciesProduct)
            {
                // if we didn't clear equations,  check if this species/product combination already exists in volList
                if (!clearExistingEquations && volList.Any(x => x.Species == spProd.SpeciesCode && x.PrimaryProduct == spProd.ProductCode))
                {
                    continue;
                }

                //  change in volume library no longer has DVEE equations used for board for board foot volume
                //  so commented out the call to build those equations
                //  October 2015
                //if (currentProduct == "01")
                //  buildVolumeEquation(currGrpCode, currentSpecies, currentProduct);
                //  Build Clark equations -- old or new -- July 2017
                if (newClarkCheckBox.Checked == true)
                {
                    buildNewClarkEquations(forestGeoCode.GeoCode, spProd.SpeciesCode, spProd.ProductCode);
                }
                else if (oldClarkCheckBox.Checked == true)
                {
                    buildClarkEquation(forestGeoCode.GeoCode, spProd.SpeciesCode, spProd.ProductCode, pulpwoodHeight);
                }   //  endif
            }

            //  Save equations in database
            DataLayer.SaveVolumeEquations(volList);

            if (calculateBiomass)
            {
                VolumeEquations ve = Services.GetRequiredService<VolumeEquations>();
                ve.UpdateBiomassCruise(volList);
            }   //  endif calculate biomass
            Close();
        }


        private void buildVolumeEquation(string currGrpCode, string currentSpecies, string currentProduct)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("8");
            //  complete equation build and put in volList
            sb.Append(currGrpCode);
            sb.Append("DVEE");
            //  Fix incomplete species code
            if (currentSpecies.Length == 2)
                sb.Append("0");
            else if (currentSpecies.Length == 1)
                sb.Append("00");
            sb.Append(currentSpecies);

            //  set remaining values for a volume equation entry
            VolumeEquationDO vel = new VolumeEquationDO();
            vel.Species = currentSpecies;
            vel.PrimaryProduct = currentProduct;
            vel.VolumeEquationNumber = sb.ToString();
            vel.StumpHeight = 1;
            vel.TopDIBPrimary = 0;
            vel.TopDIBSecondary = 0;
            vel.CalcTotal = 0;
            vel.CalcBoard = 1;
            vel.CalcCubic = 0;
            vel.CalcCord = 0;
            vel.CalcTopwood = 0;
            if (calcBiomass.Checked == true)
                vel.CalcBiomass = 1;
            else vel.CalcBiomass = 0;
            vel.Trim = 0;
            vel.SegmentationLogic = 0;
            vel.MinLogLengthPrimary = 0;
            vel.MaxLogLengthPrimary = 0;
            vel.MinMerchLength = 0;

            volList.Add(vel);

            return;
        }   //  end buildVolumeEquation


        private void buildClarkEquation(string currGeoCode, string currentSpecies,
                                        string currentProduct, int pulpwoodHeight)
        {
            //  complete equation build and put in volList
            VolumeEquationDO vel = new VolumeEquationDO();
            vel.Species = currentSpecies;
            vel.PrimaryProduct = currentProduct;
            vel.StumpHeight = 1;
            vel.TopDIBPrimary = 0;
            vel.TopDIBSecondary = 0;
            vel.CalcTotal = 0;
            vel.CalcBoard = 0;
            vel.CalcCubic = 0;
            vel.CalcCord = 0;
            vel.CalcTopwood = 0;
            if (calcBiomass.Checked == true)
                vel.CalcBiomass = 1;
            else vel.CalcBiomass = 0;
            vel.Trim = 0;
            vel.SegmentationLogic = 0;
            vel.MinLogLengthPrimary = 0;
            vel.MaxLogLengthPrimary = 0;
            vel.MinMerchLength = 0;

            string refHeightTypeIndicator = "0";  // default to 0 for height to tip, prod specific heights set below

            //  finish equation
            switch (currentProduct)
            {
                case "01":
                    {
                        //  Set sawtimber reference height 7 or 9
                        if (int.TryParse(currentSpecies, out int fiaCodeInt))
                        {
                            if (fiaCodeInt < 300)
                            { refHeightTypeIndicator = "7"; } // pine species
                            else
                            { refHeightTypeIndicator = "9"; } // hardwood species
                        }
                        else
                        { throw new ArgumentException("Invalid species fia code: " + currentSpecies, nameof(currentSpecies)); }

                        vel.CalcCubic = 1;
                        //  added per request from Gary Church
                        vel.CalcBoard = 1;

                        //  topwood included?
                        //  find current species in topwoodStatus
                        if (topwoodStatus.Any(x => x == currentSpecies))
                        {
                            vel.CalcTopwood = 1;
                        }

                        break;
                    }

                case "02":
                    {
                        //  set pulpwood reference height 4 or 0
                        //  based on selection from first window
                        if (pulpwoodHeight == 0)
                        {
                            // 4 inch DOB for all species
                            refHeightTypeIndicator = "4";
                        }
                        else if (pulpwoodHeight == 1)
                        {
                            //  total height all species
                            refHeightTypeIndicator = "0";
                        }
                        else if (pulpwoodHeight == 2)
                        {
                            if (int.TryParse(currentSpecies, out int fiaCodeInt))
                            {
                                if (fiaCodeInt < 300)
                                { refHeightTypeIndicator = "0"; } // pine species
                                else
                                { refHeightTypeIndicator = "4"; } // hardwood species
                            }
                            else
                            { throw new ArgumentException("Invalid species fia code: " + currentSpecies, nameof(currentSpecies)); }

                        }   //  endif

                        vel.StumpHeight = Convert.ToSingle(0.5);
                        vel.CalcCubic = 1;
                        //  added per request from Gary Church
                        vel.CalcBoard = 1;
                        break;
                    }

                case "08":
                    {
                        //  find any changed DIB for current species
                        var dib = LookUpDIB(currentSpecies, out var secDIB);
                        vel.StumpHeight = 0.5f;
                        vel.TopDIBPrimary = dib;
                        vel.TopDIBSecondary = secDIB;
                        vel.CalcCubic = 1;
                        vel.CalcTopwood = 1;
                        //  added per request from Gary Church
                        vel.CalcBoard = 1;
                        refHeightTypeIndicator = "8";
                        break;
                    }

                default:
                    //  any other product code such as 20 for biomass
                    refHeightTypeIndicator = "0";
                    vel.CalcCubic = 1;
                    vel.CalcBoard = 1;
                    break;
            }   //  endif currentProduct

            vel.VolumeEquationNumber = BuildVolumeEquationNumber("8", currGeoCode + refHeightTypeIndicator, "CLK", "E", currentSpecies);
            volList.Add(vel);
        }   //  end buildClarkEquation


        private void buildNewClarkEquations(string currGeoCode, string currSpecies, string currProduct)
        {
            //  For new Clark equations only
            //  July 2017

            //  complete equation and put in volList
            VolumeEquationDO vel = new VolumeEquationDO();
            vel.Species = currSpecies;
            vel.PrimaryProduct = currProduct;
            vel.StumpHeight = (currProduct == "01") ? 1 : 0.5f;
            vel.TopDIBPrimary = 0;
            vel.TopDIBSecondary = 0;
            vel.CalcTotal = 0;
            vel.CalcBoard = 1;
            vel.CalcCubic = 1;
            vel.CalcCord = 0;
            vel.CalcTopwood = topwoodStatus.Any(x => x == currSpecies) ? 1 : 0;
            vel.CalcBiomass = (calcBiomass.Checked == true) ? 1 : 0;


            //  Oct 2018 -- decision made that whatever DIB/DOB is ent
            // to the volume library is whatever user entered for those values.
            //  set top dib values
            //            if (Convert.ToInt32(currSpecies) < 300)
            //          {
            //            vel.TopDIBPrimary = 7;
            //          vel.TopDIBSecondary = 4;
            //    }
            //  else
            //            {
            //              vel.TopDIBPrimary = 9;
            //            vel.TopDIBSecondary = 4;
            //      }   //  endif

            //  find any changed DIB for current species
            //  did user even use the top DOB button?
            var dib = LookUpDIB(currSpecies, out var secDIB);
            vel.TopDIBPrimary = dib;
            vel.TopDIBSecondary = secDIB;

            vel.VolumeEquationNumber = BuildVolumeEquationNumber("8", currGeoCode + "1", "CLK", "E", currSpecies);
            volList.Add(vel);
        }

        private string BuildVolumeEquationNumber(string geoCode, string subRegionCode, string model, string coastalCode, string speciesCode)
        {
            if (speciesCode.Length > 3)
            {
                throw new ArgumentException("Species code must be 3 characters or less", nameof(speciesCode));
            }

            speciesCode = speciesCode.PadLeft(3, '0');

            return $"8{geoCode}{subRegionCode}{model}{coastalCode}{speciesCode}";
        }


        private float LookUpDIB(string currentSpecies, out float secondaryDIB)
        {
            if(DIBbySpecies == null)
            {
                secondaryDIB = 0;
                return 0;
            }

            //  find any changed DIB for this species
            var dib = DIBbySpecies.FirstOrDefault(x => x.speciesDIB == currentSpecies);
            if(dib != null)
            {
                secondaryDIB = dib.secondaryDIB;
                return dib.primaryDIB;
            }
            else
            {
                secondaryDIB = 0;
                return 0;
            }
        }   //  end findDIB


        private void onCancel(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to cancel?\nAny changes made will not be saved.", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                Close();
                return;
            }
        }

        private void onTopDIBSclick(object sender, EventArgs e)
        {
            //  July 2017 -- changes to Region 8 volume equations
            //  This no longer looks for just product 08 tree but
            //  the window display is the same for all products
            //  just didn't rename the routine.
            //            R8product08 r8prod08 = new R8product08();
            //            r8prod08.bslyr.fileName = bslyr.fileName;
            //            r8prod08.bslyr.DAL = bslyr.DAL;
            //            int rResult = r8prod08.setupDialog();
            //            if (rResult == 1)
            //            {
            //                r8prod08.ShowDialog();
            //                DIBbySpecies = r8prod08.speciesDIB;
            //            }   //  endif rResult
            //  July 2018 -- per Gary Scott
            //  use this dialog for region 8 instead
            // of the code above
            //  Save equations
            R9TopDIB r9DIB = Services.GetRequiredService<R9TopDIB>();
            r9DIB.setupDialog();
            r9DIB.Show();
            DIBbySpecies = r9DIB.jstDIB;

        }

        private void onNewClark(object sender, EventArgs e)
        {
            oldClarkCheckBox.Checked = false;
        }   //  end onNewClark

        private void onOldClark(object sender, EventArgs e)
        {
            newClarkCheckBox.Checked = false;
            //  capture pulpwood height measurement
            R8PulpwoodMeasurement pm = Services.GetRequiredService<R8PulpwoodMeasurement>();
            pm.ShowDialog();
            pulpwoodHeight = pm.pulpHeight;
            return;
        }   //  end onOldClark

    }
}
