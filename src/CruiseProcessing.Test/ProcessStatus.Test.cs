using CruiseDAL;
using CruiseProcessing.Async;
using CruiseProcessing.Data;
using CruiseProcessing.ReferenceImplmentation;
using CruiseProcessing.Services;
using DiffPlex.DiffBuilder;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using ZedGraph;

namespace CruiseProcessing.Test
{
    public class ProcessStatus_Test : TestBase
    {

        public ProcessStatus_Test(ITestOutputHelper output) : base(output)
        {
            
        }

        private Mock<IServiceProvider> GetServiceProviderMock(CpDataLayer datalayer)
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(It.Is<Type>(x => x == typeof(ICalculateTreeValues))))
                .Returns(() => new CalculateTreeValues2(datalayer, Substitute.For<ILogger<CalculateTreeValues2>>()));

            return mockServiceProvider;
        }

        [Theory]
        //[InlineData("OgTest\\BLM\\Hammer Away.cruise")]
        //[InlineData("OgTest\\BLM\\Long Nine.cruise")]
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
        public void DoPreProcessChecks(string testFileName)
        {
            var filePath = GetTestFile(testFileName);
            using var dal = new DAL(filePath);

            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(dal, mockDlLogger, biomassOptions: null);

            var mockDialogService = new Mock<IDialogService>();
            var mockLogger = new Mock<ILogger<ProcessStatus>>();
            var mockServiceProvider = GetServiceProviderMock(dataLayer);

            var processStatus = new ProcessStatus(dataLayer, mockDialogService.Object, mockLogger.Object, Substitute.For<ICruiseProcessor>());

            var result = processStatus.DoPreProcessChecks();
            result.Should().BeTrue();

        }


        [Theory]
        //[InlineData("OgTest\\BLM\\Hammer Away.cruise")]
        //[InlineData("OgTest\\BLM\\Long Nine.cruise")]
        [InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test_V3.process")]
        //[InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        //[InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise")]
        [InlineData("OgTest\\Region4\\R4_McDougal.cruise")]
        [InlineData("OgTest\\Region5\\R5.cruise")]
        //[InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise")]
        [InlineData("OgTest\\Region9\\R9.cruise")]
        [InlineData("OgTest\\Region10\\R10.cruise")]

        [InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        [InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise")]
        //[InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        //[InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        [InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise")]

        [InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        //[InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        //[InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process")]

        //[InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        [InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        [InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]
        public async Task Process(string testFileName)
        {
            var filePath = GetTestFile(testFileName);
            using var dal = new DAL(filePath);

            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(dal, mockDlLogger, biomassOptions: null);

            var mockDialogService = Substitute.For<IDialogService>();

            var cruiseProcessor = new CruiseProcessor(dataLayer, mockDialogService, Substitute.For<ILogger<CruiseProcessor>>(), CreateLogger<CalculateTreeValues2>());
            var processStatus = new ProcessStatus(dataLayer, mockDialogService, Substitute.For<ILogger<ProcessStatus>>(), cruiseProcessor);

            var result = processStatus.DoPreProcessChecks();
            if (!result)
            { throw new Exception("Skip"); }

            await processStatus.Process();

            mockDialogService.DidNotReceiveWithAnyArgs().ShowError(Arg.Any<string>());
        }

    }
}
