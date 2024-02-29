using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using System.Reflection;
using CruiseProcessing.Services;
using CruiseProcessing.Output;

namespace CruiseProcessing
{
    public class CreateTextFile
    {
        public string currentRegion;
        public List<string> mainHeaderFields = new List<string>();
        private int numOlines = 0;

        protected CPbusinessLayer DataLayer { get; }
        protected string FilePath => DataLayer.FilePath;
        protected string currentDate { get; }
        protected string currentVersion { get; }
        protected string DLLversion { get; }
        public string textFile { get; }

        public CreateTextFile(CPbusinessLayer dataLayer)
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            DLLversion = Utilities.CurrentDLLversion();
            var verson = Assembly.GetExecutingAssembly().GetName().Version.ToString(3); // only get the major.minor.build components of the version

            textFile = System.IO.Path.ChangeExtension(FilePath, "out");

            //  Get current date and time for run time
            currentDate = DateTime.Now.ToString();

            //  Set version numbers
            //currentVersion = "DRAFT.2018";
            currentVersion = DateTime.Parse(verson).ToString("MM.dd.yyyy");//"12.02.2021";
        }

        public void createTextFile(IDialogService dialogService, IEnumerable<ReportsDO> selectedReports)
        {
            var graphReports = new List<string>();
            //var mainHeaderFields = new List<string>();

            //  open up file and start writing
            using (StreamWriter strWriteOut = new StreamWriter(textFile))
            {
                //  Output banner page
                BannerPage bp = new BannerPage();
                bp.outputBannerPage(FilePath, strWriteOut, currentDate, currentVersion, DLLversion, DataLayer);
                int pageNumber = 2;
                var cruiseName = bp.cruiseName;
                var saleName = bp.saleName;

                //  fill header fields
                mainHeaderFields.Add(currentDate);
                mainHeaderFields.Add(currentVersion);
                mainHeaderFields.Add(DLLversion);
                mainHeaderFields.Add(cruiseName);
                mainHeaderFields.Add(saleName);

                var headerData = new HeaderFieldData()
                {
                    Date = currentDate,
                    Version = currentVersion,
                    DllVersion = DLLversion,
                    CruiseName = cruiseName,
                    SaleName = saleName,
                };

                //  Output equation tables as needed
                OutputEquationTables oet = new OutputEquationTables(DataLayer, headerData);
                oet.outputEquationTable(strWriteOut, ref pageNumber);

                //  Output selected reports
                //  create objects for each report category and assign header fields as needed
                
                
                
                foreach (ReportsDO rdo in selectedReports)
                {
                    try
                    {
                        switch (rdo.ReportID)
                        {
                            case "A01":
                            case "A02":
                            case "A03":
                            case "A04":
                            case "A05":
                            case "A06":
                            case "A07":
                            case "A08":
                            case "A09":
                            case "A10":
                            case "A13":
                            case "L1":
                            case "A15":
                                OutputList ol = new OutputList(DataLayer, headerData, rdo.ReportID);
                                if (currentRegion == "10" && rdo.ReportID == "L1")
                                    dialogService.ShowInformation("L1 report for Region 10 does not appear in text output file.\r\nInstead, create CSV5 to create file for cut and/or leave trees.");
                                else ol.OutputListReports(strWriteOut, ref pageNumber);
                                break;
                            case "A11":
                            case "A12":
                                OutputTreeGrade otg = new OutputTreeGrade(DataLayer, headerData, rdo.ReportID);
                                otg.CreateTreeGradeReports(strWriteOut, ref pageNumber);
                                break;
                            case "A14":
                                OutputUnitSummary ous = new OutputUnitSummary(DataLayer, headerData, rdo.ReportID);
                                ous.createUnitSummary(strWriteOut, ref pageNumber);
                                break;
                            case "ST1":
                            case "ST2":
                                OutputStats ost = new OutputStats("C", DataLayer, headerData, rdo.ReportID);
                                ost.CreateStatReports(strWriteOut, ref pageNumber);
                                break;
                            case "ST3":
                            case "ST4":
                                OutputStatsToo otoo = new OutputStatsToo(DataLayer, headerData, rdo.ReportID);
                                otoo.OutputStatReports(strWriteOut, ref pageNumber);
                                break;
                            case "VSM1":
                            case "VSM2":
                            case "VSM3":
                            case "VPA1":
                            case "VPA2":
                            case "VPA3":
                            case "VAL1":
                            case "VAL2":
                            case "VAL3":
                            case "VSM6":
                                OutputSummary os = new OutputSummary("C", DataLayer, headerData, rdo.ReportID);
                                os.OutputSummaryReports(strWriteOut, ref pageNumber);
                                break;
                            case "VSM4":
                            case "VSM5":
                                {
                                    OutputUnits ou = new OutputUnits("C", DataLayer, headerData, rdo.ReportID);
                                    ou.OutputUnitReports(strWriteOut, ref pageNumber);
                                    break;
                                }
                            case "UC1":
                            case "UC2":
                            case "UC3":
                            case "UC4":
                            case "UC5":
                            case "UC6":
                                {
                                    OutputUnits ou = new OutputUnits("C", DataLayer, headerData, rdo.ReportID);
                                    ou.OutputUnitReports(strWriteOut, ref pageNumber);
                                }
                                break;
                            case "TIM":
                                OutputTIM ot = new OutputTIM(DataLayer, headerData);
                                ot.CreateSUMfile();
                                break;
                            case "TC1":
                            case "TC2":
                            case "TC3":
                            case "TC4":
                            case "TC6":
                            case "TC8":
                            case "TC10":
                            case "TC12":
                            case "TC19":
                            case "TC20":
                            case "TC21":
                            case "TC22":
                            case "TC24":
                            case "TL1":
                            case "TL6":
                            case "TL7":
                            case "TL8":
                            case "TL9":
                            case "TL10":
                            case "TL12":
                                //  2-inch diameter class
                                OutputStandTables oStand2 = new OutputStandTables(DataLayer, headerData, rdo.ReportID);
                                oStand2.CreateStandTables(strWriteOut, ref pageNumber, 2);
                                break;
                            case "TC51":
                            case "TC52":
                            case "TC53":
                            case "TC54":
                            case "TC56":
                            case "TC57":
                            case "TC58":
                            case "TC59":
                            case "TC60":
                            case "TC62":
                            case "TC65":
                            case "TC71":
                            case "TC72":
                            case "TC74":
                            case "TL52":
                            case "TL54":
                            case "TL56":
                            case "TL58":
                            case "TL59":
                            case "TL60":
                            case "TL62":
                                //  1-inch diameter class
                                OutputStandTables oStand1 = new OutputStandTables(DataLayer, headerData, rdo.ReportID);
                                oStand1.CreateStandTables(strWriteOut, ref pageNumber, 1);
                                break;
                            case "UC7":
                            case "UC8":
                            case "UC9":
                            case "UC10":
                            case "UC11":
                            case "UC12":
                            case "UC13":
                            case "UC14":
                            case "UC15":
                            case "UC16":
                            case "UC17":
                            case "UC18":
                            case "UC19":
                            case "UC20":
                            case "UC21":
                            case "UC22":
                            case "UC23":
                            case "UC24":
                            case "UC25":
                            case "UC26":
                                //  UC stand tables
                                OutputUnitStandTables oUnits = new OutputUnitStandTables(DataLayer, headerData, rdo.ReportID);
                                oUnits.CreateUnitStandTables(strWriteOut, ref pageNumber);
                                break;
                            case "WT1":
                            case "WT2":
                            case "WT3":
                            case "WT4":
                            case "WT5":
                                OutputWeight ow = new OutputWeight(DataLayer, headerData, rdo.ReportID);
                                ow.OutputWeightReports(strWriteOut, ref pageNumber);
                                break;
                            case "LD1":
                            case "LD2":
                            case "LD3":
                            case "LD4":
                            case "LD5":
                            case "LD6":
                            case "LD7":
                            case "LD8":
                                OutputLiveDead old = new OutputLiveDead(DataLayer, headerData, rdo.ReportID);
                                old.CreateLiveDead(strWriteOut, ref pageNumber);
                                break;
                            case "L2":
                            case "L8":
                            case "L10":
                                OutputLogStock ols = new OutputLogStock(DataLayer, headerData, rdo.ReportID);
                                ols.CreateLogReports(strWriteOut, ref pageNumber);
                                break;
                            case "BLM01":
                            case "BLM02":
                            case "BLM03":
                            case "BLM04":
                            case "BLM05":
                            case "BLM06":
                            case "BLM07":
                            case "BLM08":
                            case "BLM09":
                            case "BLM10":
                                OutputBLM ob = new OutputBLM(DataLayer, dialogService, headerData, rdo.ReportID);
                                ob.CreateBLMreports(strWriteOut, ref pageNumber);
                                break;
                            case "R101":
                            case "R102":
                            case "R103":
                            case "R104":
                            case "R105":
                                OutputR1 r1 = new OutputR1(DataLayer, headerData, rdo.ReportID);
                                r1.CreateR1reports(strWriteOut, ref pageNumber);
                                break;
                            case "R201":
                            case "R202":
                            case "R203":
                            case "R204":
                            case "R205":
                            case "R206":
                            case "R207":
                            case "R208":
                                OutputR2 r2 = new OutputR2(DataLayer, dialogService, headerData, rdo.ReportID);
                                r2.CreateR2Reports(strWriteOut, ref pageNumber);
                                break;
                            case "R301":
                                OutputR3 r3 = new OutputR3(DataLayer, headerData, rdo.ReportID);
                                r3.CreateR3Reports(strWriteOut, ref pageNumber);
                                break;
                            case "R401":
                            case "R402":
                            case "R403":
                            case "R404":
                                OutputR4 r4 = new OutputR4(DataLayer, headerData, rdo.ReportID);
                                r4.CreateR4Reports(strWriteOut, ref pageNumber);
                                break;
                            case "R501":
                                OutputR5 r5 = new OutputR5(DataLayer, headerData, rdo.ReportID);
                                r5.CreateR5report(strWriteOut, ref pageNumber);
                                break;
                            case "R604":
                            case "R605":
                            case "R602":
                                OutputR6 r6 = new OutputR6(DataLayer, headerData, rdo.ReportID);
                                r6.CreateR6reports(strWriteOut, ref pageNumber);
                                break;
                            case "R801":
                            case "R802":
                                OutputR8 r8 = new OutputR8(DataLayer, headerData, rdo.ReportID);
                                r8.CreateR8Reports(strWriteOut, ref pageNumber);
                                break;
                            case "R902":
                                OutputR9 r9 = new OutputR9(DataLayer, headerData, rdo.ReportID);
                                //r9.CreateR9Reports(strWriteOut, ref pageNumber, rh);
                                r9.OutputTipwoodReport(strWriteOut, ref pageNumber);
                                break;
                            case "R001":
                            case "R002":
                            case "R003":
                            case "R004":
                            case "R005":
                            case "R006":
                            case "R007":
                            case "R008":
                            case "R009":
                                OutputR10 r10 = new OutputR10(DataLayer, dialogService, headerData, rdo.ReportID);
                                r10.CreateR10reports(strWriteOut, ref pageNumber);
                                break;
                            case "SC1":
                            case "SC2":
                            case "SC3":
                                OutputStemCounts osc = new OutputStemCounts(DataLayer, headerData, rdo.ReportID);
                                osc.createStemCountReports(strWriteOut, ref pageNumber);
                                break;
                            case "LV01":
                            case "LV02":
                            case "LV03":
                            case "LV04":
                            case "LV05":
                                OutputLeave olt = new OutputLeave(DataLayer, headerData, rdo.ReportID);
                                olt.createLeaveTreeReports(strWriteOut, ref pageNumber);
                                break;
                            case "GR01":
                            case "GR02":
                            case "GR03":
                            case "GR04":
                            case "GR05":
                            case "GR06":
                            case "GR07":
                            case "GR08":
                            case "GR09":
                            case "GR10":
                            case "GR11":
                                graphReports.Add(rdo.ReportID);
                                string graphFile = System.IO.Path.GetDirectoryName(FilePath);
                                graphFile += "\\Graphs\\";
                                graphFile += rdo.ReportID;
                                strWriteOut.WriteLine("\f");
                                strWriteOut.Write("Graph Report ");
                                strWriteOut.Write(rdo.ReportID);
                                strWriteOut.WriteLine(" can be found in the following file: ");
                                strWriteOut.WriteLine(graphFile);
                                break;
                        }   //  end switch
                    }
                    catch(Exception e)
                    {
                        dialogService.ShowError($"Error Creating Report: {rdo.ReportID}" + "\r\n" + e.Message);
                    }
                }   //  end foreach loop

                //  Any warning messages to print?
                List<ErrorLogDO> errList = DataLayer.getErrorMessages("W", "CruiseProcessing");
                List<ErrorLogDO> errListToo = DataLayer.getErrorMessages("W", "FScruiser");
                int warnFlag = 0;
                if (errList.Count > 0)
                {
                    WriteWarnings(strWriteOut, errList, ref pageNumber, headerData);
                    warnFlag = 1;
                }
                if (errListToo.Count > 0)
                {
                    WriteOtherWarnings(strWriteOut, errListToo, ref pageNumber, headerData);
                    warnFlag = 1;
                }
                if (warnFlag > 0)
                    dialogService.ShowInformation("Warning messages detected.\nCheck end of output file for the list.");

                strWriteOut.Close();

                //  create graphs if reports requested
                if (graphReports.Any())
                {
                    dialogService.ShowGraphOutputDialog(graphReports);
                }   //  endif graphs selected

            }   //  end using

            return;
        }   //  end createTextFile

