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

    }
}
