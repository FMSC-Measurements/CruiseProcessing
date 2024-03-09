using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL;
using System.Reflection;
using CruiseProcessing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CruiseProcessing
{
    public partial class MainMenu : Form
    {
        public int templateFlag;
        public int whichProcess;
        string currentRegion;

        protected string AppVerson => Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.');

        public CPbusinessLayer DataLayer => DataLayerProvider.DataLayer;
        public IDialogService DialogService { get; }
        public IServiceProvider Services { get; }
        public DataLayerContext DataLayerProvider { get; }

        public MainMenu(IServiceProvider services, DataLayerContext dataLayerProvider)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            DataLayerProvider = dataLayerProvider ?? throw new ArgumentNullException(nameof(dataLayerProvider));
            DialogService = services.GetRequiredService<IDialogService>();
            

            InitializeComponent();
            //  initially hide all buttons and labels
            processButton1.Visible = false;
            processButton2.Visible = false;
            processButton3.Visible = false;
            processButton4.Visible = false;
            processButton5.Visible = false;
            processButton6.Visible = false;
            processLabel1.Visible = false;
            processLabel2.Visible = false;
            processLabel3.Visible = false;
            processLabel4.Visible = false;
            processLabel5.Visible = false;
            processLabel6.Visible = false;
            modifyWeightFacts.Visible = false;
            modifyMerchRules.Visible = false;

            //  also disable everything but the file button so a filename has to be selected
            menuButton2.BackgroundImage = Properties.Resources.disabled_button;
            menuButton3.BackgroundImage = Properties.Resources.disabled_button;
            processBtn.BackgroundImage = Properties.Resources.disabled_button;
            menuButton5.BackgroundImage = Properties.Resources.disabled_button;
            
            menuButton2.Enabled = false;
            menuButton3.Enabled = false;
            processBtn.Enabled = false;
            menuButton5.Enabled = false;
        }

        private void onExit(object sender, EventArgs e)
        {
            Close();
        }

        private void onOutput(object sender, EventArgs e)
        {
            splashPic.Visible = false;
            whichProcess = 4;
            //  setup pics on buttons and text on labels
            processLabel1.Text = "Create Text Output File";
            processLabel2.Text = "Create HTML Output File";
            processLabel3.Text = "Create PDF Output File";
            processLabel4.Text = "Create CSV Output File";
            processLabel5.Text = "Print Preview";
            processLabel6.Text = "Add Local Volume";
            processButton1.BackgroundImage = Properties.Resources.textfile;
            processButton2.BackgroundImage = Properties.Resources.htmlfile;
            processButton3.BackgroundImage = Properties.Resources.pdffile;
            processButton4.BackgroundImage = Properties.Resources.CSVfile;
            processButton5.BackgroundImage = Properties.Resources.preview;
            processButton6.BackgroundImage = Properties.Resources.LocalVolume;

            // show or hide components needed
            processLabel1.Visible = true;
            processLabel2.Visible = true;
            processLabel3.Visible = true;
            processLabel4.Visible = true;
            processLabel5.Visible = true;
            processLabel6.Visible = true;
            processButton1.Visible = true;
            processButton2.Visible = true;
            processButton2.Enabled = false;
            processButton3.Visible = true;
            processButton3.Enabled = false;
            processButton4.Visible = true;
            processButton4.Enabled = false;
            processButton5.Visible = true;
            processButton5.Enabled = false;
            processButton6.Visible = true;
            processButton6.Enabled = false;
            modifyWeightFacts.Visible = false;
            modifyMerchRules.Visible = false;

        }   //  end onOutput


        private void onProcess(object sender, EventArgs e)
        {
            splashPic.Visible = true;
            //  setup pics on buttons and text on labels
            //processLabel1.Text = "Edit Checks";
            //processLabel2.Text = "Calculate Volumes";
            //processLabel3.Text = "Local Volume";
            //processButton1.BackgroundImage = Properties.Resources.CheckMark;
            //processButton2.BackgroundImage = Properties.Resources.SlideRule;
            //processButton3.BackgroundImage = Properties.Resources.LocalVolume;
            //  per discussion at steering team meeting -- February 2013
            //  no buttons shown when Process is clicked.
            //  Just do edit checks and keep going if passed
            //  Stop if errors found and create output error report


            //  show or hide components needed
            processLabel1.Visible = false;
            processLabel2.Visible = false;
            processLabel3.Visible = false;
            processLabel4.Visible = false;
            processLabel5.Visible = false;
            processLabel6.Visible = false;
            processButton1.Visible = false;
            processButton2.Visible = false;
            processButton3.Visible = false;
            processButton4.Visible = false;
            processButton5.Visible = false;
            processButton6.Visible = false;

            // let user know it's happening
            //  replace this with the processing status window
            ProcessStatus statusDlg = Services.GetRequiredService<ProcessStatus>();
            statusDlg.ShowDialog();   
            Cursor.Current = this.Cursor;
            modifyWeightFacts.Visible = false;
            modifyMerchRules.Visible = false;
            return;
        }   //  end onProcess


        private void onReports(object sender, EventArgs e)
        {
            splashPic.Visible = false;
            whichProcess = 2;
            //  set text and pics on labels and buttons
            processLabel1.Text = "Add Standard Reports";
            processLabel2.Text = "Add Graphical Reports";
            processButton1.BackgroundImage = Properties.Resources.standard;
            processButton2.BackgroundImage = Properties.Resources.graphs;

            //  hide or show needed items
            processLabel1.Visible = true;
            processLabel2.Visible = true;
            processLabel3.Visible = false;
            processLabel4.Visible = false;
            processLabel5.Visible = false;
            processLabel6.Visible = false;
            processButton1.Visible = true;
            processButton2.Visible = true;
            processButton3.Visible = false;
            processButton4.Visible = false;
            processButton5.Visible = false;
            processButton6.Visible = false;
            modifyWeightFacts.Visible = false;
            modifyMerchRules.Visible = false;
        }   //  end onReports

        private void onEquations(object sender, EventArgs e)
        {
            //  hide splash pic
            splashPic.Visible = false;
            //  eventually this button will have password protection for measurement specialists
            modifyWeightFacts.Visible = true;
            modifyMerchRules.Visible = true;
            //  set text and pics on labels and buttons
            processButton1.BackgroundImage = Properties.Resources.volume;
            processButton2.BackgroundImage = Properties.Resources.money3;
            //processButton3.BackgroundImage = Properties.Resources.biomass;
            //  March 2017 -- according to Karen Jones, Region 3 is nolonger using
            //  quality adjustment equations.  They are commented out here.
            //processButton3.BackgroundImage = Properties.Resources.quality;
            processButton4.BackgroundImage = Properties.Resources.R8;
            processButton5.BackgroundImage = Properties.Resources.R9;
            processLabel1.Text = "Enter Volume Equations";
            processLabel2.Text = "Enter Value Equations";
            //processLabel3.Text = "Enter Quality Adj Equations";
            processLabel4.Text = "Enter Region 8 Volume Equations";
            processLabel5.Text = "Enter Region 9 Volume Equations";



            //  check region and hide buttons as needed
            if (currentRegion == "8" || currentRegion == "08")
            {
                //  hide first and region 9 buttons
                processLabel1.Visible = false;

                processLabel2.Visible = true;
                //processLabel3.Visible = true;
                processLabel4.Visible = true;
                processLabel5.Visible = false;
                processLabel6.Visible = false;
                processButton1.Visible = false;
                processButton2.Visible = true;
                //processButton3.Visible = true;
                processButton4.Visible = true;
                processButton5.Visible = false;
                processButton6.Visible = false;
                modifyMerchRules.Visible = false;
                
            }
            else if (currentRegion == "9" || currentRegion == "09")
            {
                //  hide first and region 8 buttons
                if (doesCruiseHaveVolumeEquations())
                {
                    processLabel1.Visible = true;
                    processLabel1.Text = "Edit Volume Equations";
                    processButton1.Visible = true;
                    
                }//end if
                else
                {
                    processLabel1.Visible = false;
                    processButton1.Visible = false;
                }//end else


                processLabel2.Visible = true;
                //processLabel3.Visible = true;
                processLabel4.Visible = false;
                processLabel5.Visible = true;
                processLabel6.Visible = false;
                processButton2.Visible = true;
                //processButton3.Visible = true;
                processButton4.Visible = false;
                processButton5.Visible = true;
                processButton6.Visible = false;
                modifyMerchRules.Visible = false;
            }
            else if (templateFlag == 1)
            {
                //  disable all equation buttons except volume
                processLabel1.Visible = true;
                processLabel2.Visible = false;
                //processLabel3.Visible = false;
                processLabel4.Visible = false;
                processLabel5.Visible = false;
                processLabel6.Visible = false;
                processButton1.Visible = true;
                processButton2.Visible = false;
                //processButton3.Visible = false;
                processButton4.Visible = false;
                processButton5.Visible = false;
                processButton6.Visible = false;
                modifyMerchRules.Visible = false;
                modifyWeightFacts.Visible = false;
            }
            else
            {
                processLabel1.Visible = true;
                processLabel2.Visible = true;
                //processLabel3.Visible = true;
                processLabel4.Visible = false;
                processLabel5.Visible = false;
                processLabel6.Visible = false;
                processButton1.Visible = true;
                processButton2.Visible = true;
                //processButton3.Visible = true;
                processButton4.Visible = false;
                processButton5.Visible = false;
                processButton6.Visible = false;
            }       //  endif currentRegion

            whichProcess = 1;
        }   //  end onEquations

        private void onFile(object sender, EventArgs e)
        {
            //  Create an instance of the open file dialog
            OpenFileDialog browseDialog = new OpenFileDialog();

            //  Set filter options and filter index
            browseDialog.Filter = "Cruise Files|*.cruise;*.crz3|Template Files|*.cut;*.crz3t|All Files|*.*";
            browseDialog.FilterIndex = 1;
            browseDialog.Multiselect = false;

            var dialogResult = browseDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                OpenFile(browseDialog.FileName);
            }

        }   //  end onFile

        public void OpenFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var extention = Path.GetExtension(filePath).ToLowerInvariant();

            var isTemplate = false;
            string cruiseID = null;
            DAL dal = null;
            CruiseDatastore_V3 dal_v3 = null;

            if(extention == ".cruise")
            {
                dal = new DAL(filePath);
            }
            else if (extention == ".crz3")
            {
                var processFilePath = Path.ChangeExtension(filePath, ".process");

                var v3db = new CruiseDatastore_V3(filePath);
                cruiseID = v3db.ExecuteScalar<string>("SELECT CruiseID FROM Cruise LIMIT 1");


                var downConverter = new DownMigrator();
                if (!downConverter.EnsureCanMigrate(cruiseID, v3db, out var error_message))
                {
                    MessageBox.Show("Unable to open V3 Cruise File due to Design Checks \r\nMessages: " + error_message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    var v2db = new DAL(processFilePath, true);

                    downConverter.MigrateFromV3ToV2(cruiseID, v3db, v2db, "Cruise Processing");

                    dal = v2db;
                    dal_v3 = v3db;

                }
                catch (Exception ex)
                {
                    if (File.Exists(processFilePath))
                    { File.Delete(processFilePath); }

                    MessageBox.Show("Error Translating V3 Cruise Data \r\nError: " + ex.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            else if(extention == ".cut")
            {
                dal = new DAL(filePath);
                isTemplate = true;
            }
            else if(extention == ".crz3t")
            {
                var tempV2 = Path.ChangeExtension(filePath, ".cut.process");
                try
                {
                    var v3db = new CruiseDatastore_V3(filePath);
                    cruiseID = v3db.ExecuteScalar<string>("SELECT CruiseID FROM Cruise LIMIT 1");
                    var tempV2Db = new DAL(tempV2, true);
                    var downConverter = new DownMigrator();
                    downConverter.MigrateFromV3ToV2(cruiseID, v3db, tempV2Db, "Cruise Processing");

                    dal = tempV2Db;
                    dal_v3 = v3db;
                    isTemplate = true;

                }
                catch(Exception ex)
                {
                    if(File.Exists(tempV2))
                    { File.Delete(tempV2); }
                    return;
                }
            }
            else
            {
                MessageBox.Show("Invalid File Type Selected", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //open connection forces the connection to remain open not to close and open.  Might be good to re-work the process button click?
            dal.OpenConnection();

            var datalayer = new CPbusinessLayer(dal, dal_v3, cruiseID);

            if (isTemplate)
            {
                //  disable all buttons except equations and reports
                menuButton2.BackgroundImage = Properties.Resources.button_image;
                menuButton3.BackgroundImage = Properties.Resources.button_image;
                processBtn.BackgroundImage = Properties.Resources.disabled_button;
                menuButton5.BackgroundImage = Properties.Resources.disabled_button;
                menuButton2.Enabled = true;
                menuButton3.Enabled = true;
                processBtn.Enabled = false;
                menuButton5.Enabled = false;
            }
            else
            {
                menuButton2.BackgroundImage = Properties.Resources.button_image;
                menuButton3.BackgroundImage = Properties.Resources.button_image;
                processBtn.BackgroundImage = Properties.Resources.button_image;
                menuButton5.BackgroundImage = Properties.Resources.button_image;

                menuButton2.Enabled = true;
                menuButton3.Enabled = true;
                processBtn.Enabled = true;
                menuButton5.Enabled = true;

                //  need region number in order to hide volume button as well as region 9 button
                var sale = datalayer.GetSale();
                currentRegion = sale.Region;

                if (datalayer.saleWithNullSpecies())
                {
                    //One or more records contain incomplete data which affect processing.\n
                    MessageBox.Show("One or more records contain incomplete data which affect processing..\nPlease correct before using cruise processing.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                } 
            }

            templateFlag = (isTemplate) ? 1 : 0;

            DataLayerProvider.DataLayer = datalayer;
            //  add file name to title line at top
            if (fileName.Length > 35)
            {
                string tempName = "..." + fileName.Substring(fileName.Length - 35, 35);
                Text = tempName;
            }
        }

        protected bool EnsureFileOpen()
        {
            if (DataLayer != null) { return true; }

            MessageBox.Show("No file selected.  Cannot continue.\nPlease select a file.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }


        private void onButton1Click(object sender, EventArgs e)
        {
            if (whichProcess == 1)       //  equations
            {
                VolumeEquations volEqObj = Services.GetRequiredService<VolumeEquations>();

                if (templateFlag == 0)
                {
                    int nResult = volEqObj.setupDialog();
                    if (nResult != -1) volEqObj.ShowDialog();
                }
                else if (templateFlag == 1)
                {
                    int nResult = volEqObj.setupTemplateDialog();
                    if (nResult == 1)
                    {
                        volEqObj.templateFlag = templateFlag;
                        volEqObj.ShowDialog();
                    }   //  endif
                }   //  endif
            }
            else if(whichProcess == 2)  //  reports
            {
                //  calls routine to add standard and regional reports
                List<ReportsDO> currentReports = new List<ReportsDO>();
                //  get all reports
                currentReports = DataLayer.GetReports();
                //  and get the all reports array
                //  then check for various conditions to know what to do with the reports list
                if (currentReports.Count == 0)
                {
                    currentReports = ReportMethods.fillReportsList();
                    DataLayer.SaveReports(currentReports);


                }//end if
                else if (currentReports.Count < allReportsArray.reportsArray.GetLength(0))
                {
                    //  old or new list?  Check title
                    if (currentReports[0].Title == "" || currentReports[0].Title == null)
                    {
                        //  old reports -- update list
                        currentReports = ReportMethods.updateReportsList(currentReports, allReportsArray.reportsArray);
                        DataLayer.SaveReports(currentReports);
                    }
                    else
                    {
                        //  new reports -- just add
                        currentReports = ReportMethods.addReports(currentReports, allReportsArray.reportsArray);
                        DataLayer.SaveReports(currentReports);
                    }   //  endif

                }   //  endif
                //  now get reports selected


                currentReports = ReportMethods.deleteReports(currentReports, DataLayer);
                currentReports = DataLayer.GetSelectedReports();
                //  Get selected reports 
                ReportsDialog rd = Services.GetRequiredService<ReportsDialog>();


                rd.reportList = currentReports;
                rd.templateFlag = templateFlag;
                rd.setupDialog();
                rd.ShowDialog();
            }
            else if(whichProcess == 4)  //  output
            {
                
                //  Pull reports selected
    
                //  See if volume has been calculated (sum expansion factor since those are calculated before volume)
                //  July 2014 -- However it looks like expansion factors could be present but volume is not
                //  need to pull calculated values as well and sum net volumes
                List<TreeDO> tList = DataLayer.getTrees();
                double summedEF = tList.Sum(t => t.ExpansionFactor);
                List<TreeCalculatedValuesDO> tcvList = DataLayer.getTreeCalculatedValues();
                double summedNetBDFT = tcvList.Sum(tc => tc.NetBDFTPP);
                double summedNetCUFT = tcvList.Sum(tc => tc.NetCUFTPP);
                if (summedEF == 0 && summedNetBDFT == 0 && summedNetCUFT == 0)
                {
                    MessageBox.Show("Looks like volume has not been calculated.\nReports cannot be produced without calculated volume.\nPlease calculate volume before continuing.", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }   //  endif no volume for reports
                List<ReportsDO> selectedReports = DataLayer.GetSelectedReports(); 

                //  no reports?  let user know to go back and select reports
                if (selectedReports.Count == 0)
                {
                    MessageBox.Show("No reports selected.\nReturn to Reports section and select reports.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }   //  endif no reports

                //  Show dialog creating text file
                TextFileOutput tfo = Services.GetRequiredService<TextFileOutput>();
                tfo.selectedReports = selectedReports;
                tfo.setupDialog();
                tfo.ShowDialog();
                string outFile = tfo.outFile;
                int retrnState = tfo.retrnState;

                //  Let user know the file is complete 
                //  This shows only when the Finished button is clicked
                //  X-button click just closes the window
                if (retrnState == 0)
                {
                    StringBuilder message = new StringBuilder();
                    message.Append("Text output file is complete and can be found at:\n");
                    message.Append(outFile);
                    MessageBox.Show(message.ToString(), "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    processButton2.Enabled = true;
                    processButton3.Enabled = true;
                    processButton4.Enabled = true;
                    processButton5.Enabled = true;
                    processButton6.Enabled = true;
                    return;
                }   //  endif
            }   //  endif whichProcess
        }   //  endif onButton1Click


        private void onButton2Click(object sender, EventArgs e)
        {
            if(!EnsureFileOpen()) { return; }
           

            if(whichProcess == 1)   //  equations
            {
                if (DataLayer.saleWithNullSpecies())
                {
                    MessageBox.Show("This file has errors which affect processing.\nThis file has an invalid species, please correct before using cruise processing.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ValueEquations valEqObj = Services.GetRequiredService<ValueEquations>();

                int nResult = valEqObj.setupDialog();
                if(nResult == 1)
                    valEqObj.ShowDialog();
            }
            else if(whichProcess == 2)  //  reports
            {
                //  calls routine to add graphical reports
                GraphReportsDialog grd = Services.GetRequiredService<GraphReportsDialog>();
                grd.setupDialog();
                grd.ShowDialog();
                return;
            }
            else if(whichProcess == 4)  // output
            {
                //  calls routine to create an html output file
                HTMLoutput ho = new HTMLoutput(DataLayer, DialogService);
                ho.CreateHTMLfile();
                return;
            }   //  endif whichProcess

        }   //  end onButton2Click

        
        private void onButton3Click(object sender, EventArgs e)
        {
            if (!EnsureFileOpen()) { return; }

            //  March 2017 -- per Karen Jones, quality adjustment equations no longer used
            /*            if(whichProcess == 1)   //  equations
                        {
                            QualityAdjEquations quaEqObj = new QualityAdjEquations();
                            quaEqObj.bslyr.fileName = bslyr.fileName;
                            quaEqObj.bslyr.DAL = bslyr.DAL;
                            quaEqObj.setupDialog();
                            quaEqObj.ShowDialog();

                        }
            */
            if (whichProcess == 4)  //  output
            {
                //  calls routine to create pdf file
                PDFfileOutput pfo = Services.GetRequiredService<PDFfileOutput>();
                int nResult = pfo.setupDialog();
                if(nResult == 0)
                    pfo.ShowDialog();
                return;
            }   //  endif whichProcess
        }   //  end onButton3Click


        private void onButton4Click(object sender, EventArgs e)
        {
            if (!EnsureFileOpen()) { return; }

            if (whichProcess == 1)   //  equations
            {
                //  calls R8 volume equation entry
                R8VolEquation r8vol = Services.GetRequiredService<R8VolEquation>();
                r8vol.ShowDialog();

            }
            else if(whichProcess == 4)  //  output
            {
                //  calls routine to create CSV output file
                SelectCSV sc = Services.GetRequiredService<SelectCSV>();
                sc.setupDialog();
                sc.ShowDialog();
            }   //  endif whichProcess
        }   //  end onButton4Click


        private void onButton5Click(object sender, EventArgs e)
        {
            if (!EnsureFileOpen()) { return; }

            if (whichProcess == 1)   //  equations
            {
                //  calls R9 volume equation entry
                R9VolEquation r9vol = Services.GetRequiredService<R9VolEquation>();
                r9vol.setupDialog();
                r9vol.ShowDialog();
            }
            else if (whichProcess == 4)      //  output
            {
                //  calls routine to preview output file -- print preview
                PrintPreview p = Services.GetRequiredService<PrintPreview>();
                p.setupDialog();
                p.ShowDialog();
                return;
            }   //  endif whichProcess


        }   //  end onButton5Click

        private void onButton6Click(object sender, EventArgs e)
        {
            if (!EnsureFileOpen()) { return; }

            if (whichProcess == 4)  //  output
            {
                //  calls local volume routine
                //MessageBox.Show("Under Construction", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LocalVolume lv = Services.GetRequiredService<LocalVolume>();
                lv.setupDialog();
                lv.ShowDialog();
                return;
            }
            
        }   //  end onButton6Click

        private void onAboutClick(object sender, EventArgs e)
        {
            //  Show version number etc here
            MessageBox.Show("CruiseProcessing Version " + DateTime.Parse(AppVerson.ToString()).ToString("MM.dd.yyyy") + "\nForest Management Service Center\nFort Collins, Colorado", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }   //  end onAboutClick

        private void onModifyWeightFactors(object sender, EventArgs e)
        {
            int mResult = -1;
            ModifyWeightFactors mwf = Services.GetRequiredService<ModifyWeightFactors>();
            mResult = mwf.setupDialog();
            if(mResult == 1) mwf.ShowDialog();
            return;
        }
        
        private void onModMerchRules(object sender, EventArgs e)
        {
            ModifyMerchRules mmr = Services.GetRequiredService<ModifyMerchRules>();
            mmr.setupDialog();
            mmr.ShowDialog();
            //MessageBox.Show("Under construction", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        private bool doesCruiseHaveVolumeEquations()
        {
            List<VolumeEquationDO> myVEQList = DataLayer.getVolumeEquations();
            return myVEQList.Count > 0;
        }

    }
}
