using CruiseDAL.DataObjects;
using System.Collections.Generic;

namespace CruiseProcessing
{
    public static class BiomassEqMethods
    {
        public static void FindFactor(List<BiomassEquationDO> bioList, string currSP, string currCP, string currPR,
                                    string currLD, ref float currPCR, ref float currWF)
        {
            //  find weight factor etc for current group
            int nthRow = bioList.FindIndex(
                delegate (BiomassEquationDO bedo)
                {
                    return bedo.Species == currSP && bedo.Product == currPR &&
                           bedo.Component == currCP && bedo.LiveDead == currLD;
                });
            if (nthRow >= 0)
            {
                currPCR = bioList[nthRow].PercentRemoved;
                if (currCP == "PrimaryProd")
                    currWF = bioList[nthRow].WeightFactorPrimary;
                else if (currCP == "SecondaryProd")
                    currWF = bioList[nthRow].WeightFactorSecondary;
            }   //  endif nthRow
            return;
        }   //  end FindFactor


    }
}