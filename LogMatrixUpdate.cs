using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public partial class LogMatrixUpdate : Form
    {
        #region
        //public CPbusinessLayer bslyr = new CPbusinessLayer();
        public List<LogMatrixDO> reportMatrix = new List<LogMatrixDO>();
        public List<LogMatrix> defaultMatrix;
        public string currReport;
        public string currSaleName;
        public string currSaleNumber;
        public int returnValue = 0;
        #endregion

        public LogMatrixUpdate()
        {
            InitializeComponent();

        }

        public void setupDialog()
        {
            //  pull sale name and cruise number
            currentSaleName.Text = currSaleName;
            currentCruiseNumber.Text = currSaleNumber;
            currentReport.Text = currReport;
            int rowNum = 0;
            //  Load rest of reportMatrix in data grid at bottom
            //  Make a string to display and start with the header
            StringBuilder sb = new StringBuilder();
            logMatrixGrid.RowCount = reportMatrix.Count + 1;
            logMatrixGrid.ColumnCount = 4;
            //  resize width of columns
            DataGridViewColumn column = logMatrixGrid.Columns[0];
            column.Width = 175;
            column = logMatrixGrid.Columns[1];
            column.Width = 100;
            column = logMatrixGrid.Columns[2];
            column.Width = 125;
            column = logMatrixGrid.Columns[3];
            column.Width = 125;
            logMatrixGrid.Rows[rowNum].Cells[0].Value = "LOG SORT DESCRIPTION";
            logMatrixGrid.Rows[rowNum].Cells[1].Value = "SPECIES CODE";
            logMatrixGrid.Rows[rowNum].Cells[2].Value = "LOG GRADE CODE";
            logMatrixGrid.Rows[rowNum].Cells[3].Value = "LOG SED";
            
            foreach (LogMatrixDO rm in reportMatrix)
            {
                rowNum++;
                logMatrixGrid.Rows[rowNum].Cells[0].Value = rm.LogSortDescription;
                logMatrixGrid.Rows[rowNum].Cells[1].Value = rm.Species;

                //  create log grade code
                //  first log grade is probably not going to be blank
                //  so add it to the string
                sb.Clear();
                sb.Append(rm.LogGrade1);
                sb.Append(" ");
                if (rm.LogGrade2 != null && rm.GradeDescription != "")
                {
                    sb.Append(rm.GradeDescription);
                    sb.Append(" ");
                    sb.Append(rm.LogGrade2);
                    sb.Append(" ");
                }   //  endif
                if (rm.LogGrade3 != null && rm.GradeDescription != "")
                {
                    sb.Append(rm.GradeDescription);
                    sb.Append(" ");
                    sb.Append(rm.LogGrade3);
                    sb.Append(" ");
                }   //  endif
                if (rm.LogGrade4 != null && rm.GradeDescription != "")
                {
                    sb.Append(rm.GradeDescription);
                    sb.Append(" ");
                    sb.Append(rm.LogGrade4);
                    sb.Append(" ");
                }   //  endif
                if (rm.LogGrade5 != null && rm.GradeDescription != "")
                {
                    sb.Append(rm.GradeDescription);
                    sb.Append(" ");
                    sb.Append(rm.LogGrade5);
                    sb.Append(" ");
                }   //  endif
                if (rm.LogGrade6 != null && rm.GradeDescription != "")
                {
                    sb.Append(rm.GradeDescription);
                    sb.Append(" ");
                    sb.Append(rm.LogGrade6);
                    sb.Append(" ");
                }   //  endif
                logMatrixGrid.Rows[rowNum].Cells[2].Value = sb.ToString();

                //  Lastly is log SED
                sb.Clear();
                if (rm.SEDminimum == 0 && rm.SEDmaximum == 0)
                {
                    //  end line do nothing
                }
                else if (rm.SEDminimum > 0 && rm.SEDmaximum == 0)
                {
                    if(rm.SEDlimit != null)
                    {
                        sb.Append(rm.SEDlimit);
                        sb.Append(" ");
                    }   //  endif
                    sb.Append(rm.SEDminimum);
                    sb.Append("+");
                }
                else if (rm.SEDminimum > 0 && rm.SEDmaximum > 0)
                {
                    if (rm.SEDlimit == "thru")
                    {
                        sb.Append(rm.SEDminimum);
                        sb.Append(" ");
                        sb.Append(rm.SEDlimit);
                        sb.Append(" ");
                        sb.Append(rm.SEDmaximum);
                    }
                    else
                    {
                        sb.Append(rm.SEDlimit);
                        sb.Append(" ");
                        sb.Append(rm.SEDminimum);
                        sb.Append(" ");
                        if (rm.SEDlimit == "between")
                            sb.Append("and ");
                        sb.Append(rm.SEDmaximum);
                    }   //  endif
                }   //  endif
                //  end line
                logMatrixGrid.Rows[rowNum].Cells[3].Value = sb.ToString();
            }   // end foreach loop

            // for SED values, disable in sequence to ensure it stays in order
            descriptor1.Enabled = true;
            minSED.Enabled = true;
            descriptor2.Enabled = false;
            maxSED.Enabled = false;
            return;
        }   //  end setupDialog

        private void onAddRow(object sender, EventArgs e)
        {
            logMatrixGrid.Rows.Insert(1, 1);
            int nextRow = 1;
            logMatrixGrid.Rows[nextRow].Cells[0].Value = newLogSortDescription.Text;
            logMatrixGrid.Rows[nextRow].Cells[1].Value = speciesList.Text;

            logMatrixGrid.Rows[nextRow].Cells[2].Value = newLogGradeCode.Text;
            logMatrixGrid.Rows[nextRow].Cells[3].Value = smallEndDiameter.Text;

            //  clear previous entries
            newLogSortDescription.Clear();
            grade0.Checked = false;
            grade1.Checked = false;
            grade2.Checked = false;
            grade3.Checked = false;
            grade4.Checked = false;
            grade5.Checked = false;
            grade6.Checked = false;
            descriptorAnd.Checked = false;
            descriptorOr.Checked = false;
            descriptorCamprun.Checked = false;
            smallEndDiameter.Clear();
            descriptor1.Text = "";
            descriptor2.Text = "";
            descriptor2.Enabled = false;
            minSED.Clear();
            minSED.Enabled = true;
            maxSED.Clear();
            maxSED.Enabled = false;
            newLogGradeCode.Clear();
            newLogSortDescription.Focus();
        }

        private void onClearAll(object sender, EventArgs e)
        {
            logMatrixGrid.Rows.Clear();
        }

        private void onDeleteRow(object sender, EventArgs e)
        {
            logMatrixGrid.Rows.RemoveAt(logMatrixGrid.CurrentRow.Index);
        }

        private void onDone(object sender, EventArgs e)
        {
            //  update the reportMatrix to store changes
            if(reportMatrix != null) 
                reportMatrix.Clear();

            for (int k = 1; k < logMatrixGrid.RowCount; k++)
            {
                LogMatrixDO lmx = new LogMatrixDO();
                lmx.ReportNumber = currentReport.Text.ToString();
                lmx.LogSortDescription = logMatrixGrid.Rows[k].Cells[0].Value.ToString();
                lmx.Species = logMatrixGrid.Rows[k].Cells[1].Value.ToString();
                //  break apart log grade code to store
                string logGradeCode = logMatrixGrid.Rows[k].Cells[2].Value.ToString();
                if (logGradeCode.Contains("&"))
                    lmx.GradeDescription = "&";
                else if(logGradeCode.Contains("or"))
                    lmx.GradeDescription = "or";
                else if(logGradeCode.Contains("(camprun)"))
                    lmx.GradeDescription = "(camprun)";
                int loadedGrade = 1;
                for (int j = 0; j < logGradeCode.Length; j++)
                {
                    if (logGradeCode.Substring(j,1) == "0" || logGradeCode.Substring(j,1) == "1" ||
                        logGradeCode.Substring(j,1) == "2" || logGradeCode.Substring(j,1) == "3" ||
                        logGradeCode.Substring(j,1) == "4" || logGradeCode.Substring(j,1) == "5" ||
                        logGradeCode.Substring(j,1) == "6" || logGradeCode.Substring(j,1) == "7" ||
                        logGradeCode.Substring(j,1) == "8" || logGradeCode.Substring(j,1) == "9")
                    {
                        switch (loadedGrade)
                        {
                            case 1:
                                lmx.LogGrade1 = logGradeCode.Substring(j,1);
                                loadedGrade++;
                                break;
                            case 2:
                                lmx.LogGrade2 = logGradeCode.Substring(j,1);
                                loadedGrade++;
                                break;
                            case 3:
                                lmx.LogGrade3 = logGradeCode.Substring(j,1);
                                loadedGrade++;
                                break;
                            case 4:
                                lmx.LogGrade4 = logGradeCode.Substring(j,1);
                                loadedGrade++;
                                break;
                            case 5:
                                lmx.LogGrade5 = logGradeCode.Substring(j,1);
                                loadedGrade++;
                                break;
                            case 6:
                                lmx.LogGrade6 = logGradeCode.Substring(j,1);
                                loadedGrade++;
                                break;
                        }   //  end switch
                    }   //  endif
                }   //  end for j loop

                //  load diameter range
                string columnSED = logMatrixGrid.Rows[k].Cells[3].Value.ToString();
                int loadMinMax = 1;
                string SEDvalue = "";
                if (columnSED != " ")
                {
                    if (columnSED.Contains("thru"))
                        lmx.SEDlimit = "thru";
                    else if (columnSED.Contains("between"))
                        lmx.SEDlimit = "between";
                    else if (columnSED.Contains("greater"))
                        lmx.SEDlimit = "greater than";
                    else if (columnSED.Contains("less"))
                        lmx.SEDlimit = "less than";
                    string possibleChars = "0123456789.";
                    for (int jj = 0; jj < columnSED.Length; jj++)
                    {
                        if (possibleChars.Contains(columnSED.Substring(jj, 1)))
                            SEDvalue += columnSED[jj];
                        else if (columnSED.Substring(jj, 1) == " " && SEDvalue.Length > 0)
                        {
                            switch (loadMinMax)
                            {
                                case 1:
                                    lmx.SEDminimum = Convert.ToDouble(SEDvalue);
                                    loadMinMax++;
                                    SEDvalue = "";
                                    break;
                                case 2:
                                    lmx.SEDmaximum = Convert.ToDouble(SEDvalue);
                                    loadMinMax++;
                                    break;
                            }   //  end switch
                        }//  endif
                    }   //  end for jj loop
                    //  capture last value if length skips out of loop
                    if (loadMinMax == 1 && lmx.SEDminimum == 0)
                        if (SEDvalue != "")
                        {
                            lmx.SEDminimum = Convert.ToDouble(SEDvalue);
                            SEDvalue = "";
                        }
                        else if (lmx.SEDminimum > 0 && SEDvalue != "")
                            if (SEDvalue != "") lmx.SEDmaximum = Convert.ToDouble(SEDvalue);
                }   //  endif
                //  capture last value if length skips out of loop
                if (loadMinMax == 1 && lmx.SEDminimum == 0 && SEDvalue != "")
                    lmx.SEDminimum = Convert.ToDouble(SEDvalue);
                else if (lmx.SEDminimum > 0 && SEDvalue != "")
                    lmx.SEDmaximum = Convert.ToDouble(SEDvalue);
                reportMatrix.Add(lmx);
            }   //  end for k loop
            //  return a positive result
            returnValue = 1;
            Close();
            return;
        }   // end onDone

        private void onGrade0(object sender, EventArgs e)
        {
            newLogGradeCode.Text += "0 ";
        }   //  end onGrade0

        private void onGrade1(object sender, EventArgs e)
        {
            newLogGradeCode.Text += "1 ";
        }       //  end onGrade1

        private void onGrade2(object sender, EventArgs e)
        {
            newLogGradeCode.Text += "2 ";
        }   //  end onGrade2

        private void onGrade3(object sender, EventArgs e)
        {
            newLogGradeCode.Text += "3 ";
        }   //  end onGrade3

        private void onGrade4(object sender, EventArgs e)
        {
            newLogGradeCode.Text += "4 ";
        }   //  end onGrade4

        private void onGrade5(object sender, EventArgs e)
        {
            newLogGradeCode.Text += "5 ";
        }   //  end onGrade5

        private void onGrade6(object sender, EventArgs e)
        {
            newLogGradeCode.Text += "6 ";
        }   //  end onGrade6

        private void onDescriptorAnd(object sender, EventArgs e)
        {
            newLogGradeCode.Text += "& ";
        }   //  end onDescriptorAnd

        private void onDescriptorOr(object sender, EventArgs e)
        {
            newLogGradeCode.Text += "or ";
        }   //  end onDescriptorOr

        private void onDescriptorCamprun(object sender, EventArgs e)
        {
            newLogGradeCode.Text += "(camprun) ";
        }   //  end onDescriptorCamprun

        private void onDescriptor1Changed(object sender, EventArgs e)
        {
            if(descriptor1.Text != "none")
                smallEndDiameter.Text += descriptor1.Text;
            //  enable next field
            minSED.Enabled = true;
        }   //   end onDescriptorChanged

        private void onMinSEDchanged(object sender, EventArgs e)
        {
            smallEndDiameter.Text += " ";
            smallEndDiameter.Text += minSED.Text;
            //  enable next field
            descriptor2.Enabled = true;
        }   //  end onMinSEDchanged

        private void onDescriptor2Changed(object sender, EventArgs e)
        {
            smallEndDiameter.Text += " ";
            smallEndDiameter.Text += descriptor2.Text;
            //  enable next field
            maxSED.Enabled = true;
        }   //  end onDescriptor2Changed

        private void onmaxSEDchanged(object sender, EventArgs e)
        {
            smallEndDiameter.Text += " ";
            smallEndDiameter.Text += maxSED.Text;
        }   //  end onmaxSEDchanged

        private void onCancel(object sender, EventArgs e)
        {
            DialogResult nResult = MessageBox.Show("Do you really want to cancel?", "CONFIMRATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                //The report matrix should already have something in it so close and return so it gets resaved
                Close();
                return;
            }   //  endif

        }   //  end onCancel

    }
}
