using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseProcessing.Services;
using CruiseProcessing.Output;
using CruiseProcessing.Data;
using Microsoft.Extensions.Logging;
using CruiseProcessing.Interop;
using CruiseProcessing.OutputModels;

namespace CruiseProcessing
{
    public class CreateTextFile
    {
        public string currentRegion { get; }
        public string textFile { get; }
        protected CpDataLayer DataLayer { get; }
        public IVolumeLibrary VolLib { get; }

        protected string FilePath => DataLayer.FilePath;
        protected SaleDO Sale { get; }

        protected ILogger Log { get; }

        public CreateTextFile(CpDataLayer dataLayer, IVolumeLibrary volLib, ILogger<CreateTextFile> log)
        {
            Log = log;
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            VolLib = volLib ?? throw new ArgumentNullException(nameof(volLib));
            textFile = System.IO.Path.ChangeExtension(FilePath, "out");

            Sale = DataLayer.GetSale();
            currentRegion = Sale.Region;
        }

        public void createTextFile(IDialogService dialogService, IEnumerable<ReportsDO> selectedReports)
        {


            if (currentRegion == "10")
            {
                if (selectedReports.Any(x => x.ReportID == "L1"))
                {
                    dialogService.ShowInformation("L1 report for Region 10 does not appear in text output file.\r\nInstead, create CSV5 to create file for cut and/or leave trees.");
                }
                if (selectedReports.Any(x => x.ReportID == "R008"))
                {
                    List<LogMatrixDO> reportMatrix = DataLayer.getLogMatrix("R008");
                    if (!reportMatrix.Any())
                    {
                        dialogService.ShowError("The log matrix is missing for the R008 report.\nPlease go to Reports>Region reports>10>Additional Data to load the default log matrix.");
                    }
                }
                if (selectedReports.Any(x => x.ReportID == "R009"))
                {
                    List<LogMatrixDO> reportMatrix = DataLayer.getLogMatrix("R009");
                    if (!reportMatrix.Any())
                    {
                        dialogService.ShowError("The log matrix is missing for the R009 report.\nPlease go to Reports>Region reports>10>Additional Data to load the default log matrix.");
                    }
                }
            }

            if (selectedReports.Any(x => x.ReportID == "R208"))
            {
                //IEnumerable<StewProductCosts> stewCosts = Enumerable.Empty<StewProductCosts>();
                if (!DataLayer.doesTableExist(nameof(StewProductCosts)))
                {
                    DataLayer.CreateNewTable(nameof(StewProductCosts));
                }

                var stewCosts = DataLayer.getStewCosts()
                    .Where(x => x.includeInReport == "True")
                    .ToArray();

                if(!stewCosts.Any())
                {
                    dialogService.GetStewardshipProductCosts();

                    stewCosts = DataLayer.getStewCosts()
                    .Where(x => x.includeInReport == "True")
                    .ToArray();
                }

                if(!stewCosts.Any())
                {
                    dialogService.ShowError("R208 Report: No species groups selected to include in the report.");
                    return;
                }
            }

            //  open up file and start writing
            using StreamWriter strWriteOut = new StreamWriter(textFile);

            var headerData = DataLayer.GetReportHeaderData();
            var graphReports = CreateOutFile(selectedReports, headerData, strWriteOut, out var failedReports, out var hasWarnings);

            strWriteOut.Close();

            if (hasWarnings)
            {
                dialogService.ShowInformation("Warning messages detected.\nCheck end of output file for the list.");
            }

            if (failedReports.Any())
            {
                dialogService.ShowError($"Some Reports Failed To Generate:\r\n{String.Join(",", failedReports)}");
            }

            if (selectedReports.Any(x => x.ReportID == "TIM"))
            {
                try
                {
                    Log?.LogInformation("Creating Sum file");

                    OutputTIM ot = new OutputTIM(DataLayer, headerData);
                    ot.CreateSUMfile();
                }
                catch (Exception e)
                {
                    Log?.LogError(e, "Error Creating Sum file");
                    dialogService.ShowError($"Error Creating Report: TIM" + "\r\n" + e.Message);
                }
            }

            //  create graphs if reports requested
            if (graphReports.Any())
            {
                dialogService.ShowGraphOutputDialog(graphReports);
            }
        }

