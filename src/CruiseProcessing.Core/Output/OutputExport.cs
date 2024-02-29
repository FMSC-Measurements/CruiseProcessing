using CruiseDAL.DataObjects;
using CruiseProcessing.Output;
using CruiseProcessing.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CruiseProcessing
{
    // TODO unused class, remove?
    public class OutputExport : ReportGeneratorBase
    {
        private List<exportToOutput> exToOutput = new List<exportToOutput>();
        private int[] fieldLengths;
        private List<string> prtFields = new List<string>();
        private List<LogDO> logList = new List<LogDO>();
        private List<LogStockDO> logStockList = new List<LogStockDO>();
        private List<ReportSubtotal> unitSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> strataSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> overallTotal = new List<ReportSubtotal>();
        private string[] completeHeader;
        private int numColumns = 0;

        public IDialogService DialogService { get; }

        public OutputExport(CPbusinessLayer dataLayer, IDialogService dialogService, HeaderFieldData headerData, string reportID) : base(dataLayer, headerData, reportID)
        {
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        public void CreateExportReports(StreamWriter strWriteOut, ref int pageNumb)
        {
            //  is this a variable log length cruise?  And are there log records?
            List<SaleDO> sList = DataLayer.getSale();
            //  not sure where cruise type has disappeared to
            if (sList[0].Purpose != "V")
            {
                DialogService.ShowError("This is not a Variable Log Length cruise.\nCannot produce Export Reports.");
                return;
            }   //  endif
            logList = DataLayer.getLogs();
            logStockList = DataLayer.getLogStock();
            if (logList.Count == 0 && logStockList.Count == 0)
            {
                noDataForReport(strWriteOut, currentReport, " No log data for report ");
                return;
            }   //  endif
            logList.Clear();
            logStockList.Clear();
            //  generate reports
            string currentTitle = fillReportTitle(currentReport);
            switch (currentReport)
            {
                case "EX1":
                    numOlines = 0;
                    SetReportTitles(currentTitle, 6, 0, 0, "", "");
                    fieldLengths = new int[] { 2, 7, 6, 7, 5, 6, 9, 7, 7, 11, 3 };
                    //  need a joined log table to get tree information
                    logList = DataLayer.getTreeLogs();
                    CreateEX1(strWriteOut, ref pageNumb);
                    logList.Clear();
                    break;

                case "EX2":
                    numOlines = 0;
                    SetReportTitles(currentTitle, 6, 0, 0, "", "");
                    completeHeader = createCompleteHeader();
                    fieldLengths = new int[] { 2, 7, 6, 7, 5, 6, 9, 7, 7, 11, 3, 6, 6, 6, 6, 7, 4 };
                    //  need joined log stock table to get tree information
                    logStockList = DataLayer.getLogStockSorted();
                    int nResult = CreateEX2(strWriteOut, ref pageNumb);
                    if (nResult == -1)
                    {
                        //  output EX3 for comparison purposes
                        try
                        {
                            CreateEX3(strWriteOut, ref pageNumb);
                        }
                        finally
                        { 
                            DialogService.ShowError("ERRORS FOUND IN EXPORT GRADE DATA \r\nSee Report EX2");
                        }
                        //Application.Exit();
                        return;
                    }   //  endif
                    logStockList.Clear();
                    break;

                case "EX3":
                    //  listing of export defaults
                    numOlines = 0;
                    SetReportTitles(currentTitle, 6, 0, 0, "", "");
                    fieldLengths = new int[] { 10, 10, 5, 6, 21, 9, 8, 9, 3 };
                    CreateEX3(strWriteOut, ref pageNumb);
                    break;

                case "EX4":
                case "EX5":
                    numOlines = 0;
                    fieldLengths = new int[] { 5, 18, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 6 };
                    //  pull just unique species from LCD table
                    List<LCDDO> justSpecies = DataLayer.GetLCDgroup("", 5, "C");
                    //  each species is at least one page
                    string firstHeader = " ***** NET ";
                    string secondHeader = "  SPECIES:  ";
                    foreach (LCDDO js in justSpecies)
                    {
                        secondHeader += js.Species;
                        if (currentReport == "EX4")
                            firstHeader += "MBF *****";
                        else if (currentReport == "EX5")
                            firstHeader += "CCF *****";
                        SetReportTitles(currentTitle, 6, 0, 0, firstHeader, secondHeader);
                        CreateEX4and5(strWriteOut, ref pageNumb, js.Species);
                    }   //  end foreach loop
                    break;

                case "EX6":
                case "EX7":
                    numOlines = 0;
                    SetReportTitles(currentTitle, 6, 0, 0, "", "");
                    completeHeader = createCompleteHeader();
                    fieldLengths = new int[] { 2, 6, 6, 10, 6, 6, 10, 9, 9, 8, 8, 11, 4 };
                    List<StratumDO> stList = DataLayer.getStratum();
                    foreach (StratumDO s in stList)
                    {
                        s.CuttingUnits.Populate();
                        foreach (CuttingUnitDO cu in s.CuttingUnits)
                        {
                            CreateEX6orEX7(s, cu.Code);
                            //  output unit data
                            WriteUnitGroup(strWriteOut, ref pageNumb);
                            //  update all subtotals
                            updateSubtotal(unitSubtotal, cu.Code);
                            updateSubtotal(strataSubtotal, s.Code);
                            updateSubtotal(overallTotal, "OVERALL");

                            if (currentReport == "EX6")
                            {
                                //  output unit subtotal
                                outputSubtotals(strWriteOut, ref pageNumb, unitSubtotal, "U");
                                unitSubtotal.Clear();
                            }   //  endif
                            exToOutput.Clear();
                        }   //  end foreach loop on cutting units
                        //  output stratum subtotal
                        outputSubtotals(strWriteOut, ref pageNumb, strataSubtotal, "S");
                        strataSubtotal.Clear();
                    }   //  end foreach loop on strata
                    if (currentReport == "EX6")
                    {
                        //  output overall total
                        outputSubtotals(strWriteOut, ref pageNumb, overallTotal, "T");
                    }
                    else if (currentReport == "EX7")
                    {
                        //  output summary section
                    }   //  endif
                    break;
            }   //  end switch on report
            return;
        }   //  end CreateExportReports

        private void CreateEX1(StreamWriter strWriteOut, ref int pageNumb)
        {
            //  this is just a straight list
            //  everything needed to print is in logList
            foreach (LogDO l in logList)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                        reportHeaders.EX1EX2left, 10, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(l.Tree.Stratum.Code.PadLeft(2, ' '));
                prtFields.Add(l.Tree.CuttingUnit.Code.PadLeft(4, ' '));
                prtFields.Add(l.Tree.Plot.PlotNumber.ToString().PadLeft(4, ' '));
                prtFields.Add(l.Tree.TreeNumber.ToString().PadLeft(3, ' '));
                prtFields.Add(l.LogNumber.PadLeft(3, ' '));
                prtFields.Add(l.Tree.Species.PadLeft(6, ' '));
                prtFields.Add(l.ExportGrade);
                prtFields.Add(l.Grade);
                prtFields.Add(String.Format("{0,4:F0}", l.Length).PadLeft(4, ' '));
                prtFields.Add(String.Format("{0,3:F0}", l.SeenDefect).PadLeft(3, ' '));

                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            return;
        }   //  end CreateEX1

        private int CreateEX2(StreamWriter strWriteOut, ref int pageNumb)
        {
            //  need export data for comparison and flags for the footer and errors
            bool errorFound = false;
            int footFlag = 0;
            double minDiam = 0;
            double minBDFT = 0;
            double minLength = 0;
            double maxDefect = 0;
            string currExSort = "";
            string currExGrade = "";
            List<exportGrades> exList = DataLayer.GetExportGrade();
            //  loop through log stock and compare values to defaults to find errors
            foreach (LogStockDO lsl in logStockList)
            {
                //   find export grade in the export defaults
                int nthRow = exList.FindIndex(
                    delegate (exportGrades ex)
                    {
                        return ex.exportSort == lsl.ExportGrade;
                    });
                if (nthRow >= 0)
                {
                    currExSort = exList[nthRow].exportSort;
                    //  find log grade in export defaults for comparison
                    int mthRow = exList.FindIndex(
                        delegate (exportGrades ex)
                        {
                            return ex.exportGrade == lsl.Grade;
                        });
                    if (mthRow >= 0)
                    {
                        currExGrade = exList[mthRow].exportGrade;
                        //  compare values to get correct min or max
                        //  Small end diameter minimum
                        if (exList[nthRow].minDiam < exList[mthRow].minDiam)
                            minDiam = exList[mthRow].minDiam;
                        else minDiam = exList[nthRow].minDiam;
                        //  BDFT volume minimum
                        if (exList[nthRow].minBDFT < exList[mthRow].minBDFT)
                            minBDFT = exList[mthRow].minBDFT;
                        else minBDFT = exList[nthRow].minBDFT;
                        //  Length minimum
                        if (exList[nthRow].minLength < exList[mthRow].minLength)
                            minLength = exList[mthRow].minLength;
                        else minLength = exList[nthRow].minLength;
                        //  Defect maximum
                        if (exList[nthRow].maxDefect > exList[mthRow].maxDefect)
                            maxDefect = exList[mthRow].maxDefect;
                        else maxDefect = exList[nthRow].maxDefect;
                    }
                    else currExGrade = "";
                }
                else currExSort = "";

                //  is this log in error?
                //  check length
                if (lsl.Length < minLength)
                    errorFound = true;
                //  check diameter
                if (lsl.SmallEndDiameter < minDiam)
                    errorFound = true;
                //  check BDFT volume
                if (lsl.NetBoardFoot < minBDFT)
                    errorFound = true;
                //  check seen defect
                if (lsl.SeenDefect > maxDefect)
                    errorFound = true;
                //  check sort code for blank
                if (currExSort == "" || currExSort == " ")
                {
                    currExSort = "**";
                    footFlag = 1;
                    errorFound = true;
                }   //  endif
                //  check grade for blank
                if (currExGrade == "" || currExGrade == " ")
                {
                    currExGrade = "**";
                    footFlag = 1;
                    errorFound = true;
                }   //  endif
                //  load record into output list
                loadExportOutputList(lsl, currExSort, currExGrade, errorFound);
                errorFound = false;
            }   //  end foreach loop

            //  Write list
            writeExportList(strWriteOut, ref pageNumb, footFlag);
            if (errorFound)
                return -1;
            else return 1;
        }   //  end CreateEX2

        private void CreateEX3(StreamWriter strWriteOut, ref int pageNumb)
        {
            //  just a listing of the export grade default table
            List<exportGrades> exportList = DataLayer.GetExportGrade();
            //  because of the way this list is put together, need to loop through once to print export sort information
            //  then loop through again to print the export grade information
            foreach (exportGrades el in exportList)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                        reportHeaders.EX3columns, 8, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(el.exportSort);
                prtFields.Add("");      //  export grade not printed in this section
                prtFields.Add(el.exportCode);
                prtFields.Add(el.exportName.PadRight(9, ' '));
                prtFields.Add(String.Format("{0,2:F0}", el.minDiam).PadLeft(2, ' '));
                prtFields.Add(String.Format("{0,2:F0}", el.minLength).PadLeft(2, ' '));
                prtFields.Add(String.Format("{0,4:F0}", el.minBDFT).PadLeft(4, ' '));
                prtFields.Add(String.Format("{0,3:F0}", el.maxDefect).PadLeft(3, ' '));

                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop

            //  export grade information
            strWriteOut.WriteLine(reportConstants.longLine);
            foreach (exportGrades el in exportList)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                            reportHeaders.EX3columns, 8, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add("");      //  export sort not printed in this section
                prtFields.Add(el.exportGrade);
                prtFields.Add(el.exportCode);
                prtFields.Add(el.exportName.PadRight(9, ' '));
                prtFields.Add(String.Format("{0,2:F0}", el.minDiam).PadLeft(2, ' '));
                prtFields.Add(String.Format("{0,2:F0}", el.minLength).PadLeft(2, ' '));
                prtFields.Add(String.Format("{0,4:F0}", el.minBDFT).PadLeft(4, ' '));
                prtFields.Add(String.Format("{0,3:F0}", el.maxDefect).PadLeft(3, ' '));

                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            return;
        }   //  end CreateEX3

        private void CreateEX4and5(StreamWriter strWriteOut, ref int pageNumb, string currSP)
        {
            //  will use RegionalReports list for the list to output
            List<RegionalReports> listToOutput = new List<RegionalReports>();
            //  first need DIB classes
            List<LogStockDO> justDIBs = DataLayer.getLogDIBs();
            LoadLogDIBclasses(listToOutput, justDIBs);
            //  pull just sort and grade from logstock to finish column headers
            List<LogStockDO> justSorts = DataLayer.getLogSorts(currSP);
            //  finish column header with sort combinations
            completeHeader = createCompleteHeader(justSorts);
            numColumns = justSorts.Count;
            int nthColumn = 7;     // starts with seven because the listToOutput value starts with value7
            foreach (LogStockDO js in justSorts)
            {
                //  pull log records for loading
                List<LogStockDO> justLogs = DataLayer.getCutLogs(currSP, js.ExportGrade, js.Grade);
                LoadSortGradeData(justLogs, nthColumn, listToOutput);
                nthColumn++;
            }   //  end foreach loop
            //  write this group
            WriteCurrentGroup(strWriteOut, ref pageNumb, listToOutput);
            //  write total line
            outputTotalLine(strWriteOut, ref pageNumb, listToOutput);
            listToOutput.Clear();
            return;
        }   //  end CreateEX4and5

        private void CreateEX6orEX7(StratumDO currST, string currCU)
        {
            //  EX6 and EX7
            string currSpecies = "";
            List<PRODO> proList = DataLayer.getPRO();
            //  get correct acres
            double strAcres = Utilities.ReturnCorrectAcres(currST.Code, DataLayer, (long)currST.Stratum_CN);
            //  first pull unique sort and grade codes
            List<LogStockDO> justSorts = DataLayer.getLogSorts("");
            logStockList = DataLayer.getCutLogs();
            //  then loop through sort codes to get logs
            List<LogStockDO> justLogs = new List<LogStockDO>();
            foreach (LogStockDO js in justSorts)
            {
                //  pull logs for this group based on report
                if (currentReport == "EX6")
                {
                    justLogs = logStockList.FindAll(
                        delegate (LogStockDO l)
                        {
                            return l.Tree.Stratum.Code == currST.Code && l.Tree.CuttingUnit.Code == currCU &&
                                        l.ExportGrade == js.ExportGrade && l.Grade == js.Grade;
                        });
                }
                else if (currentReport == "EX7")
                {
                    justLogs = logStockList.FindAll(
                        delegate (LogStockDO l)
                        {
                            return l.Tree.Stratum.Code == currST.Code &&
                                l.ExportGrade == js.ExportGrade && l.Grade == js.Grade;
                        });
                }   //  endif
                //  sum values into output list
                double proratFactor = 0;
                double sumExpFac = 0;
                double sumGrossBDFT = 0;
                double sumGrossCUFT = 0;
                double sumNetBDFT = 0;
                double sumNetCUFT = 0;
                double sumDefect = 0;
                double sumLength = 0;
                foreach (LogStockDO jl in justLogs)
                {
                    //  find proration factor for the group
                    int nthRow = proList.FindIndex(
                        delegate (PRODO p)
                        {
                            return p.Stratum == jl.Tree.Stratum.Code && p.CuttingUnit == jl.Tree.CuttingUnit.Code &&
                                                p.SampleGroup == jl.Tree.SampleGroup.Code;
                        });
                    if (nthRow >= 0)
                        proratFactor = proList[nthRow].ProrationFactor;
                    else proratFactor = 1.0;
                    //  save species
                    currSpecies = jl.Tree.Species;
                    //  Board foot volumes
                    sumGrossBDFT += jl.GrossBoardFoot * jl.Tree.ExpansionFactor * proratFactor;
                    sumNetBDFT += jl.NetBoardFoot * jl.Tree.ExpansionFactor * proratFactor;
                    //  Cubic foot volumes
                    sumGrossCUFT += jl.GrossCubicFoot * jl.Tree.ExpansionFactor * proratFactor;
                    sumNetCUFT += jl.NetCubicFoot * jl.Tree.ExpansionFactor * proratFactor;
                    //  these values are summed differently for 100% method
                    if (currST.Method == "100")
                    {
                        sumExpFac += jl.Tree.ExpansionFactor;
                        sumDefect += jl.SeenDefect;
                        sumLength += jl.Length * jl.Tree.ExpansionFactor;
                    }
                    else
                    {
                        sumExpFac += jl.Tree.ExpansionFactor * strAcres;
                        sumDefect += jl.SeenDefect * jl.Tree.ExpansionFactor * strAcres;
                        sumLength += jl.Length * jl.Tree.ExpansionFactor * strAcres;
                    }   //  endif on method
                }   //  end foreach loop
                //  load group into output list
                exportToOutput eto = new exportToOutput();
                eto.export1 = currST.Code.PadLeft(2, ' ');
                eto.export2 = currCU.PadLeft(3, ' ');
                eto.export3 = currSpecies.PadRight(6, ' ');
                eto.export4 = js.ExportGrade;
                eto.export5 = js.Grade;
                eto.export11 = sumExpFac;
                eto.export12 = sumGrossBDFT;
                eto.export13 = sumNetBDFT;
                eto.export14 = sumGrossCUFT;
                eto.export15 = sumNetCUFT;
                eto.export16 = sumDefect;
                eto.export17 = sumLength;

                exToOutput.Add(eto);
            }   //  end foreach loop
            return;
        }   //  end CreateEX6

        private void updateSubtotal(List<ReportSubtotal> totalToUpdate, string currCode)
        {
            if (totalToUpdate.Count == 0)
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value1 = currCode;
                rs.Value3 = exToOutput.Sum(ex => ex.export11);
                rs.Value4 = exToOutput.Sum(ex => ex.export12);
                rs.Value5 = exToOutput.Sum(ex => ex.export13);
                rs.Value6 = exToOutput.Sum(ex => ex.export14);
                rs.Value7 = exToOutput.Sum(ex => ex.export15);
                rs.Value8 = exToOutput.Sum(ex => ex.export16);
                rs.Value9 = exToOutput.Sum(ex => ex.export17);
                totalToUpdate.Add(rs);
            }
            else
            {
                totalToUpdate[0].Value3 += exToOutput.Sum(ex => ex.export11);
                totalToUpdate[0].Value4 += exToOutput.Sum(ex => ex.export12);
                totalToUpdate[0].Value5 += exToOutput.Sum(ex => ex.export13);
                totalToUpdate[0].Value6 += exToOutput.Sum(ex => ex.export14);
                totalToUpdate[0].Value7 += exToOutput.Sum(ex => ex.export15);
                totalToUpdate[0].Value8 += exToOutput.Sum(ex => ex.export16);
                totalToUpdate[0].Value9 += exToOutput.Sum(ex => ex.export17);
            }   //  endif
            return;
        }   //  end updateSubtotal

        private void LoadSortGradeData(List<LogStockDO> justLogs, int nthColumn, List<RegionalReports> listToOutput)
        {
            //  EX4 and EX5
            int volFactor = 100;
            if (currentReport == "EX4")
                volFactor = 1000;

            double strAcres = 0;
            string currST = "**";
            double currNET = 0;
            //  loop through logs and accumulate appropriate volume
            foreach (LogStockDO jl in justLogs)
            {
                if (currST != jl.Tree.Stratum.Code)
                {
                    strAcres = Utilities.ReturnCorrectAcres(jl.Tree.Stratum.Code, DataLayer, (long)jl.Tree.Stratum_CN);
                    currST = jl.Tree.Stratum.Code;
                }   //  endif different stratum

                //  find row to load based on small end diameter
                int nthRow = FindDIBindex(jl.SmallEndDiameter, listToOutput);
                if (currentReport == "EX4")
                    currNET = jl.NetBoardFoot;
                else if (currentReport == "EX5")
                    currNET = jl.NetCubicFoot;
                currNET += currNET * jl.Tree.ExpansionFactor * strAcres;
                //  load net volume into appropriate row and column
                switch (nthColumn)
                {
                    case 7:
                        listToOutput[nthRow].value7 += currNET / volFactor;
                        break;

                    case 8:
                        listToOutput[nthRow].value8 += currNET / volFactor;
                        break;

                    case 9:
                        listToOutput[nthRow].value9 += currNET / volFactor;
                        break;

                    case 10:
                        listToOutput[nthRow].value10 += currNET / volFactor;
                        break;

                    case 11:
                        listToOutput[nthRow].value11 += currNET / volFactor;
                        break;

                    case 12:
                        listToOutput[nthRow].value12 += currNET / volFactor;
                        break;

                    case 13:
                        listToOutput[nthRow].value13 += currNET / volFactor;
                        break;

                    case 14:
                        listToOutput[nthRow].value14 += currNET / volFactor;
                        break;

                    case 15:
                        listToOutput[nthRow].value15 += currNET / volFactor;
                        break;

                    case 16:
                        listToOutput[nthRow].value16 += currNET / volFactor;
                        break;

                    case 17:
                        listToOutput[nthRow].value17 += currNET / volFactor;
                        break;
                }   //  end switch on column
            }   //  end foreach loop
            return;
        }   //  end LoadSortGradeData

        private void writeExportList(StreamWriter strWriteOut, ref int pageNumb, int footerFlag)
        {
            //  EX2
            foreach (exportToOutput eto in exToOutput)
            {
                if (eto.errorFound)
                {
                    WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                        completeHeader, 10, ref pageNumb, "");
                    prtFields.Clear();
                    prtFields.Add("");
                    prtFields.Add(eto.export1.PadLeft(2, ' '));         //  stratum
                    prtFields.Add(eto.export2.PadLeft(4, ' '));         //  cutting unit
                    prtFields.Add(eto.export3.PadLeft(4, ' '));         //  plot
                    prtFields.Add(eto.export4.PadLeft(3, ' '));         //  tree number
                    prtFields.Add(eto.export5.PadLeft(3, ' '));         //  log number
                    prtFields.Add(eto.export6.PadRight(6, ' '));        //  species
                    prtFields.Add(eto.export7);                         //  log sort
                    prtFields.Add(eto.export8);                         //  log grade
                    // log length and percent defect
                    prtFields.Add(String.Format("{0,4:F0}", eto.export11).PadLeft(4, ' '));
                    prtFields.Add(String.Format("{0,3:F0}", eto.export12).PadLeft(3, ' '));
                    //  edit results
                    prtFields.Add(eto.export9.PadLeft(2, ' '));
                    prtFields.Add(eto.export10.PadLeft(2, ' '));
                    prtFields.Add(String.Format("{0,4:F0}", eto.export13).PadLeft(4, ' '));
                    prtFields.Add(String.Format("{0,4:F0}", eto.export14).PadLeft(4, ' '));
                    prtFields.Add(String.Format("{0,4:F0}", eto.export15).PadLeft(4, ' '));
                    prtFields.Add(String.Format("{0,3:F1}", eto.export16).PadLeft(3, ' '));

                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  endif error
            }   //  end foreach loop
            //  output footer if needed
            if (footerFlag == 1)
            {
                strWriteOut.WriteLine("");
                strWriteOut.WriteLine(reportHeaders.EXfooter);
            }   //  endif footerflag

            return;
        }   //  end writeExportList

        private void WriteCurrentGroup(StreamWriter strWriteOut, ref int pageNumb, List<RegionalReports> listToOutput)
        {
            //  EX4 and EX5
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                    completeHeader, 12, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(lto.value1.PadLeft(2, ' '));
                //  will need something in here to suppress column combination not used
                switch (numColumns)
                {
                    case 11:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value8).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value9).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value10).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value11).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value12).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value13).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value14).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value15).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value16).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value17).PadLeft(6, ' '));
                        break;

                    case 10:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value8).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value9).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value10).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value11).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value12).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value13).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value14).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value15).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value16).PadLeft(6, ' '));
                        break;

                    case 9:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value8).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value9).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value10).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value11).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value12).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value13).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value14).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value15).PadLeft(6, ' '));
                        break;

                    case 8:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value8).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value9).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value10).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value11).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value12).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value13).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value14).PadLeft(6, ' '));
                        break;

                    case 7:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value8).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value9).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value10).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value11).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value12).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value13).PadLeft(6, ' '));
                        break;

                    case 6:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value8).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value9).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value10).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value11).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value12).PadLeft(6, ' '));
                        break;

                    case 5:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value8).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value9).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value10).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value11).PadLeft(6, ' '));
                        break;

                    case 4:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value8).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value9).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value10).PadLeft(6, ' '));
                        break;

                    case 3:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value8).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value9).PadLeft(6, ' '));
                        break;

                    case 2:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        prtFields.Add(String.Format("{0,6:F2}", lto.value8).PadLeft(6, ' '));
                        break;

                    case 1:
                        prtFields.Add(String.Format("{0,6:F2}", lto.value7).PadLeft(6, ' '));
                        break;
                }
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            return;
        }   //  end WriteCurrentGroup

        private void outputTotalLine(StreamWriter strWriteOut, ref int pageNumb, List<RegionalReports> listToOutput)
        {
            //  not sure what this is supposed to look like until I get some test data -- February 2014
            return;
        }   //  end outputTotalLine

        private void outputSubtotals(StreamWriter strWriteOut, ref int pageNumb, List<ReportSubtotal> totalToOutput,
                                    string whichSubtotal)
        {
            //  EX6 and EX7
            WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                completeHeader, 10, ref pageNumb, "");
            switch (whichSubtotal)
            {
                case "U":           //  unit subtotal
                    strWriteOut.WriteLine("                                  ____________________________________________________________");
                    strWriteOut.Write(" UNIT ");
                    strWriteOut.Write(totalToOutput[0].Value1.PadLeft(3, ' '));
                    strWriteOut.Write(" TOTALS                            ");
                    break;

                case "S":           //  strata subtotal
                    strWriteOut.WriteLine("                                  ____________________________________________________________");
                    strWriteOut.Write(" STRATUM ");
                    strWriteOut.Write(totalToOutput[0].Value1.PadLeft(2, ' '));
                    strWriteOut.Write(" TOTALS                           ");
                    break;

                case "T":           //  overall total
                    strWriteOut.WriteLine(reportConstants.longLine);
                    strWriteOut.WriteLine(reportConstants.longLine);
                    strWriteOut.Write(totalToOutput[0].Value1.PadLeft(8, ' '));
                    strWriteOut.Write(" TOTALS ---                       ");
                    break;
            }   //  end switch
            strWriteOut.Write(String.Format("{0,4:F0}", totalToOutput[0].Value8).PadLeft(4, ' '));
            //  board foot volume
            strWriteOut.Write(String.Format("{0,7:F0}", totalToOutput[0].Value9).PadLeft(7, ' '));
            strWriteOut.Write(String.Format("{0,7:F0}", totalToOutput[0].Value10).PadLeft(7, ' '));
            //  cubic foot volume
            strWriteOut.Write(String.Format("{0,6:F0}", totalToOutput[0].Value11).PadLeft(6, ' '));
            strWriteOut.Write(String.Format("{0,6:F0}", totalToOutput[0].Value12).PadLeft(6, ' '));
            //  average defect and length
            if (totalToOutput[0].Value8 > 0)
            {
                strWriteOut.Write(String.Format("{0,3:F0}", totalToOutput[0].Value13 / totalToOutput[0].Value8).PadLeft(3, ' '));
                strWriteOut.WriteLine(String.Format("{0,4:F0}", totalToOutput[0].Value14 / totalToOutput[0].Value8).PadLeft(4, ' '));
            }   //  endif
            // blank lines
            if (whichSubtotal == "U")
                strWriteOut.WriteLine("");
            return;
        }   //  end outputSubtotals

        private void WriteUnitGroup(StreamWriter strWriteOut, ref int pageNumb)
        {
            //  EX6 or EX7
            foreach (exportToOutput eto in exToOutput)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                        completeHeader, 10, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(eto.export1);
                if (currentReport == "EX6") prtFields.Add(eto.export2);
                prtFields.Add(eto.export3);
                prtFields.Add(eto.export4);
                prtFields.Add(eto.export5);
                //  number of logs
                prtFields.Add(String.Format("{0,4:F0}", eto.export11).PadLeft(4, ' '));
                //  board foot volume
                prtFields.Add(String.Format("{0,7:F0}", eto.export12).PadLeft(7, ' '));
                prtFields.Add(String.Format("{0,7:F0}", eto.export13).PadLeft(7, ' '));
                //  cubic foot volume
                prtFields.Add(String.Format("{0,6:F0}", eto.export14).PadLeft(6, ' '));
                prtFields.Add(String.Format("{0,6:F0}", eto.export15).PadLeft(6, ' '));
                if (eto.export11 > 0)
                {
                    //  average defect
                    prtFields.Add(String.Format("{0,3:F0}", eto.export16 / eto.export11).PadLeft(3, ' '));
                    //  average length
                    prtFields.Add(String.Format("{0,4:F0}", eto.export17 / eto.export11).PadLeft(4, ' '));
                }   //  endif
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            return;
        }   //  end WriteUnitGroup

        private void loadExportOutputList(LogStockDO justLog, string currSort, string currGrade, bool errFound)
        {
            //  EX2
            exportToOutput eto = new exportToOutput();
            eto.errorFound = errFound;
            eto.export1 = justLog.Tree.Stratum.Code;
            eto.export2 = justLog.Tree.CuttingUnit.Code;
            eto.export3 = justLog.Tree.Plot.PlotNumber.ToString();
            eto.export4 = justLog.Tree.TreeNumber.ToString();
            eto.export5 = justLog.LogNumber;
            eto.export6 = justLog.Tree.Species;
            eto.export7 = justLog.ExportGrade;
            eto.export8 = justLog.Grade;
            eto.export9 = currSort;
            eto.export10 = currGrade;
            eto.export11 = justLog.Length;
            eto.export12 = justLog.SeenDefect;
            eto.export13 = justLog.Length; ;
            eto.export14 = justLog.SmallEndDiameter;
            eto.export15 = justLog.NetBoardFoot;
            eto.export16 = justLog.SeenDefect;
            exToOutput.Add(eto);
            return;
        }   //  end loadExportOutputList

        private string[] createCompleteHeader()
        {
            //  currently used for EX2, EX6 and EX7
            string[] headerEX2 = new string[2];
            string[] headerEX6or7 = new string[3];
            switch (currentReport)
            {
                case "EX2":
                    headerEX2[0] = reportHeaders.EX1EX2left[0];
                    headerEX2[0] += reportHeaders.EX2right[0];
                    headerEX2[1] = reportHeaders.EX1EX2left[1];
                    headerEX2[1] += reportHeaders.EX2right[1];
                    return headerEX2;

                case "EX6":
                    headerEX6or7[0] = reportHeaders.EX6EX7right[0];
                    headerEX6or7[1] = reportHeaders.EX6EX7right[1];
                    headerEX6or7[2] = "  STRATA  UNIT  ";
                    headerEX6or7[2] += reportHeaders.EX6EX7right[2];
                    return headerEX6or7;

                case "EX7":
                    headerEX6or7[0] = reportHeaders.EX6EX7right[0];
                    headerEX6or7[1] = reportHeaders.EX6EX7right[1];
                    headerEX6or7[2] = "  STRATA  ";
                    headerEX6or7[2] += reportHeaders.EX6EX7right[2];
                    return headerEX6or7;
            }   //  end switch on report
            return headerEX2;
        }   //  end createCompleteHeader

        private string[] createCompleteHeader(List<LogStockDO> justSorts)
        {
            //  EX4
            string[] finnishHeader = new string[3];
            finnishHeader[0] = reportHeaders.EX4columns[0];
            finnishHeader[1] = reportHeaders.EX4columns[1];
            finnishHeader[2] = reportHeaders.EX4columns[2];
            foreach (LogStockDO js in justSorts)
            {
                //  append sort/grade combination to last header line
                finnishHeader[2] += js.ExportGrade;
                finnishHeader[2] += " ";
                finnishHeader[2] += js.Grade;
                finnishHeader[2] += "    ";
            }   //  end foreach loop
            return finnishHeader;
        }   //  end createCompleteHeader

        protected static int FindDIBindex(float SmEndDiam, List<RegionalReports> listToOutput)
        {
            string DIBtoFind = (Math.Floor(SmEndDiam + 0.5)).ToString();
            int rowToLoad = listToOutput.FindIndex(
                delegate (RegionalReports rr)
                {
                    return rr.value1 == DIBtoFind;
                });
            if (rowToLoad < 0)
                rowToLoad = 0;
            return rowToLoad;
        }   //  end FindDIBindex

        public class exportToOutput
        {
            public bool errorFound { get; set; }
            public string export1 { get; set; }
            public string export2 { get; set; }
            public string export3 { get; set; }
            public string export4 { get; set; }
            public string export5 { get; set; }
            public string export6 { get; set; }
            public string export7 { get; set; }
            public string export8 { get; set; }
            public string export9 { get; set; }
            public string export10 { get; set; }
            public double export11 { get; set; }
            public double export12 { get; set; }
            public double export13 { get; set; }
            public double export14 { get; set; }
            public double export15 { get; set; }
            public double export16 { get; set; }
            public double export17 { get; set; }
        }   //  end exportToOutput
    }
}