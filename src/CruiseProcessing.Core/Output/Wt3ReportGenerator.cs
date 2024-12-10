using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.OutputModels;
using System.Collections.Generic;
using System.IO;

namespace CruiseProcessing.Output
{
    public class Wt3ReportGenerator : Wt2Wt3ReportGeneratorBase, IReportGenerator
    {
        //private List<StratumDO> Stratum = new List<StratumDO>();
        private List<CuttingUnitDO> cList = new List<CuttingUnitDO>();

        public Wt3ReportGenerator(CpDataLayer dataLayer, IVolumeLibrary volLib, HeaderFieldData headerData)
            : base(dataLayer, volLib, headerData, "WT3")
        {
            string currentTitle = fillReportTitle(currentReport);
            SetReportTitles(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
        }

        public int GenerateReport(TextWriter strWriteOut, int startPageNum)
        {
            var pageNumb = startPageNum;
            numOlines = 0;
            //Stratum = DataLayer.GetStrata();
            cList = DataLayer.getCuttingUnits();
            processSlashLoad(strWriteOut, ref pageNumb);

            return pageNumb;
        }

        private void processSlashLoad(TextWriter strWriteOut, ref int pageNumb)
        {
            //  First, need region, forest and district
            string currReg = DataLayer.getRegion();
            string currFor = DataLayer.getForest();
            string currDist = DataLayer.getDistrict();
            double currAcres = 0;
            string currST;

            numOlines = 0;
            List<BiomassData> bList = new List<BiomassData>();

            cList = DataLayer.getCuttingUnits();
            var tdvList = DataLayer.getTreeDefaults();
            foreach (CuttingUnitDO c in cList)
            {
                c.Strata.Populate();
                foreach (StratumDO stratum in c.Strata)
                {
                    //  pull trees for the current stratum
                    List<TreeCalculatedValuesDO> treeList = DataLayer.getTreeCalculatedValues((int)stratum.Stratum_CN);
                    //  Pull stratum and species groups from LCD
                    List<LCDDO> justGroups = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2 ",
                                                    "GROUP BY Species", "C", stratum.Code, "");
                    //  Load this groups into the Biomass Data list
                    currAcres = DataLayer.GetStratumAcresCorrected(stratum.Code);
                    foreach (LCDDO jg in justGroups)
                    {
                        int nthRow = bList.FindIndex(
                            delegate (BiomassData bd)
                            {
                                return bd.SpeciesCode == jg.Species;
                            });
                        //  February 2018 -- also need the FIA code from default values
                        //  for current species
                        int ithRow = tdvList.FindIndex(
                            delegate (TreeDefaultValueDO t)
                            {
                                return t.Species == jg.Species;
                            });
                        if (nthRow < 0)
                        {
                            BiomassData b = new BiomassData();
                            b.StratumCode = jg.Stratum;
                            b.SampleGroupCode = jg.SampleGroup;
                            b.SpeciesCode = jg.Species;
                            if (ithRow >= 0)
                                b.SpeciesFIA = (int)tdvList[ithRow].FIAcode;
                            bList.Add(b);
                        }   //  endif
                    }   //  end foreach loop
                        //  caluclate each stratum for the WT3 report
                    foreach (LCDDO jg in justGroups)
                    {
                        //  pull trees for the group
                        List<TreeCalculatedValuesDO> justTrees = treeList.FindAll(
                            delegate (TreeCalculatedValuesDO tc)
                            {
                                return tc.Tree.SampleGroup.CutLeave == "C" &&
                                    tc.Tree.Stratum.Code == jg.Stratum &&
                                    tc.Tree.Species == jg.Species &&
                                    tc.Tree.SampleGroup.Code == jg.SampleGroup &&
                                    tc.Tree.CountOrMeasure == "M" &&
                                    tc.Tree.CuttingUnit.Code == c.Code;
                            });
                        //  Store stratum values
                        CalculateComponentValues(justTrees, currAcres, jg, bList, currReg);
                    }   //  end foreach loop on groups
                }   //  end for j loop on stratum
                    //  print cutting unit page here
                List<BiomassData> unitList = SumUpUnitList(strWriteOut, ref pageNumb, bList, c.Code,
                                                        c.Area);//  if any one bioSpecies in the bList is zero, skip reporet
                foreach (BiomassData bl in bList)
                {
                    if (bl.SpeciesFIA == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>Missing some FIA codes.  Cannot produce report.");
                        return;
                    }   //  endif
                }   //  end foreach
                WriteCurrentGroup(strWriteOut, ref pageNumb, "", unitList, c.Code);
                //  clear biomass data list for next cutting unit
                bList.Clear();
                numOlines = 0;
            }   //  end foreach loop cutting unit
        }   //  end processSlashLoad

