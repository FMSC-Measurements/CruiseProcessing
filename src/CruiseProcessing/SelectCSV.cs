using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseProcessing.Services;
using CruiseProcessing.Data;

namespace CruiseProcessing
{
    public partial class SelectCSV : Form
    {
        
        
        
        private string textFileName;
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
        
        protected CpDataLayer DataLayer { get; }
        public IDialogService DialogService { get; }

        protected SelectCSV()
        {
            InitializeComponent();
        }

        public SelectCSV(CpDataLayer dataLayer, IDialogService dialogService)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        public void setupDialog()
        {
            //  make sure text output file exists as these are generated from the reports in that file
            textFileName = System.IO.Path.ChangeExtension(DataLayer.FilePath, "out");
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
            string currPath = System.IO.Path.GetDirectoryName(DataLayer.FilePath);
            currPath += "\\";
            for (int j = 0; j < 11; j++)
            {
                if (filesToOutput[j] == 1)
                {
                    var csvPath = Path.Combine(currPath, CSVfileNames[j]);

                    if (j == 8)//  KPI estimates
                    {
                        var csvReportBuilder = new OutputCSV(DataLayer, DialogService, "CSV9");
                        csvReportBuilder.OutputEstimateFile(csvPath);
                    }
                    else if (j == 9) //  Timber Theft file
                    {
                        var csvReportBuilder = new OutputCSV(DataLayer, DialogService, "CSV10");
                        csvReportBuilder.OutputTimberTheft(csvPath);
                    }
                    else
                    {
                        string currentReport = null;
                        string reportToUse = null;

                        switch (j)
                        {
                            case 0:     //  A05
                                currentReport = "CSV1";
                                reportToUse = "A05";
                                break;
                            case 1:     //  A06
                                currentReport = "CSV2";
                                reportToUse = "A06";
                                break;
                            case 2:     //  A07
                                currentReport = "CSV3";
                                reportToUse = "A07";
                                break;
                            case 3:     //  A10
                                currentReport = "CSV4";
                                reportToUse = "A10";
                                break;
                            case 4:     //  L1
                                currentReport = "CSV5";
                                reportToUse = "L1:";
                                break;
                            case 5:     //  L3
                                        //  this needs to be built from scratch -- noope, fixed it to work with L2 report
                                currentReport = "CSV6";
                                reportToUse = "L2:";
                                break;
                            case 6:     //  ST1
                                currentReport = "CSV7";
                                reportToUse = "ST1";
                                break;
                            case 7:     //  UC5
                                currentReport = "CSV8";
                                reportToUse = "UC5";
                                break;
                            case 10:        //  VSM<4 
                                currentReport = "CSV11";
                                reportToUse = "VSM4";
                                break;
                        }

                        var csvReportBuilder = new OutputCSV(DataLayer, DialogService, currentReport);
                        csvReportBuilder.OutputCSVfiles(csvPath, reportToUse, textFileName);
                    }
                }
            }

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
