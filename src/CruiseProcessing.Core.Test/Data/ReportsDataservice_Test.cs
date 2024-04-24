using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CruiseProcessing.Test.Data
{
    public class ReportsDataservice_Test : TestBase
    {
        public ReportsDataservice_Test(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetDefaultReports()
        {
            var list = ReportsDataservice.GetDefaultReports();
            list.Should().NotBeEmpty();
        }
    }
}
