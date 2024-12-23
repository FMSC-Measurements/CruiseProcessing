using CruiseDAL.DataObjects;
using CruiseProcessing.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Processing
{
    public class CalculateNetVolume2 : CalculateNetVolume
    {
        public void CalculateNetVolume(string cType, string region, string pProd, VolLibNVBoutput volOutput, IReadOnlyList<LogStockDO> logStockList, TreeDO tree, float minTopDIBprimary)
        {
            var defectLogic = 1;

            float tempHidden = 0;
            var tdv = tree.TreeDefaultValue;
            if (tree.HiddenPrimary == 0)
            {

                //  now check hidden primary in TreeDefaultValue
                if (tdv.HiddenPrimary > 0)
                    tempHidden = tdv.HiddenPrimary;
                else tempHidden = 0;
            }
            else tempHidden = tree.HiddenPrimary;
            if (cType == "V")
            {
                VariableLogLength(volOutput, pProd, logStockList, tdv.CullPrimary, tempHidden, tree.SeenDefectPrimary);
            }

            if (region == "02" || region == "04" || region == "07")
                defectLogic = 2;
            else if (region == "08")
                defectLogic = 3;
            else if (region == "10")
                defectLogic = 4;
            else if (region == "01" || region == "03" || region == "05" || region == "09")
                defectLogic = 1;

            var noLogsPrimary = (int)Math.Round(volOutput.NoLogsPrimary, MidpointRounding.AwayFromZero);
            var noLogsSecondary = (int)Math.Ceiling(volOutput.NoLogsSecondary);

            double soundDefault = 0;
            if (region == "05") soundDefault = 0.25;
            else if (region == "06") soundDefault = 0.02;
            else if (region == "07") soundDefault = 0.33;
            else if (region == "10") soundDefault = 0.33;

            //  call appropriate defect routine based on region --  default is VolumeTreeDefect
            //  some regions have multiple routines called
            if (region == "05" || region == "07" || region == "10")
            {
                if (!logStockList.Any() || volOutput.TotalLogs <= 0)
                {
                    VolumeTreeDefect(region, pProd, volOutput.Volumes,
                                     defectLogic, tdv.CullPrimary,
                                     tempHidden, tree.SeenDefectPrimary,
                                     tdv.CullSecondary, tdv.HiddenSecondary, tree.SeenDefectSecondary);
                }
                else
                {
                    if (region == "10")
                    {
                        if (volOutput.TotalLogs > 0)
                        { SetLogGrades(region, logStockList, tree.Grade, noLogsPrimary, volOutput.TotalLogs, tree.RecoverablePrimary); }
                        else
                        {
                            //  check for cull tree grades
                            if (tree.Grade == "8" || tree.Grade == "9")
                            {
                                foreach (LogStockDO lsdo in logStockList)
                                { lsdo.SeenDefect = 100; }
                            }   //  endif tree grade is cull
                        } 
                    }
                    else
                    { SetLogGrades(region, logStockList, tree.Grade, noLogsPrimary, volOutput.TotalLogs, tree.RecoverablePrimary); }

                    VolumeLogDefect(region, volOutput.LogVolumes, volOutput.Volumes, logStockList, tdv.CullPrimary,
                                    tempHidden, tree.SeenDefectPrimary,
                                    tdv.CullSecondary, tdv.HiddenSecondary,
                                    tree.SeenDefectSecondary, noLogsPrimary, noLogsSecondary, volOutput.TotalLogs,
                                    tree.RecoverablePrimary, soundDefault);

                    if (region == "07" || region == "10")
                    { LogUtil(region, pProd, volOutput, logStockList, noLogsPrimary); }

                    if (region == "05" || region == "10")
                        SetDiameterClass(region, logStockList, volOutput.TotalLogs);
                }
            }
            else if (region == "06")
            {
                if (logStockList.Count() > 0)
                {
                    GetR6LogGrade(tree.Grade, logStockList, volOutput.TotalLogs, noLogsPrimary, minTopDIBprimary);
                    VolumeLogDefect(region, volOutput.LogVolumes, volOutput.Volumes, logStockList, tdv.CullPrimary,
                                    tempHidden, tree.SeenDefectPrimary,
                                    tdv.CullSecondary, tdv.HiddenSecondary,
                                    tree.SeenDefectSecondary, noLogsPrimary, noLogsSecondary, volOutput.TotalLogs,
                                    tree.RecoverablePrimary, soundDefault);
                    LogUtil(region, pProd, volOutput, logStockList, noLogsPrimary);
                    SetDiameterClass(region, logStockList, volOutput.TotalLogs);
                }
            }
            else
            {   //  default -- includes regions 1, 2, 3, 4, 8 and 9
                VolumeTreeDefect(region, pProd, volOutput.Volumes,
                                     defectLogic, tdv.CullPrimary,
                                     tempHidden, tree.SeenDefectPrimary,
                                     tdv.CullSecondary, tdv.HiddenSecondary, tree.SeenDefectSecondary);
            }

            if (volOutput.TotalLogs > 0 && (region != "05" && region != "06" && region != "10"))
            {
                SetDiameterClass(region, logStockList, volOutput.TotalLogs);
            }
        }

        private static void VariableLogLength(VolLibNVBoutput volOutput, string pProd, IReadOnlyList<LogStockDO> logStockList,
                                                float cullDefect, float hiddenDefect, float seenDefect)
        {
            var newVolume = new Volumes();

            foreach (var i in Enumerable.Range(0, volOutput.TotalLogs))
            {
                var logStock = logStockList[i];
                if (logStock.Grade == "0")
                    logStock.SeenDefect = 100;

                var totalDefect = cullDefect + hiddenDefect + seenDefect + logStock.SeenDefect;

                var logVol = volOutput.LogVolumes[i];

                logVol.NetBoardFoot = (float)Math.Round(logVol.GrossBoardFoot * (1.0 - (totalDefect / 100.0)));
                logVol.NetCubicFoot = (float)Math.Round(logVol.GrossCubicFoot * (1.0 - (totalDefect / 100.0)));

                if (pProd == "01" && (new[] { "9", "A", "B" }.Contains(logStock.Grade)))
                {
                    newVolume.GrossSecondaryCuFt += logVol.GrossCubicFoot;
                    newVolume.NetSecondaryCuFt += logVol.NetCubicFoot;
                    newVolume.GrossSecondaryBdFt += logVol.GrossBoardFoot;
                    newVolume.NetSecondaryBdFt += logVol.NetBoardFoot;
                }
                else
                {
                    newVolume.GrossBdFt += logVol.GrossBoardFoot;
                    newVolume.NetBdFt += logVol.NetBoardFoot;
                    newVolume.GrossCuFt += logVol.GrossCubicFoot;
                    newVolume.NetCuFt += logVol.NetCubicFoot;
                }
            }

            volOutput.Volumes = newVolume;
        }

        private static void VolumeTreeDefect(string currRegn, string pProd, Volumes VOL, int defectLogic,
                                             float cullDefPrimary, float hiddenDefPrimary, float seenDefPrimary,
                                             float cullDefSecondary, float hiddenDefSecondary, float seenDefSecondary)
        {
            float totalPrimDef, totalSecDef;
            float breakageDef, hiddenDef, seenDef;

            //  Calculate nets based on defect logic
            if (defectLogic == 1 || defectLogic == 3)
            {
                totalPrimDef = 1 - (cullDefPrimary + hiddenDefPrimary + seenDefPrimary) / 100;
                totalSecDef = 1 - (cullDefSecondary + hiddenDefSecondary + seenDefSecondary) / 100;
                if (totalPrimDef < 0) totalPrimDef = 0;
                if (totalSecDef < 0) totalSecDef = 0;

                VOL[4] = VOL[3] * totalPrimDef;         //  Net CUFT primary volume
                VOL[7] = VOL[6] * totalSecDef;          //  Net CUFT secondary volume
                VOL[12] = VOL[11] * totalSecDef;        //  Net BDFT secondary volume
                VOL[2] = VOL[1] * totalPrimDef;         //  Net BDFT primary volume
                VOL[10] = VOL[9] * totalPrimDef;        //  Net International volume

                //  Region 9 applies defect weirdly.  Per MVanDyck, this is how it is to be applied.  June 2008 -- bem
                if (currRegn == "9" || currRegn == "09")
                {
                    if (pProd != "01")
                    {
                        //  Apply secondary defect to primary products
                        VOL[4] = VOL[3] * totalSecDef;      //  Net CUFT primary volume
                        VOL[2] = VOL[1] * totalSecDef;      //  Net BDFT primary volume
                    }   //  endif currPP
                }   //  endif currRegn
            }
            else if (defectLogic == 2)
            {
                totalPrimDef = (1 - cullDefPrimary / 100) * (1 - hiddenDefPrimary / 100) * (1 - seenDefPrimary / 100);
                totalSecDef = (1 - cullDefSecondary / 100) * (1 - hiddenDefSecondary / 100) * (1 - seenDefSecondary / 100);
                VOL[2] = totalPrimDef * VOL[1];     //  Net BDFT primary volume
                VOL[4] = totalPrimDef * VOL[3];     //  Net CUFT primary volume
                VOL[10] = totalPrimDef * VOL[9];    //  Net International volume
                VOL[7] = totalSecDef * VOL[6];      //  Net CUFT secondary volume
                VOL[12] = totalSecDef * VOL[11];    //  Net BDFT secondary volume
            }
            else if (defectLogic == 4)
            {
                breakageDef = 1 - (cullDefPrimary / 100);
                hiddenDef = 1 - (hiddenDefPrimary / 100);
                seenDef = 1 - (seenDefPrimary / 100);
                VOL[2] = ((VOL[1] * breakageDef) * seenDef) * hiddenDef;        //  Net BDFT primary volume
                VOL[4] = ((VOL[3] * breakageDef) * seenDef) * hiddenDef;        //  Net CUFT primary volume
            }   //  endif defectLogic

            //  Round net volumes
            VOL[2] = (float)Math.Round(VOL[2] + 0.001, 0, MidpointRounding.AwayFromZero);
            VOL[4] = (float)Math.Round(VOL[4] + 0.001, 1, MidpointRounding.AwayFromZero);
            VOL[7] = (float)Math.Round(VOL[7] + 0.001, 1, MidpointRounding.AwayFromZero);
            VOL[10] = (float)Math.Round(VOL[10] + 0.001, 0, MidpointRounding.AwayFromZero);
            VOL[12] = (float)Math.Round(VOL[12] + 0.001, 0, MidpointRounding.AwayFromZero);
        }

        private void VolumeLogDefect(string currRegn, IReadOnlyList<LogVolume> LOGVOL, Volumes VOL, IReadOnlyList<LogStockDO> logStockList,
                                            float cullDefPrimary, float hiddenDefPrimary, float seenDefPrimary,
                                            float cullDefSecondary, float hiddenDefSecondary, float seenDefSecondary,
                                            int numPPlogs, int numSPlogs, int TLOGS, float currentRec, double soundDefault)
        {
            Debug.Assert(numPPlogs + numSPlogs <= LOGVOL.Count);

            //  Calculate net log volume by region
            if (currRegn == "10")
            {
                VolumeLogDefect_R10(LOGVOL, logStockList, cullDefPrimary, hiddenDefPrimary, cullDefSecondary, hiddenDefSecondary, numPPlogs, TLOGS, currentRec);
            }
            else if (currRegn == "05")
            {
                VolumeLogDefect_R5(LOGVOL, logStockList, cullDefPrimary, hiddenDefPrimary, seenDefPrimary, cullDefSecondary, hiddenDefSecondary, seenDefSecondary, numPPlogs, TLOGS);
            }
            else if (currRegn == "07")
            {
                VolumeLogDefect_R7(LOGVOL, logStockList, cullDefPrimary, hiddenDefPrimary, seenDefPrimary, cullDefSecondary, hiddenDefSecondary, seenDefSecondary, numPPlogs, TLOGS);
            }
            else if (currRegn == "06")
            {
                VolumeLogDefect_R6(LOGVOL, logStockList, cullDefPrimary, hiddenDefPrimary, seenDefPrimary, cullDefSecondary, hiddenDefSecondary, seenDefSecondary, numPPlogs, TLOGS);
            }
            else                 //  all other regions
            {
                
                for (int n = 0; n < TLOGS; n++)
                {
                    float totalDefect;

                    if (n < numPPlogs)
                        totalDefect = cullDefPrimary + hiddenDefPrimary + seenDefPrimary + logStockList[n].SeenDefect;
                    else
                        totalDefect = cullDefSecondary + hiddenDefSecondary + seenDefSecondary + logStockList[n].SeenDefect;

                    if (totalDefect > 100) totalDefect = 100;

                    var logVol = LOGVOL[n];

                    //  Net board foot
                    logVol.NetBoardFoot = (float)Math.Round(logVol.GrossBoardFoot * (1.0 - (totalDefect / 100.0)), 0, MidpointRounding.AwayFromZero);
                    //  Net cubic foot
                    logVol.NetCubicFoot = (float)Math.Round(logVol.GrossCubicFoot * (1.0 - (totalDefect / 100.0)), 1, MidpointRounding.AwayFromZero);
                }   //  end for n loop
            }   //  endif current region

            //  Check for sound log
            float percentSound;
            for (int n = 0; n < TLOGS; n++)
            {
                var logVol = LOGVOL[n];

                if (logVol[ 0] > 0)
                {
                    percentSound = logVol[2] / logVol[0];
                    if (percentSound < soundDefault)
                    {
                        logVol.NetBoardFoot = 0;
                        if (currRegn == "06") logStockList[n].Grade = "9";
                    }   //  endif
                }   //  endif

                if (logVol[3] > 0)
                {
                    percentSound = logVol[5] / logVol[3];
                    if (percentSound < soundDefault)
                    {
                        logVol.NetCubicFoot = 0;
                        if (currRegn == "06") logStockList[n].Grade = "9";
                    }   //  endif
                }   //  endif
            }   //  end for n loop

            //  Sum log volumes into tree volumes
            for (int n = 0; n < numPPlogs; n++)
            {
                var logVol = LOGVOL[n];
                VOL.NetBdFt += logVol.NetBoardFoot;
                VOL.NetCuFt += logVol.NetCubicFoot;
            }   //  end for n loop

            for (int n = 0; n < numSPlogs; n++)
            {
                var secondaryLogVol = LOGVOL[n + numPPlogs];

                VOL.NetSecondaryCuFt += secondaryLogVol.NetCubicFoot;
                VOL.NetSecondaryBdFt += secondaryLogVol.NetBoardFoot;
            }   //  end for n loop
        }

        private static void VolumeLogDefect_R5(IReadOnlyList<LogVolume> LOGVOL, IReadOnlyList<LogStockDO> logStockList, float cullDefPrimary, float hiddenDefPrimary, float seenDefPrimary, float cullDefSecondary, float hiddenDefSecondary, float seenDefSecondary, int numPPlogs, int TLOGS)
        {
            for (int n = 0; n < TLOGS; n++)
            {
                double breakageDef, hiddenDef, seenDef;

                //  find seen defect from log record
                string logNumber = (n + 1).ToString();
                float logSeenDef = 0;

                var logNumMatch = logStockList.FirstOrDefault(l => l.LogNumber == logNumber);
                if (logNumMatch != null)
                {
                    // this might be a bug
                    logSeenDef = logStockList[n].SeenDefect;
                }

                if (n < numPPlogs)
                {
                    breakageDef = 1.0 - (cullDefPrimary / 100.0);
                    hiddenDef = 1.0 - (hiddenDefPrimary / 100.0);
                    seenDef = 1.0 - ((logSeenDef + seenDefPrimary) / 100.0);
                }
                else
                {
                    breakageDef = 1.0 - (cullDefSecondary / 100.0);
                    hiddenDef = 1.0 - (hiddenDefSecondary / 100.0);
                    seenDef = 1.0 - ((logSeenDef + seenDefSecondary) / 100.0);
                }

                var logVol = LOGVOL[n];
                //  Board foot removed
                logVol.GrossRemovedBoardFoot = (float)Math.Round(logVol.GrossBoardFoot * breakageDef, 0, MidpointRounding.AwayFromZero);
                //  Cubic foot removed
                logVol.GrossRemovedCubicFoot = (float)Math.Round(logVol.GrossCubicFoot * breakageDef, 1, MidpointRounding.AwayFromZero);
                //  Net board foot
                logVol.NetBoardFoot = (float)Math.Round(((logVol.GrossBoardFoot * breakageDef) * seenDef) * hiddenDef, 0, MidpointRounding.AwayFromZero);
                //  Net cubic foot
                logVol.NetCubicFoot = (float)Math.Round((((logVol.GrossCubicFoot * breakageDef) * seenDef) * hiddenDef), 1, MidpointRounding.AwayFromZero);
            }
        }

        private static void VolumeLogDefect_R6(IReadOnlyList<LogVolume> LOGVOL, IReadOnlyList<LogStockDO> logStockList, float cullDefPrimary, float hiddenDefPrimary, float seenDefPrimary, float cullDefSecondary, float hiddenDefSecondary, float seenDefSecondary, int numPPlogs, int TLOGS)
        {
            //  November 2016 -- need to check for non-saw logs in sawtimber logs
            //  and reset defect to zero
            float totalDefect;
            for (int n = 0; n < TLOGS; n++)
            {
                var logStock = logStockList[n];
                var logVol = LOGVOL[n];

                if (n < numPPlogs)
                {


                    //  check log grade to reset defect for non-saw logs
                    if (logStock.Grade == "8")
                        totalDefect = 0;
                    else totalDefect = cullDefPrimary + hiddenDefPrimary + seenDefPrimary + logStock.SeenDefect;
                }
                else totalDefect = cullDefSecondary + hiddenDefSecondary + seenDefSecondary + logStock.SeenDefect;

                if (totalDefect > 100) totalDefect = 100;

                //  Net board foot
                logVol.NetBoardFoot = (float)Math.Round(logVol.GrossBoardFoot * (1.0 - (totalDefect / 100.0)), 0, MidpointRounding.AwayFromZero);
                //  Net cubic foot
                logVol.NetCubicFoot = (float)Math.Round(logVol.GrossCubicFoot * (1.0 - (totalDefect / 100.0)), 1, MidpointRounding.AwayFromZero);
            }
        }

        private static void VolumeLogDefect_R7(IReadOnlyList<LogVolume> LOGVOL, IReadOnlyList<LogStockDO> logStockList, float cullDefPrimary, float hiddenDefPrimary, float seenDefPrimary, float cullDefSecondary, float hiddenDefSecondary, float seenDefSecondary, int numPPlogs, int TLOGS)
        {
            for (int n = 0; n < TLOGS; n++)
            {
                var logStock = logStockList[n];
                var logVol = LOGVOL[n];

                //  Apply defect as recorded for every log and apply override exceptions below
                double totalDefect = (n < numPPlogs) ?
                      cullDefPrimary + hiddenDefPrimary + seenDefPrimary + logStock.SeenDefect
                    : cullDefSecondary + hiddenDefSecondary + seenDefSecondary + logStock.SeenDefect;

                if (totalDefect >= 100)
                {
                    //  Board foot removed
                    logVol.GrossRemovedBoardFoot = 0;
                    //  Cubic foot removed
                    logVol.GrossRemovedCubicFoot = 0;
                }
                else
                {
                    //  Board foot removed
                    logVol.GrossRemovedBoardFoot = logVol.GrossBoardFoot;
                    //  Cubic foot removed
                    logVol.GrossRemovedCubicFoot = logVol.GrossCubicFoot;
                }

                //  Calculate net volumes
                //  Net board foot
                logVol.NetBoardFoot = (float)Math.Round(logVol.GrossBoardFoot * (1.0 - (totalDefect / 100.0)), 0, MidpointRounding.AwayFromZero);
                //  Net cubic foot
                logVol.NetCubicFoot = (float)Math.Round(logVol.GrossCubicFoot * (1.0 - (totalDefect / 100.0)), 1, MidpointRounding.AwayFromZero);

                //  Now for the grades shown, override the volume calculated with zero
                if (logStock.Grade == "7" || logStock.Grade == "8")
                {
                    //  Net board foot
                    logVol.NetBoardFoot = 0;
                    //  Net cubic foot
                    logVol.NetCubicFoot = 0;

                    //  If seen defect is greater than 50%, this is a cull log.
                    //  Grade does not change but gross merch is reset to zero.
                    if (logStock.SeenDefect > 50)
                    {
                        logVol.GrossRemovedBoardFoot = 0;      //  BDFT
                        logVol.GrossRemovedCubicFoot = 0;      //  CUFT
                    }
                }
                else if (logStock.Grade == "9")
                {
                    //  reset gross removed to zero for BDFT and CUFT
                    logVol.GrossRemovedBoardFoot = 0;
                    logVol.GrossRemovedCubicFoot = 0;

                    //  reset net volume to zero for BDFT and CUFT
                    logVol.NetBoardFoot = 0;
                    logVol.NetCubicFoot = 0;
                }
            }
        }

        private static void VolumeLogDefect_R10(IReadOnlyList<LogVolume> LOGVOL, IReadOnlyList<LogStockDO> logStockList, float cullDefPrimary, float hiddenDefPrimary, float cullDefSecondary, float hiddenDefSecondary, int numPPlogs, int TLOGS, float currentRec)
        {
            for (int n = 0; n < TLOGS; n++)
            {
                double breakageDef, hiddenDef, seenDef;
                var logStock = logStockList[n];
                var logStockGrade = logStock.Grade;

                if (n < numPPlogs)
                {
                    breakageDef = 1.0 - (cullDefPrimary / 100.0);
                    hiddenDef = 1.0 - (hiddenDefPrimary / 100.0);
                }
                else
                {
                    breakageDef = 1.0 - (cullDefSecondary / 100.0);
                    hiddenDef = 1.0 - (hiddenDefSecondary / 100.0);
                }   //  endif

                //  find seen defect from log record
                string currLN = Convert.ToString(n + 1);
                var logStockMatch = logStockList.FirstOrDefault(l => l.LogNumber == currLN);
                if (logStockMatch != null)
                { seenDef = 1.0 - (logStockMatch.SeenDefect / 100); }
                else seenDef = 1.0;

                var logVol = LOGVOL[n];
                //  Gross removed volumes -- (Grades 8-9 and breakage)
                if (logStockGrade == "8" || logStockGrade == "9")
                {
                    //  no gross removed volume
                    logVol.GrossRemovedBoardFoot = 0;
                    logVol.GrossRemovedCubicFoot = 0;
                }
                else
                {
                    //  Net board foot
                    logVol.NetBoardFoot = (float)Math.Floor(((logVol.GrossBoardFoot * breakageDef) * seenDef) * hiddenDef + 0.5);
                    //  Net cubic foot
                    logVol.NetCubicFoot = (float)(Math.Floor((((logVol.GrossCubicFoot * breakageDef) * seenDef) * hiddenDef) * 10 + 0.5) / 10.0);

                    //  add recoverable percent together (tree and log) and make sure it's not large than total defect
                    //  need just log grades 0-6
                    float combinedRecPC = 0;
                    if (logStockGrade == "0" || logStockGrade == "1" || logStockGrade == "2" ||
                        logStockGrade == "3" || logStockGrade == "4" || logStockGrade == "5" ||
                        logStockGrade == "6")
                    {
                        combinedRecPC = logStock.PercentRecoverable;
                        combinedRecPC += currentRec;
                    }   //  endif log grade 0-6
                        //  then add tree breakage, tree hidden and seen from the log
                    float totalDef = cullDefPrimary + hiddenDefPrimary + logStock.SeenDefect;
                    double boardUtil = 0;
                    double cubicUtil = 0;

                    //  January 2017 -- they are no longer selloing utility voulme
                    //  so calculation of utility is no longer needed
                    //if (combinedRecPC <= totalDef)
                    //{
                    //  Calculate utility volume using combined recoverable percent
                    //    boardUtil = Math.Floor(LOGVOL[n, 0] * combinedRecPC / 100 + 0.0499);
                    //    cubicUtil = Math.Floor((LOGVOL[n, 3] * combinedRecPC / 100) * 10 + 0.0499) / 10.0;
                    //logStockList[n].PercentRecoverable = combinedRecPC;
                    //}
                    //else
                    //{
                    //  Calculate utility volume using total defect instead
                    //    boardUtil = Math.Floor(LOGVOL[n, 0] * totalDef / 100 + 0.0499);
                    //    cubicUtil = Math.Floor((LOGVOL[n, 3] * totalDef / 100) * 10 + 0.0499) / 10.0;
                    //logStockList[n].PercentRecoverable = totalDef;
                    //}   //  endif on recoverable percent
                    //  Board foot removed
                    logVol.GrossRemovedBoardFoot = (float)(Math.Round(((logVol.GrossBoardFoot * breakageDef) - boardUtil), 0, MidpointRounding.AwayFromZero));
                    //  Cubic foot removed
                    logVol.GrossRemovedCubicFoot = (float)(Math.Round(((logVol.GrossCubicFoot * breakageDef) - cubicUtil), 1, MidpointRounding.AwayFromZero));

                    //  store utility calculation in the log stock list
                    logStock.BoardUtil = (float)boardUtil;
                    logStock.CubicUtil = (float)cubicUtil;
                }
            }
        }

        private static void LogUtil(string currRegn, string currPP, VolLibNVBoutput volOutput,
                                    IReadOnlyList<LogStockDO> logStockList, int numPPlogs)
        {
            Volumes VOL = volOutput.Volumes;
            IReadOnlyList<LogVolume> LOGVOL = volOutput.LogVolumes;
            int TLOGS = volOutput.TotalLogs;

            //  Zero out volumes
            //  Start index needs to be 1 instead of zero
            //   because of change to profile models, last two position should not be set to zero
            //  position 13 is stump and 14 is tip
            //for (int n = 1; n < 15; n++)
            for (int n = 1; n < 13; n++)
            {
                if (n != 5)
                    VOL[n] = 0;
            } 

            //  Primary volume
            for (int n = 0; n < numPPlogs; n++)
            {
                var logVol = LOGVOL[n];
                var logStockGrade = logStockList[n].Grade;

                if ((currRegn == "06" && (logStockGrade == "7" || logStockGrade == "8") && currPP == "01"))
                //  January 2017 --  Region 10 no longer sells utility volume
                //  so no log grade 7 and this check doesn't apply
                // || (currRegn == "10" && logStockList[n].Grade == "7" && currPP == "01"))
                {
                    VOL[6] += logVol[3];
                    VOL[7] += logVol[5];
                    VOL[11] += logVol[0];
                    VOL[12] += logVol[2];
                }
                else
                {
                    VOL[1] += logVol[0];
                    VOL[2] += logVol[2];
                    VOL[3] += logVol[3];
                    VOL[4] += logVol[5];
                }
            }

            //  Secondary volume
            for (int n = numPPlogs; n < TLOGS; n++)
            {
                var logVol = LOGVOL[n];

                VOL[6] += logVol[3];
                VOL[7] += logVol[5];
                VOL[11] += logVol[0];
                VOL[12] += logVol[2];
            } 
        }

    }
}
