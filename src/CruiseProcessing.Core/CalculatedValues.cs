using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL;
using CruiseDAL.Schema;
using CruiseProcessing.Data;
using System.Collections;

namespace CruiseProcessing
{
    public class CalculatedValues
    {
        private CpDataLayer DataLayer;

        public CalculatedValues(CpDataLayer dataLayer)
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        public void CalcValues()
        {
            ClearCalculatedTables();
            MakePopulationIDs();
        }
        //  this will contain methods for any calculated values
        public void ClearCalculatedTables()
        {
            //  if any of these tables have data, delete all
            //  Tree calculated values
            //List<TreeCalculatedValuesDO> tcvList = bslyr.getTreeCalculatedValues();

            //if (tcvList.Count > 0)
            DataLayer.deleteTreeCalculatedValues();
            //bslyr.fileName = fileName;

            //  Log stock
            //List<LogStockDO> lsList = bslyr.getLogStock();
           // if (lsList.Count > 0)
                DataLayer.DeleteLogStock();
           // bslyr.fileName = fileName;

            //  LCD
            //List<LCDDO> lcdList = bslyr.getLCD();
            //if (lcdList.Count > 0)
                DataLayer.DeleteLCD();
            //bslyr.fileName = fileName;

            //  POP
            //List<POPDO> popList = bslyr.getPOP();
           // if (popList.Count > 0)
                DataLayer.DeletePOP();
            //bslyr.fileName = fileName;

            //  PRO
            //List<PRODO> proList = bslyr.getPRO();
            //if (proList.Count > 0)
                DataLayer.DeletePRO();
            //bslyr.fileName = fileName;

            return;
        }   //  end ClearCalculatedTables

