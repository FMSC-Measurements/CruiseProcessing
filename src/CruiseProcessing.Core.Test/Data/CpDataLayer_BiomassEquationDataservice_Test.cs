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

            var volLib = new VolumeLibrary_20241118();
            var sale = dataLayer.GetSale();
            foreach (var veq in volEqs)
            {
                if (veq.CalcBiomass != 1)
                {
                    continue;
                }

                var oldWF = dataLayer.GetWeightFactorArray(veq.Species, veq.PrimaryProduct, "L", volLib)[0];

                var tdv = dataLayer.GetTreeDefaultValues(veq.Species, veq.PrimaryProduct).First();
                volLib.LookupWeightFactorsNVB(int.Parse(sale.Region), sale.Forest, (int)tdv.FIAcode, veq.PrimaryProduct, out var newWf, out _);

                Output.WriteLine($"Species: {veq.Species} Product: {veq.PrimaryProduct} OldWF: {oldWF} NewWF: {newWf}");
                newWf.Should().Be(oldWF);
            }

        }
    }
}
