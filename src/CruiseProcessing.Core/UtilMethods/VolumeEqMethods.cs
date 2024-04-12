using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing
{
    public static class VolumeEqMethods
    {

        [Obsolete]
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

        public static void SetSpeciesAndModelValues(List<VolumeEquationDO> volEquations, string currentRegion)
        {
            //  loop through equation list and update common species and model name if blank
            foreach (VolumeEquationDO vel in volEquations)
            {
                if (String.IsNullOrEmpty(vel.CommonSpeciesName) ||
                    String.IsNullOrEmpty(vel.Model))
                {
                    var equationNumber = vel.VolumeEquationNumber;

                    //  call fillFields based on equation first character
                    if (equationNumber.Substring(0, 1) == "3")
                        SetSpeciesAndModel(vel, volumeLists.Region3Equations);
                    else if (equationNumber.Substring(0, 1) == "A")
                        SetSpeciesAndModel(vel, volumeLists.Region10Equations);
                    else if (equationNumber.Substring(0, 1) == "4" ||
                             equationNumber.Substring(0, 3) == "I15")
                        SetSpeciesAndModel(vel, volumeLists.Region4Eauations);
                    else if (currentRegion == "02")
                    {
                        if (equationNumber.Substring(0, 1) == "2" ||
                            equationNumber.Substring(0, 1) == "4" ||
                            equationNumber.Substring(0, 1) == "I")
                            SetSpeciesAndModel(vel, volumeLists.Region2Equations);
                    }
                    else if (currentRegion == "01")
                    {
                        if (equationNumber.Substring(0, 1) == "1" ||
                            equationNumber.Substring(0, 1) == "2" ||
                            equationNumber.Substring(0, 1) == "I")
                            SetSpeciesAndModel(vel, volumeLists.Region1Equations);
                    }
                    else if (currentRegion == "05")
                    {
                        if (equationNumber.Substring(0, 1) == "H" ||
                            equationNumber.Substring(0, 1) == "5" ||
                            equationNumber.Substring(0, 3) == "I15")
                            SetSpeciesAndModel(vel, volumeLists.Region5Equations);
                    }
                    else if (currentRegion == "06")
                    {
                        if (equationNumber.Substring(0, 1) == "6" ||
                            equationNumber.Substring(0, 1) == "I" ||
                            equationNumber.Substring(0, 1) == "F" ||
                            equationNumber.Substring(0, 3) == "I11" ||
                            equationNumber.Substring(0, 3) == "I12" ||
                            equationNumber.Substring(0, 3) == "I13")
                            SetSpeciesAndModel(vel, volumeLists.Region6Equations);
                    }
                }
            }
        }

        private static void SetSpeciesAndModel(VolumeEquationDO volEq, IEnumerable<VolEqList> arrayToUse)
        {
            var currentEquation = volEq.VolumeEquationNumber;

            var eq = arrayToUse.FirstOrDefault(x => x.vEquation == currentEquation);
            if(eq != null)
            {
                volEq.CommonSpeciesName = eq.vCommonName;
                volEq.Model = eq.vModelName;
            }
            else
            {
                volEq.CommonSpeciesName = " ";
                volEq.Model = " ";
            }
        }

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