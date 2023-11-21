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
    class OutputTreeGrade : CreateTextFile
    {
        public string currRept;
        private int[] fieldLengths;
        private List<string> prtFields;
        private StringBuilder sb = new StringBuilder();
        private int firstLine = 0;
        private List<ReportSubtotal> productSubTotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> saleTotal = new List<ReportSubtotal>();
        private List<ReportSubtotal> eachLine = new List<ReportSubtotal>();

        public OutputTreeGrade(CPbusinessLayer dataLayer) : base(dataLayer)
        {
        }

        public void CreateTreeGradeReports(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            string currentTitle = fillReportTitle(currRept);

            numOlines = 0;
            //  need data from LCD; cut trees only; ordered by primary product, species and tree grade
            List<LCDDO> lcdList = LCDmethods.GetCutGroupedBy("", "", 8, DataLayer);

            string volType = "";
            double volSum = 0;
            //  make sure there's data for the current report
            if (currRept == "A11")
            {
                volType = "BDFT";
                volSum = lcdList.Sum(l => l.SumNBDFT);
            }
            else if (currRept == "A12")
            {
                volType = "CUFT";
                volSum = lcdList.Sum(l => l.SumNCUFT);
            }   //  endif
            if (volSum == 0)
            {
                sb.Clear();
                sb.Append(">> No ");
                sb.Append(volType);
                sb.Append(" volume for report ");
                noDataForReport(strWriteOut,currRept,sb.ToString());
                return;
            }   //  endif volSum

            //  use a ReportSubtotal list for individual lines
            string prevSpecies = "*";
            string prevProduct = "*";
            //  loop twice to get the two separate pages
            for (int k = 0; k < 2; k++)
            {
                //  need complete report title
                if (k == 1)
                {
                    currentTitle = currentTitle.Substring(0, 32);
                    currentTitle += " ESTIMATED NUMBER OF TREES";
                    eachLine.Clear();
                    productSubTotal.Clear();
                    saleTotal.Clear();
                    prevSpecies = "*";
                    prevProduct = "*";
                    numOlines = 0;
                }   //  endif page 2
                rh.createReportTitle(currentTitle, 4, 32, 0, reportConstants.FCTO_PPO, "");


                //  Loop through LCD list and accumulate lines
                foreach (LCDDO lcd in lcdList)
                {
                    WriteReportHeading(strWriteOut, rh.reportTitles[0], rh.reportTitles[1], rh.reportTitles[2],
                                    rh.A11A12columns, 7, ref pageNumb, "");
                    //  need a new line or subtotal?
                    if (prevSpecies == "*" && prevProduct == "*")
                    {
                        prevSpecies = lcd.Species;
                        prevProduct = lcd.PrimaryProduct;
                        firstLine = 1;
                    }
                    else if (prevSpecies != lcd.Species)
                    {
                        //  output individual line
                        WriteCurrentGroup(strWriteOut, rh, ref pageNumb, 1, eachLine);
                        //  update product subtotal
                        UpdateTotals(eachLine, productSubTotal, 1);
                        //  update sale total
                        UpdateTotals(eachLine, saleTotal, 2);
                        //  empty eachLine list and save current species
                        eachLine.Clear();
                        prevSpecies = lcd.Species;
                    }
                    else if (prevProduct != lcd.PrimaryProduct)
                    {
                        //  time for a subtotal line
                        WriteCurrentGroup(strWriteOut, rh, ref pageNumb, 1, eachLine);
                        //  Update product subtotal
                        UpdateTotals(eachLine, productSubTotal, 1);
                        //  Update sale total
                        UpdateTotals(eachLine, saleTotal, 2);
                        //  Output product subtotal
                        WriteCurrentGroup(strWriteOut, rh, ref pageNumb, 2, productSubTotal);
                        //  Clear product subtotal
                        productSubTotal.Clear();
                        //  Clear each line
                        eachLine.Clear();
                        //  save species and product
                        prevProduct = lcd.PrimaryProduct;
                        prevSpecies = lcd.Species;
                    }   //  endif

                    //  Load eachLine with correct volume
                    LoadEachLine(volType, ref firstLine, lcd, k);
                }   //  end foreach loop

                //  Output last species group
                WriteCurrentGroup(strWriteOut, rh, ref pageNumb, 1, eachLine);
                //  Update product subtotal
                UpdateTotals(eachLine, productSubTotal, 1);
                //  Output product total
                WriteCurrentGroup(strWriteOut, rh, ref pageNumb, 2, productSubTotal);
                //  Update sale total
                UpdateTotals(eachLine, saleTotal, 2);
                //  Output sale total
                WriteCurrentGroup(strWriteOut, rh, ref pageNumb, 3, saleTotal);
            }   //  end for k loop

            return;
        }   //  end CreateTreeGradeReports

        private void LoadEachLine(string volType, ref int firstLine, LCDDO lcd, int whichPage)
        {
            ReportSubtotal ea = new ReportSubtotal();
            int nthRow = 0;
            //  is this the first line?
            if (firstLine == 1)
            {
                ea.Value1 = lcd.PrimaryProduct;
                ea.Value2 = lcd.Species;
                eachLine.Add(ea);
                nthRow = 0;
                firstLine = 0;
            }
            else
            {
                //  is this species in the list?
                nthRow = eachLine.FindIndex(
                    delegate(ReportSubtotal rs)
                    {
                        return rs.Value2 == lcd.Species;
                    });
                if (nthRow == -1)
                {
                    // not there; add it
                    if (firstLine == 0) ea.Value1 = "";
                    ea.Value2 = lcd.Species;
                    if (eachLine.Count == 0)
                        nthRow = 0;
                    else nthRow = eachLine.Count + 1;
                    eachLine.Add(ea);
                }   //  endif nthRow
            }   //  endif firstLine

            //  update tree grade fields with appropriate volume
            double currentValue = 0;
            if (whichPage == 1)
                currentValue = lcd.SumExpanFactor;
            else if (volType == "BDFT")
                currentValue = lcd.SumNBDFT;
            else if (volType == "CUFT")
                currentValue = lcd.SumNCUFT;
            //  also need strata acres to expand volume
            List<StratumDO> sList = DataLayer.getStratum();
            long currStrCN = StratumMethods.GetStratumCN(lcd.Stratum, sList);
            double currAcres = Utilities.ReturnCorrectAcres(lcd.Stratum, DataLayer, currStrCN);
            switch (lcd.TreeGrade)
            {
                case "0":
                    eachLine[nthRow].Value3 += currentValue * currAcres;
                    break;
                case "1":
                    eachLine[nthRow].Value4 += currentValue * currAcres;
                    break;
                case "2":
                    eachLine[nthRow].Value5 += currentValue * currAcres;
                    break;
                case "3":
                    eachLine[nthRow].Value6 += currentValue * currAcres;
                    break;
                case "4":
                    eachLine[nthRow].Value7 += currentValue * currAcres;
                    break;
                case "5":
                    eachLine[nthRow].Value8 += currentValue * currAcres;
                    break;
                case "6":
                    eachLine[nthRow].Value9 += currentValue * currAcres;
                    break;
                case "7":
                    eachLine[nthRow].Value10 += currentValue * currAcres;
                    break;
                case "8":
                    eachLine[nthRow].Value11 += currentValue * currAcres;
                    break;
                case "9":
                    eachLine[nthRow].Value12 += currentValue * currAcres;
                    break;
                case " ":
                case "":
                case null:
                    eachLine[nthRow].Value13 += currentValue * currAcres;
                    break;
            }   //  end switch
            //  Add volume to line total
            eachLine[nthRow].Value14 += currentValue * currAcres;
            return;
        }   //  end LoadEachLine

        private void WriteCurrentGroup(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb, 
                                        int whichLine, List<ReportSubtotal> listToPrint)
        {
            fieldLengths = new int[] { 1, 3, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 11, 8 };
            prtFields = new List<string>();

            foreach (ReportSubtotal rs in listToPrint)
            {
                prtFields.Clear();
                prtFields.Add("");
                switch (whichLine)
                {
                    case 1:     //  individual line
                        prtFields.Add(rs.Value1.PadLeft(2, ' '));
                        prtFields.Add(rs.Value2.PadRight(6, ' '));
                        break;
                    case 2:     //  product total line
                        strWriteOut.Write("             ");
                        strWriteOut.WriteLine(reportConstants.subtotalLine1);
                        prtFields.Add(rs.Value1.PadLeft(2,' '));
                        prtFields.Add(rs.Value2.PadRight(6,' '));
                        break;
                    case 3:     //  sale total line
                        strWriteOut.WriteLine(reportConstants.longLine);
                        strWriteOut.WriteLine("    SALE");
                        prtFields.Add(rs.Value1);
                        prtFields.Add(rs.Value2);
                        break;
                }   //  end switch

                //  add grade volumes 
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value3));
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value4));
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value5));
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value6));
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value7));
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value8));
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value9));
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value10));
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value11));
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value12));
                prtFields.Add(String.Format("{0,7:F0}".ToString().PadLeft(7, ' '), rs.Value13));
                prtFields.Add(String.Format("{0,8:F0}".ToString().PadLeft(8, ' '), rs.Value14));

                printOneRecord(fieldLengths, prtFields, strWriteOut);

                if (whichLine == 2 || whichLine == 3)
                    strWriteOut.WriteLine(reportConstants.longLine);
            }   //  end foreach loop
            return;
        }   //  end WriteCurrentGroup    


        private void UpdateTotals(List<ReportSubtotal> eachLine, List<ReportSubtotal> listToUpdate, int whichTotal)
        {
            //  update appropriate total
            switch (whichTotal)
            {
                case 1:     //  product total
                    //  is product already recorded in a list
                    if (listToUpdate.Count == 0)
                    {
                        ReportSubtotal rs = new ReportSubtotal();
                        rs.Value1 = eachLine[0].Value1;
                        rs.Value2 = "TOTAL";
                        listToUpdate.Add(rs);
                    }   //  endif
                    break;
                case 2:     //  sale total
                    //  is sale total already setup?
                    if (listToUpdate.Count == 0)
                    {
                        ReportSubtotal rs = new ReportSubtotal();
                        rs.Value1 = "  ";
                        rs.Value2 = "TOTAL ";
                        listToUpdate.Add(rs);
                    }   //  endif
                    break;
            }   //  end switch

            foreach (ReportSubtotal rs in eachLine)
            {
                listToUpdate[0].Value3 += rs.Value3;
                listToUpdate[0].Value4 += rs.Value4;
                listToUpdate[0].Value5 += rs.Value5;
                listToUpdate[0].Value6 += rs.Value6;
                listToUpdate[0].Value7 += rs.Value7;
                listToUpdate[0].Value8 += rs.Value8;
                listToUpdate[0].Value9 += rs.Value9;
                listToUpdate[0].Value10 += rs.Value10;
                listToUpdate[0].Value11 += rs.Value11;
                listToUpdate[0].Value12 += rs.Value12;
                listToUpdate[0].Value13 += rs.Value13;
                listToUpdate[0].Value14 += rs.Value14;
            }   //  end foreach loop
            return;
        }   //  end UpdateTotals

    }

}
