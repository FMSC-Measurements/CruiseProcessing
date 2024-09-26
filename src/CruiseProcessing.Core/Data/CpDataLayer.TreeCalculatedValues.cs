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
        public List<TreeCalculatedValuesDO> GetTreeCalculatedValuesByPop(POPDO pdo, string currST, string countMeasure)
        {
            //  captures tree calculated values for POP summation in SumAll

            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree", "USING (Tree_CN)")
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .Where("Stratum.Code = @p1 AND SampleGroup.CutLeave = @p2 AND SampleGroup.Code = @p3 " +
                "AND SampleGroup.PrimaryProduct = @p4 AND SampleGroup.SecondaryProduct = @p5 AND " +
                "Tree.STM = @p6 AND Tree.CountOrMeasure = @p7")
                .Read(currST, pdo.CutLeave, pdo.SampleGroup, pdo.PrimaryProduct,
                        pdo.SecondaryProduct, pdo.STM, countMeasure).ToList();
        }   //  end GetPOPtrees

        public List<TreeCalculatedValuesDO> getTreeCalculatedValues()
        {
            return DAL.From<TreeCalculatedValuesDO>().Read().ToList();
        }   //  end getTreeCalculatedValues

        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Where("t.Stratum_CN = @p1")
                .Read(currStratumCN).ToList();
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN, int currUnitCN)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Where("t.Stratum_CN = @p1 AND t.CuttingUnit_CN = @p2")
                .Read(currStratumCN, currUnitCN)
                .ToList();
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(string currSP)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Where("t.Species = @p1")
                .Read(currSP).ToList();
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN, string orderBy)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Where("t.Stratum_CN = @p1")
                .OrderBy(orderBy)
                .Read(currStratumCN)
                .ToList();
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(string currCL, string orderBy, string reportType)
        {
            // TODO remove param reportType
            // rewritten Dec 2020 - Ben
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = @p1")
                .OrderBy(orderBy)
                .Read(currCL)
                .ToList();
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getStewardshipTrees(string currUnit, string currSP, string currPP)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("CuttingUnit AS cu", "USING (CuttingUnit_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = @p1 AND cu.Code = @p2 AND t.Species = @p3 AND sg.PrimaryProduct = @p4")
                .Read("C", currUnit, currSP, currPP)
                .ToList();
        }   //  end getStewardshipTrees


        public List<TreeCalculatedValuesDO> getRegressTrees(string currSP, string currPR, string currLD, string currCM)
        {
            return DAL.From<TreeCalculatedValuesDO>()
               .Join("Tree AS t", "USING (Tree_CN)")
               .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
               .Where("t.Species = @p1 ANd sg.PrimaryProduct = @p2 AND t.LiveDead = @p3 AND t.CountOrMeasure = @p4")
               .Read(currSP, currPR, currLD, currCM).ToList();
        }   //  end getRegressTrees

        public List<TreeCalculatedValuesDO> GetTreeCalculatedValuesByLCD(LCDDO ldo, string countMeasure)
        {
            //  captures tree calculated values for LCD summation in SumAll

            //  September 2016 -- per K.Cormier -- dropping contract species from LCD identifier
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree", "USING (Tree_CN)")
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Join("TreeDefaultValue", "USING (TreeDefaultValue_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .Where("Stratum.Code = @p1 " +
                    "AND SampleGroup.CutLeave = @p2 " +
                    "AND SampleGroup.Code = @p3 " +
                    "AND Tree.Species = @p4 " +
                    "AND SampleGroup.PrimaryProduct = @p5 " +
                    "AND SampleGroup.SecondaryProduct = @p6 " +
                    "AND SampleGroup.UOM = @p7 " +
                    "AND Tree.LiveDead = @p8 " +
                    "AND Tree.Grade = @p9 " +
                    "AND Stratum.YieldComponent = @p10 " +
                    "AND Tree.STM = @p11 " +
                    "AND Tree.CountOrMeasure = @p12;")
                .Read(ldo.Stratum,
                        ldo.CutLeave,
                        ldo.SampleGroup,
                        ldo.Species,
                        ldo.PrimaryProduct,
                        ldo.SecondaryProduct,
                        ldo.UOM,
                        ldo.LiveDead,
                        ldo.TreeGrade,
                        ldo.Yield,
                        ldo.STM,
                        countMeasure).ToList();
        }

        public List<TreeCalculatedValuesDO> GetTreeCalculatedValuesByLCD(LCDDO ldo)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree", "USING (Tree_CN)")
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Join("TreeDefaultValue", "USING (TreeDefaultValue_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .Where("Stratum.Code = @p1 " +
                    "AND SampleGroup.CutLeave = @p2 " +
                    "AND SampleGroup.Code = @p3 " +
                    "AND Tree.Species = @p4 " +
                    "AND SampleGroup.PrimaryProduct = @p5 " +
                    "AND SampleGroup.SecondaryProduct = @p6 " +
                    "AND SampleGroup.UOM = @p7 " +
                    "AND Tree.LiveDead = @p8 " +
                    "AND Tree.Grade = @p9 " +
                    "AND Stratum.YieldComponent = @p10 " +
                    "AND Tree.STM = @p11; ")
                .Read(ldo.Stratum,
                        ldo.CutLeave,
                        ldo.SampleGroup,
                        ldo.Species,
                        ldo.PrimaryProduct,
                        ldo.SecondaryProduct,
                        ldo.UOM,
                        ldo.LiveDead,
                        ldo.TreeGrade,
                        ldo.Yield,
                        ldo.STM).ToList();
        }

        public void deleteTreeCalculatedValues()
        {
            //  the following code is what Ben Campbell suggested but it runs REALLY SLOW
            //  Switched back to my method which runs faster
            //List<TreeCalculatedValuesDO> tcvList = getTreeCalculatedValues();
            //foreach (TreeCalculatedValuesDO tcvdo in tcvList)
            //    tcvdo.Delete();

            //  make sure filename is complete
            /*          fileName = checkFileName(fileName);

                      //   open connection and delete data
                      using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
                      {
                          //  open connection
                          sqlconn.Open();
                          SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                          //  delete all rows
                          sqlcmd.CommandText = "DELETE FROM TreeCalculatedValues WHERE TreeCalcValues_CN>0";
                          sqlcmd.ExecuteNonQuery();
                          sqlconn.Close();
                      }   //  end using
                      */
            DAL.Execute("DELETE FROM TreeCalculatedValues WHERE TreeCalcValues_CN>0");
            return;

        }   //  end deleteTreeCalculatedValues

        public void SaveTreeCalculatedValues(List<TreeCalculatedValuesDO> tcvList)
        {
            foreach (TreeCalculatedValuesDO tcv in tcvList)
            {
                if (tcv.DAL == null)
                {
                    tcv.DAL = DAL;
                }
                tcv.Save();
            }   //  end foreach loop
            return;
        }   //  end SaveTreeCalculatedValues
    }
}
