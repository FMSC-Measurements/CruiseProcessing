using System;
using System.Threading.Tasks;

namespace CruiseProcessing
{
    public interface ICruiseProcessor
    {
        Task ProcessCruiseAsync(IProgress<string> progress);

        void ProcessCruise(IProgress<string> progress);
    }
}