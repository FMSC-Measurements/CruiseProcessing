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
        public List<LogStockDO> getLogStock()
        {
            return DAL.From<LogStockDO>().Read().ToList();
        }   //  end getLogStock
        public List<LogStockDO> getLogStockSorted()
        {
            return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("Stratum AS st", "USING (Stratum_CN)")
                .LeftJoin("Plot AS p", "USING (Plot_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Join("CuttingUnit AS cu", "USING (CuttingUnit_CN)")
                .Where("sg.CutLeave = 'C'")
                .OrderBy("st.Code", "CAST(cu.Code AS NUMERIC)", "p.PlotNumber")
                .Read().ToList();
        }   //  end getLogStockSorted



        public List<LogStockDO> getCullLogs(long currTreeCN, string currGrade)
        {
            return DAL.From<LogStockDO>()
                .Where("Tree_CN = @p1 AND Grade = @p2")
                .Read(currTreeCN, currGrade).ToList();
        }   //  end getCullLogs


        public List<LogStockDO> getLogDIBs()
        {
            return DAL.From<LogStockDO>().GroupBy("DIBClass").Read().ToList();
        }   //  end getLogDIBS


        public List<LogStockDO> getCutOrLeaveLogs(string currCL)
        {
            //  Pulls logs for Region 10 L1 output files
            return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = @p1")
                .OrderBy("Species")
                .Read(currCL).ToList();
        }   //  end getCutLogs


        public List<LogStockDO> getCutLogs()
        {
            //  Pulls just cut logs
            return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = 'C'")
                .OrderBy("Species")
                .Read().ToList();
        }   //  end getCutLogs


        public List<LogStockDO> getCutLogs(string currCL, string currSP, string currPP, int byDIBclass)
        {
            //  Needed for R501 report
            if (byDIBclass == 1)
            {
                return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = @p1 AND t.Species = @p2 AND sg.PrimaryProduct = @p3")
                .GroupBy("DIBClass")
                .Read(currCL, currSP, currPP).ToList();
            }
            else
            {
                return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = @p1 AND t.Species = @p2 AND sg.PrimaryProduct = @p3")
                .Read(currCL, currSP, currPP).ToList();
            }
        }   //  end getCutLogs


        public List<LogStockDO> getCutLogs(string currSP, string currSort, string currGrade)
        {
            // TODO check calling code. original quiry should have thrown error

            //StringBuilder sb = new StringBuilder();
            //sb.Clear();
            //sb.Append("JOIN LogStock ON LogStock.Tree_CN = Tree.Tree_CN ");
            //sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            //sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.Species = ? ");
            //sb.Append("AND LogStock.ExportGrade = ? AND LogStock.Grade = ?");

            //  used in EX reports
            return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = 'C' AND t.Species = @p1 AND LogStock.ExportGrade = @p2 AND LogStock.Grade = @p3")
                .Read(currSP, currSort, currGrade)
                .ToList();

        }   //  end getCutLogs


        public List<LogStockDO> getLogSorts(string currSP)
        {
            // TODO check calling code. original quiry should have thrown error
            //StringBuilder sb = new StringBuilder();
            //sb.Append("JOIN LogStock ON LogStock.Tree_CN = Tree.Tree_CN ");
            //sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            //if (currSP != "")
            //    sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.Species = ? ");
            //else sb.Append("WHERE SampleGroup.CutLeave = ?");
            //sb.Append("GROUP BY ExportGrade,Grade");
            //if (currSP != "")
            //    return DAL.Read<LogStockDO>("LogStock", sb.ToString(), "C", currSP);
            //else return DAL.Read<LogStockDO>("LogStock", sb.ToString(), "C");

            //  used in EX reports
            var qb = DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)");

            if (currSP != null)
            { qb.Where("SampleGroup.CutLeave = @p1 AND Tree.Species = @p2"); }
            else
            { qb.Where("SampleGroup.CutLeave = @p1"); }
            qb.GroupBy("ExportGrade", "Grade");

            return qb.Read("C", currSP).ToList();
        }   //  end getLogSorts


        public List<LogStockDO> getLogSpecies(string currSP, float minDIB, float maxDIB,
                                              string currST, string currGrade)
        {
            //  used mostly for BLM09 and BLM10 reports
            return DAL.From<LogStockDO>()
               .Join("Tree AS t", "USING (Tree_CN)")
               .Join("Stratum AS st", "USING (Stratum_CN)")
               .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
               .Where("sg.CutLeave = 'C' AND st.Code = @p1 AND t.CountOrMeasure = 'M' AND t.Species = @p2 AND LogStock.Grade = @p3 AND DIBClass >= @p4 AND DIBClass <= @p5")
               .Read(currST, currSP, currGrade, minDIB, maxDIB).ToList();
        }   //  end getLogSpecies


        public List<LogStockDO> getStrataLogs(string currST, string currGrade)
        {
            //  used mostly for BLM reports
            return DAL.From<LogStockDO>()
               .Join("Tree AS t", "USING (Tree_CN)")
               .Join("Stratum AS st", "USING (Stratum_CN)")
               .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
               .Where("sg.CutLeave = 'C' AND st.Code = @p1 AND t.CountOrMeasure = 'M' AND LogStock.Grade = @p2")
               .Read(currST, currGrade).ToList();
        }   //  end getStrataLogs



        public List<LogStockDO> getStrataLogs(string currSP, string currST, string currSG, string currSTM, string currGrade)
        {

            //StringBuilder sb = new StringBuilder();
            //sb.Clear();
            //sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            //sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            //sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            //sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            //sb.Append("WHERE SampleGroup.CutLeave = 'C' AND CountOrMeasure = 'M' AND STM = ? ");
            //sb.Append("AND Stratum.Code = ? AND Species = ? AND SampleGroup.Code = ? AND ");
            //sb.Append("LogStock.Grade = ?");

            //  used in BLM reports
            return DAL.From<LogStockDO>()
               .Join("Tree AS t", "USING (Tree_CN)")
               .Join("Stratum AS st", "USING (Stratum_CN)")
               .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
               .Join("CuttingUnit AS cu", "USING (CuttingUnit_CN)")
               .Where("sg.CutLeave = 'C' AND CountOrMeasure = 'M' AND STM = @p1 AND st.Code = @p2 AND t.Species = @p3 AND sg.Code =  @p4 AND LogStock.Grade = @p5")
               .Read(currSTM, currST, currSP, currSG, currGrade)
               .ToList();

        }   //  end getStrataLogs


        public List<LogStockDO> getUnitLogs(long currST_CN, long currCU_CN, string currGrade, string currSTM)
        {
            // mostly for BLM reports
            return DAL.From<LogStockDO>()
               .Join("Tree AS t", "USING (Tree_CN)")
               .Join("Stratum AS st", "USING (Stratum_CN)")
               .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
               .Join("CuttingUnit AS cu", "USING (CuttingUnit_CN)")
               .Where("sg.CutLeave = 'C' AND st.Stratum_CN = @p1 AND cu.CuttingUnit_CN = @p2 AND LogStock.Grade = @p3 AND t.STM = @p4")
               .Read(currST_CN, currCU_CN, currGrade, currSTM)
               .ToList();
        }   //  end getUnitLogs


        public void DeleteLogStock()
        {
            //  see note above concerning this code

            //  make sure filename is complete
            /*          fileName = checkFileName(fileName);

                      //   open connection and delete data
                      using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
                      {
                          //  open connection
                          sqlconn.Open();
                          SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                          //  delete all rows
                          sqlcmd.CommandText = "DELETE FROM LogStock WHERE LogStock_CN>0";
                          sqlcmd.ExecuteNonQuery();
                          sqlconn.Close();
                      }   //  end using
                      */
            DAL.Execute("DELETE FROM LogStock WHERE LogStock_CN>0");
            return;
        }   //  end deleteLogStock

        public void SaveLogStock(IEnumerable<LogStockDO> lsList)
        {
            foreach (var ls in lsList)
            {
                if (ls.DAL == null)
                {
                    ls.DAL = DAL;
                }   //  endif
                ls.Save();
            }
        }

        [Obsolete]
        public void SaveLogStock(List<LogStockDO> lsList, int totLogs)
        {
            LogStockDO ls = new LogStockDO();
            for (int k = 0; k < totLogs; k++)
            {
                ls = lsList[k];
                if (ls.DAL == null)
                {
                    ls.DAL = DAL;
                }   //  endif
                ls.Save();

            }   //  end for k loop
            return;
        }   //  end SaveLogStock
    }
}