        public void MakePopulationIDs()
        {
            var sale = DataLayer.GetSale();
            List<SampleGroupDO> sampleGroups = DataLayer.getSampleGroups();
            List<TreeDefaultValueDO> tdvList = DataLayer.getTreeDefaults();

            //  Load ID info into tables
            List<LCDDO> lcdList = new List<LCDDO>();
            List<POPDO> popList = new List<POPDO>();
            List<PRODO> proList = new List<PRODO>();
            List<TreeDO> tList = DataLayer.getTrees();

            //  need to check Contract Species in TDV table before doing unique
            //  reset to a blank if it is null -- September 2016
            List<TreeDefaultValueDO> treeDefaults = DataLayer.getTreeDefaults();
            foreach(TreeDefaultValueDO tdv in treeDefaults)
            {
                if (tdv.ContractSpecies == null)
                    tdv.ContractSpecies = " ";
                else if (tdv.ContractSpecies == "")
                    tdv.ContractSpecies = " ";
            }   //  end foreach
            DataLayer.SaveTreeDefaults(treeDefaults);

            foreach (SampleGroupDO sgd in sampleGroups)
            {
                
                //  Load LCD population IDs
                //  Need unique species, livedead and grade from Tree table
                // not sure about the count table -- need IDs from there?  11/2012
                List<TreeDO> distinctSpecies = DataLayer.GetDistinctSpeciesLiveDeadGradeStm((long)sgd.SampleGroup_CN);
                foreach(TreeDO t in distinctSpecies)
                {
                    LCDDO lcd = new LCDDO();
                    lcd.CutLeave = sgd.CutLeave;
                    lcd.Stratum = sgd.Stratum.Code;
                    lcd.SampleGroup = sgd.Code;
                    lcd.PrimaryProduct = sgd.PrimaryProduct;
                    lcd.SecondaryProduct = sgd.SecondaryProduct;
                    lcd.UOM = sgd.UOM ?? sale.DefaultUOM;
                    lcd.Species = t.Species;
                    lcd.LiveDead = t.LiveDead;
                    lcd.TreeGrade = t.Grade ?? "";
                    lcd.STM = t.STM;
                    //  per K.Cormier, because a null value in contract species causes
                    //  causes problems, it is being dropped from the population ID for LCD
                    //  Howeverm it will be stored as null but not used as part of the ID
                    //  September 2016
                    //  Found a slicker way to address this -- default contract species to a blank
                    //  however, this would be in the TreeDefaultValue table and only affects
                    //  new cruises.  CP still needs to set CS in TDV to blank (see above).
                    lcd.ContractSpecies = t.TreeDefaultValue.ContractSpecies;
                    //if (t.TreeDefaultValue.ContractSpecies != null)
                    //    lcd.ContractSpecies = t.TreeDefaultValue.ContractSpecies;
                    //else lcd.ContractSpecies = "";
                    if (t.Stratum.YieldComponent != null)
                        lcd.Yield = t.Stratum.YieldComponent;
                    else lcd.Yield = "";
                    //}   //  endif nthRow

                    lcdList.Add(lcd);
                }   //  end foreach loop on species

                //  Load POP population IDs
                POPDO pop = new POPDO();
                //  Don't need unique species for this group
                pop.CutLeave = sgd.CutLeave;
                pop.Stratum = sgd.Stratum.Code;
                pop.SampleGroup = sgd.Code;
                pop.PrimaryProduct = sgd.PrimaryProduct;
                pop.SecondaryProduct = sgd.SecondaryProduct;
                pop.UOM = sgd.UOM;
                //  check for sure-to-measure trees for this group
                //  Add non-sure-to-measure first  and then STM
                pop.STM = "N";
                popList.Add(pop);
                List<TreeDO> justSTM = tList.FindAll(
                    delegate(TreeDO td)
                    {
                        return sgd.CutLeave == td.SampleGroup.CutLeave && sgd.Stratum.Code == td.Stratum.Code &&
                                    sgd.Code == td.SampleGroup.Code && sgd.PrimaryProduct == td.SampleGroup.PrimaryProduct &&
                                    sgd.SecondaryProduct == td.SampleGroup.SecondaryProduct && sgd.UOM == td.SampleGroup.UOM &&
                                    td.STM == "Y";
                    });
                if (justSTM.Count > 0)
                {
                    POPDO popSTM = new POPDO();
                    popSTM.CutLeave = sgd.CutLeave;
                    popSTM.Stratum = sgd.Stratum.Code;
                    popSTM.SampleGroup = sgd.Code;
                    popSTM.PrimaryProduct = sgd.PrimaryProduct;
                    popSTM.SecondaryProduct = sgd.SecondaryProduct;
                    popSTM.UOM = sgd.UOM;
                    popSTM.STM = "Y";
                    popList.Add(popSTM);
                }   //  endif

                //  Load PRO population IDs
                //  These need cutting unit numbers -- from Cutting Unit 
                List<CuttingUnitStratumDO> strataUnits = DataLayer.getCuttingUnitStratum((long)sgd.Stratum_CN);
                foreach (CuttingUnitStratumDO cudo in strataUnits)
                {
                    PRODO pro = new PRODO();
                    pro.CutLeave = sgd.CutLeave;
                    pro.Stratum = sgd.Stratum.Code;
                    pro.CuttingUnit = cudo.CuttingUnit.Code;
                    pro.SampleGroup = sgd.Code;
                    pro.PrimaryProduct = sgd.PrimaryProduct;
                    pro.SecondaryProduct = sgd.SecondaryProduct;
                    pro.UOM = sgd.UOM;
                    //  check for sure-to-measure trees for this group
                    //  Add non-sure-to-measure first  and then STM
                    pro.STM = "N";
                    proList.Add(pro);
                    justSTM = tList.FindAll(
                        delegate(TreeDO td)
                        {
                            return sgd.CutLeave == td.SampleGroup.CutLeave && sgd.Stratum.Code == td.Stratum.Code &&
                                        cudo.CuttingUnit.Code == td.CuttingUnit.Code &&
                                        sgd.Code == td.SampleGroup.Code && sgd.PrimaryProduct == td.SampleGroup.PrimaryProduct &&
                                        sgd.SecondaryProduct == td.SampleGroup.SecondaryProduct && sgd.UOM == td.SampleGroup.UOM &&
                                        td.STM == "Y";
                        });
                    if (justSTM.Count > 0)
                    {
                        PRODO proSTM = new PRODO();
                        proSTM.CutLeave = sgd.CutLeave;
                        proSTM.Stratum = sgd.Stratum.Code;
                        proSTM.CuttingUnit = cudo.CuttingUnit.Code;
                        proSTM.SampleGroup = sgd.Code;
                        proSTM.PrimaryProduct = sgd.PrimaryProduct;
                        proSTM.SecondaryProduct = sgd.SecondaryProduct;
                        proSTM.UOM = sgd.UOM;
                        proSTM.STM = "Y";
                        proList.Add(proSTM);
                    }   //  endif
                }   //  end foreach loop on strataUnits
            }   //  end foreach loop on SampleGroup
            DataLayer.SaveLCD(lcdList);
            DataLayer.SavePOP(popList);
            DataLayer.SavePRO(proList);

            return;
        }   //  end MakePopulationIDs


        public void SumTreeCounts(StratumDO stratum,
                                  IReadOnlyList<PlotDO> stratumPlots,
                                  IReadOnlyList<CountTreeDO> stratumCountTrees,
                                  IReadOnlyList<LCDDO> stratumLcds,
                                  IReadOnlyList<POPDO> stratumPops,
                                  IReadOnlyList<PRODO> stratumPros)
        {
            SumTreeCountsLCD(stratum.Code, stratum.Method, stratumCountTrees, stratumPlots, stratumLcds);
            SumTreeCountsPOP(stratum.Code, stratumCountTrees, stratumPlots, stratumPops, stratum.Method);
            SumTreeCountsPRO(stratum.Code, stratumCountTrees, stratumPlots, stratumPros, stratum.Method);
        }

