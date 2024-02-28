using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using CruiseProcessing.Services;

namespace CruiseProcessing
{
    class OutputStats : CreateTextFile
    {
        public string currentReport;
        public string currCL;
        private int[] fieldLengths;
        private List<string> prtFields = new List<string>();
        private string[] completeHeader = new string[9];
        private List<POPDO> popList = new List<POPDO>();
        private int[] pagesToPrint = new int[3];
        private List<StatList> stage1Stats = new List<StatList>();
        private List<StatList> stage2Stats = new List<StatList>();

        public OutputStats(CPbusinessLayer dataLayer, IDialogService dialogService) : base(dataLayer, dialogService)
        {
        }

        public void CreateStatReports(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  For ST1/ST2 (DP1,DP2) LV03/LV04
            string currentTitle = fillReportTitle(currentReport);
            List<POPDO> allPOP = DataLayer.getPOP();
            //  pull cur or leave as needed
            popList = allPOP.FindAll(
                delegate(POPDO p)
                {
                    return p.CutLeave == currCL;
                });
            string volType = "";

            //  check for no data at all
            //List<POPDO> popCutOnly = POPmethods.GetCutTrees(popList);
            if (popList.Count == 0)
            {
                noDataForReport(strWriteOut, currentReport, " >>> No data available to generate this report");
                return;
            }   //  endif no data

            //  need report header
            //  initialize pages to print
            for (int j = 0; j < 3; j++)
                pagesToPrint[j] = 0;
            switch (currentReport)
            {
                case "ST1":     case "LV03":
                    if(popList.Sum(p => p.Stg1NetXPP) > 0 || popList.Sum(p => p.Stg2NetXPP) > 0)
                        pagesToPrint[0] = 1;
                    if(popList.Sum(p => p.Stg1NetXSP) > 0 || popList.Sum(p => p.Stg2NetXSP) > 0)
                        pagesToPrint[1] = 1;
                    if(popList.Sum(p => p.Stg1NetXRP) > 0 || popList.Sum(p => p.Stg2NetXRP) > 0)
                        pagesToPrint[2] = 1;
                    fieldLengths = new int[] { 1, 3, 3, 4, 4, 3, 2, 6, 6, 7, 7, 9, 12, 15, 12, 10, 9, 10, 9 };
                    rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
                    volType = "NET";
                    break;
                case "ST2":     case "LV04":
                    if(popList.Sum(p => p.Stg1GrossXPP) > 0 || popList.Sum(p => p.Stg2GrossXPP) > 0)
                        pagesToPrint[0] = 1;
                    if(popList.Sum(p => p.Stg1GrossXSP) > 0 || popList.Sum(p => p.Stg2GrossXSP) > 0)
                        pagesToPrint[1] = 1;
                    if(popList.Sum(p => p.Stg1GrossXRP) > 0 || popList.Sum(p => p.Stg2GrossXRP) > 0)
                        pagesToPrint[2] = 1;
                    fieldLengths = new int[] { 1, 3, 3, 4, 4, 3, 2, 6, 6, 7, 7, 9, 12, 15, 12, 10, 9, 10, 9 };
                    rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
                    volType = "GROSS";
                    break;
            }   //  end switch on report for pages to print


            //  process by pages (product)
            if (pagesToPrint[0] == 1)
            {
                //  primary product pages
                finishColumnHeaders(rh.ST1ST2columns, volType, "PRIMARY");
                numOlines = 0;
                ProcessPrimary(strWriteOut, rh, ref pageNumb, volType);
            }   //  endif primary pages

            if (pagesToPrint[1] == 1)
            {
                //  secondary product pages
                finishColumnHeaders(rh.ST1ST2columns, volType, "SECONDARY");
                numOlines = 0;
                ProcessSecondary(strWriteOut, rh, ref pageNumb, volType);
            }   //  endif secondary pages

            if (pagesToPrint[2] == 1)
            {
                finishColumnHeaders(rh.ST1ST2columns, volType, "RECOVERED");
                numOlines = 0;
                ProcessRecovered(strWriteOut, rh, ref pageNumb, volType);
            }   //  endif recovered pages

            return;
        }   //  end CreateStatReports


