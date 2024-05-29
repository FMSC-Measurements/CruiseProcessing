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

        public List<PRODO> getPRO()
        {
            return DAL.From<PRODO>().Read().ToList();
        }   //  end getPRO


        public List<PRODO> getPROunit(string currCU)
        {
            return DAL.From<PRODO>().Where("CuttingUnit = @p1").Read(currCU).ToList();
        }   //  end getPRO

        public void SavePRO(List<PRODO> PROlist)
        {
            foreach (PRODO DOpro in PROlist)
            {
                if (DOpro.DAL == null)
                {
                    DOpro.DAL = DAL;
                }
                DOpro.Save();
            }   //  end foreach loop
            return;
        }   //  end SavePRO

        public void DeletePRO()
        {
            DAL.Execute("DELETE FROM PRO WHERE PRO_CN>0");
        }
    }
}
