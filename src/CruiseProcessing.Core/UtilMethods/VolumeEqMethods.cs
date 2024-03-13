using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing
{
    public class VolumeEqMethods
    {
        private string speciesName = "";
        private string modelName = "";

        public static List<VolumeEquationDO> GetAllEquationsToCalc(List<VolumeEquationDO> volList,
                                                                    string currSP, string currPP)
        {
            //  This returns a list of equations for the current species and product to calculate volumes
            List<VolumeEquationDO> returnList = volList.FindAll(
                delegate (VolumeEquationDO ved)
                {
                    return ved.Species == currSP && ved.PrimaryProduct == currPP;
                });
            return returnList;
        }   //  end GetAllEquationsToCalc

        //  methods pertaining to volume equations
        public static List<VolEqList> GetRegionVolumes(string currentRegion)
        {
            volumeLists vList = new volumeLists();
            //  build a list to return to the volume equation window based on region selected
            List<VolEqList> volList = new List<VolEqList>();
            switch (currentRegion)
            {
                case "10":
                    for (int k = 0; k < 13; k++)
                    {
                        VolEqList vl = new VolEqList();
                        vl.vForest = vList.volumeArray10[k, 0].ToString();
                        vl.vCommonName = vList.volumeArray10[k, 1].ToString();
                        vl.vEquation = vList.volumeArray10[k, 2].ToString();
                        vl.vModelName = vList.volumeArray10[k, 3].ToString();
                        volList.Add(vl);
                    }   //  end for k loop
                    break;

                case "06":
                    for (int k = 0; k < 89; k++)
                    {
                        VolEqList vl = new VolEqList();
                        vl.vForest = vList.volumeArray06[k, 0].ToString();
                        vl.vCommonName = vList.volumeArray06[k, 1].ToString();
                        vl.vEquation = vList.volumeArray06[k, 2].ToString();
                        vl.vModelName = vList.volumeArray06[k, 3].ToString();
                        volList.Add(vl);
                    }   //  end for k loop
                    break;

                case "05":
                    for (int k = 0; k < 30; k++)
                    {
                        VolEqList vl = new VolEqList();
                        vl.vForest = vList.volumeArray05[k, 0].ToString();
                        vl.vCommonName = vList.volumeArray05[k, 1].ToString();
                        vl.vEquation = vList.volumeArray05[k, 2].ToString();
                        vl.vModelName = vList.volumeArray05[k, 3].ToString();
                        volList.Add(vl);
                    }   //  end for k loop
                    break;

                case "04":
                    for (int k = 0; k < 54; k++)
                    {
                        VolEqList vl = new VolEqList();
                        vl.vForest = vList.volumeArray04[k, 0].ToString();
                        vl.vCommonName = vList.volumeArray04[k, 1].ToString();
                        vl.vEquation = vList.volumeArray04[k, 2].ToString();
                        vl.vModelName = vList.volumeArray04[k, 3].ToString();
                        volList.Add(vl);
                    }   //  end for k loop
                    break;

                case "03":
                    for (int k = 0; k < 26; k++)
                    {
                        VolEqList vl = new VolEqList();
                        vl.vForest = vList.volumeArray03[k, 0].ToString();
                        vl.vCommonName = vList.volumeArray03[k, 1].ToString();
                        vl.vEquation = vList.volumeArray03[k, 2].ToString();
                        vl.vModelName = vList.volumeArray03[k, 3].ToString();
                        volList.Add(vl);
                    }   //  end for k loop
                    break;

                case "02":
                    for (int k = 0; k < 19; k++)
                    {
                        VolEqList vl = new VolEqList();
                        vl.vForest = vList.volumeArray02[k, 0].ToString();
                        vl.vCommonName = vList.volumeArray02[k, 1].ToString();
                        vl.vEquation = vList.volumeArray02[k, 2].ToString();
                        vl.vModelName = vList.volumeArray02[k, 3].ToString();
                        volList.Add(vl);
                    }   // end for k loop
                    break;

                case "01":
                    for (int k = 0; k < 14; k++)
                    {
                        VolEqList vl = new VolEqList();
                        vl.vForest = vList.volumeArray01[k, 0].ToString();
                        vl.vCommonName = vList.volumeArray01[k, 1].ToString();
                        vl.vEquation = vList.volumeArray01[k, 2].ToString();
                        vl.vModelName = vList.volumeArray01[k, 3].ToString();
                        volList.Add(vl);
                    }   //  end for k loop
                    break;

                case "11":
                    for (int k = 0; k < 20; k++)
                    {
                        VolEqList vl = new VolEqList();
                        vl.vForest = vList.volumeArray11[k, 0].ToString();
                        vl.vCommonName = vList.volumeArray11[k, 1].ToString();
                        vl.vEquation = vList.volumeArray11[k, 2].ToString();
                        vl.vModelName = vList.volumeArray11[k, 3].ToString();
                        volList.Add(vl);
                    }   //  end for k loop
                    break;
            }   //  end switch on current region

            return volList;
        }   //  end GetRegionVolumes

        public void updateVolumeList(List<VolumeEquationDO> volEquations, string currentRegion)
        {
            volumeLists vList = new volumeLists();

            //  loop through equation list and update common species and model name if blank
            foreach (VolumeEquationDO vel in volEquations)
            {
                //  are they blank?
                if ((vel.CommonSpeciesName == "" || vel.CommonSpeciesName == " " || vel.CommonSpeciesName == null) ||
                    (vel.Model == "" || vel.Model == " " || vel.Model == null))
                {
                    //  call fillFields based on equation first character
                    if (vel.VolumeEquationNumber.Substring(0, 1) == "3")
                        fillFields(vel.VolumeEquationNumber, vList.volumeArray03);
                    else if (vel.VolumeEquationNumber.Substring(0, 1) == "A")
                        fillFields(vel.VolumeEquationNumber, vList.volumeArray10);
                    else if (vel.VolumeEquationNumber.Substring(0, 1) == "4" ||
                             vel.VolumeEquationNumber.Substring(0, 3) == "I15")
                        fillFields(vel.VolumeEquationNumber, vList.volumeArray04);
                    else if (currentRegion == "02")
                    {
                        if (vel.VolumeEquationNumber.Substring(0, 1) == "2" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "4" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "I")
                            fillFields(vel.VolumeEquationNumber, vList.volumeArray02);
                    }
                    else if (currentRegion == "01")
                    {
                        if (vel.VolumeEquationNumber.Substring(0, 1) == "1" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "2" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "I")
                            fillFields(vel.VolumeEquationNumber, vList.volumeArray01);
                    }
                    else if (currentRegion == "05")
                    {
                        if (vel.VolumeEquationNumber.Substring(0, 1) == "H" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "5" ||
                            vel.VolumeEquationNumber.Substring(0, 3) == "I15")
                            fillFields(vel.VolumeEquationNumber, vList.volumeArray05);
                    }
                    else if (currentRegion == "06")
                    {
                        if (vel.VolumeEquationNumber.Substring(0, 1) == "6" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "I" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "F" ||
                            vel.VolumeEquationNumber.Substring(0, 3) == "I11" ||
                            vel.VolumeEquationNumber.Substring(0, 3) == "I12" ||
                            vel.VolumeEquationNumber.Substring(0, 3) == "I13")
                            fillFields(vel.VolumeEquationNumber, vList.volumeArray06);
                    }   //  endif
                    vel.CommonSpeciesName = speciesName;
                    vel.Model = modelName;
                }   //  endif
            }   //  end foreach loop
            return;
        }   //  end updateVolumeList

        private void fillFields(string currentEquation, string[,] arrayToUse)
        {
            for (int n = 0; n < arrayToUse.GetLength(0); n++)
            {
                if (currentEquation == arrayToUse[n, 2])
                {
                    speciesName = arrayToUse[n, 1];
                    modelName = arrayToUse[n, 3];
                    return;
                }   //  endif currentEquation
            }   //  end for n loop

            //  if it falls through to here, must be an older equation
            speciesName = " ";
            modelName = " ";
            return;
        }   //  end fillFields

        public static List<string> buildMerchArray(VolumeEquationDO vel)
        {
            var merchArray = new List<string>();
            merchArray.Add(" ");
            merchArray.Add(vel.VolumeEquationNumber.PadRight(10, ' '));
            merchArray.Add(vel.PrimaryProduct.PadLeft(2, ' '));
            merchArray.Add(String.Format("{0,2:F1}", vel.Trim));
            merchArray.Add(String.Format("{0,2:F0}", vel.MinLogLengthPrimary).PadLeft(2, ' '));
            merchArray.Add(String.Format("{0,2:F0}", vel.MaxLogLengthPrimary).PadLeft(2, ' '));
            merchArray.Add(String.Format("{0,2:F0}", vel.SegmentationLogic).PadLeft(2, ' '));
            merchArray.Add(String.Format("{0,2:F0}", vel.MinMerchLength).PadLeft(2, ' '));
            merchArray.Add(String.Format("{0,1:F0}", vel.EvenOddSegment));
            if (vel.PrimaryProduct == "01")
                merchArray.Add("N/A");
            else
            {
                switch (vel.MerchModFlag)
                {
                    case 0:
                        merchArray.Add("NO ");
                        break;

                    default:
                        merchArray.Add("YES");
                        break;
                }   //  end switch
            }   //  endif
            return merchArray;
        }   //  end buildMerchArray

        public static List<string> buildPrintArray(VolumeEquationDO vel)
        {
            string fieldFormat = "{0,2:F1}";
            var volArray = new List<string>();
            volArray.Add("     ");
            volArray.Add(vel.Species.PadRight(6, ' '));
            volArray.Add(vel.PrimaryProduct.PadLeft(2, '0'));
            volArray.Add(vel.VolumeEquationNumber);
            volArray.Add(String.Format(fieldFormat, vel.StumpHeight));
            if (vel.CalcTotal == 0)
                volArray.Add("NO ");
            else if (vel.CalcTotal == 1)
                volArray.Add("YES");
            volArray.Add(String.Format(fieldFormat, vel.TopDIBPrimary));
            if (vel.CalcBoard == 0)
                volArray.Add("NO ");
            else if (vel.CalcBoard == 1)
                volArray.Add("YES");
            if (vel.CalcCubic == 0)
                volArray.Add("NO ");
            else if (vel.CalcCubic == 1)
                volArray.Add("YES");
            if (vel.CalcCord == 0)
                volArray.Add("NO ");
            else if (vel.CalcCord == 1)
                volArray.Add("YES");
            volArray.Add(String.Format(fieldFormat, vel.TopDIBSecondary));
            if (vel.CalcTopwood == 0)
            {
                volArray.Add("NO ");
                volArray.Add("NO ");
                volArray.Add("NO ");
            }
            else if (vel.CalcTopwood == 1)
            {
                if (vel.CalcBoard == 1)
                    volArray.Add("YES");
                else volArray.Add("NO ");
                if (vel.CalcCubic == 1)
                    volArray.Add("YES");
                else volArray.Add("NO ");
                if (vel.CalcCord == 1)
                    volArray.Add("YES");
                else volArray.Add("NO ");
            }   //  endif

            //  Biomass flag
            if (vel.CalcBiomass == 0)
                volArray.Add("NO ");
            else if (vel.CalcBiomass == 1)
                volArray.Add("YES");
            return volArray;
        }   //  end buildPrintArray
    }
}