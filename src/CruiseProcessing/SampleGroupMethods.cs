using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public class SampleGroupMethods
    {
        //  edit check on sample groups
        public int CheckAllUOMandCutLeave(List<StratumDO> strList, List<SampleGroupDO> sgList, string FileName)
        {
            int errorsFound = 0;
            ErrorLogMethods elm = new ErrorLogMethods();
            foreach (StratumDO sdo in strList)
            {
                List<SampleGroupDO> strGroups = sgList.FindAll(
                    delegate(SampleGroupDO sgd)
                    {
                        return sgd.Stratum.Code == sdo.Code;
                    });
                foreach (SampleGroupDO sg in strGroups)
                {
                    int nthRow = strGroups.FindIndex(
                        delegate(SampleGroupDO sgdo)
                        {
                            return sgdo.CutLeave != sg.CutLeave && sgdo.PrimaryProduct != sg.PrimaryProduct
                                        && sgdo.UOM != sg.UOM && sgdo.Code == sg.Code;
                        });
                    if (nthRow >= 0)
                    {
                        elm.LoadError("SampleGroup", "E", "14", (long)sg.SampleGroup_CN, "Code");
                        errorsFound++;
                    }   //  endif nthRow

                //  check length of sample group cpde and issue a warning message if more than 2 characters
                    if (sg.Code.Length > 2)
                        Utilities.LogError("SampleGroup",(int) sg.SampleGroup_CN,"W","Sample Group is too long. Results may not be as expected.",FileName);
                }   //  end foreach loop
            }   //  end foreach loop
            return errorsFound;
        }   //  end CheckAllUOMandCutLeave

        public ArrayList buildPrintArray(SampleGroupDO sg)
        {
            ArrayList sgArray = new ArrayList();
            sgArray.Add("");
            sgArray.Add(sg.Stratum.Code.PadLeft(2, ' '));
            sgArray.Add(sg.Code.PadLeft(2, ' '));
            sgArray.Add(Utilities.Format("{0,4:F0}", sg.SamplingFrequency).ToString().PadLeft(4, ' '));
            sgArray.Add(Utilities.Format("{0,4:F0}", sg.KZ).ToString().PadLeft(4, ' '));
            sgArray.Add(Utilities.Format("{0,4:F0}", sg.BigBAF).ToString().PadLeft(4, ' '));
            //sgArray.Add(Utilities.FormatField(sg.SmallFPS, "{0,4:F0}").ToString().PadLeft(4, ' '));
            sgArray.Add("    ");
            if (sg.Description == null)
                sgArray.Add(" ".PadRight(25, ' '));
            else sgArray.Add(sg.Description.PadRight(25, ' '));
            return sgArray;
        }   //  end buildPrintArray

    }   //  end SampleGroupMethods
}
