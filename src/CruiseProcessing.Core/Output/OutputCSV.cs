using CruiseDAL.DataObjects;
using CruiseProcessing.Output;
using CruiseProcessing.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CruiseProcessing
{
    public class OutputCSV : ReportGeneratorBase
    {
        private static readonly string[] headerA05 = new string[19] { "STRATA", "UNIT", "PLOT", "TREE", "SPEC", "DBH", "HGT1", "HGT2", "GCUFTPP", "NCUFTPP", "GCUFTSP", "NCUFTSP", "TREEFAC", "CHARFAC", "EXPFAC", "PAGCUFTPP", "PANCUFTPP", "PAGCUFTSP", "PANCUFTSP" };
        private static readonly string[] headerA06 = new string[15] { "STRATA", "UNIT", "PLOT", "TREE", "SPEC", "DBH", "HGT1", "HGT2", "VALUEPP", "VALUESP", "TREEFAC", "CHARFAC", "EXPFAC", "PAVALUEPP", "PAVALUESP" };
        private static readonly string[] headerA07 = new string[19] { "STRATA", "UNIT", "PLOT", "TREE", "SPEC", "DBH", "HGT1", "HGT2", "GBDFTPP", "NBDFTPP", "GBDFTSP", "NBDFTSP", "TREEFAC", "CHARFAC", "EXPFAC", "PAGBDFTPP", "PANBDFTPP", "PAGBDFTSP", "PANBDFTSP" };
        private static readonly string[] headerA10 = new string[18] { "STRATA", "UNIT", "PLOT", "TREE", "SPEC", "DBH", "HGT1", "HGT2", "PRIMARYWGT", "SECONDWGT", "FOLIAGE", "LIVEBR", "DEADBR", "STEMTIP", "TOTALTREE", "TREEFAC", "CHARFAC", "EXPFAC" };
        private static readonly string[] headerL1 = new string[22] { "STRATA", "UNIT", "PLOT", "TREE", "SPEC", "PROD", "UOM", "LOGNUM", "SMDIAM", "LGDIAM", "LENGTH", "GRADE", "DEFECT", "RECV", "GBDFT", "GREMV", "NBDFT", "GCUFT", "GREMV", "NCUFT", "DIBCLS", "TOTEXP" };
        //private static readonly string[] headerL2 = new string[13] { "DIBCLS", "GRD 0", "GRD 1", "GRD 2", "GRD 3", "GRD 4", "GRD 5", "GRD 6", "GRD 7", "TOTNET", "CULL", "DEFECT", "TOTAL" };
        private static readonly string[] headerST1 = new string[18] { "STRATA", "PROD", "UOM", "SAMPGRP", "STM", "STGSAMP", "SAMPTREES", "BIG N", "SMALL N", "t VALUE", "MEAN X", "SUM OF X", "SUM X SQRD", "SD", "CV", "STD ERR", "SAM ERR", "COMB SAM ERR" };
        private static readonly string[] headerUC5 = new string[13] { "UNIT", "SPEC", "TOT EST TREES", "SAW EST TREES", "SAW GBDFT", "SAW GCUFT", "SAW NBDFT", "SAW NCUFT", "NS GBDFT", "NS GCUFT", "NS NBDFT", "NS NCUFT", "CORDS" };
        private static readonly string[] headerKPI = new string[4] { "STRATUM", "UNIT", "SPECIES", "ESTIMATE" };
        private static readonly string[] headerTheft = new string[18] { "TREE NUM", "SPECIES", "PRODUCT", "DRC", "DBH", "TOTAL HT", "MERCH HT PRIM", "TOTAL CUFT", "GROSS CUFT", "NET CUFT", "GROSS BDFT", "NET BDFT", "NET CUFT SECONDARY", "NET BDFT SECONDARY", "CORDS", "CORDS SECONDARY", "# LOGS PRIMARY", "# LOGS SECONDARY" };
        private static readonly string[] headerVSM4 = new string[14] { "STRATA", "SAMGRP", "UNIT", "PLOT", "TREE", "SPEC", "DBH", "GROSS", "NET", "EXPFAC", "KPI", "TREECNT", "RATIO", "INIT" };
        private static readonly string[] headerL1R10 = new string[46] { "CRUISE", "STRATA", "UNIT", "PLOT", "TREE", "C/L", "SG", "STM", "SPEC", "PROD", "UOM", "L/D", "YIELD", "CSPEC", "TGRADE", "LOGNUM", "SMDIAM", "LGDIAM", "LENGTH", "VALUE", "GRADE", "DEFECT", "RECV", "GBDFT", "GREMV", "NBDFT", "GCUFT", "GREMV", "NCUFT", "DIBCLS", "TOTEXP", "ACRES", "EXGRD", "NCFTUTIL", "NBFTUTIL", "DBHOB", "TOTHGT", "UNITAC", "XCOORD", "YCOORD", "ZCOORD", "FOREST", "DISTRICT", "SALENAME", "LOGMETH", "SEENDEF" };

        public IDialogService DialogService { get; }

        public OutputCSV(CPbusinessLayer dataLayer, IDialogService dialogService, string reportID) : base(dataLayer, reportID)
        {
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        // TODO make method async and display wait cursor when running
        public void OutputCSVfiles(string CSVoutFile, string reportToUse, string textOutFile)
        {
            //  check for Region 10 and L1 file first -- they have a different format for the ListToOutput
            var currentRegion = DataLayer.getRegion();
            if ((currentReport == "CSV5" || currentReport == "L1") && currentRegion == "10")
            {
                DialogService.ShowInformation("This could take awhile.\r\nPlease wait");
                List<CSVlist> ListToOutput = new List<CSVlist>();
                //  loop twice for cut or leave
                for (int k = 0; k < 2; k++)
                {
                    switch (k)
                    {
                        case 0:             //  cut trees
                            ListToOutput.Clear();
                            ListToOutput = LoadLogStockR10("C");
                            string cutOutFile = System.IO.Path.ChangeExtension(FilePath, "L1cut.csv");
                            if (File.Exists(cutOutFile))
                                File.Delete(cutOutFile);
                            writeCSV(cutOutFile, ListToOutput);
                            break;

                        case 1:             //  leave trees
                            ListToOutput.Clear();
                            ListToOutput = LoadLogStockR10("L");
                            string leaveOutFile = System.IO.Path.ChangeExtension(FilePath, "L1leave.csv");
                            if (File.Exists(leaveOutFile))
                                File.Delete(leaveOutFile);
                            if (ListToOutput.Count > 0) writeCSV(leaveOutFile, ListToOutput);
                            break;
                    }   //  end switch
                }   //  end for k loop
                return;
            }   //  endif region 10 and report is L1 (CSV5)

            //  find report to use in text output file
            int reportFound = 0;
            using (StreamReader strRead = new StreamReader(textOutFile))
            {
                string cLine;
                while ((cLine = strRead.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(cLine) && cLine.Length > 2 && cLine != "\f")
                    {
                        if (cLine.Length >= 3 && cLine.Substring(0, 3) == reportToUse)
                        {
                            switch (cLine.Substring(0, 3))
                            {
                                case "A05":
                                case "A06":
                                case "A07":
                                    reportFound = 1;
                                    //  read header lines
                                    for (int k = 0; k < 18; k++)
                                        cLine = strRead.ReadLine();
                                    //  finish reading and writing lines
                                    writeCSV(reportToUse, strRead, CSVoutFile);
                                    break;

                                case "A10":
                                    reportFound = 1;
                                    //  read header lines
                                    for (int k = 0; k < 16; k++)
                                        cLine = strRead.ReadLine();
                                    //  finish reading and writing lines
                                    writeCSV(reportToUse, strRead, CSVoutFile);
                                    break;

                                case "L1:":
                                    reportFound = 1;
                                    //  read header lines
                                    for (int k = 0; k < 15; k++)
                                        cLine = strRead.ReadLine();
                                    //  finish reading and writing lines
                                    writeCSV(reportToUse, strRead, CSVoutFile);
                                    break;

                                case "L2:":
                                    reportFound = 1;
                                    writeCSV6(CSVoutFile, strRead);
                                    break;

                                case "ST1":
                                    reportFound = 1;
                                    writeCSV7(CSVoutFile, strRead);
                                    break;

                                case "UC5":
                                    reportFound = 1;
                                    int nResult = writeCSV8(strRead, CSVoutFile);
                                    if (nResult == 1)
                                    {
                                        strRead.Close();
                                        return;
                                    }
                                    break;
                            }   //  end switch
                        }
                        else if (cLine.Length >= 4 && cLine.Substring(0, 4) == reportToUse)
                        {
                            //  works just for VSM4 (CSV11)
                            reportFound = 1;
                            writeCSV11(strRead, CSVoutFile);
                        }   //  endif
                    }   //  endif not a blank line
                }   //  end while read
            }   //  end using
            if (reportFound == 0)
            {
                //  means the report was not included in the text output file
                string errMsg = reportToUse;
                errMsg += " not included in text output file.\nCannot create ";
                errMsg += currentReport;
                errMsg += " file.\nRerun text output file with this report.";
                DialogService.ShowError(errMsg);
                return;
            }   //  endif reportFound
            return;
        }   //  end OutputCSVfile

        //  Request from Region 5 to produce CSV file for KPI esitmates from TreeEstimate table
        public void OutputEstimateFile(string CSVoutFile)
        {
            //  This produces a CSV file from the TreeEstimate table
            //  if the table is empty it is either because there are no 3P strata or
            //  the file was created prior to March 2015 when the table was implemented.
            List<TreeEstimateDO> estimatesData = DataLayer.getTreeEstimates();
            if (estimatesData.Count == 0)
            {
                DialogService.ShowError("No estimate data for the CSV9 report.\r\nCannot produce the report.");
                return;
            }   //  endif
            //  create the csv list with desired fields
            List<CSVlist> outputList = new List<CSVlist>();
            foreach (TreeEstimateDO ed in estimatesData)
            {
                CSVlist c = new CSVlist();
                c.field1 = ed.CountTree.SampleGroup.Stratum.Code;
                c.field2 = ed.CountTree.CuttingUnit.Code;
                c.field3 = ed.CountTree.TreeDefaultValue.Species;
                c.field4 = ed.KPI.ToString();
                outputList.Add(c);
            }   //
            writeCSV9(outputList, CSVoutFile);
            return;
        }   //  end OutputEstimateFile

        public void OutputTimberTheft(string CSVoutFile)
        {
            List<TreeDO> tList = DataLayer.getTrees();
            List<TreeCalculatedValuesDO> tcvList = DataLayer.getTreeCalculatedValues();
            List<CSVlist> outputList = new List<CSVlist>();

            foreach (TreeDO td in tList)
            {
                if (td.CountOrMeasure == "M")
                {
                    CSVlist c = new CSVlist();
                    c.field1 = td.TreeNumber.ToString();
                    c.field2 = td.Species;
                    c.field3 = td.TreeDefaultValue.PrimaryProduct;
                    c.field4 = td.DRC.ToString();
                    c.field5 = td.DBH.ToString();
                    c.field6 = td.TotalHeight.ToString();
                    c.field7 = td.MerchHeightPrimary.ToString();
                    //  find calculated values for this tree
                    int nthRow = tcvList.FindIndex(
                        delegate (TreeCalculatedValuesDO tcv)
                    {
                        return td.Tree_CN == tcv.Tree_CN;
                    });

                    if (nthRow >= 0)
                    {
                        c.field8 = tcvList[nthRow].TotalCubicVolume.ToString();
                        c.field9 = tcvList[nthRow].GrossCUFTPP.ToString();
                        c.field10 = tcvList[nthRow].NetCUFTPP.ToString();
                        c.field11 = tcvList[nthRow].GrossBDFTPP.ToString();
                        c.field12 = tcvList[nthRow].NetBDFTPP.ToString();
                        c.field13 = tcvList[nthRow].NetCUFTSP.ToString();
                        c.field14 = tcvList[nthRow].NetBDFTSP.ToString();
                        c.field15 = tcvList[nthRow].CordsPP.ToString();
                        c.field16 = tcvList[nthRow].CordsSP.ToString();
                        c.field17 = tcvList[nthRow].NumberlogsMS.ToString();
                        c.field18 = tcvList[nthRow].NumberlogsTPW.ToString();
                    }   //  endif
                    outputList.Add(c);
                }   //  endif measured tree
            }   //  end foreach loop on trees
            writeCSV10(outputList, CSVoutFile);

            return;
        }   //  end OutputTimberTheft

        private void writeCSV(string reportToUse, StreamReader strRead, string CSVoutput)
        {
            //  make sure CSV file is not already open
            if (IsFileOpen(CSVoutput) == 1)
            {
                string errMsg = CSVoutput;
                errMsg += " file is open.\nCannot create file.";
                DialogService.ShowError(errMsg);
                return;
            }
            else if (File.Exists(CSVoutput))
                File.Delete(CSVoutput);

            string cLine;
            int numHeaderLines = 0;
            using (StreamWriter strCSVout = new StreamWriter(CSVoutput))
            {
                //  write header line in CSV for appropriate report
                switch (reportToUse)
                {
                    case "A05":
                        writeHeaderLine(headerA05, strCSVout, 19);
                        numHeaderLines = 18;
                        break;

                    case "A06":
                        writeHeaderLine(headerA06, strCSVout, 15);
                        numHeaderLines = 15;
                        break;

                    case "A07":
                        writeHeaderLine(headerA07, strCSVout, 19);
                        numHeaderLines = 19;
                        break;

                    case "A10":
                        writeHeaderLine(headerA10, strCSVout, 18);
                        numHeaderLines = 16;
                        break;

                    case "L1:":
                        writeHeaderLine(headerL1, strCSVout, 22);
                        numHeaderLines = 16;
                        break;

                    case "UC5":
                        writeHeaderLine(headerUC5, strCSVout, 13);
                        numHeaderLines = 13;
                        break;

                    case "VSM4":
                        writeHeaderLine(headerVSM4, strCSVout, 14);
                        numHeaderLines = 12;
                        break;
                }   //  end switch
                //  finish reading records and writing CSV file
                while ((cLine = strRead.ReadLine()) != null)
                {
                    if (cLine == "\f")
                    {
                        // read next line to see if we're still in the same report
                        cLine = strRead.ReadLine();
                        if (cLine.Substring(0, 3) != reportToUse)
                        {
                            strCSVout.Close();
                            return;
                        }
                        else if (cLine.Substring(0, 3) == reportToUse)
                        {
                            //  read pass headers
                            for (int k = 0; k < numHeaderLines; k++)
                                cLine = strRead.ReadLine();
                        }   //  endif
                    }   //  endif page break
                    if (cLine != "" && cLine != " " && cLine != null)
                    {
                        if (cLine.Substring(0, 3) != reportToUse)
                            writeLine(strCSVout, cLine);
                        else
                        {
                            //  read pass headers
                            for (int k = 0; k < numHeaderLines; k++)
                                cLine = strRead.ReadLine();
                        }   //  endif
                    }   //  endif
                }   //  end while read
            }   //  end using
            return;
        }   //  end writeCSV

        private void writeCSV7(string CSVoutFile, StreamReader strRead)
        {
            //  make sure CSV file is not already open
            if (IsFileOpen(CSVoutFile) == 1)
            {
                string errMsg = CSVoutFile;
                errMsg += " file is open.\nCannot create file.";
                DialogService.ShowError(errMsg);
                return;
            }
            //  reads ST1 report from text file and creates CSV file
            string cLine = "";
            //  output header lines for primary product
            using (StreamWriter strCSVout = new StreamWriter(CSVoutFile))
            {
                strCSVout.WriteLine("PRIMARY");
                for (int k = 0; k < 17; k++)
                {
                    strCSVout.Write(headerST1[k]);
                    strCSVout.Write(",");
                }   //  end for k loop
                strCSVout.WriteLine(headerST1[17]);

                //  skip header lines in report
                for (int k = 0; k < 16; k++)
                    cLine = strRead.ReadLine();

                //  write line
                while ((cLine = strRead.ReadLine()) != null)
                {
                    if (cLine == "\f")
                    {
                        //  make sure still in ST1
                        cLine = strRead.ReadLine();
                        if (cLine.Substring(0, 3) != "ST1")
                        {
                            strCSVout.Close();
                            return;
                        }
                        else if (cLine.Substring(0, 3) == "ST1")
                        {
                            //  skip header but look for product
                            for (int k = 0; k < 16; k++)
                            {
                                if (cLine.Contains("SECONDARY"))
                                    strCSVout.WriteLine("SECONDARY");
                                else if (cLine.Contains("PRIMARY"))
                                    strCSVout.WriteLine("PRIMARY");
                                else if (cLine.Contains("RECOVERED"))
                                    strCSVout.WriteLine("RECOVERED");
                                cLine = strRead.ReadLine();
                            }   //  end for k loop
                            //  output header to CSV file
                            for (int k = 0; k < 17; k++)
                            {
                                strCSVout.Write(headerST1[k]);
                                strCSVout.Write(",");
                            }
                            strCSVout.WriteLine(headerST1[17]);
                        }
                    }
                    else if (cLine != "" && cLine != " " && cLine != null)
                    {
                        if (cLine.Substring(0, 3) != "ST1" && cLine.Substring(0, 3) != "  *")
                            writeLine(strCSVout, cLine);
                    }   //  endif
                }   //  end while read
                strCSVout.Close();
            }   //  end using
            return;
        }   //  end writeCSV7

        private int writeCSV8(StreamReader strRead, string CSVoutFile)
        {
            //  make sure CSV file is not already open
            if (IsFileOpen(CSVoutFile) == 1)
            {
                string errMsg = CSVoutFile;
                errMsg += " file is open.\nCannot create file.";
                DialogService.ShowError(errMsg);
                return 1;
            }
            //  reads UC5 report from text file and crestes CSV files
            string cLine = "";
            //  output header lines
            using (StreamWriter strCSVout = new StreamWriter(CSVoutFile))
            {
                for (int k = 0; k < 12; k++)
                {
                    strCSVout.Write(headerUC5[k]);
                    strCSVout.Write(",");
                }
                strCSVout.WriteLine(headerUC5[12]);

                //  skip header lines in report
                for (int k = 0; k < 14; k++)
                    cLine = strRead.ReadLine();

                //  write lines as needed skipping unit subtotals and quitting at subtotal summary
                while ((cLine = strRead.ReadLine()) != null)
                {
                    if (cLine == "\f")
                    {
                        //  still in UC report?
                        cLine = strRead.ReadLine();
                        if (cLine.Substring(0, 3) != "UC5")
                        {
                            strCSVout.Close();
                            return 1;
                        }
                        else if (cLine.Substring(0, 3) == "UC5")
                        {
                            //  skip header
                            for (int j = 0; j < 14; j++)
                                cLine = strRead.ReadLine();
                        }   //  endif
                    }
                    else if (cLine != "" && cLine != " " && cLine != null)
                    {
                        if (cLine.Contains("___"))
                        {
                            //  read unit subtotal lines
                            cLine = strRead.ReadLine();
                            cLine = strRead.ReadLine();
                        }
                        else if (cLine.Contains(" S U B"))
                        {
                            strCSVout.Close();
                            return 1;
                        }
                        else if (cLine.Substring(0, 3) != "UC5")
                            writeCSV8(strCSVout, cLine);
                    }   //  endif
                }   //  end while

                strCSVout.Close();
            }   //  end using
            return 1;
        }   //  end writeCSV8

        private int writeCSV11(StreamReader strRead, string CSVoutFile)
        {
            string currST = "";
            string currSG = "";
            string currTree = "";
            //  make sure CSV file is not already open
            if (IsFileOpen(CSVoutFile) == 1)
            {
                string errMsg = CSVoutFile;
                errMsg += " file is open.\nCannot create file.";
                DialogService.ShowError(errMsg);
                return 1;
            }

            //  reads VSM4 report from text file and creates CSV file
            string cLine = "";
            using (StreamWriter strCSVout = new StreamWriter(CSVoutFile))
            {
                //  write headers to CSV file
                for (int k = 0; k < 13; k++)
                {
                    strCSVout.Write(headerVSM4[k]);
                    strCSVout.Write(",");
                }   //  end for k loop
                strCSVout.WriteLine(headerVSM4[13]);

                //  skip header lines in report
                for (int k = 0; k < 10; k++)
                    cLine = strRead.ReadLine();

                //  write lines asa needed skipping subtotal sections
                while ((cLine = strRead.ReadLine()) != null)
                {
                    if (cLine == "\f")
                    {
                        //  still in VSM4 report?
                        cLine = strRead.ReadLine();
                        if (cLine.Substring(0, 4) != "VSM4")
                        {
                            strCSVout.Close();
                            return 1;
                        }
                        else if (cLine.Substring(0, 4) == "VSM4")
                        {
                            //  skip header
                            for (int j = 0; j < 10; j++)
                                cLine = strRead.ReadLine();
                        }   //  endif
                    }
                    else if (cLine != "" && cLine != " " && cLine != null)
                    {
                        if (cLine.Contains("_____"))
                        {
                            //  Read through subtotal lines
                            cLine = strRead.ReadLine();
                            cLine = strRead.ReadLine();
                            cLine = strRead.ReadLine();
                        }
                        else if (cLine.Substring(0, 4) != "VSM4")
                        {
                            //  capture certain fields
                            if (cLine.Substring(4, 2) != "  ")
                            {
                                currST = cLine.Substring(3, 2);
                                currSG = cLine.Substring(12, 2);
                                currTree = cLine.Substring(34, 4);
                            }
                            //  then write the record using these values
                            strCSVout.Write(currST);
                            strCSVout.Write(",");
                            strCSVout.Write(currSG, ",,");
                            strCSVout.Write(",");
                            strCSVout.Write(cLine.Substring(20, 3));
                            strCSVout.Write(",");
                            strCSVout.Write(cLine.Substring(27, 4));
                            strCSVout.Write(",");
                            strCSVout.Write(currTree);
                            strCSVout.Write(",");
                            strCSVout.Write(cLine.Substring(40, 6));
                            strCSVout.Write(",");
                            strCSVout.Write(cLine.Substring(55, 5));     // FBH
                            strCSVout.Write(",");
                            strCSVout.Write(cLine.Substring(58, 7));
                            strCSVout.Write(",");
                            strCSVout.Write(cLine.Substring(68, 7));
                            strCSVout.Write(",");
                            strCSVout.Write(cLine.Substring(79, 7));
                            strCSVout.Write(",");
                            strCSVout.Write(cLine.Substring(91, 5));     //  KPI
                            strCSVout.Write(",");
                            strCSVout.Write(cLine.Substring(103, 6));
                            strCSVout.Write(",");
                            strCSVout.Write(cLine.Substring(109, 6));    //  ratio
                            strCSVout.Write(",");
                            strCSVout.WriteLine(cLine.Substring(121, 3));
                        }   //  endif
                    }   //  endif
                }   //  end whiole

                strCSVout.Close();
            }   //  end using

            return 1;
        }   //  end writeCSV11

        private void writeHeaderLine(string[] currHeader, TextWriter strCSVout, int numFields)
        {
            for (int k = 0; k < numFields - 1; k++)
            {
                strCSVout.Write(currHeader[k]);
                strCSVout.Write(",");
            }   //  end for k loop
            //  last field
            strCSVout.WriteLine(currHeader[numFields - 1]);
            return;
        }   //  end writeHeaderLine

        private void writeLine(TextWriter strCSVout, string currLine)
        {
            //  writes each field on the current line to the CSV file with a comma
            StringBuilder currField = new StringBuilder();
            if (currLine.Substring(0, 10) == "__________") return;

            for (int j = 0; j < currLine.Length; j++)
            {
                if (currentReport == "CSV5" && j == 12 && currLine[j] == ' ')
                    currField.Append(" ");
                else if (currentReport == "CSV5" && j == 17 && currLine[j] == ' ')
                    currField.Append(" ");
                else if ((currentReport == "CSV1" || currentReport == "CSV3" || currentReport == "CSV4")
                            && currLine[j] == ' ' && j == 11)
                    currField.Append(" ");
                else if ((currentReport == "CSV1" || currentReport == "CSV3" || currentReport == "CSV4")
                            && currLine[j] == ' ' && j == 16)
                    currField.Append(" ");
                else if (currentReport != "CSV5" && currField.Length == 0 && j == 38)
                {
                    if (currentReport != "CSV7")
                    {
                        //  also second height if not there
                        currField.Append(" ");
                    }   //  endif
                }
                else if (currLine[j] != ' ')
                    currField.Append(currLine[j]);
                else if (currLine[j] == ' ')
                {
                    if (currField.Length > 0)
                    {
                        strCSVout.Write(currField.ToString());
                        strCSVout.Write(",");
                        currField.Remove(0, currField.Length);
                    }   //  endif
                }   //  endif
            }   //  end for j loop
            //  write last field
            strCSVout.WriteLine(currField.ToString());
            return;
        }   //  end writeLine

        private void writeCSV6(string CSVoutFile, StreamReader strRead)
        {
            //  reads L2 report and creates CSV file
            string headerLine = "";
            string nextTable = "";
            string cLine = "";
            List<CSVlist> fileToOutput = new List<CSVlist>();
            //  start header line with forest, district and cruise number
            CSVlist fto = new CSVlist();
            headerLine = DataLayer.getForest().PadLeft(2, ' ');
            headerLine += DataLayer.getDistrict();
            headerLine += DataLayer.getCruiseNumber().PadRight(5, ' ');

            //  need to get pass first header group
            for (int k = 0; k < 6; k++)
                cLine = strRead.ReadLine();
            if (cLine.Substring(0, 6) == " TABLE")
            {
                nextTable = cLine.Substring(6, 3);
                string currSP = cLine.Substring(31, cLine.Length - 31);
                headerLine += currSP.PadRight(6, ' ');
            }   //  endif
            //  read rest of header lines
            for (int k = 0; k < 5; k++)
                cLine = strRead.ReadLine();

            while ((cLine = strRead.ReadLine()) != null)
            {
                if (cLine == "\f")
                {
                    //  read next line to see if still in L2 report
                    cLine = strRead.ReadLine();
                    if (cLine.Substring(0, 2) != "L2")
                    {
                        strRead.Close();
                        break;
                    }
                    else if (cLine.Substring(0, 2) == "L2")
                    {
                        //  need table number for body of report and species for header line
                        //  so read to that line
                        for (int j = 0; j < 6; j++)
                            cLine = strRead.ReadLine();

                        if (cLine.Substring(0, 6) == " TABLE")
                        {
                            nextTable = cLine.Substring(6, 3);
                            headerLine += cLine.Substring(33, 6).PadRight(6, ' ');
                        }   //  endif
                        //  read rest of header lines
                        for (int j = 0; j < 6; j++)
                            cLine = strRead.ReadLine();
                    }   //  endif
                }
                else if (cLine.Substring(0, 3) != "___" && cLine.Substring(0, 4) != " TOT")
                {
                    //  load line in list
                    CSVlist c = new CSVlist();
                    c.field1 = nextTable.PadLeft(7, ' ');
                    c.field2 = cLine.Substring(0, 7).PadLeft(7, ' ');       //  DIB class
                    c.field3 = cLine.Substring(8, 10).PadLeft(10, ' ');     //  grade 0
                    c.field4 = cLine.Substring(18, 10).PadLeft(10, ' ');    //  grade 1
                    c.field5 = cLine.Substring(28, 10).PadLeft(10, ' ');    //  grade 2
                    c.field6 = cLine.Substring(38, 10).PadLeft(10, ' ');    //  grade 3
                    c.field7 = cLine.Substring(48, 10).PadLeft(10, ' ');    //  grade 4
                    c.field8 = cLine.Substring(58, 10).PadLeft(10, ' ');    //  grade 5
                    c.field9 = cLine.Substring(68, 10).PadLeft(10, ' ');    //  grade 6
                    c.field10 = cLine.Substring(78, 10).PadLeft(10, ' ');   //  grade 7
                    fileToOutput.Add(c);
                }   //  endif
            }   //  end while read
            // one last thing to go into header line -- run year -- just use the current year
            string currYear = DateTime.Now.Year.ToString();
            headerLine += "        RY";
            headerLine += currYear;

            outputCSV6list(fileToOutput, CSVoutFile, headerLine);
            return;
        }   //  end writeCSV6

        private void writeCSV8(TextWriter strCSVout, string currLine)
        {
            //  writes each field on the current line to the CSV* file with a comma
            StringBuilder currField = new StringBuilder();
            for (int j = 0; j < currLine.Length; j++)
            {
                if (currLine[j] != ' ')
                    currField.Append(currLine[j]);
                else if (currLine[j] == ' ')
                {
                    if (currField.Length > 0)
                    {
                        strCSVout.Write(currField.ToString());
                        strCSVout.Write(",");
                        currField.Remove(0, currField.Length);
                    }   //  endif
                }   //  endif
            }   //  end for j loop
            //  write last field
            strCSVout.WriteLine(currField.ToString());

            return;
        }   //  end writeCSV8

        private void writeCSV9(List<CSVlist> outputList, string CSVoutFile)
        {
            //  make sure file is not already open
            if (IsFileOpen(CSVoutFile) == 1)
            {
                string errMsg = CSVoutFile;
                errMsg += " file is open.\nCannot create the file.";
                DialogService.ShowError(errMsg);
                return;
            }   //  endif

            using (StreamWriter strCSVout = new StreamWriter(CSVoutFile))
            {
                //  output header
                for (int k = 0; k < 3; k++)
                {
                    strCSVout.Write(headerKPI[k]);
                    strCSVout.Write(",");
                }   //  end for k loop
                strCSVout.WriteLine(headerKPI[3]);
                //  write outputList
                foreach (CSVlist ol in outputList)
                {
                    strCSVout.Write(ol.field1);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field2);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field3);
                    strCSVout.Write(",");
                    strCSVout.WriteLine(ol.field4);
                }   //  end foreach loop
                strCSVout.Close();
            }   //  end using
            return;
        }   //  end writeCSV9

        private void writeCSV10(List<CSVlist> outputList, string CSVoutFile)
        {
            //  make sure file is not already open
            if (IsFileOpen(CSVoutFile) == 1)
            {
                string errMsg = CSVoutFile;
                errMsg += " file is open.\nCannot create the file.";
                DialogService.ShowError(errMsg);
                return;
            }   //  endif

            using (StreamWriter strCSVout = new StreamWriter(CSVoutFile))
            {
                //  output header line
                for (int k = 0; k < 17; k++)
                {
                    strCSVout.Write(headerTheft[k]);
                    strCSVout.Write(",");
                }   //  end for k loop
                strCSVout.WriteLine(headerTheft[17]);
                // write data
                foreach (CSVlist ol in outputList)
                {
                    strCSVout.Write(ol.field1);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field2);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field3);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field4);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field5);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field6);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field7);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field8);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field9);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field10);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field11);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field12);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field13);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field14);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field15);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field16);
                    strCSVout.Write(",");
                    strCSVout.Write(ol.field17);
                    strCSVout.Write(",");

                    strCSVout.WriteLine(ol.field18);
                }   //  end foreach loop
                strCSVout.Close();
            }   //  end using
            return;
        }   //  end writeCSV10

        private void outputCSV6list(List<CSVlist> listToOutput, string CSVoutFile, string headLine)
        {
            //  make sure CSV file is not already open
            if (IsFileOpen(CSVoutFile) == 1)
            {
                string errMsg = CSVoutFile;
                errMsg += " file is open.\nCannot create file.";
                DialogService.ShowError(errMsg);
                return;
            }
            else if (File.Exists(CSVoutFile))
                File.Delete(CSVoutFile);

            using (StreamWriter strWriteCSV = new StreamWriter(CSVoutFile))
            {
                //  first line is header line
                strWriteCSV.WriteLine(headLine);
                foreach (CSVlist lto in listToOutput)
                {
                    strWriteCSV.Write(lto.field1);
                    strWriteCSV.Write(",");
                    strWriteCSV.Write(lto.field2);
                    strWriteCSV.Write(",");
                    strWriteCSV.Write(lto.field3);
                    strWriteCSV.Write(",");
                    strWriteCSV.Write(lto.field4);
                    strWriteCSV.Write(",");
                    strWriteCSV.Write(lto.field5);
                    strWriteCSV.Write(",");
                    strWriteCSV.Write(lto.field6);
                    strWriteCSV.Write(",");
                    strWriteCSV.Write(lto.field7);
                    strWriteCSV.Write(",");
                    strWriteCSV.Write(lto.field8);
                    strWriteCSV.Write(",");
                    strWriteCSV.Write(lto.field9);
                    strWriteCSV.Write(",");
                    strWriteCSV.WriteLine(lto.field10);
                }       //  end foreach loop
                strWriteCSV.Close();
            }   //  end using
        }   //  end outputCSV6list

        private void writeCSV(string outputFileName, List<CSVlist> ListToOutput)
        {
            //  overloaded to write CSV files for Region 10
            StringBuilder sb = new StringBuilder();
            string deLimiter = ",";
            using (StreamWriter strCSVout = new StreamWriter(outputFileName))
            {
                //  write header line
                writeHeaderLine(headerL1R10, strCSVout, 46);

                foreach (CSVlist lto in ListToOutput)
                {
                    for (int k = 0; k <= 46; k++)
                    {
                        switch (k)
                        {
                            case 0:
                                sb.Append(lto.field1);
                                break;

                            case 1:
                                sb.Append(lto.field2);
                                break;

                            case 2:
                                sb.Append(lto.field3);
                                break;

                            case 3:
                                sb.Append(lto.field4);
                                break;

                            case 4:
                                sb.Append(lto.field5);
                                break;

                            case 5:
                                sb.Append(lto.field6);
                                break;

                            case 6:
                                sb.Append(lto.field7);
                                break;

                            case 7:
                                sb.Append(lto.field8);
                                break;

                            case 8:
                                sb.Append(lto.field9);
                                break;

                            case 9:
                                sb.Append(lto.field10);
                                break;

                            case 10:
                                sb.Append(lto.field11);
                                break;

                            case 11:
                                sb.Append(lto.field12);
                                break;

                            case 12:
                                sb.Append(lto.field13);
                                break;

                            case 13:
                                sb.Append(lto.field14);
                                break;

                            case 14:
                                sb.Append(lto.field15);
                                break;

                            case 15:
                                sb.Append(lto.field16);
                                break;

                            case 16:
                                sb.Append(lto.field17);
                                break;

                            case 17:
                                sb.Append(lto.field18);
                                break;

                            case 18:
                                sb.Append(lto.field19);
                                break;

                            case 19:
                                sb.Append(lto.field20);
                                break;

                            case 20:
                                sb.Append(lto.field21);
                                break;

                            case 21:
                                sb.Append(lto.field22);
                                break;

                            case 22:
                                sb.Append(lto.field23);
                                break;

                            case 23:
                                sb.Append(lto.field24);
                                break;

                            case 24:
                                sb.Append(lto.field25);
                                break;

                            case 25:
                                sb.Append(lto.field26);
                                break;

                            case 26:
                                sb.Append(lto.field27);
                                break;

                            case 27:
                                sb.Append(lto.field28);
                                break;

                            case 28:
                                sb.Append(lto.field29);
                                break;

                            case 29:
                                sb.Append(lto.field30);
                                break;

                            case 30:
                                sb.Append(lto.field31);
                                break;

                            case 31:
                                sb.Append(lto.field32);
                                break;

                            case 32:
                                sb.Append(lto.field33);
                                break;

                            case 33:
                                sb.Append(lto.field34);
                                break;

                            case 34:
                                sb.Append(lto.field35);
                                break;

                            case 35:
                                sb.Append(lto.field36);
                                break;

                            case 36:
                                sb.Append(lto.field37);
                                break;

                            case 37:
                                sb.Append(lto.field38);
                                break;

                            case 38:
                                sb.Append(lto.field39);
                                break;

                            case 39:
                                sb.Append(lto.field40);
                                break;

                            case 40:
                                sb.Append(lto.field41);
                                break;

                            case 41:
                                sb.Append(lto.field42);
                                break;

                            case 42:
                                sb.Append(lto.field43);
                                break;

                            case 43:
                                sb.Append(lto.field44);
                                break;

                            case 44:
                                sb.Append(lto.field45);
                                break;

                            case 45:
                                sb.Append(lto.field46);
                                break;
                        }   //  end switch on k
                        if (k != 46) sb.Append(deLimiter);
                    }   //  end for k loop
                    strCSVout.WriteLine(sb.ToString());
                    sb.Clear();
                }   //  end foreach loop
                strCSVout.Close();
            }   //  end using
            return;
        }   //  end writeCSV

        private int IsFileOpen(string outputFileName)
        {
            try
            {
                FileStream fs = new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
                fs.Close();
            }
            catch
            {
                return 1;
            }   //  end try catch

            return 0;
        }   //  end IsFileOpen

        private List<CSVlist> LoadLogStockR10(string currCL)
        {
            //  Region 10 does the L1 report differently from all other regions
            //  If the cruise has leave trees, it needs a separate file
            //  otherwise, it would be cut trees only.
            //  So this needs to loop through both and create separate lists to output
            //  open tables needed
            var sale = DataLayer.GetSale();
            List<LogStockDO> logList = DataLayer.getCutOrLeaveLogs(currCL);
            List<CSVlist> lsList = new List<CSVlist>();
            List<LogDO> origLogs = DataLayer.getLogs();

            //  Load list to return
            string currentCruise = sale.SaleNumber;
            string currentForest = sale.Forest;
            string currentDistrict = sale.District;
            string currentSalename = sale.Name;
            double strAcres = 0.0;

            foreach (LogStockDO ll in logList)
            {
                if (ll.Tree.CountOrMeasure == "M")
                {
                    //  what is stratum acres?
                    strAcres = Utilities.ReturnCorrectAcres(ll.Tree.Stratum.Code, DataLayer, (long)ll.Tree.Stratum_CN);

                    //  Find all logs for this tree
                    CSVlist c = new CSVlist();
                    c.field1 = currentCruise;
                    c.field2 = ll.Tree.Stratum.Code;
                    c.field3 = ll.Tree.CuttingUnit.Code;
                    if (ll.Tree.Plot == null)
                        c.field4 = " ";
                    else c.field4 = ll.Tree.Plot.PlotNumber.ToString();
                    c.field5 = ll.Tree.TreeNumber.ToString();
                    if (ll.Tree.SampleGroup == null)
                    {
                        c.field6 = " ";
                        c.field7 = " ";
                        c.field10 = " ";
                        c.field11 = " ";
                    }
                    else
                    {
                        c.field6 = ll.Tree.SampleGroup.CutLeave;
                        c.field7 = ll.Tree.SampleGroup.Code;
                        c.field10 = ll.Tree.SampleGroup.PrimaryProduct;
                        c.field11 = ll.Tree.SampleGroup.UOM;
                    }   //  endif
                    c.field8 = ll.Tree.STM;
                    c.field9 = ll.Tree.Species;
                    c.field12 = ll.Tree.LiveDead;
                    if (ll.Tree.TreeDefaultValue == null)
                    {
                        c.field13 = " ";
                        c.field14 = " ";
                    }
                    else
                    {
                        c.field13 = ll.Tree.Stratum.YieldComponent;
                        //c.field13 = ll.Tree.TreeDefaultValue.Chargeable;
                        if (ll.Tree.TreeDefaultValue.ContractSpecies == null)
                            c.field14 = " ";
                        else c.field14 = ll.Tree.TreeDefaultValue.ContractSpecies;
                    }   //  endif
                    c.field15 = ll.Tree.Grade;
                    c.field16 = ll.LogNumber;
                    c.field17 = ll.SmallEndDiameter.ToString();
                    c.field18 = ll.LargeEndDiameter.ToString();
                    c.field19 = ll.Length.ToString();
                    c.field20 = "   0.00";      //  value doesn't seem to be in the logstock table anymore
                    c.field21 = ll.Grade;
                    c.field22 = ll.SeenDefect.ToString();
                    c.field23 = ll.PercentRecoverable.ToString();
                    c.field24 = ll.GrossBoardFoot.ToString();
                    c.field25 = ll.BoardFootRemoved.ToString();
                    c.field26 = ll.NetBoardFoot.ToString();
                    c.field27 = ll.GrossCubicFoot.ToString();
                    c.field28 = ll.CubicFootRemoved.ToString();
                    c.field29 = ll.NetCubicFoot.ToString();
                    c.field20 = ll.DIBClass.ToString();
                    c.field31 = ll.Tree.ExpansionFactor.ToString();
                    c.field32 = strAcres.ToString();
                    c.field33 = ll.ExportGrade;
                    //  utility is calculated as follows
                    c.field34 = (ll.NetCubicFoot / 100 * ll.PercentRecoverable).ToString();
                    c.field35 = (ll.NetBoardFoot / 100 * ll.PercentRecoverable).ToString();
                    c.field36 = ll.Tree.DBH.ToString();
                    c.field37 = ll.Tree.TotalHeight.ToString();
                    c.field38 = ll.Tree.CuttingUnit.Area.ToString();
                    c.field39 = ll.Tree.XCoordinate.ToString();
                    c.field40 = ll.Tree.YCoordinate.ToString();
                    c.field41 = ll.Tree.ZCoordinate.ToString();
                    c.field42 = currentForest;
                    c.field43 = currentDistrict;
                    c.field44 = currentSalename;
                    if (ll.Tree.CuttingUnit.LoggingMethod == "" ||
                        ll.Tree.CuttingUnit.LoggingMethod == " " ||
                        ll.Tree.CuttingUnit.LoggingMethod == null)
                        c.field45 = "   ";
                    else c.field45 = ll.Tree.CuttingUnit.LoggingMethod;
                    //  have to look up seen defect from original logs
                    int nthRow = origLogs.FindIndex(
                        delegate (LogDO ld)
                        {
                            return ld.Tree_CN == ll.Tree.Tree_CN && ld.LogNumber == ll.LogNumber;
                        });
                    if (nthRow >= 0)
                        c.field46 = origLogs[nthRow].SeenDefect.ToString();
                    else c.field46 = " ";
                    lsList.Add(c);
                }   //  endif measured tree
            }   //  end foreach loop

            return lsList;
        }   //  end LoadLogStockR10
    }
}