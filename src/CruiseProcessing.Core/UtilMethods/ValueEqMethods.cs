using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class ValueEqMethods
    {
        //  Per K.Andregg some time ago, value is not used in Region 8
        static readonly string[] R1equations = new string[8] {
            "VLPP0101", 
            "VLPP0102", 
            "VLPP0103", 
            "VLPP0104",                                   
            "VLPP0106", 
            "VLPP0107", 
            "VLPP0110", 
            "VLPP0112"
        };
        static readonly string[] R2equations = new string[8] 
        {
            "VLPP0201", 
            "VLPP0202", 
            "VLPP0203", 
            "VLPP0204",                          
            "VLPP0206", 
            "VLPP0207", 
            "VLPP0210", 
            "VLPP0212"
        };
        static readonly string[] R3equations = new string[8] {
            "VLPP0301", 
            "VLPP0302", 
            "VLPP0303", 
            "VLPP0304",                           
            "VLPP0306", 
            "VLPP0307", 
            "VLPP0310", 
            "VLPP0312"
        };
        static readonly string[] R4equations = new string[4] { 
            "VLPP0401", 
            "VLPP0402", 
            "VLPP0403", 
            "VLPP0404" 
        };
        static readonly string[] R5equations = new string[8] {
            "VLPP0501", 
            "VLPP0502", 
            "VLPP0503", 
            "VLPP0504",                            
            "VLPP0506", 
            "VLPP0507", 
            "VLPP0510", 
            "VLPP0512"
        };
        static readonly string[] R8equations = new string[8] {
            "VLPP0801", 
            "VLPP0802", 
            "VLPP0803", 
            "VLPP0804",                         
            "VLPP0806", 
            "VLPP0807", 
            "VLPP0810", 
            "VLPP0812"
        };
        static readonly string[] R9equations = new string[14] {"VLPP0901", "VLPP0902", "VLPP0903", "VLPP0904",
                                                       "VLPP0905", "VLPP0906", "VLPP0907", "VLPP0908",
                                                       "VLPP0909", "VLPP0910", "VLPP0911", "VLPP0912",
                                                       "VLPP0913", "VLPP0914"};
        static readonly string[] R10equations = new string[8] {"VLPP0001", "VLPP0002", "VLPP0003", "VLPP0004",
                                                       "VLPP0006", "VLPP0007", "VLPP0010", "VLPP0012"};


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