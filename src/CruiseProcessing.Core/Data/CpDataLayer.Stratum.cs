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

        public StratumDO GetStrtum(int stratum_CN)
        {
            return DAL.From<StratumDO>().Where("Stratum_CN = @p1").Query(stratum_CN).FirstOrDefault();
        }

        public List<StratumDO> GetStrata()
        {
            return DAL.From<StratumDO>().OrderBy("Code").Read().ToList();
        }   //  end getStratum

        public IEnumerable<string> GetStratumCodesByUnit(string currUnit)
        {
            return DAL.QueryScalar<string>("SELECT st.Code FROM CuttingUnitStratum AS cust JOIN CuttingUnit AS cu USING (CuttingUnit_CN) JOIN Stratum AS st USING (Stratum_CN) WHERE cu.Code = @p1", currUnit);
        }

        public StratumDO GetStratum(string stratumCode)
        {
            return DAL.From<StratumDO>().Where("Code = @p1").Read(stratumCode).FirstOrDefault();
        }

        public string GetCruiseMethod(string stratumCode)
        {
            return DAL.ExecuteScalar<string>("SELECT Method FROM Stratum WHERE Code = @p1", stratumCode);
        }

        public double GetStratumAcres(string stratumCode)
        {
            return DAL.ExecuteScalar<double>("SELECT SUM(cu.Area) FROM CuttingUnit AS cu JOIN CuttingUnitStratum AS cust USING (CuttingUnit_CN) JOIN Stratum AS st USING (Stratum_CN) WHERE st.Code = @p1"
                , stratumCode);
        }

        public double GetStratumAcresCorrected(string stratumCode)
        {
            var method = GetCruiseMethod(stratumCode);
            if (method == "100"
                || method == "STR"
                || method == "S3P"
                || method == "3P")
            { return 1.0; }

            return GetStratumAcres(stratumCode);
        }


        //  just FIXCNT methods
        public List<StratumDO> justFIXCNTstrata()
        {
            return DAL.From<StratumDO>().Where("Method = @p1").Read("FIXCNT").ToList();
        }   //  end justFIXCNTstrata
    }
}
