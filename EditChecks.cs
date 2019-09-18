using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public class EditChecks
    {
        #region
            public string fileName;
            private int errorValue = -99;
            List<ErrorLogDO> errList = new List<ErrorLogDO>();
        #endregion

            public int TableEditChecks(IEnumerable<SaleDO> saleList, IEnumerable<StratumDO> strList,
                IEnumerable<CountTreeDO> cntList, IEnumerable<TreeDO> tList, IEnumerable<SampleGroupDO> sgList)
            {
                ErrorLogMethods.fileName = fileName;
                string currentRegion = Global.BL.getRegion();
                string isVLL = "";
                //string isVLL = Global.BL.getVLL();

                //  edit checks for each table
                //  sale table
                //  empty or more than one record?
                errorValue = SaleMethods.MoreThanOne(saleList);
                if (errorValue == 0)
                    ErrorLogMethods.LoadError("Sale", "E", "25", errorValue, "NoName");       //  error 25 -- table cannot be empty
                else if (errorValue > 1)
                    ErrorLogMethods.LoadError("Sale", "E", "28", errorValue, "SaleNumber");       //  error 28 -- more than one sale record not allowed
                if (errorValue == 1)
                {
                    //  and blank sale number
                    errorValue = SaleMethods.BlankSaleNum(saleList);
                    if (errorValue != -1) ErrorLogMethods.LoadError("Sale", "E", " 8Sale Number", errorValue,"SaleNumber");
                }   //  endif
                //  end sale table edit checks

                // *******************************************************************
                //  stratum table
//                List<StratumDO> strList = Global.BL.getStratum();
                errorValue = StratumMethods.IsEmpty(strList);
                if (errorValue != 0) 
                    ErrorLogMethods.LoadError("Stratum", "E", errorValue.ToString(), errorValue,"NoName");
                else if (errorValue == 0)    //  means there are records to check
                {
                    foreach (StratumDO sdo in strList)
                    {
                        //  check for valid fixed plot size or BAF for each stratum
                        double BAForFPS = StratumMethods.CheckMethod(strList, sdo.Code);
                        if ((sdo.Method == "PNT" || sdo.Method == "P3P" || sdo.Method == "PCM" ||
                             sdo.Method == "PCMTRE" || sdo.Method == "3PPNT") && BAForFPS == 0)
                            ErrorLogMethods.LoadError("Stratum", "E", "22", (long)sdo.Stratum_CN,"BasalAreaFactor");
                        else if ((sdo.Method == "FIX" || sdo.Method == "F3P" || sdo.Method == "FIXCNT" ||
                                  sdo.Method == "FCM") && BAForFPS == 0)
                            ErrorLogMethods.LoadError("Stratum", "E", "23", (long)sdo.Stratum_CN,"FixedPlotSize");

                        //  check for acres on area based methods
                        double currAcres = Utilities.AcresLookup((long)sdo.Stratum_CN, sdo.Code);
                        if ((sdo.Method == "PNT" || sdo.Method == "FIX" || sdo.Method == "P3P" ||
                            sdo.Method == "F3P" || sdo.Method == "PCM" || sdo.Method == "3PPNT" ||
                            sdo.Method == "FCM" || sdo.Method == "PCMTRE") && currAcres == 0)
                            ErrorLogMethods.LoadError("Stratum", "E", "24", (long)sdo.Stratum_CN,"NoName");
                        else if ((sdo.Method == "100" || sdo.Method == "3P" ||
                            sdo.Method == "S3P" || sdo.Method == "STR") && currAcres == 0)
                            ErrorLogMethods.LoadError("Stratum", "W", "Stratum has no acres", (long)sdo.Stratum_CN,"NoName");

                        //  August 2017 -- added check for valid yield component code
                        if (sdo.YieldComponent != "CL" && sdo.YieldComponent != "CD" &&
                           sdo.YieldComponent != "ND" && sdo.YieldComponent != "NL" &&
                           sdo.YieldComponent != "" && sdo.YieldComponent != " " &&
                           sdo.YieldComponent != null)
                            ErrorLogMethods.LoadError("Stratum", "W", "Yield Component has invalid code", (long)sdo.Stratum_CN, "YieldComponent");

                    }   //  end foreach loop
                }   //  endif
                //  end stratum table edit checks

                //  ************************************************************************
                // cutting unit table
                //List<CuttingUnitDO> cuList = Global.BL.getCuttingUnits();
                errorValue = CuttingUnitMethods.IsEmpty(Global.BL.getCuttingUnits());
                if (errorValue != 0) 
                    ErrorLogMethods.LoadError("Cutting Unit", "E", errorValue.ToString(), 0,"NoName");
                //  end cutting unit edit checks

                //  **************************************************************************
                //  count table
                //List<CountTreeDO> cntList = Global.BL.getCountTrees();
                foreach (StratumDO sl in strList)
                {
                    errorValue = CountTreeMethods.check3Pcounts(cntList, sl);
                    if (errorValue != 0)
                        ErrorLogMethods.LoadError("CountTree", "E", "Cannot tally by sample group for 3P strata.", (long)sl.Stratum_CN, "TreeDefaultValue");
                }   //  end foreach on stratum

                //  **************************************************************************
                //  tree table
                //List<TreeDO> tList = Global.BL.getTrees();
                errorValue = TreeListMethods.IsEmpty(tList);
                if (errorValue != 0)
                    ErrorLogMethods.LoadError("Tree", "E", errorValue.ToString(), 0, "NoName");
                else if (errorValue == 0)       //  means tree table has records
                {
                    //  make sure single stratum is NOT FIXCNT which cannot have measured trees anyway.
                    //if(strList.Count == 1 && strList[0].Method != "FIXCNT")
                    //  doesn't matter if it's a single stratum or any stratum that's FIXCNT
                    //  that method still has no measured trees so need to skip these checks.
                    if(strList.First().Method != "FIXCNT")
                    {
                        var tMeas = tList.Select(tree => tree.CountOrMeasure = "M");
                        //tList = Global.BL.JustMeasuredTrees();
                        //  March 2016 -- if the entire cruisde has no measured trees, that uis a critical erro
                        //  and should stop the program.  Since no report can be generated, a message box appears to warn the user
                        //  of the condition.
                        if (tMeas.Count() == 0)
                        {
                            MessageBox.Show("NO MEASURED TREES IN THIS CRUISE.\nCannot continue and cannot produce any reports.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return -1;
                        }   //endif
                        //  general checks include  checking for secondary top DIB greater than primary
                        //  recoverable percent greater than seen defect primary
                        //  missing species, product or uom when tree count is greater than zero or DBH is greater than zero
                        //  and check for upper stem diameter greater than DBH
                        errorValue = TreeListMethods.GeneralChecks(tList,currentRegion);
                    }   //  endif single stratum
                }   //  endif
                //  end tree table edit checks

                //  *************************************************************************
                //  log table
                List<LogDO> logList = Global.BL.getLogs().ToList();
                if (logList.Count > 0)
                {
                    errorValue = LogMethods.CheckNumberLogs(logList);
                    errorValue = LogMethods.CheckFBS(logList);
                    if (isVLL == "true")
                        errorValue = LogMethods.CheckVLL(logList);
                    errorValue = LogMethods.CheckDefect(logList);
                    if(currentRegion != "05")
                        errorValue = LogMethods.CheckLogGrade(logList);
                }   //  endif records in log table to check
                //  end log table edit checks

                //  *************************************************************************
                //  volume equation table
                List<VolumeEquationDO> volList = Global.BL.getVolumeEquations().ToList();
                errorValue = VolumeEqMethods.IsEmpty(volList);
            //  pull region
                string currRegion = saleList.First().Region;
                //string currRegion =  Global.BL.getRegion();
                if (errorValue != 0)
                    ErrorLogMethods.LoadError("VolumeEquation", "E", errorValue.ToString(), 0,"NoName");
                else if (errorValue == 0)    //  means the table is not empty and checks can proceed
                {
                //  Match unique species/product to volume equation   \
                    errorValue = VolumeEqMethods.MatchSpeciesProduct(volList, tList);
                    errorValue = VolumeEqMethods.GeneralEquationChecks(volList);
                    //  if VLL, check equations for Behrs
                    if (isVLL == "true")
                        errorValue = VolumeEqMethods.FindBehrs(volList);
                }   //  endif
                //  end volume equation edit checks

                //  *************************************************************************
                //  value equations
                // Check for valid equations
                IEnumerable<ValueEquationDO> valList = Global.BL.getValueEquations();
                if(valList.Any())       //  there are records to check
                    errorValue = ValueEqMethods.CheckEquations(valList, currentRegion);
                //  end value equation edit checks

                //  *************************************************************************
                //  quality adjustment equations
                IEnumerable<QualityAdjEquationDO> qaeList = Global.BL.getQualAdjEquations();
                if(qaeList.Any())       //  there are records to check
                    errorValue = QualityAdjMethods.CheckEquations(qaeList);
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
                List<ReportsDO> selectedReports = Global.BL.GetSelectedReports();
                if (selectedReports.Count == 0)
                {
                    ErrorLogMethods.LoadError("Reports", "E", "26", 0,"NoName");
                    errorValue++;
                }   //  endif no reports
 */
                //  ************************************************************************
                //  sample group table
                //List<SampleGroupDO> sampGroups = Global.BL.getSampleGroups();
                errorValue = SampleGroupMethods.CheckAllUOMandCutLeave(strList, sgList,fileName);
                //  ************************************************************************
                errList = ErrorLogMethods.errList;
                if (errList.Count > 0)
                {

                Global.BL.SaveErrorMessages(errList);
                }   //  endif
                return errorValue;
            }   //  end TableEditChecks

            public int MethodChecks()
            {
                IEnumerable<TreeDO> tList = Global.BL.getTrees();
                errorValue = ErrorLogMethods.CheckCruiseMethods(Global.BL.getStratum(), tList);
                errList = ErrorLogMethods.errList;
                if (errList.Count > 0) Global.BL.SaveErrorMessages(errList);
                return errorValue;
            }   //  end MethodChecks

    }   //  end EditChecks
}
