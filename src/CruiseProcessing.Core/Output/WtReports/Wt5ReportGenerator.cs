using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.OutputModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CruiseProcessing.Output
{
    public class Wt5ReportGenerator : OutputFileReportGeneratorBase, IReportGenerator
    {
        //  WT5 report
        private readonly string[] WT5columns = new string[4] {"    STRATUM:  XX                      GREEN TONS",
                                                    "__________________________________________________________________________________________________",
                                                    "                   PRIMARY    SECONDARY   --------BIOMASS COMPONENTS------- |------STEM WGT-------",
                                                    " SPECIES       |   PRODUCT    PRODUCT        TIP     BRANCHES     FOLIAGE   |    MERCH*    TOTAL**"};

        private readonly string[] WT5footer = new string[2] {" * WHOLE TREE (ABOVE GROUND) BIOMASS AS CALCULATED USING THE TOT TREE EQN SPECIFIED IN DEFAULTS (REGIONAL).",
                                                   " ** TOTAL IS THE ADDITION OF PRIMARY PRODUCT, SECONDARY PRODUCT, TIP, BRANCHES AND FOLIAGE."};

        private readonly IReadOnlyList<int> _fieldLengths;

        private List<StratumDO> Stratum = new List<StratumDO>();

        private ILogger Log { get; }

        public Wt5ReportGenerator(CpDataLayer dataLayer, ILogger<Wt5ReportGenerator> logger)
            : base(dataLayer, "WT5")
        {
            Log = logger;

            _fieldLengths = new int[] { 1, 10, 4, 3, 11, 12, 12, 12, 11, 2, 11, 8 };
            string currentTitle = fillReportTitle(currentReport);
            SetReportTitles(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
        }

        public int GenerateReport(TextWriter strWriteOut, HeaderFieldData headerData, int startPageNum)
        {
            HeaderData = headerData;
            var pageNumb = startPageNum;
            numOlines = 0;
            Log.LogInformation("Generating WT5 report");

            Stratum = DataLayer.GetStrata();
            processSaleSummaryWT5(strWriteOut, ref pageNumb);

            Log.LogInformation("WT5 report generation complete");
            return pageNumb;
        }

        private void processSaleSummaryWT5(TextWriter strWriteOut, ref int pageNumb)
        {
            //  WT5
            List<LCDDO> lcdList = DataLayer.getLCD();
            string[] completeHeader = new string[4];

            foreach (StratumDO sd in Stratum)
            {
                double subtotPrim = 0;
                double subtotSec = 0;
                double subtotTip = 0;
                double subtotBran = 0;
                double subtotFol = 0;
                double subtotTree = 0;

                double STacres = DataLayer.GetStratumAcresCorrected(sd.Code);
                finishColumnHeaders(WT5columns, sd.Code, ref completeHeader);

                //  pull stratum and species groups from LCD
                List<LCDDO> justGroups = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2 GROUP BY ", "Species", "C", sd.Code);
                //  loop by species groups
                foreach (LCDDO jg in justGroups)
                {
                    List<LCDDO> groupData = LCDmethods.GetCutOrLeave(lcdList, "C", jg.Species, sd.Code, "");
                    double totPrim = (groupData.Sum(gd => gd.SumWgtMSP)) * STacres;
                    double totSec = (groupData.Sum(gd => gd.SumWgtMSS)) * STacres;
                    double totTip = (groupData.Sum(gd => gd.SumWgtTip)) * STacres;
                    double totBran = ((groupData.Sum(gd => gd.SumWgtBBL)) + (groupData.Sum(gd => gd.SumWgtBBD))) * STacres;
                    double totFol = (groupData.Sum(gd => gd.SumWgtBFT)) * STacres;
                    double totTree = (groupData.Sum(gd => gd.SumWgtBAT)) * STacres;

                    if (totTree > 0)
                        WriteCurrentGroupWT5(strWriteOut, ref pageNumb, jg.Species, jg.BiomassProduct.ToString(), totPrim, totSec, totTip,
                                                    totBran, totFol, totTree, completeHeader);
                    //  Update subtotal
                    subtotPrim += totPrim;
                    subtotSec += totSec;
                    subtotTip += totTip;
                    subtotBran += totBran;
                    subtotFol += totFol;
                    subtotTree += totTree;
                }   //  end foreach loop
                    //  print subtotal line here
                OutputSubtotal(strWriteOut, ref pageNumb, subtotPrim, subtotSec, subtotTip, subtotBran, subtotFol,
                                subtotTree, completeHeader);
                //  each strata prints on a separate page so reset number of lines and subtotals
                numOlines = 0;
            }   //  end foreach loop
                //  overall summary page
            OverallSummary(strWriteOut, ref pageNumb, lcdList);
            return;
        }   //  end processSaleSummary

        private void OverallSummary(TextWriter strWriteOut, ref int pageNumb, List<LCDDO> lcdList)
        {
            double subtotPrim = 0;
            double subtotSec = 0;
            double subtotTip = 0;
            double subtotBran = 0;
            double subtotFol = 0;
            double subtotTree = 0;

            //  WT5 only
            double STacres;
            //  need to finish the header
            string[] completeHeader = new string[4];
            finishColumnHeaders(WT5columns, "OVERALL SALE SUMMARY", ref completeHeader);
            //  Order LCD by species and biomass product
            List<LCDDO> justGroups = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 GROUP BY ", "Species,BiomassProduct", "C", "");
            foreach (LCDDO jg in justGroups)
            {
                double totPrim = 0;
                double totSec = 0;
                double totTip = 0;
                double totBran = 0;
                double totFol = 0;
                double totTree = 0;

                List<LCDDO> groupData = LCDmethods.GetCutOrLeave(lcdList, "C", jg.Species, "", "");
                //  Sum up weights times strata acres
                foreach (LCDDO gd in groupData)
                {
                    //  find proper strata acres
                    long currStrCN = StratumMethods.GetStratumCN(gd.Stratum, Stratum);
                    STacres = Utilities.ReturnCorrectAcres(gd.Stratum, DataLayer, currStrCN);
                    totPrim += gd.SumWgtMSP * STacres;
                    totSec += gd.SumWgtMSS * STacres;
                    totTip += gd.SumWgtTip * STacres;
                    totBran += (gd.SumWgtBBL + gd.SumWgtBBD) * STacres;
                    totFol += gd.SumWgtBFT * STacres;
                    totTree += gd.SumWgtBAT * STacres;
                }   //  end foreach loop on data
                //  Write current group
                if (totTree > 0)
                    WriteCurrentGroupWT5(strWriteOut, ref pageNumb, jg.Species, jg.BiomassProduct.ToString(), totPrim, totSec,
                                            totTip, totBran, totFol, totTree, completeHeader);
                //  update totals
                subtotPrim += totPrim;
                subtotSec += totSec;
                subtotTip += totTip;
                subtotBran += totBran;
                subtotFol += totFol;
                subtotTree += totTree;
            }   //  end foreach group

            //  output subtotals
            OutputSubtotal(strWriteOut, ref pageNumb, subtotPrim, subtotSec, subtotTip, subtotBran, subtotFol, subtotTree,
                                    completeHeader);
        }   //  end OverallSummary

        private void WriteCurrentGroupWT5(TextWriter strWriteOut, ref int pageNumb, string currSP, string currBP,
                                 double MSprim, double MSsecd, double bioTip, double bioBran, double bioFol,
                                 double bioTot, string[] completeHeader)
        {
            //  WT5 only
            WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2], completeHeader, 4, ref pageNumb, "");
            var prtFields = new List<string>();
            prtFields.Add("");
            prtFields.Add(currSP.PadRight(6, ' '));
            //  removed product code as it was confusing to users.  may 2016
            //prtFields.Add(currBP.PadLeft(2, '0'));
            prtFields.Add("  ");
            prtFields.Add("|");
            prtFields.Add(String.Format("{0,8:F1}", MSprim / 2000).PadLeft(8, ' '));
            prtFields.Add(String.Format("{0,8:F1}", MSsecd / 2000).PadLeft(8, ' '));
            prtFields.Add(String.Format("{0,8:F1}", bioTip / 2000).PadLeft(8, ' '));
            prtFields.Add(String.Format("{0,8:F1}", bioBran / 2000).PadLeft(8, ' '));
            prtFields.Add(String.Format("{0,8:F1}", bioFol / 2000).PadLeft(8, ' '));
            prtFields.Add("|");
            prtFields.Add(String.Format("{0,8:F1}", bioTot / 2000).PadLeft(8, ' '));
            double overallTotal = MSprim + MSsecd + bioTip + bioBran + bioFol;
            prtFields.Add(String.Format("{0,8:F1}", overallTotal / 2000).PadLeft(8, ' '));
            printOneRecord(_fieldLengths, prtFields, strWriteOut);
        }

        private void OutputSubtotal(TextWriter strWriteOut, ref int pageNumb, double subValue1,
                            double subValue2, double subValue3, double subValue4,
                            double subValue5, double subValue6, string[] completeHeader)
        {
            //  WT5
            WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                completeHeader, 4, ref pageNumb, "");
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.Write(" ALL SPECIES   |  ");
            strWriteOut.Write(String.Format("{0,8:F1}", subValue1 / 2000).PadLeft(8, ' '));
            strWriteOut.Write("   ");
            strWriteOut.Write(String.Format("{0,8:F1}", subValue2 / 2000).PadLeft(8, ' '));
            strWriteOut.Write("    ");
            strWriteOut.Write(String.Format("{0,8:F1}", subValue3 / 2000).PadLeft(8, ' '));
            strWriteOut.Write("    ");
            strWriteOut.Write(String.Format("{0,8:F1}", subValue4 / 2000).PadLeft(8, ' '));
            strWriteOut.Write("    ");
            strWriteOut.Write(String.Format("{0,8:F1}", subValue5 / 2000).PadLeft(8, ' '));
            strWriteOut.Write("   | ");
            strWriteOut.Write(String.Format("{0,8:F1}", subValue6 / 2000).PadLeft(8, ' '));
            strWriteOut.Write("   ");
            double overallTotal = subValue1 + subValue2 + subValue3 + subValue4 + subValue5;
            strWriteOut.WriteLine(String.Format("{0,8:F1}", overallTotal / 2000).PadLeft(8, ' '));
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine(WT5footer[0]);
            strWriteOut.WriteLine(WT5footer[1]);

            return;
        }   //  end OutputSubtotal

        private void finishColumnHeaders(string[] columnsToUpdate, string currST, ref string[] completeHeader)
        {
            completeHeader[0] = columnsToUpdate[0].Replace("XX", currST);
            for (int k = 1; k < 4; k++)
                completeHeader[k] = columnsToUpdate[k];
        }
    }
}