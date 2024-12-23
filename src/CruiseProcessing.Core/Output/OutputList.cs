﻿using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using CruiseProcessing.Data;
using CruiseProcessing.Output;
using CruiseProcessing.OutputModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class OutputList : OutputFileReportGeneratorBase
    {
        #region headers
        //  A Reports
        public readonly string[] A01unit = new string[3] {"                              CUTTING   CUTTING",
                                                "     CRUISE       CUTTING     UNIT      UNIT                        LOG      PAYMENT",
                                                "     NO.          UNIT NO.    ACRES     DESCRIPTION                 METHOD   UNIT NO."};
        public readonly string[] A01stratum = new string[2] {"     CRUISE   STRATA   CRUISE   STRATA          PLOT  NO. OF STRATA                     DATE",
                                                   "     NO.      NO.      METHOD   ACRES   BAF     SIZE  PLOTS  DESCRIPTION                MMYYYY"};
        public readonly string[] A02plot = new string[2] {"     CRUISE   PLOT     CUTTING  STRATA    SLOPE           PLOT   NULL",
                                                 "     NO.      NO.      UNIT NO. NO.         %     ASPECT  KPI    PLOT?"};
        public readonly string[] A01payment = new string[2] {"     CRUISE     CUTTING             PAYMENT",
                                                    "     NO.        UNIT     STRATUM    UNIT"};
        public readonly string[] A01samplegroup = new string[2] {"          SAMPLE                 BIG   SMALL",
                                                        " STRATUM  GROUP    FREQ   KZ     BAF   FPS      DESCRIPTION"};
        public readonly string[] A03columns = new string[49] {"    STRATA","      UNIT","      PLOT","      TREE","   SPECIES",
                                                    "  PROD PRI","  PROD SEC","   SMP GRP"," CUT/LEAVE","  CNT/MEAS",
                                                    "TREE COUNT"," U OF MEAS","  ESTIMATE","P CULL&BRK",
                                                    " P HID DEF","P SEEN DEF","  P REC PD","S CULL&BRK"," S HID DEF",
                                                    "S SEEN DEF","   SLOPE %","MRKR INITL","  YLD COMP"," LIVE/DEAD",
                                                    "CONTR SPEC","TREE GRADE","  HT 1 LMB","  POLE LEN","CLEAR FACE",
                                                    " CROWN RAT","    DBH-OB","    DRC-OB","    TOT HT"," MRHT TYPE",
                                                    "MRHT LG LN","MRHT PR PD","MRHT SE PD","  REFER HT","FORM CLASS",
                                                    "Z FORM FAC"," U STM DIA"," HT U STEM","   DBH BTR","DBH DBL BT",
                                                    "TDIB PR PD","TDIB SE PD","T DEF CODE","TDIA@DF PT",
                                                    "    VOID %"};
        public readonly string[] A04counts = new string[4] {"THE FOLLOWING INFORMATION IS PROVIDED FOR TREE-BASED METHODS AND REPRESENTS WHAT ONCE WERE COUNT RECORDS",
                                                  " ",
                                                  " CUTTING            SAMPLE                        KZ      TALLY",
                                                  "   UNIT   STRATUM   GROUP   SPECIES   FREQUENCY   VALUE   COUNT   KPI      DESCRIPTION"};
        //public static readonly string[] A03remarks = new string[2] {"           CUTTING",
        //                                           "   STRATUM    UNIT   PLOT   TREE   SPECIES   REMARK"};

        public readonly string[] A08columns = new string[7] {"                     /L      %   %     /L      %   %     /L      %   %     /L      %   %     /L      %   %",
                                                   "  S                  /O      D         /O      D         /O      D         /O      D         /O      D    ",
                                                   "  T                  /G   G  E   R     /G   G  E   R     /G   G  E   R     /G   G  E   R     /G   G  E   R",
                                                   "  R  U    P   T      /    R  F   E     /    R  F   E     /    R  F   E     /    R  F   E     /    R  F   E",
                                                   "  A  N    L   R      /N   A  E   C     /N   A  E   C     /N   A  E   C     /N   A  E   C     /N   A  E   C",
                                                   "  T  I    O   E      /U   D  C   V     /U   D  C   V     /U   D  C   V     /U   D  C   V     /U   D  C   V",
                                                   "  A  T    T   E      /M   E  T   B     /M   E  T   B     /M   E  T   B     /M   E  T   B     /M   E  T   B"};
        public readonly string[] A09columns = new string[7] {"                   L                                                            %    %",
                                                   "  S                O      S     L     L                                              D",
                                                   "  T                G      M     G     E  G                                      R    E",
                                                   "  R  U    P   T                       N  R                                      E    F",
                                                   "  A  N    L   R    N      D     D     G  A   GROSS VOLUME     NET VOLUME        C    E",
                                                   "  T  I    O   E    U      I     I     T  D                                      V    C",
                                                   "  A  T    T   E    M      A     A     H  E   BDFT    CUFT     BDFT    CUFT      B    T"};
        public readonly string[] A05A07columns = new string[10] {"                               K   Z",
                                                        "                               K   Z",
                                                        "                               K   Z",
                                                        "                  S            K   Z",
                                                        "  S               P       D    K   Z",
                                                        "  T               E       B    K   Z",
                                                        "  R  U    P   T   C       H    K   Z   ************ PER TREE ************                           ***** PER ACRE/PER STRATA *****",
                                                        "  A  N    L   R   I       -    K   Z   *** PRIMARY ***    ** SECONDARY **                           ** PRIMARY **   ** SECONDARY **",
                                                        "  T  I    O   E   E       O    K   Z    GROSS    NET      GROSS   NET      TREE    CHAR      EXP     GROSS   NET      GROSS    NET",
                                                        "  A  T    T   E   S       B    K   Z    VVVV     VVVV     VVVV    VVVV      FAC     FAC      FAC     VVVV    VVVV     VVVV     VVVV"};
        public readonly string[] A06columns = new string[10] {"                               K   Z",
                                                     "                               K   Z",
                                                     "                               K   Z",
                                                     "                  S            K   Z",
                                                     "  S               P        D   K   Z",
                                                     "  T               E        B   K   Z  ************ PER TREE ************                         ******* PER ACRE/PER STRATA *******",
                                                     "  R  U  P    T    C        H   K   Z",
                                                     "  A  N  L    R    I        -   K   Z          ***** VALUE *****                                          ***** VALUE *****  ",
                                                     "  T  I  O    E    E        O   K   Z      PRIMARY        SECONDARY       TREE     CHAR     EXP        PRIMARY        SECONDARY ",
                                                     "  A  T  T    E    S        B   K   Z      PRODUCT        PRODUCT         FAC      FAC      FAC        PRODUCT        PRODUCT  "};
        public readonly string[] A10columns = new string[10] {"                                 K   Z",
                                                     "                                 K   Z",
                                                     "                                 K   Z",
                                                     "                    S            K   Z",
                                                     "  S                 P      D     K   Z",
                                                     "  T                 E      B     K   Z",
                                                     "  R  U    P    T    C      H     K   Z   ****************************** PER TREE GREEN POUNDS **********************************",
                                                     "  A  N    L    R    I      -     K   Z       **************** COMPONENT *******************************     ",
                                                     "  T  I    O    E    E      O     K   Z    PRIMARY  SECONDARY          LIVE     DEAD      STEM     TOTAL    TREE     CHAR     EXP",
                                                     "  A  T    T    E    S      B     K   Z    PRODUCT  PRODUCT   FOLIAGE  BRANCHES BRANCHES  TIP      TREE     FAC      FAC      FAC "};
        //public static readonly string A10footer = "NOTE:  PERCENT MOISTURE AND PERCENT REMOVED HAVE BEEN APPLIED TO BIOMASS COMPONENT WEIGHTS.";
        
        public readonly string[] A13plot = new string[6] {"             S",
                                                 "             T",
                                                 "    P   U    R",
                                                 "    L   N    A",
                                                 "    O   I    T",
                                                 "    T   T    A     X-METERS     Y-METERS    Z-UNIT   METADATA"};
        public readonly string[] A13tree = new string[6] {"  S",
                                                 "  T",
                                                 "  R    U     P      T",
                                                 "  A    N     L      R",
                                                 "  T    I     O      E",
                                                 "  A    T     T      E  SG     X-METERS     Y-METERS    Z-UNIT  METADATA"};


        //  new modified merch rules report (A15)
        private string[] A15columns = new string[2] {"  VOLUME                    MIN LOG LENGTH    MAX LOG LENGTH   SEGMENTATION    MIN MERCH    EVEN/ODD",
                                                    " EQUATION   PRODUCT   TRIM     PRIMARY           PRIMARY           LOGIC        LENGTH      SEGMENT     MODIFIED?"};

        //  Log level reports
        //  L1 report
        private readonly string[] L1columns = new string[9] {"                             U",
                                                   "                                                                %",
                                                   "                     S    P  O   L     S      L                      %",
                                                   "   S                 P    R  F   O     M      G      L          D",
                                                   "   T                 E    O      G                   E      G   E    R",
                                                   "   R   U   P    T    C    D  M         D      D      N      R   F    E",
                                                   "   A   N   L    R    I    U  E   N     I      I      G      A   E    C",
                                                   "   T   I   O    E    E    C  A   U     A      A      T      D   C    V    GROSS  BDFT   NET     GROSS  CUBIC  NET    DIB  TOTAL",
                                                   "   A   T   T    E    S    T  S   M     M      M      H      E   T    B    BDFT   REMVD  BDFT    CUBIC  REMVD  CUBIC  CLS  EXPANS"};

        #endregion headers


        private int[] fieldLengths;
        private List<string> prtFields = new List<string>();
        private string[] completeHeader;
        private List<string> fieldsToPrint;
        private List<TreeDO> tList = new List<TreeDO>();
        private List<TreeCalculatedValuesDO> tcvList = new List<TreeCalculatedValuesDO>();
        private List<LogStockDO> lsList = new List<LogStockDO>();
        private List<PlotDO> pList = new List<PlotDO>();

        public OutputList(CpDataLayer dataLayer, HeaderFieldData headerData, string reportID) : base(dataLayer, headerData, reportID)
        {
        }

        public void OutputListReports(TextWriter strWriteOut, ref int pageNumb)
        {
            //  Fill report title array
            string currentTitle = fillReportTitle(currentReport);

            //  create report title
            if (currentReport == "A05")
                SetReportTitles(currentTitle, 3, 15, 45, "", "");
            else if (currentReport == "A06")
                SetReportTitles(currentTitle, 2, 25, 0, "", "");
            else if (currentReport == "A07")
                SetReportTitles(currentTitle, 3, 15, 45, "", "");
            else if (currentReport == "A15")
                SetReportTitles(currentTitle, 5, 0, 0, reportConstants.NSPO, "");
            else SetReportTitles(currentTitle, 1, 0, 0, "", "");

            //  Which report?  Previous report number in parens
            numOlines = 0;
            switch (currentReport)
            {
                case "A01":          //  Strata, Unit, and Payment unit Report
                                     //  stratum page
                    {
                        numOlines = 0;
                        List<StratumDO> sList = DataLayer.GetStrata();
                        fieldLengths = new int[] { 5, 11, 7, 9, 8, 9, 6, 6, 27, 2, 4 };
                        prtFields.Clear();
                        WriteStratum(strWriteOut, sList, ref pageNumb);
                        //  cutting unit page
                        numOlines = 0;
                        List<CuttingUnitDO> cList = DataLayer.getCuttingUnits();
                        //  page 2 -- cutting units
                        fieldLengths = new int[] { 5, 17, 8, 10, 28, 9, 3 };
                        WriteUnit(1, strWriteOut, cList, ref pageNumb);

                        //  payment unit page
                        numOlines = 0;
                        fieldLengths = new int[] { 5, 13, 9, 9, 4 };
                        WriteUnit(2, strWriteOut, cList, ref pageNumb);
                        //  sample group page
                        numOlines = 0;
                        fieldLengths = new int[] { 2, 10, 7, 7, 7, 7, 7, 25 };
                        List<SampleGroupDO> sgList = DataLayer.getSampleGroups();
                        WriteSampleGroup(strWriteOut, sgList, ref pageNumb);
                        break;
                    }

                case "A02":          //  Listing of plot identification information (page three from old A1 report)List<PlotDO> pList = bslyr.getPlots();
                    {
                        pList = DataLayer.getPlotsOrdered();
                        fieldLengths = new int[] { 5, 9, 11, 10, 6, 9, 8, 7, 3 };
                        prtFields.Clear();
                        if (pList.Count == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No data for report in Plot table");
                            return;
                        }   //  endif no data
                        WritePlot(strWriteOut, ref pageNumb);
                        break;
                    }

                case "A03":          //  Listing of individual tree measurements and characteristics (A2)
                    {
                        tList = DataLayer.GetTreesOrderedByStratum();
                        //  what fields to print?
                        fieldsToPrint = TreeListMethods.MakeTreeFieldList(tList);
                        //  Build column headings based on fields to print
                        completeHeader = TreeListMethods.BuildColumnHeaders(A03columns, fieldsToPrint);
                        WriteTree(strWriteOut, ref pageNumb);

                        break;
                    }

                case "A04":          //  Listing of count table information (count page from A2 report)
                    {
                        List<CountTreeDO> ctList = DataLayer.getCountsOrdered();

                        fieldLengths = new int[] { 3, 9, 10, 6, 13, 7, 8, 8, 11, 1 };
                        prtFields.Clear();
                        //  no data?
                        if (ctList.Count == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No data for report in Count table");
                            return;
                        }   //  endif no data
                        WriteCountTrees(strWriteOut, ctList, ref pageNumb);
                        break;
                    }

                case "A05":          //  Listing of cubic foot volume   (A5)
                    {
                        tList = DataLayer.GetTreesOrderedByStratum();
                        tcvList = DataLayer.getTreeCalculatedValues();
                        fieldLengths = new int[] { 1, 3, 4, 5, 5, 7, 6, 4, 4, 8, 9, 8, 9, 8, 8, 9, 9, 9, 8, 8 };
                        prtFields.Clear();
                        //  no data?
                        var summedValue = tcvList.Sum(cvdo => cvdo.GrossCUFTPP);
                        if (summedValue == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No cubic foot volume for report");
                            return;
                        }   //  endif no data for report
                        WriteTreeCalcValues("CUFT", strWriteOut, tcvList, tList, ref pageNumb);
                        break;
                    }

                case "A07":          //  Listing of board foot volume   (A7)
                    {
                        tList = DataLayer.GetTreesOrderedByStratum();
                        tcvList = DataLayer.getTreeCalculatedValues();
                        fieldLengths = new int[] { 1, 3, 4, 5, 5, 7, 6, 4, 4, 8, 9, 8, 9, 8, 8, 9, 9, 9, 8, 8 };
                        prtFields.Clear();
                        //  no data?
                        var summedValue = tcvList.Sum(cvdo => cvdo.GrossBDFTPP);
                        if (summedValue == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No board foot volume for report");
                            return;
                        }   //  endif no data for report
                        WriteTreeCalcValues("BDFT", strWriteOut, tcvList, tList, ref pageNumb);
                        break;
                    }

                case "A06":          //  Listing of dollar value for each tree  (A6)
                    {
                        tList = DataLayer.GetTreesOrderedByStratum();
                        tcvList = DataLayer.getTreeCalculatedValues();
                        fieldLengths = new int[] { 1, 3, 4, 5, 5, 8, 6, 4, 6, 15, 15, 9, 9, 11, 15, 15 };
                        prtFields.Clear();
                        //  no data?
                        var summedValue = tcvList.Sum(tcvdo => tcvdo.ValuePP);
                        if (summedValue == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No dollar value data for this report");
                            return;
                        }   //  endif no data for report
                        WriteTreeCalcValues("VALUE", strWriteOut, tcvList, tList, ref pageNumb);
                        break;
                    }

                case "A08":          //  Listing of log grade information   (A3)
                    {
                        List<LogDO> logList = DataLayer.getLogs();
                        if (logList.Count == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No log data for this report");
                            return;
                        }   //  endif logList empty

                        tList = DataLayer.GetTreesOrderedByStratum();
                        fieldLengths = new int[] { 1, 3, 4, 5, 8, 5, 2, 4, 7, 5, 2, 4, 7, 5, 2, 4, 7, 5, 2, 4, 7, 5, 2, 4, 7 };
                        prtFields.Clear();
                        WriteLogList(strWriteOut, logList, tList, ref pageNumb);
                        break;
                    }

                case "A09":          //  Listing of log detail information for Fall, Buck and Scale  (A4)
                    {
                        var fbsTrees = DataLayer.getTrees().Where(x => x.IsFallBuckScale == 1).ToList();
                        if (!fbsTrees.Any())
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No fall, buck and scale trees for this report.");
                            return;
                        }   //  endif

                        fieldLengths = new int[] { 1, 3, 4, 5, 6, 5, 6, 6, 5, 3, 10, 7, 10, 7, 5, 5 };
                        prtFields.Clear();
                        lsList = DataLayer.getLogStock();
                        WriteFallBuckScale(strWriteOut, fbsTrees, ref pageNumb);
                        break;
                    }

                case "A10":          //  Listing of calculated biomass weights by component (A9)
                    {
                        tList = DataLayer.GetTreesOrderedByStratum();
                        tcvList = DataLayer.getTreeCalculatedValues();
                        fieldLengths = new int[] { 1, 4, 5, 5, 5, 7, 6, 4, 5, 9, 10, 9, 9, 10, 9, 9, 9, 9, 9 };
                        prtFields.Clear();
                        //  Sum up mainsteam primary to determine if report can be printed
                        var summedValue = tcvList.Sum(tcvdo => tcvdo.BiomassMainStemPrimary);
                        if (summedValue == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No biomass data for this report");
                            return;
                        }   //  endif summedValue
                        WriteTreeCalcValues("BIOMASS", strWriteOut, tcvList, tList, ref pageNumb);
                        break;
                    }

                case "A13":         //  Listing of geospatial information  (A14)
                                    //  This report prints two separate pages -- one for plot information and one for tree information
                                    //  plot information
                    {
                        pList = DataLayer.getPlots();
                        fieldLengths = new int[] { 2, 5, 6, 4, 13, 11, 12, 1 };
                        prtFields.Clear();
                        WritePlot(strWriteOut, ref pageNumb);

                        //  tree information
                        tList = DataLayer.GetTreesOrderedByStratum();
                        fieldLengths = new int[10] { 2, 3, 3, 3, 4, 3, 3, 11, 11, 11 };
                        prtFields.Clear();
                        //  reset headers
                        SetReportTitles(currentTitle, 1, 0, 0, "", "");
                        numOlines = 0;
                        WriteTree(strWriteOut, ref pageNumb);
                        break;
                    }

                case "A15":         //  listing of merch rules
                    {
                        List<VolumeEquationDO> currVolEq = DataLayer.getVolumeEquations();
                        fieldLengths = new int[] { 1, 13, 9, 10, 18, 18, 14, 13, 10, 3 };
                        prtFields.Clear();
                        numOlines = 0;
                        WriteMerchRules(strWriteOut, currVolEq, ref pageNumb);
                        break;
                    }

                case "L1":          //  Log listing
                    {
                        lsList = DataLayer.getLogStockSorted();
                        fieldLengths = new int[] { 2, 3, 4, 5, 5, 7, 3, 4, 5, 7, 7, 9, 3, 4, 6, 7, 7, 8, 7, 7, 7, 3, 8 };
                        prtFields.Clear();
                        //  no data?
                        if (lsList.Count == 0)
                        {
                            noDataForReport(strWriteOut, currentReport, " >>>> No log stock data for this report.");
                            return;
                        }   //  endif no logs
                        WriteLogStock(strWriteOut, lsList, ref pageNumb);
                        break;
                    }
            }   //  end switch on currentReport
            return;
        }   //  end OutputListReports

        private void WriteUnit(int whichPage, TextWriter strWriteOut, List<CuttingUnitDO> cList,
                                ref int pageNumb)
        {
            switch (whichPage)
            {
                case 1:     //  page one -- units
                    double[] unitAcres = new double[cList.Count];
                    int jIndex = 0;
                    foreach (CuttingUnitDO cul in cList)
                    {
                        WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2], A01unit, 10, ref pageNumb, "");
                        prtFields = CuttingUnitMethods.buildPrintArray(cul, HeaderData[3]);
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                        //  save acres for totaling
                        unitAcres[jIndex] = Convert.ToDouble(cul.Area);
                        jIndex++;
                    }   //  end foreach loop

                    //  Total cutting unit acres
                    double totalAcres = CommonEquations.SumList(unitAcres);
                    strWriteOut.WriteLine("\n        TOTAL SALE ACRES:  {0,10:F2}", totalAcres);
                    break;

                case 2:     //  page three -- payment units
                    foreach (CuttingUnitDO cud in cList)
                    {
                        WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2], A01payment, 9, ref pageNumb, "");
                        //  need stratum for each cutting unit
                        var strataList = DataLayer.GetStratumCodesByUnit(cud.Code);
                        if (strataList.Count() > 0)
                        {
                            foreach (string stratumCode in strataList)
                            {
                                prtFields = CuttingUnitMethods.buildPrintArray(cud, HeaderData[3].ToString(),
                                                                                stratumCode);
                                printOneRecord(fieldLengths, prtFields, strWriteOut);
                            }   //  end foreach stratum
                        }
                        else if (strataList.Count() == 0)
                        {
                            prtFields = CuttingUnitMethods.buildPrintArray(cud, HeaderData[3].ToString(), "  ");
                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                        }   //  end strata list is empty
                    }   //  end foreach loop
                    break;
            }   //  end switch on page
            return;
        }   //  end WriteUnit

        private void WriteStratum(TextWriter strWriteOut, List<StratumDO> sList, ref int pageNumb)
        {
            foreach (StratumDO sdo in sList)
            {
                sdo.CuttingUnits.Populate();
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                A01stratum, 9, ref pageNumb, "");
                //  need true stratum acres not used for expansion
                var totalStrataAcres = sdo.CuttingUnits.Sum(cu => cu.Area);
                //  and number of plots for the stratum
                var totalPlots = DataLayer.GetPlotsByStratum(sdo.Code).Count;

                prtFields = BuildStratumPrintArray(sdo, HeaderData.CruiseName,
                                                            totalStrataAcres, totalPlots);
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop

            return;
        }   //  end WriteStratum

        private static List<string> BuildStratumPrintArray(StratumDO stl, string cruiseName, double totalAcres,
                                            double numPlots)
        {
            var baf = (CruiseMethods.VARIABLE_RADIUS_METHODS.Contains(stl.Method)) ? stl.BasalAreaFactor : 0;
            var fps = (CruiseMethods.FIXED_SIZE_PLOT_METHODS.Contains(stl.Method)) ? stl.FixedPlotSize : 0;

            //  parameter list will have two other fields -- strata acres and number of plots
            var stratumArray = new List<string>() {
                "   ",
                cruiseName.PadRight(5, ' '),
                stl.Code.PadLeft(2, ' '),

                stl.Method.PadRight(6, ' '),
                String.Format("{0,6:F2}", totalAcres).PadLeft(6, ' '),
                String.Format("{0,7:F2}", baf).PadLeft(7, ' '),
                String.Format("{0,4:F0}", fps).PadLeft(3, ' '),
                String.Format("{0,3:F0}", numPlots).PadLeft(3, ' '),
                stl.Description ?? (" ").PadRight(25, ' '),
                String.Format("{0,2:F0}", stl.Month),
                String.Format("{0,4:F0}", stl.Year),
            };

            return stratumArray;
        }   //  end buildPrintArray

        private void WriteCountTrees(TextWriter strWriteOut, List<CountTreeDO> ctList, ref int pageNumb)
        {
            //  output count table (was originally in the old A2 report as a separate page)
            foreach (CountTreeDO cdo in ctList)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2], A04counts, 11, ref pageNumb, "");
                prtFields = CountTreeMethods.buildPrintArray(cdo, cdo.SampleGroup.Stratum.Code);
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
        }   //  end WriteCountTrees

        private void WriteTreeCalcValues(string volType, TextWriter strWriteOut, List<TreeCalculatedValuesDO> cvList,
                                        List<TreeDO> tList, ref int pageNumb)
        {

            whichHeightFields(out var hgtOne, out var hgtTwo, tList);

            //  replace as needed header lines
            string[] headerToPrint = new string[10];
            if (volType == "CUFT" || volType == "BDFT")
            {
                headerToPrint = updateHeightHeader(hgtOne, hgtTwo, "", A05A07columns);
                headerToPrint[9] = headerToPrint[9].Replace("VVVV", volType);
            }
            else if (volType == "VALUE")
                headerToPrint = updateHeightHeader(hgtOne, hgtTwo, "", A06columns);
            else if (volType == "BIOMASS")
                headerToPrint = updateHeightHeader(hgtOne, hgtTwo, "", A10columns);

            //  print lines
            foreach (TreeDO tl in tList)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                    headerToPrint, 19, ref pageNumb, "");
                if ((tl.CountOrMeasure == "M" && tl.ExpansionFactor > 0) ||
                    (tl.Stratum.Method == "FIXCNT"))
                {
                    switch (volType)
                    {
                        case "CUFT":
                            prtFields = TreeListMethods.buildPrintArray(tl, HeaderData.CruiseName, cvList,
                                                            hgtOne, hgtTwo, volType);
                            break;

                        case "BDFT":
                            prtFields = TreeListMethods.buildPrintArray(tl, HeaderData.CruiseName, cvList,
                                                            hgtOne, hgtTwo, volType);
                            break;

                        case "VALUE":
                            prtFields = TreeListMethods.buildPrintArray(tl, tcvList, hgtOne, hgtTwo, "value");
                            break;

                        case "BIOMASS":
                            prtFields = TreeListMethods.buildPrintArray(tl, tcvList, hgtOne, hgtTwo, "biomass");
                            break;
                    }   //  switch on volType
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  endif count or measure tree
            }   //  end foreach loop
            return;
        }   //  end WriteTreeCalcValues

        private void WritePlot(TextWriter strWriteOut, ref int pageNumb)
        {
            if (currentReport == "A13")
            {
                //  output plot table information for A13
                foreach (PlotDO pl in pList)
                {
                    WriteReportHeading(strWriteOut, reportTitles[0], "PLOT TABLE INFORMATION",
                                    reportTitles[2], A13plot, 7, ref pageNumb, "");
                    prtFields = PlotMethods.buildPrintArray(pl, pl.Stratum.Code, pl.CuttingUnit.Code);
                    if (prtFields.Count > 0)
                    {
                        if (prtFields[0] != null)
                            printOneRecord(fieldLengths, prtFields, strWriteOut);
                    }   //  endif
                }   //  end foreach loop
            }
            else if (currentReport == "A02")
            {
                foreach (PlotDO pl in pList)
                {
                    if (pl.Stratum_CN != null && pl.Stratum_CN != 0)
                    {
                        WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2], A02plot, 9, ref pageNumb, "");
                        prtFields = PlotMethods.buildPrintArray(pl, HeaderData.CruiseName,
                                                                pl.Stratum.Code, pl.CuttingUnit.Code);
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                        //  print remarks on separate line
                        if (pl.Remarks != "" && pl.Remarks != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("          REMARKS >>   ");
                            if (pl.Remarks.Contains('~'))
                            {
                                string remarkLine = pl.Remarks.Replace("~", "\n\t\t\t\t\t");
                                sb.Append(remarkLine);
                                strWriteOut.WriteLine(sb.ToString());
                                //  how many lines possible in the remarks line
                                //  not sure this is needed here but put it in anyway
                                //  doesn't seem like FScruiser is putting in the tilde
                                //  July 2015
                                int iLength = pl.Remarks.Length;
                                int nLines = (int)Math.Ceiling((double)iLength / 130);
                                numOlines += nLines;
                            }
                            else
                            {
                                sb.Append(pl.Remarks);
                                strWriteOut.WriteLine(sb.ToString());
                                //  how many lines possible in the remarks line
                                int iLength = pl.Remarks.Length;
                                int nLines = (int)Math.Ceiling((double)iLength / 130);
                                numOlines += nLines;
                            }   //  endif
                        }   //  endif remarks available for printing
                    }   //  endif not null
                }   //  end foreach loop
            }   //  endif currentReport
        }   //  end WritePlot

        private void WriteTree(TextWriter strWriteOut, ref int pageNumb)
        {
            if (currentReport == "A13")
            {
                //  output tree table information
                foreach (TreeDO tdo in tList)
                {
                    WriteReportHeading(strWriteOut, reportTitles[0], "TREE TABLE INFORMATION",
                                        reportTitles[2], A13tree, 7, ref pageNumb, "");
                    prtFields = TreeListMethods.buildPrintArray(tdo);
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  end foreach loop
            }
            else if (currentReport == "A03")
            {
                
                //  Output records
                foreach (TreeDO tdo in tList)
                {
                    WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                    completeHeader, 18, ref pageNumb, "");
                    prtFields = TreeListMethods.buildPrintArray(tdo, fieldsToPrint, out fieldLengths);
                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                    prtFields.Clear();
                    //  Write remarks after tree data
                    if (tdo.Remarks != "" && tdo.Remarks != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("     REMARKS: ");
                        sb.Append(tdo.Remarks);
                        strWriteOut.WriteLine(sb.ToString());
                        numOlines++;
                    }
                }   //  end foreach loop
            }   //  endif
            return;
        }   //  end WriteTree

        private void WriteLogList(TextWriter strWriteOut, List<LogDO> logList, List<TreeDO> tList, ref int pageNumb)
        {
            //  output log data (not logstock)
            //  printing is for 5 logs per line; positions 1-4 are the same until a new tree
            foreach (TreeDO tdo in tList)
            {
                //  any logs for this tree?
                List<LogDO> currentLogs = LogMethods.GetLogRecords(logList, (long)tdo.Tree_CN);
                int numbLogs = currentLogs.Count();
                if (numbLogs > 0)
                {
                    //  how many iterations for the number of logs?
                    int numIts = numbLogs / 5;
                    int remainLogs = numbLogs - numIts * 5;
                    //  Need to round iterations to nearest whole number and save remainder to print all logs

                    for (int n = 0; n < numIts; n++)
                    {
                        WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                            A08columns, 10, ref pageNumb, "");
                        switch (n)
                        {
                            case 0:     //  logs 1-5
                                prtFields = LogMethods.buildPrintArray(currentLogs, 0, 4);
                                break;

                            case 1:     //  logs 6-10
                                prtFields = LogMethods.buildPrintArray(currentLogs, 5, 9);
                                break;

                            case 2:     //  logs 11-15
                                prtFields = LogMethods.buildPrintArray(currentLogs, 10, 14);
                                break;

                            case 3:     //  logs 16-20
                                prtFields = LogMethods.buildPrintArray(currentLogs, 15, 19);
                                break;
                        }   //  end switch
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                    }   //  end for n loop

                    //  print remaining logs if needed
                    if (remainLogs > 0)
                    {
                        WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                            A08columns, 10, ref pageNumb, "");
                        prtFields = LogMethods.buildPrintArray(currentLogs, numbLogs - remainLogs, numbLogs - 1);
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                    }   //  endif remaining logs
                }   //  endif number of logs
            }   //  end foreach loop
            return;
        }   //  end WriteLogList

        private void WriteFallBuckScale(TextWriter strWriteOut, List<TreeDO> justFBS, ref int pageNumb)
        {
            //LogMethods Lms = new LogMethods();
            foreach (TreeDO tdo in justFBS)
            {
                //  find logs for this tree
                List<LogStockDO> fbsLogs = lsList.FindAll(
                    delegate (LogStockDO lsdo)
                    {
                        return lsdo.Tree_CN == tdo.Tree_CN;
                    });

                if (fbsLogs.Count > 0)
                {
                    foreach (LogStockDO lsd in fbsLogs)
                    {
                        WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                            A09columns, 10, ref pageNumb, "");
                        prtFields = LogMethods.buildPrintArray(lsd);
                        printOneRecord(fieldLengths, prtFields, strWriteOut);
                    }   //  end foreach loop
                }   //  endif
            }   //  end foreach loop

            return;
        }   //  end WriteFallBuckScale

        private void WriteLogStock(TextWriter strWriteOut, List<LogStockDO> lsList, ref int pageNumb)
        {
            //  outputs data from the logstock table -- primarily the L1 report
            foreach (LogStockDO lsdo in lsList)
            {
                if (numOlines >= 48) numOlines = 0;
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                    L1columns, 10, ref pageNumb, "");
                //  calulate total expansion factor -- expansion factor times strata acres
                double STacres = Utilities.ReturnCorrectAcres(lsdo.Tree.Stratum.Code, DataLayer, (long)lsdo.Tree.Stratum_CN);
                if (STacres == 0.0) STacres = 1.0;
                double totEF = lsdo.Tree.ExpansionFactor * STacres;
                prtFields = LogMethods.buildPrintArray(lsdo, totEF);
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            return;
        }   //  end WriteLogStock

        private void WriteSampleGroup(TextWriter strWriteOut, List<SampleGroupDO> sgList, ref int pageNumb)
        {
            //  outputs data from the sample group table
            foreach (SampleGroupDO sg in sgList)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                    A01samplegroup, 9, ref pageNumb, "");
                prtFields.Clear();
                prtFields = SampleGroupMethods.buildPrintArray(sg);
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            return;
        }   //  end WriteSampleGroup

        private void WriteMerchRules(TextWriter strWriteOut, List<VolumeEquationDO> currEQ, ref int pageNum)
        {
            //  output merch rules
            foreach (VolumeEquationDO ce in currEQ)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2], A15columns, 11, ref pageNum, "");
                prtFields.Clear();
                prtFields = VolumeEqMethods.buildMerchArray(ce);
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach

            //  output footer
            strWriteOut.WriteLine("\n\n");
            strWriteOut.WriteLine(" SEGMENT DESCRIPTIONS");
            strWriteOut.WriteLine("			21 -- If top seg < 1/2 nom log len, combine with next lowest log");
            strWriteOut.WriteLine("			22 -- Top placed with next lowest log and segmented");
            strWriteOut.WriteLine("			23 -- Top segment stands on its own");
            strWriteOut.WriteLine("			24 -- If top seg < 1/4 log len drop the top.  If top >= 1/4 and <= 3/4 nom length, ");
            strWriteOut.WriteLine("						top is 1/2 of nom log lenght, else top is nom log len.");
            return;
        }   //  end WriteMerchRules
    }
}