﻿using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Output;
using CruiseProcessing.OutputModels;
using CruiseProcessing.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CruiseProcessing
{
    public class OutputR5 : OutputFileReportGeneratorBase
    {

        //  Region 5 reports
        //  R501 report
        //  Replace X's, Z's and T's with appropriate data
        public static readonly string[] R501columns = new string[6] {" TABLE XX -  REPORT FOR SPECIES: ZZZZZZ",
                                                     "                       PRODUCT: TT",
                                                     " ",
                                                     "     LOG             #",
                                                     "     DIB            OF            GROSS             NET           GROSS             NET",
                                                     "   CLASS          LOGS             BDFT            BDFT            CUFT            CUFT"};

        private int[] fieldLengths;
        private List<string> prtFields = new List<string>();
        private List<RegionalReports> listToOutput = new List<RegionalReports>();
        private List<ReportSubtotal> totalToOutput = new List<ReportSubtotal>();
        private string[] completeHeader = new string[6];
        // TODO this should be pretty easy to remove as class field
        private int tableCounter = 0;

        public OutputR5(CpDataLayer dataLayer, HeaderFieldData headerData, string reportID) : base(dataLayer, headerData, reportID)
        {
        }

        public void CreateR5report(TextWriter strWriteOut, ref int pageNumb)
        {
            numOlines = 0;
            //  fill report title array
            string currentTitle = fillReportTitle(currentReport);
            SetReportTitles(currentTitle, 6, 0, 0, reportConstants.FCTO, "");
            fieldLengths = new int[] { 2, 12, 14, 16, 16, 16, 11 };
            //  This report prints a page for each species/product combination and log DIB classes
            //  pull species/product combinations from LCD to ultimately pull log data
            List<LCDDO> justGroups = DataLayer.getLCDOrdered("WHERE CutLeave = @p1 ", "GROUP BY Species,PrimaryProduct", "C", "");
            foreach (LCDDO jg in justGroups)
            {
                //  get DIB classes for this group
                List<LogStockDO> justDIBs = DataLayer.getCutLogs("C", jg.Species, jg.PrimaryProduct, 1);
                //  Load DIBs into output list
                LoadLogDIBclasses(listToOutput, justDIBs);

                //  create complete header for this group
                tableCounter++;
                completeHeader = createCompleteHeader(jg.Species, jg.PrimaryProduct);
                //  load values into output list
                List<LogStockDO> justLogs = DataLayer.getCutLogs("C", jg.Species, jg.PrimaryProduct, 0);
                AccumulateVolumes(justLogs);
                //  output group
                if (listToOutput.Count > 0)
                    WriteCurrentGroup(strWriteOut, ref pageNumb);
            }   //  end foreach loop
            return;
        }   //  end CreateR5report

        private void AccumulateVolumes(List<LogStockDO> justLogs)
        {
            string currST = "*";
            double currSTacres = 0;
            foreach (LogStockDO jl in justLogs)
            {
                //  pull stratum for correct acres
                if (currST != jl.Tree.Stratum.Code)
                {
                    currSTacres = Utilities.ReturnCorrectAcres(jl.Tree.Stratum.Code, DataLayer, (long)jl.Tree.Stratum_CN);
                    currST = jl.Tree.Stratum.Code;
                }   //  endif

                //  find DIB class to update
                int nthRow = listToOutput.FindIndex(
                    delegate (RegionalReports r)
                    {
                        return r.value7 == jl.DIBClass;
                    });
                if (nthRow < 0) nthRow = 0;
                listToOutput[nthRow].value8 += jl.Tree.ExpansionFactor * currSTacres;
                listToOutput[nthRow].value9 += jl.GrossBoardFoot * jl.Tree.ExpansionFactor * currSTacres;
                listToOutput[nthRow].value10 += jl.NetBoardFoot * jl.Tree.ExpansionFactor * currSTacres;
                listToOutput[nthRow].value11 += jl.GrossCubicFoot * jl.Tree.ExpansionFactor * currSTacres;
                listToOutput[nthRow].value12 += jl.NetCubicFoot * jl.Tree.ExpansionFactor * currSTacres;
            }   //  end foreach loop
            return;
        }   //  end AccumulateVolumes

        private void WriteCurrentGroup(TextWriter strWriteOut, ref int pageNumb)
        {
            foreach (RegionalReports lto in listToOutput)
            {
                WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                    completeHeader, 12, ref pageNumb, "");
                prtFields.Clear();
                prtFields.Add("");
                prtFields.Add(String.Format("{0,6:D0}", (int)(lto.value7)).PadLeft(6, ' '));
                prtFields.Add(String.Format("{0,8:F1}", lto.value8).PadLeft(8, ' '));
                prtFields.Add(String.Format("{0,11:F1}", lto.value9).PadLeft(11, ' '));
                prtFields.Add(String.Format("{0,11:F1}", lto.value10).PadLeft(11, ' '));
                prtFields.Add(String.Format("{0,11:F1}", lto.value11).PadLeft(11, ' '));
                prtFields.Add(String.Format("{0,11:F1}", lto.value12).PadLeft(11, ' '));

                printOneRecord(fieldLengths, prtFields, strWriteOut);
            }   //  end foreach loop
            //  update total
            updateTotals();
            //  output total line
            strWriteOut.WriteLine(reportConstants.longLine);
            strWriteOut.Write("  TOTALS      ");
            strWriteOut.Write(String.Format("{0,8:F1}", totalToOutput[0].Value8).PadLeft(8, ' '));
            strWriteOut.Write(String.Format("{0,11:F1}", totalToOutput[0].Value9).PadLeft(17, ' '));
            strWriteOut.Write(String.Format("{0,11:F1}", totalToOutput[0].Value10).PadLeft(16, ' '));
            strWriteOut.Write(String.Format("{0,11:F1}", totalToOutput[0].Value11).PadLeft(16, ' '));
            strWriteOut.WriteLine(String.Format("{0,11:F1}", totalToOutput[0].Value12).PadLeft(16, ' '));

            //  empty output list for next group
            listToOutput.Clear();
            totalToOutput.Clear();
            numOlines = 0;
            return;
        }   //  end WriteCurrentGroup

        private void updateTotals()
        {
            ReportSubtotal r = new ReportSubtotal();
            r.Value8 = listToOutput.Sum(l => l.value8);
            r.Value9 = listToOutput.Sum(l => l.value9);
            r.Value10 = listToOutput.Sum(l => l.value10);
            r.Value11 = listToOutput.Sum(l => l.value11);
            r.Value12 = listToOutput.Sum(l => l.value12);
            totalToOutput.Add(r);
            return;
        }   //  end updateTotals

        private string[] createCompleteHeader(string currSP, string currPP)
        {
            string[] finnishHeader = new string[6];
            finnishHeader[0] = R501columns[0];
            finnishHeader[0] = finnishHeader[0].Replace("XX", tableCounter.ToString());
            finnishHeader[0] = finnishHeader[0].Replace("ZZZZZZ", currSP.PadRight(6, ' '));
            finnishHeader[1] = R501columns[1];
            finnishHeader[1] = finnishHeader[1].Replace("TT", currPP);
            finnishHeader[2] = R501columns[2];
            finnishHeader[3] = R501columns[3];
            finnishHeader[4] = R501columns[4];
            finnishHeader[5] = R501columns[5];

            return finnishHeader;
        }   //  end createCompleteHeader
    }
}