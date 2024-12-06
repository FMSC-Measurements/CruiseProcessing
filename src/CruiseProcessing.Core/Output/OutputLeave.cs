using CruiseProcessing.Data;
using CruiseProcessing.Output;
using CruiseProcessing.OutputModels;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CruiseProcessing
{
    internal class OutputLeave : ReportGeneratorBase
    {
        public List<string> mainHeaderFields;

        public OutputLeave(CpDataLayer dataLayer, HeaderFieldData headerData, string reportID) : base(dataLayer, reportID)
        {
            HeaderData = headerData;
        }

        public HeaderFieldData HeaderData { get; }

        public void createLeaveTreeReports(TextWriter strWriteout, ref int pageNumb)
        {
            switch (currentReport)
            {
                case "LV01":
                case "LV02":
                    OutputSummary os = new OutputSummary("L", DataLayer, HeaderData, currentReport);
                    os.OutputSummaryReports(strWriteout, ref pageNumb);
                    break;

                case "LV03":
                case "LV04":
                    OutputStats osr = new OutputStats("L", DataLayer, HeaderData, currentReport);
                    osr.CreateStatReports(strWriteout, ref pageNumb);
                    break;

                case "LV05":
                    OutputUnits ou = new OutputUnits("L", DataLayer, HeaderData, currentReport);
                    ou.OutputUnitReports(strWriteout, ref pageNumb);
                    break;
            }   //  end switch on report
            return;
        }   //  end createLeaveTreeReports
    }
}