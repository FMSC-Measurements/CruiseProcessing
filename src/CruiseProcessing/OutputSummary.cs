using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    class OutputSummary : CreateTextFile
    {
        public string currentReport;
        public string currCL;
        private double totalStrataAcres;
        private double totalPerAcres;
        private double valueGrandTotal;
        private double wgtGrandTotal;
        private double totCubGrandTotal;
        private string currMethod;
        private int[] fieldLengths;
        private List<string> prtFields;
        private string[] completeHeader = new string[14];
        private List<LCDDO> lcdList = new List<LCDDO>();
        private List<StratumDO> sList = new List<StratumDO>();
        private int hgtOne = 0;
        private int hgtTwo = 0;
        private int[] sourceFlag = new int[3];
        private List<ReportSubtotal> rptSubtotal = new List<ReportSubtotal>();

        public OutputSummary(CPbusinessLayer dataLayer) : base(dataLayer)
        {
        }

        public void OutputSummaryReports(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            string currentTitle = fillReportTitle(currentReport);

            numOlines = 0;
            List<LCDDO> currentGroup = new List<LCDDO>();
            //  pull stratum table to process reports
            sList = DataLayer.getStratum();
            //  Need tree list to finish height headers
            List<TreeDO> tList = DataLayer.getTrees();
            //  check for data before continuing
            List<LCDDO> lcdList = DataLayer.getLCD();
            int nthRow = lcdList.FindIndex(
                delegate(LCDDO l)
                {
                    return l.CutLeave == currCL;
                });
            if (nthRow < 0)
            {
                noDataForReport(strWriteOut, currentReport, " >>>> No data for report ");
                return;
            }   //  endif

            foreach (StratumDO s in sList)
            {
                totalStrataAcres = Utilities.ReturnCorrectAcres(s.Code, DataLayer, (long) s.Stratum_CN);
                currMethod = s.Method;
                //  pull groups to process by report
                switch (currentReport)
                {
                    case "VSM1":      //  low level volume summary (B1)
                    case "VPA1":      //  (B2)
                    case "VAL1":      //  (B3)
                    case "VSM6":
                        currentGroup = DataLayer.GetLCDgroup(s.Code, 1, currCL);
                        //  override total strata acres for certain reports
                        if (currentReport == "VPA1")
                        {
                            totalStrataAcres = 1.0;
                            if(s.Method == "100" || s.Method == "STR" || s.Method == "3P" || s.Method == "S3P")
                                totalStrataAcres = Utilities.AcresLookup((long)s.Stratum_CN, DataLayer, s.Code);
                        }   //  endif
                        if (currentReport == "VAL1")
                        {
                            totalPerAcres = 1.0;
                            if (s.Method == "100" || s.Method == "STR" || s.Method == "3P" || s.Method == "S3P")
                                totalPerAcres = Utilities.AcresLookup((long)s.Stratum_CN, DataLayer, s.Code);
                        }   //  endif
                        break;
                    case "VSM2":      //  Sample group volume summary (CP1)
                    case "VPA2":        //  (CP2)
                    case "VAL2":        //  (CP3)
                    case "LV01":         //  Leave trees only
                        currentGroup = DataLayer.GetLCDgroup(s.Code, 2, currCL);
                        //  override total strata acres for certain reports
                        if (currentReport == "VPA2")
                        {
                            totalStrataAcres = 1.0;
                            if (s.Method == "100" || s.Method == "STR" || s.Method == "3P" || s.Method == "S3P")
                                totalStrataAcres = Utilities.AcresLookup((long)s.Stratum_CN, DataLayer, s.Code);
                        }   //  endif
                        if (currentReport == "VAL2")
                        {
                            totalPerAcres = 1.0;
                            if (s.Method == "100" || s.Method == "STR" || s.Method == "3P" || s.Method == "S3P")
                                totalPerAcres = Utilities.AcresLookup((long)s.Stratum_CN, DataLayer, s.Code);
                        }   //  endif
                        break;
                    case "VSM3":      //  Stratum volume summary (CS1)
                    case "VPA3":        //  (CS2)
                    case "VAL3":        //  (CS3)
                    case "LV02":         //  Leave trees only
                        currentGroup = DataLayer.GetLCDgroup(s.Code, 3, currCL);
                        if (currentGroup.Count == 0 && currentReport != "LV02")
                        {
                            //  changed slightly since this refers to stratum and not the report
                            StringBuilder msg = new StringBuilder();
                            msg.Append("The current report ");
                            msg.Append(currentReport);
                            msg.Append(" is not complete.  Stratum ");
                            msg.Append(s.Code);
                            msg.Append(" has no data,");
                            //strWriteOut.WriteLine("\f"); 
                            strWriteOut.WriteLine(msg.ToString());
                            numOlines++;
                        }
                        //  override total strata acres for certain reports
                        if (currentReport == "VPA3")
                        {
                            totalStrataAcres = 1.0;
                            if (s.Method == "100" || s.Method == "STR" || s.Method == "3P" || s.Method == "S3P")
                                totalStrataAcres = Utilities.AcresLookup((long)s.Stratum_CN, DataLayer, s.Code);
                        }   //  endif
                        if (currentReport == "VAL3")
                        {
                            totalPerAcres = 1.0;
                            if (s.Method == "100" || s.Method == "STR" || s.Method == "3P" || s.Method == "S3P")
                                totalPerAcres = Utilities.AcresLookup((long)s.Stratum_CN, DataLayer, s.Code);
                        }   //  endif
                        break;
                }   //  end switch

                //  then pull data for groups by report
                List<LCDDO> currentData = new List<LCDDO>();
                switch (currentReport)
                {
                    case "VSM1":        // low level volume summary (B1)
                    case "VPA1":        //  volume per acre summary (B2)
                    case "VAL1":     //  value, weight and total cubic summary (B3)
                    case "VSM6":
                        //  Create report title
                        rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCLT, "");
                        if (currentReport == "VSM1")
                        {
                            finishColumnHeaders(rh.LowLevelColumns, rh.SummaryColumns, tList);
                            fieldLengths = new int[] { 1, 3, 7, 3, 2, 3, 2, 2, 3, 3, 2, 2, 5, 6, 6, 5, 6, 6, 4, 4, 6, 6, 7, 9, 8, 9, 8, 7, 9 };
                        }
                        else if (currentReport == "VSM6")
                        {
                            finishColumnHeaders(rh.VSM6leftSide, rh.VSM6columns);
                            fieldLengths = new int[] { 1, 3, 6, 4, 2, 3, 2, 2, 3, 3, 2, 2, 6, 8, 12, 14, 10, 10 };
                        }
                        else if (currentReport == "VPA1")
                        {
                            finishColumnHeaders(rh.VPA1VAL1columns, rh.StrataSummaryColumns);
                            fieldLengths = new int[] { 1, 3, 7, 3, 2, 3, 2, 2, 3, 3, 2, 2, 5, 12, 12, 15, 11, 13, 10, 9 };
                        }
                        else if (currentReport == "VAL1")
                        {
                            finishColumnHeaders(rh.VPA1VAL1columns, rh.ValueWeightColumns);
                            fieldLengths = new int[] { 1, 3, 7, 3, 2, 3, 2, 2, 3, 3, 2, 2, 6, 12, 15, 12, 15, 12, 10, 9 };
                        }   //  endif currentReport

                        foreach (LCDDO lcd in currentGroup)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("WHERE Stratum = @p1 AND Species = @p2 AND PrimaryProduct = @p3 AND UOM = @p4 AND ");
                            sb.Append("LiveDead = @p5 AND CutLeave = @p6 AND Yield = @p7 AND SampleGroup = @p8 AND ");
                            //  September 2016 -- per K.Cormier -- dropping contract species from LCD identifier
                            //sb.Append("TreeGrade = ? AND ContractSpecies = ? AND STM = ?");
                            sb.Append("TreeGrade = @p9 AND STM = @p10");
                            currentData = DataLayer.GetLCDdata(sb.ToString(), lcd, 1, "");
                            //  need to get sourceFlag
                            FindSourceFlag(currentData);
                            //  write current group
                            WriteCurrentGroup(currentData, lcd, strWriteOut, rh, totalStrataAcres, ref pageNumb, currentReport);
                        }   //  end foreach loop
                        break;
                    case "VSM2":       //  Sample group volume summary (CP1)
                    case "VPA2":       //  Sample group volume per acre summary (CP2)
                    case "VAL2":        //  Sample group value, weight and total cubic summary (CP3)
                    case "LV01":         //  Leave tree report(CP1)
                        //  Create report title
                        if(currCL == "C")
                            rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
                        else if(currCL == "L")
                            rh.createReportTitle(currentTitle,5,0,0,reportConstants.FLTO,"");
                        switch(currentReport)
                        {
                            case "VSM2":        case "LV01":
                                finishColumnHeaders(rh.VSM2columns, rh.SummaryColumns, tList);
                                fieldLengths = new int[] { 1, 3, 3, 2, 3, 4, 2, 6, 6, 6, 6, 6, 5, 4, 4, 6, 6, 7, 9, 8, 9, 8, 7, 9 };
                                break;
                            case "VPA2":
                                finishColumnHeaders(rh.VPA2VAL2columns, rh.StrataSummaryColumns);
                                fieldLengths = new int[] { 3, 4, 5, 4, 5, 5, 12, 13, 11, 15, 11, 12, 10, 9 };
                                break;
                            case "VAL2":
                                finishColumnHeaders(rh.VPA2VAL2columns, rh.ValueWeightColumns);
                                fieldLengths = new int[] { 3, 4, 5, 4, 5, 5, 13, 12, 15, 12, 15, 12, 10, 9 };
                                break;
                        }   //  end switch on currentReport

                            foreach (LCDDO lcd in currentGroup)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("WHERE Stratum = @p1 AND PrimaryProduct = @p2 AND UOM = @p3 AND CutLeave = @p4 ");
                                sb.Append("AND SampleGroup = @p5 AND STM = @p6");
                                currentData = DataLayer.GetLCDdata(sb.ToString(), lcd, 2, currCL);
                                //  set sourceflag
                                FindSourceFlag(currentData);
                                //  write current group
                                WriteCurrentGroup(currentData, lcd, strWriteOut, rh, totalStrataAcres, ref pageNumb, currentReport);
                            }   //  end foreach loop
                        break;
                    case "VSM3":        //  Strata volume summary (CS1)
                    case "VPA3":        //  Strata volume per acre summary (CS2)
                    case "VAL3":        //  Strata value, weight and total cubic summary (CS3)
                    case "LV02":         //  Leave tree report (CS1)

                        //  Create report title
                        if (currCL == "C")
                            rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
                        else if (currCL == "L")
                            rh.createReportTitle(currentTitle, 5, 0, 0, reportConstants.FLTO, "");

                        switch(currentReport)
                        {
                            case "VSM3":        case "LV02":
                                finishColumnHeaders(rh.VSM3columns, rh.SummaryColumns, tList);
                                fieldLengths = new int[] { 1, 3, 3, 3, 3, 6, 9, 6, 5, 6, 6, 4, 4, 6, 6, 7, 9, 8, 9, 8, 7, 9 };
                                break;
                            case "VPA3":
                                finishColumnHeaders(rh.VPA3VAL3columns, rh.StrataSummaryColumns);
                                fieldLengths = new int[] { 3, 6, 5, 5, 6, 12, 12, 14, 11, 13, 10, 9 };
                                break;
                            case "VAL3":
                                finishColumnHeaders(rh.VPA3VAL3columns, rh.ValueWeightColumns);
                                fieldLengths = new int[] { 3, 6, 5, 5, 7, 12, 15, 12, 15, 12, 10, 9 };
                                break;
                        }   //  end switch on currentReport

                            foreach (LCDDO lcd in currentGroup)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("WHERE Stratum = @p1 AND PrimaryProduct = @p2 AND UOM = @p3 AND CutLeave = @p4");
                                currentData = DataLayer.GetLCDdata(sb.ToString(), lcd, 3, currCL);
                                //  set sourceflag
                                FindSourceFlag(currentData);
                                //  write current group
                                WriteCurrentGroup(currentData, lcd, strWriteOut, rh, totalStrataAcres, ref pageNumb, currentReport);
                            }   //  end foreach loop
                        break;        
                }   //  end switch

                //  write long line before next stratum
                if (currentGroup.Count > 0)
                {
                    strWriteOut.WriteLine(reportConstants.longLine);
                    numOlines++;
                }   //  endif
                //  clear height flags for next stratum
                hgtOne = 0;
                hgtTwo = 0;
            }   //  end foreach stratum loop

            //  start a new page if needed before subtotal
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], completeHeader,
                                completeHeader.Count(), ref pageNumb, "");
            if (currentReport == "VSM1" || currentReport == "VSM2" || currentReport == "VSM3" ||
                currentReport == "LV01" || currentReport == "LV02")
            {
                //  need to reset height flags to get proper values printed
                whichHeightFields(ref hgtOne, ref hgtTwo, tList);
                //  print subtotals here
                PrintSubtotals(currentReport, strWriteOut, rh, ref pageNumb);
            }   //  endif currentReport
            if (currentReport == "VAL1" || currentReport == "VAL2" || currentReport == "VAL3")
                PrintSubtotals(strWriteOut);
            if (currentReport == "VSM6") PrintTotals(strWriteOut, ref pageNumb);

            //  output footer remark
            if(numOlines >= 50)
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], completeHeader,
                    completeHeader.Count(), ref pageNumb, "");
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine("PRODUCT SOURCE KEY: \n");
            strWriteOut.WriteLine("P = PRIMARY PRODUCT");
            strWriteOut.WriteLine("S = SECONDARY PRODUCT");
            strWriteOut.WriteLine("R = RECOVERABLE PRODUCT\n");
            strWriteOut.WriteLine("The following items are not reported and/or calculated for secondary or recovered");
            strWriteOut.WriteLine("products but are shown here as blank so the associated volumes can be reported:");
            strWriteOut.WriteLine("Number of measured trees, Quad mean DBH, Mean DBH, Mean height(s), Average defect,");
            strWriteOut.WriteLine("any Ratio, Estimated number of trees.");

            return;
        }   //  end OutputSummaryReports


        private void WriteCurrentGroup(List<LCDDO> currData, LCDDO currGrp, StreamWriter strWriteOut, reportHeaders rh,
                                        double STacres, ref int pageNumb, string currRpt)
        {
            //  Outputs current group by product
            for (int k = 0; k < 3; k++)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    completeHeader, 23, ref pageNumb, "");
                //  load left side info
                prtFields = new List<string>();
                LCDmethods.LoadLCDID(ref prtFields, currGrp, sourceFlag[k], currRpt);
                switch (currRpt)
                {
                    case "VSM1":        case "VSM2":        case "VSM3":
                    case "LV01":        case "LV02":        
                        //  Load dbh and height means and ratios
                        LCDmethods.LoadLCDmeans(ref prtFields, currData, hgtOne, hgtTwo, sourceFlag[k], currRpt,
                                                STacres, currMethod); 
                        LCDmethods.LoadLCDvolumes(STacres, ref prtFields, currData, sourceFlag[k], 0);
                        break;
                    case "VSM6":
                        //  Load CUFT and tons
                        LCDmethods.LoadLCDweight(STacres, ref prtFields, currData, sourceFlag[k], 0);
                        break;
                    case "VPA1":        case "VPA2":        case "VPA3":
                        //  load expansion factor
                        if (sourceFlag[k] == 1)
                        {
                            if (currMethod == "3P" || currMethod == "S3P")
                            {
                                if (currData.Sum(ld => ld.TalliedTrees) / STacres < 1)
                                    prtFields.Add("       <1");
                                else prtFields.Add(String.Format("{0,9:F0}", currData.Sum(ld => ld.TalliedTrees) / STacres));
                            }
                            else
                            {
                                if (currData.Sum(ld => ld.SumExpanFactor) / STacres < 1)
                                    prtFields.Add("       <1");
                                else prtFields.Add(String.Format("{0,9:F0}", currData.Sum(ld => ld.SumExpanFactor) / STacres));
                            }   //  endif specific method
                        }
                        else prtFields.Add("         ");
                        //  Load volumes
                        LCDmethods.LoadLCDvolumes(STacres, ref prtFields, currData, sourceFlag[k], 1);
                        break;
                    case "VAL1":        case "VAL2":        case "VAL3":
                        //  load value, weight and total cubic and capture totals for the end
                        LCDmethods.LoadLCDvalue(totalStrataAcres, totalPerAcres, ref prtFields, currData, sourceFlag[k],
                                                ref valueGrandTotal, ref wgtGrandTotal, ref totCubGrandTotal);
                        break;
                }   //  end switch
                //  print the group
                if (sourceFlag[k] != 0)
                {
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                        completeHeader, completeHeader.Count(), ref pageNumb, "");
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                    if (currRpt == "VSM1" || currRpt == "VSM2" || currRpt == "VSM3" ||
                        currRpt == "LV01" || currRpt == "LV02")
                    {
                        //  update subtotals here
                        UpdateSubtotals(currData, currGrp, STacres, sourceFlag[k]);
                    }
                    else if (currRpt == "VSM6")
                    {
                        UpdateTotals(currData, STacres, sourceFlag[k]);
                    }   //  endif current report
                }   //  endif sourceFlag not zero
            }   //  end foreach loop
            return;
        }   //  end WriteCurrentGroup


        private void finishColumnHeaders(string[] lowLevelColumns, string[] summaryColumns, List<TreeDO> tList)
        {
            //  load up complete header
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < summaryColumns.Count();k++)
            {
                sb.Clear();
                sb.Append(lowLevelColumns[k]);
                sb.Append(summaryColumns[k]);
                completeHeader[k] = sb.ToString();
            }   //  end for loop
            //  determine which heights are used in the report
            whichHeightFields(ref hgtOne, ref hgtTwo, tList);
            //  update summary columns with height(s)
            completeHeader = updateHeightHeader(hgtOne, hgtTwo, "MEAN", completeHeader);
            return;
        }   //  end finishColumnHeaders

        private void finishColumnHeaders(string[] leftHandSide, string[] rightHandSide)
        {
            //  overloaded for second and third reports in series
            //  load up complete header
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < leftHandSide.Count(); k++)
            {
                sb.Clear();
                sb.Append(leftHandSide[k]);
                sb.Append(rightHandSide[k]);
                completeHeader[k] = sb.ToString();
            }   //  end for loop
            return;
        }   //  end finishColumnHeaders

        private void FindSourceFlag(List<LCDDO> currData)
        {
            sourceFlag[0] = 0;
            sourceFlag[1] = 0;
            sourceFlag[2] = 0;
            double currEF = currData.Sum(l => l.SumExpanFactor);
            if (currEF > 0 || currData.Sum(l => l.SumGBDFT) > 0 ||
                currData.Sum(l => l.SumGCUFT) > 0 ||
                currData.Sum(l => l.SumCords) > 0)
                sourceFlag[0] = 1;
            if (currData.Sum(l => l.SumGBDFTtop) > 0 || currData.Sum(l => l.SumGCUFTtop) > 0 ||
                currData.Sum(l => l.SumCordsTop) > 0)
                sourceFlag[1] = 2;
            if (currData.Sum(l => l.SumBDFTrecv) > 0 || currData.Sum(l => l.SumCUFTrecv) > 0 ||
                currData.Sum(l => l.SumCordsRecv) > 0)
                sourceFlag[2] = 3;

            return;
        }   //  end FindSourceFlag

        private void UpdateTotals(List<LCDDO> currData, double STacres, int currPP)
        {
            foreach (LCDDO cd in currData)
            {
                switch (currPP)
                {
                    case 1:
                        int nthRow = rptSubtotal.FindIndex(
                        delegate(ReportSubtotal rs)
                        {
                            return rs.Value1 == cd.PrimaryProduct && rs.Value2 == "P";
                        });
                        if (nthRow >= 0)
                        {
                            rptSubtotal[nthRow].Value3 += cd.SumGCUFT * STacres;
                            rptSubtotal[nthRow].Value4 += cd.SumNCUFT * STacres;
                            rptSubtotal[nthRow].Value5 += cd.SumWgtMSP * STacres;
                        }
                        else if (nthRow < 0)
                        {
                            ReportSubtotal rs = new ReportSubtotal();
                            rs.Value1 = cd.PrimaryProduct;
                            rs.Value2 = "P";
                            rs.Value3 += cd.SumGCUFT * STacres;
                            rs.Value4 += cd.SumNCUFT * STacres;
                            rs.Value5 += cd.SumWgtMSP * STacres;
                            rptSubtotal.Add(rs);
                        }   //  endif
                        break;
                    case 2:
                        nthRow = rptSubtotal.FindIndex(
                        delegate(ReportSubtotal rs)
                        {
                            return rs.Value1 == cd.SecondaryProduct && rs.Value2 == "S";
                        });
                        if (nthRow >= 0)
                        {
                            rptSubtotal[nthRow].Value3 += cd.SumGCUFTtop * STacres;
                            rptSubtotal[nthRow].Value4 += cd.SumNCUFTtop * STacres;
                            rptSubtotal[nthRow].Value5 += cd.SumWgtMSS * STacres;
                        }
                        else if (nthRow < 0)
                        {
                            ReportSubtotal rs = new ReportSubtotal();
                            rs.Value1 = cd.SecondaryProduct;
                            rs.Value2 = "S";
                            rs.Value3 += cd.SumGCUFTtop * STacres;
                            rs.Value4 += cd.SumNCUFTtop * STacres;
                            rs.Value5 += cd.SumWgtMSS * STacres;
                            rptSubtotal.Add(rs);
                        }   //  endifp
                        break;
                }   // end switch
            }   //  end foreach loop
                return;
        }   //  UpdateTotals


        private void UpdateSubtotals(List<LCDDO> currData, LCDDO currGRP, double STacres, int currSRC)
        {
            //  find group in subtotal list
            int nthRow = -1;
            foreach (LCDDO ldo in currData)
            {
                if (currSRC == 1)
                {
                    nthRow = rptSubtotal.FindIndex(
                    delegate(ReportSubtotal rs)
                    {
                        return rs.Value1 == currGRP.PrimaryProduct && rs.Value2 == "P";
                    });
                }
                else if(currSRC == 2)
                {
                    nthRow = rptSubtotal.FindIndex(
                    delegate(ReportSubtotal rs)
                    {
                        return rs.Value1 == currGRP.SecondaryProduct && rs.Value2 == "S";
                    });
                }
                else if(currSRC == 3)
                {
                    nthRow = rptSubtotal.FindIndex(
                    delegate(ReportSubtotal rs)
                    {
                        return rs.Value1 == currGRP.SecondaryProduct && rs.Value2 == "R";
                    });
                }

                if (nthRow >= 0)
                {
                    rptSubtotal[nthRow].Value3 += ldo.SumDBHOB * STacres;
                    rptSubtotal[nthRow].Value4 += ldo.SumDBHOBsqrd * STacres;
                    rptSubtotal[nthRow].Value11 += ldo.SumTotHgt * STacres;
                    rptSubtotal[nthRow].Value12 += ldo.SumMerchHgtPrim * STacres;
                    rptSubtotal[nthRow].Value13 += ldo.SumMerchHgtSecond * STacres;
                    rptSubtotal[nthRow].Value14 += ldo.SumHgtUpStem * STacres;
                    if (currSRC == 1)
                    {
                        //  primary
                        //  per K.Cormier, averages for 3P and S3P are not divided by tallied trees
                        //  Value5 is used to print estimated number of trees so it does need tallied trees
                        //  Value16 is used for average calculations so it does get expansion factor
                        //  June 2015
                        if (currMethod == "S3P" || currMethod == "3P")
                            rptSubtotal[nthRow].Value5 += ldo.TalliedTrees;
                        else rptSubtotal[nthRow].Value5 += ldo.SumExpanFactor * STacres;
                        rptSubtotal[nthRow].Value16 += ldo.SumExpanFactor * STacres;
                        rptSubtotal[nthRow].Value6 += ldo.SumGBDFT * STacres;
                        rptSubtotal[nthRow].Value7 += ldo.SumGCUFT * STacres;
                        rptSubtotal[nthRow].Value8 += ldo.SumNBDFT * STacres;
                        rptSubtotal[nthRow].Value9 += ldo.SumNCUFT * STacres;
                        rptSubtotal[nthRow].Value10 += ldo.SumCords * STacres;
                    }
                    else if (currSRC == 2)
                    {
                        // secondary
                        rptSubtotal[nthRow].Value6 += ldo.SumGBDFTtop * STacres;
                        rptSubtotal[nthRow].Value7 += ldo.SumGCUFTtop * STacres;
                        rptSubtotal[nthRow].Value8 += ldo.SumNBDFTtop * STacres;
                        rptSubtotal[nthRow].Value9 += ldo.SumNCUFTtop * STacres;
                        rptSubtotal[nthRow].Value10 += ldo.SumCordsTop * STacres;
                    }
                    else if (currSRC == 3)
                    {
                        //  recoverd
                        rptSubtotal[nthRow].Value8 += ldo.SumBDFTrecv * STacres;
                        rptSubtotal[nthRow].Value9 += ldo.SumCUFTrecv * STacres;
                        rptSubtotal[nthRow].Value10 += ldo.SumCordsRecv * STacres;
                    }   //  endif source
                }
                else if (nthRow < 0)
                {
                    ReportSubtotal rs = new ReportSubtotal();
                    rs.Value3 += ldo.SumDBHOB * STacres;
                    rs.Value4 += ldo.SumDBHOBsqrd * STacres;
                    //  for methods 3P and S3P need to capture expansion factors for mean calculations
                    //  put in different bucket -- May 2014
                    //  June 2015 -- see note above concerning what goes into which bucket
                    if (currMethod == "S3P" || currMethod == "3P")
                        rs.Value5 += ldo.TalliedTrees;
                    else rs.Value5 += ldo.SumExpanFactor * STacres;
                    rs.Value16 += ldo.SumExpanFactor * STacres;
                    rs.Value11 += ldo.SumTotHgt * STacres;
                    rs.Value12 += ldo.SumMerchHgtPrim * STacres;
                    rs.Value13 += ldo.SumMerchHgtSecond * STacres;
                    rs.Value14 += ldo.SumHgtUpStem * STacres;
                    if (currSRC == 1)
                    {
                        //  primary
                        rs.Value1 = currGRP.PrimaryProduct;
                        rs.Value2 = "P";
                        rs.Value6 += ldo.SumGBDFT * STacres;
                        rs.Value7 += ldo.SumGCUFT * STacres;
                        rs.Value8 += ldo.SumNBDFT * STacres;
                        rs.Value9 += ldo.SumNCUFT * STacres;
                        rs.Value10 += ldo.SumCords * STacres;
                    }
                    else if (currSRC == 2)
                    {
                        // secondary
                        rs.Value1 = currGRP.SecondaryProduct;
                        rs.Value2 = "S";
                        rs.Value6 += ldo.SumGBDFTtop * STacres;
                        rs.Value7 += ldo.SumGCUFTtop * STacres;
                        rs.Value8 += ldo.SumNBDFTtop * STacres;
                        rs.Value9 += ldo.SumNCUFTtop * STacres;
                        rs.Value10 += ldo.SumCordsTop * STacres;
                    }
                    else if (currSRC == 3)
                    {
                        //  recovered
                        rs.Value1 = currGRP.SecondaryProduct;
                        rs.Value2 = "R";
                        rs.Value8 += ldo.SumBDFTrecv * STacres;
                        rs.Value9 += ldo.SumCUFTrecv * STacres;
                        rs.Value10 += ldo.SumCordsRecv * STacres;
                    }   //  endif source
                    rptSubtotal.Add(rs);
                }   //  endif nthRow
            }   //  end foreach loop
            return;
        }   //  end UpdateSubtotals

        private void PrintTotals(StreamWriter strWriteOut, ref int pageNum)
        {

            string fieldFormat3 = "{0,7:F0}";
            //  Works for VSM6 report only
            reportHeaders rh = new reportHeaders();

            strWriteOut.WriteLine("                        PRPODUCT                            GROSS          NET");
            strWriteOut.WriteLine(" TOTALS --   PRODUCT    SOURE                               CUFT           CUFT       TONS ");
            strWriteOut.WriteLine("_______________________________________________________________________________________________");
            
            //  pront total lines
            for (int k = 0; k < rptSubtotal.Count; k++)
            {
                strWriteOut.Write("               ");
                strWriteOut.Write(rptSubtotal[k].Value1);
                strWriteOut.Write("        ");
                strWriteOut.Write(rptSubtotal[k].Value2);
                strWriteOut.Write("                         	   ");
                strWriteOut.Write(String.Format(fieldFormat3, rptSubtotal[k].Value3));
                strWriteOut.Write("        ");
                strWriteOut.Write(String.Format(fieldFormat3, rptSubtotal[k].Value4).PadLeft(2,' '))
;                strWriteOut.Write("   ");
                strWriteOut.WriteLine(String.Format(fieldFormat3, rptSubtotal[k].Value5 / 2000));
            }   //  end for k loop
            return;
        }   //  end PrintTotals

        private void PrintSubtotals(string currRPT, StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            string fieldFormat1 = "{0,9:F0}";
            string fieldFormat2 = "{0,9:F1}";
            string fieldFormat3 = "{0,5:F1}";
            string fieldFormat4 = "{0,7:F4}";
            string fieldFormat5 = "{0,6:F0}";
            string fieldFormat6 = "{0,8:F0}";
            string fieldFormat7 = "{0,7:F0}";
            double grand1 = 0;
            double grand2 = 0;
            double grand3 = 0;
            double grand4 = 0;
            double grand5 = 0;
            double grand6 = 0;
            //  used for means and ratio
            double grand7 = 0;      //  DBH squared
            double grand8 = 0;      //  DBH
            double grand9 = 0;      // height one
            double grand10 = 0;     //  height two
            double grand11 = 0;     //  expansion factor for 3P and S3P
            StringBuilder sb = new StringBuilder();

            //  print subtotal and grand total for just first report in each series
            switch (currRPT)
            {
                case "VSM1":
                    //  print subtotal heading
                    if (numOlines > 45 && numOlines <= 50)
                    {
                        numOlines = 50;
                        WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], completeHeader,
                                            completeHeader.Count(), ref pageNumb, "");
                    }   //  endif
                    strWriteOut.WriteLine(" TOTALS ---          PRODUCT     EST. NO                   *** GROSS VOLUME ***          *** NET VOLUME ***");
                    strWriteOut.WriteLine("            PRODUCT  SOURCE      OF TREES                  BDFT            CUFT          BDFT          CUFT           CORDS");
                    strWriteOut.WriteLine("____________________________________________________________________________________________________________________________________");
                    numOlines += 3;
                    fieldLengths = new int[] {14, 10, 10, 20, 16, 14, 14, 16, 10};
                    break;
                case "VSM2":        case "LV01":
                    //  print subtotal heading
                    strWriteOut.WriteLine("  TOTALS ------");
                    strWriteOut.WriteLine("                        QUAD                                         EST.         ************** VOLUME ***************");

                    //  build subtotal lines for mean height values
                    sb.Clear();
                    sb.Append("           PRODUCT      MEAN      MEAN    ");
                    switch (hgtOne)
                    {
                        case 1:     case 2:     case 3:     case 4:
                            sb.Append("MEAN      ");
                            break;
                        default:
                            sb.Append("          ");
                            break;
                    }   //  end switch for first height   
                    switch (hgtTwo)
                    {
                        case 1:     case 2:     case 3:     case 4:
                            sb.Append("MEAN      ");
                            break;
                        default:
                            sb.Append("          ");
                            break;
                    }   //  end switch for any second height
                    sb.Append("       # OF         **** GROSS ****       ***** NET *****");
                    strWriteOut.WriteLine(sb.ToString());
                    sb.Clear();
                    //  second line
                    sb.Append("  PRODUCT  SOURCE       DBH       DBH     ");
                    switch (hgtOne)
                    {
                        case 1:
                            sb.Append("TOT HGT   ");
                            break;
                        case 2:
                            sb.Append("MRHT PRPD ");
                            break;
                        case 3:
                            sb.Append("MRHT SEPD ");
                            break;
                        case 4:
                            sb.Append("HT U STEM ");
                            break;
                        default:
                            sb.Append("          ");
                            break;
                    }   //  end switch
                    switch (hgtTwo)
                    {
                        case 1:
                            sb.Append("TOT HGT   ");
                            break;
                        case 2:
                            sb.Append("MRHT PRPD ");
                            break;
                        case 3:
                            sb.Append("MRHT SEPD ");
                            break;
                        case 4:
                            sb.Append("HT U STEM ");
                            break;
                        default:
                            sb.Append("          ");
                            break;
                    }   //  end switch
                    sb.Append("       TREES        BDFT      CUFT        BDFT       CUFT     CORDS");
                    strWriteOut.WriteLine(sb.ToString());
                    strWriteOut.WriteLine("____________________________________________________________________________________________________________________________________");
                    numOlines += 5;
                    fieldLengths = new int[] { 4, 10, 9, 10, 10, 10, 15, 10, 11, 11, 12, 8, 9 };
                    break;
                case "VSM3":        case "LV02":
                    //  print subtotal heading
                    strWriteOut.WriteLine("  TOTALS ------");
                    strWriteOut.WriteLine("                   QUAD                             GROSS     NET       EST.      ************** VOLUME  ***************");
                    //  build subtotal lines for mean height values
                    sb.Clear();
                    sb.Append("           PRODUCT MEAN  MEAN   ");
                    switch (hgtOne)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            sb.Append("MEAN      ");
                            break;
                        default:
                            sb.Append("          ");
                            break;
                    }   //  end switch for first height   
                    switch (hgtTwo)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            sb.Append("MEAN      ");
                            break;
                        default:
                            sb.Append("          ");
                            break;
                    }   //  end switch for any second height
                    sb.Append("BF/CF     BF/CF     # OF      ***** GROSS *****     *****  NET *****");
                    strWriteOut.WriteLine(sb.ToString());
                    sb.Clear();
                    //  second line
                    sb.Append("  PRODUCT  SOURCE  DBH   DBH   ");
                    switch (hgtOne)
                    {
                        case 1:
                            sb.Append("TOT HGT   ");
                            break;
                        case 2:
                            sb.Append("MRHT PRPD ");
                            break;
                        case 3:
                            sb.Append("MRHT SEPD ");
                            break;
                        case 4:
                            sb.Append("HT U STEM ");
                            break;
                        default:
                            sb.Append("          ");
                            break;
                    }   //  end switch
                    switch (hgtTwo)
                    {
                        case 1:
                            sb.Append("TOT HGT   ");
                            break;
                        case 2:
                            sb.Append("MRHT PRPD ");
                            break;
                        case 3:
                            sb.Append("MRHT SEPD ");
                            break;
                        case 4:
                            sb.Append("HT U STEM ");
                            break;
                        default:
                            sb.Append("          ");
                            break;
                    }   //  end switch
                    sb.Append(" RATIO     RATIO     TREES     BDFT         CUFT     BDFT        CUFT     CORDS");
                    strWriteOut.WriteLine(sb.ToString());
                    strWriteOut.WriteLine("____________________________________________________________________________________________________________________________________");
                    numOlines += 5;
                    fieldLengths = new int[] { 4, 9, 5, 6, 8, 8, 11, 10, 10, 8, 13, 10, 11, 8, 10 };
                    break;
            }   //  end switch

            //  is a new page needed?
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], completeHeader,
                                completeHeader.Count(), ref pageNumb, "");
            prtFields.Clear();
            foreach (ReportSubtotal rs in rptSubtotal)
            {
                prtFields.Add("");
                prtFields.Add(rs.Value1);
                prtFields.Add(rs.Value2);
                if (currRPT == "VSM2" || currRPT == "VSM3" || currRPT == "LV01" || currRPT == "LV02")
                {
                    //  Add means for DBH and heights
                    if (rs.Value2 == "P")
                    {
                        //  According to K.Cormier, 3P and S3P talllied trees are NOT used to calculate means
                        //  june 2015 so this change was made for all mean calculations
                        if (rs.Value16 > 0)      //  means it's a 3P or S3P and use this value to calculate means
                            prtFields.Add(String.Format(fieldFormat3, Math.Sqrt(rs.Value4 / rs.Value16)));
                        //else prtFields.Add(Utilities.FormatField(Math.Sqrt(rs.Value4 / rs.Value5), fieldFormat3).ToString());
                        grand7 += rs.Value4;
                        if(rs.Value16 > 0)      //  same as above
                            prtFields.Add(String.Format(fieldFormat3, rs.Value3 / rs.Value16));
                        //else prtFields.Add(Utilities.FormatField(rs.Value3 / rs.Value5, fieldFormat3).ToString());
                        grand8 += rs.Value3;
                        //  heights
                        switch (hgtOne)
                        {
                            case 1:
                                if(rs.Value16 > 0)
                                    prtFields.Add(String.Format(fieldFormat3, rs.Value11 / rs.Value16));
                                //else prtFields.Add(Utilities.FormatField(rs.Value11 / rs.Value5, fieldFormat3).ToString());
                                grand9 += rs.Value11;
                                break;
                            case 2:
                                if(rs.Value16 > 0)
                                    prtFields.Add(String.Format(fieldFormat3, rs.Value12 / rs.Value16));
                                //else prtFields.Add(Utilities.FormatField(rs.Value12 / rs.Value5, fieldFormat3).ToString());
                                grand9 += rs.Value12;
                                break;
                            case 3:
                                if(rs.Value16 > 0)
                                    prtFields.Add(String.Format(fieldFormat3, rs.Value13 / rs.Value16));
                                //else prtFields.Add(Utilities.FormatField(rs.Value13 / rs.Value5, fieldFormat3).ToString());
                                grand9 += rs.Value13;
                                break;
                            case 4:
                                if(rs.Value16 > 0)
                                    prtFields.Add(String.Format(fieldFormat3, rs.Value14 / rs.Value16));
                                //else prtFields.Add(Utilities.FormatField(rs.Value14 / rs.Value5, fieldFormat3).ToString());
                                grand9 += rs.Value14;
                                break;
                            default:
                                prtFields.Add("     ");
                                break;
                        }   //  end switch on height one

                        switch (hgtTwo)
                        {
                            case 1:
                                if(rs.Value16 > 0)
                                    prtFields.Add(String.Format(fieldFormat3, rs.Value11 / rs.Value16));
                                //else prtFields.Add(Utilities.FormatField(rs.Value11 / rs.Value5, fieldFormat3).ToString());
                                grand10 += rs.Value11;
                                break;
                            case 2:
                                if(rs.Value16 > 0)
                                    prtFields.Add(String.Format(fieldFormat3, rs.Value12 / rs.Value16));
                                //else prtFields.Add(Utilities.FormatField(rs.Value12 / rs.Value5, fieldFormat3).ToString());
                                grand10 += rs.Value12;
                                break;
                            case 3:
                                if(rs.Value16 > 0)
                                    prtFields.Add(String.Format(fieldFormat3, rs.Value13 / rs.Value16));
                                //else prtFields.Add(Utilities.FormatField(rs.Value13 / rs.Value5, fieldFormat3).ToString());
                                grand10 += rs.Value13;
                                break;
                            case 4:
                                if(rs.Value16 > 0)
                                    prtFields.Add(String.Format(fieldFormat3, rs.Value14 / rs.Value16));
                                //else prtFields.Add(Utilities.FormatField(rs.Value14 / rs.Value5, fieldFormat3).ToString());
                                grand10 += rs.Value14;
                                break;
                            default:
                                prtFields.Add("     ");
                                break;
                        }   //  end switch on height two
                    }
                    else
                    {
                        prtFields.Add("     ");
                        prtFields.Add("     ");
                        prtFields.Add("     ");
                        prtFields.Add("     ");
                    }   //  endif primary product
                }   //  endif on current report

                if(currRPT == "VSM3" || currRPT == "LV02")
                {
                    //  add ratios to the line
                    if (rs.Value7 > 0 && rs.Value2 == "P")
                        prtFields.Add(String.Format(fieldFormat4, rs.Value6 / rs.Value7));
                    else if (rs.Value2 == "P")
                        prtFields.Add(" 0.0000");
                    else prtFields.Add("       ");
                    if (rs.Value9 > 0 && rs.Value2 == "P")
                        prtFields.Add(String.Format(fieldFormat4, rs.Value8 / rs.Value9));
                    else if (rs.Value2 == "P")
                        prtFields.Add(" 0.0000");
                    else prtFields.Add("       ");
                }   //  endif

                //  remaining volumes are pretty much the same
                if (rs.Value2 == "P")
                {
                    if(currRPT == "VSM1")
                    {
                        prtFields.Add(String.Format(fieldFormat1, rs.Value5));
                        prtFields.Add(String.Format(fieldFormat1, rs.Value6));
                        prtFields.Add(String.Format(fieldFormat1, rs.Value7));
                    }
                    else
                    {
                        prtFields.Add(String.Format(fieldFormat5, rs.Value5));
                        prtFields.Add(String.Format(fieldFormat6, rs.Value6));
                        prtFields.Add(String.Format(fieldFormat7, rs.Value7));
                    }
                }
                else if (rs.Value2 == "S")
                {
                    prtFields.Add("      ");
                    if(currRPT == "VSM1")
                    {
                        prtFields.Add(String.Format(fieldFormat1, rs.Value6));
                        prtFields.Add(String.Format(fieldFormat1, rs.Value7));
                    }
                    else
                    {
                        prtFields.Add(String.Format(fieldFormat6, rs.Value6));
                        prtFields.Add(String.Format(fieldFormat7, rs.Value7));
                    }   //  endif
                }
                else if (rs.Value2 == "R")
                {
                   prtFields.Add("        ");
                   prtFields.Add("         ");
                   prtFields.Add("         ");
                }   //  endif

                if(currRPT == "VSM1")
                {
                   prtFields.Add(String.Format(fieldFormat1, rs.Value8));
                   prtFields.Add(String.Format(fieldFormat1, rs.Value9));
                }
                else
                {
                    prtFields.Add(String.Format(fieldFormat6, rs.Value8));
                    prtFields.Add(String.Format(fieldFormat7, rs.Value9));
                }   //  endif

                //   Cords
                prtFields.Add(String.Format(fieldFormat2, rs.Value10));

                //  print subtotal line
                printOneRecord(fieldLengths, prtFields, strWriteOut);
                //  update grand total
                //  there is no estimated number of trees for recovered product.
                //if(rs.Value2 == "P" || rs.Value2 == "S") grand1 += rs.Value5;
                if (rs.Value2 == "P") grand1 += rs.Value5;
                if (rs.Value16 > 0 && rs.Value2 == "P") grand11 += rs.Value16;  //  for mean calculations when method is 3P or S3P
                grand2 += rs.Value6;
                grand3 += rs.Value7;
                grand4 += rs.Value8;
                grand5 += rs.Value9;
                grand6 += rs.Value10;
                prtFields.Clear();
            }   //  end foreach loop
                //  write grand total
                strWriteOut.WriteLine("____________________________________________________________________________________________________________________________________");
                numOlines++;
                prtFields.Clear();
                switch (currRPT)
                {
                    case "VSM1":
                        fieldLengths = new int[] { 37, 18, 17, 13, 15, 14, 10 };
                        prtFields.Add("");
                        break;
                    case "VSM2":        case "LV01":
                        fieldLengths = new int[] { 23, 10, 10, 10, 15, 10, 11, 11, 12, 8, 9 };
                        prtFields.Add("");
                        if(grand11 > 0)
                            prtFields.Add(String.Format(fieldFormat3, Math.Sqrt(grand7 / grand11)));
                        else 
                            prtFields.Add(String.Format(fieldFormat3, Math.Sqrt(grand7 / grand1)));
                        if(grand11 > 0)
                            prtFields.Add(String.Format(fieldFormat3, grand8 / grand11));
                        else 
                            prtFields.Add(String.Format(fieldFormat3, grand8 / grand1));
                        if (grand9 > 0)
                        {
                            if(grand11 > 0)
                                prtFields.Add(String.Format(fieldFormat3, grand9 / grand11));
                            else 
                                prtFields.Add(String.Format(fieldFormat3, grand9 / grand1));
                        }
                        else prtFields.Add("     ");
                        if (grand10 > 0)
                        {
                            if(grand11 > 0)
                                prtFields.Add(String.Format(fieldFormat3, grand10 / grand11));
                            else 
                                prtFields.Add(String.Format(fieldFormat3, grand10 / grand1));
                        }
                        else prtFields.Add("     ");
                        break;
                    case "VSM3":        case "LV02":
                        fieldLengths = new int[] { 18, 6, 8, 8, 11, 10, 10, 8, 13, 10, 11, 8, 10 };
                        prtFields.Add("");
                        if(grand11 > 0)     //  3P or S3P methods
                            prtFields.Add(String.Format(fieldFormat3, Math.Sqrt(grand7 / grand11)));
                        else 
                            prtFields.Add(String.Format(fieldFormat3, Math.Sqrt(grand7 / grand1)));
                        if(grand11 > 0)
                            prtFields.Add(String.Format(fieldFormat3, grand8 / grand11));
                        else 
                            prtFields.Add(String.Format(fieldFormat3, grand8 / grand1));
                        if (grand9 > 0)
                        {
                            if (grand11 > 0)
                                prtFields.Add(String.Format(fieldFormat3, grand9 / grand11));
                            else
                                prtFields.Add(String.Format(fieldFormat3, grand9 / grand1));
                        }
                        else prtFields.Add("     ");
                        if (grand10 > 0)
                        {
                            if (grand11 > 0)
                                prtFields.Add(String.Format(fieldFormat3, grand10 / grand11));
                            else
                                prtFields.Add(String.Format(fieldFormat3, grand10 / grand1));
                        }
                        else prtFields.Add("     ");
                        if (grand3 > 0)
                            prtFields.Add(String.Format(fieldFormat4, grand2 / grand3));
                        else prtFields.Add(" 0.0000");
                        if (grand5 > 0)
                            prtFields.Add(String.Format(fieldFormat4, grand4 / grand5));
                        else prtFields.Add(" 0.0000");
                        break;
                }   //  end switch on report

                prtFields.Add(String.Format(fieldFormat5, grand1));
                prtFields.Add(String.Format(fieldFormat6, grand2));
                prtFields.Add(String.Format(fieldFormat7, grand3));
                prtFields.Add(String.Format(fieldFormat6, grand4));
                prtFields.Add(String.Format(fieldFormat7, grand5));
                prtFields.Add(String.Format(fieldFormat2, grand6));
                printOneRecord(fieldLengths, prtFields, strWriteOut);
                strWriteOut.WriteLine("____________________________________________________________________________________________________________________________________");
                numOlines++;
            return;
        }   //  end PrintSubtotals


        private void PrintSubtotals(StreamWriter strWriteOut)
        {
            //  overloaded to print totals for value, weight and total cubic reports
            strWriteOut.WriteLine("                                        _________                  _________                  _________");
            strWriteOut.Write("                        TOTALS ------  ");
            strWriteOut.Write(String.Format("{0,10:F0}", valueGrandTotal));
            strWriteOut.Write("                 ");
            strWriteOut.Write(String.Format("{0,10:F0}", wgtGrandTotal));
            strWriteOut.Write("                 ");
            strWriteOut.WriteLine(String.Format("{0,10:F0}", totCubGrandTotal));
            strWriteOut.WriteLine("                                        _________                  _________                  _________");
            numOlines += 3;
            return;
        }   //  end PrintSubtotals
    }
}
