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

        public List<POPDO> GetPopsByStratum(string stratumCode)
        {
            return DAL.From<POPDO>()
                .Where("Stratum = @p1")
                .Read(stratumCode)
                .ToList();
        }

        public List<POPDO> GetPopsByStratum(string stratumCode, string cutLeave)
        {
            return DAL.From<POPDO>()
                .Where("Stratum = @p1 AND CutLeave = @p2")
                .Read(stratumCode, cutLeave)
                .ToList();
        }






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
