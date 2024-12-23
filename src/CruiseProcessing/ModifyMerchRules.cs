using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;

namespace CruiseProcessing
{
    public partial class ModifyMerchRules : Form
    {
        private List<VolumeEquationDO> vList = new List<VolumeEquationDO>();
        private List<VolumeEquationDO> justProducts = new List<VolumeEquationDO>();
        private int nthRow = 0;
        private string[] segmentDescrip = new string[] {"21 -- If top seg < 1/2 nom log len, combine with next lowest log",
                                                        "22 -- Top placed with next lowest log and segmented",
                                                        "23 -- Top segment stands on its own",
                                                        "24 -- If top seg < 1/4 log len drop the top.  If top >= 1/4 and <= 3/4 nom length, top is 1/2 of nom log lenght, else top is nom log len."};

        private int EVOD;
       

        public CpDataLayer DataLayer { get; }
        public IVolumeLibrary VolumeLibrary { get; }

        protected ModifyMerchRules()
        {
            InitializeComponent();
        }

        public ModifyMerchRules(CpDataLayer dataLayer, IVolumeLibrary volumeLibrary)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            VolumeLibrary = volumeLibrary;
        }

        public void setupDialog()
        {
            //  first, any changes made?  Indicates where data comes from
            vList = DataLayer.getVolumeEquations();
            int modFlag = (int) vList.Sum(v => v.MerchModFlag);
            if (modFlag == 0)
            {
                //  means no changes made -- need regional defaults from volume library
                //  need region
                string regText = DataLayer.getRegion();
                //  convert to integer for call to volume library
                int currReg = Convert.ToInt32(regText);
                //  then fill vList with values from volume library
                foreach (VolumeEquationDO ve in vList)
                {
                    if (ve.PrimaryProduct != "01")
                    {
                        //  need to convert volume equation and product to StringBuilder
                        var mRules = VolumeLibrary.GetMRules(currReg, ve.VolumeEquationNumber, ve.PrimaryProduct);

                        //  store return values
                        ve.Trim = mRules.trim;
                        ve.MinLogLengthPrimary = mRules.minlen;
                        ve.MaxLogLengthPrimary = mRules.maxlen;
                        ve.SegmentationLogic = mRules.opt;
                        ve.MinMerchLength = mRules.merchl;
                        ve.EvenOddSegment = 2;
                    }   //  endif
                }   //  end foreach loop
            }   //  endif merch rules modified

            //  then pull products
            ArrayList productList = new ArrayList();
            foreach (VolumeEquationDO ve in vList)
            {
                if (ve.PrimaryProduct != "01")
                {
                    //  add product if not already in the list
                    if (!productList.Contains(ve.PrimaryProduct))
                        productList.Add(ve.PrimaryProduct);
                }   //  endif
            }   //  end foreach loop

            //  load list for user to select product
            for (int k = 0; k < productList.Count; k++)
                nonsawProducts.Items.Add(productList[k]);

            //  disable navigation buttons until the product is selected
            previousButton.Enabled = false;
            nextButton.Enabled = false;
            finishedButton.Enabled = false;
            return;
        }   //  end setupDialog


