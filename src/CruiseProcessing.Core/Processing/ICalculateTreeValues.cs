using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Processing
{
    public interface ICalculateTreeValues
    {
        void ProcessTrees(string currST, string currMethod, long currST_CN);
    }
}
