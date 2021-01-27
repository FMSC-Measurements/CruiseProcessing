using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CruiseProcessing
{
    public partial class ExportDialog : Form
    {
        #region
        public string fileName;
        public bool tableExists;
        #endregion


        public ExportDialog()
        {
            InitializeComponent();
        }


        public void setupDialog(List<exportGrades> sortList, List<exportGrades> gradeList)
        {
            //  add headers to each grid
            sortGridHeader();
            gradeGridHeader();

            //  First, are the views separate?
            if (gradeList.Count() == 0)
            {
                //  not separate
                //  load both views for sortList
                int rowNum = 0;
                foreach (exportGrades eg in sortList)
                {
                    if (eg.exportSort != "")
                    {
                        logSortCodes.Rows[rowNum].Cells[0].Value = eg.exportSort.ToString();
                        logSortCodes.Rows[rowNum].Cells[1].Value = eg.exportCode.ToString();
                        logSortCodes.Rows[rowNum].Cells[2].Value = eg.exportName.ToString();
                        logSortCodes.Rows[rowNum].Cells[3].Value = eg.minDiam.ToString();
                        logSortCodes.Rows[rowNum].Cells[4].Value = eg.minLength.ToString();
                        logSortCodes.Rows[rowNum].Cells[5].Value = eg.minBDFT.ToString();
                        logSortCodes.Rows[rowNum].Cells[6].Value = eg.maxDefect.ToString();
                        rowNum++;
                    }
                    else if(eg.exportGrade != "")
                    {
                        logGradeCodes.Rows[rowNum].Cells[0].Value = eg.exportGrade.ToString();
                        logGradeCodes.Rows[rowNum].Cells[1].Value = eg.exportCode.ToString();
                        logGradeCodes.Rows[rowNum].Cells[2].Value = eg.exportName.ToString();
                        logGradeCodes.Rows[rowNum].Cells[3].Value = eg.minDiam.ToString();
                        logGradeCodes.Rows[rowNum].Cells[4].Value = eg.minLength.ToString();
                        logGradeCodes.Rows[rowNum].Cells[5].Value = eg.minBDFT.ToString();
                        logGradeCodes.Rows[rowNum].Cells[6].Value = eg.maxDefect.ToString();
                        rowNum++;
                    }   // endif
                }   //  end foreach
            }
            else
            {
                // lists are separate
                int rowNum = 0;
                foreach (exportGrades seg in sortList)
                {
                    logSortCodes.Rows[rowNum].Cells[0].Value = seg.exportSort.ToString();
                    logSortCodes.Rows[rowNum].Cells[1].Value = seg.exportCode.ToString();
                    logSortCodes.Rows[rowNum].Cells[2].Value = seg.exportName.ToString();
                    logSortCodes.Rows[rowNum].Cells[3].Value = seg.minDiam.ToString();
                    logSortCodes.Rows[rowNum].Cells[4].Value = seg.minLength.ToString();
                    logSortCodes.Rows[rowNum].Cells[5].Value = seg.minBDFT.ToString();
                    logSortCodes.Rows[rowNum].Cells[6].Value = seg.maxDefect.ToString();
                    rowNum++;
                }   //  end foreach loop on sortList

                rowNum = 0;
                foreach (exportGrades geg in gradeList)
                {
                    logGradeCodes.Rows[rowNum].Cells[0].Value = geg.exportGrade.ToString();
                    logGradeCodes.Rows[rowNum].Cells[1].Value = geg.exportCode.ToString();
                    logGradeCodes.Rows[rowNum].Cells[2].Value = geg.exportName.ToString();
                    logGradeCodes.Rows[rowNum].Cells[3].Value = geg.minDiam.ToString();
                    logGradeCodes.Rows[rowNum].Cells[4].Value = geg.minLength.ToString();
                    logGradeCodes.Rows[rowNum].Cells[5].Value = geg.minBDFT.ToString();
                    logGradeCodes.Rows[rowNum].Cells[6].Value = geg.maxDefect.ToString();
                    rowNum++;
                }   //  end foreach loop on gradeList
            }   //  endif gradeList is empty
            return;
        }   //  end setupDialog


        private void onCancel(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to cancel?\nAny changes made will not be saved.", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr == DialogResult.Yes)
            {
                Close();
                return;
            }
        }   //  end onCancel

        private void onFinished(object sender, EventArgs e)
        {
            //  stuff each grid view into appropriate lists
            List<exportGrades> sortList = new List<exportGrades>();
            List<exportGrades> gradeList = new List<exportGrades>();
            exportGrades eg = new exportGrades();
            for (int k = 0; k < logSortCodes.RowCount - 1; k++)
            {
                eg.exportSort = logSortCodes.Rows[k].Cells[0].Value.ToString();
                eg.exportCode = logSortCodes.Rows[k].Cells[1].Value.ToString();
                eg.exportName = logSortCodes.Rows[k].Cells[2].Value.ToString();
                eg.minDiam = Convert.ToDouble(logSortCodes.Rows[k].Cells[3].Value.ToString());
                eg.minLength = Convert.ToDouble(logSortCodes.Rows[k].Cells[4].Value.ToString());
                eg.minBDFT = Convert.ToDouble(logSortCodes.Rows[k].Cells[5].Value.ToString());
                eg.maxDefect = Convert.ToDouble(logSortCodes.Rows[k].Cells[6].Value.ToString());
                sortList.Add(eg);
            }   //  end for k loop

            for (int k = 0; k < logGradeCodes.RowCount - 1; k++)
            {
                eg.exportGrade = logGradeCodes.Rows[k].Cells[0].Value.ToString();
                eg.exportCode = logGradeCodes.Rows[k].Cells[1].Value.ToString();
                eg.exportName = logGradeCodes.Rows[k].Cells[2].Value.ToString();
                eg.minDiam = Convert.ToDouble(logGradeCodes.Rows[k].Cells[3].Value.ToString());
                eg.minLength = Convert.ToDouble(logGradeCodes.Rows[k].Cells[4].Value.ToString());
                eg.minBDFT = Convert.ToDouble(logGradeCodes.Rows[k].Cells[5].Value.ToString());
                eg.maxDefect = Convert.ToDouble(logGradeCodes.Rows[k].Cells[6].Value.ToString());
                gradeList.Add(eg);
            }   //  end for k loop

            //  Load these lists into database table
            //  this will need to change once export reports are tested -- Oct 2014
            //bslyr.fileName = fileName;
            //bslyr.SaveExportGrade(sortList, gradeList, tableExists);
            return;
        }   //  end onFinished


        private void sortGridHeader()
        {
            //  setup headers for the sort data view
            logSortCodes.Columns[0].HeaderText = "SORT";
            logSortCodes.Columns[1].HeaderText = "CODE";
            logSortCodes.Columns[2].HeaderText = "NAME";
            logSortCodes.Columns[3].HeaderText = "MIN DIAMETER";
            logSortCodes.Columns[4].HeaderText = "MIN LENGTH";
            logSortCodes.Columns[5].HeaderText = "MIN BDFT";
            logSortCodes.Columns[6].HeaderText = "MAX DEFECT";

            logSortCodes.Columns[0].Width = 35;
            logSortCodes.Columns[1].Width = 35;
            logSortCodes.Columns[2].Width = 35;
            logSortCodes.Columns[3].Width = 85;
            logSortCodes.Columns[4].Width = 85;
            logSortCodes.Columns[5].Width = 85;
            logSortCodes.Columns[6].Width = 85;
        }   //  end sortGridHeader


        private void gradeGridHeader()
        {
            //  setup headers for the grade data view
            logGradeCodes.Columns[0].HeaderText = "GRADE";
            logGradeCodes.Columns[1].HeaderText = "CODE";
            logGradeCodes.Columns[2].HeaderText = "NAME";
            logGradeCodes.Columns[3].HeaderText = "MIN DIAMETER";
            logGradeCodes.Columns[4].HeaderText = "MIN LENGTH";
            logGradeCodes.Columns[5].HeaderText = "MIN BDFT";
            logGradeCodes.Columns[6].HeaderText = "MAX DEFECT";

            logGradeCodes.Columns[0].Width = 35;
            logGradeCodes.Columns[1].Width = 35;
            logGradeCodes.Columns[2].Width = 35;
            logGradeCodes.Columns[3].Width = 85;
            logGradeCodes.Columns[4].Width = 85;
            logGradeCodes.Columns[5].Width = 85;
            logGradeCodes.Columns[6].Width = 85;
        }   //  end gradeGridHeader

    }
}
