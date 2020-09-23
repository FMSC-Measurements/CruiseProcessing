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
    public class OutputLogStock : CreateTextFile
    {
        #region
        public string currentReport;
        public List<ReportSubtotal> ListToOutput = new List<ReportSubtotal>();
        public List<ReportSubtotal> ListToOutputCCF = new List<ReportSubtotal>();
        public List<ReportSubtotal> ListToOutputMBF = new List<ReportSubtotal>();
        private int[] fieldLengths;
        private ArrayList prtFields = new ArrayList();
        private string currSP = "*";
        private double strAcres = 0;
        private int nthRow = 0;
        private int tableNum = 0;
        private StringBuilder extraHeadingLine = new StringBuilder();
        private double currEF = 0;
        private double currCubic = 0;
        private double currBoard = 0;
        public List<LogStockDO> logStockList = new List<LogStockDO>();
        #endregion

        public void CreateLogReports(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  Fill report title array
            string currentTitle = fillReportTitle(currentReport);
            //  Get log stock by species
            List<LogStockDO> justCutLogs = bslyr.getCutLogs();
            //  no log stock records?  no report
            if (justCutLogs.Count == 0)
            {
                noDataForReport(strWriteOut, currentReport, " >>>> No log stock records for report");
                return;
            }   //  endif testVolume


            //  Need to load DIB classes 
            ListToOutput.Clear();
            loadDIBclasses(justCutLogs, ListToOutput);
            if (currentReport == "L10")
            {
                //  load DIBs into other two lists
                loadDIBclasses(justCutLogs, ListToOutputCCF);
                loadDIBclasses(justCutLogs, ListToOutputMBF);
            }   //  endif currentReport


            List<StratumDO> sList = bslyr.getStratum();
            //  Then load and print ListToOutput based on report
            numOlines = 0;
            switch (currentReport)
            {
                case "L2":
                    fieldLengths = new int[] { 1, 7, 10, 10, 10, 10, 10, 10, 10, 10, 11, 11, 11, 10 };
                    rh.createReportTitle(currentTitle, 1, 0, 0, "", "");
                    LoadAndPrintByGrade(justCutLogs, sList, strWriteOut, rh, ref pageNumb);
                    break;
                case "L8":
                    fieldLengths = new int[] { 1, 7, 9, 12, 12, 12, 12, 9, 12, 12, 9, 12, 11 };
                    rh.createReportTitle(currentTitle, 1, 0, 0, "", "");
                    LoadAndPrintByProduct(justCutLogs, sList, strWriteOut, rh, ref pageNumb);
                    break;
                case "L10":
                    fieldLengths = new int[] { 1, 6, 2, 7, 2, 7, 2, 7, 2, 7, 2, 7, 2, 7, 2, 7, 2, 7, 2, 7, 2, 7, 2, 7, 2, 7, 2, 7, 2, 6 };
                    string joinedLine = reportConstants.FCTO;
                    joinedLine += " - ";
                    joinedLine += reportConstants.B1DC;
                    rh.createReportTitle(currentTitle, 6, 0, 0, joinedLine, reportConstants.oneInchDC);
                    LoadAndPrintByLength(justCutLogs, sList, strWriteOut, ref pageNumb, rh);
                    break;
            }   //  end switch
        }   //  end CreateLogReports

        private void LoadAndPrintByGrade(List<LogStockDO> listToLoad, List<StratumDO> sList,
                                        StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  load list for each species
            double calcGross = 0;
            double calcNet = 0;
            tableNum = 1;
            currSP = "*";
            foreach (LogStockDO ls in listToLoad)
            {
                if (currSP == "*")
                    currSP = ls.Tree.Species;
                else if (currSP != ls.Tree.Species)
                {
                    //  output table for current species
                    extraHeadingLine = LoadExtraHeading(0);
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                            rh.L2columns, 11, ref pageNumb, extraHeadingLine.ToString());
                    writeCurrentGroup(strWriteOut, ref pageNumb, rh, "{0,9:F2}", "{0,10:F2}", ListToOutput, 1000);
                    //  clear ListToOutput for next species
                    clearOutputList(ListToOutput);
                    currSP = ls.Tree.Species;
                    tableNum++;
                    numOlines = 0;
                }   //  endif
                //  load by log grade
                //  find row for dib class
                nthRow = FindDIBindex(ListToOutput, ls.SmallEndDiameter);
                //  need stratum acres
                strAcres = Utilities.ReturnCorrectAcres(ls.Tree.Stratum.Code, bslyr, (long)ls.Tree.Stratum_CN);
                calcGross = ls.GrossBoardFoot * ls.Tree.ExpansionFactor * strAcres;
                calcNet = ls.NetBoardFoot * ls.Tree.ExpansionFactor * strAcres;
                switch (ls.Grade)
                {
                    case "0":
                    default:
                        ListToOutput[nthRow].Value3 += calcNet;
                        break;
                    case "1":
                        ListToOutput[nthRow].Value4 += calcNet;
                        break;
                    case "2":
                        ListToOutput[nthRow].Value5 += calcNet;
                        break;
                    case "3":
                        ListToOutput[nthRow].Value6 += calcNet;
                        break;
                    case "4":
                        ListToOutput[nthRow].Value7 += calcNet;
                        break;
                    case "5":
                        ListToOutput[nthRow].Value8 += calcNet;
                        break;
                    case "6":
                        ListToOutput[nthRow].Value9 += calcNet;
                        break;
                    case "7":
                        ListToOutput[nthRow].Value10 += calcNet;
                        break;
                    case "8":
                    case "9":
                        ListToOutput[nthRow].Value12 += calcNet;
                        //  Total cull log
                        ListToOutput[nthRow].Value14 += calcNet;
                        break;
                }   //  end switch on log grade
                //  Load total net volume
                ListToOutput[nthRow].Value11 += calcNet;
                //  Calculate and load defect column
                if(ls.Grade != "8" && ls.Grade != "9")
                    ListToOutput[nthRow].Value13 += calcGross - calcNet;
                //  And total gross
                ListToOutput[nthRow].Value14 += calcNet;
                ListToOutput[nthRow].Value14 += calcGross - calcNet;
            }   //  end foreach loop
            //  print last species group
            //  output table for current species
            extraHeadingLine = LoadExtraHeading(0);
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    rh.L2columns, 11, ref pageNumb, extraHeadingLine.ToString());
            writeCurrentGroup(strWriteOut, ref pageNumb, rh, "{0,9:F2}", "{0,10:F2}", ListToOutput, 1000);
            return;
        }   //  end LoadAndPrintByGrade

        private void LoadAndPrintByProduct(List<LogStockDO> listToLoad, List<StratumDO> sList,
                                            StreamWriter strWriteOut,reportHeaders rh,
                                            ref int pageNumb)
        {
            //  load list by product
            tableNum = 1;
            currSP = "*";
            foreach(LogStockDO ls in listToLoad)
            {
                if(currSP == "*")
                    currSP = ls.Tree.Species;
                else if(currSP != ls.Tree.Species)
                {
                    //  output table for current species
                    extraHeadingLine = LoadExtraHeading(0);
                    WriteReportHeading(strWriteOut,rh.reportTitles[0],rh.reportTitles[1],rh.reportTitles[2],
                                        rh.L8columns,11,ref pageNumb,extraHeadingLine.ToString());
                    writeCurrentGroup(strWriteOut, ref pageNumb, rh, "{0,8:F1}", "{0,11:F1}", ListToOutput, 1);
                    //  clear ListToOutput for next species
                    clearOutputList(ListToOutput);
                    currSP = ls.Tree.Species;
                    tableNum++;
                    numOlines = 0;
                }   // endif
                //  load by product
                //  find row for dib class
                nthRow = FindDIBindex(ListToOutput,ls.SmallEndDiameter);
                //  also need stratum acres
                strAcres = Utilities.ReturnCorrectAcres(ls.Tree.Stratum.Code, bslyr, (long)ls.Tree.Stratum_CN);
                //  grab expansion factor for calculations
                currEF = ls.Tree.ExpansionFactor;
                switch(ls.Grade)
                {
                    case "7":       //  topwood
                        ListToOutput[nthRow].Value8 += currEF * strAcres;
                        ListToOutput[nthRow].Value9 += ls.GrossCubicFoot * currEF * strAcres;
                        ListToOutput[nthRow].Value10 += ls.NetCubicFoot * currEF * strAcres;
                        break;
                    default:        //  all other grades go into primary
                        ListToOutput[nthRow].Value3 += currEF * strAcres;
                        ListToOutput[nthRow].Value4 += ls.GrossBoardFoot * currEF * strAcres;
                        ListToOutput[nthRow].Value5 += ls.NetBoardFoot * currEF * strAcres;
                        ListToOutput[nthRow].Value6 += ls.GrossCubicFoot * currEF * strAcres;
                        ListToOutput[nthRow].Value7 += ls.NetCubicFoot * currEF * strAcres;
                        break;
                }   //  end switch on grade
                 
                //  add to total columns
                ListToOutput[nthRow].Value11 += currEF * strAcres;
                ListToOutput[nthRow].Value12 += ls.GrossCubicFoot * currEF * strAcres;
                ListToOutput[nthRow].Value13 += ls.NetCubicFoot * currEF * strAcres;
            }   //  end foreach loop
            //  print last species group
            //  output table for current species
            extraHeadingLine = LoadExtraHeading(0);
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                rh.L8columns, 11, ref pageNumb, extraHeadingLine.ToString());
            writeCurrentGroup(strWriteOut, ref pageNumb, rh, "{0,8:F1}", "{0,11:F1}", ListToOutput, 1);
            return;
        }   //  end LoadAndPrintByProduct


        private void LoadAndPrintByLength(List<LogStockDO> listToLoad, List<StratumDO> sList,
                                            StreamWriter strWriteOut, ref int pageNumb,
                                            reportHeaders rh)
        {
            //  load lists for each species
            foreach (LogStockDO ls in listToLoad)
            {
                if (currSP == "*")
                    currSP = ls.Tree.Species;
                else if (currSP != ls.Tree.Species)
                {
                    //  output tables for current species
                    //  log counts
                    extraHeadingLine = LoadExtraHeading(1);
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                        rh.L10columns, 17, ref pageNumb, extraHeadingLine.ToString());
                    writeCurrentGroup(strWriteOut, ref pageNumb, rh, "{0,5:F0}", "{0,6:F0}",ListToOutput,1);
                    //  CCF list
                    if (ListToOutputCCF.Sum(l => l.Value16) > 0)
                    {
                        extraHeadingLine = LoadExtraHeading(2);
                        WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                            rh.L10columns, 17, ref pageNumb, extraHeadingLine.ToString());
                        writeCurrentGroup(strWriteOut, ref pageNumb, rh, "{0,5:F0}", "{0,6:F0}", ListToOutputCCF, 100);
                    }   //  endif CCF to print
                    //  MBF list
                    if (ListToOutputMBF.Sum(l => l.Value16) > 0)
                    {
                        extraHeadingLine = LoadExtraHeading(3);
                        WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                            rh.L10columns, 17, ref pageNumb, extraHeadingLine.ToString());
                        writeCurrentGroup(strWriteOut, ref pageNumb, rh, "{0,5:F0}", "{0,6:F0}", ListToOutputMBF, 1000);
                    }   //  endif MBF to print
                    //  clear output lists for next species
                    clearOutputList(ListToOutput);
                    clearOutputList(ListToOutputCCF);
                    clearOutputList(ListToOutputMBF);
                    currSP = ls.Tree.Species;
                    tableNum++;
                    numOlines = 0;
                }   //  endif
                // load by length
                int noLength = 1;
                //  find row for dib class
                nthRow = FindDIBindex(ListToOutput, ls.SmallEndDiameter);
                //  also need stratum acres
                strAcres = Utilities.ReturnCorrectAcres(ls.Tree.Stratum.Code, bslyr, (long)ls.Tree.Stratum_CN);
                //  and grab expansion for calculations
                currEF = ls.Tree.ExpansionFactor;
                //  grab needed values for each page
                currCubic = ls.NetCubicFoot;
                currBoard = ls.NetBoardFoot;
                if (ls.Grade != "7" && ls.Grade != "8" && ls.Grade != "9")
                {
                    switch (ls.Length)
                    {
                        case 2:
                            noLength = 0;
                            break;
                        case 4:
                            noLength = 0;
                            break;
                        case 6:
                            noLength = 0;
                            break;
                        case 8:
                            ListToOutput[nthRow].Value3 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value3 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value3 += currBoard * currEF * strAcres;
                            break;
                        case 10:
                            ListToOutput[nthRow].Value4 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value4 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value4 += currBoard * currEF * strAcres;
                            break;
                        case 12:
                            ListToOutput[nthRow].Value5 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value5 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value5 += currBoard * currEF * strAcres;
                            break;
                        case 14:
                            ListToOutput[nthRow].Value6 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value6 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value6 += currBoard * currEF * strAcres;
                            break;
                        case 16:
                            ListToOutput[nthRow].Value7 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value7 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value7 += currBoard * currEF * strAcres;
                            break;
                        case 18:
                            ListToOutput[nthRow].Value8 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value8 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value8 += currBoard * currEF * strAcres;
                            break;
                        case 20:
                            ListToOutput[nthRow].Value9 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value9 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value9 += currBoard * currEF * strAcres;
                            break;
                        case 22:
                            ListToOutput[nthRow].Value10 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value10 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value10 += currBoard * currEF * strAcres;
                            break;
                        case 24:
                            ListToOutput[nthRow].Value11 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value11 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value11 += currBoard * currEF * strAcres;
                            break;
                        case 26:
                            ListToOutput[nthRow].Value12 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value12 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value12 += currBoard * currEF * strAcres;
                            break;
                        case 28:
                            ListToOutput[nthRow].Value13 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value13 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value13 += currBoard * currEF * strAcres;
                            break;
                        case 30:
                            ListToOutput[nthRow].Value14 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value14 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value14 += currBoard * currEF * strAcres;
                            break;
                        case 32:
                            ListToOutput[nthRow].Value15 += currEF * strAcres;
                            ListToOutputCCF[nthRow].Value15 += currCubic * currEF * strAcres;
                            ListToOutputMBF[nthRow].Value15 += currBoard * currEF * strAcres;
                            break;
                    }   //  end switch on length
                    //  add to total column
                    if (noLength == 1)
                    {
                        ListToOutput[nthRow].Value16 += currEF * strAcres;
                        ListToOutputCCF[nthRow].Value16 += currCubic * currEF * strAcres;
                        ListToOutputMBF[nthRow].Value16 += currBoard * currEF * strAcres;
                    }   //  endif no Length
                }   //  endif on grade
            }   //  end foreach loop
            //  print last species
            //  log counts
            extraHeadingLine = LoadExtraHeading(1);
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                rh.L10columns, 17, ref pageNumb, extraHeadingLine.ToString());
            writeCurrentGroup(strWriteOut, ref pageNumb, rh, "{0,5:F0}", "{0,6:F0}", ListToOutput, 1);
            //  CCF list
            if (ListToOutputCCF.Sum(l => l.Value16) > 0)
            {
                extraHeadingLine = LoadExtraHeading(2);
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    rh.L10columns, 17, ref pageNumb, extraHeadingLine.ToString());
                writeCurrentGroup(strWriteOut, ref pageNumb, rh, "{0,5:F0}", "{0,6:F0}", ListToOutputCCF, 100);
            }   //  endif CCF to print
            //  MBF list
            if (ListToOutputMBF.Sum(l => l.Value16) > 0)
            {
                extraHeadingLine = LoadExtraHeading(3);
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    rh.L10columns, 17, ref pageNumb, extraHeadingLine.ToString());
                writeCurrentGroup(strWriteOut, ref pageNumb, rh, "{0,5:F0}", "{0,6:F0}", ListToOutputMBF, 1000);
            }   //  endif MBF to print
            return;
        }   //  end LoadAndPrintByLength


        private void writeCurrentGroup(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh, 
                                        string formatOne, string formatTwo, List<ReportSubtotal> outputList,
                                        float convFactor)
        {
            //  print lines for each report
            string verticalBar = "|";
            foreach (ReportSubtotal ol in outputList)
            {
                prtFields.Clear();
                prtFields.Add("");
                if (currentReport == "L2" || currentReport == "L8")
                    prtFields.Add(ol.Value1.PadLeft(6, ' '));
                else if (currentReport == "L10")
                {
                    prtFields.Add(ol.Value1.PadLeft(4, ' '));
                    prtFields.Add(verticalBar);
                }   //  endif

                switch (currentReport)
                {
                    case "L2":
                        prtFields.Add(Utilities.FormatField(ol.Value3 / convFactor, formatOne).ToString().PadLeft(9, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value4 / convFactor, formatOne).ToString().PadLeft(9, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value5 / convFactor, formatOne).ToString().PadLeft(9, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value6 / convFactor, formatOne).ToString().PadLeft(9, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value7 / convFactor, formatOne).ToString().PadLeft(9, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value8 / convFactor, formatOne).ToString().PadLeft(9, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value9 / convFactor, formatOne).ToString().PadLeft(9, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value10 / convFactor, formatOne).ToString().PadLeft(9, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value11 / convFactor, formatTwo).ToString().PadLeft(10, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value12 / convFactor, formatTwo).ToString().PadLeft(10, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value13 / convFactor, formatTwo).ToString().PadLeft(10, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value14 / convFactor, formatTwo).ToString().PadLeft(10, ' '));
                        if (ol.Value14 > 0)
                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                        break;
                    case "L8":
                        //  primary product
                        prtFields.Add(Utilities.FormatField(ol.Value3, formatOne).ToString().PadLeft(8, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value4, formatTwo).ToString().PadLeft(11, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value5, formatTwo).ToString().PadLeft(11, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value6, formatTwo).ToString().PadLeft(11, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value7, formatTwo).ToString().PadLeft(11, ' '));
                        //  secondary product
                        prtFields.Add(Utilities.FormatField(ol.Value8, formatOne).ToString().PadLeft(8, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value9, formatTwo).ToString().PadLeft(11, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value10, formatTwo).ToString().PadLeft(11, ' '));
                        //  total cubic foot
                        prtFields.Add(Utilities.FormatField(ol.Value11, formatOne).ToString().PadLeft(8, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value12, formatTwo).ToString().PadLeft(11, ' '));
                        prtFields.Add(Utilities.FormatField(ol.Value13, formatTwo).ToString().PadLeft(11, ' '));
                        if (ol.Value11 > 0)
                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                        break;
                    case "L10":
                        prtFields.Add(Utilities.FormatField(ol.Value3 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value4 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value5 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value6 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value7 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value8 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value9 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value10 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value11 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value12 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value13 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value14 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value15 / convFactor, formatOne).ToString().PadLeft(5, ' '));
                        prtFields.Add(verticalBar);
                        prtFields.Add(Utilities.FormatField(ol.Value16 / convFactor, formatTwo).ToString().PadLeft(6, ' '));
                        if (ol.Value16 > 0)
                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                        break;
                }   //  end switch on report

            }   //  end foreach loop

            //  output total
            UpdateAndOutputTotal(outputList, strWriteOut, ref pageNumb, convFactor, formatOne, formatTwo);
            return;
        }   //  end writeCurrentGroup



        private void UpdateAndOutputTotal(List<ReportSubtotal> outputList,StreamWriter strWriteOut,
                                            ref int pageNumb,float convFactor,string formatOne, string formatTwo)
        {
            string verticalBar = "|";
            ArrayList pageTotals = new ArrayList();
            pageTotals.Clear();
            pageTotals.Add("");
            pageTotals.Add("TOTALS");
            if(currentReport == "L10") pageTotals.Add(verticalBar);
            switch(currentReport)
            {
                case "L2":
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value3) / convFactor, formatOne)).ToString().PadLeft(9, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value4) / convFactor, formatOne)).ToString().PadLeft(9, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value5) / convFactor, formatOne)).ToString().PadLeft(9, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value6) / convFactor, formatOne)).ToString().PadLeft(9, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value7) / convFactor, formatOne)).ToString().PadLeft(9, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value8) / convFactor, formatOne)).ToString().PadLeft(9, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value9) / convFactor, formatOne)).ToString().PadLeft(9, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value10) / convFactor, formatOne)).ToString().PadLeft(9, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value11) / convFactor, formatTwo)).ToString().PadLeft(10, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value12) / convFactor, formatTwo)).ToString().PadLeft(10, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value13) / convFactor, formatTwo)).ToString().PadLeft(10, ' ');
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value14) / convFactor, formatTwo)).ToString().PadLeft(10, ' ');
                    break;
                case "L8":
                    //  primary product
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value3), formatOne).ToString().PadLeft(8, ' '));
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value4), formatTwo).ToString().PadLeft(11, ' '));
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value5), formatTwo).ToString().PadLeft(11, ' '));
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value6), formatTwo).ToString().PadLeft(11, ' '));
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value7), formatTwo).ToString().PadLeft(11, ' '));
                    //  secondary product
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value8), formatOne).ToString().PadLeft(8, ' '));
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value9), formatTwo).ToString().PadLeft(11, ' '));
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value10), formatTwo).ToString().PadLeft(11, ' '));
                    //  total cubic foot
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value11), formatOne).ToString().PadLeft(8, ' '));
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value12), formatTwo).ToString().PadLeft(11, ' '));
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(l => l.Value13), formatTwo).ToString().PadLeft(11, ' '));
                    break;
            case "L10":
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value3) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value4) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value5) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value6) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value7) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value8) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value9) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value10) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value11) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value12) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value13) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value14) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value15) / convFactor, formatOne).ToString().PadLeft(5, ' '));
                    pageTotals.Add(verticalBar);
                    pageTotals.Add(Utilities.FormatField(outputList.Sum(o => o.Value16) / convFactor, formatTwo).ToString().PadLeft(6, ' '));
                    break;
            }   //  end switch
            strWriteOut.WriteLine(reportConstants.longLine);
            printOneRecord(fieldLengths, pageTotals, strWriteOut);
            numOlines = 0;
            return;
        }   //  end UpdateAndOutputTotal


        private StringBuilder LoadExtraHeading(int pageToLoad)
        {
            StringBuilder exhl = new StringBuilder();
            exhl.Remove(0, exhl.Length);
            switch (currentReport)
            {
                case "L2":
                case "L8":
                    exhl.Append(" TABLE ");
                    exhl.Append(tableNum);
                    exhl.Append(" - REPORT FOR SPECIES: ");
                    exhl.Append(currSP);
                    break;
                case "L10":
                    exhl.Append("NOTE:  THIS REPORT DOES NOT INCLUDE ANY COUNT OF THE SECONDARY PRODUCT OR CULL LOGS\n");
                    exhl.Append("  SPECIES - ");
                    exhl.Append(currSP.PadLeft(6, ' '));
                    switch (pageToLoad)
                    {
                        case 1:
                            exhl.Append(" - LOG COUNTS");
                            break;
                        case 2:
                            exhl.Append(" - NET CCF VOLUME");
                            break;
                        case 3:
                            exhl.Append(" - NET MBF VOLUME");
                            break;
                    }   //  end switch on page to load
                    break;
            }   //  end switch on report

            return exhl;
        }   //  end LoadExtraHeading


        private void loadDIBclasses(List<LogStockDO> justCutLogs, List<ReportSubtotal> listToLoad)
        {
            //  loads classes into specified list to load
            int maxDBH = Convert.ToInt16(justCutLogs.Max(j => j.SmallEndDiameter));
            if (maxDBH > 48) maxDBH = 48;
            int numClasses = Convert.ToInt16(Math.Floor((maxDBH - 3) + 0.5));
            // load classes into list to load
            if (numClasses < 0)
            {
                ReportSubtotal rs = new ReportSubtotal();
                numClasses = 0;
                rs.Value1 = "0";
                listToLoad.Add(rs);
            }
            else
            {
                //  otherwise load list up to number of classes
                for (int k = 4; k <= maxDBH; k++)
                {
                    ReportSubtotal rs = new ReportSubtotal();
                    if (k == 4)
                    {
                        ReportSubtotal r = new ReportSubtotal();
                        r.Value1 = "1-3";
                        listToLoad.Add(r);
                    }   //  endif
                    rs.Value1 = k.ToString();
                    listToLoad.Add(rs);
                }   //  end for k loop
            }   //  endif
            return;
        }   //  end loadDIBclasses


        private void clearOutputList(List<ReportSubtotal> listToClear)
        {
            //  leave dib classes in first value
            //  otherwise zero out double fields
            foreach (ReportSubtotal lto in listToClear)
            {
                lto.Value3 = 0;
                lto.Value4 = 0;
                lto.Value5 = 0;
                lto.Value6 = 0;
                lto.Value7 = 0;
                lto.Value8 = 0;
                lto.Value9 = 0;
                lto.Value10 = 0;
                lto.Value11 = 0;
                lto.Value12 = 0;
                lto.Value13 = 0;
                lto.Value14 = 0;
                lto.Value15 = 0;
                lto.Value16 = 0;
            }   //  end foreach loop
            return;
        }   //  end clearOutputList
    }   //  end OutputLogStock
}
