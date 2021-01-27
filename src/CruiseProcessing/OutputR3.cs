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
    public class OutputR3 : CreateTextFile
    {
        #region
        public string currentReport;
        private int[] fieldLengths;
        private ArrayList prtFields = new ArrayList();
        private List<RegionalReports> listToOutput = new List<RegionalReports>();
        private List<ReportSubtotal> totalToOutput = new List<ReportSubtotal>();
        private regionalReportHeaders rRH = new regionalReportHeaders();
        private double totalSaleAcres = 0;
        #endregion

        public void CreateR3Reports(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            // fill report title array
            string currentTitle = fillReportTitle(currentReport);
            rh.createReportTitle(currentTitle, 6, 0, 0, "", "");
            fieldLengths = new int[] { 1, 7, 3, 6, 8, 9, 7, 10, 8, 9, 9, 8, 7, 8, 7 };
            //  pull groups from LCD
            List<LCDDO> justGroups = bslyr.getLCDOrdered("WHERE CutLeave = @p1 GROUP BY ", "ContractSpecies,Species", "C", "");
            //  loop by group and sum values
            string currCS = "**";
            List<CuttingUnitDO> cList = bslyr.getCuttingUnits();
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
                    WriteCurrentGroup(strWriteOut, ref pageNumb, rh);
                    if (listToOutput.Count > 1)
                    {
                        //  update total line
                        updateTotalLine();
                        outputTotalLine(strWriteOut, ref pageNumb, rh);
                        totalToOutput.Clear();
                    }   //  endif
                    listToOutput.Clear();
                }   //  endif
                AccumulateValues(jg);
            }   //  end foreach loop
            //  output last group
            WriteCurrentGroup(strWriteOut, ref pageNumb, rh);
            if (listToOutput.Count > 1)
            {
                //  update total
                updateTotalLine();
                //  output total line
                outputTotalLine(strWriteOut, ref pageNumb, rh);
            }   //  endif
            return;
        }   //  end CreateR3Reports


        private void AccumulateValues(LCDDO jg)
        {
            //  Accumulate values for R301
            double currSTacres = 0;
            List<StratumDO> sList = bslyr.getStratum(); 
            RegionalReports rr = new RegionalReports();
            rr.value1 = jg.Species;
            rr.value2 = jg.PrimaryProduct;
            if (jg.ContractSpecies == null)
                rr.value3 = " ";
            else rr.value3 = jg.ContractSpecies;
            List<LCDDO> lcdList = bslyr.GetLCDdata("WHERE CutLeave = @p1 AND Species = @p2 AND PrimaryProduct = @p3 AND ContractSpecies = @p4", jg, 5, "");
            //  sum up values needed
            foreach (LCDDO l in lcdList)
            {
                //  find current stratum to get acres to multiply
                int nthRow = sList.FindIndex(
                    delegate(StratumDO s)
                    {
                        return s.Code == l.Stratum;
                    });
                if (nthRow >= 0)
                    currSTacres = Utilities.ReturnCorrectAcres(l.Stratum, bslyr,(long) sList[nthRow].Stratum_CN);
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


        private void WriteCurrentGroup(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  writes current contract species group for R301
            double calcValue = 0;
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                rRH.R301columns, 10, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(lto.value1.PadRight(6, ' '));
                prtFields.Add(lto.value2.PadLeft(2, ' '));
                prtFields.Add(lto.value3.PadRight(4, ' '));
                //  gross and net ccf
                prtFields.Add(Utilities.FormatField(lto.value8 / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));
                prtFields.Add(Utilities.FormatField(lto.value10 / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));
                //  CCF defect
                if (lto.value8 > 0 && lto.value2 == "01")
                    calcValue = ((lto.value8 - lto.value10) / lto.value8) * 100;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                //  total trees
                prtFields.Add(Utilities.FormatField(lto.value13, "{0,7:F0}").ToString().PadLeft(7, ' '));
                //  average gross CF per tree
                if (lto.value13 > 0)
                    calcValue = lto.value8 / lto.value13;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(4, ' '));
                //  average gross CF per acres
                if (totalSaleAcres > 0)
                    calcValue = lto.value8 / totalSaleAcres;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,6:F0}").ToString().PadLeft(6, ' '));
                //  Quad mean DBH
                if (lto.value13 > 0)
                    calcValue = Math.Sqrt(lto.value12 / lto.value13);
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(5, ' '));
                //  board foot cubic foot ratio
                if (lto.value8 > 0)
                    calcValue = (lto.value7 / lto.value8) / 10;
                else calcValue = 0.0;
                prtFields.Add(Utilities.FormatField(calcValue, "{0,5:F4}").ToString().Replace("0.",".").PadLeft(5, ' '));
                //  MBF values
                prtFields.Add(Utilities.FormatField(lto.value7 / 1000, "{0,6:F0}").ToString().PadLeft(6, ' '));
                prtFields.Add(Utilities.FormatField(lto.value9 / 1000, "{0,6:F0}").ToString().PadLeft(6, ' '));
                //  CCF topwood
                prtFields.Add(Utilities.FormatField(lto.value11 / 100, "{0,6:F0}").ToString().PadLeft(6, ' '));

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
            else if(totalToOutput.Count == 0)
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


        private void outputTotalLine(StreamWriter strWriteOut, ref int pageNumb, reportHeaders rh)
        {
            //  R301
            double calcValue = 0;
            WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2], 
                                rRH.R301columns, 10, ref pageNumb, "");
            strWriteOut.WriteLine(rRH.R3specialLine1);
            foreach (ReportSubtotal t in totalToOutput)
            {
                strWriteOut.Write("    TOTAL- ");
                strWriteOut.Write(t.Value1.PadRight(6, ' '));
                strWriteOut.Write(Utilities.FormatField(t.Value8 / 100, "{0,7:F0}").ToString().PadLeft(7, ' '));
                strWriteOut.Write(Utilities.FormatField(t.Value10 / 100, "{0,7:F0}").ToString().PadLeft(8, ' '));
                //  CCF defect
                if (t.Value8 > 0)
                    calcValue = ((t.Value8 - t.Value10) / t.Value8) * 100;
                else calcValue = 0;
                strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F1}").ToString().PadLeft(7, ' '));
                //  total trees
                strWriteOut.Write(Utilities.FormatField(t.Value13, "{0,7:F0}").ToString().PadLeft(9, ' '));
                //  average gross CF per tree
                if (t.Value13 > 0)
                    calcValue = t.Value8 / t.Value13;
                else calcValue = 0;
                strWriteOut.Write(Utilities.FormatField(calcValue, "{0,4:F0}").ToString().PadLeft(7, ' '));
                //  average gross CF per acre
                if (totalSaleAcres > 0)
                    calcValue = t.Value8 / totalSaleAcres;
                else calcValue = 0;
                strWriteOut.Write(Utilities.FormatField(calcValue, "{0,6:F0}").ToString().PadLeft(10, ' '));
                //  quad mean DBH
                if (t.Value13 > 0)
                    calcValue = Math.Sqrt(t.Value12 / t.Value13);
                else calcValue = 0;
                strWriteOut.Write(Utilities.FormatField(calcValue, "{0,8:F1}").ToString().PadLeft(8, ' '));
                //  Board foot cubic foot ratio
                if (t.Value8 > 0)
                    calcValue = (t.Value7 / t.Value8) / 10;
                else calcValue = 0;
                strWriteOut.Write(Utilities.FormatField(calcValue, "{0,5:F4}").ToString().Replace("0.",".").PadLeft(9, ' '));
                //  MBF values
                strWriteOut.Write(Utilities.FormatField(t.Value7 / 1000, "{0,6:F0}").ToString().PadLeft(9, ' '));
                strWriteOut.Write(Utilities.FormatField(t.Value9 / 1000, "{0,6:F0}").ToString().PadLeft(7, ' '));
                //  CCF topwood
                strWriteOut.WriteLine(Utilities.FormatField(t.Value11 / 100, "{0,6:F0}").ToString().PadLeft(8, ' '));
            }   //  end foreach loop
            strWriteOut.WriteLine("");
            strWriteOut.WriteLine("");
            return;
        }   //  end outputTotalLine

    }
}