        private void onItemSelected(object sender, EventArgs e)
        {
            // changing justProducts need to save to volume list
            // this is done whenever another product is selected or onFinished (see below)
            if (justProducts.Count > 0)
            {
                updateList();
                updateVolumeList();
            }   //  endif
            //  what product was selected?  Load list items for that product
            string currPR = nonsawProducts.SelectedItem.ToString();
            //  load just unique equations into list
            string currEquation = "*";
            justProducts.Clear();
            foreach (VolumeEquationDO v in vList)
            {
                if (currEquation != v.VolumeEquationNumber && currPR == v.PrimaryProduct)
                {
                    justProducts.Add(v);
                    currEquation = v.VolumeEquationNumber;
                }   //  endif
            }   //  end foreach loop
            if (justProducts.Count > 0)
            {
                trimText.Enabled = true;
                minLogLength.Enabled = true;
                maxLogLength.Enabled = true;
                segmentLogic.Enabled = true;
                minMerchLength.Enabled = true;
                previousButton.Enabled = true;
                nextButton.Enabled = true;
                finishedButton.Enabled = true;
                //  set fields to first record
                displayRecord(justProducts[0]);
                if (justProducts.Count == 1)
                {
                    MessageBox.Show("Only one record found for current product.", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    nextButton.Enabled = false;
                    previousButton.Enabled = false;
                }   //  endif
            }
            else if(justProducts.Count == 0)
                MessageBox.Show("No data for product selected\nTry a different product.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }   //  end onItemSelected

        private void onPrevious(object sender, EventArgs e)
        {
            //  Save current data whether changed or not
            updateList();
            //  display previous record
            if (nthRow == 0)
                nthRow = justProducts.Count - 1;
            else nthRow--;
            displayRecord(justProducts[nthRow]);
            return;
        }   //  onPrevious

        private void onNext(object sender, EventArgs e)
        {
            //  Save current data whether changed or not
            updateList();
            //  display next record
            if (nthRow == justProducts.Count - 1)
                nthRow = 0;
            else nthRow++;
            displayRecord(justProducts[nthRow]);
        }   //  end onNext

        
        private void updateList()
        {
            //  update current record whether changed or not
            double dValue = Convert.ToDouble(trimText.Text);
            justProducts[nthRow].Trim = (float)dValue;
            justProducts[nthRow].MinLogLengthPrimary = (float) minLogLength.Value;
            justProducts[nthRow].MaxLogLengthPrimary = (float) maxLogLength.Value;
            justProducts[nthRow].SegmentationLogic = Convert.ToInt32(segmentLogic.Text.Substring(0,2));
            justProducts[nthRow].MinMerchLength = (float) minMerchLength.Value;
            justProducts[nthRow].EvenOddSegment = EVOD;
            dValue = Convert.ToDouble(stumpHgt.Text);
            justProducts[nthRow].StumpHeight = (float)dValue;
            dValue = Convert.ToDouble(topDIB.Text);
            justProducts[nthRow].TopDIBPrimary = (float)dValue;
            justProducts[nthRow].MerchModFlag = 2;
            return;
        }   //  end updateList


        private void displayRecord(VolumeEquationDO currRecord)
        {
            trimText.Text = currRecord.Trim.ToString();
            currVolEq.Text = currRecord.VolumeEquationNumber.ToString();
            minLogLength.Value = Convert.ToDecimal(currRecord.MinLogLengthPrimary);
            maxLogLength.Value = Convert.ToDecimal(currRecord.MaxLogLengthPrimary);
            switch (currRecord.SegmentationLogic)
            {
                case 21:
                    segmentLogic.Text = segmentDescrip[0];
                    break;
                case 22:
                    segmentLogic.Text = segmentDescrip[1];
                    break;
                case 23:
                    segmentLogic.Text = segmentDescrip[2];
                    break;
                case 24:
                    segmentLogic.Text = segmentDescrip[3];
                    break;
            }   //  end switch
            minMerchLength.Value = Convert.ToDecimal(currRecord.MinMerchLength);
            if(currRecord.EvenOddSegment == 1)
               oddSegment.Checked = true;
            else if(currRecord.EvenOddSegment == 2)
               evenSegment.Checked = true;
            stumpHgt.Text = currRecord.StumpHeight.ToString();
            topDIB.Text = currRecord.TopDIBPrimary.ToString();
            return;
        }

        private void onFinished(object sender, EventArgs e)
        {
            //  update list with current entry -- next or previous may not have been pushed
            updateList();
            //  and update volume list for saving
            updateVolumeList();
            //  save volume equation list
            DataLayer.SaveVolumeEquations(vList);

            Close();
            return;
        }

        private void evenSelected(object sender, EventArgs e)
        {
            EVOD = 2;
        }   //  end evenSelected

        private void oddSelected(object sender, EventArgs e)
        {
            EVOD = 1;
        }   //  end oddSelected

        private void updateVolumeList()
        {
            foreach (VolumeEquationDO jp in justProducts)
            {
                foreach (VolumeEquationDO v in vList)
                {
                    //if (v.Species == jp.Species && v.PrimaryProduct == jp.PrimaryProduct &&
                    if(v.PrimaryProduct == jp.PrimaryProduct &&
                        v.VolumeEquationNumber == jp.VolumeEquationNumber)
                    {
                        v.MerchModFlag = jp.MerchModFlag;
                        v.MinLogLengthPrimary = jp.MinLogLengthPrimary;
                        v.MaxLogLengthPrimary = jp.MaxLogLengthPrimary;
                        v.MinMerchLength = jp.MinMerchLength;
                        v.SegmentationLogic = jp.SegmentationLogic;
                        v.Trim = jp.Trim;
                        v.EvenOddSegment = jp.EvenOddSegment;
                    }   //  endif
                }   //  end foreach loop
            }   //  end foreach loop
            return;
        }
    }
}
