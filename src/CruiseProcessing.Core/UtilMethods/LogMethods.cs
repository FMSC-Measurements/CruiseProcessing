using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;

namespace CruiseProcessing
{
    public class LogMethods
    {

        //  methods pertaining to the log table or the logstock table
        public static List<LogDO> GetLogRecords(List<LogDO> logList, long currTree_CN)
        {
            //  returns logs for specific tree
            List<LogDO> rtrnList = logList.FindAll(
                delegate (LogDO ld)
                {
                    return ld.Tree_CN == currTree_CN;
                });
            return rtrnList;
        }   //  end GetLogRecords

        public List<LogStockDO> GetLogStockRecords(List<LogStockDO> logStockList, int currTree_CN)
        {
            //  returns logs for specific tree
            List<LogStockDO> rtrnList = logStockList.FindAll(
                delegate (LogStockDO lsd)
                {
                    return lsd.Tree_CN == currTree_CN;
                });
            return rtrnList;
        }   //  end GetLogStockRecords

        //  build functions for printing
        public static List<string> buildPrintArray(List<LogDO> currLogs, int begLog, int endLog)
        {
            var logArray = new List<string>();

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
                        if (currLogs[k].Grade == "" || currLogs[k].Grade == " " || currLogs[k].Grade == null)
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
                        if (currLogs[k].Grade == "" || currLogs[k].Grade == " " || currLogs[k].Grade == null)
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
                        if (currLogs[k].Grade == "" || currLogs[k].Grade == " " || currLogs[k].Grade == null)
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
                        if (currLogs[k].Grade == "" || currLogs[k].Grade == " " || currLogs[k].Grade == null)
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
                        if (currLogs[k].Grade == "" || currLogs[k].Grade == " " || currLogs[k].Grade == null)
                            logArray.Add("0");
                        else logArray.Add(currLogs[k].Grade);
                        logArray.Add(currLogs[k].SeenDefect.ToString().PadLeft(3, ' '));
                        logArray.Add(currLogs[k].PercentRecoverable.ToString().PadLeft(3, ' '));
                        break;
                }   //  end switch
            }   //  end for k loop

            return logArray;
        }   //  end buildPrintArray

        public static List<string> buildPrintArray(LogStockDO lsdo)
        {
            //  builds line for fall, buck and scale report (A09)
            string fieldFormat2 = "{0,5:F0}";
            string fieldFormat3 = "{0,5:F1}";

            var logArray = new List<string>();
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
            logArray.Add(String.Format(fieldFormat2, lsdo.SmallEndDiameter).PadLeft(5, ' '));
            logArray.Add(String.Format(fieldFormat2, lsdo.LargeEndDiameter).PadLeft(5, ' '));
            logArray.Add(lsdo.Length.ToString().PadLeft(4, ' '));
            if (lsdo.Grade == null || lsdo.Grade == "" || lsdo.Grade == " ")
                logArray.Add("0");
            else logArray.Add(lsdo.Grade);
            logArray.Add(lsdo.GrossBoardFoot.ToString().PadLeft(7, ' '));
            logArray.Add(String.Format(fieldFormat3, lsdo.GrossCubicFoot).PadLeft(5, ' '));
            logArray.Add(lsdo.NetBoardFoot.ToString().PadLeft(7, ' '));
            logArray.Add(String.Format(fieldFormat3, lsdo.NetCubicFoot).PadLeft(5, ' '));
            logArray.Add(lsdo.PercentRecoverable.ToString().PadLeft(3, ' '));
            logArray.Add(lsdo.SeenDefect.ToString().PadLeft(3, ' '));

            return logArray;
        }   //  end buildPrintArray

        public static List<string> buildPrintArray(LogStockDO lsdo, double totalEF)
        {
            //  builds line for log file report (L1)
            string fieldFormat1 = "{0,5:F1}";
            string fieldFormat3 = "{0,3:F0}";
            string fieldFormat4 = "{0,4:F0}";
            string fieldFormat5 = "{0,6:F1}";
            string fieldFormat6 = "{0,8:F3}";

            var logArray = new List<string>();
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
            logArray.Add(String.Format(fieldFormat1, lsdo.SmallEndDiameter).PadLeft(5, ' '));
            logArray.Add(String.Format(fieldFormat1, lsdo.LargeEndDiameter).PadLeft(5, ' '));
            logArray.Add(String.Format(fieldFormat1, lsdo.Length).PadLeft(5, ' '));
            if (lsdo.Grade == null || lsdo.Grade == "" || lsdo.Grade == " ")
                logArray.Add("0");
            else logArray.Add(lsdo.Grade);
            logArray.Add(String.Format(fieldFormat3, lsdo.SeenDefect).PadLeft(3, ' '));
            logArray.Add(String.Format(fieldFormat4, lsdo.PercentRecoverable).PadLeft(4, ' '));
            logArray.Add(String.Format(fieldFormat5, lsdo.GrossBoardFoot).PadLeft(6, ' '));
            logArray.Add(String.Format(fieldFormat5, lsdo.BoardFootRemoved).PadLeft(6, ' '));
            logArray.Add(String.Format(fieldFormat5, lsdo.NetBoardFoot).PadLeft(6, ' '));
            logArray.Add(String.Format(fieldFormat5, lsdo.GrossCubicFoot).PadLeft(6, ' '));
            logArray.Add(String.Format(fieldFormat5, lsdo.CubicFootRemoved).PadLeft(6, ' '));
            logArray.Add(String.Format(fieldFormat5, lsdo.NetCubicFoot).PadLeft(6, ' '));
            logArray.Add(lsdo.DIBClass.ToString().PadLeft(2, ' '));
            logArray.Add(String.Format(fieldFormat6, totalEF).PadLeft(9, ' '));

            return logArray;
        }   //  end buildPrintArray
    }
}