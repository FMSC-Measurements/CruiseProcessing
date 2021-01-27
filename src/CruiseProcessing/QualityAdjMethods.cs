using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public class QualityAdjMethods
    {
        //  edit checks
        public int CheckEquations(List<QualityAdjEquationDO> qaeList)
        {
            int errorsFound = 0;
            ErrorLogMethods elm = new ErrorLogMethods();
            foreach (QualityAdjEquationDO qed in qaeList)
            {
                if (qed.QualityAdjEq != "QUAL0391" && qed.QualityAdjEq != "QUAL0392")
                {
                    elm.LoadError("Quality Adjustment", "E", "1", (long)qed.rowID, "QualityAdjEq");
                    errorsFound++;
                }   //  endif equation is bad
                if (qed.Coefficient1 == 0 && qed.Coefficient2 == 0 &&
                    qed.Coefficient3 == 0 && qed.Coefficient4 == 0)
                {
                    elm.LoadError("Quality Adjustment", "E", "2", (long)qed.rowID, "Coefficient");
                    errorsFound++;
                }   //  endif coefficients are missing
            }   //  end foreach loop
            return errorsFound;
        }   //  end CheckEquations


        public ArrayList buildPrintArray(QualityAdjEquationDO qae)
        {
            string fieldFormat = "{0,8:F4}";
            string yearFormat = "{0,0:F4}";
            ArrayList qualArray = new ArrayList();
            qualArray.Add(" ");
            qualArray.Add(qae.Species.PadRight(6, ' '));
            qualArray.Add(qae.QualityAdjEq);
            qualArray.Add(Utilities.Format(yearFormat, qae.Year).ToString());
            qualArray.Add(Utilities.Format(fieldFormat, qae.Coefficient1).ToString());
            qualArray.Add(Utilities.Format(fieldFormat, qae.Coefficient2).ToString());
            qualArray.Add(Utilities.Format(fieldFormat, qae.Coefficient3).ToString());
            qualArray.Add(Utilities.Format(fieldFormat, qae.Coefficient4).ToString());
            qualArray.Add(Utilities.Format(fieldFormat, qae.Coefficient5).ToString());
            qualArray.Add(Utilities.Format(fieldFormat, qae.Coefficient6).ToString());

            return qualArray;
        }   //  end buildPrintArray
    }
}
