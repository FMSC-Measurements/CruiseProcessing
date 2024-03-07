using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;

namespace CruiseProcessing
{
    public class SampleGroupMethods
    {

        public static List<string> buildPrintArray(SampleGroupDO sg)
        {
            var sgArray = new List<string>();
            sgArray.Add("");
            sgArray.Add(sg.Stratum.Code.PadLeft(2, ' '));
            sgArray.Add(sg.Code.PadLeft(2, ' '));
            sgArray.Add(String.Format("{0,4:F0}", sg.SamplingFrequency).PadLeft(4, ' '));
            sgArray.Add(String.Format("{0,4:F0}", sg.KZ).PadLeft(4, ' '));
            sgArray.Add(String.Format("{0,4:F0}", sg.BigBAF).PadLeft(4, ' '));
            //sgArray.Add(Utilities.FormatField(sg.SmallFPS, "{0,4:F0}").ToString().PadLeft(4, ' '));
            sgArray.Add("    ");
            if (sg.Description == null)
                sgArray.Add(" ".PadRight(25, ' '));
            else sgArray.Add(sg.Description.PadRight(25, ' '));
            return sgArray;
        }   //  end buildPrintArray
    }   //  end SampleGroupMethods
}