        private List<BiomassData> SumUpUnitList(TextWriter strWriteOut, ref int pageNumb, List<BiomassData> bList,
                                        string currCU, float unitAcres)
        {
            //  WT3 report
            //  Sum up strata values for current unit
            string currST = "**";
            double currProFac = 0;
            double currSTacres = 1;
            string currMeth = "";
            int jthRow;
            List<StratumDO> sList = DataLayer.GetStrata();
            List<PRODO> proList = DataLayer.getPRO();
            List<BiomassData> unitList = new List<BiomassData>();

            foreach (BiomassData b in bList)
            {
                if (currST != b.StratumCode)
                {
                    //  need stratum acres, method, and proration factor
                    jthRow = sList.FindIndex(
                        delegate (StratumDO s)
                        {
                            return s.Code == b.StratumCode;
                        });
                    currSTacres = Utilities.AcresLookup((long)sList[jthRow].Stratum_CN, DataLayer, b.StratumCode);
                    currMeth = sList[jthRow].Method;
                    switch (currMeth)
                    {
                        case "3P":
                        case "STR":
                        case "S3P":
                            jthRow = proList.FindIndex(
                            delegate (PRODO p)
                            {
                                return p.CutLeave == "C" && p.Stratum == b.StratumCode &&
                                    p.CuttingUnit == currCU && p.SampleGroup == b.SampleGroupCode;
                            });
                            if (jthRow >= 0)
                                currProFac = proList[jthRow].ProrationFactor;
                            break;

                        default:
                            currProFac = 1.0;
                            break;
                    }   //  end switch on method for proration factor
                    currST = b.StratumCode;
                }   //  endif

                //  stratum must match so check for species group in new list and add if not there
                //  or update the row found
                int ithRow = unitList.FindIndex(
                    delegate (BiomassData ul)
                    {
                        return ul.StratumCode == b.StratumCode && ul.SampleGroupCode == b.SampleGroupCode &&
                                ul.SpeciesCode == b.SpeciesCode;
                    });
                if (ithRow < 0)
                {
                    //  Add to list
                    BiomassData bd = new BiomassData();
                    bd.StratumCode = b.StratumCode;
                    bd.SampleGroupCode = b.SampleGroupCode;
                    bd.SpeciesCode = b.SpeciesCode;
                    unitList.Add(bd);
                    ithRow = unitList.Count - 1;
                }   //  endif ithRow

                //  update values
                switch (currMeth)
                {
                    case "3P":
                    case "STR":
                    case "S3P":
                        unitList[ithRow].Needles += (b.Needles * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].QuarterInch += (b.QuarterInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].OneInch += (b.OneInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].ThreeInch += (b.ThreeInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].ThreePlus += (b.ThreePlus * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].TopwoodDryWeight += (b.TopwoodDryWeight * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].CullLogWgt += (b.CullLogWgt * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].CullChunkWgt += (b.CullChunkWgt * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].DSTneedles += (b.DSTneedles * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].DSTquarterInch += (b.DSTquarterInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].DSToneInch += (b.DSToneInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].DSTthreeInch += (b.DSTthreeInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].DSTthreePlus += (b.ThreePlus * currSTacres * currProFac) / unitAcres;
                        break;

                    default:
                        unitList[ithRow].Needles += b.Needles * currProFac;
                        unitList[ithRow].QuarterInch += b.QuarterInch * currProFac;
                        unitList[ithRow].OneInch += b.OneInch * currProFac;
                        unitList[ithRow].ThreeInch += b.ThreeInch * currProFac;
                        unitList[ithRow].ThreePlus += b.ThreePlus * currProFac;
                        unitList[ithRow].TopwoodDryWeight += b.TopwoodDryWeight * currProFac;
                        unitList[ithRow].CullLogWgt += b.CullLogWgt * currProFac;
                        unitList[ithRow].CullChunkWgt += b.CullChunkWgt * currProFac;
                        unitList[ithRow].DSTneedles += b.DSTneedles * currProFac;
                        unitList[ithRow].DSTquarterInch += b.DSTquarterInch * currProFac;
                        unitList[ithRow].DSToneInch += b.DSToneInch * currProFac;
                        unitList[ithRow].DSTthreeInch += b.DSTthreeInch * currProFac;
                        unitList[ithRow].DSTthreePlus += b.ThreePlus * currProFac;
                        break;
                }   //  end switch on method
            }   //  end foreach loop

            return unitList;
        }   //  end SumUpUnitList
    }
}