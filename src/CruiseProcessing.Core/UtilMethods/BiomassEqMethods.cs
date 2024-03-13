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

        public static List<string> buildPrintArray(BiomassEquationDO bioDO, int printSpecies)
        {
            string fieldFormat = "{0,5:F1}";
            var bioArray = new List<string>();
            bioArray.Add(" ");
            if (printSpecies == 1)
            {
                bioArray.Add(bioDO.Species.PadRight(6, ' '));
                bioArray.Add(bioDO.Product.PadLeft(2, '0'));
            }
            else if (printSpecies == 0)
            {
                bioArray.Add("      ");
                bioArray.Add("  ");
            }   //  endif printSpecies
            switch (bioDO.Component)
            {
                case "TotalTreeAboveGround":
                    bioArray.Add("Total Tree        ");
                    break;

                case "LiveBranches":
                    bioArray.Add("Live Branches     ");
                    break;

                case "DeadBranches":
                    bioArray.Add("Dead Branches     ");
                    break;

                case "Foliage":
                    bioArray.Add("Foliage           ");
                    break;

                case "PrimaryProd":
                    bioArray.Add("Mainstem Primary  ");
                    break;

                case "SecondaryProd":
                    bioArray.Add("Mainstem Secondary");
                    break;

                case "StemTip":
                    bioArray.Add("Stem Tip          ");
                    break;
            }   //  end switch on component

            if (bioDO.Equation == null || bioDO.Equation == "" || bioDO.Equation.Substring(0, 3) == "   ")
                bioArray.Add("   ");
            else bioArray.Add(bioDO.Equation.PadLeft(3, '0'));
            bioArray.Add(string.Format(fieldFormat, bioDO.PercentMoisture));
            bioArray.Add(string.Format(fieldFormat, bioDO.PercentRemoved));
            bioArray.Add(bioDO.LiveDead);
            bioArray.Add(bioDO.FIAcode.ToString());
            bioArray.Add(string.Format(fieldFormat, bioDO.WeightFactorPrimary));
            bioArray.Add(string.Format(fieldFormat, bioDO.WeightFactorSecondary));
            if (bioDO.MetaData == null)
                bioArray.Add("  ");
            else bioArray.Add(bioDO.MetaData.PadRight(49, ' ').Substring(0, 49));

            return bioArray;
        }   //  end buildPrintArray
    }
}