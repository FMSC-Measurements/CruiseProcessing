﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    class OutputUnits : CreateTextFile
    {
        #region
        public string currentReport;
        public string currCL;
        private int[] fieldLengths;
        private ArrayList prtFields = new ArrayList();
        private List<StratumDO> sList = new List<StratumDO>();
        private List<TreeCalculatedValuesDO> tcvList = new List<TreeCalculatedValuesDO>();
        private List<CuttingUnitDO> cList = new List<CuttingUnitDO>();
        private List<LCDDO> lcdList = new List<LCDDO>();
        private List<PRODO> proList = new List<PRODO>();
        private List<ReportSubtotal> unitSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> strataSubtotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> grandTotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> summaryList = new List<ReportSubtotal>();
        private string[] completeHeader = new string[11];
        private double numTrees = 0.0;
        private double estTrees = 0.0;
        private double currGBDFT = 0.0;
        private double currNBDFT = 0.0;
        private double currGCUFT = 0.0;
        private double currNCUFT = 0.0;
        private double currCords = 0.0;
        private double proratFac = 0.0;
        private double currGBDFTnonsaw = 0.0;
        private double currNBDFTnonsaw = 0.0;
        private double currGCUFTnonsaw = 0.0;
        private double currNCUFTnonsaw = 0.0;
        private double currGCUFTtopwood = 0.0;
        private double sumDBHsquared = 0.0;
        private double sumExpanFactor = 0.0;
        private string recoveredFlag = "n";
        private long currSTcn;
        private long currCUcn;
        #endregion

        public void OutputUnitReports(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  This generates VSM4 as well as UC reports 1-6 -- remaining UC reports are stand table format
            string currentTitle = fillReportTitle(currentReport);
            sList = bslyr.getStratum();
            cList = bslyr.getCuttingUnits();
            proList = bslyr.getPRO();
            string orderBy = "";
            // reset summation variables
            numTrees = 0.0;
            estTrees = 0.0;
            currGBDFT = 0.0;
            currNBDFT = 0.0;
            currGCUFT = 0.0;
            currNCUFT = 0.0;
            currCords = 0.0;
            currGBDFTnonsaw = 0.0;
            currNBDFTnonsaw = 0.0;
            currGCUFTnonsaw = 0.0;
            currNCUFTnonsaw = 0.0;
            unitSubtotal.Clear();
            strataSubtotal.Clear();

            numOlines = 0;
            recoveredFlag = "n";
            switch (currentReport)
            {
                case "UC1":     case "UC2":     
                case "UC3":     case "UC4":
                    //  process by stratum because data comes from different sources based on method
                    foreach (StratumDO s in sList)
                    {
                        s.CuttingUnits.Populate();
                        currSTcn = (long) s.Stratum_CN;
                        //  Create report title
                        rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
                        switch(currentReport)
                        {
                            case "UC1":
                                finishColumnHeaders(rh.UC1columns, rh.UC1UC2right);
                                orderBy = "Species";
                                fieldLengths = new int[] { 1, 3, 4, 7, 3, 12, 8, 8, 11, 9, 11, 12, 11, 10, 11, 10 };
                                break;
                            case "UC2":
                                finishColumnHeaders(rh.UC2left, rh.UC1UC2right);
                                orderBy = "Species";
                                fieldLengths = new int[] { 1, 3, 4, 7, 3, 12, 8, 8, 11, 9, 11, 12, 11, 10, 11, 10 };
                                break;
                            case "UC3":
                                finishColumnHeaders(rh.UC3left,rh.UC3TO6right);
                                orderBy = "Species";
                                fieldLengths = new int[] { 1, 3, 5, 9, 9, 9, 10, 11, 11, 12, 10, 11, 10, 10, 10 };
                                break;
                            case "UC4":
                                finishColumnHeaders(rh.UC4left,rh.UC3TO6right);
                                orderBy = "Species";
                                fieldLengths = new int[] { 1, 3, 5, 9, 9, 9, 10, 11, 11, 12, 10, 11, 10, 10, 10 };
                                break;
                        }   //  end switch

                        //  process by method as data comes from different places
                        switch (s.Method)
                        {
                            case "100":
                                //  select cut trees from tree calculated values
                                List<TreeCalculatedValuesDO> treeList = bslyr.getTreeCalculatedValues((int) s.Stratum_CN, orderBy);
                                tcvList = treeList.FindAll(
                                    delegate(TreeCalculatedValuesDO tdo)
                                    {
                                        return tdo.Tree.SampleGroup.CutLeave == "C";
                                    });
                                break;
                            case "STR":
                            case "S3P":
                            case "3P":
                                lcdList = bslyr.GetLCDdata(s.Code, "WHERE Stratum = ? AND CutLeave = ?", orderBy);                        
                                break;
                            default:
                                lcdList = bslyr.GetLCDdata(s.Code, "WHERE Stratum = ? AND CutLeave = ? ", orderBy);
                                break;
                        }   //  end switch on method
                        LoadAndPrintProrated(strWriteOut, s, currentReport, rh, ref pageNumb);
                    
                    }   //  end foreach loop
                    //  Output grand total for the report
                    OutputGrandTotal(strWriteOut, rh, currentReport, ref pageNumb);
                    break;
                case "UC5":     case "UC6":     case "LV05": 
                    summaryList.Clear();
                    // needs to run by cutting unit
                    foreach (CuttingUnitDO c in cList)
                    {
                        c.Strata.Populate();
                        currCUcn = (long)c.CuttingUnit_CN;
                        //  Create report title
                        if (currentReport == "LV05")
                            rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FLTO, "");
                        else rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
                        switch (currentReport)
                        {
                            case "UC5":         case "LV05":
                                finishColumnHeaders(rh.UC5left, rh.UC3TO6right);
                                break;
                            case "UC6":
                                finishColumnHeaders(rh.UC6left, rh.UC3TO6right);
                                break;
                        }   //  end switch on report
                        fieldLengths = new int[] { 1, 4, 13, 9, 9, 10, 11, 11, 12, 10, 11, 10, 10, 10, 10 };

                        //  Load and print data for current cutting unit
                        LoadAndPrintProrated(strWriteOut, c, rh, ref pageNumb, summaryList);
                        if(unitSubtotal.Count > 0)
                            OutputUnitSubtotal(strWriteOut, ref pageNumb, rh, currentReport);
                        unitSubtotal.Clear();
                    }   //  end foreach loop
                    //  output Subtotal Summary and grand total for the report
                    OutputSubtotalSummary(strWriteOut, rh, ref pageNumb, summaryList);
                    OutputGrandTotal(strWriteOut, rh, currentReport, ref pageNumb);
                    break;
                case "VSM4":        //  3P Tree report (CP4)
                    //  first see if these methods exist in the cruise file
                    int nthRow = findMethods();
                    if (nthRow == -1)
                    {
                        strWriteOut.WriteLine("\f");
                        strWriteOut.WriteLine("REPORT VSM4 CANNOT BE CREATED");
                        strWriteOut.WriteLine("NO CRUISE METHODS USED IN THIS REPORT EXIST IN THE CRUISE FILE");
                        numOlines += 3;
                        return;
                    }   //  endif nthRow
                    //  pull data to process 
                    foreach (StratumDO s in sList)
                    {
                        if (s.Method == "S3P" || s.Method == "F3P" || s.Method == "3P")
                        {
                            s.CuttingUnits.Populate();
                            //  need current UOM to set report title
                            string currUOM = bslyr.getUOM((int)s.Stratum_CN);
                            //  Create report heading
                            rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO_PPO, "");
                            //  Fix reportTitles accordingly
                            rh.reportTitles[2] = rh.reportTitles[1];
                            switch (currUOM)
                            {
                                case "03":
                                    rh.reportTitles[1] = "CUFT VOLUME";
                                    break;
                                case "01":
                                    rh.reportTitles[1] = "BDFT VOLUME";
                                    break;
                                case "05":
                                    rh.reportTitles[1] = "WEIGHTS";
                                    break;
                                case "02":
                                    rh.reportTitles[1] = "CORD VOLUME";
                                    break;
                            }   //  end switch

                            fieldLengths = new int[] { 3, 9, 8, 7, 7, 6, 9, 7, 9, 10, 10, 7, 8, 10, 13, 4 };

                            //  process by groups so each cutting unit and sample group is on a separate page 
                            List<SampleGroupDO> sgList = bslyr.getSampleGroups((int)s.Stratum_CN);
                            foreach (CuttingUnitDO cud in s.CuttingUnits)
                            {
                                //  pull calculated values for this group
                                tcvList = bslyr.getTreeCalculatedValues((int)s.Stratum_CN, (int)cud.CuttingUnit_CN);
                                 //  August 2016 -- also need count records from tree table in case method is F3P
                                List<TreeDO> tUnitsList = bslyr.JustUnitTrees((int)s.Stratum_CN, (int)cud.CuttingUnit_CN);
                                List<TreeCalculatedValuesDO> justSampleGroups = new List<TreeCalculatedValuesDO>();
                                foreach (SampleGroupDO sg in sgList)
                                {
                                    justSampleGroups = tcvList.FindAll(
                                        delegate(TreeCalculatedValuesDO tdo)
                                        {
                                            return tdo.Tree.SampleGroup.Code == sg.Code;
                                        });
                                    if (s.Method == "3P" || s.Method == "S3P")
                                    {
                                        if (justSampleGroups.Count > 0)
                                        {
                                            WriteCurrentGroup(s.Code, justSampleGroups, currUOM, cud.Code, strWriteOut,
                                                            rh, ref pageNumb, sg.Code, tUnitsList, s.Method);

                                            //  print subtotal for current cutting unit and sample group
                                            OutputSubtotal(strWriteOut, currentReport);
                                        }   //  endif sample groups to print
                                    }
                                    else if (s.Method == "F3P")
                                    {
                                        //  need to find this sample group for printing
                                        List<TreeDO> justSamples = tUnitsList.FindAll(
                                            delegate(TreeDO td)
                                            {
                                                return td.SampleGroup.Code == sg.Code;
                                            });
                                        if(justSamples.Count > 0 || justSampleGroups.Count > 0)
                                        {
                                            WriteCurrentGroup(s.Code, justSampleGroups, currUOM, cud.Code, strWriteOut,
                                                        rh, ref pageNumb, sg.Code, justSamples, s.Method);

                                            //  print subtotal for current cutting unit and sample group
                                            OutputSubtotal(strWriteOut, currentReport);
                                        }
                                    }   //  endif on method
                                }   //  end foreach sample group
                            }   //  end foreach cutting unit

                        }   //  endif method
                    }   //  end foreach stratum loop
                    break;
                case "VSM5":        //  summary by cutting unit
                    orderBy = "Species";
                    fieldLengths = new int[] { 13, 10, 12, 8, 11, 9, 9, 13, 9, 5 };
                    List<ReportSubtotal> speciesList = new List<ReportSubtotal>();
                    summaryList.Clear();
                    //  process by cutting unit
                    foreach (CuttingUnitDO cud in cList) 
                    {
                        //  create report header
                        rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
                        LoadAndPrintProrated(speciesList, strWriteOut, cud, rh, ref pageNumb);
                    }   //  end foreach on cutting unit
                    //  output summary table before the grand total
                    OutputSummaryList(strWriteOut, rh, ref pageNumb, summaryList);
                    //  output grand total
                    OutputGrandTotal(strWriteOut, rh, ref pageNumb);
                    break;
            }   //  end switch on current report

            return;
        }   //  end OutputUnitReports


        private void WriteCurrentGroup(string currST, List<TreeCalculatedValuesDO> currData, string currUOM, 
                                        string currCU, StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb,
                                        string currSG, List<TreeDO> justUnits, string currMeth)
        {
            //  for VSM4 (CP4)
            int firstLine = 0;
            if (currData.Count > 0)
            {
                foreach (TreeCalculatedValuesDO tcv in currData)
                {
                    //  Setup subtotals
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                        rh.VSM4columns, 11, ref pageNumb, "");

                    prtFields = StratumMethods.buildPrintArray(tcv, currUOM, ref firstLine);
                    printOneRecord(fieldLengths, prtFields, strWriteOut);

                    prtFields.Clear();
                }   //  end foreach loop

                UpdateSubtotals(currData, currUOM, currData[0].Tree.CuttingUnit.Code);
            }   //  endif
            //  any count records to print?  Method dependent.  F3P comes from Tree data.
            prtFields.Clear();
            if (currMeth == "3P" || currMeth == "S3P")
            {
                //  moved to beginning as it will be used elsewhere
                List<CountTreeDO> cList = bslyr.getCountTrees();
                List<CountTreeDO> justCurrentCounts = CountTreeMethods.GetSingleValue(cList, currSG, currST, "", currCU, 0);
                foreach (CountTreeDO c in justCurrentCounts)
                {
                    prtFields.Add("");
                    prtFields.Add("  ");      //  blank stratum
                    prtFields.Add("  ");        //  blank sample group
                    prtFields.Add(c.CuttingUnit.Code.PadLeft(3, ' '));
                    prtFields.Add("    ");      //  blank plot
                    prtFields.Add("    ");      //  blank tree number
                    prtFields.Add(c.TreeDefaultValue.Species.PadRight(6, ' '));
                    //  intervening fields are left blank
                    prtFields.Add("     ");     //  DBH
                    prtFields.Add("       ");   //  gross volume
                    prtFields.Add("       ");   //  net volume
                    prtFields.Add("        ");  //  expansion factor
                    prtFields.Add("     ");     //  measured KPI
                    prtFields.Add(c.SumKPI.ToString().PadLeft(5, ' '));
                    prtFields.Add(c.TreeCount.ToString().PadLeft(6, ' '));
                    prtFields.Add("       ");       //  ratio not there
                    prtFields.Add("   ");           //  initials not shown
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                    //  update subtotals here with just KPI and tree count
                    unitSubtotal[0].Value8 += c.SumKPI;
                    unitSubtotal[0].Value7 += c.TreeCount;
                    prtFields.Clear();
                }   //  end foreach loop
            }
            else if (currMeth == "F3P")
            {
                //  pull current sample group from unit trees and print
                List<TreeDO> justGroups = justUnits.FindAll(
                    delegate(TreeDO td)
                    {
                        return td.CountOrMeasure == "C" && td.SampleGroup.Code == currSG;
                    });

                //  is this the first line?
                if (firstLine == 0)
                {
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                        rh.VSM4columns, 11, ref pageNumb, "");
                }   //  endif first line


                //  if no measured trees then create the subtotals 
                if (currData.Count == 0)
                {
                    ReportSubtotal rs = new ReportSubtotal();
                    rs.Value1 = currCU;
                    unitSubtotal.Add(rs);
                }   //  endif
                foreach (TreeDO jg in justGroups)
                {
                    prtFields.Add("");
                    if (firstLine == 0)
                    {
                        prtFields.Add(jg.Stratum.Code.PadLeft(2, ' '));
                        prtFields.Add(jg.SampleGroup.Code.PadLeft(2, ' '));
                        firstLine = 1;
                    }
                    else
                    {
                        prtFields.Add("  ");        // blank stratum
                        prtFields.Add("  ");        //  blank sample group
                    }   //  endif firstLine
                    prtFields.Add(jg.CuttingUnit.Code.PadLeft(3, ' '));
                    prtFields.Add(jg.Plot.PlotNumber.ToString().PadLeft(4,' '));      //  plot
                    prtFields.Add(jg.TreeNumber.ToString().PadLeft(4,' '));      //  tree number
                    prtFields.Add(jg.Species.PadRight(6, ' '));
                    // intervening fields are zero
                    prtFields.Add("  0.0");     //  DBH
                    prtFields.Add("      0");   //  gross volume
                    prtFields.Add("      0");   //  net volume
                    prtFields.Add("     0.0");  //  expansion factor
                    prtFields.Add(jg.KPI.ToString().PadLeft(5, ' '));
                    prtFields.Add(jg.TreeCount.ToString().PadLeft(6, ' '));
                    prtFields.Add("       ");       //  ratio not there
                    prtFields.Add("   ");           //  initials not shown
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                    //  update subtotals here with just KPI and tree count
                    unitSubtotal[0].Value6 += jg.KPI;
                    unitSubtotal[0].Value7 += jg.TreeCount;
                    prtFields.Clear();
                }   //  end foreach loop
            }   //  endif on method
            return;
        }   //  end WriteCurrentGroup


        private int findMethods()
        {
            int nthRow = -1;
            nthRow = sList.FindIndex(
                delegate(StratumDO s)
                {
                    return s.Method == "S3P" || s.Method == "F3P" || s.Method == "3P";
                });

            return nthRow;
        }   //  end findMethods


        private void UpdateSubtotals(List<TreeCalculatedValuesDO> currData, string currUOM, string currUnit)
        {
            //  Works for VSM4 (CP4)
            ReportSubtotal rs = new ReportSubtotal();
            rs.Value1 = currUnit;
            //  Update subtotal volume based on UOM
            switch (currUOM)
            {
                case "03":
                    rs.Value3 = currData.Sum(tcv => tcv.GrossCUFTPP);
                    rs.Value4 = currData.Sum(tcv => tcv.NetCUFTPP);
                    break;
                case "01":
                    rs.Value3 = currData.Sum(tcv => tcv.GrossBDFTPP);
                    rs.Value4 = currData.Sum(tcv => tcv.NetBDFTPP);
                    break;
                case "05":
                    rs.Value3 = currData.Sum(tcv => tcv.BiomassMainStemPrimary);
                    rs.Value4 = currData.Sum(tcv => tcv.BiomassMainStemPrimary);
                    break;
                case "02":
                    rs.Value3 = currData.Sum(tcv => tcv.CordsPP);
                    rs.Value4 = currData.Sum(tcv => tcv.CordsPP);
                    break;
            }   //  end switch on UOM

            rs.Value5 = currData.Sum(tcv => tcv.Tree.ExpansionFactor);
            rs.Value6 = currData.Sum(tcv => tcv.Tree.KPI);
            rs.Value7 = currData.Sum(tcv => tcv.Tree.TreeCount);

            unitSubtotal.Add(rs);
            return;
        }   //  end UpdateSubtotals



        private void OutputSubtotal(StreamWriter strWriteOut, string currRPT)
        {
            //  Works for VSM4 (CP4)
            //  July 2017 and VSM5 report
            switch (currRPT)
            {
                case "VSM4":
                    strWriteOut.Write("             ");
                    strWriteOut.WriteLine(reportConstants.subtotalLine1);
                    strWriteOut.Write("        SUBTOTAL    ");
                    strWriteOut.Write(unitSubtotal[0].Value1.PadLeft(3, ' '));
                    strWriteOut.Write("                                 ");
                    strWriteOut.Write(Utilities.FormatField(unitSubtotal[0].Value3, "{0,7:F1}"));
                    strWriteOut.Write(Utilities.FormatField(unitSubtotal[0].Value4, "{0,9:F1}"));
                    strWriteOut.Write(Utilities.FormatField(unitSubtotal[0].Value5, "{0,11:F2}"));
                    strWriteOut.Write(Utilities.FormatField(unitSubtotal[0].Value6, "{0,7:F0}"));
                    strWriteOut.Write(Utilities.FormatField(unitSubtotal[0].Value8, "{0,7:F0}"));
                    strWriteOut.WriteLine(Utilities.FormatField(unitSubtotal[0].Value7, "{0,9:F0}"));
                    strWriteOut.WriteLine(reportConstants.longLine);
                    strWriteOut.WriteLine("**NOTE:  Total KPI includes sum of measured tree KPI and count KPI.");
                    strWriteOut.WriteLine(reportConstants.longLine);
                    unitSubtotal.Clear();
                    numOlines += 3;
                    break;
                case "VSM5":
                    if (unitSubtotal.Count > 0)
                    {
                        strWriteOut.WriteLine(reportConstants.subtotalLine1);
                        strWriteOut.Write(" SUBTOTAL    ");
                        strWriteOut.Write(unitSubtotal[0].Value1.PadLeft(3, ' '));
                        strWriteOut.Write("                     ");
                        strWriteOut.Write(Utilities.FormatField(unitSubtotal[0].Value3, "{0,11:F0}"));
                        strWriteOut.Write(Utilities.FormatField(unitSubtotal[0].Value4, "{0,11:F0}"));
                        strWriteOut.Write(Utilities.FormatField(unitSubtotal[0].Value5, "{0,9:F0}"));
                        strWriteOut.WriteLine(Utilities.FormatField(unitSubtotal[0].Value6, "{0,9:F0}"));
                        strWriteOut.WriteLine(reportConstants.longLine);
                        unitSubtotal.Clear();
                        numOlines += 3;
                    }   //  endif
                    break;
            }   //  end switch on current report
            return;
        }   //  end OutputSubtotal

        private void OutputUnitSubtotal(StreamWriter strWriteOut, ref int pageNumb, 
                                        reportHeaders rh, string currRPT)
        {
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    completeHeader, 13, ref pageNumb, "");
            switch (currRPT)
            {   
                case "UC1":
                case "UC2":
                    strWriteOut.Write("              ");
                    strWriteOut.WriteLine(reportConstants.subtotalLine1);
                    strWriteOut.Write("              UNIT ");
                    strWriteOut.Write(unitSubtotal[0].Value1);
                    strWriteOut.Write("  TOTAL ");
                    strWriteOut.Write((CommonEquations.AverageDefectPercent(unitSubtotal[0].Value4, unitSubtotal[0].Value6)).ToString().PadLeft(3, ' '));
                    strWriteOut.Write("     {0,3:F0}", CommonEquations.AverageDefectPercent(unitSubtotal[0].Value5, unitSubtotal[0].Value7));
                    strWriteOut.Write("     {0,7:F4}", CommonEquations.BoardCubicRatio(unitSubtotal[0].Value4, unitSubtotal[0].Value5));
                    strWriteOut.Write("    {0,7:F4}", CommonEquations.BoardCubicRatio(unitSubtotal[0].Value6, unitSubtotal[0].Value7));
                    strWriteOut.Write("  {0,9:F0}", unitSubtotal[0].Value3);
                    strWriteOut.Write("  {0,9:F0}", unitSubtotal[0].Value4);
                    strWriteOut.Write("   {0,9:F0}", unitSubtotal[0].Value5);
                    strWriteOut.Write("  {0,9:F0}", unitSubtotal[0].Value6);
                    strWriteOut.Write(" {0,9:F0}", unitSubtotal[0].Value7); 
                    strWriteOut.WriteLine("  {0,10:F2}", unitSubtotal[0].Value8);
                    strWriteOut.WriteLine(" ");
                    numOlines += 3;
                    break;
                case "UC3":     case "UC4":
                case "UC5":     case "UC6":
                case "LV05":
                    strWriteOut.Write("                  ");
                    strWriteOut.WriteLine(reportConstants.subtotalLine1);
                    strWriteOut.Write("  UNIT ");
                    strWriteOut.Write(unitSubtotal[0].Value1.PadLeft(3,' '));
                    strWriteOut.Write(" TOTAL ");
                    strWriteOut.Write(" {0,7:F0}", unitSubtotal[0].Value3);
                    strWriteOut.Write("  {0,7:F0}", unitSubtotal[0].Value13);  
                    strWriteOut.Write("  {0,9:F0}", unitSubtotal[0].Value4);
                    strWriteOut.Write(" {0,9:F0}", unitSubtotal[0].Value5);
                    strWriteOut.Write("  {0,9:F0}", unitSubtotal[0].Value6);
                    strWriteOut.Write("  {0,9:F0}", unitSubtotal[0].Value7);
                    strWriteOut.Write("   {0,9:F0}", unitSubtotal[0].Value9);
                    strWriteOut.Write(" {0,9:F0}", unitSubtotal[0].Value10);
                    strWriteOut.Write("  {0,9:F0}", unitSubtotal[0].Value11);
                    if (recoveredFlag == " R")
                    {
                        strWriteOut.Write(" {0,9:F0}", unitSubtotal[0].Value12);
                        strWriteOut.Write(recoveredFlag);
                        strWriteOut.WriteLine("{0,9:F2}", unitSubtotal[0].Value8);
                        recoveredFlag = "n";
                    }
                    else if(recoveredFlag  =="n")
                    {
                        strWriteOut.Write(" {0,9:F0}", unitSubtotal[0].Value12);
                        strWriteOut.WriteLine(" {0,10:F2}", unitSubtotal[0].Value8);
                    }   //  endif recovered flag
                    strWriteOut.WriteLine(" ");
                    numOlines += 3;
                    break;
            }   //  end switch
            return;
        }   //  end OutputUnitSubtotal

        private void OutputStrataSubtotal(StreamWriter strWriteOut, ref int pageNumb, string currST, 
                                            reportHeaders rh, string currRPT)
        {
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    completeHeader, 13, ref pageNumb, "");
            //  Works with UC reports
            switch (currRPT)
            {
                case "UC1":
                case "UC2":
                    strWriteOut.Write("              ");
                    strWriteOut.WriteLine(reportConstants.subtotalLine1);
                    strWriteOut.Write("             STRATA ");
                    strWriteOut.Write(currST.PadLeft(2, ' '));
                    strWriteOut.Write("  TOTAL ");
                    strWriteOut.Write((CommonEquations.AverageDefectPercent(strataSubtotal[0].Value4, strataSubtotal[0].Value6)).ToString().PadLeft(3, ' '));
                    strWriteOut.Write("     {0,3:F0}", CommonEquations.AverageDefectPercent(strataSubtotal[0].Value5, strataSubtotal[0].Value7));
                    strWriteOut.Write("     {0,7:F4}", CommonEquations.BoardCubicRatio(strataSubtotal[0].Value4, strataSubtotal[0].Value5));
                    strWriteOut.Write("    {0,7:F4}", CommonEquations.BoardCubicRatio(strataSubtotal[0].Value6, strataSubtotal[0].Value7));
                    strWriteOut.Write("  {0,9:F0}", strataSubtotal[0].Value3);
                    strWriteOut.Write("  {0,9:F0}", strataSubtotal[0].Value4);
                    strWriteOut.Write("   {0,9:F0}", strataSubtotal[0].Value5);
                    strWriteOut.Write("  {0,9:F0}", strataSubtotal[0].Value6);
                    strWriteOut.Write(" {0,9:F0}", strataSubtotal[0].Value7);
                    strWriteOut.WriteLine("  {0,10:F2}", strataSubtotal[0].Value8);
                    strWriteOut.WriteLine(" ");
                    strWriteOut.WriteLine(reportConstants.longLine);
                    strWriteOut.WriteLine(reportConstants.longLine);
                    numOlines += 5;
                    break;
                case "UC3":
                case "UC4":
                    strWriteOut.Write("                  ");
                    strWriteOut.WriteLine(reportConstants.subtotalLine1);
                    strWriteOut.Write(" STRATA ");
                    strWriteOut.Write(currST.PadLeft(2,' '));
                    strWriteOut.Write("  TOTAL ");
                    strWriteOut.Write("{0,7:F0}", strataSubtotal[0].Value3);
                    strWriteOut.Write("  {0,7:F0}", strataSubtotal[0].Value13); 
                    strWriteOut.Write("  {0,9:F0}", strataSubtotal[0].Value4);
                    strWriteOut.Write(" {0,9:F0}", strataSubtotal[0].Value5);
                    strWriteOut.Write("  {0,9:F0}", strataSubtotal[0].Value6);
                    strWriteOut.Write("  {0,9:F0}", strataSubtotal[0].Value7);
                    strWriteOut.Write("   {0,9:F0}", strataSubtotal[0].Value9);
                    strWriteOut.Write(" {0,9:F0}", strataSubtotal[0].Value10);
                    strWriteOut.Write("  {0,9:F0}", strataSubtotal[0].Value11);
                    if (recoveredFlag == " R")
                    {
                        strWriteOut.Write(" {0,9:F0}", strataSubtotal[0].Value12);
                        strWriteOut.Write(recoveredFlag);
                        strWriteOut.WriteLine("{0,9:F2}", strataSubtotal[0].Value8);
                        recoveredFlag = "n";
                    }
                    else if(recoveredFlag == "n")
                    {
                        strWriteOut.Write(" {0,9:F0}", strataSubtotal[0].Value12);
                        strWriteOut.WriteLine(" {0,10:F2}", strataSubtotal[0].Value8);
                    }
                    strWriteOut.WriteLine(" ");
                    strWriteOut.WriteLine(reportConstants.longLine);
                    strWriteOut.WriteLine(reportConstants.longLine);
                    numOlines += 5;
                    break;
            }   //  end switch
            return;
        }   //  end OutputSubtotal


        private void OutputGrandTotal(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  works for VSM5 only
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                rh.VSM5columns, 12, ref pageNumb, "");

            strWriteOut.WriteLine("");
            strWriteOut.Write(" TOTAL                              ");
            strWriteOut.Write(Utilities.FormatField(grandTotal[0].Value3, "{0,12:F0}"));
            strWriteOut.Write(Utilities.FormatField(grandTotal[0].Value4, "{0,11:F0}"));
            strWriteOut.Write(Utilities.FormatField(grandTotal[0].Value5, "{0,9:F0}"));
            strWriteOut.WriteLine(Utilities.FormatField(grandTotal[0].Value6, "{0,9:F0}"));
            numOlines += 2;
            return;
        }   //  end OutputGrandTotal


        private void OutputGrandTotal(StreamWriter strWriteOut, reportHeaders rh, string currRPT, ref int pageNumb)
        {
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    completeHeader, 13, ref pageNumb, "");
            //  works for UC reports
            switch (currRPT)
            {
                case "UC1":
                case "UC2":
                    strWriteOut.WriteLine("");
                    strWriteOut.WriteLine("");
                    strWriteOut.WriteLine("                        TOTALS ------    EST. NO        ***** GROSS VOLUME *****      ****** NET VOLUME ******");
                    strWriteOut.WriteLine("                                         OF TREES          BDFT           CUFT           BDFT           CUFT          CORDS");
                    strWriteOut.WriteLine("                                        ________________________________________________________________________________________");
                    strWriteOut.Write("                                        {0,9:F0}", grandTotal[0].Value3);
                    strWriteOut.Write("     {0,9:F0}", grandTotal[0].Value4);
                    strWriteOut.Write("      {0,9:F0}", grandTotal[0].Value5);
                    strWriteOut.Write("      {0,9:F0}", grandTotal[0].Value6);
                    strWriteOut.Write("      {0,9:F0}", grandTotal[0].Value7);
                    strWriteOut.WriteLine("     {0,10:F2}", grandTotal[0].Value8);
                    strWriteOut.WriteLine("                                        ________________________________________________________________________________________\n");
                    numOlines += 8;
                    break;
                case "UC3":
                case "UC4":
                    strWriteOut.WriteLine("");
                    strWriteOut.WriteLine("");
                    strWriteOut.Write(" OVERALL TOTALS-- ");
                    strWriteOut.Write("{0,7:F0}", grandTotal[0].Value3);
                    strWriteOut.Write("  {0,7:F0}", grandTotal[0].Value13);    
                    strWriteOut.Write("  {0,9:F0}", grandTotal[0].Value4);
                    strWriteOut.Write(" {0,9:F0}", grandTotal[0].Value5);
                    strWriteOut.Write("  {0,9:F0}", grandTotal[0].Value6);
                    strWriteOut.Write("  {0,9:F0}", grandTotal[0].Value7);
                    strWriteOut.Write("   {0,9:F0}", grandTotal[0].Value9);
                    strWriteOut.Write(" {0,9:F0}", grandTotal[0].Value10);
                    strWriteOut.Write("  {0,9:F0}", grandTotal[0].Value11);
                    if (recoveredFlag == " R")
                    {
                        strWriteOut.Write(" {0,9:F0}", grandTotal[0].Value12);
                        strWriteOut.Write(recoveredFlag);
                        strWriteOut.WriteLine("{0,9:F2}", grandTotal[0].Value8);
                        recoveredFlag = "n";
                    }
                    else if (recoveredFlag == "n")
                    {
                        strWriteOut.Write(" {0,9:F0}", grandTotal[0].Value12);
                        strWriteOut.WriteLine(" {0,10:F2}", grandTotal[0].Value8);
                    }   //  endif recovered flag
                    numOlines += 3;
                    break;
                case "UC5":
                case "UC6":
                case "LV05":
                    
                    strWriteOut.WriteLine("");
                    strWriteOut.WriteLine("");
                    strWriteOut.Write(" OVERALL TOTALS-- ");
                    if (grandTotal.Count > 0)
                    {
                        strWriteOut.Write("{0,7:F0}", grandTotal[0].Value3);
                        strWriteOut.Write("  {0,7:F0}", grandTotal[0].Value13);
                        strWriteOut.Write("  {0,9:F0}", grandTotal[0].Value4);
                        strWriteOut.Write("  {0,9:F0}", grandTotal[0].Value5);
                        strWriteOut.Write("  {0,9:F0}", grandTotal[0].Value6);
                        strWriteOut.Write("  {0,9:F0}", grandTotal[0].Value7);
                        strWriteOut.Write("   {0,9:F0}", grandTotal[0].Value8);
                        strWriteOut.Write(" {0,9:F0}", grandTotal[0].Value9);
                        strWriteOut.Write("  {0,9:F0}", grandTotal[0].Value10);
                        if (recoveredFlag == " R")
                        {
                            strWriteOut.Write(" {0,9:F0}", grandTotal[0].Value11);
                            strWriteOut.Write(recoveredFlag);
                            strWriteOut.WriteLine("{0,10:F2}", grandTotal[0].Value12);
                            recoveredFlag = "n";
                        }
                        else if(recoveredFlag == "n")
                        {
                            strWriteOut.Write(" {0,9:F0}", grandTotal[0].Value11);
                            strWriteOut.WriteLine(" {0,10:F2}", grandTotal[0].Value12);
                        }   //  endif recovered flag
                        numOlines += 3;
                    }   //  endif
                    break;
            }   //  end switch
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine("");

            grandTotal.Clear();
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    completeHeader, 13, ref pageNumb, "");
            //  also output the footer
            for (int k = 0; k < 5; k++)
                strWriteOut.WriteLine(rh.UCfooter[k]);
            return;
        }   //  end OutputGrandTotal


        private void OutputSubtotalSummary(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb,
                                            List<ReportSubtotal> summaryList)
        {
            //  output summary headers
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                completeHeader, 13, ref pageNumb, "");
            strWriteOut.WriteLine("                  _________________________________________________________________________________________________________________");
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine(" S U B T O T A L S    ************************SAWTIMBER***********************      ****************NON-SAWTIMBER****************");
            strWriteOut.WriteLine("                   TOT EST   EST");
            if (currentReport == "UC5" || currentReport == "LV05")
            {
                strWriteOut.WriteLine("                    # OF     # OF     ** GROSS VOLUME **     ** NET VOLUME **       GROSS     GROSS      NET       NET");
                strWriteOut.WriteLine("          SPECIES   TREES    TREES      BDFT       CUFT       BDFT      CUFT        BDFT      CUFT       BDFT      CUFT     CORDS");
            }
            else if (currentReport == "UC6")
            {
                strWriteOut.WriteLine("          SAMPLE    # OF     # OF     ** GROSS VOLUME **     ** NET VOLUME **       GROSS     GROSS      NET       NET");
                strWriteOut.WriteLine("          GROUP     TREES    TREES      BDFT       CUFT       BDFT      CUFT        BDFT      CUFT       BDFT      CUFT     CORDS");
            }   //  endif
            numOlines += 7;

            //  write out summary list
            foreach (ReportSubtotal rs in summaryList)
            {
                strWriteOut.Write("          ");
                strWriteOut.Write(rs.Value1.PadRight(6, ' '));
                strWriteOut.Write("  {0,7:F0}", rs.Value3);
                strWriteOut.Write("  {0,7:F0}", rs.Value13);
                strWriteOut.Write("  {0,9:F0}", rs.Value4);
                strWriteOut.Write("  {0,9:F0}", rs.Value5);
                strWriteOut.Write("  {0,9:F0}", rs.Value6);
                strWriteOut.Write("  {0,9:F0}", rs.Value7);
                strWriteOut.Write("   {0,9:F0}", rs.Value8);
                strWriteOut.Write(" {0,9:F0}", rs.Value9);
                strWriteOut.Write("  {0,9:F0}", rs.Value10);
                strWriteOut.Write(" {0,9:F0}", rs.Value11);
                strWriteOut.WriteLine(" {0,10:F2}", rs.Value12);
                numOlines++;
            }   //  end foreach loop
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.WriteLine(reportConstants.longLine);
            numOlines += 2;
            return;
        }   //  end OutputSubtotalSummary


        private void OutputSummaryList(StreamWriter strWriteOut, reportHeaders rh, 
                                        ref int pageNumb, List<ReportSubtotal> summaryList)
        {
            //  works for VSM5 only
            if(numOlines >= 50)
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                rh.VSM5columns, 12, ref pageNumb, "");
            else
            {
                //  write just column headings
                strWriteOut.WriteLine("                                           NUMBER     GROSS    NET      SECONDARY            BDFT/CUFT");
                strWriteOut.WriteLine("                       SPECIES   PRODUCT   OF TREES   CUFT     CUFT     NET CUFT     QMD     RATIO");
                strWriteOut.WriteLine(reportConstants.longLine);
                numOlines += 3;
            }   //  endif

            foreach (ReportSubtotal sl in summaryList)
            {
                strWriteOut.Write("                       ");
                strWriteOut.Write(sl.Value1.PadRight(12, ' '));
                strWriteOut.Write(sl.Value2);
                strWriteOut.Write(Math.Round(sl.Value3,0).ToString().PadLeft(11,' '));
                strWriteOut.Write(Math.Round(sl.Value4,0).ToString().PadLeft(11,' '));
                strWriteOut.Write(Math.Round(sl.Value5,0).ToString().PadLeft(9,' '));
                strWriteOut.Write(Math.Round(sl.Value6,0).ToString().PadLeft(9,' '));
                //  QMD
                if (sl.Value8 > 0)
                {
                    double calcValue = Math.Sqrt(sl.Value7 / sl.Value8);
                    strWriteOut.Write(Math.Round(calcValue,1).ToString().PadLeft(13, ' '));
                }
                else strWriteOut.Write("          0.0");

                //  ratio
                if (sl.Value9 > 0)
                {
                    double calcValue = CommonEquations.BoardCubicRatio(sl.Value9, sl.Value5);
                    strWriteOut.WriteLine(Math.Round(calcValue,4).ToString().PadLeft(11,' '));
                }
                else strWriteOut.WriteLine("        ");

                numOlines++;
            }   //  end foreach

            strWriteOut.WriteLine(reportConstants.longLine);
            numOlines++;
            return;
        }   //  end OutputSummary List


        private void finishColumnHeaders(string[] leftHandSide, string[] rightHandSide)
        {
            //  clean up completeHeader before loading again
            for (int j = 0; j < 11; j++)
                completeHeader[j] = null;
            //  load up complete header
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < rightHandSide.Count(); k++)
            {
                sb.Clear();
                sb.Append(leftHandSide[k]);
                sb.Append(rightHandSide[k]);
                completeHeader[k] = sb.ToString();
            }   //  end for loop
            return;
        }   //  end finishColumnHeaders


        private void LoadAndPrintProrated(StreamWriter strWriteOut, StratumDO sdo, string currRPT, 
                                            reportHeaders rh, ref int pageNumb)
        {
            //  loads based on cruise method for UC reports (UC1-UC4)
            string prevCU = "**";
            prtFields = new ArrayList();

            foreach (CuttingUnitDO cuttingUnit in sdo.CuttingUnits)
            {
                currCUcn = (long)cuttingUnit.CuttingUnit_CN;
                prtFields.Add("");
                if (prevCU == "**")
                {
                    prevCU = cuttingUnit.Code;
                    if(currRPT == "UC1" || currRPT == "UC2" || currRPT == "UC3" || currRPT == "UC4")
                        prtFields.Add(sdo.Code.PadLeft(2, ' '));
                    prtFields.Add(cuttingUnit.Code.PadLeft(3, ' '));
                }
                else if(prevCU != cuttingUnit.Code)
                {
                    //  print unit total and reset values
                    unitSubtotal[0].Value1 = prevCU.PadLeft(3, ' '); ;
                    OutputUnitSubtotal(strWriteOut, ref pageNumb, rh, currentReport);
                    prevCU = cuttingUnit.Code;
                    prtFields.Clear();
                    unitSubtotal.Clear();
                    prtFields.Add("");
                    if(currRPT == "UC1" || currRPT == "UC2" || currRPT == "UC3" || currRPT == "UC4")
                        prtFields.Add(sdo.Code.PadLeft(2, ' '));
                    prtFields.Add(cuttingUnit.Code.PadLeft(3, ' '));
                }   //  endif 

                //  process based on cruise method
                switch (sdo.Method)
                {
                    case "100":
                        //  pull current unit from trees
                        List<TreeCalculatedValuesDO> justCurrentGroup = tcvList.FindAll(
                            delegate(TreeCalculatedValuesDO tdo)
                            {
                                return tdo.Tree.CuttingUnit.Code == prevCU;
                            });

                        //  sum up groups
                        SumUpGroups(justCurrentGroup, currentReport, strWriteOut, rh, ref pageNumb);
                        break;
                    case "STR":
                    case "3P":
                    case "S3P":
                        SumUpGroups(currentReport, prevCU, strWriteOut, rh, ref pageNumb);
                        break;
                    default:        //  area based methods
                        //  sum up groups in the strata and output for current unit
                        SumUpGroups(currentReport, prevCU, strWriteOut, rh, ref pageNumb);
                        break;
                }   //  end switch on method
            }   //  end for k loop
            //  output last group, unit subtotal and strata subtotal
            UpdateUnitTotal(currentReport);
            UpdateStrataTotal(currentReport);
            unitSubtotal[0].Value1 = prevCU.PadLeft(3,' ');
            OutputUnitSubtotal(strWriteOut, ref pageNumb, rh, currentReport);
            if(currentReport != "UC5" || currentReport != "UC6")
                OutputStrataSubtotal(strWriteOut, ref pageNumb, sdo.Code, rh, currentReport);
            unitSubtotal.Clear();
            strataSubtotal.Clear();
            return;
        }   //  end LoadAndPrintProrated


        private void LoadAndPrintProrated(StreamWriter strWriteOut, CuttingUnitDO cdo, reportHeaders rh, ref int pageNumb,
                                            List<ReportSubtotal> summaryList)
        {
            //  overloaded to properly print UC5-UC6
            //  pull distinct species from measured trees in Tree to get species groups for each unit for UC5
            //  or sample groups for UC6
            ArrayList groupsToProcess = new ArrayList();
            if (currentReport == "UC5" || currentReport == "LV05")
                groupsToProcess = bslyr.GetJustSpecies("Tree");
            else if (currentReport == "UC6")
                groupsToProcess = bslyr.GetJustSampleGroups();
            foreach (string gtp in groupsToProcess)
            {
                //  pull data based on strata method and species
                foreach (StratumDO stratum in cdo.Strata)
                {
                    currSTcn = (long)stratum.Stratum_CN;
                    switch (stratum.Method)
                    {
                        case "100":
                            //  data comes from trees and must be expanded
                            tcvList = bslyr.getTreeCalculatedValues((int)stratum.Stratum_CN, (int)cdo.CuttingUnit_CN);
                            List<TreeCalculatedValuesDO> currentGroup = new List<TreeCalculatedValuesDO>();
                            if (currentReport == "UC5" || currentReport == "LV05")
                            {
                                //  find cut trees and species to process
                                currentGroup = tcvList.FindAll(
                                    delegate(TreeCalculatedValuesDO tdo)
                                    {
                                        return tdo.Tree.Species == gtp && tdo.Tree.SampleGroup.CutLeave == currCL;
                                    });
                            }
                            else if (currentReport == "UC6")
                            {
                                //  find cut trees and sample groups to process
                                currentGroup = tcvList.FindAll(
                                    delegate(TreeCalculatedValuesDO tdo)
                                    {
                                        return tdo.Tree.SampleGroup.Code == gtp && tdo.Tree.SampleGroup.CutLeave == currCL;
                                    });
                            }   //  endif on current report
                            if(currentGroup.Count > 0) SumUpGroups(currentGroup);
                            break;
                        default:
                            //  otherwise data comes from LCD and is NOT expanded
                            lcdList = bslyr.getLCDOrdered("WHERE CutLeave = ? AND Stratum = ? ORDER BY ", "Species", currCL, stratum.Code);
                            List<LCDDO> currGroup = new List<LCDDO>();
                            if (currentReport == "UC5" || currentReport == "LV05")
                            {
                                //  find species to process
                                currGroup = lcdList.FindAll(
                                    delegate(LCDDO l)
                                    {
                                        return l.Species == gtp;
                                    });
                            }
                            else if (currentReport == "UC6")
                            {
                                //  find sample groups to process
                                currGroup = lcdList.FindAll(
                                    delegate(LCDDO l)
                                    {
                                        return l.SampleGroup == gtp;
                                    });
                            }   //  endif current report
                            if(currGroup.Count > 0) SumUpGroups(currGroup,cdo.Code);
                            break;
                    }   //  end switch on method
                }   //  end for loop on strata
                if (numTrees > 0)
                {
                    //  Ready to print line
                    prtFields.Add("");
                    prtFields.Add(cdo.Code.PadLeft(3, ' '));
                    if (currentReport == "UC5" || currentReport == "LV05")
                        prtFields.Add(gtp.PadRight(6, ' '));
                    else if (currentReport == "UC6") prtFields.Add(gtp.PadRight(6,' '));   //  sample group
                    WriteCurrentGroup(strWriteOut, rh, ref pageNumb);
                    UpdateSubtotalSummary(gtp, summaryList);
                    UpdateUnitTotal(currentReport);
                    unitSubtotal[0].Value1 = cdo.Code;
                }   //  endif something to print -- numTrees is not zero
                prtFields.Clear();
                // reset summation variables
                numTrees = 0.0;
                estTrees = 0.0;
                currGBDFT = 0.0;
                currNBDFT = 0.0;
                currGCUFT = 0.0;
                currNCUFT = 0.0;
                currCords = 0.0;
                currGBDFTnonsaw = 0.0;
                currNBDFTnonsaw = 0.0;
                currGCUFTnonsaw = 0.0;
                currNCUFTnonsaw = 0.0;
            }   //  end foreach loop on species

            return;
        }   //  end LoadAndPrintProrated (just UC5-UC6)


        private void LoadAndPrintProrated(List<ReportSubtotal> speciesList, StreamWriter strWriteOut, 
                                            CuttingUnitDO cdo, reportHeaders rh, ref int pageNumb)
        {
            //  overloaded method for VSM5 report -- summary by cutting unit
            //  sum species groups by stratum
            cdo.Strata.Populate();
            prtFields = new ArrayList();
            string[,] groupsToProcess = bslyr.GetUniqueSpeciesProduct();
            int numRows = groupsToProcess.GetLength(0);
            for (int k = 0; k < numRows; k++)
            {
                if (groupsToProcess[k, 0] != null)
                {
                    //  pull data based on method
                    foreach (StratumDO s in cdo.Strata)
                    {
                        switch (s.Method)
                        {
                            case "100":
                                tcvList = bslyr.getTreeCalculatedValues((int)s.Stratum_CN, (int)cdo.CuttingUnit_CN);
                                List<TreeCalculatedValuesDO> currentGroup = tcvList.FindAll(
                                    delegate(TreeCalculatedValuesDO tdo)
                                    {
                                        return tdo.Tree.Species == groupsToProcess[k, 0] &&
                                            tdo.Tree.SampleGroup.PrimaryProduct == groupsToProcess[k, 1] &&
                                            tdo.Tree.SampleGroup.CutLeave == "C";
                                    });
                                if (currentGroup.Count > 0) SumUpGroups(currentGroup);
                                break;
                            default:
                                //  any other method comes from the LCD table
                                lcdList = bslyr.getLCDOrdered("WHERE CutLeave = ? AND Stratum = ? ORDER BY ",
                                                    "Species", "C", s.Code, "");
                                List<LCDDO> currGroup = lcdList.FindAll(
                                    delegate(LCDDO l)
                                    {
                                        return l.Species == groupsToProcess[k, 0] &&
                                                l.PrimaryProduct == groupsToProcess[k, 1];
                                    });
                                if (currGroup.Count > 0) SumUpGroups(currGroup, s.Method, cdo.Code, 
                                                                        (long) cdo.CuttingUnit_CN, (long) s.Stratum_CN);
                                break;
                        }   //  end switch on method
                    }   //  end foreach loop on stratum
                    if (numTrees > 0)
                    {
                        //  ready to print line
                        prtFields.Add("");
                        prtFields.Add(cdo.Code.PadLeft(3, ' '));
                        prtFields.Add(groupsToProcess[k, 0].PadRight(6, ' '));
                        prtFields.Add(groupsToProcess[k, 1].PadRight(2, ' '));
                        WriteCurrentGroup(cdo.Code, strWriteOut, rh, ref pageNumb, prtFields, groupsToProcess[k, 1]);
                        //  This will update unit subtotals and grand total
                        UpdateUnitTotal();
                        unitSubtotal[0].Value1 = cdo.Code;
                        //  also need to update the summary list
                        UpdateSummaryList(summaryList, groupsToProcess[k,0], groupsToProcess[k,1]);
                        //  reset variables
                        prtFields.Clear();
                        numTrees = 0.0;
                        currGBDFT = 0.0;
                        currNBDFT = 0.0;
                        currGCUFT = 0.0;
                        currNCUFT = 0.0;
                        currGBDFTnonsaw = 0.0;
                        currNBDFTnonsaw = 0.0;
                        currGCUFTnonsaw = 0.0;
                        currNCUFTnonsaw = 0.0;
                        currGCUFTtopwood = 0.0;
                        sumDBHsquared = 0.0;
                        sumExpanFactor = 0.0;
                    }   //  endif
                }   //  endif groups is not null
            }   //  end for loop on species/products

            //  This should be the end of all species for the current unit
            //  print unit subtotal
            OutputSubtotal(strWriteOut, currentReport);

            return;
        }   //  end overloaded LoadAndPrintProrated

        
        private void UpdateUnitTotal()
        {
            //  works for VSM5 only
            //  Updates unit totals and grand totals
            if (unitSubtotal.Count > 0)
            {
                unitSubtotal[0].Value3 += numTrees;
                unitSubtotal[0].Value4 += currGCUFT + currGCUFTnonsaw;
                unitSubtotal[0].Value5 += currNCUFT + currNCUFTnonsaw;
                unitSubtotal[0].Value6 += currGCUFTtopwood;
            }
            else
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value3 += numTrees;
                rs.Value4 += currGCUFT + currGCUFTnonsaw;
                rs.Value5 += currNCUFT + currNCUFTnonsaw;
                rs.Value6 += currGCUFTtopwood;
                unitSubtotal.Add(rs);
            }   //  endif
            //  update grand total
            if (grandTotal.Count > 0)
            {
                grandTotal[0].Value3 += numTrees;
                grandTotal[0].Value4 += currGCUFT + currGCUFTnonsaw;
                grandTotal[0].Value5 += currNCUFT + currNCUFTnonsaw;
                grandTotal[0].Value6 += currGCUFTtopwood;
            }
            else
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value3 += numTrees;
                rs.Value4 += currGCUFT + currGCUFTnonsaw;
                rs.Value5 += currNCUFT + currNCUFTnonsaw;
                rs.Value6 += currGCUFTtopwood;
                grandTotal.Add(rs);
            }   //  endif
            return;
        }   //  end UpdateUnitTotal


        private void UpdateUnitTotal(string currRPT)
        {
            //  Works for UC reports
            //  subtotals are not prorated as the values are prorated when each line is printed
            if (unitSubtotal.Count > 0)
            {
                unitSubtotal[0].Value3 += numTrees;
                unitSubtotal[0].Value4 += currGBDFT;
                unitSubtotal[0].Value5 += currGCUFT;
                unitSubtotal[0].Value6 += currNBDFT;
                unitSubtotal[0].Value7 += currNCUFT;
                unitSubtotal[0].Value8 += currCords;
                if (currRPT == "UC3" || currRPT == "UC4" || currRPT == "UC5" || currRPT == "UC6")
                {
                    unitSubtotal[0].Value9 += currGBDFTnonsaw;
                    unitSubtotal[0].Value10 += currGCUFTnonsaw;
                    unitSubtotal[0].Value11 += currNBDFTnonsaw;
                    unitSubtotal[0].Value12 += currNCUFTnonsaw;
                    unitSubtotal[0].Value13 += estTrees;
                }   //  endif on report
            }
            else
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value3 += numTrees;
                rs.Value4 += currGBDFT;
                rs.Value5 += currGCUFT ;
                rs.Value6 += currNBDFT;
                rs.Value7 += currNCUFT;
                rs.Value8 += currCords;
                if (currRPT == "UC3" || currRPT == "UC4" || currRPT == "UC5" || currRPT == "UC6")
                {
                    rs.Value9 += currGBDFTnonsaw;
                    rs.Value10 += currGCUFTnonsaw;
                    rs.Value11 += currNBDFTnonsaw;
                    rs.Value12 += currNCUFTnonsaw;
                    rs.Value13 += estTrees;
                }   //  endif on report
                unitSubtotal.Add(rs);
            }   //  endif
            return;
        }   //  end UpdateUnitTotal

        private void UpdateStrataTotal(string currRPT)
        {
            //  Works for UC reports
            //  subtotals not prorated as the values are prorated when each line is printed
            if (strataSubtotal.Count > 0)
            {
                strataSubtotal[0].Value3 += numTrees;
                strataSubtotal[0].Value4 += currGBDFT;
                strataSubtotal[0].Value5 += currGCUFT;
                strataSubtotal[0].Value6 += currNBDFT;
                strataSubtotal[0].Value7 += currNCUFT;
                strataSubtotal[0].Value8 += currCords;
                if (currRPT == "UC3" || currRPT == "UC4")
                {
                    strataSubtotal[0].Value9 += currGBDFTnonsaw;
                    strataSubtotal[0].Value10 += currGCUFTnonsaw;
                    strataSubtotal[0].Value11 += currNBDFTnonsaw;
                    strataSubtotal[0].Value12 += currNCUFTnonsaw;
                    strataSubtotal[0].Value13 += estTrees;
                }   //  endif on report
            }
            else
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value3 += numTrees;
                rs.Value4 += currGBDFT;
                rs.Value5 += currGCUFT;
                rs.Value6 += currNBDFT;
                rs.Value7 += currNCUFT;
                rs.Value8 += currCords;
                if (currRPT == "UC3" || currRPT == "UC4")
                {
                    rs.Value9 += currGBDFTnonsaw;
                    rs.Value10 += currGCUFTnonsaw;
                    rs.Value11 += currNBDFTnonsaw;
                    rs.Value12 += currNCUFTnonsaw;
                    rs.Value13 += estTrees;
                }   //  endif on report

                strataSubtotal.Add(rs);
            }   //  endif

            //  updates grand total as well
            if (grandTotal.Count > 0)
            {
                grandTotal[0].Value3 += numTrees;
                grandTotal[0].Value4 += currGBDFT;
                grandTotal[0].Value5 += currGCUFT;
                grandTotal[0].Value6 += currNBDFT;
                grandTotal[0].Value7 += currNCUFT;
                grandTotal[0].Value8 += currCords;
                if (currRPT == "UC3" || currRPT == "UC4")
                {
                    grandTotal[0].Value9 += currGBDFTnonsaw;
                    grandTotal[0].Value10 += currGCUFTnonsaw;
                    grandTotal[0].Value11 += currNBDFTnonsaw;
                    grandTotal[0].Value12 += currNCUFTnonsaw;
                    grandTotal[0].Value13 += estTrees;
                }   //  endif
            }
            else
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value3 += numTrees;
                rs.Value4 += currGBDFT;
                rs.Value5 += currGCUFT;
                rs.Value6 += currNBDFT;
                rs.Value7 += currNCUFT;
                rs.Value8 += currCords;
                if (currRPT == "UC3" || currRPT == "UC4")
                {
                    rs.Value9 += currGBDFTnonsaw;
                    rs.Value10 += currGCUFTnonsaw;
                    rs.Value11 += currNBDFTnonsaw;
                    rs.Value12 += currNCUFTnonsaw;
                    rs.Value13 += estTrees;
                }   //  endif
                grandTotal.Add(rs);
            }   //  endif

            return;
        }   //  end UpdateStrataTotal


        private void UpdateSubtotalSummary(string currSP, List<ReportSubtotal> summaryList)
        {
            //  used by UC5-UC6 only for summary at end of report
            //  see if current species is in the list
            int nthRow = summaryList.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == currSP;
                });
            if (nthRow >= 0)
            {
                summaryList[nthRow].Value3 += numTrees;
                summaryList[nthRow].Value4 += currGBDFT;
                summaryList[nthRow].Value5 += currGCUFT;
                summaryList[nthRow].Value6 += currNBDFT;
                summaryList[nthRow].Value7 += currNCUFT;
                summaryList[nthRow].Value8 += currGBDFTnonsaw;
                summaryList[nthRow].Value9 += currGCUFTnonsaw;
                summaryList[nthRow].Value10 += currNBDFTnonsaw;
                summaryList[nthRow].Value11 += currNCUFTnonsaw;
                summaryList[nthRow].Value12 += currCords;
                summaryList[nthRow].Value13 += estTrees;
            }
            else            //  species not in list so add it
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value1 = currSP;
                rs.Value3 += numTrees;
                rs.Value4 += currGBDFT;
                rs.Value5 += currGCUFT;
                rs.Value6 += currNBDFT;
                rs.Value7 += currNCUFT;
                rs.Value8 += currGBDFTnonsaw;
                rs.Value9 += currGCUFTnonsaw;
                rs.Value10 += currNBDFTnonsaw;
                rs.Value11 += currNCUFTnonsaw;
                rs.Value12 += currCords;
                rs.Value13 += estTrees;
                summaryList.Add(rs);
            
            }   //  endif

            
            //  updates grand total as well
            if (grandTotal.Count > 0)
            {
                grandTotal[0].Value3 += numTrees;
                grandTotal[0].Value4 += currGBDFT;
                grandTotal[0].Value5 += currGCUFT;
                grandTotal[0].Value6 += currNBDFT;
                grandTotal[0].Value7 += currNCUFT;
                grandTotal[0].Value8 += currGBDFTnonsaw;
                grandTotal[0].Value9 += currGCUFTnonsaw;
                grandTotal[0].Value10 += currNBDFTnonsaw;
                grandTotal[0].Value11 += currNCUFTnonsaw;
                grandTotal[0].Value12 += currCords;
                grandTotal[0].Value13 += estTrees;
            }
            else
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value3 += numTrees;
                rs.Value4 += currGBDFT;
                rs.Value5 += currGCUFT;
                rs.Value6 += currNBDFT;
                rs.Value7 += currNCUFT;
                rs.Value8 += currGBDFTnonsaw;
                rs.Value9 += currGCUFTnonsaw;
                rs.Value10 += currNBDFTnonsaw;
                rs.Value11 += currNCUFTnonsaw;
                rs.Value12 += currCords;
                rs.Value13 += estTrees;
                grandTotal.Add(rs);
            }   //  endif
            return;
        }   //  end UpdateSubtotalSummary


        private void UpdateSummaryList(List<ReportSubtotal> summaryList, string currSP, string currPP)
        {
            //  used by VSM5 only for summary at end of report
            //  see if current species is in the list
            int nthRow = summaryList.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == currSP && rs.Value2 == currPP;
                });
            if (nthRow >= 0)
            {
                summaryList[nthRow].Value3 += numTrees;
                summaryList[nthRow].Value4 += currGCUFT + currGCUFTnonsaw;
                summaryList[nthRow].Value5 += currNCUFT + currNCUFTnonsaw;
                summaryList[nthRow].Value6 += currGCUFTtopwood;
                summaryList[nthRow].Value7 += sumDBHsquared;
                summaryList[nthRow].Value8 += sumExpanFactor;
                summaryList[nthRow].Value9 += currNBDFT;
            }
            else            //  species not in list so add it
            {
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value1 = currSP;
                rs.Value2 = currPP;
                rs.Value3 += numTrees;
                rs.Value4 += currGCUFT + currGCUFTnonsaw;
                rs.Value5 += currNCUFT + currNCUFTnonsaw;
                rs.Value6 += currGCUFTtopwood;
                rs.Value7 += sumDBHsquared;
                rs.Value8 += sumExpanFactor;
                rs.Value9 += currNBDFT;
                summaryList.Add(rs);

            }   //  endif
            return;
        }   //  end UpdateSummaryList


        private void WriteCurrentGroup(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  overloaded for UC reports
            string fieldFormat1 = "{0,3:F0}";
            string fieldFormat2 = "{0,7:F4}";
            string fieldFormat3 = "{0,9:F0}";
            string fieldFormat4 = "{0,10:F2}";
            string fieldFormat5 = "{0,7:F0}";

            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                    completeHeader, 15, ref pageNumb, "");
            

            if (currentReport == "UC1" || currentReport == "UC2")
            {
                //  load average defect
                prtFields.Add(Utilities.FormatField(CommonEquations.AverageDefectPercent(currGBDFT, currNBDFT), fieldFormat1));
                prtFields.Add(Utilities.FormatField(CommonEquations.AverageDefectPercent(currGCUFT, currNCUFT), fieldFormat1));
                //  load ratio
                prtFields.Add(Utilities.FormatField(CommonEquations.BoardCubicRatio(currGBDFT, currGCUFT), fieldFormat2));
                prtFields.Add(Utilities.FormatField(CommonEquations.BoardCubicRatio(currNBDFT, currNCUFT), fieldFormat2));
                prtFields.Add(Utilities.FormatField(numTrees,fieldFormat3));
            }
            else if(currentReport == "UC3" || currentReport == "UC4" || 
                    currentReport == "UC5" || currentReport == "UC6" ||
                    currentReport == "LV05")
            {
                //  Expansion factor is first
                prtFields.Add(Utilities.FormatField(numTrees, fieldFormat5));
                //  This is estimated number of trees -- all methods except 3P
                if (currGBDFT == 0.0 && currGCUFT == 0)
                    prtFields.Add("      0");
                else prtFields.Add(Utilities.FormatField(estTrees, fieldFormat5));
            }   // endif on report
            //  load volumes
            prtFields.Add(Utilities.FormatField(currGBDFT, fieldFormat3));
            prtFields.Add(Utilities.FormatField(currGCUFT, fieldFormat3));
            prtFields.Add(Utilities.FormatField(currNBDFT, fieldFormat3));
            prtFields.Add(Utilities.FormatField(currNCUFT, fieldFormat3));
            if (currentReport == "UC3" || currentReport == "UC4" || 
                currentReport == "UC5" || currentReport == "UC6" ||
                currentReport == "LV05")            {
                prtFields.Add(Utilities.FormatField(currGBDFTnonsaw, fieldFormat3));
                prtFields.Add(Utilities.FormatField(currGCUFTnonsaw, fieldFormat3));
                prtFields.Add(Utilities.FormatField(currNBDFTnonsaw, fieldFormat3));
                if (recoveredFlag == " R" && currNCUFTnonsaw > 0)
                {
                    prtFields.Add(Utilities.FormatField(currNCUFTnonsaw, fieldFormat3) + recoveredFlag);
                    prtFields.Add(Utilities.FormatField(currCords, "{0,9:F2}"));
                    recoveredFlag = "n";
                }
                else if(recoveredFlag == "n")
                {
                    prtFields.Add(Utilities.FormatField(currNCUFTnonsaw, fieldFormat3));
                    prtFields.Add(Utilities.FormatField(currCords, fieldFormat4));
                }   //   endif recovered flag
            }   //  endif on report
            printOneRecord(fieldLengths, prtFields, strWriteOut);
            return;
        }   //  end WriteCurrentGroup


        private void WriteCurrentGroup(string currCU, StreamWriter strWriteOut, 
                                        reportHeaders rh, ref int pageNumb, 
                                        ArrayList prtFields, string currPP)
        {
            //  overloaded for VSM5 report
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], 
                                rh.reportTitles[2], rh.VSM5columns, 12, ref pageNumb, "");

            //  load trees and volumes
            prtFields.Add(Utilities.FormatField(numTrees, "{0,5:F0}"));
            if (currPP == "01")
            {
                prtFields.Add(Utilities.FormatField(currGCUFT, "{0,5:F0}"));
                prtFields.Add(Utilities.FormatField(currNCUFT, "{0,5:F0}"));
                prtFields.Add(Utilities.FormatField(currGCUFTtopwood, "{0,5:F0}"));
            }
            else
            {
                prtFields.Add(Utilities.FormatField(currGCUFTnonsaw, "{0,5:F0}"));
                prtFields.Add(Utilities.FormatField(currNCUFTnonsaw, "{0,5:F0}"));
                prtFields.Add(Utilities.FormatField(currGCUFTtopwood, "{0,5:F0}"));
            }   //  endif

            //  print QMD here
            double calcValue = Math.Sqrt(sumDBHsquared / sumExpanFactor);
            prtFields.Add(Utilities.FormatField(calcValue,"{0,5:F1}"));
            //  print ratio here
            calcValue = 0.0;
            if (currNBDFT > 0)
            {
                calcValue = CommonEquations.BoardCubicRatio(currNBDFT, currNCUFT);
                prtFields.Add(Utilities.FormatField(calcValue, "{0,7:F4}"));
            }
            else prtFields.Add("       ");

            printOneRecord(fieldLengths, prtFields, strWriteOut);
            return;
        }   //  end WriteCurrentGroup


        private void SumUpGroups(List<TreeCalculatedValuesDO> currentGroup, string currRPT,
                                    StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  This uses the tree data to sum up values for UC reports (100% method)
            string prevSP = "**";
            string prevPP = "**";
            string prevUOM = "**";
            string prevSG = "**";

            foreach (TreeCalculatedValuesDO tcv in currentGroup)
            {
                if (prevSP == "**" && prevPP == "**" && prevUOM == "**")
                {
                    prevSG = tcv.Tree.SampleGroup.Code;
                    prevSP = tcv.Tree.Species;
                    prevPP = tcv.Tree.SampleGroup.PrimaryProduct;
                    prevUOM = tcv.Tree.SampleGroup.UOM;
                    switch (currRPT)
                    {
                        case "UC1":
                            prtFields.Add(prevSP.PadRight(6, ' '));
                            prtFields.Add(prevPP.PadLeft(2, ' '));
                            prtFields.Add(prevUOM.PadLeft(2, ' '));
                            break;
                        case "UC2":
                            prtFields.Add(tcv.Tree.SampleGroup.Code.PadRight(6, ' '));
                            prtFields.Add(prevPP.PadLeft(2, ' '));
                            prtFields.Add(prevUOM.PadLeft(2, ' '));
                            break;
                        case "UC3":
                            prtFields.Add(prevSP.PadRight(6, ' '));
                            break;
                        case "UC4":
                            prtFields.Add(tcv.Tree.SampleGroup.Code.PadRight(6, ' '));
                            break;
                    }   //  end switch

                }
                else if ((prevSP != tcv.Tree.Species && (currRPT == "UC1" || currRPT == "UC3")) ||
                         (prevSG != tcv.Tree.SampleGroup.Code && (currRPT == "UC2" || currRPT == "UC4"))|| 
                            prevPP != tcv.Tree.SampleGroup.PrimaryProduct ||
                            prevUOM != tcv.Tree.SampleGroup.UOM)
                {
                    WriteCurrentGroup(strWriteOut, rh, ref pageNumb);
                    UpdateUnitTotal(currentReport);
                    UpdateStrataTotal(currentReport);

                    prtFields.Clear();
                    prtFields.Add("");
                    if(currRPT == "UC1" || currRPT == "UC2" || currRPT == "UC3" || currRPT == "UC4")
                        prtFields.Add(tcv.Tree.Stratum.Code.PadLeft(2, ' '));
                    prtFields.Add(tcv.Tree.CuttingUnit.Code.PadLeft(3, ' '));
                    prevSG = tcv.Tree.SampleGroup.Code;
                    prevSP = tcv.Tree.Species;
                    prevPP = tcv.Tree.SampleGroup.PrimaryProduct;
                    prevUOM = tcv.Tree.SampleGroup.UOM;
                    switch (currRPT)
                    {
                        case "UC1":
                            prtFields.Add(prevSP.PadRight(6, ' '));
                            prtFields.Add(prevPP.PadLeft(2, ' '));
                            prtFields.Add(prevUOM.PadLeft(2, ' '));
                            break;
                        case "UC2":
                            prtFields.Add(tcv.Tree.SampleGroup.Code.PadRight(6, ' '));
                            prtFields.Add(prevPP.PadLeft(2, ' '));
                            prtFields.Add(prevUOM.PadLeft(2, ' '));
                            break;
                        case "UC3":
                            prtFields.Add(prevSP.PadRight(6, ' '));
                            break;
                        case "UC4":
                            prtFields.Add(tcv.Tree.SampleGroup.Code.PadRight(6, ' '));
                            break;
                    }   //  end switch
                    //  reset total values
                    numTrees = 0.0;
                    estTrees = 0.0;
                    currGBDFT = 0.0;
                    currGCUFT = 0.0;
                    currNBDFT = 0.0;
                    currNCUFT = 0.0;
                    currCords = 0.0;
                    currGBDFTnonsaw = 0.0;
                    currNBDFTnonsaw = 0.0;
                    currGCUFTnonsaw = 0.0;
                    currNCUFTnonsaw = 0.0;
                }   //  endif

                //retrieve proration factor for this group
                int nthRow = proList.FindIndex(
                    delegate(PRODO p)
                    {
                        return p.CutLeave == "C" && p.Stratum == tcv.Tree.Stratum.Code && 
                                p.CuttingUnit == tcv.Tree.CuttingUnit.Code &&
                                p.SampleGroup == prevSG && p.PrimaryProduct == prevPP && p.UOM == prevUOM;
                    });
                if (nthRow >= 0) proratFac = proList[nthRow].ProrationFactor;
                //  need current method
                string currMeth = tcv.Tree.Stratum.Method;
                switch (currRPT)
                {
                    case "UC1":
                    case "UC2":
                        //  total values
                        numTrees += tcv.Tree.ExpansionFactor;
                        if (currMeth == "3P")
                            estTrees = 0;
                        else estTrees += tcv.Tree.ExpansionFactor;
                        currGBDFT += tcv.GrossBDFTPP * tcv.Tree.ExpansionFactor;
                        currNBDFT += tcv.NetBDFTPP * tcv.Tree.ExpansionFactor;
                        currGCUFT += tcv.GrossCUFTPP * tcv.Tree.ExpansionFactor;
                        currNCUFT += tcv.NetCUFTPP * tcv.Tree.ExpansionFactor;
                        currCords += tcv.CordsPP * tcv.Tree.ExpansionFactor;
                        break;
                    case "UC3":     case "UC4":
                        //  separate sawtimber from nonsaw
                        if (tcv.Tree.SampleGroup.UOM != "05")
                        {
                            numTrees += tcv.Tree.ExpansionFactor;
                            switch (prevPP)
                            {
                                case "01":
                                    currGBDFT += tcv.GrossBDFTPP * tcv.Tree.ExpansionFactor;
                                    currNBDFT += tcv.NetBDFTPP * tcv.Tree.ExpansionFactor;
                                    currGCUFT += tcv.GrossCUFTPP * tcv.Tree.ExpansionFactor;
                                    currNCUFT += tcv.NetCUFTPP * tcv.Tree.ExpansionFactor;
                                    if(currMeth == "3P") estTrees = 0;
                                    else estTrees += tcv.Tree.ExpansionFactor;
                                    break;
                                default:
                                    currGBDFTnonsaw += tcv.GrossBDFTPP * tcv.Tree.ExpansionFactor;
                                    currNBDFTnonsaw += tcv.NetBDFTPP * tcv.Tree.ExpansionFactor;
                                    currGCUFTnonsaw += tcv.GrossCUFTPP * tcv.Tree.ExpansionFactor;
                                    currNCUFTnonsaw += tcv.NetCUFTPP * tcv.Tree.ExpansionFactor;
                                    currCords += tcv.CordsPP * tcv.Tree.ExpansionFactor;
                                    break;
                            }   //  end switch on primary product

                            //  add secondary to nonsaw
                            currGBDFTnonsaw += tcv.GrossBDFTSP * tcv.Tree.ExpansionFactor;
                            currNBDFTnonsaw += tcv.NetBDFTSP * tcv.Tree.ExpansionFactor;
                            currGCUFTnonsaw += tcv.GrossCUFTSP * tcv.Tree.ExpansionFactor;
                            currNCUFTnonsaw += tcv.NetCUFTSP * tcv.Tree.ExpansionFactor;
                            currCords += tcv.CordsSP * tcv.Tree.ExpansionFactor;

                            //  if there is recovered, add to secondary and set flag
                            if (tcv.GrossBDFTRP > 0 || tcv.GrossCUFTRP > 0)
                            {
                                recoveredFlag = " R";
                                currNBDFTnonsaw += tcv.GrossBDFTRP * tcv.Tree.ExpansionFactor;
                                currNCUFTnonsaw += tcv.GrossCUFTRP * tcv.Tree.ExpansionFactor;
                                currCords += tcv.CordsRP * tcv.Tree.ExpansionFactor;
                            }   //  endif
                        }   //  endif on unit of measure
                        break;
                }   //  end switch

            }   //  end foreach loop on tree calculated values
            //  output last group
            WriteCurrentGroup(strWriteOut, rh, ref pageNumb);
            prtFields.Clear();
            UpdateUnitTotal(currentReport);
            UpdateStrataTotal(currentReport);
            //  reset total values
            numTrees = 0.0;
            estTrees = 0.0;
            currGBDFT = 0.0;
            currGCUFT = 0.0;
            currNBDFT = 0.0;
            currNCUFT = 0.0;
            currCords = 0.0;
            currGBDFTnonsaw = 0.0;
            currNBDFTnonsaw = 0.0;
            currGCUFTnonsaw = 0.0;
            currNCUFTnonsaw = 0.0;
            return;
        }   //  end SumUpGroups for 100% method


        private void SumUpGroups(string currRPT, string currCU, StreamWriter strWriteOut, reportHeaders rh,
                                        ref int pageNumb)
        {
            //  This sums the current group from the LCD data for the UC reports
            string prevSP = "**";
            string prevPP = "**";
            string prevUOM = "**";
            string prevSG = "**";

            foreach (LCDDO ldo in lcdList)
            {
                if (prevSP == "**" && prevPP == "**" && prevUOM == "**")
                {
                    prevSG = ldo.SampleGroup;
                    prevSP = ldo.Species;
                    prevPP = ldo.PrimaryProduct;
                    prevUOM = ldo.UOM;
                    switch (currRPT)
                    {
                        case "UC1":
                            prtFields.Add(prevSP.PadRight(6, ' '));
                            prtFields.Add(prevPP.PadLeft(2, ' '));
                            prtFields.Add(prevUOM.PadLeft(2, ' '));
                            break;
                        case "UC2":
                            prtFields.Add(prevSG.PadRight(6, ' '));
                            prtFields.Add(prevPP.PadLeft(2, ' '));
                            prtFields.Add(prevUOM.PadLeft(2, ' '));
                            break;
                        case "UC3":
                            prtFields.Add(prevSP.PadRight(6, ' '));
                            break;
                        case "UC4":
                            prtFields.Add(prevSG.PadRight(6, ' '));
                            break;
                    }   //  end switch

                }
                else if ((prevSP != ldo.Species && (currRPT == "UC1" || currRPT == "UC3")) ||
                         (prevSG != ldo.SampleGroup && (currRPT == "UC2" || currRPT == "UC4")) || 
                          prevPP != ldo.PrimaryProduct || prevUOM != ldo.UOM)
                {
                    WriteCurrentGroup(strWriteOut, rh, ref pageNumb);
                    UpdateUnitTotal(currentReport);
                    UpdateStrataTotal(currentReport);

                    prtFields.Clear();
                    prtFields.Add("");
                    prtFields.Add(ldo.Stratum.PadLeft(2, ' '));
                    prtFields.Add(currCU.PadLeft(3, ' '));
                    prevSG = ldo.SampleGroup;
                    prevSP = ldo.Species;
                    prevPP = ldo.PrimaryProduct;
                    prevUOM = ldo.UOM;
                    switch (currRPT)
                    {
                        case "UC1":
                            prtFields.Add(prevSP.PadRight(6, ' '));
                            prtFields.Add(prevPP.PadLeft(2, ' '));
                            prtFields.Add(prevUOM.PadLeft(2, ' '));
                            break;
                        case "UC2":
                            prtFields.Add(prevSG.PadRight(6, ' '));
                            prtFields.Add(prevPP.PadLeft(2, ' '));
                            prtFields.Add(prevUOM.PadLeft(2, ' '));
                            break;
                        case "UC3":
                            prtFields.Add(prevSP.PadRight(6, ' '));
                            break;
                        case "UC4":
                            prtFields.Add(prevSG.PadRight(6, ' '));
                            break;
                    }   //  end switch
                    //  reset total values
                    numTrees = 0.0;
                    estTrees = 0.0;
                    currGBDFT = 0.0;
                    currGCUFT = 0.0;
                    currNBDFT = 0.0;
                    currNCUFT = 0.0;
                    currCords = 0.0;
                    currGBDFTnonsaw = 0.0;
                    currNBDFTnonsaw = 0.0;
                    currGCUFTnonsaw = 0.0;
                    currNCUFTnonsaw = 0.0;
                }   //  endif

                //retrieve proration factor for this group
                int nthRow = proList.FindIndex(
                    delegate(PRODO p)
                    {
                        return p.CutLeave == "C" && p.Stratum == ldo.Stratum && p.CuttingUnit == currCU &&
                                p.SampleGroup == ldo.SampleGroup && p.PrimaryProduct == prevPP && p.UOM == prevUOM &&
                                p.STM == ldo.STM;
                    });
                if (nthRow >= 0)
                    proratFac = proList[nthRow].ProrationFactor;

                switch (currRPT)
                {
                    case "UC1":
                    case "UC2":
                        //  total values
                        //  check on method since S3P and 3P use tallied trees instead of expansion factor
                        if (Utilities.MethodLookup(ldo.Stratum, bslyr) == "3P" ||
                            Utilities.MethodLookup(ldo.Stratum, bslyr) == "S3P")
                        {
                            numTrees += pull3PtallyTrees(proList, lcdList, ldo.SampleGroup, 
                                                ldo.Species, ldo.Stratum, ldo.PrimaryProduct, 
                                                ldo.LiveDead, ldo.STM, currCU);
                            estTrees = 0.0;
                        }
                        else if (Utilities.MethodLookup(ldo.Stratum, bslyr) == "3PPNT")
                        {
                            numTrees += ldo.SumExpanFactor * proratFac;
                            estTrees = 0.0;
                        }
                        else
                        {
                            numTrees += ldo.SumExpanFactor * proratFac;
                            estTrees += ldo.SumExpanFactor * proratFac;
                        }   //  endif//  Sum up STM trees separately as what's in the current cutting unit stays in that unit
                        if (ldo.STM == "Y")
                            //  calls capture method to sum expanded volume for each STM tree in the cutting unit
                            captureSTMtrees(currSTcn, currCUcn, ldo.SampleGroup, ldo.STM, ref currGBDFT, ref currNBDFT,
                                            ref currGCUFT, ref currNCUFT, ref currGBDFTnonsaw, ref currNBDFTnonsaw,
                                            ref currGCUFTnonsaw, ref currNCUFTnonsaw, ref currCords, proratFac);
                        else
                        {
                            currGBDFT += ldo.SumGBDFT * proratFac;
                            currNBDFT += ldo.SumNBDFT * proratFac;
                            currGCUFT += ldo.SumGCUFT * proratFac;
                            currNCUFT += ldo.SumNCUFT * proratFac;
                            currCords += ldo.SumCords * proratFac;
                        }   //  endif sure to measure
                        break;
                    case "UC3":
                    case "UC4":
                        //  separate sawtimber from nonsaw
                        if (ldo.UOM != "05")
                        {
                            //  check on method since S3P and 3P use tallied trees instead of expansion factor
                            if (Utilities.MethodLookup(ldo.Stratum, bslyr) == "3P" ||
                                Utilities.MethodLookup(ldo.Stratum, bslyr) == "S3P")
                            {
                                numTrees += pull3PtallyTrees(proList, lcdList, ldo.SampleGroup,
                                                    ldo.Species, ldo.Stratum, ldo.PrimaryProduct,
                                                    ldo.LiveDead, ldo.STM, currCU);
                                estTrees = 0.0;
                            }
                            else if (Utilities.MethodLookup(ldo.Stratum, bslyr) == "3PPNT")
                            {
                                numTrees += ldo.SumExpanFactor * proratFac;
                                estTrees = 0.0;
                            }
                            else
                            {
                                numTrees += ldo.SumExpanFactor * proratFac;
                                if (ldo.PrimaryProduct == "01" && (ldo.SumNBDFT > 0 || ldo.SumNCUFT > 0))
                                    estTrees += ldo.SumExpanFactor * proratFac;
                            }   // endif
                            //  Sum up STM trees separately as what's in the current cutting unit stays in that unit
                            if (ldo.STM == "Y")
                                //  calls capture method to sum expanded volume for each STM tree in the cutting unit
                                captureSTMtrees(currSTcn, currCUcn, ldo.SampleGroup, ldo.STM, ref currGBDFT, ref currNBDFT, 
                                                ref currGCUFT, ref currNCUFT, ref currGBDFTnonsaw, ref currNBDFTnonsaw, 
                                                ref currGCUFTnonsaw, ref currNCUFTnonsaw, ref currCords, proratFac);
                            else
                            {
                                switch (ldo.PrimaryProduct)
                                {
                                    case "01":
                                        currGBDFT += ldo.SumGBDFT * proratFac;
                                        currNBDFT += ldo.SumNBDFT * proratFac;
                                        currGCUFT += ldo.SumGCUFT * proratFac;
                                        currNCUFT += ldo.SumNCUFT * proratFac;
                                        break;
                                    default:
                                        currGBDFTnonsaw += ldo.SumGBDFT * proratFac;
                                        currNBDFTnonsaw += ldo.SumNBDFT * proratFac;
                                        currGCUFTnonsaw += ldo.SumGCUFT * proratFac;
                                        currNCUFTnonsaw += ldo.SumNCUFT * proratFac;
                                        currCords += ldo.SumCords * proratFac;
                                        break;
                                }   //  end switch

                                //  add secondary to nonsaw
                                currGBDFTnonsaw += ldo.SumGBDFTtop * proratFac;
                                currNBDFTnonsaw += ldo.SumNBDFTtop * proratFac;
                                currGCUFTnonsaw += ldo.SumGCUFTtop * proratFac;
                                currNCUFTnonsaw += ldo.SumNCUFTtop * proratFac;
                                currCords += ldo.SumCordsTop * proratFac;

                                //  now if there is recoverd, add to secondary and set flag
                                if (ldo.SumBDFTrecv > 0 || ldo.SumCUFTrecv > 0)
                                {
                                    recoveredFlag = " R";
                                    currNBDFTnonsaw += ldo.SumBDFTrecv * proratFac;
                                    currNCUFTnonsaw += ldo.SumCUFTrecv * proratFac;
                                    currCords += ldo.SumCordsRecv * proratFac;
                                }   //  endif recovered volume
                            }   //  endif STM
                        }   //  endif on unit of measure
                        break;
                }   //  end switch
            }   //  end foreach loop on tree calculated values
            //  output last group
            WriteCurrentGroup(strWriteOut, rh, ref pageNumb);
            prtFields.Clear();
            UpdateUnitTotal(currentReport);
            UpdateStrataTotal(currentReport);
            //  reset total values
            numTrees = 0.0;
            estTrees = 0.0;
            currGBDFT = 0.0;
            currGCUFT = 0.0;
            currNBDFT = 0.0;
            currNCUFT = 0.0;
            currCords = 0.0;
            currGBDFTnonsaw = 0.0;
            currNBDFTnonsaw = 0.0;
            currGCUFTnonsaw = 0.0;
            currNCUFTnonsaw = 0.0;
            return;
        }   //  end SumUpGroups for area based methods


        private void SumUpGroups(List<TreeCalculatedValuesDO> tList)
        {
            //  overloaded for UC5-UC6 100% method
            foreach (TreeCalculatedValuesDO t in tList)
            {
                //  separate sawtimber from non-saw
                if(t.Tree.SampleGroup.UOM != "05")
                {
                    numTrees += t.Tree.ExpansionFactor;
                    switch (t.Tree.SampleGroup.PrimaryProduct)
                    {
                        case "01":
                            currGBDFT += t.GrossBDFTPP * t.Tree.ExpansionFactor;
                            currNBDFT += t.NetBDFTPP * t.Tree.ExpansionFactor;
                            currGCUFT += t.GrossCUFTPP * t.Tree.ExpansionFactor;
                            currNCUFT += t.NetCUFTPP * t.Tree.ExpansionFactor;
                            estTrees += t.Tree.ExpansionFactor;
                            break;
                        default:
                            currGBDFTnonsaw += t.GrossBDFTPP * t.Tree.ExpansionFactor;
                            currNBDFTnonsaw += t.NetBDFTPP * t.Tree.ExpansionFactor;
                            currGCUFTnonsaw += t.GrossCUFTPP * t.Tree.ExpansionFactor;
                            currNCUFTnonsaw += t.NetCUFTPP * t.Tree.ExpansionFactor;
                            currCords += t.CordsPP * t.Tree.ExpansionFactor;
                            break;
                    }   //  end switch on primary product

                    //  add secondary to nonsaw
                    currGBDFTnonsaw += t.GrossBDFTSP * t.Tree.ExpansionFactor;
                    currNBDFTnonsaw += t.NetBDFTSP * t.Tree.ExpansionFactor;
                    currGCUFTnonsaw += t.GrossCUFTSP * t.Tree.ExpansionFactor;
                    currNCUFTnonsaw += t.NetCUFTSP * t.Tree.ExpansionFactor;
                    currCords += t.CordsSP * t.Tree.ExpansionFactor;

                    //  if there is recovered, add to secondary and set flag
                    if (t.GrossBDFTRP > 0 || t.GrossCUFTRP > 0)
                    {
                        recoveredFlag = " R";
                        currNBDFTnonsaw += t.GrossBDFTRP * t.Tree.ExpansionFactor;
                        currNCUFTnonsaw += t.GrossCUFTRP * t.Tree.ExpansionFactor;
                        currCords += t.CordsRP * t.Tree.ExpansionFactor;
                    }   //  endif recovered

                    //  lookup proration factor for this group
                    int nthRow = proList.FindIndex(
                        delegate(PRODO p)
                        {
                            return p.CutLeave == "C" && p.Stratum == t.Tree.Stratum.Code &&
                                p.CuttingUnit == t.Tree.CuttingUnit.Code &&
                                p.SampleGroup == t.Tree.SampleGroup.Code &&
                                p.PrimaryProduct == t.Tree.SampleGroup.PrimaryProduct &&
                                p.UOM == t.Tree.SampleGroup.UOM;
                        });
                    if (nthRow >= 0) proratFac = proList[nthRow].ProrationFactor;
                }   // endif on unit of measure
            }   //  end foreach loop on trees

            return;
        }   //  end SumUpGroups for 100% method


        private void SumUpGroups(List<LCDDO> lcdList, string currCU)
        {
            //  overloaded for UC5-UC6 all other methods using LCD
            foreach (LCDDO l in lcdList)
            {
                //  lookup proration factor for this group
                int nthRow = proList.FindIndex(
                    delegate(PRODO p)
                    {
                        return p.CutLeave == currCL && p.Stratum == l.Stratum &&
                            p.CuttingUnit == currCU &&
                            p.SampleGroup == l.SampleGroup &&
                            p.PrimaryProduct == l.PrimaryProduct &&
                            p.UOM == l.UOM && p.STM == l.STM;
                    });
                if (nthRow >= 0)
                    proratFac = proList[nthRow].ProrationFactor;

                // separate sawtimber from nonsaw
                if (l.UOM != "05")
                {
                    //  check on method since S3P and 3P use tallied trees instead of expansion factor
                    if (Utilities.MethodLookup(l.Stratum, bslyr) == "3P" ||
                        Utilities.MethodLookup(l.Stratum, bslyr) == "S3P")
                    {
                        numTrees += pull3PtallyTrees(proList, lcdList, l.SampleGroup,
                                                l.Species, l.Stratum, l.PrimaryProduct,
                                                l.LiveDead, l.STM, currCU);
                        estTrees = 0.0;
                    }
                    else if (Utilities.MethodLookup(l.Stratum, bslyr) == "3PPNT")
                    {
                        numTrees += l.SumExpanFactor * proratFac;
                        estTrees = 0.0;
                    }
                    else
                    {
                        numTrees += l.SumExpanFactor * proratFac;
                        if (l.PrimaryProduct == "01" && (l.SumNBDFT > 0 || l.SumNCUFT > 0))
                            estTrees += l.SumExpanFactor * proratFac;
                    }   //  endif
                            //  Sum up STM trees separately as what's in the current cutting unit stays in that unit
                    if (l.STM == "Y")
                        //  calls capture method to sum expanded volume for each STM tree in the cutting unit
                        captureSTMtrees(currSTcn, currCUcn, l.SampleGroup, l.STM, ref currGBDFT, ref currNBDFT,
                                        ref currGCUFT, ref currNCUFT, ref currGBDFTnonsaw, ref currNBDFTnonsaw,
                                        ref currGCUFTnonsaw, ref currNCUFTnonsaw, ref currCords, proratFac);
                    else
                    {
                        switch (l.PrimaryProduct)
                        {
                            case "01":
                                currGBDFT += l.SumGBDFT * proratFac;
                                currNBDFT += l.SumNBDFT * proratFac;
                                currGCUFT += l.SumGCUFT * proratFac;
                                currNCUFT += l.SumNCUFT * proratFac;
                                break;
                            default:
                                currGBDFTnonsaw += l.SumGBDFT * proratFac;
                                currNBDFTnonsaw += l.SumNBDFT * proratFac;
                                currGCUFTnonsaw += l.SumGCUFT * proratFac;
                                currNCUFTnonsaw += l.SumNCUFT * proratFac;
                                currCords += l.SumCords * proratFac;
                                break;
                        }   //  end switch on primary product

                        //  add secondary to nonsaw
                        currGBDFTnonsaw += l.SumGBDFTtop * proratFac;
                        currNBDFTnonsaw += l.SumNBDFTtop * proratFac;
                        currGCUFTnonsaw += l.SumGCUFTtop * proratFac;
                        currNCUFTnonsaw += l.SumNCUFTtop * proratFac;
                        currCords += l.SumCordsTop * proratFac;

                        //  if there is recovered, add to secondary and set flag
                        if (l.SumBDFTrecv > 0 || l.SumCUFTrecv > 0)
                        {
                            recoveredFlag = " R";
                            currNBDFTnonsaw += l.SumBDFTrecv * proratFac;
                            currNCUFTnonsaw += l.SumCUFTrecv * proratFac;
                            currCords += l.SumCordsRecv * proratFac;
                        }   //  endif recovered
                    }   //  endif sure to measure
                }   //  endif on unit of measure
            }   //  end foreach loop on LCD
            return;
        }   //  end SumUpGroups


        private void SumUpGroups(List<LCDDO> currGroup, string currMethod, 
                                    string currCU, long currCU_CN, long currST_CN)
        {
            //  overloaded to sum up volumes for VSM5 report
            foreach (LCDDO cg in currGroup)
            {
                double justTalliedTrees = 0;
                //  what is the proration factor current group?
                int nthRow = proList.FindIndex(
                    delegate(PRODO p)
                    {
                        return p.CutLeave == "C" && p.Stratum == cg.Stratum &&
                            p.CuttingUnit == currCU && p.SampleGroup == cg.SampleGroup &&
                            p.PrimaryProduct == cg.PrimaryProduct && p.UOM == cg.UOM &&
                            p.STM == cg.STM;
                    });
                if (nthRow >= 0)
                {
                    proratFac = proList[nthRow].ProrationFactor;
                    justTalliedTrees = proList[nthRow].TalliedTrees;
                }   //  endif

                //  sum volumes
                if (cg.UOM != "05")
                {
                    //  total trees by method
                    if (currMethod == "3P" || currMethod == "S3P")
                        numTrees += justTalliedTrees;
                    else numTrees += cg.SumExpanFactor * proratFac;

                    if (cg.STM == "Y")
                    {
                        //  need to sum up volumes differently here
                        sumSTMtrees(cg, currCU, currCU_CN, currST_CN);
                    }
                    else
                    {
                        switch (cg.PrimaryProduct)
                        {
                            case "01":
                                currGBDFT += cg.SumGBDFT * proratFac;
                                currNBDFT += cg.SumNBDFT * proratFac;
                                currGCUFT += cg.SumGCUFT * proratFac;
                                currNCUFT += cg.SumNCUFT * proratFac;
                                break;
                            default:
                                currGBDFTnonsaw += cg.SumGBDFT * proratFac;
                                currNBDFTnonsaw += cg.SumNBDFT * proratFac;
                                currGCUFTnonsaw += cg.SumGCUFT * proratFac;
                                currNCUFTnonsaw += cg.SumNCUFT * proratFac;
                                break;
                        }   //  end switch on product

                        //  accumulate topwood separately for this report
                        currGCUFTtopwood += cg.SumGCUFTtop * proratFac;

                        //  sum up expansion factor and DBH squared for quad mean calc
                        sumDBHsquared += cg.SumDBHOBsqrd;
                        sumExpanFactor += cg.SumExpanFactor;
                    }   //  endif
                }   //  endif
            }   //  end foreach loop
            return;
        }   //  end overloaded SumUpGroups

        private void sumSTMtrees(LCDDO currGroup, string currentUnit, 
                                    long currCU_CN, long currST_CN)
        {
            //  sums sure-to-measure trees for VSM5 report
            List<TreeCalculatedValuesDO> tList = bslyr.getTreeCalculatedValues((int) currST_CN,(int) currCU_CN);
            //  find all trees for this group
            List<TreeCalculatedValuesDO> justSTM = tList.FindAll(
                delegate(TreeCalculatedValuesDO tdo)
                {
                    return tdo.Tree.Species == currGroup.Species && 
                        tdo.Tree.SampleGroup.PrimaryProduct == currGroup.PrimaryProduct  &&
                        tdo.Tree.SampleGroup.Code == currGroup.SampleGroup &&
                        tdo.Tree.SampleGroup.CutLeave == "C" &&
                        tdo.Tree.STM == "Y";
                });

            foreach(TreeCalculatedValuesDO js in justSTM)
            {
                switch(js.Tree.SampleGroup.PrimaryProduct)
                {
                    case "01":
                        currGBDFT += js.GrossBDFTPP * js.Tree.ExpansionFactor;
                        currNBDFT += js.NetBDFTPP * js.Tree.ExpansionFactor;
                        currGCUFT += js.GrossCUFTPP * js.Tree.ExpansionFactor;
                        currNCUFT += js.NetCUFTPP * js.Tree.ExpansionFactor;
                        break;
                    default:
                        currGBDFTnonsaw += js.GrossBDFTPP * js.Tree.ExpansionFactor;
                        currNBDFTnonsaw += js.NetBDFTPP * js.Tree.ExpansionFactor;
                        currGCUFTnonsaw += js.GrossCUFTPP * js.Tree.ExpansionFactor;
                        currNCUFTnonsaw += js.NetCUFTPP * js.Tree.ExpansionFactor;
                        break;
                }   //  end switch

                //  sum up topwood
                currGCUFTtopwood += js.GrossCUFTSP * js.Tree.ExpansionFactor;

                //  and DBH squared
                sumDBHsquared += Math.Pow(js.Tree.DBH, 2) * js.Tree.ExpansionFactor;

                //  and expansion factor
                sumExpanFactor += js.Tree.ExpansionFactor;

                //  and increment number of trees
                numTrees++;
            }   //  end foreach loop
            return;
        }   //  end sumSTMtrees

        private double pull3PtallyTrees(List<PRODO> proList, List<LCDDO> lcdList, string currSG, 
                                        string currSP, string currST, string currPP, string currLD, 
                                        string currSTM, string currCU)
        {
            double talliedTrees = 0;
            //  Are there multiple species in the current sample group
            //  for just non-STM first
            //  need entire LCD list for UC5-6
            if (currentReport == "UC5" || currentReport == "UC6")
                lcdList = bslyr.getLCD();

            List<LCDDO> justGroups = lcdList.FindAll(
                delegate(LCDDO l)
                {
                    return l.Stratum == currST && l.SampleGroup == currSG && l.STM == "N";
                });
            if (justGroups.Count > 1)
            {
                //  find number of tallied trees in LCD for current species
                int nthRow = lcdList.FindIndex(
                    delegate(LCDDO l)
                    {
                        return l.Stratum == currST && l.Species == currSP &&
                            l.SampleGroup == currSG && l.PrimaryProduct == currPP &&
                            l.LiveDead == currLD && l.STM == currSTM;
                    });
                if (nthRow >= 0)
                    talliedTrees += lcdList[nthRow].TalliedTrees;
            }
            else
            {
                //  that means only one species per sample group
                //  so return number of tallied trees from the PRO table
                int nthRow = proList.FindIndex(
                    delegate(PRODO p)
                    {
                        return p.Stratum == currST && p.SampleGroup == currSG &&
                            p.CuttingUnit == currCU && p.PrimaryProduct == currPP &&
                            p.STM == currSTM;
                    });
                if (nthRow >= 0)
                    talliedTrees += proList[nthRow].TalliedTrees;
            }   //  endif

            return talliedTrees;
        }   //  end pull3PtallyTrees
    }
}
