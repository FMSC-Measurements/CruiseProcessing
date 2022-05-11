using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public class TreeListMethods
    {
        //  edit checks
        public int IsEmpty(List<TreeDO> tList)
        {
            if (tList.Count == 0)
                return 25;
            else return 0;
        }   //  end IsEmpty
        


        public int GeneralChecks(List<TreeDO> justMeasured, string currRegion)
        {
            int errorsFound = 0;
            ErrorLogMethods elm = new ErrorLogMethods();
            //  check for missing species, product or uom when tree count 
            //  is greater than zero or DBH is greater than zero
            List<TreeDO> errList = justMeasured.FindAll(
                delegate(TreeDO tErr)
                {
                    return tErr.Species == "" && tErr.SampleGroup.PrimaryProduct == "" && tErr.SampleGroup.UOM == "";
                });
            foreach (TreeDO tdo in errList)
            {
                if (tdo.TreeCount > 0 || tdo.DBH > 0)
                {
                    elm.LoadError("Tree", "E", "21", (long)tdo.Tree_CN, "Species");
                    errorsFound++;
                }   //  endif tree count or DBH greater than zero
            }   //  end foreach loop

            foreach (TreeDO tdo in justMeasured)
            {
                //  check for upper stem diameter greater than DBH
                if (tdo.UpperStemDiameter > tdo.DBH)
                //if(tdo.UpperStemDOB > tdo.DBH)
                {
                    elm.LoadError("Tree", "E", "18", (long)tdo.Tree_CN, "UpperStemDiameter");
                    errorsFound++;
                }   //  endif

                //  check for recoverable greater than seen defect 
                //  cannot make this check for region 10 as they do things incorrectly with defect.
                if (currRegion != "10")
                {
                    if (tdo.RecoverablePrimary > tdo.SeenDefectPrimary)
                    {
                        elm.LoadError("Tree", "E", "29", (long)tdo.Tree_CN, "RecoverablePrimary");
                        errorsFound++;
                    }   //  endif recoverable > than seen defect
                }   //  endif on region


                //  top DIB secondary greater than primary
                if (tdo.TopDIBPrimary > 0.0 && tdo.TopDIBSecondary > 0.0)
                {
                    if (tdo.TopDIBSecondary > tdo.TopDIBPrimary)
                    {
                        elm.LoadError("Tree", "E", "4", (long)tdo.Tree_CN, "TopDIBSecondary");
                        errorsFound++;
                    }   //  endif dibs 
                }   //  endif

                // check for height value recorded
                if (tdo.TotalHeight == 0 &&
                    tdo.MerchHeightPrimary == 0 &&
                    tdo.MerchHeightSecondary == 0 &&
                    tdo.UpperStemHeight == 0)
                {
                    elm.LoadError("Tree", "E", "32", (long)tdo.Tree_CN, "Height");
                    errorsFound++;
                }   //  endif


                //  check for null tree default value CN
                if (tdo.TreeDefaultValue_CN == null)
                {
                    elm.LoadError("Tree", "E", "31", (long)tdo.Tree_CN, "TreeDefaultValue");
                    errorsFound++;
                }   //  endif

                //  check for blank or null tree grade
                //  if this is true, it gets put into a separate population which
                //  can impact certain reports
                if (tdo.Grade == "" || tdo.Grade == " " || tdo.Grade == null)
                {
                    elm.LoadError("Tree", "E", "8", (long)tdo.Tree_CN, "Tree Grade");
                    errorsFound++;
                }   //  endif on tree grade
            }   //  foreach on measured trees

            return errorsFound;
        }   //  end GeneralChecks




        //  methods pertaining to tree table and/or tree calculated values        
        public List<TreeDO> GetCurrentStratum(List<TreeDO> tList, string currST)
        {
            List<TreeDO> rtrnList = new List<TreeDO>();
            
            //  all current stratum trees
            rtrnList = tList.FindAll(
                delegate(TreeDO td)
                {
                    return td.Stratum.Code == currST;
                });
            if (rtrnList != null)
                return rtrnList;

            return rtrnList;
        }   //  end GetCurrentStratum


        public List<TreeDO> GetCutLeave(List<TreeDO> tList, string currCL, string currCM)
        {
            List<TreeDO> rtrnList = new List<TreeDO>();
            //  all cut/leave trees and/or count/measure
            if(currCM == "")
            {
                rtrnList = tList.FindAll(
                    delegate(TreeDO td)
                    {
                        return td.SampleGroup.CutLeave == currCL;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }
            else if(currCM != "")       //  with count measure code
            {
                rtrnList = tList.FindAll(
                    delegate(TreeDO td)
                    {
                        return td.SampleGroup.CutLeave == currCL && td.CountOrMeasure == currCM;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }   //  endif there is count measure code

            return rtrnList;
        }   //  end GetCutLeave


        //  get current stratum and something else
        public List<TreeDO> GetCurrentStratumAndSomethingElse(List<TreeDO> tList, string currST, 
                                                                        string currCM, string currCL)
        {
            List<TreeDO> rtrnList = new List<TreeDO>();
            if (currCL != "" && currCM != "")
            {
                rtrnList = tList.FindAll(
                    delegate(TreeDO tdo)
                    {
                        return tdo.Stratum.Code == currST && tdo.SampleGroup.CutLeave == currCL && tdo.CountOrMeasure == currCM;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }   //  endif both are not blank

            if (currCM != "" && currCL == "")
            {
                rtrnList = tList.FindAll(
                    delegate(TreeDO tdo)
                    {
                        return tdo.Stratum.Code == currST && tdo.CountOrMeasure == currCM;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }   //  endif currCM not blank

            if (currCL != "" && currCM == "")
            {
                rtrnList = tList.FindAll(
                    delegate(TreeDO tdo)
                    {
                        return tdo.Stratum.Code == currST && tdo.SampleGroup.CutLeave == currCL;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }   //  endif currCL not blank

            return rtrnList;

        }   //  end GetCurrentStratumAndSomethingElse




        public List<TreeDO> GetInsuranceTrees(List<TreeDO> tList, string currSG, string currSP, string currST)
        {
            List<TreeDO> insuranceTrees = new List<TreeDO>();
            insuranceTrees = tList.FindAll(
                delegate(TreeDO td)
                {
                    return td.SampleGroup.Code == currSG && td.Species == currSP && 
                            td.CountOrMeasure == "I" && td.Stratum.Code == currST;
                });
            return insuranceTrees;
        } //  end GetInsuranceTrees


        public List<string> CheckForData(List<TreeDO> tList)
        {
            var fieldsToPrint = new List<string>();
            double summedValue = 0;
            int numRows = tList.Count();
            int munRows = 0;
            //int nullRows = 0;
            //  load constants into fields to print
            for (int n = 0; n < 10; n++)
                fieldsToPrint.Add(n.ToString());
            
            //  then loop through remaining fields to check for data
            for (int n = 10; n < 49; n++)
            {
                summedValue = 0;
                switch (n)
                {
                    case 10:        //  tree count
                        summedValue = tList.Sum(tdo => tdo.TreeCount);
                        break;
                    case 11:        //  unit of measure -- just add to list
                        summedValue = numRows;
                        break;
                    case 12:        //  KPI
                        summedValue = tList.Sum(tdo => tdo.KPI);
                        break;
                    case 13:        //  Cull defect primary product
                        //  per discussion Oct 17, 2012 numeric values will no longer contain null
                        //  they should be zero -- but skeptic me, I'm leaving the fix
                        //nullRows = tList.Count(tdo => tdo.TreeDefaultValue.CullPrimary == null);
                        //if (nullRows == 0)
                        //    summedValue = tList.Sum(tdo => tdo.TreeDefaultValue.CullPrimary);
                        //else summedValue = 0;
                        summedValue = tList.Sum(tdo => tdo.TreeDefaultValue.CullPrimary);
                        break;
                    case 14:        //  Hidden defect primary product
                        //nullRows = tList.Count(tdo => tdo.HiddenPrimary == null);
                        //if (nullRows == 0)
                        //    summedValue = tList.Sum(tdo => tdo.HiddenPrimary);
                        //else summedValue = 0;
                        summedValue = tList.Sum(tdo => tdo.HiddenPrimary);
                        if (summedValue == 0)
                        {
                            //  check for hidden in TreeDefaultValue
                            summedValue = tList.Sum(tdo => tdo.TreeDefaultValue.HiddenPrimary);
                        }   //  endif
                        break;
                    case 15:        //  Seen defect primary product
                        summedValue = tList.Sum(tdo => tdo.SeenDefectPrimary);
                        break;
                    case 16:        //  Recoverable
                        munRows = tList.Count(
                            delegate(TreeDO tdo)
                            {
                                return tdo.RecoverablePrimary == 0.0;
                            });
                        if (munRows == numRows)
                            summedValue = tList.Sum(tdo => tdo.TreeDefaultValue.Recoverable);
                        else if (munRows < numRows)
                            summedValue = tList.Sum(tdo => tdo.RecoverablePrimary);
                        break;
                    case 17:        //  Cull defect secondary product
                        summedValue = tList.Sum(tdo => tdo.TreeDefaultValue.CullSecondary);
                        break;
                    case 18:        //  Hidden defect secondary product
                        summedValue = tList.Sum(tdo => tdo.TreeDefaultValue.HiddenSecondary);
                        break;
                    case 19:        //  Seen defect secondary product
                        summedValue = tList.Sum(tdo => tdo.SeenDefectSecondary);
                        break;
                    //case 20:        //  Slope
                      //  summedValue = tList.Sum(tdo => tdo.Plot.Slope);
                        //break;
                    case 21:        //  Marker's initials
                        munRows = tList.Count(
                            delegate(TreeDO tdo)
                            {
                                return tdo.Initials != "" && tdo.Initials != " " && tdo.Initials != null;
                            });
                        if (munRows <= numRows)
                            summedValue = munRows;
                        break;
                    case 22:        //  Chargeable -- Yield Component? -- 8/28/2012 -- currently float needs to be string
                        munRows = tList.Count(
                        delegate(TreeDO tdo)
                        {
                            return tdo.Stratum.YieldComponent == "" || tdo.Stratum.YieldComponent == " " || tdo.Stratum.YieldComponent == null;
                            //return tdo.TreeDefaultValue.Chargeable == "" || tdo.TreeDefaultValue.Chargeable == " " || tdo.TreeDefaultValue.Chargeable == null;
                        });
                        if (munRows == 0)    //  means chargeable is not blank
                            summedValue = numRows;
                        else if(munRows > 0)
                            summedValue = 0;
                        break;
                    case 23:        //  Live dead
                        munRows = tList.Count(
                            delegate(TreeDO tdo)
                            {
                                return tdo.LiveDead == "" && tdo.LiveDead == " " && tdo.LiveDead == null;
                            });
                        if (munRows < numRows)
                            summedValue = numRows;
                        else if (munRows > 0)
                                summedValue = munRows;
                        break;
                    case 24:        //  Contract species
                        munRows = tList.Count(
                            delegate(TreeDO tdo)
                            {
                                return tdo.TreeDefaultValue.ContractSpecies != "" && 
                                    tdo.TreeDefaultValue.ContractSpecies != " ";
                                //  September 2016 -- dropping contract species of null from LCD identifier
                                    //tdo.TreeDefaultValue.ContractSpecies != null;
                            });
                        if (munRows <= numRows && munRows > 0)
                            summedValue = numRows;
                        else summedValue = 0;
                        break;
                    case 25:        //  Tree grade
                        munRows = tList.Count(
                            delegate(TreeDO tdo)
                            {
                                return tdo.Grade == "" || tdo.Grade == " " || tdo.Grade == null;
                            });
                        if (munRows == 0)
                            summedValue = numRows;
                        else if (munRows > 0)
                            summedValue = 0;
                        break;
                    case 26:        //  Height to first live limb
                        summedValue = tList.Sum(tdo => tdo.HeightToFirstLiveLimb);
                        break;
                    case 27:        //  Pole length
                        summedValue = tList.Sum(tdo => tdo.PoleLength);
                        break;
                    case 28:        //  Clear face
                        munRows = tList.Count(
                            delegate(TreeDO tdo)
                            {
                                return tdo.ClearFace == "" && tdo.ClearFace == null && tdo.ClearFace == "0";
                            });
                        if(munRows < numRows)
                            summedValue = munRows;
                        break;
                    case 29:        //  Crown ratio
                        summedValue = tList.Sum(tdo => tdo.CrownRatio);
                        break;
                    case 30:        //  DBH
                        summedValue = tList.Sum(tdo => tdo.DBH);
                        break;
                    case 31:        //  DRC
                        summedValue = tList.Sum(tdo => tdo.DRC);
                        break;
                    case 32:        //  Total Height
                        summedValue = tList.Sum(tdo => tdo.TotalHeight);
                        break;
                    case 33:        //  Merch height type
                        munRows = tList.Count(
                            delegate(TreeDO tdo)
                            {
                                return tdo.TreeDefaultValue.MerchHeightType == null || tdo.TreeDefaultValue.MerchHeightType == "";
                            });
                        if (munRows == 0)
                            summedValue = numRows;
                        else if(munRows > 0)
                            summedValue = 0;
                        break;
                    case 34:        //  Merch height log length
                        summedValue = tList.Sum(tdo => tdo.TreeDefaultValue.MerchHeightLogLength);
                        break;
                    case 35:        //  Merch height primary product
                        summedValue = tList.Sum(tdo => tdo.MerchHeightPrimary);
                        break;
                    case 36:        //  Merch height secondary product
                        summedValue = tList.Sum(tdo => tdo.MerchHeightSecondary);
                        break;
                    case 37:        //  Reference height percent
                        summedValue = tList.Sum(tdo => tdo.TreeDefaultValue.ReferenceHeightPercent);
                        break;
                    case 38:        //  Form class
                        munRows = tList.Count(
                            delegate(TreeDO tdo)
                            {
                                return tdo.FormClass > 0.0 || tdo.TreeDefaultValue.FormClass > 0.0;
                            });
                        if(munRows <= numRows)
                            summedValue = munRows;
                        break;
                    case 39:        //  Average Z
                        summedValue = tList.Sum(tdo => tdo.TreeDefaultValue.AverageZ);
                        break;
                    case 40:        //  Upper stem diameter 
                        summedValue = tList.Sum(tdo => tdo.UpperStemDiameter);
                        //summedValue = tList.Sum(tdo => tdo.UpperStemDOB);
                        break;
                    case 41:        //  Upper stem height
                        summedValue = tList.Sum(tdo => tdo.UpperStemHeight);
                        break;
                    case 42:        //  Bark thickness ratio
                        summedValue = tList.Sum(tdo => tdo.TreeDefaultValue.BarkThicknessRatio);
                        break;
                    case 43:        //  DBH Double bark thickness
                        summedValue = tList.Sum(tdo => tdo.DBHDoubleBarkThickness);
                        break;
                    case 44:        //  Top DIB primary product
                        summedValue = tList.Sum(tdo => tdo.TopDIBPrimary);
                        break;
                    case 45:        //  Top DIB secondary product
                        summedValue = tList.Sum(tdo => tdo.TopDIBSecondary);
                        break;
                    case 46:        //  Defect code
                        munRows = tList.Count(
                            delegate(TreeDO tdo)
                            {
                                return tdo.DefectCode == "" && tdo.DefectCode == null;
                            });
                        if(munRows > 0)
                            summedValue = munRows;
                        break;
                    case 47:        //  Diameter at defect
                        summedValue = tList.Sum(tdo => tdo.DiameterAtDefect);
                        break;
                    case 48:        // Void percent
                        summedValue = tList.Sum(tdo => tdo.VoidPercent);
                        break;
                }   //  end switch
                if (summedValue > 0) fieldsToPrint.Add(n.ToString());
            }   //  end for n loop

            return fieldsToPrint;
        }   //  end CheckForData


        public string[] BuildColumnHeaders(string[] reportColumns, List<string> FieldsToPrint)
        {
            //  used mostly for A2 reports
            //  Turn horizontal to vertical
            StringBuilder sb = new StringBuilder();
            string[] completedHeader = new string[10];
            int nthPosition = 0;
            string[] spaceArray = new string[7] { " ", "  ", "   ", "    ", "     ", "      ", "       "};

            //  loop through 10 times to get each line built for the complete header
            for(int n = 0; n < 10; n++)
            {
                for (int k = 0; k < FieldsToPrint.Count; k++)
                {
                    sb.Append(reportColumns[Convert.ToInt16(FieldsToPrint[k])].Substring(nthPosition,1));
                    //  append appropriate spaces here
                    switch (FieldsToPrint[k].ToString())
                    {
                        case "8": case "9": case "23": case "25": case "28": case "33": case "49":
                            sb.Append(spaceArray[0]);
                            break;
                        case "0": case "5": case "6": case "7": case "11": case "22": case "26": case "27": 
                        case "29": case "34": case "38":
                            sb.Append(spaceArray[1]);
                            break;
                        case "13": case "14": case "15": case "16": case "17": case "18": case "19": 
                        case "20": case "21": case "32": case "35": case "36": case "37": case "41": 
                        case "46": case "47":
                            sb.Append(spaceArray[2]);
                            break;
                        case "1": case "2": case "3": case "24": case "40": case "43": case "44": 
                        case "45": case "48":
                            sb.Append(spaceArray[3]);
                            break;
                        case "12": case "30": case "31": case "42":
                            sb.Append(spaceArray[4]);
                            break;
                        case "4":
                            sb.Append(spaceArray[5]);
                            break;
                        case "10": case "39":
                            sb.Append(spaceArray[6]);
                            break;
                    }   //  end switch
                }   //  end for k loop

                //  add completed string to header
                completedHeader[n] = sb.ToString();
                nthPosition++;
                sb.Clear();
            }   //  end for n loop

            return completedHeader;
        }   //  end BuildColumnHeaders

        //  build print array for A03 report -- individual tree listing
        public void buildPrintArray(TreeDO tdo, List<string> fieldsToPrint, ref int[] fieldLengths, 
                                            ref List<string> prtFields)
        {
            for (int k = 0; k < fieldsToPrint.Count; k++)
            {
                int fieldNumber = Convert.ToInt16(fieldsToPrint[k]);
                switch (fieldNumber)
                {
                    case 0:         //  stratum
                        prtFields.Add(tdo.Stratum.Code.PadLeft(2, ' '));
                        fieldLengths.SetValue(3, k);
                        break;
                    case 1:         //  cutting unit
                        prtFields.Add(tdo.CuttingUnit.Code.PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 2:         //  plot number
                        if (tdo.Plot == null)
                            prtFields.Add("    ");
                        else prtFields.Add(tdo.Plot.PlotNumber.ToString().PadLeft(4, ' '));
                        fieldLengths.SetValue(5, k);
                        break;
                    case 3:         // tree number
                        prtFields.Add(tdo.TreeNumber.ToString().PadLeft(4, ' '));
                        fieldLengths.SetValue(5, k);
                        break;
                    case 4:         //  species
                        if(tdo.Species == "" || tdo.Species == " " || tdo.Species == null)
                            prtFields.Add(tdo.TreeDefaultValue.Species.PadRight(6,' '));
                        else prtFields.Add(tdo.Species.PadRight(6, ' '));
                        fieldLengths.SetValue(7, k);
                        break;
                    case 5:         //  primary product
                        prtFields.Add(tdo.SampleGroup.PrimaryProduct.PadLeft(2, '0'));
                        fieldLengths.SetValue(3, k);
                        break;
                    case 6:         //  secondary product
                        prtFields.Add(tdo.SampleGroup.SecondaryProduct.PadLeft(2, ' '));
                        fieldLengths.SetValue(3, k);
                        break;
                    case 7:         //  sample group
                        prtFields.Add(tdo.SampleGroup.Code.PadLeft(2, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 8:         //  cut leave
                        prtFields.Add(tdo.SampleGroup.CutLeave);
                        fieldLengths.SetValue(2, k);
                        break;
                    case 9:         //  Count measure
                        prtFields.Add(tdo.CountOrMeasure);
                        fieldLengths.SetValue(2, k);
                        break;
                    case 10:        //  tree count
                        prtFields.Add(tdo.TreeCount.ToString().PadLeft(7, ' '));
                        fieldLengths.SetValue(8, k);
                        break;
                    case 11:        //  unit of measure
                        prtFields.Add(tdo.SampleGroup.UOM.PadLeft(2, '0'));
                        fieldLengths.SetValue(3, k);
                        break;
                    case 12:        //  KPI
                        if (tdo.KPI == 0)
                            prtFields.Add(" ");
                        else prtFields.Add(tdo.KPI.ToString().PadLeft(5, ' '));
                        fieldLengths.SetValue(6, k);
                        break;
                    case 13:        //  cull defect primary product
                        prtFields.Add(tdo.TreeDefaultValue.CullPrimary.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 14:        //  hidden defect primary product
                        //  check hidden defect from tree; if zero check in TreeDefaultValue
                        if (tdo.HiddenPrimary == 0)
                        {
                            if (tdo.TreeDefaultValue.HiddenPrimary > 0)
                                prtFields.Add(tdo.TreeDefaultValue.HiddenPrimary.ToString().PadLeft(3, ' '));
                            else prtFields.Add("  0");
                        }
                        else prtFields.Add(tdo.HiddenPrimary.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 15:        //  seen defect primary product
                        prtFields.Add(tdo.SeenDefectPrimary.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 16:        //  recoverable defect percent primary
                        if(tdo.RecoverablePrimary == 0.0)
                            prtFields.Add(tdo.TreeDefaultValue.Recoverable.ToString().PadLeft(3, ' '));
                        else prtFields.Add(tdo.RecoverablePrimary.ToString().PadLeft(3,' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 17:        //  cull defect secondary product
                        prtFields.Add(tdo.TreeDefaultValue.CullSecondary.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 18:        //  hidden defect secondary product
                        prtFields.Add(tdo.TreeDefaultValue.HiddenSecondary.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 19:        //  seen defect secondary product
                        prtFields.Add(tdo.SeenDefectSecondary.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 20:        //  slope percent
                        prtFields.Add(tdo.Plot.Slope.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 21:        //  marker's initials
                        if (tdo.Initials == null)
                            prtFields.Add("   ");
                        else if (tdo.Initials.Length > 3)
                            prtFields.Add(tdo.Initials.Substring(0, 3).PadLeft(3, ' '));
                        else prtFields.Add(tdo.Initials.PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 22:        //  yield component (now chargeable)  (now yield component 5/2015)
                        prtFields.Add(tdo.Stratum.YieldComponent.PadLeft(2, ' '));
                        //prtFields.Add(tdo.TreeDefaultValue.Chargeable.PadLeft(2, ' '));
                        fieldLengths.SetValue(3, k);
                        break;
                    case 23:        //  live dead
                        if(tdo.LiveDead == "" || tdo.LiveDead == " " || tdo.LiveDead == null)
                            prtFields.Add(tdo.TreeDefaultValue.LiveDead);
                        else prtFields.Add(tdo.LiveDead);
                        fieldLengths.SetValue(2, k);
                        break;
                    case 24:        //  contract species
                        if (tdo.TreeDefaultValue.ContractSpecies == null)
                            prtFields.Add("    ");
                        else prtFields.Add(tdo.TreeDefaultValue.ContractSpecies.PadLeft(4, ' '));
                        fieldLengths.SetValue(5, k);
                        break;
                    case 25:        //  tree grade
                        if(tdo.Grade == "" || tdo.Grade == " " || tdo.Grade == null)
                            prtFields.Add(tdo.TreeDefaultValue.TreeGrade);
                        else prtFields.Add(tdo.Grade);
                        fieldLengths.SetValue(2, k);
                        break;
                    case 26:        //  height to first live limb
                        prtFields.Add(tdo.HeightToFirstLiveLimb.ToString().PadLeft(2, ' '));
                        fieldLengths.SetValue(3, k);
                        break;
                    case 27:        //  pole length
                        prtFields.Add(tdo.PoleLength.ToString().PadLeft(2, ' '));
                        fieldLengths.SetValue(3, k);
                        break;
                    case 28:        //  clear face
                        prtFields.Add(tdo.ClearFace);
                        fieldLengths.SetValue(2, k);
                        break;
                    case 29:        //  crown ratio
                        prtFields.Add(tdo.CrownRatio.ToString().PadLeft(2, ' '));
                        fieldLengths.SetValue(3, k);
                        break;
                    case 30:        //  DBH
                        prtFields.Add(String.Format("{0,5:F1}", tdo.DBH).PadLeft(5, ' '));
                        fieldLengths.SetValue(6, k);
                        break;
                    case 31:        //  DRCOB
                        prtFields.Add(String.Format("{0,5:F1}", tdo.DRC).PadLeft(5, ' '));
                        fieldLengths.SetValue(6, k);
                        break;
                    case 32:        //  total height
                        prtFields.Add(tdo.TotalHeight.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 33:        //  merch height type
                        prtFields.Add(tdo.TreeDefaultValue.MerchHeightType);
                        fieldLengths.SetValue(2, k);
                        break;
                    case 34:        //  merch height log length
                        prtFields.Add(tdo.TreeDefaultValue.MerchHeightLogLength.ToString().PadLeft(2, ' '));
                        fieldLengths.SetValue(3, k);
                        break;
                    case 35:        //  merch height primary product
                        prtFields.Add(tdo.MerchHeightPrimary.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 36:        //  merch height secondary product
                        prtFields.Add(tdo.MerchHeightSecondary.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 37:        //  reference height percent
                        prtFields.Add(tdo.TreeDefaultValue.ReferenceHeightPercent.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 38:        //  form class
                        if(tdo.FormClass == 0)
                            prtFields.Add(tdo.TreeDefaultValue.FormClass.ToString().PadLeft(2,' '));
                        else prtFields.Add(tdo.FormClass.ToString().PadLeft(2, ' '));
                        fieldLengths.SetValue(3, k);
                        break;
                    case 39:        //  average Z form
                        prtFields.Add(String.Format("{0,7:F3}", tdo.TreeDefaultValue.AverageZ).PadLeft(7, ' '));
                        fieldLengths.SetValue(8, k);
                        break;
                    case 40:        //  upper stem diameter
                        prtFields.Add(String.Format("{0,4:F1}", tdo.UpperStemDiameter).PadLeft(4, ' '));
                        //prtFields.Add(Utilities.FormatField(tdo.UpperStemDOB,"{0,4:F1}").ToString().PadLeft(4, ' '));
                        fieldLengths.SetValue(5, k);
                        break;
                    case 41:        //  height to upper stem
                        prtFields.Add(tdo.UpperStemHeight.ToString().PadLeft(3, ' '));
                        fieldLengths.SetValue(4, k);
                        break;
                    case 42:        //  DBH bark thickness ratio
                        prtFields.Add(String.Format("{0,5:F2}", tdo.TreeDefaultValue.BarkThicknessRatio).PadLeft(5, ' '));
                        fieldLengths.SetValue(6, k);
                        break;
                    case 43:        //  DBH double bark thickness
                        prtFields.Add(String.Format("{0,4:F1}", tdo.DBHDoubleBarkThickness).PadLeft(4, ' '));
                        fieldLengths.SetValue(5, k);
                        break;
                    case 44:        //  top DIB primary product
                        prtFields.Add(String.Format("{0,4:F1}", tdo.TopDIBPrimary).PadLeft(4, ' '));
                        fieldLengths.SetValue(5, k);
                        break;
                    case 45:        //  top DIB secondary product
                        prtFields.Add(String.Format("{0,4:F1}", tdo.TopDIBSecondary).PadLeft(4, ' '));
                        fieldLengths.SetValue(5, k);
                        break;
                    case 46:        //  defect code
                        prtFields.Add(tdo.DefectCode.PadLeft(3,' '));
                        fieldLengths.SetValue(4, ' ');
                        break;
                    case 47:        //  diameter at defect
                        prtFields.Add(String.Format("{0,4:F1}", tdo.DiameterAtDefect).PadLeft(4, ' '));
                        fieldLengths.SetValue(5, k);
                        break;
                    case 48:        //  void percent
                        prtFields.Add(tdo.VoidPercent.ToString().PadLeft(2, ' '));
                        fieldLengths.SetValue(3, k);
                        break;
                }   //  end switch
            }   //  end for k loop

            return;
        }   //  end buildPrintArray for A03 report

        //  Builds print array for remarks in the A03 report
        public ArrayList buildRemarksArray(TreeDO tdo)
        {
            ArrayList remarksArray = new ArrayList();
            remarksArray.Add(" ");
            remarksArray.Add(tdo.Stratum.Code.PadLeft(2, ' '));
            remarksArray.Add(tdo.CuttingUnit.Code.PadLeft(3, ' '));
            remarksArray.Add(tdo.Plot.PlotNumber.ToString().PadLeft(4, ' '));
            remarksArray.Add(tdo.TreeNumber.ToString().PadLeft(4, ' '));
            remarksArray.Add(tdo.Species.PadRight(6, ' '));
            remarksArray.Add(tdo.Remarks??(" "));

            return remarksArray;
        }   //  end buildRemarksArray for A03 report

        //  probably going to have several build print array functions for various reports
        public List<string> buildPrintArray(TreeDO tl, string cruiseName, List<TreeCalculatedValuesDO> cvList,
                                        int hgtOne, int hgtTwo, string volType)
        {
            //  builds line for A5 or A7 reports (A05 and A07 now)
            string fieldFormat1 = "{0,5:F1}";
            string fieldFormat2 = "{0,3:F0}";
            string fieldFormat3 = "{0,7:F1}";
            string fieldFormat5 = "{0,7:F3}";
            string fieldFormat6 = "{0,8:F3}";

            var treeArray = new List<string>();
            //  first need line for calculated values for this tree (tl)
            TreeCalculatedValuesDO oneTree = cvList.Find(
                delegate(TreeCalculatedValuesDO cv)
                {
                    return cv.Tree_CN == tl.Tree_CN;
                });


            if(oneTree == null)
            {
                throw new InvalidOperationException("Missing a tree in Tree Calculated Values.");
            }//end if

            //  Load array
            treeArray.Add(" ");
            treeArray.Add(tl.Stratum.Code.PadLeft(2, ' '));
            treeArray.Add(tl.CuttingUnit.Code.PadLeft(3, ' '));
            if (tl.Plot == null)
                treeArray.Add("    ");
            else if (tl.Plot.PlotNumber > 0)
                treeArray.Add(tl.Plot.PlotNumber.ToString().PadLeft(4, ' '));
            else treeArray.Add("    ");
            treeArray.Add(tl.TreeNumber.ToString().PadLeft(4,' '));
            treeArray.Add(tl.Species??(" ").PadRight(6,' '));
            treeArray.Add(String.Format(fieldFormat1, tl.DBH).PadLeft(5, ' '));
          
            //  Add heights
            switch (hgtOne)
            {
                case 1:
                    treeArray.Add(String.Format(fieldFormat2, tl.TotalHeight).PadLeft(3, ' '));
                    break;
                case 2:
                    treeArray.Add(String.Format(fieldFormat2, tl.MerchHeightPrimary).PadLeft(3, ' '));
                    break;
                case 3:
                    treeArray.Add(String.Format(fieldFormat2, tl.MerchHeightSecondary).PadLeft(3, ' '));
                    break;
                case 4:
                    treeArray.Add(String.Format(fieldFormat2, tl.UpperStemHeight).PadLeft(3, ' '));
                    break;
                case 0:
                    treeArray.Add("   ");
                    break;
            }   //  end switch on hgtOne

            switch (hgtTwo)
            {
                case 1:
                    treeArray.Add(String.Format(fieldFormat2, tl.TotalHeight).PadLeft(3, ' '));
                    break;
                case 2:
                    treeArray.Add(String.Format(fieldFormat2, tl.MerchHeightPrimary).PadLeft(3, ' '));
                    break;
                case 3:
                    treeArray.Add(String.Format(fieldFormat2, tl.MerchHeightSecondary).PadLeft(3, ' '));
                    break;
                case 4:
                    treeArray.Add(String.Format(fieldFormat2, tl.UpperStemHeight).PadLeft(3, ' '));
                    break;
                case 0:
                    treeArray.Add("   ");
                    break;
            }   //  end switch one hgtTwo

            if(volType == "BDFT")   //  load BDFT volumes
            {
                treeArray.Add(String.Format(fieldFormat3, oneTree.GrossBDFTPP).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat3, oneTree.NetBDFTPP).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat3, oneTree.GrossBDFTSP).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat3, oneTree.NetBDFTSP).PadLeft(7, ' '));
            }
            else if(volType == "CUFT")      //  load CUFT volumes
            {
                treeArray.Add(String.Format(fieldFormat3, oneTree.GrossCUFTPP).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat3, oneTree.NetCUFTPP).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat3, oneTree.GrossCUFTSP).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat3, oneTree.NetCUFTSP).PadLeft(7, ' '));
            }   //  end if volType

            //  expansion factors
            treeArray.Add(String.Format(fieldFormat5, tl.TreeFactor).PadLeft(7, ' '));
            treeArray.Add(String.Format(fieldFormat5, tl.PointFactor).PadLeft(7, ' '));
            treeArray.Add(String.Format(fieldFormat6, tl.ExpansionFactor).PadLeft(8, ' '));

            //  expanded volumes
            double calcValue = 0;
            if (volType == "BDFT")
            {
                calcValue = tl.ExpansionFactor * oneTree.GrossBDFTPP;
                treeArray.Add(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));
                calcValue = tl.ExpansionFactor * oneTree.NetBDFTPP;
                treeArray.Add(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));
                calcValue = tl.ExpansionFactor * oneTree.GrossBDFTSP;
                treeArray.Add(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));
                calcValue = tl.ExpansionFactor * oneTree.NetBDFTSP;
                treeArray.Add(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));
              
            }
            else if(volType == "CUFT")
            {
                calcValue = tl.ExpansionFactor * oneTree.GrossCUFTPP;
                treeArray.Add(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));
                calcValue = tl.ExpansionFactor * oneTree.NetCUFTPP;
                treeArray.Add(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));
                calcValue = tl.ExpansionFactor * oneTree.GrossCUFTSP;
                treeArray.Add(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));
                calcValue = tl.ExpansionFactor * oneTree.NetCUFTSP;
                treeArray.Add(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));
            }   //  endif volType

            
            return treeArray;
        }   //  end buildPrintArray


        public List<string> buildPrintArray(TreeDO tdo, List<TreeCalculatedValuesDO> tcvList, int hgtOne, int hgtTwo,
                                                string contentType)
        {
            //  overloaded for Biomass Data report --- A10
            //  And dollar value since forms are very similar
            var treeArray = new List<string>();
            string fieldFormat1 = "{0,5:F1}";
            string fieldFormat2 = "{0,3:F0}";
            string fieldFormat3 = "{0,7:F1}";
            string fieldFormat5 = "{0,7:F3}";
            string fieldFormat6 = "{0,6:F0}";

            //  find coordinating calculated values for current tree
            TreeCalculatedValuesDO oneTree = tcvList.Find(
                delegate(TreeCalculatedValuesDO tcvdo)
                {
                    return tcvdo.Tree_CN == tdo.Tree_CN;
                });
            //  load print array
            treeArray.Add(" ");
            treeArray.Add(tdo.Stratum.Code.PadLeft(2, ' '));
            treeArray.Add(tdo.CuttingUnit.Code.PadLeft(3, ' '));
            if (tdo.Plot == null)
                treeArray.Add("    ");
            else
            {
                if (tdo.Plot.PlotNumber != 0)
                    treeArray.Add(tdo.Plot.PlotNumber.ToString().PadLeft(4, ' '));
                else treeArray.Add("    ");
            }   //  endif plot is null
            treeArray.Add(tdo.TreeNumber.ToString().PadLeft(4, ' '));
            treeArray.Add(tdo.Species??(" ").PadRight(6, ' '));
            treeArray.Add(String.Format(fieldFormat1, tdo.DBH).PadLeft(5, ' '));

            //  Add heights
            switch (hgtOne)
            {
                case 1:
                    treeArray.Add(String.Format(fieldFormat2, tdo.TotalHeight).PadLeft(3, ' '));
                    break;
                case 2:
                    treeArray.Add(String.Format(fieldFormat2, tdo.MerchHeightPrimary).PadLeft(3, ' '));
                    break;
                case 3:
                    treeArray.Add(String.Format(fieldFormat2, tdo.MerchHeightSecondary).PadLeft(3, ' '));
                    break;
                case 4:
                    treeArray.Add(String.Format(fieldFormat2, tdo.UpperStemHeight).PadLeft(3, ' '));
                    break;
                case 0:
                    treeArray.Add("   ");
                    break;
            }   //  end switch on hgtOne

            switch (hgtTwo)
            {
                case 1:
                    treeArray.Add(String.Format(fieldFormat2, tdo.TotalHeight).PadLeft(3, ' '));
                    break;
                case 2:
                    treeArray.Add(String.Format(fieldFormat2, tdo.MerchHeightPrimary).PadLeft(3, ' '));
                    break;
                case 3:
                    treeArray.Add(String.Format(fieldFormat2, tdo.MerchHeightSecondary).PadLeft(3, ' '));
                    break;
                case 4:
                    treeArray.Add(String.Format(fieldFormat2, tdo.UpperStemHeight).PadLeft(3, ' '));
                    break;
                case 0:
                    treeArray.Add("   ");
                    break;
            }   //  end switch one hgtTwo

            double calcValue = 0;
            if (contentType == "biomass")
            {
                //  add component values
                treeArray.Add(String.Format(fieldFormat6, oneTree.BiomassMainStemPrimary).PadLeft(6, ' '));
                treeArray.Add(String.Format(fieldFormat6, oneTree.BiomassMainStemSecondary).PadLeft(6, ' '));
                treeArray.Add(String.Format(fieldFormat6, oneTree.Biomassfoliage).PadLeft(6, ' '));
                treeArray.Add(String.Format(fieldFormat6, oneTree.Biomasslivebranches).PadLeft(6, ' '));
                treeArray.Add(String.Format(fieldFormat6, oneTree.Biomassdeadbranches).PadLeft(6, ' '));
                treeArray.Add(String.Format(fieldFormat6, oneTree.BiomassTip).PadLeft(6, ' '));
                treeArray.Add(String.Format(fieldFormat6, oneTree.Biomasstotalstem).PadLeft(6, ' '));

                //  add expansion factors
                treeArray.Add(String.Format(fieldFormat5, tdo.TreeFactor).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat5, tdo.PointFactor).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat5, tdo.ExpansionFactor).PadLeft(7, ' '));
            }
            else if (contentType == "value")
            {
                treeArray.Add(String.Format(fieldFormat3, oneTree.ValuePP).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat3, oneTree.ValueSP).PadLeft(7, ' '));
                calcValue = tdo.ExpansionFactor * oneTree.ValuePP;
                treeArray.Add(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));
                calcValue = tdo.ExpansionFactor * oneTree.ValueSP;
                treeArray.Add(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));

                //  expansion factors
                treeArray.Add(String.Format(fieldFormat5, tdo.TreeFactor).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat5, tdo.PointFactor).PadLeft(7, ' '));
                treeArray.Add(String.Format(fieldFormat5, tdo.ExpansionFactor).PadLeft(7, ' '));
            }   //  endif contentType

            return treeArray;
        }   //  end buildPrintArray


        public List<string> buildPrintArray(TreeDO tdo)
        {
            //  overloaded for geospatial report -- tree page (A13)
            var treeArray = new List<string>();
            //string fieldFormat1 = "{0,10:F2}";
            //string fieldFormat2 = "{0,9:F2}";

            treeArray.Add(" ");
            treeArray.Add(tdo.Stratum.Code.PadLeft(2, ' '));
            treeArray.Add(tdo.CuttingUnit.Code.PadLeft(3,' '));
            if (tdo.Plot == null)
                treeArray.Add("   ");
            else treeArray.Add(tdo.Plot.PlotNumber.ToString().PadLeft(3,' '));
            treeArray.Add(tdo.TreeNumber.ToString().PadLeft(4,' '));
            if (tdo.SampleGroup == null)
                treeArray.Add("  ");
            else treeArray.Add(tdo.SampleGroup.Code.PadLeft(2,' '));
            treeArray.Add("");
            treeArray.Add("");
            treeArray.Add("");
            treeArray.Add("");
            //  October 2012 -- tree table doesn't have coordinates or metadata at the moment
/*            if (tdo.XCoordinate != 0.0)
            {
                treeArray[6] = Utilities.FormatField(tdo.XCoordinate, fieldFormat1).ToString();
                if (tdo.YCoordinate == 0.0)
                    treeArray[7] = "---------";
                else treeArray[7] = Utilities.FormatField(tdo.YCoordinate, fieldFormat1).ToString();
                if(tdo.ZCoordinate == 0.0)
                    treeArray[8] = "---------";
                else treeArray[8] = Utilities.FormatFIeld(tdo.ZCoordinate, fieldFormat2).ToString();
                treeArray[9] = tdo.MetaData;
            }   //  endif coordinates are missing
 */
            return treeArray;
        }   //  end buildPrintArray


    }   //  end TreeListMethods
}
