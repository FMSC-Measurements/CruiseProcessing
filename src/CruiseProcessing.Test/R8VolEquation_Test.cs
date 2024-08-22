using CruiseProcessing.Data;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

using VolumeEqV2 = CruiseDAL.V2.Models.VolumeEquation;
using TreeV2 = CruiseDAL.V2.Models.Tree;
using TreeV3 = CruiseDAL.V3.Models.Tree;
using CruiseDAL;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CruiseProcessing.Test
{
    public class R8VolEquation_Test : DiTestBase
    {
        public R8VolEquation_Test(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Finish()
        {
            var init = new DatabaseInitializer_V2()
            {
                Region = "08",
            };
            var v2Db = init.CreateDatabase();
            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(v2Db, mockDlLogger);
            DataContext.DataLayer = dataLayer;

            
            var trees = new[]
            {
                new TreeV2{ TreeNumber = 1, Stratum_CN = 1, SampleGroup_CN = 1, TreeDefaultValue_CN = 1, Species = "sp1" },
                new TreeV2{ TreeNumber = 2, Stratum_CN = 1, SampleGroup_CN = 1, TreeDefaultValue_CN = 2, Species = "sp2" },
                new TreeV2{ TreeNumber = 3, Stratum_CN = 1, SampleGroup_CN = 1, TreeDefaultValue_CN = 3, Species = "sp3" },
            };
            foreach( var t in trees )
            {
                v2Db.Insert( t );
            }

            v2Db.From<VolumeEqV2>().Query().Count().Should().Be(0);

            var form = new R8VolEquation(DataContext.DataLayer, MockServiceProvider, MockDialogService);

            form.Finish(false, true);

            v2Db.From<VolumeEqV2>().Query().Count().Should().Be(3);
        }


        [Fact]
        public void Finish_V3()
        {
            var v2Path = GetTempFilePath("R8VolEq_Finish_WithV3.cruise");
            var v3Path = GetTempFilePath("R8VolEq_Finish_WithV3.crz3");

            var initV3 = new DatabaseInitializer()
            {
                Region = "08",
            };
            var v3db = initV3.CreateDatabaseFile(v3Path);

            var stratumCode = "st1";
            var sgCode = "sg1";
            var unitCode = initV3.Units.First();
            var trees = new[]
            {
                new TreeV3{ TreeNumber = 1, CuttingUnitCode = unitCode, StratumCode = stratumCode, SampleGroupCode = sgCode,  SpeciesCode = "sp1" },
                new TreeV3{ TreeNumber = 2, CuttingUnitCode = unitCode, StratumCode = stratumCode, SampleGroupCode = sgCode,  SpeciesCode = "sp2" },
                new TreeV3{ TreeNumber = 3, CuttingUnitCode = unitCode, StratumCode = stratumCode, SampleGroupCode = sgCode,  SpeciesCode = "sp3" },
            };
            foreach (var tree in trees)
            {
                initV3.AddTreeRecord(v3db, tree);
            }
            

            var v2db = new CruiseDAL.DAL(v2Path, true);
            var migrator = new DownMigrator();
            migrator.MigrateFromV3ToV2(initV3.CruiseID, v3db, v2db);

            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(v2db, v3db, initV3.CruiseID, mockDlLogger);
            DataContext.DataLayer = dataLayer;




            v2db.From<VolumeEqV2>().Query().Count().Should().Be(0);
            v2db.From<TreeV2>().Query().Count().Should().Be(3);

            var form = new R8VolEquation(DataContext.DataLayer, MockServiceProvider, MockDialogService);

            form.Finish(false, true);

            v2db.From<VolumeEqV2>().Query().Count().Should().Be(3);
            v3db.From<CruiseDAL.V3.Models.VolumeEquation>().Query().Count().Should().Be(3);
        }


        [Theory]
        [InlineData("03", "00", "3")] // test for district that doesn't have separate geo code
        [InlineData("03", "08", "2")] // test for district that does have separate geo code
        [InlineData("99", "09", null)] // test for forest that doesn't have geo code
        public void GetR8ForestGeoCode(string forest, string district, string expectedGeoCode)
        {
            var result = R8VolEquation.GetR8ForestGeoCode(forest, district);

            if(expectedGeoCode == null)
            {
                result.Should().BeNull();
            }
            else
            {
                result.GeoCode.Should().Be(expectedGeoCode);
            }
        }
    }
}
