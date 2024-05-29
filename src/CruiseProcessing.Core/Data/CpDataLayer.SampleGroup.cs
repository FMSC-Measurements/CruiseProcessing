using CruiseDAL.DataObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {

        public List<SampleGroupDO> getSampleGroups()
        {
            return DAL.From<SampleGroupDO>().Read().ToList();
        }   //  end getSampleGroups

        public List<SampleGroupDO> getSampleGroups(long currStratumCN)
        {
            return DAL.From<SampleGroupDO>()
                .Where("Stratum_CN = @p1")
                .Read(currStratumCN)
                .ToList();

        }   //  end getSampleGroups

        public IEnumerable<string> GetDistinctSampleGroupCodes()
        {
            // rewritten Dec 2020 - Ben
            return DAL.QueryScalar<string>("SELECT DISTINCT Code FROM SampleGroup;").ToList();

        }   //  end GetJustSampleGroups

        public List<SampleGroupDO> getSampleGroups(string qString)
        {
            // TODO calls to this function need to be rewritten. The qString param isn't needed
            // return DAL.Read<SampleGroupDO>("SampleGroup", qString, "C");

            //  returns just unique sample groups
            return DAL.From<SampleGroupDO>().Where("CutLeave = 'C'").GroupBy("Code").Read().ToList();
        }   //  end getSampleGroups

        public void SaveSampleGroups(List<SampleGroupDO> sgList)
        {
            foreach (SampleGroupDO sgd in sgList)
            {
                if (sgd.DAL == null)
                    sgd.DAL = DAL;
                sgd.Save();
            }   //  end foreach loop
            return;
        }   //  end SaveSampleGroups
    }
}
