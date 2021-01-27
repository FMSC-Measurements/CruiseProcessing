using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using System.Runtime.InteropServices;


namespace CruiseProcessing
{
    class OutputWeight : CreateTextFile
    {
        [DllImport("vollib.dll",CallingConvention = CallingConvention.Cdecl)] static extern void BROWNCROWNFRACTION(ref int SPCD, ref float DBH, ref float THT, ref float CR, float[] CFWT);
        [DllImport("vollib.dll",CallingConvention = CallingConvention.Cdecl)] static extern void BROWNTOPWOOD(ref int SPN, ref float GCUFTS, ref float WT);
        [DllImport("vollib.dll",CallingConvention = CallingConvention.Cdecl)] static extern void BROWNCULLLOG(ref int SPN, ref float GCUFTS, ref float WT);
        [DllImport("vollib.dll",CallingConvention = CallingConvention.Cdecl)] static extern void BROWNCULLCHUNK(ref int SPN, ref float GCUFT, ref float NCUFT, ref float FLIW, ref float WT);

        #region
        public string currentReport;
        private int[] fieldLengths;
        private ArrayList prtFields;
        private List<BiomassEquationDO> bioList = new List<BiomassEquationDO>();
        private List<StratumDO> sList = new List<StratumDO>();
        private List<CuttingUnitDO> cList = new List<CuttingUnitDO>();
        private float currWFprimary;
        private float currWFsecondary;
        private float currPCRprimary;
        private float currPCRsecondary;
        private int footFlag = 0;
        private double totPrim = 0;
        private double totSec = 0;
        private double totTip = 0;
        private double totBran = 0;
        private double totFol = 0;
        private double totTree = 0;
        private double subtotPrim = 0;
        private double subtotSec = 0;
        private double subtotTip = 0;
        private double subtotBran = 0;
        private double subtotFol = 0;
        private double subtotTree = 0;
        private List<ReportSubtotal> rptSubtotals = new List<ReportSubtotal>();
        #endregion

        public void OutputWeightReports(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            string currentTitle = fillReportTitle(currentReport);

            numOlines = 0;
            sList = bslyr.getStratum();
            cList = bslyr.getCuttingUnits();
            //  pull biomass equations to process reports (except WT2/WT3)
            if (currentReport != "WT2" && currentReport != "WT3" && currentReport != "WT5")
            {
                bioList = bslyr.getBiomassEquations();
                if (bioList.Count > 0)
                {
                    //  check weight factors -- if zero, likely there is no weight to report for any of these reports
                    double checkSum = bioList.Sum(b => b.WeightFactorPrimary);
                    if (checkSum == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, ">>  No weight factors so no weights for this report");
                        return;
                    }   //  endif checkSum

                    //  Also check for no cubic foot volume in LCD
                    List<LCDDO> wholeList = bslyr.getLCD();
                    if ((checkSum = wholeList.Sum(l => l.SumGCUFT)) == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, ">>  No cubic foot volume for this report");
                        return;
                    }   //  endif checkSum

                    //  also check for no weights
                    if ((checkSum = wholeList.Sum(l => l.SumWgtMSP)) == 0)
                    {
                        noDataForReport(strWriteOut, currentReport, ">>  No weight for this report");
                        return;
                    }   //  endif checkSum
                }
                else if (bioList.Count == 0)
                {
                    noDataForReport(strWriteOut, currentReport, ">>  No equations entered so no weights for this report");
                    return;
                }   //  endif
            }   //  endif on currentReport
            prtFields = new ArrayList();

