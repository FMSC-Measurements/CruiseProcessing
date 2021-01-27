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
        public static double AcresLookup(long currStrCN, CPbusinessLayer bslyr, string currStratum)
        {
            //  Need call to CP business layer to get all CUs for this stratum
            List<StratumDO> stratumList = bslyr.GetCurrentStratum(currStratum);

            float stratumAcres = 0;
            foreach (StratumDO sdo in stratumList)
            {
                sdo.CuttingUnits.Populate();
                foreach (CuttingUnitDO cudo in sdo.CuttingUnits)
                    stratumAcres += cudo.Area;
                
            }   //  end foreach loop
            return stratumAcres;
        }   //  end AcresLookup


        public static double ReturnCorrectAcres(string currentStratum, CPbusinessLayer bslyr, long currStrCN)
        {
            string currentMethod = MethodLookup(currentStratum, bslyr);
            if (currentMethod == "100" || currentMethod == "STR" ||
                currentMethod == "S3P" || currentMethod == "3P")
                return 1.0;
            return AcresLookup(currStrCN, bslyr, currentStratum);
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
                List<CuttingUnitStratumDO> justStratum = bslyr.GetJustStratum(cs.Stratum_CN);
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

        public static double unitAcres(List<CuttingUnitStratumDO> justStratum, CPbusinessLayer bslyr)
        {
            List<CuttingUnitDO> cutList = bslyr.getCuttingUnits();
            double acresSum = 0;

            foreach (CuttingUnitStratumDO js in justStratum)
            {
                int nthRow = cutList.FindIndex(
                    delegate(CuttingUnitDO c)
                    {
                        return c.CuttingUnit_CN == js.CuttingUnit_CN;
                    });
                if (nthRow >= 0)
                    acresSum += cutList[nthRow].Area;
            }  //  end foreach loop

            return acresSum;
        }   //  end unitAcres


        public static string MethodLookup(string currentStratum, CPbusinessLayer bslyr)
        {
            //  Need to call CP business layer to get stratum list
            List<StratumDO> stratumList = bslyr.GetCurrentStratum(currentStratum);
            if (stratumList.Count > 0)
                return stratumList[0].Method;
            else return "";            
        }   //  end MethodLookup


        public static string CurrentDLLversion()
        {
            try
            {
                //  Get current DLL version from volume library
                int iCurrentDate = 0;
                CalculateTreeValues.VERNUM2(ref iCurrentDate);

                //  Convert to a string to reformat date
                string sTemp = Convert.ToString(iCurrentDate);
                StringBuilder sDate = new StringBuilder();
                sDate.Append(sTemp.Substring(4, 2));
                sDate.Append(".");
                sDate.Append(sTemp.Substring(6, 2));
                sDate.Append(".");
                sDate.Append(sTemp.Substring(0, 4));

                return sDate.ToString();
            }
            catch
            {
                return "0.0.0.0";
            }
        }   //  end CurrentDLLversion


        public static void LogError(string tableName, int table_CN, string errLevel, string errMessage, string filename)
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

            CPbusinessLayer bslyr = new CPbusinessLayer();
            bslyr.fileName = filename;
            bslyr.SaveErrorMessages(errList);
        }   //  end LogError6

        public static StringBuilder GetIdentifier(string tableName, long CNtoFind, CPbusinessLayer bslyr)
        {
            StringBuilder ident = new StringBuilder();
            int ithRow = -1;

            switch(tableName)
            {
                case "Sale":
                    List<SaleDO> sList = bslyr.getSale();
                    ithRow = sList.FindIndex(
                        delegate(SaleDO sd)
                        {
                            return sd.Sale_CN == CNtoFind;
                        });
                    if(ithRow >= 0)
                    {
                        ident.Append("Sale number = ");
                        ident.Append(sList[ithRow].SaleNumber);
                    }
                    else ident.Append("Sale number not found");
                    break;
                case "Stratum":
                    List<StratumDO> stList = bslyr.getStratum();
                    ithRow = stList.FindIndex(
                        delegate(StratumDO sdo)
                        {
                            return sdo.Stratum_CN == CNtoFind;
                        });
                    if(ithRow >= 0)
                        ident.Append(stList[ithRow].Code);
                    else ident.Append("Stratum code not found");
                    break;
                case "Cutting Unit":
                    List<CuttingUnitDO> cList = bslyr.getCuttingUnits();
                    ithRow = cList.FindIndex(
                        delegate(CuttingUnitDO cu)
                        {
                            return cu.CuttingUnit_CN == CNtoFind;
                        });
                    if(ithRow >= 0)
                    {
                        ident.Append("   ");
                        ident.Append(cList[ithRow].Code.PadLeft(3,' '));
                    }
                    else ident.Append("Cutting unit not found");
                    break;
                case "Tree":
                    List<TreeDO> tList = bslyr.getTrees();
                    ithRow = tList.FindIndex(
                        delegate(TreeDO td)
                        {
                            return td.Tree_CN == CNtoFind;
                        });
                    if(ithRow >= 0)
                    {
                        ident.Append(tList[ithRow].Stratum.Code.PadRight(3,' '));
                        ident.Append(tList[ithRow].CuttingUnit.Code.PadLeft(3,' '));
                        if (tList[ithRow].Plot == null)
                            ident.Append("     ");
                       else if(tList[ithRow].Plot_CN == 0)
                            ident.Append("     ");
                        else ident.Append(tList[ithRow].Plot.PlotNumber.ToString().PadLeft(5,' '));
                        ident.Append(tList[ithRow].TreeNumber.ToString().PadLeft(5,' '));
                        ident.Append(" --- ");
                        if (tList[ithRow].Species == null)
                            ident.Append("       ");
                        else ident.Append(tList[ithRow].Species.PadRight(7, ' '));
                        if (tList[ithRow].SampleGroup == null)
                            ident.Append("   ");
                        else
                        {
                            if (tList[ithRow].SampleGroup.Code == "" || tList[ithRow].SampleGroup.Code == " " ||
                                tList[ithRow].SampleGroup.Code == "<Blank>" || tList[ithRow].SampleGroup.Code == null)
                                ident.Append("   ");
                            else ident.Append(tList[ithRow].SampleGroup.Code.PadRight(3, ' '));
                            ident.Append(tList[ithRow].SampleGroup.PrimaryProduct.PadRight(3, ' '));
                        }   //  endif
                    }
                    else ident.Append("Tree not found");
                    break;
                case "Log":
                    List<LogDO> lList = bslyr.getLogs();
                    ithRow = lList.FindIndex(
                        delegate(LogDO ld)
                        {
                            return ld.Log_CN == CNtoFind;
                        });
                    if(ithRow >= 0)
                    {
                        ident.Append(lList[ithRow].Tree.Stratum.Code.PadRight(3,' '));
                        ident.Append(lList[ithRow].Tree.CuttingUnit.Code.PadLeft(3,' '));
                        if (lList[ithRow].Tree.Plot == null)
                            ident.Append("     ");
                        else ident.Append(lList[ithRow].Tree.Plot.PlotNumber.ToString().PadLeft(5,' '));
                        ident.Append(lList[ithRow].Tree.TreeNumber.ToString().PadLeft(5,' '));
                        ident.Append(lList[ithRow].LogNumber.PadLeft(3, ' '));
                    }
                    else ident.Append("Log not found");
                    break;
                case "Volume Equation":
                    if(CNtoFind == 0) CNtoFind = 1;
                    List<VolumeEquationDO> vList = bslyr.getVolumeEquations();
                    ident.Append("-- --- ---- ---- --- ");
                    ident.Append(vList[(int)CNtoFind-1].Species.PadRight(7, ' '));
                    ident.Append("-- ");
                    ident.Append(vList[(int)CNtoFind-1].PrimaryProduct.PadRight(3, ' '));
                    ident.Append(vList[(int)CNtoFind-1].VolumeEquationNumber.PadRight(10,' '));
                    break;
                case "Value Equation":
                    if(CNtoFind == 0) CNtoFind = 1;
                    List<ValueEquationDO> veList = bslyr.getValueEquations();
                    ident.Append("-- --- ---- ---- --- ");
                    ident.Append(veList[(int)CNtoFind-1].Species.PadRight(7, ' '));
                    ident.Append("-- ");
                    ident.Append(veList[(int)CNtoFind-1].PrimaryProduct.PadRight(3,' '));
                    ident.Append(veList[(int)CNtoFind-1].ValueEquationNumber.PadRight(10,' '));
                    break;
                case "Quality Adjustment":
                    if(CNtoFind == 0) CNtoFind = 1;
                    List<QualityAdjEquationDO> qList = bslyr.getQualAdjEquations();
                    ident.Append("-- --- ---- ---- --- ");
                    ident.Append(qList[(int)CNtoFind-1].Species.PadRight(7, ' '));
                    ident.Append("-- -- ");
                    ident.Append(qList[(int)CNtoFind-1].QualityAdjEq.PadRight(10,' '));
                    break;
                case "SampleGroup":
                    List<SampleGroupDO> sgList = bslyr.getSampleGroups();
                    ithRow = sgList.FindIndex(
                        delegate(SampleGroupDO sgd)
                        {
                            return sgd.SampleGroup_CN == CNtoFind;
                        });
                    if(ithRow >= 0)
                    {
                        ident.Append(sgList[ithRow].Stratum.Code.PadRight(3,' '));
                        ident.Append("--- ---- ---- --- ------ ");
                        ident.Append(sgList[ithRow].Code.PadRight(3, ' '));
                        ident.Append(sgList[ithRow].PrimaryProduct.PadRight(3, ' '));
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
