using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace CruiseProcessing
{
    public partial class R8Topwood : Form
    {
        public string[] checkStatus = new string[30];
        protected CPbusinessLayer DataLayer { get; }


        protected R8Topwood()
        {
            InitializeComponent();
        }

        public R8Topwood(CPbusinessLayer dataLayer)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        private void onOK(object sender, EventArgs e)
        {
            //  set check for building volume equations later
            if (checkBox1.Checked)
                checkStatus[0] = checkBox1.Text.ToString();
            if (checkBox2.Checked)
                checkStatus[1] = checkBox2.Text.ToString();
            if (checkBox3.Checked)
                checkStatus[2] = checkBox3.Text.ToString();
            if (checkBox4.Checked)
                checkStatus[3] = checkBox4.Text.ToString();
            if (checkBox5.Checked)
                checkStatus[4] = checkBox5.Text.ToString();
            if (checkBox6.Checked)
                checkStatus[5] = checkBox6.Text.ToString();
            if (checkBox7.Checked)
                checkStatus[6] = checkBox7.Text.ToString();
            if (checkBox8.Checked)
                checkStatus[7] = checkBox8.Text.ToString();
            if (checkBox9.Checked)
                checkStatus[8] = checkBox9.Text.ToString();
            if (checkBox10.Checked)
                checkStatus[9] = checkBox10.Text.ToString();
            if (checkBox11.Checked)
                checkStatus[10] = checkBox11.Text.ToString();
            if (checkBox12.Checked)
                checkStatus[11] = checkBox12.Text.ToString();
            if (checkBox13.Checked)
                checkStatus[12] = checkBox13.Text.ToString();
            if (checkBox14.Checked)
                checkStatus[13] = checkBox14.Text.ToString();
            if (checkBox15.Checked)
                checkStatus[14] = checkBox15.Text.ToString();
            if (checkBox16.Checked)
                checkStatus[15] = checkBox16.Text.ToString();
            if (checkBox17.Checked)
                checkStatus[16] = checkBox17.Text.ToString();
            if (checkBox18.Checked)
                checkStatus[17] = checkBox18.Text.ToString();
            if (checkBox19.Checked)
                checkStatus[18] = checkBox19.Text.ToString();
            if (checkBox20.Checked)
                checkStatus[19] = checkBox20.Text.ToString();
            if (checkBox21.Checked)
                checkStatus[20] = checkBox21.Text.ToString();
            if (checkBox22.Checked)
                checkStatus[21] = checkBox22.Text.ToString();
            if (checkBox23.Checked)
                checkStatus[22] = checkBox23.Text.ToString();
            if (checkBox24.Checked)
                checkStatus[23] = checkBox24.Text.ToString();
            if (checkBox25.Checked)
                checkStatus[24] = checkBox25.Text.ToString();
            if (checkBox26.Checked)
                checkStatus[25] = checkBox26.Text.ToString();
            if (checkBox27.Checked)
                checkStatus[26] = checkBox27.Text.ToString();
            if (checkBox28.Checked)
                checkStatus[27] = checkBox28.Text.ToString();
            if (checkBox29.Checked)
                checkStatus[28] = checkBox29.Text.ToString();
            if (checkBox30.Checked)
                checkStatus[29] = checkBox30.Text.ToString();
            Close();
            return;
        }   //  end onOK

        private void onCancel(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to cancel?\nAny changes made will not be saved.", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                Close();
                return;
            }   //  endif dr
        }   //  end onCancel


        public void setupDialog()
        {
            // initially, all check boxes are not visible
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            checkBox3.Visible = false;
            checkBox4.Visible = false;
            checkBox5.Visible = false;
            checkBox6.Visible = false;
            checkBox7.Visible = false;
            checkBox8.Visible = false;
            checkBox9.Visible = false;
            checkBox10.Visible = false;
            checkBox11.Visible = false;
            checkBox12.Visible = false;
            checkBox13.Visible = false;
            checkBox14.Visible = false;
            checkBox15.Visible = false;
            checkBox16.Visible = false;
            checkBox17.Visible = false;
            checkBox18.Visible = false;
            checkBox19.Visible = false;
            checkBox20.Visible = false;
            checkBox21.Visible = false;
            checkBox22.Visible = false;
            checkBox23.Visible = false;
            checkBox24.Visible = false;
            checkBox25.Visible = false;
            checkBox26.Visible = false;
            checkBox27.Visible = false;
            checkBox28.Visible = false;
            checkBox29.Visible = false;
            checkBox30.Visible = false;

            //  reset checked status to all zero
            for (int k = 0; k < 30; k++)
                checkStatus[k] = "0";

            // Add species code to checkbox text area and set check to true
            ArrayList justSpecies = DataLayer.GetJustSpecies("Tree");
            
            for (int k = 0; k < justSpecies.Count; k++)
            {
                string species = "";
                if(justSpecies[k] != null)
                {
                    species = justSpecies[k].ToString();
                }//end if
                else
                {
                    species = "";//null
                }

                loadCheckBox(k + 1, species);

            }   //  end for k loop
        }   //  end setupDialog


        private void loadCheckBox(int nthItem, string currentSpecies)
        {
            //  load appropriate check box with species code
            switch (nthItem)
            {
                case 1:
                    checkBox1.Text = currentSpecies;
                    checkBox1.Checked = true;
                    checkBox1.Visible = true;
                    break;
                case 2:
                    checkBox2.Text = currentSpecies;
                    checkBox2.Checked = true;
                    checkBox2.Visible = true;
                    break;
                case 3:
                    checkBox3.Text = currentSpecies;
                    checkBox3.Checked = true;
                    checkBox3.Visible = true;
                    break;
                case 4:
                    checkBox4.Text = currentSpecies;
                    checkBox4.Checked = true;
                    checkBox4.Visible = true;
                    break;
                case 5:
                    checkBox5.Text = currentSpecies;
                    checkBox5.Checked = true;
                    checkBox5.Visible = true;
                    break;
                case 6:
                    checkBox6.Text = currentSpecies;
                    checkBox6.Checked = true;
                    checkBox6.Visible = true;
                    break;
                case 7:
                    checkBox7.Text = currentSpecies;
                    checkBox7.Checked = true;
                    checkBox7.Visible = true;
                    break;
                case 8:
                    checkBox8.Text = currentSpecies;
                    checkBox8.Checked = true;
                    checkBox8.Visible = true;
                    break;
                case 9:
                    checkBox9.Text = currentSpecies;
                    checkBox9.Checked = true;
                    checkBox9.Visible = true;
                    break;
                case 10:
                    checkBox10.Text = currentSpecies;
                    checkBox10.Checked = true;
                    checkBox10.Visible = true;
                    break;
                case 11:
                    checkBox11.Text = currentSpecies;
                    checkBox11.Checked = true;
                    checkBox11.Visible = true;
                    break;
                case 12:
                    checkBox12.Text = currentSpecies;
                    checkBox12.Checked = true;
                    checkBox12.Visible = true;
                    break;
                case 13:
                    checkBox13.Text = currentSpecies;
                    checkBox13.Checked = true;
                    checkBox13.Visible = true;
                    break;
                case 14:
                    checkBox14.Text = currentSpecies;
                    checkBox14.Checked = true;
                    checkBox14.Visible = true;
                    break;
                case 15:
                    checkBox15.Text = currentSpecies;
                    checkBox15.Checked = true;
                    checkBox15.Visible = true;
                    break;
                case 16:
                    checkBox16.Text = currentSpecies;
                    checkBox16.Checked = true;
                    checkBox16.Visible = true;
                    break;
                case 17:
                    checkBox17.Text = currentSpecies;
                    checkBox17.Checked = true;
                    checkBox17.Visible = true;
                    break;
                case 18:
                    checkBox18.Text = currentSpecies;
                    checkBox18.Checked = true;
                    checkBox18.Visible = true;
                    break;
                case 19:
                    checkBox19.Text = currentSpecies;
                    checkBox19.Checked = true;
                    checkBox19.Visible = true;
                    break;
                case 20:
                    checkBox20.Text = currentSpecies;
                    checkBox20.Checked = true;
                    checkBox20.Visible = true;
                    break;
                case 21:
                    checkBox21.Text = currentSpecies;
                    checkBox21.Checked = true;
                    checkBox21.Visible = true;
                    break;
                case 22:
                    checkBox22.Text = currentSpecies;
                    checkBox22.Checked = true;
                    checkBox22.Visible = true;
                    break;
                case 23:
                    checkBox23.Text = currentSpecies;
                    checkBox23.Checked = true;
                    checkBox23.Visible = true;
                    break;
                case 24:
                    checkBox24.Text = currentSpecies;
                    checkBox24.Checked = true;
                    checkBox24.Visible = true;
                    break;
                case 25:
                    checkBox25.Text = currentSpecies;
                    checkBox25.Checked = true;
                    checkBox25.Visible = true;
                    break;
                case 26:
                    checkBox26.Text = currentSpecies;
                    checkBox26.Checked = true;
                    checkBox26.Visible = true;
                    break;
                case 27:
                    checkBox27.Text = currentSpecies;
                    checkBox27.Checked = true;
                    checkBox27.Visible = true;
                    break;
                case 28:
                    checkBox28.Text = currentSpecies;
                    checkBox28.Checked = true;
                    checkBox28.Visible = true;
                    break;
                case 29:
                    checkBox29.Text = currentSpecies;
                    checkBox29.Checked = true;
                    checkBox29.Visible = true;
                    break;
                case 30:
                    checkBox30.Text = currentSpecies;
                    checkBox30.Checked = true;
                    checkBox30.Visible = true;
                    break;
            }   //  end switch statement
            return;
        }   //  end loadCheckBox
    }
}
