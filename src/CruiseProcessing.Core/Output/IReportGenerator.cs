using CruiseProcessing.OutputModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Output
{
    public interface IReportGenerator
    {
        int GenerateReport(TextWriter strWriteOut, HeaderFieldData headerData, int startPageNum);
    }
}
