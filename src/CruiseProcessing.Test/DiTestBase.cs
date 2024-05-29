using CruiseProcessing.Services;
using CruiseProcessing.ViewModel;
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
    public class DiTestBase : TestBase
    {
        public IServiceProvider MockServiceProvider { get; }
        public IDialogService MockDialogService { get; }
        public DataLayerContext DataContext { get; private set; }



        public DiTestBase(ITestOutputHelper output) : base(output)
        {
            MockServiceProvider = Substitute.For<IServiceProvider>();
            MockDialogService = Substitute.For<IDialogService>();
            DataContext = new DataLayerContext();
        }

        protected ILogger<T> GetLogger<T>()
        {
            return Substitute.For<ILogger<T>>();
        }
    }
}
