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
    public partial class CapturePercentRemoved : Form
    {
        #region
        public List<PercentRemoved> prList = new List<PercentRemoved>();
        private int nextRow = 0;
        #endregion
        public CapturePercentRemoved()
        {
            InitializeComponent();
        }

        public void setupDialog()
        {
            //  Load data grid
            nextRow = PercentRemovedGrid.Rows.Count;
            PercentRemovedGrid.Rows.Insert(0,prList.Count);
            foreach (PercentRemoved pr in prList)
            {
                PercentRemovedGrid.Rows[nextRow].Cells[0].Value = pr.bioSpecies;
                PercentRemovedGrid.Rows[nextRow].Cells[1].Value = pr.bioProduct;
                int PCremoved = Convert.ToInt32(pr.bioPCremoved);
                if (PCremoved > 0)
                    PercentRemovedGrid.Rows[nextRow].Cells[2].Value = pr.bioPCremoved;
                else PercentRemovedGrid.Rows[nextRow].Cells[2].Value = "95";
                nextRow++;
            }   //  end foreach loop
            //  set focus to percent removed column on first row
            PercentRemovedGrid.CurrentCell = PercentRemovedGrid.Rows[0].Cells[2];
        }   //  end setupDialog


        private void onFinished(object sender, EventArgs e)
        {
            prList.Clear();
            //  capture grid into list
            for (int k = 0; k < PercentRemovedGrid.Rows.Count; k++)
            {
                PercentRemoved pr = new PercentRemoved();
                pr.bioSpecies = PercentRemovedGrid.Rows[k].Cells[0].Value.ToString();
                pr.bioProduct = PercentRemovedGrid.Rows[k].Cells[1].Value.ToString();
                pr.bioPCremoved = PercentRemovedGrid.Rows[k].Cells[2].Value.ToString();
                prList.Add(pr);
            }   // end for k loop

            Close();
            return;
        }   //  onFinished

    }
}
