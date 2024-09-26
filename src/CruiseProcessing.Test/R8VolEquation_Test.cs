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
            foreach (var t in trees)
            {
                v2Db.Insert(t);
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
        [InlineData("01", "01", "4")] // test for forest that does have geo code
        public void GetR8ForestGeoCode(string forest, string district, string expectedGeoCode)
        {
            var result = R8VolEquation.GetR8ForestVolEqCodes(forest, district);

            if (expectedGeoCode == null)
            {
                result.Should().BeNull();
            }
            else
            {
                result.SubRegionCode.Should().Be(expectedGeoCode);
            }
        }

        [Fact]
        public void GetR8ForestGeoCode_CompareOld()
        {
            foreach (var num in Enumerable.Range(1, 100))
            {
                var forest = num.ToString().PadLeft(2, '0');
                foreach (var distNum in Enumerable.Range(1, 100))
                {
                    var district = distNum.ToString().PadLeft(2, '0');

                    Output.WriteLine($"forest: {forest}, district: {district}");
                    var expected = OldR8GeoCodeLookup.GetGeoCode(forest, district);
                    var result = R8VolEquation.GetR8ForestVolEqCodes(forest, district);

                    if (expected == null)
                    {
                        result.Should().BeNull();
                        Output.WriteLine("result: null");
                    }
                    else
                    {
                        result.SubRegionCode.Should().Be(expected);
                        Output.WriteLine($"result: {expected}");
                    }
                }
            }
        }


        public class OldR8GeoCodeLookup
        {
            static readonly string[,] forestDefaultList = new string[12, 3] {{"02","3","10"},
                                                        {"03","3","01"},
                                                        {"04","3","01"},
                                                        {"05","1","03"},
                                                        {"06","5","02"},
                                                        {"08","3","11"},
                                                        {"09","6","31"},
                                                        {"10","6","04"},
                                                        {"11","3","01"},
                                                        {"12","2","24"},
                                                        {"60","3","01"},
                                                        {"36","1","25"}};
            static readonly string[,] forestDistrictList = new string[33, 4] {{"01","01","4","13"},
                                                         {"01","03","1","15"},
                                                         {"01","04","4","16"},
                                                         {"01","05","4","14"},
                                                         {"01","06","4","14"},
                                                         {"01","07","4","17"},
                                                         {"03","08","2","06"},
                                                         {"06","06","5","09"},
                                                         {"07","01","5","19"},
                                                         {"07","02","5","20"},
                                                         {"07","04","5","21"},
                                                         {"07","05","5","22"},
                                                         {"07","06","7","18"},
                                                         {"07","07","4","23"},
                                                         {"07","17","4","23"},
                                                         {"08","11","3","12"},
                                                         {"08","12","3","12"},
                                                         {"08","13","3","12"},
                                                         {"08","14","3","12"},
                                                         {"08","15","3","12"},
                                                         {"08","16","3","12"},
                                                         {"09","01","6","30"},
                                                         {"09","06","6","30"},
                                                         {"09","12","6","32"},
                                                         {"10","07","7","05"},
                                                         {"11","03","1","07"},
                                                         {"11","10","2","08"},
                                                         {"12","02","3","01"},
                                                         {"12","05","1","25"},
                                                         {"13","01","5","26"},
                                                         {"13","03","5","27"},
                                                         {"13","04","5","29"},
                                                         {"13","07","5","28"}};

            public static string GetGeoCode(string forest, string district)
            {
                string currGeoCode = null;
                string currGrpCode = null;

                //  Look up geo code and group code for this forest and district (if any)
                //  First look in defaults
                for (int k = 0; k < 12; k++)
                {
                    if (forest == forestDefaultList[k, 0])
                    {
                        currGeoCode = forestDefaultList[k, 1];
                        currGrpCode = forestDefaultList[k, 2];
                    }   //  endif
                }   //  end for k loop

                //  Check for an override on district
                for (int k = 0; k < 33; k++)
                {
                    if (forest == forestDistrictList[k, 0] && district == forestDistrictList[k, 1])
                    {
                        currGeoCode = forestDistrictList[k, 2];
                        currGrpCode = forestDistrictList[k, 3];
                    }   //  endif
                }   //  end for k loop

                return currGeoCode;
            }
        }
    }
}