        private void ProcessPrimary(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, string volType)
        {
            //  Reports ST1/ST2
            List<StratumDO> sList = DataLayer.getStratum();

            foreach (StratumDO s in sList)
            {
                //  find all pop records for this stratum
                List<POPDO> justStratum = POPmethods.GetStratumData(popList,s.Code,currCL);
                //  first capture the sum of x and x squared
                foreach (POPDO p in justStratum)
                {
                    StatList slOne = new StatList();
                    StatList slTwo = new StatList();
                    //  need method for strata
                    string currMethod = Utilities.MethodLookup(p.Stratum, DataLayer);
                   
                    switch (volType)
                    {
                        case "NET":
                            slOne.SumOfX = p.Stg1NetXPP;
                            slOne.SumOfX2 = p.Stg1NetXsqrdPP;
                            break;
                        case "GROSS":
                            slOne.SumOfX = p.Stg1GrossXPP;
                            slOne.SumOfX2 = p.Stg1GrossXsqrdPP;
                            break;
                    }   //  end switch on volume type (gross or net)

                    stage1Stats.Add(slOne);
                    CalcAllStats(p, 1, currMethod);

                    //  Stage 2?
                    switch (volType)
                    {
                        case "NET":
                            slTwo.SumOfX = p.Stg2NetXPP;
                            slTwo.SumOfX2 = p.Stg2NetXsqrdPP;
                            break;
                        case "GROSS":
                            slTwo.SumOfX = p.Stg2GrossXPP;
                            slTwo.SumOfX2 = p.Stg2GrossXsqrdPP;
                            break;
                    }   //  end switch on volume type (gross or net)

                    if (slTwo.SumOfX > 0)
                    {
                        stage2Stats.Add(slTwo);
                        CalcAllStats(p, 2, currMethod);
                        CalcCombinedError(currMethod);
                        WriteCurrentGroup(p, strWriteOut, rh, ref pageNumb, 1, 1, currMethod);
                        WriteCurrentGroup(p, strWriteOut, rh, ref pageNumb, 2, 1, currMethod);
                    }
                    else if(slOne.SumOfX > 0)
                    {
                        CalcCombinedError(currMethod);
                        WriteCurrentGroup(p, strWriteOut, rh, ref pageNumb, 1, 1, currMethod);
                    }   //  endif stage 2
                    stage1Stats.Clear();
                    stage2Stats.Clear();
                }   //  end foreach loop
                if (numOlines > 0)
                {
                    strWriteOut.WriteLine();
                    numOlines++;
                }   //  endif
            }   //  end foreach stratum
            //  print footnote
            strWriteOut.WriteLine("  * CANNOT CALCULATE SAMPLING ERROR IF SMALL N EQUALS 1.");
            return;
        }   //  end ProcessPrimary


        private void ProcessSecondary(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, string volType)
        {
            //  Reports ST1/ST2
            List<StratumDO> sList = DataLayer.getStratum();

            foreach (StratumDO s in sList)
            {
                List<POPDO> justStratum = POPmethods.GetStratumData(popList, s.Code, currCL);
                //  first capture the sum of x and x squared
                foreach (POPDO p in justStratum)
                {
                    StatList slOne = new StatList();
                    StatList slTwo = new StatList();
                    //  need method for stratum
                    string currMethod = Utilities.MethodLookup(p.Stratum, DataLayer);

                    switch (volType)
                    {
                        case "NET":
                            slOne.SumOfX = p.Stg1NetXSP;
                            slOne.SumOfX2 = p.Stg1NetXsqrdSP;
                            break;
                        case "GROSS":
                            slOne.SumOfX = p.Stg1GrossXSP;
                            slOne.SumOfX2 = p.Stg1GrossXsqrdSP;
                            break;
                    }   //  end switch on volume type (gross or net)

                    stage1Stats.Add(slOne);
                    CalcAllStats(p, 1, currMethod);

                    //  Stage 2?
                    switch (volType)
                    {
                        case "NET":
                            slTwo.SumOfX = p.Stg2NetXSP;
                            slTwo.SumOfX2 = p.Stg2NetXsqrdSP;
                            break;
                        case "GROSS":
                            slTwo.SumOfX = p.Stg2GrossXSP;
                            slTwo.SumOfX2 = p.Stg2GrossXsqrdSP;
                            break;
                    }   //  end switch on volume type (gross or net)

                    if (slTwo.SumOfX > 0)
                    {
                        stage2Stats.Add(slTwo);
                        CalcAllStats(p, 2, currMethod);
                        CalcCombinedError(currMethod);
                        WriteCurrentGroup(p, strWriteOut, rh, ref pageNumb, 1, 2, currMethod);
                        WriteCurrentGroup(p, strWriteOut, rh, ref pageNumb, 2, 2, currMethod);
                    }
                    else if (slOne.SumOfX > 0)
                    {
                        if (slOne.SumOfX > 0 && slOne.SumOfX2 > 0)
                        {
                            CalcCombinedError(currMethod);
                            WriteCurrentGroup(p, strWriteOut, rh, ref pageNumb, 1, 2, currMethod);
                        }   //  endif 
                    }   //  endif stage 2
                    stage1Stats.Clear();
                    stage2Stats.Clear();
                }   //  end foreach loop
                if (numOlines > 0)
                {
                    strWriteOut.WriteLine();
                    numOlines++;
                }   //  endif
            }   //  end foreach on stratum
            //  print footnote
            //strWriteOut.Write("\x00B9");   this puts a funny character at the beginning that I can't get rid of
            strWriteOut.WriteLine("  * CANNOT CALCULATE SAMPLING ERROR IF SMALL N EQUALS 1.");
            return;
        }   //  end ProcessSecondary


