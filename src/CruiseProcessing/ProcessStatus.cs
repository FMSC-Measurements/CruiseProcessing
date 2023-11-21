using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL;

namespace CruiseProcessing
{
    public partial class ProcessStatus : Form
    {
        public CPbusinessLayer DataLayer { get; }

        protected ProcessStatus()
        {
            InitializeComponent();
        }

        public ProcessStatus(CPbusinessLayer dataLayer)
            :this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer)); ;
        }

        private void on_GO(object sender, EventArgs e)
        {
           processingStatus.Text= "READY TO BEGIN?  Click GO.";
            Cursor.Current = Cursors.WaitCursor;
            //  perform edit checks -- 
           processingStatus.Text = "Edit checking the data.  Please wait.";
           processingStatus.Refresh();
            //  calls edit check routines
            /*            string outputFileName;
                        //  check for errors from FScruiser before running edit checks
                        //  generate an error report 
                        //  June 2013
                        List<ErrorLogDO> fscList = bslyr.getErrorMessages("E", "FScruiser");
                        if (fscList.Count > 0)
                        {
                            ErrorReport eRpt = new ErrorReport();
                            eRpt.fileName = fileName;
                            eRpt.bslyr.fileName = bslyr.fileName;
                            eRpt.bslyr.DAL = bslyr.DAL;
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
                        bslyr.DeleteErrorMessages();

                        EditChecks eChecks = new EditChecks();
                        eChecks.fileName = fileName;
                        eChecks.bslyr.fileName = bslyr.fileName;
                        eChecks.bslyr.DAL = bslyr.DAL;

                        int errors = eChecks.TableEditChecks();
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
                        List<ErrorLogDO> errList = bslyr.getErrorMessages("E", "CruiseProcessing");
                        if (errList.Count > 0)
                        {
                            ErrorReport er = new ErrorReport();
                            er.fileName = fileName;
                            er.bslyr.fileName = fileName;
                            er.bslyr.DAL = bslyr.DAL;
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
               moved to EditCheck routine*/
            //  Show editCheck message -- edit checks complete

            EditChecks eChecks = new EditChecks(DataLayer);

            int err = eChecks.CheckErrors();
            if (err < 0)
            {
                Close();
                return;
            }

            editCheck.Enabled = true;

            //  next show preparation of data
            processingStatus.Text = "Preparing data for processing.";
            processingStatus.Refresh();

            //  before making IDs, need to check for blank or null secondary products in sample groups
            //  if blank, default to 02 for every region but 6 where it will be 08 instead
            //  put a warning message in the error log table indicating the secondary product was set to a default
            //  June 2013
            List<SaleDO> saleList = new List<SaleDO>();
            saleList = DataLayer.DAL.From<SaleDO>().Read().ToList();
            string currRegion = saleList[0].Region;

            //string currRegion = bslyr.getRegion();

            DefaultSecondaryProduct(currRegion);

            CalculateTreeValues calcTreeVal = new CalculateTreeValues(DataLayer);
            CalculatedValues calcVal = new CalculatedValues(DataLayer);


            //  retrieve lists needed and sets up population IDs
            //   List<SampleGroupDO> sgList = bslyr.getSampleGroups();
            //   List<TreeDefaultValueDO> tdvList = bslyr.getTreeDefaults();
            //   List<CountTreeDO> ctList = bslyr.getCountTrees();
            //   List<PlotDO> pList = bslyr.getPlots();

            //   calcVal.ClearCalculatedTables();
            //   calcVal.MakePopulationIDs(sgList, tdvList);

            calcVal.CalcValues();

            //  now need some other tables to start summing values
            List<LCDDO> lcdList = DataLayer.getLCD();
            List<POPDO> popList = DataLayer.getPOP();
            List<PRODO> proList = DataLayer.getPRO();
            List<StratumDO> sList = DataLayer.getStratum();
            List<SampleGroupDO> sgList = DataLayer.getSampleGroups();
            List<TreeDefaultValueDO> tdvList = DataLayer.getTreeDefaults();
            List<CountTreeDO> ctList = DataLayer.getCountTrees();
            List<PlotDO> pList = DataLayer.getPlots();
            List<TreeDO> tList = DataLayer.getTrees();

            //  show preparation of data is complete
            prepareCheck.Enabled = true;
            //  now loop through strata and show status message updating for each stratum
            StringBuilder sb = new StringBuilder();
            foreach (StratumDO sdo in sList)
            {
                //  update status message for next stratum
                sb.Clear();
                sb.Append("Calculating stratum ");
                sb.Append(sdo.Code);
                processingStatus.Text = sb.ToString();
                processingStatus.Refresh();
                
                //  Sum counts and KPI for LCD table
                List<PlotDO> justPlots = PlotMethods.GetStrata(pList, sdo.Code);
                //  need cut and leave trees for this
                List<LCDDO> justCurrentLCD = LCDmethods.GetStratum(lcdList, sdo.Code);
                calcVal.SumTreeCountsLCD(sdo.Code, ctList, justPlots, justCurrentLCD, sdo.Method, lcdList);

                //  Sum counts and KPI for POP table
                List<POPDO> justCurrentPOP = POPmethods.GetStratumData(popList, sdo.Code, "");
                calcVal.SumTreeCountsPOP(sdo.Code, ctList, justPlots, justCurrentPOP, sdo.Method, popList);

                //  Sum counts and KPI for PRO table
                List<PRODO> justCurrentPRO = PROmethods.GetCutTrees(proList, sdo.Code, "", "", 0);
                calcVal.SumTreeCountsPRO(sdo.Code, ctList, justPlots, justCurrentPRO, sdo.Method, proList);

                //  Calculate expansion factor
                calcVal.CalcExpFac(sdo, justPlots, justCurrentPOP);

                //  Calculate volumes
                calcTreeVal.ProcessTrees(sdo.Code, sdo.Method,(long)sdo.Stratum_CN);

                //  Update 3P tally
                if (sdo.Method == "3P")
                {
                    List<LCDDO> LCDstratum = LCDmethods.GetStratumGroupedBy(sdo.Code, DataLayer);

                    Update3Ptally(ctList, justCurrentLCD, tList, LCDstratum);
                    //  Save 
                    DataLayer.SaveLCD(justCurrentLCD);
                }   //  endif method is 3P


                //  Update expansion factors for methods 3PPNT, F3P, and P3P
                if (sdo.Method == "3PPNT" || sdo.Method == "F3P" || sdo.Method == "P3P")
                {
                    List<TreeDO> justCurrentStratum = tList.FindAll(
                        delegate(TreeDO td)
                        {
                            return td.Stratum.Code == sdo.Code;
                        });
                    List<TreeCalculatedValuesDO> tcvList = DataLayer.getTreeCalculatedValues((int)sdo.Stratum_CN);
                    UpdateExpansionFactors(justCurrentStratum, tcvList);
                    //  Save update
                    DataLayer.SaveTrees(justCurrentStratum);
                }   //  endif on method

                //  Sum data for the LCD, POP and PRO table
                SumAll Sml = new SumAll(DataLayer);
                Sml.SumAllValues(sdo.Code, sdo.Method, (int)sdo.Stratum_CN, sList, pList, justCurrentLCD,
                                    justCurrentPOP, justCurrentPRO);

                //  Update STR tally after expansion factors are summed
                if (sdo.Method == "STR")
                {

                    UpdateSTRtally(sdo.Code, justCurrentLCD, ctList, lcdList);
                    //  save
                    DataLayer.SaveLCD(lcdList);
                }   //  endif method is STR

            }   //  end foreach stratum

            //  show volume calculation is finished
            volumeCheck.Enabled = true;
            processingStatus.Text = "Processing is DONE";
            processingStatus.Refresh();
            System.Threading.Thread.Sleep(5000);
            Cursor.Current = this.Cursor;

            Close();
            return;
        }   //  end on_GO


        private void Update3Ptally(List<CountTreeDO> ctList, List<LCDDO> justCurrentLCD,
                                    List<TreeDO> tList, List<LCDDO> LCDstratum)
        {
            double TotalEF = 0;
            double TotalMeas = 0;
            double TotalCount = 0;

            //  Need LCD for 3p stratum ordered by sample group, species (and STM)
            //List<LCDDO> LCDstratum = LCDmethods.GetStratumGroupedBy(fileName, currST, bslyr);
            foreach (LCDDO l in LCDstratum)
            {
                //  find each group in just current LCD
                List<LCDDO> currentGroup = justCurrentLCD.FindAll(
                    delegate(LCDDO ld)
                    {
                        return l.SampleGroup == ld.SampleGroup && l.Species == ld.Species && l.STM == ld.STM;
                    });
                TotalEF = currentGroup.Sum(ldo => ldo.SumExpanFactor);

                if(l.STM == "N")
                {
                    //  Find counts
                    List<CountTreeDO> justCounts = ctList.FindAll(
                        delegate(CountTreeDO ctdo)
                        {
                            if (ctdo.TreeDefaultValue != null)
                                return ctdo.SampleGroup.Code == l.SampleGroup &&
                                     ctdo.TreeDefaultValue.Species == l.Species;
                            else return false;
                        });
                    if (justCounts.Count > 0)
                    {
                        TotalCount = justCounts.Sum(ct => ct.TreeCount);
                        TotalMeas = currentGroup.Sum(lcddo => lcddo.MeasuredTrees);
                        TotalCount += TotalMeas;
                    }   //  endif justCounts is empty
                }
                else if(l.STM == "Y")  TotalCount = currentGroup.Sum(lcdo => lcdo.MeasuredTrees);

                //  Add any insurance trees to count
                List<TreeDO> insurTrees = tList.FindAll(
                    delegate(TreeDO td)
                    {
                        return td.Stratum.Code == l.Stratum && td.SampleGroup.Code == l.SampleGroup && 
                                td.Species == l.Species && td.STM == l.STM;     
                    });
                if(l.STM == "N")
                    TotalCount += insurTrees.Sum(t => t.TreeCount);

                //  Recalc tallied trees for this group
                foreach (LCDDO ldo in currentGroup)
                    if (TotalEF > 0)
                        ldo.TalliedTrees = ldo.SumExpanFactor / TotalEF * TotalCount;
            }   //  end foreach loop

            return;
        }   //  end Update3Ptally


        //  August 2016 -- need to replace tally for STR method when
        //  multiple species in a sample group were tallied by sample group and not
        //  by species.  Replace with sum of expansion factor
        private void UpdateSTRtally(string currST, List<LCDDO> justCurrentLCD, List<CountTreeDO> ctList,
                                    List<LCDDO> lcdList)
        {
            //Possibly update this from Sample group instead of strata.  See templeton.
            //  pull count records for STR stratum
            List<CountTreeDO> currentGroups = ctList.FindAll(
                delegate(CountTreeDO cg)
                {
                    return cg.SampleGroup.Stratum.Code == currST;
                });


            string sampleGroupCN = "";
            //  see if TreeDefaultValue_CN is zero indicating BySampleGroup
            int totalTDV = 0;
            foreach(CountTreeDO cg in currentGroups)
            {
                //if treeDefaultValue_cn is not null tally by species
                //else tally by sample group.   
                //if (cg.TreeDefaultValue_CN != null)
                //{
                //regardless of sample group tally or species tally correct the tally.
                    string strataCode = "";

                    string sampleGroupCode = "";

                    if (cg.SampleGroup != null)
                    {
                        sampleGroupCode = cg.SampleGroup.Code;
                        if (cg.SampleGroup.Stratum != null)
                        {
                            strataCode = cg.SampleGroup.Stratum.Code;
                        }//end if
                    }//end if


                    //  replace tally with sum of expansion factor
                    foreach (LCDDO jg in justCurrentLCD)
                    {
                        if (jg.SampleGroup == sampleGroupCode && jg.Stratum == strataCode)
                        {
                            //  find in original to update
                            int nthRow = lcdList.FindIndex(
                            delegate (LCDDO ll)
                            {
                                return ll.LCD_CN == jg.LCD_CN;
                            });

                            if (nthRow >= 0)
                            {
                                lcdList[nthRow].TalliedTrees = lcdList[nthRow].SumExpanFactor;
                            }//end if
                        }//end if
                    }   //  end foreach loop

                    totalTDV += 0;

                    
                //}//end if
                //else
                //{
                //    //Don't apply if count tree has a CN (meaning its selected coby species)
                //    //totalTDV++;
                //}//end else

            }//  end foreach loop

            ////  what was sampling method
            //if (totalTDV == 0.0)
            //{
              
            //}   //  endif

            return;
        }  //  end UpdateSTRtally



        private void UpdateExpansionFactors(List<TreeDO> justCurrST, List<TreeCalculatedValuesDO> tcvList)
        {
            //  find tree calculated volume to see if expansion factor needs to be reset
            foreach (TreeDO t in justCurrST)
            {
                int nthRow = tcvList.FindIndex(
                    delegate(TreeCalculatedValuesDO tcv)
                    {
                        return tcv.Tree_CN == t.Tree_CN && tcv.GrossBDFTPP == 0 && tcv.GrossCUFTPP == 0;
                    });
                if (nthRow >= 0)
                    t.ExpansionFactor = 0;
            }   //  end foreach loop

            return;
        }   //  end UpdateExpansionFactors

        private void DefaultSecondaryProduct(string currRegion)
        {
            ErrorLogMethods elm = new ErrorLogMethods(DataLayer);

            List<SampleGroupDO> sgList = DataLayer.getSampleGroups();

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
                            elm.LoadError("SampleGroup", "W", "19", (long)sgd.SampleGroup_CN, "SecondaryProduct");
                            break;
                        case "05":
                        case "5":
                            sgd.SecondaryProduct = "20";
                            elm.LoadError("SampleGroup", "W", "19", (long)sgd.SampleGroup_CN, "SecondaryProduct");
                            break;
                        default:
                            sgd.SecondaryProduct = "02";
                            elm.LoadError("SampleGroup", "W", "19", (long)sgd.SampleGroup_CN, "SecondaryProduct");
                            break;
                    }   //  end switch
                }   //  endif
            }   //  end foreach loop
            DataLayer.SaveSampleGroups(sgList);
            return;
        }   //  end DefaultSecondaryProduct
    }
}
