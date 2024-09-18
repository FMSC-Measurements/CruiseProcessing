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
        public List<POPDO> getPOP()
        {
            return DAL.From<POPDO>().Read().ToList();
        }   //  end getPOP

        public List<TreeCalculatedValuesDO> GetPOPtrees(POPDO pdo, string currST, string countMeasure)
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


        public List<POPDO> GetUOMfromPOP()
        {
            return DAL.From<POPDO>().GroupBy("UOM").Read().ToList();
        }   //  end GetUOMfromPOP


        public void SavePOP(List<POPDO> POPlist)
        {
            foreach (POPDO DOpop in POPlist)
            {
                if (DOpop.DAL == null)
                {
                    DOpop.DAL = DAL;
                }
                DOpop.Save();
            }   //  end foreach loop
            return;
        }   //  end SavePOP

        public void DeletePOP()
        {
            //  see note above concerning this code
            //List<POPDO> popList = getPOP();
            //foreach (POPDO ppdo in popList)
            //    ppdo.Delete();

            /*            //  make sure filename is complete
                        fileName = checkFileName(fileName);

                        //   open connection and delete data
                        using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
                        {
                            //  open connection
                            sqlconn.Open();
                            SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                            //  delete all rows
                            sqlcmd.CommandText = "DELETE FROM POP WHERE POP_CN>0";
                            sqlcmd.ExecuteNonQuery();
                            sqlconn.Close();
                        }   //  end using
                        */
            DAL.Execute("DELETE FROM POP WHERE POP_CN>0");
            return;
        }   //  end deletePOP


    }
}
