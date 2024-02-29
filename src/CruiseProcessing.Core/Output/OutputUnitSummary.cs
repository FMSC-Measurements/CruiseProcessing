using CruiseDAL.DataObjects;
using CruiseProcessing.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CruiseProcessing
{
    public class OutputUnitSummary : ReportGeneratorBase
    {
        private List<RegionalReports> areaBasedOutput = new List<RegionalReports>();
        private List<RegionalReports> treeBasedBySpecies = new List<RegionalReports>();
        private List<RegionalReports> treeBaseBySampleGroup = new List<RegionalReports>();
        private List<string> prtFields = new List<string>();
        private int[] fieldLengths;

        public OutputUnitSummary(CPbusinessLayer dataLayer, HeaderFieldData headerData, string reportID) : base(dataLayer, headerData, reportID)
        {
        }

        public void createUnitSummary(StreamWriter strWriteOut, ref int pageNum)
        {
            //  fill current title
            string currentTitle = fillReportTitle(currentReport);
            //  field lengths is the same for each section
            fieldLengths = new int[] { 1, 6, 5, 4, 8, 3, 5, 9, 6 };
            SetReportTitles(currentTitle, 6, 0, 0, reportConstants.FCTO, reportConstants.FPPO);
            //  there are three sections to this report and they each have a different format.
            //  but all are by unit, stratum, sample group and species
            //  load each group as needed
            loadAreaBased();
            loadTreeBased();
            //  write output lists as needed
            numOlines = 0;
            if (areaBasedOutput.Count > 0)
                writeAreaBased(strWriteOut, ref pageNum);
            if (treeBasedBySpecies.Count > 0)
                writeTreeBasedBySpecies(strWriteOut, ref pageNum);
            if (treeBaseBySampleGroup.Count > 0)
            {
                fieldLengths = new int[] { 6, 6, 4, 8, 3, 5, 9, 6 };
                writeTreeBasedBySampleGroup(strWriteOut, ref pageNum);
            }   //  endif
            return;
        }   //  end createUnitSummary

        private void loadAreaBased()
        {
            //  get cutting units
            List<PlotDO> justPlots = DataLayer.getPlotsOrdered();
            List<CuttingUnitDO> cutList = DataLayer.getCuttingUnits();
            foreach (CuttingUnitDO cu in cutList)
            {
                cu.Strata.Populate();
                foreach (StratumDO s in cu.Strata)
                {
                    switch (s.Method)
                    {
                        case "PNT":
                        case "PCM":
                        case "PCMTRE":
                        case "P3P":
                        case "FIX":
                        case "FCM":
                        case "F3P":
                        case "FIXCNT":
                            //  get trees ordered by sample group and species for each unit
                            string[] searchValues = new string[2] { s.Stratum_CN.ToString(), cu.CuttingUnit_CN.ToString() };
                            List<TreeDO> justTrees = DataLayer.getTreesOrdered("WHERE Stratum_CN = @p1 AND CuttingUnit_CN = @p2 ORDER BY ",
                                                                                "Species", searchValues);

                            foreach (TreeDO jt in justTrees)
                            {
                                //  find group in output list
                                int nthRow = areaBasedOutput.FindIndex(
                                    delegate (RegionalReports r)
                                    {
                                        return r.value1 == cu.Code && r.value2 == s.Code &&
                                            r.value3 == jt.SampleGroup.Code && r.value4 == jt.Species &&
                                            r.value5 == jt.SampleGroup.PrimaryProduct;
                                    });
                                if (nthRow >= 0)
                                {
                                    switch (s.Method)
                                    {
                                        case "PNT":
                                        case "PCM":
                                        case "PCMTRE":
                                        case "P3P":
                                            //  per K.Cormier -- November 2017
                                            //  tree counts greater than one don't get
                                            //  summed properly
                                            //  changed the following code accordingly
                                            //areaBasedOutput[nthRow].value7++;
                                            areaBasedOutput[nthRow].value7 += jt.TreeCount;
                                            break;

                                        case "FIX":
                                        case "FCM":
                                        case "F3P":
                                        case "FIXCNT":
                                            //areaBasedOutput[nthRow].value8++;
                                            areaBasedOutput[nthRow].value8 += jt.TreeCount;
                                            break;
                                    }   //  end switch on method
                                }
                                else if (nthRow < 0)
                                {
                                    //  new group -- add it
                                    RegionalReports r = new RegionalReports();
                                    r.value1 = cu.Code;
                                    r.value2 = s.Code;
                                    r.value3 = jt.SampleGroup.Code;
                                    r.value4 = jt.Species;
                                    r.value5 = jt.SampleGroup.PrimaryProduct;
                                    switch (s.Method)
                                    {
                                        case "PNT":
                                        case "PCM":
                                        case "P3P":
                                        case "PCMTRE":
                                            //  for point methods, sum number of trees (NOT expansion factor) for average BA calculate later
                                            //  see comment above from K.Cormier on tree counts
                                            //r.value7++;
                                            r.value7 += jt.TreeCount;
                                            r.value8 = 0;
                                            break;

                                        case "FIX":
                                        case "FCM":
                                        case "F3P":
                                        case "FIXCNT":
                                            //  for fixed method, same as for point but put in different value for ease of calculation
                                            r.value7 = 0;
                                            //r.value8++;
                                            r.value8 += jt.TreeCount;
                                            break;
                                    }   //  end switch on method
                                    //  need total number of plots for this cutting unit
                                    List<PlotDO> pList = justPlots.FindAll(
                                        delegate (PlotDO p)
                                        {
                                            return p.CuttingUnit_CN == cu.CuttingUnit_CN && p.Stratum_CN == s.Stratum_CN;
                                        });
                                    r.value11 = pList.Count();
                                    r.value12 = s.BasalAreaFactor;
                                    r.value13 = s.FixedPlotSize;
                                    areaBasedOutput.Add(r);
                                }   //  endif on nthRow
                            }   //  end foreach loop  on justTrees
                            break;
                    }   //  end switch on method
                }   //  end foreach loop on stratum
            }   //  end foreach loop on cutting units
            return;
        }   //  end loadAreaBased

        private void loadTreeBased()
        {
            //  tree-based methods only
            //  reworked October 2016
            //  needs tree counts from count table and all measured trees
            List<CuttingUnitDO> cutList = DataLayer.getCuttingUnits();
            List<CountTreeDO> cntList = DataLayer.getCountTrees();
            List<SampleGroupDO> sampGroups = DataLayer.getSampleGroups();
            foreach (CuttingUnitDO cud in cutList)
            {
                cud.Strata.Populate();
                foreach (StratumDO s in cud.Strata)
                {
                    switch (s.Method)
                    {
                        case "100":
                            //  100% method has measured trees only no counts
                            string[] searchValues = new string[2] { s.Stratum_CN.ToString(), cud.CuttingUnit_CN.ToString() };
                            List<TreeDO> justMeasured = DataLayer.getTreesOrdered("WHERE Tree.CountOrMeasure = 'M' AND Tree.Stratum_CN = @p1 AND Tree.CuttingUnit_CN = @p2 ORDER BY ",
                                                            "Species", searchValues);
                            foreach (TreeDO jm in justMeasured)
                            {
                                //  Find and update group or add new
                                int nthRow = treeBasedBySpecies.FindIndex(
                                    delegate (RegionalReports r)
                                    {
                                        return r.value1 == cud.Code && r.value2 == s.Code &&
                                            r.value3 == jm.SampleGroup.Code && r.value4 == jm.Species;
                                    });

                                if (nthRow >= 0)
                                {
                                    // update count for the group
                                    treeBasedBySpecies[nthRow].value8 += jm.TreeCount;
                                }
                                else if (nthRow < 0)
                                {
                                    //  add new group
                                    RegionalReports rr = new RegionalReports();
                                    rr.value1 = cud.Code;
                                    rr.value2 = s.Code;
                                    rr.value3 = jm.SampleGroup.Code;
                                    rr.value4 = jm.Species;
                                    rr.value5 = jm.SampleGroup.PrimaryProduct;
                                    treeBasedBySpecies.Add(rr);
                                }   //  endif on nthRow
                            }   //  end foreach loop
                            break;      //  end case 100
                        case "3P":
                        case "S2P":
                        case "STR":
                            //  these methods have count records in CountTree Table
                            //  so pull all count records for cutting unit and stratum
                            //  by sample group
                            List<SampleGroupDO> justGroups = sampGroups.FindAll(
                                delegate (SampleGroupDO sg)
                                {
                                    return sg.Stratum_CN == s.Stratum_CN;
                                });

                            foreach (SampleGroupDO jg in justGroups)
                            {
                                //  pull all count records
                                List<CountTreeDO> justCounts = cntList.FindAll(
                                    delegate (CountTreeDO cl)
                                    {
                                        return cl.CuttingUnit_CN == cud.CuttingUnit_CN &&
                                            cl.SampleGroup_CN == jg.SampleGroup_CN;
                                    });

                                foreach (CountTreeDO jc in justCounts)
                                {
                                    //  load species list
                                    RegionalReports rr = new RegionalReports();
                                    rr.value1 = cud.Code;
                                    rr.value2 = s.Code;
                                    rr.value3 = jc.SampleGroup.Code;
                                    //  if TDV_CN has a valid value, load next fields as appropraite
                                    if (jc.TreeDefaultValue_CN > 0)
                                    {
                                        rr.value4 = jc.TreeDefaultValue.Species;
                                        rr.value5 = jc.TreeDefaultValue.PrimaryProduct;
                                        rr.value8 = jc.TreeCount;
                                        treeBasedBySpecies.Add(rr);
                                    }
                                    else
                                    {
                                        rr.value4 = "      ";
                                        rr.value5 = "  ";
                                        rr.value8 = jc.TreeCount;
                                        treeBaseBySampleGroup.Add(rr);
                                    }   //  endi
                                }   //  end for each loop
                            }   //  end foreach on sample groups

                            //  now add all measured trees to the outputlist
                            //  November 2017  BUT the other programs don't put the counts
                            //  in the count table if the user chooses to enter all the
                            //  counts in the tree table.  So once again, this program has to
                            //  accomodate for this by using tree counts if the count table
                            //  is empty.
                            string[] srchValues = new string[2] { s.Stratum_CN.ToString(), cud.CuttingUnit_CN.ToString() };
                            //  so if the count table returns 0 rows, need count records from the
                            //  tree table as well as measured trees.
                            if (cntList.Count == 0)
                                justMeasured = DataLayer.getTreesOrdered("WHERE Tree.TreeCount > 0 AND Tree.Stratum_CN = @p1 AND Tree.CuttingUnit_CN = @p2 ORDER BY ",
                                                            "Species", srchValues);
                            else justMeasured = DataLayer.getTreesOrdered("WHERE Tree.CountOrMeasure = 'M' AND Tree.TreeCount > 0 AND Tree.Stratum_CN = @p1 AND Tree.CuttingUnit_CN = @p2 ORDER BY ",
                                                        "Species", srchValues);
                            foreach (TreeDO jm in justMeasured)
                            {
                                if (treeBasedBySpecies.Count > 0)
                                {
                                    //  Find in species list and update or add new
                                    int nthRow = treeBasedBySpecies.FindIndex(
                                        delegate (RegionalReports r)
                                        {
                                            return r.value1 == cud.Code && r.value2 == s.Code &&
                                                   r.value3 == jm.SampleGroup.Code && r.value4 == jm.Species;
                                        });
                                    if (nthRow >= 0)
                                    {
                                        //   just update the total for the group
                                        if (jm.TreeCount > 0)
                                            treeBasedBySpecies[nthRow].value8 += jm.TreeCount;
                                    }
                                    else if (nthRow < 0)
                                    {
                                        //  new group
                                        RegionalReports rr = new RegionalReports();
                                        rr.value1 = cud.Code;
                                        rr.value2 = s.Code;
                                        rr.value3 = jm.SampleGroup.Code;
                                        rr.value4 = jm.Species;
                                        rr.value5 = jm.SampleGroup.PrimaryProduct;
                                        if (jm.TreeCount > 0)
                                            rr.value8 = jm.TreeCount;
                                        treeBasedBySpecies.Add(rr);
                                    }   //  endif on row
                                }
                                else if (treeBaseBySampleGroup.Count > 0)
                                {
                                    //  Find in species list and update or add new
                                    int nthRow = treeBaseBySampleGroup.FindIndex(
                                        delegate (RegionalReports r)
                                        {
                                            return r.value1 == cud.Code && r.value2 == s.Code &&
                                                   r.value3 == jm.SampleGroup.Code && r.value4 == jm.Species;
                                        });
                                    if (nthRow >= 0)
                                    {
                                        //   just update the total for the group
                                        if (jm.TreeCount > 0)
                                            treeBaseBySampleGroup[nthRow].value8 += jm.TreeCount;
                                    }
                                    else if (nthRow < 0)
                                    {
                                        //  new group
                                        RegionalReports rr = new RegionalReports();
                                        rr.value1 = cud.Code;
                                        rr.value2 = s.Code;
                                        rr.value3 = jm.SampleGroup.Code;
                                        rr.value4 = jm.Species;
                                        rr.value5 = jm.SampleGroup.PrimaryProduct;
                                        if (jm.TreeCount > 0)
                                            rr.value8 = jm.TreeCount;
                                        treeBaseBySampleGroup.Add(rr);
                                    }   //  endif on row
                                }
                                else
                                {
                                    //  Find in species list and update or add new
                                    int nthRow = treeBasedBySpecies.FindIndex(
                                        delegate (RegionalReports r)
                                        {
                                            return r.value1 == cud.Code && r.value2 == s.Code &&
                                                   r.value3 == jm.SampleGroup.Code && r.value4 == jm.Species;
                                        });
                                    if (nthRow >= 0)
                                    {
                                        //   just update the total for the group
                                        if (jm.TreeCount > 0)
                                            treeBasedBySpecies[nthRow].value8 += jm.TreeCount;
                                    }
                                    else if (nthRow < 0)
                                    {
                                        //  new group
                                        RegionalReports rr = new RegionalReports();
                                        rr.value1 = cud.Code;
                                        rr.value2 = s.Code;
                                        rr.value3 = jm.SampleGroup.Code;
                                        rr.value4 = jm.Species;
                                        rr.value5 = jm.SampleGroup.PrimaryProduct;
                                        if (jm.TreeCount > 0)
                                            rr.value8 = jm.TreeCount;
                                        treeBasedBySpecies.Add(rr);
                                    }   //  endif on row
                                }   //  endif
                            }   //  end foreach loop
                            break;
                    }   //  end switch on method
                }   //  end foreach loop on strata
            }   //  end foreach loop on cutting units
            return;
        }   //  end loadTreeBased

        private void writeAreaBased(StreamWriter strWriteOut, ref int pageNumb)
        {
            double calcValue = 0;
            int firstLine = 1;
            foreach (RegionalReports abo in areaBasedOutput)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                reportHeaders.A14columns, 16, ref pageNumb, "");
                if (firstLine == 1)
                {
                    //  write section header
                    strWriteOut.WriteLine("                              AREA BASED CUTTING UNITS");
                    firstLine = 0;
                }   //  endif
                prtFields.Clear();
                prtFields.Add("");
                //  load info data first
                prtFields.Add(abo.value1.PadLeft(3, ' '));
                prtFields.Add(abo.value2.PadLeft(2, ' '));
                prtFields.Add(abo.value3.PadLeft(2, ' '));
                prtFields.Add(abo.value4.PadRight(6, ' '));
                prtFields.Add(abo.value5.PadLeft(2, ' '));
                //  average BA
                if (abo.value7 > 0 && abo.value11 > 0)
                {
                    calcValue = (abo.value7 * abo.value12) / abo.value11;
                    prtFields.Add(String.Format("{0,6:F0}", calcValue).PadLeft(6, ' '));
                }
                else if (abo.value7 == 0)
                    prtFields.Add("      ");

                if (abo.value8 > 0 && abo.value11 > 0)
                {
                    calcValue = (abo.value8 * abo.value13) / abo.value11;
                    prtFields.Add(String.Format("{0,7:F0}", calcValue).PadLeft(7, ' '));
                }
                else if (abo.value8 == 0)
                    prtFields.Add("       ");

                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            strWriteOut.WriteLine(reportConstants.subtotalLine2);
            return;
        }   //  end writeAreaBased

        private void writeTreeBasedBySpecies(StreamWriter strWriteOut, ref int pageNumb)
        {
            int firstLine = 1;
            foreach (RegionalReports tbo in treeBasedBySpecies)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                reportHeaders.A14columns, 16, ref pageNumb, "");
                if (firstLine == 1)
                {
                    //  write section header
                    strWriteOut.WriteLine("                              TREE BASED CUTTING UNITS -- TALLY BY SPECIES");
                    firstLine = 0;
                }       //  endif
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(tbo.value1.PadLeft(3, ' '));
                prtFields.Add(tbo.value2.PadLeft(2, ' '));
                prtFields.Add(tbo.value3.PadLeft(2, ' '));
                prtFields.Add(tbo.value4.PadRight(6, ' '));
                prtFields.Add(tbo.value5.PadLeft(2, ' '));
                prtFields.Add("      ");
                prtFields.Add(String.Format("{0,7:F0}", tbo.value8).PadLeft(7, ' '));

                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            strWriteOut.WriteLine(reportConstants.subtotalLine2);
            return;
        }   //  end writeTreeBasedBySpecies

        private void writeTreeBasedBySampleGroup(StreamWriter strWriteOut, ref int pageNumb)
        {
            int firstLine = 1;
            foreach (RegionalReports tbo in treeBaseBySampleGroup)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                reportHeaders.A14columns, 16, ref pageNumb, "");
                if (firstLine == 1)
                {
                    //  write section header
                    strWriteOut.WriteLine("                              TREE BASED CUTTING UNITS -- TALLY BY SAMPLE GROUP");
                    firstLine = 0;
                }   //  endif
                prtFields.Clear();
                prtFields.Add(tbo.value1.PadLeft(3, ' '));
                prtFields.Add(tbo.value2.PadLeft(2, ' '));
                prtFields.Add(tbo.value3.PadLeft(2, ' '));
                prtFields.Add(tbo.value4.PadLeft(6, ' '));
                prtFields.Add(tbo.value5.PadLeft(2, ' '));
                prtFields.Add("      ");
                prtFields.Add(String.Format("{0,7:F0}", tbo.value8).PadLeft(7, ' '));
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            strWriteOut.WriteLine(reportConstants.subtotalLine2);
            return;
        }   //  end writeTreeBasedBySampleGroup
    }
}