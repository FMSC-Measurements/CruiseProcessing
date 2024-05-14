using CruiseDAL.DataObjects;
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
        public IServiceProvider MockServiceProvider { get; }
        public IDialogService MockDialogService { get; }
        public ILogger<MainWindowViewModel> MockLogger { get; }
        public DataLayerContext Dsp { get; private set; }
        public MainWindowViewModel ViewModel { get; private set; }

        public MainWindowViewModelTest(ITestOutputHelper output) : base(output)
        {
            MockServiceProvider = Substitute.For<IServiceProvider>();
            MockDialogService = Substitute.For<IDialogService>();
            MockLogger = Substitute.For<ILogger<MainWindowViewModel>>();

        }

        [Theory]
        [InlineData("12345 Region6TestTemplateData Timber Sale - TS.crz3")]
        public void OpenFile_V3(string testFileName)
        {
            var filePath = GetTestFile(testFileName);

            Dsp = new DataLayerContext()
            {
            };

            ViewModel = new MainWindowViewModel(MockServiceProvider, MockDialogService, Dsp, MockLogger);

            ViewModel.OpenFile(filePath);

            Dsp.DataLayer.Should().NotBeNull();
            Dsp.DataLayer.DAL.Should().NotBeNull();
            Dsp.DataLayer.DAL_V3.Should().NotBeNull();
            Dsp.DataLayer.CruiseID.Should().NotBeNull();
        }


        // Before adding standard reports would cause the report selection in the V3 file to be reset. This didn't affect the reports in the current
        // .process file but next time the file was opened the selected reports would be reset. 
        [Fact]
        public void Issue35_AddStandardReports_V3()
        {
            OpenFile_V3("12345 Region6TestTemplateData Timber Sale - TS.crz3");

            var v2SelectedCount = Dsp.DataLayer.DAL.From<ReportsDO>().Query().Where(x => x.Selected == true).Count();
            var v3SelectedCount = Dsp.DataLayer.DAL_V3.From<CruiseDAL.V3.Models.Reports>().Query().Where(x => x.Selected == true).Count();

            v3SelectedCount.Should().Be(v2SelectedCount);


            ViewModel.AddStandardReports();

            var v2SelectedCountAgain = Dsp.DataLayer.DAL.From<ReportsDO>().Query().Where(x => x.Selected == true).Count();
            var v3SelectedCountAgain = Dsp.DataLayer.DAL_V3.From<CruiseDAL.V3.Models.Reports>().Query().Where(x => x.Selected == true).Count();

            v3SelectedCountAgain.Should().Be(v2SelectedCountAgain);

            MockDialogService.Received().ShowStandardReports(Arg.Any<List<ReportsDO>>(), Arg.Any<bool>());
        }
    }
}
