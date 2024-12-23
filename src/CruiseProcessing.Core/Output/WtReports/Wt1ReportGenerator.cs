using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.OutputModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace CruiseProcessing.Output
{
    public class Wt1ReportGenerator : OutputFileReportGeneratorBase, IReportGenerator
    {
        protected enum CompnentType
        { Primary, Secondary };

        //  WT1 report
        private readonly string[] WT1columns = new string[3] {  "                                                                 (1)             (2)       (3)            (4)            (5)",
                                                                "           CRUISE    CONTRACT             LIVE        GROSS     WEIGHT         POUNDS   PERCENT         POUNDS          TONS",
                                                                "          SPECIES     SPECIES    PRODUCT  DEAD         CUFT     FACTOR       STANDING   REMOVED        REMOVED       REMOVED"};

        private readonly int[] WT1_FIELD_LENGTHS = new int[] { 12, 14, 10, 3, 6, 6, 11, 12, 15, 10, 15, 10 };

        private readonly string[] WT1footer = new string[5] {  "         (1)  WEIGHT FACTOR = POUNDS PER GROSS CUFT",
                                                               "         (2)  POUNDS STANDING = GROSS CUFT x WEIGHT FACTOR",
                                                               "         (3)  PERCENT REMOVED = % MATERIAL HAULED OUT ON TRUCKS",
                                                               "         (4)  POUNDS REMOVED =  POUNDS STANDING x (PERCENT REMOVED/100)",
                                                               "         (5)  TONS REMOVED = POUNDS REMOVED / 2,000"};

        private readonly string[] WT1total = new string[3] {"                                 SUMMARY",
                                                  "                 CONTRACT                         TONS",
                                                  "                  SPECIES        PRODUCT       REMOVED"};

        private ILogger Log { get; }
        public IVolumeLibrary VolumeLibrary { get; }

        private readonly IReadOnlyList<int> _fieldLengths;
        private bool ShowMissingWeightFactorFooter = false;

        public Wt1ReportGenerator(CpDataLayer dataLayer, IVolumeLibrary volumeLibrary, ILogger<Wt1ReportGenerator> logger)
            : base(dataLayer, "WT1")
        {
            Log = logger;
            VolumeLibrary = volumeLibrary;
            _fieldLengths = WT1_FIELD_LENGTHS;

            string currentTitle = fillReportTitle(currentReport);
            SetReportTitles(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
        }

        public int GenerateReport(TextWriter strWriteOut, HeaderFieldData headerData, int startPageNum)
        {
            HeaderData = headerData;
            var pageNumb = startPageNum;
            numOlines = 0;
            Log.LogInformation("Generating WT1 report");

            if (!CheckForCubicFootVolumeAndWeights(strWriteOut))
            {
                return pageNumb;
            }

            var rptSubtotals = ProcessWT1LCDGroups(strWriteOut, ref pageNumb);
            WriteWT1Subtotal(strWriteOut, rptSubtotals, ref pageNumb);

            if (ShowMissingWeightFactorFooter)
            {
                //  write note that weight factor could not be found
                strWriteOut.WriteLine("\n", "NOTE:  Weight factor could not be found for certain groups.\nGroup not printed in report.");
            }

            Log.LogInformation("WT1 report generation complete");
            return pageNumb;
        }

        private IReadOnlyList<ReportSubtotal> ProcessWT1LCDGroups(TextWriter strWriteOut, ref int pageNumb)
        {
            List<ReportSubtotal> rptSubtotals = new List<ReportSubtotal>();

            // get all Cut Species,PrimaryProduct,SecondaryProduct,LiveDead,ContractSpecies combinations in cruise
            List<LCDDO> lcdGroups = DataLayer.GetLCDgroup("", 4, "C");

            //  loop through groups list and print each species, product, contract species combination

            foreach (LCDDO group in lcdGroups)
            {
                Log.LogTrace("Processing group: Sp:{Species} Prod:{Prod} SProd:{SProd} LD:{LD} ConSp{ConSp}"
                    , group.Species, group.PrimaryProduct, group.SecondaryProduct, group.LiveDead, group.ContractSpecies);

                //  get all LCD elements for current group
                var whereClause = "WHERE Species = @p1 AND PrimaryProduct = @p2 AND SecondaryProduct = @p3 AND " +
                    " LiveDead = @p4 AND ContractSpecies = @p5 AND CutLeave = @p6";
                List<LCDDO> groupData = DataLayer.GetLCDdata(whereClause, group, 4, "");

                double grossCuFtPrimary = 0.0;
                double grossCuFtSecondary = 0.0;
                //  sum gross cuft primary and secondary
                foreach (LCDDO gd in groupData)
                {
                    double stratumAcres = DataLayer.GetStratumAcresCorrected(gd.Stratum);
                    grossCuFtPrimary += gd.SumGCUFT * stratumAcres;
                    grossCuFtSecondary += gd.SumGCUFTtop * stratumAcres;
                }

                var weightFactor = DataLayer.GetWeightFactor(group.Species, group.PrimaryProduct, group.LiveDead, VolumeLibrary);
                var percentRemoved = DataLayer.GetPercentRemoved(group.Species, group.SecondaryProduct);

                double tonsRemovedPrimary = 0.0;
                double tonsRemovedSecondary = 0.0;

                if (grossCuFtPrimary > 0)
                {
                    WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                               WT1columns, 3, ref pageNumb, "");

                    var poundsStanding = Math.Round(grossCuFtPrimary, 0, MidpointRounding.AwayFromZero) * weightFactor;
                    var poundsRemoved = poundsStanding * (percentRemoved / 100);
                    tonsRemovedPrimary = poundsRemoved / 2000;
                    WriteWT1Group(strWriteOut, group, CompnentType.Primary, grossCuFtPrimary, weightFactor, poundsStanding, percentRemoved, poundsRemoved, tonsRemovedPrimary);
                }
                if (grossCuFtSecondary > 0)
                {
                    WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                               WT1columns, 3, ref pageNumb, "");

                    var poundsStanding = Math.Round(grossCuFtSecondary, 0, MidpointRounding.AwayFromZero) * weightFactor;
                    var poundsRemoved = poundsStanding * (percentRemoved / 100);
                    tonsRemovedSecondary = poundsRemoved / 2000;
                    WriteWT1Group(strWriteOut, group, CompnentType.Secondary, grossCuFtSecondary, weightFactor, poundsStanding, percentRemoved, poundsRemoved, tonsRemovedSecondary);
                }

                UpdateWT1Subtotal(rptSubtotals, group.ContractSpecies, group.PrimaryProduct, group.SecondaryProduct, tonsRemovedPrimary, tonsRemovedSecondary);
            }
            return rptSubtotals;
        }

        private void UpdateWT1Subtotal(List<ReportSubtotal> rptSubtotals, string currCS, string currPP, string currSP, double tonsRemovedPP, double tonsRemovedSP)
        {
            //  calculate tons removed to update subtotal

            var primarySubTotal = rptSubtotals.Find(x => x.Value1 == currCS && x.Value2 == currPP);

            if (primarySubTotal != null)
            {
                primarySubTotal.Value3 += tonsRemovedPP;
            }
            else
            {
                primarySubTotal = new ReportSubtotal
                {
                    Value1 = currCS,
                    Value2 = currPP,
                    Value3 = tonsRemovedPP
                };
                rptSubtotals.Add(primarySubTotal);
            }

            var secondarySubTotal = rptSubtotals.Find(x => x.Value1 == currCS && x.Value2 == currSP);
            if (secondarySubTotal != null)
            {
                secondarySubTotal.Value4 += tonsRemovedSP;
            }
            else
            {
                secondarySubTotal = new ReportSubtotal
                {
                    Value1 = currCS,
                    Value2 = currSP,
                    Value4 = tonsRemovedSP
                };
                rptSubtotals.Add(secondarySubTotal);
            }
        }

        private void WriteWT1Group(TextWriter writer,
            LCDDO group,
            CompnentType compnentType,
            double grossCuFt,
            float weightFactor,
            double poundsStanding,
            float percentRemoved,
            double poundsRemoved,
            double tonsRemoved)
        {
            var componentIndicator = compnentType switch { CompnentType.Primary => "P", CompnentType.Secondary => "S", _ => throw new NotImplementedException() };
            var product = compnentType switch { CompnentType.Primary => group.PrimaryProduct, CompnentType.Secondary => group.SecondaryProduct, _ => throw new NotImplementedException() };

            var prtFields = new string[]
            {
                "",
                group.Species.PadRight(6, ' '),
                (group.ContractSpecies ?? "").PadLeft(4, ' '),
                product.PadLeft(2, '0'),
                componentIndicator,
                group.LiveDead.PadLeft(1, ' '),
                String.Format("{0,8:F0}", Math.Ceiling(grossCuFt)).PadLeft(8, ' '),
                String.Format("{0,8:F2}", weightFactor).PadLeft(8, ' '),
                String.Format("{0,11:F0}", poundsStanding).PadLeft(11, ' '),
                String.Format("{0,6:F2}", percentRemoved).PadLeft(6, ' '),
                String.Format("{0,11:F0}", poundsRemoved).PadLeft(11, ' '),
                String.Format("{0,10:F2}", tonsRemoved).PadLeft(10, ' ')
            };
            printOneRecord(_fieldLengths, prtFields, writer);
            numOlines++;
        }

        private void WriteWT1Subtotal(TextWriter strWriteOut, IReadOnlyList<ReportSubtotal> rptSubtotals, ref int pageNumb)
        {
            //  WT1
            WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2], WT1columns, 3, ref pageNumb, "");

            strWriteOut.WriteLine("\n");
            for (int k = 0; k < 3; k++)
                strWriteOut.WriteLine(WT1total[k]);
            strWriteOut.WriteLine(reportConstants.longLine);
            double totalTons = 0;
            foreach (ReportSubtotal rs in rptSubtotals)
            {
                // (rs.Value3 > 0 || rs.Value4 > 0)
                //{
                //    strWriteOut.Write("                      ");
                //    strWriteOut.Write(rs.Value1.PadLeft(4, ' '));
                //    strWriteOut.Write("          ");
                //    strWriteOut.Write(rs.Value2.PadRight(3, ' '));
                //}   //endif
                //  the above code misses primary product of 20 and a secondary product of 20
                //  it mislabels primary product 20
                //  changed to put secondary tons in value4 keeping primary in value 3
                //  October 2015
                //if (rs.Value2 == "01")
                //  strWriteOut.Write("P      ");
                //else strWriteOut.Write("S      ");
                if (rs.Value3 > 0)
                {
                    strWriteOut.Write("                      ");
                    strWriteOut.Write(rs.Value1.PadLeft(4, ' '));
                    strWriteOut.Write("          ");
                    strWriteOut.Write(rs.Value2.PadRight(3, ' '));
                    strWriteOut.Write("P      ");
                    strWriteOut.WriteLine(String.Format("{0,10:F2}", rs.Value3).PadLeft(10, ' '));
                    totalTons += rs.Value3;
                }
                if (rs.Value4 > 0)
                {
                    strWriteOut.Write("                      ");
                    strWriteOut.Write(rs.Value1.PadLeft(4, ' '));
                    strWriteOut.Write("          ");
                    strWriteOut.Write(rs.Value2.PadRight(3, ' '));
                    strWriteOut.Write("S      ");
                    strWriteOut.WriteLine(String.Format("{0,10:F2}", rs.Value4).PadLeft(10, ' '));
                    totalTons += rs.Value4;
                }   //  endif
            }   //  end foreach loop on subtotals
            strWriteOut.WriteLine("           						  ____________________________________");
            strWriteOut.Write("                   TOTAL TONS REMOVED         ");
            strWriteOut.WriteLine(String.Format("{0,10:F2}", totalTons).PadLeft(10, ' '));
            strWriteOut.WriteLine("\n");
            //  write footer
            for (int k = 0; k < 5; k++)
                strWriteOut.WriteLine(WT1footer[k]);

            return;
        }   //  end WriteSubtotal
    }
}