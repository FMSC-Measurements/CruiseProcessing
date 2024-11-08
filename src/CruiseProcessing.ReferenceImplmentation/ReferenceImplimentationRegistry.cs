using CruiseProcessing.Processing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.ReferenceImplmentation
{
    public static class ReferenceImplimentationRegistry
    {
        public static readonly IEnumerable<KeyValuePair<string, Type>> CruiseProcessorImplementations = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>(nameof(RefCruiseProcessor), typeof(RefCruiseProcessor)),
        };

        public static void RegisterReferenceImplimentations(this IServiceCollection services)
        {
            foreach (var impl in CruiseProcessorImplementations)
            {
                services.AddKeyedTransient(typeof(ICruiseProcessor), impl.Key, impl.Value);
            }
        }
    }
}
