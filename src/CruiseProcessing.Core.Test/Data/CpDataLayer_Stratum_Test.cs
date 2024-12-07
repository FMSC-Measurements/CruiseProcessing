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

namespace CruiseProcessing.Test.Data
{
    public class CpDataLayer_Stratum_Test : TestBase
    {
        public CpDataLayer_Stratum_Test(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetStratumCodesByUnit()
        {
            var init = new DatabaseInitializer_V2();

            var v2db = init.CreateDatabase();

            var mockLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(v2db, mockLogger, biomassOptions: null);

            var units = dataLayer.getCuttingUnits();

            var unitCode = units.First().Code;
            var stCodes = dataLayer.GetStratumCodesByUnit(unitCode);
            stCodes.Should().HaveSameCount(init.UnitStrata.Where(x => x.UnitCode == unitCode));
        }

        [Fact]
        public void GetStratumAcrest()
        {
            var init = new DatabaseInitializer_V2();

            var v2db = init.CreateDatabase();

            var mockLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(v2db, mockLogger, biomassOptions: null);


            foreach(var st in init.Strata)
            {
                var stAcres = dataLayer.GetStratumAcres(st.Code);
                var stratum = dataLayer.GetStratum(st.Code);
                stratum.CuttingUnits.Populate();
                var expectedAcres = stratum.CuttingUnits.Sum(x => x.Area);

                stAcres.Should().Be(expectedAcres);
            }
        }
    }
}
