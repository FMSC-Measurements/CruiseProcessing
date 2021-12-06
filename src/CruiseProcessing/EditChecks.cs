using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL;

namespace CruiseProcessing
{
    public class EditChecks
    {
        #region
            public string fileName;
            private int errorValue = -99;
            public DAL DAL { get; set; }
            List<ErrorLogDO> errList = new List<ErrorLogDO>();
            public ErrorLogMethods elm = new ErrorLogMethods();
            public LogMethods Lms = new LogMethods();
            public TreeListMethods Tlm = new TreeListMethods();
            public VolumeEqMethods Veq = new VolumeEqMethods();
        #endregion
        public int CheckErrors()
        {
            string outputFileName;
            CPbusinessLayer bslyr = new CPbusinessLayer();
            bslyr.DAL = DAL;
            bslyr.fileName = fileName;
        //  check for errors from FScruiser before running edit checks
        //  generate an error report 
        //  June 2013
        List<ErrorLogDO> fscList = bslyr.getErrorMessages("E", "FScruiser");
            if (fscList.Count > 0)
            {
                ErrorReport eRpt = new ErrorReport();
                eRpt.fileName = fileName;
                eRpt.bslyr.fileName = bslyr.fileName;
                eRpt.bslyr.DAL = DAL;
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


            int errors = TableEditChecks();
            if (errors == -1)
            {
                //  no measured trees detected in the cruise.  critical errpor stops the program.
                return(-1);
            }   //  endif
            errors = MethodChecks();
            if (errors == -1)
            {
                //  empty stratum detected and user wants to quit
                return(-1);
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
            return (0);
        }

        public int TableEditChecks()
            {
                CPbusinessLayer bslyr = new CPbusinessLayer();
                bslyr.DAL = DAL;
                bslyr.fileName = fileName;
                elm.fileName = fileName;
                string currentRegion = bslyr.getRegion();
                string isVLL = "";
                //string isVLL = bslyr.getVLL();

                //  edit checks for each table
                //  sale table
                List<SaleDO> saleList = bslyr.getSale();
                //  empty or more than one record?
                errorValue = SaleMethods.MoreThanOne(saleList);
                if (errorValue == 0)
                    elm.LoadError("Sale", "E", "25", errorValue, "NoName");       //  error 25 -- table cannot be empty
                else if (errorValue > 1)
                    elm.LoadError("Sale", "E", "28", errorValue, "SaleNumber");       //  error 28 -- more than one sale record not allowed
                if (errorValue == 1)
                {
                    //  and blank sale number
                    errorValue = SaleMethods.BlankSaleNum(saleList);
                    if (errorValue != -1) elm.LoadError("Sale", "E", " 8Sale Number", errorValue,"SaleNumber");
                }   //  endif
                //  end sale table edit checks

                // *******************************************************************
                //  stratum table
                List<StratumDO> strList = bslyr.getStratum();
                errorValue = StratumMethods.IsEmpty(strList);
                if (errorValue != 0) 
                    elm.LoadError("Stratum", "E", errorValue.ToString(), errorValue,"NoName");
                else if (errorValue == 0)    //  means there are records to check
                {
                    foreach (StratumDO sdo in strList)
                    {
                        //  check for valid fixed plot size or BAF for each stratum
                        double BAForFPS = StratumMethods.CheckMethod(strList, sdo.Code);
                        if ((sdo.Method == "PNT" || sdo.Method == "P3P" || sdo.Method == "PCM" ||
                             sdo.Method == "PCMTRE" || sdo.Method == "3PPNT") && BAForFPS == 0)
                            elm.LoadError("Stratum", "E", "22", (long)sdo.Stratum_CN,"BasalAreaFactor");
                        else if ((sdo.Method == "FIX" || sdo.Method == "F3P" || sdo.Method == "FIXCNT" ||
                                  sdo.Method == "FCM") && BAForFPS == 0)
                            elm.LoadError("Stratum", "E", "23", (long)sdo.Stratum_CN,"FixedPlotSize");

                        //  check for acres on area based methods
                        double currAcres = Utilities.AcresLookup((long)sdo.Stratum_CN, bslyr, sdo.Code);
                        if ((sdo.Method == "PNT" || sdo.Method == "FIX" || sdo.Method == "P3P" ||
                            sdo.Method == "F3P" || sdo.Method == "PCM" || sdo.Method == "3PPNT" ||
                            sdo.Method == "FCM" || sdo.Method == "PCMTRE") && currAcres == 0)
                            elm.LoadError("Stratum", "E", "24", (long)sdo.Stratum_CN,"NoName");
                        else if ((sdo.Method == "100" || sdo.Method == "3P" ||
                            sdo.Method == "S3P" || sdo.Method == "STR") && currAcres == 0)
                            elm.LoadError("Stratum", "W", "Stratum has no acres", (long)sdo.Stratum_CN,"NoName");

                        //  August 2017 -- added check for valid yield component code
                        if (sdo.YieldComponent != "CL" && sdo.YieldComponent != "CD" &&
                           sdo.YieldComponent != "ND" && sdo.YieldComponent != "NL" &&
                           sdo.YieldComponent != "" && sdo.YieldComponent != " " &&
                           sdo.YieldComponent != null)
                            elm.LoadError("Stratum", "W", "Yield Component has invalid code", (long)sdo.Stratum_CN, "YieldComponent");

                    }   //  end foreach loop
                }   //  endif
                //  end stratum table edit checks

                //  ************************************************************************
                // cutting unit table
                List<CuttingUnitDO> cuList = bslyr.getCuttingUnits();
                errorValue = CuttingUnitMethods.IsEmpty(cuList);
                if (errorValue != 0) 
                    elm.LoadError("Cutting Unit", "E", errorValue.ToString(), 0,"NoName");
                //  end cutting unit edit checks

                //  **************************************************************************
                //  count table
                List<CountTreeDO> cntList = bslyr.getCountTrees();
                foreach (StratumDO sl in strList)
                {
                    errorValue = CountTreeMethods.check3Pcounts(cntList, bslyr, sl);
                    if (errorValue != 0)
                        elm.LoadError("CountTree", "E", "Cannot tally by sample group for 3P strata.", (long)sl.Stratum_CN, "TreeDefaultValue");
                }   //  end foreach on stratum

                //  **************************************************************************
                //  tree table
                List<TreeDO> tList = bslyr.getTrees();
                errorValue = Tlm.IsEmpty(tList);
                if (errorValue != 0)
                    elm.LoadError("Tree", "E", errorValue.ToString(), 0, "NoName");
                else if (errorValue == 0)       //  means tree table has records
                {
                    //  make sure single stratum is NOT FIXCNT which cannot have measured trees anyway.
                    //if(strList.Count == 1 && strList[0].Method != "FIXCNT")
                    //  doesn't matter if it's a single stratum or any stratum that's FIXCNT
                    //  that method still has no measured trees so need to skip these checks.
                    if(strList[0].Method != "FIXCNT")
                    {
                        tList = bslyr.JustMeasuredTrees();
                        //  March 2016 -- if the entire cruisde has no measured trees, that uis a critical erro
                        //  and should stop the program.  Since no report can be generated, a message box appears to warn the user
                        //  of the condition.
                        if (tList.Count == 0)
                        {
                            MessageBox.Show("NO MEASURED TREES IN THIS CRUISE.\nCannot continue and cannot produce any reports.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return -1;
                        }   //endif
                        //  general checks include  checking for secondary top DIB greater than primary
                        //  recoverable percent greater than seen defect primary
                        //  missing species, product or uom when tree count is greater than zero or DBH is greater than zero
                        //  and check for upper stem diameter greater than DBH
                        errorValue = Tlm.GeneralChecks(tList,currentRegion);
                    }   //  endif single stratum
                }   //  endif
                //  end tree table edit checks

                //  *************************************************************************
                //  log table
                List<LogDO> logList = bslyr.getLogs();
                if (logList.Count > 0)
                {
                    errorValue = Lms.CheckNumberLogs(logList);
                    errorValue = Lms.CheckFBS(logList);
                    if (isVLL == "true")
                        errorValue = Lms.CheckVLL(logList);
                    errorValue = Lms.CheckDefect(logList);
                    if(currentRegion != "05")
                        errorValue = Lms.CheckLogGrade(logList);
                }   //  endif records in log table to check
                //  end log table edit checks

                //  *************************************************************************
                //  volume equation table
                List<VolumeEquationDO> volList = bslyr.getVolumeEquations();
                Veq.DAL = DAL;
                errorValue = Veq.IsEmpty(volList);
                //  pull region
                string currRegion = bslyr.getRegion();
                if (errorValue != 0)
                    elm.LoadError("VolumeEquation", "E", errorValue.ToString(), 0,"NoName");
                else if (errorValue == 0)    //  means the table is not empty and checks can proceed
                {
                    //  Match unique species/product to volume equation                    
                    errorValue = Veq.MatchSpeciesProduct(volList, tList);
                    errorValue = Veq.GeneralEquationChecks(volList);
                    //  if VLL, check equations for Behrs
                    if (isVLL == "true")
                        errorValue = Veq.FindBehrs(volList);
                }   //  endif
                    //  end volume equation edit checks

            //  *************************************************************************
            //  value equations
            // Check for valid equations
            ValueEqMethods Vem = new ValueEqMethods();
                List<ValueEquationDO> valList = bslyr.getValueEquations();
                if(valList.Count > 0)       //  there are records to check
                    errorValue = Vem.CheckEquations(valList, currentRegion);
            //  end value equation edit checks

            //  *************************************************************************
            //  quality adjustment equations
                QualityAdjMethods Qam = new QualityAdjMethods();
                List<QualityAdjEquationDO> qaeList = bslyr.getQualAdjEquations();
                if(qaeList.Count > 0)       //  there are records to check
                    errorValue = Qam.CheckEquations(qaeList);
            //  end quality adjustment equation edit checks

            //  ************************************************************************
            //  June 2014 -- apparently the error log table is not meant to capture
            //  errors on the basis of an entire table.  Only on individual records.
            //  so if no reports have been selected, this check needs to move to
            //  the Output section when the user clicks GO.  If the reports list
            //  comes up empty then display a message box telling them to go back
            //  and enter reports.
            //  no reports selected
            /*
                            List<ReportsDO> selectedReports = bslyr.GetSelectedReports();
                            if (selectedReports.Count == 0)
                            {
                                elm.LoadError("Reports", "E", "26", 0,"NoName");
                                errorValue++;
                            }   //  endif no reports
             */
            //  ************************************************************************
            //  sample group table
                SampleGroupMethods Sgm = new SampleGroupMethods();
                List<SampleGroupDO> sampGroups = bslyr.getSampleGroups();
                errorValue = Sgm.CheckAllUOMandCutLeave(strList, sampGroups,fileName);
                //  ************************************************************************
                errList = elm.errList;
                if (errList.Count > 0)
                {
                    bslyr.fileName = fileName;
                    bslyr.SaveErrorMessages(errList);
                }   //  endif
                return errorValue;
            }   //  end TableEditChecks

            public int MethodChecks()
            {
                CPbusinessLayer bslyr = new CPbusinessLayer();
                bslyr.DAL = DAL;
                bslyr.fileName = fileName;

                List<StratumDO> strList = bslyr.getStratum();
                List<TreeDO> tList = bslyr.getTrees();
                elm.bslyr.fileName = fileName;
                elm.bslyr.DAL = bslyr.DAL;
                errorValue = elm.CheckCruiseMethods(strList, tList);
                errList = elm.errList;
                bslyr.fileName = fileName;
                if (errList.Count > 0) bslyr.SaveErrorMessages(errList);
                return errorValue;
            }   //  end MethodChecks

    }   //  end EditChecks
}
