using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;

namespace CruiseProcessing
{
    public static class Utilities
    {

        //  AcresLookup will go away once the updated ReturnCorrectAcres is tested and complete
        public static double AcresLookup(long currStrCN, string currStratum)
        {
            //  Need call to CP business layer to get all CUs for this stratum

            float stratumAcres = 0;
            foreach (StratumDO sdo in Global.BL.GetCurrentStratum(currStratum))
            {
                sdo.CuttingUnits.Populate();
                foreach (CuttingUnitDO cudo in sdo.CuttingUnits)
                    stratumAcres += cudo.Area;
                
            }   //  end foreach loop
            return stratumAcres;
        }   //  end AcresLookup


        public static double ReturnCorrectAcres(string currentStratum, long currStrCN)
        {
            string currentMethod = MethodLookup(currentStratum);
            if (currentMethod == "100" || currentMethod == "STR" ||
                currentMethod == "S3P" || currentMethod == "3P")
                return 1.0;
            return AcresLookup(currStrCN, currentStratum);
        }   //  end ReturnCorrectAcres

/*  following code is for change to capture stratum acres and needs testing when DAL is updated -- April 2015
        public static double ReturnCorrectAcres(StratumDO cs, CPbusinessLayer bslyr)
        {
            double strataAcres = 0;
            if (cs.Method == "100" || cs.Method == "STR" ||
                cs.Method == "S3P" || cs.Method == "3P")
                return 1.0;
            else
            {
                //  pull units for current stratum
                List<CuttingUnitStratumDO> justStratum = Global.BL.GetJustStratum(cs.Stratum_CN);
                //  sum area
                strataAcres = justStratum.Sum(j => j.StrataArea);
                if (strataAcres == 0)
                {
                    //  find acres for all units
                    strataAcres = unitAcres(justStratum, gbslyr);
                    return strataAcres;
                }
                else if (strataAcres > 0)
                    return strataAcres;
            }   //  endif
            return strataAcres;
        }   //  end ReturnCorrectAcres
        */

        public static double unitAcres(List<CuttingUnitStratumDO> justStratum)
        {
            double acresSum = 0;

            foreach (CuttingUnitStratumDO js in justStratum)
            {
                CuttingUnitDO cut = Global.BL.getCuttingUnits().FirstOrDefault(c => c.CuttingUnit_CN == js.CuttingUnit_CN);
                if (cut != null)
                    acresSum += cut.Area;
            }  //  end foreach loop

            return acresSum;
        }   //  end unitAcres


        public static string MethodLookup(string currentStratum)
        {
            //  Need to call CP business layer to get stratum list

            return Global.BL.GetCurrentStratum(currentStratum).FirstOrDefault()?.Method ?? "";
            //List<StratumDO> stratumList = Global.BL.GetCurrentStratum(currentStratum);
            //if (stratumList.Count > 0)
            //    return stratumList[0].Method;
            //else return "";            
        }   //  end MethodLookup


        public static string CurrentDLLversion()
        {
            //  Get current DLL version from volume library
            int iCurrentDate = 0;
            CalculateTreeValues.VERNUM2(ref iCurrentDate);

            //  Convert to a string to reformat date
            string sTemp = Convert.ToString(iCurrentDate);
            StringBuilder sDate = new StringBuilder();
            sDate.Append(sTemp.Substring(4,2));
            sDate.Append(".");
            sDate.Append(sTemp.Substring(6,2));
            sDate.Append(".");
            sDate.Append(sTemp.Substring(0, 4));

            return sDate.ToString();
        }   //  end CurrentDLLversion


        public static StringBuilder FormatField(float fieldToFormat, string formatOfField)
        {
            StringBuilder SB = new StringBuilder();
            SB.Remove(0, SB.Length);
            SB.AppendFormat(formatOfField, fieldToFormat);
            return SB;
        }   //  end FormatField for floats

        public static StringBuilder FormatField(double fieldToFormat, string formatOfField)
        {
            StringBuilder SB = new StringBuilder();
            SB.Remove(0, SB.Length);
            SB.AppendFormat(formatOfField, fieldToFormat);
            return SB;
        }   //  end FormatField for doubles

