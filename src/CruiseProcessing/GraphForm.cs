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
using CruiseProcessing.Output;
using ZedGraph;

namespace CruiseProcessing
{
    public partial class GraphForm : Form
    {
        public string chartType;
        public string currTitle;
        public string currXtitle;
        public string currYtitle;
        public string currSP;
        public int graphNum;
        public string cruiseNum;
        public string currSaleName;
        public List<TreeDO> treeList = new List<TreeDO>();
        public List<ReportGeneratorBase.ReportSubtotal> graphData = new List<ReportGeneratorBase.ReportSubtotal>();
        public List<LogStockDO> logStockList = new List<LogStockDO>();
        public List<LCDDO> lcdList = new List<LCDDO>();

        protected CPbusinessLayer DataLayer { get; }

        public GraphForm()
        {
            InitializeComponent();
        }

        public GraphForm(CPbusinessLayer dataLayer)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        public void GraphForm_Load(object sender, EventArgs e)
        {
            switch (chartType)
            {
                case "SCATTER":
                    CreateScatterGraph(zedGraphControl1);
                    SetSize();
                    break;
                case "PIE":
                    CreatePieGraph(zedGraphControl1);
                    SetSize();
                    break;
                case "BAR":
                    CreateBarGraph(zedGraphControl1);
                    SetSize();
                    break;
            }   //  end switch
            return;
        }   //  end GraphForm_Load

        private void GraphForm_Resize(object sender, EventArgs e)
        {
            SetSize();
        }

        private void SetSize()
        {
            zedGraphControl1.Location = new Point(10, 10);
            //  Leave a small margin around the outside of the control
            zedGraphControl1.Size = new Size(ClientRectangle.Width - 20, ClientRectangle.Height - 20);
        }   //  end SetSize


        private void CreateScatterGraph(ZedGraphControl zgc)
        {
            //  get a references to the GraphPane
            GraphPane currPane = zgc.GraphPane;
            currPane.CurveList.Clear();
            //  Set the titles
            currPane.Title.Text = currTitle;
            currPane.XAxis.Title.Text = currXtitle;
            currPane.YAxis.Title.Text = currYtitle;
            
            //  add data for the graph
            PointPairList DBHlist = new PointPairList();
            DBHlist.Clear();
            foreach (TreeDO td in treeList)
            {
                DBHlist.Add(td.DBH, td.TotalHeight);
            }   //  end foreach loop
            
            //  add the curve
            LineItem scatterCurve = currPane.AddCurve("DBH", DBHlist, Color.Black, SymbolType.Diamond);
            //hide the line to make a scatter graph
            scatterCurve.Line.IsVisible = false;
            //  hide the symbol outline
            scatterCurve.Symbol.Border.IsVisible = false;
            //  fill symbol interior with color
            scatterCurve.Symbol.Fill = new Fill(Color.MediumBlue);
            //  fill the background of the chart rect and pane
            currPane.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow, 45.0f);
            currPane.Fill = new Fill(Color.White, Color.SlateGray, 45.0f);
            currPane.Legend.IsVisible = false;

            //  refigure axes sunce the data have changed
            zgc.AxisChange();
            
            //  save graph
            Size newSize = new Size(337,320);
            Bitmap currBMP = new Bitmap(currPane.GetImage(), newSize);
            string outputFile = System.IO.Path.GetDirectoryName(DataLayer.FilePath);
            outputFile += "\\Graphs\\";
            outputFile += currSaleName;
            System.IO.Directory.CreateDirectory(outputFile);
            outputFile += "\\";
            outputFile += "GR01";
            outputFile += "_";
            outputFile += currSP;
            outputFile += ".jpg";
            currBMP.Save(@outputFile, System.Drawing.Imaging.ImageFormat.Jpeg);

        }   //  end CreateScatterGraph


