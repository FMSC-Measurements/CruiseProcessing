using CruiseDAL;
using CruiseDAL.DataObjects;
using CruiseProcessing.Config;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
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

        [Fact]
        public void UpdateBiomassEquations()
        {

            var speciesCodes = new[]
            {
                "101",
                "102",
            };
            var init = new DatabaseInitializer_V2(speciesCodes);
            init.TreeDefaults = new[]
            {
                new TreeDefaultValue{ Species = "101", FIAcode = 101, PrimaryProduct = "01", LiveDead = "L" },
                new TreeDefaultValue{ Species = "101", FIAcode = 101, PrimaryProduct = "01", LiveDead = "D" },
                new TreeDefaultValue{ Species = "102", FIAcode = 102, PrimaryProduct = "01", LiveDead = "L" },
            };
            init.SampleGroups = null;



            var database = init.CreateDatabase();
            var datalayer = new CpDataLayer(database, CreateLogger<CpDataLayer>(), biomassOptions: null);

            var mockDialogService = Substitute.For<IDialogService>();
            var mockServiceProvider = Substitute.For<IServiceProvider>();
            var volumeEquationsForm = new VolumeEquations(datalayer, mockDialogService, mockServiceProvider);

            var volumeEquations = new[]
            {
                new VolumeEquationDO { Species = "101", PrimaryProduct = "01", CalcBiomass = 1 },
                new VolumeEquationDO { Species = "102", PrimaryProduct = "01", CalcBiomass = 1 },
            }.ToList();

            volumeEquationsForm.UpdateBiomass(volumeEquations, "1", "01");

            // get all the volume equations cross joined with all the tree defaults and count them
            var crossJoinCount = volumeEquations.GroupJoin(init.TreeDefaults,
                i => i.Species, // key from VolumeEquation
                o => o.Species, // key from TreeDefaultValue
                (i, o) => (i, o)) // select vol eqs joined with tree defaults
                .Sum(x => x.o.Count()); // sum the count of tree defaults for each volume equation

            // ensure that the number of generated biomass equations matches whats expected
            var bioEqs = datalayer.getBiomassEquations();
            bioEqs.Should().HaveCount(crossJoinCount * CpDataLayer.BIOMASS_COMPONENTS.Count);
        }

    }
}
