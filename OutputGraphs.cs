using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ZedGraph;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public class OutputGraphs 
    {
        #region
        public string currentReport;
        public string fileName;
        private StringBuilder currTitle = new StringBuilder();
        private GraphForm gf = new GraphForm();
        private List<LCDDO> speciesTotal = new List<LCDDO>();
        private List<TreeDO> treesByDBH = new List<TreeDO>();
        private List<TreeDO> justMeasured = new List<TreeDO>();
        public CPbusinessLayer bslyr = new CPbusinessLayer();
        #endregion


        public void createGraphs()
        {
            List<TreeDO> tList = bslyr.getTrees();
            List<LCDDO> lcdList = bslyr.getLCD();
            List<StratumDO> sList = bslyr.getStratum();
            List<LCDDO> justSpecies = bslyr.getLCDOrdered("WHERE CutLeave = ?", "GROUP BY Species", "C", "");
            //  pull salename and number to put in graph title
            //  also need it to create subfolder for graphs
            List<SaleDO> saleList = bslyr.getSale();
            string currSaleName = saleList[0].Name;
            if (currSaleName.Length > 25)
                currSaleName = currSaleName.Remove(25, currSaleName.Length);
            string currSaleNumber = saleList[0].SaleNumber;
            // pull data needed and call appropriate graph routine for report
            switch (currentReport)
            {
                case "GR01":
                    foreach (LCDDO js in justSpecies)
                    {
                        //  pull all trees for each species
                        List<TreeDO> justTrees = tList.FindAll(
                            delegate(TreeDO td)
                            {
                                return td.Species == js.Species && td.CountOrMeasure == "M";
                            });
                        currTitle.Append("DBH AND TOTAL HEIGHT BY ");
                        if (justTrees.Count == 0)
                        {
                            noDataForGraph(js.Species, "Species");
                            break;
                        }
                        else
                        {
                            currTitle.Append(js.Species);
                            currTitle.Append("\nSale number: ");
                            currTitle.Append(currSaleNumber);
                            currTitle.Append("   Sale name:  ");
                            currTitle.Append(currSaleName);
                            gf.currTitle = currTitle.ToString();
                            gf.currXtitle = "DBH";
                            gf.currYtitle = "TOTAL HEIGHT";
                            gf.graphNum = 1;
                            gf.currSP = js.Species;
                            gf.fileName = fileName;
                            gf.chartType = "SCATTER";
                            gf.treeList = justTrees;
                            gf.currSaleName = currSaleName;
                            gf.ShowDialog();
                        }   //  endif no data
                        currTitle.Remove(0, currTitle.Length);
                        justTrees.Clear();
                    }   //  end foreach loop
                    break;
                case "GR02":
                    if (lcdList.Count == 0)
                    {
                        MessageBox.Show("No data found for report.\nTry processing the cruise again.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }   //  endif no data
                    //  total expansion factor for each species to get data for the sale
                    foreach (LCDDO js in justSpecies)
                    {
                        List<LCDDO> speciesGroup = lcdList.FindAll(
                            delegate(LCDDO l)
                            {
                                return l.Species == js.Species;
                            });
                        expandData(speciesGroup, "EXPFAC", sList, js.Species);
                    }   //  end foreach justSpecies
                    currTitle.Remove(0, currTitle.Length);
                    currTitle.Append("SPECIES DISTRIBUTION FOR THE SALE");
                    currTitle.Append("\nSale number: ");
                    currTitle.Append(currSaleNumber);
                    currTitle.Append("   Sale name:  ");
                    currTitle.Append(currSaleName);
                    gf.currTitle = currTitle.ToString();
                    gf.currXtitle = "";
                    gf.currYtitle = "";
                    gf.graphNum = 2;
                    gf.chartType = "PIE";
                    gf.lcdList = speciesTotal;
                    gf.fileName = fileName;
                    gf.currSaleName = currSaleName;
                    gf.ShowDialog();
                    
                    break;
                case "GR03":
                    speciesTotal.Clear();
                    //  total net CUFT volume for sawtimber only
                    foreach (LCDDO js in justSpecies)
                    {
                        List<LCDDO> speciesGroup = lcdList.FindAll(
                            delegate(LCDDO l)
                            {
                                return l.Species == js.Species && l.PrimaryProduct == "01";
                            });
                        expandData(speciesGroup, "VOL", sList, js.Species);
                    }   //  end foreach justSpecies
                    currTitle.Remove(0, currTitle.Length);
                    currTitle.Append("VOLUME BY SPECIES -- SAWTIMBER ONLY");
                    currTitle.Append("\nSale number: ");
                    currTitle.Append(currSaleNumber);
                    currTitle.Append("   Sale name:  ");
                    currTitle.Append(currSaleName);
                    gf.currTitle = currTitle.ToString();
                    gf.currXtitle = "";
                    gf.currYtitle = "";
                    gf.graphNum = 3;
                    gf.chartType = "PIE";
                    gf.lcdList = speciesTotal;
                    gf.fileName = fileName;
                    gf.currSaleName = currSaleName;
                    gf.ShowDialog();
                    break;
                case "GR04":
                    List<LCDDO> justProduct = bslyr.getLCDOrdered("WHERE CutLeave = ?", "GROUP BY PrimaryProduct", "C", "");

                    speciesTotal.Clear();
                    //  total net CUFT volume for sawtimber only
                    foreach (LCDDO jp in justProduct)
                    {
                        List<LCDDO> productGroup = lcdList.FindAll(
                            delegate(LCDDO l)
                            {
                                return l.PrimaryProduct == jp.PrimaryProduct;
                            });
                        expandData(productGroup, "VOL", sList, jp.PrimaryProduct);
                    }   //  end foreach justSpecies
                    currTitle.Remove(0, currTitle.Length);
                    currTitle.Append("VOLUME BY PRODUCT");
                    currTitle.Append("\nSale number: ");
                    currTitle.Append(currSaleNumber);
                    currTitle.Append("   Sale name:  ");
                    currTitle.Append(currSaleName);
                    gf.currTitle = currTitle.ToString();
                    gf.currXtitle = "";
                    gf.currYtitle = "";
                    gf.graphNum = 4;
                    gf.chartType = "PIE";
                    gf.lcdList = speciesTotal;
                    gf.fileName = fileName;
                    gf.currSaleName = currSaleName;
                    gf.ShowDialog();
                    break;
                case "GR05":
                    //  per request from Region 10, give option to use 16 or 32 foot logs
                    //  for this graph --  July 2017
                    int whichLength = 16;
                    selectLength getLength = new selectLength();
                    getLength.ShowDialog();
                    if (getLength.lengthSelected == 0)
                        whichLength = 16;
                    else whichLength = getLength.lengthSelected;
                    //  pull by species for separate graphs
                    List<LogStockDO> logsTotal = new List<LogStockDO>();
                    List<LogStockDO> lsList = bslyr.getLogStock();
                    //  need to expand also
                    foreach (LCDDO js in justSpecies)
                    {
                        // pull all logs
                        List<LogStockDO> justSelectLogs = lsList.FindAll(
                            delegate(LogStockDO lsd)
                            {
                                return lsd.Length == whichLength && lsd.Tree.Species == js.Species;
                            });
                        if (justSelectLogs.Count == 0)
                        {
                            MessageBox.Show("Graph cannot be created.\nNo logs found.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        }   //  endif
                        logsTotal = LoadLogs(justSelectLogs, js.Species, sList);
                        currTitle.Remove(0, currTitle.Length);
                        if (whichLength == 16)
                            currTitle.Append("NUMBER OF 16-FOOT LOGS BY DIB CLASS -- ");
                        else if(whichLength == 32)
                            currTitle.Append("NUMBER OF 32-FOOT LOGS BY DIB CLASS -- ");
                        currTitle.Append(js.Species);
                        currTitle.Append("\nSale number: ");
                        currTitle.Append(currSaleNumber);
                        currTitle.Append("   Sale name:  ");
                        currTitle.Append(currSaleName);
                        gf.currTitle = currTitle.ToString();
                        gf.currXtitle = "DIB CLASS";
                        gf.currYtitle = "NUMBER OF LOGS";
                        gf.graphNum = 5;
                        gf.chartType = "BAR";
                        gf.logStockList = logsTotal;
                        gf.currSP = js.Species;
                        gf.fileName = fileName;
                        gf.currSaleName = currSaleName;
                        gf.ShowDialog();
                    }   //  end foreach loop on species
                    break;
                case "GR06":
                    //  pull just measured and cut trees
                    justMeasured = tList.FindAll(
                        delegate(TreeDO td)
                        {
                            return td.CountOrMeasure == "M" && td.SampleGroup.CutLeave == "C";
                        });
                    //  expand data and group by DBH class
                    expandData(justMeasured, sList);
                    currTitle.Remove(0, currTitle.Length);
                    currTitle.Append("DIAMETER DISTRIBUTION FOR THE SALE");
                    currTitle.Append("\nSale number: ");
                    currTitle.Append(currSaleNumber);
                    currTitle.Append("   Sale name:  ");
                    currTitle.Append(currSaleName);
                    gf.currTitle = currTitle.ToString();
                    gf.currXtitle = "DBH";
                    gf.currYtitle = "NUMBER OF TREES";
                    gf.graphNum = 6;
                    gf.chartType = "BAR";
                    gf.treeList = treesByDBH;
                    gf.fileName = fileName;
                    gf.currSaleName = currSaleName;
                    gf.ShowDialog();
                    break;
                case "GR07":
                    //  need by species
                    foreach (LCDDO js in justSpecies)
                    {
                        //  need species, measured and cut trees
                        justMeasured = tList.FindAll(
                            delegate(TreeDO td)
                            {
                                return td.CountOrMeasure == "M" && td.SampleGroup.CutLeave == "C" && 
                                            td.Species == js.Species;
                            });
                        //  expand data
                        expandData(justMeasured, sList);
                        //  load
                        currTitle.Remove(0,currTitle.Length);
                        currTitle.Append("DIAMETER DISTRIBUTION FOR SPECIES ");
                        currTitle.Append(js.Species);
                        currTitle.Append("\nSale number: ");
                        currTitle.Append(currSaleNumber);
                        currTitle.Append("   Sale name:  ");
                        currTitle.Append(currSaleName);
                        gf.currTitle = currTitle.ToString();
                        gf.currXtitle = "DBH";
                        gf.currYtitle = "NUMBER OF TREES";
                        gf.graphNum = 7;
                        gf.chartType = "BAR";
                        gf.treeList = treesByDBH;
                        gf.currSP = js.Species;
                        gf.fileName = fileName;
                        gf.currSaleName = currSaleName;
                        gf.ShowDialog();
                        treesByDBH.Clear();
                    }   //  end foreach species
                    break;
                case "GR08":
                    //  need by stratum
                    foreach (StratumDO s in sList)
                    {
                        //  need measured and cut trees for each stratum
                        justMeasured = tList.FindAll(
                            delegate(TreeDO td)
                            {
                                return td.CountOrMeasure == "M" && td.SampleGroup.CutLeave == "C" &&
                                            td.Stratum.Code == s.Code;
                            });
                        //  expand data
                        expandData(justMeasured, sList);
                        //  load
                        currTitle.Remove(0, currTitle.Length);
                        currTitle.Append("DIAMETER DISTRIBUTION BY STRATUM ");
                        currTitle.Append(s.Code);
                        currTitle.Append("\nSale number: ");
                        currTitle.Append(currSaleNumber);
                        currTitle.Append("   Sale name:  ");
                        currTitle.Append(currSaleName);
                        gf.currTitle = currTitle.ToString();
                        gf.currXtitle = "DBH";
                        gf.currYtitle = "NUMBER OF TREES";
                        gf.graphNum = 8;
                        gf.chartType = "BAR";
                        gf.treeList = treesByDBH;
                        gf.currSP = s.Code;
                        gf.fileName = fileName;
                        gf.currSaleName = currSaleName;
                        gf.ShowDialog();
                        treesByDBH.Clear();
                    }   //  end foreach stratum
                    break;
                case "GR09":
                    //  need values from TreEstimate table
                    //  if that table is empty, it means no 3P strata or the file was created
                    //  prior to March 2015 when the table was implemented.
                    List<TreeEstimateDO> treeEstimates = bslyr.getTreeEstimates();
                    if (treeEstimates.Count == 0)
                    {
                        MessageBox.Show("No estimate data is available for GR09.\nCannot produce this graph.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }   //  endif
                    List<TreeDO> uniqueSpecies = bslyr.GetUniqueSpecies();
                    List<CountTreeDO> cList = bslyr.getCountTrees();
                    List<CreateTextFile.ReportSubtotal> graphData = new List<CreateTextFile.ReportSubtotal>();
                    foreach (TreeDO u in uniqueSpecies)
                    {
                        //  pull all tree default value for current species
                        List<CountTreeDO> countSpecies = cList.FindAll(
                            delegate(CountTreeDO ct)
                            {
                                return ct.TreeDefaultValue_CN == u.TreeDefaultValue_CN;
                            });
                        //  accumulate all count tree cn in TreeEstimate for this species
                        foreach (CountTreeDO c in countSpecies)
                        {
                            List<TreeEstimateDO> justEst = treeEstimates.FindAll(
                                delegate(TreeEstimateDO te)
                                {
                                    return te.CountTree_CN == c.CountTree_CN;
                                });
                            if(justEst.Count > 0) buildKPIdata(justEst, graphData);
                        }   //  end foreach loop
                        //  reset categories based on Ken's logic
                        List<CreateTextFile.ReportSubtotal> categoryData = new List<CreateTextFile.ReportSubtotal>();
                        if (graphData.Count > 0)
                        {
                            resetCategories(graphData, categoryData);
                            //  load
                            currTitle.Remove(0, currTitle.Length);
                            currTitle.Append("KPI DISTRIBUTION BY SPECIES -- ");
                            currTitle.Append(u.Species);
                            currTitle.Append("\nSale number: ");
                            currTitle.Append(currSaleNumber);
                            currTitle.Append("   Sale name:  ");
                            currTitle.Append(currSaleName);
                            gf.currTitle = currTitle.ToString();
                            gf.currXtitle = "KPI";
                            gf.currYtitle = "NUMBER OF TREES";
                            gf.graphNum = 9;
                            gf.chartType = "BAR";
                            gf.graphData = categoryData;
                            gf.currSP = u.Species;
                            gf.fileName = fileName;
                            gf.currSaleName = currSaleName;
                            gf.ShowDialog();
                            graphData.Clear();
                            categoryData.Clear();
                        }   //  endif
                    }   //  end foreach loop
                    break;
                case "GR10":
                    List<CreateTextFile.ReportSubtotal> dataToGraph = new List<CreateTextFile.ReportSubtotal>();
                    //  need to loop by stratum
                    foreach (StratumDO s in sList)
                    {
                        if(s.BasalAreaFactor > 0)
                        {
                            //  Pull plots for this stratum
                            List<PlotDO> pList = bslyr.GetStrataPlots(s.Code);
                            double numplots = pList.Count;

                            //  Pull tree data for this stratum
                                List<TreeDO> treeList = bslyr.getTrees();
                            //  Pull stratum from LCD to get species in the stratum
                            List<LCDDO> lList = bslyr.getLCD();
                            List<LCDDO> justStratum = lList.FindAll(
                                delegate(LCDDO ld)
                                {
                                    return ld.Stratum == s.Code;
                                });
                            //  Then find number of trees for the stratum and each species in the stratum
                            foreach(LCDDO js in justStratum)
                            {
                                List<TreeDO> justTrees = treeList.FindAll(
                                    delegate(TreeDO td)
                                    {
                                        return td.Stratum.Code == s.Code && td.Species == js.Species &&
                                                td.SampleGroup.CutLeave == "C" && td.LiveDead == js.LiveDead;
                                    });

                                //  Load into graphData --  see if species is already in the list
                                int nthRow = dataToGraph.FindIndex(
                                    delegate(CreateTextFile.ReportSubtotal cr)
                                    {
                                        return cr.Value1 == js.Species;
                                    });
                                if (nthRow < 0)
                                {
                                    CreateTextFile.ReportSubtotal rs = new CreateTextFile.ReportSubtotal();
                                    rs.Value1 = js.Species;
                                    rs.Value3 = (s.BasalAreaFactor * justTrees.Count) / numplots;
                                    dataToGraph.Add(rs);
                                }
                                else if (nthRow >= 0)
                                {
                                    dataToGraph[nthRow].Value3 += (s.BasalAreaFactor * justTrees.Count) / numplots;
                                }   //  endif

                            }   //  end foreach loop
                            
                            //  load graphData
                            if(dataToGraph.Count > 0)
                            {
                                currTitle.Remove(0,currTitle.Length);
                                currTitle.Append("BAF PER ACRE BY SPECIES FOR STRATUM ");
                                currTitle.Append(s.Code);
                                currTitle.Append("\nSale Number:  ");
                                currTitle.Append(currSaleNumber);
                                currTitle.Append("\n");
                                currTitle.Append("   Sale Name:  ");
                                currTitle.Append(currSaleName);

                                gf.currTitle = currTitle.ToString();
                                gf.currXtitle = "";
                                gf.currYtitle = "";
                                gf.graphNum = 10;
                                gf.chartType = "PIE";
                                gf.currSP = s.Code;
                                gf.graphData = dataToGraph;
                                gf.fileName = fileName;
                                gf.currSaleName = currSaleName;
                                gf.ShowDialog();
                                dataToGraph.Clear();
                            }   //  endif
                        }   //  endif on basal area factor
                    }   //  end foreach loop on stratum
                    break;
                case "GR11":
                    //  pull data by sample group
                    List<SampleGroupDO> sgList = bslyr.getSampleGroups();
                    float numPlots = 0;
                    //  find all trees for each group
                    foreach (SampleGroupDO sg in sgList)
                    {
                        if (sg.Stratum.BasalAreaFactor > 0)
                        {
                            //  find all strata for this group in LCD
                            List<LCDDO> lList = bslyr.getLCD();
                            List<LCDDO> justStrata = lList.FindAll(
                                delegate(LCDDO l)
                                {
                                    return l.SampleGroup == sg.Code;
                                });
                            //  find number of plots for the strata
                            string currST = "*";
                            foreach (LCDDO js in justStrata)
                            {
                                if (currST != js.Stratum)
                                {
                                    List<PlotDO> pList = bslyr.GetStrataPlots(js.Stratum);
                                    numPlots += pList.Count();
                                    currST = js.Stratum;
                                }   //  endif
                            }   //  end foreach loop

                            // now find all trees for the sample group
                            justMeasured = tList.FindAll(
                                delegate(TreeDO td)
                                {
                                    return td.SampleGroup_CN == sg.SampleGroup_CN &&
                                        td.CountOrMeasure == "M" &&
                                        td.SampleGroup.CutLeave == "C" &&
                                        td.Stratum_CN == sg.Stratum_CN;
                                });
                                
                            //  load DIB classes
                            LoadDIBclass(justMeasured);

                            //  Accumulate number of trees for each class
                            foreach (TreeDO jm in justMeasured)
                            {
                                //  find class to update
                                int nthRow = findDIBindex(treesByDBH, jm.DBH);
                                if (nthRow >= 0)
                                    treesByDBH[nthRow].TreeCount++;
                            }   //  end foreach loop

                            //  Calculate
                            foreach (TreeDO tbd in treesByDBH)
                            {
                                //  Calculate value for each DIB class
                                tbd.TreeCount = (float)((sg.Stratum.BasalAreaFactor * tbd.TreeCount) / numPlots);
                            }   //  end foreach loop

                        //  load dat6a for gtraph
                        currTitle.Remove(0, currTitle.Length);
                        currTitle.Append("BAF PER ACRE FOR SAMPLE GROUP ");
                        currTitle.Append(sg.Code);
                        currTitle.Append("\nSaleNumber: ");
                        currTitle.Append(currSaleNumber);
                        currTitle.Append("\nSale name: ");
                        currTitle.Append(currSaleName);
                        gf.currTitle = currTitle.ToString();
                        gf.currXtitle = "DBH Class";
                        gf.currYtitle = "Basal Area Factor";
                        gf.graphNum = 11;
                        gf.chartType = "BAR";
                        gf.currSP = sg.Code;
                        gf.treeList = treesByDBH;
                        gf.fileName = fileName;
                        gf.currSaleName = currSaleName;
                        gf.ShowDialog();
                    }   //  endif
                }   //  end foreach loop
                break;
            }   //  end switch on report
            return;
        }   //  end createGraphs


        private void expandData(List<TreeDO> listToExpand, List<StratumDO> sList)
        {
            //  expands number of trees by DBH class on tree data
            double calcValue = 0;
            //  fill DBH classes
            LoadDIBclasses(listToExpand);

            //  Then loop through list, find DBH class and expand number of trees
            foreach (TreeDO lte in listToExpand)
            {
                //  need strata acres to complete value
                int nthRow = sList.FindIndex(
                    delegate(StratumDO s)
                    {
                        return s.Code == lte.Stratum.Code;
                    });
                double currAC = Utilities.ReturnCorrectAcres(lte.Stratum.Code,bslyr,(long)sList[nthRow].Stratum_CN);
                //if (sList[nthRow].Method == "3P")
                    // do what?
                //    calcValue = 0;
                //else calcValue = lte.ExpansionFactor * currAC;
                calcValue = lte.ExpansionFactor * currAC;
                //  find DBH class to load
                int mthRow = findDBHindex(treesByDBH, lte.DBH);
                treesByDBH[mthRow].ExpansionFactor += (float)calcValue;
            }   //  end foreach loop
            
            return;
        }   //  end expandData for tree list


        private void expandData(List<LCDDO> listToExpand, string valueToExpand, List<StratumDO> sList,
                                                    string currSP)
        {
            //  expands valueToExpand in LCD data
            double calcValue = 0;
            foreach (LCDDO lte in listToExpand)
            {
                //  need strata acres to complete value
                int nthRow = sList.FindIndex(
                    delegate(StratumDO s)
                    {
                        return s.Code == lte.Stratum;
                    });
                double currAC = Utilities.ReturnCorrectAcres(lte.Stratum, bslyr, (long)sList[nthRow].Stratum_CN);
                switch (valueToExpand)
                {
                    case "EXPFAC":
                        if (sList[nthRow].Method == "3P")
                            calcValue += lte.TalliedTrees;
                        else calcValue += lte.SumExpanFactor * currAC;
                        break;
                    case "VOL":
                        calcValue += lte.SumNCUFT * currAC;
                        break;
                }   //  end switch
            }   //  end foreach loop

            //  load list with calculated values
            LCDDO lcd = new LCDDO();
            lcd.Species = currSP;
            if (valueToExpand == "EXPFAC")
                lcd.SumExpanFactor = calcValue;
            else if (valueToExpand == "VOL")
                lcd.SumNCUFT = calcValue;
            speciesTotal.Add(lcd);
            calcValue = 0;
            return;
        }   //  end expandData for LCD data


        private void LoadDIBclasses(List<TreeDO> justMeasured)
        {
            float maxDBH = justMeasured.Max(j => j.DBH);
            maxDBH = (float)Math.Round(maxDBH);
            for (int k = 0; k <= maxDBH; k++)
            {
                TreeDO td = new TreeDO();
                td.DBH = k;
                treesByDBH.Add(td);
            }   //  end for k loop
            return;
        }   //  end LoadDIBclasses


        private void LoadDIBclass(List<TreeDO> justMeaqsured)
        {
            //  overloaded function for graph 11
            //  2-inch diameter classes
            float maxDBH = justMeasured.Max(j=> j.DBH);
            maxDBH = (float)Math.Round(maxDBH);
            for (int k = 0; k <= maxDBH; k += 2)
            {
                TreeDO td = new TreeDO();
                td.TreeCount++;
                td.DBH = k;
                treesByDBH.Add(td);
            }   //  end for loop
            return;
        }   ///  end LoadDIBclass


        private int findDBHindex(List<TreeDO> listToSearch, double currDBH)
        {
            float DBHtoFind = (float)Math.Round(currDBH);
            int rowToLoad = listToSearch.FindIndex(
                delegate(TreeDO lts)
                {
                    return lts.DBH == DBHtoFind;
                });
            return rowToLoad;
        }   //  end findDBHindex


        private int findDIBindex(List<TreeDO> listToSearch, double currDBH)
        {
            //  overloaded function for graph 11
            float DBHtoFind = 0;
            if ((int)currDBH % 2 == 0)
                DBHtoFind = (int)currDBH;
            else DBHtoFind = (int)(currDBH + 1.0);
            int rowToLoad = listToSearch.FindIndex(
                delegate(TreeDO lts)
                {
                    return lts.DBH == DBHtoFind;
                });
            return rowToLoad;
        }   //  end findDIBindex


        private void noDataForGraph(string currGroup, string whichGroup)
        {
            StringBuilder msgText = new StringBuilder();
            msgText.Append("No data found for ");
            msgText.Append(whichGroup);
            msgText.Append(" ");
            msgText.Append(currGroup);
            msgText.Append("\nCould not produce a graph.");
            MessageBox.Show(msgText.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }   //  end noDataForGraph


        private List<LogStockDO> LoadLogs(List<LogStockDO> justSixteens, string currSP, List<StratumDO> sList)
        {
            List<LogStockDO> returnLogs = new List<LogStockDO>();
            List<LogStockDO> justDIBs = bslyr.getLogDIBs();
            foreach (LogStockDO jd in justDIBs)
            {
                //  find all logs for DIB class
                List<LogStockDO> justLogs = justSixteens.FindAll(
                    delegate(LogStockDO l)
                    {
                        return l.DIBClass == jd.DIBClass;
                    });
                //  expand and sum
                double calcValue = 0;
                foreach(LogStockDO jl in justLogs)
                {
                    // find acres
                    int nthRow = sList.FindIndex(
                        delegate(StratumDO s)
                        {
                            return s.Code == jl.Tree.Stratum.Code;
                        });
                    double currAC = Utilities.ReturnCorrectAcres(jl.Tree.Stratum.Code, bslyr, (long)sList[nthRow].Stratum_CN);
                    calcValue += (float)jl.Tree.ExpansionFactor * currAC;
                }   //  end foreach loop
                //  load DIB class into report list
                if (calcValue > 0)
                {
                    LogStockDO ls = new LogStockDO();
                    ls.DIBClass = jd.DIBClass;
                    //  since Tree is not defined in this list, must put calculated value into
                    //  a log stock valied field such as net cubic.
                    ls.NetCubicFoot = (float)calcValue;
                    returnLogs.Add(ls);
                }   //  endif
            }   //  end foreach loop
            return returnLogs;
        }   //  end LoadLogs


        //  this works just for graph 9 -- by KPI
        private void buildKPIdata(List<TreeEstimateDO> KPIgroups, List<CreateTextFile.ReportSubtotal> graphData)
        {
            //  total up number of trees from tree estimate data for each KPI/species group
            foreach (TreeEstimateDO kg in KPIgroups)
            {
                // loop through these KPIs and store
                int nthRow = graphData.FindIndex(
                    delegate(CreateTextFile.ReportSubtotal r)
                    {
                        return r.Value4 == kg.KPI;
                    });
                if (nthRow == -1)
                {
                    //  add new group
                    CreateTextFile.ReportSubtotal rs = new CreateTextFile.ReportSubtotal();
                    rs.Value4 = kg.KPI;
                    rs.Value3++;
                    graphData.Add(rs);
                }
                else if(nthRow >= 0)       
                    graphData[nthRow].Value3++;
            }   //  end foreach loop
            return;
        }   //  end buildKPIdata

        //  this works for graph 9 and set specific categories for KPI data
        private void resetCategories(List<CreateTextFile.ReportSubtotal> graphData,
                                    List<CreateTextFile.ReportSubtotal> categoryData)
        {
            double[] scalingData = new double[4] { 0.25, 0.5, 1.0, 2.0 };
            //  create categories in categoryData
            double MaxKPI = graphData.Max(g => g.Value4);
            double MinKPI = graphData.Min(g => g.Value4);
            double dataRange = MaxKPI - MinKPI;
            double scaleInterval = Math.Floor(dataRange / 20);
            if (scaleInterval <= 0.0) scaleInterval = 1.0;
            //  first position in categoryData
            CreateTextFile.ReportSubtotal rs = new CreateTextFile.ReportSubtotal();
            rs.Value4 = Math.Floor((scaleInterval * scalingData[0] + MinKPI));
            categoryData.Add(rs);
            int prevRow = categoryData.Count - 1;
            //  then loop through to build remaining categories
            //  three times for first scaling factor
            for (int j = 0; j < 3; j++)
            {
                CreateTextFile.ReportSubtotal r1 = new CreateTextFile.ReportSubtotal();
                r1.Value4 = Math.Floor((scaleInterval * scalingData[0] + categoryData[prevRow].Value4));
                categoryData.Add(r1);
                prevRow++;
            }   //  end for j loop
            //  next group
            for (int j = 0; j < 4; j++)
            {
                CreateTextFile.ReportSubtotal r2 = new CreateTextFile.ReportSubtotal();
                r2.Value4 = Math.Floor((scaleInterval * scalingData[1] + categoryData[prevRow].Value4));
                categoryData.Add(r2);
                prevRow++;
            }   //  end for j loop
            //  third group
            for (int j = 0; j < 4; j++)
            {
                CreateTextFile.ReportSubtotal r3 = new CreateTextFile.ReportSubtotal();
                r3.Value4 = Math.Floor((scaleInterval * scalingData[2] + categoryData[prevRow].Value4));
                categoryData.Add(r3);
                prevRow++;
            }   //  end for j loop
            //  and last group
            for (int j = 0; j < 4; j++)
            {
                CreateTextFile.ReportSubtotal r4 = new CreateTextFile.ReportSubtotal();
                r4.Value4 = Math.Floor((scaleInterval * scalingData[3] + categoryData[prevRow].Value4));
                categoryData.Add(r4);
                prevRow++;
            }   //  end for j loop

            //  now accumulate graphData into categoryData
            foreach (CreateTextFile.ReportSubtotal gd in graphData)
            {
                for (int j = 0; j < categoryData.Count; j++)
                {
                    if (j == 0 && gd.Value4 <= categoryData[0].Value4)
                        categoryData[0].Value3 += gd.Value3;
                    else if(j == 15 && gd.Value4 > categoryData[j].Value4)
                        categoryData[j].Value3 += gd.Value3;
                    //else if (gd.Value4 >= categoryData[j].Value4 && gd.Value4 < categoryData[j + 1].Value4)
                    else if(gd.Value4 > categoryData[j].Value4)
                        categoryData[j].Value3 += gd.Value3;
                }   //  for k loop
            }   //  end foreach loop
            return;
        }   //  end resetCateogires
    }
}
