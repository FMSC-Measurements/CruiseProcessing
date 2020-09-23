using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    class HTMLoutput
    {
        #region
        public string fileName;
        private string[] HTMLcommands = new string[7] {"<H2><A name=Index>Index</A></H2>",
	                                                   "<H2><A href=\"#XXXX\">XXX Report</A></H2>",
	                                                   "<p style = \"page-break-before: always\">",
	                                                   "<pre>",
	                                                   "<H2><A name=XXXX>XX</A></H2>",
	                                                   "<H2><A href=\"#Index\">Back to Index</A></H2>",
	                                                   "</pre>"};
        public CPbusinessLayer bslyr = new CPbusinessLayer();
        #endregion

        public void CreateHTMLfile()
        {
            int[] eqTables = new int[4] { 0, 0, 0, 0 };
            string outputFileName;
            string HTMLoutFile;

            //  need list of reports selected to build index
            List<ReportsDO> reportsSelected = bslyr.GetSelectedReports();

            //  check for equation tables with records to output
            List<VolumeEquationDO> veList = bslyr.getVolumeEquations();
            List<ValueEquationDO> vaList = bslyr.getValueEquations();
            List<QualityAdjEquationDO> qaList = bslyr.getQualAdjEquations();
            List<BiomassEquationDO> bsList = bslyr.getBiomassEquations();

            if (veList.Count > 0) eqTables[0] = 1;
            if (vaList.Count > 0) eqTables[1] = 1;
            if (qaList.Count > 0) eqTables[2] = 1;
            if (bsList.Count > 0) eqTables[3] = 1;

            //  fix the output filename
            outputFileName = System.IO.Path.ChangeExtension(fileName, "out");
            //  does it exist?
            if (!File.Exists(outputFileName))
            {
                MessageBox.Show("Cannot create HTML file because the output file cannot be found.\nMake sure the output file has been created\nfor the current cruise file.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }   //  endif if does not exist

            //  create HTML filename
            HTMLoutFile = System.IO.Path.ChangeExtension(fileName, "html");

            //  need the index
            ArrayList HTMLindex = BuildTheIndex(eqTables, reportsSelected);

            //  Output records from the output file
            using (StreamWriter strWriteOut = new StreamWriter(HTMLoutFile))
            {
                //  output index first
                foreach (object obj in HTMLindex)
                    strWriteOut.WriteLine(obj);

                //  Read/Write records from OUT file
                string line;
                string currentRecord;
                string currentReport = "**";
                string previousReport = "**";
                using (StreamReader strRead = new StreamReader(outputFileName))
                {
                    while ((line = strRead.ReadLine()) != null)
                    {
                        if (line == "\f")
                        {
                            //  don't put "back to index"; just a new page
                                strWriteOut.WriteLine(HTMLcommands[2]);
                        }
                        else if (line == "" || line == " ")
                        {
                            strWriteOut.WriteLine(line);
                        }
                        else if (line != "" && line != " ")
                        {
                            if (line.Contains("CRUISE REPORT HEADER"))
                            {
                                strWriteOut.WriteLine(HTMLcommands[3]);
                                strWriteOut.WriteLine(line);
                            }
                            else if (line.Contains("VOLUME EQUATION TABLE"))
                            {
                                currentReport = "VOLUME EQUATION TABLE";
                                if (previousReport != currentReport)
                                {
                                    strWriteOut.WriteLine(HTMLcommands[5]);
                                    previousReport = currentReport;
                                    strWriteOut.WriteLine(HTMLcommands[2]);
                                }
                                else strWriteOut.WriteLine(HTMLcommands[2]);
                                currentRecord = HTMLcommands[4];
                                currentRecord = currentRecord.Replace("XXXX", "VolEq");
                                currentRecord = currentRecord.Replace("XX", "Volume Equation Table");
                                strWriteOut.WriteLine(currentRecord);
                                strWriteOut.WriteLine(line);
                            }
                            else if (line.Contains("VALUE EQUATION TABLE"))
                            {
                                currentReport = "VALUE EQUATION TABLE";
                                if (previousReport != currentReport)
                                {
                                    strWriteOut.WriteLine(HTMLcommands[5]);
                                    previousReport = currentReport;
                                    strWriteOut.WriteLine(HTMLcommands[2]);
                                }
                                else strWriteOut.WriteLine(HTMLcommands[2]);
                                currentRecord = HTMLcommands[4];
                                currentRecord = currentRecord.Replace("XXXX", "ValEq");
                                currentRecord = currentRecord.Replace("XX", "Value Equation Table");
                                strWriteOut.WriteLine(currentRecord);
                                strWriteOut.WriteLine(line);
                            }
                            else if (line.Contains("QUALITY ADJUSTMENT"))
                            {
                                currentReport = "QUALITY ADJUSTMENT EQUATION TABLE";
                                if (previousReport != currentReport)
                                {
                                    strWriteOut.WriteLine(HTMLcommands[5]);
                                    previousReport = currentReport;
                                    strWriteOut.WriteLine(HTMLcommands[2]);
                                }
                                else strWriteOut.WriteLine(HTMLcommands[2]);
                                currentRecord = HTMLcommands[4];
                                currentRecord = currentRecord.Replace("XXXX", "QAEq");
                                currentRecord = currentRecord.Replace("XX", "Quality Adjustment Equation Table");
                                strWriteOut.WriteLine(currentRecord);
                                strWriteOut.WriteLine(line);
                            }
                            else if (line.Contains("BIOMASS EQUATION TABLE"))
                            {
                                currentReport = "BIOMASS EQUATION TABLE";
                                if (previousReport != currentReport)
                                {
                                    strWriteOut.WriteLine(HTMLcommands[5]);
                                    previousReport = currentReport;
                                    strWriteOut.WriteLine(HTMLcommands[2]);
                                }
                                else strWriteOut.WriteLine(HTMLcommands[2]);
                                currentRecord = HTMLcommands[4];
                                currentRecord = currentRecord.Replace("XXXX", "BiomassEq");
                                currentRecord = currentRecord.Replace("XX", "Biomass Equation Table");
                                strWriteOut.WriteLine(currentRecord);
                                strWriteOut.WriteLine(line);
                            }
                            else if (line.Contains(':'))
                            {
                                //  find out if this is a report
                                int nthRow = reportsSelected.FindIndex(
                                    delegate(ReportsDO rdo)
                                    {
                                        return rdo.ReportID == line.Substring(0, line.IndexOf(':'));
                                    });
                                if (nthRow >= 0)
                                {
                                    // it's definitely a report
                                    currentReport = reportsSelected[nthRow].ReportID;
                                    if (previousReport != currentReport)
                                    {
                                        strWriteOut.WriteLine(HTMLcommands[5]);
                                        previousReport = currentReport;
                                        strWriteOut.WriteLine(HTMLcommands[2]);
                                    }
                                    else strWriteOut.WriteLine(HTMLcommands[2]);
                                    currentRecord = HTMLcommands[4];
                                    currentRecord = currentRecord.Replace("XXXX", currentReport);
                                    currentRecord = currentRecord.Replace("XX", currentReport);
                                    strWriteOut.WriteLine(currentRecord);
                                    strWriteOut.WriteLine(line);
                                }
                                else strWriteOut.WriteLine(line);
                            }
                            else
                            {
                                strWriteOut.WriteLine(line);
                            }   //  endif line contains
                        }   //  endif
                }   //  end while read
            }   //  end using read
                
                strWriteOut.WriteLine(HTMLcommands[5]);
                strWriteOut.WriteLine(HTMLcommands[6]);
            }   //  end using write

            //  show user where it is
            StringBuilder sb = new StringBuilder();
            sb.Append("HTML File has been created.\nIt can be found at:\n");
            sb.Append(HTMLoutFile);
            MessageBox.Show(sb.ToString(), "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return;
        }   //  end CreateHTMLfile


        private ArrayList BuildTheIndex(int[] eqTables, List<ReportsDO> selectedReports)
        {
            ArrayList indexToReturn = new ArrayList();
            indexToReturn.Add(HTMLcommands[0]);

            string indexLine;
            //  Check for equation tables
            for (int k = 0; k < 4; k++)
            {
                indexLine = HTMLcommands[1];
                if (eqTables[k] == 1)
                {
                    switch (k)
                    {
                        case 0:     //  volume equations
                            indexLine = indexLine.Replace("XXXX", "VolEq");
                            indexLine = indexLine.Replace("XXX Report", "Volume Equation Table");
                            break;
                        case 1:     //  value equations
                            indexLine = indexLine.Replace("XXXX", "ValEq");
                            indexLine = indexLine.Replace("XXX Report", "Value Equation Table");
                            break;
                        case 2:     //  quality adjustment
                            indexLine = indexLine.Replace("XXXX", "QAEq");
                            indexLine = indexLine.Replace("XXX Report", "Quality Adjustment Equation Table");
                            break;
                        case 3:     //  biomass equations
                            indexLine = indexLine.Replace("XXXX", "BiomassEq");
                            indexLine = indexLine.Replace("XXX Report", "Biomass Equation Table");
                            break;
                    }   //  end switch
                    indexToReturn.Add(indexLine);
                }   //  endif
            }   //  end for k loop

            //  now reports
            foreach (ReportsDO rdo in selectedReports)
            {
                indexLine = HTMLcommands[1];
                indexLine = indexLine.Replace("XXXX", rdo.ReportID);
                indexLine = indexLine.Replace("XXX", rdo.ReportID);
                indexToReturn.Add(indexLine);
            }   //  end foreach loop
            return indexToReturn;
        }   //  end BuildTheIndex
    }
}
