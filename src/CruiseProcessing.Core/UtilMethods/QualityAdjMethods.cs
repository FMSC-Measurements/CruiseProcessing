using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;

namespace CruiseProcessing
{
    public class QualityAdjMethods
    {
        public static List<string> buildPrintArray(QualityAdjEquationDO qae)
        {
            string fieldFormat = "{0,8:F4}";
            string yearFormat = "{0,0:F4}";
            var qualArray = new List<string>();
            qualArray.Add(" ");
            qualArray.Add(qae.Species.PadRight(6, ' '));
            qualArray.Add(qae.QualityAdjEq);
            qualArray.Add(String.Format(yearFormat, qae.Year));
            qualArray.Add(String.Format(fieldFormat, qae.Coefficient1));
            qualArray.Add(String.Format(fieldFormat, qae.Coefficient2));
            qualArray.Add(String.Format(fieldFormat, qae.Coefficient3));
            qualArray.Add(String.Format(fieldFormat, qae.Coefficient4));
            qualArray.Add(String.Format(fieldFormat, qae.Coefficient5));
            qualArray.Add(String.Format(fieldFormat, qae.Coefficient6));

            return qualArray;
        }   //  end buildPrintArray
    }
}