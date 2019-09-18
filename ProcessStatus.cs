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

namespace CruiseProcessing
{
    public partial class ProcessStatus : Form
    {

        public ProcessStatus()
        {
            InitializeComponent();
        }
        public string fileName;

        private void on_GO(object sender, EventArgs e)
        {
            processingStatus.Text= "READY TO BEGIN?  Click GO.";
            Cursor.Current = Cursors.WaitCursor;
            //  perform edit checks -- 
            processingStatus.Text = "Edit checking the data.  Please wait.";
            processingStatus.Refresh();
            //  calls edit check routines
            string outputFileName;
            //  check for errors from FScruiser before running edit checks
            //  generate an error report 
            //  June 2013
            List<ErrorLogDO> fscList = Global.BL.getErrorMessages("E", "FScruiser").ToList();
            if (fscList.Count > 0)
            {
                ErrorReport eRpt = new ErrorReport();
                eRpt.fileName = fileName;
                outputFileName = eRpt.PrintFScruiserErrors(fscList);
                string outputMessage = "ERRORS FROM FSCRUISER FOUND!\nCorrect data and rerun\nOutput file is:" + outputFileName;
                MessageBox.Show(outputMessage, "ERRORS", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //  request made to open error report in preview -- May 2015
                PrintPreview pp = new PrintPreview();
                pp.fileName = outputFileName;
                pp.setupDialog();
                pp.ShowDialog();
                Environment.Exit(0);     
            }   //  endif report needed

            //  clear out error log table for just CruiseProcessing before performing checks
            Global.BL.DeleteErrorMessages();

            EditChecks eChecks = new EditChecks();
            eChecks.fileName = fileName;

            List<SaleDO> saleList = Global.BL.getSale().ToList();
            List<SampleGroupDO> sgList = Global.BL.getSampleGroups().ToList();
            List<StratumDO> sList = Global.BL.getStratum().ToList();
            List<CountTreeDO> ctList = Global.BL.getCountTrees().ToList();
            List<TreeDO> tList = Global.BL.getTrees().ToList();

            int errors = eChecks.TableEditChecks(Global.BL.getSale(),
                sList,
                ctList,
                tList,
                sgList);

            if (errors == -1)
            {
                //  no measured trees detected in the cruise.  critical errpor stops the program.
                Close();
                return;
            }   //  endif
            errors = eChecks.MethodChecks();
            if (errors == -1)
            {
                //  empty stratum detected and user wants to quit
                Close();
                return;
            }   //  endif
            //  just check the ErrorLog table for entries
            List<ErrorLogDO> errList = Global.BL.getErrorMessages("E", "CruiseProcessing").ToList();
            if (errList.Count > 0)
            {
                ErrorReport er = new ErrorReport();
                er.fileName = fileName;
                outputFileName = er.PrintErrorReport(errList);
                string outputMessage = "ERRORS FOUND!\nCorrect data and rerun\nOutput file is:" + outputFileName;
                MessageBox.Show(outputMessage, "ERRORS", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //  request made to open error report in preview -- May 2015
                PrintPreview pp = new PrintPreview();
                pp.fileName = outputFileName;
                pp.setupDialog();
                pp.ShowDialog();
                Environment.Exit(0);
            }   //  endif report needed
  
            //  Show editCheck message -- edit checks complete
            editCheck.Enabled = true;
            
            //  next show preparation of data
            processingStatus.Text = "Preparing data for processing.";
            processingStatus.Refresh();

            //  before making IDs, need to check for blank or null secondary products in sample groups
            //  if blank, default to 02 for every region but 6 where it will be 08 instead
            //  put a warning message in the error log table indicating the secondary product was set to a default
            //  June 2013
            string currRegion = Global.BL.getRegion();
            DefaultSecondaryProduct(currRegion);

            CalculatedValues calcVal = new CalculatedValues();


            //  retrieve lists needed and sets up population IDs
            calcVal.fileName = fileName;
            //List<TreeCalculatedValuesDO> tcvList = Global.BL.getTreeCalculatedValues();
            //List<TreeDefaultValueDO> tdvList = Global.BL.getTreeDefaults();
            List<CuttingUnitDO> cuList = Global.BL.getCuttingUnits().ToList();
            List<LCDDO> lcdList = Global.BL.getLCD().ToList();
            List<POPDO> popList = Global.BL.getPOP().ToList();
            List<PRODO> proList = Global.BL.getPRO().ToList();
            List<PlotDO> pList = Global.BL.getPlots().ToList();
            //List<VolumeEquationDO> vqList = Global.BL.getVolumeEquations();

            calcVal.ClearCalculatedTables();

            calcVal.MakePopulationIDs(sgList);
            //  now need some other tables to start summing values
            List<PlotDO> justPlots;
            //  show preparation of data is complete
            prepareCheck.Enabled = true;
            //  now loop through strata and show status message updating for each stratum
            StringBuilder sb = new StringBuilder();
            foreach (StratumDO sdo in sList)
            {
                CalculateTreeValues calcTreeVal = new CalculateTreeValues();
                //  update status message for next stratum
                sb.Remove(0, sb.Length);
                sb.Append("Calculating stratum ");
                sb.Append(sdo.Code);
                processingStatus.Text = sb.ToString();
                processingStatus.Refresh();
                
                //  Sum counts and KPI for LCD table
                justPlots = PlotMethods.GetStrata(pList, sdo.Code).ToList();
                //  need cut and leave trees for this
                //List<LCDDO> justCurrentLCD = LCDmethods.GetStratum(lcdList, sdo.Code);
                calcVal.SumTreeCountsLCD(sdo.Code, ctList, justPlots, sdo.Method, lcdList);

                //  Sum counts and KPI for POP table
               // List<POPDO> justCurrentPOP = POPmethods.GetStratumData(popList, sdo.Code, "");
                calcVal.SumTreeCountsPOP(sdo.Code, ctList, justPlots, sdo.Method, popList);

                //  Sum counts and KPI for PRO table
               // List<PRODO> justCurrentPRO = Global.BL.getPRO(sdo.Code);
                calcVal.SumTreeCountsPRO(sdo.Code, (long)sdo.Stratum_CN, tList, cuList, ctList, sgList, justPlots, proList, sdo.Method);

                //  Calculate expansion factor
                calcVal.CalcExpFac(sdo, justPlots, popList);

                //  Calculate volumes
                //calcTreeVal.fileName = fileName;
                calcTreeVal.ProcessTrees(saleList, tList, Global.BL.getVolumeEquations(), sdo.Code, sdo.Method, (long)sdo.Stratum_CN);


                //  Update 3P tally
                //List<TreeDO> justCurrentStratum = Global.BL.getTreeStratum((long)sdo.Stratum_CN);

                if (sdo.Method == "3P")
                {
                    Update3Ptally(ctList, sdo.Code, lcdList);
                }   //  endif method is 3P


                //  Update expansion factors for methods 3PPNT, F3P, and P3P
                if (sdo.Method == "3PPNT" || sdo.Method == "F3P" || sdo.Method == "P3P")
                {
                    //List<TreeCalculatedValuesDO> tcvList = Global.BL.getTreeCalculatedValues((int)sdo.Stratum_CN);
                    UpdateExpansionFactors(Global.BL.getTreeCalculatedValues(), (long)sdo.Stratum_CN);
                    //  Save update
                }   //  endif on method

                //  Sum data for the LCD, POP and PRO table
                SumAll sall = new SumAll();
                sall.fileName = fileName;
                sall.SumAllValues(sdo.Code, sdo.Method, (long)sdo.Stratum_CN, sList, pList, lcdList,
                                    popList, proList);
                Global.BL.SavePOP(popList);
                Global.BL.SavePRO(proList);
                Global.BL.SaveLCD(lcdList);

                //  Update STR tally after expansion factors are summed
                if (sdo.Method == "STR")
                {

                    UpdateSTRtally(fileName, sdo.Code, ctList, lcdList);
                }   //  endif method is STR

            }   //  end foreach stratum

            //  show volume calculation is finished
            volumeCheck.Enabled = true;
            processingStatus.Text = "Processing is DONE";
            processingStatus.Refresh();
            System.Threading.Thread.Sleep(5000);
            Close();
            return;
        }   //  end on_GO


        private void Update3Ptally(List<CountTreeDO> ctList, string currST,List<LCDDO>lcdList)
        {
            double TotalEF = 0;
            double TotalMeas = 0;
            double TotalCount = 0;

            //  Need LCD for 3p stratum ordered by sample group, species (and STM)
            //List<LCDDO> LCDstratum = Global.BL.getLCDOrdered("WHERE CutLeave = ? AND Stratum = ?  GROUP BY ", "SampleGroup,Species,STM", "C", currST);
            foreach (LCDDO l in Global.BL.getLCDOrdered("WHERE CutLeave = ? AND Stratum = ?  GROUP BY ", "SampleGroup,Species,STM", "C", currST))
            {
                //  find each group in just current LCD
                //lcdList.FindAll()
                var currentGroup = lcdList.FindAll(lcd => lcd.Stratum == currST &&
                                           lcd.SampleGroup == l.SampleGroup && lcd.Species == l.Species && lcd.STM == l.STM);
                //delegate (LCDDO ld)
                //    {
                //        return l.SampleGroup == ld.SampleGroup && l.Species == ld.Species && l.STM == ld.STM;
                //    });
                TotalEF = currentGroup.Sum(ldo => ldo.SumExpanFactor);

                if(l.STM == "N")
                {
                    //  Find counts
                    //List<CountTreeDO> justCounts = ctList.FindAll(
                    //    delegate(CountTreeDO ctdo)
                    //    {
                    //        if (ctdo.TreeDefaultValue != null)
                    //            return ctdo.SampleGroup.Code == l.SampleGroup &&
                    //                 ctdo.TreeDefaultValue.Species == l.Species;
                    //        else return false;
                    //    });
                    var justCounts = ctList.FindAll(ct => ct.SampleGroup.Code == l.SampleGroup && ct.TreeDefaultValue.Species == l.Species);
                    if (justCounts.Count > 0)
                    {
                        TotalCount = justCounts.Sum(ct => ct.TreeCount);
                        TotalMeas = currentGroup.Sum(lcddo => lcddo.MeasuredTrees);
                        TotalCount += TotalMeas;
                    }   //  endif justCounts is empty
                }
                else if(l.STM == "Y")  TotalCount = currentGroup.Sum(lcdo => lcdo.MeasuredTrees);

                //  Add any insurance trees to count
                if(l.STM == "N")
                    TotalCount += Global.BL.getInsureTrees(l.Stratum, l.SampleGroup, l.Species, l.STM).Sum(t => t.TreeCount);

                //  Recalc tallied trees for this group
                foreach (LCDDO ldo in currentGroup)
                    if (TotalEF > 0)
                        ldo.TalliedTrees = ldo.SumExpanFactor / TotalEF * TotalCount;
            }   //  end foreach loop
            //  Save 
            Global.BL.SaveLCD(lcdList);

            return;
        }   //  end Update3Ptally


        //  August 2016 -- need to replace tally for STR method when
        //  multiple species in a sample group were tallied by sample group and not
        //  by species.  Replace with sum of expansion factor
        private void UpdateSTRtally(string fileName, string currST, List<CountTreeDO> ctList, List<LCDDO> lcdList)
        {
            //  pull count records for STR stratum
            List<CountTreeDO> currentGroups = ctList.FindAll(
                delegate(CountTreeDO cg)
                {
                    return cg.SampleGroup.Stratum.Code == currST;
                });
            //  see if TreeDefaultValue_CN is zero indicating BySampleGroup
            int totalTDV = 0;
            foreach(CountTreeDO cg in currentGroups)
            {
                if(cg.TreeDefaultValue_CN != null)
                    totalTDV++;
                else totalTDV += 0;
            }   //  end foreach loop

            //  what was sampling method
            if (totalTDV == 0.0)
            {
                //  replace tally with sum of expansion factor
                foreach (LCDDO jg in lcdList.FindAll(lcd => lcd.Stratum == currST))
                {
                    //  find in original to update
                    int nthRow = lcdList.FindIndex(
                        delegate(LCDDO ll)
                        {
                            return ll.LCD_CN == jg.LCD_CN;
                        });
                        if (nthRow >= 0)
                           lcdList[nthRow].TalliedTrees = lcdList[nthRow].SumExpanFactor;
                            
                }   //  end foreach loop
            }   //  endif
                //  save
            Global.BL.SaveLCD(lcdList);

            return;
        }  //  end UpdateSTRtally



        private void UpdateExpansionFactors(IEnumerable<TreeCalculatedValuesDO> tcvList, long StrCN)
        {
            //  find tree calculated volume to see if expansion factor needs to be reset
            foreach (TreeDO t in Global.BL.getTreeStratum(StrCN))
            {
                //int nthRow = tcvList.FindIndex(
                //    delegate(TreeCalculatedValuesDO tcv)
                //    {
                //        return tcv.Tree_CN == t.Tree_CN && tcv.GrossBDFTPP == 0 && tcv.GrossCUFTPP == 0;
                //    });
                if (tcvList.Where(tcv => tcv.Tree_CN == t.Tree_CN && tcv.GrossBDFTPP == 0 && tcv.GrossCUFTPP == 0).Any())
                    t.ExpansionFactor = 0;
            }   //  end foreach loop
            Global.BL.SaveTreeCalculatedValues(tcvList);
            return;

        }   //  end UpdateExpansionFactors

        private void DefaultSecondaryProduct(string currRegion)
        {

            List<SampleGroupDO> sgList = Global.BL.getSampleGroups().ToList();

            foreach (SampleGroupDO sgd in sgList)
            {
                if (sgd.SecondaryProduct == "" || sgd.SecondaryProduct == " " ||
                    sgd.SecondaryProduct == "  " || sgd.SecondaryProduct == null)
                {
                    switch (currRegion)
                    {
                        case "06":
                        case "6":
                            sgd.SecondaryProduct = "08";
                            ErrorLogMethods.LoadError("SampleGroup", "W", "19", (long)sgd.SampleGroup_CN, "SecondaryProduct");
                            break;
                        case "05":
                        case "5":
                            sgd.SecondaryProduct = "20";
                            ErrorLogMethods.LoadError("SampleGroup", "W", "19", (long)sgd.SampleGroup_CN, "SecondaryProduct");
                            break;
                        default:
                            sgd.SecondaryProduct = "02";
                            ErrorLogMethods.LoadError("SampleGroup", "W", "19", (long)sgd.SampleGroup_CN, "SecondaryProduct");
                            break;
                    }   //  end switch
                }   //  endif
            }   //  end foreach loop
            Global.BL.SaveSampleGroups(sgList);
            return;
        }   //  end DefaultSecondaryProduct
    }
}
