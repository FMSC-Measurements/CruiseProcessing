using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseProcessing.Output;
using System.Collections.ObjectModel;

namespace CruiseProcessing
{
    public class OutputStemCounts : ReportGeneratorBase
    {
        private List<string> prtFields = new List<string>();
        private string[] completeHeader = new string[3];
        private List<StandTables> countsToOutput = new List<StandTables>();
        private readonly string oneHeader = "CUTTING UNIT:  ";
        private readonly IReadOnlyList<string> twoHeader = new[] { "CUTTING UNIT:  ", "STRATUM:  ", "NUMBER OF PLOTS --" };
        private string threeHeader = "STRATUM:  ";
        private int footFlag = 0;

        public OutputStemCounts(CPbusinessLayer dataLayer, HeaderFieldData headerData, string reportID) : base(dataLayer, headerData, reportID)
        {
        }

        public void createStemCountReports(StreamWriter strWriteOut, ref int pageNumb)
        {
            //  fill report title array
            string currentTitle = fillReportTitle(currentReport);

            //  pull just fixcnt methods from stratum table
            List<StratumDO> justFIXCNT = DataLayer.justFIXCNTstrata();
            //  any stratum matches?
            if (justFIXCNT.Count == 0)
            {
                noDataForReport(strWriteOut, currentReport, " cannot produce report.  No FIXCNT stratum in the cruise.");
                return;
            }   //  endif no data

            //  otherwise, loop by stratum to get data
            foreach (StratumDO jf in justFIXCNT)
            {
                //  pull species for the stratum
                List<LCDDO> justSpecies = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2 ",
                                                        "GROUP BY Species", "C", jf.Code, "");                
                //  pull current stratum from tree
                List<TreeDO> justTrees = DataLayer.JustFIXCNTtrees((long)jf.Stratum_CN);

                //  process by stratum and report
                string secondLine = reportConstants.FPPO;
                secondLine += " -- ";
                secondLine += reportConstants.B1DC;
                SetReportTitles(currentTitle, 6, 0, 0, secondLine, reportConstants.oneInchDC);
                switch (currentReport)
                {
                    case "SC1":         case "SC2":
                        createByUnit(jf, justSpecies, justTrees, strWriteOut, ref pageNumb);
                        break;
                    case "SC3":
                        createByStratum(jf, justSpecies, justTrees, strWriteOut, ref pageNumb);
                        break;
                }   //  end switch on report
                numOlines = 0;

            }   //  end foreach loop
            return;
        }   //  end createStemCountReports


        private void createByUnit(StratumDO currST, List<LCDDO> justSpecies, List<TreeDO> justTrees,
                                StreamWriter strWriteOut, ref int pageNumb)
        {
            int numOplots = 0;
            //  SC1 and SC2 reports by units
            //  cutting unit acres used in SC2 to expand unit per acre value
            currST.CuttingUnits.Populate();
            List<PlotDO> pList = DataLayer.getPlots();
            foreach (CuttingUnitDO cu in currST.CuttingUnits)
            {
                //  need number of plots for SC2 report
                List<PlotDO> justPlots = pList.FindAll(
                    delegate(PlotDO p)
                    {
                        return p.Stratum_CN == currST.Stratum_CN && p.CuttingUnit_CN == cu.CuttingUnit_CN;
                    });
                numOplots = justPlots.Count();
                //  load headers for each cutting unit
                if (currentReport == "SC1")
                    completeHeader = createCompleteHeader(justSpecies, cu.Code, currST.Code, 0);
                else if (currentReport == "SC2")
                    completeHeader = createCompleteHeader(justSpecies, cu.Code, currST.Code, numOplots);
                //  clear out stand table list for next unit
                countsToOutput.Clear();
                //  load DIB classes for this stratum and load into output list
                List<TreeDO> justDIBS = DataLayer.getTreeDBH("C", currST.Code, "C");
                double DIBsum = justDIBS.Sum(j => j.DBH);
                if (DIBsum > 0)
                    LoadTreeDIBclasses(justDIBS[justDIBS.Count - 1].DBH, countsToOutput, 3);
                else
                {
                    // no DIBs recorded so only one class to accumulate
                    StandTables s = new StandTables();
                    s.dibClass = "0";
                    countsToOutput.Add(s);
                    footFlag = 1;
                }   //  endif
                //  find all species for this cutting unit
                int nthColumn = 1;
                int nthRow = 0;
                foreach (LCDDO js in justSpecies)
                {
                    List<TreeDO> justGroup = justTrees.FindAll(
                        delegate(TreeDO t)
                        {
                            return t.CuttingUnit.Code == cu.Code && t.Species == js.Species;
                        });
                    //  then load output list -- find row first for each tree
                    if (justGroup.Count > 0)
                    {
                        foreach (TreeDO jg in justGroup)
                        {
                            //  find index in output list for DBH
                            nthRow = FindTreeDIBindex(countsToOutput, jg.DBH, 3);
                            if (nthRow >= 0)
                            {
                                if (currentReport == "SC2")
                                    loadCountsToOutput(nthRow, nthColumn, jg.ExpansionFactor, cu.Area);
                                else if (currentReport == "SC1")
                                    loadCountsToOutput(nthRow, nthColumn, jg.ExpansionFactor, 1);
                            }
                        }   //  end foreach loop
                        nthColumn++;
                    }
                }   //  end foreach loop on species groups

                // output current cutting unit
                double calcValue = countsToOutput.Sum(c => c.lineTotal);
                if (calcValue > 0)
                {
                    WriteCurrentPage(strWriteOut, ref pageNumb, justSpecies);
                    numOlines = 0;
                }   //  endif
            }   //  end foreach loop on cutting unit
            return;
        }   //  end createByUnit


