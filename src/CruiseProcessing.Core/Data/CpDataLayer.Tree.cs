using CruiseDAL.DataObjects;
using CruiseProcessing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        public List<TreeDO> getTrees()
        {
            return DAL.From<TreeDO>().Read().ToList();
        }   //  end getTrees

        public List<TreeDO> GetUniqueSpecies()
        {
            // TODO optimize this method and calling methods

            return DAL.From<TreeDO>().GroupBy("Species").Read().ToList();
        }   //  end GetUniqueSpecies

        public IEnumerable<string> GetDistinctTreeSpeciesCodes()
        {
            return DAL.QueryScalar<string>("SELECT DISTINCT Species FROM Tree  WHERE CountOrMeasure='M'").ToList();
        }

        public List<TreeDO> GetDistinctSpeciesLiveDeadGradeStm(long SG_CN)
        {
            return DAL.From<TreeDO>().Where("SampleGroup_CN = @p1").GroupBy("Species", "LiveDead", "Grade", "STM").Read(SG_CN).ToList();
        }   //  end GetDistinctSpecies

        public IEnumerable<TreeDO> GetTreesByStratum(string stratumCode)
        {
            return DAL.From<TreeDO>()
                .Join("Stratum", "USING (Stratum_CN)")
                .Where("Stratum.Code = @p1")
                .Query(stratumCode).ToArray();
        }

        public List<TreeDO> getTreesOrdered(string searchString, string orderBy, string[] searchValues)
        {
            return DAL.Read<TreeDO>("SELECT * FROM Tree " + searchString + orderBy + ";", searchValues).ToList();
        }

        public List<TreeDO> GetTreesOrderedByUnit()
        {
            return DAL.From<TreeDO>()
                .Join("CuttingUnit", "USING (CuttingUnit_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .LeftJoin("Plot", "USING (Plot_CN)")
                .OrderBy("CuttingUnit.Code", "ifnull(Plot.PlotNumber, 0)", "Stratum.Code", "TreeNumber").Read()
                .ToList();
        }


        public List<TreeDO> GetTreesOrderedByStratum()
        {
            return DAL.From<TreeDO>()
                .Join("CuttingUnit", "USING (CuttingUnit_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .LeftJoin("Plot", "USING (Plot_CN)")
                .OrderBy("Stratum.Code", "CuttingUnit.Code", "ifnull(Plot.PlotNumber, 0)", "Tree.TreeNumber")
                .Read().ToList();
        }   //  end getTreesSorted


        public List<TreeDO> getUnitTrees(long currST_CN, long currCU_CN)
        {
            return DAL.From<TreeDO>()
                .Join("CuttingUnit", "USING (CuttingUnit_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Where("SampleGroup.CutLeave = 'C' AND Tree.CountOrMeasure = 'M' AND Tree.Stratum_CN = @p1 AND Tree.CuttingUnit_CN = @p2")
                .Read(currST_CN, currCU_CN).ToList();
        }   //  end getUnitTrees


        public List<TreeDO> getUniqueStewardGroups()
        {
            return DAL.From<TreeDO>()
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Where("SampleGroup.CutLeave = 'C'")
                .GroupBy("CuttingUnit_CN", "Species", "Tree.SampleGroup_CN")
                .Read().ToList();
        }   //  end getUniqueStewardGroups


        public List<TreeDO> getTreeDBH(string currCL)
        {
            //  works for stand tables DIB classes for the sale

            return DAL.From<TreeDO>()
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Where(" SampleGroup.CutLeave = @p1 AND Tree.CountOrMeasure = 'M'")
                .GroupBy("DBH")
                .Read(currCL).ToList();

        }   //  end getTreeDBH


        public List<TreeDO> getTreeDBH(string currCL, string currST, string currCM)
        {
            //  works for stand tables DIB classes for current stratum

            return DAL.From<TreeDO>()
                .Join("Stratum", "USING (Stratum_CN)")
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Where(" SampleGroup.CutLeave = @p1 AND Stratum.Code = @p2 AND Tree.CountOrMeasure = @p3")
                .GroupBy("DBH")
                .Read(currCL, currST, currCM).ToList();
        }   //  end getTreeDBH


        public List<TreeDO> JustMeasuredTrees()
        {
            return DAL.From<TreeDO>().Where("CountOrMeasure = 'M'").Read().ToList();
        }   //  end JustMeasuredTrees


        public List<TreeDO> JustMeasuredTrees(long currST_CN)
        {
            return DAL.From<TreeDO>().Where("CountOrMeasure = 'M' AND Stratum_CN = @p1").Read(currST_CN).ToList();
        }   //  end overloaded JustMeasuredTrees


        public List<TreeDO> JustUnitTrees(long currST_CN, long currCU_CN)
        {
            return DAL.From<TreeDO>().Where("Stratum_CN = @p1 AND CuttingUnit_CN = @p2").Read(currST_CN, currCU_CN).ToList();
        }   //  end JustUnitTrees

        public List<TreeDO> JustFIXCNTtrees(long currST_CN)
        {
            return DAL.From<TreeDO>().Where("CountOrMeasure = 'C' AND Stratum_CN = @p1").Read(currST_CN).ToList();
        }   //  end JustFIXCNTtrees

        public List<TreeDO> getLCDtrees(LCDDO currLCD, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("JOIN Stratum JOIN SampleGroup WHERE Tree.LiveDead = @p1 AND Tree.Species = @p2 AND Tree.Grade = @p3 AND ");
            sb.Append("Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND SampleGroup.Code = @p4 AND ");
            sb.Append("Stratum.Code = @p5 AND Tree.CountOrMeasure = @p6 AND Tree.STM = @p7");
            return DAL.Read<TreeDO>("SELECT * FROM Tree " + sb.ToString() + ";", new object[] { currLCD.LiveDead, currLCD.Species, currLCD.TreeGrade,
                                            currLCD.SampleGroup, currLCD.Stratum, cntMeas, currLCD.STM }).ToList();
        }   //  end getLCDtrees


        public List<TreeDO> getPOPtrees(POPDO currPOP, string cntMeas)
        {
            StringBuilder sb = new StringBuilder(); ;
            sb.Append("JOIN Stratum JOIN SampleGroup WHERE Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("SampleGroup.Code = @p1 AND Stratum.Code = @p2 AND Tree.CountOrMeasure = @p3 AND Tree.STM = @p4");
            return DAL.Read<TreeDO>("SELECT * FROM Tree " + sb.ToString() + ";", new object[] { currPOP.SampleGroup, currPOP.Stratum, cntMeas, currPOP.STM }).ToList();
        }   //  end getPOPtrees


        public List<TreeDO> getPROtrees(PRODO currPRO, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("JOIN Stratum JOIN SampleGroup JOIN CuttingUnit WHERE Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN AND ");
            sb.Append("SampleGroup.Code = @p1 AND Stratum.Code = @p2 AND CuttingUnit.Code = @p3 AND Tree.CountOrMeasure = @p4 ");
            sb.Append("AND Tree.STM = @p5");
            return DAL.Read<TreeDO>("SELECT * FROM Tree " + sb.ToString() + ";", new object[] { currPRO.SampleGroup, currPRO.Stratum, currPRO.CuttingUnit, cntMeas, currPRO.STM }).ToList();
        }   //  end getPROtrees

        public string[,] GetUniqueSpeciesProduct()
        {
            // TODO optimize

            List<TreeDO> tList = DAL.From<TreeDO>()
                .Where("Species IS NOT NULL AND SampleGroup_CN IS NOT NULL")
                .GroupBy("Species", "SampleGroup_CN").Query().ToList();

            int numSpecies = tList.Count();
            string[,] speciesProduct = new string[numSpecies, 2];
            int nthRow = -1;
            foreach (TreeDO t in tList)
            {
                //  is the combination already in the list?
                int iFound = -1;
                for (int k = 0; k < speciesProduct.GetLength(0); k++)
                {
                    if (speciesProduct[k, 0] == t.Species && speciesProduct[k, 1] == t.SampleGroup.PrimaryProduct)
                        iFound = 1;
                }   //  end for loop
                if (iFound == -1)
                {
                    //if (t.CountOrMeasure != "I")
                    //{
                    nthRow++;
                    speciesProduct[nthRow, 0] = t.Species;
                    speciesProduct[nthRow, 1] = t.SampleGroup.PrimaryProduct;
                    //}   //  endif
                }   //  endif
            }   //  end foreach loop

            return speciesProduct;
        }   //  end GetUniqueSpeciesProduct

        public IReadOnlyCollection<SpeciesProduct> GetUniqueSpeciesProductFromTrees()
        {
            return DAL.Query<SpeciesProduct>("SELECT DISTINCT t.Species AS SpeciesCode, sg.PrimaryProduct AS ProductCode " +
                "FROM Tree AS t " +
                "JOIN SampleGroup AS sg USING (SampleGroup_CN) " +
                "WHERE t.Species IS NOT NULL AND length(t.Species) > 0 AND t.SampleGroup_CN IS NOT NULL").ToArray();
        }

        public IReadOnlyCollection<SpeciesProductLiveDead> GetUniqueSpeciesProductLiveDeadFromTrees()
        {
            return DAL.Query<SpeciesProductLiveDead>("SELECT DISTINCT t.Species AS SpeciesCode, sg.PrimaryProduct AS ProductCode, t.LiveDead AS LiveDead " +
                "FROM Tree AS t " +
                "JOIN SampleGroup AS sg USING (SampleGroup_CN) " +
                "WHERE t.Species IS NOT NULL AND length(t.Species) > 0 AND t.LiveDead IS NOT NULL AND t.LiveDead IN ('L', 'D') AND t.SampleGroup_CN IS NOT NULL").ToArray();
        }

        public IReadOnlyCollection<SpeciesProductLiveDead> GetUniqueSpeciesProductLiveDeadFromTrees(string species, string product)
        {
            return DAL.Query<SpeciesProductLiveDead>("SELECT DISTINCT tdv.Species AS SpeciesCode, tdv.FiaCode, sg.PrimaryProduct AS ProductCode, tdv.LiveDead AS LiveDead " +
                "FROM Tree AS t " +
                "JOIN SampleGroup AS sg USING (SampleGroup_CN) " +
                "JOIN TreeDefaultValue AS tdv USING (TreeDefaultValue_CN) " +
                "WHERE t.Species = @p1 AND sg.PrimaryProduct = @p2 AND t.LiveDead IS NOT NULL AND t.LiveDead IN ('L', 'D')", species, product).ToArray();
        }

        public void SaveTrees(List<TreeDO> tList)
        {
            foreach (TreeDO tdo in tList)
            {
                if (tdo.DAL == null)
                {
                    tdo.DAL = DAL;
                }
                tdo.Save();
            }   //  end foreach loop
            return;
        }   //  end SaveTrees
    }
}
