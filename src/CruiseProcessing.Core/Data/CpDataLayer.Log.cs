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

        public List<LogDO> getLogs()
        {
            return DAL.From<LogDO>().Read().ToList();
        }   //  end getLogs



        //  Log Table and Log Stock Table
        // *******************************************************************************************
        public List<LogDO> getTreeLogs(long currTreeCN)
        {
            return DAL.From<LogDO>()
                .Where("Tree_CN = @p1")
                .Read(currTreeCN).ToList();
        }   //  getTreeLogs

        // only used for EX1 report
        public List<LogDO> getTreeLogs()
        {
            //StringBuilder sb = new StringBuilder();
            //sb.Clear();
            //sb.Append("JOIN Log ON Log.Tree_CN = Tree.Tree_CN");
            return DAL.From<LogDO>().Read().ToList();
        }   //  end getTreeLogs






    }
}
