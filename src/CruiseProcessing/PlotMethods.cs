using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public static class PlotMethods
    {
        //  methods pertaining to the plot table
        public static List<PlotDO> GetStrata(List<PlotDO> pList, string currST)
        {
            List<PlotDO> rtrnList = pList.FindAll(
                delegate(PlotDO pd)
                {
                    return pd.Stratum.Code == currST;
                });
            return rtrnList;
        }   //  end GetStrata


        public static int FindDuplicatePlots(List<PlotDO> pList, string currST, string currCU, long currPL)
        {
            List<PlotDO> rtrnList = pList.FindAll(
                delegate(PlotDO pd)
                {
                    return pd.PlotNumber == currPL && pd.CuttingUnit.Code == currCU && pd.Stratum.Code == currST;
                });
            if (rtrnList.Count > 1)
                return 7;
            else if (rtrnList.Count <= 0)
                return 0;

            return 0;
        }   //  end FindDuplicatePlots


        public static ArrayList buildPrintArray(PlotDO pl, string cruiseName, string stratumCode, 
                                                    string unitCode)
        {
            ArrayList plotArray = new ArrayList();
            plotArray.Add("     ");
            plotArray.Add(cruiseName.PadRight(5, ' '));
            plotArray.Add(pl.PlotNumber.ToString().PadLeft(4, ' '));
            plotArray.Add(unitCode.PadLeft(3, ' '));
            plotArray.Add(stratumCode.PadLeft(2, ' '));
            plotArray.Add(pl.Slope.ToString().PadLeft(6,' '));
            plotArray.Add(pl.Aspect.ToString().PadLeft(3,' '));
            plotArray.Add(pl.KPI.ToString().PadLeft(6, ' '));
            if (pl.IsEmpty == "1" || pl.IsEmpty == "True")
                plotArray.Add("YES");
            else plotArray.Add("   ");

            return plotArray;
        }   //  end buildPrintArray

        public static ArrayList buildPrintArray(PlotDO pl, string stratumCode, string unitCode)
        {
            //  overloaded to build array for report A13 (A14) -- plot page
            ArrayList plotArray = new ArrayList();
            string fieldFormat1 = "{0,10:F2}";
            string fieldFormat2 = "{0,9:F2}";
          
            //  print plot table
            if (pl.XCoordinate != 0.0)
            {
                plotArray.Add("");
                plotArray.Add(pl.PlotNumber.ToString().PadLeft(4, ' '));
                plotArray.Add(unitCode.PadLeft(3, ' '));
                plotArray.Add(stratumCode.PadLeft(2, ' '));
                plotArray.Add(Utilities.Format(fieldFormat1, pl.XCoordinate).ToString());
                if (pl.YCoordinate == 0.0)
                    plotArray.Add("---------");
                else plotArray.Add(Utilities.Format(fieldFormat2, pl.YCoordinate).ToString());
                if (pl.ZCoordinate == 0.0)
                    plotArray.Add("---------");
                else plotArray.Add(Utilities.Format(fieldFormat1, pl.ZCoordinate).ToString());
                plotArray.Add(pl.MetaData??(" "));
            }   //  endif coordinates exist
            return plotArray;
        }   //  end buildPrintArray

    }
}
