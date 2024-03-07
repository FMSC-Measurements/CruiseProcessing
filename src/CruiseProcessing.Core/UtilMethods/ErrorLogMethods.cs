using CruiseDAL.DataObjects;
using CruiseProcessing.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing
{
    public class ErrorLogMethods
    {
        public List<ErrorLogDO> errList { get; } = new List<ErrorLogDO>();
        protected CPbusinessLayer DataLayer { get; }

        public ErrorLogMethods(CPbusinessLayer dataLayer)
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        public void LoadError(string nameOfTable, string errorLevel,
                                    string errorNumber, long CN_identifier, string colName)
        {
            //  need a business layer function here to load error messages
            ErrorLogDO eld = new ErrorLogDO();
            eld.TableName = nameOfTable;
            eld.Level = errorLevel;
            eld.CN_Number = CN_identifier;
            eld.ColumnName = colName;
            eld.Message = errorNumber;
            eld.Program = "CruiseProcessing";
            errList.Add(eld);
            return;
        }   //  end LoadError

        //  Method checks here since not specific to one particular table
 


    }   //  end ErrorLogMethods
}