        private void ProcessRecovered(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, string volType)
        {
            //  Reports ST1/ST2
            List<StratumDO> sList = DataLayer.getStratum();

            foreach (StratumDO s in sList)
            {
                List<POPDO> justStratum = POPmethods.GetStratumData(popList, s.Code, currCL);
                //  first capture the sum of x and x squared
                foreach (POPDO p in justStratum)
                {
                    StatList slOne = new StatList();
                    StatList slTwo = new StatList();
                    //  need method for stratum
                    string currMethod = Utilities.MethodLookup(p.Stratum, DataLayer);
                    switch (volType)
                    {
                        case "NET":
                            slOne.SumOfX = p.Stg1NetXRP;
                            slOne.SumOfX2 = p.Stg1NetXRsqrdRP;
                            break;
                        case "GROSS":
                            slOne.SumOfX = p.Stg1GrossXRP;
                            slOne.SumOfX2 = p.Stg1GrossXsqrdRP;
                            break;
                    }   //  end switch on volume type (gross or net)

                    stage1Stats.Add(slOne);
                    CalcAllStats(p, 1, currMethod);

                    //  Stage 2?
                    switch (volType)
                    {
                        case "NET":
                            slTwo.SumOfX = p.Stg2NetXRP;
                            slTwo.SumOfX2 = p.Stg2NetXsqrdRP;
                            break;
                        case "GROSS":
                            slTwo.SumOfX = p.Stg2GrossXRP;
                            slTwo.SumOfX2 = p.Stg2GrossXsqrdRP;
                            break;
                    }   //  end switch on volume type (gross or net)


                    if (slTwo.SumOfX > 0)
                    {
                        stage2Stats.Add(slTwo);
                        CalcAllStats(p, 2, currMethod);
                        CalcCombinedError(currMethod);
                        WriteCurrentGroup(p, strWriteOut, rh, ref pageNumb, 1, 3, currMethod);
                        WriteCurrentGroup(p, strWriteOut, rh, ref pageNumb, 2, 3, currMethod);
                    }
                    else if (slOne.SumOfX > 0)
                    {
                        if (slOne.SumOfX > 0 && slOne.SumOfX2 > 0)
                        {
                            CalcCombinedError(currMethod);
                            WriteCurrentGroup(p, strWriteOut, rh, ref pageNumb, 1, 3, currMethod);
                        }   //  endif
                    }   //  endif stage 2
                    stage1Stats.Clear();
                    stage2Stats.Clear();
                }   //  end foreach loop
                if (numOlines > 0)
                {
                    strWriteOut.WriteLine();
                    numOlines++;
                }   //  endif
            }   //  end foreach on stratum
            //  print footnote
            strWriteOut.WriteLine("  * CANNOT CALCULATE SAMPLING ERROR IF SMALL N EQUALS 1.");
            return;
        }   //  end ProcessRecovered


