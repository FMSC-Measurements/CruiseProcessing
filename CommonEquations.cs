using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;

namespace CruiseProcessing
{
    public static class CommonEquations
    {

        public static int CalculateDiameterClass(float diameterValue)
        {
            return (int)Math.Round(diameterValue);
        }   //  end CalculateDiameterClass


        public static double ExpandedValue(float value1, float value2, float value3)
        {
            //  returns the expanded value
            return value1 * value2 * value3;
        }   //  end ExpandedValue


        public static double SumList(double[] listToSum)
        {
            //  Sums the double field to sum
            double summedValue = 0.0;
            foreach (double item in listToSum)
                summedValue += item;

            return summedValue;
        }   //  end SumList



        public static long SumList(long[] listToSum)
        {
            //  Sums long field and returns long
            long summedValue = 0;
            foreach (long item in listToSum)
                summedValue += item;
            return summedValue;
        }   //  end SumList



        public static double BoardCubicRatio(double value1, double value2)
        {
            //  Calculates the board or cubic ratio
            if (value2 > 0)
                return value1 / value2;
            else return 0.0;
        }   //  end BoardCubicRatio


        public static double CalculateRecoverySalvage(double numerAtor, double denominAtor)
        {
            //  Calculates the recovery salvage percent
            if (denominAtor > 0)
                return (numerAtor / denominAtor) * 100;
            else return 0.0;
        }   //  end CalculateRecoverySalvage


        public static double SquaredList(double[] listToSum)
        {
            //  Calculates square value and sum field to sum
            double summedValue = 0.0;
            foreach (double item in listToSum)
                summedValue += item * item;
            return summedValue;
        }   //  end SquaredList


        public static double RoundExpansionFactor(double expFac)
        {
            double calcValue = 0.0;
            return calcValue = (int)((expFac * 1000) + 0.5) / 1000.0;
        }   //  end RoundExpansionFactor


        public static double PointSampleFrequency(double stratumBAF, float treeDBH)
        {
            //  Calculates the point sample frequency
            double denominAtor = 0.005454 * Math.Pow(treeDBH, 2.0);
            if (denominAtor > 0)
                return stratumBAF / denominAtor;
            else return 0.0;
        }   //  end PointSampleFrequency


        public static double CalcTreeFactor(double firstStageTrees, double treesMeasured, double treesTallied, 
                                        int order1, int order2)
        {
            //  Calculates the tree factor
            double calcValue = 0.0;
            calcValue = 0.0;
            if (order1 == 1 && order2 == 2)
            {
                if (treesMeasured > 0)
                    calcValue = firstStageTrees / treesMeasured;
            }
            else if (order1 == 3 && order2 == 1)
            {
                if (firstStageTrees > 0)
                    calcValue = treesTallied / firstStageTrees;
            }
            else if (order1 == 3 && order2 == 2)
            {
                if (treesMeasured > 0)
                    calcValue = treesTallied / treesMeasured;
            }

            return calcValue;
        }   //  end CalcTreeFactor


        public static double Frequency3P(double sumKPI, float treeKPI, double treesMeasured)
        {
            double calcValue = 0.0;
            //  Calculates the frequency for 3P methods
            double denominAtor = treesMeasured * treeKPI;
            if (denominAtor > 0)
                return calcValue = sumKPI / denominAtor;
            else return 0.0;
        }   //  end Frequency3P


        public static float NumberOfMeasuredPlots(string currentStratum, List<TreeDO> treeList)
        {
            //  Calculates the number of measured plots for the current stratum
            //  Business layer function -- go to stratum look up CN for currentStratum, pull all CN from trees
            //  rework as this is no longer a valid method
            List<TreeDO> plotTrees = treeList.FindAll(
                delegate(TreeDO td)
                {
                    return td.TreeNumber == 0 && td.CountOrMeasure == "M";
                });

            if (plotTrees != null)
                return plotTrees.Count();
            else return 0.0F;
        }   //  end NumberOfMeasuredPlots


        public static double NumberOfPlots(string currST, string fileName)
        {
            //  Finds total number of plots
            return Global.BL.GetStrataPlots(currST).Count();
        }   //  end NumberOfPlots


        public static int AverageDefectPercent(double value1, double value2)
        {
            //  Calculates average defect percent
            if (value1 > 0)
                return (int)(((value1 - value2) / value1) * 100);
            else return 0;
        }   //  end AverageDefectPercent


        public static double Calculate3PTrees(List<LCDDO> currentGroup, string currMeth)
        {
            //  Calculates number of trees for 3P method only
            double totalTalliedTrees = 0.0;
            double totalExpFac = 0.0;

            if (currMeth == "S3P" || currMeth == "3P")
                return currentGroup.Sum(l => l.TalliedTrees);
            else
            {
                //  sum expansion factors based on STM
                totalTalliedTrees = currentGroup.Sum(l => l.TalliedTrees);
                for (int k = 0; k < currentGroup.Count; k++)
                {
                    if (currentGroup[k].STM == "Y")
                        totalExpFac += currentGroup[k].SumExpanFactor;
                    else if (currentGroup[k].STM == "N")
                    {
                        if (currentGroup[k].SumExpanFactor > 0)
                            totalExpFac += currentGroup[k].SumExpanFactor * totalTalliedTrees / currentGroup[k].SumExpanFactor;
                        else totalExpFac += totalTalliedTrees;
                    }   //  endif
                }   //  end for k loop
                return totalExpFac;
            }   //  endif current method
        }   //  end Calculate 3PTrees


    }   //  end class CommonEquations
}
