using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using CruiseProcessing.Data;

namespace CruiseProcessing
{
    public static class ReportMethods
    {
        public static readonly string[] CSV_REPORTS = new string[6] { "CSV1", "CSV2", "CSV3", "CSV4", "CSV5", "CSV6" };

        // I dont think csv reports 
        public static void deleteCSVReports(List<ReportsDO> rList, CpDataLayer bslyr)
        {
            //  probably used infrequently
            //  currently deletes CSV reports from the reports list
            //  however it does not delete in the database
            //  so do that first
            string[] reportsToDelete = CSV_REPORTS;
            for (int j = 0; j < 6; j++)
            {
                bslyr.DeleteReport(reportsToDelete[j]);
            }
        }

    }
}
