using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public class CreateTextFile
    {
        #region
            public string fileName;
            public string currentDate;
            public string currentVersion;
            public string DLLversion;
            public string cruiseName;
            public string saleName;
            public string currentRegion;
            public List<ReportsDO> selectedReports;
            reportHeaders rh = new reportHeaders();
            public ArrayList mainHeaderFields = new ArrayList();
            public int numOlines = 0;
            public string textFile;
            private ArrayList graphReports = new ArrayList();
            public CPbusinessLayer bslyr = new CPbusinessLayer();
        #endregion

        public void createTextFile()
        {
            //  Create output filename
            textFile = System.IO.Path.ChangeExtension(fileName,"out");

            //  Get current date and time for run time
            currentDate = DateTime.Now.ToString();
            
            //  Set version numbers
            //currentVersion = "DRAFT.2018";
            currentVersion = "10.01.2019";
            DLLversion = Utilities.CurrentDLLversion();
 
            
            //  open up file and start writing
            using(StreamWriter strWriteOut = new StreamWriter(textFile))
            {
                //  Output banner page
                BannerPage bp = new BannerPage();
                bp.outputBannerPage(fileName, strWriteOut, currentDate, currentVersion, DLLversion, bslyr);
                int pageNumber = 2;
                cruiseName = bp.cruiseName;
                saleName = bp.saleName;

                //  fill header fields
                mainHeaderFields.Add(currentDate);
                mainHeaderFields.Add(currentVersion);
                mainHeaderFields.Add(DLLversion);
                mainHeaderFields.Add(cruiseName);
                mainHeaderFields.Add(saleName);

                //  Output equation tables as needed
                OutputEquationTables oet = new OutputEquationTables();
                oet.bslyr.fileName = fileName;
                oet.bslyr.DAL = bslyr.DAL;
                oet.mainHeaderFields = mainHeaderFields;
                oet.outputEquationTable(strWriteOut, rh, ref pageNumber);

                //  Output selected reports
                //  create objects for each report category and assign header fields as needed
                OutputUnits ou = new OutputUnits();
                OutputLiveDead old = new OutputLiveDead();
                OutputLogStock ols = new OutputLogStock();
                foreach (ReportsDO rdo in selectedReports)
                {
                    
                    switch (rdo.ReportID)
                    {
                        case "A01":     case "A02":     case "A03":     case "A04":
                        case "A05":     case "A06":     case "A07":     case "A08":
                        case "A09":     case "A10":     case "A13":     case "L1": 
                        case "A15":
                            OutputList ol = new OutputList();
                            ol.bslyr.fileName = fileName;
                            ol.bslyr.DAL = bslyr.DAL;
                            ol.mainHeaderFields = mainHeaderFields;
                            ol.currentReport = rdo.ReportID.ToString();
                            if (currentRegion == "10" && rdo.ReportID == "L1")
                                MessageBox.Show("L1 report for Region 10 does not appear in text output file.\nInstead, create CSV5 to create file for cut and/or leave trees.","INFORMATION",MessageBoxButtons.OK,MessageBoxIcon.Information);
                            else ol.OutputListReports(strWriteOut, rh, ref pageNumber);
                            break;
                        case "A11":     case "A12":
                            OutputTreeGrade otg = new OutputTreeGrade();
                            otg.currRept = rdo.ReportID;
                            otg.mainHeaderFields = mainHeaderFields;
                            otg.fileName = fileName;
                            otg.bslyr.fileName = fileName;
                            otg.bslyr.DAL = bslyr.DAL;
                            otg.CreateTreeGradeReports(strWriteOut, rh, ref pageNumber);
                            break;
                        case "A14":
                            OutputUnitSummary ous = new OutputUnitSummary();
                            ous.currentReport = rdo.ReportID;
                            ous.mainHeaderFields = mainHeaderFields;
                            ous.fileName = fileName;
                            ous.bslyr.fileName = fileName;
                            ous.bslyr.DAL = bslyr.DAL;
                            ous.createUnitSummary(strWriteOut, ref pageNumber, rh);
                            break;
                        case "ST1":     case "ST2":
                            OutputStats ost = new OutputStats();
                            ost.currCL = "C";
                            ost.fileName = fileName;
                            ost.bslyr.fileName = fileName;
                            ost.bslyr.DAL = bslyr.DAL;
                            ost.mainHeaderFields = mainHeaderFields;
                            ost.currentReport = rdo.ReportID;
                            ost.CreateStatReports(strWriteOut, rh, ref pageNumber);
                            break;
                        case "ST3":     case "ST4":
                            OutputStatsToo otoo = new OutputStatsToo();
                            otoo.fileName = fileName;
                            otoo.bslyr.fileName = fileName;
                            otoo.bslyr.DAL = bslyr.DAL;
                            otoo.mainHeaderFields = mainHeaderFields;
                            otoo.currentReport = rdo.ReportID;
                            otoo.OutputStatReports(strWriteOut, rh, ref pageNumber);
                            break;
                        case "VSM1":    case "VSM2":    case "VSM3":
                        case "VPA1":    case "VPA2":    case "VPA3":
                        case "VAL1":    case "VAL2":    case "VAL3":
                        case "VSM6":
                            OutputSummary os = new OutputSummary();
                            os.currCL = "C";
                            os.currentReport = rdo.ReportID;
                            os.fileName = fileName;
                            os.bslyr.fileName = fileName;
                            os.bslyr.DAL = bslyr.DAL;
                            os.mainHeaderFields = mainHeaderFields;
                            os.OutputSummaryReports(strWriteOut, rh, ref pageNumber);
                            break;
                        case "VSM4":        case "VSM5":   
                            ou.fileName = fileName;
                            ou.bslyr.fileName = fileName;
                            ou.bslyr.DAL = bslyr.DAL;
                            ou.mainHeaderFields = mainHeaderFields;
                            ou.currentReport = rdo.ReportID;
                            ou.OutputUnitReports(strWriteOut, rh, ref pageNumber);
                            break;
                        case "UC1":     case "UC2":     case "UC3":
                        case "UC4":     case "UC5":     case "UC6":
                            ou.fileName = fileName;
                            ou.bslyr.fileName = fileName;
                            ou.bslyr.DAL = bslyr.DAL;
                            ou.currCL = "C";
                            ou.mainHeaderFields = mainHeaderFields;
                            ou.currentReport = rdo.ReportID;
                            ou.OutputUnitReports(strWriteOut, rh, ref pageNumber);
                            break;
                        case "TIM":
                            OutputTIM ot = new OutputTIM();
                            ot.fileName = fileName;
                            ot.bslyr.fileName = fileName;
                            ot.bslyr.DAL = bslyr.DAL;
                            ot.cruiseNum = cruiseName;
                            ot.currentVersion = currentVersion;
                            ot.CreateSUMfile();
                            break;
                        case "TC1":     case "TC2":     case "TC3":     case "TC4":
                        case "TC6":     case "TC8":     case "TC10":    case "TC12":
                        case "TC19":    case "TC20":    case "TC21":    case "TC22":
                        case "TC24":    case "TL1":     case "TL6":     case "TL7":
                        case "TL8":     case "TL9":     case "TL10":    case "TL12":
                            //  2-inch diameter class
                            OutputStandTables oStand2 = new OutputStandTables();
                            oStand2.fileName = fileName;
                            oStand2.bslyr.fileName = fileName;
                            oStand2.bslyr.DAL = bslyr.DAL;
                            oStand2.mainHeaderFields = mainHeaderFields;
                            oStand2.currentReport = rdo.ReportID;
                            oStand2.CreateStandTables(strWriteOut, rh, ref pageNumber, 2);
                            break;
                        case "TC51":    case "TC52":    case "TC53":    case "TC54":
                        case "TC56":    case "TC57":    case "TC58":    case "TC59":
                        case "TC60":    case "TC62":    case "TC65":    case "TC71":
                        case "TC72":    case "TC74":    case "TL52":    case "TL54":
                        case "TL56":    case "TL58":    case "TL59":    case "TL60":
                        case "TL62":
                            //  1-inch diameter class
                            OutputStandTables oStand1 = new OutputStandTables();
                            oStand1.fileName = fileName;
                            oStand1.bslyr.fileName = fileName;
                            oStand1.bslyr.DAL = bslyr.DAL;
                            oStand1.mainHeaderFields = mainHeaderFields;
                            oStand1.currentReport = rdo.ReportID;
                            oStand1.CreateStandTables(strWriteOut, rh, ref pageNumber, 1);
                            break;
                        case "UC7":     case "UC8":     case "UC9":     case "UC10":
                        case "UC11":    case "UC12":    case "UC13":    case "UC14":
                        case "UC15":    case "UC16":    case "UC17":    case "UC18":
                        case "UC19":    case "UC20":    case "UC21":    case "UC22":
                        case "UC23":    case "UC24":    case "UC25":    case "UC26":
                            //  UC stand tables
                            OutputUnitStandTables oUnits = new OutputUnitStandTables();
                            oUnits.fileName = fileName;
                            oUnits.bslyr.fileName = fileName;
                            oUnits.bslyr.DAL = bslyr.DAL;
                            oUnits.mainHeaderFields = mainHeaderFields;
                            oUnits.currentReport = rdo.ReportID;
                            oUnits.CreateUnitStandTables(strWriteOut, rh, ref pageNumber);
                            break;
                        case "WT1":     case "WT2":     case "WT3":     case "WT4":
                        case "WT5":
                            OutputWeight ow = new OutputWeight();
                            ow.fileName = fileName;
                            ow.bslyr.fileName = fileName;
                            ow.bslyr.DAL = bslyr.DAL;
                            ow.mainHeaderFields = mainHeaderFields;
                            ow.currentReport = rdo.ReportID;
                            ow.OutputWeightReports(strWriteOut, rh, ref pageNumber);
                            break;
                        case "LD1":     case "LD2":
                        case "LD3":     case "LD4":
                        case "LD5":     case "LD6":
                        case "LD7":     case "LD8":
                            old.currentReport = rdo.ReportID;
                            old.fileName = fileName;
                            old.bslyr.fileName = fileName;
                            old.bslyr.DAL = bslyr.DAL;
                            old.mainHeaderFields = mainHeaderFields;
                            old.CreateLiveDead(strWriteOut, rh, ref pageNumber);
                            break;
                        case "L2":     case "L8":   case "L10":
                            ols.currentReport = rdo.ReportID;
                            ols.fileName = fileName;
                            ols.bslyr.fileName = fileName;
                            ols.bslyr.DAL = bslyr.DAL;
                            ols.mainHeaderFields = mainHeaderFields;
                            ols.CreateLogReports(strWriteOut, rh, ref pageNumber);
                            break;
                        case "BLM01":   case "BLM02":   case "BLM03":   case "BLM04":
                        case "BLM05":   case "BLM06":   case "BLM07":   case "BLM08":
                        case "BLM09":   case "BLM10":
                            OutputBLM ob = new OutputBLM();
                            ob.currentReport = rdo.ReportID;
                            ob.fileName = fileName;
                            ob.bslyr.fileName = fileName;
                            ob.bslyr.DAL = bslyr.DAL;
                            ob.mainHeaderFields = mainHeaderFields;
                            ob.CreateBLMreports(strWriteOut, rh, ref pageNumber);
                            break;
                        case "R101":        case "R102":        
                        case "R103":        case "R104":
                        case "R105":
                            OutputR1 r1 = new OutputR1();
                            r1.currentReport = rdo.ReportID;
                            r1.fileName = fileName;
                            r1.bslyr.fileName = fileName;
                            r1.bslyr.DAL = bslyr.DAL;
                            r1.mainHeaderFields = mainHeaderFields;
                            r1.CreateR1reports(strWriteOut, ref pageNumber, rh);
                            break;
                        case "R201":        case "R202":        case "R203":
                        case "R204":        case "R205":        case "R206": 
                        case "R207":        case "R208":    
                            OutputR2 r2 = new OutputR2();
                            r2.currentReport = rdo.ReportID;
                            r2.fileName = fileName;
                            r2.bslyr.fileName = fileName;
                            r2.bslyr.DAL = bslyr.DAL;
                            r2.mainHeaderFields = mainHeaderFields;
                            r2.CreateR2Reports(strWriteOut, ref pageNumber, rh);
                            break;  
                        case "R301":
                            OutputR3 r3 = new OutputR3();
                            r3.currentReport = rdo.ReportID;
                            r3.fileName = fileName;
                            r3.bslyr.fileName = fileName;
                            r3.bslyr.DAL = bslyr.DAL;
                            r3.mainHeaderFields = mainHeaderFields;
                            r3.CreateR3Reports(strWriteOut, ref pageNumber, rh);
                            break;
                        case "R401":    case "R402":        case "R403":        case "R404":
                            OutputR4 r4 = new OutputR4();
                            r4.currentReport = rdo.ReportID;
                            r4.fileName = fileName;
                            r4.bslyr.fileName = fileName;
                            r4.bslyr.DAL = bslyr.DAL;
                            r4.mainHeaderFields = mainHeaderFields;
                            r4.CreateR4Reports(strWriteOut, ref pageNumber, rh);
                            break;
                        case "R501":
                            OutputR5 r5 = new OutputR5();
                            r5.currentReport = rdo.ReportID;
                            r5.fileName = fileName;
                            r5.bslyr.fileName = fileName;
                            r5.bslyr.DAL = bslyr.DAL;
                            r5.mainHeaderFields = mainHeaderFields;
                            r5.CreateR5report(strWriteOut, ref pageNumber, rh);
                            break;
                        case "R604":    case "R605":    case "R602":
                            OutputR6 r6 = new OutputR6();
                            r6.currentReport = rdo.ReportID;
                            r6.fileName = fileName;
                            r6.bslyr.fileName = fileName;
                            r6.bslyr.DAL = bslyr.DAL;
                            r6.mainHeaderFields = mainHeaderFields;
                            r6.CreateR6reports(strWriteOut, ref pageNumber, rh);
                            break;
                        case "R801":        case "R802":
                            OutputR8 r8 = new OutputR8();
                            r8.currentReport = rdo.ReportID;
                            r8.fileName = fileName;
                            r8.bslyr.fileName = fileName;
                            r8.bslyr.DAL = bslyr.DAL;
                            r8.mainHeaderFields = mainHeaderFields;
                            r8.CreateR8Reports(strWriteOut, ref pageNumber, rh);
                            break;
                        case "R902":
                            OutputR9 r9 = new OutputR9();
                            r9.currentReport = rdo.ReportID;
                            r9.fileName = fileName;
                            r9.bslyr.fileName = fileName;
                            r9.bslyr.DAL = bslyr.DAL;
                            r9.mainHeaderFields = mainHeaderFields;
                            //r9.CreateR9Reports(strWriteOut, ref pageNumber, rh);
                            r9.OutputTipwoodReport(strWriteOut, rh, ref pageNumber);
                            break;
                        case "R001":    case "R002":    case "R003":    case "R004":
                        case "R005":    case "R006":    case "R007":    case "R008":
                        case "R009":
                            OutputR10 r10 = new OutputR10();
                            r10.currentReport = rdo.ReportID;
                            r10.fileName = fileName;
                            r10.bslyr.fileName = fileName;
                            r10.bslyr.DAL = bslyr.DAL;
                            r10.mainHeaderFields = mainHeaderFields;
                            r10.CreateR10reports(strWriteOut, ref pageNumber, rh);
                            break;
                        case "SC1":         case "SC2":         case "SC3":
                            OutputStemCounts osc = new OutputStemCounts();
                            osc.currentReport = rdo.ReportID;
                            osc.fileName = fileName;
                            osc.bslyr.fileName = fileName;
                            osc.bslyr.DAL = bslyr.DAL;
                            osc.mainHeaderFields = mainHeaderFields;
                            osc.createStemCountReports(strWriteOut, ref pageNumber, rh);
                            break;
                        case "LV01":         case "LV02":
                        case "LV03":         case "LV04":
                        case "LV05":
                            OutputLeave olt = new OutputLeave();
                            olt.currentReport = rdo.ReportID;
                            olt.bslyr.fileName = bslyr.fileName;
                            olt.bslyr.DAL = bslyr.DAL;
                            olt.mainHeaderFields = mainHeaderFields;
                            olt.createLeaveTreeReports(strWriteOut, ref pageNumber, rh);
                            break;
                        case "GR01":        case "GR02":
                        case "GR03":        case "GR04":
                        case "GR05":        case "GR06":
                        case "GR07":        case "GR08":
                        case "GR09":        case "GR10":
                        case "GR11":
                            graphReports.Add(rdo.ReportID);
                            string graphFile = System.IO.Path.GetDirectoryName(fileName);
                            graphFile += "\\Graphs\\";
                            graphFile += rdo.ReportID;
                            strWriteOut.WriteLine("\f");
                            strWriteOut.Write("Graph Report ");
                            strWriteOut.Write(rdo.ReportID);
                            strWriteOut.WriteLine(" can be found in the following file: ");
                            strWriteOut.WriteLine(graphFile);
                            break;
                    }   //  end switch
                }   //  end foreach loop

                //  Any warning messages to print?
                List<ErrorLogDO> errList = bslyr.getErrorMessages("W", "CruiseProcessing");
                List<ErrorLogDO> errListToo = bslyr.getErrorMessages("W", "FScruiser");
                int warnFlag = 0;
                if (errList.Count > 0)
                {
                    WriteWarnings(strWriteOut, errList, ref pageNumber);
                    warnFlag = 1;
                }
                if(errListToo.Count > 0)
                {
                    WriteOtherWarnings(strWriteOut, errListToo, ref pageNumber);
                    warnFlag = 1;
                }
                if(warnFlag > 0)
                    MessageBox.Show("Warning messages detected.\nCheck end of output file for the list.", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);

                strWriteOut.Close();

                //  create graphs if reports requested
                if (graphReports.Count > 0)
                {
                    graphOutputDialog dog = new graphOutputDialog();
                    dog.fileName = fileName;
                    dog.bslyr.fileName = fileName;
                    dog.bslyr.DAL = bslyr.DAL;
                    dog.graphReports = graphReports;
                    dog.ShowDialog();
                }   //  endif graphs selected

            }   //  end using

            return;
        }   //  end createTextFile


        public StringBuilder buildPrintLine(int[] fieldLengths, ArrayList oneLine)
        {
            StringBuilder printLine = new StringBuilder();
            string fieldToAdd;
            int k = 0;
            foreach(object obj in oneLine)
            {
                fieldToAdd = Convert.ToString(obj);
                printLine.Append(fieldToAdd.PadRight(fieldLengths[k]));
                k++;
            }   //  end foreach loop

            return printLine;
        }   //  end buildPrintLine


        public void noDataForReport(StreamWriter strWriteOut, string reportToPrint, string reportMessage)
        {
            //  output message that the report has no data so report could not be generated
            strWriteOut.WriteLine("\f");
            strWriteOut.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            StringBuilder sb = new StringBuilder();
            sb.Append(reportToPrint);
            sb.Append(reportMessage);
            strWriteOut.WriteLine(sb.ToString());
            strWriteOut.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        }   //  end noDataForReport


        public void whichHeightFields(ref int hgtOne, ref int hgtTwo, List<TreeDO> tList)
        {
            double heightSum = 0.0;
            //  Total height?
            foreach (TreeDO tl in tList)
                heightSum += Convert.ToDouble(tl.TotalHeight);

            if (heightSum > 0.0)
                hgtOne = 1;

            //  Merch Height Primary Product?
            heightSum = 0.0;
            foreach (TreeDO tl in tList)
                heightSum += Convert.ToDouble(tl.MerchHeightPrimary);

            if (heightSum > 0.0 && hgtOne == 0)
                hgtOne = 2;
            else if (heightSum > 0.0 && hgtOne != 0)
            {
                hgtTwo = 2;
                return;
            }   //  endif

            //  Merch Height Secondary Product
            heightSum = 0.0;
            foreach (TreeDO tl in tList)
                heightSum += Convert.ToDouble(tl.MerchHeightSecondary);
            if (heightSum > 0.0 && hgtOne == 0)
                hgtOne = 3;
            else if (heightSum > 0.0 && hgtOne != 0)
            {
                hgtTwo = 3;
                return;
            }   //  endif

            //  Height to upper stem
            heightSum = 0.0;
            foreach (TreeDO tl in tList)
                heightSum += Convert.ToDouble(tl.UpperStemHeight);
            if (heightSum > 0.0 && hgtOne == 0)
                hgtOne = 4;
            else if (heightSum > 0.0 && hgtOne != 0)
            {
                hgtTwo = 4;
                return;
            }
        }   //  end whichHeightFields


        public string[] updateHeightHeader(int hgtOne, int hgtTwo, string titlePrefix, string[] headerToUpdate)
        {
            string[] updatedHeader = new string[headerToUpdate.Count()];
            string header1 = "   TOT HGT";
            string header2 = " MRHT PRPD";
            string header3 = " MRHT SEPD";
            string header4 = " HT U STEM";
            StringBuilder sb = new StringBuilder();
            sb.Append(titlePrefix);
            switch (hgtOne)
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
            if (hgtTwo != 0)
            {
                sb.Clear();
                sb.Append(titlePrefix);
                switch (hgtTwo)
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
            else if(hgtTwo == 0)
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
        }   //  end updateHeightHeader


        public string fillReportTitle(string currReport)
        {
            allReportsArray ara = new allReportsArray();
            string currTitle = ara.findReportTitle(currReport);
            //  Add report number to title
            currTitle = currTitle.Insert(0, ": ");
            currTitle = currTitle.Insert(0,currReport);
            return currTitle;            
        }   //  end fillReportTitle


        public void WriteReportHeading(StreamWriter strWriteOut, string TitleOne, string TitleTwo, 
                                        string TitleThree, string[] headerToPrint, int lineIncrement,
                                        ref int pageNumber, string extraHeader)
        {
            if (numOlines == 0 || numOlines >= 50)
            {
                //  Page break
                strWriteOut.WriteLine("\f");
                numOlines = 0;

                //  Write report main heading
                rh.pageNum = pageNumber++;
                rh.outputReportHeader(strWriteOut, mainHeaderFields, TitleOne, TitleTwo, TitleThree);
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
            return;
        }   //  end WriteReportHeading


        public void captureSTMtrees(long currSTcn, long currCUcn, string currSG, string currSTM, ref double GBDFTsum, 
                                    ref double NBDFTsum, ref double GCUFTsum, ref double NCUFTsum, ref double GBDFTnonsaw, 
                                    ref double NBDFTnonsaw, ref double GCUFTnonsaw, ref double NCUFTnonsaw, 
                                    ref double CordSum, double currProFac)
        {
            //  retrieve calc values for current stratum
            List<TreeCalculatedValuesDO> tList = bslyr.getTreeCalculatedValues((int)currSTcn, (int)currCUcn);
            //  Find all STM trees in the current cutting unit
            List<TreeCalculatedValuesDO> justSTM = tList.FindAll(
                delegate(TreeCalculatedValuesDO tcv)
                {
                    return tcv.Tree.SampleGroup.Code == currSG && tcv.Tree.STM == currSTM;
                });
            //  expand and sum up
            foreach (TreeCalculatedValuesDO js in justSTM)
            {
                switch (js.Tree.SampleGroup.PrimaryProduct)
                {
                    case "01":
                        GBDFTsum += js.GrossBDFTPP * js.Tree.ExpansionFactor * currProFac;
                        NBDFTsum += js.NetBDFTPP * js.Tree.ExpansionFactor * currProFac;
                        GCUFTsum += js.GrossCUFTPP * js.Tree.ExpansionFactor * currProFac;
                        NCUFTsum += js.NetCUFTPP * js.Tree.ExpansionFactor * currProFac;
                        break;
                    default:
                        GBDFTnonsaw += js.GrossBDFTPP * js.Tree.ExpansionFactor * currProFac;
                        NBDFTnonsaw += js.NetBDFTPP * js.Tree.ExpansionFactor * currProFac;
                        GCUFTnonsaw += js.GrossCUFTPP * js.Tree.ExpansionFactor * currProFac;
                        NCUFTnonsaw += js.NetCUFTPP * js.Tree.ExpansionFactor * currProFac;
                        CordSum += js.CordsPP * js.Tree.ExpansionFactor * currProFac;
                        break;
                }   //  end switch on primary product

                //  topwood totals
                GBDFTnonsaw += js.GrossBDFTSP * js.Tree.ExpansionFactor * currProFac;
                NBDFTnonsaw += js.NetBDFTSP * js.Tree.ExpansionFactor * currProFac;
                GCUFTnonsaw += js.GrossCUFTSP * js.Tree.ExpansionFactor * currProFac;
                NCUFTnonsaw += js.NetCUFTSP * js.Tree.ExpansionFactor * currProFac;
                CordSum += js.CordsSP * js.Tree.ExpansionFactor * currProFac;

                //  Recovered totals
                if (js.GrossBDFTRP > 0 || js.GrossCUFTRP > 0)
                {
                    NBDFTnonsaw += js.GrossBDFTRP * js.Tree.ExpansionFactor * currProFac;
                    NCUFTnonsaw += js.GrossCUFTRP * js.Tree.ExpansionFactor * currProFac;
                    CordSum += js.CordsRP * js.Tree.ExpansionFactor * currProFac;
                }   //  endif
            }   //  end foreach loop

            return;
        }   //  end captureSTMtrees
        

        public void printOneRecord(StreamWriter strWriteOut, ArrayList prtFields)
        {
            StringBuilder oneRecord = new StringBuilder();
            for (int k = 0; k < prtFields.Count; k++)
                oneRecord.Append(prtFields[k]);
            strWriteOut.WriteLine(oneRecord.ToString());
            numOlines++;
        }   //  end printOneRecord


        public void printOneRecord(int[] fieldLengths, ArrayList prtFields, StreamWriter strWriteOut)
        {
            StringBuilder oneRecord = buildPrintLine(fieldLengths, prtFields);
            strWriteOut.WriteLine(oneRecord.ToString());
            numOlines++;
        }   //  end printOneRecord


        private void WriteWarnings(StreamWriter strWriteOut, List<ErrorLogDO> errList, ref int pageNumb)
        {
            //  warning messages list
            string[] WarnMessages = new string[22]  {"   ",
                                                    "NO VOLUME EQUATION MATCH",
                                                    "NO FORM CLASS",
                                                    "DBH LESS THAN ONE",
                                                    "TREE HEIGHT LESS THAN 4.5",
                                                    "D2H IS OUT OF BOUNDS",
                                                    "NO SPECIES MATCH",
                                                    "ILLEGAL PP LOG HEIGHT",
                                                    "ILLEGAL SP LOG HEIGHT",
                                                    "NO UPPER STEM MEASUREMENTS",
                                                    "ILLEGAL UPPER STEM HEIGHT",
                                                    "UNABLE TO FIT PROFILE GIVEN DBH, MERCH HT & TOP DIA",
                                                    "TREE HAS MORE THAN 20 LOGS",
                                                    "TOP DIAMETER GREATER THAN DBH INSIDE BARK",
                                                    "BARK EQUATION DOES NOT EXIST OR YIELDS NEGATIVE DBHIB",
                                                    "INVALID BIOMASS EQUATION",
                                                    "PRIMARY PRODUCT HEIGHT REQUIRED FOR BIOMASS CALCULATION",
                                                    "SECONDARY PRODUCT HEIGHT REQUIRED FRO BIOMASS CALCULATION",
                                                    "RECOVERABLE DEFECT GREATER THAN SUM OF DEFECTS -- SUM OF DEFECTS USED IN CALCULATION",
                                                    "SECONDARY PRODUCT WAS BLANK IN SAMPLE GROUPS -- DEFAULT VALUE USED",
                                                    "MORE THAN TWO UOMs DETECTED--THIS FILE WILL NOT LOAD IN TIM",
                                                    "BIOMASS FLAG NOT CHECKED -- NO WEIGHT CALCULATED"};
            numOlines = 0;
            string[] warningHeader = new string[] {"   IDENTIFIER                        WARNING MESSAGE"};
            StringBuilder sb = new StringBuilder();
            foreach (ErrorLogDO eld in errList)
            {
                WriteReportHeading(strWriteOut, "WARNING MESSAGES", "", "", warningHeader, 8, ref pageNumb, "");
                //  build identifier
                if(eld.TableName == "SUM file")
                {
                    sb.Append(eld.TableName);
                    sb.Append("  ---------    ");
                    sb.Append(WarnMessages[Convert.ToInt16(eld.Message)]);
                }
                else if(eld.TableName == "Stratum" || eld.TableName == "VolumeEquation")
                {
                    sb.Append(Utilities.GetIdentifier(eld.TableName, eld.CN_Number, bslyr));
                    sb.Append("   ");
                    sb.Append(eld.Message);
                }
                else
                {
                    sb.Append(Utilities.GetIdentifier(eld.TableName, eld.CN_Number, bslyr));
                    sb.Append("   ");
                    if (eld.Message == "30")
                    {
                        //  this is just a warning and will never use the record in the error list
                        sb.Append("Sample group must have at least one measured tree");
                    }
                    else if (eld.Message.Length > 4)
                        sb.Append(eld.Message);
                    else sb.Append(WarnMessages[Convert.ToInt16(eld.Message)]);
                }   //  endif
                strWriteOut.WriteLine(sb.ToString());
                numOlines++;
                sb.Clear();

            }   //  end foreach loop


            //  output the key for the identifier
            strWriteOut.WriteLine();
            strWriteOut.WriteLine("Not all fields are used as identifier information.");
            strWriteOut.WriteLine("In general, the following order is used.");
            strWriteOut.WriteLine("Stratum number");
            strWriteOut.WriteLine("Cutting unit number");
            strWriteOut.WriteLine("Plot number");
            strWriteOut.WriteLine("Tree number");
            strWriteOut.WriteLine("Log number");
            strWriteOut.WriteLine("Species code");
            strWriteOut.WriteLine("Sample group");
            strWriteOut.WriteLine("Primary product");
            strWriteOut.WriteLine("Equation number");
            numOlines += 12;
            return;
        }   //  end WriteWarnings


        private void WriteOtherWarnings(StreamWriter strWriteOut, List<ErrorLogDO> errListToo, ref int pageNumb)
        {
            //  adds warnings from FScruiser and/or CruiseManager to the bottom of the output text file
            numOlines = 0;
            string[] warningHeader = new string[] { "   IDENTIFIER                        WARNING MESSAGE" };
            StringBuilder sb = new StringBuilder();
            foreach (ErrorLogDO elt in errListToo)
            {
                WriteReportHeading(strWriteOut, "WARNING MESSAGES", "", "", warningHeader, 8, ref pageNumb, "");
                //  build identifier
                sb.Append(Utilities.GetIdentifier(elt.TableName, elt.CN_Number, bslyr));
                sb.Append("   ");
                sb.Append(elt.Message);
                strWriteOut.WriteLine(sb.ToString());
                numOlines++;
                sb.Clear();
            }   //  end foreach loop

            //  output the key for the identifier
            strWriteOut.WriteLine();
            strWriteOut.WriteLine("Not all fields are used as identifier information.");
            strWriteOut.WriteLine("In general, the ised order is used.");
            strWriteOut.WriteLine("Stratum number");
            strWriteOut.WriteLine("Cutting unit number");
            strWriteOut.WriteLine("Plot number");
            strWriteOut.WriteLine("Tree number");
            strWriteOut.WriteLine("Log number");
            strWriteOut.WriteLine("Species code");
            strWriteOut.WriteLine("Sample group");
            strWriteOut.WriteLine("Primary product");
            strWriteOut.WriteLine("Equation number");
            return;
        }   //  end WriteOtherWarnings


        public void LoadLogDIBclasses(List<LogStockDO> justDIBs, List<ReportSubtotal> ListToOutput)
        {
            //  uses ReportSubtotal (BLM reports)
            foreach (LogStockDO jd in justDIBs)
            {
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = jd.DIBClass.ToString();
                ListToOutput.Add(r);
            }   //  end foreach loop
            return;
        }   //  end LoadLogDIBclasses


        public void LoadLogDIBclasses(List<RegionalReports> listToOutput, List<LogStockDO> justDIBs)
        {
            //  uses RegionalReports
            foreach (LogStockDO jd in justDIBs)
            {
                RegionalReports r = new RegionalReports();
                r.value7 = jd.DIBClass;
                listToOutput.Add(r);
            }   //  end foreach loop
            return;
        }   //  end LoadLogDIBclasses

        
        public void LoadTreeDIBclasses(float MaxDBH, List<StandTables> ListToLoad, int classInterval)
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
                    for (int k = startNum; k <= MaxDBH; k+=2)
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
        }   //  end LoadTreeDIBclasses


        public int FindDIBindex(List<ReportSubtotal> ListToOutput, float SmEndDiam)
        {   
            string DIBtoFind = (Math.Floor(SmEndDiam + 0.5)).ToString();
            int rowToLoad = ListToOutput.FindIndex(
                delegate(ReportSubtotal r)
                {
                    return r.Value1 == DIBtoFind;
                });
            if(rowToLoad < 0)
                rowToLoad = 0;
            return rowToLoad;
        }   //  end FindDIBindex


        public int FindDIBindex(float SmEndDiam, List<RegionalReports> listToOutput)
        {
            string DIBtoFind = (Math.Floor(SmEndDiam + 0.5)).ToString();
            int rowToLoad = listToOutput.FindIndex(
                delegate(RegionalReports rr)
                {
                    return rr.value1 == DIBtoFind;
                });
            if (rowToLoad < 0)
                rowToLoad = 0;
            return rowToLoad;
        }   //  end FindDIBindex

        
        public int FindTreeDIBindex(List<StandTables> ListToSearch, double currDBH, int classInterval)
        {
            string DIBtoFind = "";
            switch(classInterval)
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
                delegate(StandTables s)
                {
                    return s.dibClass == DIBtoFind;
                });
            if(rowToLoad < 0)
                rowToLoad = 0;

            return rowToLoad;
        }   //  end FindTreeDIBindex


        public double CalculateQuadMean(double deNominator, double numErator)
        {
            if (numErator > 0)
                return Math.Sqrt(deNominator / numErator);
            else return 0;            
        }   //  end CalculateQuadMean


        //  list class for any subtotal line(s)
        public class ReportSubtotal
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }
            public double Value3 { get; set; }
            public double Value4 { get; set; }
            public double Value5 { get; set; }
            public double Value6 { get; set; }
            public double Value7 { get; set; }
            public double Value8 { get; set; }
            public double Value9 { get; set; }
            public double Value10 { get; set; }
            public double Value11 { get; set; }
            public double Value12 { get; set; }
            public double Value13 { get; set; }
            public double Value14 { get; set; }
            public double Value15 { get; set; }
            public double Value16 { get; set; }
        }   //  end ReportSubtotal


        //  list for stand table reports
        public class StandTables
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
        }   //  end StandTables


        public class RegionalReports
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
        }   //  end RegionalReports


        //  list class for CSV files
        public class CSVlist
        {
            public string field1 { get; set; }
            public string field2 { get; set; }
            public string field3 { get; set; }
            public string field4 { get; set; }
            public string field5 { get; set; }
            public string field6 { get; set; }
            public string field7 { get; set; }
            public string field8 { get; set; }
            public string field9 { get; set; }
            public string field10 { get; set; }
            public string field11 { get; set; }
            public string field12 { get; set; }
            public string field13 { get; set; }
            public string field14 { get; set; }
            public string field15 { get; set; }
            public string field16 { get; set; }
            public string field17 { get; set; }
            public string field18 { get; set; }
            public string field19 { get; set; }
            public string field20 { get; set; }
            public string field21 { get; set; }
            public string field22 { get; set; }
            public string field23 { get; set; }
            public string field24 { get; set; }
            public string field25 { get; set; }
            public string field26 { get; set; }
            public string field27 { get; set; }
            public string field28 { get; set; }
            public string field29 { get; set; }
            public string field30 { get; set; }
            public string field31 { get; set; }
            public string field32 { get; set; }
            public string field33 { get; set; }
            public string field34 { get; set; }
            public string field35 { get; set; }
            public string field36 { get; set; }
            public string field37 { get; set; }
            public string field38 { get; set; }
            public string field39 { get; set; }
            public string field40 { get; set; }
            public string field41 { get; set; }
            public string field42 { get; set; }
            public string field43 { get; set; }
            public string field44 { get; set; }
            public string field45 { get; set; }
            public string field46 { get; set; }
        }   //  end CSVlist

        //  list class for biomass reports (WT2/WT3)
        public class BiomassData
        {
            public string userStratum { get; set; }
            public string userSG { get; set; }
            public string userSpecies { get; set; }
            public int bioSpecies { get; set; }
            public double needles { get; set; }
            public double quarterInch { get; set; }
            public double oneInch { get; set; }
            public double threeInch { get; set; }
            public double threePlus { get; set; }
            public double topwoodDryWeight { get; set; }
            public double cullLogWgt { get; set; }
            public double cullChunkWgt { get; set; }
            public double FLIW { get; set; }
            public double DSTneedles { get; set; }
            public double DSTquarterInch { get; set; }
            public double DSToneInch { get; set; }
            public double DSTthreeInch { get; set; }
            public double DSTthreePlus { get; set; }
        }   //  end BiomassData

    }
}

