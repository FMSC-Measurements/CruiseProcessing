using CruiseDAL;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.Processing;
using CruiseProcessing.Services;
using CruiseProcessing.ViewModel;
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
using System.Web;
using Xunit.Abstractions;

namespace CruiseProcessing.Test.ViewModel
{
    public class ProcessCruiseViewModel_Test : TestBase
    {
        public ProcessCruiseViewModel_Test(ITestOutputHelper output) : base(output)
        {
        }

        private IServiceProvider GetServiceProviderMock(CpDataLayer datalayer)
        {
            var mockServiceProvider = Substitute.For<IServiceProvider>();
            
            mockServiceProvider.GetService(typeof(CalculateTreeValues2))
                .Returns(new CalculateTreeValues2(datalayer, VolumeLibraryInterop.Default, Substitute.For<ILogger<CalculateTreeValues2>>()));

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

            var mockDialogService = Substitute.For<IDialogService>();
            var mockLogger = CreateLogger<ProcessStatus>();
            var mockServiceProvider = GetServiceProviderMock(dataLayer);
            var mockServiceCollection = Substitute.For<IServiceCollection>();

            var processStatus = new ProcessCruiseViewModel(dataLayer, mockDialogService, mockServiceProvider, CreateLogger<ProcessCruiseViewModel>(), mockServiceCollection);

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
        public async Task ProcessCruiseAsync(string testFileName)
        {
            var filePath = GetTestFile(testFileName);
            using var dal = new DAL(filePath);

            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(dal, mockDlLogger, biomassOptions: null);

            var mockDialogService = Substitute.For<IDialogService>();
            var mockLogger = CreateLogger<ProcessStatus>();
            var mockServiceProvider = GetServiceProviderMock(dataLayer);
            var mockServiceCollection = Substitute.For<IServiceCollection>();

            mockServiceProvider.GetService(typeof(ICruiseProcessor))
                .Returns(new CruiseProcessor(dataLayer, mockDialogService, Substitute.For<ILogger<CruiseProcessor>>()));

            var processStatus = new ProcessCruiseViewModel(dataLayer, mockDialogService, mockServiceProvider, CreateLogger<ProcessCruiseViewModel>(), mockServiceCollection);

            var result = processStatus.DoPreProcessChecks();
            if (!result)
            { throw new Exception("Skip"); }

            await processStatus.ProcessCruiseAsync();

            mockDialogService.DidNotReceiveWithAnyArgs().ShowError(Arg.Any<string>());
        }
    }
}
