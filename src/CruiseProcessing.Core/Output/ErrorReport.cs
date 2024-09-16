using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using CruiseProcessing.Data;
using CruiseProcessing.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CruiseProcessing
{
    public class ErrorReport : OutputFileReportGeneratorBase
    {
        public const int MAX_LINES_PER_PAGE = 54;

        public static readonly string[,] ERROR_MESSAGE_LOOKUP = new string[32, 2] {{"1","Invalid volume, value or quality adjustment equation number"},
                                                        {"2","Value or quality adjustment coefficient missing on this record"},
                                                        {"3","Must find primary product before secondary product"},
                                                        {"4","Secondary top DIB is greater than primary top DIB"},
                                                        {"5","Duplicate volumes requested on same species/product code"},
                                                        {"6","Same equation number with different top diameters"},
                                                        {"7","Invalid ... number"},
                                                        {"8","... cannot be blank or empty"},
                                                        {"9","... contains an invalid code"},
                                                        {"10","Tree count cannot be greater than 1 for this record"},
                                                        {"11","Count/measure code is in error on this record"},
                                                        {"12","No volume equation match"},
                                                        {"13","More than 20 logs for this tree"},
                                                        {"14","Strata/Sample Group tree must have same UOM, product and cut/leave codes"}, // not possible due to data structure
                                                        {"15","Plot cannot have both count and measure trees"},
                                                        {"16","Method must have species, product, UOM, and/or leave codes blank"},
                                                        {"17","Method must have one sample group per stratum"},
                                                        {"18","Cannot have an upper stem diameter greater than DBH"},
                                                        {"19","Variable log length cruise is missing mandatory fields"},
                                                        {"20","Gross data must be greater than or equal to net data"},
                                                        {"21","Species, product or UOM codes must be entered on this record"},
                                                        {"22","Basal area factor required for Point cruises"},
                                                        {"23","Fixed plot size required for Fixed Plot cruises"},
                                                        {"24","Acres required for area based cruises"},
                                                        {"25","... table cannot be empty"},
                                                        {"26","No reports selected"},
                                                        {"27","KPI estimate cannot be zero for this tree"},
                                                        {"28","More than one sale record not allowed"},
                                                        {"29","Percent Recoverable cannot be greater than Percent Seen Defect"},
                                                        {"30","Sample group has no measured trees"},
                                                        {"31","Invalid tree default value key code on tree; Contact FMSC for help"},
                                                        {"32","No height recorded for this tree"}};

        public static readonly string[] WARNING_MESSAGE_LOOKUP = new string[]  {"   ",
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

        // see CreateTextFile.WriteWarnings for additional messages

        private static readonly string[] ERROR_HEADER = new string[6] {"                                                      ****  ERROR REPORT  ****",
                                                       "                                                          FOR CRUISE NUMBER",
                                                       "",
                                                       " THESE ERRORS WERE GENERATED BY XXXXX",
                                                       " TABLE                                                                                   IDENTIFICATION",
                                                       " NAME               ERROR                                                                ST CU  PL   TR   LG  SP     SG PR EQ"};

        public ErrorReport(CpDataLayer dataLayer, HeaderFieldData headerData)
            : base(dataLayer, headerData) // this report has its own writeHeaders method so it doesn't use HeaderFieldData
        {
            Sale = DataLayer.GetSale();
        }

        public SaleDO Sale { get; }

        public string PrintErrorReport(IEnumerable<ErrorLogDO> errList, string programName)
        {
            //  fix filename for output
            var outFile = System.IO.Path.ChangeExtension(FilePath, "out");
            

            //  open output file and write report
            using (StreamWriter strWriteOut = new StreamWriter(outFile))
            {
                WriteErrorReport(strWriteOut, errList, programName, outFile);

                strWriteOut.Close();
            }   //  end using

            return outFile;
        }

        public void WriteErrorReport(TextWriter writer, IEnumerable<ErrorLogDO> errList, string programName, string outFile)
        {
            //  what's the current region?
            string currRegion = DataLayer.getRegion();

            int pageNumber = 1;
            //  Output banner page except for BLM
            if (currRegion != "07" && currRegion != "7" && currRegion != "BLM")
            {
                var reports = DataLayer.GetSelectedReports();

                var bannerPage = BannerPage.GenerateBannerPage(FilePath, HeaderData, Sale, reports, Enumerable.Empty<VolumeEquationDO>());
                writer.Write(bannerPage);
                pageNumber++;
            }   //  endif

            pageNumber = WriteErrors(writer, errList.Where(e => e.Level == "E"), programName, outFile, pageNumber);

            WriteWarnings(writer, errList.Where(e => e.Level == "W"), ref pageNumber, HeaderData, DataLayer);
        }

        private int WriteErrors(TextWriter strWriteOut, IEnumerable<ErrorLogDO> errList, string programName, string outFile, int pageNumber)
        {
            //  output errors only -- warnings printed in regular output file
            var fieldLengths = new int[] { 1, 18, 69, 43 };

            var errorHeading = new List<string>(ERROR_HEADER);
            //  add program name to error heading
            errorHeading[3] = errorHeading[3].Replace("XXXXX", programName);
            //  add cruise number to report title
            string cruiseNumber = DataLayer.getCruiseNumber();
            errorHeading[2] = "                                                              " + cruiseNumber;

            //  write appropriate error messages
            foreach (ErrorLogDO errorLog in errList)
            {
                numOlines = WriteHeader_ErrorLog(strWriteOut, outFile, errorHeading, HeaderData.Date, numOlines, ref pageNumber);

                var prtFields = MakeErrorEntryFields(errorLog);

                string line = buildPrintLine(fieldLengths, prtFields);
                strWriteOut.WriteLine(line);
                numOlines++;
            }

            //  output identification key
            OutputIdKey(strWriteOut);
            return pageNumber;
        }

        private List<string> MakeErrorEntryFields(ErrorLogDO eld)
        {
            var prtFields = new List<string>();

            prtFields.Add("");
            prtFields.Add(eld.TableName);

            if (eld.TableName == "CountTree" || eld.Program != "CruiseProcessing")
            {
                var message = eld.Message ?? "";
                if (message.Length > 69)
                {
                    message = message.Substring(0, 69);
                }

                prtFields.Add(message);
                //  add appropriate identifier for table
                var ident = GetIdentifier(eld.TableName, eld.CN_Number);
                prtFields.Add(ident);

                return prtFields;
            }
            else
            {
                string ident = null;
                var currError = new StringBuilder();
                currError.Append(eld.Message);

                var errMessage = GetErrorMessage(eld.Message);
                if (!string.IsNullOrEmpty(errMessage))
                {
                    currError.Append("-");

                    if (eld.Message == "25")
                    {
                        errMessage = errMessage.Replace("...", eld.TableName);
                        ident = eld.TableName;
                    }
                    else if (eld.Message == "7"
                        || eld.Message == "8"
                        || eld.Message == "9")
                    {
                        errMessage = errMessage.Replace("...", eld.ColumnName);
                    }
                }
                currError.Append(errMessage);

                var curErrorStr = currError.ToString().PadRight(69, ' ');
                if(curErrorStr.Length > 69)
                { curErrorStr = curErrorStr.Substring(0, 69); }

                prtFields.Add(curErrorStr);

                ident = ident ?? GetIdentifier(eld.TableName, eld.CN_Number);
                prtFields.Add(ident);
            }

            return prtFields;
        }

        protected string GetIdentifier(string tableName, long CNtoFind)
        {
            return GetIdentifier(tableName, CNtoFind, DataLayer);
        }

        public static string GetIdentifier(string tableName, long CNtoFind, CpDataLayer bslyr)
        {
            StringBuilder ident = new StringBuilder();
            int ithRow = -1;

            switch (tableName)
            {
                case "Sale":
                    var sale = bslyr.GetAllSaleRecords().Where(s => s.Sale_CN == CNtoFind)
                        .FirstOrDefault();
                    if (sale != null)
                    {
                        ident.Append("Sale number = ");
                        ident.Append(sale.SaleNumber);
                    }
                    else ident.Append("Sale number not found");
                    break;

                case "Stratum":
                    var stratum = bslyr.GetStrtum((int)CNtoFind);
                    if (stratum != null)
                        ident.Append(stratum.Code);
                    else ident.Append("Stratum code not found");
                    break;

                case "Cutting Unit":
                    List<CuttingUnitDO> cList = bslyr.getCuttingUnits();
                    ithRow = cList.FindIndex(
                        delegate (CuttingUnitDO cu)
                        {
                            return cu.CuttingUnit_CN == CNtoFind;
                        });
                    if (ithRow >= 0)
                    {
                        ident.Append("   ");
                        ident.Append(cList[ithRow].Code.PadLeft(3, ' '));
                    }
                    else ident.Append("Cutting unit not found");
                    break;

                case "Tree":
                    List<TreeDO> tList = bslyr.getTrees();
                    ithRow = tList.FindIndex(
                        delegate (TreeDO td)
                        {
                            return td.Tree_CN == CNtoFind;
                        });
                    if (ithRow >= 0)
                    {
                        ident.Append(tList[ithRow].Stratum.Code.PadRight(3, ' '));
                        ident.Append(tList[ithRow].CuttingUnit.Code.PadLeft(3, ' '));
                        if (tList[ithRow].Plot == null)
                            ident.Append("     ");
                        else if (tList[ithRow].Plot_CN == 0)
                            ident.Append("     ");
                        else ident.Append(tList[ithRow].Plot.PlotNumber.ToString().PadLeft(5, ' '));
                        ident.Append(tList[ithRow].TreeNumber.ToString().PadLeft(5, ' '));
                        ident.Append(" --- ");
                        if (tList[ithRow].Species == null)
                            ident.Append("       ");
                        else ident.Append(tList[ithRow].Species.PadRight(7, ' '));
                        if (tList[ithRow].SampleGroup == null)
                            ident.Append("   ");
                        else
                        {
                            if (tList[ithRow].SampleGroup.Code == "" || tList[ithRow].SampleGroup.Code == " " ||
                                tList[ithRow].SampleGroup.Code == "<Blank>" || tList[ithRow].SampleGroup.Code == null)
                                ident.Append("   ");
                            else ident.Append(tList[ithRow].SampleGroup.Code.PadRight(3, ' '));
                            ident.Append(tList[ithRow].SampleGroup.PrimaryProduct.PadRight(3, ' '));
                        }   //  endif
                    }
                    else ident.Append("Tree not found");
                    break;

                case "Log":
                    List<LogDO> lList = bslyr.getLogs();
                    ithRow = lList.FindIndex(
                        delegate (LogDO ld)
                        {
                            return ld.Log_CN == CNtoFind;
                        });
                    if (ithRow >= 0)
                    {
                        ident.Append(lList[ithRow].Tree.Stratum.Code.PadRight(3, ' '));
                        ident.Append(lList[ithRow].Tree.CuttingUnit.Code.PadLeft(3, ' '));
                        if (lList[ithRow].Tree.Plot == null)
                            ident.Append("     ");
                        else ident.Append(lList[ithRow].Tree.Plot.PlotNumber.ToString().PadLeft(5, ' '));
                        ident.Append(lList[ithRow].Tree.TreeNumber.ToString().PadLeft(5, ' '));
                        ident.Append(lList[ithRow].LogNumber.PadLeft(3, ' '));
                    }
                    else ident.Append("Log not found");
                    break;

                case "Volume Equation":
                    if (CNtoFind == 0) CNtoFind = 1;
                    List<VolumeEquationDO> vList = bslyr.getVolumeEquations();
                    ident.Append("-- --- ---- ---- --- ");
                    ident.Append(vList[(int)CNtoFind - 1].Species.PadRight(7, ' '));
                    ident.Append("-- ");
                    ident.Append(vList[(int)CNtoFind - 1].PrimaryProduct.PadRight(3, ' '));
                    ident.Append(vList[(int)CNtoFind - 1].VolumeEquationNumber.PadRight(10, ' '));
                    break;

                case "Value Equation":
                    if (CNtoFind == 0) CNtoFind = 1;
                    List<ValueEquationDO> veList = bslyr.getValueEquations();
                    ident.Append("-- --- ---- ---- --- ");
                    ident.Append(veList[(int)CNtoFind - 1].Species.PadRight(7, ' '));
                    ident.Append("-- ");
                    ident.Append(veList[(int)CNtoFind - 1].PrimaryProduct.PadRight(3, ' '));
                    ident.Append(veList[(int)CNtoFind - 1].ValueEquationNumber.PadRight(10, ' '));
                    break;

                case "Quality Adjustment":
                    if (CNtoFind == 0) CNtoFind = 1;
                    List<QualityAdjEquationDO> qList = bslyr.getQualAdjEquations();
                    ident.Append("-- --- ---- ---- --- ");
                    ident.Append(qList[(int)CNtoFind - 1].Species.PadRight(7, ' '));
                    ident.Append("-- -- ");
                    ident.Append(qList[(int)CNtoFind - 1].QualityAdjEq.PadRight(10, ' '));
                    break;

                case "SampleGroup":
                    List<SampleGroupDO> sgList = bslyr.getSampleGroups();
                    ithRow = sgList.FindIndex(
                        delegate (SampleGroupDO sgd)
                        {
                            return sgd.SampleGroup_CN == CNtoFind;
                        });
                    if (ithRow >= 0)
                    {
                        ident.Append(sgList[ithRow].Stratum.Code.PadRight(3, ' '));
                        ident.Append("--- ---- ---- --- ------ ");
                        ident.Append(sgList[ithRow].Code.PadRight(3, ' '));
                        ident.Append(sgList[ithRow].PrimaryProduct.PadRight(3, ' '));
                    }
                    else ident.Append("Sample Group not found");
                    break;
                default:
                    { return tableName ?? ""; }
            }   //  end switch
            return ident.ToString();
        }   //  end GetIdentifier

        public static string GetErrorMessage(string errNumber)
        {
            if (errNumber.Length <= 2)
            {
                for (int k = 0; k < ERROR_MESSAGE_LOOKUP.GetLength(0); k++)
                {
                    if (errNumber == ERROR_MESSAGE_LOOKUP[k, 0])
                        return ERROR_MESSAGE_LOOKUP[k, 1];
                }   //  end for k loop
            }

            return ""; // errNumber is not a number and not in messages, then we can assume it is the error message
        }   //  end getErrorMessage

        public static void WriteWarnings(TextWriter strWriteOut, IEnumerable<ErrorLogDO> errList, ref int pageNumb, HeaderFieldData headerData, CpDataLayer dataLayer)
        {
            var lineNumber = 0;
            string[] warningHeader = new string[] { "   IDENTIFIER                        WARNING MESSAGE" };

            foreach (ErrorLogDO eld in errList)
            {
                lineNumber = OutputFileReportGeneratorBase.WriteReportHeading(strWriteOut, "WARNING MESSAGES", "", "", warningHeader, 8, ref pageNumb, "", headerData, lineNumber, null);

                strWriteOut.WriteLine(ErrorReport.GetWarningLine(eld, dataLayer));
                lineNumber++;
            }   //  end foreach loop

            //  output the key for the identifier
            WriteWarningFieldOrders(strWriteOut);
            lineNumber += 12;
        }

        public static string GetWarningLine(ErrorLogDO eld, CpDataLayer dataLayer)
        {
            StringBuilder sb = new StringBuilder();
            if (eld.Program == "CruiseProcessing")
            {
                //  build identifier
                if (eld.TableName == "SUM file")
                {
                    sb.Append(eld.TableName);
                    sb.Append("  ---------    ");
                    sb.Append(GetWarningMessage(eld.Message));
                }
                else if (eld.TableName == "Stratum" || eld.TableName == "VolumeEquation")
                {
                    sb.Append(GetIdentifier(eld.TableName, eld.CN_Number, dataLayer));
                    sb.Append("   ");
                    sb.Append(eld.Message);
                }
                else
                {
                    sb.Append(GetIdentifier(eld.TableName, eld.CN_Number, dataLayer));
                    sb.Append("   ");
                    if (eld.Message == "30")
                    {
                        //  this is just a warning and will never use the record in the error list
                        sb.Append("Sample group must have at least one measured tree");
                    }
                    else sb.Append(GetWarningMessage(eld.Message));
                }   //  endif
            }
            else
            {
                sb.Append(GetIdentifier(eld.TableName, eld.CN_Number, dataLayer));
                sb.Append("   ");
                sb.Append(eld.Message);
            }

            return sb.ToString();
        }

        public static string GetWarningMessage(string errLogMsg)
        {
            if (errLogMsg.Length <= 4
                && int.TryParse(errLogMsg, out var imsg)
                && (imsg >= 0 && imsg < WARNING_MESSAGE_LOOKUP.Length))
            {
                return WARNING_MESSAGE_LOOKUP[imsg];
            }

            return errLogMsg;
        }

        private static int WriteHeader_ErrorLog(TextWriter strWriteOut, string outFile, IReadOnlyList<string> headings, string currentDate, int lineNum, ref int pageNumber)
        {
            if (lineNum >= MAX_LINES_PER_PAGE || lineNum == 0)
            {
                strWriteOut.WriteLine("\f");
                strWriteOut.WriteLine("RUN DATE & TIME " + currentDate + "                                                                                     PAGE  " + pageNumber);
                strWriteOut.WriteLine("FILENAME:  " + outFile);
                lineNum = 2;
                //  write report title and column headings
                for (int j = 0; j < 5; j++)
                {
                    strWriteOut.WriteLine(headings[j]);
                    lineNum++;
                }   //  end for j loop

                pageNumber++;
            }   //  end numOlines

            return lineNum;
        }   //  end writeHeaders

        private static void OutputIdKey(TextWriter strWriteOut)
        {
            strWriteOut.WriteLine("\n");
            strWriteOut.WriteLine("Order of Identification Elements");
            strWriteOut.WriteLine("Some elements may be blank");
            strWriteOut.WriteLine("ST = Stratum");
            strWriteOut.WriteLine("CU = Cutting Unit");
            strWriteOut.WriteLine("PL = Plot");
            strWriteOut.WriteLine("TR = Tree number");
            strWriteOut.WriteLine("LG = log number");
            strWriteOut.WriteLine("SP = Species");
            strWriteOut.WriteLine("SG = Sample Group");
            strWriteOut.WriteLine("PR = Primary Product");
            strWriteOut.WriteLine("EQ = Equation");
            return;
        }   //  end OutputIdKey

        private static void WriteWarningFieldOrders(TextWriter strWriteOut)
        {
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
        }
    }   //  end ErrorReport
}