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

        public List<ErrorLogDO> getErrorMessages(string errLevel, string errProgram)
        {
            List<ErrorLogDO> errLog = new List<ErrorLogDO>();
            if (errProgram == "FScruiser")
                return DAL.From<ErrorLogDO>()
                    .Where("Level = @p1 AND (Program LIKE'%FScruiser%' OR Program LIKE '%CruiseSystemManager%' OR Program LIKE '%CruiseManage%')")
                    .Read(errLevel)
                    .ToList();
            else if (errProgram == "CruiseProcessing")
                return DAL.From<ErrorLogDO>()
                    .Where("Level = @p1 AND Program = @p2")
                    .Read(errLevel, errProgram)
                    .ToList();

            return errLog;
        }   //  end getErrorMessages

        public void DeleteCruiseProcessingErrorMessages()
        {
            //  deletes warnings and errors messages just for CruiseProcessing
            DAL.Execute("DELETE FROM ErrorLog WHERE Program = 'CruiseProcessing'");
        }   //  end deleteErrorMessages

        public void SaveErrorMessages(List<ErrorLogDO> errList)
        {
            // rewritten DEC 2020 - Ben
            foreach (ErrorLogDO eld in errList)
            {

                if (eld.DAL == null)
                { eld.DAL = DAL; }


                //  February 2015 -- due to constraining levels in the error log table
                //  duplicates are not allowed.  for example, if a Region 8 volume equation
                //  has two errors on the same tree (BDFT and CUFT), only one error is allowed
                //  to be logged in the table
                //  so we will ignore uniqe errors when saving
                try
                {
                    eld.Save();
                }
                catch (FMSC.ORM.UniqueConstraintException)
                { }
            }   //  end foreach loop

            return;
        }   //  end SaveErrorMessages

        public void LogError(string tableName, int table_CN, string errLevel, string errMessage)
        {
            List<ErrorLogDO> errList = new List<ErrorLogDO>();
            ErrorLogDO eldo = new ErrorLogDO();

            eldo.TableName = tableName;
            eldo.CN_Number = table_CN;
            eldo.Level = errLevel;
            eldo.ColumnName = "Volume";
            eldo.Message = errMessage;
            eldo.Program = "CruiseProcessing";
            errList.Add(eldo);

            SaveErrorMessages(errList);
        }
    }
}
