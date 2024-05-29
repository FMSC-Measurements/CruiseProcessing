using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        private static readonly string[] ExportValuesColums =  new string[]
        {
            "exportSort TEXT,",
            "exportGrade TEXT,",
            "exportCode TEXT,",
            "exportName TEXT,",
            "minDiam TEXT,",
            "minLength TEXT,",
            "minBDFT TEXT,",
            "maxDefect TEXT"
        };

        private static readonly string[] LogMatrixColumns = new string[]
        {
            "ReportNumber TEXT,",
            "GradeDescription TEXT,",
            "LogSortDescription TEXT,",
            "Species TEXT,",
            "LogGrade1 TEXT,",
            "LogGrade2 TEXT,",
            "LogGrade3 TEXT,",
            "LogGrade4 TEXT,",
            "LogGrade5 TEXT,",
            "LogGrade6 TEXT,",
            "SEDlimit TEXT,",
            "SEDminimum REAL,",
            "SEDmaximum REAL"
        };

        private static readonly string[] StewProductCostsColumns = new string[]
        {
            "costUnit TEXT,",
            "costSpecies TEXT,",
            "costProduct TEXT,",
            "costPounds REAL,",
            "costCost REAL,",
            "scalePC REAL,",
            "includeInReport TEXT"
        };

        public int CreateNewTable(string tableName)
        {
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            string queryString = "";
            //  build query
            if (tableName == "ExportValues")
            {
                queryString = createNewTableQuery(tableName, ExportValuesColums);
            }
            else if (tableName == "LogMatrix")
            {
                queryString = createNewTableQuery(tableName, LogMatrixColumns);
            }
            else if (tableName == "StewProductCosts")
            {
                queryString = createNewTableQuery("StewProductCosts", StewProductCostsColumns);

            }   //  endif 

            DAL.Execute(queryString);
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            // {
            //     sqlconn.Open();
            //     SQLiteCommand sqlcmd = sqlconn.CreateCommand();
            //     sqlcmd.CommandText = queryString;
            //     sqlcmd.ExecuteNonQuery();

            //     sqlconn.Close();
            // }   //  end using

            return 1;
        }   //  end createNewTable

        private string createNewTableQuery(string tableName, params string[] valuesList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(tableName);
            sb.Append(" (");

            //  add values to command
            foreach (string s in valuesList)
                sb.Append(s);

            //  add close parens and semi-colon
            sb.Append(");");

            return sb.ToString();
        }   //  end createNewTableQuery


        public bool doesTableExist(string tableName)
        {
            return DAL.CheckTableExists(tableName);
        }
    }
}