        private void CreateBarGraph(ZedGraphControl zgc)
        {
            //  get reference to the GraphPane
            GraphPane currPane = zgc.GraphPane;
            currPane.CurveList.Clear();
            //  set titles
            currPane.Title.Text = currTitle;
            currPane.Title.FontSpec.IsItalic = true;
            currPane.Title.FontSpec.Size = 24;
            currPane.Title.FontSpec.Family = "Times New Roman";
            currPane.XAxis.Title.Text = currXtitle;
            currPane.YAxis.Title.Text = currYtitle;
  
            //  Add data for the graph
            PointPairList graphList = new PointPairList();
            graphList.Clear();
            double maxDBH;
            double minDBH = 0;
            string legendTitle = "";
            switch (graphNum)
            {
                case 5:
                    //  had to store the values in net cubic instead of expansion factor
                    foreach (LogStockDO lsd in logStockList)
                        graphList.Add(lsd.DIBClass, lsd.NetCubicFoot, lsd.DIBClass / 5);
                    legendTitle = "NUMBER OF LOGS";
                    currPane.XAxis.Scale.Min = 0;
                    maxDBH = Convert.ToInt16(logStockList.Max(l => l.DIBClass));
                    currPane.XAxis.Scale.Max = maxDBH;
                    currPane.XAxis.Scale.MajorStep = 2;                   
                    break;
                case 6:     case 7:     case 8:
                    foreach (TreeDO td in treeList)
                        graphList.Add(td.DBH,td.ExpansionFactor);
                    legendTitle = "NUMBER OF TREES";
                    maxDBH = Convert.ToInt16(treeList.Max(t => t.DBH));
                    currPane.XAxis.Scale.Max = maxDBH + 1;
                    //  find first dbh class with value
                    foreach (TreeDO t in treeList)
                    {
                        if (t.ExpansionFactor > 0)
                        {
                            minDBH = t.DBH;
                            break;
                        }   //  endif
                    }   //  end foreach loop
                    currPane.XAxis.Scale.Min = minDBH - 1;
                    currPane.XAxis.Scale.MajorStep = 2;
                    break;
                case 9:
                    foreach (ReportGeneratorBase.ReportSubtotal gd in graphData)
                        graphList.Add(gd.Value4, gd.Value3);
                    legendTitle = "NUMBER OF TREES";
                    double MaxKPI = Convert.ToInt16(graphData.Max(g => g.Value4));
                    currPane.XAxis.Scale.Max = MaxKPI + 1;
                    break;
                case 11:
                    foreach (TreeDO td in treeList)
                        graphList.Add(td.DBH, td.TreeCount);
                    legendTitle = "BASAL AREA PER ACRE";
                    maxDBH = Convert.ToInt16(treeList.Max(t=>t.DBH));
                    currPane.XAxis.Scale.Max = maxDBH + 1;
                    //  find first dbh class with value
                    foreach(TreeDO t in treeList)
                    {
                        if(t.TreeCount > 0)
                        {
                            minDBH = t.DBH;
                            break;
                        }   //  endif
                    }   //  end foreach loop
                    currPane.X2Axis.Scale.Min = minDBH-1;
                    currPane.X2Axis.Scale.MajorStep = 2;
                    break;
            }   //  end switch
            
            //  add the curve
            BarItem barCurve = currPane.AddBar(legendTitle, graphList, Color.Blue);
            barCurve.Bar.Fill = new Fill(Color.ForestGreen);
            barCurve.Bar.Fill.Type = FillType.Solid;  
            
            currPane.Chart.Fill = new Fill(Color.White, Color.FromArgb(153, 204, 255), 45);
            currPane.Fill = new Fill(Color.White, Color.FromArgb(255, 255, 225), 45);
            //  tell ZedGraph to calculate the axis ranges
            zgc.AxisChange();

            //  save graphs
            Size newSize = new Size(337, 320);
            Bitmap currBMP = new Bitmap(currPane.GetImage(), newSize);
            string outputFile = System.IO.Path.GetDirectoryName(DataLayer.FilePath);
            outputFile += "\\Graphs\\";
            outputFile += currSaleName;
            System.IO.Directory.CreateDirectory(outputFile);
            outputFile += "\\";
            //  add graph report to file name
            switch (graphNum)
            {
                case 5:
                    outputFile += "GR05";
                    outputFile += "_";
                    outputFile += currSP;
                    break;
                case 6:
                    outputFile += "GR06";
                    break;
                case 7:
                    outputFile += "GR07";
                    outputFile += "_";
                    outputFile += currSP;
                    break;
                case 8:
                    outputFile += "GR08";
                    outputFile += "_";
                    outputFile += currSP;
                    break;
                case 9:
                    outputFile += "GR09";
                    outputFile += "_";
                    outputFile += currSP;
                    break;
                case 11:
                    outputFile += "GR11";
                    outputFile += "_";
                    outputFile += currSP;
                    break;
            }   //  end switch
            outputFile += ".jpg";
            currBMP.Save(@outputFile, System.Drawing.Imaging.ImageFormat.Jpeg);
            return;
        }   //  end CreateBarGraph


