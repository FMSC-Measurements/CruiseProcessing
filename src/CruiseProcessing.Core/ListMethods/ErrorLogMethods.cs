using CruiseDAL.DataObjects;
using CruiseProcessing.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing
{
    public class ErrorLogMethods
    {
        public List<ErrorLogDO> errList { get; } = new List<ErrorLogDO>();
        protected CPbusinessLayer DataLayer { get; }

        public ErrorLogMethods(CPbusinessLayer dataLayer)
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        public void LoadError(string nameOfTable, string errorLevel,
                                    string errorNumber, long CN_identifier, string colName)
        {
            //  need a business layer function here to load error messages
            ErrorLogDO eld = new ErrorLogDO();
            eld.TableName = nameOfTable;
            eld.Level = errorLevel;
            eld.CN_Number = CN_identifier;
            eld.ColumnName = colName;
            eld.Message = errorNumber;
            eld.Program = "CruiseProcessing";
            errList.Add(eld);
            return;
        }   //  end LoadError

        //  Method checks here since not specific to one particular table
        public int CheckCruiseMethods(List<StratumDO> strList, List<TreeDO> tList, IDialogService dialogService)
        {
            int errsFound = 0;
            //  pull trees by stratum for cruise method checks
            foreach (StratumDO str in strList)
            {
                List<TreeDO> treeList = TreeListMethods.GetCurrentStratum(tList, str.Code);

                //  warn user if the stratum has no trees at all
                if (treeList.Count == 0)
                {
                    string warnMsg = "WARNING!  Stratum ";
                    warnMsg += str.Code;
                    warnMsg += " has no trees recorded.  Some reports may not be complete.\nContinue?";

                    if (!dialogService.ShowWarningAskYesNo(warnMsg))
                        return -1;
                }   //  endif no trees

                //  What needs to be checked?  By method
                switch (str.Method)
                {
                    case "100":
                        //  Check for all measured trees in stratum
                        errsFound += CheckAllMeasured(treeList);
                        //  Check for tree count greater than 1
                        foreach (TreeDO tdo in treeList)
                        {
                            if (tdo.TreeCount > 1)
                            {
                                LoadError("Tree", "E", "10", (long)tdo.Tree_CN, "TreeCount");
                                errsFound++;
                            }   //  endif tree count greater than 1
                        }   //  end foreach loop
                        break;

                    case "FIX":
                    case "PNT":
                        //  Check for all measured trees in stratum
                        errsFound += CheckAllMeasured(treeList);
                        //  Check for no measured trees per sample group
                        errsFound += CheckForMeasuredTrees(treeList, (int)str.Stratum_CN, str.Method);
                        break;

                    case "3P":
                    case "STR":
                    case "S3P":
                        //  Check for measured trees with no DBH or height
                        errsFound += CheckForNoDBH(treeList);
                        //  Check 3P only for measured trees with no KPI
                        if (str.Method == "3P") errsFound += CheckForNoKPI(treeList);
                        //  Check for sample group in CountTree with no measured tree
                        errsFound += CheckForMeasuredTrees(treeList, (int)str.Stratum_CN, str.Method);
                        break;

                    case "F3P":
                    case "P3P":
                        //  Check for measured tree with no KPI
                        errsFound += CheckForNoKPI(treeList);
                        //  Check for measured trees with no DBH or height
                        errsFound += CheckForNoDBH(treeList);
                        //  Check for no measured trees when total tree count > 0
                        errsFound += CheckForMeasuredTrees(treeList, (int)str.Stratum_CN, str.Method);
                        break;

                    case "3PPNT":
                        foreach (TreeDO tdo in treeList)
                        {
                            // Check for blank product, uom and cut/leave on count trees
                            if (tdo.CountOrMeasure == "C" && tdo.SampleGroup.PrimaryProduct != "" &&
                                tdo.SampleGroup.UOM != "" && tdo.SampleGroup.CutLeave != "")
                            {
                                LoadError("Tree", "E", "16", (long)tdo.Tree_CN, "NoName");
                                errsFound++;
                            }   //  endif
                            //  Check for blank species, product, UOM and cut/leave on measured trees numbered zero
                            if (tdo.CountOrMeasure == "M" && tdo.TreeNumber == 0 &&
                                tdo.Species != "" && tdo.SampleGroup.PrimaryProduct != "" &&
                                tdo.SampleGroup.UOM != "" && tdo.SampleGroup.CutLeave != "")
                            {
                                LoadError("Tree", "E", "16", (long)tdo.Tree_CN, "NoName");
                                errsFound++;
                            }   //  endif
                            //  Check for count tree in a measured plot
                            //  Pull plot records first
                            List<TreeDO> justPlots = treeList.FindAll(
                                delegate (TreeDO jp)
                                {
                                    return jp.CountOrMeasure == "M" && jp.Plot.PlotNumber == 0;
                                });
                            foreach (TreeDO jpo in justPlots)
                            {
                                if (jpo.CountOrMeasure == "C" && jpo.TreeNumber != 0)
                                {
                                    LoadError("Tree", "E", "15", (long)tdo.Tree_CN, "NoName");
                                    errsFound++;
                                }   //  endif
                            }   //  end foreach loop on justPlots
                            //  Check for more than one sample group in the stratum
                            int nthRow = treeList.FindIndex(
                                delegate (TreeDO td)
                                {
                                    return tdo.SampleGroup.Code != treeList[0].SampleGroup.Code;
                                });
                            if (nthRow >= 0)
                            {
                                LoadError("Tree", "E", "17", (long)str.Stratum_CN, "NoName");
                                errsFound++;
                            }   //  endif nthRow
                        }   //  end foreach loop
                        //  Check for no measured tree when total tree count > 0
                        errsFound += CheckForMeasuredTrees(treeList, (int)str.Stratum_CN, str.Method);
                        break;

                    case "FIXCNT":
                        //  no measured trees allowed
                        int mthRow = treeList.FindIndex(
                            delegate (TreeDO tdo)
                            {
                                return tdo.CountOrMeasure == "M";
                            });
                        if (mthRow >= 0)
                        {
                            LoadError("Tree", "E", "9", (long)treeList[mthRow].Tree_CN, "CountOrMeasure");
                            errsFound++;
                        }   //  endif nthRow
                        //  UOM must be 04
                        List<TreeDO> justUOM = treeList.FindAll(
                            delegate (TreeDO ju)
                            {
                                return ju.SampleGroup.UOM != "04";
                            });
                        if (justUOM.Count > 0)
                        {
                            LoadError("SampleGroup", "E", "7", (long)str.Stratum_CN, "UOM");
                            errsFound++;
                        }   //  endif
                        break;

                    case "PCM":
                    case "FCM":
                        //  Check for no measured tree when total tree count > 0
                        errsFound += CheckForMeasuredTrees(treeList, (int)str.Stratum_CN, str.Method);
                        break;
                }   //  end switch on method
            }   //  end foreach loop on stratum
            return errsFound;
        }   //  end CheckCruiseMethods

        private int CheckAllMeasured(List<TreeDO> treeList)
        {
            int totalErrs = 0;
            List<TreeDO> allMeasured = treeList.FindAll(
                delegate (TreeDO tdo)
                {
                    return tdo.CountOrMeasure == "C";
                });
            if (allMeasured.Count > 0)
            {
                foreach (TreeDO alm in allMeasured)
                {
                    LoadError("Tree", "E", "11", (long)alm.Tree_CN, "NoName");
                    totalErrs++;
                }   //  end foreach loop
            }   //  endif
            return totalErrs;
        }   //  end CheckAllMeasured

        private int CheckForNoKPI(List<TreeDO> treeList)
        {
            int totalErrs = 0;
            List<TreeDO> noKPI = treeList.FindAll(
                delegate (TreeDO nk)
                {
                    return nk.CountOrMeasure == "M" && nk.KPI == 0.0 && nk.STM == "N";
                });
            if (noKPI.Count > 0)
            {
                foreach (TreeDO nok in noKPI)
                {
                    LoadError("Tree", "E", "27", (long)nok.Tree_CN, "NoName");
                    totalErrs++;
                }   //  end foreach loop
            }   //  endif
            return totalErrs;
        }   //  end CheckForNoKPI

        private int CheckForNoDBH(List<TreeDO> treeList)
        {
            int totalErrs = 0;
            List<TreeDO> noDBH = treeList.FindAll(
                delegate (TreeDO nd)
                {
                    return nd.CountOrMeasure == "M" && nd.DBH == 0.0 && nd.DRC == 0.0;
                });
            if (noDBH.Count > 0)
            {
                foreach (TreeDO nod in noDBH)
                {
                    LoadError("Tree", "E", "11", (long)nod.Tree_CN, "NoName");
                    totalErrs++;
                }   //  end foreach loop
            }   //  endif

            return totalErrs;
        }   //  end CheckForNoDBH

        private int CheckForMeasuredTrees(List<TreeDO> treeList, int currST_CN, string currMeth)
        {
            var dataLayer = DataLayer;
            int numErrs = 0;
            ErrorLogMethods elm = new ErrorLogMethods(dataLayer); // TODO fix this instantiating a new elm. I think we can just use the current instance. We may need to create a class level accumulator to count errors. 
            //  if the tree list is empty, could be the strata just doesn't have any trees.
            //  this is probably a cruise in process so return no errors on this stratum.
            //  October 2014
            if (treeList.Count == 0)
                return numErrs;

            //  need sample group(s) for current stratum
            List<SampleGroupDO> sgList = dataLayer.getSampleGroups(currST_CN);
            //  check is method based
            switch (currMeth)
            {
                case "3P":
                case "STR":
                case "S3P":
                    foreach (SampleGroupDO sg in sgList)
                    {
                        //  find count records for the sample group
                        List<CountTreeDO> justGroups = dataLayer.getCountTrees((long)sg.SampleGroup_CN);
                        //  sum up tree count
                        float totalCount = justGroups.Sum(jg => jg.TreeCount);
                        if (totalCount > 0)
                        {
                            //  Any measured trees?  look for just one measured tree in the stratum
                            List<TreeDO> justMeasured = treeList.FindAll(
                                delegate (TreeDO t)
                                {
                                    return t.CountOrMeasure == "M" && t.SampleGroup_CN == sg.SampleGroup_CN && t.Stratum_CN == currST_CN;
                                });
                            if (justMeasured.Count == 0)
                            {
                                //  this is the error
                                elm.LoadError("SampleGroup", "W", "30", (long)sg.SampleGroup_CN, "NoName");
                                numErrs++;
                            }   //  endif
                        }   //  endif totalCount
                    }   //  end foreach loop
                    break;

                default:        //  all area based
                    foreach (SampleGroupDO sg in sgList)
                    {
                        List<TreeDO> justGroups = treeList.FindAll(
                            delegate (TreeDO t)
                            {
                                return t.SampleGroup_CN == sg.SampleGroup_CN;
                            });

                        float totalCount = justGroups.Sum(jg => jg.TreeCount);
                        if (totalCount > 0)
                        {
                            //  any measured trees?
                            List<TreeDO> justMeasured = justGroups.FindAll(
                                delegate (TreeDO td)
                                {
                                    return td.CountOrMeasure == "M";
                                });
                            if (justMeasured.Count == 0)
                            {
                                // here's the error
                                elm.LoadError("SampleGroup", "W", "30", (long)sg.SampleGroup_CN, "NoName");
                                numErrs++;
                            }   //  endif
                        }   //  endif totalCount
                    }   //  end foreach loop
                    break;
            }   //  end switch
            return numErrs;
        }   //  end CheckForMeasuredTrees
    }   //  end ErrorLogMethods
}