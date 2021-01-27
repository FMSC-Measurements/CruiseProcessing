using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CruiseDAL.DataObjects;
using CruiseDAL;


namespace CruiseProcessing
{
    public class VolumeEqMethods
    {
        #region
            private string speciesName = "";
            private string modelName = "";
            ErrorLogMethods elm = new ErrorLogMethods();
            public DAL DAL { get; set; }
        #endregion

        //  edit checks
        public int IsEmpty(List<VolumeEquationDO> volList)
        {
            if (volList.Count == 0)
                return 25;
            else return 0;
        }   //  end IsEmpty


        public List<VolumeEquationDO> GetAllEquationsToCalc(List<VolumeEquationDO> volList,
                                                                    string currSP, string currPP)
        {
            //  This returns a list of equations for the current species and product to calculate volumes
            List<VolumeEquationDO> returnList = volList.FindAll(
                delegate(VolumeEquationDO ved)
                {
                    return ved.Species == currSP && ved.PrimaryProduct == currPP;
                });
            return returnList;
        }   //  end GetAllEquationsToCalc



        public int MatchSpeciesProduct(List<VolumeEquationDO> volList, List<TreeDO> tList)
        {
            int errorsFound = 0;
            //  find each species or species/product combination in volume equations
            int nthRow = -1;
            List<TreeDO> tList1 = DAL.From<TreeDO>()
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .GroupBy("Tree.Species", "SampleGroup.PrimaryProduct")
                .Read().ToList();

            //List<TreeDO> tList1 = DAL.Read<TreeDO>("Tree", "GROUP BY Species, Tree.SampleGroup.PrimaryProduct", null);
            VolumeEquationDO ved;
            foreach (TreeDO td in tList1)
            {
                nthRow = 0;
//                foreach(VolumeEquationDO ved in volList)
//                {
//                    if (ved.Species == td.Species && ved.PrimaryProduct == td.SampleGroup.PrimaryProduct)
//                        nthRow++;
//                }
                ved = volList.Find(item => item.Species == td.Species && item.PrimaryProduct == td.SampleGroup.PrimaryProduct);
                //                nthRow = volList.FindIndex(
//                    delegate(VolumeEquationDO ved)
//                    {
//                        return ved.Species == td.Species && ved.PrimaryProduct == td.SampleGroup.PrimaryProduct;
//                    });
//                if (nthRow == 0)
                if(ved == null)
                {
                    elm.LoadError("Tree", "E", "12", (long)td.Tree_CN, "Species");
                    errorsFound++;
                }   //  endif nthRow
            }   //  end for k loop
            return errorsFound;
        }   //  end MatchSpeciesProduct


        public int GeneralEquationChecks(List<VolumeEquationDO> volList)
        {
            //  Checks for more than one quation for species/product combination
            //  check duplicate volumes requested on species/product combination
            //  for non-profile models (DVE) make sure primary flags are set if secondary also set
            //  check for secondary top DIB greater than primary top DIB
            //  check for same equation number with different primary top diameters (DVE only)
            //  check length of equation -- possible species error
            int errorsFound = 0;
            List<VolumeEquationDO> dupEquations = new List<VolumeEquationDO>();
            foreach (VolumeEquationDO ved in volList)
            {
                //  Capture equation for non-profile models, species and product for error report if needed
                if (ved.VolumeEquationNumber.Contains("DVE") == true)
                {
                    // make sure primary flags are set
                    if (ved.CalcTopwood == 1 && (ved.CalcBoard == 0 && ved.CalcCubic == 0 && ved.CalcCord == 0))
                    {
                        elm.LoadError("VolumeEquation", "E", "3", (long)ved.rowID, "VolumeFlags");
                        errorsFound++;
                    }   //  endif

                    //  check for different top DIBs
                    dupEquations = volList.FindAll(
                        delegate(VolumeEquationDO voleq)
                        {
                            return voleq.VolumeEquationNumber == ved.VolumeEquationNumber && 
                                   voleq.PrimaryProduct == ved.PrimaryProduct &&
                                   voleq.TopDIBPrimary != ved.TopDIBPrimary;
                        });
                    if (dupEquations.Count > 1)  //  means top DIBs are different
                    {
                        elm.LoadError("VolumeEquation", "E", "6", (long)ved.rowID, "TopDIBPrimary");
                        errorsFound++;
                    }
                }   //  endif non-profile model

                if (ved.TopDIBSecondary > ved.TopDIBPrimary)
                {
                    elm.LoadError("VolumeEquation", "E", "4", (long)ved.rowID, "TopDIBSecondary");
                    errorsFound++;
                }

                if (ved.VolumeEquationNumber.Length != 10)
                {
                    elm.LoadError("VolumeEquation", "E", "1", (long)ved.rowID, "VolumeEquationNumber");
                    errorsFound++;
                }

                dupEquations = volList.FindAll(
                    delegate(VolumeEquationDO voleq)
                    {
                        return voleq.VolumeEquationNumber == ved.VolumeEquationNumber && 
                               voleq.Species == ved.Species && 
                               voleq.PrimaryProduct == ved.PrimaryProduct;
                    });
                if (dupEquations.Count > 1)
                {
                    elm.LoadError("VolumeEquation", "E", "5", (long)ved.rowID, "VolumeEquationNumber");
                    errorsFound++;
                }

            }   //  end foreach loop
            return errorsFound;
        }   //  end GeneralEquationChecks


