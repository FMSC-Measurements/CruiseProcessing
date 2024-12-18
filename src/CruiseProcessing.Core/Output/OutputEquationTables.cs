using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Output;
using CruiseProcessing.OutputModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private readonly string[] QualityEqHeaders = new string[1] { "   SPECIES  EQUATION  YEAR(MMYYYY)   COEFFICIENT 1  COEFFICIENT 2  COEFFICIENT 3  COEFFICIENT 4  COEFFICIENT 5 COEFFICIENT 6" };

        public OutputEquationTables(CpDataLayer dataLayer, HeaderFieldData headerData) : base(dataLayer, headerData)
        {
        }

        public void outputEquationTable(TextWriter strWriteOut, ref int pageNumb)
        {
            string currRegion = DataLayer.getRegion();
            if (currRegion == "7" || currRegion == "07") { return; }

            var spProdValues = DataLayer.GetUniqueSpeciesProductFromTrees()
            .Select(x => (x.SpeciesCode, x.ProductCode))
            .ToHashSet();

            //  Volume equation table
            var veqList = DataLayer.getVolumeEquations().Where(x => spProdValues.Contains((x.Species, x.PrimaryProduct))).ToList();
            if (veqList.Count > 0)
            {
                WriteReportHeading(strWriteOut, "VOLUME EQUATION TABLE", "", "", VolEqHeaders, 12, ref pageNumb, "");

                int[] fieldLengths = new int[15] { 5, 10, 6, 14, 11, 10, 6, 6, 6, 14, 6, 6, 6, 9, 7 };
                foreach (VolumeEquationDO vel in veqList)
                {
                    //  build an array of fields to pass into creating a data table for printing
                    var prtFields = VolumeEqMethods.buildPrintArray(vel);
                    var oneRecord = buildPrintLine(fieldLengths, prtFields);
                    strWriteOut.WriteLine(oneRecord);
                    numOlines++;
                }   //  end foreach loop
            }   //  endif volume equations exist

            //  Value equations
            var valList = DataLayer.getValueEquations()
                .Where(x => spProdValues.Contains((x.Species, x.PrimaryProduct))).ToList();
            if (valList.Count > 0)
            {
                numOlines = 0;
                WriteReportHeading(strWriteOut, "VALUE EQUATION TABLE", "", "", ValEqHeaders, 9, ref pageNumb, "");

                int[] fieldLengths = new int[11] { 1, 10, 8, 5, 12, 16, 16, 16, 16, 16, 12 };
                foreach (ValueEquationDO val in valList)
                {
                    // build array first of fields to pass for creating data table
                    var prtFields = ValueEqMethods.buildPrintArray(val);
                    var oneRecord = buildPrintLine(fieldLengths, prtFields);
                    strWriteOut.WriteLine(oneRecord);
                    numOlines++;
                }   //  end foreach loop
            }   //  endif value equations exist




            var spList = DataLayer.GetUniqueSpeciesCodesFromTrees().ToHashSet();
            //  Quality adjustment equations
            var qualList = DataLayer.getQualAdjEquations()
                .Where(x => spList.Contains(x.Species)).ToList();
            if (qualList.Count > 0)
            {
                numOlines = 0;
                WriteReportHeading(strWriteOut, "QUALITY ADJUSTMENT EQUATION TABLE", "", "", QualityEqHeaders, 8, ref pageNumb, "");

                int[] fieldLengths = new int[10] { 5, 7, 15, 13, 15, 15, 15, 15, 15, 15 };
                foreach (QualityAdjEquationDO qal in qualList)
                {
                    //  build array of fields for creating data table
                    var prtFields = QualityAdjMethods.buildPrintArray(qal);
                    var oneRecord = buildPrintLine(fieldLengths, prtFields);
                    strWriteOut.WriteLine(oneRecord);
                    numOlines++;
                }   //  end foreach loop
            }   //  endif quality adjustment equations exist
        }   //  end outputEquationTable
    }
}