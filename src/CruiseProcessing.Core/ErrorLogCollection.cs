using CruiseDAL.DataObjects;
using System.Collections.Generic;

namespace CruiseProcessing
{
    public class ErrorLogCollection : List<ErrorLogDO>
    {
        public void AddError(string nameOfTable, string errorLevel,
                                    string errorNumber, long CN_identifier, string colName)
        {
            ErrorLogDO eld = new ErrorLogDO();
            eld.TableName = nameOfTable;
            eld.Level = errorLevel;
            eld.CN_Number = CN_identifier;
            eld.ColumnName = colName;
            eld.Message = errorNumber;
            eld.Program = "CruiseProcessing";
            Add(eld);
        }
    }
}