using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Output
{
    public class OutputFileReportGeneratorBase : ReportGeneratorBase
    {
        public readonly string[] reportTitles = new string[3];

        public HeaderFieldData HeaderData { get; set; }

        [Obsolete("Use HeaderData instead")]
        protected string currentDate => HeaderData.Date; // only used in ErrorReport.cs
        [Obsolete("Use HeaderData instead")]
        protected string currentVersion => HeaderData.Version; // only used in OutputTim

        public OutputFileReportGeneratorBase(CPbusinessLayer dataLayer, HeaderFieldData headerData, string reportID = "") : base(dataLayer, reportID)
        {
            HeaderData = headerData;
        }

        protected void printOneRecord(int[] fieldLengths, IEnumerable<string> prtFields, TextWriter strWriteOut)
        {
            var oneRecord = buildPrintLine(fieldLengths, prtFields);
            strWriteOut.WriteLine(oneRecord);
            numOlines++;
        }

        protected void printOneRecord(TextWriter strWriteOut, IEnumerable<string> prtFields)
        {
            StringBuilder oneRecord = new StringBuilder();
            foreach (var str in prtFields)
            {
                oneRecord.Append(str);
            }
            strWriteOut.WriteLine(oneRecord.ToString());
            numOlines++;
        }

        protected void WriteReportHeading(TextWriter strWriteOut, string TitleOne, string TitleTwo,
                                        string TitleThree, string[] headerToPrint, int lineIncrement,
                                        ref int pageNumber, string extraHeader)
        {
            numOlines = WriteReportHeading(strWriteOut, TitleOne, TitleTwo, TitleThree,
                headerToPrint, lineIncrement, ref pageNumber, extraHeader, HeaderData, numOlines);
        }

        public static int WriteReportHeading(TextWriter strWriteOut, string TitleOne, string TitleTwo,
                                string TitleThree, string[] headerToPrint, int lineIncrement,
                                ref int pageNumber, string extraHeader, HeaderFieldData headerData, int numOlines, IReadOnlyList<string> reportTitles = null)
        {
            reportTitles ??= new string[3];

            if (numOlines == 0 || numOlines >= 50)
            {
                //  Page break
                strWriteOut.WriteLine("\f");
                numOlines = 0;

                //  Write report main heading
                var pageNum = pageNumber++;
                outputReportHeader(strWriteOut, headerData, TitleOne, TitleTwo, TitleThree, pageNum, reportTitles);
                strWriteOut.WriteLine();

                if (extraHeader != "")
                {
                    strWriteOut.WriteLine(extraHeader);
                    strWriteOut.WriteLine();
                }   //  endif extra header needed

                //  write column headers
                for (int k = 0; k < headerToPrint.Count(); k++)
                {
                    if (headerToPrint[k] == null)
                        lineIncrement--;
                    else strWriteOut.WriteLine(headerToPrint[k]);
                }   //  end for loop
                strWriteOut.WriteLine(reportConstants.longLine);
                numOlines++;
                numOlines += lineIncrement;
            }   //  endif numOlines
            return numOlines;
        }

        // TODO we can remove mainHeaderData as a pram now that this method is in our report generator class
        public void outputReportHeader(TextWriter strWriteOut, IReadOnlyList<string> mainHeaderData, string titleOne,
                                string titleTwo, string titleThree, int pageNum)
        {
            outputReportHeader(strWriteOut, HeaderData, titleOne, titleTwo, titleThree, pageNum, reportTitles);
        }

        public static void outputReportHeader(TextWriter strWriteOut, IReadOnlyList<string> mainHeaderData, string titleOne,
                                string titleTwo, string titleThree, int pageNum, IReadOnlyList<string> reportTitles)
        {

            //  write report title and page number
            var sb = new StringBuilder();
            sb.Append(titleOne.PadRight(124, ' '));
            sb.Append("PAGE ");
            sb.Append(pageNum);
            strWriteOut.WriteLine(sb.ToString());

            //  if subtitles present, output those
            if ((reportTitles[1] != "" && reportTitles[1] != null) || titleTwo != "")
                strWriteOut.WriteLine(titleTwo);
            if ((reportTitles[2] != "" && reportTitles[2] != null) || titleThree != "")
                strWriteOut.WriteLine(titleThree);

            //  write remaining info
            sb.Clear();
            sb.Append("CRUISE#: ");
            sb.Append(mainHeaderData[3].ToString().PadRight(5, ' '));
            sb.Append("      SALE#: ");
            sb.Append(mainHeaderData[3].ToString().PadRight(5, ' '));
            strWriteOut.WriteLine(sb.ToString());

            sb.Clear();
            sb.Append("SALENAME: ");
            sb.Append(mainHeaderData[4].ToString().PadRight(103, ' '));
            sb.Append("VERSION: ");
            sb.Append(mainHeaderData[1]);
            strWriteOut.WriteLine(sb.ToString());

            sb.Clear();
            sb.Append("RUN DATE & TIME: ");
            sb.Append(mainHeaderData[0]);
            sb.Append("                                                             VOLUME LIBRARY VERSION: ");
            sb.Append(mainHeaderData[2]);
            strWriteOut.WriteLine(sb.ToString());

            strWriteOut.WriteLine();
            return;
        }

        public void SetReportTitles(string currentTitle, int doWhat, int whereSplit1, int whereSplit2,
                                       string whichReportConstant, string secondReportConstant)
        {
            reportTitles[0] = "";
            reportTitles[1] = "";
            reportTitles[2] = "";

            //  what's done depends on doWhat
            switch (doWhat)
            {
                case 1:     //  just upper case the title and put in report title array
                    reportTitles[0] = currentTitle.ToUpper();
                    break;
                case 2:     //  upper case and split once
                    reportTitles[0] = currentTitle.Substring(0, whereSplit1).ToUpper();
                    reportTitles[1] = currentTitle.Substring(whereSplit1, currentTitle.Length - whereSplit1).ToUpper();
                    break;
                case 3:     //  upper case and split twice
                    reportTitles[0] = currentTitle.Substring(0, whereSplit1).ToUpper();
                    reportTitles[1] = currentTitle.Substring(whereSplit1, whereSplit2 - whereSplit1).ToUpper();
                    reportTitles[2] = currentTitle.Substring(whereSplit2, currentTitle.Length - whereSplit2).ToUpper();
                    break;
                case 4:     //  two line split with report constant
                    reportTitles[0] = currentTitle.Substring(0, whereSplit1).ToUpper();
                    reportTitles[1] = currentTitle.Substring(whereSplit1, currentTitle.Length - whereSplit1).ToUpper();
                    reportTitles[2] = whichReportConstant;
                    break;
                case 5:     //  one line with report constant
                    reportTitles[0] = currentTitle.ToUpper();
                    reportTitles[1] = whichReportConstant;
                    break;
                case 6:     //  one line with two report constants
                    reportTitles[0] = currentTitle.ToUpper();
                    reportTitles[1] = whichReportConstant;
                    reportTitles[2] = secondReportConstant;
                    break;
                case 7:     //  upper case and split one with report constant
                    reportTitles[0] = currentTitle.Substring(0, whereSplit1).ToUpper();
                    reportTitles[1] = currentTitle.Substring(whereSplit1, currentTitle.Length - whereSplit1).ToUpper();
                    reportTitles[2] = whichReportConstant;
                    break;

            }   //  end switch on doWhat

            return;
        }

        protected static void whichHeightFields(out HeightFieldType hgtOne, out HeightFieldType hgtTwo, List<TreeDO> tList)
        {
            hgtOne = HeightFieldType.Unknown;
            hgtTwo = HeightFieldType.Unknown;

            double totalHeightSum = tList.Sum(x => Convert.ToDouble(x.TotalHeight));

            if (totalHeightSum > 0.0)
                hgtOne = HeightFieldType.Total;

            double merchHeightSum = tList.Sum(x => Convert.ToDouble(x.MerchHeightPrimary));
            if (merchHeightSum > 0.0)
            {
                if (hgtOne == HeightFieldType.Unknown)
                {
                    hgtOne = HeightFieldType.MerchPrimary;
                }
                else
                {
                    hgtTwo = HeightFieldType.MerchPrimary;
                    return;
                }
            }

            var mechHeightSecondSum = tList.Sum(x => Convert.ToDouble(x.MerchHeightSecondary));
            if (mechHeightSecondSum > 0.0)
            {
                if (hgtOne == HeightFieldType.Unknown)
                {
                    hgtOne = HeightFieldType.MerchSecondary;
                }
                else
                {
                    hgtTwo = HeightFieldType.MerchSecondary;
                    return;
                }
            }

            var upperStemHeightSum = tList.Sum(x => Convert.ToDouble(x.UpperStemHeight));

            if (upperStemHeightSum > 0.0)
            {
                if (hgtOne == HeightFieldType.Unknown)
                {
                    hgtOne = HeightFieldType.UpperStem;
                }
                else
                {
                    hgtTwo = HeightFieldType.UpperStem;
                    return;
                }
            }
        }

        protected static string[] updateHeightHeader(HeightFieldType hgtOne, HeightFieldType hgtTwo, string titlePrefix, string[] headerToUpdate)
        {
            string[] updatedHeader = new string[headerToUpdate.Count()];
            string header1 = "   TOT HGT";
            string header2 = " MRHT PRPD";
            string header3 = " MRHT SEPD";
            string header4 = " HT U STEM";
            StringBuilder sb = new StringBuilder();
            sb.Append(titlePrefix);
            switch ((int)hgtOne)
            {
                case 1:
                    if (titlePrefix.Length > 0)
                    {
                        sb.Insert(0, "  ");
                        sb.Append(header1.Substring(2, 8));
                    }
                    else sb.Append(header1);
                    break;
                case 2:
                    sb.Append(header2);
                    break;
                case 3:
                    sb.Append(header3);
                    break;
                case 4:
                    sb.Append(header4);
                    break;
            }   //  end switch on hgtOne


            //  apply change to header
            for (int k = 0; k < headerToUpdate.Count(); k++)
                updatedHeader[k] = headerToUpdate[k].Replace("K", sb[k].ToString());

            //  second height
            if (hgtTwo != HeightFieldType.Unknown)
            {
                sb.Clear();
                sb.Append(titlePrefix);
                switch ((int)hgtTwo)
                {
                    case 1:
                        if (titlePrefix.Length > 0)
                        {
                            sb.Insert(0, "  ");
                            sb.Append(header1.Substring(2, 8));
                        }
                        else sb.Append(header1);
                        break;
                    case 2:
                        sb.Append(header2);
                        break;
                    case 3:
                        sb.Append(header3);
                        break;
                    case 4:
                        sb.Append(header4);
                        break;
                }   //  end switch on hgtTwo

            }
            else
            {
                //  make sb blank
                sb.Clear();
                for (int j = 0; j < headerToUpdate.Count(); j++)
                    sb.Append(" ");
            }   //  endif hgtTwo has a value

            //  apply change to header
            for (int k = 0; k < headerToUpdate.Count(); k++)
                updatedHeader[k] = updatedHeader[k].Replace("Z", sb[k].ToString());

            return updatedHeader;
        }

        protected static string fillReportTitle(string currReport)
        {
            string currTitle = allReportsArray.findReportTitle(currReport);
            //  Add report number to title
            currTitle = currTitle.Insert(0, ": ");
            currTitle = currTitle.Insert(0, currReport);
            return currTitle;
        }

        public static void noDataForReport(TextWriter strWriteOut, string reportToPrint, string reportMessage)
        {
            //  output message that the report has no data so report could not be generated
            strWriteOut.WriteLine("\f");
            strWriteOut.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            strWriteOut.Write(reportToPrint);
            strWriteOut.WriteLine(reportMessage);
            strWriteOut.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        }

        public static string buildPrintLine(int[] fieldLengths, IEnumerable<string> oneLine)
        {
            StringBuilder printLine = new StringBuilder();
            foreach (var (str, k) in oneLine.Select((x, i) => (x, i)))
            {
                printLine.Append(str.PadRight(fieldLengths[k]));
            }

            return printLine.ToString();
        }

        protected static void LoadLogDIBclasses(List<RegionalReports> listToOutput, List<LogStockDO> justDIBs)
        {
            //  uses RegionalReports
            foreach (LogStockDO jd in justDIBs)
            {
                RegionalReports r = new RegionalReports();
                r.value7 = jd.DIBClass;
                listToOutput.Add(r);
            }   //  end foreach loop
            return;
        }

        protected static void LoadTreeDIBclasses(float MaxDBH, List<StandTables> ListToLoad, int classInterval)
        {
            //  loads DIB classes for stand table reports
            int startNum;
            //  set first class
            switch (classInterval)
            {
                case 1:
                    //  round maxDBH
                    MaxDBH = (float)Math.Round(MaxDBH);
                    StandTables oneS = new StandTables();
                    oneS.dibClass = "1-3";
                    ListToLoad.Add(oneS);
                    startNum = 4;
                    for (int k = startNum; k <= MaxDBH; k++)
                    {
                        StandTables st = new StandTables();
                        st.dibClass = startNum.ToString();
                        ListToLoad.Add(st);
                        startNum += classInterval;
                    }   // end for k loop
                    break;
                case 2:
                    //  round MaxDBH
                    MaxDBH = (float)Math.Floor(MaxDBH);
                    StandTables twoS = new StandTables();
                    twoS.dibClass = "1-4";
                    ListToLoad.Add(twoS);
                    startNum = 6;
                    //  if max DBH is odd, add one to get proper size class
                    if ((int)MaxDBH % 2 != 0) MaxDBH++;
                    for (int k = startNum; k <= MaxDBH; k += 2)
                    {
                        StandTables s = new StandTables();
                        s.dibClass = startNum.ToString();
                        ListToLoad.Add(s);
                        startNum += classInterval;
                    }   //  end for k loop
                    break;
                case 3:         //  for stem count reports
                    MaxDBH = (float)Math.Round(MaxDBH);
                    for (int k = 0; k <= MaxDBH; k++)
                    {
                        StandTables cto = new StandTables();
                        cto.dibClass = k.ToString();
                        ListToLoad.Add(cto);
                    }   //  end for k loop
                    break;
            }   //  end switch

            return;
        }

        protected static int FindTreeDIBindex(List<StandTables> ListToSearch, double currDBH, int classInterval)
        {
            string DIBtoFind = "";
            switch (classInterval)
            {
                case 1:
                    if (currDBH < 3.6)
                        return 0;
                    else DIBtoFind = ((int)(currDBH + 0.49)).ToString();
                    break;
                case 2:
                    if (currDBH <= 4.9)
                        return 0;
                    else
                    {
                        if (((int)currDBH % 2) == 0)
                            DIBtoFind = ((int)currDBH).ToString();
                        else DIBtoFind = ((int)(currDBH + 1.0)).ToString();
                    }   //  endif
                    break;
                case 3:
                    //  works for stem count reports
                    DIBtoFind = ((int)currDBH).ToString();
                    break;
            }   //  end switch
            int rowToLoad = ListToSearch.FindIndex(
                delegate (StandTables s)
                {
                    return s.dibClass == DIBtoFind;
                });
            if (rowToLoad < 0)
                rowToLoad = 0;

            return rowToLoad;
        }

        protected class StandTables
        {
            public string dibClass { get; set; }
            public double species1 { get; set; }
            public double species2 { get; set; }
            public double species3 { get; set; }
            public double species4 { get; set; }
            public double species5 { get; set; }
            public double species6 { get; set; }
            public double species7 { get; set; }
            public double species8 { get; set; }
            public double species9 { get; set; }
            public double species10 { get; set; }
            public double lineTotal { get; set; }
        }

        protected class RegionalReports
        {
            public string value1 { get; set; }
            public string value2 { get; set; }
            public string value3 { get; set; }
            public string value4 { get; set; }
            public string value5 { get; set; }
            public string value6 { get; set; }
            public double value7 { get; set; }
            public double value8 { get; set; }
            public double value9 { get; set; }
            public double value10 { get; set; }
            public double value11 { get; set; }
            public double value12 { get; set; }
            public double value13 { get; set; }
            public double value14 { get; set; }
            public double value15 { get; set; }
            public double value16 { get; set; }
            public double value17 { get; set; }
            public double value18 { get; set; }
            public double value19 { get; set; }
            public double value20 { get; set; }
            public string value21 { get; set; }
        }
    }
}
