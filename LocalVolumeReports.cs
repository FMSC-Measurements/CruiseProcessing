using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    class LocalVolumeReports
    {
        #region
        private ArrayList DBHclass = new ArrayList();
        private string[] colsPageOne = new string[4];
        private string[] colsPageTwo = new string[2];
        private string dashes = "----";
        private string currSale;
        private string currSaleName;
        private string currDate;
        private StringBuilder ReportTitle = new StringBuilder();
        public CPbusinessLayer bslyr = new CPbusinessLayer();
        #endregion
        public int OutputLocalVolume(string fileName)
        {
            //  first, does text file exists?  Can' write local volume if it hasn't been created
            string outFile = System.IO.Path.ChangeExtension(fileName, "out");
            if(!File.Exists(outFile))
            {
                MessageBox.Show("Local Volume Reports are added to the text output file.\nThat file does not exist.  Please create the output file and rerun Local Volume.",
                                "WARNING",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return -1;
            }   //  endif file doesn't exists

            //  Pull regression results table
            List<RegressionDO> resultsList = bslyr.getRegressionResults();

            if (resultsList.Count == 0)
            {
                MessageBox.Show("No regression results.\nCannot produce reports.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }   //  endif
            // Initialize report title
            ReportTitle.Append("LOCAL VOLUME TABLE REPORT - ");
            //  need sale info to complete the heading
            List<SaleDO> sList = bslyr.getSale();
            currSale = sList[0].SaleNumber;
            currSaleName = sList[0].Name;
            currDate = DateTime.Now.ToString();

            
            // first primary
            List<RegressionDO> justPrimary = resultsList.FindAll(
                delegate(RegressionDO rr)
                {
                    return rr.rVolType == "Primary";
                });
            //  create page one
            writePageOne(outFile, justPrimary);
            // separate secondary list
            List<RegressionDO> justSecondary = resultsList.FindAll(
                delegate(RegressionDO rr)
                {
                    return rr.rVolType == "Secondary";
                });

            if (justSecondary.Count > 0)
            {
                ReportTitle.Replace("Primary", "Secondary");
                writePageOne(outFile, justPrimary, justSecondary);
            }   //  endif secondary

            //  create page two -- primary
            ReportTitle.Replace("Secondary", "Primary");
            writePageTwo(outFile, justPrimary, 1);
            if (justSecondary.Count > 0)
            {
                //  and secondary
                writePageTwo(outFile, justSecondary, 2);
            }   //  endif secondary

            return 0;
        }   //  end OutputLocalVolume


        private int howManyPages(List<RegressionDO> justGroups, ref int[] lastGroups, ref int[] fieldLengths)
        {
            int numOpages = 0;
            int numOplaces = 132;
            int numOgroups = 0;
            int nthCol = 1;
            fieldLengths[0] = 8;

            if (justGroups.Count <= 10)
            {
                numOpages = 1;
                numOgroups = justGroups.Count;
                lastGroups[numOgroups - 1] = 1;
                //  load field lengths
                foreach(RegressionDO jg in justGroups)
                {
                    //  when only one species group is regresssed and the spcies
                    //  code is less than 6, need to padd it up to six and add the 3 spaces
                    if(jg.rSpeices.Length < 6)
                        fieldLengths[nthCol] = (jg.rSpeices.PadLeft(6,' ').Length) + 3;
                    else fieldLengths[nthCol] = jg.rSpeices.Length + 3;
                    nthCol++;
                }   //  end foreach loop
            }
            else
            {
                foreach (RegressionDO jg in justGroups)
                {
                    numOplaces -= jg.rSpeices.Length + 3;
                    numOgroups++;
                    fieldLengths[nthCol] = jg.rSpeices.Length + 3;
                    nthCol++;
                    if (numOplaces < jg.rSpeices.Length + 3)
                    {
                        numOpages++;
                        numOplaces = 132;
                        lastGroups[numOgroups - 1] = 1;
                    }   //  endif
                }   //  end foreach loop
            }   //  endif number of groups
            if (numOpages == 0) numOpages = 1;
            if (numOplaces < 132) numOpages++;
            return numOpages;
        }   //  end howManyPages


        private void writePageOne(string outFile, List<RegressionDO> justGroups)
        {
            //  primary product
            //  how many pages needed?  based on number of species groups plus length of species string
            int[] lastGroups = new int[justGroups.Count + 1]; ;
            int[] fieldLengths = new int[justGroups.Count + 1];
            int numOpages = howManyPages(justGroups, ref lastGroups, ref fieldLengths);
            StringBuilder prtFields = new StringBuilder();
            int firstGOP = 0;
            int lastGOP = 0;
            using (StreamWriter strWriteOut = new StreamWriter(outFile, true))
            {
                for (int j = 1; j <= numOpages; j++)
                {
                    //  find last group on page here
                    for (int k = 0; k < lastGroups.Count(); k++)
                    {
                        if (lastGroups[k] == 1)
                        {
                            firstGOP = lastGOP;
                            lastGOP = k;
                            lastGroups[k] = 0;
                            break;
                        }   //  endif
                    }   //  end for k loop
                    if (j == numOpages && numOpages > 1)
                    {
                        firstGOP = lastGOP + 1;
                        lastGOP = justGroups.Count - 1;
                    }   //  endif

                    // fill DBH class
                    DBHclass.Clear();
                    createDBHclass(justGroups);
                    //  create page header
                    createHeader(1, justGroups, firstGOP, lastGOP, "", "", "");
                    //  output header information
                    ReportTitle.Append(justGroups[0].rVolume + " Primary");
                    int numOlines = 0;
                    if (numOlines == 0)
                        outputHeader(strWriteOut, 1, numOlines);
                    //  load predicted volumes
                    List<PageOne> PageOneTable = LoadPageOne(justGroups, firstGOP, lastGOP);
                    prtFields.Remove(0, prtFields.Length);
                    //  output table
                    foreach (PageOne pot in PageOneTable)
                    {
                        prtFields = buildPrintLine(fieldLengths, pot);
                        strWriteOut.WriteLine(prtFields);
                        numOlines++;
                    }   //  end foreach loop
                    //  clear print table for next page
                    PageOneTable.Clear();
                }   //  end for j loop
                strWriteOut.Close();
            }   //  end using
            return;
        }   // end writePageOne


        private List<PageOne> LoadPageOne(List<RegressionDO> justGroups, int firstGOP, int lastGOP)
        {
            //  need to loop through DBH class to get the predicted volume or dashes on the line
            List<PageOne> LoadedTable = new List<PageOne>();
            LoadedTable.Clear();
            //  load DBHclass first
            for (int k = 0; k < DBHclass.Count; k++)
            {
                PageOne po = new PageOne();
                po.value1 = Convert.ToDouble(DBHclass[k].ToString());
                LoadedTable.Add(po);
            }   //  end for k loop
            double calcValue = 0;
            foreach (PageOne pot in LoadedTable)
            {
                PageOne po = new PageOne();
                for (int jg = firstGOP; jg <= lastGOP; jg++)
                {
                        int minTest = (int)Math.Floor(justGroups[jg].rMinDbh);
                        float maxTest = (float)Math.Ceiling(justGroups[jg].rMaxDbh);
                    if ((int)Math.Floor(justGroups[jg].rMinDbh) <= (int)pot.value1 && (float)Math.Ceiling(justGroups[jg].rMaxDbh) >= pot.value1)
                    {
                        calcValue = CalculatePredictedVolume(pot.value1, justGroups[jg].CoefficientA, justGroups[jg].CoefficientB, justGroups[jg].CoefficientC, justGroups[jg].RegressModel);
                        if (calcValue > 0)
                        {
                            //  convert to string
                            switch (jg)
                            {
                                case 0:
                                    pot.value2 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 1:
                                    pot.value3 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 2:
                                    pot.value4 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 3:
                                    pot.value5 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 4:
                                    pot.value6 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 5:
                                    pot.value7 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 6:
                                    pot.value8 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 7:
                                    pot.value9 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 8:
                                    pot.value10 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 9:
                                    pot.value11 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 10:
                                    pot.value12 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 11:
                                    pot.value13 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 12:
                                    pot.value14 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 13:
                                    pot.value15 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 14:
                                    pot.value16 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 15:
                                    pot.value17 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 16:
                                    pot.value18 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 17:
                                    pot.value19 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                                case 18:
                                    pot.value20 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                    break;
                            }   // end switch
                        }   //  endif calcValue greater than zero
                    }
                    else
                    {
                        switch (jg)
                        {
                            case 0:
                                pot.value2 = dashes;
                                break;
                            case 1:
                                pot.value3 = dashes;
                                break;
                            case 2:
                                pot.value4 = dashes;
                                break;
                            case 3:
                                pot.value5 = dashes;
                                break;
                            case 4:
                                pot.value6 = dashes;
                                break;
                            case 5:
                                pot.value7 = dashes;
                                break;
                            case 6:
                                pot.value8 = dashes;
                                break;
                            case 7:
                                pot.value9 = dashes;
                                break;
                            case 8:
                                pot.value10 = dashes;
                                break;
                            case 9:
                                pot.value11 = dashes;
                                break;
                            case 10:
                                pot.value12 = dashes;
                                break;
                            case 11:
                                pot.value13 = dashes;
                                break;
                            case 12:
                                pot.value14 = dashes;
                                break;
                            case 13:
                                pot.value15 = dashes;
                                break;
                            case 14:
                                pot.value16 = dashes;
                                break;
                            case 15:
                                pot.value17 = dashes;
                                break;
                            case 16:
                                pot.value18 = dashes;
                                break;
                            case 17:
                                pot.value19 = dashes;
                                break;
                            case 18:
                                pot.value20 = dashes;
                                break;
                        }   // end switch
                    }   //  endif
                }   //  end foreach loop
            }   //  end for k loop

            return LoadedTable;
        }   //  end LoadPageOne



        private void writePageOne(string outFile, List<RegressionDO> justPrimary, List<RegressionDO> justSecondary)
        {
            //  overloaded to produce pages for topwood
            //  how many pages needed?  based on number of species groups plus length of species string
            int[] lastGroups = new int[justSecondary.Count + 1];
            int[] fieldLengths = new int[justSecondary.Count + 1];
            int numOpages = howManyPages(justSecondary, ref lastGroups, ref fieldLengths);
            StringBuilder prtFields = new StringBuilder();
            int firstGOP = 0;
            int lastGOP = 0;
            using (StreamWriter strWriteOut = new StreamWriter(outFile, true))
            {
                for (int j = 0; j < numOpages; j++)
                {
                    //  find last group on page here
                    for (int k = 0; k < lastGroups.Count(); k++)
                    {
                        if (lastGroups[k] == 1)
                        {
                            firstGOP = lastGOP;
                            lastGOP = k;
                            lastGroups[k] = 0;
                            break;
                        }   //  endif
                    }   //  end for k loop

                    // fill DBH class
                    DBHclass.Clear();
                    createDBHclass(justSecondary);
                    //  create page header
                    createHeader(1, justSecondary, firstGOP, lastGOP, "", "", "");
                    //  output header information
                    ReportTitle.Replace("Primary", "Secondary");
                    int numOlines = 0;
                    if (numOlines == 0)
                        outputHeader(strWriteOut, 1, numOlines);
                    //  load predicted volumes
                    List<PageOne> PageOneTable = LoadPageOne(justPrimary, justSecondary);
                    prtFields.Remove(0, prtFields.Length);
                    //  output table
                    foreach (PageOne pot in PageOneTable)
                    {
                        prtFields = buildPrintLine(fieldLengths, pot);
                        strWriteOut.WriteLine(prtFields);
                        numOlines++;
                    }   //  end foreach loop
                    //  clear print table for next page
                    PageOneTable.Clear();
                }   //  end for j loop
                strWriteOut.Close();
            }   //  end using

            return;
        }   // end writePageOne
        

        private List<PageOne> LoadPageOne(List<RegressionDO> justPrimary, List<RegressionDO> justSecondary)
        {
            //  overloaded for topwood
            //  need to loop through DBH class to get the predicted volume or dashes on the line
            List<PageOne> LoadedTable = new List<PageOne>();
            //  load DBHclass first
            for (int k = 0; k < DBHclass.Count; k++)
            {
                PageOne po = new PageOne();
                po.value1 = Convert.ToDouble(DBHclass[k].ToString());
                LoadedTable.Add(po);
            }   //  end for k loop

            double calcTW = 0;
            double calcMS = 0;
            double calcValue = 0;
            foreach (PageOne pot in LoadedTable)
            {
                PageOne po = new PageOne();
                for (int jg = 0; jg < justSecondary.Count; jg++)
                {
                    if ((int)Math.Floor(justSecondary[jg].rMinDbh) <= (int)pot.value1 && (float)Math.Ceiling(justSecondary[jg].rMaxDbh) >= pot.value1)
                    {
                        // find current class in primary list
                        int nthRow = justPrimary.FindIndex(
                            delegate(RegressionDO jp)
                            {
                                return jp.rSpeices == justSecondary[jg].rSpeices && jp.rProduct == justSecondary[jg].rProduct &&
                                    jp.rLiveDead == justSecondary[jg].rLiveDead && (int)Math.Floor(jp.rMinDbh) <= (int)pot.value1 && 
                                    (float)Math.Ceiling(jp.rMaxDbh) >= pot.value1;
                            });
                        if (nthRow >= 0)
                        {
                            calcMS = CalculatePredictedVolume(pot.value1, justPrimary[nthRow].CoefficientA, justPrimary[nthRow].CoefficientB,
                                                        justPrimary[nthRow].CoefficientC, justPrimary[nthRow].RegressModel);
                            calcTW = CalculatePredictedVolume(pot.value1, justSecondary[jg].CoefficientA, justSecondary[jg].CoefficientB,
                                                            justSecondary[jg].CoefficientC, justSecondary[jg].RegressModel);
                            calcValue = calcTW - calcMS;
                        }
                        else calcValue = 0.0;
                        //  convert to string
                        switch (jg)
                        {
                            case 0:
                                pot.value2 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 1:
                                pot.value3 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 2:
                                pot.value4 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 3:
                                pot.value5 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 4:
                                pot.value6 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 5:
                                pot.value7 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 6:
                                pot.value8 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 7:
                                pot.value9 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 8:
                                pot.value10 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 9:
                                pot.value11 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 10:
                                pot.value12 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 11:
                                pot.value13 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 12:
                                pot.value14 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 13:
                                pot.value15 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 14:
                                pot.value16 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 15:
                                pot.value17 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 16:
                                pot.value18 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 17:
                                pot.value19 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                            case 18:
                                pot.value20 = Utilities.FormatField(calcValue, "{0,6:F2}").ToString().PadLeft(6, ' ');
                                break;
                        }   // end switch
                    }
                    else
                    {
                        switch (jg)
                        {
                            case 0:
                                pot.value2 = dashes;
                                break;
                            case 1:
                                pot.value3 = dashes;
                                break;
                            case 2:
                                pot.value4 = dashes;
                                break;
                            case 3:
                                pot.value5 = dashes;
                                break;
                            case 4:
                                pot.value6 = dashes;
                                break;
                            case 5:
                                pot.value7 = dashes;
                                break;
                            case 6:
                                pot.value8 = dashes;
                                break;
                            case 7:
                                pot.value9 = dashes;
                                break;
                            case 8:
                                pot.value10 = dashes;
                                break;
                            case 9:
                                pot.value11 = dashes;
                                break;
                            case 10:
                                pot.value12 = dashes;
                                break;
                            case 11:
                                pot.value13 = dashes;
                                break;
                            case 12:
                                pot.value14 = dashes;
                                break;
                            case 13:
                                pot.value15 = dashes;
                                break;
                            case 14:
                                pot.value16 = dashes;
                                break;
                            case 15:
                                pot.value17 = dashes;
                                break;
                            case 16:
                                pot.value18 = dashes;
                                break;
                            case 17:
                                pot.value19 = dashes;
                                break;
                            case 18:
                                pot.value20 = dashes;
                                break;
                        }   // end switch
                    }   //  endif
                }   //  end foreach loop
            }   //  end for k loop

            return LoadedTable;
        }   //  end LoadPageOne


        private void writePageTwo(string outFile, List<RegressionDO> justGroups, int whichPage)
        {
            //  update Report Title
            if (whichPage == 2)
                ReportTitle.Replace("Primary", "Secondary");
            else ReportTitle.Append("  -  Regression Results");
            int numOlines = 0;

            using (StreamWriter strWriteOut = new StreamWriter(outFile, true))
            {
                foreach (RegressionDO jg in justGroups)
                {
                    createHeader(2, justGroups, 0, 0,jg.rSpeices, jg.rProduct, jg.rLiveDead);
                    if (numOlines == 0 || numOlines >= 50)
                        outputHeader(strWriteOut, 2,numOlines);
                    else outputHeader(strWriteOut, 2, numOlines);

                    //  write group line
                    strWriteOut.Write("      ");
                    strWriteOut.Write(jg.RegressModel.PadRight(11, ' '));
                    strWriteOut.Write(Utilities.FormatField(jg.TotalTrees, "{0,6:F0}").ToString().PadRight(8, ' '));
                    strWriteOut.Write(Utilities.FormatField(jg.Rsquared, "{0,6:F4}").ToString().PadRight(9, ' '));
                    strWriteOut.Write(Utilities.FormatField(jg.MeanSE, "{0,7:F4}").ToString().PadRight(10, ' '));
                    strWriteOut.Write(Utilities.FormatField(jg.rMinDbh, "{0,3:F1}").ToString().PadRight(7, ' '));
                    strWriteOut.Write(Utilities.FormatField(jg.rMaxDbh, "{0,3:F1}").ToString().PadRight(7, ' '));
                    //  build equation
                    StringBuilder currEquation = buildEquation(jg.CoefficientA, jg.CoefficientB, jg.CoefficientC, jg.RegressModel);
                    strWriteOut.WriteLine(currEquation.ToString());
                    strWriteOut.WriteLine("\n");
                    numOlines++;
                }   //  end foreach loop
                strWriteOut.Close();
            }   //  end using
            return;
        }   // end writePageTwo


        private void outputHeader(StreamWriter strWriteOut, int whichPage, int numOlines)
        {
            if (numOlines == 0 || numOlines >= 50)
            {
                strWriteOut.WriteLine("\f");
                strWriteOut.WriteLine(ReportTitle);
                strWriteOut.Write("CRUISE #:   ");
                strWriteOut.Write(currSale);
                strWriteOut.Write("     SALE #:  ");
                strWriteOut.WriteLine(currSale);
                strWriteOut.Write("SALENAME:  ");
                strWriteOut.WriteLine(currSaleName);
                strWriteOut.Write("RUN DATE & TIME:  ");
                strWriteOut.WriteLine(currDate);
                strWriteOut.WriteLine("\n\n");
                numOlines = 6;
            }   //endif
            //  write column lines
            switch (whichPage)
            {
                case 1:
                    for (int j = 0; j < 4; j++)
                    {
                        strWriteOut.WriteLine(colsPageOne[j]);
                        numOlines++;
                    }
                    break;
                case 2:
                    strWriteOut.WriteLine(colsPageTwo[0]);
                    numOlines++;
                    strWriteOut.WriteLine(colsPageTwo[1]);
                    numOlines++;
                    break;
            }   //  end switch
            
            return;
        }   //  end outputHeader


        //  calculate predicted volume
        private double CalculatePredictedVolume(double currDBH, double coef1, double coef2, double coef3, string currModel)
        {
            double calcValue = 0.0;
            switch (currModel)
            {
                case "Linear":
                    calcValue = coef1 + coef2 * currDBH;
                    break;
                case "Quadratic":
                    calcValue = coef1 + coef2 * currDBH + coef3 * currDBH * currDBH;
                    break;
                case "Log":
                    calcValue = coef1 + coef2 * (Math.Log(currDBH));
                    break;
                case "Power":
                    calcValue = coef1 * Math.Pow(currDBH, coef2);
                    break;
            }   //  end switch
            return calcValue;
        }   //  end CalculatePredictedVolume


        private StringBuilder buildEquation(double coef1, double coef2, double coef3, string currModel)
        {
            //  this is for page two
            StringBuilder regrEq = new StringBuilder();

            //  every equation starts with coefficient 1
            regrEq.Append(Utilities.FormatField(coef1,"{0,8:F6}").ToString());

            switch (currModel)
            {
                case "Linear":
                    regrEq.Append(" + ");
                    regrEq.Append(Utilities.FormatField(coef2,"{0,8:F6}").ToString());
                    regrEq.Append("*DBH");
                    break;
                case "Quadratic":
                    regrEq.Append(" + ");
                    regrEq.Append(Utilities.FormatField(coef2,"{0,8:F6}").ToString());
                    regrEq.Append("*DBH + ");
                    regrEq.Append(Utilities.FormatField(coef3,"{0,8:F6}").ToString());
                    regrEq.Append("*DBH*DBH");
                    break;
                case "Log":
                    regrEq.Append(" + ");
                    regrEq.Append(Utilities.FormatField(coef2,"{0,8:F6}").ToString());
                    regrEq.Append("*ln(DBH)");
                    break;
                case "Power":
                    regrEq.Append("*(DBH^");
                    regrEq.Append(Utilities.FormatField(coef2,"{0,8:F6}").ToString());
                    regrEq.Append(")");
                    break;
            }   //  end switch
            return regrEq;
        }   //  end buildEquation


        private void createDBHclass(List<RegressionDO> currGroup)
        {
            // creates DBH classes for first page
            // compare minDBH values to find lowest calss
            float currMin = currGroup.Min(cg => cg.rMinDbh);
            float currMax = currGroup.Max(cg => cg.rMaxDbh);

            int startNum = (int)Math.Floor(currMin);
            currMax = (float)Math.Ceiling(currMax);
            for (int k = startNum; k < currMax + 1; k++)
                DBHclass.Add(k);
            return;
        }   //  end createDBHclass


        private void createHeader(int whichPage, List<RegressionDO> currResults, int firstGOP, int lastGOP,
                                        string currSP, string currPR, string currLD)
        {
            //  create header for specified page
            int iLength = 0;
            string singleDash = "-";
            //  cler header in case of multiple pages
            //  but only for the first page
            colsPageOne[0] = "";
            colsPageOne[1] = "";
            colsPageOne[2] = "";
            colsPageOne[3] = "";
            switch (whichPage)
            {
                case 1:     //  page one
                    colsPageOne[0] = " Species";
                    colsPageOne[1] = " Product";
                    colsPageOne[2] = "     L/D";
                    colsPageOne[3] = "     DBH---";
                    for (int k = firstGOP; k <= lastGOP; k++)
                    {
                        iLength = currResults[k].rSpeices.Length;
                        colsPageOne[0] += "   ";
                        colsPageOne[1] += "   ";
                        colsPageOne[2] += "   ";
                        colsPageOne[3] += "---"; ;
                        if(iLength < 6)
                        {
                            colsPageOne[0] += currResults[k].rSpeices.PadLeft(6, ' ');
                            colsPageOne[1] += currResults[k].rProduct.PadLeft(6, ' ');
                            colsPageOne[2] += currResults[k].rLiveDead.PadLeft(6, ' ');
                        }
                        else 
                        {
                            colsPageOne[0] += currResults[k].rSpeices;
                            colsPageOne[1] += currResults[k].rProduct.PadLeft(iLength,' ');
                            colsPageOne[2] += currResults[k].rLiveDead.PadLeft(iLength,' ');
                        }   //  endif
                    }   //  end for each loop
                    for (int j = 1; j < colsPageOne[0].Length + 1; j++)
                        colsPageOne[3] = colsPageOne[3].Insert(colsPageOne[3].Length, singleDash);
                    break;
                case 2:     //  page two
                    colsPageTwo[0] = " Species   ";
                    colsPageTwo[0] += currSP;
                    colsPageTwo[0] += "     Product   ";
                    colsPageTwo[0] += currPR;
                    colsPageTwo[0] += "     Live/Dead   ";
                    colsPageTwo[0] += currLD;
                    colsPageTwo[1] = "      Model      Number  RSquare  MS Error  Min    Max    Equation";
                    break;
            }   //  end switch
            return;
        }   //  end createHeader


        private void LoadFieldLengths(List<RegressionDO> justGroups, int[] fieldLengths)
        {
            int nthCol = 1;
            //  first load DBHclass length
            fieldLengths[0] = 8;
            foreach (RegressionDO jg in justGroups)
            {
                if (jg.rSpeices.Length <= 6)
                    fieldLengths[nthCol] = 3 + 6;
                else
                    fieldLengths[nthCol] = 3 + jg.rSpeices.Length;
                nthCol++;
            }   //  end foreach
            return;
        }   //  end LoadFieldLengths


        public StringBuilder buildPrintLine(int[] fieldLengths, PageOne oneLine)
        {
            StringBuilder printLine = new StringBuilder();
            for (int j = 0; j < fieldLengths.Count(); j++)
            {
                switch (j)
                {
                    case 0:
                        printLine.Append(oneLine.value1.ToString().PadLeft(fieldLengths[j], ' '));
                        break;
                    case 1:     //  since this is the first column it only needs to be padded by six spaces
                        if (oneLine.value2 != null)
                            printLine.Append(oneLine.value2.PadLeft(fieldLengths[j], ' '));
                        else printLine.Append("      ");
                        break;
                    case 2:
                        if(oneLine.value3 != null)
                            printLine.Append(oneLine.value3.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 3:
                        if(oneLine.value4 != null)
                            printLine.Append(oneLine.value4.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 4:
                        if(oneLine.value5 != null)
                            printLine.Append(oneLine.value5.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 5:
                        if(oneLine.value6 != null)
                            printLine.Append(oneLine.value6.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 6:
                        if(oneLine.value7 != null)
                            printLine.Append(oneLine.value7.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 7:
                        if(oneLine.value8 != null)
                            printLine.Append(oneLine.value8.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 8:
                        if(oneLine.value9 != null)
                            printLine.Append(oneLine.value9.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 9:
                        if(oneLine.value10 != null)
                            printLine.Append(oneLine.value10.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 10:
                        if(oneLine.value11 != null)
                            printLine.Append(oneLine.value11.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 11:
                        if(oneLine.value12 != null)
                            printLine.Append(oneLine.value12.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 12:
                        if(oneLine.value13 != null)
                            printLine.Append(oneLine.value13.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 13:
                        if(oneLine.value14 != null)
                            printLine.Append(oneLine.value14.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 14:
                        if(oneLine.value15 != null)
                            printLine.Append(oneLine.value15.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 15:
                        if(oneLine.value16 != null)
                            printLine.Append(oneLine.value16.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 16:
                        if(oneLine.value17 != null)
                            printLine.Append(oneLine.value17.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 17:
                        if(oneLine.value18 != null)
                            printLine.Append(oneLine.value18.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 18:
                        if(oneLine.value19 != null)
                            printLine.Append(oneLine.value19.PadLeft(fieldLengths[j], ' '));
                        break;
                    case 19:
                        if(oneLine.value20 != null)
                            printLine.Append(oneLine.value20.PadLeft(fieldLengths[j], ' '));
                        break;
                }   //  end switch
            }   //  end for j loop
            return printLine;
        }   //  end buildPrintLine


        public class PageOne
        {
            public double value1 { get; set; }
            public string value2 { get; set; }
            public string value3 { get; set; }
            public string value4 { get; set; }
            public string value5 { get; set; }
            public string value6 { get; set; }
            public string value7 { get; set; }
            public string value8 { get; set; }
            public string value9 { get; set; }
            public string value10 { get; set; }
            public string value11 { get; set; }
            public string value12 { get; set; }
            public string value13 { get; set; }
            public string value14 { get; set; }
            public string value15 { get; set; }
            public string value16 { get; set; }
            public string value17 { get; set; }
            public string value18 { get; set; }
            public string value19 { get; set; }
            public string value20 { get; set; }
        }   //  end PageOne
    }
}
