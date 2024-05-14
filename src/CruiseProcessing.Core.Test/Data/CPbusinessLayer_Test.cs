using CruiseDAL;
using CruiseDAL.DataObjects;
using FluentAssertions;
using NSubstitute.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

using Reports = CruiseDAL.V3.Models.Reports;

namespace CruiseProcessing.Test.Data
{
    public class CPbusinessLayer_Test : TestBase
    {
        public CPbusinessLayer_Test(ITestOutputHelper output) : base(output)
        {
        }



        [Fact]
        public void SaveReports()
        {
            var init = new DatabaseInitializer_V2();

            var v2db = init.CreateDatabase();

            var dataLayer = new CPbusinessLayer(v2db);

            dataLayer.GetReports().Should().BeEmpty();

            var reports = new[]
            {
                new ReportsDO {ReportID = "something"},
                new ReportsDO {ReportID = "somethingElse"},

            }.ToList();

            dataLayer.SaveReports(reports);

            var reportsAgain = dataLayer.GetReports();

            reportsAgain.Should().BeEquivalentTo(reports);
        }

        [Fact]
        public void SaveReports_WithV3()
        {
            var v2Path = GetTempFilePath("SaveReports_WithV3.cruise");
            var v3Path = GetTempFilePath("SaveReports_WithV3.crz3");

            var initV3 = new DatabaseInitializer(false);
            var v3db = initV3.CreateDatabaseFile(v3Path);

            var v2db = new CruiseDAL.DAL(v2Path, true);
            var migrator = new DownMigrator();
            migrator.MigrateFromV3ToV2(initV3.CruiseID, v3db, v2db);

            var dataLayer = new CPbusinessLayer(v2db, v3db, initV3.CruiseID);

            dataLayer.GetReports().Should().BeEmpty();

            var reports = new[]
            {
                new ReportsDO {ReportID = "something"},
                new ReportsDO {ReportID = "somethingElse"},

            }.ToList();

            dataLayer.SaveReports(reports);

            var reportsAgain = dataLayer.GetReports();

            reportsAgain.Should().BeEquivalentTo(reports);

            v3db.From<Reports>().Count().Should().Be(reports.Count);
        }
    }
}