        // NOTE: Converted files - sum of KPI for 3P and S3P is the total of the SumKPI from the Count table
        //  plus the sum of measured KPI from the tree table.  No longer need to sum KPI from Count Tree and Tree
        //  April 2013
        //  methods to sum trees and KPI for individual methods
        public void SumTreeCountsLCD(string currST, string currMethod, IReadOnlyList<CountTreeDO> ctList,
                                            IReadOnlyList<PlotDO> justPlots, IReadOnlyList<LCDDO> stratumLcds)
        {

            string currSG = "*";
            string prevSP = "*";
            //  Sums trees and counts from either the CountTree table or tree count records
            foreach (LCDDO lcd in stratumLcds)
            {
                double firstSum = 0.0;
                double talliedSum = 0.0;
                double totalMeasuredKPI = 0.0;
                double totalKPI = 0.0;
                //  find measured trees for current LCD group
                List<TreeDO> lcdTrees = DataLayer.getLCDtrees(lcd, "M");
                //  measured trees is just a count of the trees for current LCD group
                lcd.MeasuredTrees = lcdTrees.Count();
               
                //  now need count trees from tree count records
                List<TreeDO> lcdCntTrees = DataLayer.getLCDtrees(lcd, "C");
                //  Sum all tree counts for first stage and total tallied
                //  see note in POP section on 100% method
                if (currMethod == "STR" || currMethod == "100")
                    firstSum = lcdTrees.Count();
                else if(currMethod != "3P")
                {
                    firstSum = lcdTrees.Sum(tdo => tdo.TreeCount);
                    firstSum += lcdCntTrees.Sum(tdo => tdo.TreeCount);
                }

                if (lcd.STM == "Y")
                {
                    firstSum += lcd.MeasuredTrees;
                    //talliedSum += lcdTrees.Sum(tdo => tdo.TreeCount);
                    talliedSum += lcd.MeasuredTrees;
                }
                else
                {
                    //if (currMethod == "3P" || currMethod == "100")
                    if(currMethod == "100")
                        talliedSum += lcd.MeasuredTrees;
                    else
                    {
                        talliedSum = lcdTrees.Sum(tdo => tdo.TreeCount);
                        talliedSum += lcdCntTrees.Sum(tdo => tdo.TreeCount);
                    }   //  endif on current method
                }   //  endif
                //  insurance trees go into just the tallied tree count
                List<TreeDO> lcdInsurance = DataLayer.getLCDtrees(lcd, "I");
                talliedSum += lcdInsurance.Sum(tdo => tdo.TreeCount);
                //  Complete totals based on method --  KPIs too
                switch (currMethod)
                {
                    case "STR":
                        currSG = "*";
                        prevSP = "*";
                        foreach (CountTreeDO ctd in ctList)
                        {
                            if (lcd.Stratum == ctd.SampleGroup.Stratum.Code &&
                                lcd.SampleGroup == ctd.SampleGroup.Code)
                            {
                                if (currSG != lcd.SampleGroup && prevSP != lcd.Species &&
                                    (ctd.TreeDefaultValue_CN == 0 || ctd.TreeDefaultValue_CN == null))
                                {
                                    //  multiple species for this sample group so avoid double counting
                                    talliedSum += ctd.TreeCount;
                                    currSG = lcd.SampleGroup;
                                    prevSP = lcd.Species;
                                }
                                else if(currSG == lcd.SampleGroup && prevSP != lcd.Species &&
                                    (ctd.TreeDefaultValue_CN == 0 || ctd.TreeDefaultValue_CN == null))
                                {
                                    //  do not add tree count
                                    prevSP = lcd.Species;
                                }
                                else if(currSG != lcd.SampleGroup && prevSP != lcd.Species &&
                                    (ctd.TreeDefaultValue_CN > 0 ||ctd.TreeDefaultValue_CN == null))
                                {
                                    //  sample group probably doesn't have multiple species
                                    talliedSum += ctd.TreeCount;
                                }
                                else if(currSG == lcd.SampleGroup && prevSP == lcd.Species &&
                                    (ctd.TreeDefaultValue_CN > 0 || ctd.TreeDefaultValue_CN == null))
                                {
                                    talliedSum += ctd.TreeCount;
                                }   //  endif
                            }   //  endif
                        }   //  end foreach count tree
                    
                        break;
                    case "3P":
                    case "S3P":
                        {
                            var lcdCountCounts = ctList.Where(
                                delegate (CountTreeDO ctd)
                                {
                                    return lcd.CutLeave == ctd.SampleGroup.CutLeave &&
                                           lcd.Stratum == ctd.SampleGroup.Stratum.Code &&
                                           lcd.SampleGroup == ctd.SampleGroup.Code &&
                                           lcd.Species == ctd.TreeDefaultValue.Species &&
                                           lcd.PrimaryProduct == ctd.SampleGroup.PrimaryProduct &&
                                           lcd.SecondaryProduct == ctd.SampleGroup.SecondaryProduct &&
                                           lcd.UOM == ctd.SampleGroup.UOM &&
                                           lcd.LiveDead == ctd.TreeDefaultValue.LiveDead &&
                                           lcd.Yield == ctd.SampleGroup.Stratum.YieldComponent &&
                                           //  per K.Cormier, dropping contract species from LCD ID
                                           //lcd.ContractSpecies == ctd.TreeDefaultValue.ContractSpecies &&
                                           lcd.TreeGrade == ctd.TreeDefaultValue.TreeGrade;
                                }).ToArray();
                            if (lcd.STM == "N")
                            {
                                firstSum += lcd.MeasuredTrees;
                                talliedSum += lcdCountCounts.Sum(ctd => ctd.TreeCount);
                                totalKPI += lcdCountCounts.Sum(ctd => ctd.SumKPI);
                            }

                            //  Sum measured KPI from trees
                            totalMeasuredKPI = lcdTrees.Sum(tdo => tdo.KPI);
                            //  for S3P and 3P, sum of KPI is total of count and measured KPI from the trees == November 2013
                            //  even though KPI on an individual may be zero for one or the other
                            //  however, countTree has sum of count and measured KPI for 3P so adding measured and count
                            //  from trees will double count for 3P ONLY -- December 2013
                            if (currMethod == "S3P")
                            {
                                totalKPI += lcdCntTrees.Sum(lc => lc.KPI);
                                totalKPI += lcdTrees.Sum(lt => lt.KPI);
                            }   //   endif on method
                            break;
                        }
                    case "F3P":
                    case "P3P":
                        //  Sum measured KPI from trees
                        totalMeasuredKPI = lcdTrees.Sum(tdo => tdo.KPI);
                        totalKPI = lcdCntTrees.Sum(tdo => tdo.KPI);
                        totalKPI += totalMeasuredKPI;
                        break;
                    case "3PPNT":
                        //  this will probably no longer work on legacy data until
                        //  the conversion program handles this change
                        if (lcdTrees.Count > 0)
                        {
                            //  means there are measured trees so this was a measured plot
                            //  KPI in the plot table is measured
                            totalMeasuredKPI = justPlots.Sum(pd => pd.KPI);
                            //  plus add to total KPI
                            totalKPI = justPlots.Sum(pd => pd.KPI);
                        }
                        else
                        {
                            //  no measured trees means this is a count plot
                            //  just add plot KPI to total KPI
                            totalKPI += justPlots.Sum(pd => pd.KPI);
                        }   //  endif
                        //  this piece of code will no longer work -- September 2013
                        //totalMeasuredKPI = lcdTrees.Sum(tdo => tdo.KPI);
                        //totalKPI += lcdCountCounts.Sum(ctd => ctd.SumKPI);
                        //  Add plot KPI to total KPI
                        //totalKPI += justPlots.Sum(pd => pd.KPI); 
                        break;
                }   //  end switch on method

                lcd.FirstStageTrees = firstSum;
                lcd.TalliedTrees = talliedSum;
                lcd.SumKPI = totalKPI;
                lcd.SumMeasuredKPI = totalMeasuredKPI;
            }   //  end foreach loop

        }


