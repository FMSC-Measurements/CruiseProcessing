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
    public class OutputStemCounts : CreateTextFile
    {
        #region
        public string currentReport;
        private ArrayList prtFields = new ArrayList();
        private string[] completeHeader = new string[3];
        private List<StandTables> countsToOutput = new List<StandTables>();
        private string oneHeader = "CUTTING UNIT:  ";
        private string[] twoHeader = new string[] { "CUTTING UNIT:  ", "STRATUM:  ", "NUMBER OF PLOTS --" };
        private string threeHeader = "STRATUM:  ";
        private int footFlag = 0;
        #endregion

        public void createStemCountReports(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  fill report title array
            string currentTitle = fillReportTitle(currentReport);

            //  pull just fixcnt methods from stratum table
            //  any stratum matches?
            if (!Global.BL.justFIXCNTstrata().Any())
            {
                noDataForReport(strWriteOut, currentReport, " cannot produce report.  No FIXCNT strtaum in the cruise.");
                return;
            }   //  endif no data

            //  otherwise, loop by stratum to get data
            foreach (StratumDO jf in Global.BL.justFIXCNTstrata())
            {
                //  pull species for the stratum
                IEnumerable<LCDDO> justSpecies = Global.BL.getLCDOrdered("WHERE CutLeave = ? AND Stratum = ? ",
                                                        "GROUP BY Species", "C", jf.Code, "");                
                //  pull current stratum from tree
                IEnumerable<TreeDO> justTrees = Global.BL.JustFIXCNTtrees((long)jf.Stratum_CN);

                //  process by stratum and report
                string secondLine = reportConstants.FPPO;
                secondLine += " -- ";
                secondLine += reportConstants.B1DC;
                rh.createReportTitle(currentTitle, 6, 0, 0, secondLine, reportConstants.oneInchDC);
                switch (currentReport)
                {
                    case "SC1":         case "SC2":
                        createByUnit(jf, justSpecies, justTrees, strWriteOut, ref pageNumb, rh);
                        break;
                    case "SC3":
                        createByStratum(jf, justSpecies, justTrees, strWriteOut, ref pageNumb, rh);
                        break;
                }   //  end switch on report
                numOlines = 0;

            }   //  end foreach loop
            return;
        }   //  end createStemCountReports


        private void createByUnit(StratumDO currST, IEnumerable<LCDDO> justSpecies, IEnumerable<TreeDO> justTrees,
                                StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            int numOplots = 0;
            //  SC1 and SC2 reports by units
            //  cutting unit acres used in SC2 to expand unit per acre value
            currST.CuttingUnits.Populate();
            IEnumerable<PlotDO> pList = Global.BL.getPlots();
            foreach (CuttingUnitDO cu in currST.CuttingUnits)
            {
                //  need number of plots for SC2 report
                numOplots = pList.Where(
                    p => p.Stratum_CN == currST.Stratum_CN && p.CuttingUnit_CN == cu.CuttingUnit_CN).Count();
                //  load headers for each cutting unit
                if (currentReport == "SC1")
                    completeHeader = createCompleteHeader(justSpecies, cu.Code, currST.Code, 0);
                else if (currentReport == "SC2")
                    completeHeader = createCompleteHeader(justSpecies, cu.Code, currST.Code, numOplots);
                //  clear out stand table list for next unit
                countsToOutput.Clear();
                //  load DIB classes for this stratum and load into output list
                List<TreeDO> justDIBS = Global.BL.getTreeDBH("C", currST.Code, "C").ToList();
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
                    IEnumerable<TreeDO> justGroup = justTrees.Where(t => t.CuttingUnit.Code == cu.Code && t.Species == js.Species);
                    //  then load output list -- find row first for each tree
                    if (justGroup.Any())
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
                    WriteCurrentPage(strWriteOut, ref pageNumb, rh, justSpecies);
                    numOlines = 0;
                }   //  endif
            }   //  end foreach loop on cutting unit
            return;
        }   //  end createByUnit


        private void createByStratum(StratumDO currST, IEnumerable<LCDDO> justSpecies, IEnumerable<TreeDO> justTrees,
                                StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  SC3 reports by stratum
                completeHeader = createCompleteHeader(justSpecies, "", currST.Code, 0);
                //  clear out stand table list for next unit
                countsToOutput.Clear();
                //  load DIB classes for this stratum and load into output list
                List<TreeDO> justDIBS = Global.BL.getTreeDBH("C", currST.Code, "C").ToList();
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
                    //  then load output list -- find row first for each tree
                    if (justTrees.Where(t => t.Species == js.Species).Any())
                    {
                        foreach (TreeDO jg in justTrees.Where(t => t.Species == js.Species))
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
                    WriteCurrentPage(strWriteOut, ref pageNumb, rh, justSpecies);
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


        private void WriteCurrentPage(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh, 
                                        IEnumerable<LCDDO> justSpecies)
        {
            //  should work for every report
            string verticalLine = " |";
            foreach (StandTables cto in countsToOutput)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                    completeHeader, 11, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add(cto.dibClass.PadLeft(4, ' '));
                prtFields.Add("   ");
                prtFields.Add(verticalLine);
                for (int k = 0; k < justSpecies.Count(); k++)
                {
                    switch (k)
                    {
                        case 0:
                            prtFields.Add(Utilities.FormatField(cto.species1, "{0,9:F1}").ToString().PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 1:
                            prtFields.Add(Utilities.FormatField(cto.species2, "{0,9:F1}").ToString().PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 2:
                            prtFields.Add(Utilities.FormatField(cto.species3, "{0,9:F1}").ToString().PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 3:
                            prtFields.Add(Utilities.FormatField(cto.species4, "{0,9:F1}").ToString().PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 4:
                            prtFields.Add(Utilities.FormatField(cto.species5, "{0,9:F1}").ToString().PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 5:
                            prtFields.Add(Utilities.FormatField(cto.species6, "{0,9:F1}").ToString().PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 6:
                            prtFields.Add(Utilities.FormatField(cto.species7, "{0,9:F1}").ToString().PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 7:
                            prtFields.Add(Utilities.FormatField(cto.species8, "{0,9:F1}").ToString().PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 8:
                            prtFields.Add(Utilities.FormatField(cto.species9, "{0,9:F1}").ToString().PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 9:
                            prtFields.Add(Utilities.FormatField(cto.species10, "{0,9:F1}").ToString().PadLeft(9, ' '));
                            prtFields.Add(verticalLine);
                            break;
                    }   //  end switch
                }   //  end foreach loop on just species
                //  load line total
                prtFields.Add(Utilities.FormatField(cto.lineTotal, "{0,9:F1}").ToString().PadLeft(9, ' '));
                printOneRecord(strWriteOut, prtFields);
            }   //  end foreach loop
            // output total line
            outputTotalLine(strWriteOut, ref pageNumb, rh, justSpecies);

            //  output footer line if needed
            if (footFlag == 1)
            {
                strWriteOut.WriteLine("");
                strWriteOut.WriteLine("  NO DBH ENTRIES IN DATA SO NO DIB CLASSES AVAILABLE.");
            }   //  endif
            return;
        }   //  end WriteCurrentPage


        private void outputTotalLine(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh, 
                                        IEnumerable<LCDDO> justSpecies)
        {
            //  output headers if needed
            string verticalLine = " |";
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                completeHeader, 11, ref pageNumb, "");
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.Write(" TOTALS |");
            for (int k = 0; k < justSpecies.Count(); k++)
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
                strWriteOut.Write(Utilities.FormatField(calcValue, "{0,9:F1}").ToString().PadLeft(9, ' '));
                strWriteOut.Write(verticalLine);
            }   //  end for k loop
            //  sum up total column
            calcValue = countsToOutput.Sum(c => c.lineTotal);
            strWriteOut.WriteLine(Utilities.FormatField(calcValue, "{0,9:F1}").ToString().PadLeft(9, ' '));
            return;
        }   //  end outputTotalLine


        private string[] createCompleteHeader(IEnumerable<LCDDO> justSpecies, string currCU, string currST, int currPlots)
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
