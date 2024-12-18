using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Output
{
    public static class HostBuilderExtentions
    {
        public static IServiceCollection AddOutputReportGenerators(this IServiceCollection serviceProvider)
        {

            serviceProvider.AddTransient<Wt1ReportGenerator>();
            serviceProvider.AddTransient<Wt2ReportGenerator>();
            serviceProvider.AddTransient<Wt3ReportGenerator>();
            serviceProvider.AddTransient<Wt4ReportGenerator>();
            serviceProvider.AddTransient<Wt5ReportGenerator>();

            return serviceProvider;
        }
    }
}
