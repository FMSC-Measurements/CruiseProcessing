using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        public List<exportGrades> GetExportGrade()
        {
            // rewritten Dec 2020 - Ben

            return DAL.Query<exportGrades>("SELECT * FROM ExportValues;").ToList();

        }   //  end GetExportGrade


        public void SaveExportGrade(List<exportGrades> sortList,
                                    List<exportGrades> gradeList, bool tableExists)
        {
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            //  open connection and save data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //  open connection
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //  if table exists, delete all rows
            if (tableExists == true)
            {
                //sqlcmd.CommandText = "DELETE FROM ExportValues";
                //sqlcmd.ExecuteNonQuery();
                DAL.Execute("DELETE FROM ExportValues");
            }   //  endif

            StringBuilder sb = new StringBuilder();
            //  create query for each list
            foreach (exportGrades eg in sortList)
            {
                var _sb = createInsertQuery(eg, "sort");
                DAL.Execute(_sb.ToString());
                // sqlcmd.CommandText = sb.ToString();
                // sqlcmd.ExecuteNonQuery();
            }   //  end foreach on sortList

            foreach (exportGrades eg in gradeList)
            {
                var _sb = createInsertQuery(eg, "grade");
                DAL.Execute(_sb.ToString());

            }   //  end foreach on gradeList
                //sqlconn.Close();
                //}   //  end using

            return;

            StringBuilder createInsertQuery(exportGrades eg, string sortOrGrade)
            {
                StringBuilder queryString = new StringBuilder();
                queryString.Append("INSERT INTO ExportValues ");
                queryString.Append("(exportSort,exportGrade,exportCode,exportName,minDiam,minLength,minBDFT,maxDefect)");

                //  enter the actual values for the record
                queryString.Append(" VALUES ('");
                if (sortOrGrade == "sort")
                {
                    queryString.Append(eg.exportSort);
                    queryString.Append("','','");          //  blank exportGrade
                }
                else if (sortOrGrade == "grade")
                {
                    queryString.Append("','");        //  blank exportSort
                    queryString.Append(eg.exportGrade);
                    queryString.Append("','");
                }   //  endif sortOrGrade

                queryString.Append(eg.exportCode);
                queryString.Append("','");
                queryString.Append(eg.exportName);
                queryString.Append("','");
                queryString.Append(eg.minDiam);
                queryString.Append("','");
                queryString.Append(eg.minLength);
                queryString.Append("','");
                queryString.Append(eg.minBDFT);
                queryString.Append("','");
                queryString.Append(eg.maxDefect);
                queryString.Append("');");

                return queryString;
            }   //  end createInsertQuery
        }   //  end SaveExportGrade
    }
}
