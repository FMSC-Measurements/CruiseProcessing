using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public static class WeightEqMethods
    {
        //  edit checks
        public static int MatchSpeciesProduct(List<WeightEquationDO> wgtList, string[,] justSpPr,
                                                ArrayList justTreeSp)
        {
            int errorsFound = 0;
            //  find each species or species/product combination in weight equations
            int nthRow = -1;
            for (int k = 0; k < justSpPr.Length; k++)
            {
                nthRow = wgtList.FindIndex(
                    delegate(WeightEquationDO wed)
                    {
                        return wed.Species == justSpPr[k, 0] && wed.PrimaryProduct == justSpPr[k, 1];
                    });
                if (nthRow == -1)        //  species/product not found in weight equations
                {
                    ErrorLogMethods.LoadError("TreeDefaultValue", "E", "18", k);
                    errorsFound++;
                }
                else nthRow = -1;
            }   //  end for k loop

            //  Find any species from tree table not in weight equations (user could have overridden default value)
            nthRow = -1;
            for (int k = 0; k < justTreeSp.Count; k++)
            {
                nthRow = wgtList.FindIndex(
                    delegate(WeightEquationDO wed)
                    {
                        return wed.Species == justTreeSp[k].ToString();
                    });
                if (nthRow == -1)
                {
                    ErrorLogMethods.LoadError("Tree", "E", "43", k);
                    errorsFound++;
                }
                else nthRow = -1;
            }   //  end for k loop
            return errorsFound;
        }   //  end MatchSpeciesProduct


        public static int CheckEquations(List<WeightEquationDO> wgtList)
        {
            int errorsFound = 0;
            foreach (WeightEquationDO wed in wgtList)
            {
                if (wed.WeightFactorPrimary == 0 && wed.PercentRemovedPrimary == 0)
                {
                    ErrorLogMethods.LoadError("Weight", "E", "2", wed.rowID);
                    errorsFound++;
                }   //  endif factors are zero

                //  duplicates?
                List<WeightEquationDO> dupEquations = wgtList.FindAll(
                    delegate(WeightEquationDO wedo)
                    {
                        return wedo.Species == wed.Species && wedo.LiveDead == wed.LiveDead && wedo.PrimaryProduct == wed.PrimaryProduct;
                    });
                if (dupEquations.Count > 1)
                {
                    ErrorLogMethods.LoadError("Weight", "E", "7", wed.rowID);
                    errorsFound++;
                }   //  endif dupEquations
            }   //  end foreach loop

            return errorsFound;
        }   //  end CheckEquations



        public static string[] buildPrintArray(WeightEquationDO wel)
        {
            string fieldFormat = "{0,5:F2}";
            string[] wgtArray = new string[10];
            wgtArray[0] = " ";
            wgtArray[1] = wel.Species.PadRight(6, ' ');
            wgtArray[2] = "GENPP004";
            wgtArray[3] = wel.LiveDead.PadLeft(1, ' ');
            wgtArray[4] = wel.PrimaryProduct.PadLeft(2, '0');
            wgtArray[5] = Utilities.FormatField((float)wel.WeightFactorPrimary, fieldFormat).ToString().PadLeft(6,' ');
            wgtArray[6] = Utilities.FormatField((float)wel.PercentRemovedPrimary, fieldFormat).ToString().PadLeft(6, ' ');
            if (wel.SecondaryProduct != null)
                wgtArray[7] = wel.SecondaryProduct.PadLeft(2, ' ');
            else wgtArray[7] = "  ";
            if (wel.WeightFactorSecondary != null)
                wgtArray[8] = Utilities.FormatField((float)wel.WeightFactorSecondary, fieldFormat).ToString().PadLeft(6, ' ');
            else wgtArray[8] = "   0.0";
            if (wel.PercentRemovedSecondary != null)
                wgtArray[9] = Utilities.FormatField((float)wel.PercentRemovedSecondary, fieldFormat).ToString().PadLeft(6, ' ');
            else wgtArray[9] = "   0.0";

            return wgtArray;
        }   //  end buildPrintArray
    }
}