        public int FindBehrs(List<VolumeEquationDO> volList)
        {
            int errorsFound = 0;
            foreach (VolumeEquationDO ved in volList)
            {
                if(ved.VolumeEquationNumber.Contains("BEH"))
                {
                    elm.LoadError("VolumeEquation","W","Variable log length diameters cannot be computed with BEH equations",(long)ved.rowID, "NoName");
                    errorsFound++;
                }
                else if(ved.VolumeEquationNumber.Contains("MAT"))
                {
                    elm.LoadError("VolumeEquation","W","Variable log length diameters cannot be computed with MAT equations",(long)ved.rowID, "NoName");
                    errorsFound++;
                }
                else if(ved.VolumeEquationNumber.Contains("DVE"))
                {
                    elm.LoadError("VolumeEquation","W","Variable log length diameters cannot be computed with DVE equations",(long)ved.rowID, "NoName");
                    errorsFound++;
                }
                else if(ved.VolumeEquationNumber.Contains("CLK"))
                {
                    elm.LoadError("VolumeEquation","W","Variable log length diameters cannot be computed with CLK equations",(long)ved.rowID, "NoName");
                    errorsFound++;
                }   //  endif on equation number

            }   //  end foreach loop
            return errorsFound;
        }   //  end FindBehrs


        //  methods pertaining to volume equations
        public List<VolEqList> GetRegionVolumes(string currentRegion)
        {
            volumeLists vList = new volumeLists();
            //  build a list to return to the volume equation window based on region selected
            List<VolEqList> volList = new List<VolEqList>();
            switch(currentRegion)
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
                    for (int k = 0; k < 88; k++)
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
                        vl.vForest = vList.volumeArray01[k,0].ToString();
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


        public void updateVolumeList(List<VolumeEquationDO> volEquations, string fileName, string currentRegion)
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
                    else if (vel.VolumeEquationNumber.Substring(0,1) == "4"  || 
                             vel.VolumeEquationNumber.Substring(0,3) == "I15")
                        fillFields(vel.VolumeEquationNumber, vList.volumeArray04);
                    else if (currentRegion == "02")
                    {
                        if (vel.VolumeEquationNumber.Substring(0, 1) == "2" || 
                            vel.VolumeEquationNumber.Substring(0, 1) == "4" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "I")
                            fillFields(vel.VolumeEquationNumber, vList.volumeArray02);
                    }
                    else if(currentRegion == "01")
                    {
                        if (vel.VolumeEquationNumber.Substring(0, 1) == "1" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "2" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "I")
                            fillFields(vel.VolumeEquationNumber, vList.volumeArray01);
                    }
                    else if(currentRegion == "05")
                    {
                        if (vel.VolumeEquationNumber.Substring(0, 1) == "H" ||
                            vel.VolumeEquationNumber.Substring(0, 1) == "5" ||
                            vel.VolumeEquationNumber.Substring(0, 3) == "I15")
                            fillFields(vel.VolumeEquationNumber, vList.volumeArray05);
                    }
                    else if(currentRegion == "06")
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


        public ArrayList buildMerchArray(VolumeEquationDO vel)
        {
            ArrayList merchArray = new ArrayList();
            merchArray.Add(" ");
            merchArray.Add(vel.VolumeEquationNumber.PadRight(10,' '));
            merchArray.Add(vel.PrimaryProduct.PadLeft(2, ' '));
            merchArray.Add(Utilities.Format("{0,2:F1}", vel.Trim).ToString());
            merchArray.Add(Utilities.Format("{0,2:F0}", vel.MinLogLengthPrimary).ToString().PadLeft(2,' '));
            merchArray.Add(Utilities.Format("{0,2:F0}", vel.MaxLogLengthPrimary).ToString().PadLeft(2,' '));
            merchArray.Add(Utilities.Format("{0,2:F0}", vel.SegmentationLogic).ToString().PadLeft(2,' '));
            merchArray.Add(Utilities.Format("{0,2:F0}", vel.MinMerchLength).ToString().PadLeft(2,' '));
            merchArray.Add(Utilities.Format("{0,1:F0}", vel.EvenOddSegment).ToString());
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


        public ArrayList buildPrintArray(VolumeEquationDO vel)
        {
            string fieldFormat = "{0,2:F1}";
            ArrayList volArray = new ArrayList();
            volArray.Add("     ");
            volArray.Add(vel.Species.PadRight(6, ' '));
            volArray.Add(vel.PrimaryProduct.PadLeft(2, '0'));
            volArray.Add(vel.VolumeEquationNumber);
            volArray.Add(Utilities.Format(fieldFormat, vel.StumpHeight).ToString());
            if (vel.CalcTotal == 0)
                volArray.Add("NO ");
            else if (vel.CalcTotal == 1)
                volArray.Add("YES");
            volArray.Add(Utilities.Format(fieldFormat, vel.TopDIBPrimary).ToString());
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
            volArray.Add(Utilities.Format(fieldFormat, vel.TopDIBSecondary).ToString());
            if (vel.CalcTopwood == 0)
            {
                volArray.Add("NO ");
                volArray.Add("NO ");
                volArray.Add("NO ");
            }
            else if(vel.CalcTopwood == 1)
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
