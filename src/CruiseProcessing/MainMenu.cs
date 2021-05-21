using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL;

namespace CruiseProcessing
{
    public partial class MainMenu : Form
    {
        #region
        public string fileName;
        public string newTemplateFile;
        public int templateFlag;
        public int whichProcess;
        string currentRegion;
        public DAL DAL { get; set; }
        //CPbusinessLayer bslyr = new CPbusinessLayer();
        //public string[,] addedReports = new string[,] { {"A14", "Summary of Species Based on Unit Level Data" },
          //                                              {"R208", "Stewardship Average Product Cost"},
            //                                            {"LV01","Volume Summary for Sample Group"},
              //                                          {"LV02","Volume Summary for Strata"},
                //                                        {"LV03","Net Volume Statistics for Sample Group"},
                  //                                      {"LV04","Gross Volume Statistics for Sample Group"},
                    //                                    {"LV05","Volume by Species within Cutting Unit Across All Stratum"}};
        #endregion

        public MainMenu()
        {
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
            menuButton4.BackgroundImage = Properties.Resources.disabled_button;
            menuButton5.BackgroundImage = Properties.Resources.disabled_button;
            
            menuButton2.Enabled = false;
            menuButton3.Enabled = false;
            menuButton4.Enabled = false;
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
            ProcessStatus statusDlg = new ProcessStatus();
            //statusDlg.bslyr = bslyr;
            statusDlg.DAL = DAL;
            statusDlg.fileName = fileName;
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
                processLabel1.Visible = false;
                processLabel2.Visible = true;
                //processLabel3.Visible = true;
                processLabel4.Visible = false;
                processLabel5.Visible = true;
                processLabel6.Visible = false;
                processButton1.Visible = false;
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
            //  clear filename in case user wants to change files
            fileName = "";
            //  Create an instance of the open file dialog
            OpenFileDialog browseDialog = new OpenFileDialog();

            //  Set filter options and filter index
            browseDialog.Filter = "Cruise files (.cruise)|*.cruise|(.CRUIISE)|*.CRUISE|(.crz3)|*.crz3|(.CRZ3)|*.CRZ3|All Files (*.*)|*.*";
            browseDialog.FilterIndex = 1;

            browseDialog.Multiselect = false;

            //  February 2017 -- nowhave the option to edit volume equations and selected reports
            //  on template files.  Need a flag indicating it is a template file so some
            //  tasks are skipped that would apply to just cruise files.
            templateFlag = 0;
            //  capture filename selected
            while (fileName == "" || fileName == null)
            {
                DialogResult dResult = browseDialog.ShowDialog();

                if (dResult == DialogResult.Cancel)
                {
                    DialogResult dnr = MessageBox.Show("No filename selected.  Do you really want to cancel?", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dnr == DialogResult.Yes)
                        return;
                }
                if (dResult == DialogResult.OK)
                {
                    fileName = browseDialog.FileName;
                    if (fileName.EndsWith(".cut") || fileName.EndsWith(".CUT"))
                    {
                        //  disable all buttons except equations and reports
                        menuButton2.BackgroundImage = Properties.Resources.button_image;
                        menuButton3.BackgroundImage = Properties.Resources.button_image;
                        menuButton4.BackgroundImage = Properties.Resources.disabled_button;
                        menuButton5.BackgroundImage = Properties.Resources.disabled_button;
                        menuButton2.Enabled = true;
                        menuButton3.Enabled = true;
                        menuButton4.Enabled = false;
                        menuButton5.Enabled = false;

                        //  Have user enter a different file to preserve the regional tempalte file
                        DialogResult dr = MessageBox.Show("Do you want to use a different filename for any changes made?", "QUESTION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr == DialogResult.Yes)
                        {
                            
                            //  show dialog to captue new filanem
                            EnterNewFilename enf = new EnterNewFilename();
                            enf.ShowDialog();
                            //  copy original file to new filename
                            string pathName = Path.GetDirectoryName(fileName);
                            pathName += "\\";
                            newTemplateFile = enf.templateFilename;
                            newTemplateFile = newTemplateFile.Insert(0, pathName);
                            File.Copy(fileName, newTemplateFile, true);
                            fileName = newTemplateFile;
                            DAL = new DAL(newTemplateFile);
                            templateFlag = 1;
                        }
                        else if (dr == DialogResult.No)
                        {
                            //fileName = fileName;
                            DAL = new DAL(fileName);
                            if (fileName.EndsWith(".CUT") || fileName.EndsWith(".cut"))
                            {
                                newTemplateFile = fileName;
                                templateFlag = 1;
                            }
                            else templateFlag = 0;
                        }   //  endif

                    }
                    else if (!fileName.EndsWith(".cruise") && !fileName.EndsWith(".CRUISE"))
                    {
                        if (!fileName.EndsWith(".crz3") && !fileName.EndsWith(".CRZ3"))
                        {
                            //  is it a cruise file?
                            MessageBox.Show("File selected is not a cruise file.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else
                        {
                            var v3Database = new CruiseDatastore_V3(fileName);

                            string V2FileName = fileName.Replace(".crz3", "").Replace(".CRZ3", "");

                            //V2FileName = V2FileName + "WBTest.cruise";
                            V2FileName = V2FileName + ".process";

                            try
                            {
                                var cruiseIDs = v3Database.QueryScalar<string>("SELECT CruiseID FROM Cruise;").ToArray();
                                if (cruiseIDs.Length > 1)
                                {
                                    MessageBox.Show("File contains multiple cruises. \r\nOpening files with multiple cruises is not supported yet", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                var cruiseID = cruiseIDs.First();

                                string cruiseV2Path = V2FileName;


                                CruiseDatastoreBuilder_V2 V2Builder = new CruiseDatastoreBuilder_V2();
                                Updater_V2 v2Updater = new Updater_V2();

                                CruiseDatastore myV2DAL = new DAL(cruiseV2Path, true);
                                //CruiseDatastore v2DB = new CruiseDatastore(cruiseV2Path, true, V2Builder, v2Updater);

                                DownMigrator myMyigrator = new DownMigrator();


                                //CONVERT LOGIC NEEDED HERE.                                                
                                myMyigrator.MigrateFromV3ToV2(cruiseID, v3Database, myV2DAL);


                                fileName = V2FileName;
                            }
                            catch(Exception ex)
                            {
                                MessageBox.Show("This version 3 file has issues. \r\n", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                      
                    }   //  endif            
                }
            };  //  end while

            if (templateFlag == 0)
            {
                //  check for fatal errors before doing anything else --  October 2014
                //fileName = fileName;
                DAL = new DAL(fileName);
                
                //open connection forces the connection to remain open not to close and open.  Might be good to re-work the process button click?
                DAL.OpenConnection();

                string[] errors;
 //               bool ithResult = bslyr.DAL.HasCruiseErrors(out errors);
                bool ithResult = false;
                if (ithResult)
                {
                    MessageBox.Show("This file has errors which affect processing.\nCannot continue until these are fixed in CruiseManager.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    menuButton2.BackgroundImage = Properties.Resources.disabled_button;
                    menuButton3.BackgroundImage = Properties.Resources.disabled_button;
                    menuButton4.BackgroundImage = Properties.Resources.disabled_button;
                    menuButton5.BackgroundImage = Properties.Resources.disabled_button;
                    menuButton2.Enabled = false;
                    menuButton3.Enabled = false;
                    menuButton4.Enabled = false;
                    menuButton5.Enabled = false;
                    return; 
                }   //  end check for fatal errors

                // if filename is not blank, enable remaining menu buttons
                if (fileName.Contains(".cruise") || fileName.Contains(".CRUISE") || fileName.Contains(".process") || fileName.Contains(".PROCESS"))
                {
                    menuButton2.BackgroundImage = Properties.Resources.button_image;
                    menuButton3.BackgroundImage = Properties.Resources.button_image;
                    menuButton4.BackgroundImage = Properties.Resources.button_image;
                    menuButton5.BackgroundImage = Properties.Resources.button_image;

                    menuButton2.Enabled = true;
                    menuButton3.Enabled = true;
                    menuButton4.Enabled = true;
                    menuButton5.Enabled = true;

                    //  need region number in order to hide volume button as well as region 9 button
                    List<SaleDO> saleList = new List<SaleDO>();
                    saleList = DAL.From<SaleDO>().Read().ToList();
                    currentRegion = saleList[0].Region; 
                }   //  endif
            }   //  endif tempalteFlag
            //  add file name to title line at top
            if (fileName.Length > 35)
            {
                string tempName = "..." + fileName.Substring(fileName.Length - 35, 35);
                Text = tempName;
            }   //  endif fileName

        }   //  end onFile


        private void onButton1Click(object sender, EventArgs e)
        {
            CPbusinessLayer bslyr = new CPbusinessLayer();
            bslyr.DAL = DAL;

            if (whichProcess == 1)       //  equations
            {
                VolumeEquations volEqObj = new VolumeEquations();
                volEqObj.bslyr.fileName = fileName;
                volEqObj.bslyr.DAL = DAL;
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
                if (templateFlag == 1)
                {
                    bslyr.fileName = newTemplateFile;
                    bslyr.DAL = new DAL(newTemplateFile);
                }
                else bslyr.fileName = fileName;
                //  get all reports
                currentReports = bslyr.GetReports();
                //  and get the all reports array
                allReportsArray ara = new allReportsArray();
                //  then check for various conditions to know what to do with the reports list
                if (currentReports.Count == 0)
                {
                    currentReports = ReportMethods.fillReportsList();
                    bslyr.SaveReports(currentReports);
                }
                else if (currentReports.Count < ara.reportsArray.GetLength(0))
                {
                    //  old or new list?  Check title
                    if (currentReports[0].Title == "" || currentReports[0].Title == null)
                    {
                        //  old reports -- update list
                        currentReports = ReportMethods.updateReportsList(currentReports, ara);
                        bslyr.SaveReports(currentReports);
                    }
                    else
                    {
                        //  new reports -- just add
                        currentReports = ReportMethods.addReports(currentReports, ara);
                        bslyr.SaveReports(currentReports);
                    }   //  endif
                }   //  endif
                //  now get reports selected
                currentReports = ReportMethods.deleteReports(currentReports, bslyr);
                currentReports = bslyr.GetSelectedReports();
                //  Get selected reports 
                ReportsDialog rd = new ReportsDialog();
                rd.fileName = fileName;
                rd.bslyr.fileName = bslyr.fileName;
                rd.bslyr.DAL = bslyr.DAL;
                rd.reportList = currentReports;
                rd.templateFlag = templateFlag;
                rd.setupDialog();
                rd.ShowDialog();
            }
            else if(whichProcess == 4)  //  output
            {
                
                //  Pull reports selected
                bslyr.fileName = fileName;
                //  See if volume has been calculated (sum expansion factor since those are calculated before volume)
                //  July 2014 -- However it looks like expansion factors could be present but volume is not
                //  need to pull calculated values as well and sum net volumes
                List<TreeDO> tList = bslyr.getTrees();
                double summedEF = tList.Sum(t => t.ExpansionFactor);
                List<TreeCalculatedValuesDO> tcvList = bslyr.getTreeCalculatedValues();
                double summedNetBDFT = tcvList.Sum(tc => tc.NetBDFTPP);
                double summedNetCUFT = tcvList.Sum(tc => tc.NetCUFTPP);
                if (summedEF == 0 && summedNetBDFT == 0 && summedNetCUFT == 0)
                {
                    MessageBox.Show("Looks like volume has not been calculated.\nReports cannot be produced without calculated volume.\nPlease calculate volume before continuing.", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }   //  endif no volume for reports
                List<ReportsDO> selectedReports = bslyr.GetSelectedReports(); 

                //  no reports?  let user know to go back and select reports
                if (selectedReports.Count == 0)
                {
                    MessageBox.Show("No reports selected.\nReturn to Reports section and select reports.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }   //  endif no reports

                //  Show dialog creating text file
                TextFileOutput tfo = new TextFileOutput();
                tfo.selectedReports = selectedReports;
                tfo.fileName = fileName;
                tfo.currRegion = currentRegion;
                tfo.bslyr = bslyr;
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
            //  Cannot continue if filename is blank
            if (fileName == "")
            {
                MessageBox.Show("No filename selected.  Cannot continue.\nPlease select a filename.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }   //  endif fileName

            if(whichProcess == 1)   //  equations
            {
                ValueEquations valEqObj = new ValueEquations();
                valEqObj.bslyr.fileName = fileName;
                valEqObj.bslyr.DAL = DAL;
                int nResult = valEqObj.setupDialog();
                if(nResult == 1)
                    valEqObj.ShowDialog();
            }
            else if(whichProcess == 2)  //  reports
            {
                //  calls routine to add graphical reports
                GraphReportsDialog grd = new GraphReportsDialog();
                grd.bslyr.fileName = fileName;
                grd.bslyr.DAL = DAL;
                grd.setupDialog();
                grd.ShowDialog();
                return;
            }
            else if(whichProcess == 4)  // output
            {
                //  calls routine to create an html output file
                HTMLoutput ho = new HTMLoutput();
                ho.fileName = fileName;
                ho.bslyr.fileName = fileName;
                ho.bslyr.DAL = DAL;
                ho.CreateHTMLfile();
                return;
            }   //  endif whichProcess

        }   //  end onButton2Click

        
        private void onButton3Click(object sender, EventArgs e)
        {
            //  Cannot continue if filename is blank
            if (fileName == "")
            {
                MessageBox.Show("No filename selected.  Cannot continue.\nPlease select a filename.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }   //  endif fileName

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
            if(whichProcess == 4)  //  output
            {
                //  calls routine to create pdf file
                PDFfileOutput pfo = new PDFfileOutput();
                pfo.fileName = fileName;
                pfo.bslyr.fileName = fileName;
                pfo.bslyr.DAL = DAL;
                int nResult = pfo.setupDialog();
                if(nResult == 0)
                    pfo.ShowDialog();
                return;
            }   //  endif whichProcess
        }   //  end onButton3Click


        private void onButton4Click(object sender, EventArgs e)
        {
            //  Cannot continue if filename is blank
            if (fileName == "")
            {
                MessageBox.Show("No filename selected.  Cannot continue.\nPlease select a filename.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }   //  endif fileName

            if(whichProcess == 1)   //  equations
            {
                //  calls R8 volume equation entry
                R8VolEquation r8vol = new R8VolEquation();
                r8vol.bslyr.fileName = fileName;
                r8vol.fileName = fileName;
                r8vol.bslyr.DAL = DAL;
                r8vol.ShowDialog();

            }
            else if(whichProcess == 4)  //  output
            {
                //  calls routine to create CSV output file
                SelectCSV sc = new SelectCSV();
                sc.fileName = fileName;
                sc.bslyr.fileName = fileName;
                sc.bslyr.DAL = DAL;
                sc.setupDialog();
                sc.ShowDialog();
            }   //  endif whichProcess
        }   //  end onButton4Click


        private void onButton5Click(object sender, EventArgs e)
        {
            //  Cannot continue if filename is blank
            if (fileName == "")
            {
                MessageBox.Show("No filename selected.  Cannot continue.\nPlease select a filename.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }   //  endif fileName

            if (whichProcess == 1)   //  equations
            {
                //  calls R9 volume equation entry
                R9VolEquation r9vol = new R9VolEquation();
                r9vol.fileName = fileName;
                r9vol.bslyr.fileName = fileName;
                r9vol.bslyr.DAL = DAL;
                r9vol.setupDialog();
                r9vol.ShowDialog();
            }
            else if (whichProcess == 4)      //  output
            {
                //  calls routine to preview output file -- print preview
                PrintPreview p = new PrintPreview();
                p.fileName = fileName;
                p.setupDialog();
                p.ShowDialog();
                return;
            }   //  endif whichProcess


        }   //  end onButton5Click

        private void onButton6Click(object sender, EventArgs e)
        {
            //  Cannot continue if filename is blank
            if (fileName == "")
            {
                MessageBox.Show("No filename selected.  Cannot continue.\nPlease select a filename.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }   //  endif fileName

            if(whichProcess == 4)  //  output
            {
                //  calls local volume routine
                //MessageBox.Show("Under Construction", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
           
                LocalVolume lv = new LocalVolume();
                lv.fileName = fileName;
                lv.bslyr.fileName = fileName;
                lv.bslyr.DAL = DAL;
                lv.setupDialog();
                lv.ShowDialog();
                return;
            }
            
        }   //  end onButton6Click

        private void onAboutClick(object sender, EventArgs e)
        {
            //  Show version number etc here
            MessageBox.Show("CruiseProcessing Version 2021.06.01\nForest Management Service Center\nFort Collins, Colorado", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }   //  end onAboutClick

        private void onModifyWeightFactors(object sender, EventArgs e)
        {
            int mResult = -1;
            ModifyWeightFactors mwf = new ModifyWeightFactors();
            mwf.bslyr.fileName = fileName;
            mwf.bslyr.DAL = DAL;
            mResult = mwf.setupDialog();
            if(mResult == 1) mwf.ShowDialog();
            return;
        }
        
        private void onModMerchRules(object sender, EventArgs e)
        {
            ModifyMerchRules mmr = new ModifyMerchRules();
            mmr.bslyr.fileName = fileName;
            mmr.bslyr.DAL = DAL;
            mmr.setupDialog();
            mmr.ShowDialog();
            //MessageBox.Show("Under construction", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

    }
}
