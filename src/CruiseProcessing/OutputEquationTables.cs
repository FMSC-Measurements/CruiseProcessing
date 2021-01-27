﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public class OutputEquationTables : CreateTextFile
    {
        public void outputEquationTable(StreamWriter strWriteOut, reportHeaders rh, ref int pageNumb)
        {
            //  Define table lists
            List<VolumeEquationDO> veqList = new List<VolumeEquationDO>();
            List<ValueEquationDO> valList = new List<ValueEquationDO>();
            List<BiomassEquationDO> bioList = new List<BiomassEquationDO>();
            List<QualityAdjEquationDO> qualList = new List<QualityAdjEquationDO>();
            VolumeEqMethods Vem = new VolumeEqMethods();
            string currRegion = bslyr.getRegion();
            if (currRegion != "7" && currRegion != "07")
            {
                //  Volume equation table
                veqList = bslyr.getVolumeEquations();
                if (veqList.Count > 0)
                {
                    WriteReportHeading(strWriteOut, "VOLUME EQUATION TABLE", "", "", rh.VolEqHeaders, 12, ref pageNumb, "");

                    int[] fieldLengths = new int[15] { 5, 10, 6, 14, 11, 10, 6, 6, 6, 14, 6, 6, 6, 9, 7 };
                    ArrayList prtFields = new ArrayList();
                    foreach (VolumeEquationDO vel in veqList)
                    {
                        //  build an array of fields to pass into creating a data table for printing
                        prtFields = Vem.buildPrintArray(vel);
                        StringBuilder oneRecord = buildPrintLine(fieldLengths, prtFields);
                        strWriteOut.WriteLine(oneRecord.ToString());
                        numOlines++;
                    }   //  end foreach loop
                }   //  endif volume equations exist

                //  Value equations
                valList = bslyr.getValueEquations();
                if (valList.Count > 0)
                {
                    numOlines = 0;
                    WriteReportHeading(strWriteOut, "VALUE EQUATION TABLE", "", "", rh.ValEqHeaders, 9, ref pageNumb, "");

                    int[] fieldLengths = new int[11] { 1, 10, 8, 5, 12, 16, 16, 16, 16, 16, 12 };
                    ArrayList prtFields = new ArrayList();
                    foreach (ValueEquationDO val in valList)
                    {
                        // build array first of fields to pass for creating data table
                        prtFields = ValueEqMethods.buildPrintArray(val);
                        StringBuilder oneRecord = buildPrintLine(fieldLengths, prtFields);
                        strWriteOut.WriteLine(oneRecord.ToString());
                        numOlines++;
                    }   //  end foreach loop
                }   //  endif value equations exist

                //  Biomass equations
                bioList = bslyr.getBiomassEquations();
                if (bioList.Count > 0)
                {
                    numOlines = 0;
                    WriteReportHeading(strWriteOut, "BIOMASS EQUATION TABLE", "", "", rh.BiomassHeaders, 10, ref pageNumb, "");

                    int[] fieldLengths = new int[12] { 2, 8, 5, 20, 7, 8, 8, 4, 6, 9, 6, 1 };
                    ArrayList prtFields = new ArrayList();
                    string prevSP = "**";
                    string prevPP = "**";
                    int printSpecies = 0;
                    foreach (BiomassEquationDO bel in bioList)
                    {
                        //  new header?
                        WriteReportHeading(strWriteOut, "BIOMASS EQUATION TABLE", "", "", rh.BiomassHeaders, 10, ref pageNumb, "");

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
                        StringBuilder oneRecord = buildPrintLine(fieldLengths, prtFields);
                        strWriteOut.WriteLine(oneRecord.ToString());
                        numOlines++;
                    }   //  end foreach loop

                }   //  endif biomass equations exist

                //  Quality adjustment equations
                QualityAdjMethods Qam = new QualityAdjMethods();
                qualList = bslyr.getQualAdjEquations();
                if (qualList.Count > 0)
                {
                    numOlines = 0;
                    WriteReportHeading(strWriteOut, "QUALITY ADJUSTMENT EQUATION TABLE", "", "", rh.QualityEqHeaders, 8, ref pageNumb, "");

                    int[] fieldLengths = new int[10] { 5, 7, 15, 13, 15, 15, 15, 15, 15, 15 };
                    ArrayList prtFields = new ArrayList();
                    foreach (QualityAdjEquationDO qal in qualList)
                    {
                        //  build array of fields for creating data table
                        prtFields = Qam.buildPrintArray(qal);
                        StringBuilder oneRecord = buildPrintLine(fieldLengths, prtFields);
                        strWriteOut.WriteLine(oneRecord.ToString());
                        numOlines++;
                    }   //  end foreach loop
                }   //  endif quality adjustment equations exist
            }   //  endif not BLM
            return;
        }   //  end outputEquationTable
    }
}
