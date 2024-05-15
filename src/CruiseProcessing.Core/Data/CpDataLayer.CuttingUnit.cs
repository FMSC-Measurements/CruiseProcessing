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
        public List<CuttingUnitDO> getCuttingUnits()
        {
            return DAL.From<CuttingUnitDO>().OrderBy("Code").Read().ToList();
        }   //  end getCuttingUnits

        public List<CuttingUnitDO> getPaymentUnits()
        {
            return DAL.From<CuttingUnitDO>().GroupBy("PaymentUnit").Read().ToList();
        }   //  end getPaymentUnits

        public List<CuttingUnitDO> getLoggingMethods()
        {
            //  returns unique logging methods
            return DAL.From<CuttingUnitDO>().GroupBy("LoggingMethod").Read().ToList();
        }   //  end getLoggingMethods
    }
}
