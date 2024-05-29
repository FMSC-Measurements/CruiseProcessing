using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        public List<CountTreeDO> getCountTrees()
        {
            return DAL.From<CountTreeDO>().Read().ToList();
        }   //  end getCountTrees

        public List<CountTreeDO> getCountTrees(long currSG_CN)
        {
            return DAL.From<CountTreeDO>().Where("SampleGroup_CN = @p1").Read(currSG_CN).ToList();
        }   //  end overload of getCountTrees


        public List<CountTreeDO> getCountsOrdered()
        {
            return DAL.From<CountTreeDO>().OrderBy("CuttingUnit_CN").Read().ToList();
        }   //  end getCountsOrdered


        public List<CountTreeDO> getCountsOrdered(string searchString, string orderBy, string[] searchValues)
        {
            return DAL.Read<CountTreeDO>("SELECT * FROM CountTree " + searchString + orderBy + ";", searchValues).ToList();
        }   //  end getCountsOrdered
    }
}