        public static StringBuilder FormatField(long fieldToFormat, string formatOfField)
        {
            StringBuilder SB = new StringBuilder();
            SB.Remove(0, SB.Length);
            SB.AppendFormat(formatOfField, fieldToFormat);
            return SB;
        }   //  end FormatField for longs

        public static StringBuilder FormatField(int fieldToFormat, string formatOfField)
        {
            StringBuilder SB = new StringBuilder();
            SB.Remove(0, SB.Length);
            SB.AppendFormat(formatOfField, fieldToFormat);
            return SB;
        }   //  end FormatField for int


        public static void LogError(string tableName, int table_CN, string errLevel, string errMessage)
        {
            List<ErrorLogDO> errList = new List<ErrorLogDO>();
            ErrorLogDO eldo = new ErrorLogDO();

            eldo.TableName = tableName;
            eldo.CN_Number = table_CN;
            eldo.Level = errLevel;
            eldo.ColumnName = "Volume";
            eldo.Message = errMessage;
            eldo.Program = "CruiseProcessing";
            errList.Add(eldo);

        }   //  end LogError6

        public static StringBuilder GetIdentifier(string tableName, long CNtoFind)
        {
            StringBuilder ident = new StringBuilder();

            switch(tableName)
            {
                case "Sale":
                    SaleDO sale = Global.BL.getSale().FirstOrDefault(sd => sd.Sale_CN == CNtoFind);
                    if(sale != null)
                    {
                        ident.Append("Sale number = ");
                        ident.Append(sale.SaleNumber);
                    }
                    else ident.Append("Sale number not found");
                    break;
                case "Stratum":
                    StratumDO strat = Global.BL.getStratum().FirstOrDefault(sdo => sdo.Stratum_CN == CNtoFind);
                    if(strat != null)
                        ident.Append(strat.Code);
                    else ident.Append("Stratum code not found");
                    break;
                case "Cutting Unit":
                    CuttingUnitDO cudo = Global.BL.getCuttingUnits().FirstOrDefault(cu => cu.CuttingUnit_CN == CNtoFind);
                    if(cudo != null)
                    {
                        ident.Append("   ");
                        ident.Append(cudo.Code.PadLeft(3,' '));
                    }
                    else ident.Append("Cutting unit not found");
                    break;
                case "Tree":
                    TreeDO tdo = Global.BL.getTrees().FirstOrDefault(td => td.Tree_CN == CNtoFind);
                    if(tdo != null)
                    {
                        ident.Append(tdo.Stratum.Code.PadRight(3,' '));
                        ident.Append(tdo.CuttingUnit.Code.PadLeft(3,' '));
                        if (tdo.Plot == null)
                            ident.Append("     ");
                       else if(tdo.Plot_CN == 0)
                            ident.Append("     ");
                        else ident.Append(tdo.Plot.PlotNumber.ToString().PadLeft(5,' '));
                        ident.Append(tdo.TreeNumber.ToString().PadLeft(5,' '));
                        ident.Append(" --- ");
                        if (tdo.Species == null)
                            ident.Append("       ");
                        else ident.Append(tdo.Species.PadRight(7, ' '));
                        if (tdo.SampleGroup == null)
                            ident.Append("   ");
                        else
                        {
                            if (tdo.SampleGroup.Code == "" || tdo.SampleGroup.Code == " " ||
                                tdo.SampleGroup.Code == "<Blank>" || tdo.SampleGroup.Code == null)
                                ident.Append("   ");
                            else ident.Append(tdo.SampleGroup.Code.PadRight(3, ' '));
                            ident.Append(tdo.SampleGroup.PrimaryProduct.PadRight(3, ' '));
                        }   //  endif
                    }
                    else ident.Append("Tree not found");
                    break;
                case "Log":
                    LogDO log = Global.BL.getLogs().FirstOrDefault(ld => ld.Log_CN == CNtoFind);
                    if (log != null)
                    {
                        ident.Append(log.Tree.Stratum.Code.PadRight(3,' '));
                        ident.Append(log.Tree.CuttingUnit.Code.PadLeft(3,' '));
                        if (log.Tree.Plot == null)
                            ident.Append("     ");
                        else ident.Append(log.Tree.Plot.PlotNumber.ToString().PadLeft(5,' '));
                        ident.Append(log.Tree.TreeNumber.ToString().PadLeft(5,' '));
                        ident.Append(log.LogNumber.PadLeft(3, ' '));
                    }
                    else ident.Append("Log not found");
                    break;
                case "Volume Equation":
                    if (CNtoFind == 0) CNtoFind = 1;
                    //List<VolumeEquationDO> vList = Global.BL.getVolumeEquations();
                    VolumeEquationDO ve = Global.BL.getVolumeEquations().ElementAt((int)CNtoFind - 1);
                    ident.Append("-- --- ---- ---- --- ");
                    ident.Append(ve.Species.PadRight(7, ' '));
                    ident.Append("-- ");
                    ident.Append(ve.PrimaryProduct.PadRight(3, ' '));
                    ident.Append(ve.VolumeEquationNumber.PadRight(10, ' '));

                    //ident.Append("-- --- ---- ---- --- ");
                    //ident.Append(vList[(int)CNtoFind-1].Species.PadRight(7, ' '));
                    //ident.Append("-- ");
                    //ident.Append(vList[(int)CNtoFind-1].PrimaryProduct.PadRight(3, ' '));
                    //ident.Append(vList[(int)CNtoFind-1].VolumeEquationNumber.PadRight(10,' '));
                    break;
                case "Value Equation":
                    if(CNtoFind == 0) CNtoFind = 1;
                    ValueEquationDO veq = Global.BL.getValueEquations().ElementAt((int)CNtoFind - 1);
                    ident.Append("-- --- ---- ---- --- ");
                    ident.Append(veq.Species.PadRight(7, ' '));
                    ident.Append("-- ");
                    ident.Append(veq.PrimaryProduct.PadRight(3, ' '));
                    ident.Append(veq.ValueEquationNumber.PadRight(10, ' '));
                    //List<ValueEquationDO> veList = Global.BL.getValueEquations();
                    //ident.Append("-- --- ---- ---- --- ");
                    //ident.Append(veList[(int)CNtoFind-1].Species.PadRight(7, ' '));
                    //ident.Append("-- ");
                    //ident.Append(veList[(int)CNtoFind-1].PrimaryProduct.PadRight(3,' '));
                    //ident.Append(veList[(int)CNtoFind-1].ValueEquationNumber.PadRight(10,' '));
                    break;
                case "Quality Adjustment":
                    if(CNtoFind == 0) CNtoFind = 1;
                    QualityAdjEquationDO qe = Global.BL.getQualAdjEquations().ElementAt((int)CNtoFind - 1);
                    ident.Append("-- --- ---- ---- --- ");
                    ident.Append(qe.Species.PadRight(7, ' '));
                    ident.Append("-- -- ");
                    ident.Append(qe.QualityAdjEq.PadRight(10,' '));
                    //List<QualityAdjEquationDO> qList = Global.BL.getQualAdjEquations();
                    //ident.Append("-- --- ---- ---- --- ");
                    //ident.Append(qList[(int)CNtoFind-1].Species.PadRight(7, ' '));
                    //ident.Append("-- -- ");
                    //ident.Append(qList[(int)CNtoFind-1].QualityAdjEq.PadRight(10,' '));
                    break;
                case "SampleGroup":
                    SampleGroupDO sg = Global.BL.getSampleGroups().FirstOrDefault(sgd => sgd.SampleGroup_CN == CNtoFind);
                    if(sg != null)
                    {
                        ident.Append(sg.Stratum.Code.PadRight(3,' '));
                        ident.Append("--- ---- ---- --- ------ ");
                        ident.Append(sg.Code.PadRight(3, ' '));
                        ident.Append(sg.PrimaryProduct.PadRight(3, ' '));
                    }
                    else ident.Append("Sample Group not found");
                    break;
            }   //  end switch
            return ident;
        }   //  end GetIdentifier


        public static bool IsFileInUse(string fileToCheck)
        {
            FileStream fs = null;
            
            try
            {
                fs = File.Create(fileToCheck);
            }
            catch (IOException e)
            {
                string nResult = e.GetBaseException().ToString();
                //  the file is unavailable because it is
                //  still being written to
                //  or being processed by another thread
                //  or does not exist
                return true;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return false;
        }   //  end IsFileInUse
    }
}