        private void createByStratum(StratumDO currST, List<LCDDO> justSpecies, List<TreeDO> justTrees,
                                StreamWriter strWriteOut, ref int pageNumb)
        {
            //  SC3 reports by stratum
                completeHeader = createCompleteHeader(justSpecies, "", currST.Code, 0);
                //  clear out stand table list for next unit
                countsToOutput.Clear();
                //  load DIB classes for this stratum and load into output list
                List<TreeDO> justDIBS = DataLayer.getTreeDBH("C", currST.Code, "C");
                double DIBsum = justDIBS.Sum(j => j.DBH);
                if (DIBsum > 0)
                    LoadTreeDIBclasses(justDIBS[justDIBS.Count - 1].DBH, countsToOutput, 3);
                else
                {
                    // no DIBs recorded so only one class to accumulate
                    StandTables s = new StandTables();
                    s.dibClass = "0";
                    countsToOutput.Add(s);
                    footFlag = 1;
                }   //  endif
                //  find all species for this cutting unit
                int nthColumn = 1;
                int nthRow = 0;
                foreach (LCDDO js in justSpecies)
                {
                    List<TreeDO> justGroup = justTrees.FindAll(
                        delegate(TreeDO t)
                        {
                            return t.Species == js.Species;
                        });
                    //  then load output list -- find row first for each tree
                    if (justGroup.Count > 0)
                    {
                        foreach (TreeDO jg in justGroup)
                        {
                            //  find index in output list for DBH
                            nthRow = FindTreeDIBindex(countsToOutput, jg.DBH, 3);
                            if (nthRow >= 0)
                                loadCountsToOutput(nthRow, nthColumn, jg.ExpansionFactor, 1);
                        }   //  end foreach loop
                        nthColumn++;
                    }
                }   //  end foreach loop on species groups

                // output current cutting unit
                double calcValue = countsToOutput.Sum(c => c.lineTotal);
                if (calcValue > 0)
                {
                    WriteCurrentPage(strWriteOut, ref pageNumb, justSpecies);
                    numOlines = 0;
                }   //  endif
            return;
        }   //  end createByStratum


        private void loadCountsToOutput(int currRow, int currCol, double currEF, double currAC)
        {
            switch (currCol)
            {
                case 1:
                    countsToOutput[currRow].species1 += currEF * currAC;
                    break;
                case 2:
                    countsToOutput[currRow].species2 += currEF * currAC;
                    break;
                case 3:
                    countsToOutput[currRow].species3 += currEF * currAC;
                    break;
                case 4:
                    countsToOutput[currRow].species4 += currEF * currAC;
                    break;
                case 5:
                    countsToOutput[currRow].species5 += currEF * currAC;
                    break;
                case 6:
                    countsToOutput[currRow].species6 += currEF * currAC;
                    break;
                case 7:
                    countsToOutput[currRow].species7 += currEF * currAC;
                    break;
                case 8:
                    countsToOutput[currRow].species8 += currEF * currAC;
                    break;
                case 9:
                    countsToOutput[currRow].species9 += currEF * currAC;
                    break;
                case 10:
                    countsToOutput[currRow].species10 += currEF * currAC;
                    break;
            }   //  end switch
            countsToOutput[currRow].lineTotal += currEF * currAC;
            return;
        }   //  end loadCountsToOutput


