using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    class OutputLeave
    {
        #region
        public string currentReport;
        public ArrayList mainHeaderFields;
        #endregion

        public void createLeaveTreeReports(StreamWriter strWriteout, ref int pageNumb, reportHeaders rh)
        {
            switch (currentReport)
            {
                case "LV01":         case "LV02":
                    OutputSummary os = new OutputSummary();
                    os.currCL = "L";
                    os.currentReport = currentReport;
                    os.mainHeaderFields = mainHeaderFields;
                    os.OutputSummaryReports(strWriteout, rh, ref pageNumb);
                    break;
                case "LV03":         case "LV04":
                    OutputStats osr = new OutputStats();
                    osr.currCL = "L";
                    osr.currentReport = currentReport;
                    osr.mainHeaderFields = mainHeaderFields;
                    osr.CreateStatReports(strWriteout, rh, ref pageNumb);
                    break;
                case "LV05":
                    OutputUnits ou = new OutputUnits();
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
