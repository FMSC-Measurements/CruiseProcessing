using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public class OutputList : CreateTextFile
    {
        #region
            double totalStrataAcres;
            double totalPlots;
            public string currentReport;
            private int[] fieldLengths;
            private ArrayList prtFields = new ArrayList();
            private float summedValue = 0;
            private string[] completeHeader;
            private ArrayList fieldsToPrint;
            private List<TreeDO> tList = new List<TreeDO>();
            private List<TreeCalculatedValuesDO> tcvList = new List<TreeCalculatedValuesDO>();
            private List<LogStockDO> lsList = new List<LogStockDO>();
            private List<PlotDO> pList = new List<PlotDO>();
            LogMethods Lms = new LogMethods();
            TreeListMethods Tlm = new TreeListMethods();
        #endregion


        public void OutputListReports(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
            {                
                //  Fill report title array
                string currentTitle = fillReportTitle(currentReport);

                //  create report title
                if (currentReport == "A05")
                    rh.createReportTitle(currentTitle, 3, 15, 45, "", "");
                else if (currentReport == "A06")
                    rh.createReportTitle(currentTitle, 2, 25, 0, "", "");
                else if (currentReport == "A07")
                    rh.createReportTitle(currentTitle, 3, 15, 45, "", "");
                else if (currentReport == "A15")
                    rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.NSPO, "");
                else rh.createReportTitle(currentTitle, 1, 0, 0, "", "");

                //  Which report?  Previous report number in parens
                numOlines = 0;
                switch (currentReport)
                {
                    case "A01":          //  Strata, Unit, and Payment unit Report
                        //  stratum page
                        numOlines = 0;
                        List<StratumDO> sList = bslyr.getStratum();
                        fieldLengths = new int[] { 5, 11, 7, 9, 8, 9, 6, 6, 27, 2, 4 };
                        prtFields.Clear();
                        WriteStratum(strWriteOut, rh, sList, ref pageNumb);
                        //  cutting unit page 
                        numOlines = 0;
                        List<CuttingUnitDO> cList = bslyr.getCuttingUnits();
                        //  page 2 -- cutting units
                        fieldLengths = new int[] { 5, 17, 8, 10, 28, 9, 3 };
                        WriteUnit(1, strWriteOut, rh, cList, ref pageNumb);
                       
                        //  payment unit page
                        numOlines = 0;
                        fieldLengths = new int[] { 5, 13, 9, 9, 4 };
                        WriteUnit(2, strWriteOut, rh,cList, ref pageNumb);
                        //  sample group page
                        numOlines = 0;
                        fieldLengths = new int[] { 2, 10, 7, 7, 7, 7, 7, 25 };
                        List<SampleGroupDO> sgList = bslyr.getSampleGroups();
                        WriteSampleGroup(strWriteOut, rh, sgList, ref pageNumb);
                        break;
                    case "A02":          //  Listing of plot identification information (page three from old A1 report)List<PlotDO> pList = bslyr.getPlots();
                        pList = bslyr.getPlotsOrdered();
                        fieldLengths = new int[] { 5, 9, 11, 10, 6, 9, 8, 7, 3 };
                        prtFields.Clear();
                        if (pList.Count == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No data for report in Plot table");
                            return;
                        }   //  endif no data
                        WritePlot(strWriteOut, rh, ref pageNumb);
                        break;
                    case "A03":          //  Listing of individual tree measurements and characteristics (A2)
                        tList = bslyr.getTreesSorted();
                        //  what fields to print?
                        fieldsToPrint = Tlm.CheckForData(tList);
                        //  Build column headings based on fields to print
                        completeHeader = Tlm.BuildColumnHeaders(rh.A03columns, fieldsToPrint);
                        fieldLengths = new int[fieldsToPrint.Count];
                        prtFields.Clear();
                        WriteTree(strWriteOut, rh, ref pageNumb);

                        break;
                    case "A04":          //  Listing of count table information (count page from A2 report)
                        List<CountTreeDO> ctList = bslyr.getCountsOrdered();

                        fieldLengths = new int[] { 3, 9, 10, 6, 13, 7, 8, 8, 11, 1 };
                        prtFields.Clear();
                        //  no data?
                        if (ctList.Count == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No data for report in Count table");
                            return;
                        }   //  endif no data
                        WriteCountTrees(strWriteOut, rh, ctList, ref pageNumb);
                        break;
                    case "A05":          //  Listing of cubic foot volume   (A5)
                        tList = bslyr.getTreesSorted();
                        tcvList = bslyr.getTreeCalculatedValues();
                        fieldLengths = new int[] { 1, 3, 4, 5, 5, 7, 6, 4, 4, 8, 9, 8, 9, 8, 8, 9, 9, 9, 8, 8 };
                        prtFields.Clear();
                        //  no data?
                        summedValue = tcvList.Sum(cvdo => cvdo.GrossCUFTPP);
                        if (summedValue == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No cubic foot volume for report");
                            return;
                        }   //  endif no data for report
                        WriteTreeCalcValues("CUFT", strWriteOut, rh, tcvList, tList, ref pageNumb);
                        break;
                    case "A07":          //  Listing of board foot volume   (A7)
                        tList = bslyr.getTreesSorted();
                        tcvList = bslyr.getTreeCalculatedValues();
                        fieldLengths = new int[] { 1, 3, 4, 5, 5, 7, 6, 4, 4, 8, 9, 8, 9, 8, 8, 9, 9, 9, 8, 8 };
                        prtFields.Clear();
                        //  no data?
                        summedValue = tcvList.Sum(cvdo => cvdo.GrossBDFTPP);
                        if (summedValue == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No board foot volume for report");
                            return;
                        }   //  endif no data for report
                        WriteTreeCalcValues("BDFT", strWriteOut, rh, tcvList, tList, ref pageNumb);
                        break;
                    case "A06":          //  Listing of dollar value for each tree  (A6)
                        tList = bslyr.getTrees();
                        tcvList = bslyr.getTreeCalculatedValues();
                        fieldLengths = new int[] { 1, 3, 4, 5, 5, 8, 6, 4, 6, 15, 15, 9, 9, 11, 15, 15 };
                        prtFields.Clear();
                        //  no data?
                        summedValue = tcvList.Sum(tcvdo => tcvdo.ValuePP);
                        if (summedValue == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No dollar value data for this report");
                            return;
                        }   //  endif no data for report
                        WriteTreeCalcValues("VALUE", strWriteOut, rh, tcvList, tList, ref pageNumb);
                        break;
                    case "A08":          //  Listing of log grade information   (A3)
                        List<LogDO> logList = bslyr.getLogs();
                        if (logList.Count == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No log data for this report");
                            return;
                        }   //  endif logList empty

                        tList = bslyr.getTreesSorted();
                        fieldLengths = new int[] { 1, 3, 4, 5, 8, 5, 2, 4, 7, 5, 2, 4, 7, 5, 2, 4, 7, 5, 2, 4, 7, 5, 2, 4, 7 };
                        prtFields.Clear();
                        WriteLogList(strWriteOut, rh, logList, tList, ref pageNumb);
                        break;
                    case "A09":          //  Listing of log detail information for Fall, Buck and Scale  (A4)
                        tList = bslyr.getTrees();
                        List<TreeDO> justFBS = tList.FindAll(
                            delegate(TreeDO tdo)
                            {
                                return tdo.IsFallBuckScale == 1;
                            });
                        if (justFBS.Count == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No fall, buck and scale trees for this report.");
                            return;
                        }   //  endif 

                        fieldLengths = new int[] { 1, 3, 4, 5, 6, 5, 6, 6, 5, 3, 10, 7, 10, 7, 5, 5 };
                        prtFields.Clear();
                        lsList = bslyr.getLogStock();
                        WriteFallBuckScale(strWriteOut, rh, justFBS, ref pageNumb);
                        break;
                    case "A10":          //  Listing of calculated biomass weights by component (A9)
                        tList = bslyr.getTrees();
                        tcvList = bslyr.getTreeCalculatedValues();
                        fieldLengths = new int[] { 1, 4, 5, 5, 5, 7, 6, 4, 5, 9, 10, 9, 9, 10, 9, 9, 9, 9, 9 };
                        prtFields.Clear();
                        //  Sum up mainsteam primary to determine if report can be printed
                        summedValue = tcvList.Sum(tcvdo => tcvdo.BiomassMainStemPrimary);
                        if (summedValue == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No biomass data for this report");
                            return;
                        }   //  endif summedValue
                        WriteTreeCalcValues("BIOMASS", strWriteOut, rh, tcvList, tList, ref pageNumb);
                        break;
                    case "A13":         //  Listing of geospatial information  (A14)
                        //  This report prints two separate pages -- one for plot information and one for tree information
                        //  plot information
                        pList = bslyr.getPlots();
                        fieldLengths = new int[] { 2, 5, 6, 4, 13, 11, 12, 1 };
                        prtFields.Clear();
                        WritePlot(strWriteOut, rh, ref pageNumb);
                        
                        //  tree information
                        tList = bslyr.getTrees();
                        fieldLengths = new int[10] { 2, 3, 3, 3, 4, 3, 3, 11, 11, 11 };
                        prtFields.Clear();
                        //  reset headers
                        rh.createReportTitle(currentTitle, 1, 0, 0, "", "");
                        numOlines = 0;
                        WriteTree(strWriteOut, rh, ref pageNumb);
                        break;
                    case "A15":         //  listing of merch rules
                        List<VolumeEquationDO> currVolEq = bslyr.getVolumeEquations();
                        fieldLengths = new int[] { 1, 13, 9, 10, 18, 18, 14, 13, 10, 3 };
                        prtFields.Clear();
                        numOlines = 0;
                        WriteMerchRules(strWriteOut, rh, currVolEq, ref pageNumb);
                        break;
                    case "L1":          //  Log listing
                        lsList = bslyr.getLogStockSorted();
                        fieldLengths = new int[] { 2, 3, 4, 5, 5, 7, 3, 4, 5, 7, 7, 9, 3, 4, 6, 7, 7, 8, 7, 7, 7, 3, 8 };
                        prtFields.Clear();
                        //  no data?
                        if (lsList.Count == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No log stock data for this report.");
                            return;
                        }   //  endif no logs
                        WriteLogStock(strWriteOut,rh,lsList, ref pageNumb);
                        break;
                }   //  end switch on currentReport
                return;
            }   //  end OutputListReports


            private void WriteUnit(int whichPage, StreamWriter strWriteOut, reportHeaders rh,
                                    List<CuttingUnitDO> cList, ref int pageNumb)
            {
                switch (whichPage)
                {
                    case 1:     //  page one -- units       
                        double[] unitAcres = new double[cList.Count];
                        int jIndex = 0;
                        foreach (CuttingUnitDO cul in cList)
                        {
                            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], 
                                                rh.reportTitles[2], rh.A01unit, 10, ref pageNumb, "");
                            prtFields = CuttingUnitMethods.buildPrintArray(cul, mainHeaderFields[3].ToString());
                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                            //  save acres for totaling
                            unitAcres[jIndex] = Convert.ToDouble(cul.Area);
                            jIndex++;
                        }   //  end foreach loop

                        //  Total cutting unit acres
                        double totalAcres = CommonEquations.SumList(unitAcres);
                        strWriteOut.WriteLine("\n        TOTAL SALE ACRES:  {0,10:F2}", totalAcres);
                        break;
                    case 2:     //  page three -- payment units
                        foreach (CuttingUnitDO cud in cList)
                        {
                            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1],
                                                rh.reportTitles[2], rh.A01payment, 9, ref pageNumb, "");
                            //  need stratum for each cutting unit
                            ArrayList strataList = bslyr.GetUnitStrata(cud.Code);
                            if(strataList.Count > 0)
                            {
                                foreach (object obj in strataList)
                                {
                                    string stratumCode = Convert.ToString(obj);
                                    prtFields = CuttingUnitMethods.buildPrintArray(cud, mainHeaderFields[3].ToString(),
                                                                                    stratumCode);
                                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                                }   //  end foreach stratum
                            }
                            else if (strataList.Count == 0)
                            {
                                prtFields = CuttingUnitMethods.buildPrintArray(cud, mainHeaderFields[3].ToString(), "  ");
                                printOneRecord(fieldLengths, prtFields, strWriteOut);
                            }   //  end strata list is empty

                        }   //  end foreach loop
                        break;
                }   //  end switch on page
                return;
            }   //  end WriteUnit


            private void WriteStratum(StreamWriter strWriteOut, reportHeaders rh, List<StratumDO> sList, ref int pageNumb)
            {
                foreach (StratumDO sdo in sList)
                {
                    sdo.CuttingUnits.Populate();
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    rh.A01stratum, 9, ref pageNumb, "");
                    //  need true stratum acres not used for expansion
                    totalStrataAcres = sdo.CuttingUnits.Sum(cu => cu.Area);
                    //  and number of plots for the stratum
                    totalPlots = CommonEquations.NumberOfPlots(sdo.Code, fileName, bslyr);

                    prtFields = StratumMethods.buildPrintArray(sdo, mainHeaderFields[3].ToString(),
                                                                totalStrataAcres, totalPlots);
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  end foreach loop

                return;
            }   //  end WriteStratum


            private void WriteCountTrees(StreamWriter strWriteOut, reportHeaders rh, List<CountTreeDO> ctList, ref int pageNumb)
            {
                //  output count table (was originally in the old A2 report as a separate page)
                foreach (CountTreeDO cdo in ctList)
                {
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], 
                                        rh.reportTitles[2], rh.A04counts, 11, ref pageNumb, "");
                    prtFields = CountTreeMethods.buildPrintArray(cdo, cdo.SampleGroup.Stratum.Code);
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  end foreach loop
            }   //  end WriteCountTrees


            private void WriteTreeCalcValues(string volType, StreamWriter strWriteOut, reportHeaders rh,
                                            List<TreeCalculatedValuesDO> cvList, List<TreeDO> tList, ref int pageNumb)
            {                
                //  which heights to be used?
                int hgtOne = 0;
                int hgtTwo = 0;
                
                whichHeightFields(ref hgtOne, ref hgtTwo, tList);

                //  replace as needed header lines
                string[] headerToPrint = new string[10];
                if (volType == "CUFT" || volType == "BDFT")
                {
                    headerToPrint = updateHeightHeader(hgtOne, hgtTwo, "", rh.A05A07columns);
                    headerToPrint[9] = headerToPrint[9].Replace("VVVV", volType);
                }
                else if (volType == "VALUE")
                    headerToPrint = updateHeightHeader(hgtOne, hgtTwo, "", rh.A06columns);
                else if(volType == "BIOMASS")
                    headerToPrint = updateHeightHeader(hgtOne, hgtTwo, "", rh.A10columns);

                //  print lines
                foreach (TreeDO tl in tList)
                {
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                        headerToPrint, 19, ref pageNumb, "");
                    if ((tl.CountOrMeasure == "M" && tl.ExpansionFactor > 0) ||
                        (tl.Stratum.Method == "FIXCNT"))
                    {
                        switch(volType)
                        {
                            case "CUFT":
                                prtFields = Tlm.buildPrintArray(tl, mainHeaderFields[3].ToString(), cvList,
                                                                hgtOne, hgtTwo, volType);
                                break;
                            case "BDFT":
                                prtFields = Tlm.buildPrintArray(tl, mainHeaderFields[3].ToString(), cvList,
                                                                hgtOne, hgtTwo, volType);
                                break;
                            case "VALUE":
                                prtFields = Tlm.buildPrintArray(tl, tcvList, hgtOne, hgtTwo, "value");
                                break;
                            case "BIOMASS":
                                prtFields = Tlm.buildPrintArray(tl, tcvList, hgtOne, hgtTwo, "biomass");
                                break;
                            }   //  switch on volType
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                    }   //  endif count or measure tree
                }   //  end foreach loop
                return;
            }   //  end WriteTreeCalcValues


            private void WritePlot(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
            {
                if (currentReport == "A13")
                {
                    //  output plot table information for A13
                    foreach (PlotDO pl in pList)
                    {
                        WriteReportHeading(strWriteOut, rh.reportTitles[0], "PLOT TABLE INFORMATION",
                                        rh.reportTitles[2], rh.A13plot, 7, ref pageNumb, "");
                        prtFields = PlotMethods.buildPrintArray(pl, pl.Stratum.Code, pl.CuttingUnit.Code);
                        if (prtFields.Count > 0)
                        {
                            if (prtFields[0] != null)
                                printOneRecord(fieldLengths, prtFields, strWriteOut);
                        }   //  endif
                    }   //  end foreach loop
                }
                else if (currentReport == "A02")
                {
                    foreach (PlotDO pl in pList)
                    {
                        if (pl.Stratum_CN != null && pl.Stratum_CN != 0)
                        {
                            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1],
                                                rh.reportTitles[2], rh.A02plot, 9, ref pageNumb, "");
                            prtFields = PlotMethods.buildPrintArray(pl, mainHeaderFields[3].ToString(),
                                                                    pl.Stratum.Code, pl.CuttingUnit.Code);
                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                            //  print remarks on separate line
                            if (pl.Remarks != "" && pl.Remarks != null)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("          REMARKS >>   ");
                                if (pl.Remarks.Contains('~'))
                                {
                                    string remarkLine = pl.Remarks.Replace("~", "\n\t\t\t\t\t");
                                    sb.Append(remarkLine);
                                    strWriteOut.WriteLine(sb.ToString());
                                    //  how many lines possible in the remarks line
                                    //  not sure this is needed here but put it in anyway
                                    //  doesn't seem like FScruiser is putting in the tilde
                                    //  July 2015
                                    int iLength = pl.Remarks.Length;
                                    int nLines = (int)Math.Ceiling((double)iLength / 130);
                                    numOlines += nLines;
                                }
                                else
                                {
                                    sb.Append(pl.Remarks);
                                    strWriteOut.WriteLine(sb.ToString());
                                    //  how many lines possible in the remarks line
                                    int iLength = pl.Remarks.Length;
                                    int nLines = (int) Math.Ceiling((double)iLength / 130);
                                    numOlines += nLines;
                                }   //  endif
                            }   //  endif remarks available for printing
                        }   //  endif not null
                    }   //  end foreach loop
                }   //  endif currentReport
            }   //  end WritePlot


            private void WriteTree(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
            {
                if (currentReport == "A13")
                {
                    //  output tree table information
                    foreach (TreeDO tdo in tList)
                    {
                        WriteReportHeading(strWriteOut, rh.reportTitles[0], "TREE TABLE INFORMATION",
                                            rh.reportTitles[2], rh.A13tree, 7, ref pageNumb, "");
                        prtFields = Tlm.buildPrintArray(tdo);
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                    }   //  end foreach loop
                }
                else if (currentReport == "A03")
                {
                    //  Output records
                    foreach (TreeDO tdo in tList)
                    {
                        WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                        completeHeader, 18, ref pageNumb, "");
                        Tlm.buildPrintArray(tdo, fieldsToPrint, ref fieldLengths, ref prtFields);
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                        prtFields.Clear();
                        //  Write remarks after tree data
                        if (tdo.Remarks != "" && tdo.Remarks != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("     REMARKS: ");
                            sb.Append(tdo.Remarks);
                            strWriteOut.WriteLine(sb.ToString());
                            numOlines++;
                        }
                    }   //  end foreach loop
                }   //  endif
                return;
            }   //  end WriteTree


            private void WriteLogList(StreamWriter strWriteOut, reportHeaders rh, List<LogDO> logList, List<TreeDO> tList,
                                        ref int pageNumb)
            {
                //  output log data (not logstock)
                //  printing is for 5 logs per line; positions 1-4 are the same until a new tree
                foreach (TreeDO tdo in tList)
                {
                    //  any logs for this tree?
                    List<LogDO> currentLogs = Lms.GetLogRecords(logList, (long)tdo.Tree_CN);
                    int numbLogs = currentLogs.Count();
                    if (numbLogs > 0)
                    {
                        //  how many iterations for the number of logs?
                        int numIts = numbLogs / 5;
                        int remainLogs = numbLogs - numIts * 5;
                        //  Need to round iterations to nearest whole number and save remainder to print all logs

                        for (int n = 0; n < numIts; n++)
                        {
                            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                                rh.A08columns, 10, ref pageNumb, "");
                            switch (n)
                            {
                                case 0:     //  logs 1-5
                                    prtFields = Lms.buildPrintArray(currentLogs, 0, 4);
                                    break;
                                case 1:     //  logs 6-10
                                    prtFields = Lms.buildPrintArray(currentLogs, 5, 9);
                                    break;
                                case 2:     //  logs 11-15
                                    prtFields = Lms.buildPrintArray(currentLogs, 10, 14);
                                    break;
                                case 3:     //  logs 16-20
                                    prtFields = Lms.buildPrintArray(currentLogs, 15, 19);
                                    break;
                            }   //  end switch
                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                        }   //  end for n loop

                        //  print remaining logs if needed
                        if (remainLogs > 0)
                        {
                            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                                rh.A08columns, 10, ref pageNumb, "");
                            prtFields = Lms.buildPrintArray(currentLogs, numbLogs - remainLogs, numbLogs - 1);
                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                        }   //  endif remaining logs
                    }   //  endif number of logs
                }   //  end foreach loop
                return;
            }   //  end WriteLogList


            private void WriteFallBuckScale(StreamWriter strWriteOut, reportHeaders rh, List<TreeDO> justFBS, ref int pageNumb)
            {
                //LogMethods Lms = new LogMethods();
                foreach (TreeDO tdo in justFBS)
                {
                    //  find logs for this tree
                    List<LogStockDO> fbsLogs = lsList.FindAll(
                        delegate(LogStockDO lsdo)
                        {
                            return lsdo.Tree_CN == tdo.Tree_CN;
                        });

                    if (fbsLogs.Count > 0)
                    {
                        foreach (LogStockDO lsd in fbsLogs)
                        {
                            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                                rh.A09columns, 10, ref pageNumb, "");
                            prtFields = Lms.buildPrintArray(lsd);
                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                        }   //  end foreach loop
                    }   //  endif 
                }   //  end foreach loop

                return;
            }   //  end WriteFallBuckScale


            private void WriteLogStock(StreamWriter strWriteOut, reportHeaders rh, List<LogStockDO> lsList, ref int pageNumb)
            {
                //  outputs data from the logstock table -- primarily the L1 report
                foreach (LogStockDO lsdo in lsList)
                {
                    if (numOlines >= 48) numOlines = 0;
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                        rh.L1columns, 10, ref pageNumb, "");
                    //  calulate total expansion factor -- expansion factor times strata acres
                    double STacres = Utilities.ReturnCorrectAcres(lsdo.Tree.Stratum.Code, bslyr, (long)lsdo.Tree.Stratum_CN);
                    if (STacres == 0.0) STacres = 1.0;
                    double totEF = lsdo.Tree.ExpansionFactor * STacres;
                    prtFields = Lms.buildPrintArray(lsdo,totEF);
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  end foreach loop
                return;
            }   //  end WriteLogStock


            private void WriteSampleGroup(StreamWriter strWriteOut, reportHeaders rh, List<SampleGroupDO> sgList, ref int pageNumb)
            {
                SampleGroupMethods Sgm = new SampleGroupMethods();
                //  outputs data from the sample group table
                foreach (SampleGroupDO sg in sgList)
                {
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                        rh.A01samplegroup, 9, ref pageNumb, "");
                    prtFields.Clear();
                    prtFields = Sgm.buildPrintArray(sg);
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  end foreach loop
                return;
            }   //  end WriteSampleGroup


            private void WriteMerchRules(StreamWriter strWriteOut, reportHeaders rh, List<VolumeEquationDO> currEQ, ref int pageNum)
            {
                VolumeEqMethods Vem = new VolumeEqMethods();
                //  output merch rules
                foreach (VolumeEquationDO ce in currEQ)
                {
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], 
                                    rh.reportTitles[2], rh.A15columns, 11, ref pageNum, "");
                    prtFields.Clear();
                    prtFields = Vem.buildMerchArray(ce);
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  end foreach

                //  output footer
                strWriteOut.WriteLine("\n\n");
                strWriteOut.WriteLine(" SEGMENT DESCRIPTIONS");
                strWriteOut.WriteLine("			21 -- If top seg < 1/2 nom log len, combine with next lowest log");
                strWriteOut.WriteLine("			22 -- Top placed with next lowest log and segmented");
                strWriteOut.WriteLine("			23 -- Top segment stands on its own");
                strWriteOut.WriteLine("			24 -- If top seg < 1/4 log len drop the top.  If top >= 1/4 and <= 3/4 nom length, ");
                strWriteOut.WriteLine("						top is 1/2 of nom log lenght, else top is nom log len.");
                return;
            }   //  end WriteMerchRules

    }
}
