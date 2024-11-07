using CruiseDAL;
using CruiseDAL.DataObjects;
using CruiseDAL.V2.Models;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CruiseProcessing.Test.Data
{
    public class CpDataLayer_BiomassEquationDataservice_Test : TestBase
    {
        public CpDataLayer_BiomassEquationDataservice_Test(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(1, "01", "L", "sp1", "01", 101, 101)]
        [InlineData(1, "01", "L", "sp1", "01", 0, 999)] // if input fia code is invalid output fiacode should be 999
        public void CreateBiomassEquations_Mockedup(int region, string forest, string liveDead, string species, string prod, int fiaCode, int expectedFiaCode)
        {
            var volumeEquation = new VolumeEquationDO { Species = species, PrimaryProduct = prod };

            var tdvs = new[] { new TreeDefaultValueDO { LiveDead = liveDead, Species = species, PrimaryProduct = prod, FIAcode = fiaCode }, };

            var dataLayerMock = Substitute.For<CpDataLayer>();

            var bioEqs = dataLayerMock.CreateBiomassEquations(volumeEquation, tdvs, region, forest, 95.0f);

            bioEqs.Should().HaveCount(CpDataLayer.BIOMASS_COMPONENTS.Count);
            bioEqs.Should().OnlyContain(x => x.FIAcode == fiaCode);

            bioEqs.Where(x => x.Component == "PrimaryProd").Single().WeightFactorPrimary.Should().BeGreaterThan(0);
            bioEqs.Where(x => x.Component == "SecondaryProd").Single().WeightFactorSecondary.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CreateBiomassEquations()
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

            foreach (var tree in trees)
            {
                db.Insert(tree);
            }


            var dataLayer = new CpDataLayer(db, Substitute.For<ILogger<CpDataLayer>>(), biomassOptions: null, true);

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

            var bioEqs = dataLayer.CreateBiomassEquations(volumeEquations, 1, "01", PRs);

            bioEqs.Should().HaveCountGreaterOrEqualTo(volumeEquations.Length * CpDataLayer.BIOMASS_COMPONENTS.Count);
            bioEqs.Should().HaveCountLessOrEqualTo(2 * volumeEquations.Length * CpDataLayer.BIOMASS_COMPONENTS.Count);

            bioEqs.Where(x => x.Species == "101" && x.Product == "01" && x.LiveDead == "L" && x.Component == "PrimaryProd").Should().HaveCount(1);
            bioEqs.Where(x => x.Species == "101" && x.Product == "01" && x.LiveDead == "D" && x.Component == "PrimaryProd").Should().HaveCount(1);


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



        [Theory]
        [InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        [InlineData("OgTest\\Region5\\R5.cruise")]
        public void CompareWeightFactors(string fileName)
        {
            base.LogLevel = LogLevel.None;
            var filePath = GetTestFile(fileName);

            var mockLogger = CreateLogger<CpDataLayer>();
            var dal = new DAL(filePath);
            var dataLayer = new CpDataLayer(dal, mockLogger, biomassOptions: null);

            var volEqs = dataLayer.getVolumeEquations();
            if (volEqs.Any(x => x.CalcBiomass == 1) == false)
            {
                throw new Exception("Skipping test, no biomass equations found");// we are primarily interested in checking for changes in biomass calculation
            }

            var volLib = new VolumeLibrary_20241101();
            var sale = dataLayer.GetSale();
            foreach(var veq in volEqs)
            {
                if(veq.CalcBiomass != 1)
                {
                    continue;
                }

                var oldWF = dataLayer.GetPrimaryWeightFactorAndMoistureContent(veq.Species, veq.PrimaryProduct, "L");

                var tdv = dataLayer.GetTreeDefaultValues(veq.Species, veq.PrimaryProduct).First();
                volLib.LookupWeightFactors2(int.Parse(sale.Region), sale.Forest, (int)tdv.FIAcode, veq.PrimaryProduct, out var newWf, out _);

                Output.WriteLine($"Species: {veq.Species} Product: {veq.PrimaryProduct} OldWF: {oldWF.WeightFactor} NewWF: {newWf}");
                newWf.Should().Be(oldWF.WeightFactor);
            }

        }
    }
}
