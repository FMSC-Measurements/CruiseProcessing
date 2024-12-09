using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.OutputModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CruiseDAL.Schema.PRO;

namespace CruiseProcessing.Output
{
    public class Wt1ReprtGenerator : OutputFileReportGeneratorBase
    {
        protected enum CompnentType { Primary, Secondary };

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

        ILogger Logger { get; }

        private readonly IReadOnlyList<int> _fieldLengths;
        private bool ShowMissingWeightFactorFooter = false;

        public Wt1ReprtGenerator(CpDataLayer dataLayer, IVolumeLibrary volLib, HeaderFieldData headerData, ILogger logger)
            : base(dataLayer, volLib, headerData, "WT1")
        {
            Logger = logger;
            _fieldLengths = WT1_FIELD_LENGTHS;

            string currentTitle = fillReportTitle(currentReport);
            SetReportTitles(currentTitle, 5, 0, 0, reportConstants.FCTO, "");
        }

        public void GenerateReport(TextWriter strWriteOut, ref int pageNumb)
        {
            numOlines = 0;
            Logger.LogInformation("Generating WT1 report");

            if (!CheckForCubicFootVolumeAndWeights(strWriteOut))
            {
                return;
            }

            var rptSubtotals = ProcessWT1LCDGroups(strWriteOut, ref pageNumb);
            WriteWT1Subtotal(strWriteOut, rptSubtotals, ref pageNumb);

            Logger.LogInformation("WT1 report generation complete");
        }

        private IReadOnlyList<ReportSubtotal> ProcessWT1LCDGroups(TextWriter strWriteOut, ref int pageNumb)
        {
            List<ReportSubtotal> rptSubtotals = new List<ReportSubtotal>();

            // get all Cut Species,PrimaryProduct,SecondaryProduct,LiveDead,ContractSpecies combinations in cruise
            List<LCDDO> lcdGroups = DataLayer.GetLCDgroup("", 4, "C");

            //  loop through groups list and print each species, product, contract species combination

            foreach (LCDDO group in lcdGroups)
            {
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

                var primaryBioValues = DataLayer.GetPrimaryWeightFactorAndMoistureContent(group.Species, group.PrimaryProduct, group.LiveDead);
                var weightFactorSecondary = DataLayer.GetSecondaryWeightFactor(group.Species, group.SecondaryProduct, group.LiveDead);
                if (primaryBioValues == null || primaryBioValues.WeightFactor == 0.0
                    || weightFactorSecondary == null)
                {
                    Logger.LogInformation($"Weight factor not found for group: Sp:{group.Species} Prod:{group.PrimaryProduct} SProd:{group.SecondaryProduct} LD:{group.LiveDead}");
                    ShowMissingWeightFactorFooter = true;  // set flag to indicate group wasn't printed in footer
                    continue;
                }

                var weightFactorPrimary = primaryBioValues.WeightFactor;
                var percentRemovedPrimary = DataLayer.GetPrecentRemoved(group.Species, group.SecondaryProduct);
                var percentRemovedSecondary = percentRemovedPrimary; // secondary percent removed is the same as primary

                double tonsRemovedPrimary = 0.0;
                double tonsRemovedSecondary = 0.0;

                if (grossCuFtPrimary > 0)
                {
                    WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                               WT1columns, 3, ref pageNumb, "");

                    var poundsStanding = Math.Round(grossCuFtPrimary, 0, MidpointRounding.AwayFromZero) * weightFactorPrimary;
                    var poundsRemoved = poundsStanding * (percentRemovedPrimary / 100);
                    tonsRemovedPrimary = poundsRemoved / 2000;
                    WriteWT1Group(strWriteOut, group, CompnentType.Primary, grossCuFtPrimary, weightFactorPrimary, poundsStanding, percentRemovedPrimary, poundsRemoved, tonsRemovedPrimary);
                }
                if (grossCuFtSecondary > 0)
                {
                    WriteReportHeading(strWriteOut, reportTitles[0], reportTitles[1], reportTitles[2],
                               WT1columns, 3, ref pageNumb, "");

                    var poundsStanding = Math.Round(grossCuFtSecondary, 0, MidpointRounding.AwayFromZero) * weightFactorSecondary.Value;
                    var poundsRemoved = poundsStanding * (percentRemovedSecondary / 100);
                    tonsRemovedSecondary = poundsRemoved / 2000;
                    WriteWT1Group(strWriteOut, group, CompnentType.Secondary, grossCuFtSecondary, weightFactorSecondary.Value, poundsStanding, percentRemovedSecondary, poundsRemoved, tonsRemovedSecondary);

                }

                UpdateWT1Subtotal(rptSubtotals, group.ContractSpecies, group.PrimaryProduct, group.SecondaryProduct, tonsRemovedPrimary, tonsRemovedSecondary);

            }
            return rptSubtotals;
        }

        private void UpdateWT1Subtotal(List<ReportSubtotal> rptSubtotals, string currCS, string currPP, string currSP, double tonsRemovedPP, double tonsRemovedSP)
        {
            //  currently works for WT1
            //  calculate tons removed to update subtotal

            //  find group in subtotal list
            int nthRow = rptSubtotals.FindIndex(
                delegate (ReportSubtotal rs)
                {
                    return rs.Value1 == currCS && rs.Value2 == currPP;
                });
            if (nthRow >= 0)
            {
                rptSubtotals[nthRow].Value3 += tonsRemovedPP;
            }
            else
            {
                //  group not in the subtotal list so add it
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value1 = currCS;
                rs.Value2 = currPP;
                rs.Value3 = tonsRemovedPP;
                rptSubtotals.Add(rs);
            }   //  endif

            //  repeat for secondary product
            nthRow = rptSubtotals.FindIndex(
                delegate (ReportSubtotal rs)
                {
                    return rs.Value1 == currCS && rs.Value2 == currSP;
                });
            if (nthRow >= 0)
                rptSubtotals[nthRow].Value4 += tonsRemovedSP;
            else
            {
                //  add group
                ReportSubtotal rs = new ReportSubtotal();
                rs.Value1 = currCS;
                rs.Value2 = currSP;
                rs.Value4 = tonsRemovedSP;
                rptSubtotals.Add(rs);
            }   //  endif
            return;
        }   //  end UpdateSubtotal

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

            if (ShowMissingWeightFactorFooter)
            {
                //  write note that weight factor could not be found
                strWriteOut.WriteLine("\n", "NOTE:  Weight factor could not be found for certain groups.\nGroup not printed in report.");
            }   //  endif footFlag
            return;
        }   //  end WriteSubtotal
    }
}
