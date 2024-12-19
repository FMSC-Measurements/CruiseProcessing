using CruiseDAL;
using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.Output;
using CruiseProcessing.Processing;
using CruiseProcessing.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Xunit.Abstractions;

namespace CruiseProcessing.Test.Output
{
    public class OutputWeight_Test : TestBase
    {
        public OutputWeight_Test(ITestOutputHelper output) : base(output)
        {
        }

        //[Theory]
        //[InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        //[InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", "OgTest\\Region3\\R3_PCM_FIXCNT.out")]
        //public void OutputWeightReports_WT1(string cruiseFileName, string outFileName)
        //{             //  test for WT1 report
        //    var filePath = GetTestFile(cruiseFileName);

        //    using var db = new DAL(filePath);

        //    var dataLayer = new CpDataLayer(db, Substitute.For<ILogger<CpDataLayer>>(), biomassOptions: null);

        //    var reportID = "WT1";
        //    var headerData = dataLayer.GetReportHeaderData();
        //    var outputWeight = new OutputWeight(dataLayer, VolumeLibraryInterop.Default, headerData, reportID);

        //    var writer = new System.IO.StringWriter();
        //    int pageNum = 0;
        //    outputWeight.OutputWeightReports(writer, ref pageNum);

        //    var output = writer.ToString();
        //    //Output.WriteLine(output);

        //    if (outFileName != null)
        //    {
        //        ExtractReport(outFileName, reportID);
        //    }
        //}

