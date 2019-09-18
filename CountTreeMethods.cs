using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public static class CountTreeMethods
    {
        //  methods pertaining to count tree table
        public static IEnumerable<CountTreeDO> GetSingleValue(IEnumerable<CountTreeDO> ctList, string currSG, string currST, 
                                            string currSP, string currCU, int groupBy)
        {
            List<CountTreeDO> rtrnList = new List<CountTreeDO>();
            //  get data for stratum and cutting unit (VSM4 uses this)
            if (currST != "" && currCU != "" && currSG != "")
            {
                rtrnList = ctList.Where(ctd => ctd.SampleGroup.Stratum.Code == currST && ctd.CuttingUnit.Code == currCU &&
                            ctd.SampleGroup.Code == currSG).ToList();

                if (rtrnList != null && groupBy == 0)
                    return rtrnList;
            }   //  endif currST

            return rtrnList;
        }   //  end GetSingleValue

        public static int check3Pcounts(IEnumerable<CountTreeDO> countList, StratumDO currStratum)
        {
            int checkResult = 0;
            //  is this a 3P stratum?
            if (currStratum.Method == "3P")
            {
                //  find stratum in CountTree by finding cutting unit first
                //List<CuttingUnitStratumDO> uList = ;
                foreach (CuttingUnitStratumDO ul in Global.BL.getCuttingUnitStratum((long)currStratum.Stratum_CN))
                {
//                    List<CountTreeDO> justCounts = ;

                    foreach (CountTreeDO jc in Global.BL.getJustCounts((long)ul.CuttingUnit_CN, currStratum.Code))
                        if (jc.TreeDefaultValue_CN == null)
                            checkResult++;
                }   //  end foreach on cutting unit
            }   //  endif 3P stratum
            return checkResult;
        }   //  end check3Pcounts


        public static ArrayList buildPrintArray(CountTreeDO cdo, string stratumCode)
        {
            ArrayList countList = new ArrayList();
            countList.Add(" ");
            countList.Add(cdo.CuttingUnit.Code.PadLeft(3,' '));
            countList.Add(stratumCode.PadLeft(2,' '));
            countList.Add(cdo.SampleGroup.Code.PadLeft(2,' '));
            if (cdo.TreeDefaultValue == null)
                countList.Add("      ");
            else countList.Add(cdo.TreeDefaultValue.Species.PadRight(6, ' '));
            countList.Add(cdo.SampleGroup.SamplingFrequency.ToString().PadLeft(4,' '));
            countList.Add(cdo.SampleGroup.KZ.ToString().PadLeft(5,' '));
            countList.Add(cdo.TreeCount.ToString().PadLeft(5,' '));
            countList.Add(cdo.SumKPI.ToString().PadLeft(6, ' '));
            if (cdo.Tally == null)
                countList.Add(" ");
            else countList.Add(cdo.Tally.Description);

            return countList;
        }   //  end buildPrintArray

    }   //  end CountTreeMethods
}
