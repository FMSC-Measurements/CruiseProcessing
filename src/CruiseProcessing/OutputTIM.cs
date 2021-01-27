using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    class OutputTIM : CreateTextFile
    {
        #region
        public string cruiseNum;
        private string regionNumber;
        private string[] productTypes = new string[3];
        private List<SumFields> sumList = new List<SumFields>();
        #endregion

        public void CreateSUMfile()
        {
            //  pull tables needed initially
            List<SaleDO> sList = bslyr.getSale();
            List<CuttingUnitDO> cuList = bslyr.getCuttingUnits();
            List<StratumDO> strList = bslyr.getStratum();
            List<POPDO> popList = bslyr.getPOP();
            List<PRODO> proList = bslyr.getPRO();
            //  need to pull constant values from sale list
            regionNumber = sList[0].Region;
            //  already have "cruise" number through CreateTextFile (saleName)

            //  setup filename for SUM file
            string SUMout = System.IO.Path.GetDirectoryName(fileName);
            SUMout += "\\";
            SUMout += cruiseNum;     //  Request for sale (cruise) number to be used as SUM filename and the dump file
            SUMout += ".sum";

            //  check for more than two UOM and issue warning that SUM file will not load into TIM
            List<POPDO> UOMgroups = bslyr.GetUOMfromPOP();
            if (UOMgroups.Count > 2)
                Utilities.LogError("SUM file", 0, "W", "20",fileName);

            //  FIXCNT and UOM 04 will not load into TIM -- loop by UOM groups
            foreach (POPDO pop in UOMgroups)
            {
                if (pop.UOM != "04")
                {
                    //  find total error for the cruise
                    double saleErr = CalculateNetErr(pop.UOM, popList, strList);
                    
                    //  Load the various records
                    using (StreamWriter strSumOut = new StreamWriter(SUMout))
                    {
                        //  load 1A records
                        sumList.Clear();
                        Load1A(strSumOut, saleErr, sList);
                        //  Load 2A records
                        sumList.Clear();
                        Load2A(strSumOut, cuList);
                        //  Load 3A records
                        sumList.Clear();
                        Load3A(strSumOut, strList);
                        //  Load 1V records
                        sumList.Clear();
                        Load1V(strSumOut, pop, popList, proList);
                        //  Load 2V records
                        sumList.Clear();
                        for (int j = 0; j < 3; j++)
                            productTypes[j] = "";
                        Load2V(strSumOut, cuList, proList, strList);
                        //  Load 3V records
                        sumList.Clear();
                        for (int j = 0; j < 3; j++)
                            productTypes[j] = "";
                        Load3V(strSumOut, strList, proList);

                        strSumOut.Close();
                    }   //  end using

                }   //  endif uom is not 04
            }   //  end foreach loop
            
            /*  until I can figure out how to refresh the TextFileOutput window, don't do this
            StringBuilder message = new StringBuilder();
            message.Append("The TIM report was selected and \n");
            message.Append("the SUM file is complete and can be found at:\n");
            message.Append(SUMout);
            MessageBox.Show(message.ToString(), "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
            */
            //  the dump file is no longer going to be comma-delimited -- I think
            //  going to try renaming the SQLite database with the addition of ".dmp"
            //  test load was -- successful?  April 2013 --  NO
            //  use crz.dmp and salename like the sum file
            string dumpFile = System.IO.Path.GetDirectoryName(fileName);
            dumpFile += "\\";
            dumpFile += cruiseNum + ".crz.dmp";
            File.Copy(fileName, dumpFile, true);

            return;
        }   //  end CreateSUMfile


        private void Load1A(StreamWriter strSumOut, double saleErr, List<SaleDO> sList)
        {
            //  Never more than one sale record so just load and print
            SumFields sf = new SumFields();
            sf.recType = "1A";
            sf.saleNum = sList[0].SaleNumber;
            sf.currRG = sList[0].Region;
            sf.alpha1 = sList[0].Forest;
            sf.alpha2 = sList[0].District;

            //  calendar year is system year
            sf.alpha3 = DateTime.Today.Year.ToString();

            //  if sale purpose is blank, default to timber sale (TS)
            if (sList[0].Purpose == "" || sList[0].Purpose == " " || sList[0].Purpose == null)
                sf.alpha4 = "TS  ";
            else if (sList[0].Purpose.Length <= 4)
                sf.alpha4 = sList[0].Purpose.PadRight(4, ' ');
            else
            {
                //  seems FScruiser is not putting in the two character code anymore
                //  need to fix that as follows -- August 2014
                switch (sList[0].Purpose)
                {
                    case "Timber Sale":
                        sf.alpha4 = "TS  ";
                        break;
                    case "Check Cruise":
                        sf.alpha4 = "CC  ";
                        break;
                    case "Right of Way":
                        sf.alpha4 = "RO  ";
                        break;
                    case "Other":
                        sf.alpha4 = "OT  ";
                        break;
                }   //  end switch on purpose

            }   //  endif


            //  load sale err
            sf.alpha5 = Utilities.FormatField(saleErr, "{0,5:F1}").ToString().PadLeft(5, ' ');

            //  version number is just MMDDYY
            sf.alpha6 = currentVersion.Substring(0, 2);
            sf.alpha6 += currentVersion.Substring(3, 2);
            sf.alpha6 += currentVersion.Substring(8, 2);
            sumList.Add(sf);

            WriteCurrentRecords(strSumOut, "1A");
            return;
        }   //  end Load1A

        private void Load2A(StreamWriter strSumOut, List<CuttingUnitDO> cutList)
        {
            foreach (CuttingUnitDO cu in cutList)
            {
                SumFields sf = new SumFields();
                sf.recType = "2A";
                sf.saleNum = cruiseNum;
                sf.currCU = cu.Code;
                sf.alpha1 = Utilities.FormatField(cu.Area,"{0,8:F2}").ToString();
                if (cu.Description == null)
                    sf.alpha2 = "";
                else sf.alpha2 = cu.Description;
                if (cu.LoggingMethod == "" || cu.LoggingMethod == " " || cu.LoggingMethod == null)
                    sf.alpha3 = "~  ";
                else sf.alpha3 = cu.LoggingMethod.TrimEnd();
                if(cu.PaymentUnit == "" || cu.PaymentUnit == " "|| cu.PaymentUnit == null)
                    sf.alpha4 = "~  ";
                else sf.alpha4 = cu.PaymentUnit;
                sf.currRG = regionNumber;
                sumList.Add(sf);
            }   //  end foreach loop

            WriteCurrentRecords(strSumOut, "2A");
            return;
        }   //  end Load2A


        private void Load3A(StreamWriter strSumOut, List<StratumDO> strList)
        {
            foreach(StratumDO sd in strList)
            {
                //  don't load FIXCNT as TIM doesn't accept it yet
                if(sd.Method != "FIXCNT")
                {
                    SumFields sf = new SumFields();
                    sf.recType = "3A";
                    sf.saleNum = cruiseNum;
                    sf.currST = sd.Code;
                    sf.currRG = regionNumber;
                    sf.alpha1 = sd.Method;
                    sf.alpha2 = sd.BasalAreaFactor.ToString();
                    sf.alpha3 = sd.FixedPlotSize.ToString();
                    if (sd.Description == null)
                        sf.alpha4 = "";
                    else sf.alpha4 = sd.Description;
                    sf.alpha5 = sd.Month.ToString();
                    sf.alpha6 = sd.Year.ToString();
                    sumList.Add(sf);
                }   //  endif on method
            }   //  end foreach loop

            WriteCurrentRecords(strSumOut, "3A");
            return;
        }   //  end Load3A


        private void Load1V(StreamWriter strSumOut, POPDO pop, List<POPDO> popList, List<PRODO> proList)
        {
            List<LCDDO> lcdList = bslyr.getLCD();
            //  need just cut trees from pop list
            List<POPDO> justCutPops = POPmethods.GetCutTrees(popList);
            foreach (POPDO jcp in justCutPops)
            {
                //  Determine if there is data for this group by summing up expansion factor from LCD
                List<LCDDO> sampGrpsLCD = LCDmethods.GetCutOnlyMultipleValue(lcdList, jcp.Stratum, jcp.SampleGroup, 
                                                                                      "", "C", jcp.UOM, jcp.STM);
                //  reset productTypes to prevent extraneous record
                for (int k = 0; k < 3; k++)
                    productTypes[k] = "";
                double tempSum = sampGrpsLCD.Sum(ldo => ldo.SumExpanFactor);
                if (tempSum > 0)
                {
                    //  Determine products needed
                    productTypes[0] = "PP";
                    if (sampGrpsLCD.Sum(ldo => ldo.SumGCUFTtop) > 0)
                        productTypes[1] = "SP";
                    if (sampGrpsLCD.Sum(ldo => ldo.SumCUFTrecv) > 0)
                        productTypes[2] = "RP";

                    //  need unit for 100% and STM of Y
                    string currMeth = Utilities.MethodLookup(jcp.Stratum, bslyr);
                    if (currMeth == "100" || jcp.STM == "Y")
                    {
                        //  find unit numbers in the PRO table for this group
                        List<PRODO> justUnits = PROmethods.GetCutTrees(proList, jcp.Stratum, jcp.SampleGroup, 
                                                                            jcp.UOM, jcp.STM);
                        //  create records for each unit and product
                        foreach (PRODO ju in justUnits)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                SumFields sf = new SumFields();
                                sf.recType = "1V";
                                sf.saleNum = cruiseNum;
                                sf.currRG = regionNumber;
                                sf.currUOM = jcp.UOM;
                                if (jcp.SampleGroup == "" || jcp.SampleGroup == " " || jcp.SampleGroup == null)
                                    sf.currSG = "~ ";
                                else sf.currSG = jcp.SampleGroup;
                                sf.currST = jcp.Stratum;
                                sf.LCDunit = ju.CuttingUnit;
                                switch (productTypes[k])
                                {
                                    case "PP":
                                        sf.currPR = jcp.PrimaryProduct;
                                        sf.prodSource = "P";
                                        sumList.Add(sf);
                                        break;
                                    case "SP":
                                        sf.currPR = jcp.SecondaryProduct;
                                        sf.prodSource = "S";
                                        sumList.Add(sf);
                                        break;
                                    case "RP":
                                        sf.currPR = jcp.SecondaryProduct;
                                        sf.prodSource = "R";
                                        sumList.Add(sf);
                                        break;
                                }   //  end switch
                            }   //  end for loop for product type
                        }   //  end foreach loop                        
                    }
                    else if(currMeth != "FIXCNT")
                    {
                        //  create a record for each product type
                        for (int k = 0; k < 3; k++)
                        {
                            SumFields sf = new SumFields();
                            sf.recType = "1V";
                            sf.saleNum = cruiseNum;
                            sf.currRG = regionNumber;
                            sf.currUOM = jcp.UOM;
                            if (jcp.SampleGroup == "" || jcp.SampleGroup == " " || jcp.SampleGroup == null)
                                sf.currSG = "~ ";
                            else sf.currSG = jcp.SampleGroup;
                            sf.currST = jcp.Stratum;
                            sf.LCDunit = "~  ";
                            switch (productTypes[k])
                            {
                                case "PP":
                                    sf.currPR = jcp.PrimaryProduct;
                                    sf.prodSource = "P";
                                    sumList.Add(sf);
                                    break;
                                case "SP":
                                    sf.currPR = jcp.SecondaryProduct;
                                    sf.prodSource = "S";
                                    sumList.Add(sf);
                                    break;
                                case "RP":
                                    sf.currPR = jcp.SecondaryProduct;
                                    sf.prodSource = "R";
                                    sumList.Add(sf);
                                    break;
                            }   //  end switch
                        }   //  end for loop
                    }   //  endif method and STM
                }   //  endif tempSum
            }   //  end foreach loop

            WriteCurrentRecords(strSumOut, "1V");
            return;
        }   //  end Load1V


        private void Load2V(StreamWriter strSumOut, List<CuttingUnitDO> cuList, List<PRODO> proList,List<StratumDO> strList)
        {
            double PCPOPSTG = 0.0;
            List<LCDDO> lcdList = bslyr.getLCD();
            foreach(StratumDO sd in strList)
            {
                string currMeth = sd.Method;
                double STacres = Utilities.AcresLookup((long)sd.Stratum_CN, bslyr, sd.Code);
                if(currMeth != "FIXCNT")
                {
                    //  Pull strata from PRO table
                    List<PRODO> justStrata = PROmethods.GetCutTrees(proList,sd.Code,"","", 1);
                    foreach(PRODO js in justStrata)
                    {
                        //  reset product types to prevent extraneous records
                        for (int k = 0; k < 3; k++)
                            productTypes[k] = "";
                        //  what products are used?
                        List<LCDDO> sampGrpsLCD = LCDmethods.GetCutOnlyMultipleValue(lcdList, js.Stratum, js.SampleGroup,
                                                                                          "", "C", js.UOM, js.STM);
                        if (sampGrpsLCD.Sum(ldo => ldo.SumGCUFT) > 0)
                            productTypes[0] = "PP";
                        if (sampGrpsLCD.Sum(ldo => ldo.SumGCUFTtop) > 0)
                            productTypes[1] = "SP";
                        if (sampGrpsLCD.Sum(ldo => ldo.SumCUFTrecv) > 0)
                            productTypes[2] = "RP";

                        //  calculate PCPOPSTG
                        switch (currMeth)
                        {
                            case "100":
                            case "STR":
                            case "3P":
                            case "S3P":
                                PCPOPSTG = js.ProrationFactor * 100;
                                break;
                            default:
                                if (STacres > 0)
                                {
                                    double currArea = CuttingUnitMethods.GetUnitAcres(cuList, js.CuttingUnit);
                                    PCPOPSTG = (currArea / STacres) * 100;
                                }
                                    break;
                        }   //  end switch on method
                        //  create records for each product group
                        for (int k = 0; k < 3; k++)
                        {
                            SumFields sf = new SumFields();
                            switch (productTypes[k])
                            {
                                case "PP":
                                    sf.recType = "2V";
                                    sf.saleNum = cruiseNum;
                                    sf.currST = sd.Code;
                                    sf.currCU = js.CuttingUnit;
                                    sf.currRG = regionNumber;
                                    if (currMeth == "100" || js.STM == "Y")
                                        sf.LCDunit = js.CuttingUnit;
                                    else sf.LCDunit = "~  ";
                                    sf.currPR = js.PrimaryProduct;
                                    sf.prodSource = "P";
                                    sf.currUOM = js.UOM;
                                    if(js.SampleGroup.Length > 2) js.SampleGroup = js.SampleGroup.Substring(0, 2);
                                    if (js.SampleGroup == "" || js.SampleGroup == " " || js.SampleGroup == null)
                                        sf.currSG = "~ ";
                                    else sf.currSG = js.SampleGroup;
                                    sf.alpha1 = Utilities.FormatField(PCPOPSTG, "{0,10:F6}").ToString();
                                    sumList.Add(sf);
                                    break;
                                case "SP":
                                    sf.recType = "2V";
                                    sf.saleNum = cruiseNum;
                                    sf.currST = sd.Code;
                                    sf.currCU = js.CuttingUnit;
                                    sf.currRG = regionNumber;
                                    if (currMeth == "100" || js.STM == "Y")
                                        sf.LCDunit = js.CuttingUnit;
                                    else sf.LCDunit = "~  ";
                                    sf.currPR = js.SecondaryProduct;
                                    sf.prodSource = "S";
                                    sf.currUOM = js.UOM;
                                    if (js.SampleGroup == "" || js.SampleGroup == " " || js.SampleGroup == null)
                                        sf.currSG = "~ ";
                                    else sf.currSG = js.SampleGroup;
                                    sf.alpha1 = Utilities.FormatField(PCPOPSTG, "{0,10:F6}").ToString();
                                    sumList.Add(sf);
                                    break;
                                case "RP":
                                    sf.recType = "2V";
                                    sf.saleNum = cruiseNum;
                                    sf.currST = sd.Code;
                                    sf.currCU = js.CuttingUnit;
                                    sf.currRG = regionNumber;
                                    if (currMeth == "100" || js.STM == "Y")
                                        sf.LCDunit = js.CuttingUnit;
                                    else sf.LCDunit = "~  ";
                                    sf.currPR = js.SecondaryProduct;
                                    sf.prodSource = "R";
                                    sf.currUOM = js.UOM;
                                    if (js.SampleGroup == "" || js.SampleGroup == " " || js.SampleGroup == null)
                                        sf.currSG = "~ ";
                                    else sf.currSG = js.SampleGroup;
                                    sf.alpha1 = Utilities.FormatField(PCPOPSTG, "{0,10:F6}").ToString();
                                    sumList.Add(sf);
                                    break;
                            }   //  end switch on product type
                        }   //  for k loop on product type
                    }   //  end foreach loop  on PRO strata
                }   //  endif current method
            }   //  end foreach loop on strata

                            
            WriteCurrentRecords(strSumOut, "2V");
            return;
        }   //  end Load2V


        private void Load3V(StreamWriter strSumOut, List<StratumDO> strList, List<PRODO> proList)
        {
            double STacres;
            ArrayList justUnits = new ArrayList();
            List<LCDDO> lcdList = bslyr.getLCD();
            List<TreeDO> tList = bslyr.getTrees();
            TreeListMethods Tlm = new TreeListMethods();
            //  Process by stratum
            foreach (StratumDO sd in strList)
            {
                //  save correct acres for later use   
                STacres = Utilities.ReturnCorrectAcres(sd.Code, bslyr, (long)sd.Stratum_CN);
                if (sd.Method != "FIXCNT")
                {
                    //  need LCD by stratum
                    List<LCDDO> justStrata = LCDmethods.GetCutOrLeave(lcdList, "C", "", sd.Code, "");
                    

                    foreach (LCDDO js in justStrata)
                    {
                        //  reset productTypes to prevent extraneous records
                        for (int k = 0; k < 3; k++)
                            productTypes[k] = "";
                        //  and what products are needed?
                        productTypes[0] = "PP";
                        if(js.SumGCUFTtop > 0)
                            productTypes[1] = "SP";
                        if (js.SumCUFTrecv > 0)
                            productTypes[2] = "RP";
                        //  need to find unit for this species group
                        justUnits.Clear();
                        if (sd.Method == "100" || js.STM == "Y")
                        {
                            List<TreeDO> currentStratum = Tlm.GetCurrentStratum(tList, sd.Code);
                            justUnits = getLCDunits(currentStratum,"C","M",js.Species,js.STM);
                            foreach (object ju in justUnits)
                            {
                                //  pull trees
                                List<TreeCalculatedValuesDO> tcvList = bslyr.getTreeCalculatedValues((int) sd.Stratum_CN);
                                List<TreeCalculatedValuesDO> justTrees = tcvList.FindAll(
                                    delegate(TreeCalculatedValuesDO tcv)
                                    {
                                        return tcv.Tree.CuttingUnit.Code == ju.ToString() && tcv.Tree.Species == js.Species && tcv.Tree.SampleGroup.UOM == js.UOM &&
                                            tcv.Tree.SampleGroup.Code == js.SampleGroup && tcv.Tree.SampleGroup.PrimaryProduct == js.PrimaryProduct &&
                                            tcv.Tree.SampleGroup.SecondaryProduct == js.SecondaryProduct && tcv.Tree.LiveDead == js.LiveDead &&
                                            tcv.Tree.Grade == js.TreeGrade && tcv.Tree.TreeDefaultValue.ContractSpecies == js.ContractSpecies &&
                                            tcv.Tree.Stratum.YieldComponent == js.Yield && tcv.Tree.STM == js.STM;
                                            //tcv.Tree.TreeDefaultValue.Chargeable == js.Yield && tcv.Tree.STM == js.STM;
                                    });
                                
                                Create100percent(justTrees, js.Stratum, js.UOM, js.SampleGroup, js.PrimaryProduct, ju.ToString(), js.SecondaryProduct, js.Species, 
                                                    js.LiveDead, js.TreeGrade, js.Yield, js.ContractSpecies, STacres);
                            }   //  end foreach loop
                        }
                        else if (sd.Method != "100")
                        {
                            justUnits.Add("~  ");
                            CreateOther(js, STacres, justUnits, sd.Method);
                        }   //  endif on method
                    }   //  end foreach loop on LCD data
                }   //  endif on method
            }   //  end foreach loop on stratum

            WriteCurrentRecords(strSumOut, "3V");
            return;
        }   //  end Load3V


        private void Create100percent(List<TreeCalculatedValuesDO> justTrees, string currST, string currUOM, string currSG, string currPP, string currCU,
                                string currSP, string currSpec, string currLD, string currTGrade, string currYLD, string currCS, double currAC)
        {
            //  Loads 3V records for 100 percent method or STM = Y
            for (int k = 0; k < 3; k++)
            {
                SumFields sf = new SumFields();
                sf.recType = "3V";
                sf.saleNum = cruiseNum;
                sf.currST = currST;
                sf.currRG = regionNumber;
                sf.currUOM = currUOM;
                sf.LCDunit = currCU;
                if(currSG.Length > 2) currSG = currSG.Substring(0, 2);
                if (currSG == "" || currSG == " " || currSG == null)
                    sf.currSG = "~ ";
                else sf.currSG = currSG;

                switch (productTypes[k])
                {
                    case "PP":
                        sf.currPR = currPP;
                        sf.prodSource = "P";
                        break;
                    case "SP":
                        sf.currPR = currSP;
                        sf.prodSource = "S";
                        break;
                    case "RP":
                        sf.currPR = currSP;
                        sf.prodSource = "R";
                        break;
                }   //  end switch

                //  load remaining identifying fields
                sf.alpha1 = currSpec;
                if(currLD == "")
                    sf.alpha2 = "L";
                else sf.alpha2 = currLD;
                if(currTGrade == "")
                    sf.alpha3 = "~";
                else sf.alpha3 = currTGrade;
                if(currYLD == "")
                    sf.alpha4 = "CL";
                else sf.alpha4 = currYLD.ToUpper();
                if (currCS == null)
                    sf.alpha5 = " ";
                else sf.alpha5 = currCS.ToUpper();

                //  sum up tree values as needed
                double SumEF = justTrees.Sum(j => j.Tree.ExpansionFactor);
                double calcValue = 0;

                if (SumEF > 0)
                {
                    switch (productTypes[k])
                    {
                        case "PP":
                            calcValue = justTrees.Sum(j => j.Tree.DBH * j.Tree.ExpansionFactor);
                            //  mean DBH
                            if (SumEF > 0)
                                sf.alpha6 = Utilities.FormatField(calcValue / SumEF, "{0,5:F1}").ToString();
                            else sf.alpha6 = "  0.0";


                            //  average height
                            double hgtOne = 0.0;
                            double hgtTwo = 0.0;
                            CalculateAverageHeight(justTrees, ref hgtOne, ref hgtTwo);
                            if (regionNumber == "08" || regionNumber == "09")
                            {
                                //  average heights are switched in position
                                sf.alpha7 = Utilities.FormatField(hgtTwo, "{0,5:F1}").ToString();
                                sf.alpha8 = Utilities.FormatField(hgtOne, "{0,5:F1}").ToString();
                            }
                            else
                            {
                                sf.alpha7 = Utilities.FormatField(hgtOne, "{0,5:F1}").ToString();
                                sf.alpha8 = Utilities.FormatField(hgtTwo, "{0,5:F1}").ToString();
                            }   //  endif
                            //  quad mean DBH
                            calcValue = justTrees.Sum(jt => Math.Pow(jt.Tree.DBH, 2));
                            sf.alpha10 = Utilities.FormatField(Math.Sqrt(calcValue / SumEF), "{0,5:F1}").ToString();
                            sf.alpha11 = Utilities.FormatField(calcValue, "{0,5:F1}").ToString();
                            break;
                        case "SP":      case "RP":
                            sf.alpha6 = "0.0";
                            sf.alpha7 = "0.0";
                            sf.alpha8 = "0.0";
                            sf.alpha10 = "0.0";
                            sf.alpha11 = "0.0";
                            break;
                    }   //  end switch on product

                    sf.alpha9 = Utilities.FormatField(SumEF, "{0,11:F2}").ToString();
                }   //  endif SumEF

                //  sum up volumes
                double calcCUFT = 0.0;
                double calcBDFT = 0.0;
                double calcWGT = 0.0;

                switch(productTypes[k])
                {
                    case "PP":
                        calcCUFT = justTrees.Sum(jt => jt.NetCUFTPP * jt.Tree.ExpansionFactor);
                        calcBDFT = justTrees.Sum(jt => jt.NetBDFTPP * jt.Tree.ExpansionFactor);
                        calcWGT = justTrees.Sum(jt => jt.BiomassMainStemPrimary * jt.Tree.ExpansionFactor);
                        if(calcCUFT > 0 || calcBDFT > 0 || calcWGT > 0)
                        {
                            //  measured trees
                            sf.alpha12 = Utilities.FormatField(justTrees.Count, "{0,5:F0}").ToString();
                            //  total CUFT
                            calcValue = justTrees.Sum(jt=>jt.TotalCubicVolume * jt.Tree.ExpansionFactor);
                            sf.alpha13 = Utilities.FormatField(calcValue * currAC, "{0,8:F0}").ToString();
                            //  gross cuft
                            calcValue = justTrees.Sum(jt => jt.GrossCUFTPP * jt.Tree.ExpansionFactor);
                            sf.alpha14 = Utilities.FormatField(calcValue * currAC, "{0,8:F0}").ToString();
                            //  gross CUFT removed
                            calcValue = justTrees.Sum(jt => jt.GrossCUFTRemvPP * jt.Tree.ExpansionFactor);
                            sf.alpha15 = Utilities.FormatField(calcValue * currAC, "{0,8:F0}").ToString();
                            //  net cuft
                            sf.alpha16 = Utilities.FormatField(calcCUFT * currAC, "{0,8:F0}").ToString();
                            //  gross bdft
                            calcValue = justTrees.Sum(jt => jt.GrossBDFTPP * jt.Tree.ExpansionFactor);
                            sf.alpha17 = Utilities.FormatField(calcValue * currAC, "{0,9:F0}").ToString();
                            //  gross BDFT removed
                            calcValue = justTrees.Sum(jt => jt.GrossBDFTRemvPP * jt.Tree.ExpansionFactor);
                            sf.alpha18 = Utilities.FormatField(calcValue * currAC, "{0,9:F0}").ToString();
                            //  net BDFT
                            sf.alpha19 = Utilities.FormatField(calcBDFT * currAC, "{0,9:F0}").ToString();
                            //  skip
                            sf.alpha20 = "     0.0";
                            //  other values
                            sf.alpha21 = Utilities.FormatField((calcWGT * currAC) / 2000, "{0,8:F2}").ToString();
                            calcValue = justTrees.Sum(jt => jt.ValuePP * jt.Tree.ExpansionFactor);
                            sf.alpha22 = Utilities.FormatField(calcValue * currAC, "{0,11:F2}").ToString();
                            calcValue = justTrees.Sum(jt => jt.CordsPP * jt.Tree.ExpansionFactor);
                            sf.alpha23 = Utilities.FormatField(calcValue * currAC, "{0,8:F2}").ToString();
                            sf.alpha24 = Utilities.FormatField(SumEF * currAC, "{0,8:F0}").ToString();
                            calcValue = justTrees.Sum(jt=>jt.NumberlogsMS * jt.Tree.ExpansionFactor);
                            sf.alpha25 = Utilities.FormatField(calcValue * currAC, "{0,8:F0}").ToString();
                            sf.alpha26 = "     0.0";
                            sumList.Add(sf);
                        }   //  endif
                        break;
                    case "SP":
                        calcCUFT = justTrees.Sum(jt => jt.NetCUFTSP * jt.Tree.ExpansionFactor);
                        calcBDFT = justTrees.Sum(jt => jt.NetBDFTSP * jt.Tree.ExpansionFactor);
                        calcWGT = justTrees.Sum(jt => jt.BiomassMainStemSecondary * jt.Tree.ExpansionFactor);
                        if(calcCUFT > 0 || calcBDFT > 0 || calcWGT > 0)
                        {
                            //  measured trees
                            sf.alpha12 = Utilities.FormatField(justTrees.Count, "{0,5:F0}").ToString();
                            //  total CUFT
                            sf.alpha13 = "     0.0";
                            //  gross cuft
                            //  per K.Cormier only output secondary when UOM matches volume type -- August 2014
                            if (currUOM == "03")
                            {
                                calcValue = justTrees.Sum(jt => jt.GrossCUFTSP * jt.Tree.ExpansionFactor);
                                sf.alpha14 = Utilities.FormatField(calcValue * currAC, "{0,8:F0}").ToString();
                            }
                            else sf.alpha14 = "     0.0";
                            //  gross CUFT removed
                            sf.alpha15 = "     0.0";
                            //  net cuft
                            sf.alpha16 = Utilities.FormatField(calcCUFT * currAC, "{0,8:F0}").ToString();
                            //  gross bdft
                            //  see note above under CUFT
                            if (currUOM == "01")
                            {
                                calcValue = justTrees.Sum(jt => jt.GrossBDFTSP * jt.Tree.ExpansionFactor);
                                sf.alpha17 = Utilities.FormatField(calcValue * currAC, "{0,9:F0}").ToString();
                            }
                            else sf.alpha17 = "      0.0";
                            //  gross BDFT removed
                            sf.alpha18 = "     0.0";
                            //  net BDFT
                            sf.alpha19 = Utilities.FormatField(calcBDFT * currAC, "{0,9:F0}").ToString();
                            //  skip
                            sf.alpha20 = "     0.0";
                            //  other values
                            //  same thing as with BDFT and CUFT --  match unit of measure
                            if (currUOM == "05")
                                sf.alpha21 = Utilities.FormatField((calcWGT * currAC) / 2000, "{0,8:F2}").ToString();
                            else sf.alpha21 = "    0.00";
                            calcValue = justTrees.Sum(jt => jt.ValueSP * jt.Tree.ExpansionFactor);
                            sf.alpha22 = Utilities.FormatField(calcValue * currAC, "{0,11:F2}").ToString();
                            calcValue = justTrees.Sum(jt => jt.CordsSP * jt.Tree.ExpansionFactor);
                            sf.alpha23 = Utilities.FormatField(calcValue * currAC, "{0,8:F2}").ToString();
                            sf.alpha24 = Utilities.FormatField(SumEF * currAC, "{0,8:F0}").ToString();
                            calcValue = justTrees.Sum(jt=>jt.NumberlogsTPW * jt.Tree.ExpansionFactor);
                            sf.alpha25 = Utilities.FormatField(calcValue * currAC, "{0,8:F0}").ToString();
                            sf.alpha26 = "     0.0";
                            sumList.Add(sf);
                        }   //  endif
                        break;
                    case "RP":                     
                        calcCUFT = justTrees.Sum(jt => jt.GrossCUFTRP * jt.Tree.ExpansionFactor);
                        calcBDFT = justTrees.Sum(jt => jt.GrossBDFTRP * jt.Tree.ExpansionFactor);
                        if (calcCUFT > 0 || calcBDFT > 0)
                        {
                            //  measured trees
                            sf.alpha12 = Utilities.FormatField(justTrees.Count, "{0,5:F0}").ToString();
                            //  total CUFT
                            sf.alpha13 = "     0.0";
                            //  gross cuft
                            sf.alpha14 = "     0.0";
                            //  gross CUFT removed
                            sf.alpha15 = "     0.0";
                            //  net cuft
                            sf.alpha16 = Utilities.FormatField(calcCUFT * currAC, "{0,8:F0}").ToString();
                            //  gross bdft
                            sf.alpha17 = "     0.0";
                            //  gross BDFT removed
                            sf.alpha18 = "     0.0";
                            //  net BDFT
                            sf.alpha19 = Utilities.FormatField(calcBDFT * currAC, "{0,9:F0}").ToString();
                            //  skip
                            sf.alpha20 = "     0.0";
                            //  other values
                            sf.alpha21 = "     0.0";
                            calcValue = justTrees.Sum(jt => jt.ValueRP * jt.Tree.ExpansionFactor);
                            sf.alpha22 = Utilities.FormatField(calcValue * currAC, "{0,11:F2}").ToString();
                            calcValue = justTrees.Sum(jt => jt.CordsRP * jt.Tree.ExpansionFactor);
                            sf.alpha23 = Utilities.FormatField(calcValue * currAC, "{0,8:F2}").ToString();
                            sf.alpha24 = Utilities.FormatField(SumEF * currAC, "{0,8:F0}").ToString();
                            sf.alpha25 = "     0.0";
                            sf.alpha26 = "     0.0";
                            sumList.Add(sf);
                        }   //  endif
                        break;
                }   //  end switch
            }   //  end for k loop
            return;
        }   //  end Create100percent


        private void CreateOther(LCDDO js, double STacres, ArrayList justUnits, string currMethod)
        {
            foreach (object ju in justUnits)
            {
                for (int k = 0; k < 3; k++)
                {
                    SumFields sf = new SumFields();
                    sf.recType = "3V";
                    sf.saleNum = cruiseNum;
                    sf.currST = js.Stratum;
                    sf.currRG = regionNumber;
                    sf.currUOM = js.UOM;
                    sf.LCDunit = ju.ToString();
                    if(js.SampleGroup.Length > 2) js.SampleGroup = js.SampleGroup.Substring(0, 2);
                    if (js.SampleGroup == "" || js.SampleGroup == " " || js.SampleGroup == null)
                        sf.currSG = "~ ";
                    else sf.currSG = js.SampleGroup;
                    switch (productTypes[k])
                    {
                        case "PP":
                            sf.currPR = js.PrimaryProduct;
                            sf.prodSource = "P";
                            break;
                        case "SP":
                            sf.currPR = js.SecondaryProduct;
                            sf.prodSource = "S";
                            break;
                        case "RP":
                            sf.currPR = js.SecondaryProduct;
                            sf.prodSource = "R";
                            break;
                    }   //  end switch on product type


                    // load remaining fields
                    sf.alpha1 = js.Species;
                    if (js.LiveDead == "")
                        sf.alpha2 = "L";
                    else sf.alpha2 = js.LiveDead;
                    if (js.TreeGrade == "")
                        sf.alpha3 = "~";
                    else sf.alpha3 = js.TreeGrade;
                    if (js.Yield == "")
                        sf.alpha4 = "CL";
                    else sf.alpha4 = js.Yield.ToUpper();
                    if (js.ContractSpecies == null)
                        sf.alpha5 = " ";
                    else sf.alpha5 = js.ContractSpecies.ToUpper();

                    //  calculated values
                    double hgtOne = 0.0;
                    double hgtTwo = 0.0;
                    if (js.SumExpanFactor > 0)
                    {
                        switch (productTypes[k])
                        {
                            case "PP":
                                //  mean DBH
                                sf.alpha6 = Utilities.FormatField((js.SumDBHOB / js.SumExpanFactor), "{0,5:F1}").ToString();
                                //  average heights
                                CalculateAverageHeight(js, ref hgtOne, ref hgtTwo);
                                if (regionNumber == "08" || regionNumber == "09")
                                {
                                    // average heights are switched in position
                                    sf.alpha7 = Utilities.FormatField(hgtTwo, "{0,5:F1}").ToString();
                                    sf.alpha8 = Utilities.FormatField(hgtOne, "{0,5:F1}").ToString();
                                }
                                else
                                {
                                    sf.alpha7 = Utilities.FormatField(hgtOne, "{0,5:F1}").ToString();
                                    sf.alpha8 = Utilities.FormatField(hgtTwo, "{0,5:F1}").ToString();
                                }   //  endif

                                //  quad mean DBH
                                if (currMethod == "STR" || currMethod == "3P" || currMethod == "S3P")
                                {
                                    sf.alpha10 = Utilities.FormatField(Math.Sqrt(js.SumDBHOBsqrd / js.SumExpanFactor), "{0,5:F1}").ToString();
                                    sf.alpha11 = Utilities.FormatField(js.SumDBHOBsqrd, "{0,11:F2}").ToString();
                                }
                                else
                                {
                                    sf.alpha10 = Utilities.FormatField(Math.Sqrt(js.SumDBHOBsqrd / js.SumExpanFactor), "{0,5:F1}").ToString();
                                    sf.alpha11 = Utilities.FormatField(js.SumDBHOBsqrd * STacres, "{0,11:F2}").ToString();
                                }   //  endif on method
                                break;
                            case "SP":
                            case "RP":
                                sf.alpha6 = "0.0";
                                sf.alpha7 = "0.0";
                                sf.alpha8 = "0.0";
                                sf.alpha10 = "0.0";
                                sf.alpha11 = "0.0";
                                break;
                        }   //  end switch on product

                        //  corrected expansion factor is the same for all products
                        if (currMethod == "S3P" || currMethod == "3P")
                        {
                            if (js.STM == "Y")
                                sf.alpha9 = Utilities.FormatField(js.SumExpanFactor, "{0,11:F2}").ToString();
                            else
                                sf.alpha9 = Utilities.FormatField((js.SumExpanFactor * js.TalliedTrees / js.SumExpanFactor), "{0,11:F2}").ToString();
                        }
                        else sf.alpha9 = Utilities.FormatField(js.SumExpanFactor * STacres, "{0,11:F2}").ToString(); ;
                    }   //  endif expansion factor

                    //  fields not calculated
                    switch (productTypes[k])
                    {
                        case "PP":
                            sf.alpha12 = Utilities.FormatField(js.MeasuredTrees, "{0,5:F0}").ToString();
                            sf.alpha13 = Utilities.FormatField(js.SumTotCubic * STacres, "{0,8:F0}").ToString();
                            sf.alpha14 = Utilities.FormatField(js.SumGCUFT * STacres, "{0,8:F0}").ToString();
                            sf.alpha15 = Utilities.FormatField(js.SumGCUFTremv * STacres, "{0,8:F0}").ToString();
                            sf.alpha16 = Utilities.FormatField(js.SumNCUFT * STacres, "{0,8:F0}").ToString();
                            sf.alpha17 = Utilities.FormatField(js.SumGBDFT * STacres, "{0,9:F0}").ToString();
                            sf.alpha18 = Utilities.FormatField(js.SumGBDFTremv * STacres, "{0,9:F0}").ToString();
                            sf.alpha19 = Utilities.FormatField(js.SumNBDFT * STacres, "{0,9:F0}").ToString();
                            sf.alpha20 = "     0.0";
                            sf.alpha21 = Utilities.FormatField(js.SumWgtMSP * STacres / 2000, "{0,8:F2}").ToString();
                            sf.alpha22 = Utilities.FormatField(js.SumValue * STacres, "{0,11:F2}").ToString();
                            sf.alpha23 = Utilities.FormatField(js.SumCords * STacres, "{0,8:F2}").ToString();
                            sf.alpha24 = Utilities.FormatField(js.SumExpanFactor * STacres, "{0,8:F0}").ToString();
                            sf.alpha25 = Utilities.FormatField(js.SumLogsMS * STacres, "{0,8:F0}").ToString();
                            sf.alpha26 = "     0.0";
                            if (js.SumNCUFT > 0 || js.SumNBDFT > 0 || js.SumWgtMSP > 0)
                                sumList.Add(sf);
                            break;
                        case "SP":
                            sf.alpha12 = Utilities.FormatField(js.MeasuredTrees, "{0,5:F0}").ToString();
                            sf.alpha13 = "     0.0";
                            sf.alpha14 = Utilities.FormatField(js.SumGCUFTtop * STacres, "{0,8:F0}").ToString();
                            sf.alpha15 = "     0.0";
                            sf.alpha16 = Utilities.FormatField(js.SumNCUFTtop * STacres, "{0,8:F0}").ToString();
                            sf.alpha17 = Utilities.FormatField(js.SumGBDFTtop * STacres, "{0,9:F0}").ToString();
                            sf.alpha18 = "     0.0";
                            sf.alpha19 = Utilities.FormatField(js.SumNBDFTtop * STacres, "{0,9:F0}").ToString();
                            sf.alpha20 = "     0.0";
                            sf.alpha21 = Utilities.FormatField(js.SumWgtMSS * STacres / 2000, "{0,8:F2}").ToString();
                            sf.alpha22 = Utilities.FormatField(js.SumTopValue * STacres, "{0,11:F2}").ToString();
                            sf.alpha23 = Utilities.FormatField(js.SumCordsTop * STacres, "{0,8:F2}").ToString();
                            sf.alpha24 = Utilities.FormatField(js.SumExpanFactor * STacres, "{0,8:F0}").ToString();
                            sf.alpha25 = Utilities.FormatField(js.SumLogsTop * STacres, "{0,8:F0}").ToString();
                            sf.alpha26 = "     0.0";
                            if (js.SumNCUFTtop > 0 || js.SumNBDFTtop > 0 || js.SumWgtMSS > 0)
                                sumList.Add(sf);
                            break;
                        case "RP":
                            sf.alpha12 = Utilities.FormatField(js.MeasuredTrees, "{0,5:F0}").ToString();
                            sf.alpha13 = "     0.0";
                            sf.alpha14 = "     0.0";
                            sf.alpha15 = "     0.0";
                            sf.alpha16 = Utilities.FormatField(js.SumCUFTrecv * STacres, "{0,8:F0}").ToString();
                            sf.alpha17 = "     0.0";
                            sf.alpha18 = "     0.0";
                            sf.alpha19 = Utilities.FormatField(js.SumBDFTrecv, "{0,9:F0}").ToString();
                            sf.alpha20 = "     0.0";
                            sf.alpha21 = "     0.0";
                            sf.alpha22 = Utilities.FormatField(js.SumValueRecv * STacres, "{0,11:F2}").ToString();
                            sf.alpha23 = Utilities.FormatField(js.SumCordsRecv * STacres, "{0,8:F2}").ToString();
                            sf.alpha24 = Utilities.FormatField(js.SumExpanFactor * STacres, "{0,8:F0}").ToString();
                            sf.alpha25 = "     0.0";
                            sf.alpha26 = "     0.0";
                            if (js.SumCUFTrecv > 0 || js.SumBDFTrecv > 0)
                                sumList.Add(sf);
                            break;
                    }   //end switch
                }   //  end for k loop on product
            }   //  end foreach unit
            return;
        }   //  end CreateOthers


        private void WriteCurrentRecords(StreamWriter strSumOut, string recToPrint)
        {
            StringBuilder prtLine = new StringBuilder();

            switch (recToPrint)
            {
                case "1A":
                    foreach (SumFields sf in sumList)
                    {
                        prtLine.Remove(0,prtLine.Length);
                        prtLine.Append(sf.recType);
                        prtLine.Append(sf.saleNum.PadRight(5,' '));     //  cruise number
                        prtLine.Append("  ");                           //  cruise number ends in col7; sale number starts in col 10
                        prtLine.Append(sf.saleNum.PadLeft(5, '0'));     //  sale number
                        prtLine.Append(sf.currRG.PadLeft(2, '0'));      //  region number
                        prtLine.Append(sf.alpha1.PadLeft(2, '0'));      //  forest number
                        if (sf.alpha2 == null)                          //  district number
                            prtLine.Append("  ");
                        else
                            prtLine.Append(sf.alpha2.PadLeft(2, '0'));     
                        prtLine.Append(sf.alpha3);                     //  calendar year
                        prtLine.Append("                          ");    //  need 25 blanks here
                        prtLine.Append(sf.alpha4);                     //  purpose
                        prtLine.Append(sf.alpha5);                     //  total error
                        prtLine.Append(sf.alpha6);                     //  version
                        prtLine.Append("P ");
                        prtLine.Append("C");
                        prtLine.Append("+");
                        prtLine.Append("Y");
                        strSumOut.WriteLine(prtLine);
                    }   //  end foreach loop
                    break;
                case "2A":
                    foreach(SumFields sf in sumList)
                    {
                        prtLine.Remove(0,prtLine.Length);
                        prtLine.Append(sf.recType);
                        prtLine.Append(sf.saleNum.PadRight(5,' '));      //  cruise number
                        prtLine.Append("  ");                           //  see note above
                        prtLine.Append(sf.currCU.PadRight(3,' '));      //  cutting unit number
                        prtLine.Append(sf.alpha1.PadLeft(8,' '));       //  acres
                        if (sf.alpha2 == null)                          //  description
                            prtLine.Append(" ".PadRight(25, ' '));
                        else prtLine.Append(sf.alpha2.PadRight(25,' '));     
                        prtLine.Append(sf.alpha3.PadRight(3,' '));      //  logging method
                        prtLine.Append(sf.alpha4.PadRight(3,' '));      //  payment unit
                        prtLine.Append("      ");
                        prtLine.Append(sf.currRG.PadLeft(2,'0'));       //  region number
                        strSumOut.WriteLine(prtLine);
                    }   //  end foreach loop
                    break;
                case "3A":
                    foreach (SumFields sf in sumList)
                    {
                        prtLine.Remove(0, prtLine.Length);
                        prtLine.Append(sf.recType);
                        prtLine.Append(sf.saleNum.PadRight(5, ' '));     //  cruise number
                        prtLine.Append(sf.currST.PadLeft(2, ' '));      //  stratum number
                        prtLine.Append(sf.alpha1.PadRight(6, ' '));     //  cruise method
                        prtLine.Append(sf.alpha2.PadRight(6, ' '));      //  basal area factor
                        prtLine.Append(sf.alpha3.PadRight(4, ' '));      //  fixed plot size
                        prtLine.Append(sf.alpha4.PadRight(25, ' '));     //  description
                        prtLine.Append(sf.alpha5.PadLeft(2, '0'));      //  month
                        prtLine.Append(sf.alpha6.PadLeft(4,' '));       //  year
                        prtLine.Append(sf.currRG.PadLeft(2,'0'));       //  region number
                        strSumOut.WriteLine(prtLine);
                    }   //  end foreach loop
                    break;
                case "1V":
                    foreach (SumFields sf in sumList)
                    {
                        prtLine.Remove(0, prtLine.Length);
                        prtLine.Append(sf.recType);
                        prtLine.Append(sf.saleNum.PadRight(5, ' '));    //  cruise number
                        prtLine.Append(sf.currST.PadLeft(2, ' '));      //  stratum number
                        prtLine.Append("     ");
                        prtLine.Append(sf.currRG.PadLeft(2, '0'));      //  region number
                        prtLine.Append(sf.LCDunit.PadRight(3,' '));     //  unit for 100% and STM
                        prtLine.Append(sf.currPR.PadLeft(2, ' '));      //  product
                        prtLine.Append(sf.currUOM.PadRight(3, ' '));    //  unit of measure
                        prtLine.Append(sf.prodSource);                  //  product source (P,S or R)
                        prtLine.Append(sf.currSG.PadRight(2, ' '));     //  sample group
                        strSumOut.WriteLine(prtLine);
                    }   //  end foreach loop
                    break;
                case "2V":
                    foreach (SumFields sf in sumList)
                    {
                        prtLine.Remove(0, prtLine.Length);
                        prtLine.Append(sf.recType);
                        prtLine.Append(sf.saleNum.PadRight(5, ' '));    //  cruise number
                        prtLine.Append(sf.currST.PadLeft(2, ' '));      //  stratum number
                        prtLine.Append(sf.currCU.PadRight(3, ' '));     //  cutting unit number
                        prtLine.Append("  ");
                        prtLine.Append(sf.currRG.PadLeft(2, '0'));      //  region number
                        prtLine.Append(sf.LCDunit.PadRight(3,' '));     //  unit for 100% and STM
                        prtLine.Append(sf.currPR.PadLeft(2, ' '));      //  product
                        prtLine.Append(sf.currUOM.PadRight(3, ' '));    //  unit of measure
                        prtLine.Append(sf.prodSource);                  //  product sourcce (P,S or R)
                        prtLine.Append(sf.currSG.PadRight(2, ' '));     //  sample group
                        prtLine.Append(sf.alpha1);                      //  PCPOPSTG
                        strSumOut.WriteLine(prtLine);
                    }   //  end foreach loop
                    break;
                case "3V":
                    foreach (SumFields sf in sumList)
                    {
                        prtLine.Remove(0, prtLine.Length);
                        prtLine.Append(sf.recType);
                        prtLine.Append(sf.saleNum.PadRight(5,' '));     //  cruise number
                        prtLine.Append(sf.currST.PadLeft(2,' '));       //  stratum number
                        prtLine.Append("     ");
                        prtLine.Append(sf.currRG.PadLeft(2,'0'));       //  region number
                        prtLine.Append(sf.LCDunit.PadRight(3,' '));     //  unit for 100% and STM
                        prtLine.Append(sf.currPR.PadLeft(2,' '));       //  product
                        prtLine.Append(sf.currUOM.PadRight(3,' '));     //  unit of measure
                        prtLine.Append(sf.prodSource);                  //  product source (P,S or R)
                        prtLine.Append(sf.currSG.PadRight(2,' '));      //  sample group
                        prtLine.Append(sf.alpha1.PadRight(6,' '));      //  species
                        prtLine.Append(sf.alpha2);                      //  live/dead
                        prtLine.Append(sf.alpha3);                      //  tree grade
                        prtLine.Append(sf.alpha4);                      //  yield component
                        prtLine.Append(sf.alpha5.PadLeft(4,' '));       //  contract species
                        prtLine.Append(sf.alpha6.PadLeft(5,' '));       //  mean DBH
                        prtLine.Append(sf.alpha7.PadLeft(5,' '));       //  average height 1
                        prtLine.Append(sf.alpha8.PadLeft(5,' '));       //  average height 2
                        prtLine.Append(sf.alpha9.PadLeft(11,' '));      //  expansion factor
                        prtLine.Append(sf.alpha10.PadLeft(5,' '));      //  quad mean DBH
                        prtLine.Append(sf.alpha11.PadLeft(11,' '));     //  DBH squared
                        prtLine.Append(sf.alpha12.PadLeft(5,' '));      //  stage 1 samples
                        prtLine.Append(sf.alpha13.PadLeft(8,' '));      //  total cubic
                        prtLine.Append(sf.alpha14.PadLeft(8,' '));      //  gross cubic
                        prtLine.Append(sf.alpha15.PadLeft(8,' '));      //  gross cubic removed
                        prtLine.Append(sf.alpha16.PadLeft(8,' '));      //  net cubic
                        prtLine.Append(sf.alpha17.PadLeft(9,' '));      //  gross board
                        prtLine.Append(sf.alpha18.PadLeft(9,' '));      //  gross board removed
                        prtLine.Append(sf.alpha19.PadLeft(9,' '));      //  net board
                        prtLine.Append(sf.alpha20.PadLeft(8,' '));      //  topwood weight
                        prtLine.Append(sf.alpha21.PadLeft(8,' '));      //  mainstem weight
                        prtLine.Append(sf.alpha22.PadLeft(11,' '));     //  value
                        prtLine.Append(sf.alpha23.PadLeft(8,' '));      //  cords
                        prtLine.Append("                ");
                        prtLine.Append(sf.alpha24.PadLeft(8,' '));      //  expansion factor
                        prtLine.Append(sf.alpha25.PadLeft(8,' '));      //  number of logs
                        prtLine.Append(sf.alpha26.PadLeft(8,' '));      //  other volume
                        strSumOut.WriteLine(prtLine);
                    }   //  end foreach loop
                    break;

            }   //  end switch

            return;
        }   //  end WriteCurrentRecords


        private void CalculateAverageHeight(List<TreeCalculatedValuesDO> justTrees, ref double hgtOne, ref double hgtTwo)
        {
            // for 1005% only
            hgtOne = 0.0;
            hgtTwo = 0.0;
            double sumTH = 0.0;
            double sumMHP = 0.0;
            double sumMHS = 0.0;
            double sumUSH = 0.0;
            //  need to check height type before summing
            foreach (TreeCalculatedValuesDO jt in justTrees)
            {
                if (jt.Tree.TreeDefaultValue.MerchHeightType == "L" || jt.Tree.TreeDefaultValue.MerchHeightType == "l")
                {
                    sumTH += jt.Tree.TotalHeight * jt.Tree.ExpansionFactor;
                    sumMHP += jt.Tree.MerchHeightPrimary / 10 * jt.Tree.ExpansionFactor;
                    sumMHS += jt.Tree.MerchHeightSecondary / 10 * jt.Tree.ExpansionFactor;
                    sumUSH += jt.Tree.UpperStemHeight * jt.Tree.ExpansionFactor;
                }
                else
                {
                    sumTH += jt.Tree.TotalHeight * jt.Tree.ExpansionFactor;
                    sumMHP += jt.Tree.MerchHeightPrimary * jt.Tree.ExpansionFactor;
                    sumMHS += jt.Tree.MerchHeightSecondary * jt.Tree.ExpansionFactor;
                    sumUSH += jt.Tree.UpperStemHeight * jt.Tree.ExpansionFactor;
                }   //  endif
            }   //  end foreach loop
            double currEF = justTrees.Sum(jt => jt.Tree.ExpansionFactor);

            //  height one -- check total height first
            if (sumTH > 0)
            {
                hgtOne = sumTH / currEF;

                if (sumMHP > 0)
                    hgtTwo = sumMHP / currEF;
                else if (sumMHS > 0)
                    hgtTwo = sumMHS / currEF;
                else if (sumUSH > 0)
                    hgtTwo = sumUSH / currEF;
            }
            else if(sumTH == 0 && sumMHP > 0)
            {
                hgtOne = sumMHP / currEF;
                if (sumMHS > 0)
                    hgtTwo = sumMHS / currEF;
                else if (sumUSH > 0)
                    hgtTwo = sumUSH / currEF;
            }
            else if(hgtOne == 0.0 && hgtTwo  == 0.0)
            {
                hgtOne = sumMHS / currEF;
                hgtTwo = sumUSH / currEF;
            }   //  endif
            return;
        }   //  end CalculateAverageHeight


        private void CalculateAverageHeight(LCDDO js, ref double hgtOne, ref double hgtTwo)
        {
            //  all methods except 100%
            hgtOne = 0.0;
            hgtTwo = 0.0;
            //  calculate height one
            if (js.SumTotHgt > 0)
            {
                hgtOne = js.SumTotHgt / js.SumExpanFactor;
                if (js.SumMerchHgtPrim > 0)
                    hgtTwo = js.SumMerchHgtPrim / js.SumExpanFactor;
                else if (js.SumMerchHgtSecond > 0)
                    hgtTwo = js.SumMerchHgtSecond / js.SumExpanFactor;
                else if (js.SumHgtUpStem > 0)
                    hgtTwo = js.SumHgtUpStem / js.SumExpanFactor;
            }
            else if(js.SumTotHgt == 0 && js.SumMerchHgtPrim > 0)
            {
                hgtOne = js.SumMerchHgtPrim / js.SumExpanFactor;
                if (js.SumMerchHgtSecond > 0)
                    hgtTwo = js.SumMerchHgtSecond / js.SumExpanFactor;
                else if (js.SumHgtUpStem > 0)
                    hgtTwo = js.SumHgtUpStem / js.SumExpanFactor;
            }
            else if(hgtOne == 0.0 && hgtTwo == 0.0)
            {
                hgtOne = js.SumMerchHgtSecond / js.SumExpanFactor;
                hgtTwo = js.SumHgtUpStem / js.SumExpanFactor;
            }   //  endif
        }   //  end CalculateAverageHeight


        private double CalculateNetErr(string currUOM, List<POPDO> popList, List<StratumDO> strList)
        {
            double TotalNetErr = 0.0;
            double SummedNetVol = 0.0;
            double SummedError = 0.0;
            double TotalNetVol = 0.0;
            double TotalSummedErr = 0.0;
            List<LCDDO> lcdList = bslyr.getLCD();
            //  process by stratum
            foreach (StratumDO sd in strList)
            {
                if (sd.Method != "FIXCNT")
                {
                    double STacres = Utilities.ReturnCorrectAcres(sd.Code, bslyr, (long)sd.Stratum_CN);
                    //  pull sample groups for the stratum and loop accordingly
                    List<SampleGroupDO> currSampleGroups = bslyr.getSampleGroups((int)sd.Stratum_CN);
                    double SGnetVol = 0.0;
                    SummedNetVol = 0.0;
                    SummedError = 0.0;
                    for (int k = 0; k < 3; k++)
                    {
                        foreach (SampleGroupDO sg in currSampleGroups)
                        {
                            //  Pull LCD data needed
                            List<LCDDO> justGroup = LCDmethods.GetCutOnlyMultipleValue(lcdList, sd.Code, sg.Code, "", "C", sg.UOM, "");
                            SGnetVol = TotalVolume(sg.UOM, k, justGroup);
                            SGnetVol *= STacres;
                            //  Pull population from POP
                            List<POPDO> currentPOP = POPmethods.GetMultipleData(popList, sd.Code, sg.Code, "", "", sg.UOM, "C");
                            foreach (POPDO pd in currentPOP)
                                CalcProductNetError(pd, ref SummedNetVol, ref SummedError, sd.Method, SGnetVol, k);
                        }   //  end foreach loop
                    }   //  end loop for product
                    //  Calculate strata error
                    //if (SummedNetVol > 0)
                      //  TotalSummedErr += Math.Sqrt(SummedError) / SummedNetVol;
                    TotalSummedErr += SummedError;
                    TotalNetVol += SummedNetVol;
                }   //  endif not FIXCNT
            }   //  end foreach stratum

            if(TotalNetVol > 0)
                TotalNetErr = Math.Sqrt(TotalSummedErr)/ TotalNetVol;
            return TotalNetErr;
        }   //  end CalculateNetError


        private void CalcProductNetError(POPDO pd, ref double SummedNetVol, ref double SummedError, string currMethod,
                                            double SGnetVol, int ithProduct)
        {
            double Stg1X = 0.0;
            double Stg2X = 0.0;
            double Stg1X2 = 0.0;
            double Stg2X2 = 0.0;
            double SampErr1 = 0.0;
            double SampErr2 = 0.0;
            //  retrieve values by product
            switch (ithProduct)
            {
                case 0:     //  primary
                    Stg1X = pd.Stg1NetXPP;
                    Stg2X = pd.Stg2NetXPP;
                    Stg1X2 = pd.Stg1NetXsqrdPP;
                    Stg2X2 = pd.Stg2NetXsqrdPP;
                    break;
                case 1:     //  secondary
                    Stg1X = pd.Stg1NetXSP;
                    Stg2X = pd.Stg2NetXSP;
                    Stg1X2 = pd.Stg1NetXsqrdSP;
                    Stg2X2 = pd.Stg2NetXsqrdSP;
                    break;
                case 2:     //  recovered
                    Stg1X = pd.Stg1NetXRP;
                    Stg2X = pd.Stg2NetXRP;
                    Stg1X2 = pd.Stg1NetXRsqrdRP;
                    Stg2X2 = pd.Stg2NetXsqrdRP;
                    break;
            }   //  end switch on product

            //  Stage 1 calculations
            if (pd.StageOneSamples > 1)
            {
                //  Determine t value
                double Tvalue = CommonStatistics.LookUpT(((int)pd.StageOneSamples) - 1);
                //  Calculate stat values to get sampling error
                double Mean1 = CommonStatistics.MeanOfX(Stg1X, pd.StageOneSamples);
                double SE1 = CommonStatistics.StdError(Stg1X2, Stg1X, (float)pd.StageOneSamples, currMethod, 
                                                        (float)pd.TalliedTrees, 1);
                SampErr1 = CommonStatistics.SampleError(Mean1, SE1, Tvalue);
            }   //  endif stage 1

            //  Stage 2 calculations
            if (pd.StageTwoSamples > 1)
            {
                //  Determine t value
                double Tvalue = CommonStatistics.LookUpT(((int)pd.StageTwoSamples) - 1);
                double Mean2 = CommonStatistics.MeanOfX(Stg2X, pd.StageTwoSamples);
                double SE2 = CommonStatistics.StdError(Stg2X2, Stg2X, (float)pd.StageTwoSamples, currMethod, 
                                                        (float)pd.TalliedTrees, 2);
                SampErr2 = CommonStatistics.SampleError(Mean2, SE2, Tvalue);
            }   //  endif stage 2

            double CombinedErr = CommonStatistics.CombinedSamplingError(currMethod, SampErr1, SampErr2);
            double SGerror = Math.Pow((SGnetVol * CombinedErr), 2);

            SummedNetVol += SGnetVol;
            SummedError += SGerror;
            return;
        }   //  end CalcProductNetError


        private double TotalVolume(string currUOM, int nthProduct, List<LCDDO> currGroup)
        {
            double calcNetVol = 0.0;
            switch (nthProduct)
            {
                case 0:     //  primary
                    switch (currUOM)
                    {
                        case "01":
                            calcNetVol = currGroup.Sum(ldo => ldo.SumNBDFT);
                            break;
                        case "02":
                            calcNetVol = currGroup.Sum(ldo => ldo.SumCords);
                            break;
                        case "03":
                            calcNetVol = currGroup.Sum(ldo => ldo.SumNCUFT);
                            break;
                    }   //  end switch on UOM
                    break;
                case 1:     //  secondary
                    switch (currUOM)
                    {
                        case "01":
                            calcNetVol = currGroup.Sum(ldo => ldo.SumNBDFTtop);
                            break;
                        case "02":
                            calcNetVol = currGroup.Sum(ldo => ldo.SumCordsTop);
                            break;
                        case "03":
                            calcNetVol = currGroup.Sum(ldo => ldo.SumNCUFTtop);
                            break;
                    }   //  end switch on UOM
                    break;
                case 2:     //  recovered
                    switch (currUOM)
                    {
                        case "01":
                            calcNetVol = currGroup.Sum(ldo => ldo.SumBDFTrecv);
                            break;
                        case "02":
                            calcNetVol = currGroup.Sum(ldo => ldo.SumCordsRecv);
                            break;
                        case "03":
                            calcNetVol = currGroup.Sum(ldo => ldo.SumCUFTrecv);
                            break;
                    }   //  end switch on UOM
                    break;
            }   //  end switch on product
            return calcNetVol;
        }   //  end TotalVolume


        private ArrayList getLCDunits(List<TreeDO> currentStratum, string currCL, string currCM, string currSP, string currSTM)
        {
            List<TreeDO> justGroup = currentStratum.FindAll(
                delegate(TreeDO t)
                {
                    return t.SampleGroup.CutLeave == currCL && t.CountOrMeasure == currCM && 
                                    t.Species == currSP && t.STM == currSTM;
                });

            ArrayList unitGroups = new ArrayList();
            foreach (TreeDO jg in justGroup)
            {
                if (unitGroups.Contains(jg.CuttingUnit.Code))
                {
                    //  do nothing
                }
                else unitGroups.Add(jg.CuttingUnit.Code);
                
            }   //  end foreach
            return unitGroups;
        }   //  end getLCDunits



        //  list class for data to output for each record type
        public class SumFields
        {
            public string recType { get; set; }
            public string saleNum { get; set; }
            public string currRG { get; set; }
            public string currCU { get; set; }
            public string currST { get; set; }
            public string LCDunit { get; set; }     //  unit for 100% and STM of yes
            public string currPR { get; set; }
            public string currUOM { get; set; }
            public string prodSource { get; set; }      //  P, S or R
            public string currSG { get; set; }
            public string alpha1 { get; set; }
            public string alpha2 { get; set; }
            public string alpha3 { get; set; }
            public string alpha4 { get; set; }
            public string alpha5 { get; set; }
            public string alpha6 { get; set; }
            public string alpha7 { get; set; }
            public string alpha8 { get; set; }
            public string alpha9 { get; set; }
            public string alpha10 { get; set; }
            public string alpha11 { get; set; }
            public string alpha12 { get; set; }
            public string alpha13 { get; set; }
            public string alpha14 { get; set; }
            public string alpha15 { get; set; }
            public string alpha16 { get; set; }
            public string alpha17 { get; set; }
            public string alpha18 { get; set; }
            public string alpha19 { get; set; }
            public string alpha20 { get; set; }
            public string alpha21 { get; set; }
            public string alpha22 { get; set; }
            public string alpha23 { get; set; }
            public string alpha24 { get; set; }
            public string alpha25 { get; set; }
            public string alpha26 { get; set; }

        }   //  end SumFields
    }
}