        public void SumTreeCountsPOP(string currST, IReadOnlyList<CountTreeDO> stratumCountTrees, IReadOnlyList<PlotDO> stratumPlots,
                                            IReadOnlyList<POPDO> stratumPops, string currMethod)
        {
            //  also need first and second stage sample counts
            double stage1 = 0.0;

            double stage2 = 0.0;
            //  Sums trees and counts from either the Count Tree table or tree count records
            foreach (POPDO pop in stratumPops)
            {
                double firstSum = 0.0;
                double talliedSum = 0.0;
                double totalKPI = 0.0;
                double totalMeasuredKPI = 0.0;
                //  find measured trees for current POP group
                List<TreeDO> popTrees = DataLayer.getPOPtrees(pop, "M");
                //  measured tree is just a count of the trees for the current group
                pop.MeasuredTrees = popTrees.Count();

                //  now need count trees from tree count records
                List<TreeDO> popCntTrees = DataLayer.getPOPtrees(pop, "C");

                //  sum all tree counts for first stage and total tallied
                if (currMethod == "STR" || currMethod == "100")
                {
                    firstSum = popTrees.Count();
                    //  I thought tree count was supposed to default to 1 for 100% -- Feb 2014
                    //  Apparently, FScruiser can put whatever it wants in tree count
                }
                else if(currMethod != "3P")
                {
                    firstSum = popTrees.Sum(tdo => tdo.TreeCount);
                    firstSum += popCntTrees.Sum(tdo => tdo.TreeCount);
                }
                if (pop.STM == "Y")
                {
                    firstSum += pop.MeasuredTrees;
                    talliedSum += pop.MeasuredTrees;
                    //talliedSum = popTrees.Sum(tdo => tdo.TreeCount);
                }
                else
                {
                    //  see note above on 100% method
                    //if (currMethod == "3P" || currMethod == "100")
                    if(currMethod == "100")
                        talliedSum += pop.MeasuredTrees;
                    else
                    {
                        talliedSum = popTrees.Sum(tdo => tdo.TreeCount);
                        talliedSum += popCntTrees.Sum(tdo => tdo.TreeCount);
                    }   //  endif on current method
                }   //  endif
                //  insurance trees go into just the tallied tree count
                List<TreeDO> lcdInsurance = DataLayer.getPOPtrees(pop, "I");
                talliedSum += lcdInsurance.Sum(tdo => tdo.TreeCount);
                //  Complete totals for tree count and stage samples and KPIs based on method
                stage1 = stratumPlots.Count();

                switch (currMethod)
                {
                    case "100":
                        {
                            stage1 = popTrees.Count();
                            break;
                        }
                    case "STR":
                        {
                            stage1 = popTrees.Count();
                            var popCountCounts = stratumCountTrees.Where(
                                delegate (CountTreeDO ctd)
                                {
                                    return pop.CutLeave == ctd.SampleGroup.CutLeave &&
                                           pop.Stratum == ctd.SampleGroup.Stratum.Code &&
                                           pop.SampleGroup == ctd.SampleGroup.Code &&
                                           pop.PrimaryProduct == ctd.SampleGroup.PrimaryProduct &&
                                           pop.SecondaryProduct == ctd.SampleGroup.SecondaryProduct &&
                                           pop.UOM == ctd.SampleGroup.UOM;
                                });
                            talliedSum += popCountCounts.Sum(ctd => ctd.TreeCount);
                            break;
                        }
                    case "S3P":
                    case "3P":
                        {
                            if (currMethod == "S3P") stage2 = popTrees.Count();
                            var popCountCounts = stratumCountTrees.Where(
                                delegate (CountTreeDO ctd)
                                {
                                    return pop.CutLeave == ctd.SampleGroup.CutLeave &&
                                           pop.Stratum == ctd.SampleGroup.Stratum.Code &&
                                           pop.SampleGroup == ctd.SampleGroup.Code &&
                                           pop.PrimaryProduct == ctd.SampleGroup.PrimaryProduct &&
                                           pop.SecondaryProduct == ctd.SampleGroup.SecondaryProduct &&
                                           pop.UOM == ctd.SampleGroup.UOM;
                                });
                            if (pop.STM == "N")
                            {
                                stage1 = popTrees.Count();
                                firstSum += pop.MeasuredTrees;
                                talliedSum += popCountCounts.Sum(ctd => ctd.TreeCount);
                                totalKPI += popCountCounts.Sum(ctd => ctd.SumKPI);
                            }
                            //  Sum measured KPI from trees
                            totalMeasuredKPI = popTrees.Sum(tdo => tdo.KPI);
                            //  for S3P and 3P, sum of KPI is total of count and measured KPI from the trees == November 2013
                            //  even though individual KPI may be zero for either one.
                            //  however, countTree has sum of count and measured KPI for 3P so adding measured and count
                            //  from trees will double count for 3P ONLY -- December 2013
                            if (currMethod == "S3P")
                            {
                                totalKPI += popCntTrees.Sum(lc => lc.KPI);
                                totalKPI += popTrees.Sum(lt => lt.KPI);
                            }   //  endif on method
                            break;
                        }
                    case "PCM":
                    case "PCMTRE":
                        {
                            stage2 = popTrees.Count();
                            break;
                        }
                    case "F3P":
                    case "P3P":
                        {
                            stage2 = popTrees.Count();
                            totalMeasuredKPI = popTrees.Sum(tdo => tdo.KPI);
                            totalKPI = popCntTrees.Sum(tdo => tdo.KPI);
                            totalKPI += totalMeasuredKPI;
                            //totalKPI += popCountCounts.Sum(ctd => ctd.SumKPI); // plot methods don't have KPI in count tree table
                            break;
                        }
                    case "FCM":
                        {
                            //  August 2014 -- according to Ken C. this needs to be just the total measured trees
                            //  and not the sum of tree counts.  Some tree counts could be zero when they should be one.
                            //stage2 = popTrees.Sum(p => p.TreeCount);
                            stage2 = pop.MeasuredTrees;
                            break;
                        }
                    case "3PPNT":
                        {
                            //  Stage 2 samples
                            foreach (PlotDO pdo in stratumPlots)
                            {
                                int nthRow = popTrees.FindIndex(
                                    delegate (TreeDO tdo)
                                    {
                                        return tdo.Plot_CN == pdo.Plot_CN;
                                    });
                                if (nthRow >= 0)
                                {
                                    stage2++;
                                    nthRow = -1;
                                }   //  endif nthRow
                            }   //  end foreach loop
                                //  this will probably no longer work on legacy data until
                                //  the conversion program handles this change
                            if (popTrees.Count > 0)
                            {
                                //  means there are measured trees so this was a measured plot
                                //  KPI in the plot table is measured
                                totalMeasuredKPI = stratumPlots.Sum(pd => pd.KPI);
                                //  plus add to total KPI
                                totalKPI = stratumPlots.Sum(pd => pd.KPI);
                            }
                            else
                            {
                                //  no measured trees means this is a count plot
                                //  just add plot KPI to total KPI
                                totalKPI += stratumPlots.Sum(pd => pd.KPI);
                            }   //  endif
                                //  this piece of code will no longer work -- September 2013
                                //totalMeasuredKPI = lcdTrees.Sum(tdo => tdo.KPI);
                                //totalKPI += lcdCountCounts.Sum(ctd => ctd.SumKPI);
                                //  Add plot KPI to total KPI
                                //totalKPI += justPlots.Sum(pd => pd.KPI); 
                                //totalMeasuredKPI = popTrees.Sum(tdo => tdo.KPI);
                                //totalKPI += popCntTrees.Sum(tdo => tdo.KPI);
                                //  Also need plot KPI for this method
                                //totalKPI += justPlots.Sum(pd => pd.KPI);
                            break;
                        }
                }   //  end switch on method
                pop.FirstStageTrees = firstSum;
                pop.TalliedTrees = talliedSum;
                pop.SumKPI = totalKPI;
                pop.SumMeasuredKPI = totalMeasuredKPI;
                pop.StageOneSamples = stage1;
                pop.StageTwoSamples = stage2;
            }   //  end foreach loop
        } 


