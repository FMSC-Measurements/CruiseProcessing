using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public class OutputUnitStandTables : CreateTextFile
    {
        #region
        public string currentReport;
        private ArrayList prtFields = new ArrayList();
        private double unitPF = 0;
        private string[] columnHeader = new string[3];
        private List<StandTables> reportData = new List<StandTables>();
        private string GroupedBy;
        private string whatValue;
        private string whatProduct;
        private double[] columnTotals = new double[11];
        private int numPages = 0;
        private int begGroup;
        private int endGroup;
        private double sawVolume = 0;
        private double nonSawVolume = 0;
        private double puSawTotal = 0;
        private double puNonSawTotal = 0;
        private double grSawTotal = 0;
        private double grNonSawTotal = 0;
        private List<PRODO> proList = new List<PRODO>();
        #endregion

        public void CreateUnitStandTables(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  June 2017 need all LCD records
            //List<LCDDO> lcdList = bslyr.getLCD();
            //  fill report title array --  different for these reports
            string currentTitle = unitReportTitle();

            //  set switches based on report
            //  GroupedBy
            switch (currentReport)
            {
                case "UC7":     case "UC8":     case "UC9":     case "UC10":
                case "UC11":    case "UC12":    case "UC13":    case "UC14":
                case "UC15":
                    GroupedBy = "Species";
                    break;
                case "UC16":    case "UC17":    case "UC18":    case "UC19":
                case "UC20":    case "UC21":    case "UC22":    case "UC23":
                case "UC24":
                    GroupedBy = "SampleGroup";
                    break;
                case "UC25":    case "UC26":
                    GroupedBy = "ContractSpecies";
                    break;
            }   //  end switch
            //  What product"
            switch (currentReport)
            {
                case "UC7":     case "UC8":     case "UC9":     case "UC10":    case "UC11": 
                case "UC16":    case "UC17":    case "UC18":    case "UC19":    case "UC20":
                    whatProduct = "SAW";
                    break;
                case "UC12":     case "UC13":     case "UC14":     case "UC21":    case "UC22": 
                case "UC23":
                    whatProduct = "NSAW";
                    break;
                case "UC15":     case "UC24":    case "UC25":    case "UC26":
                    whatProduct = "BOTH";
                    break;
            }   //  end switch
            //  What value?
            switch (currentReport)
            {
                case "UC7":     case "UC16":
                    whatValue = "GBDFT";
                    break;
                case "UC8":     case "UC17":
                    whatValue = "NBDFT";
                    break;
                case "UC9":     case "UC18":
                case "UC12":    case "UC21":
                    whatValue = "GCUFT";
                    break;
                case "UC10":    case "UC19":
                case "UC13":    case "UC22":
                case "UC25":    case "UC26":
                    whatValue = "NCUFT";
                    break;
                case "UC11":    case "UC20":
                case "UC15":    case "UC24":
                    whatValue = "TREES";
                    break;
                case "UC14":    case "UC23":
                    whatValue = "CORDS";
                    break;
            }   //  end switch

            // finish main header
            string secondLine = "";
            switch (currentReport)
            {
                case "UC7":     case "UC8":     case "UC9":     case "UC10":
                case "UC11":    case "UC12":    case "UC13":    case "UC14":
                case "UC15":
                    secondLine = reportConstants.WCUAAS.Replace("XXX", "SPECIES");
                    break;
                case "UC16":    case "UC17":    case "UC18":    case "UC19":
                case "UC20":    case "UC21":    case "UC22":    case "UC23":
                case "UC24":
                    secondLine = reportConstants.WCUAAS.Replace("XXX", "SAMPLE GROUP");
                    break;
                case "UC25":    
                    secondLine = reportConstants.WPUAS;
                    break;
                case "UC26":
                    secondLine = reportConstants.BCUFS;
                    break;
            }   //  end switch
            rh.createReportTitle(currentTitle, 6, 0, 0, secondLine, reportConstants.FCTO);

            //  need cutting unit numbers
            List<CuttingUnitDO> cList = bslyr.getCuttingUnits();
            //  get all LCD data
            List<LCDDO> lcdList = bslyr.getLCD();
            //  get all PRO data
            proList = bslyr.getPRO();

            //  load into report data
            foreach (CuttingUnitDO c in cList)
            {
                StandTables s = new StandTables();
                s.dibClass = c.Code;
                reportData.Add(s);
            }   //  end foreach loop

            //  need LCD data to process
            List<LCDDO> speciesGroups = new List<LCDDO>();
            switch (GroupedBy)
            {
                case "Species":
                    if (whatProduct == "SAW")
                        speciesGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 AND PrimaryProduct = @p2 ",
                                                            "GROUP BY Species", "C", "01");
                    else if (whatProduct == "NSAW" || whatProduct == "BOTH")
                        speciesGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 ", "GROUP BY Species", "C", "");
                    break;
                case "SampleGroup":
                    if (whatProduct == "SAW")
                        speciesGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 AND PrimaryProduct = @p2 ", 
                                                            "GROUP BY SampleGroup", "C", "01");
                    else if (whatProduct == "NSAW" || whatProduct == "BOTH")
                        speciesGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 ", "GROUP BY SampleGroup", "C", "");
                    break;
                case "ContractSpecies":
                    speciesGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 ", "GROUP BY ContractSpecies", "C", "");
                    break;
            }   //  end switch

            if (currentReport == "UC25" || currentReport == "UC26")
            {
                VolumeSummary(strWriteOut, rh, ref pageNumb, speciesGroups, lcdList, proList, cList, 
                                secondLine, currentTitle);
                return;
            }   //  endif report UC25 or UC26
            //  how many pages?
            numPages = (int)Math.Ceiling((decimal)speciesGroups.Count / 10);
            //  loop by number of pages 
            bool lastPage = false;
            //  table is loaded for each page
            for (int n = 1; n <= numPages; n++)
            {
                switch (n)
                {
                    case 1:
                        begGroup = 0;
                        if (speciesGroups.Count < 10)
                            endGroup = speciesGroups.Count;
                        else endGroup = 10;
                        break;
                    case 2:
                        begGroup = 10;
                        endGroup = speciesGroups.Count;
                        break;
                    case 3:
                        begGroup = 20;
                        endGroup = speciesGroups.Count;
                        break;
                    case 4:
                        begGroup = 30;
                        endGroup = speciesGroups.Count;
                        break;
                }   //  end switch
                //  pull lcd data for each group
                //  but beware of STM groups
                int tableColumn = 0;
                for (int k = begGroup; k < endGroup; k++)
                {
                    //  process by cutting unit (except for UC25-UC26)
                    if (currentReport != "UC25" && currentReport != "UC26")
                    {
                        //  speciesGroups will not capture differing STM codes
                        //  needs to pull all LCD record for the group to gett
                        //  accurate prorated volume
                        
                        foreach (CuttingUnitDO cu in cList)
                        {
                            cu.Strata.Populate();
                            //  pull data for groups by stratum
                            foreach (StratumDO stratum in cu.Strata)
                            {
                                List<TreeCalculatedValuesDO> unitTrees = bslyr.getTreeCalculatedValues((int)stratum.Stratum_CN, (int)cu.CuttingUnit_CN);
                                List<TreeCalculatedValuesDO> currentTrees = new List<TreeCalculatedValuesDO>();
                                if (stratum.Method == "100")
                                {
                                    if (GroupedBy == "Species")
                                    {
                                        currentTrees = unitTrees.FindAll(
                                            delegate(TreeCalculatedValuesDO ut)
                                            {
                                                return ut.Tree.Stratum_CN == stratum.Stratum_CN &&
                                                    ut.Tree.CuttingUnit_CN == cu.CuttingUnit_CN &&
                                                    ut.Tree.Species == speciesGroups[k].Species;
                                            });                                        
                                    }
                                    else if (GroupedBy == "SampleGroup")
                                    {
                                        currentTrees = unitTrees.FindAll(
                                            delegate(TreeCalculatedValuesDO ut)
                                            {
                                                return ut.Tree.Stratum_CN == stratum.Stratum_CN &&
                                                    ut.Tree.CuttingUnit_CN == cu.CuttingUnit_CN &&
                                                    ut.Tree.SampleGroup.Code == speciesGroups[k].SampleGroup;
                                            });
                                    }   //  endif
                                    //  There shouldn't be any STM trees in 100% method so total and put in reportData
                                    if(currentTrees.Count > 0)
                                        LoadUnitData(currentTrees, cu, tableColumn);
                                }
                                else
                                {
                                    List<LCDDO> allGroups = new List<LCDDO>();
                                    if (GroupedBy == "Species")
                                    {
                                        //  pull nonSTM groups first
                                        if (whatProduct == "SAW")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.Species == speciesGroups[k].Species &&
                                                        ll.PrimaryProduct == speciesGroups[k].PrimaryProduct &&
                                                        ll.CutLeave == "C" && ll.STM == "N";
                                                });
                                        }
                                        else if (whatProduct == "NSAW")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.Species == speciesGroups[k].Species &&
                                                        ll.PrimaryProduct != "01" &&
                                                        ll.CutLeave == "C" && ll.STM == "N";
                                                });
                                        }
                                        else if (whatProduct == "BOTH")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.Species == speciesGroups[k].Species &&
                                                        ll.CutLeave == "C" && ll.STM == "N";
                                                });

                                        }   // endif
                                    }
                                    else if(GroupedBy == "SampleGroup")
                                    {
                                        //  pull nonSTM groups first
                                        if (whatProduct == "SAW")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.SampleGroup == speciesGroups[k].SampleGroup &&
                                                        ll.PrimaryProduct == speciesGroups[k].PrimaryProduct &&
                                                        ll.CutLeave == "C" && ll.STM == "N";
                                                });
                                        }
                                        else if (whatProduct == "NSAW")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.SampleGroup == speciesGroups[k].SampleGroup &&
                                                        ll.PrimaryProduct != "01" &&
                                                        ll.CutLeave == "C" && ll.STM == "N";
                                                });
                                        }
                                        else if (whatProduct == "BOTH")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.Species == speciesGroups[k].Species &&
                                                        ll.CutLeave == "C" && ll.STM == "N";
                                                });

                                        }       //  endif
                                    }   // endif
                                    if (allGroups.Count > 0)
                                        LoadUnitData(allGroups, cu, tableColumn, stratum.Code);

                                    //  Then pull STM groups and loop through to get trees for the unit
                                    if(GroupedBy == "Species")
                                    {
                                        if (whatProduct == "SAW")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.Species == speciesGroups[k].Species &&
                                                        ll.PrimaryProduct == speciesGroups[k].PrimaryProduct &&
                                                        ll.CutLeave == "C" && ll.STM == "Y";
                                                });
                                        }
                                        else if (whatProduct == "NSAW")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.Species == speciesGroups[k].Species &&
                                                        ll.PrimaryProduct != "01" &&
                                                        ll.CutLeave == "C" && ll.STM == "Y";
                                                });
                                        }
                                        else if (whatProduct == "BOTH")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.Species == speciesGroups[k].Species &&
                                                        ll.CutLeave == "C" && ll.STM == "N";
                                                });

                                        }   //  endif
                                    }
                                    else if(GroupedBy == "SampleGroup")
                                    {
                                        if (whatProduct == "SAW")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.SampleGroup == speciesGroups[k].SampleGroup &&
                                                        ll.PrimaryProduct == speciesGroups[k].PrimaryProduct &&
                                                        ll.CutLeave == "C" && ll.STM == "Y";
                                                });
                                        }
                                        else if (whatProduct == "NSAW")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.SampleGroup == speciesGroups[k].SampleGroup &&
                                                        ll.PrimaryProduct != "01" &&
                                                        ll.CutLeave == "C" && ll.STM == "Y";
                                                });
                                        }
                                        else if (whatProduct == "BOTH")
                                        {
                                            allGroups = lcdList.FindAll(
                                                delegate(LCDDO ll)
                                                {
                                                    return ll.Stratum == stratum.Code &&
                                                        ll.Species == speciesGroups[k].Species &&
                                                        ll.CutLeave == "C" && ll.STM == "N";
                                                });

                                        }   //  endif
                                    }   //  endif
                                
                                    if (allGroups.Count > 0)
                                    {
                                        foreach (LCDDO ag in allGroups)
                                        {
                                            if (GroupedBy == "Species")
                                            {
                                                if (whatProduct == "SAW")
                                                {
                                                    currentTrees = unitTrees.FindAll(
                                                        delegate(TreeCalculatedValuesDO ut)
                                                        {
                                                            return ut.Tree.Stratum_CN == stratum.Stratum_CN &&
                                                                ut.Tree.CuttingUnit_CN == cu.CuttingUnit_CN &&
                                                                ut.Tree.Species == ag.Species &&
                                                                ut.Tree.SampleGroup.PrimaryProduct == ag.PrimaryProduct &&
                                                                ut.Tree.SampleGroup.CutLeave == "C" && ut.Tree.STM == "Y";
                                                        });
                                                }
                                                else if (whatProduct == "NSAW")
                                                {
                                                    currentTrees = unitTrees.FindAll(
                                                        delegate(TreeCalculatedValuesDO ut)
                                                        {
                                                            return ut.Tree.Stratum_CN == stratum.Stratum_CN &&
                                                                ut.Tree.CuttingUnit_CN == cu.CuttingUnit_CN &&
                                                                ut.Tree.Species == ag.Species &&
                                                                ut.Tree.SampleGroup.PrimaryProduct != "01" &&
                                                                ut.Tree.SampleGroup.CutLeave == "C" && ut.Tree.STM == "Y";
                                                        });
                                                }

                                                else if (whatProduct == "BOTH")
                                                {
                                                    currentTrees = unitTrees.FindAll(
                                                        delegate(TreeCalculatedValuesDO ut)
                                                        {
                                                            return ut.Tree.Stratum_CN == stratum.Stratum_CN &&
                                                                ut.Tree.CuttingUnit_CN == cu.CuttingUnit_CN &&
                                                                ut.Tree.Species == ag.Species &&
                                                                ut.Tree.SampleGroup.CutLeave == "C" && ut.Tree.STM == "Y";
                                                        });
                                                }   //  endif
                                            }
                                            else if (GroupedBy == "SampleGroup")
                                            {
                                                if (whatProduct == "SAW")
                                                {
                                                    currentTrees = unitTrees.FindAll(
                                                        delegate(TreeCalculatedValuesDO ut)
                                                        {
                                                            return ut.Tree.Stratum_CN == stratum.Stratum_CN &&
                                                                ut.Tree.CuttingUnit_CN == cu.CuttingUnit_CN &&
                                                                ut.Tree.SampleGroup.Code == speciesGroups[k].SampleGroup &&
                                                                ut.Tree.SampleGroup.PrimaryProduct == speciesGroups[k].PrimaryProduct &&
                                                                ut.Tree.SampleGroup.CutLeave == "C" && ut.Tree.STM == "Y";
                                                        });
                                                }
                                                else if (whatProduct == "NSAW")
                                                {
                                                    currentTrees = unitTrees.FindAll(
                                                        delegate(TreeCalculatedValuesDO ut)
                                                        {
                                                            return ut.Tree.Stratum_CN == stratum.Stratum_CN &&
                                                                ut.Tree.CuttingUnit_CN == cu.CuttingUnit_CN &&
                                                                ut.Tree.SampleGroup.Code == speciesGroups[k].SampleGroup &&
                                                                ut.Tree.SampleGroup.PrimaryProduct != "01" &&
                                                                ut.Tree.SampleGroup.CutLeave == "C" && ut.Tree.STM == "Y";
                                                        });
                                                }
                                                else if (whatProduct == "BOTH")
                                                {
                                                    currentTrees = unitTrees.FindAll(
                                                        delegate(TreeCalculatedValuesDO ut)
                                                        {
                                                            return ut.Tree.Stratum_CN == stratum.Stratum_CN &&
                                                                ut.Tree.CuttingUnit_CN == cu.CuttingUnit_CN &&
                                                                ut.Tree.Species == ag.Species &&
                                                                ut.Tree.SampleGroup.CutLeave == "C" && ut.Tree.STM == "Y";
                                                        });
                                                }   //  endif
                                            }   //  endif

                                            if(currentTrees.Count > 0)
                                                LoadUnitData(currentTrees, cu, tableColumn);
                                        }   //  end foreach loop
                                    }   //  endif
                                    
                                }   //  end if
                            }   //  end for j loop
                        }   //  end foreach loop on cutting units
                    }   //  endif report not 25 or 26
                    tableColumn++;
                }   //  end for k loop
                numOlines = 0;
                if (endGroup == speciesGroups.Count) lastPage = true;
                LoadColumnHeader(speciesGroups, lastPage);
                writeCurrentPage(strWriteOut, rh, ref pageNumb, endGroup, lastPage);
                clearOutputList(reportData);
            }   //  end for n loop on number of pages

            return;
        }   //  end CreateUnitStandTables


        private void LoadUnitData(List<LCDDO> currentGroup, CuttingUnitDO currentUnit, int whichColumn,
                                    string currST)
        {
            double currTrees = 0;
            //  which row?
            int nthRow = FindUnitIndex(currentUnit.Code);
            //  what method for number of trees
            string currMeth = Utilities.MethodLookup(currST, bslyr);

            //  sum up group
            foreach (LCDDO cg in currentGroup)
            {
                //  Proration factor
                int proRow = GetProrationFactor(cg, currentUnit.Code);
                if (proRow >= 0)
                {
                    unitPF = proList[proRow].ProrationFactor;
                    if (currMeth == "3P" || currMeth == "S3P")
                        currTrees = cg.TalliedTrees;
                    //currTrees = proList[proRow].TalliedTrees;
                    else currTrees = cg.SumExpanFactor;
                }   //  endif

                switch (whatProduct)
                {
                    case "SAW":
                        if (cg.PrimaryProduct == "01" || cg.PrimaryProduct == "08")
                        {
                            switch (whatValue)
                            {
                                case "GBDFT":
                                    LoadProperColumn(cg.SumGBDFT * unitPF, whichColumn, nthRow);
                                    break;
                                case "NBDFT":
                                    LoadProperColumn(cg.SumNBDFT * unitPF, whichColumn, nthRow);
                                    break;
                                case "GCUFT":
                                    LoadProperColumn(cg.SumGCUFT * unitPF, whichColumn, nthRow);
                                    break;
                                case "NCUFT":
                                    LoadProperColumn(cg.SumNCUFT * unitPF, whichColumn, nthRow);
                                    break;
                                case "CORDS":
                                    LoadProperColumn(cg.SumCords * unitPF, whichColumn, nthRow);
                                    break;
                                case "TREES":
                                    LoadProperColumn(currTrees * unitPF, whichColumn, nthRow);
                                    break;
                            }   //  end switch on value
                        }   //  endif product 01
                        break;
                    case "NSAW":
                        if (cg.PrimaryProduct != "01")
                        {
                            switch (whatValue)
                            {
                                case "GCUFT":
                                    LoadProperColumn(cg.SumGCUFT * unitPF, whichColumn, nthRow);
                                    break;
                                case "NCUFT":
                                    LoadProperColumn(cg.SumNCUFT * unitPF, whichColumn, nthRow);
                                    break;
                                case "CORDS":
                                    LoadProperColumn(cg.SumCords * unitPF, whichColumn, nthRow);
                                    break;
                            }   //  end switch on value
                        }   //  endif not product 01
                        //  add in topwood regardless of primary product
                        switch (whatValue)
                        {
                            case "GCUFT":
                                LoadProperColumn(cg.SumGCUFTtop * unitPF, whichColumn, nthRow);
                                break;
                            case "NCUFT":
                                LoadProperColumn(cg.SumNCUFTtop * unitPF, whichColumn, nthRow);
                                break;
                            case "CORDS":
                                LoadProperColumn(cg.SumCordsTop * unitPF, whichColumn, nthRow);
                                break;
                        }   //  end switch on value
                        break;
                    case "BOTH":
                        switch (whatValue)
                        {
                            case "TREES":
                                //  load appropriate values for select cruise methods
                                LoadProperColumn(currTrees * unitPF, whichColumn, nthRow);
                                break;
                            case "NCUFT":
                                //  probably going to be used for UC25/UC26
                                LoadProperColumn(cg.SumNCUFT * unitPF, whichColumn, nthRow);
                                break;
                        }   //  end switch on value
                        break;
                }   //  end switch on product
            }   //  end foreach loop on currentGroup
            return;
        }   //  end LoadUnitData


        private void LoadUnitData(List<TreeCalculatedValuesDO> currentTrees, CuttingUnitDO currentUnit, int whichColumn)
        {
            //  overloaded for processing 100% units
            //  no proration factor used but tree volume is expanded
            int nthRow = FindUnitIndex(currentUnit.Code);

            //  sum tree volume into appropriate column
            foreach (TreeCalculatedValuesDO ct in currentTrees)
            {
                switch (whatProduct)
                {
                    case "SAW":
                        if (ct.Tree.SampleGroup.PrimaryProduct == "01")
                        {
                            switch (whatValue)
                            {
                                case "GBDFT":
                                    LoadProperColumn(ct.GrossBDFTPP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                    break;
                                case "NBDFT":
                                    LoadProperColumn(ct.NetBDFTPP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                    break;
                                case "GCUFT":
                                    LoadProperColumn(ct.GrossCUFTPP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                    break;
                                case "NCUFT":
                                    LoadProperColumn(ct.NetCUFTPP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                    break;
                                case "CORDS":
                                    LoadProperColumn(ct.CordsPP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                    break;
                                case "TREES":
                                    LoadProperColumn(ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                    break;
                            }   //  end switch
                        }   //  endif on primary product
                        break;
                    case "NSAW":
                        if (ct.Tree.SampleGroup.PrimaryProduct != "01")
                        {
                            switch (whatValue)
                            {
                                case "GCUFT":
                                    LoadProperColumn(ct.GrossCUFTPP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                    break;
                                case "NCUFT":
                                    LoadProperColumn(ct.NetCUFTPP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                    break;
                                case "CORDS":
                                    LoadProperColumn(ct.CordsPP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                    break;
                            }   //  end switch
                        }   //  endif on primary product
                        //  add topwood regardless of primary product value
                        switch (whatValue)
                        {
                            case "GCUFT":
                                LoadProperColumn(ct.GrossCUFTSP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                break;
                            case "NCUFT":
                                LoadProperColumn(ct.NetCUFTSP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                break;
                            case "CORDS":
                                LoadProperColumn(ct.CordsSP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                break;
                        }   //  end switch
                        break;
                    case "BOTH":
                        switch (whatValue)
                        {
                            case "TREES":
                                LoadProperColumn(ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                break;
                            case "NCUFT":
                                LoadProperColumn(ct.NetCUFTPP * ct.Tree.ExpansionFactor, whichColumn, nthRow);
                                break;
                        }   //  end switch
                        break;
                }   //  end switch on product
            }   //  end foreach loop

            return;
        }   //  end LoadUnitData


        private void LoadProperColumn(double valuToLoad, int nthColumn, int whichRow)
        {
            switch (nthColumn)
            {
                case 0:
                    reportData[whichRow].species1 += valuToLoad;
                    break;
                case 1:
                    reportData[whichRow].species2 += valuToLoad;
                    break;
                case 2:
                    reportData[whichRow].species3 += valuToLoad;
                    break;
                case 3:
                    reportData[whichRow].species4 += valuToLoad;
                    break;
                case 4:
                    reportData[whichRow].species5 += valuToLoad;
                    break;
                case 5:
                    reportData[whichRow].species6 += valuToLoad;
                    break;
                case 6:
                    reportData[whichRow].species7 += valuToLoad;
                    break;
                case 7:
                    reportData[whichRow].species8 += valuToLoad;
                    break;
                case 8:
                    reportData[whichRow].species9 += valuToLoad;
                    break;
                case 9:
                    reportData[whichRow].species10 += valuToLoad;
                    break;
            }//  end switch
            //  add to line total colum
            reportData[whichRow].lineTotal += valuToLoad;
            return;
        }   //  end LoadProperColumn


        private void LoadColumnHeader(List<LCDDO> speciesGroups, bool lastPage)
        {
            //  just a single line for the headers
            string verticalBar = " |   ";
            if (GroupedBy == "Species")
            {
                columnHeader[0] = "  SPEC--- |   ";
                for (int k = begGroup; k < endGroup; k++)
                {
                    columnHeader[0] += speciesGroups[k].Species.PadRight(6, ' ');
                    columnHeader[0] += verticalBar;
                }   //  end for k loop
            }
            else if (GroupedBy == "SampleGroup")
            {
                columnHeader[0] = "SMP GRP---|   ";
                for (int k = begGroup; k < endGroup; k++)
                {
                    columnHeader[0] += speciesGroups[k].SampleGroup.PadRight(6, ' ');
                    columnHeader[0] += verticalBar;
                }   //  end for k loop
            }   //  endif
            //  add total column on last page
            if (lastPage)
                columnHeader[0] += "TOTALS";
            return;
        }   //  end LoadColumnHeader


        private void writeCurrentPage(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, 
                                        int lastGroup, bool lastPage)
        {
            string verticalBar = " | ";
            prtFields.Clear();
            //  total columns for this page
            columnTotals[0] = reportData.Sum(r => r.species1);
            columnTotals[1] = reportData.Sum(r => r.species2);
            columnTotals[2] = reportData.Sum(r => r.species3);
            columnTotals[3] = reportData.Sum(r => r.species4);
            columnTotals[4] = reportData.Sum(r => r.species5);
            columnTotals[5] = reportData.Sum(r => r.species6);
            columnTotals[6] = reportData.Sum(r => r.species7);
            columnTotals[7] = reportData.Sum(r => r.species8);
            columnTotals[8] = reportData.Sum(r => r.species9);
            columnTotals[9] = reportData.Sum(r => r.species10);
            //  need to adjust lastGroup to account for multiple pages
            switch (numPages)
            {
                case 2:
                    if (lastPage) lastGroup = lastGroup - 10;
                    break;
                case 3:
                    if (lastPage) lastGroup = lastGroup - 20;
                    break;
                case 4:
                    if (lastPage) lastGroup = lastGroup - 30;
                    break;
            }   //  end switch on number of pages

            foreach (StandTables rd in reportData)
            {
                prtFields.Clear();
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    columnHeader, 10, ref pageNumb, "");
                prtFields.Add("  ");
                prtFields.Add(rd.dibClass.PadLeft(5, ' '));
                prtFields.Add("  ");
                prtFields.Add(verticalBar);
                for (int k = 0; k < lastGroup; k++)
                {
                    switch (k)
                    {
                        case 0:
                            prtFields.Add(Utilities.FormatField(rd.species1, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalBar);
                            break;
                        case 1:
                            prtFields.Add(Utilities.FormatField(rd.species2, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalBar);
                            break;
                        case 2:
                            prtFields.Add(Utilities.FormatField(rd.species3, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalBar);
                            break;
                        case 3:
                            prtFields.Add(Utilities.FormatField(rd.species4, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalBar);
                            break;
                        case 4:
                            prtFields.Add(Utilities.FormatField(rd.species5, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalBar);
                            break;
                        case 5:
                            prtFields.Add(Utilities.FormatField(rd.species6, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalBar);
                            break;
                        case 6:
                            prtFields.Add(Utilities.FormatField(rd.species7, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalBar);
                            break;
                        case 7:
                            prtFields.Add(Utilities.FormatField(rd.species8, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalBar);
                            break;
                        case 8:
                            prtFields.Add(Utilities.FormatField(rd.species9, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalBar);
                            break;
                        case 9:
                            prtFields.Add(Utilities.FormatField(rd.species10, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalBar);
                            break;
                    }   //  end switch on k
                }   // end for k loop
                //  output line total column if this is the last page
                if (lastPage)
                    prtFields.Add(Utilities.FormatField(rd.lineTotal, "{0,9:F0}").ToString().PadLeft(9, ' '));

                printOneRecord(strWriteOut, prtFields);
                //  clear print fields for next line
                prtFields.Clear();
            }   //  end foreach loop

            //  Load and print total line
            prtFields.Clear();
            prtFields.Add("  TOTALS  | ");
            for (int k = 0; k < lastGroup; k++)
            {
                prtFields.Add(Utilities.FormatField(columnTotals[k],"{0,8:F0}").ToString().PadLeft(8,' '));
                prtFields.Add(verticalBar);
            }   //  end for k loop
            if(lastPage)
            {
                // sum up line totals
                columnTotals[10] = reportData.Sum(r => r.lineTotal);
                prtFields.Add(Utilities.FormatField(columnTotals[10],"{0,9:F0}").ToString().PadLeft(9,' '));
            }   //  endif lastPage
            printOneRecord(strWriteOut,prtFields);
            return;
        }   //  end writeCurrentGroup

/*
        private List<LCDDO> GetCurrentGroup(List<LCDDO> allLCD, LCDDO js, string currST)
        {
            List<LCDDO> groupToReturn = new List<LCDDO>();
            switch (GroupedBy)
            {
                case "Species":
                    groupToReturn = allLCD.FindAll(
                        delegate(LCDDO l)
                        {
                            return l.Stratum == currST && l.Species == js.Species && l.STM == js.STM;
                        });
                    break;
                case "SampleGroup":
                    groupToReturn = allLCD.FindAll(
                        delegate(LCDDO l)
                        {
                            return l.Stratum == currST && l.SampleGroup == js.SampleGroup && l.STM == js.STM;
                        });
                    break;
                case "ContractSpecies":
                    groupToReturn = allLCD.FindAll(
                        delegate(LCDDO l)
                        {
                            return l.Stratum == currST && l.ContractSpecies == js.ContractSpecies && l.STM == js.STM;
                        });
                    break;
            }   //  end switch
            return groupToReturn;
        }   //  end GetCurrentGroup
*/

        private string unitReportTitle()
        {
            //  build report title
            string completeTitle = currentReport;
            switch (currentReport)
            {
                case "UC7":     case "UC16":
                    completeTitle += ":  GROSS BDFT VOLUME ";
                    completeTitle += reportConstants.FSTO;
                    break;
                case "UC8":     case "UC17":
                    completeTitle += ":  NET BDFT VOLUME";
                    completeTitle += reportConstants.FSTO;
                    break;
                case "UC9":     case "UC18":
                    completeTitle += ":  GROSS CUFT VOLUME";
                    completeTitle += reportConstants.FSTO;
                    break;
                case "UC10":    case "UC19":
                    completeTitle += ":  NET CUFT VOLUME";
                    completeTitle += reportConstants.FSTO;
                    break;
                case "UC11":    case "UC20":
                    completeTitle += ":  EST. NUMBER OF TREES";
                    completeTitle += reportConstants.FSTO;
                    break;
                case "UC12":    case "UC21":
                    completeTitle += ":  GROSS CUFT VOLUME";
                    completeTitle += reportConstants.FNSTO;
                    break;
                case "UC13":    case "UC22":
                    completeTitle += ":  NET CUFT VOLUME";
                    completeTitle += reportConstants.FNSTO;
                    break;
                case "UC14":    case "UC23":
                    completeTitle += ":  CORD WOOD VOLUME";
                    completeTitle += reportConstants.FNSTO;
                    break;
                case "UC15":    case "UC24":
                    completeTitle += ":  TOTAL ESTIMATED NUMBER OF TREES";
                    break;
                case "UC25":
                    completeTitle += ":  VOLUME BY CONTRACT SPECIES & PRODUCT";
                    break;
                case "UC26":
                    completeTitle += ":  VOLUME SUMMARY BY CONTRACT SPECIES & PRODUCT";
                    break;
            }   //  end switch
            return completeTitle;
        }   //  end unitReportTitle


        private int GetProrationFactor(LCDDO currGrp, string currCU)
        {
            int jthRow = -1;
            jthRow = proList.FindIndex(
               delegate(PRODO p)
               {
                   return p.CutLeave == "C" && p.Stratum == currGrp.Stratum && p.CuttingUnit == currCU &&
                           p.SampleGroup == currGrp.SampleGroup && p.PrimaryProduct == currGrp.PrimaryProduct &&
                           p.SecondaryProduct == currGrp.SecondaryProduct &&
                           p.STM == currGrp.STM;
               });
            return jthRow;
        }   //  end GetProrationFactor

        
        private int FindUnitIndex(string currCU)
        {
            int nthRow = reportData.FindIndex(
                delegate(StandTables rd)
                {
                    return rd.dibClass == currCU;
                });
            return nthRow;
        }   //  end FindUnitIndex

        
        private void VolumeSummary(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, 
                            List<LCDDO> speciesGroups, List<LCDDO> lcdList, List<PRODO> proList,
                            List<CuttingUnitDO> cList, string secondLine, string currentTitle)
        {
            //  Primarily for UC25/UC26
            double totalAcres = 0;
            double puAcres = 0;
            List<ReportSubtotal> csSummary = new List<ReportSubtotal>();
            //  finish header
            rh.createReportTitle(currentTitle, 6, 0, 0, secondLine, reportConstants.FCTO);
            List<CuttingUnitDO> paymentGroups = new List<CuttingUnitDO>();
            if (currentReport == "UC25")
            {
                //  need payment unit groups from the cutting unit table
                paymentGroups = bslyr.getPaymentUnits();
            }
            else if (currentReport == "UC26")
                paymentGroups = bslyr.getCuttingUnits();

            //  load species groups into subtotal summary
            foreach (LCDDO s in speciesGroups)
            {
                ReportSubtotal r = new ReportSubtotal();
                if (s.ContractSpecies == null)
                    r.Value1 = " ";
                else r.Value1 = s.ContractSpecies;
                csSummary.Add(r);
            }   //  end foreach loop

            //  need to process separately for each report as they report different groups
            if(currentReport == "UC25")
            {
                //  process by payment unit as there could be multiple contract species in the unit
                foreach (CuttingUnitDO pg in paymentGroups)
                {
                    //  what's the acres for the current payment group?
                    List<CuttingUnitDO> justGroup = cList.FindAll(
                        delegate(CuttingUnitDO c)
                        {
                            return c.PaymentUnit == pg.PaymentUnit;
                        });
                    puAcres = justGroup.Sum(j => j.Area);
                    totalAcres += puAcres;


                    int firstFlag = 1;
                    foreach (LCDDO sg in speciesGroups)
                    {
                        List<LCDDO> stmGroup = lcdList.FindAll(
                            delegate(LCDDO ll)
                            {
                                return ll.ContractSpecies == sg.ContractSpecies &&
                                    ll.STM == "Y";
                            });
                        if (stmGroup.Count > 0)
                        {
                            foreach (CuttingUnitDO jg in justGroup)
                            {
                                jg.Strata.Populate();
                                foreach (StratumDO stratum in jg.Strata)
                                {
                                    if (stratum.Method != "100")
                                    {
                                        List<TreeCalculatedValuesDO> justUnitTrees = bslyr.getTreeCalculatedValues((int)stratum.Stratum_CN, (int)jg.CuttingUnit_CN);
                                        List<TreeCalculatedValuesDO> stmTrees = justUnitTrees.FindAll(
                                            delegate(TreeCalculatedValuesDO ut)
                                            {
                                                return ut.Tree.CuttingUnit_CN == jg.CuttingUnit_CN &&
                                                    ut.Tree.TreeDefaultValue.ContractSpecies == sg.ContractSpecies &&
                                                    ut.Tree.STM == "Y";
                                            });
                                        SumVolume(stmTrees);
                                    }   //  endif strata
                                }   //  end for looop on strata
                            }   //  end foreach cutting unit
                        }   //  endif
                        List<LCDDO> nonSTMgroup = lcdList.FindAll(
                            delegate(LCDDO LL)
                            {
                                return LL.ContractSpecies == sg.ContractSpecies &&
                                    LL.STM == "N";
                            });

                        foreach (CuttingUnitDO jg in justGroup)
                        {
                            jg.Strata.Populate();
                            foreach (StratumDO stratum in jg.Strata)
                            {
                                if (stratum.Method == "100")
                                {
                                    List<TreeCalculatedValuesDO> justUnitTrees = bslyr.getTreeCalculatedValues((int)stratum.Stratum_CN, (int)jg.CuttingUnit_CN);
                                    SumVolume(justUnitTrees);
                                }
                                else
                                {
                                    List<LCDDO> currentGroup = nonSTMgroup.FindAll(
                                        delegate(LCDDO ll)
                                        {
                                            return ll.Stratum == stratum.Code;
                                        });
                                    SumVolume(currentGroup, jg.Code);
                                }
                            }   //  end for j loop on stratum
                        }   //  end foreach cutting unit
                        // output contract species group
                        writeContractSpeciesGroup(strWriteOut, rh, ref pageNumb, firstFlag, sawVolume,
                                                nonSawVolume, sg.ContractSpecies, pg.PaymentUnit, puAcres);
                        //  update contract species summary
                        UpdateContractSpeciesSummary(csSummary, sg.ContractSpecies, sawVolume, nonSawVolume);
                        
                        sawVolume = 0;
                        nonSawVolume = 0;
                        firstFlag = 0;
                    }   //  end foreach species group
                    //  output payment unit subtotal
                    writePaymentGroup(strWriteOut, rh, ref pageNumb, pg.PaymentUnit, puAcres, 
                                    puSawTotal, puNonSawTotal);
                    puSawTotal = 0;
                    puNonSawTotal = 0;
                    firstFlag = 1;
                }   //  end foreach loop on cutting unit
            }
            else if(currentReport == "UC26")
            {
                foreach (CuttingUnitDO pg in paymentGroups)
                {
                    //  acres is accumulated just as above but just for cutting units and then totaled
                    List<CuttingUnitDO> justUnits = cList.FindAll(
                        delegate(CuttingUnitDO cu)
                        {
                            return cu.Code == pg.Code;
                        });
                    puAcres = justUnits.Sum(j=>j.Area);
                    totalAcres += puAcres;

                    int firstFlag = 1;
                    string currCS = "";
                    foreach (LCDDO sg in speciesGroups)
                    {
                        //  need to capture all species in LCD to ensure STM is picked up June 2017
                        List<LCDDO> stmGroup = lcdList.FindAll(
                            delegate(LCDDO ll)
                            {
                                return ll.ContractSpecies == sg.ContractSpecies &&
                                    ll.STM == "Y";
                            });
                        if(stmGroup.Count > 0)
                        {
                            pg.Strata.Populate();
                            foreach (StratumDO stratum in pg.Strata)
                            {
                                if(stratum.Method != "100")
                                {
                                   List<TreeCalculatedValuesDO> justUnitTrees = bslyr.getTreeCalculatedValues((int)stratum.Stratum_CN, (int)pg.CuttingUnit_CN);
                                   List<TreeCalculatedValuesDO> stmTrees = justUnitTrees.FindAll(
                                       delegate(TreeCalculatedValuesDO ut)
                                       {
                                           return ut.Tree.CuttingUnit_CN == pg.CuttingUnit_CN &&
                                                   ut.Tree.TreeDefaultValue.ContractSpecies  == sg.ContractSpecies &&
                                                   ut.Tree.STM == "Y";
                                       });
                                       SumVolume(stmTrees);
                                }   //  endif strata
                            }   //  end for loop on strata
                        }   //  endif
                        //  now capture non STM groups
                        List<LCDDO> nonSTMgroup = lcdList.FindAll(
                            delegate(LCDDO LL)
                            {
                                return LL.ContractSpecies == sg.ContractSpecies &&
                                    LL.STM == "N";
                            });
                        
                        pg.Strata.Populate();
                        foreach (StratumDO stratum in pg.Strata)
                        {
                            if (stratum.Method == "100")
                            {
                                List<TreeCalculatedValuesDO> justUnitTrees = bslyr.getTreeCalculatedValues((int)stratum.Stratum_CN, (int)pg.CuttingUnit_CN);
                                SumVolume(justUnitTrees);
                            }
                            else
                            {
                                List<LCDDO> currentGroup = nonSTMgroup.FindAll(
                                    delegate(LCDDO ll)
                                    {
                                        return ll.Stratum == stratum.Code;
                                    });
                                SumVolume(currentGroup, pg.Code);
                            }   //  endif
                        }   //  end for loop on stratum
                        if (sg.ContractSpecies == null)
                            currCS = " ";
                        else currCS = sg.ContractSpecies;
                    //  write cutting unit group -- no subtotals printed
                    writeContractSpeciesGroup(strWriteOut, rh, ref pageNumb, firstFlag, sawVolume,
                                            nonSawVolume, currCS, pg.Code, puAcres);
                    //  update contract species summary
                    UpdateContractSpeciesSummary(csSummary, currCS, sawVolume, nonSawVolume);
                        
                    sawVolume = 0;
                    nonSawVolume = 0;
                    firstFlag = 0;
                    }   //  end foreach loop on species groups
                    strWriteOut.WriteLine("                                                   ________________________________________");
                    numOlines++;
                }   //  end foreach loop on cutting unit
            }   //  endif
            //  write contract species summary here and then grand total
            writeSummary(strWriteOut, rh, ref pageNumb, csSummary);
            writeGrandTotal(strWriteOut, rh, ref pageNumb, totalAcres, grSawTotal, grNonSawTotal);
            return;
        }   //  end VolumeSummary


        private void UpdateContractSpeciesSummary(List<ReportSubtotal> csSummary, string currCS, 
                                                    double csSawVol, double csNonSawVol)
        {
            //  find contract species in summary list
            int ithRow = csSummary.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == currCS;
                });
            if (ithRow >= 0)
            {
                csSummary[ithRow].Value3 += csSawVol;
                csSummary[ithRow].Value4 += csNonSawVol;
            }   //  endif ithRow
            return;
        }   //  end UpdateContractSpeciesSummary


        private void writeContractSpeciesGroup(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb,
                                        int firstFlag, double sawVol, double nonsawVol, string currCS,
                                        string currPU, double currAcres)
        {
            //  UC25 or UC26
            prtFields.Clear();
            if (currPU == null) currPU = " ";
            switch(currentReport)
            {
                case "UC25":
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                        rh.UC25columns, 11, ref pageNumb, "");
                    prtFields.Add("                                 ");
                    if (firstFlag == 1)
                        prtFields.Add(currPU.PadLeft(4, ' '));
                    else prtFields.Add("    ");
                    //   leave acres blank except on subtotal line
                    prtFields.Add("               ");
                    prtFields.Add(currCS.PadRight(4, ' '));
                    prtFields.Add("       ");
                    prtFields.Add(Utilities.FormatField(sawVol / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));
                    prtFields.Add("          ");
                    prtFields.Add(Utilities.FormatField(nonsawVol / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));
                    printOneRecord(strWriteOut, prtFields);
                    break;
                case "UC26":
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    rh.UC26columns, 12, ref pageNumb, "");
                    prtFields.Add("                                 ");
                    if(firstFlag == 1)
                    {
                        prtFields.Add(currPU.PadLeft(4,' '));
                        prtFields.Add("     ");
                        prtFields.Add(Utilities.FormatField(currAcres,"{0,5:F0}").ToString().PadLeft(5,' '));
                    }
                    else prtFields.Add("              ");
                    prtFields.Add("     ");
                    prtFields.Add(currCS.PadRight(4,' '));
                    prtFields.Add("       ");
                    prtFields.Add(Utilities.FormatField(sawVol/100,"{0,7:F2}").ToString().PadLeft(7,' '));
                    prtFields.Add("          ");
                    prtFields.Add(Utilities.FormatField(nonsawVol/100,"{0,7:F2}").ToString().PadLeft(7,' '));
                    printOneRecord(strWriteOut, prtFields);
                    break;
            }   //  end switch on report
            return;
        }   //  end writeContractSpeciesGroup


        private void writePaymentGroup(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, 
                                    string currPU, double currAcres, double sawVol, double NSawVol)
        {
            //  UC25 or UC26
            string groupLine = "                                                   ________________________________________";
            string[] totalText = new string[] { "PAYMENT UNIT", "TOTAL", "ALL" };
            prtFields.Clear();
            strWriteOut.WriteLine(groupLine);
            numOlines++;
            if(currentReport == "UC25")
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                rh.UC25columns, 12, ref pageNumb, "");

            prtFields.Add("             ");
            prtFields.Add(totalText[0]);
            prtFields.Add(" ");
            if (currPU != null)
                prtFields.Add(currPU.PadLeft(4, ' '));
            else prtFields.Add("    ");
            prtFields.Add(" ");
            prtFields.Add(totalText[1]);
            prtFields.Add("      ");
            prtFields.Add(Utilities.FormatField(currAcres, "{0,5:F0}").ToString().PadLeft(5, ' '));
            prtFields.Add("    ");
            prtFields.Add(totalText[2]);
            prtFields.Add("         ");
            if (currentReport == "UC26")
                prtFields.Add(Utilities.FormatField(sawVol / 100, "{0,7:F2}").ToString().PadLeft(7, ' '));
            else prtFields.Add(Utilities.FormatField(sawVol / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));
            prtFields.Add("          ");
            if (currentReport == "UC26")
                prtFields.Add(Utilities.FormatField(NSawVol / 100, "{0,7:F2}").ToString().PadLeft(7, ' '));
            else prtFields.Add(Utilities.FormatField(NSawVol / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));

            printOneRecord(strWriteOut, prtFields);
            strWriteOut.WriteLine(reportConstants.longLine);
            numOlines++;
            return;
        }   //  end writePaymentGroup


        private void writeSummary(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, 
                                    List<ReportSubtotal> csSummary)
        {
            // UC25 or UC26
            double totalNonSaw = 0;
            strWriteOut.WriteLine(reportConstants.longLine);
            numOlines++;
            strWriteOut.WriteLine("                   CONTRACT SPECIES SUMMARY");
            numOlines++;

            //  write all saw contract species and total nonsaw to print
            foreach (ReportSubtotal cs in csSummary)
            {
                prtFields.Clear();
                prtFields.Add("                                                    ");
                prtFields.Add(cs.Value1.PadRight(4, ' '));
                prtFields.Add("       ");
                if (currentReport == "UC26")
                    prtFields.Add(Utilities.FormatField(cs.Value3 / 100, "{0,7:F2}").ToString().PadLeft(7, ' '));
                else prtFields.Add(Utilities.FormatField(cs.Value3 / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));
                totalNonSaw += cs.Value4;
                printOneRecord(strWriteOut, prtFields);
            }   //  end foreach loop
            //  print nonsaw line
            prtFields.Clear();
            prtFields.Add("                                                   NS              ---          ");
            if (currentReport == "UC26")
                prtFields.Add(Utilities.FormatField(totalNonSaw / 100, "{0,7:F2}").ToString().PadLeft(7, ' '));
            else prtFields.Add(Utilities.FormatField(totalNonSaw / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));
            printOneRecord(strWriteOut, prtFields);
            return;
        }   //  end writeSummary


        private void writeGrandTotal(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, 
                                     double totAcres, double gSawVol, double gNonSawVol)
        {
            //  UC25 or uC26
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.WriteLine(reportConstants.longLine);
            prtFields.Clear();
            prtFields.Add("          GRAND TOTALS      ALL           ");
            prtFields.Add(Utilities.FormatField(totAcres, "{0,5:F0}").ToString().PadLeft(5, ' '));
            prtFields.Add("    ALL         ");
            if (currentReport == "UC26")
                prtFields.Add(Utilities.FormatField(gSawVol / 100, "{0,7:F2}").ToString().PadLeft(7, ' '));
            else prtFields.Add(Utilities.FormatField(gSawVol / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));
            prtFields.Add("          ");
            if (currentReport == "UC26")
                prtFields.Add(Utilities.FormatField(gNonSawVol / 100, "{0,7:F2}").ToString().PadLeft(7, ' '));
            else prtFields.Add(Utilities.FormatField(gNonSawVol / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));

            printOneRecord(strWriteOut, prtFields);
            return;
        }   //  end writeGrandTotal


        private void SumVolume(List<LCDDO> currGroup, string currUnit)
        {
            //  UC25 / UC26 -- non 100% strata
            //  get proration factor for each group and sum by product
            foreach (LCDDO cg in currGroup)
            {
                if (cg.CutLeave == "C")
                {
                    int nthRow = GetProrationFactor(cg, currUnit);
                    if (nthRow >= 0)
                        unitPF = proList[nthRow].ProrationFactor;
                    else unitPF = 0.0;
                    if (cg.PrimaryProduct == "01")
                    {
                        sawVolume += cg.SumNCUFT * unitPF;
                        nonSawVolume += cg.SumNCUFTtop * unitPF;
                        //  update payment unit subtotal
                        puSawTotal += cg.SumNCUFT * unitPF;
                        puNonSawTotal += cg.SumNCUFTtop * unitPF;
                        //  update grand total
                        grSawTotal += cg.SumNCUFT * unitPF;
                        grNonSawTotal += cg.SumNCUFTtop * unitPF;
                    }
                    else if (cg.PrimaryProduct != "01")
                    {
                        nonSawVolume += cg.SumNCUFT * unitPF;
                        nonSawVolume += cg.SumNCUFTtop * unitPF;
                        //  update payment unit subtotal
                        puNonSawTotal += cg.SumNCUFT * unitPF;
                        puNonSawTotal += cg.SumNCUFTtop * unitPF;
                        //  update grand total
                        grNonSawTotal += cg.SumNCUFT * unitPF;
                        grNonSawTotal += cg.SumNCUFTtop * unitPF;
                    }   //  endif
                }   //  endif cut trees only
            }   //  end foreach loop
            return;
        }   //  end SumVolume


        private void SumVolume(List<TreeCalculatedValuesDO> justUnitTrees)
        {
            //  UC25/UC26 100% method and STM trees
            //  doesn't prorate -- what's in the unit stays in the unit
            foreach (TreeCalculatedValuesDO ju in justUnitTrees)
            {
                if (ju.Tree.SampleGroup.CutLeave == "C")
                {
                    if (ju.Tree.SampleGroup.PrimaryProduct == "01")
                    {
                        sawVolume += ju.NetCUFTPP * ju.Tree.ExpansionFactor;
                        nonSawVolume += ju.NetCUFTSP * ju.Tree.ExpansionFactor;
                        //  update payment unit subtotal
                        puSawTotal += ju.NetCUFTPP * ju.Tree.ExpansionFactor;
                        puNonSawTotal += ju.NetCUFTSP * ju.Tree.ExpansionFactor;
                        //  update grand total
                        grSawTotal += ju.NetCUFTPP * ju.Tree.ExpansionFactor;
                        grNonSawTotal += ju.NetCUFTSP * ju.Tree.ExpansionFactor;
                    }
                    else if (ju.Tree.SampleGroup.PrimaryProduct != "01")
                    {
                        nonSawVolume += ju.NetCUFTPP * ju.Tree.ExpansionFactor;
                        nonSawVolume += ju.NetCUFTSP * ju.Tree.ExpansionFactor;
                        //  update payment unit subtotal
                        puNonSawTotal += ju.NetCUFTPP * ju.Tree.ExpansionFactor;
                        puNonSawTotal += ju.NetCUFTSP * ju.Tree.ExpansionFactor;
                        //  update gtrand total
                        grNonSawTotal += ju.NetCUFTPP * ju.Tree.ExpansionFactor;
                        grNonSawTotal += ju.NetCUFTSP * ju.Tree.ExpansionFactor;
                    }   //  endif
                }   //  endif cut trees only
            }   //  end foreach loop
            return;
        }   //  end SumVolume


        private void clearOutputList(List<StandTables> listToClear)
        {
            //  clears out everything except dib class
            foreach (StandTables rd in reportData)
            {
                rd.species1 = 0;
                rd.species2 = 0;
                rd.species3 = 0;
                rd.species4 = 0;
                rd.species5 = 0;
                rd.species6 = 0;
                rd.species7 = 0;
                rd.species8 = 0;
                rd.species9 = 0;
                rd.species10 = 0;
            }   //  end foreach loop
            return;
        }   //  end clearOutputList

    }   //  end OutputUnitStandTables
}
