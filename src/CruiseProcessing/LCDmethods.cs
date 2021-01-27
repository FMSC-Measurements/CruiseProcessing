using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public static class LCDmethods
    {
        //  methods pertaining to the LCD table
        public static List<LCDDO> GetStratum(List<LCDDO> LCDlist, string currST)
        {
            List<LCDDO> rtrnList = LCDlist.FindAll(
                delegate(LCDDO ld)
                {
                    return ld.Stratum == currST;
                });
            return rtrnList;
        }   //  end GetStratum


        public static List<LCDDO> GetCutOrLeave(List<LCDDO> LCDlist, string currCL, string currST, string currPR, int prodType)
        {
            List<LCDDO> rtrnList = new List<LCDDO>();
            //  pull cut trees, current stratum and requested product
            if (prodType == 1)       //  primary
            {
                rtrnList = LCDlist.FindAll(
                    delegate(LCDDO lcdo)
                    {
                        return lcdo.CutLeave == currCL && lcdo.Stratum == currST && lcdo.PrimaryProduct == currPR;
                    });
            }
            else if (prodType == 2)      //  secondary or recovered
            {
                rtrnList = LCDlist.FindAll(
                    delegate(LCDDO lcdo)
                    {
                        return lcdo.CutLeave == currCL && lcdo.Stratum == currST && lcdo.SecondaryProduct == currPR;
                    });
            }   //  endif prodType
            return rtrnList;
        }   //  end GetCutOrLeave


        public static List<LCDDO> GetCutOrLeave(List<LCDDO> LCDlist, string currCL, string currSP, 
                                                string currST, string currLD)
        {

            List<LCDDO> rtrnList = new List<LCDDO>();
            //  this returns one of the following:  all cut trees, all cut trees and current species
            //  or all cut trees and current stratum
            //  species
            if (currSP != "" && currCL == "" && currST == "" && currLD == "")
            {
                //  pull cut trees and current species from LCD list
                rtrnList = LCDlist.FindAll(
                    delegate(LCDDO lcdo)
                    {
                        return lcdo.CutLeave == "C" && lcdo.Species == currSP;
                    });
                return rtrnList;
            }   //  endif currSP

            //  stratum --  may need to setup another query for leave trees
            if (currST != "" && currCL == "C" && currSP == "" && currLD == "")
            {
                //  pull cut trees and current stratum from LCD list
                rtrnList = LCDlist.FindAll(
                    delegate(LCDDO lcdo)
                    {
                        return lcdo.CutLeave == currCL && lcdo.Stratum == currST;
                    });
                return rtrnList;
            }   //  endif currST

            //  cutleave
            if (currCL != "" && currST == "" && currSP == "" && currLD == "")
            {
                //  pull cut or leave trees based on the current Cut/Leave code
                rtrnList = LCDlist.FindAll(
                    delegate(LCDDO lcdo)
                    {
                        return lcdo.CutLeave == currCL;
                    });
                return rtrnList;
            }   //  endif currST and currSP are blank

            //  cutleave, stratum and species
            if (currCL != "" && currST != "" && currSP != "" && currLD == "")
            {
                // pulls cutleave, stratum and species
                rtrnList = LCDlist.FindAll(
                    delegate(LCDDO lcdo)
                    {
                        return lcdo.CutLeave == currCL && lcdo.Stratum == currST && lcdo.Species == currSP;
                    });
                return rtrnList;
            }   //  endif

            //  cut/leave and species
            if (currSP != "" && currCL != "")
            {
                rtrnList = LCDlist.FindAll(
                    delegate(LCDDO lcdo)
                    {
                        return lcdo.CutLeave == currCL && lcdo.Species == currSP;
                    });
                return rtrnList;
            }   //  endif species and cut/leave


            return rtrnList;
        }   //  end GetCutOrLeave


        public static List<LCDDO> GetCutOnlyMultipleValue(List<LCDDO> LCDlist, string currST, string currSG, 
                                                            string currSP, string currCL, string currUOM, string currSTM)
        {
            List<LCDDO> rtrnList = new List<LCDDO>();
            //  pull cut trees only based on parameters given
            //  stratum, sample group and species
            if (currST != "" && currSG != "" && currSP != "")
            {
                rtrnList = LCDlist.FindAll(
                    delegate(LCDDO lcdo)
                    {
                        return lcdo.CutLeave == "C" && lcdo.Stratum == currST && lcdo.SampleGroup == currSG && lcdo.Species == currSP;
                    });
                return rtrnList;
            }   //  endif stratum, sample group and species requested


            //  species and sample group
            if (currSP != "" && currSG != "")
            {
                rtrnList = LCDlist.FindAll(
                    delegate(LCDDO lcdo)
                    {
                        return lcdo.CutLeave == "C" && lcdo.Species == currSP && lcdo.SampleGroup == currSG;
                    });
                return rtrnList;
            }   //  endif species and sample group requested

            //  unit of measure
            if (currUOM != "" && currST == "" && currSP == "" && currSG == "")
            {
                rtrnList = LCDlist.FindAll(
                    delegate(LCDDO lcdo)
                    {
                        return lcdo.CutLeave == "C" && lcdo.UOM == currUOM;
                    });
                return rtrnList;
            }   //  endif unit of measure requested


            //  cut/leave, stratum and sample group
            if (currCL != "" && currST != "" && currUOM != "")
            {
                if (currSTM != "")
                {
                    rtrnList = LCDlist.FindAll(
                        delegate(LCDDO lcdo)
                        {
                            return lcdo.CutLeave == currCL && lcdo.Stratum == currST &&
                                    lcdo.SampleGroup == currSG && lcdo.STM == currSTM;
                        });
                }
                else if (currSTM == "")
                {
                    rtrnList = LCDlist.FindAll(
                        delegate(LCDDO lcdo)
                        {
                            return lcdo.CutLeave == currCL && lcdo.Stratum == currST &&
                                        lcdo.SampleGroup == currSG;
                        });
                }   //  endif
                return rtrnList;
            }   //  endif cut/leave, stratum and sample group requested
            return rtrnList;
        }   //  end GetCutOnlyMultipleValue


        public static List<LCDDO> GetCutGroupedBy(string currST, string currPP, int groupBy, CPbusinessLayer bslyr)
        {
            List<LCDDO> LCDlist = new List<LCDDO>();
            if (currST != "")
            {
                LCDlist = bslyr.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2 ORDER BY ", "Species,SampleGroup,LiveDead", "C", currST);
            }   //  endif currST not blank

            if (currPP != "")
            {
                LCDlist = bslyr.getLCDOrdered("WHERE PrimaryProduct = @p1 ORDER BY ", "ContractSpecies", currPP,"");
            }   //  endif currPP not blank


            switch (groupBy)
            {/*
                case 1:         //  group by contract species
                    return LCDlist = bslyr.getLCDOrdered("WHERE CutLeave = ? ORDER BY ", "ContractSpecies", "C","");
                case 2:         //  group by and order by primary product and species
                    return LCDlist = bslyr.getLCDOrdered("WHERE CutLeave = ? ORDER BY ", "PrimaryProduct,Species", "C","");
                case 3:         //  group by species and biomass product
                    return LCDlist = bslyr.getLCDOrdered("WHERE CutLeave = ? ORDER BY ", "Species,BiomassProduct", "C","");
                case 4:         //  group by species and primary product
                    return LCDlist = bslyr.getLCDOrdered("WHERE CutLeave = ? ORDER BY ", "Species,PrimaryProduct", "C","");
                case 5:         //  group by species, primary product, secondary product, live/dead and contract species
                    return LCDlist = bslyr.getLCDOrdered("WHERE CutLeave = ? ORDER BY ", "Species,PrimaryProduct,SecondaryProduct,LiveDead,ContractSpecies", "C","");
                case 6:         //  group by stratum
                    return LCDlist = bslyr.getLCDOrdered("WHERE CutLeave = ? ORDER BY ", "Stratum", "C","");
                case 7:         //  group by primary product, secondary product and species
                    return LCDlist = bslyr.getLCDOrdered("WHERE CutLeave = ? ORDER BY ", "PrimaryProduct,SecondaryProduct,Species", "C","");
              */
                case 8:         //  order by primary product, species and tree grade
                    LCDlist = bslyr.getLCDOrdered("WHERE CutLeave = @p1 ORDER BY ", "PrimaryProduct,Species,TreeGrade", "C", "");
                    break;
                case 9:         //  really grouped by primary product and species for live/dead reports
                    LCDlist = bslyr.getLCDOrdered("WHERE CutLeave = @p1 GROUP BY ", "PrimaryProduct,SecondaryProduct,Species", "C", "");
                    break;
            }   //  end switch
            return LCDlist;
        }   //  end GetCutGroupedBy


        public static List<LCDDO> GetStratumGroupedBy(string fileName, string currentST, CPbusinessLayer bslyr)
        {
            List<LCDDO> justStratum = new List<LCDDO>();
            justStratum = bslyr.getLCDOrdered("WHERE CutLeave = @p1 AND Stratum = @p2  GROUP BY ","SampleGroup,Species,STM","C",currentST);
            return justStratum;
        }   //  end GetStratumGroupedBy


        public static void LoadLCDID(ref ArrayList prtFields, LCDDO currGRP , int currPP, string currRPT)
        {
            prtFields.Add("");
            prtFields.Add(currGRP.Stratum.PadLeft(2,' '));
            //  first part of line is report dependent
            switch (currRPT)
            {
                case "VSM1":        case "VPA1":        case "VAL1":    case "VSM6":
                    prtFields.Add(currGRP.Species.PadRight(6, ' '));
                    if (currPP == 1)
                    {
                        prtFields.Add(currGRP.PrimaryProduct.PadLeft(2, '0'));
                        prtFields.Add("P");
                    }
                    else if (currPP == 2)
                    {
                        prtFields.Add(currGRP.SecondaryProduct.PadLeft(2, '0'));
                        prtFields.Add("S");
                    }
                    else if(currPP == 3)
                    {
                        prtFields.Add(currGRP.SecondaryProduct.PadLeft(2, '0'));
                        prtFields.Add("R");
                    }
                    prtFields.Add(currGRP.UOM.PadLeft(2, '0'));
                    prtFields.Add(currGRP.LiveDead);
                    prtFields.Add(currGRP.CutLeave.Trim());
                    prtFields.Add(currGRP.Yield.Trim());
                    prtFields.Add(currGRP.SampleGroup.PadLeft(2, ' '));
                    prtFields.Add(currGRP.STM);
                    prtFields.Add(currGRP.TreeGrade.PadLeft(1,' '));
                    if (currGRP.ContractSpecies == null)
                        prtFields.Add("    ");
                    else prtFields.Add(currGRP.ContractSpecies.PadRight(4, ' '));
                    break;
                case "VSM2":        case "VPA2":        case "VAL2":        case "LV01":
                    if (currPP == 1)
                    {
                        prtFields.Add(currGRP.PrimaryProduct.PadLeft(2, '0'));
                        prtFields.Add("P");
                    }
                    else if(currPP == 2)
                    {
                        prtFields.Add(currGRP.SecondaryProduct.PadLeft(2,'0'));
                        prtFields.Add("S");
                    }
                    else if(currPP == 3)
                    {
                        prtFields.Add(currGRP.SecondaryProduct.PadLeft(2,'0'));
                        prtFields.Add("R");
                    }
                    prtFields.Add(currGRP.UOM.PadLeft(2,'0'));
                    prtFields.Add(currGRP.SampleGroup.PadLeft(2,' '));
                    prtFields.Add(currGRP.STM.PadLeft(1,' '));
                    break;
                case "VSM3":        case "VPA3":        case "VAL3":        case "LV02":
                    if (currPP == 1)
                    {
                        prtFields.Add(currGRP.PrimaryProduct.PadLeft(2, '0'));
                        prtFields.Add("P");
                    }
                    else if (currPP == 2)
                    {
                        prtFields.Add(currGRP.SecondaryProduct.PadLeft(2, '0'));
                        prtFields.Add("S");
                    }
                    else if(currPP == 3)
                    {
                        prtFields.Add(currGRP.SecondaryProduct.PadLeft(2, '0'));
                        prtFields.Add("R");
                    }
                    prtFields.Add(currGRP.UOM.PadLeft(2,'0'));
                    break;
            }   //  end switch on report
            return;
        }   //  end LoadLCDID


        public static void LoadLCDmeans(ref ArrayList prtFields, List<LCDDO> currData, int hgtOne, int hgtTwo,
                                    int currPP, string currRPT, double STacres, string currMeth)
        {
            double summedValue;
            string fieldFormat1 = "{0,5:F0}";
            string fieldFormat2 = "{0,5:F1}";
            string fieldFormat3 = "{0,4:F1}";
            string fieldFormat4 = "{0,3:F0}";
            string fieldFormat5 = "{0,5:F3}";
            string fieldFormat6 = "{0,6:F0}";

            
            //  load total measured trees and total tallied trees based on report
            switch(currRPT)
            {
                case "VSM1":        //  prints only measured trees
                    if(currPP == 1)
                    {
                        summedValue = currData.Sum(ldo => ldo.MeasuredTrees);
                        prtFields.Add(Utilities.FormatField(summedValue,fieldFormat1).ToString());
                    }
                    else prtFields.Add("      ");
                    break;
                case "VSM2":        //  prints both measured and tallied trees
                case "VSM3":
                case "LV01":
                case "LV02":
                    if(currPP == 1)
                    {
                        summedValue = currData.Sum(ldo => ldo.MeasuredTrees);
                        prtFields.Add(Utilities.FormatField(summedValue,fieldFormat1).ToString());
                        summedValue = currData.Sum(ldo => ldo.TalliedTrees);
                        prtFields.Add(Utilities.FormatField(summedValue,fieldFormat1).ToString());
                    }
                    else
                    {
                        prtFields.Add("      ");
                        prtFields.Add("      ");
                    }
                    break;
                
            }   //  end switch

            //  remainder of fields are the same for all three reports except for products
            double summedEF;
            if(currPP == 1)
            {
                //  mean and quad mean dbh
                summedEF = currData.Sum(ldo => ldo.SumExpanFactor);
                if(summedEF > 0)
                {
                    summedValue = currData.Sum(ldo => ldo.SumDBHOBsqrd);
                    prtFields.Add(Utilities.FormatField((Math.Sqrt(summedValue/summedEF)),fieldFormat2).ToString());
                    summedValue = currData.Sum(ldo => ldo.SumDBHOB);
                    prtFields.Add(Utilities.FormatField((summedValue/summedEF),fieldFormat3).ToString());
                }
                else
                {
                    prtFields.Add("  0.0");
                    prtFields.Add(" 0.0");
                }   // endifload
                //  mean heights
                double numerAtor1 = 0;
                double numerAtor2 = 0;
                switch (hgtOne)
                {
                    case 1:     //  total height
                        numerAtor1 = currData.Sum(ldo => ldo.SumTotHgt);
                        break;
                    case 2:     //  Merch hgt PP
                        numerAtor1 = currData.Sum(ldo => ldo.SumMerchHgtPrim);
                        break;
                    case 3:     //  Merch hgt SP
                        numerAtor1 = currData.Sum(ldo => ldo.SumMerchHgtSecond);
                        break;
                    case 4:     //  hgt to upper stem diam
                        numerAtor1 = currData.Sum(ldo => ldo.SumHgtUpStem);
                        break;
                }   //  end switch
                switch (hgtTwo)
                {
                    case 1:     //  total height
                        numerAtor2 = currData.Sum(ldo => ldo.SumTotHgt);
                        break;
                    case 2:     //  Merch hgt PP
                        numerAtor2 = currData.Sum(ldo => ldo.SumMerchHgtPrim);
                        break;
                    case 3:     //  Merch hgt SP
                        numerAtor2 = currData.Sum(ldo => ldo.SumMerchHgtSecond);
                        break;
                    case 4:     //  hgt to upper stem diam
                        numerAtor2 = currData.Sum(ldo => ldo.SumHgtUpStem);
                        break;
                }   // end switch
                if(summedEF > 0)
                {
                    if(numerAtor1 > 0)
                        prtFields.Add(Utilities.FormatField((numerAtor1 / summedEF),fieldFormat2).ToString());
                    else prtFields.Add("     ");
                    if(numerAtor2 > 0)
                        prtFields.Add(Utilities.FormatField((numerAtor2 / summedEF),fieldFormat2).ToString());
                    else prtFields.Add("     ");
                }   //  endif

                //  Average defect --  calculated only for primary product
                summedValue = currData.Sum(ldo => ldo.SumGBDFT);
                double sumValueToo = currData.Sum(ldo => ldo.SumNBDFT);
                prtFields.Add(Utilities.FormatField((CommonEquations.AverageDefectPercent(summedValue, sumValueToo)),fieldFormat4).ToString());
                summedValue = currData.Sum(ldo => ldo.SumGCUFT);
                sumValueToo = currData.Sum(ldo => ldo.SumNCUFT);
                prtFields.Add(Utilities.FormatField((CommonEquations.AverageDefectPercent(summedValue, sumValueToo)),fieldFormat4).ToString());

                //  Ratio is similar -- calculated for primary product only
                summedValue = currData.Sum(ldo => ldo.SumGBDFT);
                sumValueToo = currData.Sum(ldo => ldo.SumGCUFT);
                prtFields.Add(Utilities.FormatField((CommonEquations.BoardCubicRatio(summedValue, sumValueToo)),fieldFormat5).ToString());
                summedValue = currData.Sum(ldo => ldo.SumNBDFT);
                sumValueToo = currData.Sum(ldo => ldo.SumNCUFT);
                prtFields.Add(Utilities.FormatField((CommonEquations.BoardCubicRatio(summedValue, sumValueToo)),fieldFormat5).ToString());

                //  Expansion factor is only printed for primary product
                //  and is different for S3P and 3P
                if (currMeth == "S3P" || currMeth == "3P")
                    summedEF = CommonEquations.Calculate3PTrees(currData, currMeth);
                prtFields.Add(Utilities.FormatField(summedEF * STacres, fieldFormat6).ToString());
            }
            else
            {
                prtFields.Add("     ");
                prtFields.Add("    ");
                prtFields.Add("     ");
                prtFields.Add("     ");
                prtFields.Add("   ");
                prtFields.Add("   ");
                prtFields.Add("     ");
                prtFields.Add("     ");
                prtFields.Add("      ");
            }   //  endif primary product
            return;
        }   //  end LoadLCDmeans


        public static void LoadLCDvolumes(double STacres, ref ArrayList prtFields, List<LCDDO> currData, int currPP,
                                            int perAcre)
        {
            double summedValue = 0;
            string fieldFormat7 = "{0,8:F0}";
            string fieldFormat8 = "{0,7:F0}";
            string fieldFormat9 = "{0,6:F1}";
            string fieldFormat10 = "{0,9:F0}";
            string fieldFormat11 = "{0,9:F1}";

            //  loads volume fields based on product
            if(currPP == 1)
            {
                summedValue = currData.Sum(ldo => ldo.SumGBDFT);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat7).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat10).ToString());
                summedValue = 0;
                summedValue = currData.Sum(ldo => ldo.SumGCUFT);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat8).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat10).ToString());
                summedValue = 0;
                summedValue = currData.Sum(ldo => ldo.SumNBDFT);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat7).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat10).ToString());
                summedValue = 0;
                summedValue = currData.Sum(ldo => ldo.SumNCUFT);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat8).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat10).ToString());
                summedValue = 0;
                summedValue = currData.Sum(ldo => ldo.SumCords);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat9).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat11).ToString());
                return;
            }
            else if (currPP == 2)
            {
                summedValue = currData.Sum(ldo => ldo.SumGBDFTtop); ;
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat7).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat10).ToString());
                summedValue = currData.Sum(ldo => ldo.SumGCUFTtop);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat8).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat10).ToString());
                summedValue = currData.Sum(ldo => ldo.SumNBDFTtop);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat7).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat10).ToString());
                summedValue = currData.Sum(ldo => ldo.SumNCUFTtop);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat8).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat10).ToString());
                summedValue = currData.Sum(ldo => ldo.SumCordsTop);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat9).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat11).ToString());
                return;
            }
            else if(currPP == 3)
            {
                prtFields.Add("        ");
                prtFields.Add("       ");
                summedValue = currData.Sum(ldo => ldo.SumBDFTrecv);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat7).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat10).ToString());
                summedValue = currData.Sum(ldo => ldo.SumCUFTrecv);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat8).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat10).ToString());
                summedValue = currData.Sum(ldo => ldo.SumCordsRecv);
                if (perAcre == 0)
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat9).ToString());
                else if (perAcre == 1)
                    prtFields.Add(Utilities.FormatField(summedValue / STacres, fieldFormat11).ToString());
                return;
            }   //  endif

            return ;
        }   //  end LoadLCDvolumes

        public static void LoadLCDvalue(double STacres, double Pacres, ref ArrayList prtFields, 
                                        List<LCDDO> currData, int currPP, ref double valueTotal,
                                        ref double wgtTotal, ref double totCubicTotal)
        {
            double summedValue;
            string fieldFormat1 = "{0,10:F0}";

            //  loads value, weight and total cubic based on product
            switch (currPP)
            {
                case 1:     //  primary
                    summedValue = currData.Sum(ldo => ldo.SumValue);
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat1).ToString());
                    prtFields.Add(Utilities.FormatField(summedValue / Pacres, fieldFormat1).ToString());
                    valueTotal += summedValue * STacres;

                    summedValue = currData.Sum(ldo => ldo.SumWgtMSP);
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat1).ToString());
                    prtFields.Add(Utilities.FormatField(summedValue / Pacres, fieldFormat1).ToString());
                    wgtTotal += summedValue * STacres;

                    summedValue = currData.Sum(ldo => ldo.SumTotCubic);
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat1).ToString());
                    prtFields.Add(Utilities.FormatField(summedValue / Pacres, fieldFormat1).ToString());
                    totCubicTotal += summedValue * STacres;
                    break;
                case 2:         // secondary
                    summedValue = currData.Sum(ldo => ldo.SumTopValue);
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat1).ToString());
                    prtFields.Add(Utilities.FormatField(summedValue / Pacres, fieldFormat1).ToString());
                    valueTotal += summedValue * STacres;

                    summedValue = currData.Sum(ldo => ldo.SumWgtMSS);
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat1).ToString());
                    prtFields.Add(Utilities.FormatField(summedValue / Pacres, fieldFormat1).ToString());
                    wgtTotal += summedValue * STacres;

                    //  no total cubic for secondary
                    prtFields.Add("         0");
                    prtFields.Add("         0");
                    break;
                case 3:         //  recovered
                    summedValue = currData.Sum(ldo => ldo.SumValueRecv);
                    prtFields.Add(Utilities.FormatField(summedValue * STacres, fieldFormat1).ToString());
                    prtFields.Add(Utilities.FormatField(summedValue / Pacres, fieldFormat1).ToString());
                    valueTotal += summedValue * STacres;

                    //  no weight or total cubic for recovered
                    prtFields.Add("         0");
                    prtFields.Add("         0");
                    prtFields.Add("         0");
                    prtFields.Add("         0");
                    break;
            }   // end switch
            return;
        }   //  end LoadLCDvalue

        public static void LoadLCDweight(double STacres, ref ArrayList prtFields, List<LCDDO> currData, int currPP,
                                            int perAcre)
        {
            double summedValue;
            string fieldFormat7 = "{0,5:F2}";
            string fieldFormat8 = "{0,6:F0}";
            foreach (LCDDO cd in currData)
            {
                //  loads volume fields based on product
                if (currPP == 1)
                {
                    //  Calculate ratios
                    if (cd.SumWgtMSP > 0)
                        summedValue = cd.SumGCUFT / (cd.SumWgtMSP / 2000);
                    else summedValue = 0;
                    prtFields.Add(Utilities.FormatField(summedValue, fieldFormat7).ToString());

                    if (cd.SumWgtMSP > 0)
                        summedValue = cd.SumNCUFT / (cd.SumWgtMSP / 2000);
                    else summedValue = 0;
                    prtFields.Add(Utilities.FormatField(summedValue, fieldFormat7).ToString());


                    prtFields.Add(Utilities.FormatField(cd.SumGCUFT * STacres, fieldFormat8).ToString());
                    prtFields.Add(Utilities.FormatField(cd.SumNCUFT * STacres, fieldFormat8).ToString());
                    prtFields.Add(Utilities.FormatField((cd.SumWgtMSP / 2000) * STacres, fieldFormat8).ToString());


                }
                if (currPP == 2)
                {
                    //  Calculate ratios
                    summedValue = 0;
                    prtFields.Add(Utilities.FormatField(summedValue, fieldFormat7).ToString());
                    summedValue = 0;
                    prtFields.Add(Utilities.FormatField(summedValue, fieldFormat7).ToString());

                    prtFields.Add(Utilities.FormatField(cd.SumGCUFTtop * STacres, fieldFormat8).ToString());
                    prtFields.Add(Utilities.FormatField(cd.SumNCUFTtop * STacres, fieldFormat8).ToString());
                    prtFields.Add(Utilities.FormatField((cd.SumWgtMSS / 2000) * STacres, fieldFormat8).ToString());

                }
            }
            return;
        }   //  end LoadLCDweight  
    }
}
