﻿using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;

namespace CruiseProcessing
{
    public class QualityAdjMethods
    {
        public ErrorLogMethods ELM { get; }

        public QualityAdjMethods(ErrorLogMethods elm)
        {
            ELM = elm;
        }

        //  edit checks
        public int CheckEquations(List<QualityAdjEquationDO> qaeList)
        {
            int errorsFound = 0;
            foreach (QualityAdjEquationDO qed in qaeList)
            {
                if (qed.QualityAdjEq != "QUAL0391" && qed.QualityAdjEq != "QUAL0392")
                {
                    ELM.LoadError("Quality Adjustment", "E", "1", (long)qed.rowID, "QualityAdjEq");
                    errorsFound++;
                }   //  endif equation is bad
                if (qed.Coefficient1 == 0 && qed.Coefficient2 == 0 &&
                    qed.Coefficient3 == 0 && qed.Coefficient4 == 0)
                {
                    ELM.LoadError("Quality Adjustment", "E", "2", (long)qed.rowID, "Coefficient");
                    errorsFound++;
                }   //  endif coefficients are missing
            }   //  end foreach loop
            return errorsFound;
        }   //  end CheckEquations

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