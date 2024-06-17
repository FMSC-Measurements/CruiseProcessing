using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using CruiseDAL.V3.Models;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CruiseProcessing
{
    public class EditChecks
    {
        public static ErrorLogCollection CheckErrors(CpDataLayer dataLayer)
        {
            var errors = new ErrorLogCollection();

            //  clear out error log table for just CruiseProcessing before performing checks
            dataLayer.DeleteCruiseProcessingErrorMessages();

            string currentRegion = dataLayer.getRegion();
            bool isVLL = dataLayer.getVLL() == "V";
            //string isVLL = bslyr.getVLL();

            ValidateSale(dataLayer, errors);

            ValidateStrata(dataLayer, errors);

            ValidateSampleGroups(dataLayer, errors);

            ValidateUnits(dataLayer, errors);

            ValidateCountTrees(dataLayer, errors);

            ValidateTrees(dataLayer, errors);

            ValidateLogs(currentRegion, isVLL, dataLayer, errors);

            ValidateVolumeEqs(isVLL, dataLayer, errors);

            CheckCruiseMethods(dataLayer, errors);

            return errors;
        }

        private static void ValidateSale(CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            List<SaleDO> saleList = dataLayer.GetAllSaleRecords();
            var saleCount = saleList.Count;
            if (saleCount == 0)
                errors.AddError("Sale", "E", "25", 0, "NoName");       //  error 25 -- table cannot be empty
            else if (saleCount > 1)
                errors.AddError("Sale", "E", "28", 28, "SaleNumber");       //  error 28 -- more than one sale record not allowed
            if (saleCount == 1)
            {
                var sale = saleList[0];

                //  and blank sale number
                if (String.IsNullOrWhiteSpace(sale.SaleNumber))
                { errors.AddError("Sale", "E", " 8Sale Number", sale.Sale_CN.Value, "SaleNumber"); }

                // CalculateTreeValues.ProcessTree requires district to be a integer value
                if (!int.TryParse(sale.District, out var _))
                {
                    errors.AddError("Sale", "E", " Sale District Not a Number", sale.Sale_CN.Value, "District");
                }
            }
        }

        private static void ValidateCountTrees(CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            List<StratumDO> strList = dataLayer.GetStrata();

            foreach (StratumDO st in strList)
            {
                if (Has3PcountsMissingTDV(dataLayer, st))
                    errors.AddError("CountTree", "E", "Cannot tally by sample group for 3P strata.", (long)st.Stratum_CN, "TreeDefaultValue");
            }

            bool Has3PcountsMissingTDV(CpDataLayer dataLayer, StratumDO currStratum)
            {
                //  is this a 3P stratum?
                if (currStratum.Method == "3P")
                {
                    List<CountTreeDO> cntList = dataLayer.getCountTrees();
                    //  find stratum in CountTree by finding cutting unit first
                    List<CuttingUnitStratumDO> uList = dataLayer.getCuttingUnitStratum((long)currStratum.Stratum_CN);
                    foreach (CuttingUnitStratumDO ul in uList)
                    {
                        List<CountTreeDO> justCounts = cntList.FindAll(
                            delegate (CountTreeDO ctd)
                            {
                                return ctd.CuttingUnit_CN == ul.CuttingUnit_CN && ctd.SampleGroup.Stratum.Code == currStratum.Code;
                            });   //  end
                        foreach (CountTreeDO jc in justCounts)
                            if (jc.TreeDefaultValue_CN == null)
                                return true;
                    }
                }
                return false;
            }
        }

        private static void ValidateUnits(CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            List<CuttingUnitDO> cuList = dataLayer.getCuttingUnits();
            if (cuList.Count == 0)
                errors.AddError("Cutting Unit", "E", "25", 0, "NoName"); // cruise has no units
        }

        private static void ValidateStrata(CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            List<StratumDO> strList = dataLayer.GetStrata();
            if (strList.Count == 0)
            {
                errors.AddError("Stratum", "E", "25", 25, "NoName"); // cruise has no strata
            } 
            else
            {
                foreach (StratumDO sdo in strList)
                {
                    ValidateStratum(sdo, dataLayer, errors);
                }   //  end foreach loop
            }
        }

        private static void ValidateStratum(StratumDO sdo, CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            //  check for valid fixed plot size or BAF for each stratum
            double BAForFPSvalue = StratumMethods.GetBafOrFps(sdo);
            if ((sdo.Method == "PNT"
                || sdo.Method == "P3P"
                || sdo.Method == "PCM"
                || sdo.Method == "PCMTRE"
                || sdo.Method == "3PPNT") // is variable radius cruise method
                && BAForFPSvalue == 0)
            {
                errors.AddError("Stratum", "E", "22", (long)sdo.Stratum_CN, "BasalAreaFactor");
            }
            else if ((sdo.Method == "FIX"
                || sdo.Method == "F3P"
                || sdo.Method == "FIXCNT"
                || sdo.Method == "FCM") // is fixed size plot method
                && BAForFPSvalue == 0)
            {
                errors.AddError("Stratum", "E", "23", (long)sdo.Stratum_CN, "FixedPlotSize");
            }

            //  check for acres on area based methods
            double currAcres = Utilities.AcresLookup((long)sdo.Stratum_CN, dataLayer, sdo.Code);

            if ((sdo.Method == "PNT"
                || sdo.Method == "FIX"
                || sdo.Method == "P3P"
                || sdo.Method == "F3P"
                || sdo.Method == "PCM"
                || sdo.Method == "3PPNT"
                || sdo.Method == "FCM"
                || sdo.Method == "PCMTRE")
                && currAcres == 0)
            {
                errors.AddError("Stratum", "E", "24", (long)sdo.Stratum_CN, "NoName");
            }
            else if ((sdo.Method == "100"
                || sdo.Method == "3P"
                || sdo.Method == "S3P"
                || sdo.Method == "STR")
                && currAcres == 0)
            {
                errors.AddError("Stratum", "W", "Stratum has no acres", (long)sdo.Stratum_CN, "NoName");
            }

            //  August 2017 -- added check for valid yield component code
            if (sdo.YieldComponent != "CL"
                && sdo.YieldComponent != "CD"
                && sdo.YieldComponent != "ND"
                && sdo.YieldComponent != "NL"
                && sdo.YieldComponent != ""
                && sdo.YieldComponent != " "
                && sdo.YieldComponent != null)
            {
                errors.AddError("Stratum", "W", "Yield Component has invalid code", (long)sdo.Stratum_CN, "YieldComponent");
            }
        }

        private static void ValidateSampleGroups(CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            List<StratumDO> strList = dataLayer.GetStrata();
            List<SampleGroupDO> sampGroups = dataLayer.getSampleGroups();

            foreach (SampleGroupDO sg in sampGroups)
            {
                if (String.IsNullOrWhiteSpace(sg.PrimaryProduct))
                {
                    errors.AddError("SampleGroup", "E", $"Sg: {sg.Code} Missing Primary Product", sg.SampleGroup_CN.Value, nameof(SampleGroupDO.PrimaryProduct));
                }

                if (String.IsNullOrWhiteSpace(sg.UOM))
                {
                    errors.AddError("SampleGroup", "E", $"Sg: {sg.Code} Missing UOM", sg.SampleGroup_CN.Value, nameof(SampleGroupDO.UOM));
                }

                if (sg.CutLeave != "C" && sg.CutLeave != "L")
                {
                    errors.AddError("SampleGroup", "E", $"Sg: {sg.Code} Invalid CutLeave Value", sg.SampleGroup_CN.Value, nameof(SampleGroupDO.CutLeave));
                }

                //  check length of sample group code and issue a warning message if more than 2 characters
                if (sg.Code.Length > 2)
                    dataLayer.LogError("SampleGroup", (int)sg.SampleGroup_CN, "W", "Sample Group is too long. Results may not be as expected.");
            }   //  end foreach loop
        }

        private static bool ValidateTrees(CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            var currRegion = dataLayer.getRegion();
            var trees = dataLayer.getTrees().ToArray();

            if (trees.Length == 0)
            {
                errors.AddError("Tree", "E", "25", 0, "NoName");
            }

            foreach (var t in trees.Where(x => String.IsNullOrEmpty(x.Species)))
            {
                errors.AddError("Tree", "E", "21", t.Tree_CN.Value, "Species");
            }

            foreach (var t in trees.Where(x => x.TreeDefaultValue_CN == null))
            {
                errors.AddError("Tree", "E", "31", t.Tree_CN.Value, "TreeDefaultValue");
            }

            // validate measure trees
            var allMeasureTrees = dataLayer.JustMeasuredTrees();

            //  March 2016 -- if the entire cruise has no measured trees, that is a critical error
            //  and should stop the program.  Since no report can be generated, a message box appears to warn the user
            //  of the condition.
            // TODO verify comment
            if (allMeasureTrees.Count == 0)
            {
                throw new InvalidOperationException("Cruise Contains No Measure Trees");
            }

            var strata = dataLayer.GetStrata();
            foreach (var st in strata)
            {
                // fixcnt is not required to have height and typically doesn't have any measurements other than dbh
                var isFixCNT = st.Method == CruiseMethods.FIXCNT;
                if(isFixCNT) { continue; }

                var stratumMeasureTrees = dataLayer.JustMeasuredTrees(st.Stratum_CN.Value);

                foreach (var tree in stratumMeasureTrees)
                {
                    if (tree.UpperStemDiameter > tree.DBH)
                    {
                        errors.AddError("Tree", "E", "18", tree.Tree_CN.Value, "UpperStemDiameter");
                    }

                    //  check for recoverable greater than seen defect
                    //  cannot make this check for region 10 as they do things incorrectly with defect.
                    if (currRegion != "10")
                    {
                        if (tree.RecoverablePrimary > tree.SeenDefectPrimary)
                        {
                            errors.AddError("Tree", "E", "29", tree.Tree_CN.Value, "RecoverablePrimary");
                        }
                    }

                    //  top DIB secondary greater than primary
                    if (tree.TopDIBPrimary > 0.0 && tree.TopDIBSecondary > 0.0)
                    {
                        if (tree.TopDIBSecondary > tree.TopDIBPrimary)
                        {
                            errors.AddError("Tree", "E", "4", tree.Tree_CN.Value, "TopDIBSecondary");
                        }
                    }

                    // check heights
                    if (tree.TotalHeight == 0 &&
                            tree.MerchHeightPrimary == 0 &&
                            tree.MerchHeightSecondary == 0 &&
                            tree.UpperStemHeight == 0)
                    {
                        errors.AddError("Tree", "E", "32", tree.Tree_CN.Value, "Height");
                    }

                    //  check for blank or null tree grade
                    //  if this is true, it gets put into a separate population which
                    //  can impact certain reports
                    if (String.IsNullOrWhiteSpace(tree.Grade))
                    {
                        errors.AddError("Tree", "E", "8", tree.Tree_CN.Value, "Tree Grade");
                    }
                }
            }

            return true;
        }

        private static void ValidateLogs(string currentRegion, bool isVLL, CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            List<LogDO> logList = dataLayer.getLogs();
            if (logList.Count > 0) { return; }

            var trees = dataLayer.getTrees();

            foreach (var tree in trees)
            {
                var treeLogs = LogMethods.GetLogRecords(logList, tree.Tree_CN.Value);
                if (treeLogs.Count == 0) continue;

                if (treeLogs.Count > 20)
                {
                    errors.AddError("Tree", "E", "13", tree.Tree_CN.Value, "NoName");
                }

                if (tree.IsFallBuckScale == 1)
                {
                    foreach (var log in treeLogs)
                    {
                        if (log.NetBoardFoot > log.GrossBoardFoot)
                            errors.AddError("Log", "E", "20", log.Log_CN.Value, "GrossVolume");
                        if (log.NetCubicFoot > log.GrossCubicFoot)
                            errors.AddError("Log", "E", "20", log.Log_CN.Value, "GrossVolume");
                    }
                }

                // note: Vertical Log Length can't be enabled by user and hasn't been used since V1
                // VLL is typicaly just used for export grade/high value stuff.
                if (isVLL)
                {
                    foreach (LogDO ld in treeLogs)
                    {
                        if (String.IsNullOrWhiteSpace(ld.LogNumber))
                        {
                            errors.AddError("Log", "E", "19", (long)ld.Log_CN, "LogNumber");
                        }   //  endif log number missing
                        if (String.IsNullOrWhiteSpace(ld.Grade))
                        {
                            errors.AddError("Log", "E", "19", (long)ld.Log_CN, "Grade");
                        }   //  endif log grade missing
                        if (String.IsNullOrWhiteSpace(ld.ExportGrade))
                        {
                            errors.AddError("Log", "E", "19", (long)ld.Log_CN, "ExportGrade");
                        }   //  endif export grade (log sort) missing
                        if (ld.Length == 0)
                        {
                            errors.AddError("Log", "E", "19", (long)ld.Log_CN, "Length");
                        }   //  endif log length missing
                    }
                }

                foreach (LogDO ld in treeLogs)
                {
                    // Check Defect
                    if (ld.PercentRecoverable > ld.SeenDefect)
                    {
                        errors.AddError("Log", "E", "29", (long)ld.Log_CN, "PercentRecoverable");
                    }

                    if (currentRegion != "05" && String.IsNullOrWhiteSpace(ld.Grade))
                    {
                        errors.AddError("Log", "E", "9", (long)ld.Log_CN, "Grade");
                    }
                }
            }
        }

        private static void ValidateVolumeEqs(bool isVLL, CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            List<VolumeEquationDO> volList = dataLayer.getVolumeEquations();
            //  pull region
            if (volList.Count == 0)
                errors.AddError("VolumeEquation", "E", "25", 0, "NoName");
            else //  means the table is not empty and checks can proceed
            {
                // check all sp prod combos have a volume equation
                var spProdTrees = dataLayer.DAL.Query<SpeciesProduct>("SELECT t.Tree_CN as RecID,  t.Species, sg.PrimaryProduct " +
                    "FROM Tree as t " +
                    "JOIN SampleGroup AS sg USING (SampleGroup_CN) " +
                    "GROUP BY t.Species, sg.PrimaryProduct;").ToArray();

                // TODO should this only check measure trees? Would this be better to do the check at the SG level?
                foreach (var tree in spProdTrees)
                {
                    if (!volList.Any(x => x.Species == tree.Species && x.PrimaryProduct == tree.PrimaryProduct))
                    {
                        errors.AddError("Tree", "E", "12", tree.RecID, "Species");
                    }
                }

                foreach (var volEq in volList)
                {
                    if (volEq.VolumeEquationNumber.Length > 10)
                    {
                        errors.AddError("VolumeEquation", "E", "1", volEq.rowID.Value, "VolumeEquationNumber");
                    }

                    if (volEq.VolumeEquationNumber.Contains("DVE"))
                    {
                        if (volEq.CalcTopwood == 1
                            && (volEq.CalcBoard == 0 && volEq.CalcCubic == 0 && volEq.CalcCord == 0))
                        {
                            errors.AddError("VolumeEquation", "E", "3", volEq.rowID.Value, "VolumeFlags");
                        }

                        if (volList.Any(x =>
                            x.rowID != volEq.rowID
                            && x.VolumeEquationNumber == volEq.VolumeEquationNumber
                            && x.PrimaryProduct == volEq.PrimaryProduct
                            && x.TopDIBPrimary == volEq.TopDIBPrimary))
                        {
                            errors.AddError("VolumeEquation", "E", "6", volEq.rowID.Value, "TopDIBPrimary");
                        }
                    }

                    if (volEq.TopDIBSecondary > volEq.TopDIBPrimary)
                    {
                        errors.AddError("VolumeEquation", "E", "4", volEq.rowID.Value, "TopDIBSecondary");
                    }

                    // check for duplicate volEqs
                    if (volList.Any(x =>
                    {
                        return x.rowID != volEq.rowID
                        && x.VolumeEquationNumber == volEq.VolumeEquationNumber
                        && x.Species == volEq.Species
                        && x.PrimaryProduct == volEq.PrimaryProduct;
                    }))
                    {
                        errors.AddError("VolumeEquation", "E", "5", volEq.rowID.Value, "VolumeEquationNumber");
                    }
                }

                if (isVLL)
                {
                    foreach (VolumeEquationDO ved in volList)
                    {
                        if (ved.VolumeEquationNumber.Contains("BEH"))
                        {
                            errors.AddError("VolumeEquation", "W", "Variable log length diameters cannot be computed with BEH equations", (long)ved.rowID, "NoName");
                        }
                        else if (ved.VolumeEquationNumber.Contains("MAT"))
                        {
                            errors.AddError("VolumeEquation", "W", "Variable log length diameters cannot be computed with MAT equations", (long)ved.rowID, "NoName");
                        }
                        else if (ved.VolumeEquationNumber.Contains("DVE"))
                        {
                            errors.AddError("VolumeEquation", "W", "Variable log length diameters cannot be computed with DVE equations", (long)ved.rowID, "NoName");
                        }
                        else if (ved.VolumeEquationNumber.Contains("CLK"))
                        {
                            errors.AddError("VolumeEquation", "W", "Variable log length diameters cannot be computed with CLK equations", (long)ved.rowID, "NoName");
                        }   //  endif on equation number
                    }   //  end foreach loop
                }
            }
        }

        private class SpeciesProduct
        {
            public long RecID { get; set; }
            public string Species { get; set; }
            public string PrimaryProduct { get; set; }
        }

        public static int CheckCruiseMethods(CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            List<StratumDO> strata = dataLayer.GetStrata();

            int errsFound = 0;
            //  pull trees by stratum for cruise method checks
            foreach (StratumDO st in strata)
            {
                var stratumTrees = dataLayer.GetTreesByStratum(st.Code);

                //  What needs to be checked?  By method
                switch (st.Method)
                {
                    case "100":
                        //  Check for all measured trees in stratum
                        errsFound += CheckMethodHasNoCountTrees(stratumTrees, errors);
                        //  Check for tree count greater than 1
                        foreach (TreeDO tree in stratumTrees)
                        {
                            if (tree.TreeCount > 1)
                            {
                                errors.AddError("Tree", "E", "10", tree.Tree_CN.Value, "TreeCount");
                                errsFound++;
                            }   //  endif tree count greater than 1
                        }   //  end foreach loop
                        break;

                    case "FIX":
                    case "PNT":
                        //  Check for all measured trees in stratum
                        errsFound += CheckMethodHasNoCountTrees(stratumTrees, errors);
                        //  Check for no measured trees per sample group
                        errsFound += CheckForMeasuredTrees(stratumTrees, st.Stratum_CN.Value, st.Method, dataLayer, errors);
                        break;

                    case "3P":
                    case "STR":
                    case "S3P":
                        if (st.Method == "3P")
                        {
                            errsFound += CheckForNoKPI(stratumTrees, errors);
                        }

                        //  Check for measured trees with no DBH or height
                        errsFound += CheckForNoDBH(stratumTrees, errors);
                        //  Check 3P only for measured trees with no KPI

                        //  Check for sample group in CountTree with no measured tree
                        errsFound += CheckForMeasuredTrees(stratumTrees, st.Stratum_CN.Value, st.Method, dataLayer, errors);
                        break;

                    case "F3P":
                    case "P3P":
                        //  Check for measured tree with no KPI
                        errsFound += CheckForNoKPI(stratumTrees, errors);
                        //  Check for measured trees with no DBH or height
                        errsFound += CheckForNoDBH(stratumTrees, errors);
                        //  Check for no measured trees when total tree count > 0
                        errsFound += CheckForMeasuredTrees(stratumTrees, st.Stratum_CN.Value, st.Method, dataLayer, errors);
                        break;

                    case "3PPNT":

                        //  Check for more than one sample group in the stratum
                        var sampleGroups = dataLayer.getSampleGroups(st.Stratum_CN.Value);
                        if (sampleGroups.Count > 1)
                        {
                            errors.AddError("Stratum", "E", "17", st.Stratum_CN.Value, "NoName");
                        }

                        // Check for plot only has Count or Measure trees
                        var plots = dataLayer.GetStrataPlots(st.Code);
                        foreach (var plot in plots)
                        {
                            var plotCMcodes = stratumTrees.Where(x => x.Plot_CN == plot.Plot_CN).Select(x => x.CountOrMeasure).Distinct();
                            if (plotCMcodes.Count() > 1)
                            {
                                errors.AddError("Plot", "E", "15", plot.Plot_CN.Value, "NoName");
                            }
                        }

                        //  Check for no measured tree when total tree count > 0
                        errsFound += CheckForMeasuredTrees(stratumTrees, st.Stratum_CN.Value, st.Method, dataLayer, errors);
                        break;

                    case "FIXCNT":
                        //  no measured trees allowed
                        if (stratumTrees.Any(t => t.CountOrMeasure == "M"))
                        {
                            errors.AddError("Stratum", "E", "FixCNT Stratum Can Not Have Measure Trees", st.Stratum_CN.Value, "NoName");
                            errsFound++;
                        }   //  endif nthRow
                        //  UOM must be 04
                        var stSampleGroups = dataLayer.getSampleGroups(st.Stratum_CN.Value);

                        foreach (var sg in stSampleGroups.Where(x => x.UOM != "04"))
                        {
                            errors.AddError("SampleGroup", "E", "7", (long)st.Stratum_CN, "UOM");
                            errsFound++;
                        }

                        break;

                    case "PCM":
                    case "FCM":
                        //  Check for no measured tree when total tree count > 0
                        errsFound += CheckForMeasuredTrees(stratumTrees, st.Stratum_CN.Value, st.Method, dataLayer, errors);
                        break;
                }   //  end switch on method
            }   //  end foreach loop on stratum
            return errsFound;
        }   //  end CheckCruiseMethods

        private static int CheckMethodHasNoCountTrees(IEnumerable<TreeDO> treeList, ErrorLogCollection errors)
        {
            var errsFound = 0;
            foreach (var tree in treeList
                .Where(x => x.CountOrMeasure == "C"))
            {
                errors.AddError("Tree", "E", "11", tree.Tree_CN.Value, "NoName");
                errsFound++;
            }
            return errsFound;
        }

        private static int CheckForNoKPI(IEnumerable<TreeDO> treeList, ErrorLogCollection errors)
        {
            int totalErrs = 0;
            var noKPI = treeList.Where(nk => nk.CountOrMeasure == "M" && nk.KPI == 0.0 && nk.STM == "N");
            if (noKPI.Any())
            {
                foreach (TreeDO nok in noKPI)
                {
                    errors.AddError("Tree", "E", "27", (long)nok.Tree_CN, "NoName");
                    totalErrs++;
                }
            }
            return totalErrs;
        }

        private static int CheckForNoDBH(IEnumerable<TreeDO> treeList, ErrorLogCollection errors)
        {
            int totalErrs = 0;
            var noDBH = treeList.Where(nd => nd.CountOrMeasure == "M" && nd.DBH == 0.0 && nd.DRC == 0.0);
            if (noDBH.Any())
            {
                foreach (TreeDO nod in noDBH)
                {
                    errors.AddError("Tree", "E", "11", (long)nod.Tree_CN, "NoName");
                    totalErrs++;
                }
            }
            return totalErrs;
        }

        private static int CheckForMeasuredTrees(IEnumerable<TreeDO> treeList, long currST_CN, string currMeth, CpDataLayer dataLayer, ErrorLogCollection errors)
        {
            int numErrs = 0;
            //ErrorLogMethods elm = new ErrorLogMethods(dataLayer); // TODO fix this instantiating a new elm. I think we can just use the current instance. We may need to create a class level accumulator to count errors.
            //  if the tree list is empty, could be the strata just doesn't have any trees.
            //  this is probably a cruise in process so return no errors on this stratum.
            //  October 2014
            if (!treeList.Any())
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
                            if (!treeList.Any(t => t.CountOrMeasure == "M" && t.SampleGroup_CN == sg.SampleGroup_CN && t.Stratum_CN == currST_CN))
                            {
                                //  this is the error
                                errors.AddError("SampleGroup", "W", "30", (long)sg.SampleGroup_CN, "NoName");
                                numErrs++;
                            }   //  endif
                        }   //  endif totalCount
                    }   //  end foreach loop
                    break;

                default:        //  all area based
                    foreach (SampleGroupDO sg in sgList)
                    {
                        var sgTrees = treeList.Where(t => t.SampleGroup_CN == sg.SampleGroup_CN)
                            .ToArray();

                        float totalCount = sgTrees.Sum(jg => jg.TreeCount);
                        if (totalCount > 0)
                        {
                            //  any measured trees?
                            if (!sgTrees.Any(t => t.CountOrMeasure == "M"))
                            {
                                // here's the error
                                errors.AddError("SampleGroup", "W", "30", (long)sg.SampleGroup_CN, "NoName");
                                numErrs++;
                            }
                        }
                    }
                    break;
            }
            return numErrs;
        }
    }   //  end EditChecks
}