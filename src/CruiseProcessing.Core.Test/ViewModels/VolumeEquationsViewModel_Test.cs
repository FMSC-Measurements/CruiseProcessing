using CruiseDAL.DataObjects;
using CruiseDAL;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using CruiseProcessing.ViewModels;
using FluentAssertions;
using CruiseDAL.V2.Models;
using Microsoft.Extensions.DependencyInjection;
using CruiseProcessing.Services;
using Microsoft.Extensions.Options;
using CruiseProcessing.Config;

namespace CruiseProcessing.Test.ViewModels
{
    public class VolumeEquationsViewModel_Test : TestBase
    {
        public VolumeLibrary_20241118 VolumeLibrary { get; }

        public VolumeEquationsViewModel_Test(ITestOutputHelper output) : base(output)
        {
            VolumeLibrary = new VolumeLibrary_20241118();

        }

        [Theory]
        [InlineData(1, "01", "L", "sp1", "01", 101, 101)]
        [InlineData(1, "01", "L", "sp1", "01", 0, 999)] // if input fia code is invalid output fiacode should be 999
        public void CreateBiomassEquations_Mockedup(int region, string forest, string liveDead, string species, string prod, int fiaCode, int expectedFiaCode)
        {
            var volumeEquation = new VolumeEquationDO { Species = species, PrimaryProduct = prod };

            var tdvs = new[] { new TreeDefaultValueDO { LiveDead = liveDead, Species = species, PrimaryProduct = prod, FIAcode = fiaCode }, };

            var dataLayerMock = Substitute.For<CpDataLayer>();
            dataLayerMock.GetTreeDefaultValues(Arg.Is(species), Arg.Is(prod))
                .Returns(tdvs);

            var host = ImplicitHost;
            var viewModel = new VolumeEquationsViewModel(dataLayerMock,
                host.Services.GetRequiredService<IDialogService>(),
                VolumeLibrary,
                host.Services.GetRequiredService<IOptions<BiomassEquationOptions>>(),
                CreateLogger<VolumeEquationsViewModel>());

            var bioEqs = viewModel.CreateBiomassEquations(volumeEquation, region, forest, 95.0f);

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


            var dataLayer = new CpDataLayer(db, CreateLogger<CpDataLayer>(), biomassOptions: null, true);

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

            var host = ImplicitHost;
            var viewModel = new VolumeEquationsViewModel(dataLayer,
                host.Services.GetRequiredService<IDialogService>(),
                VolumeLibrary,
                host.Services.GetRequiredService<IOptions<BiomassEquationOptions>>(),
                CreateLogger<VolumeEquationsViewModel>());

            var bioEqs = viewModel.CreateBiomassEquations(volumeEquations, 1, "01", PRs);

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




    }
}

