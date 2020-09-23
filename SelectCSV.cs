using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CruiseProcessing
{
    public partial class SelectCSV : Form
    {
        #region
        public string fileName;
        public CPbusinessLayer bslyr = new CPbusinessLayer();
        private string textFileName;
        private string CSVoutFile;
        private int[] filesToOutput = new int[11];
        private string[] CSVfileNames = new string[11] { "ReportA05.csv", 
                                                        "ReportA06.csv", 
                                                        "ReportA07.csv", 
                                                        "ReportA10.csv", 
                                                        "ReportL1.csv", 
                                                        "ReportL2.csv",
                                                        "ReportST1.csv",
                                                        "ReportUC5.csv",
                                                        "ReportKPIestimates.csv",
                                                        "TimberTheft.csv",
                                                        "ReportVSM4.csv"};
        #endregion

        public SelectCSV()
        {
            InitializeComponent();
        }

        public void setupDialog()
        {
            //  make sure text output file exists as these are generated from the reports in that file
            textFileName = System.IO.Path.ChangeExtension(fileName, "out");
            if (!File.Exists(textFileName))
            {
                MessageBox.Show("TEXT OUTPUT FILE COULD NOT BE FOUND.\nPlease create the text output file to continue.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }   //  endif
            for (int k = 0; k < 10; k++)
                filesToOutput[k] = 0;
        }   //  end setupDialog


        private void CSV1_checked(object sender, EventArgs e)
        {
            filesToOutput[0] = 1;
        }   //  end CSV1_checked


        private void CSV2_checked(object sender, EventArgs e)
        {
            filesToOutput[1] = 1;
        }   //  end CSV2_checked

        private void CSV3_checked(object sender, EventArgs e)
        {
            filesToOutput[2] = 1;
        }   //  end CSV3_checked

        private void CSV4_checked(object sender, EventArgs e)
        {
            filesToOutput[3] = 1;
        }   //  end CSV4_checked

        private void CSV5_checked(object sender, EventArgs e)
        {
            filesToOutput[4] = 1;
        }   //  end CSV5_checked

        private void CSV6_checked(object sender, EventArgs e)
        {
            filesToOutput[5] = 1;
        }   //  end CSV6_checked

        private void onCreateFiles(object sender, EventArgs e)
        {
            OutputCSV ocsv = new OutputCSV();
            ocsv.fileName = fileName;
            ocsv.bslyr.fileName = bslyr.fileName;
            ocsv.bslyr.DAL = bslyr.DAL;
            string currPath = System.IO.Path.GetDirectoryName(fileName);
            currPath += "\\";
            for (int j = 0; j < 11; j++)
            {
                if (filesToOutput[j] == 1)
                {
                    CSVoutFile = currPath;
                    CSVoutFile += CSVfileNames[j];
                    switch (j)
                    {
                        case 0:     //  A05
                            ocsv.currentReport = "CSV1";
                            ocsv.OutputCSVfiles(CSVoutFile, "A05", textFileName);
                            break;
                        case 1:     //  A06
                            ocsv.currentReport = "CSV2";
                            ocsv.OutputCSVfiles(CSVoutFile, "A06", textFileName);
                            break;
                        case 2:     //  A07
                            ocsv.currentReport = "CSV3";
                            ocsv.OutputCSVfiles(CSVoutFile, "A07", textFileName);
                            break;
                        case 3:     //  A10
                            ocsv.currentReport = "CSV4";
                            ocsv.OutputCSVfiles(CSVoutFile, "A10", textFileName);
                            break;
                        case 4:     //  L1
                            ocsv.currentReport = "CSV5";
                            ocsv.OutputCSVfiles(CSVoutFile, "L1:", textFileName);
                            break;
                        case 5:     //  L3
                            //  this needs to be built from scratch -- noope, fixed it to work with L2 report
                            ocsv.currentReport = "CSV6";
                            ocsv.fileName = fileName;
                            ocsv.OutputCSVfiles(CSVoutFile, "L2:", textFileName);
                            break;
                        case 6:     //  ST1
                            ocsv.currentReport = "CSV7";
                            ocsv.fileName = fileName;
                            ocsv.OutputCSVfiles(CSVoutFile, "ST1", textFileName);
                            break;
                        case 7:     //  UC5
                            ocsv.currentReport = "CSV8";
                            ocsv.fileName = fileName;
                            ocsv.OutputCSVfiles(CSVoutFile, "UC5", textFileName);
                            break;
                        case 8:     //  KPI estimates
                            ocsv.currentReport = "CSV9";
                            ocsv.OutputEstimateFile(CSVoutFile);
                            break;
                        case 9: //  Timber Theft file
                            ocsv.currentReport = "CSV10";
                            ocsv.OutputTimberTheft(CSVoutFile);
                            break;
                        case 10:        //  VSM<4 
                            ocsv.currentRegion = "CSV11";
                            ocsv.OutputCSVfiles(CSVoutFile, "VSM4", textFileName);
                            break;
                    }   //  end switch
                }   //  endif
                CSVoutFile = "";
            }   //  end for j loop

            MessageBox.Show("All CSV files requested have been created.\nCheck directory for .CSV files", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
          
            return;
        }   //  end onCreateFiles

        private void onFinished(object sender, EventArgs e)
        {
            Close();
            return;
        }

        private void CSV7_checked(object sender, EventArgs e)
        {
            filesToOutput[6] = 1;
        }

        private void CSV8_checked(object sender, EventArgs e)
        {
            filesToOutput[7] = 1;
        }

        private void CSV9_checked(object sender, EventArgs e)
        {
            filesToOutput[8] = 1;
        }

        private void CSV10_checked(object sender, EventArgs e)
        {
            filesToOutput[9] = 1;
        }

        private void CSV11_checked(object sender, EventArgs e)
        {
            filesToOutput[10] = 1;            
        }

    }
}
