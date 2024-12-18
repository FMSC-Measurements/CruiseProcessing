using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.OutputModels;
using FMSC.ORM.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

namespace CruiseProcessing.Output
{
    public class Wt2ReportGenerator : Wt2Wt3ReportGeneratorBase, IReportGenerator
    {
        private List<StratumDO> Stratum = new List<StratumDO>();

        public Wt2ReportGenerator(CpDataLayer dataLayer, IVolumeLibrary volLib, ILogger<Wt2ReportGenerator> logger)
            : base(dataLayer, volLib, "WT2", logger)
        {
            string currentTitle = fillReportTitle(currentReport);
            SetReportTitles(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
        }

        public int GenerateReport(TextWriter strWriteOut, HeaderFieldData headerData, int startPageNum)
        {
            HeaderData = headerData;
            var pageNumb = startPageNum;
            numOlines = 0;
            Stratum = DataLayer.GetStrata();
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

            List<TreeDefaultValueDO> tdvList = DataLayer.getTreeDefaults();
            //  Load summary groups for last page
            List<BiomassData> summaryList = new List<BiomassData>();
            LoadSummaryGroups(summaryList, tdvList);
            //  process by stratum since each page is a separate stratum or cutting unit
            foreach (StratumDO s in Stratum)
            {
                List<BiomassData> bList = new List<BiomassData>();
                currST = s.Code;
                currAcres = DataLayer.GetStratumAcresCorrected(currST);
                //  pull all trees for the current stratum as well as current acres and the method
                List<TreeCalculatedValuesDO> treeList = DataLayer.getTreeCalculatedValues((int)s.Stratum_CN);

                //  Pull stratum and species groups from LCD
                List<LCDDO> justGroups = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2 ",
                                                        "GROUP BY Species", "C", s.Code, "");
                //  Load these groups into a BiomassData list
                foreach (LCDDO jg in justGroups)
                {
                    int ithRow = tdvList.FindIndex(
                        delegate (TreeDefaultValueDO t)
                        {
                            return t.Species == jg.Species;
                        });
                    BiomassData b = new BiomassData();
                    b.StratumCode = jg.Stratum;
                    b.SampleGroupCode = jg.SampleGroup;
                    b.SpeciesCode = jg.Species;
                    b.SpeciesFIA = (int)tdvList[ithRow].FIAcode;
                    bList.Add(b);
                }   //  end foreach loop

                //  Calculate and print each stratum for a WT2 report
                foreach (LCDDO jg in justGroups)
                {
                    //  pull all trees for the stratum
                    List<TreeCalculatedValuesDO> tcvList = treeList.FindAll(
                        delegate (TreeCalculatedValuesDO tcv)
                        {
                            return tcv.Tree.SampleGroup.CutLeave == "C" &&
                                    tcv.Tree.Stratum.Code == jg.Stratum &&
                                    tcv.Tree.Species == jg.Species &&
                                    tcv.Tree.SampleGroup.Code == jg.SampleGroup &&
                                    tcv.Tree.CountOrMeasure == "M";
                        });
                    //  Calculate and store stratum values
                    CalculateComponentValues(tcvList, currAcres, jg, bList, currReg);
                }   //  end foreach loop
                    //  if any one bioSpecies in the bList is zero, skip reporet
                foreach (BiomassData bl in bList)
                {
                    if (bl.SpeciesFIA == 0)
                    {
                        strWriteOut.Write("Missing some FIA codes.\nCannot produce report  .");
                        strWriteOut.WriteLine(currentReport);
                        return;
                    }   //  endif
                }   //  end foreach
                    //  write page for current stratum or cutting unit
                WriteCurrentGroup(strWriteOut, ref pageNumb, currST, bList, "");
                //  update summary list for last page
                AcumilateWt2BioDataTotals(summaryList, bList);

                numOlines = 0;
            }   //  end foreach loop on stratum or cutting unit
                //  output summary page
            WriteCurrentGroup(strWriteOut, ref pageNumb, "SALE", summaryList, "");
        }

        private void LoadSummaryGroups(List<BiomassData> summaryList, List<TreeDefaultValueDO> tdList)
        {
            //  Pull species groups from LCD to load into summary list
            List<LCDDO> summaryGroups = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 ", "GROUP BY Species", "C", "");
            foreach (LCDDO sg in summaryGroups)
            {
                //  find FIA code
                int nthRow = tdList.FindIndex(
                    delegate (TreeDefaultValueDO t)
                    {
                        return t.Species == sg.Species;
                    });
                BiomassData b = new BiomassData();
                b.StratumCode = sg.Stratum;
                b.SampleGroupCode = sg.SampleGroup;
                b.SpeciesCode = sg.Species;
                b.SpeciesFIA = (int)tdList[nthRow].FIAcode;
                summaryList.Add(b);
            }
        }

        private void AcumilateWt2BioDataTotals(List<BiomassData> summaryList, List<BiomassData> bList)
        {
            //  update summary list with current stratum/cutting unit list
            foreach (BiomassData b in bList)
            {
                //  find each group in the summary list to update
                int ithRow = summaryList.FindIndex(
                    delegate (BiomassData sl)
                    {
                        return sl.SpeciesCode == b.SpeciesCode && sl.SampleGroupCode == b.SampleGroupCode &&
                            sl.SpeciesFIA == b.SpeciesFIA;
                    });
                if (ithRow >= 0)
                {
                    summaryList[ithRow].Needles += b.Needles;
                    summaryList[ithRow].QuarterInch += b.QuarterInch;
                    summaryList[ithRow].OneInch += b.OneInch;
                    summaryList[ithRow].ThreeInch += b.ThreeInch;
                    summaryList[ithRow].ThreePlus += b.ThreePlus;
                    summaryList[ithRow].TopwoodDryWeight += b.TopwoodDryWeight;
                    summaryList[ithRow].CullLogWgt += b.CullLogWgt;
                    summaryList[ithRow].CullChunkWgt += b.CullChunkWgt;
                    summaryList[ithRow].DSTneedles += b.DSTneedles;
                    summaryList[ithRow].DSTquarterInch += b.DSTquarterInch;
                    summaryList[ithRow].DSToneInch += b.DSToneInch;
                    summaryList[ithRow].DSTthreeInch += b.DSTthreeInch;
                    summaryList[ithRow].DSTthreePlus += b.DSTthreePlus;
                }   //  endif ithRow
            }   //  end foreach loop
        }   //  end UpdateSummaryList
    }
}