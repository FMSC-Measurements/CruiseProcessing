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

namespace CruiseProcessing
{
    public partial class R9TopDIB : Form
    {
        #region
        public string fileName;
        public List<JustDIBs> jstDIB = new List<JustDIBs>();
        private int selectedRow;
        #endregion


        public R9TopDIB()
        {
            InitializeComponent();
        }

        public void setupDialog()
        {
            //  Get unique species list from trees
            ArrayList uniqueSpecies = Global.BL.GetJustSpecies("Tree");

            //  any volume equations??
            List<VolumeEquationDO> volList = Global.BL.getVolumeEquations().ToList();
            if (volList.Count <= 0)
            {
                //  Initialize dibs as zero
                jstDIB.Clear();
                for (int k = 0; k < uniqueSpecies.Count; k++)
                {
                    JustDIBs jd = new JustDIBs();
                    jd.speciesDIB = uniqueSpecies[k].ToString();
                    jd.productDIB = "01";
                    jd.primaryDIB = 0.0F;
                    jd.secondaryDIB = 0.0F;
                    jstDIB.Add(jd);
                }   //  end for k loop
            }
            else if(volList.Count > 0)
            {
                //  Initialize dibs from volume equations
                jstDIB.Clear();
                for (int k = 0; k < uniqueSpecies.Count; k++)
                {
                    JustDIBs jd = new JustDIBs();
                    string currentSpecies = uniqueSpecies[k].ToString();
                    VolumeEquationDO ve = volList.FirstOrDefault(v => v.Species == currentSpecies);
                    if (ve != null)
                    {
                        jd.speciesDIB = currentSpecies;
                        jd.productDIB = ve.PrimaryProduct.ToString();
                        jd.primaryDIB = ve.TopDIBPrimary;
                        jd.secondaryDIB = ve.TopDIBSecondary;
                        jstDIB.Add(jd);
                    }   //  endif nthRow
                }   //  end for k loop
            }   //  endif volumes

            //  Now the list can be loaded into the data grid -- it is not bound
            int rowNum = 0;
            completedDIBs.RowCount = jstDIB.Count;
            completedDIBs.ColumnCount = 3;
            foreach (JustDIBs jd in jstDIB)
            {
                completedDIBs.Rows[rowNum].Cells[0].Value = jd.speciesDIB;
                completedDIBs.Rows[rowNum].Cells[1].Value = jd.primaryDIB;
                completedDIBs.Rows[rowNum].Cells[2].Value = jd.secondaryDIB;
                rowNum++;
            }   //  end foreach loop

            return;
        }   //  end setupDialog

        private void onFinished(object sender, EventArgs e)
        {
            //  Capture DIB list to use in creating equations
            jstDIB.Clear();
            for(int k=0;k<completedDIBs.RowCount;k++)
            {
                JustDIBs jd = new JustDIBs();
                jd.speciesDIB = completedDIBs.Rows[k].Cells[0].Value.ToString();
                jd.productDIB = "01";
                jd.primaryDIB = Convert.ToSingle(completedDIBs.Rows[k].Cells[1].Value);
                jd.secondaryDIB = Convert.ToSingle(completedDIBs.Rows[k].Cells[2].Value);
                jstDIB.Add(jd);
            }   //  end foreach
            Close();
            return;
        }   //  end onFinished

        private void onCellEnter(object sender, DataGridViewCellEventArgs e)
        {
            //  Move DIBs to edit boxes
            DIBspecies.Text = completedDIBs.Rows[e.RowIndex].Cells[0].Value.ToString();
            sawtimberDIB.Text = completedDIBs.Rows[e.RowIndex].Cells[1].Value.ToString();
            nonSawDIB.Text = completedDIBs.Rows[e.RowIndex].Cells[2].Value.ToString();
            selectedRow = e.RowIndex;
            return;
        }   //  end onCellEnter

        private void onNonSawChanged(object sender, EventArgs e)
        {
            //  Move updated item back to data grid
            completedDIBs.Rows[selectedRow].Cells[2].Value = nonSawDIB.Text.ToString();
            return;
        }   //  end onNonSawChanged

        private void onSawChanged(object sender, EventArgs e)
        {
            //  Move updated item back to data grid
            completedDIBs.Rows[selectedRow].Cells[1].Value = sawtimberDIB.Text.ToString();
            return;
        }   //  end onSawChanged
    }
}
