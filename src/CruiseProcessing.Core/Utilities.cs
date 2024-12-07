using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseProcessing.Data;

namespace CruiseProcessing
{
    public static class Utilities
    {

        //  AcresLookup will go away once the updated ReturnCorrectAcres is tested and complete
        public static double AcresLookup(long currStrCN, CpDataLayer bslyr, string currStratum)
        {
            //  Need call to CP business layer to get all CUs for this stratum
            var stratum = bslyr.GetStratum(currStratum);

            float stratumAcres = 0;
            stratum.CuttingUnits.Populate();
            foreach (CuttingUnitDO cudo in stratum.CuttingUnits)
            {
                stratumAcres += cudo.Area;
            }
            return stratumAcres;
        }   //  end AcresLookup


        public static double ReturnCorrectAcres(string currentStratum, CpDataLayer bslyr, long currStrCN)
        {
            string currentMethod = MethodLookup(currentStratum, bslyr);
            if (currentMethod == "100" || currentMethod == "STR" ||
                currentMethod == "S3P" || currentMethod == "3P")
                return 1.0;
            return AcresLookup(currStrCN, bslyr, currentStratum);
        }   //  end ReturnCorrectAcres

/*  following code is for change to capture stratum acres and needs testing when DAL is updated -- April 2015
        public static double ReturnCorrectAcres(StratumDO cs, CPbusinessLayer bslyr)
        {
            double strataAcres = 0;
            if (cs.Method == "100" || cs.Method == "STR" ||
                cs.Method == "S3P" || cs.Method == "3P")
                return 1.0;
            else
            {
                //  pull units for current stratum
                List<CuttingUnitStratumDO> justStratum = bslyr.GetJustStratum(cs.Stratum_CN);
                //  sum area
                strataAcres = justStratum.Sum(j => j.StrataArea);
                if (strataAcres == 0)
                {
                    //  find acres for all units
                    strataAcres = unitAcres(justStratum, gbslyr);
                    return strataAcres;
                }
                else if (strataAcres > 0)
                    return strataAcres;
            }   //  endif
            return strataAcres;
        }   //  end ReturnCorrectAcres
        */

        public static double unitAcres(List<CuttingUnitStratumDO> justStratum, CpDataLayer bslyr)
        {
            List<CuttingUnitDO> cutList = bslyr.getCuttingUnits();
            double acresSum = 0;

            foreach (CuttingUnitStratumDO js in justStratum)
            {
                int nthRow = cutList.FindIndex(
                    delegate(CuttingUnitDO c)
                    {
                        return c.CuttingUnit_CN == js.CuttingUnit_CN;
                    });
                if (nthRow >= 0)
                    acresSum += cutList[nthRow].Area;
            }  //  end foreach loop

            return acresSum;
        }   //  end unitAcres


        public static string MethodLookup(string currentStratum, CpDataLayer bslyr)
        {
            //  Need to call CP business layer to get stratum list
            List<StratumDO> stratumList = bslyr.GetStratum(currentStratum);
            if (stratumList.Count > 0)
                return stratumList[0].Method;
            else return "";            
        }   //  end MethodLookup

        


        public static bool IsFileInUse(string fileToCheck)
        {
            FileStream fs = null;
            
            try
            {
                fs = File.Create(fileToCheck);
            }
            catch (IOException e)
            {
                string nResult = e.GetBaseException().ToString();
                //  the file is unavailable because it is
                //  still being written to
                //  or being processed by another thread
                //  or does not exist
                return true;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return false;
        }   //  end IsFileInUse
    }
}