        private void CreatePieGraph(ZedGraphControl zgc)
        {
            //get reference to the GraphPane
            GraphPane currPane = zgc.GraphPane;
            currPane.CurveList.Clear();
            //  Set titles
            currPane.Title.Text = currTitle;
            currPane.Title.FontSpec.IsItalic = true;
            currPane.Title.FontSpec.Size = 28;
            currPane.Title.FontSpec.Family = "Times New Roman";

            //  fill the pane background with a color gradient
            currPane.Fill = new Fill(Color.White,Color.ForestGreen,45.0f);
            //  No fill for the chart background
            currPane.Chart.Fill.Type = FillType.None;

            //  Set the legend to an arbitrary location
            currPane.Legend.Position = LegendPos.InsideBotRight;
            currPane.Legend.Location = new Location(0.95f,0.12f,CoordType.PaneFraction,
                                                    AlignH.Right,AlignV.Top);
            currPane.Legend.FontSpec.Size = 8f;
            currPane.Legend.IsHStack = false;

            //  Add pie slices
            int listTotal = 0;
            if (graphNum != 10)
                listTotal = lcdList.Count;
            else listTotal = graphData.Count;
            double[] valuesForSlices = new double[listTotal];
            string[] labelsForSlices = new string[listTotal];
            int listCnt = 0;
            double valuesTotal = 0;
            string totalLabel = "";
            //  request was made to put percentages on each group
            //  to capture the small areas that don't show up well in the chart
            double totalValue = 0;
            double percentOfTotal = 0;
            StringBuilder combinedLabel = new StringBuilder();
            string convertedNumber = "";
            switch (graphNum)
            {
                case 2:
                    totalValue = lcdList.Sum(l => l.SumExpanFactor);
                    //  dump needed values into arrays
                    foreach(LCDDO l in lcdList)
                    {
                        valuesForSlices[listCnt] = l.SumExpanFactor;
                        combinedLabel.Append(l.Species);
                        //  calculate percentage
                        percentOfTotal = l.SumExpanFactor / totalValue * 100;
                        combinedLabel.Append(" ");
                        convertedNumber = String.Format("{0,4:F1}", percentOfTotal);
                        combinedLabel.Append(convertedNumber);
                        combinedLabel.Append("%");
                        labelsForSlices[listCnt] = combinedLabel.ToString();
                        listCnt++;
                        combinedLabel.Remove(0, combinedLabel.Length);
                    }   //  end foreach loop
                    valuesTotal = lcdList.Sum(l => l.SumExpanFactor);
                    totalLabel = "TOTAL NUMBER OF TREES\n";
                    totalLabel += String.Format("{0,10:F0}", valuesTotal);
                    break;
                case 3:
                    totalValue = lcdList.Sum(l => l.SumNCUFT);
                    //  dump needed values into arrays
                    foreach (LCDDO l in lcdList)
                    {
                        valuesForSlices[listCnt] = l.SumNCUFT;
                        combinedLabel.Append(l.Species);
                        //  calculate percentage
                        percentOfTotal = l.SumNCUFT / totalValue * 100;
                        combinedLabel.Append(" ");
                        convertedNumber = String.Format("{0,4:F1}", percentOfTotal);
                        combinedLabel.Append(convertedNumber);
                        combinedLabel.Append("%");
                        labelsForSlices[listCnt] = combinedLabel.ToString();
                        listCnt++;
                        combinedLabel.Remove(0, combinedLabel.Length);
                    }   //  end foreach loop
                    valuesTotal = lcdList.Sum(l => l.SumNCUFT);
                    totalLabel = "TOTAL NET CUFT VOLUME\n";
                    totalLabel += String.Format("{0,10:F0}", valuesTotal);
                    break;
                case 4:
                    totalValue = lcdList.Sum(l => l.SumNCUFT);
                    //  dump needed values into arrays
                    foreach (LCDDO l in lcdList)
                    {
                        valuesForSlices[listCnt] = l.SumNCUFT;
                        //  NOTE -- for this report, primary product was dumped into species
                        //  for convenience in coding
                        combinedLabel.Append(l.Species);
                        //  calculate percentage
                        percentOfTotal = l.SumNCUFT / totalValue * 100;
                        combinedLabel.Append(" ");
                        convertedNumber = String.Format("{0,4:F1}", percentOfTotal);
                        combinedLabel.Append(convertedNumber);
                        combinedLabel.Append("%");
                        labelsForSlices[listCnt] = combinedLabel.ToString();
                        listCnt++;
                        combinedLabel.Remove(0, combinedLabel.Length);
                    }   //  end foreach loop
                    valuesTotal = lcdList.Sum(l => l.SumNCUFT);
                    totalLabel = "TOTAL NET CUFT VOLUME\n";
                    totalLabel += String.Format("{0,10:F0}", valuesTotal);
                    break;
                case 10:
                    //  dump needed values into arrays
                    foreach (ReportGeneratorBase.ReportSubtotal gd in graphData)
                    {
                        valuesForSlices[listCnt] = gd.Value3;
                        combinedLabel.Append(gd.Value1);
                        //  calculate percentage
                        combinedLabel.Append(" ");
                        convertedNumber = String.Format("{0,4:F1}", gd.Value3);
                        combinedLabel.Append(convertedNumber);
                        labelsForSlices[listCnt] = combinedLabel.ToString();

                        listCnt++;
                        combinedLabel.Remove(0,combinedLabel.Length);
                    }   //  end foreach loop
                    valuesTotal = totalValue;
                    totalLabel = "TOTAL BAF PER ACRE ";
                    totalLabel += String.Format("{0,10:F0}", valuesTotal);
                    break;
            }   //  end switch

            PieItem[] pieSlices = currPane.AddPieSlices(valuesForSlices, labelsForSlices);
            foreach(PieItem p in pieSlices)
                p.LabelDetail.FontSpec.Size = 18f;
               
            //  Make a text label to highlight the total value
            TextObj text = new TextObj(totalLabel, 0.10F, 0.18F, CoordType.ChartFraction);
            text.Location.AlignH = AlignH.Center;
            text.Location.AlignV = AlignV.Bottom;
            text.FontSpec.Size = 10f;
            text.FontSpec.Border.IsVisible = false;
            text.FontSpec.Fill = new Fill(Color.White, Color.FromArgb(0, 255, 64), 45F);
            text.FontSpec.StringAlignment = StringAlignment.Center;
            currPane.GraphObjList.Add(text);

            //  Create a drop shadow for the total value text item
            TextObj text2 = new TextObj(text);
            text2.FontSpec.Fill = new Fill(Color.Black);
            text2.Location.X += 0.008f;
            text2.Location.Y += 0.01f;
            currPane.GraphObjList.Add(text2);

            //  Calculate the axis scale ranges
            zgc.AxisChange();


            //  save graphs
            Size newSize = new Size(337, 320);
            Bitmap currBMP = new Bitmap(currPane.GetImage(), newSize);
            string outputFile = System.IO.Path.GetDirectoryName(DataLayer.FilePath);
            outputFile += "\\Graphs\\";
            outputFile += currSaleName;
            System.IO.Directory.CreateDirectory(outputFile);
            outputFile += "\\";
            //  add graph report to file name
            switch (graphNum)
            {
                case 2:
                    outputFile += "GR02";
                    break;
                case 3:
                    outputFile += "GR03";
                    break;
                case 4:
                    outputFile += "GR04";
                    break;
                case 10:
                    outputFile += "GR10";
                    break;
            }   //  end switch
            outputFile += "_";
            outputFile += currSP;
            outputFile += ".jpg";
            currBMP.Save(@outputFile, System.Drawing.Imaging.ImageFormat.Jpeg);

            return;
        }   //  end CreatePieGraph
    }
}