        public IEnumerable<string> CreateOutFile(IEnumerable<ReportsDO> selectedReports, HeaderFieldData headerData, TextWriter strWriteOut, out IEnumerable<string> failedReports, out bool hasWarnings)
        {
            Log?.LogInformation("Begin Creating Out File. Region: {Region} Selected Reports: {SelectedReports}", currentRegion, string.Join(", ", selectedReports.Select(x => x.ReportID)));

            hasWarnings = false;
            var graphReports = new List<string>();
            var volumeEquations = DataLayer.getVolumeEquations();
            var failedReportsList = new List<string>();

            //  Output banner page
            strWriteOut.Write(BannerPage.GenerateBannerPage(FilePath, headerData, Sale, selectedReports, volumeEquations));

            int pageNumber = 2;

            //  Output equation tables as needed
            OutputEquationTables oet = new OutputEquationTables(DataLayer, headerData);
            oet.outputEquationTable(strWriteOut, ref pageNumber);

            //  Output selected reports
            //  create objects for each report category and assign header fields as needed
            foreach (ReportsDO rdo in selectedReports)
            {
                Log?.LogInformation("Report: " + rdo.ReportID + ", Status:{Status}", "Starting");

                if (ReportsDataservice.IsGraphReport(rdo.ReportID))
                {
                    graphReports.Add(rdo.ReportID);
                    var graphsDirectoryPath = DataLayer.GetGraphsFolderPath();

                    var graphFilePath = Path.Combine(graphsDirectoryPath, rdo.ReportID);

                    strWriteOut.WriteLine("\f"); // page break/form feed
                    strWriteOut.Write("Graph Report ");
                    strWriteOut.Write(rdo.ReportID);
                    strWriteOut.WriteLine(" can be found in the following file: ");
                    strWriteOut.WriteLine(graphFilePath);

                    Log?.LogInformation("Report: " + rdo.ReportID + ", Status:{Status}", "Done");

                    continue;
                }


                try
                {
                    var reportPageNum = pageNumber;
                    using var reportStringWriter = new StringWriter();
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
                            if (currentRegion == "10" && rdo.ReportID == "L1")
                                continue;

                            OutputList ol = new OutputList(DataLayer, headerData, rdo.ReportID);
                            ol.OutputListReports(reportStringWriter, ref reportPageNum);
                            break;

                        case "A11":
                        case "A12":
                            OutputTreeGrade otg = new OutputTreeGrade(DataLayer, headerData, rdo.ReportID);
                            otg.CreateTreeGradeReports(strWriteOut, ref reportPageNum);
                            break;

                        case "A14":
                            OutputUnitSummary ous = new OutputUnitSummary(DataLayer, headerData, rdo.ReportID);
                            ous.createUnitSummary(strWriteOut, ref reportPageNum);
                            break;

                        case "ST1":
                        case "ST2":
                            OutputStats ost = new OutputStats("C", DataLayer, headerData, rdo.ReportID);
                            ost.CreateStatReports(strWriteOut, ref reportPageNum);
                            break;

                        case "ST3":
                        case "ST4":
                            OutputStatsToo otoo = new OutputStatsToo(DataLayer, headerData, rdo.ReportID);
                            otoo.OutputStatReports(strWriteOut, ref reportPageNum);
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
                            os.OutputSummaryReports(strWriteOut, ref reportPageNum);
                            break;

                        case "VSM4":
                        case "VSM5":
                            {
                                OutputUnits ou = new OutputUnits("C", DataLayer, headerData, rdo.ReportID);
                                ou.OutputUnitReports(strWriteOut, ref reportPageNum);
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
                                ou.OutputUnitReports(strWriteOut, ref reportPageNum);
                            }
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
                            oStand2.CreateStandTables(strWriteOut, ref reportPageNum, 2);
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
                            oStand1.CreateStandTables(strWriteOut, ref reportPageNum, 1);
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
                            oUnits.CreateUnitStandTables(strWriteOut, ref reportPageNum);
                            break;

                        case "WT1":
                            {
                                var ow1 = new Wt1ReportGenerator(DataLayer, headerData, Log);
                                reportPageNum = ow1.GenerateReport(strWriteOut, reportPageNum);
                                break;
                            }
                        case "WT2":
                            {
                                var reportGenerator = new Wt2ReportGenerator(DataLayer, VolLib, headerData);
                                reportPageNum = reportGenerator.GenerateReport(strWriteOut, reportPageNum);
                                break;
                            }
                        case "WT3":
                                                        {
                                var reportGenerator = new Wt3ReportGenerator(DataLayer, VolLib, headerData);
                                reportPageNum = reportGenerator.GenerateReport(strWriteOut, reportPageNum);
                                break;
                            }
                        case "WT4":
                            {
                                var reportGenerator = new Wt4ReportGenerator(DataLayer, headerData, Log);
                                reportPageNum = reportGenerator.GenerateReport(strWriteOut, reportPageNum);
                                break;
                            }
                        case "WT5":
                            {
                                var reportGenerator = new Wt5ReportGenerator(DataLayer, headerData, Log);
                                reportPageNum = reportGenerator.GenerateReport(strWriteOut, reportPageNum);
                                break;
                            }

                        case "LD1":
                        case "LD2":
                        case "LD3":
                        case "LD4":
                        case "LD5":
                        case "LD6":
                        case "LD7":
                        case "LD8":
                            OutputLiveDead old = new OutputLiveDead(DataLayer, headerData, rdo.ReportID);
                            old.CreateLiveDead(strWriteOut, ref reportPageNum);
                            break;

                        case "L2":
                        case "L8":
                        case "L10":
                            OutputLogStock ols = new OutputLogStock(DataLayer, headerData, rdo.ReportID);
                            ols.CreateLogReports(strWriteOut, ref reportPageNum);
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
                            OutputBLM ob = new OutputBLM(DataLayer, headerData, rdo.ReportID);
                            ob.CreateBLMreports(strWriteOut, ref reportPageNum);
                            break;

                        case "R101":
                        case "R102":
                        case "R103":
                        case "R104":
                        case "R105":
                            OutputR1 r1 = new OutputR1(DataLayer, headerData, rdo.ReportID);
                            r1.CreateR1reports(strWriteOut, ref reportPageNum);
                            break;

                        case "R201":
                        case "R202":
                        case "R203":
                        case "R204":
                        case "R205":
                        case "R206":
                        case "R207":
                        case "R208":
                            OutputR2 r2 = new OutputR2(DataLayer, headerData, rdo.ReportID);
                            r2.CreateR2Reports(strWriteOut, ref reportPageNum);
                            break;

                        case "R301":
                            OutputR3 r3 = new OutputR3(DataLayer, headerData, rdo.ReportID);
                            r3.CreateR3Reports(strWriteOut, ref reportPageNum);
                            break;

                        case "R401":
                        case "R402":
                        case "R403":
                        case "R404":
                            OutputR4 r4 = new OutputR4(DataLayer, headerData, rdo.ReportID);
                            r4.CreateR4Reports(strWriteOut, ref reportPageNum);
                            break;

                        case "R501":
                            OutputR5 r5 = new OutputR5(DataLayer, headerData, rdo.ReportID);
                            r5.CreateR5report(strWriteOut, ref reportPageNum);
                            break;

                        case "R604":
                        case "R605":
                        case "R602":
                            OutputR6 r6 = new OutputR6(DataLayer, headerData, rdo.ReportID);
                            r6.CreateR6reports(strWriteOut, ref reportPageNum);
                            break;

                        case "R801":
                        case "R802":
                            OutputR8 r8 = new OutputR8(DataLayer, headerData, rdo.ReportID);
                            r8.CreateR8Reports(strWriteOut, ref reportPageNum);
                            break;

                        case "R902":
                            OutputR9 r9 = new OutputR9(DataLayer, headerData, rdo.ReportID);
                            //r9.CreateR9Reports(strWriteOut, ref pageNumber, rh);
                            r9.OutputTipwoodReport(strWriteOut, ref reportPageNum);
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
                            OutputR10 r10 = new OutputR10(DataLayer, headerData, rdo.ReportID);
                            r10.CreateR10reports(strWriteOut, ref reportPageNum);
                            break;

                        case "SC1":
                        case "SC2":
                        case "SC3":
                            OutputStemCounts osc = new OutputStemCounts(DataLayer, headerData, rdo.ReportID);
                            osc.createStemCountReports(strWriteOut, ref reportPageNum);
                            break;

                        case "LV01":
                        case "LV02":
                        case "LV03":
                        case "LV04":
                        case "LV05":
                            OutputLeave olt = new OutputLeave(DataLayer, headerData, rdo.ReportID);
                            olt.createLeaveTreeReports(strWriteOut, ref reportPageNum);
                            break;
                    }

                    strWriteOut.Write(reportStringWriter.ToString());
                    pageNumber = reportPageNum;
                    Log?.LogInformation("Report: " + rdo.ReportID + ", Status:{Status}", "Done");
                }
                catch (Exception e)
                {
                    Log?.LogInformation("Report: " + rdo.ReportID + ", Status:{Status}", "Error");
                    Log?.LogError(e, "Error Creating Report: {ReportID}", rdo.ReportID);
                    failedReportsList.Add(rdo.ReportID);
                }
            }   //  end foreach loop

            //  Any warning messages to print?
            List<ErrorLogDO> cpErrors = DataLayer.getErrorMessages("W", "CruiseProcessing");
            List<ErrorLogDO> fsCrzErrors = DataLayer.getErrorMessages("W", "FScruiser");
            if (cpErrors.Any())
            {
                Log?.LogWarning("CP Warning messages detected.");
                ErrorReport.WriteWarnings(strWriteOut, cpErrors, ref pageNumber, headerData, DataLayer);
                hasWarnings = true;
            }
            if (fsCrzErrors.Any())
            {
                Log?.LogWarning("Audit Rule Warning messages detected.");
                ErrorReport.WriteWarnings(strWriteOut, fsCrzErrors, ref pageNumber, headerData, DataLayer);
                hasWarnings = true;
            }
            //ErrorReport.WriteInfo(strWriteOut, ref pageNumber, headerData, DataLayer);

            failedReports = failedReportsList.ToArray();

            return graphReports;
        }

    }
}