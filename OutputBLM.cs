using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public class OutputBLM : CreateTextFile
    {
        #region
        public string currentReport;
        private int[] fieldLengths;
        private ArrayList prtFields = new ArrayList();
        private string extraLine = "VOLUMES BASED ON XX FOOT EQUATIONS";
        private double currGRS = 0.0;
        private double currNET = 0.0;
        private double currREM = 0.0;
        private double currAcres = 0.0;
        private string volType = "BOARD";
        private List<ReportSubtotal> listToOutput = new List<ReportSubtotal>();
        private List<ReportSubtotal> totalToOutput = new List<ReportSubtotal>();
        private List<ReportSubtotal> PerCentList = new List<ReportSubtotal>();
        private List<LogStockDO> justLogs = new List<LogStockDO>();
        private List<LogStockDO> justLive = new List<LogStockDO>();
        private List<LogStockDO> justDead = new List<LogStockDO>();
        private string currSP;
        private double currLIVE = 0;
        private double currDEAD = 0;
        private double[] percentsArray = new double[6];
        private string[] columnHeaderI = new string[] {"         QUAD    AVG GM   TOTAL      GROSS",
                                                       "         MEAN     LOG      NET       MERCH      GROSS                  GROSS      GROSS    TOTAL #  TOTAL #                AVG VOL",
                                                       "STRATA   DBH     VOLUME   VOLUME     VOLUME     VOLUME  RCVRY  SLVG    VOLUME     VOLUME    MERCH    LOGS     NUM    NUM   / ACRE",
                                                       "NUMBER  (INCH)   (XXXX)   (XXXX)     (XXXX)     (XXXX)   (%)    (%)  (GRADE 7)  (GRADE 8)   LOGS   (GRD 7-9)  TREES* ACRES  (XXXX)"};
        private string[] columnHeaderII = new string[] {"         QUAD   AVG GM   TOTAL      GROSS",
                                                        "         MEAN    LOG      NET       MERCH      GROSS             GROSS     GROSS     TOTAL  TOTAL #          PERCENT VOLUME BY GRADE",
                                                        "SPECIES  DBH    VOLUME   VOLUME     VOLUME     VOLUME RCVRY SLVG VOLUME    VOLUME    MERCH   LOGS    NUM",
                                                        "        (INCH)  (XXXX)   (XXXX)     (XXXX)     (XXXX)  (%)   (%) (GRADE 7) (GRADE 8) LOGS   (GR 7-9) TREES*    1   2   3   4   5   6"};
        private string[] columnHeaderIII = new string[] {"                   QUAD   AVG GM   TOTAL      GROSS",
                                                         "                   MEAN    LOG      NET       MERCH      GROSS              GROSS      GROSS      TOTAL #  TOTAL #   EST.",
                                                         " UNIT   SPECIES    DBH    VOLUME   VOLUME     VOLUME     VOLUME RCVRY SLVG  VOLUME     VOLUME     MERCH    LOGS      NO.",
                                                         " NUMBER           (INCH)  (XXXX)   (XXXX)     (XXXX)     (XXXX)  (%)   (%)  (GRADE 7)  (GRADE 8)  LOGS     (GRD 7-9) TREES"};
        private string[] columnHeaderIV = new string[] {" DIB |        L   O   G         G   R   A   D  E                     |         |   LOG GRADE    | HD/VIS |        |        |",
                                                        "CLASS|                                                               |  NET    |                | DEFECT | GROSS  |  GRADE | GROSS",
                                                        "  1\" |      0        1        2        3        4        5        6  | VOLUME  |    7       8   | (1-8)  | MERCH  |    9   | VOLUME"};
        private string[] completeHeader = new string[4];
        #endregion

        public void CreateBLMreports(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  Fill report title array
            string currentTitle = fillReportTitle(currentReport);
            //  fix extra line here
            List<VolumeEquationDO> vList = bslyr.getVolumeEquations();
            int nthRow = vList.FindIndex(
                delegate(VolumeEquationDO ved)
                {
                    return ved.VolumeEquationNumber.Substring(3, 4) == "B32W";
                });
            if (nthRow >= 0)
                extraLine = extraLine.Replace("XX", "32");
            else extraLine = extraLine.Replace("XX", "16");
            //  is there data in log stock to create report?
            List<LogStockDO> logList = bslyr.getLogStock();
            if (logList.Count == 0)
            {
                noDataForReport(strWriteOut, currentReport, " >>>> No log stock records for report");
                return;
            }   //  endif no log stock records

            //  reset volume type for specific reports
            if (currentReport == "BLM02" || currentReport == "BLM04" ||
                currentReport == "BLM06" || currentReport == "BLM08" ||
                currentReport == "BLM10")
                volType = "CUBIC";

            List<TreeDO> treeList = new List<TreeDO>();
            List<CuttingUnitDO> cutList = new List<CuttingUnitDO>();
            List<LCDDO> speciesGroups = new List<LCDDO>();
            //  process reports
            switch (currentReport)
            {
                case "BLM01":
                case "BLM02":        //  reports by stratum
                    //  pull tables to be used
                    List<StratumDO> sList = bslyr.getStratum();
                    List<LCDDO> lcdList = bslyr.getLCD();
                    numOlines = 0;
                    AccumulateByStrata(sList, lcdList);
                    fieldLengths = new int[] { 1, 7, 7, 9, 9, 11, 12, 7, 6, 11, 9, 9, 9, 9, 7, 8 };
                    completeHeader = createCompleteHeader();
                    rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FCTO, extraLine);
                    writeCurrentGroups(strWriteOut, ref pageNumb, rh);
                    updateTotalLine();
                    outputTotalLine(strWriteOut, rh, ref pageNumb);
                    //  output footnote
                    strWriteOut.WriteLine("");
                    strWriteOut.WriteLine("*  NOTE:  THIS TOTAL INCLUDES CULL TREES.");
                    //  output CSV file
                    createCSVfile();
                    break;
                case "BLM03":
                case "BLM04":        //  reports by unit -- prorated
                    numOlines = 0;
                    cutList = bslyr.getCuttingUnits();
                    AccumulateByUnit(cutList);
                    fieldLengths = new int[] { 1, 7, 7, 9, 9, 11, 12, 7, 6, 11, 9, 9, 9, 9, 7, 8 };
                    completeHeader = createCompleteHeader();
                    rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FCTO, extraLine);
                    writeCurrentGroups(strWriteOut, ref pageNumb, rh);
                    updateTotalLine();
                    outputTotalLine(strWriteOut, rh, ref pageNumb);
                    //  output CSV file
                    createCSVfile();
                    break;
                case "BLM05":
                case "BLM06":        //  report by species
                    //  pull tables to be used
                    speciesGroups = bslyr.getLCDOrdered("", "GROUP BY Species", "C", "");
                    treeList = bslyr.getTrees();
                    numOlines = 0;
                    List<ReportSubtotal> PerCentArray = AccumulateBySpecies(treeList, speciesGroups);
                    fieldLengths = new int[] { 1, 7, 6, 9, 9, 11, 11, 5, 5, 9, 9, 9, 9, 9, 4, 4, 4, 4, 4, 3 };
                    completeHeader = createCompleteHeader();
                    rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FCTO, extraLine);
                    writeCurrentGroups(strWriteOut, ref pageNumb, rh, PerCentArray);
                    updateTotalLine();
                    outputTotalLine(strWriteOut, rh, ref pageNumb);
                    //  output footnote
                    strWriteOut.WriteLine("");
                    strWriteOut.WriteLine("*  NOTE:  THIS TOTAL INCLUDES CULL TREES.");
                    //  output CSV file
                    createCSVfile();
                    break;
                case "BLM07":
                case "BLM08":        //  report by unit and species -- prorated
                    cutList = bslyr.getCuttingUnits();
                    List<CSVlist> CSV7and8 = new List<CSVlist>();
                    speciesGroups = bslyr.getLCDOrdered("WHERE CutLeave = ?", "GROUP BY Species", "C", "");
                    numOlines = 0;
                    fieldLengths = new int[] { 1, 6, 10, 6, 10, 9, 11, 11, 5, 7, 11, 9, 9, 9, 8 };
                    completeHeader = createCompleteHeader();
                    rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FCTO, extraLine);
                    foreach (CuttingUnitDO cud in cutList)
                    {
                        cud.Strata.Populate();
                        List<ReportSubtotal> unitTotals = AccumulateByUnitSpecies(cud, speciesGroups);
                        writeCurrentGroups(unitTotals, strWriteOut, ref pageNumb, rh);
                        //  need to capture CSV records here
                        capture7and8CSV(listToOutput, CSV7and8);
                        listToOutput.Clear();
                        unitTotals.Clear();
                    }   //  end foreach cutting unit
                    outputTotalLine(strWriteOut, rh, ref pageNumb);
                    //  output CSV file
                    createCSVfile(CSV7and8);
                    break;
                case "BLM09":
                case "BLM10":        //  report by DIB class and species
                    numOlines = 0;
                    completeHeader = createCompleteHeader();
                    fieldLengths = new int[] { 1, 4, 9, 9, 9, 9, 9, 9, 11, 8, 9, 11, 7, 11, 7, 9 };
                    ArrayList justSpecies = bslyr.GetJustSpecies("Tree");
                    //  load log DIBs into output list
                    List<LogStockDO> justDIBs = bslyr.getLogDIBs();
                    //  process by species
                    foreach (object js in justSpecies)
                    {
                        LoadLogDIBclasses(justDIBs, listToOutput);
                        currSP = Convert.ToString(js);
                        extraLine = "    SPECIES:  ";
                        extraLine += currSP.PadLeft(6, ' ');
                        rh.createReportTitle(currentTitle, 6, 0, 0, reportConstants.FCTO, "");
                        AccumulateBySpeciesDIB();
                        writeCurrentGroups(rh, ref pageNumb, strWriteOut);
                        //  update total line
                        updateTotalLine();
                        outputTotalLine(strWriteOut, rh, ref pageNumb);
                        listToOutput.Clear();
                        totalToOutput.Clear();
                        numOlines = 0;
                    }   //  end foreach loop on species
                    break;
            }   //  end switch on report
            return;
        }   //  end CreateBLMreports


        private void AccumulateByStrata(List<StratumDO> sList, List<LCDDO> lcdList)
        {
            //  loop by stratum
            foreach (StratumDO s in sList)
            {
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = s.Code;
                s.CuttingUnits.Populate();
                
                //  need strata acres for expansion
                currAcres = Utilities.ReturnCorrectAcres(s.Code, bslyr, (long)s.Stratum_CN);
                //  pull stratum from LCD to calculate quad mean DBH later -- sum expansion factor
                List<LCDDO> justStrata = lcdList.FindAll(
                    delegate(LCDDO l)
                    {
                        return l.Stratum == s.Code;
                    });
                AccumulateExpFac(r, s.Method, justStrata, justStrata[0], 1);
                //  also need true stratum acres not used for expansion
                r.Value11 = s.CuttingUnits.Sum(scu => scu.Area);
                AccumulateVolume(r, s.Code, currAcres, "N", "", "");
                //  add stratum to output list
                listToOutput.Add(r);
            }   //  end foreach loop
            return;
        }   //  end AccumulateByStrata


        private List<ReportSubtotal> AccumulateBySpecies(List<TreeDO> treeList, List<LCDDO> speciesGroups)
        {
            //  load six lines into percent list for each species
            foreach (LCDDO lcd in speciesGroups)
            {
                for (int j = 0; j < 6; j++)
                {
                    ReportSubtotal pcl = new ReportSubtotal();
                    pcl.Value1 = lcd.Species;
                    int pcGrade = j + 1;
                    pcl.Value2 = pcGrade.ToString();
                    PerCentList.Add(pcl);
                }   //  end for j loop
            }
            //  loop by species groups
            foreach (LCDDO sg in speciesGroups)
            {
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = sg.Species;

                //  find all species in LCD
                List<LCDDO> currentSpecies = bslyr.getLCDOrdered("WHERE CutLeave = ? AND Species = ?", 
                                                        "", "C", sg.Species, "");
                foreach (LCDDO cs in currentSpecies)
                {
                    //  need acres for current stratum
                    List<StratumDO> sList = bslyr.GetCurrentStratum(cs.Stratum);
                    currAcres = Utilities.ReturnCorrectAcres(cs.Stratum, bslyr, (long)sList[0].Stratum_CN);
                    AccumulateExpFac(r, sList[0].Method, currentSpecies, cs, 2);
                    //  need all trees for the current species and stratum
                    AccumulateVolume(r, cs.Stratum, currAcres, cs.STM, cs.SampleGroup, cs.Species);
                }   //  end foreach loop
                listToOutput.Add(r);
            }   //  end foreach species group
            return PerCentList;
        }   //  end AccumulateBySpecies


        private void AccumulateByUnit(List<CuttingUnitDO> cutList)
        {
            //  Works for BLM03 and BLM04
            List<PRODO> proList = bslyr.getPRO();
            List<ReportSubtotal> strataSums = new List<ReportSubtotal>();
            List<ReportSubtotal> unitSums = new List<ReportSubtotal>();
            List<LCDDO> lcdList = bslyr.getLCD();
            string currMethod = "";
            //  fill output list with cutting units
            foreach (CuttingUnitDO cud in cutList)
            {
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = cud.Code;
                //  unit acres
                r.Value11 = cud.Area;
                //  which strata is this unit in?
                cud.Strata.Populate();
                strataSums.Clear();
                unitSums.Clear();
                //  accumulate volume
                foreach (StratumDO stratum in cud.Strata)
                {
                    List<StratumDO> currStratum = bslyr.GetCurrentStratum(stratum.Code);
                    //  strata acres for expansion
                    double currAcres = Utilities.ReturnCorrectAcres(currStratum[0].Code, bslyr, (long)currStratum[0].Stratum_CN);

                    if (currStratum[0].Method != "100")
                    {
                        currMethod = currStratum[0].Method;
                        //  need to accumulate by strata and sample group for proper proration
                        List<LCDDO> justStrata = LCDmethods.GetCutOrLeave(lcdList, "C", "", currStratum[0].Code, "");
                        foreach (LCDDO js in justStrata)
                        {
                            if (js.STM == "N")
                            {
                                //  just for non sure-to-measure
                                ReportSubtotal ss = new ReportSubtotal();
                                ss.Value1 = currStratum[0].Code;
                                ss.Value2 = js.SampleGroup;
                                //  Accumulate expansion factor, DBH squared  
                                ss.Value15 = js.SumDBHOBsqrd * currAcres;
                                ss.Value14 = js.SumExpanFactor;
                                ss.Value16 = js.SumExpanFactor * currAcres;
                                //  curracres needs to be 1.0 because area-based methods have proration factor set to acres.
                                currAcres = 1.0;
                                AccumulateUnitVolume(ss, currStratum[0].Code, "N", currAcres, js.Species);
                                strataSums.Add(ss);
                            }
                            else if (js.STM == "Y")
                            {
                                //  any stm = Y?
                                ReportSubtotal uu = new ReportSubtotal();
                                uu.Value1 = cud.Code;
                                uu.Value2 = js.SampleGroup;
                                AccumulateUnitVolume(uu, currStratum[0].Code, currAcres, cud.Code, (long)currStratum[0].Stratum_CN, (long)cud.CuttingUnit_CN, js.STM);
                                uu.Value16 = js.SumExpanFactor * currAcres;
                                uu.Value15 = js.SumDBHOBsqrd * currAcres;
                                //  for 3P methods find unit talled trees in pro list
                                int mthRow = proList.FindIndex(
                                    delegate(PRODO p)
                                    {
                                        return p.Stratum == currStratum[0].Code && p.CuttingUnit == cud.Code &&
                                            p.STM == "Y" && p.SampleGroup == js.SampleGroup;
                                    });
                                if (mthRow >= 0)
                                    uu.Value14 = proList[mthRow].TalliedTrees;
                                else uu.Value14 = js.SumExpanFactor;
                                unitSums.Add(uu);
                            }   //  endif
                        }   //  end foreach
                    }
                    else if (currStratum[0].Method == "100")
                    {
                        ReportSubtotal uu = new ReportSubtotal();
                        uu.Value1 = cud.Code;
                        AccumulateUnitVolume(uu, currStratum[0].Code, currAcres, cud.Code, (long)currStratum[0].Stratum_CN, (long)cud.CuttingUnit_CN, "N");
                        unitSums.Add(uu);
                    }   //  endif on method

                }   //  end for k loop


                //  prorate each list (strataSums and unitSums) into listToOutput
                if (strataSums.Count > 0)
                {
                        foreach (ReportSubtotal ss in strataSums)
                        {
                            //  find all records for sample group and unit
                            List<PRODO> justUnits = proList.FindAll(
                                delegate(PRODO p)
                                {
                                    return p.Stratum == ss.Value1 && p.CuttingUnit == r.Value1 &&
                                            p.CutLeave == "C" && p.STM == "N" &&
                                            p.SampleGroup == ss.Value2;
                                });
                            foreach (PRODO ju in justUnits)
                            {
                                r.Value3 += ss.Value3 * ju.ProrationFactor;
                                r.Value4 += ss.Value4 * ju.ProrationFactor;
                                r.Value5 += ss.Value5 * ju.ProrationFactor;
                                r.Value6 += ss.Value6 * ju.ProrationFactor;
                                r.Value7 += ss.Value7 * ju.ProrationFactor;
                                r.Value8 += ss.Value8 * ju.ProrationFactor;
                                r.Value10 += ss.Value10 * ju.ProrationFactor;
                                r.Value12 += ss.Value12 * ju.ProrationFactor;
                                r.Value13 += ss.Value13 * ju.ProrationFactor;
                                if (currMethod == "S3P" || currMethod == "3P")
                                    r.Value14 += ju.TalliedTrees;
                                else r.Value14 += ss.Value14 * ju.ProrationFactor;
                                r.Value15 += ss.Value15;
                                r.Value16 += ss.Value16;
                            }   //  end foreach loop on justUnits
                        }   //  end foreach loop  on strataSums
                }   //  endif strataSums has records
                //  repeat for unitSums which is 100%  and STM=Y and does not get prorated
                if (unitSums.Count > 0)
                {
                    foreach (ReportSubtotal un in unitSums)
                    {
                        r.Value3 += un.Value3;
                        r.Value4 += un.Value4;
                        r.Value5 += un.Value5;
                        r.Value6 += un.Value6;
                        r.Value7 += un.Value7;
                        r.Value8 += un.Value8;
                        r.Value10 += un.Value10;
                        r.Value12 += un.Value12;
                        r.Value13 += un.Value13;
                        r.Value14 += un.Value14;
                        r.Value15 += un.Value15;
                        r.Value16 += un.Value16;
                    }   //  end foreach loop on unitSums
                }   //  endif unitSums has records

                listToOutput.Add(r);
            }   //  end foreach loop on cutting unit

            return;
        }   //  end AccumulateByUnit



        private List<ReportSubtotal> AccumulateByUnitSpecies(CuttingUnitDO currUnit, List<LCDDO> speciesGroups)
        {
            //  BLM07 / BLM08
            List<ReportSubtotal> unitTotals = new List<ReportSubtotal>();
            List<ReportSubtotal> strataSums = new List<ReportSubtotal>();
            List<ReportSubtotal> unitSums = new List<ReportSubtotal>();
            List<LCDDO> lcdList = bslyr.getLCD();
            List<PRODO> proList = bslyr.getPROunit(currUnit.Code);
            List<LogStockDO> justLogs = new List<LogStockDO>();
            string currMethod = "";
            //  load species into list to output
            foreach (LCDDO sg in speciesGroups)
            {
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = currUnit.Code;
                r.Value2 = sg.Species;
                //  process current unit by strata and species groups
                currUnit.Strata.Populate();
                //  find all logs for each species in each stratum in speciesGroups
                strataSums.Clear();
                unitSums.Clear();
                foreach (StratumDO stratum in currUnit.Strata)
                {
                    //  Get current acres
                    List<StratumDO> currStratum = bslyr.GetCurrentStratum(stratum.Code);
                    currAcres = Utilities.ReturnCorrectAcres(currStratum[0].Code, bslyr, (long)currStratum[0].Stratum_CN);
                    currMethod = currStratum[0].Method;
                    //  need sample groups for current species to ensure proration factor is applied appropriately
                    List<LCDDO> justSpecies = LCDmethods.GetCutOrLeave(lcdList, "C", sg.Species, currStratum[0].Code, "");

                    if (currMethod != "100")
                    {
                        foreach (LCDDO js in justSpecies)
                        {
                            if (js.STM == "N")
                            {
                                //  process non-STM
                                ReportSubtotal ss = new ReportSubtotal();
                                ss.Value1 = js.Stratum;
                                ss.Value2 = js.SampleGroup;
                                //  curracres needs to be 1.0 because area-based methods have proration factor set to acres.
                                currAcres = 1.0;
                                AccumulateVolume(ss, js.Stratum, currAcres, "N", js.SampleGroup, js.Species);
                                strataSums.Add(ss);
                            }
                            else if (js.STM == "Y")
                            {
                                //  and any sure-to-meausre
                                ReportSubtotal uu = new ReportSubtotal();
                                uu.Value1 = currUnit.Code;
                                uu.Value2 = sg.SampleGroup;
                                //AccumulateUnitVolume(uu, js.Stratum, currAcres, currUnit.Code, (long)currStratum[0].Stratum_CN, (long)currUnit.CuttingUnit_CN, js.STM);
                                AccumulateVolume(uu, js.Stratum, currAcres, "N", js.SampleGroup, js.Species);
                                unitSums.Add(uu);
                            }
                        }   //  end foreach loop
                    }
                    else if (currMethod == "100")
                    {
                        foreach (LCDDO js in justSpecies)
                        {
                            ReportSubtotal uu = new ReportSubtotal();
                            uu.Value1 = currUnit.Code;
                            uu.Value2 = sg.SampleGroup;
                            uu.Value15 = sg.SumDBHOBsqrd * currAcres;
                            uu.Value16 = sg.SumExpanFactor * currAcres;
                            //AccumulateUnitVolume(uu, sg.Stratum, currAcres, currUnit.Code, (long)currStratum[0].Stratum_CN, (long)currUnit.CuttingUnit_CN, "N");
                            AccumulateVolume((long) currStratum[0].Stratum_CN, (long) currUnit.CuttingUnit_CN, uu, currAcres, js.Species);
                            unitSums.Add(uu);
                        }   //  endif
                    }   //  endif method

                
                //  sum up DBH squared and expansion factors
                //  find current stratum and species in LCD list to sum up for certain methods
                //  For S3P, 3P and STR, number of trees comes from PRO list
                    List<LCDDO> strataGroups = lcdList.FindAll(
                        delegate(LCDDO ld)
                        {
                            return ld.Species == sg.Species && ld.Stratum == currStratum[0].Code;
                        });
                    foreach (LCDDO str in strataGroups)
                    {
                        if (currMethod == "S3P" || currMethod == "3P" || currMethod == "STR")
                        {
                            //  find tallied trees in PRO list
                            int mthRow = proList.FindIndex(
                                delegate(PRODO p)
                                {
                                    return p.Stratum == currStratum[0].Code && p.SampleGroup == str.SampleGroup &&
                                            p.CuttingUnit == currUnit.Code && p.CutLeave == "C" &&
                                            p.STM == str.STM;
                                });
                            if (mthRow >= 0)
                            {
                                if (proList[mthRow].ProrationFactor > 0)
                                {
                                    r.Value15 += str.SumDBHOBsqrd * currAcres;
                                    r.Value16 += str.SumExpanFactor * currAcres;
                                }   //  endif
                                
                                    r.Value14 += proList[mthRow].TalliedTrees;
                            }   //  endif mthRow
                        }
                        else if(currMethod != "100")
                        {
                            //  find proration factor for group
                            int mthRow = proList.FindIndex(
                                delegate(PRODO p)
                                {
                                    return p.Stratum == str.Stratum && p.SampleGroup == str.SampleGroup &&
                                        p.CuttingUnit == currUnit.Code && p.CutLeave == "C" &&
                                        p.STM == str.STM;
                                });
                            if (mthRow >= 0)
                                r.Value14 += str.SumExpanFactor * proList[mthRow].ProrationFactor;
                            else r.Value14 += str.SumExpanFactor;
                            r.Value15 += str.SumDBHOBsqrd * currAcres;
                            r.Value16 += str.SumExpanFactor * currAcres;
                        }
                        else if(currMethod == "100")
                        {
                            List<TreeDO> tList = bslyr.getTrees();
                            List<TreeDO> justTrees = tList.FindAll(
                                delegate(TreeDO tt)
                                {
                                    return tt.Stratum.Code == str.Stratum && tt.CuttingUnit.Code == currUnit.Code &&
                                        tt.Species == str.Species && tt.SampleGroup.Code == str.SampleGroup;
                                });
                            r.Value14 += justTrees.Count; 
                            r.Value15 += str.SumDBHOBsqrd * currAcres;
                            r.Value16 += str.SumExpanFactor * currAcres;
                        }   //  endif on method
                    }   //  end foreach loop on strataGroups

                }   //  end foreach loop                

                //  now apply proration factor for the  unit
                if (strataSums.Count > 0)
                {
                        foreach (ReportSubtotal ss in strataSums)
                        {
                            //  find all proration records for sample group and unit
                            List<PRODO> justUnits = proList.FindAll(
                                delegate(PRODO p)
                                {
                                    return p.Stratum == ss.Value1 && p.CuttingUnit == r.Value1 && 
                                            p.CutLeave == "C" && p.STM == "N" && p.SampleGroup == ss.Value2;
                                });
                            foreach (PRODO ju in justUnits)
                            {
                                r.Value3 += ss.Value3 * ju.ProrationFactor;
                                r.Value4 += ss.Value4 * ju.ProrationFactor;
                                r.Value5 += ss.Value5 * ju.ProrationFactor;
                                r.Value6 += ss.Value6 * ju.ProrationFactor;
                                r.Value7 += ss.Value7 * ju.ProrationFactor;
                                r.Value8 += ss.Value8 * ju.ProrationFactor;
                                r.Value10 += ss.Value10 * ju.ProrationFactor;
                                r.Value12 += ss.Value12 * ju.ProrationFactor;
                                r.Value13 += ss.Value13 * ju.ProrationFactor;
                                //  if no trees were collected for the current sample group
                                //  don't sum DBH squared values
                                //  throws off quad mean calculation to have total strata
                                if (ju.ProrationFactor > 0)
                                {
                                    r.Value15 += ss.Value15;
                                    r.Value16 += ss.Value16;
                                }   //  endif

                            }   //  end foreach loop  on justUnits
                        }   //  end foreach loop on strataSums
                }   //  endif strataSums has records
                //  repeat for unitSums which is 100% or STM=Y and does not get prorated
                if (unitSums.Count > 0)
                {
                    foreach (ReportSubtotal un in unitSums)
                    {
                        r.Value3 += un.Value3;
                        r.Value4 += un.Value4;
                        r.Value5 += un.Value5;
                        r.Value6 += un.Value6;
                        r.Value7 += un.Value7;
                        r.Value8 += un.Value8;
                        r.Value10 += un.Value10;
                        r.Value12 += un.Value12;
                        r.Value13 += un.Value13;
                        r.Value15 += un.Value15;
                        r.Value16 += un.Value16;
                    }   //  end foreach loop on unitSums
                }   //  endif unitSums has records
                listToOutput.Add(r);
            }   //  end foreach loop on species groups in listToOutput
            //  update unit total
            unitTotals = updateUnitTotal();
            return unitTotals;
        }   //  end AccumulateByUnitSpecies


        private void AccumulateExpFac(ReportSubtotal currObj, string currMeth, List<LCDDO> currStrata,
                                        LCDDO currGrp, int whatToUse)
        {
            //  This accumulates expansion based on method for specific reports
            if(currMeth == "S3P" || currMeth == "3P")
            {
                switch (whatToUse)
                {
                    case 1:
                        currObj.Value14 = currStrata.Sum(c => c.TalliedTrees);
                        currObj.Value16 = currStrata.Sum(c => c.SumExpanFactor * currAcres);
                        break;
                    case 2:
                        currObj.Value14 += currGrp.TalliedTrees;
                        currObj.Value16 += currGrp.SumExpanFactor * currAcres;
                        break;
                }   //  end switch
            }
            else
            {
                switch (whatToUse)
                {
                    case 1:
                        currObj.Value14 = currStrata.Sum(c => c.SumExpanFactor * currAcres);
                        currObj.Value16 = currStrata.Sum(c => c.SumExpanFactor * currAcres);
                        break;
                    case 2:
                        currObj.Value14 += currGrp.SumExpanFactor * currAcres;
                        currObj.Value16 += currGrp.SumExpanFactor * currAcres;
                        break;
                }   //  end switch
            }   //  endif on method
            //  sum DBH squared for quad mean DBH
            switch (whatToUse)
            {
                case 1:
                    currObj.Value15 = currStrata.Sum(c => c.SumDBHOBsqrd) * currAcres;
                    break;
                case 2:
                    currObj.Value15 += currGrp.SumDBHOBsqrd * currAcres;
                    break;
            }   //  end switch
            return;
        }   //  end AccumulateExpFac


        private void AccumulateUnitVolume(ReportSubtotal currOBJ, string currST, string currSTM, double currAC, string currSP)
        {
            //  Works for BLM03 and BLM04 --  should work for any STM -- but doesnt't -- only STM=N
            string[] logGrades = new string[12] { "", " ","0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            //  loop by log grades
            for (int j = 0; j < 12; j++)
            {
                //justLogs = bslyr.getStrataLogs(currST, currSTM, logGrades[j], currOBJ.Value2, currSP);
                justLogs = bslyr.getStrataLogs(currSP, currST, currOBJ.Value2, currSTM, logGrades[j]);
                if (justLogs.Count > 0)
                {
                    SumVolume(currAC);
                    //  then sum by log grade
                    LoadLogGrades(currOBJ, logGrades[j], justLogs, currAC, "");
                }   //  endif
            }   //  end for j loop
            return;
        }   //  end AccumulateUnitVolume


        private void AccumulateUnitVolume(ReportSubtotal currOBJ, string currST, double cAcres, string currCU, long currST_CN, long currCU_CN, string currSTM)
        {
            //  Woroks for BLM03 and BLM04 100% method only and STM=Y
            string[] logGrades = new string[12] { "", " ", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            //  need all unit trees
            List<TreeDO> allTrees = bslyr.getUnitTrees(currST_CN, currCU_CN);
            //  sum needed values for logs in each tree except
            //  for expansion factor which is just from trees.
            List<TreeDO> justTrees = allTrees.FindAll(
                delegate(TreeDO t)
                {
                    return t.STM == currSTM;
                });
            if (currSTM == "N")
            {
                currOBJ.Value14 = justTrees.Sum(jt => jt.ExpansionFactor);            
                currOBJ.Value16 = justTrees.Sum(jt => jt.ExpansionFactor * cAcres);
                currOBJ.Value15 = justTrees.Sum(jt => jt.DBH * jt.DBH * cAcres);
            }   //  endif
            //  then sum logs by grade for each tree
            for (int j = 0; j < 12; j++)
            {
                justLogs = bslyr.getUnitLogs(currST_CN, currCU_CN, logGrades[j], currSTM);
                if (justLogs.Count > 0)
                {
                    SumVolume(cAcres);
                    //  the sum by log grade
                    LoadLogGrades(currOBJ, logGrades[j], justLogs, cAcres, "");
                }   //  endif
            }   //  end for j loop
            return;
        }   //  end AccumulateUnitVolume


        private void AccumulateVolume(ReportSubtotal currObj, string currST, double cAcres, string currSTM, 
                                        string currSG, string currSP)
        {
            //  Works for BLM reports -- not unit reports
            string[] logGrades = new string[12] { "", " ", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            //  loop by grade to sum volume
            for (int j = 0; j < 12; j++)
            {
                switch (currentReport)
                {
                    case "BLM01":       case "BLM02":
                        justLogs = bslyr.getStrataLogs(currST, logGrades[j]);
                        break;
                    case "BLM05":       case "BLM06":
                    case "BLM07":       case "BLM08":
                        justLogs = bslyr.getStrataLogs(currSP, currST, currSG, currSTM, logGrades[j]);
                        break;
                }   //  end switch on report

                if (justLogs.Count > 0)
                {
                    SumVolume(cAcres);
                    //  then sum by log grade  
                    LoadLogGrades(currObj, logGrades[j], justLogs, cAcres, currSP);
                }   //  endif

            }   //  end for j loop
            return;
        }   //  end AccumulateVolume


        private void AccumulateVolume(long currST_CN, long currCU_CN, ReportSubtotal currObj,
                                        double cAcres, string currSP)
        {
            //  Works just for BLM07/08 reports
            string[] logGrades = new string[12] { "", " ", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            //  loop by grade to sum volume
            for (int j = 0; j < 12; j++)
            {

                List<LogStockDO> unitLogs = bslyr.getUnitLogs(currST_CN, currCU_CN, logGrades[j], "N");
                //  find justLogs the currentDate species in the unit
                justLogs = unitLogs.FindAll(
                    delegate(LogStockDO ul)
                    {
                        return ul.Tree.Species == currSP;
                    });
                if (justLogs.Count > 0)
                {
                    SumVolume(cAcres);
                    //  then sum by log grade  
                    LoadLogGrades(currObj, logGrades[j], justLogs, cAcres, currSP);
                }   //  endif

            }   //  end for j loop
            return;
        }   //  end AccumulateVolume


        private void AccumulateBySpeciesDIB()
        {
            //  for BLM09 and BLM10
            string[] logGrades = new string[] { "", " ", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            double minDIB = 0;
            double maxDIB = 0;
            List<StratumDO> sList = bslyr.getStratum();
            //  Accumulate by DIB class and log grade
            foreach (ReportSubtotal lto in listToOutput)
            {

                if (Convert.ToDouble(lto.Value1) > 0)
                {
                    foreach (StratumDO s in sList)
                    {
                        //  get strata acres
                        currAcres = Utilities.ReturnCorrectAcres(s.Code, bslyr, (long)s.Stratum_CN);

                        //  a single decimal works to a certain extent
                        //  but because SSQLite adds decimals, some logs get dropped from the report
                        //  changed the addition or subtraction of decimals to four digits to see
                        //  if those logs get appropriately picked up
                        //  July 2014
                        minDIB = Convert.ToDouble(lto.Value1) - 0.5;
                        maxDIB = Convert.ToDouble(lto.Value1) + 0.5;
                        foreach (object lg in logGrades)
                        {
                            List<LogStockDO> justLogs = bslyr.getLogSpecies(currSP, (float)minDIB, (float)maxDIB,
                                                                            s.Code, lg.ToString());

                            if (justLogs.Count > 0)
                            {
                                //  capture volume based on report
                                switch (currentReport)
                                {
                                    case "BLM09":
                                        //  board foot
                                        //  load based on grade
                                        switch (lg.ToString())
                                        {
                                            case "0":       case " ":       case "":
                                                lto.Value3 += justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "1":
                                                lto.Value4 += justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "2":
                                                lto.Value5 += justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "3":
                                                lto.Value6 += justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "4":
                                                lto.Value7 += justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "5":
                                                lto.Value8 += justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "6":
                                                lto.Value9 += justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "7":
                                                lto.Value11 += justLogs.Sum(jl => jl.BoardFootRemoved * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "8":
                                                lto.Value12 += justLogs.Sum(jl => jl.BoardFootRemoved * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "9":
                                                lto.Value15 += justLogs.Sum(jl => jl.GrossBoardFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                        }   //  end switch on grade
                                        //  total gross includes all grades
                                        lto.Value16 += justLogs.Sum(jl => jl.GrossBoardFoot * jl.Tree.ExpansionFactor) * currAcres;
                                        //  total net is just grades 0 through 6
                                        if (lg.ToString() != "7" && lg.ToString() != "8" && lg.ToString() != "9")
                                            lto.Value10 += justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor) * currAcres;
                                        //  total gross merch
                                        if (lg.ToString() != "9")
                                            lto.Value14 += justLogs.Sum(jl => jl.BoardFootRemoved * jl.Tree.ExpansionFactor) * currAcres;
                                        break;
                                    case "BLM10":
                                        //  cubic foot
                                        //  load based on grade
                                        switch (lg.ToString())
                                        {
                                            case "0":
                                            case " ":
                                            case "":
                                                lto.Value3 += justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "1":
                                                lto.Value4 += justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "2":
                                                lto.Value5 += justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "3":
                                                lto.Value6 += justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "4":
                                                lto.Value7 += justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "5":
                                                lto.Value8 += justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "6":
                                                lto.Value9 += justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "7":
                                                lto.Value11 += justLogs.Sum(jl => jl.CubicFootRemoved * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "8":
                                                lto.Value12 += justLogs.Sum(jl => jl.CubicFootRemoved * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                            case "9":
                                                lto.Value15 += justLogs.Sum(jl => jl.GrossCubicFoot * jl.Tree.ExpansionFactor) * currAcres;
                                                break;
                                        }   //  end switch on grade
                                        //  total gross includes all grades
                                        lto.Value16 += justLogs.Sum(jl => jl.GrossCubicFoot * jl.Tree.ExpansionFactor) * currAcres;
                                        //  total net is just grades 0 through 6
                                        if (lg.ToString() != "7" && lg.ToString() != "8" && lg.ToString() != "9")
                                            lto.Value10 += justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor) * currAcres;
                                        //  total gross merch
                                        if (lg.ToString() != "9")
                                            lto.Value14 += justLogs.Sum(jl => jl.CubicFootRemoved * jl.Tree.ExpansionFactor) * currAcres;
                                        break;
                                }   //  end switch on report

                                //  Calculate defect 1-8 here
                                lto.Value13 = lto.Value14 - (lto.Value10 + lto.Value11 + lto.Value12);
                            }   //  end foreach loop on log grades
                        }   //  endif there are logs to process
                    }   //  end foreach loop on stratum
                }   //  endif DIB not negative
            }   //  end foreach loop on listToOutput
            return;
        }   //  end AccumulateBySpeciesDIB


        private void writeCurrentGroups(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  This works for BLM01, BLM02, BLM03, BLM04
            double calcValue = 0;
            //  build print array for each group in output list
            foreach (ReportSubtotal lto in listToOutput)
            {
                if (lto.Value14 > 0)
                {
                    prtFields.Clear();
                    prtFields.Add("");
                    //  output header if needed
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                        completeHeader, 13, ref pageNumb, "");
                    //  set first field based on current report
                    switch (currentReport)
                    {
                        case "BLM01":
                        case "BLM02":
                            // by stratum
                            prtFields.Add(lto.Value1.PadLeft(2, ' '));
                            break;
                        case "BLM03":
                        case "BLM04":
                            //  by unit
                            prtFields.Add(lto.Value1.PadLeft(3, ' '));
                            break;
                    }   // end switch
                    //  rest of line
                    //  Quad mean dbh
                    calcValue = CalculateQuadMean(lto.Value15, lto.Value16);
                    prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                    //  Avg GM log volume
                    calcValue = CalculateAvgGrossMerch(lto.Value5, lto.Value6, lto.Value8);
                    prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    //  Volumes
                    prtFields.Add(Utilities.FormatField(lto.Value4, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value5, "{0,10:F0}").ToString().PadLeft(10, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value3, "{0,10:F0}").ToString().PadLeft(10, ' '));
                    //  Recovery percent
                    calcValue = CommonEquations.CalculateRecoverySalvage(lto.Value4, lto.Value3);
                    prtFields.Add(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(4, ' '));
                    //  Salvage percent
                    calcValue = CommonEquations.CalculateRecoverySalvage(lto.Value7, lto.Value4);
                    prtFields.Add(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(4, ' '));
                    //  grade volumes
                    prtFields.Add(Utilities.FormatField(lto.Value12, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value13, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value6, "{0,9:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value10, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value14, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value11, "{0,6:F0}").ToString().PadLeft(6, ' '));
                    //  Average volume per acre
                    calcValue = CalculateAvgVolPerAcre(lto.Value4, lto.Value11);
                    prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));

                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  endif no value
            }   //  end foreach loop
            return;
        }   //  end writeCurrentGroups


        private void writeCurrentGroups(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh, 
                                            List<ReportSubtotal> PerCentList)
        {
            //  Works for BLM05 and BLM06
            double calcValue = 0;
            //  build the print array for each group in the output list
            foreach (ReportSubtotal lto in listToOutput)
            {
                prtFields.Clear();
                prtFields.Add("");
                //  output header if needed
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                        completeHeader, 13, ref pageNumb, "");
                //  set first field to species
                prtFields.Add(lto.Value1.PadLeft(6, ' '));
                //  rest of line
                calcValue = CalculateQuadMean(lto.Value15, lto.Value16);
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                //  Avg GM log volume
                calcValue = CalculateAvgGrossMerch(lto.Value5, lto.Value6, lto.Value8);
                prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));
                //  Volumes
                prtFields.Add(Utilities.FormatField(lto.Value4, "{0,8:F0}").ToString().PadLeft(8, ' '));
                prtFields.Add(Utilities.FormatField(lto.Value5, "{0,10:F0}").ToString().PadLeft(10, ' '));
                prtFields.Add(Utilities.FormatField(lto.Value3, "{0,10:F0}").ToString().PadLeft(10, ' '));
                //  Recovery percent
                calcValue = CommonEquations.CalculateRecoverySalvage(lto.Value4, lto.Value3);
                prtFields.Add(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(4, ' '));
                //  Salvage percent
                calcValue = CommonEquations.CalculateRecoverySalvage(lto.Value7, lto.Value4);
                prtFields.Add(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(4, ' '));
                //  grade volumes
                prtFields.Add(Utilities.FormatField(lto.Value12, "{0,8:F0}").ToString().PadLeft(8, ' '));
                prtFields.Add(Utilities.FormatField(lto.Value13, "{0,8:F0}").ToString().PadLeft(8, ' '));
                prtFields.Add(Utilities.FormatField(lto.Value6, "{0,8:F0}").ToString().PadLeft(8, ' '));
                prtFields.Add(Utilities.FormatField(lto.Value10, "{0,8:F0}").ToString().PadLeft(8,' '));
                prtFields.Add(Utilities.FormatField(lto.Value14, "{0,8:F0}").ToString().PadLeft(8,' '));
                //  percent list
                //  find all of current species in the percent list
                List<ReportSubtotal> currSpecies = PerCentList.FindAll(
                    delegate(ReportSubtotal cs)
                    {
                        return cs.Value1 == lto.Value1;
                    });
                double totalPercent = currSpecies.Sum(c => c.Value3);
                foreach (ReportSubtotal cs in currSpecies)
                {
                    if (totalPercent > 0)

                        calcValue = (cs.Value3 / totalPercent) * 100;
                    else calcValue = 0.0;
                    prtFields.Add(Utilities.FormatField(Math.Round(calcValue,0,MidpointRounding.AwayFromZero), "{0,3:F0}").ToString().PadLeft(3, ' '));
                }   //  end foreach loop
                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            return;
        }   //  end writeCurrentGroups

        
        private void writeCurrentGroups(List<ReportSubtotal> unitTotals,StreamWriter strWriteOut, 
                                                    ref int pageNumb, reportHeaders rh)
        {
            //  Works for BLM07 and BLM08
            double calcValue = 0;
            //  build print array for each group in output list
            foreach (ReportSubtotal lto in listToOutput)
            {
                if (lto.Value3 > 0)
                {
                    prtFields.Clear();
                    prtFields.Add("");
                    //  output header if needed
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                        completeHeader, 13, ref pageNumb, "");
                    prtFields.Add(lto.Value1.PadLeft(3, ' '));
                    prtFields.Add(lto.Value2.PadLeft(6, ' '));
                    //  rest of line
                    //  Quad mean dbh
                    calcValue = CalculateQuadMean(lto.Value15, lto.Value16);
                    prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                    //  Avg GM log volume
                    calcValue = CalculateAvgGrossMerch(lto.Value5, lto.Value6, lto.Value8);
                    prtFields.Add(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    //  Volumes
                    prtFields.Add(Utilities.FormatField(lto.Value4, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value5, "{0,10:F0}").ToString().PadLeft(10, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value3, "{0,10:F0}").ToString().PadLeft(10, ' '));
                    //  Recovery percent
                    calcValue = CommonEquations.CalculateRecoverySalvage(lto.Value4, lto.Value3);
                    prtFields.Add(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(4, ' '));
                    //  Salvage percent
                    calcValue = CommonEquations.CalculateRecoverySalvage(lto.Value7, lto.Value4);
                    prtFields.Add(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(4, ' '));
                    //  grade volumes
                    prtFields.Add(Utilities.FormatField(lto.Value12, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value13, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value6, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value10, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value14, "{0,8:F0}").ToString().PadLeft(8, ' '));

                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  endif gross volume is zero -- do not print
            }   //  end foreach loop

            //  output unit total line if unit has volume
            if(listToOutput.Sum(lto => lto.Value14) > 0)
                outputUnitTotal(listToOutput[0].Value1, unitTotals, strWriteOut, rh, ref pageNumb);
            return;
        }   //  end writeCurrentGroups


        private void writeCurrentGroups(reportHeaders rh, ref int pageNumb, StreamWriter strWriteOut)
        {
            //  for BLM09 and BLM10
            //  build print array for each class line in output list
            foreach (ReportSubtotal lto in listToOutput)
            {
                prtFields.Clear();
                prtFields.Add("");
                //  output headers if needed
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                    completeHeader, 14, ref pageNumb, extraLine);
                if (Convert.ToDouble(lto.Value1) > 0)
                {
                    prtFields.Add(lto.Value1.PadLeft(2, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value3, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value4, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value5, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value6, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value7, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value8, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value9, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value10, "{0,7:F0}").ToString().PadLeft(7, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value11, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value12, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value13, "{0,6:F0}").ToString().PadLeft(6, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value14, "{0,9:F0}").ToString().PadLeft(9, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value15, "{0,6:F0}").ToString().PadLeft(6, ' '));
                    prtFields.Add(Utilities.FormatField(lto.Value16, "{0,9:F0}").ToString().PadLeft(9, ' '));

                    printOneRecord(fieldLengths, prtFields, strWriteOut);
                }   //  endif DIB class is greater than zero
            }   //  end foreach loop
            return;
        }   //  end writeCurrentGroups


        private void outputUnitTotal(string currUnit, List<ReportSubtotal> unitTotal, 
                                    StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                completeHeader, 13, ref pageNumb, "");
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.Write("UNIT ");
            strWriteOut.Write(currUnit.PadLeft(3, ' '));
            strWriteOut.Write(" TOTALS  ");
            //  Quad mean dbh
            calcValue = CalculateQuadMean(unitTotal[0].Value15, unitTotal[0].Value16);
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
            //  Avg GM log volume
            calcValue = CalculateAvgGrossMerch(unitTotal[0].Value5, unitTotal[0].Value6, unitTotal[0].Value8);
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(9, ' '));
            //  Volumes
            strWriteOut.Write(Utilities.FormatField(unitTotal[0].Value4, "{0,8:F0}").ToString().PadLeft(10, ' '));
            strWriteOut.Write(Utilities.FormatField(unitTotal[0].Value5, "{0,10:F0}").ToString().PadLeft(11, ' '));
            strWriteOut.Write(Utilities.FormatField(unitTotal[0].Value3, "{0,10:F0}").ToString().PadLeft(11, ' '));
            //  Recovery percent
            calcValue = CommonEquations.CalculateRecoverySalvage(unitTotal[0].Value4, unitTotal[0].Value3);
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(5, ' '));
            //  Salvage percent
            calcValue = CommonEquations.CalculateRecoverySalvage(unitTotal[0].Value7, unitTotal[0].Value4);
            strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(5, ' '));
            //  grade volumes
            strWriteOut.Write(Utilities.FormatField(unitTotal[0].Value12, "{0,8:F0}").ToString().PadLeft(11, ' '));
            strWriteOut.Write(Utilities.FormatField(unitTotal[0].Value13, "{0,8:F0}").ToString().PadLeft(11, ' '));
            strWriteOut.Write(Utilities.FormatField(unitTotal[0].Value6, "{0,8:F0}").ToString().PadLeft(9, ' '));
            strWriteOut.Write(Utilities.FormatField(unitTotal[0].Value10, "{0,8:F0}").ToString().PadLeft(9, ' '));
            strWriteOut.WriteLine(Utilities.FormatField(unitTotal[0].Value14, "{0,8:F0}").ToString().PadLeft(9, ' '));
            numOlines++;
            strWriteOut.WriteLine();
            numOlines++;
            return;
        }   //  end outputUnitTotal


        private List<ReportSubtotal> updateUnitTotal()
        {
            List<ReportSubtotal> updatedTotal = new List<ReportSubtotal>();
            ReportSubtotal rs = new ReportSubtotal();
            rs.Value3 = listToOutput.Sum(l => l.Value3);
            rs.Value4 = listToOutput.Sum(l => l.Value4);
            rs.Value5 = listToOutput.Sum(l => l.Value5);
            rs.Value6 = listToOutput.Sum(l => l.Value6);
            rs.Value7 = listToOutput.Sum(l => l.Value7);
            rs.Value8 = listToOutput.Sum(l => l.Value8);
            rs.Value10 = listToOutput.Sum(l => l.Value10);
            rs.Value11 = listToOutput.Sum(l => l.Value11);
            rs.Value12 = listToOutput.Sum(l => l.Value12);
            rs.Value13 = listToOutput.Sum(l => l.Value13);
            rs.Value14 = listToOutput.Sum(l => l.Value14);
            rs.Value15 = listToOutput.Sum(l => l.Value15);
            rs.Value16 = listToOutput.Sum(l => l.Value16);

            updatedTotal.Add(rs);
            if (totalToOutput.Count == 0)
                totalToOutput.Add(rs);
            else
            {
                // add unit total to total to output
                totalToOutput[0].Value3 += rs.Value3;
                totalToOutput[0].Value4 += rs.Value4;
                totalToOutput[0].Value5 += rs.Value5;
                totalToOutput[0].Value6 += rs.Value6;
                totalToOutput[0].Value7 += rs.Value7;
                totalToOutput[0].Value8 += rs.Value8;
                totalToOutput[0].Value10 += rs.Value10;
                totalToOutput[0].Value11 += rs.Value11;
                totalToOutput[0].Value12 += rs.Value12;
                totalToOutput[0].Value13 += rs.Value13;
                totalToOutput[0].Value14 += rs.Value14;
                totalToOutput[0].Value15 += rs.Value15;
                totalToOutput[0].Value16 += rs.Value16;
            }   //  endif
            return updatedTotal;
        }   //  end updateUnitTotal


        private void updateTotalLine()
        {
            ReportSubtotal rs = new ReportSubtotal();
            rs.Value3 = listToOutput.Sum(l => l.Value3);
            rs.Value4 = listToOutput.Sum(l => l.Value4);
            rs.Value5 = listToOutput.Sum(l => l.Value5);
            rs.Value6 = listToOutput.Sum(l => l.Value6);
            rs.Value7 = listToOutput.Sum(l => l.Value7);
            rs.Value8 = listToOutput.Sum(l => l.Value8);
            rs.Value9 = listToOutput.Sum(l => l.Value9);
            rs.Value10 = listToOutput.Sum(l => l.Value10);
            rs.Value12 = listToOutput.Sum(l => l.Value12);
            rs.Value13 = listToOutput.Sum(l => l.Value13);
            rs.Value14 = listToOutput.Sum(l => l.Value14);
            rs.Value15 = listToOutput.Sum(l => l.Value15);
            rs.Value16 = listToOutput.Sum(l => l.Value16);
                
            //  need totalToOutput sum acres separately in case something was suppressed.
            foreach (ReportSubtotal lto in listToOutput)
            {
                if (lto.Value14 > 0)
                    rs.Value11 += lto.Value11;
            }   //  end foreach loop for acres
            totalToOutput.Add(rs);
            
            return;
        }   //  end updateTotalLine


        private void outputTotalLine(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                completeHeader, 13, ref pageNumb, "");
            strWriteOut.WriteLine(reportConstants.longLine);
            if (currentReport == "BLM07" || currentReport == "BLM08")
                strWriteOut.Write(" TOTALS-      ");
            else if (currentReport == "BLM09" || currentReport == "BLM10")
                strWriteOut.Write(" TOT ");
            else strWriteOut.Write(" TOTALS-");
            switch (currentReport)
            {
                case "BLM01":       case "BLM02":
                case "BLM03":       case "BLM04":
                    //  Quad Mean DBH
                    calcValue = CalculateQuadMean(totalToOutput[0].Value15, totalToOutput[0].Value16);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                    //  Avg GM log volume
                    calcValue = CalculateAvgGrossMerch(totalToOutput[0].Value5,
                                                        totalToOutput[0].Value6,
                                                        totalToOutput[0].Value8);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(10, ' '));
                    //  Volumes
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value4, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value5, "{0,10:F0}").ToString().PadLeft(11, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value3, "{0,10:F0}").ToString().PadLeft(11, ' '));
                    //  Recovery percent
                    calcValue = CommonEquations.CalculateRecoverySalvage(totalToOutput[0].Value4, totalToOutput[0].Value3);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(6, ' '));
                    //  Salvage percent
                    calcValue = CommonEquations.CalculateRecoverySalvage(totalToOutput[0].Value7, totalToOutput[0].Value4);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(7, ' '));
                    //  grade volumes
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value12, "{0,8:F0}").ToString().PadLeft(10, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value13, "{0,8:F0}").ToString().PadLeft(11, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value6, "{0,8:F0}").ToString().PadLeft(10, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value10, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value14, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value11, "{0,6:F0}").ToString().PadLeft(7, ' '));
                    //  Average volume per acre
                    calcValue = CalculateAvgVolPerAcre(totalToOutput[0].Value4, totalToOutput[0].Value11);
                    strWriteOut.WriteLine(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    break;
                case "BLM05":                case "BLM06":
                    //  Quad Mean DBH
                    calcValue = CalculateQuadMean(totalToOutput[0].Value15, totalToOutput[0].Value16);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                    //  Avg GM log volume
                    calcValue = CalculateAvgGrossMerch(totalToOutput[0].Value5,
                                                        totalToOutput[0].Value6,
                                                        totalToOutput[0].Value8);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    //  Volumes
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value4, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value5, "{0,10:F0}").ToString().PadLeft(11, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value3, "{0,10:F0}").ToString().PadLeft(11, ' '));
                    //  Recovery percent
                    calcValue = CommonEquations.CalculateRecoverySalvage(totalToOutput[0].Value4, totalToOutput[0].Value3);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(5, ' '));
                    //  Salvage percent
                    calcValue = CommonEquations.CalculateRecoverySalvage(totalToOutput[0].Value7, totalToOutput[0].Value4);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(5, ' '));
                    //  grade volumes
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value12, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value13, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value6, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value10, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value14, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    break;
                case "BLM07":                case "BLM08":
                //  Quad Mean DBH
                    calcValue = CalculateQuadMean(totalToOutput[0].Value15, totalToOutput[0].Value16);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(8, ' '));
                    //  Avg GM log volume
                    calcValue = CalculateAvgGrossMerch(totalToOutput[0].Value5,
                                                        totalToOutput[0].Value6,
                                                        totalToOutput[0].Value8);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    //  Volumes
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value4, "{0,8:F0}").ToString().PadLeft(10, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value5, "{0,10:F0}").ToString().PadLeft(11, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value3, "{0,10:F0}").ToString().PadLeft(11, ' '));
                    //  Recovery percent
                    calcValue = CommonEquations.CalculateRecoverySalvage(totalToOutput[0].Value4, totalToOutput[0].Value3);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(5, ' '));
                    //  Salvage percent
                    calcValue = CommonEquations.CalculateRecoverySalvage(totalToOutput[0].Value7, totalToOutput[0].Value4);
                    strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(5, ' '));
                    //  grade volumes
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value12, "{0,8:F0}").ToString().PadLeft(11, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value13, "{0,8:F0}").ToString().PadLeft(11, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value6, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value10, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value14, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    break;
                case "BLM09":               case "BLM10":
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value3, "{0,8:F0}").ToString().PadLeft(8, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value4, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value5, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value6, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value7, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value8, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value9, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value10, "{0,7:F0}").ToString().PadLeft(10, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value11, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value12, "{0,8:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value13, "{0,6:F0}").ToString().PadLeft(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value14, "{0,9:F0}").ToString().PadLeft(10, ' '));
                    strWriteOut.Write(Utilities.FormatField(totalToOutput[0].Value15, "{0,6:F0}").ToString().PadLeft(8, ' '));
                    strWriteOut.WriteLine(Utilities.FormatField(totalToOutput[0].Value16, "{0,9:F0}").ToString().PadLeft(10, ' '));
                    break;
            }   //  end switch
            return;
        }   //  end outputTotalLine


        private void SumVolume(double cAcres)
        {
            //  Sums appropriate volume for type of volume indicated
            if (justLogs.Count > 0)
            {
                justLive = justLogs.FindAll(
                    delegate(LogStockDO ls)
                    {
                        return ls.Tree.LiveDead == "L";
                    });
                justDead = justLogs.FindAll(
                    delegate(LogStockDO lsd)
                    {
                        return lsd.Tree.LiveDead == "D";
                    });
            }   //  end if
            //  first sum up based on volume type
            switch (volType)
            {
                case "BOARD":
                    currGRS = justLogs.Sum(jl => jl.GrossBoardFoot * jl.Tree.ExpansionFactor * cAcres);
                    currNET = justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor * cAcres);
                    if (justLive.Count > 0) currLIVE = justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor * cAcres);
                    if (justDead.Count > 0) currDEAD = justLogs.Sum(jl => jl.NetBoardFoot * jl.Tree.ExpansionFactor * cAcres);
                    currREM = justLogs.Sum(jl => jl.BoardFootRemoved * jl.Tree.ExpansionFactor * cAcres);
                    break;
                case "CUBIC":
                    currGRS = justLogs.Sum(jl => jl.GrossCubicFoot * jl.Tree.ExpansionFactor * cAcres);
                    currNET = justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor * cAcres);
                    if (justLive.Count > 0) currLIVE = justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor * cAcres);
                    if (justDead.Count > 0) currDEAD = justLogs.Sum(jl => jl.NetCubicFoot * jl.Tree.ExpansionFactor * cAcres);
                    currREM = justLogs.Sum(jl => jl.CubicFootRemoved * jl.Tree.ExpansionFactor * cAcres);
                    break;
            }   //  end switch on volume type
            return;
        }   //  end SumVolume


        private void LoadLogGrades(ReportSubtotal currOBJ, string currLogGrade, List<LogStockDO> justLogs, double cAcres, string currSP)
        {
            //  Loads current log grade into current object for report
            currOBJ.Value3 += currGRS;
            switch (currLogGrade)
            {
                case "0":       case "1":       case "2":       case "3":
                case "4":       case "5":       case "6":       case "":
                case " ":       case null:
                    currOBJ.Value5 += currREM;
                    //  sum live and dead separately
                    currOBJ.Value4 += currLIVE;
                    currOBJ.Value7 += currDEAD;
                    currOBJ.Value6 += justLogs.Sum(jl => jl.Tree.ExpansionFactor * cAcres);
                    break;
                case "7":
                    currOBJ.Value12 += currGRS;
                    currOBJ.Value5 += currREM;
                    currOBJ.Value10 += justLogs.Sum(jl => jl.Tree.ExpansionFactor * cAcres);
                    //  also need to sum 7 and 8 together
                    currOBJ.Value8 += justLogs.Sum(jl => jl.Tree.ExpansionFactor * cAcres);
                    break;
                case "8":
                    currOBJ.Value13 += currGRS;
                    currOBJ.Value5 += currREM;
                    currOBJ.Value10 += justLogs.Sum(jl => jl.Tree.ExpansionFactor * cAcres);
                    //  and need 7 and 8 summed together
                    currOBJ.Value8 += justLogs.Sum(jl => jl.Tree.ExpansionFactor * cAcres);
                    break;
                case "9":
                    currOBJ.Value10 += justLogs.Sum(jl => jl.Tree.ExpansionFactor * cAcres);
                    break;
            }   //  end switch on log grade

            //  accumulate percents for reports 5 and 6
            if (currentReport == "BLM05" || currentReport == "BLM06")
                //  load percent array
                LoadPerCentList(currNET, currLogGrade, currSP);
            return;
        }   //  end LoadLogGrades


        
        public double CalculateAvgGrossMerch(double denomInator, double numeRator1, double numeRator2)
        {
            if (numeRator1 > 0)
                return denomInator / (numeRator1 + numeRator2);
            else return 0;
        }   //  end CalculateAvgGrossMerch


        private double CalculateAvgVolPerAcre(double denomInator, double numeRator)
        {
            if (denomInator < 500.0)
                return -1;
            else if (numeRator > 0)
                return denomInator / numeRator;
            else return 0;
        }   //  end CalculateAvgVolPerAcre


        private void LoadPerCentList(double currNet, string logGrade, string currSP)
        {
            //  find species and log grade in PerCentList
            int nthRow = PerCentList.FindIndex(
                delegate(ReportSubtotal r)
                {
                    return r.Value1 == currSP && r.Value2 == logGrade;
                });
            if (nthRow >= 0)
                PerCentList[nthRow].Value3 += currNet;
            else PerCentList[0].Value3 += currNET;
            return;
        }   //  end LoadPerCentList


        private string[] createCompleteHeader()
        {
            string[] finnishHeader = new string[4];
            switch (currentReport)
            {
                case "BLM01":
                    //  board foot report
                    finnishHeader = columnHeaderI;
                    finnishHeader[3] = finnishHeader[3].Replace("XXXX", "BDFT");
                    break;
                case "BLM03":
                    //  board foot report
                    finnishHeader = columnHeaderI;
                    finnishHeader[2] = finnishHeader[2].Replace("STRATA", "UNIT  ");
                    finnishHeader[3] = finnishHeader[3].Replace("XXXX", "BDFT");
                    finnishHeader[3] = finnishHeader[3].Replace("TREES*", "TREES");
                    break;
                case "BLM05":
                    //  board foot report
                    finnishHeader = columnHeaderII;
                    finnishHeader[3] = finnishHeader[3].Replace("XXXX", "BDFT");
                    break;
                case "BLM07":
                    //  board foot report
                    finnishHeader = columnHeaderIII;
                    finnishHeader[3] = finnishHeader[3].Replace("XXXX", "BDFT");
                    break;
                case "BLM02":
                    //  cubic foot report
                    finnishHeader = columnHeaderI;
                    finnishHeader[3] = finnishHeader[3].Replace("XXXX", "CUFT");
                    break;
                case "BLM04":
                    // cubic foot report
                    finnishHeader = columnHeaderI;
                    finnishHeader[2] = finnishHeader[2].Replace("STRATA", "UNIT  ");
                    finnishHeader[3] = finnishHeader[3].Replace("XXXX", "CUFT");
                    finnishHeader[3] = finnishHeader[3].Replace("TREES*", "TREES");
                    break;
                case "BLM06":
                    //  cubic foot report
                    finnishHeader = columnHeaderII;
                    finnishHeader[3] = finnishHeader[3].Replace("XXXX", "CUFT");
                    break;
                case "BLM08":
                    //  cubic foot report
                    finnishHeader = columnHeaderIII;
                    finnishHeader[3] = finnishHeader[3].Replace("XXXX", "CUFT");
                    break;
                case "BLM09":
                case "BLM10":
                    //  log grade report
                    finnishHeader[0] = columnHeaderIV[0];
                    finnishHeader[1] = columnHeaderIV[1];
                    finnishHeader[2] = columnHeaderIV[2];
                    break;
            }   //  end switch
            return finnishHeader;
        }   //  end createCompleteHeader



        private void createCSVfile()
        {
            string CSVoutfile = System.IO.Path.ChangeExtension(fileName, currentReport);
            CSVoutfile += ".csv";
            OutputCSV oc = new OutputCSV();
            //  first need to load CSVlist with first few fields and then finish based on specific report
            List<CSVlist> CSVoutputList = new List<CSVlist>();
            switch(currentReport)
            {
                case "BLM01":       case "BLM02":
                case "BLM03":       case "BLM04":
                    foreach (ReportSubtotal lto in listToOutput)
                    {
                        CSVlist cv = new CSVlist();
                        cv.field1 = lto.Value1;
                        cv.field2 = CalculateQuadMean(lto.Value15, lto.Value16).ToString();
                        cv.field3 = CalculateAvgGrossMerch(lto.Value5, lto.Value6, lto.Value8).ToString();
                        cv.field4 = lto.Value4.ToString();
                        cv.field5 = lto.Value5.ToString();
                        cv.field6 = lto.Value3.ToString();
                        cv.field7 = CommonEquations.CalculateRecoverySalvage(lto.Value4, lto.Value3).ToString();
                        cv.field8 = CommonEquations.CalculateRecoverySalvage(lto.Value7, lto.Value4).ToString();
                        cv.field9 = lto.Value12.ToString();
                        cv.field10 = lto.Value13.ToString();
                        cv.field11 = lto.Value6.ToString();
                        cv.field12 = lto.Value10.ToString();
                        cv.field13 = lto.Value14.ToString();
                        cv.field14 = lto.Value11.ToString();
                        cv.field15 = CalculateAvgVolPerAcre(lto.Value4, lto.Value11).ToString();
                        CSVoutputList.Add(cv);
                    }   //  end foreach loop
                    writeCSV(CSVoutfile, CSVoutputList, 15);
                    break;
                case "BLM05":       case "BLM06":
                    foreach (ReportSubtotal lto in listToOutput)
                    {
                        CSVlist cv = new CSVlist();
                        cv.field1 = lto.Value1;
                        cv.field2 = CalculateQuadMean(lto.Value15, lto.Value16).ToString();
                        cv.field3 = CalculateAvgGrossMerch(lto.Value5, lto.Value6, lto.Value8).ToString();
                        cv.field4 = lto.Value4.ToString();
                        cv.field5 = lto.Value5.ToString();
                        cv.field6 = lto.Value3.ToString();
                        cv.field7 = CommonEquations.CalculateRecoverySalvage(lto.Value4, lto.Value3).ToString();
                        cv.field8 = CommonEquations.CalculateRecoverySalvage(lto.Value7, lto.Value8).ToString();
                        cv.field9 = lto.Value12.ToString();
                        cv.field10 = lto.Value13.ToString();
                        cv.field11 = lto.Value6.ToString();
                        cv.field12 = lto.Value10.ToString();
                        cv.field13 = lto.Value14.ToString();
                    
                        //  find all of current species in the percent list
                        List<ReportSubtotal> currSpecies = PerCentList.FindAll(
                            delegate(ReportSubtotal cs)
                            {
                                return cs.Value1 == lto.Value1;
                            });
                        double totalPercent = currSpecies.Sum(c => c.Value3);
                        if (totalPercent > 0)
                        {
                            cv.field14 = Math.Round(((currSpecies[0].Value3 / totalPercent) * 100), 0, MidpointRounding.AwayFromZero).ToString();
                            cv.field15 = Math.Round(((currSpecies[1].Value3 / totalPercent) * 100), 0, MidpointRounding.AwayFromZero).ToString();
                            cv.field16 = Math.Round(((currSpecies[2].Value3 / totalPercent) * 100), 0, MidpointRounding.AwayFromZero).ToString();
                            cv.field17 = Math.Round(((currSpecies[3].Value3 / totalPercent) * 100), 0, MidpointRounding.AwayFromZero).ToString();
                            cv.field18 = Math.Round(((currSpecies[4].Value3 / totalPercent) * 100), 0, MidpointRounding.AwayFromZero).ToString();
                            cv.field19 = Math.Round(((currSpecies[5].Value3 / totalPercent) * 100), 0, MidpointRounding.AwayFromZero).ToString();
                            CSVoutputList.Add(cv);
                        }   //  endif
                    }   //  end foreach loop
                    writeCSV(CSVoutfile, CSVoutputList, 19);
                    break;
            }   //  end switch on currentReport
            return;
        }   //  end createCSVfile


        private void createCSVfile(List<CSVlist> CSV7and8)
        {
            // does CSV file for just BLM07 and BLM08
            string CSVoutfile = System.IO.Path.ChangeExtension(fileName, currentReport);
            CSVoutfile += ".csv";
            OutputCSV oc = new OutputCSV();
            //  first need to load CSVlist with first few fields and then finish based on specific report
            List<CSVlist> CSVoutputList = new List<CSVlist>();
            foreach (CSVlist c in CSV7and8)
            {
                writeCSV(CSVoutfile, CSV7and8, 14);
            }   //  end foreach loop
            return;
        }   //  end createCSVfile


        private void capture7and8CSV(List<ReportSubtotal> listToOutput,List<CSVlist> CSV7and8)
        {
            foreach (ReportSubtotal lto in listToOutput)
            {
                CSVlist cv = new CSVlist();
                cv.field1 = lto.Value1;
                cv.field2 = lto.Value2;
                cv.field3 = CalculateQuadMean(lto.Value15, lto.Value16).ToString();
                cv.field4 = CalculateAvgGrossMerch(lto.Value5, lto.Value6, lto.Value8).ToString();
                cv.field5 = lto.Value4.ToString();
                cv.field6 = lto.Value5.ToString();
                cv.field7 = lto.Value3.ToString();
                cv.field8 = CommonEquations.CalculateRecoverySalvage(lto.Value4, lto.Value3).ToString();
                cv.field9 = CommonEquations.CalculateRecoverySalvage(lto.Value7, lto.Value4).ToString();
                cv.field10 = lto.Value12.ToString();
                cv.field11 = lto.Value13.ToString();
                cv.field12 = lto.Value6.ToString();
                cv.field13 = lto.Value10.ToString();
                cv.field14 = lto.Value14.ToString();
                CSV7and8.Add(cv);
            }   //  end foreach loop
        }   //  end capture7and8CSV

        private void writeCSV(string outputFileName, List<CSVlist> ListToOutput, int numOfields)
        {
            StringBuilder sb = new StringBuilder();
            string deLimiter = ",";
            using (StreamWriter strCSVout = new StreamWriter(outputFileName))
            {
                foreach (CSVlist lto in ListToOutput)
                {
                    for (int k = 0; k <= numOfields; k++)
                    {
                        switch (k)
                        {
                            case 0:
                                sb.Append(lto.field1);
                                break;
                            case 1:
                                sb.Append(lto.field2);
                                break;
                            case 2:
                                sb.Append(lto.field3);
                                break;
                            case 3:
                                sb.Append(lto.field4);
                                break;
                            case 4:
                                sb.Append(lto.field5);
                                break;
                            case 5:
                                sb.Append(lto.field6);
                                break;
                            case 6:
                                sb.Append(lto.field7);
                                break;
                            case 7:
                                sb.Append(lto.field8);
                                break;
                            case 8:
                                sb.Append(lto.field9);
                                break;
                            case 9:
                                sb.Append(lto.field10);
                                break;
                            case 10:
                                sb.Append(lto.field11);
                                break;
                            case 11:
                                sb.Append(lto.field12);
                                break;
                            case 12:
                                sb.Append(lto.field13);
                                break;
                            case 13:
                                sb.Append(lto.field14);
                                break;
                            case 14:
                                sb.Append(lto.field15);
                                break;
                            case 15:
                                sb.Append(lto.field16);
                                break;
                            case 16:
                                sb.Append(lto.field17);
                                break;
                            case 17:
                                sb.Append(lto.field18);
                                break;
                            case 18:
                                sb.Append(lto.field19);
                                break;
                            case 19:
                                sb.Append(lto.field20);
                                break;
                            case 20:
                                sb.Append(lto.field21);
                                break;
                            case 21:
                                sb.Append(lto.field22);
                                break;
                            case 22:
                                sb.Append(lto.field23);
                                break;
                            case 23:
                                sb.Append(lto.field24);
                                break;
                            case 24:
                                sb.Append(lto.field25);
                                break;
                            case 25:
                                sb.Append(lto.field26);
                                break;
                            case 26:
                                sb.Append(lto.field27);
                                break;
                            case 27:
                                sb.Append(lto.field28);
                                break;
                            case 28:
                                sb.Append(lto.field29);
                                break;
                            case 29:
                                sb.Append(lto.field30);
                                break;
                            case 30:
                                sb.Append(lto.field31);
                                break;
                            case 31:
                                sb.Append(lto.field32);
                                break;
                            case 32:
                                sb.Append(lto.field33);
                                break;
                            case 33:
                                sb.Append(lto.field34);
                                break;
                            case 34:
                                sb.Append(lto.field35);
                                break;
                            case 35:
                                sb.Append(lto.field36);
                                break;
                            case 36:
                                sb.Append(lto.field37);
                                break;
                            case 37:
                                sb.Append(lto.field38);
                                break;
                            case 38:
                                sb.Append(lto.field39);
                                break;
                            case 39:
                                sb.Append(lto.field40);
                                break;
                            case 40:
                                sb.Append(lto.field41);
                                break;
                            case 41:
                                sb.Append(lto.field42);
                                break;
                            case 42:
                                sb.Append(lto.field43);
                                break;
                            case 43:
                                sb.Append(lto.field44);
                                break;
                            case 44:
                                sb.Append(lto.field45);
                                break;
                            case 45:
                                sb.Append(lto.field46);
                                break;
                        }   //  end switch
                        if (k != numOfields) sb.Append(deLimiter);
                    }   //  end for k loop
                    strCSVout.WriteLine(sb.ToString());
                    sb.Clear();
                }   //  end foreach loop
                strCSVout.Close();
            }   //  end using
            return;
        }   //  end writeCSV

    }   //  end class OutputBLM
}
