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

namespace CruiseProcessing.Test.Output
{
    public class OutputWeight_Test : TestBase
    {
        public OutputWeight_Test(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void OutputWeightReports_WT1()
        {             //  test for WT1 report
            var filePath = GetTestFile("02376_Cowboy Salvage_TS_202405071110_GalaxyTabActive3-18CJU.process.cruise");

            using var db = new DAL(filePath);

            var dataLayer = new CpDataLayer(db, Substitute.For<ILogger<CpDataLayer>>(), biomassOptions: null);


            var reportID = "WT1";
            var headerData = dataLayer.GetReportHeaderData();
            var outputWeight = new OutputWeight(dataLayer, VolumeLibraryInterop.Default, headerData, reportID);


            var writer = new System.IO.StringWriter();
            int pageNum = 0;
            outputWeight.OutputWeightReports(writer, ref pageNum);

            var output = writer.ToString();
            Output.WriteLine(output);
        }
    }
}
