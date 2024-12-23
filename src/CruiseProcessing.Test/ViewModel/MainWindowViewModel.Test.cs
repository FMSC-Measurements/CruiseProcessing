using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
using CruiseProcessing.ViewModel;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CruiseProcessing.Test.ViewModel
{
    public class MainWindowViewModelTest : TestBase
    {

        public MainWindowViewModelTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("12345 Region6TestTemplateData Timber Sale - TS.crz3")]
        public void OpenFile_V3(string testFileName)
        {
            var viewModel = OpenFileHelper(testFileName);
            viewModel.DataLayer.Dispose();
        }

        protected MainWindowViewModel OpenFileHelper(string testFileName)
        {
            var filePath = GetTestFile(testFileName);

            var mockDialogService = Substitute.For<IDialogService>();
            var mockLogger = Substitute.For<ILogger<MainWindowViewModel>>();
            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var mockServiceProvider = Substitute.For<IServiceProvider>();

            mockServiceProvider.GetService<ILogger<CpDataLayer>>().Returns(mockDlLogger);

            var dsc = new DataLayerContext()
            {
            };

            var viewModel = new MainWindowViewModel(mockServiceProvider, mockDialogService, dsc, mockLogger);

            viewModel.OpenFile(filePath);

            dsc.DataLayer.Should().NotBeNull();
            dsc.DataLayer.DAL.Should().NotBeNull();
            dsc.DataLayer.DAL_V3.Should().NotBeNull();
            dsc.DataLayer.CruiseID.Should().NotBeNull();

            return viewModel;
        }


        // Before adding standard reports would cause the report selection in the V3 file to be reset. This didn't affect the reports in the current
        // .process file but next time the file was opened the selected reports would be reset. 
        [Fact]
        public void Issue35_AddStandardReports_V3()
        {
            var viewModel = OpenFileHelper("12345 Region6TestTemplateData Timber Sale - TS.crz3");
            var dsc = viewModel.DataserviceProvider;

            var v2SelectedCount = dsc.DataLayer.DAL.From<ReportsDO>().Query().Where(x => x.Selected == true).Count();
            var v3SelectedCount = dsc.DataLayer.DAL_V3.From<CruiseDAL.V3.Models.Reports>().Query().Where(x => x.Selected == true).Count();

            v3SelectedCount.Should().Be(v2SelectedCount);


            viewModel.AddStandardReports();

            var v2SelectedCountAgain = dsc.DataLayer.DAL.From<ReportsDO>().Query().Where(x => x.Selected == true).Count();
            var v3SelectedCountAgain = dsc.DataLayer.DAL_V3.From<CruiseDAL.V3.Models.Reports>().Query().Where(x => x.Selected == true).Count();

            v3SelectedCountAgain.Should().Be(v2SelectedCountAgain);

            viewModel.DialogService.Received().ShowStandardReports(Arg.Any<List<ReportsDO>>(), Arg.Any<bool>());

            viewModel.DataLayer.Dispose();
        }
    }
}
