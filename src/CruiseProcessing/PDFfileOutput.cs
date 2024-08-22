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
using Microsoft.Extensions.DependencyInjection;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
using Microsoft.Extensions.Logging;

namespace CruiseProcessing
{
    public partial class PDFfileOutput : Form
    {
        private string outputFileName;
        private string PDFoutFile;
        private ArrayList graphReports = new ArrayList();
        private string[] graphFlag = new string[9];
        protected CpDataLayer DataLayer { get; }
        public IServiceProvider Services { get; }
        public IDialogService DialogService { get; }
        public ILogger Log { get; }

        protected PDFfileOutput()
        {
            InitializeComponent();
        }

        public PDFfileOutput(CpDataLayer dataLayer, IDialogService dialogService, ILogger<PDFfileOutput> logger, IServiceProvider services)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            Log = logger ?? throw new ArgumentNullException(nameof(logger));
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
                    DialogService.ShowError(PDFoutFile + " is open by another program.\r\nCannot continue until the file is closed.");
                    return 1;
                }   //  endif
            }   //  endif file exists
            //  are there any graph files to add to the PDF file?
            if (CheckForGraphReports() == true)
            {
                var sale = DataLayer.GetSale();
                string currSaleName = sale.Name;
                string dirPath = Path.Combine(Path.GetDirectoryName(DataLayer.FilePath), "Graphs", currSaleName);

                DirectoryInfo di = new DirectoryInfo(dirPath);
                try
                {
                    var attrs = di.Attributes;
                }
                catch (DirectoryNotFoundException)
                {
                    DialogService.ShowError("Unable to locate graphs folder for this sale.\r\nPlease regenerate output to create graphs.");
                    return 1;
                }
                catch (UnauthorizedAccessException)
                {
                    DialogService.ShowError("Unable to access graphs folder for this sale.");
                    return 1;
                }
                catch (PathTooLongException)
                {
                    DialogService.ShowError(dirPath + " is too long.");
                    return 1;
                }

                foreach (var graphFileName in di.GetFiles("*.jpg").Select(x => x.Name))
                {
                    var currPath = Path.Combine(dirPath, graphFileName);

                    graphReports.Add(currPath);
                    // set graphFlag for titles later
                    switch (graphFileName.Substring(0, 4))
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

            outputFileName = outputFileToConvert.Text;
            if (!File.Exists(outputFileName))
            {
                MessageBox.Show("Output file is needed to create the PDF file.\nCould not find selected filename.\nMake sure the output file has been created and try again.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            using StreamReader outFileReader = new StreamReader(outputFileName);


            using var fileStream = new FileStream(PDFoutFile, FileMode.Create);
            try
            {
                Document PDFdoc = new Document(new iTextSharp.text.Rectangle(792f, 612f));
                
                PdfWriter.GetInstance(PDFdoc, fileStream);

                WritePDF(PDFdoc, outFileReader);
            }
            catch (Exception ex)
            {

                DialogService.ShowError("Error writing PDF file.\n" + ex.Message);
                Log?.LogError(ex, "Error writing PDF file.");

                if(File.Exists(PDFoutFile))
                {
                    File.Delete(PDFoutFile);
                }
                return;
            }



            //  add watermark
            PDFwatermarkDlg watermarkDialog = Services.GetRequiredService<PDFwatermarkDlg>();
            watermarkDialog.ShowDialog();
            if (watermarkDialog.H2OmarkSelection == 2 || watermarkDialog.H2OmarkSelection == 3 || watermarkDialog.H2OmarkSelection == 4)
            {

                string sourceFilePath = PDFoutFile;
                var outputFilePath = System.IO.Path.ChangeExtension(sourceFilePath, "orig.pdf");
                if (File.Exists(outputFilePath)) { File.Delete(outputFilePath); }

                StringBuilder WM = new StringBuilder();
                if (watermarkDialog.H2OmarkSelection == 2)
                    WM.Append("DRAFT");
                else if (watermarkDialog.H2OmarkSelection == 3)
                    WM.Append("CONTRACT OF RECORD");
                else if (watermarkDialog.H2OmarkSelection == 4)
                    WM.Append("CRUISE OF RECORD");
                if (watermarkDialog.includeDate == 1)
                {
                    WM.Append("\n");
                    WM.Append(DateTime.Now.Month.ToString());
                    WM.Append("/");
                    WM.Append(DateTime.Now.Day.ToString());
                    WM.Append("/");
                    WM.Append(DateTime.Now.Year.ToString());
                }   //  endif

                try
                {
                    if (watermarkDialog.includeDate == 0)
                        AddWatermarkText(sourceFilePath, PDFoutFile, WM.ToString(), 60.0f, 0.3f, 45.0f);
                    else if (watermarkDialog.includeDate == 1)
                        AddWatermarkText(sourceFilePath, PDFoutFile, WM.ToString(), 40.0f, 0.3f, 45.0f);

                    File.Copy(sourceFilePath, outputFilePath, true);
                }
                catch (Exception ex)
                {
                    DialogService.ShowError("Error adding watermark to PDF file.\n" + ex.Message);
                    Log?.LogError(ex, "Error adding watermark to PDF file.");
                    return;
                }
                finally
                {
                    File.Delete(outputFilePath);
                }
            }   //  endif

            //  let user know file is created and where it can be found
            DialogService.ShowInformation(Name + " has completed creating the PDF file.\n" + PDFoutFile);

        }   //  end onConvert


        private void WritePDF(Document PDFdoc, StreamReader outFileReader)
        {
            //  open outfile and read into PDF
            PDFdoc.Open();
            string line = null;
            iTextSharp.text.Font courier = FontFactory.GetFont("Courier", 8);


            while ((line = outFileReader.ReadLine()) != null)
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

            //  add graph reports
            if (graphReports.Count > 0)
            {
                addGraphReports(PDFdoc);
            }   //  endif graph reports
            PDFdoc.Close();
        }

        private void onFinished(object sender, EventArgs e)
        {
            Close();
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


            using var outputFileStream = new System.IO.FileStream(outputFile, System.IO.FileMode.CreateNew);

            reader = new iTextSharp.text.pdf.PdfReader(sourceFile);
            var rect = reader.GetPageSizeWithRotation(1);
            stamper = new PdfStamper(reader, outputFileStream, '\0', true);

            iTextSharp.text.pdf.BaseFont watermarkFont = iTextSharp.text.pdf.BaseFont.CreateFont(iTextSharp.text.pdf.BaseFont.COURIER,
                    iTextSharp.text.pdf.BaseFont.CP1252, iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED);
            var gstate = new iTextSharp.text.pdf.PdfGState();
            gstate.FillOpacity = watermarkFontOpacity;
            gstate.StrokeOpacity = watermarkFontOpacity;
            var pageCount = reader.NumberOfPages;
            for (int i = 1; i <= pageCount; i++)
            {
                var underContent = stamper.GetUnderContent(i);
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
        }
    }
}
