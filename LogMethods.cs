﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public class LogMethods
    {
        ErrorLogMethods elm = new ErrorLogMethods();
        //  edit checks on log table
        public int CheckNumberLogs(List<LogDO> logList)
        {
            int errorsFound = 0;
            long prevTree_CN = -1;
            foreach (LogDO ld in logList)
            {
                if (prevTree_CN != ld.Tree_CN)
                {
                    prevTree_CN = Convert.ToInt64(ld.Tree_CN);
                    List<LogDO> justLogs = GetLogRecords(logList, Convert.ToInt64(ld.Tree_CN));
                    if (justLogs.Count > 20)
                    {
                        elm.LoadError("Log", "E", "13", (long)ld.Log_CN, "NoName");
                        errorsFound++;
                    }   //  endif logs count
                }   //  endif 
            }   //  end foreach loop
            return errorsFound;
        }   //  end CheckNumberLogs


        public int CheckFBS(List<LogDO> logList)
        {
            int errorsFound = 0;
            bool netGross = false;
            List<LogDO> justFBS = logList.FindAll(
                delegate(LogDO l)
                {
                    return l.Tree.IsFallBuckScale == 1;
                });
            foreach (LogDO j in justFBS)
            {
                //  check board and cubic volume for net larger than gross
                if (j.NetBoardFoot > j.GrossBoardFoot)
                    netGross = true;
                if (j.NetCubicFoot > j.GrossCubicFoot)
                    netGross = true;
             
                if (netGross == true)
                {
                    elm.LoadError("Log", "E", "20", (long)j.Log_CN, "GrossVolume");
                    errorsFound++;
                    netGross = false;
                }   //  endif net greater than gross
            }   //  end foreach loop
            return errorsFound;
        }   //  end CheckFBS


        public int CheckVLL(List<LogDO> logList)
        {
            int errorsFound = 0;
            foreach (LogDO ld in logList)
            {
                if (ld.LogNumber == "" || ld.LogNumber == " " || ld.LogNumber == null)
                {
                    elm.LoadError("Log", "E", "19", (long)ld.Log_CN, "LogNumber");
                    errorsFound++;
                }   //  endif log number missing
                if (ld.Grade == "" || ld.Grade == " " || ld.Grade == null)
                {
                    elm.LoadError("Log", "E", "19", (long)ld.Log_CN, "Grade");
                    errorsFound++;
                }   //  endif log grade missing
                if (ld.ExportGrade == "" || ld.ExportGrade == " " || ld.ExportGrade == null)
                {
                    elm.LoadError("Log", "E", "19", (long)ld.Log_CN, "ExportGrade");
                    errorsFound++;
                }   //  endif export grade (log sort) missing
                if (ld.Length == 0)
                {
                    elm.LoadError("Log", "E", "19", (long)ld.Log_CN, "Length");
                    errorsFound++;
                }   //  endif log length missing
            }   //  end foreach loop
            return errorsFound;
        }   //  end CheckVLL


        public int CheckDefect(List<LogDO> logList)
        {
            int errorsFound = 0;
            foreach (LogDO ld in logList)
            {
                if (ld.PercentRecoverable > ld.SeenDefect)
                {
                    elm.LoadError("Log", "E", "29", (long)ld.Log_CN, "PercentRecoverable");
                    errorsFound++;
                }
            }   //  end foreach

            return errorsFound;
        }   //  end CheckDefect


        //  Added a check for log grade of null
        public int CheckLogGrade(List<LogDO> logList)
        {
            int errorsFound = 0;
            foreach (LogDO ld in logList)
            {
                if(ld.Grade == null)
                {
                    elm.LoadError("Log","E","9",(long)ld.Log_CN, "Loggrae");
                    errorsFound++;
                }   //  endif

            }   //  end foreach
            return errorsFound;
        }   //  end CheckLogGrade


        //  methods pertaining to the log table or the logstock table
        public List<LogDO> GetLogRecords(List<LogDO> logList, long currTree_CN)
        {
            //  returns logs for specific tree
            List<LogDO> rtrnList = logList.FindAll(
                delegate(LogDO ld)
                {
                    return ld.Tree_CN == currTree_CN;
                });
            return rtrnList;
        }   //  end GetLogRecords


        public List<LogStockDO> GetLogStockRecords(List<LogStockDO> logStockList, int currTree_CN)
        {
            //  returns logs for specific tree
            List<LogStockDO> rtrnList = logStockList.FindAll(
                delegate(LogStockDO lsd)
                {
                    return lsd.Tree_CN == currTree_CN;
                });
            return rtrnList;
        }   //  end GetLogStockRecords


        //  build functions for printing
        public ArrayList buildPrintArray(List<LogDO> currLogs, int begLog, int endLog)
        {
            ArrayList logArray = new ArrayList();

            logArray.Add(" ");
            logArray.Add(currLogs[0].Tree.Stratum.Code.PadLeft(2, ' '));
            logArray.Add(currLogs[0].Tree.CuttingUnit.Code.PadLeft(3, ' '));
            if (currLogs[0].Tree == null || currLogs[0].Tree.Plot == null || currLogs[0].Tree.Plot.PlotNumber == 0)
                logArray.Add("    ");
            else logArray.Add(currLogs[0].Tree.Plot.PlotNumber.ToString().PadLeft(4, ' '));
            logArray.Add(currLogs[0].Tree.TreeNumber.ToString().PadLeft(4, ' '));

            for (int k = begLog; k <= endLog; k++)
            {
                switch (k)
                {
                    case 0:         // log number one
                    case 5:
                    case 10:
                    case 15:
                        logArray.Add(currLogs[k].LogNumber.ToString().PadLeft(4, ' '));
                        if(currLogs[k].Grade == "" || currLogs[k].Grade == " " || currLogs[k].Grade == null)
                            logArray.Add("0");
                        else logArray.Add(currLogs[k].Grade);
                        logArray.Add(currLogs[k].SeenDefect.ToString().PadLeft(3, ' '));
                        logArray.Add(currLogs[k].PercentRecoverable.ToString().PadLeft(3, ' '));
                        break;
                    case 1:         //  log number two
                    case 6:
                    case 11:
                    case 16:
                        logArray.Add(currLogs[k].LogNumber.ToString().PadLeft(4, ' '));
                        if(currLogs[k].Grade == "" || currLogs[k].Grade == " " || currLogs[k].Grade == null)
                            logArray.Add("0");
                        else logArray.Add(currLogs[k].Grade);
                        logArray.Add(currLogs[k].SeenDefect.ToString().PadLeft(3, ' '));
                        logArray.Add(currLogs[k].PercentRecoverable.ToString().PadLeft(3, ' '));
                        break;
                    case 2:         //  log number three
                    case 7:
                    case 12:
                    case 17:
                        logArray.Add(currLogs[k].LogNumber.ToString().PadLeft(4, ' '));
                        if(currLogs[k].Grade == "" || currLogs[k].Grade == " " || currLogs[k].Grade == null)
                            logArray.Add("0");
                        else logArray.Add(currLogs[k].Grade);
                        logArray.Add(currLogs[k].SeenDefect.ToString().PadLeft(3, ' '));
                        logArray.Add(currLogs[k].PercentRecoverable.ToString().PadLeft(3, ' '));
                        break;
                    case 3:         //  log number four
                    case 8:
                    case 13:
                    case 18:
                        logArray.Add(currLogs[k].LogNumber.ToString().PadLeft(4, ' '));
                        if(currLogs[k].Grade == "" || currLogs[k].Grade == " " || currLogs[k].Grade == null)
                            logArray.Add("0");
                        else logArray.Add(currLogs[k].Grade);
                        logArray.Add(currLogs[k].SeenDefect.ToString().PadLeft(3, ' '));
                        logArray.Add(currLogs[k].PercentRecoverable.ToString().PadLeft(3, ' '));
                        break;
                    case 4:         //  log number five
                    case 9:
                    case 14:
                    case 19:
                        logArray.Add(currLogs[k].LogNumber.ToString().PadLeft(4, ' '));
                        if(currLogs[k].Grade == "" || currLogs[k].Grade == " " || currLogs[k].Grade == null)
                            logArray.Add("0");
                        else logArray.Add(currLogs[k].Grade);
                        logArray.Add(currLogs[k].SeenDefect.ToString().PadLeft(3, ' '));
                        logArray.Add(currLogs[k].PercentRecoverable.ToString().PadLeft(3, ' '));
                        break;
                }   //  end switch
            }   //  end for k loop

            return logArray;
        }   //  end buildPrintArray


        public ArrayList buildPrintArray(LogStockDO lsdo)
        {
            //  builds line for fall, buck and scale report (A09)
            string fieldFormat2 = "{0,5:F0}";
            string fieldFormat3 = "{0,5:F1}";

            ArrayList logArray = new ArrayList();
            logArray.Add(" ");
            logArray.Add(lsdo.Tree.Stratum.Code.PadLeft(2, ' '));
            logArray.Add(lsdo.Tree.CuttingUnit.Code.PadLeft(3, ' '));
            if (lsdo.Tree.Plot != null)
            {
                if (lsdo.Tree.Plot.PlotNumber == 0)
                    logArray.Add("    ");
                else logArray.Add(lsdo.Tree.Plot.PlotNumber.ToString().PadLeft(4, ' '));
            }
            else logArray.Add("    ");
            logArray.Add(lsdo.Tree.TreeNumber.ToString().PadLeft(4, ' '));
            logArray.Add(lsdo.LogNumber.ToString().PadLeft(4, ' '));
            logArray.Add(Utilities.FormatField(lsdo.SmallEndDiameter,fieldFormat2).ToString().PadLeft(5, ' '));
            logArray.Add(Utilities.FormatField(lsdo.LargeEndDiameter,fieldFormat2).ToString().PadLeft(5, ' '));
            logArray.Add(lsdo.Length.ToString().PadLeft(4, ' '));
            if (lsdo.Grade == null || lsdo.Grade == "" || lsdo.Grade == " ")
                logArray.Add("0");
            else logArray.Add(lsdo.Grade);
            logArray.Add(lsdo.GrossBoardFoot.ToString().PadLeft(7, ' '));
            logArray.Add(Utilities.FormatField(lsdo.GrossCubicFoot,fieldFormat3).ToString().PadLeft(5, ' '));
            logArray.Add(lsdo.NetBoardFoot.ToString().PadLeft(7, ' '));
            logArray.Add(Utilities.FormatField(lsdo.NetCubicFoot,fieldFormat3).ToString().PadLeft(5, ' '));
            logArray.Add(lsdo.PercentRecoverable.ToString().PadLeft(3, ' '));
            logArray.Add(lsdo.SeenDefect.ToString().PadLeft(3, ' '));

            return logArray;
        }   //  end buildPrintArray


        public ArrayList buildPrintArray(LogStockDO lsdo, double totalEF)
        {
            //  builds line for log file report (L1)
            string fieldFormat1 = "{0,5:F1}";
            string fieldFormat3 = "{0,3:F0}";
            string fieldFormat4 = "{0,4:F0}";
            string fieldFormat5 = "{0,6:F1}";
            string fieldFormat6 = "{0,8:F3}";

            ArrayList logArray = new ArrayList();
            logArray.Add("  ");
            logArray.Add(lsdo.Tree.Stratum.Code.PadLeft(2, ' '));
            logArray.Add(lsdo.Tree.CuttingUnit.Code.PadLeft(3, ' '));
            if (lsdo.Tree.Plot == null)
                logArray.Add("    ");
            else if (lsdo.Tree.Plot.PlotNumber == 0)
                logArray.Add("    ");
            else logArray.Add(lsdo.Tree.Plot.PlotNumber.ToString().PadLeft(4, ' '));
            logArray.Add(lsdo.Tree.TreeNumber.ToString().PadLeft(4, ' '));
            logArray.Add(lsdo.Tree.Species.PadRight(6, ' '));
            logArray.Add(lsdo.Tree.SampleGroup.PrimaryProduct.PadLeft(2, ' '));
            logArray.Add(lsdo.Tree.SampleGroup.UOM.PadLeft(2, ' '));
            logArray.Add(lsdo.LogNumber.ToString().PadLeft(4, ' '));
            logArray.Add(Utilities.FormatField(lsdo.SmallEndDiameter, fieldFormat1).ToString().PadLeft(5, ' '));
            logArray.Add(Utilities.FormatField(lsdo.LargeEndDiameter, fieldFormat1).ToString().PadLeft(5, ' '));
            logArray.Add(Utilities.FormatField(lsdo.Length, fieldFormat1).ToString().PadLeft(5, ' '));
            if (lsdo.Grade == null || lsdo.Grade == "" || lsdo.Grade == " ")
                logArray.Add("0");
            else logArray.Add(lsdo.Grade);
            logArray.Add(Utilities.FormatField(lsdo.SeenDefect, fieldFormat3).ToString().PadLeft(3, ' '));
            logArray.Add(Utilities.FormatField(lsdo.PercentRecoverable, fieldFormat4).ToString().PadLeft(4, ' '));
            logArray.Add(Utilities.FormatField(lsdo.GrossBoardFoot,fieldFormat5).ToString().PadLeft(6,' '));
            logArray.Add(Utilities.FormatField(lsdo.BoardFootRemoved,fieldFormat5).ToString().PadLeft(6,' '));
            logArray.Add(Utilities.FormatField(lsdo.NetBoardFoot,fieldFormat5).ToString().PadLeft(6,' '));
            logArray.Add(Utilities.FormatField(lsdo.GrossCubicFoot, fieldFormat5).ToString().PadLeft(6, ' '));
            logArray.Add(Utilities.FormatField(lsdo.CubicFootRemoved, fieldFormat5).ToString().PadLeft(6, ' '));
            logArray.Add(Utilities.FormatField(lsdo.NetCubicFoot, fieldFormat5).ToString().PadLeft(6, ' '));
            logArray.Add(lsdo.DIBClass.ToString().PadLeft(2, ' '));
            logArray.Add(Utilities.FormatField(totalEF, fieldFormat6).ToString().PadLeft(9, ' '));

            return logArray;
        }   //  end buildPrintArray
    }
}
