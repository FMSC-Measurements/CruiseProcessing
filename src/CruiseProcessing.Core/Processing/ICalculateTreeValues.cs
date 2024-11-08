using CruiseProcessing.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Processing
{
    public interface ICalculateTreeValues
    {
        IVolumeLibrary VolLib { get; }

        string GetVersion();

        void ProcessTrees(string currST, string currMethod, long currST_CN);
    }
}
