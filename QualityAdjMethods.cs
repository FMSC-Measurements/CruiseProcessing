using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public static class QualityAdjMethods
    {
        //  edit checks
        public static int CheckEquations(IEnumerable<QualityAdjEquationDO> qaeList)
        {

            int errorsFound = 0;
            foreach (QualityAdjEquationDO qed in qaeList)
            {
                if (qed.QualityAdjEq != "QUAL0391" && qed.QualityAdjEq != "QUAL0392")
                {
                    ErrorLogMethods.LoadError("Quality Adjustment", "E", "1", (long)qed.rowID, "QualityAdjEq");
                    errorsFound++;
                }   //  endif equation is bad
                if (qed.Coefficient1 == 0 && qed.Coefficient2 == 0 &&
                    qed.Coefficient3 == 0 && qed.Coefficient4 == 0)
                {
                    ErrorLogMethods.LoadError("Quality Adjustment", "E", "2", (long)qed.rowID, "Coefficient");
                    errorsFound++;
                }   //  endif coefficients are missing
            }   //  end foreach loop
            return errorsFound;
        }   //  end CheckEquations


        public static ArrayList buildPrintArray(QualityAdjEquationDO qae)
        {
            string fieldFormat = "{0,8:F4}";
            string yearFormat = "{0,0:F4}";
            ArrayList qualArray = new ArrayList();
            qualArray.Add(" ");
            qualArray.Add(qae.Species.PadRight(6, ' '));
            qualArray.Add(qae.QualityAdjEq);
            qualArray.Add(Utilities.FormatField(qae.Year,yearFormat).ToString());
            qualArray.Add(Utilities.FormatField(qae.Coefficient1, fieldFormat).ToString());
            qualArray.Add(Utilities.FormatField(qae.Coefficient2, fieldFormat).ToString());
            qualArray.Add(Utilities.FormatField(qae.Coefficient3, fieldFormat).ToString());
            qualArray.Add(Utilities.FormatField(qae.Coefficient4, fieldFormat).ToString());
            qualArray.Add(Utilities.FormatField(qae.Coefficient5, fieldFormat).ToString());
            qualArray.Add(Utilities.FormatField(qae.Coefficient6, fieldFormat).ToString());

            return qualArray;
        }   //  end buildPrintArray
    }
}
