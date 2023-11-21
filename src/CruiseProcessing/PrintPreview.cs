using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CruiseProcessing
{
    public partial class PrintPreview : Form
    {
        
        private string fileToPreview;

        protected PrintPreview()
        {
            InitializeComponent();
        }

        public PrintPreview(CPbusinessLayer dataLayer)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        protected CPbusinessLayer DataLayer { get; }
        protected string FilePath => DataLayer.FilePath;


        public void setupDialog()
        {
            fileToPreview = System.IO.Path.ChangeExtension(DataLayer.FilePath, "out");

            //  make sure it exists
            if (!File.Exists(fileToPreview))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(fileToPreview);
                sb.Append("\ncould not be found.  ");
                sb.Append("Cancel or make a selection using the Browse button");
                MessageBox.Show(sb.ToString(), "OOPS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                previewFileName.Text = "";
                return;
            }   //  endif file exists
            previewFileName.Text = fileToPreview;
            //  fill rich text box with default file
            fillTextBox();
            return;
        }   //  end setupDialog


        private void onBrowse(object sender, EventArgs e)
        {
            fileToPreview = System.IO.Path.ChangeExtension(DataLayer.FilePath, "out");

            //  make sure it exists
            if (!File.Exists(fileToPreview))
            {
                MessageBox.Show("Output file not found.\nCancel or make a selection\nusing Browse... button.", "OOPS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                previewFileName.Text = "";
                return;
            }   //  endif file exists

            //  Create an instance of the open file dialog
            OpenFileDialog browseDialog = new OpenFileDialog();

            //  Set filter options and filter index
            browseDialog.Filter = "Output files (.out)|*.out|All Files (*.*)|*.*";
            browseDialog.FilterIndex = 1;

            browseDialog.Multiselect = false;

            //  capture filename selected
            while (fileToPreview == "" || fileToPreview == null)
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
                    fileToPreview = browseDialog.FileName;
                    previewFileName.Text = fileToPreview;
                    fillTextBox();
                }   //  endif dResult
            };  //  end while
            return;
        }   //  end onBrowse


        private void onCancel(object sender, EventArgs e)
        {
            Close();
            return;
        }   //  end onCancel

        private void fillTextBox()
        {
            reportView.LoadFile(fileToPreview, RichTextBoxStreamType.PlainText);
            return;
        }   //  end fillTextBox
    }
}