        private void CalcAllStats(POPDO pop, int whichStage, string currMethod)
        {
            //  Reports ST1/ST2
            //  first capture which stage samples to use
            int stageSamples;
            switch (whichStage)
            {
                case 1:
                    stageSamples = (int) pop.StageOneSamples;
                    if (stageSamples == 1.0)
                        stage1Stats[0].theMean = CommonStatistics.MeanOfX(stage1Stats[0].SumOfX, stageSamples);
                    else
                    {

                        stage1Stats[0].theTvalue = CommonStatistics.LookUpT(stageSamples - 1);
                        stage1Stats[0].theMean = CommonStatistics.MeanOfX(stage1Stats[0].SumOfX, stageSamples);
                        stage1Stats[0].theSD = CommonStatistics.StdDeviation(stage1Stats[0].SumOfX2, stage1Stats[0].SumOfX, stageSamples);
                        stage1Stats[0].theCV = CommonStatistics.CoeffVariation(stage1Stats[0].theMean, stage1Stats[0].theSD);
                        stage1Stats[0].theSE = CommonStatistics.StdError(stage1Stats[0].SumOfX2, stage1Stats[0].SumOfX, stageSamples, currMethod, (float)pop.TalliedTrees, 1);
                        stage1Stats[0].theSampErr = CommonStatistics.SampleError(stage1Stats[0].theMean, stage1Stats[0].theSE, stage1Stats[0].theTvalue);
                    }   //  endif
                    break;
                case 2:
                    stageSamples = (int) pop.StageTwoSamples;
                    if (stageSamples == 1.0 && stage2Stats[0].SumOfX2 > 0)
                        stage2Stats[0].theMean = CommonStatistics.MeanOfX(stage2Stats[0].SumOfX, stageSamples);
                    else if(stageSamples > 0 && stage2Stats[0].SumOfX2 > 0)
                    {
                        stage2Stats[0].theTvalue = CommonStatistics.LookUpT(stageSamples - 1);
                        stage2Stats[0].theMean = CommonStatistics.MeanOfX(stage2Stats[0].SumOfX, stageSamples);
                        stage2Stats[0].theSD = CommonStatistics.StdDeviation(stage2Stats[0].SumOfX2, stage2Stats[0].SumOfX, stageSamples);
                        stage2Stats[0].theCV = CommonStatistics.CoeffVariation(stage2Stats[0].theMean, stage2Stats[0].theSD);
                        stage2Stats[0].theSE = CommonStatistics.StdError(stage2Stats[0].SumOfX2, stage2Stats[0].SumOfX, stageSamples, currMethod, (float)pop.TalliedTrees, 2);
                        stage2Stats[0].theSampErr = CommonStatistics.SampleError(stage2Stats[0].theMean, stage2Stats[0].theSE, stage2Stats[0].theTvalue);
                    }   //  endif
                    break;
            }   //  end switch on stage
        }   //  end CalcAllStats


        private void CalcCombinedError(string currMeth)
        {
            //  Reports ST1/ST2
            if (stage2Stats.Count > 0)
            {
                double ce = CommonStatistics.CombinedSamplingError(currMeth, stage1Stats[0].theSampErr, stage2Stats[0].theSampErr);
                stage1Stats[0].CombSampErr = ce;
                stage2Stats[0].CombSampErr = ce;
            }
            else stage1Stats[0].CombSampErr = stage1Stats[0].theSampErr;
            return;
        }   //  end CalcCombinedError


        private void WriteCurrentGroup(POPDO p, StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, 
                                            int whichStage, int whichProd, string currMeth)
        {
            //  ST1/ST2     (DP1/DP2)
            string fieldFormat1 = "{0,5:F0}";
            string fieldFormat2 = "{0,6:F3}";
            string fieldFormat3 = "{0,8:F1}";
            string fieldFormat4 = "{0,11:F2}";
            string fieldFormat5 = "{0,14:F2}";
            string fieldFormat6 = "{0,11:F4}";
            string fieldFormat7 = "{0,9:F4}";
            string fieldFormat8 = "{0,8:F3}";
            string fieldFormat9 = "{0,9:F3}";

            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                completeHeader, 9, ref pageNumb, "");
            prtFields.Add("");
            prtFields.Add(p.Stratum.PadLeft(2, ' '));
            switch (whichProd)
            {
                case 1:     //  Primary
                    prtFields.Add(p.PrimaryProduct.PadLeft(2, '0'));
                    break;
                case 2:     //  secondary
                case 3:     //  recovered
                    prtFields.Add(p.SecondaryProduct.PadLeft(2, '0'));
                    break;
            }   //  end switch on product

