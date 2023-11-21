using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CruiseProcessing
{
    public class OutputR10 : CreateTextFile
    {
        public string currentReport;
        private int[] fieldLengths;
        private List<string> prtFields = new List<string>();
        private List<RegionalReports> listToOutput = new List<RegionalReports>();
        private List<ReportSubtotal> totalToOutput = new List<ReportSubtotal>();
        private regionalReportHeaders rRH = new regionalReportHeaders();
        private double currGRS = 0;
        private double currNET = 0;
        private double currREM = 0;

        //private double currUTIL = 0;
        private double convFactor = 100.0;

        private int footFlag;
        private string[] completeHeader;

        public OutputR10(CPbusinessLayer dataLayer) : base(dataLayer)
        {
        }

        public void CreateR10reports(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  Fill report title array
            string currentTitle = fillReportTitle(currentReport);

            //  grab LCD list
            List<LCDDO> lcdList = DataLayer.getLCD();
            //  Any data for current report?
            switch (currentReport)
            {
                case "R001":
                case "R003":
                case "R006":
                    currGRS = lcdList.Sum(l => l.SumGBDFT);
                    if (currGRS == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No board foot volume for report");
                        return;
                    }   //  endif
                    break;

                case "R002":
                case "R004":
                case "R007":
                case "R005":
                    currGRS = lcdList.Sum(l => l.SumGCUFT);
                    if (currGRS == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, " >>>> No cubic foot volume for report");
                        return;
                    }   //  endif
                    break;
            }   //  end switch

            //  process reports
            List<StratumDO> sList = DataLayer.getStratum();
            List<LogStockDO> logList = DataLayer.getCutLogs();
            //  group LCD by species as many reports run by species
            List<LCDDO> speciesList = DataLayer.getLCDOrdered("WHERE CutLeave = @p1", "GROUP BY Species", "C", "");
            switch (currentReport)
            {
                case "R001":
                case "R002":
                    numOlines = 0;
                    fieldLengths = new int[] { 1, 8, 10, 13, 10, 11, 11, 9 };
                    completeHeader = createCompleteHeader();
                    rh.createReportTitle(currentTitle, 6, 0, 0, "", "");
                    if (currentReport == "R001") convFactor = 1000.0;
                    int currRow = 0;
                    foreach (LCDDO sl in speciesList)
                    {
                        //  Load volume into regional reports list
                        RegionalReports r = new RegionalReports();
                        r.value1 = sl.Species;
                        listToOutput.Add(r);
                        AccumulateVolume(logList, sl.Species, currRow, sList);
                        currRow++;
                    }   //  end foreach
                    WriteCurrentGroup(strWriteOut, ref pageNumb, rh, completeHeader);
                    updateTotal();
                    outputTotal(strWriteOut);
                    outputLogSummary(strWriteOut, lcdList, sList);
                    break;

                case "R003":
                case "R004":
                    numOlines = 0;
                    fieldLengths = new int[] { 1, 8, 5, 11, 17, 17, 17, 17, 17, 8 };
                    completeHeader = createCompleteHeader(speciesList);
                    string updatedLine = reportConstants.SVSG;
                    if (currentReport == "R003")
                        updatedLine = updatedLine.Replace("XXX", "MBF");
                    else if (currentReport == "R004")
                        updatedLine = updatedLine.Replace("XXX", "CCF");
                    rh.createReportTitle(currentTitle, 6, 0, 0, updatedLine, "");
                    if (currentReport == "R003") convFactor = 1000.0;
                    AccumulateByLogGrade(logList, speciesList, sList);
                    //  need to total each species before printing so net percent can be calculated
                    //  at print time
                    updateGradeTotals();
                    WriteCurrentGroup(strWriteOut, ref pageNumb, rh, completeHeader);
                    outputTotal(strWriteOut);
                    break;

                case "R005":
                    //  June 2017 -- R005 is by logging method and if blank report cannot be generated.
                    //  Check for any logging method of blank or null
                    List<CuttingUnitDO> cutList = DataLayer.getCuttingUnits();
                    int noMethod = 0;
                    foreach (CuttingUnitDO ct in cutList)
                    {
                        if (ct.LoggingMethod == "" || ct.LoggingMethod == " " || ct.LoggingMethod == null)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> One or more logging methods are blank.  Cannot create this report.");
                            noMethod = -1;
                        }   //  endif
                    }   //  end foreach loop
                    if (noMethod != -1)
                    {
                        numOlines = 0;
                        fieldLengths = new int[] { 1, 4, 7, 3, 7, 6, 9, 8, 9, 10, 10, 8, 8, 8, 9, 9, 8, 5 };
                        completeHeader = createCompleteHeader();
                        rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FCTO_PPO, "--  CUBIC FOOT  --");
                        AccumulateMostlyLCD(lcdList, speciesList, sList, strWriteOut, ref pageNumb, rh);
                    }   //  endif
                    break;

                case "R006":
                case "R007":
                    OutputFileBySpeciesDiameter();
                    break;

                case "R008":
                case "R009":
                    OutputLogMatrixFile();
                    break;
            }   //  end switch
            return;
        }   //  end CreateR10reports

        private void AccumulateVolume(List<LogStockDO> logList, string currSP,
                                        int currRow, List<StratumDO> sList)
        {
            //  accumulate volume based on report
            footFlag = (int)logList[0].Tree.TreeDefaultValue.MerchHeightLogLength;
            string currST = "*";
            double currAC = 0;

            //  Pull current species from log list
            List<LogStockDO> justLogs = logList.FindAll(
                delegate (LogStockDO l)
                {
                    return l.Tree.Species == currSP;
                });
            foreach (LogStockDO jl in justLogs)
            {
                //  need strata acres
                if (currST != jl.Tree.Stratum.Code)
                {
                    currST = jl.Tree.Stratum.Code;
                    currAC = Utilities.ReturnCorrectAcres(currST, DataLayer, (long)jl.Tree.Stratum_CN);
                }   //  endif

                switch (currentReport)
                {
                    case "R001":
                    case "R003":
                        currGRS = jl.GrossBoardFoot;
                        currNET = jl.NetBoardFoot;
                        currREM = jl.BoardFootRemoved;
                        //currUTIL = jl.BoardUtil;
                        break;

                    case "R002":
                    case "R004":
                        currGRS = jl.GrossCubicFoot;
                        currNET = jl.NetCubicFoot;
                        currREM = jl.CubicFootRemoved;
                        //currUTIL = jl.CubicUtil;
                        break;
                }   //  end switch

                //  sum up volume for each position in the regional reports list
                //  accumulate next values by log grade
                switch (jl.Grade)
                {
                    case "0":
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                        //  Gross volume
                        listToOutput[currRow].value7 += currGRS * jl.Tree.ExpansionFactor * currAC;
                        //  Gross removed volume no utility
                        listToOutput[currRow].value8 += currREM * jl.Tree.ExpansionFactor * currAC;
                        //  Net volume with utility
                        listToOutput[currRow].value9 += currNET * jl.Tree.ExpansionFactor * currAC;
                        if (jl.Length == 32)
                            listToOutput[currRow].value10 += jl.Tree.ExpansionFactor * currAC;
                        //  now summing utility volume on these grades
                        //if (jl.PercentRecoverable > 0)
                        //{
                        //  total defects
                        //  double totalDefect = jl.Tree.TreeDefaultValue.CullPrimary + jl.Tree.TreeDefaultValue.HiddenPrimary + jl.SeenDefect;
                        //double comboRec = jl.Tree.RecoverablePrimary + jl.PercentRecoverable;
                        //if (comboRec <= totalDefect)
                        // {
                        //listToOutput[currRow].value11 += (currGRS * jl.Tree.ExpansionFactor * currAC) * jl.PercentRecoverable / 100 + 0.0499;
                        //listToOutput[currRow].value11 += currUTIL * jl.Tree.ExpansionFactor * currAC;
                        //}   //  endif
                        //}   //  endif
                        break;
                        //case "7":
                        //  No longer have grade 7 -- January 2017
                        //  Per C.Anderson in R10, include grade 7 with recovered utility volume (total utility)
                        //  October 2013
                        //listToOutput[currRow].value11 += currNET * jl.Tree.ExpansionFactor * currAC;
                        //break;
                }   //  end switch on log grade
                //  total logs based on length
                //if (footFlag == 32)
                //    listToOutput[currRow].value10 += (jl.Length / 32) * jl.Tree.ExpansionFactor * currAC;
                //else listToOutput[currRow].value10 += jl.Tree.ExpansionFactor * currAC;
                //listToOutput[currRow].value10 += jl.Tree.ExpansionFactor * currAC;
            }   //  end foreach
            return;
        }   //  end AccumulateVolume

        private void AccumulateByLogGrade(List<LogStockDO> logList, List<LCDDO> speciesList, List<StratumDO> sList)
        {
            //  R003/R004
            double stratumAcres;
            string[] logGrades = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            //  each row in the regional reports list is a log grade
            //  values on the row indicate the species and the gross/net volume
            //  key for the list:
            //  value1 is a species with value7 the gross and value8 the net for that species
            //  value2 is a species -- value9 = gross; value10 = net
            //  value3 is a species -- value11 = gross; value12 = net
            //  value4 is a species -- value13 = gross; value14 = net
            //  It is rare for Region 10 to have more than 4 species but the list will accomodate one more species
            //  Changed value 5 for log grade to value21 to accomodate additional species as follows
            //  value5 is a species -- value15 = gross; value 16 = net
            //  value6 is a species -- value17 = gross; value 18 = net
            //  so grade rows are added first
            for (int j = 0; j < 10; j++)
            {
                RegionalReports r = new RegionalReports();
                r.value21 = logGrades[j];
                for (int jj = 0; jj < speciesList.Count; jj++)
                {
                    switch (jj)
                    {
                        case 0:
                            r.value1 = speciesList[jj].Species;
                            break;

                        case 1:
                            r.value2 = speciesList[jj].Species;
                            break;

                        case 2:
                            r.value3 = speciesList[jj].Species;
                            break;

                        case 3:
                            r.value4 = speciesList[jj].Species;
                            break;

                        case 4:
                            r.value5 = speciesList[jj].Species;
                            break;

                        case 5:
                            r.value6 = speciesList[jj].Species;
                            break;
                    }   //  end switch
                }   //  end for loop
                listToOutput.Add(r);
            }   //  end for loop

            //  process by species
            foreach (LCDDO sl in speciesList)
            {
                // Find all logs by species, stratum and log grade
                for (int k = 0; k < 10; k++)
                {
                    currGRS = 0;
                    currNET = 0;
                    // loop by stratum
                    foreach (StratumDO s in sList)
                    {
                        //  need acres for expansion
                        stratumAcres = Utilities.ReturnCorrectAcres(s.Code, DataLayer, (long)s.Stratum_CN);

                        List<LogStockDO> justLogs = logList.FindAll(
                            delegate (LogStockDO l)
                            {
                                return l.Tree.Species == sl.Species && l.Tree.Stratum.Code == s.Code &&
                                        l.Tree.SampleGroup.CutLeave == "C" && l.Grade == logGrades[k];
                            });
                        if (justLogs.Count > 0)
                        {
                            //  pull and sum volume based on report
                            switch (currentReport)
                            {
                                case "R003":
                                    currGRS = justLogs.Sum(j => j.GrossBoardFoot * j.Tree.ExpansionFactor) * stratumAcres;
                                    currNET = justLogs.Sum(j => j.NetBoardFoot * j.Tree.ExpansionFactor) * stratumAcres;
                                    break;

                                case "R004":
                                    currGRS = justLogs.Sum(j => j.GrossCubicFoot * j.Tree.ExpansionFactor * stratumAcres);
                                    currNET = justLogs.Sum(j => j.NetCubicFoot * j.Tree.ExpansionFactor * stratumAcres);
                                    break;
                            }   //  end switch

                            //  load into list
                            if (sl.Species == listToOutput[k].value1)
                            {
                                listToOutput[k].value7 += currGRS;
                                listToOutput[k].value8 += currNET;
                            }
                            else if (sl.Species == listToOutput[k].value2)
                            {
                                listToOutput[k].value9 += currGRS;
                                listToOutput[k].value10 += currNET;
                            }
                            else if (sl.Species == listToOutput[k].value3)
                            {
                                listToOutput[k].value11 += currGRS;
                                listToOutput[k].value12 += currNET;
                            }
                            else if (sl.Species == listToOutput[k].value4)
                            {
                                listToOutput[k].value13 += currGRS;
                                listToOutput[k].value14 += currNET;
                            }
                            else if (sl.Species == listToOutput[k].value5)
                            {
                                listToOutput[k].value15 += currGRS;
                                listToOutput[k].value16 += currNET;
                            }
                            else if (sl.Species == listToOutput[k].value6)
                            {
                                listToOutput[k].value17 += currGRS;
                                listToOutput[k].value18 += currNET;
                            }   //  endif
                        }   //  endif justLogs has records
                    }   //  end foreach loop
                }   //  end for k loop
            }   //  end foreach loop
            return;
        }   //  end AccumulateByLogGrade

        private void AccumulateMostlyLCD(List<LCDDO> lcdList, List<LCDDO> speciesList, List<StratumDO> sList,
                                          StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  R005
            double currUnitAcres = 0;
            double totalAcres = 0;
            double currLOGS = 0;
            double currHGT = 0;
            double currDBH = 0;
            double currEF = 0;
            string prevST = "*";
            double currAcres = 0;
            List<ReportSubtotal> logMethodSubtotal = new List<ReportSubtotal>();
            //  Which height field to use?
            int hgtOne = 0;
            int hgtTwo = 0;
            List<TreeDO> tList = DataLayer.getTrees();
            whichHeightFields(ref hgtOne, ref hgtTwo, tList);

            //  need PRO table
            List<PRODO> proList = DataLayer.getPRO();
            //  and cutting unit table
            List<CuttingUnitDO> unitList = DataLayer.getCuttingUnits();
            //  and just logging methods
            List<CuttingUnitDO> justMethods = DataLayer.getLoggingMethods();

            //  process by logging method
            foreach (CuttingUnitDO jm in justMethods)
            {
                //  find units for current log method
                List<CuttingUnitDO> justUnits = unitList.FindAll(
                    delegate (CuttingUnitDO c)
                    {
                        return c.LoggingMethod == jm.LoggingMethod;
                    });
                //  then process by stratum, species, and pp=01 only
                foreach (LCDDO sl in speciesList)
                {
                    currGRS = 0;
                    currNET = 0;
                    currREM = 0;
                    foreach (CuttingUnitDO ju in justUnits)
                    {
                        ju.Strata.Populate();

                        // find LCD records by stratum, species, cut trees and pp=01
                        foreach (StratumDO sd in ju.Strata)
                        {
                            //  find correct acres for current stratum
                            if (prevST != sd.Code)
                            {
                                currAcres = Utilities.ReturnCorrectAcres(sd.Code, DataLayer, (long)sd.Stratum_CN);
                                prevST = sd.Code;
                            }   //  endif

                            //  find all species, stratum and product in lcd
                            List<LCDDO> justSpecies = lcdList.FindAll(
                                delegate (LCDDO l)
                                {
                                    return l.Stratum == sd.Code && l.CutLeave == "C" &&
                                        l.Species == sl.Species && l.PrimaryProduct == "01";
                                });
                            //  find proration factor and sum up volume and other values
                            double proFactor = FindProrationFactor("C", "01", sd.Code, sl.Species, ju.Code,
                                                                    proList, tList);

                            // Gross cubic
                            currGRS += justSpecies.Sum(j => j.SumGCUFT) * proFactor;
                            //  Net cubic
                            currNET += justSpecies.Sum(j => j.SumNCUFT) * proFactor;
                            //  Removed
                            currREM += justSpecies.Sum(j => j.SumGCUFTremv) * proFactor;
                            //  Logs
                            currLOGS += justSpecies.Sum(j => j.SumLogsMS) * proFactor;
                            //  sum appropriate height
                            switch (hgtOne)
                            {
                                case 1:
                                    currHGT += justSpecies.Sum(j => j.SumTotHgt) * proFactor;
                                    break;

                                case 2:
                                    currHGT += justSpecies.Sum(j => j.SumMerchHgtPrim) * proFactor;
                                    break;

                                case 3:
                                    currHGT += justSpecies.Sum(j => j.SumMerchHgtSecond) * proFactor;
                                    break;

                                case 4:
                                    currHGT += justSpecies.Sum(j => j.SumHgtUpStem) * proFactor;
                                    break;
                            }   //  end switch on height
                            //  DBHOB
                            currDBH += justSpecies.Sum(j => j.SumDBHOB) * proFactor;
                            //  Expansion factor
                            currEF += justSpecies.Sum(j => j.SumExpanFactor) * proFactor;
                        }   //  end foreach loop on stratum
                    }   //  end foreach loop on units for logging method

                    RegionalReports rr = new RegionalReports();
                    rr.value1 = jm.LoggingMethod;
                    rr.value2 = sl.Species;
                    rr.value3 = "01";
                    rr.value7 = currGRS;
                    rr.value8 = currNET;
                    rr.value9 = currREM;
                    rr.value10 = currLOGS;
                    rr.value11 = currEF;
                    rr.value12 = currDBH;
                    rr.value13 = currHGT;
                    listToOutput.Add(rr);
                    currGRS = 0;
                    currNET = 0;
                    currREM = 0;
                    currLOGS = 0;
                    currEF = 0;
                    currDBH = 0;
                    currHGT = 0;
                }   //  end foreach loop on species list
                //  write current logging method group
                //  sum unit acres for printing
                currUnitAcres = justUnits.Sum(j => j.Area);
                WriteCurrentGroup(currUnitAcres, strWriteOut, ref pageNumb, rh);
                updateSubtotals(ref logMethodSubtotal);
                updateOverallTotal();
                totalAcres += currUnitAcres;
                outputTotalSubtotal(logMethodSubtotal, 1, strWriteOut, ref pageNumb, currUnitAcres);
                listToOutput.Clear();
                logMethodSubtotal.Clear();
            }   //  end foreach loop on logging methods list
            // output final total
            outputTotalSubtotal(totalToOutput, 2, strWriteOut, ref pageNumb, totalAcres);
            return;
        }   // end AccumulateMostlyLCD

        private void OutputFileBySpeciesDiameter()
        {
            //   R006/R007
            //  capture cruise number
            string cruiseNumb = DataLayer.getCruiseNumber();
            //  update headers for output file based on report
            string[] completeHeaders = new string[5];
            regionalReportHeaders rrh = new regionalReportHeaders();
            completeHeader = rrh.R006R007columns;
            switch (currentReport)
            {
                case "R006":
                    completeHeader[0] = completeHeader[0].Replace("TTTTT", "BOARD");
                    completeHeader[0] = completeHeader[0].Replace("ZZZ", "MBF");
                    completeHeader[0] = completeHeader[0].Replace("XXXXXX", cruiseNumb.PadLeft(6, ' '));
                    break;

                case "R007":
                    completeHeader[0] = completeHeader[0].Replace("TTTTT", "CUBIC");
                    completeHeader[0] = completeHeader[0].Replace("ZZZ", "CCF");
                    completeHeader[0] = completeHeader[0].Replace("XXXXXX", cruiseNumb.PadLeft(6, ' '));
                    break;
            }   //  end switch on currentReport

            //  create and open output file
            string outputFile = System.IO.Path.GetDirectoryName(FilePath);
            outputFile += "\\";
            outputFile += cruiseNumb;
            outputFile += "_";
            outputFile += currentReport;
            outputFile += ".txt";
            //  output heading lines
            using (StreamWriter textFile = new StreamWriter(outputFile))
            {
                foreach (object obj in completeHeader)
                    textFile.WriteLine(obj);
                textFile.WriteLine(reportConstants.longLine);

                //  pull log stock records ordered by species
                List<LogStockDO> justLogs = DataLayer.getCutLogs();
                //  also need a list of just species
                List<LCDDO> speciesList = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 ", "GROUP BY Species", "C", "");
                foreach (LCDDO sl in speciesList)
                {
                    //  sum logs for current species
                    List<LogStockDO> justSpecies = justLogs.FindAll(
                        delegate (LogStockDO ls)
                        {
                            return ls.Tree.Species == sl.Species;
                        });
                    int diamFlag = SumLogData(justSpecies, cruiseNumb, sl.Species);
                    OutputLogData(textFile);
                    if (diamFlag > 0)
                        textFile.WriteLine("  Logs with a small end diameter of less than 5.5 are NOT included in this report.");
                }   //  end foreach loop
                textFile.Close();
            }   //  end using
            return;
        }   //  end OutputFileBySpeciesDiameter

        private void OutputLogMatrixFile()
        {
            List<LogMatrix> outputMatrix = new List<LogMatrix>();
            List<LogMatrixDO> reportMatrix = DataLayer.getLogMatrix(currentReport);
            if (reportMatrix.Count > 0)
                outputMatrix = CreateLogMatrix(reportMatrix);
            else if (reportMatrix.Count == 0)
            {
                //  can't create these reports -- tell user
                MessageBox.Show("The log matrix is missing for the R008 or R009 reports.\nPlease go to Reports>Region reports>10>Additional Data to load the default log matrix.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }   //  end

            //  Accumulate volumes for each category
            AccumulateCategories(reportMatrix, outputMatrix);
            //  output file
            WriteOutputFile(outputMatrix);
            return;
        }   //  end OutputLogMatrixFile

        private void AccumulateCategories(List<LogMatrixDO> reportMatrix, List<LogMatrix> outputMatrix)
        {
            //  R008/R009
            List<LogStockDO> logList = DataLayer.getCutLogs();
            int nthRow = 0;
            string currLG = "";

            List<LogStockDO> justLogs = new List<LogStockDO>();
            foreach (LogMatrixDO rm in reportMatrix)
            {
                //  Find logs for each grade
                for (int j = 1; j < 7; j++)
                {
                    switch (j)
                    {
                        case 1:
                            currLG = rm.LogGrade1;
                            break;

                        case 2:
                            currLG = rm.LogGrade2;
                            break;

                        case 3:
                            currLG = rm.LogGrade3;
                            break;

                        case 4:
                            currLG = rm.LogGrade4;
                            break;

                        case 5:
                            currLG = rm.LogGrade5;
                            break;

                        case 6:
                            currLG = rm.LogGrade6;
                            break;
                    }   //  end switch
                    //  and max/min diameters
                    if ((rm.SEDlimit == "" || rm.SEDminimum == 0) && currLG != null)
                    {
                        justLogs = logList.FindAll(
                            delegate (LogStockDO l)
                            {
                                return l.Grade == currLG && l.Tree.Species == rm.Species.TrimEnd(' ');
                            });
                    }
                    else if ((rm.SEDlimit == null && rm.SEDminimum > 0) && currLG != null)
                    {
                        justLogs = logList.FindAll(
                            delegate (LogStockDO l)
                            {
                                return l.Grade == currLG && l.Tree.Species == rm.Species.TrimEnd(' ') &&
                                    l.SmallEndDiameter >= rm.SEDminimum;
                            });
                    }
                    else if ((rm.SEDlimit == "greater than") && currLG != null)
                    {
                        justLogs = logList.FindAll(
                            delegate (LogStockDO l)
                            {
                                return l.Grade == currLG && l.SmallEndDiameter >= rm.SEDminimum &&
                                        l.Tree.Species == rm.Species.TrimEnd(' ');
                            });
                    }
                    else if ((rm.SEDlimit == "less than") && currLG != null)
                    {
                        justLogs = logList.FindAll(
                            delegate (LogStockDO l)
                            {
                                return l.Grade == currLG && l.SmallEndDiameter <= rm.SEDminimum &&
                                        l.Tree.Species == rm.Species.TrimEnd(' ');
                            });
                    }
                    else if ((rm.SEDlimit == "between" || rm.SEDlimit == "thru") && currLG != null)
                    {
                        justLogs = logList.FindAll(
                            delegate (LogStockDO l)
                            {
                                return l.Grade == currLG && l.SmallEndDiameter >= rm.SEDminimum &&
                                            l.SmallEndDiameter <= rm.SEDmaximum &&
                                            l.Tree.Species == rm.Species.TrimEnd(' ');
                            });
                    }   //  endif
                    //  Calculate appropriate MBF and store in outputMatrix
                    if (justLogs.Count > 0)
                    {
                        CalculateVolume(outputMatrix, nthRow, "Gross", justLogs);
                        CalculateVolume(outputMatrix, nthRow, "Net", justLogs);
                        CalculateVolume(outputMatrix, nthRow, "Rem", justLogs);
                        CalculateVolume(outputMatrix, nthRow, "Util", justLogs);
                        justLogs.Clear();
                    }   //  endif
                }   //  end for j loop
                nthRow++;
            }   //  end foreach loop
        }   //  end AccumulateCategories

        private int SumLogData(List<LogStockDO> justSpecies, string currCR, string currSP)
        {
            //  load listToOutput with constants
            string[] diameterGroups = new string[] { "06 - 11", "12 - 17", "18 - 23", "24+    " };
            string[] logGrades = new string[] { "0", "1", "2", "3", "4", "5", "6", "7" };
            string volType = "B";
            string currST = "**";
            double currAC = 0;
            int diameterFlag = 0;
            if (currentReport == "R007") volType = "C";
            //  file regional reports list with constants
            for (int j = 0; j < 4; j++)
            {
                RegionalReports rr = new RegionalReports();
                rr.value1 = currCR;
                rr.value2 = currSP;
                rr.value3 = diameterGroups[j];
                listToOutput.Add(rr);
            }   //  end foreach loop
            //  pull each log grade and sum into listToOutput
            int nthRow = -1;
            for (int k = 0; k < 8; k++)
            {
                List<LogStockDO> justLogs = justSpecies.FindAll(
                    delegate (LogStockDO js)
                    {
                        return js.Grade == logGrades[k];
                    });
                foreach (LogStockDO jl in justLogs)
                {
                    //  check diameter to get proper row in listToOutput
                    if (jl.SmallEndDiameter >= 5.5 && jl.SmallEndDiameter <= 11.5)
                        nthRow = 0;
                    else if (jl.SmallEndDiameter >= 11.6 && jl.SmallEndDiameter <= 17.5)
                        nthRow = 1;
                    else if (jl.SmallEndDiameter >= 17.6 && jl.SmallEndDiameter <= 23.5)
                        nthRow = 2;
                    else if (jl.SmallEndDiameter >= 23.6)
                        nthRow = 3;
                    else if (jl.SmallEndDiameter < 5.5)
                        diameterFlag++;
                    //  get correct stratum acres for expansion but only if stratum changed
                    if (currST != jl.Tree.Stratum.Code)
                    {
                        currAC = Utilities.ReturnCorrectAcres(jl.Tree.Stratum.Code, DataLayer,
                                                            (long)jl.Tree.Stratum_CN);
                        currST = jl.Tree.Stratum.Code;
                    }   //  endif
                    if (volType == "B")
                        currNET = jl.NetBoardFoot;
                    else if (volType == "C")
                        currNET = jl.NetCubicFoot;

                    switch (logGrades[k])
                    {
                        case "0":
                            listToOutput[nthRow].value7 += currNET * jl.Tree.ExpansionFactor * currAC;
                            break;

                        case "1":
                            listToOutput[nthRow].value8 += currNET * jl.Tree.ExpansionFactor * currAC;
                            break;

                        case "2":
                            listToOutput[nthRow].value9 += currNET * jl.Tree.ExpansionFactor * currAC;
                            break;

                        case "3":
                            listToOutput[nthRow].value10 += currNET * jl.Tree.ExpansionFactor * currAC;
                            break;

                        case "4":
                            listToOutput[nthRow].value11 += currNET * jl.Tree.ExpansionFactor * currAC;
                            break;

                        case "5":
                            listToOutput[nthRow].value12 += currNET * jl.Tree.ExpansionFactor * currAC;
                            break;

                        case "6":
                            listToOutput[nthRow].value13 += currNET * jl.Tree.ExpansionFactor * currAC;
                            break;

                        case "7":
                            listToOutput[nthRow].value14 += currNET * jl.Tree.ExpansionFactor * currAC;
                            break;
                    }   //  end switch on log grade
                }   //  end foreach loop
            }   //  end for k loop
            return diameterFlag;
        }   //  end SumLogData

        private void outputLogSummary(StreamWriter strWriteOut, List<LCDDO> lcdList, List<StratumDO> sList)
        {
            //  R001/R002
            double calcValue = 0;
            //  modify summary labels
            string volType = "";
            string[] summaryLabels = rRH.R001R002parTwo;
            switch (currentReport)
            {
                case "R001":
                    volType = "MBF";
                    break;

                case "R002":
                    volType = "CCF";
                    break;
            }   //  end switch
            summaryLabels[2] = summaryLabels[2].Replace("XXX", volType);
            summaryLabels[3] = summaryLabels[3].Replace("XXX", volType);
            summaryLabels[4] = summaryLabels[4].Replace("XXX", volType);
            summaryLabels[5] = summaryLabels[5].Replace("XXX", volType);
            summaryLabels[6] = summaryLabels[6].Replace("XXX", volType);
            summaryLabels[10] = summaryLabels[10].Replace("xxx", volType);
            if (currentReport == "R001")
            {
                summaryLabels[11] = summaryLabels[11].Replace("xxx", "MBF");
                summaryLabels[11] = summaryLabels[11].Replace("zzz", "CCF");
            }
            else if (currentReport == "R002")
            {
                summaryLabels[11] = summaryLabels[11].Replace("xxx", "CCF");
                summaryLabels[11] = summaryLabels[11].Replace("zzz", "MBF");
            }   //  endif

            //  write heading lines
            strWriteOut.WriteLine("\n\n\n\n");
            strWriteOut.WriteLine(summaryLabels[0]);
            strWriteOut.WriteLine(summaryLabels[1]);

            //  Calculate first line -- January 2017 now number 32 foot logs per gross removed
            if (totalToOutput[0].Value3 > 0)
                calcValue = listToOutput.Sum(l => l.value10) / totalToOutput[0].Value3;
            else calcValue = 0.0;
            strWriteOut.Write(summaryLabels[2]);
            strWriteOut.WriteLine(String.Format("{0,6:F2}", calcValue).PadLeft(6, ' '));

            //  Second line -- January 2017 -- now Avg gross removed per 32 foot log
            //  March 2017 -- per email from Region 10, remove this line
            /*            calcValue = 0;
                        if (listToOutput.Sum(l => l.value10) > 0)
                            calcValue = totalToOutput[0].Value3 / listToOutput.Sum(l => l.value10);
                        else calcValue = 0.0;
                        strWriteOut.Write(summaryLabels[3]);
                        if (currentReport == "R001")
                            strWriteOut.WriteLine(Utilities.FormatField(calcValue, "{0,6:F3}").ToString().PadLeft(6, ' '));
                        else if(currentReport == "R002")
                            strWriteOut.WriteLine(Utilities.FormatField(calcValue,"{0,6:F2}").ToString().PadLeft(6,' '));
            */
            //  Third line --  needs total sale acres -- changed January 2017 to
            //  Third line -- Avg net removed per 32 foot log
            calcValue = 0;
            if (listToOutput.Sum(l => l.value10) > 0)
                calcValue = totalToOutput[0].Value7 / listToOutput.Sum(l => l.value10);
            else calcValue = 0.0;
            strWriteOut.Write(summaryLabels[4]);
            if (currentReport == "R001")
                strWriteOut.WriteLine(String.Format("{0,6:F3}", calcValue).PadLeft(6, ' '));
            else if (currentReport == "R002")
                strWriteOut.WriteLine(String.Format("{0,6:F2}", calcValue).PadLeft(6, ' '));
            //            calcValue = 0;
            //            List<CuttingUnitDO> cutList = bslyr.getCuttingUnits();
            //            double totalSaleAcres = cutList.Sum(c => c.Area);
            //            if(totalSaleAcres > 0) calcValue = totalToOutput[0].Value6 / totalSaleAcres;
            //            strWriteOut.Write(summaryLabels[4]);
            //            strWriteOut.WriteLine(Utilities.FormatField(calcValue,"{0,6:F2}").ToString().PadLeft(6,' '));

            //  Fourth and fifth line -- January 2017 -- gross and net removed per acre
            calcValue = 0;
            List<CuttingUnitDO> cutList = DataLayer.getCuttingUnits();
            double totalSaleAcres = cutList.Sum(c => c.Area);
            //  March 2017 -- per email from Region 10, remove gross line
            //  gross
            //            if(totalSaleAcres > 0) calcValue = totalToOutput[0].Value3 / totalSaleAcres;
            //            strWriteOut.Write(summaryLabels[5]);
            //            if(currentReport == "R001")
            //                strWriteOut.WriteLine(Utilities.FormatField(calcValue,"{0,6:F3}").ToString().PadLeft(6,' '));
            //            else if(currentReport == "R002")
            //                strWriteOut.WriteLine(Utilities.FormatField(calcValue,"{0,6:F2}").ToString().PadLeft(6,' '));

            //  net
            calcValue = 0;
            if (totalSaleAcres > 0) calcValue = totalToOutput[0].Value7 / totalSaleAcres;
            strWriteOut.Write(summaryLabels[6]);
            if (currentReport == "R001")
                strWriteOut.WriteLine(String.Format("{0,6:F3}", calcValue).PadLeft(6, ' '));
            else if (currentReport == "R002")
                strWriteOut.WriteLine(String.Format("{0,6:F2}", calcValue).PadLeft(6, ' '));

            //  Sixth line -- scaling defect -- January 2017 no change
            calcValue = 0;
            if (totalToOutput[0].Value3 > 0)
                calcValue = ((totalToOutput[0].Value3 - totalToOutput[0].Value7) / totalToOutput[0].Value3) * 100;
            strWriteOut.Write(summaryLabels[7]);
            strWriteOut.WriteLine(String.Format("{0,6:F2}", calcValue).PadLeft(6, ' '));

            //  Seventh line -- January 2017 --  Mean DBH for cut trees only
            calcValue = 0;
            //  Notes from previous CP version -- this was deleted per K.Dinsmore (10/2003)
            //  And then put back by K.Dinsmore (12/2004)
            //  has to run by stratum to properly expand values
            //  and now revised January 2017
            // And revised again to show quad mean -- February 2018

            double summedDBHsqrd = 0;
            double summedEF = 0;
            foreach (StratumDO s in sList)
            {
                double stratumAcres = Utilities.ReturnCorrectAcres(s.Code, DataLayer, (long)s.Stratum_CN);
                //  pull cuts and current stratum from lcd list
                List<LCDDO> justStratum = lcdList.FindAll(
                    delegate (LCDDO l)
                    {
                        return l.Stratum == s.Code && l.CutLeave == "C";
                    });
                //  sum up values needed
                //summedDBH += justStratum.Sum(j => j.SumDBHOB) * stratumAcres;
                summedDBHsqrd += justStratum.Sum(j => j.SumDBHOBsqrd) * stratumAcres;
                summedEF += justStratum.Sum(j => j.SumExpanFactor) * stratumAcres;
            }   //  end foreach on stratum list
            //  now output line
            if (summedEF > 0)
                //calcValue = summedDBH / summedEF;
                calcValue = Math.Sqrt(summedDBHsqrd / summedEF);
            calcValue = Math.Round(calcValue, 1, MidpointRounding.AwayFromZero);
            strWriteOut.Write(summaryLabels[8]);
            strWriteOut.WriteLine(String.Format("{0,6:F1}", calcValue).PadLeft(6, ' '));

            //  Eighth line is blank
            strWriteOut.WriteLine(summaryLabels[9]);

            //  Ninth line -- January 2017 -- Average cut tree sawlog volume, net
            calcValue = 0;
            summedEF = 0;
            //  need to check stratum method to get proper total tree count
            foreach (StratumDO s in sList)
            {
                double stratumAcres = Utilities.ReturnCorrectAcres(s.Code, DataLayer, (long)s.Stratum_CN);
                //  pull cuts and current stratum from lcd list
                List<LCDDO> justStratum = lcdList.FindAll(
                    delegate (LCDDO l)
                    {
                        return l.Stratum == s.Code && l.CutLeave == "C";
                    });
                if (s.Method == "3P" || s.Method == "S3P")
                    summedEF += justStratum.Sum(j => j.TalliedTrees);
                else summedEF += justStratum.Sum(j => j.SumExpanFactor) * stratumAcres;
            }   //  end foreach
            if (summedEF > 0)
                calcValue = totalToOutput[0].Value7 / summedEF;
            //  output line
            strWriteOut.Write(summaryLabels[10]);
            if (currentReport == "R001")
                strWriteOut.WriteLine(String.Format("{0,6:F3}", calcValue).PadLeft(6, ' '));
            else if (currentReport == "R002")
                strWriteOut.WriteLine(String.Format("{0,6:F2}", calcValue).PadLeft(6, ' '));

            // Tenth and last line -- January 2017 -- Sale conversion ratio
            calcValue = 0;
            //  use net unless they want something different
            double summedBDFT = 0.0;
            double summedCUFT = 0.0;
            double strAcres = 0.0;
            foreach (LCDDO l in lcdList)
            {
                //  find stratum in strata list to get correct acres
                int nthRow = sList.FindIndex(
                    delegate (StratumDO sl)
                    {
                        return sl.Code == l.Stratum;
                    });
                strAcres = Utilities.ReturnCorrectAcres(sList[nthRow].Code, DataLayer, (long)sList[nthRow].Stratum_CN);
                summedBDFT += l.SumNBDFT * strAcres;
                summedCUFT += l.SumNCUFT * strAcres;
            }   //  end foreach loop
            if (currentReport == "R001" && summedCUFT > 0)
                calcValue = (summedBDFT / convFactor) / (summedCUFT / 100);
            else if (currentReport == "R002" && summedBDFT > 0)
                calcValue = (summedCUFT / convFactor) / (summedBDFT / 1000);
            //  output line
            strWriteOut.Write(summaryLabels[11]);
            strWriteOut.WriteLine(String.Format("{0,6:F2}", calcValue).PadLeft(6, ' '));

            //  Sixth line --  woods defect -- January 2017 -- removed
            //            calcValue = 0;
            //            if(totalToOutput[0].Value3 > 0)
            //                calcValue = ((totalToOutput[0].Value3 - totalToOutput[0].Value7) / totalToOutput[0].Value3) * 100;
            //            strWriteOut.Write(summaryLabels[7]);
            //            strWriteOut.WriteLine(Utilities.FormatField(calcValue,"{0,6:F2}").ToString().PadLeft(6,' '));

            //  DONE!
            return;
        }   //  end outputLogSummary

        private void WriteCurrentGroup(StreamWriter strWriteOut, ref int pageNumb,
                                        reportHeaders rh, string[] completeHeader)
        {
            //  Works for R001/R002 and R003/R004
            switch (currentReport)
            {
                case "R001":
                case "R002":
                    //double calcValue = 0;
                    foreach (RegionalReports lto in listToOutput)
                    {
                        WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1],
                                rh.reportTitles[2], completeHeader, 14, ref pageNumb, "");
                        prtFields.Clear();
                        prtFields.Add("");
                        prtFields.Add(" ");
                        prtFields.Add(lto.value1.PadLeft(3, '0').PadRight(6, ' '));
                        if (currentReport == "R001")
                        {
                            //  gross volume
                            prtFields.Add(String.Format("{0,10:F3}", lto.value7 / convFactor).PadLeft(10, ' '));
                            //  Net volume
                            prtFields.Add(String.Format("{0,10:F3}", lto.value9 / convFactor).PadLeft(10, ' '));
                        }
                        else if (currentReport == "R002")
                        {
                            //  gross volume
                            prtFields.Add(String.Format("{0,10:F2}", lto.value7 / convFactor).PadLeft(10, ' '));
                            //  Net volume
                            prtFields.Add(String.Format("{0,10:F2}", lto.value9 / convFactor).PadLeft(10, ' '));
                        }

                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                    }   //  end for each loop
                    break;

                case "R003":
                case "R004":
                    double netPerCent = 0;
                    int lastLine = listToOutput.Count;
                    int currentLine = 1;
                    foreach (RegionalReports lto in listToOutput)
                    {
                        WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                            completeHeader, 11, ref pageNumb, "");
                        prtFields.Clear();
                        prtFields.Add("");
                        prtFields.Add(" ");
                        //  print line for gross
                        prtFields.Add(lto.value21);
                        prtFields.Add("STD GRS");
                        if (lto.value1 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value7 / convFactor).PadLeft(8, ' '));
                        if (lto.value2 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value9 / convFactor).PadLeft(8, ' '));
                        if (lto.value3 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value11 / convFactor).PadLeft(8, ' '));
                        if (lto.value4 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value13 / convFactor).PadLeft(8, ' '));
                        if (lto.value5 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value15 / convFactor).PadLeft(8, ' '));
                        if (lto.value6 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value17 / convFactor).PadLeft(8, ' '));
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                        //  print line for net
                        prtFields.Clear();
                        prtFields.Add("");
                        prtFields.Add(" ");
                        prtFields.Add(" ");     //  grade is blank on this line
                        prtFields.Add("    NET");
                        if (lto.value1 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value8 / convFactor).PadLeft(8, ' '));
                        if (lto.value2 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value10 / convFactor).PadLeft(8, ' '));
                        if (lto.value3 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value12 / convFactor).PadLeft(8, ' '));
                        if (lto.value4 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value14 / convFactor).PadLeft(8, ' '));
                        if (lto.value5 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value16 / convFactor).PadLeft(8, ' '));
                        if (lto.value6 != null)
                            prtFields.Add(String.Format("{0,8:F2}", lto.value18 / convFactor).PadLeft(8, ' '));
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                        //  print line for net percent
                        prtFields.Clear();
                        prtFields.Add("");
                        prtFields.Add(" ");
                        prtFields.Add(" ");     //  grade is blank on this line
                        prtFields.Add("   NET %");
                        if (totalToOutput[0].Value4 > 0)
                            netPerCent = lto.value8 / totalToOutput[0].Value4;
                        if (lto.value1 != null) prtFields.Add(String.Format("{0,8:P2}", netPerCent).PadLeft(7, ' '));
                        netPerCent = 0;
                        if (totalToOutput[0].Value6 > 0)
                            netPerCent = lto.value10 / totalToOutput[0].Value6;
                        if (lto.value2 != null) prtFields.Add(String.Format("{0,8:P2}", netPerCent).PadLeft(7, ' '));
                        netPerCent = 0;
                        if (totalToOutput[0].Value8 > 0)
                            netPerCent = lto.value12 / totalToOutput[0].Value8;
                        if (lto.value3 != null) prtFields.Add(String.Format("{0,8:P2}", netPerCent).PadLeft(7, ' '));
                        netPerCent = 0;
                        if (totalToOutput[0].Value10 > 0)
                            netPerCent = lto.value14 / totalToOutput[0].Value10;
                        if (lto.value4 != null) prtFields.Add(String.Format("{0,8:P2}", netPerCent).PadLeft(7, ' '));
                        netPerCent = 0;
                        if (totalToOutput[0].Value12 > 0)
                            netPerCent = lto.value16 / totalToOutput[0].Value12;
                        if (lto.value5 != null) prtFields.Add(String.Format("{0,8:P2}", netPerCent).PadLeft(7, ' '));
                        netPerCent = 0;
                        if (totalToOutput[0].Value14 > 0)
                            netPerCent = lto.value18 / totalToOutput[0].Value14;
                        if (lto.value6 != null) prtFields.Add(String.Format("{0,8:P2}", netPerCent).PadLeft(7, ' '));
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                        if (currentLine != lastLine)
                        {
                            //  add some blank lines
                            strWriteOut.WriteLine("");
                            currentLine++;
                            numOlines++;
                        }   //  endif
                    }   //  end foreach loop
                    break;
            }   //  end switch on current report
            return;
        }   //  end WriteCurrentGroup

        private void WriteCurrentGroup(double unitAcres, StreamWriter strWriteOut,
                                            ref int pageNumb, reportHeaders rh)
        {
            //  R005 only
            double calcValue = 0;
            int loadAcres = 0;

            regionalReportHeaders rrh = new regionalReportHeaders();
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                                rrh.R005columns, 17, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                //  method, species, product
                prtFields.Add(lto.value1);
                prtFields.Add(lto.value2.PadLeft(3, '0').PadRight(6, ' '));
                prtFields.Add(lto.value3.PadLeft(2, '0'));
                //  calculate scale defect percent
                calcValue = 0;
                if (lto.value9 > 0)
                    calcValue = ((lto.value9 - lto.value8) / lto.value9) * 100;
                prtFields.Add(String.Format("{0,5:F1}", calcValue).PadLeft(5, ' '));
                //  calculate woods defect percent
                calcValue = 0;
                if (lto.value7 > 0)
                    calcValue = ((lto.value7 - lto.value8) / lto.value7) * 100;
                prtFields.Add(String.Format("{0,5:F1}", calcValue).PadLeft(5, ' '));
                //  Gross removed and calculate gross removed per acre
                prtFields.Add(String.Format("{0,8:F0}", lto.value7 / 100.0).PadLeft(8, ' '));
                calcValue = 0;
                if (unitAcres > 0)
                    calcValue = (lto.value7 / 100.0) / unitAcres;
                prtFields.Add(String.Format("{0,6:F0}", calcValue).PadLeft(6, ' '));
                //  Average log volume
                calcValue = 0;
                if (lto.value10 > 0)
                    calcValue = (lto.value7 / 100.0) / lto.value10;
                prtFields.Add(String.Format("{0,7:F2}", calcValue).PadLeft(7, ' '));
                //  Net volume and calculate net per acre
                prtFields.Add(String.Format("{0,8:F0}", lto.value8 / 100.0).PadLeft(8, ' '));
                calcValue = 0;
                if (unitAcres > 0)
                    calcValue = (lto.value8 / 100.0) / unitAcres;
                prtFields.Add(String.Format("{0,8:F0}", calcValue).PadLeft(8, ' '));
                //Estimated trees and calculate trees per acre
                prtFields.Add(String.Format("{0,6:F0}", lto.value11).PadLeft(6, ' '));
                calcValue = 0;
                if (unitAcres > 0)
                    calcValue = lto.value11 / unitAcres;
                prtFields.Add(String.Format("{0,6:F1}", calcValue).PadLeft(6, ' '));
                //  Mean DBH
                calcValue = 0;
                if (lto.value11 > 0)
                    calcValue = lto.value12 / lto.value11;
                prtFields.Add(String.Format("{0,6:F1}", calcValue).PadLeft(6, ' '));
                //  Mean height
                calcValue = 0;
                if (lto.value11 > 0)
                    calcValue = lto.value13 / lto.value11;
                prtFields.Add(String.Format("{0,7:F1}", calcValue).PadLeft(7, ' '));
                //  logs per ccf
                calcValue = 0;
                if (lto.value10 > 0)
                    calcValue = 1 / ((lto.value7 / 100.0) / lto.value10);
                prtFields.Add(String.Format("{0,7:F1}", calcValue).PadLeft(7, ' '));
                //  Standing gross per acre
                calcValue = 0;
                if (unitAcres > 0)
                    calcValue = (lto.value7 / 100.0) / unitAcres;
                prtFields.Add(String.Format("{0,7:F1}", calcValue).PadLeft(7, ' '));
                //  unit acres
                if (loadAcres == 0)
                {
                    prtFields.Add(String.Format("{0,5:F0}", unitAcres).PadLeft(5, ' '));
                    loadAcres = 1;
                }
                else prtFields.Add("");
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   // end foreach loop
            return;
        }   // end WriteCurrentGroup

        private void WriteOutputFile(List<LogMatrix> outputMatrix)
        {
            //  R008/R009
            string outFile = "";
            //  per Dan O'Leary, Region 10, headers are different for each of these output files
            string outputHeader = "";
            if (currentReport == "R008")
            {
                outputHeader = "SALE NAME,CRUISE NUMBER,LOG SORT DESCRIPTION,SPECIES CODE,LOG GRADE CODE,LOG SED,GVOL STANDING MBF,GVOL REMVD MBF,NVOL RMVD MBF,NVOL UTILITY MBF";
                outFile = System.IO.Path.ChangeExtension(FilePath, "008");
            }
            else if (currentReport == "R009")
            {
                outputHeader = "SALE NAME,CRUISE NUMBER,LOG SORT DESCRIPTION,SPECIES CODE,LOG GRADE CODE,LOG SED,GVOL REMVD MBF,NVOL RMVD MBF,NVOL UTILITY MBF";
                outFile = System.IO.Path.ChangeExtension(FilePath, "009");
            }
            using (StreamWriter textOut = new StreamWriter(outFile))
            {
                //  output heading line
                textOut.WriteLine(outputHeader);
                foreach (LogMatrix om in outputMatrix)
                {
                    if (om.speciesCode == "351   " && currentReport == "R009")
                        textOut.WriteLine();
                    textOut.Write(om.nameSale.PadRight(25, ' '));
                    textOut.Write(",");
                    textOut.Write(om.cruiseNumb.PadLeft(5, ' '));
                    textOut.Write(",");
                    textOut.Write(om.logSortDescrip.PadRight(30, ' '));
                    textOut.Write(",");
                    textOut.Write(om.speciesCode.PadLeft(3, '0').PadRight(6, ' '));
                    textOut.Write(",");
                    textOut.Write(om.logGradeCode.PadRight(13, ' '));
                    textOut.Write(",");
                    textOut.Write(om.logSED.PadRight(23, ' '));
                    textOut.Write(",");
                    if (om.standingGross == null)
                        textOut.Write("    0.000");
                    else textOut.Write(om.standingGross.PadLeft(9, ' '));
                    textOut.Write(",");
                    if (currentReport == "R008")
                    {
                        if (om.grossRemoved == null)
                            textOut.Write("    0.000");
                        else textOut.Write(om.grossRemoved.PadLeft(9, ' '));
                        textOut.Write(",");
                    }   //  endif
                    if (om.netRemoved == null)
                        textOut.Write("    0.000");
                    else textOut.Write(om.netRemoved.PadLeft(9, ' '));
                    textOut.Write(",");
                    if (om.netUtility == null)
                        textOut.WriteLine("    0.000");
                    else textOut.WriteLine(om.netUtility.PadLeft(9, ' '));
                    if (om.speciesCode == "351   " && currentReport == "R009")
                        textOut.WriteLine();
                }   //  end foreach loop
                textOut.Close();
            }   //  end using
            return;
        }   //  end WriteOutputFile

        private void OutputLogData(StreamWriter textFile)
        {
            if (currentReport == "R006") convFactor = 1000.0;
            foreach (RegionalReports lto in listToOutput)
            {
                textFile.Write(lto.value1.PadLeft(6, ' '));
                textFile.Write(lto.value2.PadLeft(8, ' '));
                textFile.Write(lto.value3.PadLeft(11, ' '));
                textFile.Write(String.Format("{0,10:F2}", lto.value7 / convFactor).PadLeft(15, ' '));
                textFile.Write(String.Format("{0,10:F2}", lto.value8 / convFactor).PadLeft(12, ' '));
                textFile.Write(String.Format("{0,10:F2}", lto.value9 / convFactor).PadLeft(13, ' '));
                textFile.Write(String.Format("{0,10:F2}", lto.value10 / convFactor).PadLeft(13, ' '));
                textFile.Write(String.Format("{0,10:F2}", lto.value12 / convFactor).PadLeft(13, ' '));
                textFile.Write(String.Format("{0,10:F2}", lto.value13 / convFactor).PadLeft(13, ' '));
                textFile.WriteLine(String.Format("{0,10:F2}", lto.value14 / convFactor).PadLeft(13, ' '));
            }   //  end foreach loop
            // blank line between species
            textFile.WriteLine();
            //  clear output list for next species
            listToOutput.Clear();
            return;
        }   //  end OutputLogData

        private void updateTotal()
        {
            //  R001/R002
            ReportSubtotal rs = new ReportSubtotal();
            rs.Value3 = listToOutput.Sum(l => l.value7) / convFactor;
            rs.Value6 = listToOutput.Sum(l => l.value8) / convFactor;
            rs.Value7 = listToOutput.Sum(l => l.value9) / convFactor;
            totalToOutput.Add(rs);
        }   //  end updateTotal

        private void updateGradeTotals()
        {
            //  R003/R004
            ReportSubtotal rs = new ReportSubtotal();
            //  gross for each species
            rs.Value3 = listToOutput.Sum(l => l.value7);
            rs.Value5 = listToOutput.Sum(l => l.value9);
            rs.Value7 = listToOutput.Sum(l => l.value11);
            rs.Value9 = listToOutput.Sum(l => l.value13);
            rs.Value11 = listToOutput.Sum(l => l.value15);
            rs.Value13 = listToOutput.Sum(l => l.value17);
            //  net for each species
            rs.Value4 = listToOutput.Sum(l => l.value8);
            rs.Value6 = listToOutput.Sum(l => l.value10);
            rs.Value8 = listToOutput.Sum(l => l.value12);
            rs.Value10 = listToOutput.Sum(l => l.value14);
            rs.Value12 = listToOutput.Sum(l => l.value16);
            rs.Value14 = listToOutput.Sum(l => l.value18);
            totalToOutput.Add(rs);
            return;
        }   //  end updateGradeTotals

        private void updateSubtotals(ref List<ReportSubtotal> logMethodSubtotal)
        {
            //  R005
            ReportSubtotal rs = new ReportSubtotal();
            rs.Value1 = listToOutput[0].value1;
            rs.Value7 = listToOutput.Sum(l => l.value7);
            rs.Value8 = listToOutput.Sum(l => l.value8);
            rs.Value9 = listToOutput.Sum(l => l.value9);
            rs.Value10 = listToOutput.Sum(l => l.value10);
            rs.Value11 = listToOutput.Sum(l => l.value11);
            rs.Value12 = listToOutput.Sum(l => l.value12);
            rs.Value13 = listToOutput.Sum(l => l.value13);
            logMethodSubtotal.Add(rs);
            return;
        }   //  end updateSubtotals

        private void updateOverallTotal()
        {
            //  R005
            if (totalToOutput.Count > 0)
            {
                totalToOutput[0].Value7 += listToOutput.Sum(l => l.value7);
                totalToOutput[0].Value8 += listToOutput.Sum(l => l.value8);
                totalToOutput[0].Value9 += listToOutput.Sum(l => l.value9);
                totalToOutput[0].Value10 += listToOutput.Sum(l => l.value10);
                totalToOutput[0].Value11 += listToOutput.Sum(l => l.value11);
                totalToOutput[0].Value12 += listToOutput.Sum(l => l.value12);
                totalToOutput[0].Value13 += listToOutput.Sum(l => l.value13);
            }
            else
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value7 = listToOutput.Sum(l => l.value7);
                rs.Value8 = listToOutput.Sum(l => l.value8);
                rs.Value9 = listToOutput.Sum(l => l.value9);
                rs.Value10 = listToOutput.Sum(l => l.value10);
                rs.Value11 = listToOutput.Sum(l => l.value11);
                rs.Value12 = listToOutput.Sum(l => l.value12);
                rs.Value13 = listToOutput.Sum(l => l.value13);
                totalToOutput.Add(rs);
            }   //  endif
            return;
        }   //  end updateSubtotals

        private void outputTotal(StreamWriter strWriteOut)
        {
            switch (currentReport)
            {
                case "R001":
                case "R002":
                    strWriteOut.WriteLine("      ----------   ------------  ---------");
                    strWriteOut.Write("      All Species  ");
                    if (currentReport == "R001")
                    {
                        strWriteOut.Write(String.Format("{0,10:F3}", totalToOutput[0].Value3).PadLeft(10, ' '));
                        strWriteOut.WriteLine(String.Format("{0,10:F3}", totalToOutput[0].Value7).PadLeft(13, ' '));
                    }
                    else if (currentReport == "R002")
                    {
                        strWriteOut.Write(String.Format("{0,10:F2}", totalToOutput[0].Value3).PadLeft(10, ' '));
                        strWriteOut.WriteLine(String.Format("{0,10:F2}", totalToOutput[0].Value7).PadLeft(13, ' '));
                    }
                    break;

                case "R003":
                case "R004":
                    //  build columns based on nuber of species recorded
                    StringBuilder SB = new StringBuilder();
                    StringBuilder allGrades = new StringBuilder();
                    StringBuilder netLine = new StringBuilder();
                    StringBuilder lastLine = new StringBuilder();
                    SB.Append(" -----------    -----");
                    allGrades.Append(" ALL GRADES   STD GRS    ");
                    netLine.Append("                  NET    ");
                    lastLine.Append("                NET %");
                    if (listToOutput[0].value1 != null)
                    {
                        SB.Append("  ---------------");
                        allGrades.Append(String.Format("{0,9:F2}", totalToOutput[0].Value3 / convFactor).PadLeft(9, ' '));
                        allGrades.Append("        ");
                        netLine.Append(String.Format("{0,9:F2}", totalToOutput[0].Value4 / convFactor).PadLeft(9, ' '));
                        netLine.Append("        ");
                        lastLine.Append("     100.00 %");
                    }   //  ENDIF
                    if (listToOutput[0].value2 != null)
                    {
                        SB.Append("  ---------------");
                        allGrades.Append(String.Format("{0,9:F2}", totalToOutput[0].Value5 / convFactor).PadLeft(9, ' '));
                        allGrades.Append("        ");
                        netLine.Append(String.Format("{0,9:F2}", totalToOutput[0].Value6 / convFactor).PadLeft(9, ' '));
                        netLine.Append("        ");
                        lastLine.Append("         100.00 %");
                    }   //  ENDIF
                    if (listToOutput[0].value3 != null)
                    {
                        SB.Append("  ---------------");
                        allGrades.Append(String.Format("{0,9:F2}", totalToOutput[0].Value7 / convFactor).PadLeft(9, ' '));
                        allGrades.Append("        ");
                        netLine.Append(String.Format("{0,9:F2}", totalToOutput[0].Value8 / convFactor).PadLeft(9, ' '));
                        netLine.Append("        ");
                        lastLine.Append("         100.00 %");
                    }   //  ENDIF
                    if (listToOutput[0].value4 != null)
                    {
                        SB.Append("  ---------------");
                        allGrades.Append(String.Format("{0,9:F2}", totalToOutput[0].Value9 / convFactor).PadLeft(9, ' '));
                        allGrades.Append("        ");
                        netLine.Append(String.Format("{0,9:F2}", totalToOutput[0].Value10 / convFactor).PadLeft(9, ' '));
                        netLine.Append("        ");
                        lastLine.Append("         100.00 %");
                    }   //  ENDIF
                    if (listToOutput[0].value5 != null)
                    {
                        SB.Append("  ---------------");
                        allGrades.Append(String.Format("{0,9:F2}", totalToOutput[0].Value11 / convFactor).PadLeft(9, ' '));
                        allGrades.Append("        ");
                        netLine.Append(String.Format("{0,9:F2}", totalToOutput[0].Value12 / convFactor).PadLeft(9, ' '));
                        netLine.Append("        ");
                        lastLine.Append("         100.00 %");
                    }   //  ENDIF
                    if (listToOutput[0].value6 != null)
                    {
                        SB.Append("  ---------------");
                        allGrades.Append(String.Format("{0,9:F2}", totalToOutput[0].Value13 / convFactor).PadLeft(9, ' '));
                        allGrades.Append("        ");
                        netLine.Append(String.Format("{0,9:F2}", totalToOutput[0].Value14 / convFactor).PadLeft(9, ' '));
                        netLine.Append("        ");
                        lastLine.Append("         100.00 %");
                    }   //  ENDIF

                    strWriteOut.WriteLine(SB.ToString());
                    strWriteOut.WriteLine(allGrades.ToString());
                    strWriteOut.WriteLine(netLine.ToString());
                    strWriteOut.WriteLine(lastLine.ToString());
                    break;
            }   //  end switch on report
            return;
        }   //  end outputTotal

        private void outputTotalSubtotal(List<ReportSubtotal> outputTotal, int totalToPrint,
                                            StreamWriter strWriteOut, ref int pageNumb, double unitAcres)
        {
            //  R005
            double calcValue = 0;
            switch (totalToPrint)
            {
                case 1:
                    strWriteOut.WriteLine();
                    strWriteOut.Write("     TOTAL ");
                    strWriteOut.Write(outputTotal[0].Value1.PadRight(4, ' '));
                    break;

                case 2:
                    strWriteOut.WriteLine(reportConstants.longLine);
                    strWriteOut.Write(" TOTALS        ");
                    break;
            }   //  end switch
            //  calculate scale defect percent
            calcValue = 0;
            if (outputTotal[0].Value9 > 0)
                calcValue = ((outputTotal[0].Value9 - outputTotal[0].Value8) / outputTotal[0].Value9) * 100;
            strWriteOut.Write(String.Format("{0,5:F1}", calcValue).PadLeft(5, ' '));
            //  calculate woods defect percent
            calcValue = 0;
            if (outputTotal[0].Value7 > 0)
                calcValue = ((outputTotal[0].Value7 - outputTotal[0].Value8) / outputTotal[0].Value7) * 100;
            strWriteOut.Write(String.Format("{0,5:F1}", calcValue).PadLeft(7, ' '));
            //  Gross removed and calculate gross removed per acre
            strWriteOut.Write(String.Format("{0,8:F0}", outputTotal[0].Value9 / 100.0).PadLeft(9, ' '));
            calcValue = 0;
            if (unitAcres > 0)
                calcValue = (outputTotal[0].Value9 / 100.0) / unitAcres;
            strWriteOut.Write(String.Format("{0,6:F0}", calcValue).PadLeft(7, ' '));
            //  Average log volume
            calcValue = 0;
            if (outputTotal[0].Value10 > 0)
                calcValue = (outputTotal[0].Value7 / 100.0) / outputTotal[0].Value10;
            strWriteOut.Write(String.Format("{0,7:F2}", calcValue).PadLeft(9, ' '));
            //  Net volume and calculate net per acre
            strWriteOut.Write(String.Format("{0,8:F0}", outputTotal[0].Value8 / 100.0).PadLeft(10, ' '));
            calcValue = 0;
            if (unitAcres > 0)
                calcValue = (outputTotal[0].Value8 / 100.0) / unitAcres;
            strWriteOut.Write(String.Format("{0,8:F0}", calcValue).PadLeft(10, ' '));
            //Estimated trees and calculate trees per acre
            strWriteOut.Write(String.Format("{0,6:F0}", outputTotal[0].Value11).PadLeft(8, ' '));
            calcValue = 0;
            if (unitAcres > 0)
                calcValue = outputTotal[0].Value11 / unitAcres;
            strWriteOut.Write(String.Format("{0,6:F1}", calcValue).PadLeft(8, ' '));
            //  Mean DBH
            calcValue = 0;
            if (outputTotal[0].Value11 > 0)
                calcValue = outputTotal[0].Value12 / outputTotal[0].Value11;
            strWriteOut.Write(String.Format("{0,6:F1}", calcValue).PadLeft(8, ' '));
            //  Mean height
            calcValue = 0;
            if (outputTotal[0].Value11 > 0)
                calcValue = outputTotal[0].Value13 / outputTotal[0].Value11;
            strWriteOut.Write(String.Format("{0,7:F1}", calcValue).PadLeft(9, ' '));
            //  logs per ccf
            calcValue = 0;
            if (outputTotal[0].Value10 > 0)
                calcValue = 1 / ((outputTotal[0].Value7 / 100.0) / outputTotal[0].Value10);
            strWriteOut.Write(String.Format("{0,7:F1}", calcValue).PadLeft(9, ' '));
            //  Standing gross per acre
            calcValue = 0;
            if (unitAcres > 0)
                calcValue = (outputTotal[0].Value7 / 100.0) / unitAcres;
            strWriteOut.WriteLine(String.Format("{0,7:F1}", calcValue).PadLeft(9, ' '));
            if (totalToPrint == 1)
                strWriteOut.WriteLine(reportConstants.longLine);
            return;
        }   //  end outputTotalSubtotal

        private string[] createCompleteHeader()
        {
            string[] finnishHeader = new string[7];
            finnishHeader[6] = rRH.R10specialLine;
            for (int k = 0; k < 5; k++)
                finnishHeader[k] = rRH.R001R002columns[k];

            switch (currentReport)
            {
                case "R001":
                    finnishHeader[0] = finnishHeader[0].Replace("XXX", "MBF");
                    break;

                case "R002":
                    finnishHeader[0] = finnishHeader[0].Replace("XXX", "CCF");
                    break;
            }   //  end switch on current report
            return finnishHeader;
        }   //  end createCompleteHeader

        private string[] createCompleteHeader(List<LCDDO> speciesList)
        {
            //   R003/R004
            string[] finnishHeader = new string[2];
            finnishHeader[0] = "   GRADE                   ";
            finnishHeader[1] = " -----------    -----";
            foreach (LCDDO sl in speciesList)
            {
                finnishHeader[0] += sl.Species.PadLeft(3, '0').PadRight(6, ' ');
                finnishHeader[0] += "           ";
                finnishHeader[1] += "  ---------------";
            }   //  end foreach loop
            return finnishHeader;
        }   //  end createCompleteHeader

        private double FindProrationFactor(string currCL, string currPP, string currST, string currSP,
                                            string currCU, List<PRODO> pList, List<TreeDO> tList)
        {
            //  find proration factor
            int nthRow = tList.FindIndex(
                delegate (TreeDO t)
                {
                    return t.SampleGroup.CutLeave == currCL && t.Stratum.Code == currST &&
                        t.Species == currSP && t.SampleGroup.PrimaryProduct == currPP;
                });
            if (nthRow >= 0)
            {
                nthRow = pList.FindIndex(
                    delegate (PRODO p)
                    {
                        return p.CutLeave == currCL && p.Stratum == currST && p.CuttingUnit == currCU &&
                                p.SampleGroup == tList[nthRow].SampleGroup.Code && p.PrimaryProduct == currPP;
                    });
            }
            else return 0;
            if (nthRow >= 0)
                return pList[nthRow].ProrationFactor;
            else return 0;
        }   //  end FindProrationFactor

        private void CalculateVolume(List<LogMatrix> outputMatrix, int nthRow, string volType, List<LogStockDO> justLogs)
        {
            //  R008/R009
            string prevStratum = "*";
            double currAC = 0;
            double calcValue = 0;
            double currentValue = 0;

            foreach (LogStockDO jl in justLogs)
            {
                //  stratum acres
                if (prevStratum != jl.Tree.Stratum.Code)
                {
                    currAC = Utilities.ReturnCorrectAcres(jl.Tree.Stratum.Code, DataLayer, (long)jl.Tree.Stratum_CN);
                    prevStratum = jl.Tree.Stratum.Code;
                }   //  endif
                switch (volType)
                {
                    case "Gross":
                        calcValue += jl.GrossBoardFoot * jl.Tree.ExpansionFactor * currAC;
                        break;

                    case "Net":
                        calcValue += jl.NetBoardFoot * jl.Tree.ExpansionFactor * currAC;
                        break;

                    case "Rem":
                        calcValue += jl.BoardFootRemoved * jl.Tree.ExpansionFactor * currAC;
                        break;

                    case "Util":
                        if (jl.PercentRecoverable > 0)
                            calcValue += (jl.NetBoardFoot * jl.Tree.ExpansionFactor * currAC) *
                                            (jl.PercentRecoverable / 100) + 0.049;
                        break;
                }   //  end switch on volume type
            }   //  end foreach loop
            //  store calculated value in appropriate column
            switch (volType)
            {
                case "Gross":
                    currentValue = Convert.ToDouble(outputMatrix[nthRow].standingGross);
                    currentValue += calcValue / 1000;
                    outputMatrix[nthRow].standingGross = String.Format("{0,9:F3}", currentValue);
                    break;

                case "Net":
                    currentValue = Convert.ToDouble(outputMatrix[nthRow].netRemoved);
                    currentValue += calcValue / 1000;
                    outputMatrix[nthRow].netRemoved = String.Format("{0,9:F3}", currentValue);
                    break;

                case "Rem":
                    currentValue = Convert.ToDouble(outputMatrix[nthRow].grossRemoved);
                    currentValue += calcValue / 1000;
                    outputMatrix[nthRow].grossRemoved = String.Format("{0,9:F3}", currentValue);
                    break;

                case "Util":
                    currentValue = Convert.ToDouble(outputMatrix[nthRow].netUtility);
                    currentValue += calcValue / 1000;
                    outputMatrix[nthRow].netUtility = String.Format("{0,9:F3}", currentValue);
                    break;
            }   //  end switch
            return;
        }   //  end CalculateVolume

        private List<LogMatrix> CreateLogMatrix(List<LogMatrixDO> reportMatrix)
        {
            //  find current report
            List<LogMatrixDO> justCurrentReport = reportMatrix.FindAll(
                delegate (LogMatrixDO lm)
                {
                    return lm.ReportNumber == currentReport;
                });
            //  pull salename and cruise number
            List<SaleDO> sList = DataLayer.getSale();
            string currSN = sList[0].Name;
            string currCN = sList[0].SaleNumber;
            //  load output matrix
            List<LogMatrix> matrixToOutput = new List<LogMatrix>();
            StringBuilder sb = new StringBuilder();
            foreach (LogMatrixDO jcr in justCurrentReport)
            {
                LogMatrix mto = new LogMatrix();
                mto.nameSale = currSN;
                mto.cruiseNumb = currCN;
                mto.logSortDescrip = jcr.LogSortDescription;
                mto.speciesCode = jcr.Species;
                //  concatenate grade for log grade description
                sb.Clear();
                buildLogGradeDescription(jcr.LogGrade1, "", sb);
                buildLogGradeDescription(jcr.LogGrade2, jcr.GradeDescription, sb);
                buildLogGradeDescription(jcr.LogGrade3, jcr.GradeDescription, sb);
                buildLogGradeDescription(jcr.LogGrade4, jcr.GradeDescription, sb);
                buildLogGradeDescription(jcr.LogGrade5, jcr.GradeDescription, sb);
                buildLogGradeDescription(jcr.LogGrade6, jcr.GradeDescription, sb);
                mto.logGradeCode = sb.ToString().TrimStart(' ');
                //  concatenate diameter description
                sb.Clear();
                buildDiameterDescription(jcr.SEDlimit, jcr.SEDminimum, jcr.SEDmaximum, sb);
                mto.logSED = sb.ToString();
                matrixToOutput.Add(mto);
            }   //  end foreach loop
            return matrixToOutput;
        }   //  end CreateLogMatrix

        private void buildLogGradeDescription(string currLG, string currGD, StringBuilder updatedDescrip)
        {
            //  R008/R009
            if (currLG != "" && currLG != null)
            {
                updatedDescrip.Append(" ");
                updatedDescrip.Append(currGD);
                updatedDescrip.Append(" ");
                updatedDescrip.Append(currLG);
            }   //  endif
            return;
        }   //  end buildLogGradeDescription

        private void buildDiameterDescription(string currLimit, double currMin, double currMax,
                                StringBuilder updatedDiameter)
        {
            //  R008/R009
            switch (currLimit)
            {
                case "":
                case null:
                    if (currMin == 0)
                        updatedDiameter.Append(" ");
                    else if (currMax == 0 && currMin > 0)
                    {
                        updatedDiameter.Append("  ");
                        updatedDiameter.Append(String.Format("{0,4:F1}", currMin));
                        updatedDiameter.Append("+");
                    }   //  endif
                    break;

                case "between":
                    updatedDiameter.Append("  ");
                    updatedDiameter.Append(currLimit);
                    updatedDiameter.Append(" ");
                    updatedDiameter.Append(String.Format("{0,4:F1}", currMin));
                    updatedDiameter.Append(" and ");
                    updatedDiameter.Append(String.Format("{0,4:F1}", currMax));
                    break;

                case "greater":
                case "less":
                    updatedDiameter.Append("  ");
                    updatedDiameter.Append(currLimit);
                    updatedDiameter.Append(" than ");
                    updatedDiameter.Append(String.Format("{0,4:F1}", currMin));
                    break;

                case "thru":
                    updatedDiameter.Append("  ");
                    updatedDiameter.Append(String.Format("{0,4:F1}", currMin));
                    updatedDiameter.Append(" ");
                    updatedDiameter.Append(currLimit);
                    updatedDiameter.Append(" ");
                    updatedDiameter.Append(String.Format("{0,4:F1}", currMax));
                    break;
            }   //  end switch
            return;
        }   //  end buildDiameterDescription
    }
}