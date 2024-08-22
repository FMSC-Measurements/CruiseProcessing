using CruiseDAL;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
using DiffPlex.DiffBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

using VolumeEqV2 = CruiseDAL.V2.Models.VolumeEquation;
using TreeV2 = CruiseDAL.V2.Models.Tree;

namespace CruiseProcessing.Test
{
    public class R9VolEquation_Test : DiTestBase
    {
        public R9VolEquation_Test(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void OnFinished()
        {
            bool calcTopwood = true;

            var init = new DatabaseInitializer_V2(speciesCodes: new[] {"101", "102", "103"})
            {
                Region = "09",
            };

            using var v2Db = init.CreateDatabase();
            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(v2Db, mockDlLogger);
            DataContext.DataLayer = dataLayer;

            var trees = new[]
           {
                new TreeV2{ TreeNumber = 1, Stratum_CN = 1, SampleGroup_CN = 1, TreeDefaultValue_CN = 1, Species = "101" },
                new TreeV2{ TreeNumber = 2, Stratum_CN = 1, SampleGroup_CN = 1, TreeDefaultValue_CN = 2, Species = "102" },
                new TreeV2{ TreeNumber = 3, Stratum_CN = 1, SampleGroup_CN = 1, TreeDefaultValue_CN = 3, Species = "103" },
                new TreeV2{ TreeNumber = 43, Stratum_CN = 1, SampleGroup_CN = 1, TreeDefaultValue_CN = 3, Species = "abc" }, // non-numeric species code will generate error message
            };
            foreach (var t in trees)
            {
                v2Db.Insert(t);
            }

            v2Db.From<VolumeEqV2>().Query().Count().Should().Be(0);

            var form = new R9VolEquation(DataContext.DataLayer, MockDialogService, MockServiceProvider);
            //MockDialogService.Received(1).ShowError(It.IsAny<string>()); form.setupDialog();

            if (calcTopwood)
            {
                foreach (var i in Enumerable.Range(0, form.topwoodFlags.Count))
                {
                    form.topwoodFlags[i] = "1";
                }
            }

            form.Finish(false, true); // update biomass displays a dialog so I'm not going to test update biomass here

            var valEqsAgain = v2Db.From<VolumeEqV2>().Query().ToArray();
            valEqsAgain.Should().HaveCount(3);

            // verify error message from non-numeric species code
            MockDialogService.Received().ShowError(Arg.Any<string>());
        }

        //[Fact]
        //public void Issue_Missing_FSCodes(string testFileName, string expectedOutputFileName, string expectedFailingReports = "")
        //{
        //    var fileName = "Issues\\R9DavidMissingSp\\000001 Leslie Timber Sale - TS.crz3", "Issues\\R9DavidMissingSp\\000001 Leslie Timber Sale - TS.041523.out";

        //    var filePath = GetTestFile(fileName);

        //    using var v3db = new CruiseDatastore_V3(filePath);
        //    var cruiseID = v3db.From<CruiseDAL.V3.Models.Cruise>().Query().Single().CruiseID;

        //    var v2Path = GetTempFilePath(".process", Path.GetFileNameWithoutExtension(testFileName) + ".ProcessAndVerityOutput_V3");
        //    using var dal = new DAL(v2Path, true);

        //    var migrator = new DownMigrator();
        //    migrator.MigrateFromV3ToV2(cruiseID, v3db, dal);

        //    var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
        //    var dataLayer = new CpDataLayer(dal, mockDlLogger);

        //    var mockDialogService = new Mock<IDialogService>();
        //    var mockLogger = new Mock<ILogger<ProcessStatus>>();
        //    var mockServiceProvider = GetServiceProviderMock(dataLayer);
        //    var processStatus = new ProcessStatus(dataLayer, mockDialogService.Object, mockLogger.Object, mockServiceProvider.Object);

        //    if (dataLayer.getRegion() == "09"
        //        && dataLayer.getValueEquations().Count == 0)
        //    {
        //        var r9VolEq = new R9VolEquation(dataLayer, mockServiceProvider.Object);
        //        r9VolEq.setupDialog();
        //        foreach (var i in Enumerable.Range(0, r9VolEq.topwoodFlags.Count))
        //        {
        //            r9VolEq.topwoodFlags[i] = "1";
        //        }
        //        r9VolEq.onFinished(null, null);
        //    }

        //    var result = processStatus.DoPreProcessChecks();
        //    if (!result)
        //    { throw new Exception("Skip"); }


        //    var mockProgress = new Mock<IProgress<string>>();
        //    processStatus.ProcessCore(mockProgress.Object);

        //    var ctf = new CreateTextFile(dataLayer);

        //    var stringWriter = new StringWriter();

        //    var reports = dataLayer.GetSelectedReports();
        //    var headerData = dataLayer.GetReportHeaderData();
        //    _ = ctf.CreateOutFile(reports, headerData, stringWriter, out var failedReports, out var hasWarnings);

        //    if (expectedFailingReports.Any() || expectedFailingReports != string.Empty)
        //    {
        //        var expectedFailReportsArray = expectedFailingReports.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        failedReports.Should().Contain(expectedFailReportsArray);
        //    }
        //    //hasWarnings.Should().BeFalse();


        //    var expectedOutputPath = GetTestFile(expectedOutputFileName);
        //    var expectedOutput = File.OpenText(expectedOutputPath).ReadToEnd();

        //    var resultOutput = stringWriter.ToString();
        //    var diff = InlineDiffBuilder.Diff(expectedOutput, resultOutput);

        //    var changedLines = diff.Lines
        //        .Where((x) =>
        //        {
        //            return x.Type != DiffPlex.DiffBuilder.Model.ChangeType.Unchanged
        //                && !x.Text.StartsWith("SALENAME:")
        //                && !x.Text.StartsWith("RUN DATE")
        //                && !x.Text.StartsWith("USDA FOREST SERVICE")
        //                && !x.Text.StartsWith("WASHINGTON")
        //                && !x.Text.StartsWith("FILENAME:");
        //        })
        //        .ToArray();

        //    foreach (var line in changedLines)
        //    {
        //        Output.WriteLine(line.Position.ToString().PadLeft(5) + ":" + line.Text);
        //    }

        //    changedLines.Should().HaveCount(0);



        //}
    }
}
