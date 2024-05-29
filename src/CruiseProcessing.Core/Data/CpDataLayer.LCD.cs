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
        public List<LCDDO> getLCD()
        {
            return DAL.From<LCDDO>().Read().ToList();
        }   //  end getLCD

        public List<LCDDO> getLCDOrdered(string searchString, string orderBy, string currCutLeave,
                                    string currST, string currPP)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(searchString);
            sb.Append(orderBy);
            if (currST == "")
                return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currCutLeave }).ToList();
            else if (currCutLeave == "")
                return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currST }).ToList();
            else if (currCutLeave != "" && currST != "" && currPP != "")
                return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currCutLeave, currST, currPP }).ToList();
            else return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currCutLeave, currST }).ToList();
        }   //  end getLCDOrdered


        public List<LCDDO> getLCDOrdered(string searchString, string orderBy, string currCutLeave,
                                            string currPP)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(searchString);
            sb.Append(orderBy);
            if (currPP != "")
                return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currCutLeave, currPP }).ToList();
            else return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currCutLeave }).ToList();
        }   //  end getLCDOrdered


        public List<LCDDO> GetLCDgroup(string currST, int currRPT, string currCL)
        {
            // rewritten Dec 2020 - Ben

            StringBuilder sb = new StringBuilder(); ;
            switch (currRPT)
            {
                case 1:         //  VSM1
                    sb.Append("SELECT Stratum,Species,PrimaryProduct,SecondaryProduct,UOM,LiveDead,CutLeave,Yield,SampleGroup,");
                    sb.Append("TreeGrade,ContractSpecies,STM FROM LCD WHERE Stratum = '");
                    sb.Append(currST);
                    sb.Append("';");
                    break;
                case 2:         //  VSM2
                    sb.Append("SELECT Stratum,PrimaryProduct,SecondaryProduct,UOM,SampleGroup,CutLeave,STM ");
                    sb.Append("FROM LCD WHERE Stratum = '");
                    sb.Append(currST);
                    sb.Append("' AND CutLeave = '");
                    sb.Append(currCL);
                    sb.Append("' GROUP BY SampleGroup,STM;");
                    break;
                case 3:         // VSM3
                    sb.Append("SELECT Stratum,PrimaryProduct,SecondaryProduct,UOM FROM LCD WHERE Stratum = '");
                    sb.Append(currST);
                    sb.Append("' AND CutLeave = '");
                    sb.Append(currCL);
                    sb.Append("' GROUP BY Stratum,PrimaryProduct;");
                    break;
                case 4:         //  WT1
                    sb.Append("SELECT Species,PrimaryProduct,SecondaryProduct,LiveDead,ContractSpecies from LCD WHERE CutLeave = '");
                    sb.Append("C");
                    sb.Append("' GROUP BY Species,PrimaryProduct,SecondaryProduct,LiveDead,ContractSpecies");
                    break;
                case 5:         //  WT4
                    sb.Append("SELECT Species FROM LCD WHERE CutLeave='C' GROUP BY Species");
                    break;
            }   //  end switch on report number

            return DAL.Query<LCDDO>(sb.ToString()).ToList();
        }   //  end GetLCDgroup

        public List<LCDDO> GetLCDgroup(string fileName, string groupedBy)
        {
            // rewritten Dec 2020 - Ben

            //  overloaded for stat report  ST3

            return DAL.Query<LCDDO>("SELECT Stratum,PrimaryProduct,SecondaryProduct,UOM FROM LCD WHERE CutLeave = 'C' GROUP BY " + groupedBy).ToList();
        }   //  end GetLCDgroup


        public List<LCDDO> GetLCDdata(string whereClause, LCDDO lcd, int reportNum, string currCL)
        {
            //  report number key
            //  1 = VSM1
            //  2 = VSM2
            //  3 = VSM3
            //  4 = WT1
            //  5 = R301

            var commandText = "SELECT * FROM LCD " + whereClause + ";";

            switch (reportNum)
            {
                case 1:
                    return DAL.Read<LCDDO>(commandText, new object[] { lcd.Stratum, lcd.Species, lcd.PrimaryProduct, lcd.UOM,
                                            lcd.LiveDead, lcd.CutLeave, lcd.Yield, lcd.SampleGroup, lcd.TreeGrade, lcd.STM }).ToList();
                //  September 2016 -- dropping contract species from LCD identifier
                //lcd.ContractSpecies, lcd.STM);
                case 2:
                    return DAL.Read<LCDDO>(commandText, new object[] { lcd.Stratum, lcd.PrimaryProduct, lcd.UOM, currCL,
                                                lcd.SampleGroup, lcd.STM }).ToList();
                case 3:
                    return DAL.Read<LCDDO>(commandText, new object[] { lcd.Stratum, lcd.PrimaryProduct, lcd.UOM, currCL, lcd.STM }).ToList();
                case 4:
                    return DAL.Read<LCDDO>(commandText, new object[] { lcd.Species, lcd.PrimaryProduct, lcd.SecondaryProduct,
                                                lcd.LiveDead, lcd.ContractSpecies, "C" }).ToList();
                case 5:
                    return DAL.Read<LCDDO>(commandText, new object[] { "C", lcd.Species, lcd.PrimaryProduct, lcd.ContractSpecies }).ToList();
            }   //  end switch
            List<LCDDO> emptyList = new List<LCDDO>();
            return emptyList;
        }   //  end GetLCDdata


        public List<LCDDO> GetLCDdata(string currST, string whereClause, string orderBy)
        {
            var commandText = "SELECT * FROM LCD " + whereClause + ";";

            //  works for UC1/UC2 reports -- maybe all UC reports?
            return DAL.Read<LCDDO>(commandText, new object[] { currST, "C", orderBy }).ToList();
        }   //  end GetLCDdata


        public List<LCDDO> GetLCDdata(string whereClause, string currST)
        {
            var commandText = "SELECT * FROM LCD " + whereClause + ";";

            //  works for regional report R104
            return DAL.Read<LCDDO>(commandText, new[] { currST }).ToList();
        }   //  end GetLCDdata

        public void SaveLCD(List<LCDDO> LCDlist)
        {
            foreach (LCDDO DOlcd in LCDlist)
            {
                if (DOlcd.DAL == null)
                {
                    DOlcd.DAL = DAL;
                }
                DOlcd.Save();
            }   //  end foreach loop
            return;
        }   //  end SaveLCD

        public void DeleteLCD()
        {
            //  see note above concerning this code
            //List<LCDDO> lcdList = getLCD();
            //foreach (LCDDO lcdo in lcdList)
            //    lcdo.Delete();
            //  make sure filename is complete
            /*            fileName = checkFileName(fileName);

                        //   open connection and delete data
                        using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
                        {
                            //  open connection
                            sqlconn.Open();
                            SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                            //  delete all rows
                            sqlcmd.CommandText = "DELETE FROM LCD WHERE LCD_CN>0";
                            sqlcmd.ExecuteNonQuery();
                            sqlconn.Close();
                        }   //  end using
                        */
            DAL.Execute("DELETE FROM LCD WHERE LCD_CN>0");
            return;
        }   //  end deleteLCD
    }
}