        private void WriteCurrentPage(StreamWriter strWriteOut, ref int pageNumb, List<LCDDO> justSpecies)
        {
            //  should work for every report
            string verticalLine = " |";
            foreach (StandTables cto in countsToOutput)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                    completeHeader, 11, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add(cto.dibClass.PadLeft(4, ' '));
                prtFields.Add("   ");
                prtFields.Add(verticalLine);
                for (int k = 0; k < justSpecies.Count; k++)
                {
                    switch (k)
                    {
                        case 0:
                            prtFields.Add(String.Format("{0,9:F1}", cto.species1).PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 1:
                            prtFields.Add(String.Format("{0,9:F1}", cto.species2).PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 2:
                            prtFields.Add(String.Format("{0,9:F1}", cto.species3).PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 3:
                            prtFields.Add(String.Format("{0,9:F1}", cto.species4).PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 4:
                            prtFields.Add(String.Format("{0,9:F1}", cto.species5).PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 5:
                            prtFields.Add(String.Format("{0,9:F1}", cto.species6).PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 6:
                            prtFields.Add(String.Format("{0,9:F1}", cto.species7).PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 7:
                            prtFields.Add(String.Format("{0,9:F1}", cto.species8).PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 8:
                            prtFields.Add(String.Format("{0,9:F1}", cto.species9).PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 9:
                            prtFields.Add(String.Format("{0,9:F1}", cto.species10).PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                    }   //  end switch
                }   //  end foreach loop on just species
                //  load line total
                prtFields.Add(String.Format("{0,9:F1}", cto.lineTotal).PadLeft(9, ' '));
                printOneRecord(strWriteOut, prtFields);
            }   //  end foreach loop
            // output total line
            outputTotalLine(strWriteOut, ref pageNumb, justSpecies);

            //  output footer line if needed
            if (footFlag == 1)
            {
                strWriteOut.WriteLine("");
                strWriteOut.WriteLine("  NO DBH ENTRIES IN DATA SO NO DIB CLASSES AVAILABLE.");
            }   //  endif
            return;
        }   //  end WriteCurrentPage


        private void outputTotalLine(StreamWriter strWriteOut, ref int pageNumb, List<LCDDO> justSpecies)
        {
            //  output headers if needed
            string verticalLine = " |";
            double calcValue = 0;
            WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                completeHeader, 11, ref pageNumb, "");
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.Write(" TOTALS |");
            for (int k = 0; k < justSpecies.Count; k++)
            {
                switch (k)
                {
                    case 0:
                        calcValue = countsToOutput.Sum(c => c.species1);
                        break;
                    case 1:
                        calcValue = countsToOutput.Sum(c => c.species2);
                        break;
                    case 2:
                        calcValue = countsToOutput.Sum(c => c.species3);
                        break;
                    case 3:
                        calcValue = countsToOutput.Sum(c => c.species4);
                        break;
                    case 4:
                        calcValue = countsToOutput.Sum(c => c.species5);
                        break;
                    case 5:
                        calcValue = countsToOutput.Sum(c => c.species6);
                        break;
                    case 6:
                        calcValue = countsToOutput.Sum(c => c.species7);
                        break;
                    case 7:
                        calcValue = countsToOutput.Sum(c => c.species8);
                        break;
                    case 8:
                        calcValue = countsToOutput.Sum(c => c.species9);
                        break;
                    case 9:
                        calcValue = countsToOutput.Sum(c => c.species10);
                        break;
                }   //  end switch
                strWriteOut.Write(String.Format("{0,9:F1}", calcValue).PadLeft(9, ' '));
                strWriteOut.Write(verticalLine);
            }   //  end for k loop
            //  sum up total column
            calcValue = countsToOutput.Sum(c => c.lineTotal);
            strWriteOut.WriteLine(String.Format("{0,9:F1}", calcValue).PadLeft(9, ' '));
            return;
        }   //  end outputTotalLine


        private string[] createCompleteHeader(List<LCDDO> justSpecies, string currCU, string currST, int currPlots)
        {
            string[] finnishHeader = new string[3];
            StringBuilder sb = new StringBuilder();
            finnishHeader[1] = "";
            switch (currentReport)
            {
                case "SC1":
                    sb.Append(oneHeader);
                    sb.Append(currCU.PadLeft(4, ' '));
                    break;
                case "SC2":
                    sb.Append(twoHeader[0]);
                    sb.Append(currCU.PadLeft(4,' '));
                    sb.Append("     ");
                    sb.Append(twoHeader[1]);
                    sb.Append(currST.PadLeft(2, ' '));
                    sb.Append("      ");
                    sb.Append(twoHeader[2]);
                    sb.Append(currPlots.ToString().PadLeft(5, ' '));
                    break;
                case "SC3":
                    sb.Append(threeHeader);
                    sb.Append(currST.PadLeft(2, ' '));
                    break;
            }   //  end switch on report
            finnishHeader[0] = sb.ToString();
            sb.Remove(0,sb.Length);
            sb.Append(" SPECIES|  ");
            foreach (LCDDO js in justSpecies)
            {
                sb.Append(js.Species.PadRight(6,' '));
                sb.Append("  |  ");
            }   //  end foreach loop on just species
            sb.Append("TOTALS");
            finnishHeader[2] = sb.ToString();
            return finnishHeader;
        }   //  end createCompleteHeader
    }
}
