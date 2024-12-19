using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.OutputModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Output
{
    public class BiomassEquationReportGenerator : OutputFileReportGeneratorBase, IReportGenerator
    {
        private readonly string[] BiomassHeaders = new string[2] {"                                                              FIA    WEIGHT FACTOR",
                                                        " SPECIES PROD  COMPONENT           EQU  % MOIST  % REMV  L/D  CODE   PRIM     SECD     METDATA"};

        public BiomassEquationReportGenerator(CpDataLayer dataLayer, string reportID = "") : base(dataLayer, reportID)
        {
        }

        public int GenerateReport(TextWriter strWriteOut, HeaderFieldData headerData, int startPageNum)
        {
            int[] fieldLengths = new int[12] { 2, 8, 5, 20, 7, 8, 8, 4, 6, 9, 6, 1 };
            string currRegion = DataLayer.getRegion();
            if (currRegion == "7" || currRegion == "07") { return startPageNum; }

            int pageNumb = startPageNum;
            HeaderData = headerData;

            if (DataLayer.NVBWeightFactorCache.Any())
            {
                WriteReportHeading(strWriteOut, "BIOMASS EQUATION TABLE", "", "", BiomassHeaders, 10, ref pageNumb, "");

                foreach (var key in DataLayer.NVBWeightFactorCache.Keys)
                {
                    var weightFactor = DataLayer.NVBWeightFactorCache[key];
                    var species = key.species;
                    var product = key.product;
                    var liveDead = key.liveDead;
                    var percentRemoved = DataLayer.GetPercentRemoved(species, product);

                    WriteReportHeading(strWriteOut, "BIOMASS EQUATION TABLE", "", "", BiomassHeaders, 10, ref pageNumb, "");


                    var prtFields = BuildPrintArray(species, product, (float?)null, percentRemoved, liveDead, weightFactor);
                    var oneRecord = buildPrintLine(fieldLengths, prtFields);
                    strWriteOut.WriteLine(oneRecord);
                    numOlines++;
                }

                return pageNumb;
            }
            var spProdValues = DataLayer.GetUniqueSpeciesProductFromTrees()
                .Select(x => (x.SpeciesCode, x.ProductCode))
                .ToHashSet();

            //  Biomass equations
            var bioList = DataLayer.getBiomassEquations()
                .Where(x => spProdValues.Contains((x.Species, x.Product))).ToList();
            if (bioList.Count > 0)
            {
                numOlines = 0;
                WriteReportHeading(strWriteOut, "BIOMASS EQUATION TABLE", "", "", BiomassHeaders, 10, ref pageNumb, "");

                string prevSP = "**";
                string prevPP = "**";
                int printSpecies = 0;
                foreach (BiomassEquationDO bel in bioList)
                {
                    //  new header?
                    WriteReportHeading(strWriteOut, "BIOMASS EQUATION TABLE", "", "", BiomassHeaders, 10, ref pageNumb, "");

                    if (prevSP != bel.Species || prevPP != bel.Product)
                    {
                        printSpecies = 1;
                        prevSP = bel.Species;
                        //  find biomass product in the sample group table

                        prevPP = bel.Product;
                    }
                    else if (prevSP == bel.Species || prevPP == bel.Product)
                    {
                        printSpecies = 0;
                    }   //  endif
                        //  build array for creating equation line
                    var prtFields = buildPrintArray(bel, printSpecies);
                    var oneRecord = buildPrintLine(fieldLengths, prtFields);
                    strWriteOut.WriteLine(oneRecord);
                    numOlines++;
                }

                return pageNumb;
            }
            else
            {
                // no biomass equations
                // lets see of there is anything in the CRZSPDFTWeightFactorCache

                if(DataLayer.CRZSPDFTWeightFactorCache.Any())
                {
                    WriteReportHeading(strWriteOut, "BIOMASS EQUATION TABLE", "", "", BiomassHeaders, 10, ref pageNumb, "");

                    foreach (var key in DataLayer.CRZSPDFTWeightFactorCache.Keys)
                    {
                        var weightFactors = DataLayer.CRZSPDFTWeightFactorCache[key];
                        var species = key.species;
                        var product = key.product;
                        var liveDead = key.liveDead;
                        var percentRemoved = DataLayer.GetPercentRemoved(species, product);

                        WriteReportHeading(strWriteOut, "BIOMASS EQUATION TABLE", "", "", BiomassHeaders, 10, ref pageNumb, "");


                        var prtFields = BuildPrintArray(species, product, weightFactors[2], percentRemoved, liveDead, weightFactors[0], weightFactors[1]);
                        var oneRecord = buildPrintLine(fieldLengths, prtFields);
                        strWriteOut.WriteLine(oneRecord);
                        numOlines++;
                    }
                }


            }

            return pageNumb;
        }

        public static List<string> BuildPrintArray(string species, string product, float? percentMoisture, float percentRemoved, string livedead, float weightFactor, float? weightFactor2 = null)
        {
            string fieldFormat = "{0,5:F1}";
            var printArray = new List<string>();
            printArray.Add(" ");
            printArray.Add(species.PadRight(6, ' '));
            printArray.Add(product.PadLeft(2, '0'));
            printArray.Add("                 ");
            printArray.Add("   ");
            printArray.Add(string.Format(fieldFormat, percentMoisture));
            printArray.Add(string.Format(fieldFormat, percentRemoved));
            printArray.Add(livedead);
            printArray.Add("   ");
            printArray.Add(string.Format(fieldFormat, weightFactor));
            printArray.Add(string.Format(fieldFormat, weightFactor2));
            printArray.Add("  ");

            return printArray;
        }

        public static List<string> buildPrintArray(BiomassEquationDO bioDO, int printSpecies)
        {
            string fieldFormat = "{0,5:F1}";
            var bioArray = new List<string>();
            bioArray.Add(" ");
            if (printSpecies == 1)
            {
                bioArray.Add(bioDO.Species.PadRight(6, ' '));
                bioArray.Add(bioDO.Product.PadLeft(2, '0'));
            }
            else if (printSpecies == 0)
            {
                bioArray.Add("      ");
                bioArray.Add("  ");
            }   //  endif printSpecies
            switch (bioDO.Component)
            {
                case "TotalTreeAboveGround":
                    bioArray.Add("Total Tree        ");
                    break;

                case "LiveBranches":
                    bioArray.Add("Live Branches     ");
                    break;

                case "DeadBranches":
                    bioArray.Add("Dead Branches     ");
                    break;

                case "Foliage":
                    bioArray.Add("Foliage           ");
                    break;

                case "PrimaryProd":
                    bioArray.Add("Mainstem Primary  ");
                    break;

                case "SecondaryProd":
                    bioArray.Add("Mainstem Secondary");
                    break;

                case "StemTip":
                    bioArray.Add("Stem Tip          ");
                    break;
            }   //  end switch on component

            if (bioDO.Equation == null || bioDO.Equation == "" || bioDO.Equation.Substring(0, 3) == "   ")
                bioArray.Add("   ");
            else bioArray.Add(bioDO.Equation.PadLeft(3, '0'));
            bioArray.Add(string.Format(fieldFormat, bioDO.PercentMoisture));
            bioArray.Add(string.Format(fieldFormat, bioDO.PercentRemoved));
            bioArray.Add(bioDO.LiveDead);
            bioArray.Add(bioDO.FIAcode.ToString());
            bioArray.Add(string.Format(fieldFormat, bioDO.WeightFactorPrimary));
            bioArray.Add(string.Format(fieldFormat, bioDO.WeightFactorSecondary));
            if (bioDO.MetaData == null)
                bioArray.Add("  ");
            else bioArray.Add(bioDO.MetaData.PadRight(49, ' ').Substring(0, 49));

            return bioArray;
        }   //  end buildPrintArray


    }
}
