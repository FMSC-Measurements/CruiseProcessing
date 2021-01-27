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
    public partial class QualityAdjEquations : Form
    {
        #region
        private List<QualityAdjEquationDO> qaList = new List<QualityAdjEquationDO>();
        public string fileName;
        private int trackRow = -1;
        public CPbusinessLayer bslyr = new CPbusinessLayer();
        #endregion

        public QualityAdjEquations()
        {
            InitializeComponent();
        }


        public void setupDialog()
        {
            //  if there are equations, show in grid
            //  if not, just initialize the grid
            qaList = bslyr.getQualAdjEquations();
            qualityAdjEquationDOBindingSource.DataSource = qaList;
            qualityEquationList.DataSource = qualityAdjEquationDOBindingSource;

            equationList.Enabled = false;
        }   //  end setupDialog
        
        
        private void onCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (qualityEquationList.CurrentRow.IsNewRow == true)
                trackRow = qualityEquationList.CurrentCell.RowIndex;
            else
                trackRow = qualityEquationList.CurrentCell.RowIndex;

            if (qualityEquationList.Rows[e.RowIndex].Cells[2].Selected == true)
                equationList.Enabled = true;
        }   //  end onCellClick


        private void onInsertClick(object sender, EventArgs e)
        {
            if (trackRow >= 0)
            {
                qualityEquationList.CurrentCell = qualityEquationList.Rows[trackRow].Cells[2];
                qualityEquationList.EditMode = DataGridViewEditMode.EditOnEnter;
            }
            else if (trackRow == -1)
            {
                //  means user didn't click a cell which could be the first row is being edited
                //  so set trackRow to zero for first row
                trackRow = 0;
            }   //  endif trackRow

            if (trackRow < qaList.Count)
            {
                //  must be a change
                qaList[trackRow].QualityAdjEq = equationList.SelectedItem.ToString();
            }
            else if (trackRow >= qaList.Count)
            {
                //  it's a new record
                QualityAdjEquationDO qado = new QualityAdjEquationDO();
                qado.QualityAdjEq = equationList.SelectedItem.ToString();
                qaList.Add(qado);
            }   //  endif trackRow

            qualityAdjEquationDOBindingSource.ResetBindings(false);
            qualityEquationList.ClearSelection();
            qualityEquationList.CurrentCell = qualityEquationList.Rows[trackRow].Cells[3];

            equationList.Enabled = false;
        }   //  end onInsertClick


        private void onFinished(object sender, EventArgs e)
        {
            //  Make sure user wants to save all entries
            DialogResult nResult = MessageBox.Show("Do you want to save changes?","CONFIRMATION",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                bslyr.SaveQualityAdjEquations(qaList);
                Cursor.Current = this.Cursor;
            }   //  endif
            Close();
            return;
        }   //  end onFinished

        private void onCancel(object sender, EventArgs e)
        {
            DialogResult nResult = MessageBox.Show("Are you sure you really want to cancel?", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                Close();
                return;
            }   //  endif nResult
        }

        private void onDelete(object sender, EventArgs e)
        {
            DataGridViewRow row = qualityEquationList.SelectedRows[0];
            qualityEquationList.Rows.Remove(row);
        }   //  end onCancel






    }
}
