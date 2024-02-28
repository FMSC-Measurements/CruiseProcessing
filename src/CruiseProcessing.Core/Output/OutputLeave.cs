using CruiseProcessing.Services;
using System.Collections;
using System.IO;

namespace CruiseProcessing
{
    internal class OutputLeave : CreateTextFile
    {
        public string currentReport;
        public ArrayList mainHeaderFields;

        public OutputLeave(CPbusinessLayer dataLayer, IDialogService dialogService) : base(dataLayer, dialogService)
        {
        }

        public void createLeaveTreeReports(StreamWriter strWriteout, ref int pageNumb, reportHeaders rh)
        {
            switch (currentReport)
            {
                case "LV01":
                case "LV02":
                    OutputSummary os = new OutputSummary(DataLayer, DialogService);
                    os.currCL = "L";
                    os.currentReport = currentReport;
                    os.mainHeaderFields = mainHeaderFields;
                    os.OutputSummaryReports(strWriteout, rh, ref pageNumb);
                    break;

                case "LV03":
                case "LV04":
                    OutputStats osr = new OutputStats(DataLayer, DialogService);
                    osr.currCL = "L";
                    osr.currentReport = currentReport;
                    osr.mainHeaderFields = mainHeaderFields;
                    osr.CreateStatReports(strWriteout, rh, ref pageNumb);
                    break;

                case "LV05":
                    OutputUnits ou = new OutputUnits(DataLayer, DialogService);
                    ou.currCL = "L";
                    ou.currentReport = currentReport;
                    ou.mainHeaderFields = mainHeaderFields;
                    ou.OutputUnitReports(strWriteout, rh, ref pageNumb);
                    break;
            }   //  end switch on report
            return;
        }   //  end createLeaveTreeReports
    }
}