        public void SumTreeCountsPRO(string currST, IReadOnlyList<CountTreeDO> stratumCountTrees, IReadOnlyList<PlotDO> justPlots, IReadOnlyList<PRODO> justCurrentPRO, string currMethod)
        {
            //  Sums tree counts from either the Count Tree table or tree count records
            foreach (PRODO pro in justCurrentPRO)
            {
                double firstSum = 0.0;
                double talliedSum = 0.0;
                double totalKPI = 0.0;
                double totalMeasuredKPI = 0.0;
                //  find measured trees for current PRO group
                List<TreeDO> proTrees = DataLayer.getPROtrees(pro, "M");
                //  measured trees is just a count of the trees for current PRO group
                pro.MeasuredTrees = proTrees.Count();

                //  now need count trees from tree count records
                List<TreeDO> proCntTrees = DataLayer.getPROtrees(pro, "C");
                //  Sum all tree counts for first stage and total tallied
                //  see note in POP or LCD section on 100% method
                if (currMethod == "STR" || currMethod == "100")
                    firstSum = proTrees.Count();
                else if(currMethod != "3P")
                {
                    firstSum = proTrees.Sum(tdo => tdo.TreeCount);
                    firstSum += proCntTrees.Sum(tdo => tdo.TreeCount);
                }

                if (pro.STM == "Y")
                {
                    firstSum += pro.MeasuredTrees;
                    talliedSum += pro.MeasuredTrees;
                    //talliedSum = proTrees.Sum(tdo => tdo.TreeCount);
                }
                else
                {
                    //if (currMethod == "3P" || currMethod == "100")
                    if(currMethod == "100")
                        talliedSum += pro.MeasuredTrees;
                    else
                    {
                        talliedSum = proTrees.Sum(tdo => tdo.TreeCount);
                        talliedSum += proCntTrees.Sum(tdo => tdo.TreeCount);
                    }   //  endif on current method
                }   //  endif
                //  insurance trees go into just the tallied tree count
                List<TreeDO> lcdInsurance = DataLayer.getPROtrees(pro, "I");
                talliedSum += lcdInsurance.Sum(tdo => tdo.TreeCount);
                //  Complete totals and KPIs based on method
                //List<CountTreeDO> proCountCounts = new List<CountTreeDO>();
                switch (currMethod)
                {
                    case "3P":
                    case "S3P":
                        {
                            var proCountCounts = stratumCountTrees.Where(
                                delegate (CountTreeDO ctd)
                                {
                                    return pro.CutLeave == ctd.SampleGroup.CutLeave &&
                                           pro.Stratum == ctd.SampleGroup.Stratum.Code &&
                                           pro.SampleGroup == ctd.SampleGroup.Code &&
                                           pro.CuttingUnit == ctd.CuttingUnit.Code &&
                                           pro.PrimaryProduct == ctd.SampleGroup.PrimaryProduct &&
                                           pro.SecondaryProduct == ctd.SampleGroup.SecondaryProduct &&
                                           pro.UOM == ctd.SampleGroup.UOM;
                                });
                            if (pro.STM == "N")
                            {
                                firstSum += pro.MeasuredTrees;
                                talliedSum += proCountCounts.Sum(ctd => ctd.TreeCount);
                                totalKPI += proCountCounts.Sum(ctd => ctd.SumKPI);
                            }
                            //  Sum measured KPI from trees
                            totalMeasuredKPI = proTrees.Sum(tdo => tdo.KPI);
                            //  for S3P, sum of KPI is total of count and measured KPI from the trees == November 2013
                            //  even though individual KPI may be zero for either method
                            //  however, countTree has sum of count and measured KPI for 3P so adding measured and count
                            //  from trees will double count for 3P ONLY -- December 2013
                            if (currMethod == "S3P")
                            {
                                totalKPI += proCntTrees.Sum(lc => lc.KPI);
                                totalKPI += proTrees.Sum(lt => lt.KPI);
                            }   //  endif on current method
                            break;
                        }
                    case "STR":
                        {
                            var proCountCounts = stratumCountTrees.Where(
                                delegate (CountTreeDO ctd)
                                {
                                    return pro.CutLeave == ctd.SampleGroup.CutLeave &&
                                           pro.Stratum == ctd.SampleGroup.Stratum.Code &&
                                           pro.SampleGroup == ctd.SampleGroup.Code &&
                                           pro.CuttingUnit == ctd.CuttingUnit.Code &&
                                           pro.PrimaryProduct == ctd.SampleGroup.PrimaryProduct &&
                                           pro.SecondaryProduct == ctd.SampleGroup.SecondaryProduct &&
                                           pro.UOM == ctd.SampleGroup.UOM;
                                });
                            talliedSum += proCountCounts.Sum(ctd => ctd.TreeCount);
                            break;
                        }
                    case "F3P":
                    case "P3P":
                        {
                            //  Sum measured KPI from trees
                            totalMeasuredKPI = proTrees.Sum(tdo => tdo.KPI);
                            totalKPI = proCntTrees.Sum(tdo => tdo.KPI);
                            totalKPI += totalMeasuredKPI;
                            //totalKPI += proCountCounts.Sum(ctd => ctd.SumKPI); // kpis not stored in count tree table for plot methods
                            break;
                        }
                    case "3PPNT":
                        {
                            //  this will probably no longer work on legacy data until
                            //  the conversion program handles this change
                            if (proTrees.Count > 0)
                            {
                                //  means there are measured trees so this was a measured plot
                                //  KPI in the plot table is measured
                                totalMeasuredKPI = justPlots.Sum(pd => pd.KPI);
                                //  plus add to total KPI
                                totalKPI = justPlots.Sum(pd => pd.KPI);
                            }
                            else
                            {
                                //  no measured trees means this is a count plot
                                //  just add plot KPI to total KPI
                                totalKPI += justPlots.Sum(pd => pd.KPI);
                            }   //  endif
                                //  this piece of code will no longer work -- September 2013
                                //totalMeasuredKPI = lcdTrees.Sum(tdo => tdo.KPI);
                                //totalKPI += lcdCountCounts.Sum(ctd => ctd.SumKPI);
                                //  Add plot KPI to total KPI
                                //totalKPI += justPlots.Sum(pd => pd.KPI); 
                                //  Sum measured KPI from trees
                                //totalMeasuredKPI = proTrees.Sum(tdo => tdo.KPI);
                                //totalKPI += proCountCounts.Sum(ctd => ctd.SumKPI);
                                //  Add plot KPI to total KPI
                                //totalKPI += justPlots.Sum(pd => pd.KPI);
                            break;
                        }
                }   //  end switch on method

                pro.FirstStageTrees = firstSum;
                pro.TalliedTrees = talliedSum;
                pro.SumKPI = totalKPI;
                pro.SumMeasuredKPI = totalMeasuredKPI;
            }   //  end foreach loop

        }

