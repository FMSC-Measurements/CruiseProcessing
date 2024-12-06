using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Output;
using CruiseProcessing.OutputModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CruiseProcessing
{
    public class OutputEquationTables : OutputFileReportGeneratorBase
    {
        //  Equation table headers
        private readonly string[] VolEqHeaders = new string[5] {"                                                               PRIMARY                     SECONDARY",
                                                      "                                                       ******* PRODUCT ********      ****** PRODUCT ******",
                                                      "                                            TOTAL       MIN                             MIN",
                                                      "                     VOLUME       STUMP     CUBIC       TOP                             TOP",
                                                      "     SPECIES  PROD   EQUATION     HEIGHT    VOLUME      DIB   BDFT  CUFT  CORDS         DIB   BDFT  CUFT  CORDS   BIOMASS"};
        private readonly string[] ValEqHeaders = new string[2] {"         PRODUCT         VALUE",
                                                      " SPECIES  CODE   GRADE  EQUATION   COEFFICIENT 1   COEFFICIENT 2   COEFFICIENT 3   COEFFICIENT 4   COEFFICIENT 5   COEFFICIENT 6"};
        private readonly string[] BiomassHeaders = new string[2] {"                                                              FIA    WEIGHT FACTOR",
                                                        " SPECIES PROD  COMPONENT           EQU  % MOIST  % REMV  L/D  CODE   PRIM     SECD     METDATA"};
        private readonly string[] QualityEqHeaders = new string[1] { "   SPECIES  EQUATION  YEAR(MMYYYY)   COEFFICIENT 1  COEFFICIENT 2  COEFFICIENT 3  COEFFICIENT 4  COEFFICIENT 5 COEFFICIENT 6" };

        public OutputEquationTables(CpDataLayer dataLayer, HeaderFieldData headerData) : base(dataLayer, headerData)
        {
        }

        public void outputEquationTable(TextWriter strWriteOut, ref int pageNumb)
        {
            //  Define table lists
            List<VolumeEquationDO> veqList = new List<VolumeEquationDO>();
            List<ValueEquationDO> valList = new List<ValueEquationDO>();
            List<BiomassEquationDO> bioList = new List<BiomassEquationDO>();
            List<QualityAdjEquationDO> qualList = new List<QualityAdjEquationDO>();
            string currRegion = DataLayer.getRegion();
            if (currRegion != "7" && currRegion != "07")
            {
                //  Volume equation table
                veqList = DataLayer.getVolumeEquations();
                if (veqList.Count > 0)
                {
                    WriteReportHeading(strWriteOut, "VOLUME EQUATION TABLE", "", "", VolEqHeaders, 12, ref pageNumb, "");

                    int[] fieldLengths = new int[15] { 5, 10, 6, 14, 11, 10, 6, 6, 6, 14, 6, 6, 6, 9, 7 };
                    var prtFields = new List<string>();
                    foreach (VolumeEquationDO vel in veqList)
                    {
                        //  build an array of fields to pass into creating a data table for printing
                        prtFields = VolumeEqMethods.buildPrintArray(vel);
                        var oneRecord = buildPrintLine(fieldLengths, prtFields);
                        strWriteOut.WriteLine(oneRecord);
                        numOlines++;
                    }   //  end foreach loop
                }   //  endif volume equations exist

                //  Value equations
                valList = DataLayer.getValueEquations();
                if (valList.Count > 0)
                {
                    numOlines = 0;
                    WriteReportHeading(strWriteOut, "VALUE EQUATION TABLE", "", "", ValEqHeaders, 9, ref pageNumb, "");

                    int[] fieldLengths = new int[11] { 1, 10, 8, 5, 12, 16, 16, 16, 16, 16, 12 };
                    var prtFields = new List<string>();
                    foreach (ValueEquationDO val in valList)
                    {
                        // build array first of fields to pass for creating data table
                        prtFields = ValueEqMethods.buildPrintArray(val);
                        var oneRecord = buildPrintLine(fieldLengths, prtFields);
                        strWriteOut.WriteLine(oneRecord);
                        numOlines++;
                    }   //  end foreach loop
                }   //  endif value equations exist

                //  Biomass equations
                bioList = DataLayer.getBiomassEquations();
                if (bioList.Count > 0)
                {
                    numOlines = 0;
                    WriteReportHeading(strWriteOut, "BIOMASS EQUATION TABLE", "", "", BiomassHeaders, 10, ref pageNumb, "");

                    int[] fieldLengths = new int[12] { 2, 8, 5, 20, 7, 8, 8, 4, 6, 9, 6, 1 };
                    var prtFields = new List<string>();
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
                        prtFields = BiomassEqMethods.buildPrintArray(bel, printSpecies);
                        var oneRecord = buildPrintLine(fieldLengths, prtFields);
                        strWriteOut.WriteLine(oneRecord);
                        numOlines++;
                    }   //  end foreach loop
                }   //  endif biomass equations exist

                //  Quality adjustment equations
                qualList = DataLayer.getQualAdjEquations();
                if (qualList.Count > 0)
                {
                    numOlines = 0;
                    WriteReportHeading(strWriteOut, "QUALITY ADJUSTMENT EQUATION TABLE", "", "", QualityEqHeaders, 8, ref pageNumb, "");

                    int[] fieldLengths = new int[10] { 5, 7, 15, 13, 15, 15, 15, 15, 15, 15 };
                    var prtFields = new List<string>();
                    foreach (QualityAdjEquationDO qal in qualList)
                    {
                        //  build array of fields for creating data table
                        prtFields = QualityAdjMethods.buildPrintArray(qal);
                        var oneRecord = buildPrintLine(fieldLengths, prtFields);
                        strWriteOut.WriteLine(oneRecord);
                        numOlines++;
                    }   //  end foreach loop
                }   //  endif quality adjustment equations exist
            }   //  endif not BLM
            return;
        }   //  end outputEquationTable
    }
}