using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using CruiseProcessing.Data;
using Microsoft.Extensions.DependencyInjection;

namespace CruiseProcessing
{
    public partial class ReportsDialog : Form
    {
        //TODO template flag
        public int templateFlag;
        public List<ReportsDO> reportList = new List<ReportsDO>();
        private List<ReportsDO> allReports = new List<ReportsDO>();

        public IServiceProvider Services { get; }
        protected CpDataLayer DataLayer { get; }


        protected ReportsDialog()
        {
            InitializeComponent();
        }

        public ReportsDialog(CpDataLayer dataLayer, IServiceProvider services)
            : this()
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        public void setupDialog()
        {
            //  Load selected reports list box
            foreach (ReportsDO rl in reportList)
                reportsSelected.Items.Add(rl.ReportID);

            //  disable certain buttons
            additionalData.Enabled = false;
            regionList.Enabled = false;
        }   //  end setupDialog


        private void AddOne_Click(object sender, EventArgs e)
        {
            int numItems = availableReports.SelectedItems.Count;
            for (int n = 0; n < numItems; n++)
            {
                string currentItem = availableReports.SelectedItems[n].ToString();
                int nthPosition = currentItem.IndexOf("-");
                currentItem = currentItem.Substring(0, nthPosition);
                // is it already in the list?  don't add
                if (reportsSelected.Items.Contains(currentItem))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Report ");
                    sb.Append(currentItem);
                    sb.Append(" is already in the selected reports list.\nIt was not added to the list.");
                    MessageBox.Show(sb.ToString(), "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                    reportsSelected.Items.Add(currentItem);
            }   //  end for n loop

            return;
        }   //  end AddOne_Click


        private void AddAll_Click(object sender, EventArgs e)
        {
            int numItems = availableReports.Items.Count;
            for (int k = 0; k < numItems; k++)
            {
                string currentItem = availableReports.Items[k].ToString();
                string currentReport = currentItem.Substring(0, currentItem.IndexOf("-"));
                reportsSelected.Items.Add(currentReport);
            }   //  end for k loop

            return;
        }   //  end AddAll_Click


        private void removeAll_Click(object sender, EventArgs e)
        {
            reportsSelected.Items.Clear();
        }   //  end removeAll_Click



        private void removeOne_Click(object sender, EventArgs e)
        {
            reportsSelected.Items.Remove(reportsSelected.SelectedItem);
        }   //  end removeOne_Click


        private void onSelectedIndexChanged(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            //  pull all reports for the group and display in available reports
            string selectedGroup = reportGroups.SelectedItem.ToString();
            if (selectedGroup.Substring(1, 1) == "-")
                selectedGroup = selectedGroup.Remove(1);
            else if (selectedGroup.Substring(2, 1) == "-")
                selectedGroup = selectedGroup.Remove(2);
            else if (selectedGroup.Substring(3, 1) == "-")
                selectedGroup = selectedGroup.Remove(3);

            //  display selected group reports
            if (selectedGroup == "")
            {
                //  have user select a group if it gets to here
                MessageBox.Show("Please select a Report Group", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }   //  endif selectedGroup

            if (selectedGroup == "R")
            {
                additionalData.Enabled = false;
                regionList.Enabled = true;
                availableReports.Items.Clear();
                return;
            }   //  endif selectedGroup is regional reports

            List<ReportsDO> allReports = DataLayer.GetReports();
            availableReports.Items.Clear();
            if (selectedGroup == "L")
            {
                // just log level reports -- the only report group with a one character category name
                string[] logReptNum = new string[4] { "L1", "L2", "L8", "L10" };
                string[] logReptTitle = new string[4] { "Log Grade File",
                                                        "Log Stock Table - MBF",
                                                        "Log Stock Table - Board and Cubic",
                                                        "Log Counts and Volume by Length and Species" };
                for (int j = 0; j < 4; j++)
                {
                    sb.Clear();
                    sb.Append(logReptNum[j]);
                    sb.Append("--");
                    sb.Append(logReptTitle[j]);
                    availableReports.Items.Add(sb.ToString());
                }   //  end for j loop
                return;
            }   //  endif group is log level reports

            var reportsArray = ReportsDataservice.reportsArray;
            for (int k = 0; k < reportsArray.GetLength(0); k++)
            {
                if (reportsArray[k, 0].StartsWith(selectedGroup))
                {
                    sb.Clear();
                    sb.Append(reportsArray[k, 0]);
                    sb.Append("--");
                    sb.Append(reportsArray[k, 1]);
                    availableReports.Items.Add(sb.ToString());
                }   //  endif
            }   //  end for k loop
            additionalData.Enabled = false;
            regionList.Enabled = false;

            //  turn on additional data button for specific report groups
            if (selectedGroup == "EX")
            {
                regionList.Enabled = false;
                additionalData.Enabled = true;

            }   //  endif

            return;
        }   //  end onSelectedIndexChanged


        private void onRegionSelectedIndexChanged(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            string reportToSelect = "";

            //  pull all reports for the region selected
            string selectedRegion = regionList.SelectedItem.ToString();
            if (selectedRegion == "1")
                reportToSelect = "R1";
            else if (selectedRegion == "2")
            {
                reportToSelect = "R2";
                additionalData.Enabled = true;
            }
            else if (selectedRegion == "3")
                reportToSelect = "R3";
            else if (selectedRegion == "4")
                reportToSelect = "R4";
            else if (selectedRegion == "5")
                reportToSelect = "R5";
            else if (selectedRegion == "6")
                reportToSelect = "R6";
            else if (selectedRegion == "8")
                reportToSelect = "R8";
            else if (selectedRegion == "9")
                reportToSelect = "R9";
            else if (selectedRegion == "10")
            {
                //  if region 10 ever gets more than nine reports, this will have to change somehow
                reportToSelect = "R00";
                additionalData.Enabled = true;
            }   //  endif
            else if (selectedRegion == "IDL")
            {
                availableReports.Items.Clear();
                availableReports.Items.Add("IDL1--Idaho Dept of Lands Summary of Cruise Data");
                return;
            }   //  endif selectedRegion

            allReports = DataLayer.GetReports();
            List<ReportsDO> groupList = allReports.FindAll(
                delegate (ReportsDO rl)
                {
                    return rl.ReportID.Contains(reportToSelect);
                });
            availableReports.Items.Clear();
            foreach (ReportsDO gl in groupList)
            {
                sb.Clear();
                sb.Append(gl.ReportID);
                sb.Append("--");
                sb.Append(gl.Title);
                availableReports.Items.Add(sb.ToString());
            }   //  end foreach
            return;
        }   //  end onRegionSelectedIndexChanged

        private void onAdditional(object sender, EventArgs e)
        {
            if (regionList.SelectedItem.ToString() == "10")
            {
                //  get correct password before continuing
                PasswordProtect pp = Services.GetRequiredService<PasswordProtect>();
                pp.ShowDialog();
                if (pp.passwordResponse != "OK")
                {
                    Close();
                    return;
                }   //  endif

                string currentSale = " ";
                string currentSaleNum = " ";
                if (templateFlag != 1)
                {
                    var sale = DataLayer.GetSale();
                    currentSale = sale.Name;
                    currentSaleNum = sale.SaleNumber;
                }   //  endif
                List<LogMatrixDO> checkMatrix = new List<LogMatrixDO>();
                try
                {
                    // is it empty?
                    checkMatrix = DataLayer.getLogMatrix("R008");
                    if (checkMatrix.Count == 0)
                    {
                        //  load default matrix for both reports
                        checkMatrix.Clear();
                        checkMatrix = loadDefaultMatrix(currentSale, currentSaleNum);
                        //  save default matrix
                        DataLayer.SaveLogMatrix(checkMatrix, "");
                    }   //  endif
                }
                catch
                {
                    //   need to create the table and load the default
                    int iDone = DataLayer.CreateNewTable("LogMatrix");
                    checkMatrix = loadDefaultMatrix(currentSale, currentSaleNum);
                    //  save default matrix
                    DataLayer.SaveLogMatrix(checkMatrix, "");
                }   //  endif

                //  see if log matrix table needs to be updated
                List<LogMatrixDO> reportMatrix = new List<LogMatrixDO>();
                DialogResult dr = MessageBox.Show("Reports R008 and R009 use a log matrix.\nDo you want to update the matrix now?\nNOTE: If the Log Matrix does not exist, the default matrix is used.", "QUESTION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    DialogResult d8 = MessageBox.Show("The log matrix is different for each report.\nUpdate R008?", "QUESTION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (d8 == DialogResult.Yes)
                    {
                        reportMatrix = DataLayer.getLogMatrix("R008");
                        LogMatrixUpdate lmu = Services.GetRequiredService<LogMatrixUpdate>();
                        lmu.reportMatrix = reportMatrix;
                        lmu.currSaleName = currentSale;
                        lmu.currSaleNumber = currentSaleNum;
                        lmu.currReport = "R008";
                        lmu.setupDialog();
                        lmu.ShowDialog();
                        reportMatrix = lmu.reportMatrix;
                        DataLayer.SaveLogMatrix(reportMatrix, "R008");

                        //  need to update R009?
                        DialogResult d9 = MessageBox.Show("Update R009?", "QUESTION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (d9 == DialogResult.Yes)
                        {
                            //  retrieve R009 matrix and update
                            reportMatrix = DataLayer.getLogMatrix("R009");
                            LogMatrixUpdate lmx = Services.GetRequiredService<LogMatrixUpdate>();
                            lmx.reportMatrix = reportMatrix;
                            lmx.currSaleNumber = currentSale;
                            lmx.currSaleNumber = currentSaleNum;
                            lmx.currReport = "R009";
                            lmx.setupDialog();
                            lmx.ShowDialog();
                            int rtnResult = lmx.returnValue;
                            if (rtnResult == 1)
                            {
                                reportMatrix = lmx.reportMatrix;
                                DataLayer.SaveLogMatrix(reportMatrix, "R009");
                            }   //  endif
                        }       //  endif
                    }
                    else if (d8 == DialogResult.No)
                    {
                        DialogResult d9 = MessageBox.Show("Update R009?", "QUESTION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (d9 == DialogResult.Yes)
                        {
                            //  retrieve R009 matrix and update
                            reportMatrix = DataLayer.getLogMatrix("R009");
                            LogMatrixUpdate lmx = Services.GetRequiredService<LogMatrixUpdate>();
                            lmx.reportMatrix = reportMatrix;
                            lmx.currSaleNumber = currentSale;
                            lmx.currSaleNumber = currentSaleNum;
                            lmx.currReport = "R009";
                            lmx.setupDialog();
                            lmx.ShowDialog();
                            int rtnResult = lmx.returnValue;
                            if (rtnResult == 1)
                            {
                                reportMatrix = lmx.reportMatrix;
                                DataLayer.SaveLogMatrix(reportMatrix, "R009");
                            }   //  endif
                        }       //  endif
                    }   //  endif
                }   //  endif
            }   //  endif

            if (availableReports.Items[0].ToString().Substring(0, 2) == "EX")
            {
                // not yet tested --  January 2012
                MessageBox.Show("Under Construction", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                additionalData.Enabled = false;
                //additionalData.Enabled = true;
                return;
                /*
                                //  display dialog for capturing export grade values
                                //  first need region number
                                string currentRegion = "";
                                currentRegion = bslyr.findSingleField("Region", "Sale");

                                //  need dialog object
                                ExportDialog ed = new ExportDialog();
                                //  Then does the ExportValues table exist?  Need to fill list with defaults for all regions or Region 10
                                bool tableExists = bslyr.doesTableExist("ExportValues");
                                if (tableExists == false)
                                {
                                    //  need to create the table
                                    int nResult = bslyr.CreateNewTable("ExportValues");
                                    //  then get defaults to load into dialog for user to update
                                    exportGrades eg = new exportGrades();
                                    //  sort list first
                                    List<exportGrades> egSortList = eg.createDefaultList(currentRegion, "sort");
                                    List<exportGrades> egGradeList = eg.createDefaultList(currentRegion, "grade");

                                    ed.setupDialog(egSortList, egGradeList);
                                }
                                else if (tableExists == true)
                                {
                                    //  just get data in the table for display
                                    List<exportGrades> egList = bslyr.GetExportGrade();
                                    List<exportGrades> dummyList = new List<exportGrades>();
                                    ed.setupDialog(egList,dummyList);
                                }   //  endif tableExists
                                ed.fileName = fileName;
                                ed.tableExists = tableExists;
                                ed.ShowDialog();

                                additionalData.Enabled = false;
                 */
            }

        }   //  end onAdditional

        private void onCancel(object sender, EventArgs e)
        {
            // Are you sure sure sure?
            DialogResult dr = MessageBox.Show("Are you sure you want to cancel?\nAny changes made will not be saved.", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr == DialogResult.Yes)
            {
                Close();
                return;
            }   //  endif
        }   //  end onCancel

        private void onFinished(object sender, EventArgs e)
        {

            //  reset selected to zero before updating
            foreach (ReportsDO rl in reportList)
            {
                rl.Selected = false;
            }   //  end foreach

            //  update selected reports and store in database
            //  check for no reports selected
            if (reportsSelected.Items.Count <= 0)
            {
                DialogResult dr = MessageBox.Show("NO REPORTS SELECTED\nOK to Exit?", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    Close();
                    return;
                }
                else if (dr == DialogResult.No)
                    return;

            }   //  endif no reports selected

            for (int k = 0; k < reportsSelected.Items.Count; k++)
            {
                string sReport = reportsSelected.Items[k].ToString();
                int ithRow = reportList.FindIndex(
                    delegate (ReportsDO rl)
                    {
                        return rl.ReportID == sReport;
                    });
                if (ithRow >= 0)
                    reportList[ithRow].Selected = true;
                else if (ithRow < 0)     //  added report -- update report list
                {
                    ReportsDO rd = new ReportsDO();
                    rd.ReportID = sReport;
                    rd.Selected = true;
                    reportList.Add(rd);
                }   //  endif ithRow
            }   //  end foreach loop

            //  Update reports table in database
            DataLayer.updateReports(reportList);
            Close();
            return;
        }   //  end onFinished


        private List<LogMatrixDO> loadDefaultMatrix(string currSaleName, string currSaleNumber)
        {
            //  R008/R009
            //  This loads the default into the database NOT into the update dialog.
            //  Load and save database table
            List<LogMatrixDO> newLogMatrix = new List<LogMatrixDO>();
            ArrayList parsedList = new ArrayList();
            LogMatrix dLists = new LogMatrix();
            //  report R008
            for (int j = 0; j < dLists.sortDescrip008.Count(); j++)
            {
                LogMatrixDO lmx = new LogMatrixDO();
                lmx.ReportNumber = "R008";
                lmx.LogSortDescription = dLists.sortDescrip008[j];
                lmx.Species = dLists.species008[j];
                parsedList = parseOnBlank(dLists.grade008[j]);
                StoreGrade(lmx, parsedList);
                parsedList = parseOnBlank(dLists.diameter008[j]);
                StoreDiameter(lmx, parsedList);
                newLogMatrix.Add(lmx);
            }   //  end for j loop
            //  report R009
            for (int j = 0; j < dLists.sortDescrip009.Count(); j++)
            {
                LogMatrixDO lmx = new LogMatrixDO();
                lmx.ReportNumber = "R009";
                lmx.LogSortDescription = dLists.sortDescrip009[j];
                lmx.Species = dLists.species009[j];
                parsedList = parseOnBlank(dLists.grade009[j]);
                StoreGrade(lmx, parsedList);
                parsedList = parseOnBlank(dLists.diameter009[j]);
                StoreDiameter(lmx, parsedList);
                newLogMatrix.Add(lmx);
            }   //  end for j loop

            return newLogMatrix;
        }   //  end loadDefaultMatrix


        private ArrayList parseOnBlank(string oneLine)
        {
            ArrayList listToReturn = new ArrayList();
            StringBuilder sb = new StringBuilder();
            //  strip off blanks to the right first
            oneLine = oneLine.TrimEnd(' ');
            oneLine = oneLine.TrimEnd('+');
            oneLine = oneLine.TrimStart(' ');
            int iLength = oneLine.Length;
            if (iLength == 1)
                listToReturn.Add(oneLine);
            else
            {
                //  parse line
                for (int i = 0; i < iLength; i++)
                {
                    if (oneLine.Substring(i, 1) == " ")
                    {
                        listToReturn.Add(sb.ToString());
                        sb.Clear();
                    }
                    else sb.Append(oneLine.Substring(i, 1));
                }   //  end for i loop
                //  add last string to parsed list
                listToReturn.Add(sb.ToString());
            }   //  endif
            return listToReturn;
        }   //  end parseOnBlank


        private void StoreGrade(LogMatrixDO currLMX, ArrayList currPL)
        {
            //  stores parsed grade for R008/R009
            if (currPL.Count == 1)
            {
                currLMX.GradeDescription = "";
                currLMX.LogGrade1 = currPL[0].ToString();
            }
            else
            {
                switch (currPL.Count)
                {
                    case 2:
                        currLMX.LogGrade1 = currPL[0].ToString();
                        currLMX.GradeDescription = currPL[1].ToString();
                        break;
                    case 3:
                        currLMX.LogGrade1 = currPL[0].ToString();
                        currLMX.GradeDescription = currPL[1].ToString();
                        currLMX.LogGrade2 = currPL[2].ToString();
                        break;
                    case 5:
                        currLMX.LogGrade1 = currPL[0].ToString();
                        currLMX.GradeDescription = currPL[1].ToString();
                        currLMX.LogGrade2 = currPL[2].ToString();
                        currLMX.LogGrade3 = currPL[4].ToString();
                        break;
                    case 7:
                        currLMX.LogGrade1 = currPL[0].ToString();
                        currLMX.GradeDescription = currPL[1].ToString();
                        currLMX.LogGrade2 = currPL[2].ToString();
                        currLMX.LogGrade3 = currPL[4].ToString();
                        currLMX.LogGrade4 = currPL[6].ToString();
                        break;
                }   //  end switch
            }   //  endif
            return;
        }   //  end StoreGrade

        private void StoreDiameter(LogMatrixDO currLMX, ArrayList currPL)
        {
            //  stores parsed diameters for R008/R009
            if (currPL.Count == 1)
            {
                if (currPL[0].ToString() == "")
                    currLMX.SEDminimum = 0;
                else currLMX.SEDminimum = Convert.ToDouble(currPL[0]);
            }
            else if (currPL.Count == 0)
                currLMX.SEDminimum = 0;
            else
            {
                switch (currPL.Count)
                {
                    case 2:
                        currLMX.SEDlimit = currPL[0].ToString();
                        currLMX.SEDminimum = Convert.ToDouble(currPL[1]);
                        break;
                    case 3:
                        if (currPL[0].ToString() == "greater" ||
                            currPL[0].ToString() == "less")
                        {
                            currLMX.SEDlimit = currPL[0].ToString();
                            currLMX.SEDlimit += " ";
                            currLMX.SEDlimit += currPL[1].ToString();
                            currLMX.SEDminimum = Convert.ToDouble(currPL[2]);
                        }
                        else
                        {
                            currLMX.SEDminimum = Convert.ToDouble(currPL[0]);
                            currLMX.SEDlimit = currPL[1].ToString();
                            currLMX.SEDmaximum = Convert.ToDouble(currPL[2]);
                        }   //  endif
                        break;
                    case 4:
                        currLMX.SEDlimit = currPL[0].ToString();
                        currLMX.SEDminimum = Convert.ToDouble(currPL[1]);
                        currLMX.SEDmaximum = Convert.ToDouble(currPL[3]);
                        break;
                }   //  end switch   
            }   // endif
            return;
        }   //  end StoreDiameter
    }
}
