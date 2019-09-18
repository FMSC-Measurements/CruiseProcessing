using System;
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
    public partial class R8VolEquation : Form
    {
        #region
        public string fileName;
        public float secDIB;
        List<VolumeEquationDO> volList = new List<VolumeEquationDO>();
        string[] topwoodStatus = new string[30];
        int pulpwoodHeight = -1;
        //string[,] DIBbySpecies;
        List<JustDIBs> DIBbySpecies = new List<JustDIBs>();
        string[,] forestDefaultList = new string[12,3] {{"02","3","10"},
                                                        {"03","3","01"},
                                                        {"04","3","01"},
                                                        {"05","1","03"},
                                                        {"06","5","02"},
                                                        {"08","3","11"},
                                                        {"09","6","31"},
                                                        {"10","6","04"},
                                                        {"11","3","01"},
                                                        {"12","2","24"},
                                                        {"60","3","01"},
                                                        {"36","1","25"}};
        string[,] forestDistrictList = new string[33,4] {{"01","01","4","13"},
                                                         {"01","03","1","15"},
                                                         {"01","04","4","16"},
                                                         {"01","05","4","14"},
                                                         {"01","06","4","14"},
                                                         {"01","07","4","17"},
                                                         {"03","08","2","06"},
                                                         {"06","06","5","09"},
                                                         {"07","01","5","19"},
                                                         {"07","02","5","20"},
                                                         {"07","04","5","21"},
                                                         {"07","05","5","22"},
                                                         {"07","06","7","18"},
                                                         {"07","07","4","23"},
                                                         {"07","17","4","23"},
                                                         {"08","11","3","12"},
                                                         {"08","12","3","12"},
                                                         {"08","13","3","12"},
                                                         {"08","14","3","12"},
                                                         {"08","15","3","12"},
                                                         {"08","16","3","12"},
                                                         {"09","01","6","30"},
                                                         {"09","06","6","30"},
                                                         {"09","12","6","32"},
                                                         {"10","07","7","05"},
                                                         {"11","03","1","07"},
                                                         {"11","10","2","08"},
                                                         {"12","02","3","01"},
                                                         {"12","05","1","25"},
                                                         {"13","01","5","26"},
                                                         {"13","03","5","27"},
                                                         {"13","04","5","29"},
                                                         {"13","07","5","28"}};
        #endregion


        public R8VolEquation()
        {
            InitializeComponent();
        }


        private void onTopwoodClick(object sender, EventArgs e)
        {
            //  setup dialog and get checked species in return
            R8Topwood r8top = new R8Topwood();
            r8top.setupDialog(); 
            r8top.ShowDialog();
            topwoodStatus = r8top.checkStatus;
        }   //  end onTopwoodClick


        private void onOK(object sender, EventArgs e)
        {
            //  open volume equation table and remove all before building and saving equations
            Global.BL.deleteVolumeEquations();
            volList.Clear();

            //  Need to build volume equation and store in table, so goes into VolumeEqList
            string currentForest = "";
            string currentDistrict = "";
            string currGeoCode = "";
            string currGrpCode = "";
            foreach (SaleDO sd in Global.BL.getSale())
            {
                currentForest = sd.Forest;
                if (sd.District == null)
                    currentDistrict = "";
                else currentDistrict = sd.District;
            }   //  end foreach

            //  Look up geo code and group code for this forest and district (if any)
            //  First look in defaults
            for (int k = 0; k < 12; k++)
            {
                if (currentForest == forestDefaultList[k, 0])
                {
                    currGeoCode = forestDefaultList[k, 1];
                    currGrpCode = forestDefaultList[k, 2];
                }   //  endif
            }   //  end for k loop

            //  Check for an override on district
            for (int k = 0; k < 33; k++)
            {
                if (currentForest == forestDistrictList[k, 0] && currentDistrict == forestDistrictList[k, 1])
                {
                    currGeoCode = forestDistrictList[k, 2];
                    currGrpCode = forestDistrictList[k, 3];
                }   //  endif
            }   //  end for k loop

            //  if geocode and group code are still blank, means forest or district are incorrect
            if (currGeoCode == "" || currGrpCode == "")
            {
                MessageBox.Show("Could not find Forest and/or District number.\nCannot complete equations.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }   //  endif

            //  get unique species/product combinations
            string[,] speciesProduct = Global.BL.GetUniqueSpeciesProduct();
            for(int k=0;k<speciesProduct.GetLength(0);k++)
            {
                //  need species and product
                string currentSpecies = speciesProduct[k, 0];
                string currentProduct = speciesProduct[k, 1];

                //  call build equation for this combination
                if (currentSpecies != null && currentProduct != null)
                {
                    //  change in volume library no longer has DVEE equations used for board for board foot volume
                    //  so commented out the call to build those equations
                    //  October 2015
                    //if (currentProduct == "01")
                      //  buildVolumeEquation(currGrpCode, currentSpecies, currentProduct);
                    //  Build Clark equations -- old or new -- July 2017
                    if (newClarkCheckBox.Checked == true)
                        buildNewClarkEquations(currGeoCode, currentSpecies, currentProduct);
                    else if (oldClarkCheckBox.Checked == true)
                    {
                        if (pulpwoodHeight >= 0)
                            buildClarkEquation(currGeoCode, currentSpecies, currentProduct, pulpwoodHeight);
                        else if(pulpwoodHeight < 0)
                        {
                            Close();
                            return;
                        }   //  endif
                    }   //  endif
                }   //  endif no null
            }   //  end foreach

            //  Save equations in database
            Global.BL.SaveVolumeEquations(volList);

            if (calcBiomass.Checked == true)
            {
                VolumeEquations ve = new VolumeEquations();
                ve.fileName = fileName;
                ve.updateBiomass(volList);
            }   //  endif calculate biomass
            Close();
            return;
        }   //  end onOK


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
            StringBuilder sb = new StringBuilder();
            secDIB = 0;
            sb.Append("8");
            sb.Append(currGeoCode);
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

            //  finish equation
            if (currentProduct == "01")
            {
                //  Set sawtimber reference height 7 or 9
                // pine species < 300
                if (Convert.ToInt32(currentSpecies) < 300)
                    sb.Append("7");
                else sb.Append("9");    //  hardwoods

                vel.CalcCubic = 1;
                //  added per request from Gary Church
                vel.CalcBoard = 1;

                //  topwood included?
                //  find current species in topwoodStatus
                for (int j = 0; j < topwoodStatus.Length; j++)
                {
                    if (topwoodStatus[j] == currentSpecies)
                    {
                        vel.CalcTopwood = 1;
                        break;
                    }   //  endif
                }   //  end for j loop
            }
            else if(currentProduct == "02")
            {
                //  set pulpwood reference height 4 or 0
                //  based on selection from first window
                if (pulpwoodHeight == 0)
                {
                    // 4 inch DOB for all species
                    sb.Append("4");
                }
                else if(pulpwoodHeight == 1)
                {
                    //  total height all species
                    sb.Append("0");
                }
                else if(pulpwoodHeight == 2)
                {
                    //  pine total height hardwood 4-inch dob
                    if (Convert.ToInt32(currentSpecies) < 300)
                        sb.Append("0");
                    else sb.Append("4");
                }   //  endif

                vel.StumpHeight = Convert.ToSingle(0.5);
                vel.CalcCubic = 1;
                //  added per request from Gary Church
                vel.CalcBoard = 1;
            }
            else if(currentProduct == "08")
            {
                //  find any changed DIB for current species
                double DIBfound = findDIB(currentSpecies);
                vel.StumpHeight = Convert.ToSingle(0.5);
                vel.TopDIBPrimary = Convert.ToSingle(DIBfound);
                vel.TopDIBSecondary = Convert.ToSingle(secDIB);
                vel.CalcCubic = 1;
                vel.CalcTopwood = 1;
                //  added per request from Gary Church
                vel.CalcBoard = 1;
                sb.Append("8");
            }
            else        
            {
                //  any other product code such as 20 for biomass
                sb.Append("0");
                vel.CalcCubic = 1;
                vel.CalcBoard = 1;
            }   //  endif currentProduct

            //  add remaining components to equation
            sb.Append("CLKE");
            //  Fix species length
            if (currentSpecies.Length == 2)
                sb.Append("0");
            else if (currentSpecies.Length == 1)
                sb.Append("00");
            sb.Append(currentSpecies);
            
            vel.VolumeEquationNumber = sb.ToString();
            volList.Add(vel);
            return;
        }   //  end buildClarkEquation


        private void buildNewClarkEquations(string currGeoCode, string currSpecies, string currProduct)
        {
            //  For new Clark equations only
            //  July 2017
            StringBuilder sb = new StringBuilder();
            secDIB = 0;
            sb.Append("8");
            sb.Append(currGeoCode);
            sb.Append("1");

            //  complete equation and put in volList
            VolumeEquationDO vel = new VolumeEquationDO();
            vel.Species = currSpecies;
            vel.PrimaryProduct = currProduct;
            vel.StumpHeight = 1;
            vel.TopDIBPrimary = 0;
            vel.TopDIBSecondary = 0;
            vel.CalcTotal = 0;
            vel.CalcBoard = 1;
            vel.CalcCubic = 1;
            vel.CalcCord = 0;
            vel.CalcTopwood = 0;

            if (calcBiomass.Checked == true)
                vel.CalcBiomass = 1;
            else vel.CalcBiomass = 0;

            //  Finish equation
            //  topwood included?
            for (int j = 0; j < topwoodStatus.Length; j++)
            {
                if (topwoodStatus[j] == currSpecies)
                {
                    vel.CalcTopwood = 1;
                    break;
                }   //  endif
            }   //  end for loop

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
            if (DIBbySpecies != null)
            {
                double DIBfound = findDIB(currSpecies);
                vel.TopDIBPrimary = Convert.ToSingle(DIBfound);
                vel.TopDIBSecondary = Convert.ToSingle(secDIB);
            }   //  endif

            sb.Append("CLKE");
            //  fix species length
            if (currSpecies.Length == 2)
                sb.Append("0");
            else if (currSpecies.Length == 1)
                sb.Append("00");
            sb.Append(currSpecies);

            vel.VolumeEquationNumber = sb.ToString();
            volList.Add(vel);
            return;
        }   //  buildNewClarkEquations


        private float findDIB(string currentSpecies)
        {
            //  find any changed DIB for this species
            int nthRow = DIBbySpecies.FindIndex(
                delegate(JustDIBs dbs)
                {
                    return dbs.speciesDIB == currentSpecies;
                });
            if (nthRow != -1)
            {
                secDIB = DIBbySpecies[nthRow].secondaryDIB;
                return DIBbySpecies[nthRow].primaryDIB;
            }
            else return 0;
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
//            r8prod08.Global.BL.fileName = Global.BL.fileName;
//            r8prod08.Global.BL.DAL = Global.BL.DAL;
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
            R9TopDIB r9DIB = new R9TopDIB();
            r9DIB.fileName = fileName;
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
            R8PulpwoodMeasurement pm = new R8PulpwoodMeasurement();
            pm.ShowDialog();
            pulpwoodHeight = pm.pulpHeight;
            return;
        }   //  end onOldClark

    }
}
