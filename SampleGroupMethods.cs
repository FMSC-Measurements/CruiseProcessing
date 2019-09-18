using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public static class SampleGroupMethods
    {
        //  edit check on sample groups
        public static int CheckAllUOMandCutLeave(IEnumerable<StratumDO> strList, IEnumerable<SampleGroupDO> sgList, string FileName)
        {

            int errorsFound = 0;
            foreach (StratumDO sdo in strList)
            {
                var sgdo = sgList.Where(spg => spg.Stratum.Code == sdo.Code);
                foreach (SampleGroupDO sg in sgdo)
                {
//                    int nthRow = strGroups.FindIndex(
//                        delegate(SampleGroupDO sgdo)
//                        {
                            var smpList = sgdo.Where(item => item.CutLeave != sg.CutLeave && item.PrimaryProduct != sg.PrimaryProduct &&
                            item.UOM != sg.UOM && item.Code == sg.Code);
//                            return sgdo.CutLeave != sg.CutLeave && sgdo.PrimaryProduct != sg.PrimaryProduct
 //                                       && sgdo.UOM != sg.UOM && sgdo.Code == sg.Code;
 //                       });
                    if (smpList.Any())
                    {
                        ErrorLogMethods.LoadError("SampleGroup", "E", "14", (long)sg.SampleGroup_CN, "Code");
                        errorsFound++;
                    }   //  endif nthRow

                //  check length of sample group cpde and issue a warning message if more than 2 characters
                    if (sg.Code.Length > 2)
                        Utilities.LogError("SampleGroup",(int) sg.SampleGroup_CN,"W","Sample Group is too long. Results may not be as expected.");
                }   //  end foreach loop
            }   //  end foreach loop
            return errorsFound;
        }   //  end CheckAllUOMandCutLeave

        public static ArrayList buildPrintArray(SampleGroupDO sg)
        {
            ArrayList sgArray = new ArrayList();
            sgArray.Add("");
            sgArray.Add(sg.Stratum.Code.PadLeft(2, ' '));
            sgArray.Add(sg.Code.PadLeft(2, ' '));
            sgArray.Add(Utilities.FormatField(sg.SamplingFrequency, "{0,4:F0}").ToString().PadLeft(4, ' '));
            sgArray.Add(Utilities.FormatField(sg.KZ, "{0,4:F0}").ToString().PadLeft(4, ' '));
            sgArray.Add(Utilities.FormatField(sg.BigBAF, "{0,4:F0}").ToString().PadLeft(4, ' '));
            //sgArray.Add(Utilities.FormatField(sg.SmallFPS, "{0,4:F0}").ToString().PadLeft(4, ' '));
            sgArray.Add("    ");
            if (sg.Description == null)
                sgArray.Add(" ".PadRight(25, ' '));
            else sgArray.Add(sg.Description.PadRight(25, ' '));
            return sgArray;
        }   //  end buildPrintArray

    }   //  end SampleGroupMethods
}
