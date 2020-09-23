using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public  class CalculateNetVolume
    {
        #region
        private int defectLogic = 1;
        private double soundDefault = 0;
        #endregion

        public void calcNetVol(string cruiseType, float[] VOL, float[,] LOGVOL, List<LogStockDO> logStockList, 
                                    TreeDO currTree, string currRegion, float NOLOGP, float NOLOGS, int TLOGS, float MTOPP,
                                    string currPP)
        {
            //  March 2015 --  because Region 5 records hidden defect differently from other regions
            //  the value on a tree record needs to be checked; if 0 look in TDV and set if greater than zero
            //  Otherwise hidden defect will default to zero
            float tempHidden = 0;
            if (currTree.HiddenPrimary == 0)
            {
                //  now check hidden primary in TreeDefaultValue
                if (currTree.TreeDefaultValue.HiddenPrimary > 0)
                    tempHidden = currTree.TreeDefaultValue.HiddenPrimary;
                else tempHidden = 0;
            }
            else tempHidden = currTree.HiddenPrimary;
            if (cruiseType == "V")
                VariableLogLength(VOL, LOGVOL, logStockList, TLOGS, currPP, currTree.TreeDefaultValue.CullPrimary,
                                    tempHidden, currTree.SeenDefectPrimary);
            
            //  set defect flag based on region
            if (currRegion == "02" || currRegion == "04" || currRegion == "07")
                defectLogic = 2;
            else if (currRegion == "08")
                defectLogic = 3;
            else if (currRegion == "10")
                defectLogic = 4; 
            else if (currRegion == "01" || currRegion == "03" ||
                     currRegion == "05" || currRegion == "09")
                defectLogic = 1;

            //  set primary and secondary number of logs to integer for loop but must round not truncate them
            NOLOGP = (float)Math.Round(NOLOGP);
            NOLOGS = (float)Math.Round(NOLOGS+.5);

            //  set percent sound based on region
            if (currRegion == "05") soundDefault = 0.25;
            else if (currRegion == "06") soundDefault = 0.02;
            else if (currRegion == "07") soundDefault = 0.33;
            else if (currRegion == "10") soundDefault = 0.33;

            //  call appropriate defect routine based on region --  default is VolumeTreeDefect
            //  some regions have multiple routines called
            if (currRegion == "05" || currRegion == "07" || currRegion == "10")
            {
                if (logStockList.Count() <= 0 || TLOGS <= 0)
                    VolumeTreeDefect(currTree.TreeDefaultValue.CullPrimary, tempHidden,
                                     currTree.SeenDefectPrimary, currTree.TreeDefaultValue.CullSecondary,
                                     currTree.TreeDefaultValue.HiddenSecondary, currTree.SeenDefectSecondary,
                                     ref VOL, currRegion, currPP);
                else
                {
                    if (currRegion == "10")
                    {
                        if (TLOGS > 0)
                            SetLogGrades(currRegion, logStockList, currTree.Grade, (int)NOLOGP, TLOGS, currTree.RecoverablePrimary);
                        else
                        {
                            //  check for cull tree grades
                            if (currTree.Grade == "8" || currTree.Grade == "9")
                            {
                                foreach (LogStockDO lsdo in logStockList)
                                    lsdo.SeenDefect = 100;
                            }   //  endif tree grade is cull
                        }   //  endif
                    }
                    else
                        SetLogGrades(currRegion, logStockList, currTree.Grade, (int)NOLOGP, TLOGS, currTree.RecoverablePrimary);

                    VolumeLogDefect(currRegion, LOGVOL, VOL, logStockList, currTree.TreeDefaultValue.CullPrimary,
                                    tempHidden, currTree.SeenDefectPrimary,
                                    currTree.TreeDefaultValue.CullSecondary, currTree.TreeDefaultValue.HiddenSecondary,
                                    currTree.SeenDefectSecondary, (int)NOLOGP, (int)NOLOGS, TLOGS, 
                                    currTree.RecoverablePrimary);
                    
                    if (currRegion == "07" || currRegion == "10")
                        LogUtil(VOL, LOGVOL, TLOGS, (int)NOLOGP, (int)NOLOGS, logStockList, currRegion, currPP);

                    if (currRegion == "05" || currRegion == "10")
                        SetDiameterClass(currRegion, logStockList, TLOGS);
                }   //  endif no logs
            }
            else if (currRegion == "06")
            {
                if (logStockList.Count() > 0)
                {
                    GetR6LogGrade(currTree.Grade, logStockList, TLOGS, (int)NOLOGP, MTOPP);
                    VolumeLogDefect(currRegion, LOGVOL, VOL, logStockList, currTree.TreeDefaultValue.CullPrimary,
                                    tempHidden, currTree.SeenDefectPrimary,
                                    currTree.TreeDefaultValue.CullSecondary, currTree.TreeDefaultValue.HiddenSecondary,
                                    currTree.SeenDefectSecondary, (int)NOLOGP, (int)NOLOGS, TLOGS, 
                                    currTree.RecoverablePrimary);
                    LogUtil(VOL, LOGVOL, TLOGS, (int)NOLOGP, (int)NOLOGS, logStockList, currRegion, currPP);
                    SetDiameterClass(currRegion, logStockList, TLOGS);
                }   //  endif logStockList has records
            }
            else
            {   //  default -- includes regions 1, 2, 3, 4, 8 and 9
                VolumeTreeDefect(currTree.TreeDefaultValue.CullPrimary, tempHidden,
                                currTree.SeenDefectPrimary, currTree.TreeDefaultValue.CullSecondary,
                                currTree.TreeDefaultValue.HiddenSecondary, currTree.SeenDefectSecondary, 
                                ref VOL, currRegion, currPP);
            }   //  endif currRegion

            if (TLOGS > 0 && (currRegion != "05" && currRegion != "06" && currRegion != "10"))
                SetDiameterClass(currRegion, logStockList, TLOGS);
            return;
        }   // end calcNetVol


        private void VolumeTreeDefect(float currentDef1, float currentDef2, float currentDef3,
                                             float currentDef4, float currentDef5, float currentDef6,
                                             ref float[] VOL, string currRegn, string currPP)
        {
            float totalPrimDef, totalSecDef;
            float breakageDef, hiddenDef, seenDef;

            //  Calculate nets based on defect logic
            if (defectLogic == 1 || defectLogic == 3)
            {
                totalPrimDef = 1 - (currentDef1 + currentDef2 + currentDef3) / 100;
                totalSecDef = 1 - (currentDef4 + currentDef5 + currentDef6) / 100;
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
                    if (currPP != "01")
                    {
                        //  Apply secondary defect to primary products
                        VOL[4] = VOL[3] * totalSecDef;      //  Net CUFT primary volume
                        VOL[2] = VOL[1] * totalSecDef;      //  Net BDFT primary volume
                    }   //  endif currPP
                }   //  endif currRegn
            }
            else if(defectLogic == 2)
            {
                totalPrimDef = (1 - currentDef1 / 100) * (1 - currentDef2 / 100) * (1 - currentDef3 / 100);
                totalSecDef = (1 - currentDef4 / 100) * (1 - currentDef5 / 100) * (1 - currentDef6 / 100);
                VOL[2] = totalPrimDef * VOL[1];     //  Net BDFT primary volume
                VOL[4] = totalPrimDef * VOL[3];     //  Net CUFT primary volume
                VOL[10] = totalPrimDef * VOL[9];    //  Net International volume
                VOL[7] = totalSecDef * VOL[6];      //  Net CUFT secondary volume
                VOL[12] = totalSecDef * VOL[11];    //  Net BDFT secondary volume
            }
            else if(defectLogic == 4)
            {
                breakageDef = 1 - (currentDef1 / 100);
                hiddenDef = 1 - (currentDef2 / 100);
                seenDef = 1 - (currentDef3 / 100);
                VOL[2] = ((VOL[1] * breakageDef) * seenDef) * hiddenDef;        //  Net BDFT primary volume
                VOL[4] = ((VOL[3] * breakageDef) * seenDef) * hiddenDef;        //  Net CUFT primary volume
            }   //  endif defectLogic

            //  Round net volumes
            VOL[2] = (float)Math.Round(VOL[2] + 0.001,0,MidpointRounding.AwayFromZero);
            VOL[4] = (float)Math.Round(VOL[4] + 0.001,1,MidpointRounding.AwayFromZero);
            VOL[7] = (float)Math.Round(VOL[7] + 0.001,1,MidpointRounding.AwayFromZero);
            VOL[10] = (float)Math.Round(VOL[10] + 0.001,0,MidpointRounding.AwayFromZero);
            VOL[12] = (float)Math.Round(VOL[12] + 0.001,0,MidpointRounding.AwayFromZero);
            return;
        }   //  end VolumeTreeDefect


        private void SetLogGrades(string currRegn, List<LogStockDO> logStockList, string currTG,
                                          int numPPlogs, int TLOGS, float currDefRec)
        {
            //  skip first log record (0) -- loop starts with next log (1)
            for (int n = 0; n < TLOGS; n++)
            {
                if (n < numPPlogs)
                {
                    //  For every log except the first one and log grade is blank
                    //  grade the grade from the previous log 
                    //  also defect for Region 10
                    if (n != 0 && 
                        (logStockList[n].Grade == "" || 
                        logStockList[n].Grade == " " || 
                        logStockList[n].Grade == null))
                    {
                        logStockList[n].Grade = logStockList[n - 1].Grade;
                        if (currRegn == "10") logStockList[n].SeenDefect = logStockList[n - 1].SeenDefect;
                    }   //  endif on grade

                    //  Make adjustments as needed for select regions
                    if (currRegn == "05")
                    {
                        if (logStockList[n].Grade == "0") logStockList[n].Grade = currTG;
                    }
                    else if (currRegn == "10")
                    {
                        //  July 2014
                        //  this no longer applies since R10 may record recoverable at the log level
                        //logStockList[n].PercentRecoverable = currDefRec;
                        //  January 2017 -- Region 10 no longer sells utility volume so
                        //  log grade 7 is recoded to 8 if the volume library spits out a grade 7
                        if (logStockList[n].Grade == "7")
                        {
                            logStockList[n].SeenDefect = 100;
                            logStockList[n].PercentRecoverable = 0;
                        }   //  endif
                    }
                    else
                    {
                        if (logStockList[n].Grade == "0") logStockList[n].Grade = currTG;
                    }   // endif current region
                }
                else             //  topwood
                {
                    if (currRegn == "05")
                    {
                        logStockList[n].Grade = "T";
                        if (logStockList[n].SeenDefect >= 99) logStockList[n].SeenDefect = 100;
                    }
                    else if(currRegn == "07")
                    {
                        logStockList[n].Grade = "7";
                        if (logStockList[n].SeenDefect >= 99) logStockList[n].SeenDefect = 100;
                    }
                    else if(currRegn == "10")
                    {
                        //  January 2017 -- see above concerning use of grade 7
                        //logStockList[n].Grade = "7";
                        logStockList[n].Grade = "8";
                        if (logStockList[n].SeenDefect >= 99) logStockList[n].SeenDefect = 100;
                        logStockList[n].PercentRecoverable = 0;
                    }   //  endif current Region
                }   //  endif n

                if (currRegn == "10")
                {
                    if((logStockList[n].Grade == "" || logStockList[n].Grade == " ") &&
                        (currTG == "8" || currTG == "9") || 
                        (logStockList[n].Grade == "8" || logStockList[n].Grade == "9"))
                    {
                        logStockList[n].SeenDefect = 100;
                        logStockList[n].PercentRecoverable = 0;
                    }   //  endif
                }   //  endif current region
            }   //  end for n loop

            //  if no logs, should skip the loop to here
            if (logStockList.Count == 0)
            {
                for (int k = 0; k < TLOGS; k++)
                {
                    logStockList[k].Grade = currTG;
                    if (currRegn == "10" && (currTG == "8" || currTG == "9"))
                        logStockList[k].SeenDefect = 100;
                }   //  end for k loop
            }   //  endif no logs

            //  For Regions 7 (BLM) or 10, if the first log has a blank grade, reset it to zero.  Per KCormier June 2003 -- bem
            if(currRegn == "07" || currRegn == "10")
            {
                if(logStockList[0].Grade == "" || logStockList[0].Grade == " ")
                    logStockList[0].Grade = "0";
            }   //  endif current region
            return;
        }   //  end SetLogGrades


        private void VolumeLogDefect(string currRegn, float[,] LOGVOL, float[] VOL, List<LogStockDO> logStockList,
                                            float currentDef1, float currentDef2, float currentDef3,
                                            float currentDef4, float currentDef5, float currentDef6,
                                            int numPPlogs, int numSPlogs, int TLOGS, float currentRec)
        {
            double breakageDef, hiddenDef, seenDef;

            //  Calculate net log volume by region
            if (currRegn == "10")
            {
                for (int n = 0; n < TLOGS; n++)
                {
                    if (n < numPPlogs)
                    {
                        breakageDef = 1.0 - (currentDef1 / 100.0);
                        hiddenDef = 1.0 - (currentDef2 / 100.0);
                    }
                    else
                    {
                        breakageDef = 1.0 - (currentDef4 / 100.0);
                        hiddenDef = 1.0 - (currentDef5 / 100.0);
                    }   //  endif

                    //  find seen defect from log record
                    string currLN = Convert.ToString(n + 1);
                    int nthRow = logStockList.FindIndex(
                        delegate(LogStockDO ls)
                        {
                            return ls.LogNumber == currLN;
                        });
                    if (nthRow >= 0)
                        seenDef = 1.0 - (logStockList[nthRow].SeenDefect / 100);
                    else seenDef = 1.0;

                    //  Gross removed volumes -- (Grades 8-9 and breakage)
                    if (logStockList[n].Grade == "8" || logStockList[n].Grade == "9")
                    {
                        //  no gross removed volume
                        LOGVOL[n, 1] = 0;
                        LOGVOL[n, 4] = 0;
                    }
                    else
                    {
                        //  Net board foot
                        LOGVOL[n, 2] = (float)Math.Floor(((LOGVOL[n, 0] * breakageDef) * seenDef) * hiddenDef + 0.5);
                        //  Net cubic foot
                        LOGVOL[n, 5] = (float)(Math.Floor((((LOGVOL[n, 3] * breakageDef) * seenDef) * hiddenDef) * 10 + 0.5)/10.0);

                        //  add recoverable percent together (tree and log) and make sure it's not large than total defect
                        //  need just log grades 0-6
                        float combinedRecPC = 0;
                        if (logStockList[n].Grade == "0" || logStockList[n].Grade == "1" || logStockList[n].Grade == "2" ||
                            logStockList[n].Grade == "3" || logStockList[n].Grade == "4" || logStockList[n].Grade == "5" ||
                            logStockList[n].Grade == "6")
                        {
                            combinedRecPC = logStockList[n].PercentRecoverable;
                            combinedRecPC += currentRec;
                        }   //  endif log grade 0-6
                        //  then add tree breakage, tree hidden and seen from the log
                        float totalDef = currentDef1 + currentDef2 + logStockList[n].SeenDefect;
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
                        LOGVOL[n, 1] = (float)(Math.Round(((LOGVOL[n, 0] * breakageDef) - boardUtil),0,MidpointRounding.AwayFromZero));
                        //  Cubic foot removed
                        LOGVOL[n, 4] = (float)(Math.Round(((LOGVOL[n, 3] * breakageDef) - cubicUtil),1,MidpointRounding.AwayFromZero));

                        //  store utility calculation in the log stock list
                        logStockList[n].BoardUtil =  (float) boardUtil;
                        logStockList[n].CubicUtil = (float) cubicUtil;
                    }   //  endif

                }   //  end for n loop
            }
            else if(currRegn == "05")
            {
                for(int n=0;n<TLOGS;n++)
                {
                    //  find seen defect from log record
                    string currLN = Convert.ToString(n + 1);
                    float logSeenDef = 0;
                    int nthRow = logStockList.FindIndex(
                        delegate(LogStockDO ls)
                        {
                            return ls.LogNumber == currLN;
                        });
                    if (nthRow >= 0) logSeenDef = logStockList[n].SeenDefect;

                    if(n < numPPlogs)
                    {
                        breakageDef = 1.0 - (currentDef1/100.0);
                        hiddenDef = 1.0 - (currentDef2/100.0);
                        seenDef = 1.0 - ((logSeenDef + currentDef3)/100.0);
                    }
                    else
                    {
                        breakageDef = 1.0 - (currentDef4/100.0);
                        hiddenDef = 1.0 - (currentDef5/100.0);
                        seenDef = 1.0 - ((logSeenDef + currentDef6)/100.0);
                    }   //  endif
                    //  Board foot removed
                    LOGVOL[n, 1] = (float)Math.Round(LOGVOL[n, 0] * breakageDef,0,MidpointRounding.AwayFromZero);
                    //  Cubic foot removed
                    LOGVOL[n, 4] = (float)Math.Round(LOGVOL[n, 3] * breakageDef,1,MidpointRounding.AwayFromZero);
                    //  Net board foot
                    LOGVOL[n, 2] = (float)Math.Round(((LOGVOL[n, 0] * breakageDef) * seenDef) * hiddenDef,0,MidpointRounding.AwayFromZero);
                    //  Net cubic foot
                    LOGVOL[n, 5] = (float)Math.Round((((LOGVOL[n, 3] * breakageDef) * seenDef) * hiddenDef),1,MidpointRounding.AwayFromZero);

                    
                }   //  end for n loop
            }
            else if(currRegn == "07")
            {
                double totalDefect;
                for(int n=0;n<TLOGS;n++)
                {
                    //  Apply defect as recorded for every log and apply override exceptions below
                    if(n < numPPlogs)
                        totalDefect = currentDef1 + currentDef2 + currentDef3 + logStockList[n].SeenDefect;
                    else
                        totalDefect = currentDef4 + currentDef5 + currentDef6 + logStockList[n].SeenDefect;

                    if(totalDefect >= 100)
                    {
                        //  Board foot removed
                        LOGVOL[n, 1] = 0;
                        //  Cubic foot removed
                        LOGVOL[n, 4] = 0;
                    }
                    else
                    {
                        //  Board foot removed
                        LOGVOL[n, 1] = LOGVOL[n, 0];
                        //  Cubic foot removed
                        LOGVOL[n, 4] = LOGVOL[n, 3];
                    }   //  endif 

                    //  Calculate net volumes
                    //  Net board foot
                    LOGVOL[n, 2] = (float)Math.Round(LOGVOL[n, 0] * (1.0 - (totalDefect/100.0)),0,MidpointRounding.AwayFromZero);
                    //  Net cubic foot
                    LOGVOL[n, 5] = (float)Math.Round(LOGVOL[n, 3] * (1.0 - (totalDefect/100.0)),1,MidpointRounding.AwayFromZero);

                    //  Now for the grades shown, override the volume calculated with zero
                    if(logStockList[n].Grade == "7" || logStockList[n].Grade == "8")
                    {
                        //  Net board foot
                        LOGVOL[n, 2] = 0;
                        //  Net cubic foot
                        LOGVOL[n, 5] = 0;

                        //  If seen defect is greater than 50%, this is a cull log.
                        //  Grade does not change but gross merch is reset to zero.
                        if(logStockList[n].SeenDefect > 50)
                        {
                            LOGVOL[n, 1] = 0;      //  BDFT
                            LOGVOL[n, 4] = 0;      //  CUFT
                        }
                    }
                    else if(logStockList[n].Grade == "9")
                    {
                        //  reset gross removed to zero for BDFT and CUFT
                        LOGVOL[n, 1] = 0;
                        LOGVOL[n, 4] = 0;

                        //  reset net volume to zero for BDFT and CUFT
                        LOGVOL[n, 2] = 0;
                        LOGVOL[n, 5] = 0;
                    }   //  endif
                }   //  end for n loop
            }
            else if (currRegn == "06")
            {
                //  November 2016 -- need to check for non-saw logs in sawtimber logs
                //  and reset defect to zero
                float totalDefect;
                for (int n = 0; n < TLOGS; n++)
                {
                    if (n < numPPlogs)
                    {
                        //  check log grade to reset defect for non-saw logs
                        if (logStockList[n].Grade == "8")
                            totalDefect = 0;
                        else totalDefect = currentDef1 + currentDef2 + currentDef3 + logStockList[n].SeenDefect;
                    }
                    else totalDefect = currentDef4 + currentDef5 + currentDef6 + logStockList[n].SeenDefect;

                    if (totalDefect > 100) totalDefect = 100;

                    //  Net board foot
                    LOGVOL[n, 2] = (float)Math.Round(LOGVOL[n, 0] * (1.0 - (totalDefect / 100.0)), 0, MidpointRounding.AwayFromZero);
                    //  Net cubic foot
                    LOGVOL[n, 5] = (float)Math.Round(LOGVOL[n, 3] * (1.0 - (totalDefect / 100.0)), 1, MidpointRounding.AwayFromZero);
                }   //  for n loop
            }
            else                 //  all other regions
            {
                float totalDefect;
                for (int n = 0; n < TLOGS; n++)
                {
                    if (n < numPPlogs)
                        totalDefect = currentDef1 + currentDef2 + currentDef3 + logStockList[n].SeenDefect;
                    else
                        totalDefect = currentDef4 + currentDef5 + currentDef6 + logStockList[n].SeenDefect;

                    if (totalDefect > 100) totalDefect = 100;

                    //  Net board foot
                    LOGVOL[n, 2] = (float)Math.Round(LOGVOL[n, 0] * (1.0 - (totalDefect / 100.0)), 0, MidpointRounding.AwayFromZero);
                    //  Net cubic foot
                    LOGVOL[n, 5] = (float)Math.Round(LOGVOL[n, 3] * (1.0 - (totalDefect / 100.0)), 1, MidpointRounding.AwayFromZero);
                }   //  end for n loop
            }   //  endif current region

            //  Check for sound log
            float percentSound;
            for(int n=0;n<TLOGS;n++)
            {
                if(LOGVOL[n, 0] > 0)
                {
                    percentSound = LOGVOL[n, 2]/LOGVOL[n, 0];
                    if(percentSound < soundDefault)
                    {
                        LOGVOL[n, 2] = 0;
                        if(currRegn == "06") logStockList[n].Grade = "9";
                    }   //  endif
                }   //  endif

                if(LOGVOL[n, 3] > 0)
                {
                    percentSound = LOGVOL[n, 5]/LOGVOL[n, 3];
                    if(percentSound < soundDefault)
                    {
                        LOGVOL[n, 5] = 0;
                        if(currRegn == "06") logStockList[n].Grade = "9";
                    }   //  endif
                }   //  endif
            }   //  end for n loop

            //  Sum log volumes into tree volumes
            for(int n=0;n<numPPlogs;n++)
            {
                VOL[2] += LOGVOL[n, 2];
                VOL[4] += LOGVOL[n, 5];
            }   //  end for n loop

            for(int n=0;n<numSPlogs;n++)
            {
                VOL[7] += LOGVOL[n+numPPlogs, 5];
                VOL[12] += LOGVOL[n+numPPlogs, 2];
            }   //  end for n loop
            return;
        }   //  end VolumeLogDefect


        private void LogUtil(float[] VOL, float[,] LOGVOL, int TLOGS, int numPPlogs, int numSPlogs, 
                                    List<LogStockDO> logStockList, string currRegn, string currPP)
        {
            //  Zero out volumes
            //  Start index needs to be 1 instead of zero
            //   because of change to profile models, last two position should not be set to zero
            //  position 13 is stump and 14 is tip
            //for (int n = 1; n < 15; n++)
            for (int n = 1; n < 13; n++)
            {
                if (n != 5)
                    VOL[n] = 0;
            }   //  end for n loop

            //  Primary volume
            for (int n = 0; n < numPPlogs; n++)
            {
                if ((currRegn == "06" && (logStockList[n].Grade == "7" || logStockList[n].Grade == "8") && currPP == "01"))
                    //  January 2017 --  Region 10 no longer sells utility volume
                    //  so no log grade 7 and this check doesn't apply
                    // || (currRegn == "10" && logStockList[n].Grade == "7" && currPP == "01"))
                {
                    VOL[6] += LOGVOL[n, 3];
                    VOL[7] += LOGVOL[n, 5];
                    VOL[11] += LOGVOL[n, 0];
                    VOL[12] += LOGVOL[n, 2];
                }
                else
                {
                    VOL[1] += LOGVOL[n, 0];
                    VOL[2] += LOGVOL[n, 2];
                    VOL[3] += LOGVOL[n, 3];
                    VOL[4] += LOGVOL[n, 5];
                }   //  endif
            }   //  end for n loop

            //  Secondary volume
            for (int n = numPPlogs; n < TLOGS; n++)
            {
                VOL[6] += LOGVOL[n, 3];
                VOL[7] += LOGVOL[n, 5];
                VOL[11] += LOGVOL[n, 0];
                VOL[12] += LOGVOL[n, 2];
            }   //  end for n loop
            return;
        }   //  end LogUtil


        private void SetDiameterClass(string currRegn, List<LogStockDO> logStockList, int TLOGS)
        {
            for (int n = 0; n < TLOGS; n++)
            {
                //  used to have small end diameter scale used here.  Need to calculate it like the volume library
                //  now that only the calculated small end diameter is stored.
                int roundedSED;
                
                //  For Region 6, make adjustments as needed
                if (currRegn == "6" || currRegn == "06")
                {
                    //  round diameter first so this matches the old program -- February 2014
                    roundedSED = Convert.ToInt16(Math.Round(logStockList[n].SmallEndDiameter, 0, MidpointRounding.AwayFromZero));
                    roundedSED = (int)(((roundedSED / 3.0) - 0.53) + 0.49);
                    if (roundedSED < 1)
                        logStockList[n].DIBClass = 1;
                    else if (roundedSED > 28)
                        logStockList[n].DIBClass = 28;
                    else
                        logStockList[n].DIBClass = roundedSED;
                }
                else if (currRegn == "7" || currRegn == "07")
                    logStockList[n].DIBClass = (float)Math.Round(logStockList[n].SmallEndDiameter - 0.1);
                else
                    logStockList[n].DIBClass = (float)Math.Round(logStockList[n].SmallEndDiameter);
                
            }   //  end for n loop
            return;
        }   //  end SetDiameterClass


        private void GetR6LogGrade(string currTG, List<LogStockDO> logStockList, int TLOGS, 
                                            int numPPlogs, float MTOPP)
        {
            /*  These comments are from the original GETLOG/GETR6GRADE in NatCRS
             *  New log grade rules:
             *      All log grades <= 6 become grade 1 if sed >= 5
             *      Log grades of 7 or 8 become 7
             *      if sed < 5, log grade becomes 7
             *      If log grade is 9 stays 9
             *      1st (if westside) dupe both grade & defect for both halfs of the six possible longlogs.
             *      2nd  standardize all cull logs.  A log is cull if grade is >= 8 or if defect is >= 7. Make all culls 8 over 9.
             *      3rd now fill in any missing grades.  start at top of tree and work down searching for any grade zero.
             *      Substitute grade of first non-cull log above.  if none default to min. grade for species.
             *      Last cull-out any (westside) logs above nine 32's.  (eastside) logs above twelve 16's.
             *      All logs end up graded.
             *      All cull logs end up with lgrd=8 and vdef=9
             *      All other logs end up with lgrd=(1-7) and vdef=(0=6)
             */
            //  NOTE:  per conversation with Chriss Roemer and Ken Cormier
            //  Zone 2 is no longer used in this routine -- January 2003

            for (int n = 0; n < TLOGS; n++)
            {
                if(logStockList[n].Grade == "0" || logStockList[n].Grade == "" || 
                    logStockList[n].Grade == " " || logStockList[n].Grade == null)
                    logStockList[n].Grade = currTG;

                string gradeTest = "0123456";
                //if (currTG.IndexOf(gradeTest) >= 0 || logStockList[n].Grade.IndexOf(gradeTest) >= 0)
                if(gradeTest.IndexOf(currTG) >= 0 || gradeTest.IndexOf(logStockList[n].Grade) >= 0)
                {
                    //  Per Jeff Penman, this check now uses top dib as recorded on volume equations
                    //  February 2008 -- bem
                    //  Per Jeff Penman, added setting seen defect to zero when this check happens
                    //  June 2015 -- bem
                    if (logStockList[n].SmallEndDiameter < MTOPP)
                    {
                        logStockList[n].Grade = "8";
                        logStockList[n].SeenDefect = 0;
                    }
                    //  per Jeff Penman, need to check any log grade of 8 to reset defect to zero -- June 2015
                    if (logStockList[n].Grade == "8" && logStockList[n].SeenDefect > 0)
                        logStockList[n].SeenDefect = 0;
                }   //  endif

                if (n > numPPlogs) logStockList[n].Grade = "8";

                if (logStockList[n].Grade == "9" || logStockList[n].SeenDefect > 98)
                {
                    logStockList[n].SeenDefect = 100;
                    logStockList[n].Grade = "9";
                }   //  endif
            }   //  end for n loop

            if (TLOGS >= 19)
            {
                for (int n = 19; n < TLOGS; n++)
                {
                    logStockList[n].Grade = "9";
                    logStockList[n].SeenDefect = 100;
                }   //  end for n loop
            }   //  endif

            return;
        }   //  end GetR6LogGrade


        private void VariableLogLength(float[] VOL, float[,] LOGVOL, List<LogStockDO> logStockList, int TLOGS,
                                                string currPP, float currentDef1, float currentDef2, float currentDef3)
        {
            //  zero volume array
            for (int n = 0; n < 15; n++)
            {
                VOL[n] = 0;
            }   //  end for n loop

            //  Process all logs
            for (int n = 0; n < TLOGS; n++)
            {
                //  reset defect if needed
                if(logStockList[n].Grade == "0")
                    logStockList[n].SeenDefect = 100;

                //  Accumulate defect and apply to log volume
                float totalDefect = currentDef1 + currentDef2 + currentDef3 + logStockList[n].SeenDefect;
                if (totalDefect > 100) totalDefect = 100;

                LOGVOL[n, 2] = (float)Math.Round(LOGVOL[n, 0] * (1.0 - (totalDefect/100.0)));
                LOGVOL[n, 5] = (float)Math.Round(LOGVOL[n, 3] * (1.0 - (totalDefect / 100.0)));

                if((logStockList[n].Grade == "9" || logStockList[n].Grade == "A" || logStockList[n].Grade == "B")
                        && currPP == "01")
                {
                    VOL[6] += LOGVOL[n, 3];
                    VOL[7] += LOGVOL[n, 5];
                    VOL[11] += LOGVOL[n, 0];
                    VOL[12] += LOGVOL[n, 2];
                }
                else
                {
                    VOL[1] += LOGVOL[n, 0];
                    VOL[2] += LOGVOL[n, 2];
                    VOL[3] += LOGVOL[n, 3];
                    VOL[4] += LOGVOL[n, 5];
                }   //  endif
            }   //  end for n loop
            return;
        }   //  end VariableLogLength
    }
}
