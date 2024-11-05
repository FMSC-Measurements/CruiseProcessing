using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Processing
{
    public interface ICruiseProcessor
    {
        Task ProcessCruiseAsync(IProgress<string> progress);

        void ProcessCruise(IProgress<string> progress);
    }
}
