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
        public List<RegressionDO> getRegressionResults()
        {
            //  pulls regression data for local volume reports
            return DAL.From<RegressionDO>().Read().ToList();
        }   //  end getRegressionResults

        public void DeleteRegressions()
        {
            DAL.Execute("DELETE FROM Regression");
        }


        public void SaveRegress(List<RegressionDO> resultsList)
        {
            //  Then saves the updated regression results list
            foreach (RegressionDO rl in resultsList)
            {
                if (rl.DAL == null)
                {
                    rl.DAL = DAL;
                }
                rl.Save();
            }   //  end foreach loop
            return;

        }   //  end SaveRegress
    }
}
