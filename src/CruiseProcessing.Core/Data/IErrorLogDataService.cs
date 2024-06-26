using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public interface IErrorLogDataService
    {
        void LogError(string tableName, int table_CN, string errLevel, string errMessage, string columnName);

        //void LogError(string tableName, int table_CN, string errLevel, string errMessage);
    }
}
