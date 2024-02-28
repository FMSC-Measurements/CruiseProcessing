using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class ValueEqMethods
    {
        public ErrorLogMethods ELM { get; }

        public ValueEqMethods(ErrorLogMethods elm)
        {
            ELM = elm;
        }

        //  edit checks
        public int CheckEquations(List<ValueEquationDO> valList, string currRegion)
        {
            //  Per K.Andregg some time ago, value is not used in Region 8
            string[] R1equations = new string[8] {"VLPP0101", "VLPP0102", "VLPP0103", "VLPP0104",
                                                      "VLPP0106", "VLPP0107", "VLPP0110", "VLPP0112"};
            string[] R2equations = new string[8] {"VLPP0201", "VLPP0202", "VLPP0203", "VLPP0204",
                                                      "VLPP0206", "VLPP0207", "VLPP0210", "VLPP0212"};
            string[] R3equations = new string[8] {"VLPP0301", "VLPP0302", "VLPP0303", "VLPP0304",
                                                      "VLPP0306", "VLPP0307", "VLPP0310", "VLPP0312"};
            string[] R4equations = new string[4] { "VLPP0401", "VLPP0402", "VLPP0403", "VLPP0404" };
            string[] R5equations = new string[8] {"VLPP0501", "VLPP0502", "VLPP0503", "VLPP0504",
                                                      "VLPP0506", "VLPP0507", "VLPP0510", "VLPP0512"};
            string[] R8equations = new string[8] {"VLPP0801", "VLPP0802", "VLPP0803", "VLPP0804",
                                                    "VLPP0806", "VLPP0807", "VLPP0810", "VLPP0812"};
            string[] R9equations = new string[14] {"VLPP0901", "VLPP0902", "VLPP0903", "VLPP0904",
                                                       "VLPP0905", "VLPP0906", "VLPP0907", "VLPP0908",
                                                       "VLPP0909", "VLPP0910", "VLPP0911", "VLPP0912",
                                                       "VLPP0913", "VLPP0914"};
            string[] R10equations = new string[8] {"VLPP0001", "VLPP0002", "VLPP0003", "VLPP0004",
                                                       "VLPP0006", "VLPP0007", "VLPP0010", "VLPP0012"};
            int errorsFound = 0;
            bool badEquation = false;
            bool badCoefficient = false;
            foreach (ValueEquationDO val in valList)
            {
                switch (currRegion)
                {
                    case "01":
                    case "1":
                        if (R1equations.Contains(val.ValueEquationNumber) == false)
                            badEquation = true;
                        if (val.Coefficient1 == 0 && val.Coefficient2 == 0)
                            badCoefficient = true;
                        break;

                    case "02":
                    case "2":
                        if (R2equations.Contains(val.ValueEquationNumber) == false)
                            badEquation = true;
                        if (val.Coefficient1 == 0 && val.Coefficient2 == 0)
                            badCoefficient = true;
                        break;

                    case "03":
                    case "3":
                        if (R3equations.Contains(val.ValueEquationNumber) == false)
                            badEquation = true;
                        if (val.Coefficient1 == 0 && val.Coefficient2 == 0)
                            badCoefficient = true;
                        break;

                    case "04":
                    case "4":
                        if (R4equations.Contains(val.ValueEquationNumber) == false)
                            badEquation = true;
                        if (val.Coefficient1 == 0 && val.Coefficient2 == 0 &&
                            val.Coefficient3 == 0 && val.Coefficient4 == 0)
                            badCoefficient = true;
                        break;

                    case "05":
                    case "5":
                        if (R5equations.Contains(val.ValueEquationNumber) == false)
                            badEquation = true;
                        if (val.Coefficient1 == 0 && val.Coefficient2 == 0)
                            badCoefficient = true;
                        break;

                    case "08":
                    case "8":
                        if (R8equations.Contains(val.ValueEquationNumber) == false)
                            badEquation = true;
                        if (val.Coefficient1 == 0 && val.Coefficient2 == 0)
                            badCoefficient = true;
                        break;

                    case "09":
                    case "9":
                        if (R9equations.Contains(val.ValueEquationNumber) == false)
                            badEquation = true;
                        if (val.Coefficient1 == 0 && val.Coefficient2 == 0)
                            badCoefficient = true;
                        break;

                    case "10":
                        if (R10equations.Contains(val.ValueEquationNumber) == false)
                            badEquation = true;
                        if (val.Coefficient1 == 0 && val.Coefficient2 == 0)
                            badCoefficient = true;
                        break;
                }   //  end switch on region

                if (val.ValueEquationNumber == null)
                    badEquation = true;

                if (badEquation)
                {
                    ELM.LoadError("ValueEquation", "E", "1", (long)val.rowID, "ValueEquationNumber");
                    errorsFound++;
                    badEquation = false;
                }   //  endif bad equation
                if (badCoefficient)
                {
                    ELM.LoadError("ValueEquation", "E", "2", (long)val.rowID, "Coefficient");
                    errorsFound++;
                    badCoefficient = false;
                }   //  endif badCoefficient
            }   //  end foreach loop
            return errorsFound;
        }   //  end CheckEquations

        public static List<string> buildPrintArray(ValueEquationDO val)
        {
            StringBuilder sb = new StringBuilder();
            string fieldFormat = "{0,11:F7}"; ;
            var valArray = new List<string>();
            valArray.Add(" ");
            valArray.Add(val.Species.PadRight(6, ' '));
            valArray.Add(val.PrimaryProduct.PadLeft(2, '0'));
            valArray.Add(val.Grade);
            valArray.Add(val.ValueEquationNumber);

            //  coefficients
            valArray.Add(String.Format(fieldFormat, val.Coefficient1));
            valArray.Add(String.Format(fieldFormat, val.Coefficient2));
            valArray.Add(String.Format(fieldFormat, val.Coefficient3));
            valArray.Add(String.Format(fieldFormat, val.Coefficient4));
            valArray.Add(String.Format(fieldFormat, val.Coefficient5));
            valArray.Add(String.Format(fieldFormat, val.Coefficient6));
            return valArray;
        }   //  end buildPrintArray
    }
}