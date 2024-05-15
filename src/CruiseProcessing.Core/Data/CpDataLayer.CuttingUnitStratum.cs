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
        public List<CuttingUnitStratumDO> getCuttingUnitStratum(long currStratumCN)
        {
            return DAL.From<CuttingUnitStratumDO>().Where("Stratum_CN = @p1").Read(currStratumCN).ToList();
        }   //  end getCuttingUnitStratum

        public string[,] GetStrataUnits(long currST_CN)
        {
            // TODO optimize

            //  Get all units for the current stratum
            int nthRow = 0;
            string[,] unitAcres = new string[25, 2];

            List<StratumDO> stList = DAL.From<StratumDO>().Where("Stratum_CN = @p1").Read(currST_CN).ToList();
            foreach (StratumDO sd in stList)
            {
                sd.CuttingUnits.Populate();
                foreach (CuttingUnitDO cud in sd.CuttingUnits)
                {
                    if (nthRow < 25)
                    {
                        unitAcres[nthRow, 0] = cud.Code;
                        unitAcres[nthRow, 1] = cud.Area.ToString();
                        nthRow++;
                    }   //  endif nthRow
                }   //  end foreach loop on cutting units
            }   //  end foreach loop on strata
            return unitAcres;
        }   //  end GetStrataUnits
    }
}