        private void WriteWarnings(StreamWriter strWriteOut, List<ErrorLogDO> errList, ref int pageNumb, HeaderFieldData headerData)
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
            string[] warningHeader = new string[] { "   IDENTIFIER                        WARNING MESSAGE" };

            foreach (ErrorLogDO eld in errList)
            {
                StringBuilder sb = new StringBuilder();

                numOlines = ReportGeneratorBase.WriteReportHeading(strWriteOut, "WARNING MESSAGES", "", "", warningHeader, 8, ref pageNumb, "", headerData, numOlines, null);
                //  build identifier
                if (eld.TableName == "SUM file")
                {
                    sb.Append(eld.TableName);
                    sb.Append("  ---------    ");
                    sb.Append(WarnMessages[Convert.ToInt16(eld.Message)]);
                }
                else if (eld.TableName == "Stratum" || eld.TableName == "VolumeEquation")
                {
                    sb.Append(Utilities.GetIdentifier(eld.TableName, eld.CN_Number, DataLayer));
                    sb.Append("   ");
                    sb.Append(eld.Message);
                }
                else
                {
                    sb.Append(Utilities.GetIdentifier(eld.TableName, eld.CN_Number, DataLayer));
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
        }


        private void WriteOtherWarnings(StreamWriter strWriteOut, List<ErrorLogDO> errListToo, ref int pageNumb, HeaderFieldData headerData)
        {
            //  adds warnings from FScruiser and/or CruiseManager to the bottom of the output text file
            numOlines = 0;
            string[] warningHeader = new string[] { "   IDENTIFIER                        WARNING MESSAGE" };
            
            foreach (ErrorLogDO elt in errListToo)
            {
                StringBuilder sb = new StringBuilder();
                numOlines = ReportGeneratorBase.WriteReportHeading(strWriteOut, "WARNING MESSAGES", "", "", warningHeader, 8, ref pageNumb, "", headerData, numOlines, null);
                //  build identifier
                sb.Append(Utilities.GetIdentifier(elt.TableName, elt.CN_Number, DataLayer));
                sb.Append("   ");
                sb.Append(elt.Message);
                strWriteOut.WriteLine(sb.ToString());
                numOlines++;
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

    }
}

