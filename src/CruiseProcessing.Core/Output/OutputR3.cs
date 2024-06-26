using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Output;
using CruiseProcessing.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CruiseProcessing
{
    public class OutputR3 : OutputFileReportGeneratorBase
    {

        //  Region 3 reports
        //  R301 report
        private readonly string[] R301columns = new string[3] {"                                                     AVE      AVE    QUAD",
                                                     "           CONT    *********CCF********    TOTAL    GROSS    GR0SS   MEAN    RATIO    *****MBF****     CCF",
                                                     " SPEC   PD SPEC    GROSS     NET    DEF    TREES  CF/TREE  CF/ACRE    DBH   MBF:CCF   GROSS    NET     TOP"};
        private readonly string R3specialLine1 = "                    ---------------------------------------------------------------------------------------";
        //  per Karen Jones -- drop the summary at the bottom of the R301 report -- January 2014
        //public string R3specialLine2 = "                   ----------------------------------------------------";
        //public string[] R301total = new string[5] {" ******* PP SAWTIMBER QUALITY ADJUSTMENT AND VALUE INFORMATION ********",
        //                                           "                                       $/CCF                   LUMBER",
        //                                           "         CONT               PCT     TEA#- 22004    RATIO      RECOVERY",
        //                                           " SPEC PD SPEC      QMDBH   GRADE    QUALITY-ADJ   MBF:CCF      FACTOR",
        //                                           " ----------------------------------------------------------------------"};

        private int[] fieldLengths;
        private List<string> prtFields = new List<string>();
        private List<RegionalReports> listToOutput = new List<RegionalReports>();
        private List<ReportSubtotal> totalToOutput = new List<ReportSubtotal>();
        private double totalSaleAcres = 0;

        public OutputR3(CpDataLayer dataLayer, HeaderFieldData headerData, string reportID) : base(dataLayer, headerData, reportID)
        {
        }

        public void CreateR3Reports(TextWriter strWriteOut, ref int pageNumb)
        {
            // fill report title array
            string currentTitle = fillReportTitle(currentReport);
            SetReportTitles(currentTitle, 6, 0, 0, "", "");
            fieldLengths = new int[] { 1, 7, 3, 6, 8, 9, 7, 10, 8, 9, 9, 8, 7, 8, 7 };
            //  pull groups from LCD
            List<LCDDO> justGroups = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 GROUP BY ", "ContractSpecies,Species", "C", "");
            //  loop by group and sum values
            string currCS = "**";
            List<CuttingUnitDO> cList = DataLayer.getCuttingUnits();
            totalSaleAcres = cList.Sum(c => c.Area);
            foreach (LCDDO jg in justGroups)
            {
                if (currCS == "**")
                    currCS = jg.ContractSpecies;
                else if (currCS != jg.ContractSpecies)
                {
                    //  output contract species group
                    currCS = jg.ContractSpecies;
                    //  output current contract species
                    WriteCurrentGroup(strWriteOut, ref pageNumb);
                    if (listToOutput.Count > 1)
                    {
                        //  update total line
                        updateTotalLine();
                        outputTotalLine(strWriteOut, ref pageNumb);
                        totalToOutput.Clear();
                    }   //  endif
                    listToOutput.Clear();
                }   //  endif
                AccumulateValues(jg);
            }   //  end foreach loop
            //  output last group
            WriteCurrentGroup(strWriteOut, ref pageNumb);
            if (listToOutput.Count > 1)
            {
                //  update total
                updateTotalLine();
                //  output total line
                outputTotalLine(strWriteOut, ref pageNumb);
            }   //  endif
            return;
        }   //  end CreateR3Reports

        private void AccumulateValues(LCDDO jg)
        {
            //  Accumulate values for R301
            double currSTacres = 0;
            List<StratumDO> sList = DataLayer.GetStrata();
            RegionalReports rr = new RegionalReports();
            rr.value1 = jg.Species;
            rr.value2 = jg.PrimaryProduct;
            if (jg.ContractSpecies == null)
                rr.value3 = " ";
            else rr.value3 = jg.ContractSpecies;
            List<LCDDO> lcdList = DataLayer.GetLCDdata("WHERE CutLeave = @p1 AND Species = @p2 AND PrimaryProduct = @p3 AND ContractSpecies = @p4", jg, 5, "");
            //  sum up values needed
            foreach (LCDDO l in lcdList)
            {
                //  find current stratum to get acres to multiply
                int nthRow = sList.FindIndex(
                    delegate (StratumDO s)
                    {
                        return s.Code == l.Stratum;
                    });
                if (nthRow >= 0)
                    currSTacres = Utilities.ReturnCorrectAcres(l.Stratum, DataLayer, (long)sList[nthRow].Stratum_CN);
                else currSTacres = 0;

                rr.value7 += l.SumGBDFT * currSTacres;
                rr.value8 += l.SumGCUFT * currSTacres;
                rr.value9 += l.SumNBDFT * currSTacres;
                rr.value10 += l.SumNCUFT * currSTacres;
                rr.value11 += l.SumNCUFTtop * currSTacres;
                rr.value12 += l.SumDBHOBsqrd * currSTacres;
                rr.value13 += l.SumExpanFactor * currSTacres;
            }   //  end foreach loop
            listToOutput.Add(rr);
            return;
        }   //  end AccumulateValues

        private void WriteCurrentGroup(TextWriter strWriteOut, ref int pageNumb)
        {
            //  writes current contract species group for R301
            double calcValue = 0;
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                R301columns, 10, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(lto.value1.PadRight(6, ' '));
                prtFields.Add(lto.value2.PadLeft(2, ' '));
                prtFields.Add(lto.value3.PadRight(4, ' '));
                //  gross and net ccf
                prtFields.Add(String.Format("{0,7:F0}", lto.value8 / 100).PadLeft(7, ' '));
                prtFields.Add(String.Format("{0,7:F0}", lto.value10 / 100).PadLeft(7, ' '));
                //  CCF defect
                if (lto.value8 > 0 && lto.value2 == "01")
                    calcValue = ((lto.value8 - lto.value10) / lto.value8) * 100;
                else calcValue = 0.0;
                prtFields.Add(String.Format("{0,5:F1}", calcValue).PadLeft(5, ' '));
                //  total trees
                prtFields.Add(String.Format("{0,7:F0}", lto.value13).PadLeft(7, ' '));
                //  average gross CF per tree
                if (lto.value13 > 0)
                    calcValue = lto.value8 / lto.value13;
                else calcValue = 0.0;
                prtFields.Add(String.Format("{0,4:F0}", calcValue).PadLeft(4, ' '));
                //  average gross CF per acres
                if (totalSaleAcres > 0)
                    calcValue = lto.value8 / totalSaleAcres;
                else calcValue = 0.0;
                prtFields.Add(String.Format("{0,6:F0}", calcValue).PadLeft(6, ' '));
                //  Quad mean DBH
                if (lto.value13 > 0)
                    calcValue = Math.Sqrt(lto.value12 / lto.value13);
                else calcValue = 0.0;
                prtFields.Add(String.Format("{0,5:F1}", calcValue).PadLeft(5, ' '));
                //  board foot cubic foot ratio
                if (lto.value8 > 0)
                    calcValue = (lto.value7 / lto.value8) / 10;
                else calcValue = 0.0;
                prtFields.Add(String.Format("{0,5:F4}", calcValue).Replace("0.", ".").PadLeft(5, ' '));
                //  MBF values
                prtFields.Add(String.Format("{0,6:F0}", lto.value7 / 1000).PadLeft(6, ' '));
                prtFields.Add(String.Format("{0,6:F0}", lto.value9 / 1000).PadLeft(6, ' '));
                //  CCF topwood
                prtFields.Add(String.Format("{0,6:F0}", lto.value11 / 100).PadLeft(6, ' '));

                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            if (listToOutput.Count == 1)
            {
                //  Write two lines between contract species groups
                strWriteOut.WriteLine("");
                strWriteOut.WriteLine("");
            }   //  endif
            return;
        }   //  end WriteCurrentGroup

        private void updateTotalLine()
        {
            //  R301
            if (totalToOutput.Count > 0)
            {
                totalToOutput[0].Value7 = listToOutput.Sum(l => l.value7);
                totalToOutput[0].Value8 = listToOutput.Sum(l => l.value8);
                totalToOutput[0].Value9 = listToOutput.Sum(l => l.value9);
                totalToOutput[0].Value10 = listToOutput.Sum(l => l.value10);
                totalToOutput[0].Value11 = listToOutput.Sum(l => l.value11);
                totalToOutput[0].Value12 = listToOutput.Sum(l => l.value12);
                totalToOutput[0].Value13 = listToOutput.Sum(l => l.value12);
            }
            else if (totalToOutput.Count == 0)
            {
                ReportSubtotal r = new ReportSubtotal();
                r.Value1 = listToOutput[0].value3;
                r.Value7 = listToOutput.Sum(l => l.value7);
                r.Value8 = listToOutput.Sum(l => l.value8);
                r.Value9 = listToOutput.Sum(l => l.value9);
                r.Value10 = listToOutput.Sum(l => l.value10);
                r.Value11 = listToOutput.Sum(l => l.value11);
                r.Value12 = listToOutput.Sum(l => l.value12);
                r.Value13 = listToOutput.Sum(l => l.value13);
                totalToOutput.Add(r);
            }   //  endif
        }   //  end updateTotalLine

        private void outputTotalLine(TextWriter strWriteOut, ref int pageNumb)
        {
            //  R301
            double calcValue = 0;
            WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                R301columns, 10, ref pageNumb, "");
            strWriteOut.WriteLine(R3specialLine1);
            foreach (ReportSubtotal t in totalToOutput)
            {
                strWriteOut.Write("    TOTAL- ");
                strWriteOut.Write(t.Value1.PadRight(6, ' '));
                strWriteOut.Write(String.Format("{0,7:F0}", t.Value8 / 100).PadLeft(7, ' '));
                strWriteOut.Write(String.Format("{0,7:F0}", t.Value10 / 100).PadLeft(8, ' '));
                //  CCF defect
                if (t.Value8 > 0)
                    calcValue = ((t.Value8 - t.Value10) / t.Value8) * 100;
                else calcValue = 0;
                strWriteOut.Write(String.Format("{0,5:F1}", calcValue).PadLeft(7, ' '));
                //  total trees
                strWriteOut.Write(String.Format("{0,7:F0}", t.Value13).PadLeft(9, ' '));
                //  average gross CF per tree
                if (t.Value13 > 0)
                    calcValue = t.Value8 / t.Value13;
                else calcValue = 0;
                strWriteOut.Write(String.Format("{0,4:F0}", calcValue).PadLeft(7, ' '));
                //  average gross CF per acre
                if (totalSaleAcres > 0)
                    calcValue = t.Value8 / totalSaleAcres;
                else calcValue = 0;
                strWriteOut.Write(String.Format("{0,6:F0}", calcValue).PadLeft(10, ' '));
                //  quad mean DBH
                if (t.Value13 > 0)
                    calcValue = Math.Sqrt(t.Value12 / t.Value13);
                else calcValue = 0;
                strWriteOut.Write(String.Format("{0,8:F1}", calcValue).PadLeft(8, ' '));
                //  Board foot cubic foot ratio
                if (t.Value8 > 0)
                    calcValue = (t.Value7 / t.Value8) / 10;
                else calcValue = 0;
                strWriteOut.Write(String.Format("{0,5:F4}", calcValue).Replace("0.", ".").PadLeft(9, ' '));
                //  MBF values
                strWriteOut.Write(String.Format("{0,6:F0}", t.Value7 / 1000).PadLeft(9, ' '));
                strWriteOut.Write(String.Format("{0,6:F0}", t.Value9 / 1000).PadLeft(7, ' '));
                //  CCF topwood
                strWriteOut.WriteLine(String.Format("{0,6:F0}", t.Value11 / 100).PadLeft(8, ' '));
            }   //  end foreach loop
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine("");
            return;
        }   //  end outputTotalLine
    }
}