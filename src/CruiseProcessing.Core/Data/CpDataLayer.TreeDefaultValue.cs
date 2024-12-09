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
        public List<TreeDefaultValueDO> getTreeDefaults()
        {
            return DAL.From<TreeDefaultValueDO>().Read().ToList();
        }   //  end getTreeDefaults

        public IEnumerable<TreeDefaultValueDO> GetTreeDefaultValues(string species, string product)
        {
            return DAL.From<TreeDefaultValueDO>()
                .Where("Species = @p1 AND PrimaryProduct = @p2")
                .Query(species, product)
                .ToList();
        }

        public IEnumerable<string> GetAllSpeciesCodes()
        {
            return DAL.QueryScalar<string>("SELECT DISTINCT Species FROM TreeDefaultValue").ToList();
        }

        public int GetFIACode(string species)
        {
            return DAL.ExecuteScalar<int>("SELECT FIAcode FROM TreeDefaultValue WHERE Species = @p1 LIMIT 1", species);
        }

        public IEnumerable<string> GetDistincePrimaryProductCodes()
        {
            return DAL.QueryScalar<string>("SELECT DISTINCT PrimaryProduct FROM TreeDefaultValue").ToList();
        }

        public void SaveTreeDefaults(List<TreeDefaultValueDO> treeDefaults)
        {
            //  Then saves the updated regression results list
            foreach (TreeDefaultValueDO tdv in treeDefaults)
            {
                if (tdv.DAL == null)
                {
                    tdv.DAL = DAL;
                }
                tdv.Save();
            }   //  end foreach loop
            return;

        }   //  end SaveTreeDefaults

        public List<TreeDefaultValueDO> GetUniqueSpeciesProductLiveDead()
        {
            ////  make sure filename is complete
            //fileName = checkFileName(fileName);

            ////  build query string
            //StringBuilder sb = new StringBuilder();
            //sb.Clear();
            //sb.Append("SELECT DISTINCT Species,LiveDead,PrimaryProduct FROM TreeDefaultValue");
            //List<TreeDefaultValueDO> tdvList = new List<TreeDefaultValueDO>();

            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //    sqlcmd.CommandText = sb.ToString();
            //    sqlcmd.ExecuteNonQuery();
            //    SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
            //    if (sdrReader.HasRows)
            //    {
            //        while (sdrReader.Read())
            //        {
            //            TreeDefaultValueDO td = new TreeDefaultValueDO();
            //            td.Species = sdrReader.GetString(0);
            //            td.LiveDead = sdrReader.GetString(1);
            //            td.PrimaryProduct = sdrReader.GetString(2);
            //            tdvList.Add(td);
            //        }   //  end while read
            //    }   //  endif
            //    sqlconn.Close();
            //}   //  end using
            //return tdvList;


            // rewritten Dec 2020 - Ben
            return DAL.Read<TreeDefaultValueDO>("SELECT DISTINCT Species,LiveDead,PrimaryProduct FROM TreeDefaultValue", null).ToList();
        }   //  end GetUniqueSpeciesProductLiveDead

        // *  July 2017 -- changes to Region 8 volume equations -- this code is no longer used
        // * July 2017 --  decision made to hold off on releasing the changes to R8 volume equations
        // * so this code is replaced.
        public ArrayList GetProduct08Species()
        {
            // TODO optimize

            ArrayList justSpecies = new ArrayList();

            List<TreeDefaultValueDO> tdvList = DAL.From<TreeDefaultValueDO>().Where("PrimaryProduct = '08'").GroupBy("Species").Read().ToList();

            foreach (TreeDefaultValueDO tdv in tdvList)
                justSpecies.Add(tdv.Species);

            return justSpecies;
        }   //  end GetProduct08Species

        /*  December 2013 -- need to change where species/product comes from.  Template files will have all volume equations
         *  but it was found if only one or more species are used, folks don't want to see all those equations
         *  so changed this to pull those combinations from Tree instead of the default
                public string[,] GetUniqueSpeciesProduct()
                {
                    DAL = new CruiseDAL.DAL(fileName);

                    List<TreeDefaultValueDO> tdvList = DAL.Read<TreeDefaultValueDO>("TreeDefaultValue", "GROUP BY Species, PrimaryProduct", null);
                    int numSpecies = tdvList.Count();
                    string[,] speciesProduct = new string[numSpecies, 2];
                    int nthRow = 0;
                    foreach (TreeDefaultValueDO tdv in tdvList)
                    {
                        speciesProduct[nthRow,0] = tdv.Species;
                        speciesProduct[nthRow,1] = tdv.PrimaryProduct;
                        nthRow++;
                    }   //  end foreach loop
                    return speciesProduct;
                }   //  end GetUniqueSpeciesProduct
        */
    }
}
