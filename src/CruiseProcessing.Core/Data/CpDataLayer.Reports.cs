using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        //  Reports table
        // *******************************************************************************************
        public void SaveReports(List<ReportsDO> rList)
        {
            //  This saves the updated reports list
            foreach (ReportsDO rd in rList)
            {
                if (rd.DAL == null)
                {
                    rd.DAL = DAL;
                }
                rd.Save();
            }   //  end foreach loop

            if (DAL_V3 != null)
            {
                foreach (var report in rList)
                {
                    DAL_V3.Execute2(
@"INSERT INTO Reports (
    ReportID,
    CruiseID,
    Selected,
    Title )
VALUES (
    @ReportID,
    @CruiseID,
    @Selected,
    @Title )
ON CONFLICT (ReportID, CruiseID) DO UPDATE SET
    Selected = @Selected,
    Title = @Title
WHERE ReportID = @ReportID AND CruiseID = @CruiseID;",
                    new
                    {
                        report.ReportID,
                        CruiseID,
                        report.Selected,
                        report.Title
                    });
                }

            }
        }


        public List<ReportsDO> GetReports()
        {
            //  Retrieve reports
            return DAL.From<ReportsDO>().OrderBy("ReportID").Read().ToList();
        }   //  end GetReports


        public List<ReportsDO> GetSelectedReports()
        {
            return DAL.From<ReportsDO>().Where("Selected = 'True' OR Selected = '1'").OrderBy("ReportID").Read().ToList();
        }   //  end GetSelectedReports



        public void updateReports(List<ReportsDO> reportList)
        {
            // rewritten Dec 2020 - Ben
            //  this updates the reports list after user has selected reports

            foreach (ReportsDO rdo in reportList)
            {
                DAL.Execute("UPDATE Reports SET Selected =  @p1 WHERE ReportID = @p2;", rdo.Selected, rdo.ReportID);
            }

            if (DAL_V3 != null)
            {
                foreach (ReportsDO rdo in reportList)
                {
                    DAL_V3.Execute("UPDATE Reports SET Selected =  @p1 WHERE ReportID = @p2 AND CruiseID = @p3;", rdo.Selected, rdo.ReportID, CruiseID);
                }
            }
        }

        public void DeleteReport(string reportID)
        {
            // rewritten Dec 2020 - Ben

            //  deletes the report from the reports table
            DAL.Execute("DELETE FROM Reports WHERE ReportID= @p1;", reportID);

            if (DAL_V3 != null)
            {
                DAL_V3.Execute("DELETE FROM Reports WHERE ReportID= @p1 AND CruiseID = @p2;", reportID, CruiseID);
            }
        }
    }
}
