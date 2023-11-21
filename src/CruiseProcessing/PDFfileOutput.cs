using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public partial class PDFfileOutput : Form
    {
        private string outputFileName;
        private string PDFoutFile;
        private ArrayList graphReports = new ArrayList();
        private string[] graphFlag = new string[9];
        protected CPbusinessLayer DataLayer { get; }

        protected PDFfileOutput()
        {
            InitializeComponent();
        }

        public PDFfileOutput(CPbusinessLayer dataLayer)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        public int setupDialog()
        {
            outputFileName = System.IO.Path.ChangeExtension(DataLayer.FilePath, "out");
            outputFileToConvert.Text = outputFileName;
            PDFoutFile = System.IO.Path.ChangeExtension(outputFileName, "PDF");
            PDFoutputFile.Text = PDFoutFile;
            if (File.Exists(PDFoutFile))
            {
                if (Utilities.IsFileInUse(PDFoutFile))
                {
                    //  make sure PDFfile is not open or attached to a process
                    MessageBox.Show("PDF File is open by another program.\nCannot continue until the file is closed.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return 1;
                }   //  endif
            }   //  endif file exists
            //  are there any graph files to add to the PDF file?
            if (CheckForGraphReports() == true)
            {
                List<SaleDO> saleList = DataLayer.getSale();
                string currSaleName = saleList[0].Name;
                string dirPath = System.IO.Path.GetDirectoryName(DataLayer.FilePath);
                dirPath += "\\Graphs\\";
                dirPath += currSaleName;
                DirectoryInfo di = new DirectoryInfo(dirPath);
                string currPath = "";
                int nextRow = 0;
                foreach (var fi in di.GetFiles("*.jpg"))
                {
                    currPath = System.IO.Path.GetDirectoryName(DataLayer.FilePath);
                    currPath += "\\Graphs\\";
                    currPath += currSaleName;
                    currPath += "\\";
                    currPath += fi.Name;
                    graphReports.Add(currPath);
                    nextRow++;
                    currPath = "";
                    // set graphFlag for titles later
                    switch (fi.Name.Substring(0, 4))
                    {
                        case "GR01":
                            graphFlag[0] = "GR01";
                            break;
                        case "GR02":
                            graphFlag[1] = "GR02";
                            break;
                        case "GR03":
                            graphFlag[2] = "GR03";
                            break;
                        case "GR04":
                            graphFlag[3] = "GR04";
                            break;
                        case "GR05":
                            graphFlag[4] = "GR05";
                            break;
                        case "GR06":
                            graphFlag[5] = "GR06";
                            break;
                        case "GR07":
                            graphFlag[6] = "GR07";
                            break;
                        case "GR08":
                            graphFlag[7] = "GR08";
                            break;
                        case "GR09":
                            graphFlag[8] = "GR09";
                            break;
                    }   //  end switch
                }   //  end foreach loop
            }   //  endif graph reports

            return 0;
        }   //  end setupDialog


        private void onBrowse(object sender, EventArgs e)
        {
            //  clear filename in case user wants to change files
            outputFileName = "";
            //  Create an instance of the open file dialog
            OpenFileDialog browseDialog = new OpenFileDialog();

            //  Set filter options and filter index
            browseDialog.Filter = "Output files (.out)|*.out|All Files (*.*)|*.*";
            browseDialog.FilterIndex = 1;

            browseDialog.Multiselect = false;

            //  capture filename selected
            while (outputFileName == "" || outputFileName == null)
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
                    outputFileName = browseDialog.FileName;
                    //  confirm it is an output file
                    var extention = Path.GetExtension(DataLayer.FilePath).ToLowerInvariant();
                    if (extention != ".out")
                    {
                        MessageBox.Show("File selected is not an output file.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }   //  endif
                }
            };  //  end while

            outputFileToConvert.Text = outputFileName;
            return;
        }   //  end onBrowse


        private void onConvert(object sender, EventArgs e)
        {
            //  disable convert button to avoid accidental double click
            convert_Button.Enabled = false;
            Document PDFdoc = new Document(new iTextSharp.text.Rectangle(792f, 612f));
            PdfWriter.GetInstance(PDFdoc, new FileStream(PDFoutFile, FileMode.Create));
            outputFileName = outputFileToConvert.Text;

            //  make sure output file exists before trying to create the PDF file
            if (File.Exists(outputFileName))
            {
                //  open outfile and read into PDF
                PDFdoc.Open();
                string line = null;
                iTextSharp.text.Font courier = FontFactory.GetFont("Courier", 8);
                using (StreamReader strRead = new StreamReader(outputFileName))
                {
                    while ((line = strRead.ReadLine()) != null)
                    {
                        Chunk oneChunk;
                        if (line == "")
                            oneChunk = new Chunk(Environment.NewLine);
                        else oneChunk = new Chunk(line, courier);
                        if (line == "\f")
                        {
                            PDFdoc.NewPage();
                        }
                        else
                        {
                            Phrase p1 = new Phrase();
                            p1.Add(oneChunk);
                            Paragraph p = new Paragraph();
                            //p.Leading = 10f;
                            p.Leading = 9f;
                            p.Add(p1);
                            PDFdoc.Add(p);
                        }   //  endif page break
                    }   //  end while
                }   //  end using

                //  add graph reports
                if (graphReports.Count > 0)
                {
                    addGraphReports(PDFdoc);
                }   //  endif graph reports
                PDFdoc.Close();
            }
            else
            {
                MessageBox.Show("Output file is needed to create the PDF file.\nCould not find selected filename.\nMake sure the output file has been created and try again.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }   //  endif output file exists

            //  add watermark
            PDFwatermarkDlg pwd = new PDFwatermarkDlg();
            pwd.ShowDialog();
            if (pwd.H2OmarkSelection == 2 || pwd.H2OmarkSelection == 3 || pwd.H2OmarkSelection == 4)
            {

                string PDForiginalText = PDFoutFile;
                PDForiginalText = System.IO.Path.ChangeExtension(PDForiginalText, "orig.pdf");
                System.IO.File.Copy(PDFoutFile, PDForiginalText, true);
                System.IO.File.Delete(PDFoutFile);
                StringBuilder WM = new StringBuilder();
                if (pwd.H2OmarkSelection == 2)
                    WM.Append("DRAFT");
                else if (pwd.H2OmarkSelection == 3)
                    WM.Append("CONTRACT OF RECORD");
                else if (pwd.H2OmarkSelection == 4)
                    WM.Append("CRUISE OF RECORD");
                if (pwd.includeDate == 1)
                {
                    WM.Append("\n");
                    WM.Append(DateTime.Now.Month.ToString());
                    WM.Append("/");
                    WM.Append(DateTime.Now.Day.ToString());
                    WM.Append("/");
                    WM.Append(DateTime.Now.Year.ToString());
                }   //  endif
                if (pwd.includeDate == 0)
                    AddWatermarkText(PDForiginalText, PDFoutFile, WM.ToString(), 60.0f, 0.3f, 45.0f);
                else if (pwd.includeDate == 1)
                    AddWatermarkText(PDForiginalText, PDFoutFile, WM.ToString(), 40.0f, 0.3f, 45.0f);

                System.IO.File.Delete(PDForiginalText);
            }   //  endif

            //  let user know file is created and where it can be found
            StringBuilder sb = new StringBuilder();
            sb.Append("PDF File has been created.\n");
            sb.Append(PDFoutFile);
            MessageBox.Show(sb.ToString(), "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }   //  end onConvert

        private void onFinished(object sender, EventArgs e)
        {
            Close();
            return;
        }   //  end onFinished


        private bool CheckForGraphReports()
        {
            List<ReportsDO> selReports = DataLayer.GetSelectedReports();
            int nthRow = selReports.FindIndex(
                delegate (ReportsDO r)
                {
                    return r.ReportID.Substring(0, 2) == "GR";
                });
            if (nthRow >= 0)
                return true;
            else return false;
        }   //  end CheckForGraphReports


        private void addGraphReports(Document PDFdoc)
        {
            Chunk oneChunk = new Chunk();
            ArrayList imagesToPrint = new ArrayList();
            //  loop through graph flag to pull report images
            for (int k = 0; k < graphFlag.Count(); k++)
            {
                if (graphFlag[k] != null)
                {
                    //  pull images to print
                    imagesToPrint.Clear();
                    for (int j = 0; j < graphReports.Count; j++)
                    {
                        if (graphReports[j].ToString().Contains(graphFlag[k]))
                            imagesToPrint.Add(graphReports[j]);
                    }   //  end for j loop
                    //  now load those images into the iTextSharp image array
                    iTextSharp.text.Image[] img = new iTextSharp.text.Image[imagesToPrint.Count];
                    for (int j = 0; j < imagesToPrint.Count; j++)
                    {
                        img[j] = iTextSharp.text.Image.GetInstance(String.Format(imagesToPrint[j].ToString()));
                    }   //  end for j loop

                    //  create title for the report
                    oneChunk = setGraphTitle(graphFlag[k]);

                    //  Create a table with 3 columns
                    PdfPTable graphTable = new PdfPTable(3);
                    graphTable.TotalWidth = 750f;
                    graphTable.LockedWidth = true;
                    //  add graphs
                    for (int j = 0; j < imagesToPrint.Count; j++)
                    {
                        //  add images
                        img[j].ScaleToFit(200f, 250f);
                        PdfPCell aCell = new PdfPCell(img[j]);
                        aCell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        graphTable.AddCell(aCell);
                    }   //  end for j loop
                    // prep to add to document
                    graphTable.CompleteRow();
                    //  add title
                    PDFdoc.NewPage();
                    Phrase p1 = new Phrase();
                    p1.Add(oneChunk);
                    Paragraph p = new Paragraph();
                    p.Font.SetFamily("TIMES");
                    p.Font.Size = 14f;
                    p.Font.SetStyle("BOLD");
                    p.Add(p1);
                    PDFdoc.Add(p);
                    p.Clear();
                    p.Add(Environment.NewLine);
                    PDFdoc.Add(p);
                    //  add graph table
                    PDFdoc.Add(graphTable);
                }   //  endif graphFlag is not null
            }   //  end for j loop on graphFlag
            return;
        }   //  end addGraphReports

        private Chunk setGraphTitle(string currRept)
        {
            Chunk titleChunk = new Chunk();
            switch (currRept)
            {
                case "GR01":
                    titleChunk = new Chunk("GR01 -- DBH AND TOTAL HEIGHT BY SPECIES");
                    break;
                case "GR02":
                    titleChunk = new Chunk("GR02 -- SPECIES DISTRIBUTION FOR THE SALE");
                    break;
                case "GR03":
                    titleChunk = new Chunk("GR03 -- VOLUME BY SPECIES -- SAWTIMBER ONLY");
                    break;
                case "GR04":
                    titleChunk = new Chunk("GR04 -- VOLUME BY PRODUCT");
                    break;
                case "GR05":
                    titleChunk = new Chunk("GR05 -- NUMBER OF 16-FOOT LOGS BY DIB CLASS BY SPECIES");
                    break;
                case "GR06":
                    titleChunk = new Chunk("GR06 -- DIAMETER DISTRIBUTION FOR THE SALE");
                    break;
                case "GR07":
                    titleChunk = new Chunk("GR07 -- DIAMETER DISTRIBUTION BY SPECIES");
                    break;
                case "GR08":
                    titleChunk = new Chunk("GR08 -- DIAMETER DISTRIBUTION BY STRATUM");
                    break;
            }   //  end switch on report
            return titleChunk;
        }   //  end setGraphTitle


        private void AddWatermarkText(string sourceFile, string outputFile, string watermarkText, float watermarkFontSize, float watermarkFontOpacity, float watermarkRotation)
        {
            iTextSharp.text.pdf.PdfReader reader = null;
            iTextSharp.text.pdf.PdfStamper stamper = null;
            iTextSharp.text.pdf.PdfGState gstate = null;
            iTextSharp.text.pdf.PdfContentByte underContent = null;
            iTextSharp.text.Rectangle rect = null;

            int pageCount = 0;
            try
            {
                reader = new iTextSharp.text.pdf.PdfReader(sourceFile);
                rect = reader.GetPageSizeWithRotation(1);
                stamper = new PdfStamper(reader, new System.IO.FileStream(outputFile, System.IO.FileMode.CreateNew), '\0', true);

                iTextSharp.text.pdf.BaseFont watermarkFont = iTextSharp.text.pdf.BaseFont.CreateFont(iTextSharp.text.pdf.BaseFont.COURIER,
                        iTextSharp.text.pdf.BaseFont.CP1252, iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED);
                gstate = new iTextSharp.text.pdf.PdfGState();
                gstate.FillOpacity = watermarkFontOpacity;
                gstate.StrokeOpacity = watermarkFontOpacity;
                pageCount = reader.NumberOfPages;
                for (int i = 1; i <= pageCount; i++)
                {
                    underContent = stamper.GetUnderContent(i);
                    underContent.SaveState();
                    underContent.SetGState(gstate);
                    underContent.SetColorFill(iTextSharp.text.BaseColor.DARK_GRAY);
                    underContent.BeginText();
                    underContent.SetFontAndSize(watermarkFont, watermarkFontSize);
                    underContent.SetTextMatrix(30, 30);
                    underContent.ShowTextAligned(iTextSharp.text.Element.ALIGN_CENTER, watermarkText, rect.Width / 2, rect.Height / 2, watermarkRotation);
                    underContent.EndText();
                    underContent.RestoreState();
                }   //  end for i loop

                stamper.Close();
                reader.Close();
            }   //  end try
            catch (Exception ex)
            {
                throw ex;
            }   //  end
            return;
        }   //  end AddWatermark
    }
}
