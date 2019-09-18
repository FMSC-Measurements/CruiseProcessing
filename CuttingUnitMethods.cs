using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public static class CuttingUnitMethods
    {
        //  edit checks
        public static int IsEmpty(IEnumerable<CuttingUnitDO> cuList)
        {
            return cuList.Any() ? 0 : 25;
        }   //  end IsEmpty

        public static int ValidLogMethod(CuttingUnitDO cudo)
        {
            string[] logMethods = new string[33] { "401", "410", "420", "421", "422", "423", 
                                                   "430", "431", "432", "433", "434", "435", 
                                                   "440", "441", "442", "450", "451", "452", 
                                                   "453", "454", "460", "461", "462", "463", 
                                                   "464", "470", "480", "481", "482", "483", 
                                                   "490", "491", "492" };
            int iFound = -1;
            if (cudo.LoggingMethod == "") return 1;
            for (int k = 0; k < 33; k++)
            {
                cudo.LoggingMethod = cudo.LoggingMethod.TrimEnd(' ');
                if(cudo.LoggingMethod.Equals(logMethods[k]))
                    iFound = k;
            }   //  end for k loop
            
            return iFound;
        }   //  end ValidLogMethod


        //  methods pertaining to cutting unit table
        public static double GetUnitAcres(IEnumerable<CuttingUnitDO> CutUnitList, string currCutUnit)
        {
            //double currAcres = 1.0;
            ////List<CuttingUnitDO> rtrnList = CutUnitList.FindAll(
            ////    delegate(CuttingUnitDO cud)
            ////    {
            ////        return cud.Code == currCutUnit;
            ////    });
            //var rtrnList = CutUnitList.FirstOrDefault(cu => cu.Code == currCutUnit);
            //if(rtrnList != null)
            //    currAcres = Convert.ToDouble(rtrnList.Area);

            return CutUnitList.FirstOrDefault(cu => cu.Code == currCutUnit)?.Area ?? 1d;
        }   //  end GetUnitAcres


        public static ArrayList buildPrintArray(CuttingUnitDO cul, string cruiseName)
        {
            ArrayList cutUnitArray = new ArrayList();
            cutUnitArray.Add("   ");
            cutUnitArray.Add(cruiseName.PadRight(5, ' '));
            cutUnitArray.Add(cul.Code.PadLeft(3, ' '));
            cutUnitArray.Add(Utilities.FormatField(cul.Area,"{0,7:F2}").ToString().PadLeft(7, ' '));
            cutUnitArray.Add(cul.Description??(" ").PadRight(25, ' '));
            cutUnitArray.Add(cul.LoggingMethod??(" ").PadRight(3, ' '));
            cutUnitArray.Add(cul.PaymentUnit??(" ").PadLeft(4, ' '));

            return cutUnitArray;
        }   //  end buildPrintArray

        //  overloaded to build line for payment unit page in A01 report
        public static ArrayList buildPrintArray(CuttingUnitDO cud, string cruiseName, string currentStratum)
        {
            ArrayList currArray = new ArrayList();
            currArray.Add("     ");
            currArray.Add(cruiseName.PadRight(5,' '));
            currArray.Add(cud.Code.PadLeft(3,' '));
            currArray.Add(currentStratum.PadLeft(2,' '));
            if (cud.PaymentUnit == null)
                currArray.Add("   ");
            else currArray.Add(cud.PaymentUnit.PadLeft(3,' '));
            return currArray;
        }   // buildPrintArray

    }
}