        [Theory]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process.cruise", "Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process.out")]
        [InlineData("OgTest\\Region3\\R3_FCM_100.cruise", "OgTest\\Region3\\R3_FCM_100.out")]
        [InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", "OgTest\\Region3\\R3_PCM_FIXCNT.out")]
        public void OutputWeightReports_WT1(string cruiseFileName, string outFileName)
        {
            CompareWeightReportCore(cruiseFileName, outFileName, "WT1", reprocess: false);
        }

        [Theory]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process.cruise", "Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process.out")]
        [InlineData("OgTest\\Region3\\R3_FCM_100.cruise", "OgTest\\Region3\\R3_FCM_100.out")]
        [InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", "OgTest\\Region3\\R3_PCM_FIXCNT.out")]
        public void ProcessAndOutputWeightReports_WT1(string cruiseFileName, string outFileName)
        {
            CompareWeightReportCore(cruiseFileName, outFileName, "WT1", reprocess: true);
        }

        [Theory]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.crz3", null)]
        [InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", null)]
        public void OutputWeightReports_WT2(string cruiseFileName, string outFileName)
        {
            CompareWeightReportCore(cruiseFileName, outFileName, "WT2", reprocess: false);
        }

        [Theory]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.crz3", null)]
        [InlineData("OgTest\\Region3\\R3_FCM_100.cruise", null)]
        [InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", null)]
        public void ProcessAndOutputWeightReports_WT2(string cruiseFileName, string outFileName)
        {
            CompareWeightReportCore(cruiseFileName, outFileName, "WT2", reprocess: true);
        }

        [Theory]
        [InlineData("OgTest\\Region3\\R3_FCM_100.cruise", null)]
        [InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", null)]
        public void OutputWeightReports_WT3(string cruiseFileName, string outFileName)
        {
            CompareWeightReportCore(cruiseFileName, outFileName, "WT3", reprocess: false);
        }

        [Theory]
        [InlineData("OgTest\\Region3\\R3_FCM_100.cruise", null)]
        [InlineData("Version3Testing\\STR\\98765 test STR TS.cruise", null)]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.crz3", null)]
        [InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", null)]
        public void ProcessAndOutputWeightReports_WT3(string cruiseFileName, string outFileName)
        {
            CompareWeightReportCore(cruiseFileName, outFileName, "WT3", reprocess: true);
        }

        [Theory]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.crz3", null)]
        [InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", "OgTest\\Region3\\R3_PCM_FIXCNT.out")]
        public void OutputWeightReports_WT4(string cruiseFileName, string outFileName)
        {
            CompareWeightReportCore(cruiseFileName, outFileName, "WT4", reprocess: false);
        }

        [Theory]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process.cruise", "Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process.out")]
        [InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", "OgTest\\Region3\\R3_PCM_FIXCNT.out")]
        public void ProcessAndOutputWeightReports_WT4(string cruiseFileName, string outFileName)
        {             //  test for WT1 report
            CompareWeightReportCore(cruiseFileName, outFileName, "WT4", reprocess: true);
        }

        [Theory]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process.cruise", "Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process.out")]
        [InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", "OgTest\\Region3\\R3_PCM_FIXCNT.out")]
        public void ProcessAndOutputWeightReports_WT5(string cruiseFileName, string outFileName)
        {
            CompareWeightReportCore(cruiseFileName, outFileName, "WT5", reprocess: true);
        }

        [Theory]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process.cruise", "Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process.out")]
        [InlineData("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise", null)]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise", "OgTest\\Region3\\R3_PCM_FIXCNT.out")]
        public void OutputWeightReports_WT5(string cruiseFileName, string outFileName)
        {
            CompareWeightReportCore(cruiseFileName, outFileName, "WT5", reprocess: false);
        }

        protected void CompareWeightReportCore(string cruiseFileName, string outFileName, string reportID, bool reprocess, bool updateBio = false)
        {
            var filePath = GetTestFile(cruiseFileName);

            using CpDataLayer dataLayer = GetCpDataLayer(filePath);

            // if comparing to original out file we should check to see if the report is selected
            // otherwise we can still try to generate the report
            if (outFileName != null)
            {
                dataLayer.GetSelectedReports().Select(x => x.ReportID).Should().Contain(reportID);
            }

            var volumeLibrary = new VolumeLibrary_20241118();

            //if(updateBio)
            //{
            //    BiomassHelpers.UpdateBiomass(dataLayer);
            //}

            if (reprocess)
            {
                BiomassHelpers.UpdateBiomass(dataLayer);

                List<ErrorLogDO> fscList = dataLayer.getErrorMessages("E", "FScruiser");
                var errors = EditChecks.CheckErrors(dataLayer);

                if (fscList.Any())
                { throw new Exception("Skip - Cruise FSC errors"); }
                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        Output.WriteLine($"Table: {error.TableName} Msg:{ErrorReport.GetErrorMessage(error.Message)}");
                    }

                    throw new Exception("Skip - Cruise errors");
                }

                var processor = new CruiseProcessor(dataLayer, Substitute.For<IDialogService>(), CreateLogger<CruiseProcessor>());
                processor.ProcessCruise(null);
            }

            var headerData = dataLayer.GetReportHeaderData();
            IReportGenerator outputWeight = reportID switch
            {
                "WT1" => new Wt1ReportGenerator(dataLayer, volumeLibrary, CreateLogger<Wt1ReportGenerator>()),
                "WT2" => new Wt2ReportGenerator(dataLayer, volumeLibrary, CreateLogger<Wt2ReportGenerator>()),
                "WT3" => new Wt3ReportGenerator(dataLayer, volumeLibrary, CreateLogger<Wt3ReportGenerator>()),
                "WT4" => new Wt4ReportGenerator(dataLayer, CreateLogger<Wt4ReportGenerator>()),
                "WT5" => new Wt5ReportGenerator(dataLayer, CreateLogger<Wt5ReportGenerator>()),
                _ => throw new NotImplementedException(),
            };


            var writer = new System.IO.StringWriter();
            int pageNum = 0;
            pageNum = outputWeight.GenerateReport(writer, headerData, pageNum);

            var output = writer.ToString();
            output.Should().NotBeNullOrEmpty();
            if (outFileName != null)
            {
                CompareOutput(outFileName, reportID, output);
            }
            else
            {
                Output.WriteLine(output);
            }
        }

        private CpDataLayer GetCpDataLayer(string filePath)
        {
            var fileExtention = System.IO.Path.GetExtension(filePath);
            if (fileExtention == ".crz3")
            {
                var migrator = new DownMigrator();
                var v3Db = new CruiseDatastore_V3(filePath);
                var v2Db = new DAL();
                var cruiseID = v3Db.QueryScalar<string>("SELECT CruiseID FROM Cruise").First();
                migrator.MigrateFromV3ToV2(cruiseID, v3Db, v2Db);
                return new CpDataLayer(v2Db, CreateLogger<CpDataLayer>(), biomassOptions: null);
            }
            else
            {
                var db = new DAL(filePath);
                return new CpDataLayer(db, CreateLogger<CpDataLayer>(), biomassOptions: null);
            }
        }

        private void CompareOutput(string outFileName, string reportID, string output)
        {
            var originalReportPages = ExtractReportPagesFromFile(outFileName).Where(x => x.ReportID == reportID).ToArray();
            originalReportPages.Should().NotBeEmpty();

            var newReportPages = OutputParser.ExtractReportPages(output).ToArray();
            newReportPages.Should().NotBeEmpty();

            newReportPages.Length.Should().Be(originalReportPages.Length);

            foreach (var (orgRpt, newRpt) in originalReportPages.Zip(newReportPages, (orgRpt, newRpt) => (orgRpt, newRpt)))
            {

                orgRpt.ReportID.Should().Be(newRpt.ReportID);
                //orgRpt.ReportTitle.Should().Be(newRpt.ReportTitle);
                //orgRpt.PageNumber.Should().Be(newRpt.PageNumber);
                //orgRpt.ReportSubtitle.Should().Be(newRpt.ReportSubtitle);

                var orgContent = orgRpt.ReportContent;
                var newContent = newRpt.ReportContent;

                Output.WriteLine(newContent);

                try
                {
                    // WT1 had a column added so we cant compare it now
                    if(reportID == "WT5")
                    {
                        orgContent = orgContent.Replace("ALL SPECUES", "ALL SPECIES");
                        orgContent = orgContent.Replace("NaN", "0.0");
                    }
                    if (reportID != "WT1")
                    {
                        Assert.Equal(orgContent, newContent, ignoreCase: true, ignoreWhiteSpaceDifferences: true);
                    }
                    else
                    {
                        Output.WriteLine("expected:" + orgContent);
                        Output.WriteLine("actual  :" + newContent);
                    }
                }
                catch (Exception e)
                {
                    Output.WriteLine("expected:" + orgContent);
                    Output.WriteLine("actual  :" + newContent);

                    throw;
                }

                //orgContent.Should().Be(newContent);
                //foreach (var r in replaceRegex)
                //{
                //    orgContent = r.Regex.Replace(orgContent, r.Replacement);
                //    newContent = r.Regex.Replace(newContent, r.Replacement);
                //}

            }


            //var stringReader = new System.IO.StringReader(output);

            //var lines = ExtractReport(outFileName, reportID).ToArray();
            ////lines.Should().NotBeEmpty();
            //if (lines.Length == 0)
            //{
            //    Output.WriteLine("report not found in original report");
            //}

            //foreach (var (line, i) in lines.Select((x, i) => (x, i)))
            //{
            //    //Output.WriteLine(line);
            //    var expected = line;
            //    var actual = stringReader.ReadLine();
            //    Output.WriteLine(actual);
            //    if (actual == "\f")
            //    {
            //        actual = stringReader.ReadLine();
            //        Output.WriteLine(actual);
            //    }

            //    if (ignoreRegex.Any(x => x.IsMatch(expected)))
            //    {
            //        continue;
            //    }

            //    foreach (var r in replaceRegex)
            //    {
            //        expected = r.Regex.Replace(expected, r.Replacement);
            //    }

            //    try
            //    {
            //        Assert.Equal(expected, actual, ignoreCase: true, ignoreWhiteSpaceDifferences: true);
            //    }
            //    catch (Exception e)
            //    {
            //        Output.WriteLine("expected:" + expected);
            //        Output.WriteLine("actual  :" + actual);

            //        throw;
            //    }

            //    //actual.Should().BeEquivalentTo(expected, because: $"line {i}");
            //}
        }

        protected IEnumerable<ReportPage> ExtractReportPagesFromFile(string fileName)
        {
            var allText = ReadAllText(fileName);
            return OutputParser.ExtractReportPages(allText);
        }

        protected IEnumerable<string> ExtractReportPagesFromFile(string fileName, string reportID)
        {
            var allText = ReadAllText(fileName);



            var reportPageRegex = new Regex("((?<reportID>" + reportID + "): (?<reportTitle>([\\w\\p{P}]+ )+)? *PAGE (?<pageNumber>\\d+)\\n)(?<reportSubtitle>(( ?[\\w\\(\\)\\d]+ ?)+\\n){0,2})(CRUISE#: (?<cruiseNumber>\\d+) +SALE#: (?<saleNumber>\\d+)\\n)(SALENAME: (?<saleName>\\S+) +VERSION: (?<cpVersion>\\d+\\.\\d+\\.\\d+)\\n)(RUN DATE & TIME: (?<runDateTime>\\d+\\/\\d+\\/\\d+\\s\\d+:\\d+:\\d+\\s\\w+) +VOLUME LIBRARY VERSION:\\s(?<volLibVersion>\\d+\\.\\d+\\.\\d+)\\n)\\n+(?<reportContent>([ \\d\\w\\t\\p{P}\\p{S}]*\\n)+)"
                , RegexOptions.Multiline);

            var matchs = reportPageRegex.Matches(allText);
            for (int i = 0; i < matchs.Count; i++)
            {
                var matchreportID = matchs[i].Groups["reportID"].Value;
                var reportTitle = matchs[i].Groups["reportTitle"].Value;
                var pageNumber = matchs[i].Groups["pageNumber"].Value;
                var reportSubtitle = matchs[i].Groups["reportSubtitle"].Value;
                var reportContent = matchs[i].Groups["reportContent"].Value;

                yield return matchs[i].Value;
            }
        }

        protected IEnumerable<string> ExtractReport(string fileName, string reportID)
        {
            var outFilePath = GetTestFile(fileName);
            var outTextReader = System.IO.File.OpenText(outFilePath);

            while (true)
            {
                var line = outTextReader.ReadLine();
                if (line == null) { break; }

                //detect new page
                if (line == "\f")
                {
                    line = outTextReader.ReadLine();
                    if (line == null) { break; }
                    //detect start of report
                    if (line.Length >= 3 && line.Substring(0, 3) == reportID)
                    {
                        while (true)
                        {
                            yield return line;

                            //read next line
                            line = outTextReader.ReadLine();
                            if (line == null) { break; }
                            // detect new page
                            if (line == "\f")
                            {
                                line = outTextReader.ReadLine();
                                // detect end of document or start of new report
                                if (line == null
                                    || (line.Length >= 3 && line.Substring(0, 3) != reportID))
                                { yield break; }
                            }
                        }
                    }
                }
            }


        }

        protected string ReadAllText(string fileName)
        {
            var filePath = GetTestFile(fileName);
            return System.IO.File.ReadAllText(filePath);
        }
    }
}