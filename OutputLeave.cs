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
        public CPbusinessLayer bslyr = new CPbusinessLayer();
        #endregion

        public void createLeaveTreeReports(StreamWriter strWriteout, ref int pageNumb, reportHeaders rh)
        {
            switch (currentReport)
            {
                case "LV01":         case "LV02":
                    OutputSummary os = new OutputSummary();
                    os.currCL = "L";
                    os.currentReport = currentReport;
                    os.bslyr.fileName = bslyr.fileName;
                    os.bslyr.DAL = bslyr.DAL;
                    os.mainHeaderFields = mainHeaderFields;
                    os.OutputSummaryReports(strWriteout, rh, ref pageNumb);
                    break;
                case "LV03":         case "LV04":
                    OutputStats osr = new OutputStats();
                    osr.currCL = "L";
                    osr.currentReport = currentReport;
                    osr.bslyr.fileName = bslyr.fileName;
                    osr.bslyr.DAL = bslyr.DAL;
                    osr.mainHeaderFields = mainHeaderFields;
                    osr.CreateStatReports(strWriteout, rh, ref pageNumb);
                    break;
                case "LV05":
                    OutputUnits ou = new OutputUnits();
                    ou.currCL = "L";
                    ou.currentReport = currentReport;
                    ou.bslyr.fileName = bslyr.fileName;
                    ou.bslyr.DAL = bslyr.DAL;
                    ou.mainHeaderFields = mainHeaderFields;
                    ou.OutputUnitReports(strWriteout, rh, ref pageNumb);
                    break;
            }   //  end switch on report
            return;
        }   //  end createLeaveTreeReports

    }
}