            switch (currentReport)
            {
                case "WT1":
                    //  pull LCD groups to process
                    List<LCDDO> justGroups = bslyr.GetLCDgroup("", 4, "C");
                    fieldLengths = new int[] { 12, 14, 10, 3, 6, 11, 12, 15, 10, 15, 10 };
                    rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
                    processLCDgroups(strWriteOut, justGroups, rh,ref pageNumb);
                    WriteSubtotal(strWriteOut, rh, ref pageNumb);
                    break;
                case "WT2":     case "WT3":
                    rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
                    processSlashLoad(strWriteOut, rh, ref pageNumb);
                    break;
                case "WT4":
                    fieldLengths = new int[] { 3, 11, 13, 17, 18, 19, 5 };
                    rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.BCUFS, "");
                    rh.reportTitles[2] = reportConstants.FCTO;
                    processUnits(strWriteOut, cList, rh, ref pageNumb);
                    break;
                case "WT5":
                    sList = bslyr.getStratum();
                    fieldLengths = new int[] { 1, 10, 4, 3, 11, 12, 12, 12, 11, 2, 11, 8 };
                    rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
                    processSaleSummary(strWriteOut, rh, ref pageNumb);
                    break;
            }   //  end switch on report
            return;
        }   //  end OutputWeightReports

        private void processLCDgroups(StreamWriter strWriteOut, List<LCDDO> justGroups, reportHeaders rh, ref int pageNumb)
        {
            //  loop through groups list and print each species, product, contract species combination
            double currGRS = 0.0;
            double currGRST = 0.0;
            foreach (LCDDO jg in justGroups)
            {
                //  pull group from LCD
                StringBuilder sb = new StringBuilder();
                sb.Clear();
                sb.Append("WHERE Species = @p1 AND PrimaryProduct = @p2 AND SecondaryProduct = @p3 AND ");
                sb.Append(" LiveDead = @p4 AND ContractSpecies = @p5 AND CutLeave = @p6");
                List<LCDDO> groupData = bslyr.GetLCDdata(sb.ToString(), jg, 4, "");

                //  Find weight factors etc for current group
                BiomassEqMethods.FindFactor(bioList, jg.Species, "PrimaryProd", jg.PrimaryProduct, 
                                            jg.LiveDead, ref currPCRprimary, ref currWFprimary);
                BiomassEqMethods.FindFactor(bioList, jg.Species, "SecondaryProd", jg.PrimaryProduct, 
                                            jg.LiveDead, ref currPCRsecondary, ref currWFsecondary);

                double poundsPP = 0;
                double poundsSP = 0;
                if (currWFprimary == 0.0)
                    footFlag = 1;
                else
                {
                    //  sum gross cuft primary and secondary
                    foreach (LCDDO gd in groupData)
                    {
                        int currSTRcn = StratumMethods.GetStratumCN(gd.Stratum, sList);
                        double STacres = Utilities.ReturnCorrectAcres(gd.Stratum, bslyr, currSTRcn);
                        currGRS += gd.SumGCUFT * STacres;
                        currGRST += gd.SumGCUFTtop * STacres;
                    }   //  end foreach loop
                }   //  endif weight factor is zero
                //  write current group
                if (currGRS > 0 || currGRST > 0)
                {
                    WriteCurrentGroup(strWriteOut, jg, currGRS, currGRST, ref poundsPP, ref poundsSP, rh, ref pageNumb);
                    //  create a line for the subtotal if needed
                    UpdateSubtotal(jg.ContractSpecies, jg.PrimaryProduct, jg.SecondaryProduct, currGRS, currGRST, 
                                    poundsPP, poundsSP);
                    //  reset variables
                    currGRS = 0.0;
                    currGRST = 0.0;
                    poundsPP = 0.0;
                    poundsSP = 0.0;
                }   //  endif
            }   //  end foreach loop
            return;
        }   //  end processLCDgroups


        private void processUnits(StreamWriter strWriteOut, List<CuttingUnitDO> cList, reportHeaders rh, ref int pageNumb)
        {
            //  WT4
            int firstLine = 1;
            string currMeth;
            double prorateFactor = 0;
            double unitSaw = 0;
            double unitNonsawPP = 0;
            double unitNonsawSP = 0;
            double totalUnitSaw = 0;
            double totalUnitNonsawPP = 0;
            double totalUnitNonsawSP = 0;
            double grandTotalSaw = 0;
            double grandTotalNonsawPP = 0;
            double grandTotalNonsawSP = 0;
            List<PRODO> proList = bslyr.getPRO();
            List<LCDDO> lcdList = bslyr.getLCD();

            foreach (CuttingUnitDO cdo in cList)
            {
                cdo.Strata.Populate();
                //  get species groups from LCD
                List<LCDDO> justGroups = bslyr.GetLCDgroup("", 5, "C");
                foreach (LCDDO jg in justGroups)
                {
                    //  loop through stratum in the current unit
                    foreach (StratumDO sd in cdo.Strata)
                    {
                        //  get group data
                        List<LCDDO> groupData = LCDmethods.GetCutOrLeave(lcdList, "C", jg.Species, sd.Code, "");
                        foreach (LCDDO gd in groupData)
                        {
                            currMeth = Utilities.MethodLookup(gd.Stratum, bslyr);
                            if (currMeth == "100" || gd.STM == "Y")
                            {
                                //pull all trees for current unit
                                List<TreeCalculatedValuesDO> currentGroup = new List<TreeCalculatedValuesDO>();
                                List<TreeCalculatedValuesDO> justUnitTrees = bslyr.getTreeCalculatedValues((int)sd.Stratum_CN, (int)cdo.CuttingUnit_CN);
                                if (gd.STM == "Y")
                                {
                                    //  pull sure-to-measure trees for current unit
                                    currentGroup = justUnitTrees.FindAll(
                                        delegate(TreeCalculatedValuesDO jut)
                                        {
                                            return jut.Tree.STM == "Y";
                                        });
                                }
                                else if (currMeth == "100")
                                {
                                    //  pull all trees for current unit
                                    currentGroup = justUnitTrees.FindAll(
                                        delegate(TreeCalculatedValuesDO jut)
                                        {
                                            return jut.Tree.Species == gd.Species;
                                        });
                                }   //  endif
                                //  sum up weights based on product 
                                foreach (TreeCalculatedValuesDO cg in currentGroup)
                                {
                                    switch (cg.Tree.SampleGroup.PrimaryProduct)
                                    {
                                        case "01":
                                            unitSaw += cg.BiomassMainStemPrimary * cg.Tree.ExpansionFactor;
                                            break;
                                        default:
                                            unitNonsawPP = cg.BiomassMainStemPrimary * cg.Tree.ExpansionFactor;
                                            break;
                                    }   //  end switch on product
                                    unitNonsawSP = cg.BiomassMainStemSecondary * cg.Tree.ExpansionFactor;
                                }   //  end foreach loop
                            }

                            else
                            {
                                //  find proration factor for current group
                                List<PRODO> pList = PROmethods.GetMultipleData(proList, "C", gd.Stratum,
                                                                cdo.Code, gd.SampleGroup, "", "", gd.STM, 1);
                                if (pList.Count == 1) prorateFactor = (float)pList[0].ProrationFactor;
                                //  Sum up weights by product
                                switch (gd.PrimaryProduct)
                                {
                                    case "01":
                                        unitSaw += gd.SumWgtMSP * prorateFactor;
                                        break;
                                    default:
                                        unitNonsawPP += gd.SumWgtMSP * prorateFactor;
                                        break;
                                }   //  end switch
                                unitNonsawSP += gd.SumWgtMSS * prorateFactor;
                            }   //  endif
                        }   //  end foreach loop on group
                    }   //  end foreach loop on stratum
                    if(unitSaw > 0 || unitNonsawPP > 0)
                        WriteCurrentGroup(strWriteOut, rh, ref pageNumb, ref firstLine, unitSaw, unitNonsawPP, unitNonsawSP, 
                                        cdo.Area, cdo.Code, jg.Species);

                    //  Update subtotals
                    totalUnitSaw += unitSaw / 2000;
                    totalUnitNonsawPP += unitNonsawPP / 2000;
                    totalUnitNonsawSP += unitNonsawSP / 2000;
                    unitSaw = 0;
                    unitNonsawPP = 0;
                    unitNonsawSP = 0;
                    prtFields.Clear();
                }   //  end foreach loop
                if (totalUnitSaw > 0)
                {
                    //  Output subtotal line
                    OutputTotalLine(strWriteOut, rh, ref pageNumb, totalUnitSaw, totalUnitNonsawPP, totalUnitNonsawSP, 1);
                }   //  endif totalUnitSaw
                //  update grand total
                grandTotalSaw += totalUnitSaw;
                grandTotalNonsawPP += totalUnitNonsawPP;
                grandTotalNonsawSP += totalUnitNonsawSP;
                totalUnitSaw = 0;
                totalUnitNonsawPP = 0;
                totalUnitNonsawSP = 0;
                firstLine = 1;
            }   //  end foreach loop
            //  output grand total here
            OutputTotalLine(strWriteOut, rh, ref pageNumb, grandTotalSaw, grandTotalNonsawPP, grandTotalNonsawSP, 2);
            return;
        }   //  end processUnits

        
        private void processSaleSummary(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  WT5
            List<LCDDO> lcdList = bslyr.getLCD();
            string[] completeHeader = new string[4];

            prtFields.Clear();
            foreach (StratumDO sd in sList)
            {
                    double STacres = Utilities.ReturnCorrectAcres(sd.Code, bslyr, (long)sd.Stratum_CN);
                    finishColumnHeaders(rh.WT5columns, sd.Code, ref completeHeader);

                    //  pull stratum and species groups from LCD
                    List<LCDDO> justGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2 GROUP BY ", "Species", "C", sd.Code);
                    //  loop by species groups
                    foreach (LCDDO jg in justGroups)
                    {
                        List<LCDDO> groupData = LCDmethods.GetCutOrLeave(lcdList, "C", jg.Species, sd.Code, "");
                        totPrim = (groupData.Sum(gd => gd.SumWgtMSP)) * STacres;
                        totSec = (groupData.Sum(gd => gd.SumWgtMSS)) * STacres;
                        totTip = (groupData.Sum(gd => gd.SumWgtTip)) * STacres;
                        totBran = ((groupData.Sum(gd => gd.SumWgtBBL)) + (groupData.Sum(gd => gd.SumWgtBBD))) * STacres;
                        totFol = (groupData.Sum(gd => gd.SumWgtBFT)) * STacres;
                        totTree = (groupData.Sum(gd => gd.SumWgtBAT)) * STacres;

                        if(totTree > 0)
                            WriteCurrentGroup(strWriteOut, rh, ref pageNumb, jg.Species, jg.BiomassProduct.ToString(), totPrim, totSec,
                                                        totTip, totBran, totFol, totTree, completeHeader);
                        //  Update subtotal
                        subtotPrim += totPrim;
                        subtotSec += totSec;
                        subtotTip += totTip;
                        subtotBran += totBran;
                        subtotFol += totFol;
                        subtotTree += totTree;
                        totPrim = 0;
                        totSec = 0;
                        totTip = 0;
                        totBran = 0;
                        totFol = 0;
                        totTree = 0;
                    }   //  end foreach loop
                    //  print subtotal line here
                    OutputSubtotal(strWriteOut, rh, ref pageNumb, subtotPrim, subtotSec, subtotTip, subtotBran, 
                                    subtotFol, subtotTree, completeHeader);
                    //  each strata prints on a separate page so reset number of lines and subtotals
                    numOlines = 0;
                    subtotPrim = 0;
                    subtotSec = 0;
                    subtotTip = 0;
                    subtotBran = 0;
                    subtotFol = 0;
                    subtotTree = 0;
                }   //  end foreach loop
                //  overall summary page
                OverallSummary(strWriteOut, rh, ref pageNumb, lcdList);
            return;
        }   //  end processSaleSummary


        private void finishColumnHeaders(string[] columnsToUpdate, string currST, ref string[] completeHeader)
        {
            completeHeader[0] = columnsToUpdate[0].Replace("XX", currST);
            for (int k = 1; k < 4; k++)
                completeHeader[k] = columnsToUpdate[k];

                return;
        }   //  end finishColumnHeaders


        private void WriteCurrentGroup(StreamWriter strWriteOut, LCDDO jg, double currGRS, double currGRST,
                                        ref double poundsPP, ref double poundsSP, reportHeaders rh, ref int pageNumb)
        {
            //  WT1 only
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                               rh.WT1columns, 3, ref pageNumb, "");
            
            if (currGRS > 0)
            {
                //  write primary product 
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(jg.Species.PadRight(6, ' '));
                if (jg.ContractSpecies == null)
                    prtFields.Add("    ");
                else prtFields.Add(jg.ContractSpecies.PadLeft(4, ' '));
                prtFields.Add(jg.PrimaryProduct.PadLeft(2, '0'));
                prtFields.Add("P");
                prtFields.Add(Utilities.Format("{0,8:F0}", Math.Floor(currGRS + 0.501)).ToString().PadLeft(8, ' '));
                prtFields.Add(Utilities.Format("{0,8:F2}", currWFprimary).ToString().PadLeft(8, ' '));
                prtFields.Add(Utilities.Format("{0,11:F0}", Math.Round(currGRS, 0, MidpointRounding.AwayFromZero) * currWFprimary).ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.Format("{0,6:F2}", currPCRprimary).ToString().PadLeft(6, ' '));
                poundsPP = Math.Round(currGRS, 0, MidpointRounding.AwayFromZero) * currWFprimary * (currPCRprimary / 100);
                prtFields.Add(Utilities.Format("{0,11:F0}", poundsPP).ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.Format("{0,10:F2}", poundsPP / 2000).ToString().PadLeft(10, ' '));
                printOneRecord(fieldLengths, prtFields, strWriteOut);
                numOlines++;
            }   //  endif

            //  clear print fields and print secondary product
            if (currGRST > 0)
            {
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(jg.Species.PadRight(6, ' '));
                if (jg.ContractSpecies == null)
                    prtFields.Add("    ");
                else prtFields.Add(jg.ContractSpecies.PadLeft(4, ' '));
                prtFields.Add(jg.SecondaryProduct.PadLeft(2, '0'));
                prtFields.Add("S");
                prtFields.Add(Utilities.Format("{0,8:F0}", Math.Floor(currGRST + 0.501)).ToString().PadLeft(8, ' '));
                if (currWFsecondary == 0.0) currWFsecondary = currWFprimary;
                if (currPCRsecondary == 0.0) currPCRsecondary = currPCRprimary;
                prtFields.Add(Utilities.Format("{0,8:F2}", currWFsecondary).ToString().PadLeft(8, ' '));
                prtFields.Add(Utilities.Format("{0,11:F0}", Math.Round(currGRST, 0, MidpointRounding.AwayFromZero) * currWFsecondary).ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.Format("{0,6:F2}", currPCRsecondary).ToString().PadLeft(6, ' '));
                poundsSP = Math.Round(currGRST, 0, MidpointRounding.AwayFromZero) * currWFsecondary * (currPCRsecondary / 100);
                prtFields.Add(Utilities.Format("{0,11:F0}", poundsSP).ToString().PadLeft(11, ' '));
                prtFields.Add(Utilities.Format("{0,10:F2}", poundsSP / 2000).ToString().PadLeft(10, ' '));
                printOneRecord(fieldLengths, prtFields, strWriteOut);
                numOlines++;
            }   //  endif secondary has volume
            return;
        }   //  end Write CurrentGroup


        private void WriteCurrentGroup(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, ref int firstLine,
                                        double unitSaw, double unitNonsawPP, double unitNonsawSP,
                                        double unitAcres, string unitCode, string currSP)
        {
            //  WT4 only
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                    rh.WT4columns, 10, ref pageNumb, "");
            prtFields.Add("");
            if (firstLine == 1)
            {
                prtFields.Add(unitCode.PadLeft(3, ' '));
                prtFields.Add(Math.Round(unitAcres,1,MidpointRounding.AwayFromZero).ToString().PadLeft(5, ' '));
                firstLine = 0;
            }
            else
            {
                prtFields.Add("   ");
                prtFields.Add("     ");
            }   //  endif it's the first line
            prtFields.Add(currSP.PadLeft(6, ' '));
            prtFields.Add(Utilities.Format("{0,5:F2}", unitSaw / 2000).ToString().PadLeft(8, ' '));
            prtFields.Add(Utilities.Format("{0,5:F2}", unitNonsawPP / 2000).ToString().PadLeft(8, ' '));
            prtFields.Add(Utilities.Format("{0,5:F2}", unitNonsawSP / 2000).ToString().PadLeft(8, ' '));
            printOneRecord(fieldLengths, prtFields, strWriteOut);
        }   //  end WriteCurrentGroup


        private void WriteCurrentGroup(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, string currSP, 
                                        string currBP, double MSprim, double MSsecd, double bioTip, double bioBran, 
                                        double bioFol, double bioTot, string[] completeHeader)
        {
            //  WT5 only
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], completeHeader, 4, ref pageNumb, "");
            prtFields.Add("");
            prtFields.Add(currSP.PadRight(6, ' '));
            //  removed product code as it was confusing to users.  may 2016
            //prtFields.Add(currBP.PadLeft(2, '0'));
            prtFields.Add("  ");
            prtFields.Add("|"  );
            prtFields.Add(Utilities.Format("{0,8:F1}", MSprim / 2000).ToString().PadLeft(8, ' '));
            prtFields.Add(Utilities.Format("{0,8:F1}", MSsecd / 2000).ToString().PadLeft(8, ' '));
            prtFields.Add(Utilities.Format("{0,8:F1}", bioTip / 2000).ToString().PadLeft(8, ' '));
            prtFields.Add(Utilities.Format("{0,8:F1}", bioBran / 2000).ToString().PadLeft(8, ' '));
            prtFields.Add(Utilities.Format("{0,8:F1}", bioFol / 2000).ToString().PadLeft(8, ' '));
            prtFields.Add("|");
            prtFields.Add(Utilities.Format("{0,8:F1}", bioTot / 2000).ToString().PadLeft(8, ' '));
            double overallTotal = MSprim + MSsecd + bioTip + bioBran + bioFol;
            prtFields.Add(Utilities.Format("{0,8:F1}", overallTotal / 2000).ToString().PadLeft(8, ' '));
            printOneRecord(fieldLengths, prtFields, strWriteOut);
            prtFields.Clear();
            return;
        }   //  end WriteCurrentGroup



        private void UpdateSubtotal(string currCS, string currPP, string currSP, double currGRS, double currGRST, 
                                    double poundsPP, double poundsSP)
        {
            //  currently works for WT1
            //  calculate tons removed to update subtotal

            double tonsRemovedPP = poundsPP / 2000;
            double tonsRemovedSP = poundsSP / 2000;
                
            //  find group in subtotal list
            int nthRow = rptSubtotals.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == currCS && rs.Value2 == currPP;
                });
            if (nthRow >= 0)
            {
                rptSubtotals[nthRow].Value3 += tonsRemovedPP;
            }
            else
            {
                //  group not in the subtotal list so add it
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value1 = currCS;
                rs.Value2 = currPP;
                rs.Value3 = tonsRemovedPP;
                rptSubtotals.Add(rs);
            }   //  endif

            //  repeat for secondary product
            nthRow = rptSubtotals.FindIndex(
                delegate(ReportSubtotal rs)
                {
                    return rs.Value1 == currCS && rs.Value2 == currSP;
                });
            if (nthRow >= 0)
                rptSubtotals[nthRow].Value4 += tonsRemovedSP;
            else
            {
                //  add group
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value1 = currCS;
                rs.Value2 = currSP;
                rs.Value4 = tonsRemovedSP;
                rptSubtotals.Add(rs);
            }   //  endif 
            return;
        }   //  end UpdateSubtotal


        private void WriteSubtotal(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  WT1
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], rh.WT1columns, 3, ref pageNumb, "");

            strWriteOut.WriteLine("\n");
            for (int k = 0; k < 3; k++)
                strWriteOut.WriteLine(rh.WT1total[k]);
            strWriteOut.WriteLine(reportConstants.longLine);
            double totalTons = 0;
            foreach (ReportSubtotal rs in rptSubtotals)
            {
                // (rs.Value3 > 0 || rs.Value4 > 0)
                //{
                //    strWriteOut.Write("                      ");
                //    strWriteOut.Write(rs.Value1.PadLeft(4, ' '));
                //    strWriteOut.Write("          ");
                //    strWriteOut.Write(rs.Value2.PadRight(3, ' '));
                //}   //endif
                //  the above code misses primary product of 20 and a secondary product of 20
                //  it mislabels primary product 20
                //  changed to put secondary tons in value4 keeping primary in value 3
                //  October 2015
                //if (rs.Value2 == "01")
                  //  strWriteOut.Write("P      ");
                //else strWriteOut.Write("S      ");
                if (rs.Value3 > 0)
                {
                    strWriteOut.Write("                      ");
                    strWriteOut.Write(rs.Value1.PadLeft(4, ' '));
                    strWriteOut.Write("          ");
                    strWriteOut.Write(rs.Value2.PadRight(3, ' '));
                    strWriteOut.Write("P      ");
                    strWriteOut.WriteLine(Utilities.Format("{0,10:F2}", rs.Value3).ToString().PadLeft(10, ' '));
                    totalTons += rs.Value3;
                }
                if (rs.Value4 > 0)
                {
                    strWriteOut.Write("                      ");
                    strWriteOut.Write(rs.Value1.PadLeft(4, ' '));
                    strWriteOut.Write("          ");
                    strWriteOut.Write(rs.Value2.PadRight(3, ' '));
                    strWriteOut.Write("S      ");
                    strWriteOut.WriteLine(Utilities.Format("{0,10:F2}", rs.Value4).ToString().PadLeft(10, ' '));
                    totalTons += rs.Value4;
                }   //  endif
            }   //  end foreach loop on subtotals
            strWriteOut.WriteLine("           						  ____________________________________");
            strWriteOut.Write("                   TOTAL TONS REMOVED         ");
            strWriteOut.WriteLine(Utilities.Format("{0,10:F2}", totalTons).ToString().PadLeft(10, ' '));
            strWriteOut.WriteLine("\n");
            //  write footer
            for (int k = 0; k < 5; k++)
                strWriteOut.WriteLine(rh.WT1footer[k]);

            if (footFlag == 1)
            {
                //  write note that weight factor could not be found
                strWriteOut.WriteLine("\n", "NOTE:  Weight factor could not be found for certain groups.\nGroup not printed in report.");
            }   //  endif footFlag
            return;
        }   //  end WriteSubtotal

        private void OutputTotalLine(StreamWriter strWriteOut,reportHeaders rh, ref int pageNumb, double totalValue1, 
                                    double totalValue2, double totalValue3, int whichTotal)
        {
            //  WT4
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    rh.WT4columns, 10, ref pageNumb, "");
            strWriteOut.WriteLine("                                            __________________________________________________");
            if (whichTotal == 1)
            {
                strWriteOut.Write("        SUBTOTAL                            ");
                strWriteOut.Write("{0,8:F2}", totalValue1);
                strWriteOut.Write("          ");
                strWriteOut.Write("{0,8:F2}", totalValue2);
                strWriteOut.Write("             ");
                strWriteOut.WriteLine("{0,6:F2}", totalValue3);
                strWriteOut.WriteLine("");
                numOlines += 3;
            }
            else if (whichTotal == 2)
            {
                strWriteOut.Write("        GRAND TOTAL                     ");
                strWriteOut.Write("{0,12:F2}", totalValue1);
                strWriteOut.Write("         ");
                strWriteOut.Write("{0,9:F2}", totalValue2);
                strWriteOut.Write("          ");
                strWriteOut.WriteLine("{0,9:F2}", totalValue3);
            }   //  endif on type of total line
            return;
        }   //  end OutputTotalLine

        private void OutputSubtotal(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, 
                                    double subValue1, double subValue2, double subValue3, 
                                    double subValue4, double subValue5, double subValue6,
                                    string[] completeHeader)
        {
            //  WT5
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                completeHeader, 4, ref pageNumb, "");
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.Write(" ALL SPECIES   |  ");
            strWriteOut.Write(Utilities.Format("{0,8:F1}", subValue1 / 2000).ToString().PadLeft(8, ' '));
            strWriteOut.Write("   ");
            strWriteOut.Write(Utilities.Format("{0,8:F1}", subValue2 / 2000).ToString().PadLeft(8, ' '));
            strWriteOut.Write("    ");
            strWriteOut.Write(Utilities.Format("{0,8:F1}", subValue3 / 2000).ToString().PadLeft(8, ' '));
            strWriteOut.Write("    ");
            strWriteOut.Write(Utilities.Format("{0,8:F1}", subValue4 / 2000).ToString().PadLeft(8, ' '));
            strWriteOut.Write("    ");
            strWriteOut.Write(Utilities.Format("{0,8:F1}", subValue5 / 2000).ToString().PadLeft(8, ' '));
            strWriteOut.Write("   | ");
            strWriteOut.Write(Utilities.Format("{0,8:F1}", subValue6 / 2000).ToString().PadLeft(8, ' '));
            strWriteOut.Write("   ");
            double overallTotal = subValue1 + subValue2 + subValue3 + subValue4 + subValue5;
            strWriteOut.WriteLine(Utilities.Format("{0,8:F1}", overallTotal / 2000).ToString().PadLeft(8, ' '));
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine(rh.WT5footer[0]);
            strWriteOut.WriteLine(rh.WT5footer[1]);

            return;
        }   //  end OutputSubtotal


        private void OverallSummary(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, List<LCDDO> lcdList)
        {
            //  WT5 only
            double STacres;
            //  need to finish the header
            string[] completeHeader = new string[4];
            finishColumnHeaders(rh.WT5columns, "OVERALL SALE SUMMARY", ref completeHeader);
            //  Order LCD by species and biomass product
            List<LCDDO> justGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 GROUP BY ", "Species,BiomassProduct", "C", "");
            foreach (LCDDO jg in justGroups)
            {
                List<LCDDO> groupData = LCDmethods.GetCutOrLeave(lcdList, "C", jg.Species, "", "");
                //  Sum up weights times strata acres
                foreach (LCDDO gd in groupData)
                {
                    //  find proper strata acres
                    long currStrCN = StratumMethods.GetStratumCN(gd.Stratum, sList);
                    STacres = Utilities.ReturnCorrectAcres(gd.Stratum, bslyr, currStrCN);
                    totPrim += gd.SumWgtMSP * STacres;
                    totSec += gd.SumWgtMSS * STacres;
                    totTip += gd.SumWgtTip * STacres;
                    totBran += (gd.SumWgtBBL + gd.SumWgtBBD) * STacres;
                    totFol += gd.SumWgtBFT * STacres;
                    totTree += gd.SumWgtBAT * STacres;
                }   //  end foreach loop on data
                //  Write current group
                if(totTree > 0)
                    WriteCurrentGroup(strWriteOut, rh, ref pageNumb, jg.Species, jg.BiomassProduct.ToString(), totPrim, 
                                            totSec, totTip, totBran, totFol, totTree, completeHeader);
                //  update totals
                subtotPrim += totPrim;
                subtotSec += totSec;
                subtotTip += totTip;
                subtotBran += totBran;
                subtotFol += totFol;
                subtotTree += totTree;
                totPrim = 0;
                totSec = 0;
                totTip = 0;
                totBran = 0;
                totFol = 0;
                totTree = 0;
            }   //  end foreach group

            //  output subtotals
            OutputSubtotal(strWriteOut, rh, ref pageNumb, subtotPrim, subtotSec, subtotTip, subtotBran, subtotFol, 
                                    subtotTree, completeHeader);
            return;
        }   //  end OverallSummary


        private void processSlashLoad(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  First, need region, forest and district
            string currReg = bslyr.getRegion();
            string currFor = bslyr.getForest();
            string currDist = bslyr.getDistrict();
            double currAcres = 0;
            string currST;

            numOlines = 0;
            List<BiomassData> bList = new List<BiomassData>();
                
            switch (currentReport)
            {
                case "WT2":
                    List<TreeDefaultValueDO> tdvList = bslyr.getTreeDefaults();
                    //  Load summary groups for last page
                    List<BiomassData> summaryList = new List<BiomassData>();
                    LoadSummaryGroups(summaryList, tdvList);
                    //  process by stratum since each page is a separate stratum or cutting unit
                    foreach (StratumDO s in sList)
                    {
                        currST = s.Code;
                        //  pull all trees for the current stratum as well as current acres and the method
                        List<TreeCalculatedValuesDO> treeList = bslyr.getTreeCalculatedValues((int)s.Stratum_CN);
                        currAcres = Utilities.ReturnCorrectAcres(s.Code, bslyr, (long)s.Stratum_CN);
                        //  Pull stratum and species groups from LCD
                        List<LCDDO> justGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2 ", 
                                                                "GROUP BY Species", "C", s.Code, "");
                        //  Load these groups into a BiomassData list
                        foreach (LCDDO jg in justGroups)
                        {
                            int ithRow = tdvList.FindIndex(
                                delegate(TreeDefaultValueDO t)
                                {
                                    return t.Species == jg.Species;
                                });
                            BiomassData b = new BiomassData();
                            b.userStratum = jg.Stratum;
                            b.userSG = jg.SampleGroup;
                            b.userSpecies = jg.Species;
                            b.bioSpecies = (int)tdvList[ithRow].FIAcode;
                            bList.Add(b);
                        }   //  end foreach loop

                        //  Calculate and print each stratum for a WT2 report
                        foreach (LCDDO jg in justGroups)
                        {
                            //  pull all trees for the stratum
                            List<TreeCalculatedValuesDO> tcvList = treeList.FindAll(
                                delegate(TreeCalculatedValuesDO tcv)
                                {
                                    return tcv.Tree.SampleGroup.CutLeave == "C" && 
                                            tcv.Tree.Stratum.Code == jg.Stratum &&
                                            tcv.Tree.Species == jg.Species && 
                                            tcv.Tree.SampleGroup.Code == jg.SampleGroup &&
                                            tcv.Tree.CountOrMeasure == "M";
                                });
                            //  Calculate and store stratum values
                            CalculateComponentValues(tcvList, currAcres, jg, bList, currReg, currFor, currDist);
                                      
                        }   //  end foreach loop
                        //  if any one bioSpecies in the bList is zero, skip reporet
                        foreach(BiomassData bl in bList)
                        {
                            if(bl.bioSpecies == 0)
                            {
                                strWriteOut.Write("Missing some FIA codes.\nCannot produce report  .");
                                strWriteOut.WriteLine(currentReport);
                                return;
                            }   //  endif
                        }   //  end foreach
                        //  write page for current stratum or cutting unit
                        WriteCurrentGroup(strWriteOut, rh, ref pageNumb, currST, bList, "");
                        //  update summary list for last page
                        UpdateSummaryList(summaryList, bList);
                        //  clear list for next stratum or cutting unit
                        bList.Clear();
                        numOlines = 0;
                    }   //  end foreach loop on stratum or cutting unit
                    //  output summary page
                    WriteCurrentGroup(strWriteOut, rh, ref pageNumb, "SALE", summaryList, "");
                    break;
                case "WT3":
                    cList = bslyr.getCuttingUnits();
                    tdvList = bslyr.getTreeDefaults();
                    foreach (CuttingUnitDO c in cList)
                    {
                        c.Strata.Populate();
                        foreach (StratumDO stratum in c.Strata)
                        {
                            //  pull trees for the current stratum
                            List<TreeCalculatedValuesDO> treeList = bslyr.getTreeCalculatedValues((int)stratum.Stratum_CN);
                             //  Pull stratum and species groups from LCD
                            List<LCDDO> justGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2 ",
                                                            "GROUP BY Species", "C", stratum.Code, "");
                            //  Load this groups into the Biomass Data list
                            currAcres = Utilities.ReturnCorrectAcres(stratum.Code, bslyr, 
                                                (long)stratum.Stratum_CN);
                            foreach (LCDDO jg in justGroups)
                            {
                                int nthRow = bList.FindIndex(
                                    delegate(BiomassData bd)
                                    {
                                        return bd.userSpecies == jg.Species;
                                    });
                                //  February 2018 -- also need the FIA code from default values
                                //  for current species
                                int ithRow = tdvList.FindIndex(
                                    delegate(TreeDefaultValueDO t)
                                    {
                                      return t.Species == jg.Species;
                                    });
                                if(nthRow < 0)
                                {
                                    BiomassData b = new BiomassData();
                                    b.userStratum = jg.Stratum;
                                    b.userSG = jg.SampleGroup;
                                    b.userSpecies = jg.Species;
                                    if(ithRow >=0)
                                        b.bioSpecies = (int)tdvList[ithRow].FIAcode;
                                    bList.Add(b);
                                }   //  endif
                            }   //  end foreach loop
                            //  caluclate each stratum for the WT3 report
                            foreach (LCDDO jg in justGroups)
                            {
                                //  pull trees for the group
                                List<TreeCalculatedValuesDO> justTrees = treeList.FindAll(
                                    delegate(TreeCalculatedValuesDO tc)
                                    {
                                        return tc.Tree.SampleGroup.CutLeave == "C" &&
                                            tc.Tree.Stratum.Code == jg.Stratum &&
                                            tc.Tree.Species == jg.Species &&
                                            tc.Tree.SampleGroup.Code == jg.SampleGroup &&
                                            tc.Tree.CountOrMeasure == "M" &&
                                            tc.Tree.CuttingUnit.Code == c.Code;
                                    });
                                //  Store stratum values
                                CalculateComponentValues(justTrees, currAcres, jg, bList, currReg, currFor, currDist);
                            }   //  end foreach loop on groups
                        }   //  end for j loop on stratum
                        //  print cutting unit page here
                        List<BiomassData> unitList = SumUpUnitList(strWriteOut, rh, ref pageNumb, bList,
                                                                c.Code, c.Area);//  if any one bioSpecies in the bList is zero, skip reporet
                        foreach (BiomassData bl in bList)
                        {
                            if (bl.bioSpecies == 0)
                            {
                                noDataForReport(strWriteOut, currentReport, " >>>Missing some FIA codes.  Cannot produce report.");
                                return;
                            }   //  endif
                        }   //  end foreach
                        WriteCurrentGroup(strWriteOut, rh, ref pageNumb, "", unitList, c.Code);
                        //  clear biomass data list for next cutting unit
                        bList.Clear();
                        numOlines = 0;
                    }   //  end foreach loop cutting unit
                    break;
            }   //  end switch on report
            return;
        }   //  end processSlashLoad


        private int CalculateComponentValues(List<TreeCalculatedValuesDO> currentData, double currAcres, 
                                            LCDDO jg, List<BiomassData> bList,
                                            string currReg, string currFor, string currDist)
        {
            int currFIA = 0;
            //  load biomass list
            float[] crownFractionWGT = new float[5];
            float topwoodWGT = 0;
            float cullLogWGT = 0;
            float cullChunkWGT = 0;

            //  have to convert acres to float because of biomass function calls -- they only take float values
            float floatAcres = (float)currAcres;
            // find FIA for current group
            int nthRow = bList.FindIndex(
                delegate(BiomassData b)
                {
                    return b.userStratum == jg.Stratum && 
                            b.userSG == jg.SampleGroup && b.userSpecies == jg.Species;
                });
            if (nthRow >= 0)
            {
                currFIA = bList[nthRow].bioSpecies;
                if (currFIA == 0) return -1;
                //  need percent removed to calculate fraction left in the woods
                //  Per K.Cormier, FLIW = 1.0 - percent removed
                //  August 2013
                List<BiomassEquationDO> beList = bslyr.getBiomassEquations();
                int mthRow = beList.FindIndex(
                    delegate(BiomassEquationDO be)
                    {
                        return be.Species == jg.Species && be.FIAcode == currFIA;
                    });
                float currFLIW = 0;
                if (mthRow >= 0)
                {
                    currFLIW = (float)1.0 - (beList[mthRow].PercentRemoved / 100);
                    bList[nthRow].FLIW = currFLIW;
                }   //  endif
                
                //  crown section and damaged small trees
                foreach (TreeCalculatedValuesDO cd in currentData)
                {
                    float currDBH = cd.Tree.DBH;
                    float currHGT = 0;
                    float CR = 0;
                    if (currReg == "9" || currReg == "09")
                    {
                        currHGT = cd.Tree.TotalHeight;
                        BROWNCROWNFRACTION(ref currFIA, ref currDBH, ref currHGT, ref CR, crownFractionWGT);
                    }
                    else
                    {
                        currHGT = cd.Tree.MerchHeightPrimary;
                        BROWNCROWNFRACTION(ref currFIA, ref currDBH, ref currHGT, ref CR, crownFractionWGT);
                    }       //  endif
                    if (cd.Tree.DBH > 6)
                    {
                        //  load crown section
                        bList[nthRow].needles += crownFractionWGT[0];
                        bList[nthRow].quarterInch += crownFractionWGT[1];
                        bList[nthRow].oneInch += crownFractionWGT[2];
                        bList[nthRow].threeInch += crownFractionWGT[3];
                        bList[nthRow].threePlus += crownFractionWGT[4];
                    }
                    else if(cd.Tree.DBH <= 6)
                    {
                        //  load into damaged small trees
                        bList[nthRow].DSTneedles += crownFractionWGT[0];
                        bList[nthRow].DSTquarterInch += crownFractionWGT[1];
                        bList[nthRow].DSToneInch += crownFractionWGT[2];
                        bList[nthRow].DSTthreeInch += crownFractionWGT[3];
                        bList[nthRow].DSTthreePlus += crownFractionWGT[4];
                    }   //  endif on DBH

                    //  Sum up values for three-inch plus section
                    float grsVol = 0;
                    float netVol = 0;
                    //  Topwood weight
                    grsVol = currentData.Sum(c => c.GrossCUFTSP * c.Tree.ExpansionFactor);
                    grsVol = grsVol * floatAcres;
                    BROWNTOPWOOD(ref currFIA, ref grsVol, ref topwoodWGT);
                    bList[nthRow].topwoodDryWeight = topwoodWGT;

                    //  Cull chunk weight
                    grsVol = cd.GrossCUFTPP * cd.Tree.ExpansionFactor * floatAcres;
                    netVol = cd.NetCUFTPP * cd.Tree.ExpansionFactor * floatAcres;
                    BROWNCULLCHUNK(ref currFIA,  ref grsVol, ref netVol, ref currFLIW, ref cullChunkWGT); 
                        bList[nthRow].cullChunkWgt += cullChunkWGT;   


                    //  Pull grade 9 logs for current group
                    List<LogStockDO> justCullLogs = bslyr.getCullLogs((long)cd.Tree_CN, "9");
                    foreach (LogStockDO jcl in justCullLogs)
                    {
                        grsVol = jcl.GrossCubicFoot;
                        BROWNCULLLOG(ref currFIA,  ref grsVol, ref cullLogWGT);
                        bList[nthRow].cullLogWgt = cullLogWGT * jcl.Tree.ExpansionFactor * floatAcres;
                    }   //  end foreach loop
                }   //  end foreach loop
            }   //  endif nthRow
            return 1;
        }   //  end CalculateComponentValues


        private List<BiomassData> SumUpUnitList(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, 
                                        List<BiomassData> bList, string currCU, float unitAcres)
        {
            //  WT3 report
            //  Sum up strata values for current unit
            string currST = "**";
            double currProFac = 0;
            double currSTacres = 1;
            string currMeth = "";
            int jthRow;
            List<StratumDO> sList = bslyr.getStratum();
            List<PRODO> proList = bslyr.getPRO();
            List<BiomassData> unitList = new List<BiomassData>();

            foreach (BiomassData b in bList)
            {
                if (currST != b.userStratum)
                {
                    //  need stratum acres, method, and proration factor
                    jthRow = sList.FindIndex(
                        delegate(StratumDO s)
                        {
                            return s.Code == b.userStratum;
                        });
                    currSTacres = Utilities.AcresLookup((long)sList[jthRow].Stratum_CN, bslyr, b.userStratum);
                    currMeth = sList[jthRow].Method;
                    switch (currMeth)
                    {
                        case "3P":
                        case "STR":
                        case "S3P":
                            jthRow = proList.FindIndex(
                            delegate(PRODO p)
                            {
                                return p.CutLeave == "C" && p.Stratum == b.userStratum &&
                                    p.CuttingUnit == currCU && p.SampleGroup == b.userSG;
                            });
                            if (jthRow >= 0)
                                currProFac = proList[jthRow].ProrationFactor;
                            break;
                        default:
                            currProFac = 1.0;
                            break;
                    }   //  end switch on method for proration factor
                    currST = b.userStratum;
                }   //  endif

                //  stratum must match so check for species group in new list and add if not there
                //  or update the row found
                int ithRow = unitList.FindIndex(
                    delegate(BiomassData ul)
                    {
                        return ul.userStratum == b.userStratum && ul.userSG == b.userSG && 
                                ul.userSpecies == b.userSpecies;
                    });
                if (ithRow < 0)
                {
                    //  Add to list
                    BiomassData bd = new BiomassData();
                    bd.userStratum = b.userStratum;
                    bd.userSG = b.userSG;
                    bd.userSpecies = b.userSpecies;
                    unitList.Add(bd);
                    ithRow = unitList.Count - 1;
                }   //  endif ithRow

                //  update values
                switch (currMeth)
                {
                    case "3P":
                    case "STR":
                    case "S3P":
                        unitList[ithRow].needles += (b.needles * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].quarterInch += (b.quarterInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].oneInch += (b.oneInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].threeInch += (b.threeInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].threePlus += (b.threePlus * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].topwoodDryWeight += (b.topwoodDryWeight * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].cullLogWgt += (b.cullLogWgt * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].cullChunkWgt += (b.cullChunkWgt * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].DSTneedles += (b.DSTneedles * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].DSTquarterInch += (b.DSTquarterInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].DSToneInch += (b.DSToneInch* currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].DSTthreeInch += (b.DSTthreeInch * currSTacres * currProFac) / unitAcres;
                        unitList[ithRow].DSTthreePlus += (b.threePlus * currSTacres * currProFac) / unitAcres;
                        break;
                    default:
                        unitList[ithRow].needles += b.needles * currProFac;
                        unitList[ithRow].quarterInch += b.quarterInch * currProFac;
                        unitList[ithRow].oneInch += b.oneInch * currProFac;
                        unitList[ithRow].threeInch += b.threeInch * currProFac;
                        unitList[ithRow].threePlus += b.threePlus * currProFac;
                        unitList[ithRow].topwoodDryWeight += b.topwoodDryWeight * currProFac;
                        unitList[ithRow].cullLogWgt += b.cullLogWgt * currProFac;
                        unitList[ithRow].cullChunkWgt += b.cullChunkWgt * currProFac;
                        unitList[ithRow].DSTneedles += b.DSTneedles * currProFac;
                        unitList[ithRow].DSTquarterInch += b.DSTquarterInch * currProFac;
                        unitList[ithRow].DSToneInch += b.DSToneInch * currProFac;
                        unitList[ithRow].DSTthreeInch += b.DSTthreeInch * currProFac;
                        unitList[ithRow].DSTthreePlus += b.threePlus * currProFac;
                        break;
                }   //  end switch on method
            }   //  end foreach loop

            return unitList;
        }   //  end SumUpUnitList


        private void WriteCurrentGroup(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb,
                                        string currST, List<BiomassData> bList, string currCU)
        {
            //  output WT2/WT3
            double lineTotal = 0;
            //  finish headers
            string[] completeHeader = new string[5];
            switch (currentReport)
            {
                case "WT2":
                    completeHeader = completeHeaderColumns(currST, bList, rh);
                    break;
                case "WT3":
                    completeHeader = completeHeaderColumns(currCU, bList, rh);
                    break;
            }   //  end switch on current report
            prtFields.Clear();
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                completeHeader, 13, ref pageNumb, "");
            //  crown section
            for (int n = 0; n < 5; n++)
            {
                prtFields.Add("         ");
                prtFields.Add(rh.WT2crown[n]);
                prtFields.Add("     ");
                lineTotal = LoadLine(n+1, bList);
                //  total column
                prtFields.Add(Utilities.Format("{0,6:F2}", lineTotal / 2000).ToString().PadLeft(6, ' '));
                printOneRecord(strWriteOut, prtFields);
                lineTotal = 0;
                prtFields.Clear();
            }   //  end for n loop

            //  three inch plus section -- topwood dry weight
            strWriteOut.WriteLine("                    |");
            strWriteOut.WriteLine("3\"+                 |");
            strWriteOut.WriteLine("____________________|");
            for (int n = 0; n < 4; n++)
            {
                prtFields.Clear();
                prtFields.Add("        ");
                prtFields.Add(rh.WT2threeplus[n]);
                prtFields.Add("     ");
                lineTotal = LoadLine(n+6, bList);
                if (n != 3)
                {
                    //  no line total for FLIW
                    prtFields.Add(Utilities.Format("{0,6:F2}", lineTotal / 2000).ToString().PadLeft(6, ' '));
                    printOneRecord(strWriteOut, prtFields);
                }
                else printOneRecord(strWriteOut,prtFields);
                lineTotal = 0;
            }   //  end for n loop


            //  damaged small trees
            strWriteOut.WriteLine("                    |");
            strWriteOut.WriteLine("DAMAGED SMALL TREES |");
            strWriteOut.WriteLine("____________________|");
            for (int n = 0; n < 5; n++)
            {
                prtFields.Clear();
                prtFields.Add("         ");
                prtFields.Add(rh.WT2crown[n]);
                prtFields.Add("     ");
                lineTotal = LoadLine(n+10, bList);
                prtFields.Add(Utilities.Format("{0,6:F2}", lineTotal / 2000).ToString().PadLeft(6, ' '));
                printOneRecord(strWriteOut, prtFields);
                lineTotal = 0;
            }   //  end for n loop

            strWriteOut.WriteLine("                    |");
            strWriteOut.WriteLine(reportConstants.longLine);
            //  totals section here
            double overallTotal = 0;
            strWriteOut.WriteLine("                    |");
            strWriteOut.Write("                    |");
            //  need to loop through list to get blanks for number of species
            foreach(BiomassData b in bList)
                strWriteOut.Write("          ");
            if (currST != "SALE" && currST != "")
            {
                strWriteOut.WriteLine("  STRATUM TOTAL/AC");
                strWriteOut.Write("TOTAL               |");
            }
            else if (currST == "SALE")
            {
                strWriteOut.WriteLine("  SALE TOTAL/AC");
                strWriteOut.Write("TOTAL               |");                
            }   //  endif
            if (currCU != "")
            {
                strWriteOut.WriteLine("  UNIT TOTAL/AC");
                strWriteOut.Write("TOTAL               |");
            }   //  endif
            foreach (BiomassData b in bList)
                strWriteOut.Write("          ");
            strWriteOut.WriteLine("  ALL SPECIES");
            prtFields.Clear();
            prtFields.Add("OVEN DRY TONS/AC    |");
            prtFields.Add("     ");
            foreach (BiomassData b in bList)
            {
                double sumValue = 0;
                sumValue += b.needles;
                sumValue += b.quarterInch;
                sumValue += b.threeInch;
                sumValue += b.threePlus;
                sumValue += b.topwoodDryWeight;
                sumValue += b.cullLogWgt;
                sumValue += b.cullChunkWgt;
                sumValue += b.DSTneedles;
                sumValue += b.DSToneInch;
                sumValue += b.DSTquarterInch;
                sumValue += b.DSTthreeInch;
                sumValue += b.DSTthreePlus;
                prtFields.Add(Utilities.Format("{0,6:F2}", sumValue / 2000).ToString().PadLeft(6, ' '));
                prtFields.Add("    ");
                overallTotal += sumValue;
            }   //  end foreach loop
            prtFields.Add(Utilities.Format("{0,6:F2}", overallTotal / 2000).ToString().PadLeft(6, ' '));
            printOneRecord(strWriteOut, prtFields);

            //  print footer
            strWriteOut.WriteLine(" ");
            strWriteOut.WriteLine("   FLIW = Fraction Left In Woods");
            return;
        }   //  end WriteCurrentGroup


        private double LoadLine(int whichComponent, List<BiomassData> bList)
        {
            double totalLine = 0;
            switch(whichComponent)
            {
                case 1:     //  crown needles
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.needles / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.needles;
                    }   //  end foreach loop
                    break;
                case 2:     //  crown quarter inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.quarterInch / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.quarterInch;
                    }   //  end foreach loop
                    break;
                case 3:     //  crown one inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.oneInch / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.oneInch;
                    }   //  end foreach loop
                    break;
                case 4:     //  crown three inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.threeInch / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.threeInch;
                    }   //  end foreach loop
                    break;
                case 5:     //  crown three inch plus
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.threePlus / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.threePlus;
                    }   //  end foreach loop
                    break;
                case 6:     //  topwood
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.topwoodDryWeight / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.topwoodDryWeight;
                    }   //  end foreach loop
                    break;
                case 7:     //  cull volume
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.cullLogWgt / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.cullLogWgt;
                    }   //  end foreach loop
                    break;
                case 8:     //  cull chunk weight
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.cullChunkWgt / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.cullChunkWgt;
                    }   //  end foreach loop
                    break;
                case 9:     //  FLIW -- has no total column
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.FLIW).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine = 0;
                    }   //  end foreach loop
                    break;
                case 10:    //  dam small trees needles
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.DSTneedles / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.DSTneedles;
                    }   //  end foreach loop
                    break;
                case 11:        //  dam small trees quarter inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.DSTquarterInch / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.DSTquarterInch;
                    }   //  end foreach loop
                    break;
                case 12:        //  dam small trees one inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.DSToneInch / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.DSToneInch;
                    }   //  end foreach loop
                    break;
                case 13:        //  dam small trees three inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.DSTthreeInch / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.DSTthreeInch;
                    }   //  end foreach loop
                    break;
                case 14:        //  dam small trees three inch plus
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(Utilities.Format("{0,6:F2}", b.DSTthreePlus / 2000).ToString().PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.DSTthreePlus;
                    }   //  end foreach loop
                    break;
            }   //  end switch
            return totalLine;
        }   //  end LoadLine


        private string[] completeHeaderColumns(string currType, List<BiomassData> bList, reportHeaders rh)
        {
            string[] finnishHeader = new string[5];
            switch (currentReport)
            {
                case "WT2":
                    if (currType != "SALE")
                        finnishHeader[0] = rh.WT2columns[0].Replace("XXXX", currType.PadLeft(2, ' '));
                    else if (currType == "SALE")
                        finnishHeader[0] = "OVERALL SALE SUMMARY";
                    break;
                case "WT3":
                    finnishHeader[0] = rh.WT2columns[2].Replace("XXXX", currType.PadLeft(4, ' '));
                    break;
            }   //  end switch

            //  second line
            finnishHeader[1] = rh.WT2columns[1];
            finnishHeader[2] = reportConstants.longLine;
            //  load species columns
            finnishHeader[3] = "CROWNS              |";
            foreach(BiomassData b in bList)
            {
                finnishHeader[3] += "    ";
                finnishHeader[3] += b.userSpecies.PadLeft(6,' ');
            }   //  end foreach loop

            finnishHeader[3] += "     TOTAL";
            finnishHeader[4] = "____________________|_______________________________________________________________________________________________________________";

            return finnishHeader;
        }   //  end completeHeaderColumns


        private void LoadSummaryGroups(List<BiomassData> summaryList, List<TreeDefaultValueDO> tdList)
        {
            //  Pull species groups from LCD to load into summary list
            List<LCDDO> summaryGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 ", "GROUP BY Species", "C", "");
            foreach (LCDDO sg in summaryGroups)
            {
                //  find FIA code
                int nthRow = tdList.FindIndex(
                    delegate(TreeDefaultValueDO t)
                    {
                        return t.Species == sg.Species;
                    });
                BiomassData b = new BiomassData();
                b.userStratum = sg.Stratum;
                b.userSG = sg.SampleGroup;
                b.userSpecies = sg.Species;
                b.bioSpecies = (int)tdList[nthRow].FIAcode;
                summaryList.Add(b);
            }   //  end foreach loop
            return;
        }   //  end LoadSummaryGroups


        private void UpdateSummaryList(List<BiomassData> summaryList, List<BiomassData> bList)
        {
            //  update summary list with current stratum/cutting unit list
            foreach (BiomassData b in bList)
            {
                //  find each group in the summary list to update
                int ithRow = summaryList.FindIndex(
                    delegate(BiomassData sl)
                    {
                        return sl.userSpecies == b.userSpecies && sl.userSG == b.userSG &&
                            sl.bioSpecies == b.bioSpecies;
                    });
                if (ithRow >= 0)
                {
                    summaryList[ithRow].needles += b.needles;
                    summaryList[ithRow].quarterInch += b.quarterInch;
                    summaryList[ithRow].oneInch += b.oneInch;
                    summaryList[ithRow].threeInch += b.threeInch;
                    summaryList[ithRow].threePlus += b.threePlus;
                    summaryList[ithRow].topwoodDryWeight += b.topwoodDryWeight;
                    summaryList[ithRow].cullLogWgt += b.cullLogWgt;
                    summaryList[ithRow].cullChunkWgt += b.cullChunkWgt;
                    summaryList[ithRow].DSTneedles += b.DSTneedles;
                    summaryList[ithRow].DSTquarterInch += b.DSTquarterInch;
                    summaryList[ithRow].DSToneInch += b.DSToneInch;
                    summaryList[ithRow].DSTthreeInch += b.DSTthreeInch;
                    summaryList[ithRow].DSTthreePlus += b.DSTthreePlus;
                }   //  endif ithRow
            }   //  end foreach loop

            return;
        }   //  end UpdateSummaryList


    }   //  end class OutputWeight
}
