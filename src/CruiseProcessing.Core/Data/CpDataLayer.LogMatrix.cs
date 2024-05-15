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
        public List<LogMatrixDO> getLogMatrix(string currReport)
        {
            return DAL.From<LogMatrixDO>().Where("ReportNumber = @p1").Read(currReport).ToList();
        }   //  end getLogMatrix

        public void SaveLogMatrix(List<LogMatrixDO> lmList, string currReport)
        {
            List<LogMatrixDO> currLogMatrix = getLogMatrix(currReport);
            //  same problem as with volume equations below.
            // need to delete current report form log matrix before saving
            if (currReport != "")
            {
                if (currLogMatrix.Count > 0)
                {
                    foreach (LogMatrixDO lmd in currLogMatrix)
                    {
                        if (lmd.ReportNumber == currReport)
                            lmd.Delete();
                    }   //  end foreach loop
                }   //  endif
            }   //  endif
            foreach (LogMatrixDO lmx in lmList)
            {
                if (lmx.DAL == null)
                {
                    lmx.DAL = DAL;
                }   //  endif
                lmx.Save();
            }   //  end foreach loop           
            return;
        }   //  end SaveLogMatrix
    }
}
