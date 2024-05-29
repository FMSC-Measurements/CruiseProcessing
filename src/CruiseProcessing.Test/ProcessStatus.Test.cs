using CruiseDAL;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
using DiffPlex.DiffBuilder;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using ZedGraph;

namespace CruiseProcessing.Test
{
    public class ProcessStatus_Test : TestBase
    {

        public ProcessStatus_Test(ITestOutputHelper output) : base(output)
        {
            
        }

        private Mock<IServiceProvider> GetServiceProviderMock(CpDataLayer datalayer)
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(It.Is<Type>(x => x == typeof(ICalculateTreeValues))))
                .Returns(() => new CalculateTreeValues2(datalayer));

            return mockServiceProvider;
        }

        [Theory]
        //[InlineData("OgTest\\BLM\\Hammer Away.cruise")]
        //[InlineData("OgTest\\BLM\\Long Nine.cruise")]
        [InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test_V3.process")]
        [InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise")]
        [InlineData("OgTest\\Region4\\R4_McDougal.cruise")]
        [InlineData("OgTest\\Region5\\R5.cruise")]
        [InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise")]
        [InlineData("OgTest\\Region9\\R9.cruise")]
        [InlineData("OgTest\\Region10\\R10.cruise")]

        [InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise")]
        [InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        [InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise")]
        [InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        [InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        [InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise")]

        [InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        [InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        [InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        [InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]
        public void DoPreProcessChecks(string testFileName)
        {
            var filePath = GetTestFile(testFileName);
            using var dal = new DAL(filePath);

            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(dal, mockDlLogger);

            var mockDialogService = new Mock<IDialogService>();
            var mockLogger = new Mock<ILogger<ProcessStatus>>();
            var mockServiceProvider = GetServiceProviderMock(dataLayer);

            var processStatus = new ProcessStatus(dataLayer, mockDialogService.Object, mockLogger.Object, mockServiceProvider.Object);

            var result = processStatus.DoPreProcessChecks();
            result.Should().BeTrue();

        }

        [Theory]
        //[InlineData("OgTest\\BLM\\Hammer Away.cruise")]
        //[InlineData("OgTest\\BLM\\Long Nine.cruise")]
        [InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test_V3.process")]
        //[InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        //[InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise")]
        [InlineData("OgTest\\Region4\\R4_McDougal.cruise")]
        [InlineData("OgTest\\Region5\\R5.cruise")]
        //[InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise")]
        [InlineData("OgTest\\Region9\\R9.cruise")]
        [InlineData("OgTest\\Region10\\R10.cruise")]

        [InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        [InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise")]
        //[InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        //[InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        [InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise")]

        [InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        //[InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        //[InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process")]

        //[InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        [InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        [InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]
        public void ProcessCore(string testFileName)
        {
            var filePath = GetTestFile(testFileName);
            using var dal = new DAL(filePath);

            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(dal, mockDlLogger);

            var mockDialogService = new Mock<IDialogService>();
            var mockLogger = new Mock<ILogger<ProcessStatus>>();

            var mockServiceProvider = GetServiceProviderMock(dataLayer);

            var processStatus = new ProcessStatus(dataLayer, mockDialogService.Object, mockLogger.Object, mockServiceProvider.Object);

            var result = processStatus.DoPreProcessChecks();
            if(!result)
            { throw new Exception("Skip"); }

            dal.TransactionDepth.Should().Be(0, "Before Process");
            var mockProgress = new Mock<IProgress<string>>();
            processStatus.ProcessCore(mockProgress.Object);

            dal.TransactionDepth.Should().Be(0, "After Process");

        }



        [Theory]
        //[InlineData("OgTest\\BLM\\Hammer Away.cruise")]
        //[InlineData("OgTest\\BLM\\Long Nine.cruise")]
        [InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise", "OgTest\\Region1\\R1_FrenchGulch.out")]
        [InlineData("OgTest\\Region2\\R2_Test.cruise", "OgTest\\Region2\\R2_Test.out")]
        [InlineData("OgTest\\Region2\\R2_Test_V3.process", "OgTest\\Region2\\R2_Test_V3.out")]
        //[InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        //[InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise", "OgTest\\Region3\\R3_PNT_FIXCNT.out")]
        [InlineData("OgTest\\Region4\\R4_McDougal.cruise", "OgTest\\Region4\\R4_McDougal.out")]
        [InlineData("OgTest\\Region5\\R5.cruise", "OgTest\\Region5\\R5.out")]
        //[InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise", "OgTest\\Region8\\R8.out")]
        [InlineData("OgTest\\Region9\\R9.cruise", "OgTest\\Region9\\R9.out")]
        [InlineData("OgTest\\Region10\\R10.cruise", "OgTest\\Region10\\R10.out", "R008,R009")]

        [InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise", "Version3Testing\\3P\\87654 test 3P TS.out")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        [InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process", "Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.out")]

        [InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise", "Version3Testing\\FIX\\20301 Cold Springs Recon.out")]
        //[InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        //[InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        [InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise", "Version3Testing\\PCM\\27504_Spruce East_TS.out")]

        [InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise", "Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        //[InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        //[InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process", "Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.out")]

        //[InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        //[InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")] no out file
        //[InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")] no out file

        
        public void ProcessAndVerityOutput(string testFileName, string expectedOutputFileName, string expectedFailingReports = "")
        {
            var filePath = GetTestFile(testFileName);
            using var dal = new DAL(filePath);

            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(dal, mockDlLogger);

            var mockDialogService = new Mock<IDialogService>();
            var mockLogger = new Mock<ILogger<ProcessStatus>>();

            var mockServiceProvider = GetServiceProviderMock(dataLayer);

            var processStatus = new ProcessStatus(dataLayer, mockDialogService.Object, mockLogger.Object, mockServiceProvider.Object);

            var result = processStatus.DoPreProcessChecks();
            if (!result)
            { throw new Exception("Skip"); }

            
            var mockProgress = new Mock<IProgress<string>>();
            processStatus.ProcessCore(mockProgress.Object);

            var ctf = new CreateTextFile(dataLayer);

            var stringWriter = new StringWriter();

            var reports = dataLayer.GetSelectedReports();
            var headerData = dataLayer.GetReportHeaderData();
            _ = ctf.CreateOutFile(reports, headerData, stringWriter, out var failedReports, out var hasWarnings);

            if (expectedFailingReports.Any() || expectedFailingReports != string.Empty)
            {
                var expectedFailReportsArray = expectedFailingReports.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                failedReports.Should().Contain(expectedFailReportsArray);
            }
            //hasWarnings.Should().BeFalse();


            var expectedOutputPath = GetTestFile(expectedOutputFileName);
            var expectedOutput = File.OpenText(expectedOutputPath).ReadToEnd();

            var diff = InlineDiffBuilder.Diff(expectedOutput, stringWriter.ToString());

            var changedLines = diff.Lines
                .Where((x) => 
                {
                        return x.Type != DiffPlex.DiffBuilder.Model.ChangeType.Unchanged
                            && !x.Text.StartsWith("SALENAME:")
                            && !x.Text.StartsWith("RUN DATE")
                            && !x.Text.StartsWith("USDA FOREST SERVICE")
                            && !x.Text.StartsWith("WASHINGTON")
                            && !x.Text.StartsWith("FILENAME:");
                })
                .ToArray();

            foreach(var line in changedLines)
            {
                Output.WriteLine(line.Position.ToString().PadLeft(5) + ":" + line.Text);
            }

            changedLines.Should().HaveCount(0);



        }

        [Theory]
        [InlineData("Issues\\20383_Jiffy Stewardship_TS.crz3", "Issues\\20383_Jiffy Stewardship_TS.04.15.2023.out")]
        public void ProcessAndVerityOutput_V3(string testFileName, string expectedOutputFileName, string expectedFailingReports = "")
        {
            var filePath = GetTestFile(testFileName);

            using var v3db = new CruiseDatastore_V3(filePath);
            var cruiseID = v3db.From<CruiseDAL.V3.Models.Cruise>().Query().Single().CruiseID;

            var v2Path = GetTempFilePath(".process", Path.GetFileNameWithoutExtension(testFileName) + ".ProcessAndVerityOutput_V3");
            using var dal = new DAL(v2Path, true);

            var migrator = new DownMigrator();
            migrator.MigrateFromV3ToV2(cruiseID, v3db, dal);

            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(dal, mockDlLogger);

            var mockDialogService = new Mock<IDialogService>();
            var mockLogger = new Mock<ILogger<ProcessStatus>>();
            var mockServiceProvider = GetServiceProviderMock(dataLayer);
            var processStatus = new ProcessStatus(dataLayer, mockDialogService.Object, mockLogger.Object, mockServiceProvider.Object);

            if (dataLayer.getRegion() == "09"
                && dataLayer.getValueEquations().Count == 0)
            {
                var r9VolEq = new R9VolEquation(dataLayer, mockServiceProvider.Object);
                r9VolEq.onFinished(null, null);
            }

            var result = processStatus.DoPreProcessChecks();
            if (!result)
            { throw new Exception("Skip"); }


            var mockProgress = new Mock<IProgress<string>>();
            processStatus.ProcessCore(mockProgress.Object);

            var ctf = new CreateTextFile(dataLayer);

            var stringWriter = new StringWriter();

            var reports = dataLayer.GetSelectedReports();
            var headerData = dataLayer.GetReportHeaderData();
            _ = ctf.CreateOutFile(reports, headerData, stringWriter, out var failedReports, out var hasWarnings);

            if (expectedFailingReports.Any() || expectedFailingReports != string.Empty)
            {
                var expectedFailReportsArray = expectedFailingReports.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                failedReports.Should().Contain(expectedFailReportsArray);
            }
            //hasWarnings.Should().BeFalse();


            var expectedOutputPath = GetTestFile(expectedOutputFileName);
            var expectedOutput = File.OpenText(expectedOutputPath).ReadToEnd();

            var resultOutput = stringWriter.ToString();
            var diff = InlineDiffBuilder.Diff(expectedOutput, resultOutput);

            var changedLines = diff.Lines
                .Where((x) =>
                {
                    return x.Type != DiffPlex.DiffBuilder.Model.ChangeType.Unchanged
                        && !x.Text.StartsWith("SALENAME:")
                        && !x.Text.StartsWith("RUN DATE")
                        && !x.Text.StartsWith("USDA FOREST SERVICE")
                        && !x.Text.StartsWith("WASHINGTON")
                        && !x.Text.StartsWith("FILENAME:");
                })
                .ToArray();

            foreach (var line in changedLines)
            {
                Output.WriteLine(line.Position.ToString().PadLeft(5) + ":" + line.Text);
            }

            changedLines.Should().HaveCount(0);



        }
    }
}
