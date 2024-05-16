using CruiseDAL;
using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
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
    public class CpDataLayer_Test : TestBase
    {
        public CpDataLayer_Test(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetStratumCodesByUnit()
        {
            var init = new DatabaseInitializer_V2();

            var v2db = init.CreateDatabase();

            var mockLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(v2db, mockLogger);

            var units = dataLayer.getCuttingUnits();

            var unitCode = units.First().Code;
            var stCodes = dataLayer.GetStratumCodesByUnit(unitCode);
            stCodes.Should().HaveSameCount(init.UnitStrata.Where(x => x.UnitCode == unitCode));
        }

        [Fact]
        public void SaveReports()
        {
            var init = new DatabaseInitializer_V2();

            
            var v2db = init.CreateDatabase();
            var mockLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(v2db, mockLogger);

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

            var mockLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(v2db, v3db, initV3.CruiseID, mockLogger);

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

        [Fact]
        public void SaveBiomassEquations_V3()
        {
            var v2Path = GetTempFilePath("SaveReports_WithV3.cruise");
            var v3Path = GetTempFilePath("SaveReports_WithV3.crz3");

            var initV3 = new DatabaseInitializer(false);
            var v3db = initV3.CreateDatabaseFile(v3Path);

            var v2db = new CruiseDAL.DAL(v2Path, true);
            var migrator = new DownMigrator();
            migrator.MigrateFromV3ToV2(initV3.CruiseID, v3db, v2db);

            var mockLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(v2db, v3db, initV3.CruiseID, mockLogger);

            var bioEqs = new[]
            {
                new BiomassEquationDO{ Species = "sp1", Product = "01", Component = "something", LiveDead = "L" },
                new BiomassEquationDO{ Species = "sp2", Product = "01", Component = "something", LiveDead = "L" },
                new BiomassEquationDO{ Species = "sp3", Product = "01", Component = "something", LiveDead = "L" },
            };

            dataLayer.SaveBiomassEquations(bioEqs.ToList());

            mockLogger.DidNotReceive().Log(LogLevel.Error, default);

            v3db.From<CruiseDAL.V3.Models.BiomassEquation>().Count().Should().Be(bioEqs.Length);
        }
    }
}
