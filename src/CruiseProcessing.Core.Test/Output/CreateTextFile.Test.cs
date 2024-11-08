using CruiseDAL;
using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CruiseProcessing.Test.Output
{
    public class CreateTextFileTest : TestBase
    {
        public CreateTextFileTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test_V3.process")]
        [InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise")]
        [InlineData("OgTest\\Region4\\R4_McDougal.cruise")]
        [InlineData("OgTest\\Region5\\R5.cruise")]
        [InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise")]
        [InlineData("OgTest\\Region9\\R9.cruise")]
        [InlineData("OgTest\\Region10\\R10.cruise")]

        [InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise")]
        [InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        [InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise")]
        [InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        [InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        [InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise")]

        [InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        [InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        [InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        [InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]
        public void CreateOutFile(string testFileName)
        {

            var filePath = GetTestFile(testFileName);
            using var dal = new DAL(filePath);

            var mockLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(dal, mockLogger, biomassOptions: null);

            List<TreeDO> tList = dataLayer.getTrees();
            double summedEF = tList.Sum(t => t.ExpansionFactor);
            List<TreeCalculatedValuesDO> tcvList = dataLayer.getTreeCalculatedValues();
            double summedNetBDFT = tcvList.Sum(tc => tc.NetBDFTPP);
            double summedNetCUFT = tcvList.Sum(tc => tc.NetCUFTPP);
            if (summedEF == 0 && summedNetBDFT == 0 && summedNetCUFT == 0)
            {
                throw new Exception("not processed");
            }   //  endif no volume for reports
            List<ReportsDO> selectedReports = dataLayer.GetSelectedReports();

            //  no reports?  let user know to go back and select reports
            if (selectedReports.Count == 0)
            {
                throw new Exception("no reports selected");
            }   //  endif no reports

            using StringWriter strWriteOut = new StringWriter();

            var headerData = dataLayer.GetReportHeaderData();
            var ctf = new CreateTextFile(dataLayer, VolumeLibraryInterop.Default, Substitute.For<ILogger<CreateTextFile>>());
            _ = ctf.CreateOutFile(selectedReports, headerData, strWriteOut, out var failedReports, out var hasWarnings);

            strWriteOut.ToString().Should().NotBeNullOrEmpty();

            if(hasWarnings)
            {
                Output.WriteLine("INFO::::Output Has Warnings");
            }

            Output.WriteLine(strWriteOut.ToString());

        }
    }
}
