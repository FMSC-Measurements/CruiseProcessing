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
    public class OutputStandTables : CreateTextFile
    {
        #region
        public string currentReport;
        private ArrayList prtFields = new ArrayList();
        private double strAcres = 0;
        private int nthRow = 0;
        private double currEF = 0;
        private string[] columnHeader = new string[3];
        private List<StandTables> reportData = new List<StandTables>();
        private List<TreeCalculatedValuesDO> treeData = new List<TreeCalculatedValuesDO>();
        private string StratumOrSale;
        private string GroupedBy;
        private string whatValue;
        private string whatProduct;
        private string whatCutCode;
        private string joinedLine;
        private string currSale;
        private double[] columnTotals = new double[11];
        private int numPages = 0;
        private int begGroup;
        private int endGroup;
        #endregion

        public void CreateStandTables(StreamWriter strWriteOut, reportHeaders rh,
                                        ref int pageNumb, int classInterval)
        {
            //  fill report title array
            //  This will change depending on the stand table report
            string currentTitle = fillReportTitle(currentReport);
            //  get current sale number for those reports by sale
            List<SaleDO> sList = bslyr.getSale();
            currSale = sList[0].SaleNumber;
            //  what is the cut/leave code?
            if (currentReport.Substring(0, 2) == "TC")
                whatCutCode = "C";
            else if (currentReport.Substring(0, 2) == "TL")
                whatCutCode = "L";


            //  Load switches based on report
            //  Stratum or sale?
            switch (currentReport)
            {
                case "TC1":     case "TC2":     case "TC3":     case "TC4":     case "TC6":
                case "TC51":    case "TC52":    case "TC53":    case "TC54":    case "TC56":
                case "TC65":    case "TL1":     case "TL6":     case "TL52":    case "TL54":
                case "TL56":
                    StratumOrSale = "STRATUM";
                    break;
                default:
                    StratumOrSale = "SALE";
                    break;
            }   //  end switch for stratum or sale
            //  Grouped by?
            switch (currentReport)
            {
                case "TC19":    case "TC20":    case "TC21":    case "TC22":    case "TC24":
                case "TC71":    case "TC72":    case "TC74":    case "TC65":
                    GroupedBy = "S";
                    break;
                default:
                    GroupedBy = "SPU";
                    break;
            }   //  end switch for grouped by
            //  What product?
            switch (currentReport)
            {
                case "TC6":     case "TC12":    case "TC24":    case "TC56":    case "TC62":
                case "TC74":    case "TL6":     case "TL12":    case "TL56":    case "TL62":
                    whatProduct = "NONE";
                    break;
                case "TC19":    case "TC20":    case "TC21":    case "TC22":    case "TC65":
                case "TC71":    case "TC72":
                    whatProduct = "BOTH";
                    break;
                default:
                    whatProduct = "ONLY";
                    break;
            }   //  end switch for product
            //  And finally what value (volume or estimated trees)?
            switch (currentReport)
            {
                case "TC1":     case "TC51":    case "TC19":    case "TC57":    
                case "TL1":     case "TL7":
                    whatValue = "GBDFT";
                    break;
                case "TC2":     case "TC8":     case "TC20":    case "TC52":    case "TC58":
                case "TL8":     case "TL52":    case "TL58":
                    whatValue = "NBDFT";
                    break;
                case "TC3":     case "TC21":    case "TC53":    case "TC59":    case "TC71":
                case "TC65":    case "TL9":     case "TL59":
                    whatValue = "GCUFT";
                    break;
                case "TC4":     case "TC10":    case "TC22":    case "TC54":    case "TC60":
                case "TC72":    case "TL10":    case "TL54":    case "TL60":
                    whatValue = "NCUFT";
                    break;
                case "TC6":     case "TC12":    case "TC24":    case "TC56":    case "TC62":
                case "TC74":    case "TL6":     case "TL12":    case "TL56":    case "TL62":
                    whatValue = "TREES";
                    break;
            }   //  end switch for value

            //  retrieve tree data based on stratum/sale and grouped by
            string orderedBy = "";
            if (StratumOrSale == "STRATUM" && GroupedBy == "SPU")
                orderedBy = "Stratum.Code,Species,PrimaryProduct,UOM";
            else if (StratumOrSale == "STRATUM" && GroupedBy == "S")
                orderedBy = "Stratum.Code,Species";
            else if (StratumOrSale == "SALE" && GroupedBy == "SPU")
                orderedBy = "Species,PrimaryProduct,UOM";
            else if (StratumOrSale == "SALE" && GroupedBy == "S")
                orderedBy = "Species";
            treeData = bslyr.getTreeCalculatedValues(whatCutCode, orderedBy, StratumOrSale);

            if (treeData.Count == 0)
            {
                if (whatCutCode == "C")
                    noDataForReport(strWriteOut, currentReport, " >>>> No cut tree data for this report.");
                else if (whatCutCode == "L")
                    noDataForReport(strWriteOut, currentReport, " >>>> No leave tree data for this report.");
                return;
            }   //  endif no data for report
            //  Process by stratum or sale
            switch (StratumOrSale)
            {
                case "STRATUM":
                    CalculateAndPrintStratum(classInterval, currentTitle, strWriteOut, 
                                            ref pageNumb, rh);
                    break;
                case "SALE":
                    CalculateAndPrintSale(classInterval, currentTitle, strWriteOut,
                                            ref pageNumb,rh);
                    break;
            }   //  end switch

            return;
        }   //  end CreateStandTables


        private void CalculateAndPrintStratum(int classInterval, 
                                            string currentTitle, StreamWriter strWriteOut,
                                            ref int pageNumb, reportHeaders rh)
        {
            //  header and column headers are stratum dependent so loop by stratum
            List<StratumDO> strList = bslyr.getStratum();
            foreach (StratumDO s in strList)
            {
                reportData.Clear();
                //  Load DIB classes for the current stratum
                List<TreeDO> justDIBs = bslyr.getTreeDBH(whatCutCode, s.Code, "M");
                if (justDIBs.Count > 0)
                {
                    LoadTreeDIBclasses(justDIBs[justDIBs.Count - 1].DBH, reportData, classInterval);

                    //  need strata data from all tree data
                    List<TreeCalculatedValuesDO> justStrataData = treeData.FindAll(
                        delegate(TreeCalculatedValuesDO tcv)
                        {
                            return tcv.Tree.Stratum.Code == s.Code;
                        });
                    //  update current title
                    string strataTitle = currentTitle.Replace("X", s.Code);
                    //  set secondary header appropriately
                    switch (whatProduct)
                    {
                        case "NONE":
                            joinedLine = "";
                            break;
                        case "BOTH":
                            joinedLine = reportConstants.PASP;
                            joinedLine += " - ";
                            break;
                        case "ONLY":
                            joinedLine = reportConstants.FPPO;
                            joinedLine += " - ";
                            break;
                    }   //  end switch
                    switch (classInterval)
                    {
                        case 1:         //  one-inch diameter class
                            joinedLine += reportConstants.B1DC;
                            rh.createReportTitle(strataTitle, 6, 0, 0, joinedLine, reportConstants.oneInchDC);
                            break;
                        case 2:         //  two-inch diameter class
                            joinedLine += reportConstants.B2DC;
                            rh.createReportTitle(strataTitle, 6, 0, 0, joinedLine, reportConstants.twoInchDC);
                            break;
                    }   //  end switch
                    //  load column headers here
                    List<LCDDO> justGroups = new List<LCDDO>();
                    if (whatProduct == "BOTH")
                        justGroups = bslyr.getLCDOrdered("WHERE Stratum = ?", "GROUP BY Stratum,Species", s.Code, whatCutCode);
                    else
                        justGroups = bslyr.getLCDOrdered("WHERE Stratum = ?", "GROUP BY Stratum,Species,PrimaryProduct,UOM", s.Code, whatCutCode);
                    //  loop through calculated data to fill stand table
                    processGroups(justGroups, strWriteOut, rh, ref pageNumb, justStrataData, classInterval, s.Code);

                }
                else
                    noDataForReport(strWriteOut, currentReport, ">>>No tree data for current strata; could not produce table");
            }   //  end foreach loop on stratum

            return;
        }   //  end CalculateAndPrintStratum


        private void CalculateAndPrintSale(int classInterval, string currentTitle, StreamWriter strWriteOut,
                                            ref int pageNumb, reportHeaders rh)
        {
            //  Header and colums are sale dependent
            reportData.Clear();

            //  DIB classes will be the same for each page so load 'em up
            List<TreeDO> justDIBs = bslyr.getTreeDBH(whatCutCode);
            LoadTreeDIBclasses(justDIBs[justDIBs.Count - 1].DBH, reportData, classInterval);

            //  update current title
            string saleTitle = currentTitle.Replace("X", currSale);
            //  set secondary header appropriately
            switch (whatProduct)
            {
                case "NONE":
                    joinedLine = "";
                    break;
                case "BOTH":
                    joinedLine = reportConstants.PASP;
                    joinedLine += " - ";
                    break;
                case "ONLY":
                    joinedLine = reportConstants.FPPO;
                    joinedLine += " - ";
                    break;
            }   //  end switch
            switch (classInterval)
            {
                case 1:     //  one-inch diameter class
                    joinedLine += reportConstants.B1DC;
                    rh.createReportTitle(saleTitle, 6, 0, 0, joinedLine, reportConstants.oneInchDC);
                    break;
                case 2:     //  two-inch diameter class
                    joinedLine += reportConstants.B2DC;
                    rh.createReportTitle(saleTitle, 6, 0, 0, joinedLine, reportConstants.twoInchDC);
                    break;
            }   //  end switch

            //  how many species groups?  Determines how many pages
            List<LCDDO> speciesGroups = new List<LCDDO>();
            switch (GroupedBy)
            {
                case "SPU":
                    speciesGroups = bslyr.getLCDOrdered("", "GROUP BY Species,PrimaryProduct,UOM", whatCutCode, "");
                    break;
                case "S":
                    speciesGroups = bslyr.getLCDOrdered("", "GROUP BY Species", whatCutCode, "");
                    break;
            }   //  end switch on GroupedBy

            processGroups(speciesGroups, strWriteOut, rh, ref pageNumb, treeData, classInterval, "");
            return;
        }   //  end CalculateAndPrintSale


        private void processGroups(List<LCDDO> groupsToPrint, StreamWriter strWriteout, reportHeaders rh, 
                                    ref int pageNumb, List<TreeCalculatedValuesDO> currentData,
                                    int classInterval, string currST)
        {
            numPages = (int)Math.Ceiling((decimal)groupsToPrint.Count / 10);
            bool lastPage = false;
            //  Load report table for each page
            for (int j = 1; j <= numPages; j++)
            {
                switch (j)
                {
                    case 1:
                        begGroup = 0;
                        if (groupsToPrint.Count < 10)
                            endGroup = groupsToPrint.Count;
                        else endGroup = 10;
                        break;
                    case 2:
                        begGroup = 10;
                        if (groupsToPrint.Count < 20)
                            endGroup = groupsToPrint.Count;
                        else endGroup = 20;
                        break;
                    case 3:
                        begGroup = 20;
                        if (groupsToPrint.Count < 30)
                            endGroup = groupsToPrint.Count();
                        else endGroup = 30;
                        break;
                    case 4:
                        begGroup = 30;
                        if (groupsToPrint.Count < 40)
                            endGroup = groupsToPrint.Count();
                        else endGroup = 40;
                        break;
                }   //  end switch

                //  pull data for each group
                int tableColumn = 0;
                for (int k = begGroup; k < endGroup; k++)
                {
                    List<TreeCalculatedValuesDO> currentGroupData = GetCurrentGroup(currentData, groupsToPrint[k]);
                    foreach (TreeCalculatedValuesDO cgd in currentGroupData)
                    {
                        strAcres = Utilities.ReturnCorrectAcres(cgd.Tree.Stratum.Code, bslyr, (long)cgd.Tree.Stratum_CN);
                        currEF = cgd.Tree.ExpansionFactor;
                        LoadStandTable(cgd, reportData, groupsToPrint, classInterval, tableColumn);
                    }   //  end foreach loop
                    tableColumn++;
                }   //  end for k loop
                //  print current page
                numOlines = 0;
                if (endGroup == groupsToPrint.Count) lastPage = true;
                if (StratumOrSale == "SALE")
                    LoadColumnHeader("", groupsToPrint, lastPage);
                else LoadColumnHeader(currST, groupsToPrint, lastPage);
                writeCurrentGroup(strWriteout, rh, ref pageNumb, reportData, endGroup, columnHeader, lastPage);
                clearOutputList(reportData);
                //  also clera headers
                columnHeader[0] = null;
                columnHeader[1] = null;
                columnHeader[2] = null;
            }   //  end for j loop
            return;
        }   //  end processGroups


        private void LoadColumnHeader(string currST, List<LCDDO> speciesGroups, bool lastPage)
        {
            //  for sale reports
            string verticalBar = "|   ";
            columnHeader[0] = " SPEC   |   ";
            columnHeader[1] = " PROD   |   ";
            columnHeader[2] = " U OF M |   ";
            switch (GroupedBy)
            {
                case "SPU":
                    for (int j = begGroup; j < endGroup; j++)
                    {
                        columnHeader[0] += speciesGroups[j].Species.PadRight(6,' ');
                        columnHeader[1] += speciesGroups[j].PrimaryProduct.PadRight(6, ' ');
                        columnHeader[2] += speciesGroups[j].UOM.PadRight(6, ' ');
                        columnHeader[0] += verticalBar;
                        columnHeader[1] += verticalBar;
                        columnHeader[2] += verticalBar;
                    }   //  end foreach loop
                    //  add total column header
                    if(lastPage) columnHeader[2] += "TOTALS";
                    break;
                case "S":
                    for(int j = begGroup; j < endGroup; j++)
                    {
                        columnHeader[0] += speciesGroups[j].Species.PadRight(6, ' ');
                        columnHeader[0] += verticalBar;
                        columnHeader[1] += "ALL   |   ";
                        columnHeader[2] += "ALL   |   ";
                    }   //  end foreach loop
                    if(lastPage) columnHeader[2] += "TOTALS";
                    break;
            }   //  end switch on StratumOrSale
            return;
        }   //  end LoadColumnHeader


        private List<TreeCalculatedValuesDO> GetCurrentGroup(List<TreeCalculatedValuesDO> treeData, LCDDO jg)
        {
            List<TreeCalculatedValuesDO> groupToReturn = new List<TreeCalculatedValuesDO>();
            switch (GroupedBy)
            {
                case "SPU":
                    groupToReturn = treeData.FindAll(
                        delegate(TreeCalculatedValuesDO tcv)
                        {
                            return tcv.Tree.Species == jg.Species &&
                                tcv.Tree.SampleGroup.PrimaryProduct == jg.PrimaryProduct &&
                                tcv.Tree.SampleGroup.UOM == jg.UOM;
                        });
                    break;
                case "S":
                    groupToReturn = treeData.FindAll(
                        delegate(TreeCalculatedValuesDO tcv)
                        {
                            return tcv.Tree.Species == jg.Species;
                        });
                    break;
            }   //  end switch
            return groupToReturn;
        }   //  end GetCurrentGroup

        private void LoadStandTable(TreeCalculatedValuesDO jsd, List<StandTables> listToLoad, 
                                   List<LCDDO> justGroups, int classInterval, int whichColumn)
        {
            //  Which row?
            nthRow = FindTreeDIBindex(listToLoad, jsd.Tree.DBH, classInterval);

            //string currMeth = Utilities.MethodLookup(justGroups[whichColumn].Stratum, bslyr);
            string currMeth = Utilities.MethodLookup(jsd.Tree.Stratum.Code, bslyr);

            //  Sum up appropriate value and products
            //  first sum up primary product then check whatProduct to add in secondary
            switch (whatValue)
            {
                case "GBDFT":
                    LoadProperColumn(listToLoad, jsd.GrossBDFTPP * currEF * strAcres, whichColumn);
                    break;
                case "NBDFT":
                    LoadProperColumn(listToLoad, jsd.NetBDFTPP * currEF * strAcres, whichColumn);
                    break;
                case "GCUFT":
                    LoadProperColumn(listToLoad, jsd.GrossCUFTPP * currEF * strAcres, whichColumn);
                    break;
                case "NCUFT":
                    LoadProperColumn(listToLoad, jsd.NetCUFTPP * currEF * strAcres, whichColumn);
                    break;
                case "TREES":
                    //  need to check for 3P and use tally trees instead
                    if (currMeth != "3P")
                        LoadProperColumn(listToLoad, currEF * strAcres, whichColumn);
                    else if (currMeth == "3P" || currMeth == "S3P")
                    {
                        if (justGroups[whichColumn].SumExpanFactor > 0)
                        {
                            if (jsd.Tree.STM == "Y")
                                LoadProperColumn(listToLoad, jsd.Tree.ExpansionFactor, whichColumn);
                            else
                            {
                                List<LCDDO> lcdList = bslyr.getLCD();
                                List<LCDDO> allGroups = lcdList.FindAll(
                                    delegate(LCDDO l)
                                    {
                                        return l.Stratum == jsd.Tree.Stratum.Code &&
                                        l.SampleGroup == jsd.Tree.SampleGroup.Code &&
                                            l.Species == jsd.Tree.Species;
                                    });

                                double allTallied = allGroups.Sum(ag => ag.TalliedTrees);
                                double allExpanFac = allGroups.Sum(ag => ag.SumExpanFactor);
                                double calcValue = allTallied * jsd.Tree.ExpansionFactor / allExpanFac;
                                LoadProperColumn(listToLoad, calcValue, whichColumn);
                            }   //  endif
                        }
                    }   //  endif
                    break;
            }   //  end switch
            //  secondary
            if (whatProduct == "BOTH")
            {
                switch (whatValue)
                {
                    case "GBDFT":
                        LoadProperColumn(listToLoad, jsd.GrossBDFTSP * currEF * strAcres, whichColumn);
                        break;
                    case "NBDFT":
                        LoadProperColumn(listToLoad, jsd.NetBDFTSP * currEF * strAcres, whichColumn);
                        break;
                    case "GCUFT":
                        LoadProperColumn(listToLoad, jsd.GrossCUFTSP * currEF * strAcres, whichColumn);
                        break;
                    case "NCUFT":
                        LoadProperColumn(listToLoad, jsd.NetCUFTSP * currEF * strAcres, whichColumn);
                        break;
                }   //  switch 
            }   //  endif what product
            return;
        }   //  end LoadStandTable


        private void LoadProperColumn(List<StandTables> listToLoad, double valueToLoad, int nthColumn)
        {
            switch (nthColumn)
            {
                case 0:
                    listToLoad[nthRow].species1 += valueToLoad;
                    break;
                case 1:
                    listToLoad[nthRow].species2 += valueToLoad;
                    break;
                case 2:
                    listToLoad[nthRow].species3 += valueToLoad;
                    break;
                case 3:
                    listToLoad[nthRow].species4 += valueToLoad;
                    break;
                case 4:
                    listToLoad[nthRow].species5 += valueToLoad;
                    break;
                case 5:
                    listToLoad[nthRow].species6 += valueToLoad;
                    break;
                case 6:
                    listToLoad[nthRow].species7 += valueToLoad;
                    break;
                case 7:
                    listToLoad[nthRow].species8 += valueToLoad;
                    break;
                case 8:
                    listToLoad[nthRow].species9 += valueToLoad;
                    break;
                case 9:
                    listToLoad[nthRow].species10 += valueToLoad;
                    break;
            }   //  end switch
            //  Add to line total column
            listToLoad[nthRow].lineTotal += valueToLoad;
            return;
        }   //  end LoadProperColumn


        private void writeCurrentGroup(StreamWriter strWriteOut, reportHeaders rh,
                                        ref int pageNumb, List<StandTables> listToPrint, int lastGroup,
                                        string[] headerToPrint, bool lastPage)
        {
            string verticalLine = " |";
            prtFields.Clear();
            //  total columns for this page
            columnTotals[0] = listToPrint.Sum(l => l.species1);
            columnTotals[1] = listToPrint.Sum(l => l.species2);
            columnTotals[2] = listToPrint.Sum(l => l.species3);
            columnTotals[3] = listToPrint.Sum(l => l.species4);
            columnTotals[4] = listToPrint.Sum(l => l.species5);
            columnTotals[5] = listToPrint.Sum(l => l.species6);
            columnTotals[6] = listToPrint.Sum(l => l.species7);
            columnTotals[7] = listToPrint.Sum(l => l.species8);
            columnTotals[8] = listToPrint.Sum(l => l.species9);
            columnTotals[9] = listToPrint.Sum(l => l.species10);
            //  need to adjust lastGroup to account for multiple pages
            int lastTotal = 10;
            switch (numPages)
            {
                case 1:
                    lastTotal = lastGroup;
                    break;
                case 2:
                    if (lastPage)
                    {
                        lastGroup = lastGroup - 10;
                        lastTotal = lastGroup;
                    }
                    
                    break;
                case 3:
                    if (lastPage)
                    {
                        lastGroup = lastGroup - 20;
                        lastTotal = lastGroup;
                    }
                    break;
                case 4:
                    if (lastPage)
                    {
                        lastGroup = lastGroup - 30;
                        lastTotal = lastGroup;
                    }
                    break;
            }   //  end switch on number of pages

            foreach (StandTables ltp in listToPrint)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                        headerToPrint, 11, ref pageNumb, "");

                prtFields.Add(ltp.dibClass.PadLeft(5, ' '));
                prtFields.Add("   |");

                for (int k = 0; k < lastGroup; k++)
                {
                    switch (k)
                    {
                        case 0:
                            prtFields.Add(Utilities.FormatField(ltp.species1, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 1:
                            prtFields.Add(Utilities.FormatField(ltp.species2, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 2:
                            prtFields.Add(Utilities.FormatField(ltp.species3, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 3:
                            prtFields.Add(Utilities.FormatField(ltp.species4, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 4:
                            prtFields.Add(Utilities.FormatField(ltp.species5, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 5:
                            prtFields.Add(Utilities.FormatField(ltp.species6, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 6:
                            prtFields.Add(Utilities.FormatField(ltp.species7, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 7:
                            prtFields.Add(Utilities.FormatField(ltp.species8, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 8:
                            prtFields.Add(Utilities.FormatField(ltp.species9, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalLine);
                            break;
                        case 9:
                            prtFields.Add(Utilities.FormatField(ltp.species10, "{0,8:F0}").ToString().PadLeft(8, ' '));
                            prtFields.Add(verticalLine);
                            break;
                    }   //  end switch
                }   //  end for k loop
                //  output line total column if this is the last page
                if (lastPage)
                    prtFields.Add(Utilities.FormatField(ltp.lineTotal, "{0,9:F0}").ToString().PadLeft(9, ' '));
                printOneRecord(strWriteOut, prtFields);
                //  clear print fields for next line
                prtFields.Clear();
            }   //  end foreach loop

            //  Load and print total line
            prtFields.Clear();
            prtFields.Add(" TOTALS |");
            for (int k = 0; k < lastTotal; k++)
            {
                prtFields.Add(Utilities.FormatField(columnTotals[k], "{0,8:F0}").ToString().PadLeft(8, ' '));
                prtFields.Add(verticalLine);
            }   //  end for k loop
            if (lastPage)
            {
                //  sum up line totals
                columnTotals[10] = listToPrint.Sum(l => l.lineTotal);
                prtFields.Add(Utilities.FormatField(columnTotals[10], "{0,9:F0}").ToString().PadLeft(9, ' '));
            }   //  endif
            printOneRecord(strWriteOut, prtFields);
            return;
        }   //  end writeCurrentGroup


        
        private void clearOutputList(List<StandTables> listToClear)
        {
            //  clears out everything except dib class
            foreach (StandTables ltc in listToClear)
            {
                ltc.species1 = 0;
                ltc.species2 = 0;
                ltc.species3 = 0;
                ltc.species4 = 0;
                ltc.species5 = 0;
                ltc.species6 = 0;
                ltc.species7 = 0;
                ltc.species8 = 0;
                ltc.species9 = 0;
                ltc.species10 = 0;
            }   //  end foreach loop
            return;
        }   //  end clearOutputList
    }   //  end class OutputStandTables
}