        public void CalcExpFac(StratumDO sdo, List<PlotDO> justPlots, List<POPDO> justCurrentPOP)
        {
            //  Calculates expansion factor, tree factor and point factor for each tree in the current population
            

            //  Need total number of plots and measured plots (3PPNT only)
            double totalPlots = justPlots.Count();
            

            //  process by population
            foreach (POPDO pdo in justCurrentPOP)
            {
                //  pull trees for population
                List<TreeDO> justTrees = new List<TreeDO>();
                if (sdo.Method == "FIXCNT")
                    justTrees = DataLayer.getPOPtrees(pdo, "C");
                else
                    justTrees = DataLayer.getPOPtrees(pdo, "M");

                double totalMeasPlots = 0.0; // for 3PPNT
                //  3PPNT uses measured plots
                if (sdo.Method == "3PPNT")
                {
                    foreach (PlotDO p in justPlots)
                    {
                        int nthRow = justTrees.FindIndex(
                            delegate(TreeDO td)
                            {
                                return td.Plot_CN == p.Plot_CN && td.Stratum_CN == p.Stratum_CN;
                            });
                        if (nthRow >= 0)
                            totalMeasPlots++;
                    }
                }

                //  calculate factors on each tree
                foreach (TreeDO tdo in justTrees)
                {
                    double expansionFactor = 0.0;
                    double treeFactor = 0.0;
                    double pointFactor = 0.0;

                    //  Calculate point factor for P3P S3P F3P PCM FCM and 3PPNT
                    switch (sdo.Method)
                    {
                        case "P3P":
                        case "S3P":
                        case "F3P":
                            pointFactor = CommonEquations.Frequency3P(pdo.SumKPI, tdo.KPI, pdo.MeasuredTrees);
                            break;
                        case "PCM":
                        case "PCMTRE":
                            pointFactor = CommonEquations.CalcTreeFactor(pdo.FirstStageTrees, pdo.MeasuredTrees, 0.0, 1, 2);
                            break;
                        case "FCM":
                            pointFactor = CommonEquations.CalcTreeFactor(0.0, pdo.MeasuredTrees, pdo.TalliedTrees, 3, 2);
                            break;
                        case "3PPNT":
                            //  uses plot KPI
                            PlotDO plotKPI = justPlots.Find(
                                delegate(PlotDO p)
                                {
                                    return p.Plot_CN == tdo.Plot_CN;
                                });
                            pointFactor = CommonEquations.Frequency3P(pdo.SumKPI, plotKPI.KPI, totalMeasPlots);
                            break;
                    }   //  end switch on method to calculate point factor

                    //  Calculate tree factor for FIXCNT FIX F3P FCM PNT P3P PCM 3PPNT and S3P
                    switch (sdo.Method)
                    {
                        case "FIXCNT":
                        case "FIX":
                        case "F3P":
                            treeFactor = sdo.FixedPlotSize;
                            break;
                        case "FCM":
                            if (totalPlots > 0) treeFactor = sdo.FixedPlotSize / totalPlots;
                            break;
                        case "PNT":
                        case "P3P":
                        case "PCM":
                        case "PCMTRE":
                        case "3PPNT":
                            if (tdo.DBH > 0)
                                treeFactor = CommonEquations.PointSampleFrequency(sdo.BasalAreaFactor, tdo.DBH);
                            else if (tdo.DRC > 0)
                                treeFactor = CommonEquations.PointSampleFrequency(sdo.BasalAreaFactor, tdo.DRC);
                            break;
                        case "S3P":
                            treeFactor = CommonEquations.CalcTreeFactor(pdo.FirstStageTrees, 0.0, pdo.TalliedTrees, 3, 1);
                            break;
                    }   //  end switch on method to calculate tree factor

                    //  Calculate expansion factor
                    switch (sdo.Method)
                    {
                        case "100":         
                            expansionFactor = 1.0;
                            break;
                        case "FIXCNT":
                            if (totalPlots > 0) expansionFactor = (sdo.FixedPlotSize / totalPlots) * tdo.TreeCount;
                            break;
                        case "FIX":
                            if (totalPlots > 0) expansionFactor = sdo.FixedPlotSize / totalPlots;
                            break;
                        case "PNT":
                            if (totalPlots > 0) expansionFactor = treeFactor / totalPlots;
                            break;
                        case "STR":
                            expansionFactor = CommonEquations.CalcTreeFactor(0.0, pdo.MeasuredTrees, pdo.TalliedTrees, 3, 2);
                            break;
                        case "P3P":
                        case "F3P":
                        case "PCM":
                        case "PCMTRE":
                        case "3PPNT":
                            if (totalPlots > 0) expansionFactor = (treeFactor * pointFactor) / totalPlots;
                            break;
                        case "FCM":
                        case "S3P":
                            expansionFactor = treeFactor * pointFactor;
                            break;
                        case "3P":
                            expansionFactor = CommonEquations.Frequency3P(pdo.SumKPI, tdo.KPI, pdo.MeasuredTrees);
                            break;
                    }   //  end switch on method to calculate expansion factor
                    if (tdo.STM == "Y") expansionFactor = 1.0;

                    //  round just the expansion factor
                    expansionFactor = CommonEquations.RoundExpansionFactor(expansionFactor);
                    //  store factors
                    tdo.ExpansionFactor = (float)expansionFactor;
                    tdo.TreeFactor = (float)treeFactor;
                    tdo.PointFactor = (float)pointFactor;
                }
                //  save this bunch of trees
                DataLayer.SaveTrees(justTrees);
            }
        }

    }
}
