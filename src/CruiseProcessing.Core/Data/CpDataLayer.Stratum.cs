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

        public List<StratumDO> GetStrata()
        {
            return DAL.From<StratumDO>().OrderBy("Code").Read().ToList();
        }   //  end getStratum

        public IEnumerable<string> GetStratumCodesByUnit(string currUnit)
        {
            return DAL.QueryScalar<string>("SELECT st.Code FROM CuttingUnitStratum AS cust JOIN CuttingUnit AS cu USING (CuttingUnit_CN) JOIN Stratum AS st USING (Stratum_CN) WHERE cu.Code = @p1", currUnit);
        }

        public List<StratumDO> GetCurrentStratum(string currentStratum)
        {
            //  Pull current stratum from table
            return DAL.From<StratumDO>().Where("Code = @p1").Read(currentStratum).ToList();
        }   //  end GetCurrentStratum


        //  just FIXCNT methods
        public List<StratumDO> justFIXCNTstrata()
        {
            return DAL.From<StratumDO>().Where("Method = @p1").Read("FIXCNT").ToList();
        }   //  end justFIXCNTstrata
    }
}