            prtFields.Add(p.UOM.PadLeft(2, ' '));
            prtFields.Add(p.SampleGroup.PadLeft(2, ' '));
            prtFields.Add(p.STM);

            //  now print indicated stage
            switch (whichStage)
            {
                case 1:
                    prtFields.Add("1");
                    //  number of sample trees
                    prtFields.Add(String.Format(fieldFormat1, p.FirstStageTrees));
                    //  Big N
                    switch (currMeth)
                    {
                        case "100":     case "STR":     case "S3P":     case "3P":
                            prtFields.Add(String.Format(fieldFormat1, p.TalliedTrees));
                            break;
                        default:
                            prtFields.Add("    0");
                            break;
                    }   //  end switch on current method
                    //  Small N
                    prtFields.Add(String.Format(fieldFormat1, p.StageOneSamples));

                    prtFields.Add(String.Format(fieldFormat2, stage1Stats[0].theTvalue));
                    prtFields.Add(String.Format(fieldFormat3, stage1Stats[0].theMean));
                    prtFields.Add(String.Format(fieldFormat4, stage1Stats[0].SumOfX));
                    prtFields.Add(String.Format(fieldFormat5, stage1Stats[0].SumOfX2));
                    prtFields.Add(String.Format(fieldFormat6, stage1Stats[0].theSD));
                    prtFields.Add(String.Format(fieldFormat7, stage1Stats[0].theCV));
                    prtFields.Add(String.Format(fieldFormat8, stage1Stats[0].theSE));
                    prtFields.Add(String.Format(fieldFormat7, stage1Stats[0].theSampErr));
                    prtFields.Add(String.Format(fieldFormat9, stage1Stats[0].CombSampErr));
                    break;
                case 2:
                    prtFields.Add("2");
                    //  number of sample trees
                    prtFields.Add(String.Format(fieldFormat1, p.MeasuredTrees));
                    //  Big N becomes stage 1 small N
                    prtFields.Add(String.Format(fieldFormat1, p.StageOneSamples));
                    //  Small N becomes stage 2 samples
                    prtFields.Add(String.Format(fieldFormat1, p.StageTwoSamples));

                    prtFields.Add(String.Format(fieldFormat2, stage2Stats[0].theTvalue));
                    prtFields.Add(String.Format(fieldFormat3, stage2Stats[0].theMean));
                    prtFields.Add(String.Format(fieldFormat4, stage2Stats[0].SumOfX));
                    prtFields.Add(String.Format(fieldFormat5, stage2Stats[0].SumOfX2));
                    prtFields.Add(String.Format(fieldFormat6, stage2Stats[0].theSD));
                    prtFields.Add(String.Format(fieldFormat7, stage2Stats[0].theCV));
                    prtFields.Add(String.Format(fieldFormat8, stage2Stats[0].theSE));
                    prtFields.Add(String.Format(fieldFormat7, stage2Stats[0].theSampErr));
                    prtFields.Add(String.Format(fieldFormat9, stage2Stats[0].CombSampErr));
                    break;
            }   //  end switch on whichStage
            printOneRecord(fieldLengths, prtFields, strWriteOut);
            prtFields.Clear();
            return;
        }   //  end WriteCurrentGroup


        private void finishColumnHeaders(string[] headerToUse, string volType, string prodType)
        {
            //  clear out completeHeader first
            for (int j = 0; j < 9; j++)
                completeHeader[j] = null;

            for (int j = 0; j < headerToUse.Count(); j++)
                completeHeader[j] = headerToUse[j];
        
            completeHeader[2] = completeHeader[2].Replace("ZZZZZZZZZZ", prodType);
            completeHeader[2] = completeHeader[2].Replace("XXXXX", volType);
                                                                
            return;
        }   //  end finishColumnHeaders


        public class StatList
        {
            //  list of statistics variables for output
            public double theMean = 0.0;
            public double SumOfX = 0.0;
            public double SumOfX2 = 0.0;
            public double theSD = 0.0;
            public double theCV = 0.0;
            public double theSE = 0.0;
            public double theSampErr = 0.0;
            public double CombSampErr = 0.0;
            public double theTvalue = 0.0;
        }   //  end StatList
    }

}
