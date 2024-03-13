using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Services.Logging
{
    public class NullScope : IDisposable
    {
        public static IDisposable Instance { get; } = new NullScope();
        public void Dispose() { }
    }
}
