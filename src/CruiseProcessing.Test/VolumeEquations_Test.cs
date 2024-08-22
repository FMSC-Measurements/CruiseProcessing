using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

using TreeDefaultValue = CruiseDAL.V2.Models.TreeDefaultValue;

namespace CruiseProcessing.Test
{
    public class VolumeEquations_Test : DiTestBase
    {
        public VolumeEquations_Test(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(1, "01", "L", "sp1", "01", 101, 101)]
        [InlineData(1, "01", "L", "sp1", "01", 0, 999)] // if input fia code is invalid output fiacode should be 999
        public void MakeBiomassEquations(int region, string forest, string liveDead, string species, string prod, int fiaCode, int expectedFiaCode)
        {
            var volumeEquation = new VolumeEquationDO { Species = species, PrimaryProduct = prod };

            var result = VolumeEquations.MakeBiomassEquations(volumeEquation, region, forest, fiaCode, liveDead, 95.0f);

            result.Should().HaveCount(VolumeEquations.BIOMASS_COMPONENTS.Count);
            result.Should().OnlyContain(x => x.FIAcode == fiaCode);

            result.Where(x => x.Component == "PrimaryProd").Single().WeightFactorPrimary.Should().BeGreaterThan(0);
            result.Where(x => x.Component == "SecondaryProd").Single().WeightFactorSecondary.Should().BeGreaterThan(0);
        }


        [Fact]
        public void UpdateBiomassCruise()
        {
            var treeDefaults = new[]
{
                new TreeDefaultValue{ Species = "101", FIAcode = 101, PrimaryProduct = "01", LiveDead = "L" },
                new TreeDefaultValue{ Species = "101", FIAcode = 101, PrimaryProduct = "01", LiveDead = "D" },
                new TreeDefaultValue{ Species = "102", FIAcode = 102, PrimaryProduct = "01", LiveDead = "L" },
            };

            var init = new DatabaseInitializer_V2()
            {
                TreeDefaults = treeDefaults,
            };

            using var db = init.CreateDatabase();

            var trees = treeDefaults.Select((tdv, i) => MakeTree(i + 1, tdv)).ToArray();

            foreach(var tree in trees)
            {
                db.Insert(tree);
            }


            var dataLayer = new CpDataLayer(db, Substitute.For<ILogger<CpDataLayer>>(), true);

            var volumeEquations = new[]
            {
                new VolumeEquationDO{ CalcBiomass = 1, Species = "101", PrimaryProduct = "01", VolumeEquationNumber = "1234567890" },
                new VolumeEquationDO{ CalcBiomass = 1, Species = "102", PrimaryProduct = "01", VolumeEquationNumber = "1234567890" },
            };

            var PRs = new[]
            {
                new PercentRemoved{ bioSpecies = "101", bioProduct = "01", bioPCremoved = "95.0" },
                new PercentRemoved{ bioSpecies = "102", bioProduct = "01", bioPCremoved = "96.0" },
            };

            VolumeEquations.UpdateBiomassTemplate(dataLayer, volumeEquations, "01", "01", PRs);

            var biomassEqsAgain = db.From<CruiseDAL.V2.Models.BiomassEquation>().Query().ToArray();

            biomassEqsAgain.Should().HaveCountGreaterOrEqualTo(volumeEquations.Length * VolumeEquations.BIOMASS_COMPONENTS.Count);
            biomassEqsAgain.Should().HaveCountLessOrEqualTo(2 * volumeEquations.Length * VolumeEquations.BIOMASS_COMPONENTS.Count);

            biomassEqsAgain.Where(x => x.Species == "101" && x.Product == "01" && x.LiveDead == "L" && x.Component == "PrimaryProd").Should().HaveCount(1);
            biomassEqsAgain.Where(x => x.Species == "101" && x.Product == "01" && x.LiveDead == "D" && x.Component == "PrimaryProd").Should().HaveCount(1);


            TreeDO MakeTree(int treeNumber, TreeDefaultValue tdv)
            {
                return new TreeDO
                {
                    TreeNumber = treeNumber,
                    CuttingUnit_CN = 1,
                    Stratum_CN = 1,
                    SampleGroup_CN = 1,
                    Species = tdv.Species,
                    LiveDead = tdv.LiveDead,
                    TreeDefaultValue_CN = tdv.TreeDefaultValue_CN ?? throw new ArgumentException("expected tdv TreeDefaultValue_CN to not be null"),
                };
            }
        }

        [Fact]
        public void UpdateBiomassTemplate()
        {
            var treeDefaults = new[]
            {
                new TreeDefaultValue{ Species = "101", FIAcode = 101, PrimaryProduct = "01", LiveDead = "L" },
                new TreeDefaultValue{ Species = "102", FIAcode = 102, PrimaryProduct = "01", LiveDead = "L" },
            };

            var init = new DatabaseInitializer_V2()
            {
                TreeDefaults = treeDefaults,
            };

            using var db = init.CreateDatabase();

            var dataLayer = new CpDataLayer(db, Substitute.For<ILogger<CpDataLayer>>(), true);

            var volumeEquations = new[]
            {
                new VolumeEquationDO{ CalcBiomass = 1, Species = "101", PrimaryProduct = "01", VolumeEquationNumber = "1234567890" },
                new VolumeEquationDO{ CalcBiomass = 1, Species = "102", PrimaryProduct = "01", VolumeEquationNumber = "02" },
            };

            var PRs = new[]
            {
                new PercentRemoved{ bioSpecies = "101", bioProduct = "01", bioPCremoved = "95.0" },
                new PercentRemoved{ bioSpecies = "102", bioProduct = "01", bioPCremoved = "96.0" },
            };

            VolumeEquations.UpdateBiomassTemplate(dataLayer, volumeEquations, "01", "01", PRs);

            var biomassEqsAgain = db.From<CruiseDAL.V2.Models.BiomassEquation>().Query().ToArray();

            biomassEqsAgain.Should().HaveCount(volumeEquations.Length * VolumeEquations.BIOMASS_COMPONENTS.Count);


        }
    }
}
