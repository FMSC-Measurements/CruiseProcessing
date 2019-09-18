using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;

namespace CruiseProcessing
{
    public static class CommonStatistics
    {
        public static double MeanOfX(double numerAtor, double denominAtor)
        {
            //  Calculates the mean of X
            if (denominAtor > 0)
                return numerAtor / denominAtor;
            else return 0.0;
        }   //  end MeanOfX


        public static double CoeffVariation(double currentMean, double currentSD)
        {
            //  Calculates the coefficient of variation
            if(currentMean > 0)
                return currentSD/currentMean * 100;
            else return 0.0;
        }   //  end CoeffVariation


        public static double SampleError(double currentMean, double currentSE, double Tvalue)
        {
            //  Calculates the sample error
            if (currentMean > 0)
                return currentSE / currentMean * 100 * Tvalue;
            else return 0.0;
        }   //  end SampleError


        public static double CombinedSamplingError(string currentMethod, double sampErrStage1, double sampErrStage2)
        {
            //  Calculates combined sampling error based on cruise method
            if (currentMethod == "100")
                return 0.0;
            else if (currentMethod == "S3P" || currentMethod == "F3P" ||
                    currentMethod == "P3P" || currentMethod == "PCMTRE" ||
                    currentMethod == "3PPNT" || currentMethod == "FCM" ||
                    currentMethod == "PCM")
                return Math.Sqrt(Math.Pow(sampErrStage1, 2.0) + Math.Pow(sampErrStage2, 2.0));
            else return sampErrStage1;
        }   //  end CombinedSamplingError


        public static double CalculateConfidence(double value1, double value2, string upperLower)
        {
            //  Calculates upper or lower confidence limit
            if (upperLower == "+")
                return value1 + value1 * value2 / 100;
            else if (upperLower == "-")
                return value1 - value1 * value2 / 100;
            return 0.0;
        }   //  end CalculateConfidence


        public static double StdDeviation(double value1, double value2, float value3)
        {
            //  Calculates the standard deviation
            double radCan = 0.0;
            if (value3 > 0)
                radCan = (value1 - (Math.Pow(value2, 2.0) / value3)) / (value3 - 1);

            if (radCan > 0)
                return Math.Sqrt(radCan);
            else return 0.0;
        }   //  end StdDeviation


        public static double StdError(double value1, double value2, float value3, string currentMethod, 
                                float BigN, int whatStage)
        {
            //  Calculates standard error
            double radCan = 0.0;
            if (value3 > 0)
                radCan = (value1 - (Math.Pow(value2, 2.0)) / value3) / ((value3 - 1) * value3);
            if (whatStage == 1)
            {
                if (currentMethod == "100")
                    radCan = 0.0;
                else if (currentMethod == "STR")
                {
                    if (BigN > 0)
                        radCan = radCan * (1 - (value3 / BigN));
                }   //  endif currentMethod
            }   //  endif whatStage

            if (radCan < 0.0) radCan = 0.0;
            return Math.Sqrt(radCan);
        }   //  end StdError


        public static double LookUpT(int degreesFreedom)
        {
            double[] Ttable = new double[30] {12.706,4.303,3.182,2.776,2.571,2.447,
                                                2.365,2.306,2.262,2.228,2.201,2.179,
                                                2.160,2.145,2.131,2.120,2.110,2.101,
                                                2.093,2.086,2.080,2.074,2.069,2.064,
                                                2.060,2.056,2.052,2.048,2.045,2.042};
            //  Adjust degrees of freedom for a zero-based array index
            degreesFreedom--;
            if (degreesFreedom > 29)
                return 2.0;
            else if (degreesFreedom < 0)
                return 0.0;
            else return Ttable[degreesFreedom];
        }   //  end LookUpT

/*  not quite sure yet how to get this to work
        public double CalculateMean(List<TreeDO> treeList, double summedEF, int sumType)
        {
            //  Calculate regular mean
            double summedValues = 0.0;

            if (sumType == 1)    // int
            {
                foreach (int item in treeList)
                    summedValues += item;
            }
            else if (sumType == 2)   //  float
            {
                foreach (float item in treeList)
                    summedValues += item;
            }
            else if (sumType == 3)   //  double
            {
                foreach (double item in treeList)
                    summedValues += item;
            }   //  endif sumType

            if (summedEF > 0)
                return summedValues / summedEF;
            else return 0.0;
        }   //  end CalculateMean
*/
    }   //  end class CommonStatistics
}
