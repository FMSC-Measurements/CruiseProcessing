using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.OutputModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Output
{
    public class Wt2Wt3ReportGeneratorBase : OutputFileReportGeneratorBase
    {
        //  WT2 and WT3 reports
        //  WT2columns includes line for WT3 header -- rest is the same for WT3
        protected readonly string[] WT2columns = new string[3] {"STRATUM:  XXXX",
                                                    "SLASH LOAD            SPECIES",
                                                    "CUTTING UNIT:  XXXX"};
        protected readonly string[] WT2crown = new string[5] { "   NEEDLES |", "  0 - 1/4\" |", "  1/4 - 1\" |", "    1 - 3\" |", "       3\"+ |" };
        protected readonly string[] WT2threeplus = new string[4] { "    TOPWOOD |", "CULL VOLUME |", "     CHUNKS |", "       FLIW |" };

        public IVolumeLibrary VolLib { get; }

        public Wt2Wt3ReportGeneratorBase(CpDataLayer dataLayer, IVolumeLibrary volLib, HeaderFieldData headerData, string reportID)
            : base(dataLayer, headerData, reportID)
        {
            VolLib = volLib ?? throw new ArgumentNullException(nameof(volLib));
        }

        protected void WriteCurrentGroup(TextWriter strWriteOut, ref int pageNumb, string currST,
                                        List<BiomassData> bList, string currCU)
        {
            //  output WT2/WT3
            //double lineTotal = 0;
            //  finish headers
            string[] completeHeader = new string[5];
            switch (currentReport)
            {
                case "WT2":
                    completeHeader = completeHeaderColumns(currST, bList);
                    break;

                case "WT3":
                    completeHeader = completeHeaderColumns(currCU, bList);
                    break;
            }   //  end switch on current report


            WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                                completeHeader, 13, ref pageNumb, "");
            //  crown section
            for (int n = 0; n < 5; n++)
            {
                var prtFields = new List<string>();
                prtFields.Add("         ");
                prtFields.Add(WT2crown[n]);
                prtFields.Add("     ");
                double lineTotal = LoadLine(prtFields, n + 1, bList);
                //  total column
                prtFields.Add(String.Format("{0,6:F2}", lineTotal / 2000).PadLeft(6, ' '));
                printOneRecord(strWriteOut, prtFields);
            }   //  end for n loop

            //  three inch plus section -- topwood dry weight
            strWriteOut.WriteLine("                    |");
            strWriteOut.WriteLine("3\"+                 |");
            strWriteOut.WriteLine("____________________|");
            for (int n = 0; n < 4; n++)
            {
                var prtFields = new List<string>();
                prtFields.Add("        ");
                prtFields.Add(WT2threeplus[n]);
                prtFields.Add("     ");
                double lineTotal = LoadLine(prtFields, n + 6, bList);
                if (n != 3)
                {
                    //  no line total for FLIW
                    prtFields.Add(String.Format("{0,6:F2}", lineTotal / 2000).PadLeft(6, ' '));
                    printOneRecord(strWriteOut, prtFields);
                }
                else printOneRecord(strWriteOut, prtFields);
            }   //  end for n loop

            //  damaged small trees
            strWriteOut.WriteLine("                    |");
            strWriteOut.WriteLine("DAMAGED SMALL TREES |");
            strWriteOut.WriteLine("____________________|");
            for (int n = 0; n < 5; n++)
            {
                var prtFields = new List<string>();
                prtFields.Add("         ");
                prtFields.Add(WT2crown[n]);
                prtFields.Add("     ");
                double lineTotal = LoadLine(prtFields, n + 10, bList);
                prtFields.Add(String.Format("{0,6:F2}", lineTotal / 2000).PadLeft(6, ' '));
                printOneRecord(strWriteOut, prtFields);
            }   //  end for n loop

            strWriteOut.WriteLine("                    |");
            strWriteOut.WriteLine(reportConstants.longLine);
            //  totals section here
            WriteTotals(strWriteOut, currST, bList, currCU);

            //  print footer
            strWriteOut.WriteLine(" ");
            strWriteOut.WriteLine("   FLIW = Fraction Left In Woods");

            
        }   //  end WriteCurrentGroup

        protected void WriteTotals(TextWriter strWriteOut, string currST, List<BiomassData> bList, string currCU)
        {
            double overallTotal = 0;
            strWriteOut.WriteLine("                    |");
            strWriteOut.Write("                    |");
            //  need to loop through list to get blanks for number of species
            foreach (BiomassData b in bList)
                strWriteOut.Write("          ");
            if (currST != "SALE" && currST != "")
            {
                strWriteOut.WriteLine("  STRATUM TOTAL/AC");
                strWriteOut.Write("TOTAL               |");
            }
            else if (currST == "SALE")
            {
                strWriteOut.WriteLine("  SALE TOTAL/AC");
                strWriteOut.Write("TOTAL               |");
            }   //  endif
            if (currCU != "")
            {
                strWriteOut.WriteLine("  UNIT TOTAL/AC");
                strWriteOut.Write("TOTAL               |");
            }   //  endif
            foreach (BiomassData b in bList)
                strWriteOut.Write("          ");
            strWriteOut.WriteLine("  ALL SPECIES");


            var prtFields = new List<string>();
            prtFields.Add("OVEN DRY TONS/AC    |");
            prtFields.Add("     ");
            foreach (BiomassData b in bList)
            {
                double sumValue = 0;
                sumValue += b.Needles;
                sumValue += b.QuarterInch;
                sumValue += b.ThreeInch;
                sumValue += b.ThreePlus;
                sumValue += b.TopwoodDryWeight;
                sumValue += b.CullLogWgt;
                sumValue += b.CullChunkWgt;
                sumValue += b.DSTneedles;
                sumValue += b.DSToneInch;
                sumValue += b.DSTquarterInch;
                sumValue += b.DSTthreeInch;
                sumValue += b.DSTthreePlus;
                prtFields.Add(String.Format("{0,6:F2}", sumValue / 2000).PadLeft(6, ' '));
                prtFields.Add("    ");
                overallTotal += sumValue;
            }   //  end foreach loop
            prtFields.Add(String.Format("{0,6:F2}", overallTotal / 2000).PadLeft(6, ' '));
            printOneRecord(strWriteOut, prtFields);
        }

        protected double LoadLine(List<string> prtFields, int whichComponent, List<BiomassData> bList)
        {
            double totalLine = 0;
            switch (whichComponent)
            {
                case 1:     //  crown needles
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.Needles / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.Needles;
                    }   //  end foreach loop
                    break;

                case 2:     //  crown quarter inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.QuarterInch / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.QuarterInch;
                    }   //  end foreach loop
                    break;

                case 3:     //  crown one inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.OneInch / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.OneInch;
                    }   //  end foreach loop
                    break;

                case 4:     //  crown three inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.ThreeInch / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.ThreeInch;
                    }   //  end foreach loop
                    break;

                case 5:     //  crown three inch plus
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.ThreePlus / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.ThreePlus;
                    }   //  end foreach loop
                    break;

                case 6:     //  topwood
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.TopwoodDryWeight / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.TopwoodDryWeight;
                    }   //  end foreach loop
                    break;

                case 7:     //  cull volume
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.CullLogWgt / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.CullLogWgt;
                    }   //  end foreach loop
                    break;

                case 8:     //  cull chunk weight
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.CullChunkWgt / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.CullChunkWgt;
                    }   //  end foreach loop
                    break;

                case 9:     //  FLIW -- has no total column
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.FractionLeftInWoods).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine = 0;
                    }   //  end foreach loop
                    break;

                case 10:    //  dam small trees needles
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.DSTneedles / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.DSTneedles;
                    }   //  end foreach loop
                    break;

                case 11:        //  dam small trees quarter inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.DSTquarterInch / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.DSTquarterInch;
                    }   //  end foreach loop
                    break;

                case 12:        //  dam small trees one inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.DSToneInch / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.DSToneInch;
                    }   //  end foreach loop
                    break;

                case 13:        //  dam small trees three inch
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.DSTthreeInch / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.DSTthreeInch;
                    }   //  end foreach loop
                    break;

                case 14:        //  dam small trees three inch plus
                    foreach (BiomassData b in bList)
                    {
                        prtFields.Add(String.Format("{0,6:F2}", b.DSTthreePlus / 2000).PadLeft(6, ' '));
                        prtFields.Add("    ");
                        totalLine += b.DSTthreePlus;
                    }   //  end foreach loop
                    break;
            }   //  end switch
            return totalLine;
        }   //  end LoadLine

        private string[] completeHeaderColumns(string currType, List<BiomassData> bList)
        {
            string[] finnishHeader = new string[5];
            switch (currentReport)
            {
                case "WT2":
                    // currType can be stratumCode, cuttingUnitCode, or "SALE"
                    if (currType != "SALE")
                        finnishHeader[0] = WT2columns[0].Replace("XXXX", currType.PadLeft(2, ' '));
                    else if (currType == "SALE")
                        finnishHeader[0] = "OVERALL SALE SUMMARY";
                    break;

                case "WT3":
                    finnishHeader[0] = WT2columns[2].Replace("XXXX", currType.PadLeft(4, ' '));
                    break;
            }   //  end switch

            //  second line
            finnishHeader[1] = WT2columns[1];
            finnishHeader[2] = reportConstants.longLine;
            //  load species columns
            finnishHeader[3] = "CROWNS              |"
                + string.Concat(bList.Select(b => "    " + b.SpeciesCode.PadLeft(6, ' ')))
                + "     TOTAL";

            finnishHeader[4] = "____________________|_______________________________________________________________________________________________________________";

            return finnishHeader;
        }   //  end completeHeaderColumns

        protected int CalculateComponentValues(List<TreeCalculatedValuesDO> currentData, double currAcres,
                            LCDDO lcdSpecies, List<BiomassData> bList,
                            string region)
        {
            // find FIA for current group
            int nthRow = bList.FindIndex(
                delegate (BiomassData b)
                {
                    return b.StratumCode == lcdSpecies.Stratum &&
                            b.SampleGroupCode == lcdSpecies.SampleGroup && b.SpeciesCode == lcdSpecies.Species;
                });
            if (nthRow >= 0)
            {
                float floatAcres = (float)currAcres;
                var bioData = bList[nthRow];
                return CalculateComponentValues(currentData, lcdSpecies, region, floatAcres, bioData);
            }   //  endif nthRow
            return 1;
        }   //  end CalculateComponentValues

        protected int CalculateComponentValues(List<TreeCalculatedValuesDO> currentData, LCDDO lcdSpecies, string region, float floatAcres, BiomassData bioData)
        {
            var currFIA = bioData.SpeciesFIA;
            if (currFIA == 0) return -1;
            //  need percent removed to calculate fraction left in the woods
            //  Per K.Cormier, FLIW = 1.0 - percent removed
            //  August 2013

            var percentRemoved = DataLayer.GetPrecentRemoved(lcdSpecies.Species, lcdSpecies.PrimaryProduct);
            bioData.FractionLeftInWoods = 1.0d - ((double)percentRemoved / 100.0d);

            //  crown section and damaged small trees
            foreach (TreeCalculatedValuesDO cd in currentData)
            {
                var tree = cd.Tree;

                float currDBH = cd.Tree.DBH;
                float currHGT = (region == "9" || region == "09") ? tree.TotalHeight : tree.MerchHeightPrimary;
                float CR = 0;
                var crfwt = VolLib.BrownCrownFraction(currFIA, currDBH, currHGT, CR);
                if (tree.DBH > 6)
                {
                    //  load crown section
                    bioData.Needles += crfwt.Needles;
                    bioData.QuarterInch += crfwt.QuarterInch;
                    bioData.OneInch += crfwt.OneInch;
                    bioData.ThreeInch += crfwt.ThreeInch;
                    bioData.ThreePlus += crfwt.ThreePlus;
                }
                else if (tree.DBH <= 6)
                {
                    //  load into damaged small trees
                    bioData.DSTneedles += crfwt.Needles;
                    bioData.DSTquarterInch += crfwt.QuarterInch;
                    bioData.DSToneInch += crfwt.OneInch;
                    bioData.DSTthreeInch += crfwt.ThreeInch;
                    bioData.DSTthreePlus += crfwt.ThreePlus;
                }

                //  Sum up values for three-inch plus section
                float grsVol = 0;
                float netVol = 0;
                //  Topwood weight
                grsVol = currentData.Sum(c => c.GrossCUFTSP * c.Tree.ExpansionFactor);
                grsVol = grsVol * floatAcres;
                VolLib.BrownTopwood(currFIA, grsVol, out var topwoodWGT);
                bioData.TopwoodDryWeight = topwoodWGT;

                //  Cull chunk weight
                grsVol = cd.GrossCUFTPP * tree.ExpansionFactor * floatAcres;
                netVol = cd.NetCUFTPP * tree.ExpansionFactor * floatAcres;
                var fliw = (float)bioData.FractionLeftInWoods;
                VolLib.BrownCullChunk(currFIA, grsVol, netVol, fliw, out var cullChunkWGT);
                bioData.CullChunkWgt += cullChunkWGT;

                //  Pull grade 9 logs for current group
                List<LogStockDO> justCullLogs = DataLayer.getCullLogs((long)cd.Tree_CN, "9");
                foreach (LogStockDO jcl in justCullLogs)
                {
                    grsVol = jcl.GrossCubicFoot;
                    VolLib.BrownCullLog(currFIA, grsVol, out var cullLogWGT);
                    bioData.CullLogWgt = cullLogWGT * tree.ExpansionFactor * floatAcres;
                }   //  end foreach loop
            }   //  end foreach loop

            return 1;
        }


        protected class BiomassData
        {
            public string StratumCode { get; set; }
            public string SampleGroupCode { get; set; }
            public string SpeciesCode { get; set; }
            public int SpeciesFIA { get; set; }
            public double Needles { get; set; }
            public double QuarterInch { get; set; }
            public double OneInch { get; set; }
            public double ThreeInch { get; set; }
            public double ThreePlus { get; set; }
            public double TopwoodDryWeight { get; set; }
            public double CullLogWgt { get; set; }
            public double CullChunkWgt { get; set; }
            public double FractionLeftInWoods { get; set; }

            //  damaged small trees (DST)
            public double DSTneedles { get; set; }
            public double DSTquarterInch { get; set; }
            public double DSToneInch { get; set; }
            public double DSTthreeInch { get; set; }
            public double DSTthreePlus { get; set; }
        }
    }
}
