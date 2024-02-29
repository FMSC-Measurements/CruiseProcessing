using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL;
using System;

namespace CruiseProcessing
{
    public class CPbusinessLayer
    {
        protected string CruiseID { get; } // for v3 files
        public string FilePath { get; }
        public DAL DAL { get; }
        public CruiseDatastore_V3 DAL_V3 { get; }


        public CPbusinessLayer(DAL dal, CruiseDatastore_V3 dal_V3, string cruiseID)
        {
            if (DAL_V3 != null && string.IsNullOrEmpty(cruiseID)) { throw new InvalidOperationException("v3 DAL was set, expected CruiseID"); }

            DAL = dal;
            DAL_V3 = dal_V3;
            CruiseID = cruiseID; 
            FilePath = DAL.Path;
        }

        // *******************************************************************************************
        public string createNewTableQuery(string tableName, params string[] valuesList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(tableName);
            sb.Append(" (");

            //  add values to command
            foreach (string s in valuesList)
                sb.Append(s);

            //  add close parens and semi-colon
            sb.Append(");");

            return sb.ToString();
        }   //  end createNewTableQuery



        public string[] createExportArray()
        {
            //  create valuesList for export grade sorts and grade
            string[] exportDataArray = new string[8] {"exportSort TEXT,",
                                                      "exportGrade TEXT,",
                                                      "exportCode TEXT,",
                                                      "exportName TEXT,",
                                                      "minDiam TEXT,",
                                                      "minLength TEXT,",
                                                      "minBDFT TEXT,",
                                                      "maxDefect TEXT"};

            return exportDataArray;
        }   //  end createExportArray


        public string[] createLogMatrixArray()
        {
            //  create valuesList for log matrix output files for Region 10
            string[] logMatrixArray = new string[13] {"ReportNumber TEXT,",
                                                      "GradeDescription TEXT,",
                                                      "LogSortDescription TEXT,",
                                                      "Species TEXT,",
                                                      "LogGrade1 TEXT,",
                                                      "LogGrade2 TEXT,",
                                                      "LogGrade3 TEXT,",
                                                      "LogGrade4 TEXT,",
                                                      "LogGrade5 TEXT,",
                                                      "LogGrade6 TEXT,",
                                                      "SEDlimit TEXT,",
                                                      "SEDminimum REAL,",
                                                      "SEDmaximum REAL"};
            return logMatrixArray;
        }   //  end createLogMatrixArray


        public string[] createStewCostsArray()
        {
            //  create valuesList for stewardship product costs list for Region 2
            string[] stewCostsArray = new string[7] {"costUnit TEXT,",
                                                     "costSpecies TEXT,",
                                                     "costProduct TEXT,",
                                                     "costPounds REAL,",
                                                     "costCost REAL,",
                                                     "scalePC REAL,",
                                                     "includeInReport TEXT"};
            return stewCostsArray;
        }   //  end createStewCostsArray




        public int CreateNewTable(string tableName)
        {
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            string[] valuesList;
            string queryString = "";
            //  build query
            if (tableName == "ExportValues")
            {
                valuesList = createExportArray();
                queryString = createNewTableQuery(tableName, valuesList);
            }
            else if (tableName == "LogMatrix")
            {
                valuesList = createLogMatrixArray();
                queryString = createNewTableQuery(tableName, valuesList);
            }
            else if (tableName == "StewProductCosts")
            {
                valuesList = createStewCostsArray();
                queryString = createNewTableQuery("StewProductCosts", valuesList);

            }   //  endif 

            DAL.Execute(queryString);
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            // {
            //     sqlconn.Open();
            //     SQLiteCommand sqlcmd = sqlconn.CreateCommand();
            //     sqlcmd.CommandText = queryString;
            //     sqlcmd.ExecuteNonQuery();

            //     sqlconn.Close();
            // }   //  end using

            return 1;
        }   //  end createNewTable


        public bool doesTableExist(string tableName)
        {
            return DAL.CheckTableExists(tableName);
        }

        /*        public void ContainColumn(string ColumnName, string TableName)
                {
                    List<TreeCalculatedValuesDO> tcvList = DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues",null,null);

                    if (tcvList.Contains(TreeCalculatedValuesDO tipWood))
                    {
                    }
                    else
                    {
                        string query = "ALTER TABLE " + "ADD COLUMN " + ColumnName + " DOUBLE;";
                        dbc.updateTreeCalculatedValuestable(query);
                    }
                }   //  end ContainColumn
        */
        // *******************************************************************************************
        //  Gets on each table -- pulls all data from specified table
        public List<SaleDO> getSale()
        {
            return DAL.From<SaleDO>().Read().ToList();
        }   //  end getSale

        public List<StratumDO> getStratum()
        {
            return DAL.From<StratumDO>().OrderBy("Code").Read().ToList();
        }   //  end getStratum

        public List<CuttingUnitDO> getCuttingUnits()
        {
            return DAL.From<CuttingUnitDO>().OrderBy("Code").Read().ToList();
        }   //  end getCuttingUnits

        public List<CuttingUnitDO> getPaymentUnits()
        {
            return DAL.From<CuttingUnitDO>().GroupBy("PaymentUnit").Read().ToList();
        }   //  end getPaymentUnits

        public List<CuttingUnitDO> getLoggingMethods()
        {
            //  returns unique logging methods
            return DAL.From<CuttingUnitDO>().GroupBy("LoggingMethod").Read().ToList();
        }   //  end getLoggingMethods

        public List<CuttingUnitStratumDO> getCuttingUnitStratum(long currStratumCN)
        {
            return DAL.From<CuttingUnitStratumDO>().Where("Stratum_CN = @p1").Read(currStratumCN).ToList();
        }   //  end getCuttingUnitStratum


        public List<PlotDO> getPlots()
        {
            return DAL.From<PlotDO>().OrderBy("PlotNumber").Read().ToList();
        }   //  end getPlots

        public List<SampleGroupDO> getSampleGroups()
        {
            return DAL.From<SampleGroupDO>().Read().ToList();
        }   //  end getSampleGroups

        public List<TreeDefaultValueDO> getTreeDefaults()
        {
            return DAL.From<TreeDefaultValueDO>().Read().ToList();
        }   //  end getTreeDefaults

        public List<CountTreeDO> getCountTrees()
        {
            return DAL.From<CountTreeDO>().Read().ToList();
        }   //  end getCountTrees

        public List<TreeDO> getTrees()
        {
            return DAL.From<TreeDO>().Read().ToList();
        }   //  end getTrees

        public List<LogDO> getLogs()
        {
            return DAL.From<LogDO>().Read().ToList();
        }   //  end getLogs

        public List<LogStockDO> getLogStock()
        {
            return DAL.From<LogStockDO>().Read().ToList();
        }   //  end getLogStock

        public List<VolumeEquationDO> getVolumeEquations()
        {
            return DAL.From<VolumeEquationDO>().Read().ToList();
        }   //  end getVolumeEquations

        public List<ValueEquationDO> getValueEquations()
        {
            return DAL.From<ValueEquationDO>().Read().ToList();
        }   //  end getValueEquations


        public List<BiomassEquationDO> getBiomassEquations()
        {
            return DAL.From<BiomassEquationDO>().Read().ToList();
        }   //  end getBiomassEquations

        public List<QualityAdjEquationDO> getQualAdjEquations()
        {
            return DAL.From<QualityAdjEquationDO>().Read().ToList();
        }   //  end getQualAdjEquations


        public List<TreeEstimateDO> getTreeEstimates()
        {
            return DAL.From<TreeEstimateDO>().Read().ToList();
        }   //  end getTreeEstimates

        //  TreeCalculatedValues
        // *******************************************************************************************
        public List<TreeCalculatedValuesDO> getTreeCalculatedValues()
        {
            return DAL.From<TreeCalculatedValuesDO>().Read().ToList();
        }   //  end getTreeCalculatedValues

        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Where("t.Stratum_CN = @p1")
                .Read(currStratumCN).ToList();
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN, int currUnitCN)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Where("t.Stratum_CN = @p1 AND t.CuttingUnit_CN = @p2")
                .Read(currStratumCN, currUnitCN)
                .ToList();
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(string currSP)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Where("t.Species = @p1")
                .Read(currSP).ToList();
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN, string orderBy)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Where("t.Stratum_CN = @p1")
                .OrderBy(orderBy)
                .Read(currStratumCN)
                .ToList();
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(string currCL, string orderBy, string reportType)
        {
            // TODO remove param reportType
            // rewritten Dec 2020 - Ben
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)") 
                .Where("sg.CutLeave = @p1")
                .OrderBy(orderBy)
                .Read(currCL)
                .ToList();
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getStewardshipTrees(string currUnit, string currSP, string currPP)
        {
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("CuttingUnit AS cu", "USING (CuttingUnit_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = @p1 AND cu.Code = @p2 AND t.Species = @p3 AND sg.PrimaryProduct = @p4")
                .Read("C", currUnit, currSP, currPP)
                .ToList();
        }   //  end getStewardshipTrees


        public List<TreeCalculatedValuesDO> getRegressTrees(string currSP, string currPR, string currLD, string currCM)
        {
            return DAL.From<TreeCalculatedValuesDO>()
               .Join("Tree AS t", "USING (Tree_CN)")
               .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
               .Where("t.Species = @p1 ANd sg.PrimaryProduct = @p2 AND t.LiveDead = @p3 AND t.CountOrMeasure = @p4")
               .Read(currSP, currPR, currLD, currCM).ToList();
        }   //  end getRegressTrees


        //  Log Table and Log Stock Table
        // *******************************************************************************************
        public List<LogDO> getTreeLogs(long currTreeCN)
        {
            return DAL.From<LogDO>()
                .Where("Tree_CN = @p1")
                .Read(currTreeCN).ToList();
        }   //  getTreeLogs


        public List<LogDO> getTreeLogs()
        {
            // TODO check calling code, original query should have thrown exception
            //StringBuilder sb = new StringBuilder();
            //sb.Clear();
            //sb.Append("JOIN Log ON Log.Tree_CN = Tree.Tree_CN");
            return DAL.From<LogDO>().Read().ToList();
        }   //  end getTreeLogs


        public List<LogStockDO> getLogStockSorted()
        {
            return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("Stratum AS st", "USING (Stratum_CN)")
                .LeftJoin("Plot AS p", "USING (Plot_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Join("CuttingUnit AS cu", "USING (CuttingUnit_CN)")
                .Where("sg.CutLeave = 'C'")
                .OrderBy("st.Code", "CAST(cu.Code AS NUMERIC)", "p.PlotNumber")
                .Read().ToList();
        }   //  end getLogStockSorted



        public List<LogStockDO> getCullLogs(long currTreeCN, string currGrade)
        {
            return DAL.From<LogStockDO>()
                .Where("Tree_CN = @p1 AND Grade = @p2")
                .Read(currTreeCN, currGrade).ToList();
        }   //  end getCullLogs


        public List<LogStockDO> getLogDIBs()
        {
            return DAL.From<LogStockDO>().GroupBy("DIBClass").Read().ToList();
        }   //  end getLogDIBS


        public List<LogStockDO> getCutOrLeaveLogs(string currCL)
        {
            //  Pulls logs for Region 10 L1 output files
            return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = @p1")
                .OrderBy("Species")
                .Read(currCL).ToList();
        }   //  end getCutLogs


        public List<LogStockDO> getCutLogs()
        {
            //  Pulls just cut logs
            return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = 'C'")
                .OrderBy("Species")
                .Read().ToList();
        }   //  end getCutLogs


        public List<LogStockDO> getCutLogs(string currCL, string currSP, string currPP, int byDIBclass)
        {
            //  Needed for R501 report
            if (byDIBclass == 1)
            {
                return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = @p1 AND t.Species = @p2 AND sg.PrimaryProduct = @p3")
                .GroupBy("DIBClass")
                .Read(currCL, currSP, currPP).ToList();
            }
            else
            {
                return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = @p1 AND t.Species = @p2 AND sg.PrimaryProduct = @p3")
                .Read(currCL, currSP, currPP).ToList();
            }
        }   //  end getCutLogs


        public List<LogStockDO> getCutLogs(string currSP, string currSort, string currGrade)
        {
            // TODO check calling code. original quiry should have thrown error

            //StringBuilder sb = new StringBuilder();
            //sb.Clear();
            //sb.Append("JOIN LogStock ON LogStock.Tree_CN = Tree.Tree_CN ");
            //sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            //sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.Species = ? ");
            //sb.Append("AND LogStock.ExportGrade = ? AND LogStock.Grade = ?");

            //  used in EX reports
            return DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
                .Where("sg.CutLeave = 'C' AND t.Species = @p1 AND LogStock.ExportGrade = @p2 AND LogStock.Grade = @p3")
                .Read(currSP, currSort, currGrade)
                .ToList();

        }   //  end getCutLogs


        public List<LogStockDO> getLogSorts(string currSP)
        {
            // TODO check calling code. original quiry should have thrown error
            //StringBuilder sb = new StringBuilder();
            //sb.Append("JOIN LogStock ON LogStock.Tree_CN = Tree.Tree_CN ");
            //sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            //if (currSP != "")
            //    sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.Species = ? ");
            //else sb.Append("WHERE SampleGroup.CutLeave = ?");
            //sb.Append("GROUP BY ExportGrade,Grade");
            //if (currSP != "")
            //    return DAL.Read<LogStockDO>("LogStock", sb.ToString(), "C", currSP);
            //else return DAL.Read<LogStockDO>("LogStock", sb.ToString(), "C");

            //  used in EX reports
            var qb = DAL.From<LogStockDO>()
                .Join("Tree AS t", "USING (Tree_CN)")
                .Join("SampleGroup AS sg", "USING (SampleGroup_CN)");

            if (currSP != null)
            { qb.Where("SampleGroup.CutLeave = @p1 AND Tree.Species = @p2"); }
            else
            { qb.Where("SampleGroup.CutLeave = @p1"); }
            qb.GroupBy("ExportGrade", "Grade");

            return qb.Read("C", currSP).ToList();
        }   //  end getLogSorts


        public List<LogStockDO> getLogSpecies(string currSP, float minDIB, float maxDIB,
                                              string currST, string currGrade)
        {
            //  used mostly for BLM09 and BLM10 reports
            return DAL.From<LogStockDO>()
               .Join("Tree AS t", "USING (Tree_CN)")
               .Join("Stratum AS st", "USING (Stratum_CN)")
               .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
               .Where("sg.CutLeave = 'C' AND st.Code = @p1 AND t.CountOrMeasure = 'M AND t.Species = @p2 AND LogStock.Grade = @p3 AND DIBClass >= @p4 AND DIBClass <= @p5")
               .Read(currST, currSP, currGrade, minDIB, maxDIB).ToList();
        }   //  end getLogSpecies


        public List<LogStockDO> getStrataLogs(string currST, string currGrade)
        {
            //  used mostly for BLM reports
            return DAL.From<LogStockDO>()
               .Join("Tree AS t", "USING (Tree_CN)")
               .Join("Stratum AS st", "USING (Stratum_CN)")
               .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
               .Where("sg.CutLeave = 'C' AND st.Code = @p1 AND t.CountOrMeasure = 'M AND LogStock.Grade = @p3")
               .Read(currST, currGrade).ToList();
        }   //  end getStrataLogs



        public List<LogStockDO> getStrataLogs(string currSP, string currST, string currSG, string currSTM, string currGrade)
        {

            //StringBuilder sb = new StringBuilder();
            //sb.Clear();
            //sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            //sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            //sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            //sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            //sb.Append("WHERE SampleGroup.CutLeave = 'C' AND CountOrMeasure = 'M' AND STM = ? ");
            //sb.Append("AND Stratum.Code = ? AND Species = ? AND SampleGroup.Code = ? AND ");
            //sb.Append("LogStock.Grade = ?");

            //  used in BLM reports
            return DAL.From<LogStockDO>()
               .Join("Tree AS t", "USING (Tree_CN)")
               .Join("Stratum AS st", "USING (Stratum_CN)")
               .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
               .Join("CuttingUnit AS cu", "USING (CuttingUnit_CN)")
               .Where("sg.CutLeave = 'C' AND CountOrMeasure = 'M' AND STM = @p1 AND st.Code = @p2 AND t.Species = @p3 AND sg.Code =  @p4 AND LogStock.Grade = @p5")
               .Read(currSTM, currST, currSP, currSG, currGrade)
               .ToList();

        }   //  end getStrataLogs


        public List<LogStockDO> getUnitLogs(long currST_CN, long currCU_CN, string currGrade, string currSTM)
        {
            // mostly for BLM reports
            return DAL.From<LogStockDO>()
               .Join("Tree AS t", "USING (Tree_CN)")
               .Join("Stratum AS st", "USING (Stratum_CN)")
               .Join("SampleGroup AS sg", "USING (SampleGroup_CN)")
               .Join("CuttingUnit AS cu", "USING (CuttingUnit_CN)")
               .Where("sg.CutLeave = 'C' AND st.Stratum_CN = @p1 AND cu.CuttingUnit_CN = @p2 AND LogStock.Grade = @p3 AND t.STM = @p4")
               .Read(currST_CN, currCU_CN, currGrade, currSTM)
               .ToList();
        }   //  end getUnitLogs


        //  Error Log Table
        // *******************************************************************************************
        public List<ErrorLogDO> getErrorMessages(string errLevel, string errProgram)
        {
            List<ErrorLogDO> errLog = new List<ErrorLogDO>();
            if (errProgram == "FScruiser")
                return DAL.From<ErrorLogDO>()
                    .Where("Level = @p1 AND (Program LIKE'%FScruiser%' OR Program LIKE '%CruiseSystemManager%' OR Program LIKE '%CruiseManage%')")
                    .Read(errLevel)
                    .ToList();
            else if (errProgram == "CruiseProcessing")
                return DAL.From<ErrorLogDO>()
                    .Where("Level = @p1 AND Program = @p2")
                    .Read(errLevel, errProgram)
                    .ToList();

            return errLog;
        }   //  end getErrorMessages


        //  LCD POP and PRO Tables
        // *******************************************************************************************
        public List<LCDDO> getLCD()
        {
            return DAL.From<LCDDO>().Read().ToList();
        }   //  end getLCD

        public List<POPDO> getPOP()
        {
            return DAL.From<POPDO>().Read().ToList();
        }   //  end getPOP

        public List<PRODO> getPRO()
        {
            return DAL.From<PRODO>().Read().ToList();
        }   //  end getPRO

        //  Miscellaneous tables like log matrix, regression results
        // *******************************************************************************************
        public List<LogMatrixDO> getLogMatrix(string currReport)
        {
            return DAL.From<LogMatrixDO>().Where("ReportNumber = @p1").Read(currReport).ToList();
        }   //  end getLogMatrix


        public List<RegressionDO> getRegressionResults()
        {
            //  pulls regression data for local volume reports
            return DAL.From<RegressionDO>().Read().ToList();
        }   //  end getRegressionResults


        //  functions returning single elements
        // *******************************************************************************************
        //  like region
        public string getRegion()
        {
            //  retrieve sale record
            List<SaleDO> saleList = getSale();
            return saleList[0].Region;
        }   //  end getRegion

        //  forest
        public string getForest()
        {
            List<SaleDO> saleList = getSale();
            return saleList[0].Forest;
        }   //  end getForest

        //  district
        public string getDistrict()
        {
            List<SaleDO> saleList = getSale();
            return saleList[0].District;
        }   //  end getDistrict

        public string getCruiseNumber()
        {
            List<SaleDO> saleList = getSale();
            return saleList[0].SaleNumber;
        }   //  end getCruiseNumber

        public string getUOM(int currStratumCN)
        {
            List<SampleGroupDO> sgList = DAL.From<SampleGroupDO>()
                .Where("Stratum_CN = @p1")
                .Read(currStratumCN)
                .ToList();

            return sgList[0].UOM;
        }   //  end getUOM

        //  Sample Groups
        // *******************************************************************************************
        public List<SampleGroupDO> getSampleGroups(int currStratumCN)
        {
            return DAL.From<SampleGroupDO>()
                .Where("Stratum_CN = @p1")
                .Read(currStratumCN)
                .ToList();

        }   //  end getSampleGroups

        public List<SampleGroupDO> getSampleGroups(string qString)
        {
            // TODO calls to this function need to be rewritten. The qString param isn't needed
            // return DAL.Read<SampleGroupDO>("SampleGroup", qString, "C");

            //  returns just unique sample groups
            return DAL.From<SampleGroupDO>().Where("CutLeave = 'C'").GroupBy("Code").Read().ToList();
        }   //  end getSampleGroups


        // *******************************************************************************************
        //  Like variable log length from global configuration
        public string getVLL()
        {
            List<GlobalsDO> globList = DAL.From<GlobalsDO>().Where("Key = @p1 AND Block = @p2").Read("VLL", "Global").ToList();
            if (globList.Count > 0)
                return "V";
            else return "false";
        }   //  end getVLL


        //  Stratum table
        // *******************************************************************************************
        public List<StratumDO> GetCurrentStratum(string currentStratum)
        {
            //  Pull current stratum from table
            return DAL.From<StratumDO>().Where("Code = @p1").Read(currentStratum).ToList();
        }   //  end GetCurrentStratum


        //  just FIXCNT methods
        public List<StratumDO> justFIXCNTstrata()
        {
            return DAL.From<StratumDO>().Where("Method = @p1").Read("FIXCNT").ToList();
        }   //  end justFIXCNTstrata


        //  Plot table
        // *******************************************************************************************
        public List<PlotDO> GetStrataPlots(string currStratum)
        {
            return DAL.From<PlotDO>()
                .Join("Stratum AS st", "USING (Stratum_CN)")
                .Where("st.Code = @p1")
                .Read(currStratum)
                .ToList();
        }   //  end GetStrataPlots


        public List<PlotDO> getPlotsOrdered()
        {
            return DAL.From<PlotDO>()
                .Join("Stratum AS st", "USING (Stratum_CN)")
                .Join("CuttingUnit AS cu", "USING (CuttingUnit_CN)")
                .OrderBy("st.Code", "cu.Code", "PlotNumber")
                .Read().ToList();
        }   //  end getPlotsOrdered


        //  LCD table
        // *******************************************************************************************
        public List<LCDDO> getLCDOrdered(string searchString, string orderBy, string currCutLeave,
                                            string currST, string currPP)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(searchString);
            sb.Append(orderBy);
            if (currST == "")
                return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currCutLeave }).ToList();
            else if (currCutLeave == "")
                return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currST }).ToList();
            else if (currCutLeave != "" && currST != "" && currPP != "")
                return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currCutLeave, currST, currPP }).ToList();
            else return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currCutLeave, currST }).ToList();
        }   //  end getLCDOrdered


        public List<LCDDO> getLCDOrdered(string searchString, string orderBy, string currCutLeave,
                                            string currPP)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(searchString);
            sb.Append(orderBy);
            if (currPP != "")
                return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currCutLeave, currPP }).ToList();
            else return DAL.Read<LCDDO>("SELECT * FROM LCD " + sb.ToString() + ";", new object[] { currCutLeave }).ToList();
        }   //  end getLCDOrdered


        public List<LCDDO> GetLCDgroup(string currST, int currRPT, string currCL)
        {
            // rewritten Dec 2020 - Ben

            StringBuilder sb = new StringBuilder();;
            switch (currRPT)
            {
                case 1:         //  VSM1
                    sb.Append("SELECT Stratum,Species,PrimaryProduct,SecondaryProduct,UOM,LiveDead,CutLeave,Yield,SampleGroup,");
                    sb.Append("TreeGrade,ContractSpecies,STM FROM LCD WHERE Stratum = '");
                    sb.Append(currST);
                    sb.Append("';");
                    break;
                case 2:         //  VSM2
                    sb.Append("SELECT Stratum,PrimaryProduct,SecondaryProduct,UOM,SampleGroup,CutLeave,STM ");
                    sb.Append("FROM LCD WHERE Stratum = '");
                    sb.Append(currST);
                    sb.Append("' AND CutLeave = '");
                    sb.Append(currCL);
                    sb.Append("' GROUP BY SampleGroup,STM;");
                    break;
                case 3:         // VSM3
                    sb.Append("SELECT Stratum,PrimaryProduct,SecondaryProduct,UOM FROM LCD WHERE Stratum = '");
                    sb.Append(currST);
                    sb.Append("' AND CutLeave = '");
                    sb.Append(currCL);
                    sb.Append("' GROUP BY Stratum,PrimaryProduct;");
                    break;
                case 4:         //  WT1
                    sb.Append("SELECT Species,PrimaryProduct,SecondaryProduct,LiveDead,ContractSpecies from LCD WHERE CutLeave = '");
                    sb.Append("C");
                    sb.Append("' GROUP BY Species,PrimaryProduct,SecondaryProduct,LiveDead,ContractSpecies");
                    break;
                case 5:         //  WT4
                    sb.Append("SELECT Species FROM LCD WHERE CutLeave='C' GROUP BY Species");
                    break;
            }   //  end switch on report number

            return DAL.Query<LCDDO>(sb.ToString()).ToList();
        }   //  end GetLCDgroup

        public List<LCDDO> GetLCDgroup(string fileName, string groupedBy)
        {
            // rewritten Dec 2020 - Ben

            //  overloaded for stat report  ST3

            return DAL.Query<LCDDO>("SELECT Stratum,PrimaryProduct,SecondaryProduct,UOM FROM LCD WHERE CutLeave = 'C' GROUP BY " + groupedBy).ToList();
        }   //  end GetLCDgroup


        public List<LCDDO> GetLCDdata(string whereClause, LCDDO lcd, int reportNum, string currCL)
        {
            //  report number key
            //  1 = VSM1
            //  2 = VSM2
            //  3 = VSM3
            //  4 = WT1
            //  5 = R301

            var commandText = "SELECT * FROM LCD " + whereClause + ";";

            switch (reportNum)
            {
                case 1:
                    return DAL.Read<LCDDO>(commandText, new object[] { lcd.Stratum, lcd.Species, lcd.PrimaryProduct, lcd.UOM,
                                            lcd.LiveDead, lcd.CutLeave, lcd.Yield, lcd.SampleGroup, lcd.TreeGrade, lcd.STM }).ToList();
                //  September 2016 -- dropping contract species from LCD identifier
                //lcd.ContractSpecies, lcd.STM);
                case 2:
                    return DAL.Read<LCDDO>(commandText, new object[] { lcd.Stratum, lcd.PrimaryProduct, lcd.UOM, currCL,
                                                lcd.SampleGroup, lcd.STM }).ToList();
                case 3:
                    return DAL.Read<LCDDO>(commandText, new object[] { lcd.Stratum, lcd.PrimaryProduct, lcd.UOM, currCL, lcd.STM }).ToList();
                case 4:
                    return DAL.Read<LCDDO>(commandText, new object[] { lcd.Species, lcd.PrimaryProduct, lcd.SecondaryProduct,
                                                lcd.LiveDead, lcd.ContractSpecies, "C" }).ToList();
                case 5:
                    return DAL.Read<LCDDO>(commandText, new object[] { "C", lcd.Species, lcd.PrimaryProduct, lcd.ContractSpecies }).ToList();
            }   //  end switch
            List<LCDDO> emptyList = new List<LCDDO>();
            return emptyList;
        }   //  end GetLCDdata


        public List<LCDDO> GetLCDdata(string currST, string whereClause, string orderBy)
        {
            var commandText = "SELECT * FROM LCD " + whereClause + ";";

            //  works for UC1/UC2 reports -- maybe all UC reports?
            return DAL.Read<LCDDO>(commandText, new object[] { currST, "C", orderBy }).ToList();
        }   //  end GetLCDdata


        public List<LCDDO> GetLCDdata(string whereClause, string currST)
        {
            var commandText = "SELECT * FROM LCD " + whereClause + ";";

            //  works for regional report R104
            return DAL.Read<LCDDO>(commandText, new[] { currST }).ToList();
        }   //  end GetLCDdata


        public List<TreeCalculatedValuesDO> GetLCDtrees(string currST, LCDDO ldo, string currCM)
        {
            //  captures tree calculated values for LCD summation in SumAll

            //  September 2016 -- per K.Cormier -- dropping contract species from LCD identifier
            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree", "USING (Tree_CN)")
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Join("TreeDefaultValue", "USING (TreeDefaultValue_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .Where("Stratum.Code = @p1 AND SampleGroup.CutLeave = @p2 AND SampleGroup.Code = @p3 " +
                        "AND Tree.Species = @p4 AND SampleGroup.PrimaryProduct = @p5 AND " +
                        "SampleGroup.SecondaryProduct = @p6 AND SampleGroup.UOM = @p7 AND " +
                        "Tree.LiveDead = @p8 AND Tree.Grade = @p9 AND Stratum.YieldComponent = @p10 AND " +
                        "Tree.STM = @p11 and Tree.CountOrMeasure = @p12")
                .Read(currST, ldo.CutLeave,
                        ldo.SampleGroup, ldo.Species, ldo.PrimaryProduct, ldo.SecondaryProduct,
                        //  see comment above
                        //ldo.UOM, ldo.LiveDead, ldo.TreeGrade, ldo.Yield, ldo.ContractSpecies, 
                        ldo.UOM, ldo.LiveDead, ldo.TreeGrade, ldo.Yield,
                        ldo.STM, currCM).ToList();
        }   //  end GetLCDtrees

        // POP table
        // *******************************************************************************************
        public List<TreeCalculatedValuesDO> GetPOPtrees(POPDO pdo, string currST, string currCM)
        {
            //  captures tree calculated values for POP summation in SumAll

            return DAL.From<TreeCalculatedValuesDO>()
                .Join("Tree", "USING (Tree_CN)")
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .Where("Stratum.Code = @p1 AND SampleGroup.CutLeave = @p2 AND SampleGroup.Code = @p3 " +
                "AND SampleGroup.PrimaryProduct = @p4 AND SampleGroup.SecondaryProduct = @p5 AND " +
                "Tree.STM = @p6 AND Tree.CountOrMeasure = @p7")
                .Read(currST, pdo.CutLeave, pdo.SampleGroup, pdo.PrimaryProduct,
                        pdo.SecondaryProduct, pdo.STM, currCM).ToList();
        }   //  end GetPOPtrees


        public List<POPDO> GetUOMfromPOP()
        {
            return DAL.From<POPDO>().GroupBy("UOM").Read().ToList();
        }   //  end GetUOMfromPOP


        //  PRO table
        // *******************************************************************************************
        public List<PRODO> getPROunit(string currCU)
        {
            return DAL.From<PRODO>().Where("CuttingUnit = @p1").Read(currCU).ToList();
        }   //  end getPRO


        //  Count table
        // *******************************************************************************************
        public List<CountTreeDO> getCountTrees(long currSG_CN)
        {
            return DAL.From<CountTreeDO>().Where("SampleGroup_CN = @p1").Read(currSG_CN).ToList();
        }   //  end overload of getCountTrees


        public List<CountTreeDO> getCountsOrdered()
        {
            return DAL.From<CountTreeDO>().OrderBy("CuttingUnit_CN").Read().ToList();
        }   //  end getCountsOrdered


        public List<CountTreeDO> getCountsOrdered(string searchString, string orderBy, string[] searchValues)
        {
            return DAL.Read<CountTreeDO>("SELECT * FROM CountTree " + searchString + orderBy + ";", searchValues).ToList();
        }   //  end getCountsOrdered

        //  Tree table
        // *******************************************************************************************
        public List<TreeDO> getTreesOrdered(string searchString, string orderBy, string[] searchValues)
        {
            return DAL.Read<TreeDO>("SELECT * FROM Tree " + searchString + orderBy + ";", searchValues).ToList();
        }   //  end getTreesOrdered


        public List<TreeDO> getTreesSorted()
        {
            return DAL.From<TreeDO>()
                .Join("CuttingUnit", "USING (CuttingUnit_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .LeftJoin("Plot", "USING (Plot_CN)")
                .OrderBy("Stratum.Code", "CuttingUnit.Code", "ifnull(Plot.PlotNumber, 0)", "Tree.TreeNumber")
                .Read().ToList();
        }   //  end getTreesSorted


        public List<TreeDO> getUnitTrees(long currST_CN, long currCU_CN)
        {
            return DAL.From<TreeDO>()
                .Join("CuttingUnit", "USING (CuttingUnit_CN)")
                .Join("Stratum", "USING (Stratum_CN)")
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Where("SampleGroup.CutLeave = 'C' AND Tree.CountOrMeasure = 'M' AND Tree.Stratum_CN = @p1 AND Tree.CuttingUnit_CN = @p2")
                .Read(currST_CN, currCU_CN).ToList();
        }   //  end getUnitTrees


        public List<TreeDO> getUniqueStewardGroups()
        {
            return DAL.From<TreeDO>()
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Where("SampleGroup.CutLeave = 'C'")
                .GroupBy("CuttingUnit_CN", "Species", "Tree.SampleGroup_CN")
                .Read().ToList();
        }   //  end getUniqueStewardGroups


        public List<TreeDO> getTreeDBH(string currCL)
        {
            //  works for stand tables DIB classes for the sale

            return DAL.From<TreeDO>()
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Where(" SampleGroup.CutLeave = @p1 AND Tree.CountOrMeasure = 'M'")
                .GroupBy("DBH")
                .Read(currCL).ToList();

        }   //  end getTreeDBH


        public List<TreeDO> getTreeDBH(string currCL, string currST, string currCM)
        {
            //  works for stand tables DIB classes for current stratum

            return DAL.From<TreeDO>()
                .Join("Stratum", "USING (Stratum_CN)")
                .Join("SampleGroup", "USING (SampleGroup_CN)")
                .Where(" SampleGroup.CutLeave = @p1 AND Stratum.Code = @p2 AND Tree.CountOrMeasure = @p3")
                .GroupBy("DBH")
                .Read(currCL, currST, currCM).ToList();
        }   //  end getTreeDBH


        public List<TreeDO> JustMeasuredTrees()
        {
            return DAL.From<TreeDO>().Where("CountOrMeasure = 'M'").Read().ToList();
        }   //  end JustMeasuredTrees


        public List<TreeDO> JustMeasuredTrees(long currST_CN)
        {
            return DAL.From<TreeDO>().Where("CountOrMeasure = 'M' AND Stratum_CN = @p1").Read(currST_CN).ToList();
        }   //  end overloaded JustMeasuredTrees


        public List<TreeDO> JustUnitTrees(long currST_CN, long currCU_CN)
        {
            return DAL.From<TreeDO>().Where("Stratum_CN = @p1 AND CuttingUnit_CN = @p2").Read(currST_CN, currCU_CN).ToList();
        }   //  end JustUnitTrees

        public List<TreeDO> JustFIXCNTtrees(long currST_CN)
        {
            return DAL.From<TreeDO>().Where("CountOrMeasure = 'C' AND Stratum_CN = @p1").Read(currST_CN).ToList();
        }   //  end JustFIXCNTtrees


        //  Save data
        // *******************************************************************************************
        public void SaveTreeCalculatedValues(List<TreeCalculatedValuesDO> tcvList)
        {
            foreach (TreeCalculatedValuesDO tcv in tcvList)
            {
                if (tcv.DAL == null)
                {
                    tcv.DAL = DAL;
                }
                tcv.Save();
            }   //  end foreach loop
            return;
        }   //  end SaveTreeCalculatedValues


        public void SaveTrees(List<TreeDO> tList)
        {
            foreach (TreeDO tdo in tList)
            {
                if (tdo.DAL == null)
                {
                    tdo.DAL = DAL;
                }
                tdo.Save();
            }   //  end foreach loop
            return;
        }   //  end SaveTrees


        public void SaveLogStock(List<LogStockDO> lsList, int totLogs)
        {
            LogStockDO ls = new LogStockDO();
            for (int k = 0; k < totLogs; k++)
            {
                ls = lsList[k];
                if (ls.DAL == null)
                {
                    ls.DAL = DAL;
                }   //  endif
                ls.Save();

            }   //  end for k loop
            return;
        }   //  end SaveLogStock



        public void SaveLogMatrix(List<LogMatrixDO> lmList, string currReport)
        {
            List<LogMatrixDO> currLogMatrix = getLogMatrix(currReport);
            //  same problem as with volume equations below.
            // need to delete current report form log matrix before saving
            if (currReport != "")
            {
                if (currLogMatrix.Count > 0)
                {
                    foreach (LogMatrixDO lmd in currLogMatrix)
                    {
                        if (lmd.ReportNumber == currReport)
                            lmd.Delete();
                    }   //  end foreach loop
                }   //  endif
            }   //  endif
            foreach (LogMatrixDO lmx in lmList)
            {
                if (lmx.DAL == null)
                {
                    lmx.DAL = DAL;
                }   //  endif
                lmx.Save();
            }   //  end foreach loop           
            return;
        }   //  end SaveLogMatrix


        public void SaveVolumeEquations(List<VolumeEquationDO> volumeEquationList)
        {

            //  need to delete equations in order to update the database
            //  not sure why this has to happen but the way the DAL save works
            //  is if the user deleted an equation, the Save does not consider that
            //  when the equation list is updated ???????

            List<VolumeEquationDO> volList = getVolumeEquations();
            foreach (VolumeEquationDO vdo in volList)
            {
                int nthRow = volumeEquationList.FindIndex(
                    delegate (VolumeEquationDO ve)
                    {
                        return ve.VolumeEquationNumber == vdo.VolumeEquationNumber && ve.Species == vdo.Species &&
                                ve.PrimaryProduct == vdo.PrimaryProduct;
                    });
                if (nthRow < 0)
                {
                    vdo.Delete();
                }//end if
            }   //  end foreach loop

            foreach (VolumeEquationDO veq in volumeEquationList)
            {
                if (veq.DAL == null)
                {
                    veq.DAL = DAL;
                }
                veq.Save();
            }   //  end foreach loop


            return;
        }   //  end SaveVolumeEquations


       
        public void SaveValueEquations(List<ValueEquationDO> valList)
        {
            //  need to delete equations in order to update the database
            //  not sure why this has to happen but the way the DAL save works
            //  is if the user deleted an equation, the Save does not consider that
            //  when the equation list is updated ???????
            //  it also doesn't recognize deleting a row and then adding it back in
            //  says it's a constraint violation.  need an undo method for deletes
            //  December 2013
            List<ValueEquationDO> vList = getValueEquations();
            foreach (ValueEquationDO v in vList)
            {
                int nthRow = valList.FindIndex(
                    delegate (ValueEquationDO ved)
                    {
                        return ved.ValueEquationNumber == v.ValueEquationNumber && ved.Species == v.Species;
                    });
                if (nthRow < 0)
                    v.Delete();
            }   //  end foreach loop
            foreach (ValueEquationDO veq in valList)
            {
                if (veq.DAL == null)
                {
                    veq.DAL = DAL;
                }
                veq.Save();
            }   //  end foreach loop

            return;
        }   //  end SaveValueEquations


        public void SaveQualityAdjEquations(List<QualityAdjEquationDO> qaEquationList)
        {
            //  need to delete equations in order to update the database
            //  not sure why this has to happen but the way the DAL save works
            //  is if the user deleted an equation, the Save does not consider that
            //  when the equation list is updated ???????
            List<QualityAdjEquationDO> qaList = getQualAdjEquations();
            foreach (QualityAdjEquationDO qdo in qaList)
            {
                int nthRow = qaEquationList.FindIndex(
                    delegate (QualityAdjEquationDO qed)
                    {
                        return qed.QualityAdjEq == qdo.QualityAdjEq && qed.Species == qdo.Species;
                    });
                if (nthRow < 0)
                    qdo.Delete();
            }   //  end foreach loop
            foreach (QualityAdjEquationDO qae in qaEquationList)
            {
                if (qae.DAL == null)
                {
                    qae.DAL = DAL;
                }
                qae.Save();
            }   //  end foreach loop

            return;
        }   //  end SaveQualityAdjEquations


        public void SaveBiomassEquations(List<BiomassEquationDO> bioList)
        {
            foreach (BiomassEquationDO beq in bioList)
            {
                if (beq.DAL == null)
                {
                    beq.DAL = DAL;
                }
                beq.Save();
            }   //  end foreach loop

            return;
        }   //  end SaveBiomassEquations


        public void SaveSampleGroups(List<SampleGroupDO> sgList)
        {
            foreach (SampleGroupDO sgd in sgList)
            {
                if (sgd.DAL == null)
                    sgd.DAL = DAL;
                sgd.Save();
            }   //  end foreach loop
            return;
        }   //  end SaveSampleGroups


        public void SaveErrorMessages(List<ErrorLogDO> errList)
        {
            // rewritten DEC 2020 - Ben
            foreach (ErrorLogDO eld in errList)
            {

                if (eld.DAL == null)
                { eld.DAL = DAL; }


                //  February 2015 -- due to constraining levels in the error log table
                //  duplicates are not allowed.  for example, if a Region 8 volume equation
                //  has two errors on the same tree (BDFT and CUFT), only one error is allowed
                //  to be logged in the table
                //  so we will ignore uniqe errors when saving
                try
                {
                    eld.Save();
                }
                catch (FMSC.ORM.UniqueConstraintException)
                { }
            }   //  end foreach loop

            return;
        }   //  end SaveErrorMessages

        public void LogError(string tableName, int table_CN, string errLevel, string errMessage)
        {
            List<ErrorLogDO> errList = new List<ErrorLogDO>();
            ErrorLogDO eldo = new ErrorLogDO();

            eldo.TableName = tableName;
            eldo.CN_Number = table_CN;
            eldo.Level = errLevel;
            eldo.ColumnName = "Volume";
            eldo.Message = errMessage;
            eldo.Program = "CruiseProcessing";
            errList.Add(eldo);

            SaveErrorMessages(errList);
        } 


        public void SaveLCD(List<LCDDO> LCDlist)
        {
            foreach (LCDDO DOlcd in LCDlist)
            {
                if (DOlcd.DAL == null)
                {
                    DOlcd.DAL = DAL;
                }
                DOlcd.Save();
            }   //  end foreach loop
            return;
        }   //  end SaveLCD


        public void SavePOP(List<POPDO> POPlist)
        {
            foreach (POPDO DOpop in POPlist)
            {
                if (DOpop.DAL == null)
                {
                    DOpop.DAL = DAL;
                }
                DOpop.Save();
            }   //  end foreach loop
            return;
        }   //  end SavePOP


        public void SavePRO(List<PRODO> PROlist)
        {
            foreach (PRODO DOpro in PROlist)
            {
                if (DOpro.DAL == null)
                {
                    DOpro.DAL = DAL;
                }
                DOpro.Save();
            }   //  end foreach loop
            return;
        }   //  end SavePRO


        //  Get select data
        // *******************************************************************************************
        public ArrayList GetJustSpecies(string whichTable)
        {
            // TODO rewrite calling code to not use ArrayList
            // rewritten Dec 2020 - Ben

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT DISTINCT Species FROM ");
            if (whichTable == "Tree")
            {
                sb.Append(whichTable);
                sb.Append(" WHERE CountOrMeasure='M'");
            }
            else sb.Append(whichTable);

            var result = DAL.QueryScalar<string>(sb.ToString()).ToList();

            return new ArrayList(result);
        }   //  end GetJustSpecies


        public ArrayList GetJustPrimaryProduct()
        {
            ArrayList justProduct = new ArrayList();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT DISTINCT PrimaryProduct FROM TreeDefaultValue");

            var result = DAL.QueryScalar<string>(sb.ToString()).ToList();
            return new ArrayList(result);

        }   //  end GetJustPrimaryProduct


        public ArrayList GetJustSampleGroups()
        {
            // rewritten Dec 2020 - Ben
            var result = DAL.QueryScalar<string>("SELECT DISTINCT Code FROM SampleGroup;").ToList();
            return new ArrayList(result);

        }   //  end GetJustSampleGroups

        // *  July 2017 -- changes to Region 8 volume equations -- this code is no longer used
        // * July 2017 --  decision made to hold off on releasing the changes to R8 volume equations
        // * so this code is replaced.
        public ArrayList GetProduct08Species()
        {
            // TODO optimize

            ArrayList justSpecies = new ArrayList();

            List<TreeDefaultValueDO> tdvList = DAL.From<TreeDefaultValueDO>().Where("PrimaryProduct = '08'").GroupBy("Species").Read().ToList();

            foreach (TreeDefaultValueDO tdv in tdvList)
                justSpecies.Add(tdv.Species);

            return justSpecies;
        }   //  end GetProduct08Species

        /*  December 2013 -- need to change where species/product comes from.  Template files will have all volume equations
         *  but it was found if only one or more species are used, folks don't want to see all those equations
         *  so changed this to pull those combinations from Tree instead of the default
                public string[,] GetUniqueSpeciesProduct()
                {
                    DAL = new CruiseDAL.DAL(fileName);

                    List<TreeDefaultValueDO> tdvList = DAL.Read<TreeDefaultValueDO>("TreeDefaultValue", "GROUP BY Species, PrimaryProduct", null);
                    int numSpecies = tdvList.Count();
                    string[,] speciesProduct = new string[numSpecies, 2];
                    int nthRow = 0;
                    foreach (TreeDefaultValueDO tdv in tdvList)
                    {
                        speciesProduct[nthRow,0] = tdv.Species;
                        speciesProduct[nthRow,1] = tdv.PrimaryProduct;
                        nthRow++;
                    }   //  end foreach loop
                    return speciesProduct;
                }   //  end GetUniqueSpeciesProduct
        */

        public string[,] GetUniqueSpeciesProduct()
        {
            // TODO optimize

            List<TreeDO> tList = DAL.From<TreeDO>().GroupBy("Species", "SampleGroup_CN").Read().ToList();
            int numSpecies = tList.Count();
            string[,] speciesProduct = new string[numSpecies, 2];
            int nthRow = -1;
            foreach (TreeDO t in tList)
            {
                //  is the combination already in the list?
                int iFound = -1;
                for (int k = 0; k < speciesProduct.GetLength(0); k++)
                {
                    if (speciesProduct[k, 0] == t.Species && speciesProduct[k, 1] == t.SampleGroup.PrimaryProduct)
                        iFound = 1;
                }   //  end for loop
                if (iFound == -1)
                {
                    //if (t.CountOrMeasure != "I")
                    //{
                    nthRow++;
                    speciesProduct[nthRow, 0] = t.Species;
                    speciesProduct[nthRow, 1] = t.SampleGroup.PrimaryProduct;
                    //}   //  endif
                }   //  endif
            }   //  end foreach loop

            return speciesProduct;
        }   //  end GetUniqueSpeciesProduct


        public List<TreeDO> GetUniqueSpecies()
        {
            // TODO optimize this method and calling methods

            return DAL.From<TreeDO>().GroupBy("Species").Read().ToList();
        }   //  end GetUniqueSpecies


        public string[,] GetStrataUnits(long currST_CN)
        {
            // TODO optimize

            //  Get all units for the current stratum
            int nthRow = 0;
            string[,] unitAcres = new string[25, 2];

            List<StratumDO> stList = DAL.From<StratumDO>().Where("Stratum_CN = @p1").Read(currST_CN).ToList();
            foreach (StratumDO sd in stList)
            {
                sd.CuttingUnits.Populate();
                foreach (CuttingUnitDO cud in sd.CuttingUnits)
                {
                    if (nthRow < 25)
                    {
                        unitAcres[nthRow, 0] = cud.Code;
                        unitAcres[nthRow, 1] = cud.Area.ToString();
                        nthRow++;
                    }   //  endif nthRow
                }   //  end foreach loop on cutting units
            }   //  end foreach loop on strata
            return unitAcres;
        }   //  end GetStrataUnits


        public ArrayList GetUnitStrata(string currUnit)
        {
            // TODO optimize

            ArrayList unitStratum = new ArrayList();

            List<CuttingUnitDO> cList = DAL.From<CuttingUnitDO>().Where("Code = @p1").Read(currUnit).ToList();
            foreach (CuttingUnitDO cud in cList)
            {
                cud.Strata.Populate();
                foreach (StratumDO sd in cud.Strata)
                    unitStratum.Add(sd.Code);
            }   //  end for each unit
            return unitStratum;
        }   //  end GetUnitStrata


        public List<JustDIBs> GetJustDIBs()
        {
            // TODO can be improved. Just use JustDIB as the data object instead of using VolEq and an intermediate

            //  retrieve species, product and both DIBs from volume equation table
            List<JustDIBs> jstDIBs = new List<JustDIBs>();

            List<VolumeEquationDO> volList = DAL.From<VolumeEquationDO>().GroupBy("Species", "PrimaryProduct").Read().ToList();
            foreach (VolumeEquationDO veq in volList)
            {
                JustDIBs js = new JustDIBs();
                js.speciesDIB = veq.Species;
                js.productDIB = veq.PrimaryProduct;
                js.primaryDIB = veq.TopDIBPrimary;
                js.secondaryDIB = veq.TopDIBSecondary;
                jstDIBs.Add(js);
            }   //  end foreach loop

            return jstDIBs;
        }   //  end GetJustDIBs


        public List<TreeDefaultValueDO> GetUniqueSpeciesProductLiveDead()
        {
            ////  make sure filename is complete
            //fileName = checkFileName(fileName);

            ////  build query string
            //StringBuilder sb = new StringBuilder();
            //sb.Clear();
            //sb.Append("SELECT DISTINCT Species,LiveDead,PrimaryProduct FROM TreeDefaultValue");
            //List<TreeDefaultValueDO> tdvList = new List<TreeDefaultValueDO>();

            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //    sqlcmd.CommandText = sb.ToString();
            //    sqlcmd.ExecuteNonQuery();
            //    SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
            //    if (sdrReader.HasRows)
            //    {
            //        while (sdrReader.Read())
            //        {
            //            TreeDefaultValueDO td = new TreeDefaultValueDO();
            //            td.Species = sdrReader.GetString(0);
            //            td.LiveDead = sdrReader.GetString(1);
            //            td.PrimaryProduct = sdrReader.GetString(2);
            //            tdvList.Add(td);
            //        }   //  end while read
            //    }   //  endif
            //    sqlconn.Close();
            //}   //  end using
            //return tdvList;


            // rewritten Dec 2020 - Ben
            return DAL.Read<TreeDefaultValueDO>("SELECT DISTINCT Species,LiveDead,PrimaryProduct FROM TreeDefaultValue", null).ToList();
        }   //  end GetUniqueSpeciesProductLiveDead


        public List<RegressGroups> GetUniqueSpeciesGroups()
        {
            // TODO optimize

            //  used for Local Volume table

            List<TreeDO> tList = DAL.From<TreeDO>().Where("CountOrMeasure = 'M'").GroupBy("Species", "SampleGroup_CN", "LiveDead").Read().ToList();

            List<RegressGroups> rgList = new List<RegressGroups>();
            foreach (TreeDO t in tList)
            {
                if (!rgList.Exists(r => r.rgSpecies == t.Species && r.rgLiveDead == t.LiveDead && r.rgProduct == t.SampleGroup.PrimaryProduct))
                {
                    RegressGroups r = new RegressGroups();
                    r.rgSpecies = t.Species;
                    r.rgProduct = t.SampleGroup.PrimaryProduct;
                    r.rgLiveDead = t.LiveDead;
                    r.rgSelected = 0;
                    rgList.Add(r);
                }
            }   //  end foreach loop

            return rgList;
        }   //  end GetUniqueSpeciesGroups


        public List<TreeDO> GetDistinctSpecies(long SG_CN)
        {
            return DAL.From<TreeDO>().Where("SampleGroup_CN = @p1").GroupBy("Species", "LiveDead", "Grade", "STM").Read(SG_CN).ToList();
        }   //  end GetDistinctSpecies



        public List<TreeDO> getLCDtrees(LCDDO currLCD, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("JOIN Stratum JOIN SampleGroup WHERE Tree.LiveDead = @p1 AND Tree.Species = @p2 AND Tree.Grade = @p3 AND ");
            sb.Append("Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND SampleGroup.Code = @p4 AND ");
            sb.Append("Stratum.Code = @p5 AND Tree.CountOrMeasure = @p6 AND Tree.STM = @p7");
            return DAL.Read<TreeDO>("SELECT * FROM Tree " + sb.ToString() + ";", new object[] { currLCD.LiveDead, currLCD.Species, currLCD.TreeGrade,
                                            currLCD.SampleGroup, currLCD.Stratum, cntMeas, currLCD.STM }).ToList();
        }   //  end getLCDtrees


        public List<TreeDO> getPOPtrees(POPDO currPOP, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();;
            sb.Append("JOIN Stratum JOIN SampleGroup WHERE Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("SampleGroup.Code = @p1 AND Stratum.Code = @p2 AND Tree.CountOrMeasure = @p3 AND Tree.STM = @p4");
            return DAL.Read<TreeDO>("SELECT * FROM Tree " + sb.ToString() + ";", new object[] { currPOP.SampleGroup, currPOP.Stratum, cntMeas, currPOP.STM }).ToList();
        }   //  end getPOPtrees


        public List<TreeDO> getPROtrees(PRODO currPRO, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("JOIN Stratum JOIN SampleGroup JOIN CuttingUnit WHERE Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN AND ");
            sb.Append("SampleGroup.Code = @p1 AND Stratum.Code = @p2 AND CuttingUnit.Code = @p3 AND Tree.CountOrMeasure = @p4 ");
            sb.Append("AND Tree.STM = @p5");
            return DAL.Read<TreeDO>("SELECT * FROM Tree " + sb.ToString() + ";", new object[] { currPRO.SampleGroup, currPRO.Stratum, currPRO.CuttingUnit, cntMeas, currPRO.STM }).ToList();
        }   //  end getPROtrees



        //  Reports table
        // *******************************************************************************************
        public void SaveReports(List<ReportsDO> rList)
        {
            //  This saves the updated reports list
            foreach (ReportsDO rd in rList)
            {
                if (rd.DAL == null)
                {
                    rd.DAL = DAL;
                }
                rd.Save();
            }   //  end foreach loop

            if (DAL_V3 != null)
            {
                insertReportsV3();
            }

            return;
        }   //  end InsertReports


        public List<ReportsDO> GetReports()
        {
            //  Retrieve reports
            return DAL.From<ReportsDO>().OrderBy("ReportID").Read().ToList();
        }   //  end GetReports


        public List<ReportsDO> GetSelectedReports()
        {
            return DAL.From<ReportsDO>().Where("Selected = 'True' OR Selected = '1'").OrderBy("ReportID").Read().ToList();
        }   //  end GetSelectedReports



        public void updateReports(List<ReportsDO> reportList)
        {
            // rewritten Dec 2020 - Ben
            //  this updates the reports list after user has selected reports

            if (DAL_V3 != null)
            {
                foreach (ReportsDO rdo in reportList)
                {
                    //update both V3 and V2 databases.
                    DAL_V3.Execute("UPDATE Reports SET Selected =  @p1 WHERE ReportID = @p2;", rdo.Selected, rdo.ReportID);
                    //DAL.Execute("UPDATE Reports SET Selected =  @p1 WHERE ReportID = @p2;", rdo.Selected, rdo.ReportID);
                }   //  end foreach loop     
            }//end if


            foreach (ReportsDO rdo in reportList)
            {
                DAL.Execute("UPDATE Reports SET Selected =  @p1 WHERE ReportID = @p2;", rdo.Selected, rdo.ReportID);
            }   //  end foreach loop     

            return;
        }   //  end updateReports



        public void deleteReport(string reptToDelete)
        {
            // rewritten Dec 2020 - Ben

            //  deletes the report from the reports table
            DAL.Execute("DELETE FROM Reports WHERE ReportID= @p1;", reptToDelete);
        }   //  end deleteReport


        //  Specific to EX reports
        // *******************************************************************************************
        public List<exportGrades> GetExportGrade()
        {
            // rewritten Dec 2020 - Ben

            return DAL.Query<exportGrades>("SELECT * FROM ExportValues;").ToList();

        }   //  end GetExportGrade


        public void SaveExportGrade(List<exportGrades> sortList,
                                    List<exportGrades> gradeList, bool tableExists)
        {
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            //  open connection and save data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //  open connection
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //  if table exists, delete all rows
            if (tableExists == true)
            {
                //sqlcmd.CommandText = "DELETE FROM ExportValues";
                //sqlcmd.ExecuteNonQuery();
                DAL.Execute("DELETE FROM ExportValues");
            }   //  endif

            StringBuilder sb = new StringBuilder();
            //  create query for each list
            foreach (exportGrades eg in sortList)
            {
                var _sb = eg.createInsertQuery(eg, "sort");
                DAL.Execute(_sb.ToString());
                // sqlcmd.CommandText = sb.ToString();
                // sqlcmd.ExecuteNonQuery();
            }   //  end foreach on sortList

            foreach (exportGrades eg in gradeList)
            {
                var _sb = eg.createInsertQuery(eg, "grade");
                DAL.Execute(_sb.ToString());

            }   //  end foreach on gradeList
                //sqlconn.Close();
                //}   //  end using

            return;
        }   //  end SaveExportGrade


        //  specific to stewardship reports (R208)
        // *******************************************************************************************
        public List<StewProductCosts> getStewCosts()
        {
            // rewritten Dec 2020 - Ben

            return DAL.Query<StewProductCosts>("SELECT * FROM StewProductCosts;").ToList();
        }   //  end getStewCosts


        public void SaveStewCosts(List<StewProductCosts> stList)
        {
            // TODO
            foreach (StewProductCosts st in stList)
            {
                var sb = st.createInsertQuery(st);
                DAL.Execute(sb.ToString());

            }   //  end foreach loop                
        }   //  end SaveStewCosts



        //  Delete all records from tables containing calculated values
        // *******************************************************************************************
        public void deleteTreeCalculatedValues()
        {
            //  the following code is what Ben Campbell suggested but it runs REALLY SLOW
            //  Switched back to my method which runs faster
            //List<TreeCalculatedValuesDO> tcvList = getTreeCalculatedValues();
            //foreach (TreeCalculatedValuesDO tcvdo in tcvList)
            //    tcvdo.Delete();

            //  make sure filename is complete
            /*          fileName = checkFileName(fileName);

                      //   open connection and delete data
                      using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
                      {
                          //  open connection
                          sqlconn.Open();
                          SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                          //  delete all rows
                          sqlcmd.CommandText = "DELETE FROM TreeCalculatedValues WHERE TreeCalcValues_CN>0";
                          sqlcmd.ExecuteNonQuery();
                          sqlconn.Close();
                      }   //  end using
                      */
            DAL.Execute("DELETE FROM TreeCalculatedValues WHERE TreeCalcValues_CN>0");
            return;

        }   //  end deleteTreeCalculatedValues


        public void DeleteLogStock()
        {
            //  see note above concerning this code

            //  make sure filename is complete
            /*          fileName = checkFileName(fileName);

                      //   open connection and delete data
                      using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
                      {
                          //  open connection
                          sqlconn.Open();
                          SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                          //  delete all rows
                          sqlcmd.CommandText = "DELETE FROM LogStock WHERE LogStock_CN>0";
                          sqlcmd.ExecuteNonQuery();
                          sqlconn.Close();
                      }   //  end using
                      */
            DAL.Execute("DELETE FROM LogStock WHERE LogStock_CN>0");
            return;
        }   //  end deleteLogStock


        public void DeleteLCD()
        {
            //  see note above concerning this code
            //List<LCDDO> lcdList = getLCD();
            //foreach (LCDDO lcdo in lcdList)
            //    lcdo.Delete();
            //  make sure filename is complete
            /*            fileName = checkFileName(fileName);

                        //   open connection and delete data
                        using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
                        {
                            //  open connection
                            sqlconn.Open();
                            SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                            //  delete all rows
                            sqlcmd.CommandText = "DELETE FROM LCD WHERE LCD_CN>0";
                            sqlcmd.ExecuteNonQuery();
                            sqlconn.Close();
                        }   //  end using
                        */
            DAL.Execute("DELETE FROM LCD WHERE LCD_CN>0");
            return;
        }   //  end deleteLCD


        public void DeletePOP()
        {
            //  see note above concerning this code
            //List<POPDO> popList = getPOP();
            //foreach (POPDO ppdo in popList)
            //    ppdo.Delete();

            /*            //  make sure filename is complete
                        fileName = checkFileName(fileName);

                        //   open connection and delete data
                        using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
                        {
                            //  open connection
                            sqlconn.Open();
                            SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                            //  delete all rows
                            sqlcmd.CommandText = "DELETE FROM POP WHERE POP_CN>0";
                            sqlcmd.ExecuteNonQuery();
                            sqlconn.Close();
                        }   //  end using
                        */
            DAL.Execute("DELETE FROM POP WHERE POP_CN>0");
            return;
        }   //  end deletePOP


        public void DeletePRO()
        {
            ////  see note above concerning this code
            ////List<PRODO> proList = getPRO();
            ////foreach (PRODO prdo in proList)
            ////    prdo.Delete();
            ////  make sure filename is complete
            //fileName = checkFileName(fileName);

            ////   open connection and delete data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //    //  open connection
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //    //  delete all rows
            //    sqlcmd.CommandText = "DELETE FROM PRO WHERE PRO_CN>0";
            //    sqlcmd.ExecuteNonQuery();
            //    sqlconn.Close();
            //}   //  end using
            //return;

            DAL.Execute("DELETE FROM PRO WHERE PRO_CN>0");
        }   //  end deletePRO


        public void DeleteErrorMessages()
        {
            //  deletes warnings and errors messages just for CruiseProcessing
            //  make sure filename is complete
            /*            fileName = checkFileName(fileName);

                        //   open connection and delete data
                        using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
                        {
                            //  open connection
                            sqlconn.Open();
                            SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                            //  delete all rows
                            sqlcmd.CommandText = "DELETE FROM ErrorLog WHERE Program = 'CruiseProcessing'";
                            sqlcmd.ExecuteNonQuery();
                            sqlconn.Close();
                        }   //  end using
                        */
            DAL.Execute("DELETE FROM ErrorLog WHERE Program = 'CruiseProcessing'");
            return;
        }   //  end deleteErrorMessages


        public void deleteVolumeEquations()
        {
            //  used mostly in region 8 volume equations
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            //  open connection and delete data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            // {
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();
            //    //  delete all rows
            //    sqlcmd.CommandText = ("DELETE FROM VolumeEquation");
            //    sqlcmd.ExecuteNonQuery();
            //    sqlconn.Close();
            DAL.Execute("DELETE FROM VolumeEquation");
            //}   //  end using
            //return;
        }   //  end deleteVolumeEquations


        //  Regression table
        public void DeleteRegressions()
        {
            //  fix filename
            //string tempFileName = checkFileName(fileName);

            //  open connection and delete data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(tempFileName))
            //{
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();
            //    //  delete all rows
            //    sqlcmd.CommandText = "DELETE FROM Regression";
            //    sqlcmd.ExecuteNonQuery();
            //    sqlconn.Close();
            //}   //  end using
            DAL.Execute("DELETE FROM Regression");
            return;
        }   //  end deleteRegressiosn


        public void SaveRegress(List<RegressionDO> resultsList)
        {
            //  Then saves the updated regression results list
            foreach (RegressionDO rl in resultsList)
            {
                if (rl.DAL == null)
                {
                    rl.DAL = DAL;
                }
                rl.Save();
            }   //  end foreach loop
            return;

        }   //  end SaveRegress


        public void ClearBiomassEquations()
        {
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            //  open connection and delete data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //sqlconn.Open();
            //SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //  delete all rows
            //sqlcmd.CommandText = "DELETE FROM BiomassEquation WHERE rowid>=0";
            DAL.Execute("DELETE FROM BiomassEquation WHERE rowid >= 0");
            //sqlcmd.ExecuteNonQuery();
            //sqlconn.Close();
            //}   //  end using
            return;
        }   //  end clearBiomassEquations


        public void SaveTreeDefaults(List<TreeDefaultValueDO> treeDefaults)
        {
            //  Then saves the updated regression results list
            foreach (TreeDefaultValueDO tdv in treeDefaults)
            {
                if (tdv.DAL == null)
                {
                    tdv.DAL = DAL;
                }
                tdv.Save();
            }   //  end foreach loop
            return;

        }   //  end SaveTreeDefaults


        #region V3 methods

        public void insertReportsV3()
        {
            DAL_V3.BeginTransaction();
            try
            {
                //make sure the reports is empty.
                DAL_V3.Execute("DELETE FROM REPORTS");

                var reportsArray = allReportsArray.reportsArray;
                for (int k = 0; k < reportsArray.GetLength(0); k++)
                {
                    //  since this is an initial list where none exists, selected will always be zero or false
                    string ReportID = reportsArray[k, 0];
                    bool Selected = false;
                    string Title = reportsArray[k, 1];
                    string CreatedBy = "";


                    DAL_V3.Execute2(
                    @"INSERT INTO Reports (
                    ReportID,
                    CruiseID,
                    Selected,
                    Title
                ) VALUES (
                    @ReportID,
                    @CruiseID,
                    @Selected,
                    @Title
                );",
                    new
                    {
                        ReportID,
                        CruiseID,
                        Selected,
                        Title
                    });

                }//  end for k loop

                DAL_V3.CommitTransaction();
            }
            catch
            {
                DAL_V3.RollbackTransaction();
                throw;
            }

        }

        public void syncVolumeEquationToV3()
        {
            List<VolumeEquationDO> myVE = this.getVolumeEquations();

            //delete all from V3 file
            DAL_V3.BeginTransaction();
            try
            {
                //make sure the reports is empty.
                DAL_V3.Execute("DELETE FROM VolumeEquation");

                foreach (VolumeEquationDO volEq in myVE)
                {
                    int VolumeEquation_CN;  //   VolumeEquation_CN INTEGER PRIMARY KEY AUTOINCREMENT,

                    string Species;                 //Species TEXT NOT NULL,
                    Species = volEq.Species;

                    string PrimaryProduct;          //PrimaryProduct TEXT NOT NULL,
                    PrimaryProduct = volEq.PrimaryProduct;

                    string VolumeEquationNumber;    //VolumeEquationNumber TEXT NOT NULL,
                    VolumeEquationNumber = volEq.VolumeEquationNumber;

                    float StumpHeight;              //StumpHeight REAL Default 0.0,
                    StumpHeight = volEq.StumpHeight;

                    float TopDIBPrimary;            //TopDIBPrimary REAL Default 0.0,
                    TopDIBPrimary = volEq.TopDIBPrimary;

                    float TopDIBSecondary;          //TopDIBSecondary REAL Default 0.0,
                    TopDIBSecondary = volEq.TopDIBSecondary;

                    long CalcTotal;                  //CalcTotal INTEGER Default 0,
                    CalcTotal = volEq.CalcTotal;

                    long CalcBoard;                  //CalcBoard INTEGER Default 0,
                    CalcBoard = volEq.CalcBoard;

                    long CalcCubic;                  //CalcCubic INTEGER Default 0,
                    CalcCubic = volEq.CalcCubic;

                    long CalcCord;                   //CalcCord INTEGER Default 0,
                    CalcCord = volEq.CalcCord;

                    long CalcTopwood;                //CalcTopwood INTEGER Default 0,
                    CalcTopwood = volEq.CalcTopwood;

                    long CalcBiomass;                //CalcBiomass INTEGER Default 0,
                    CalcBiomass = volEq.CalcBiomass;

                    float Trim;                     //Trim REAL Default 0.0,
                    Trim = volEq.Trim;

                    long SegmentationLogic;          //SegmentationLogic INTEGER Default 0,
                    SegmentationLogic = volEq.SegmentationLogic;

                    float MinLogLengthPrimary;      //MinLogLengthPrimary REAL Default 0.0,
                    MinLogLengthPrimary = volEq.MinLogLengthPrimary;

                    float MaxLogLengthPrimary;      //MaxLogLengthPrimary REAL Default 0.0,
                    MaxLogLengthPrimary = volEq.MaxLogLengthPrimary;

                    //float MinLogLengthSecondary;    no longer used in the data objects. //MinLogLengthSecondary REAL Default 0.0,

                    //float MaxLogLengthSecondary;    No longer used //MaxLogLengthSecondary REAL Default 0.0, 
                    //MaxLogLengthSecondary = volEq.MaxLogLengthSecondary;

                    float MinMerchLength;           //MinMerchLength REAL Default 0.0,
                    MinMerchLength = volEq.MinMerchLength;

                    string Model;                   //Model TEXT,
                    Model = volEq.Model;

                    string CommonSpeciesName;       //CommonSpeciesName TEXT,
                    CommonSpeciesName = volEq.CommonSpeciesName;

                    long MerchModFlag;               //MerchModFlag INTEGER Default 0,
                    MerchModFlag = volEq.MerchModFlag;

                    long EvenOddSegment;             //EvenOddSegment INTEGER Default 0,
                    EvenOddSegment = volEq.EvenOddSegment;


                    DAL_V3.Execute2(
                    @"INSERT INTO VolumeEquation (
                    CruiseID,
                    Species,
                    PrimaryProduct,
                    VolumeEquationNumber,
                    StumpHeight,
                    TopDIBPrimary,
                    TopDIBSecondary,
                    CalcTotal,
                    CalcBoard,
                    CalcCubic,
                    CalcCord,
                    CalcTopwood,
                    CalcBiomass,
                    Trim,
                    SegmentationLogic,
                    MinLogLengthPrimary,
                    MaxLogLengthPrimary,
                    MinMerchLength,
                    Model,
                    CommonSpeciesName,
                    MerchModFlag,
                    EvenOddSegment
                ) VALUES (
                    @CruiseID,
                    @Species,
                    @PrimaryProduct,
                    @VolumeEquationNumber,
                    @StumpHeight,
                    @TopDIBPrimary,
                    @TopDIBSecondary,
                    @CalcTotal,
                    @CalcBoard,
                    @CalcCubic,
                    @CalcCord,
                    @CalcTopwood,
                    @CalcBiomass,
                    @Trim,
                    @SegmentationLogic,
                    @MinLogLengthPrimary,
                    @MaxLogLengthPrimary,
                    @MinMerchLength,
                    @Model,
                    @CommonSpeciesName,
                    @MerchModFlag,
                    @EvenOddSegment
                );",
                    new
                    {
                        CruiseID,
                        Species,
                        PrimaryProduct,
                        VolumeEquationNumber,
                        StumpHeight,
                        TopDIBPrimary,
                        TopDIBSecondary,
                        CalcTotal,
                        CalcBoard,
                        CalcCubic,
                        CalcCord,
                        CalcTopwood,
                        CalcBiomass,
                        Trim,
                        SegmentationLogic,
                        MinLogLengthPrimary,
                        MaxLogLengthPrimary,
                        MinMerchLength,
                        Model,
                        CommonSpeciesName,
                        MerchModFlag,
                        EvenOddSegment
                    });



                }//end for each

                DAL_V3.CommitTransaction();
            }
            catch
            {
                DAL_V3.RollbackTransaction();
                throw;
            }
    

        }//end sync vol equation

        public void syncBiomassEquationToV3()
        {
            List<BiomassEquationDO> myBE = this.getBiomassEquations();            

            //delete all from V3 file
            DAL_V3.BeginTransaction();
            try
            {
                //make sure the reports is empty.
                DAL_V3.Execute("DELETE FROM BiomassEquation");

                foreach (BiomassEquationDO bioEq in myBE)
                {
                    //Species TEXT NOT NULL,
                    string Species;
                    Species = bioEq.Species;

                    //Product TEXT NOT NULL,
                    string Product;
                    Product = bioEq.Product;

                    //Component TEXT NOT NULL,
                    string Component;
                    Component = bioEq.Component;

                    //LiveDead TEXT NOT NULL,
                    string LiveDead;
                    LiveDead = bioEq.LiveDead;

                    //FIAcode INTEGER NOT NULL,
                    long FIAcode;
                    FIAcode = bioEq.FIAcode;

                    //Equation TEXT,
                    string Equation;
                    Equation = bioEq.Equation;

                    //PercentMoisture REAL Default 0.0,
                    float PercentMoisture;
                    PercentMoisture = bioEq.PercentMoisture;

                    //PercentRemoved REAL Default 0.0,
                    float PercentRemoved;
                    PercentRemoved = bioEq.PercentRemoved;

                    //MetaData TEXT,
                    string MetaData;
                    MetaData = bioEq.MetaData;

                    //WeightFactorPrimary REAL Default 0.0,
                    float WeightFactorPrimary;
                    WeightFactorPrimary = bioEq.WeightFactorPrimary;

                    //WeightFactorSecondary REAL Default 0.0,
                    float WeightFactorSecondary;
                    WeightFactorSecondary = bioEq.WeightFactorSecondary;




                    DAL_V3.Execute2(
                   @"INSERT INTO BiomassEquation (
                    CruiseID,
                    Species,
                    Product,
                    Component,
                    LiveDead,
                    FIAcode,
                    Equation,
                    PercentMoisture,
                    PercentRemoved,
                    MetaData,
                    WeightFactorPrimary,
                    WeightFactorSecondary
                ) 
                VALUES ( 
                    @CruiseID,
                    @Species,
                    @Product,
                    @Component,
                    @LiveDead,
                    @FIAcode,
                    @Equation,
                    @PercentMoisture,
                    @PercentRemoved,
                    @MetaData,
                    @WeightFactorPrimary,
                    @WeightFactorSecondary
                );",
                    new
                    {
                        CruiseID,
                        Species,
                        Product,
                        Component,
                        LiveDead,
                        FIAcode,
                        Equation,
                        PercentMoisture,
                        PercentRemoved,
                        MetaData,
                        WeightFactorPrimary,
                        WeightFactorSecondary
                    });

                }//end foreach

                DAL_V3.CommitTransaction();
            }
            catch
            {
                DAL_V3.RollbackTransaction();
                throw;
            }

        }//end sync biomass

        public void syncValueEquationToV3()
        {
            List<ValueEquationDO> myVE = this.getValueEquations();

            //delete all from V3 file
            DAL_V3.BeginTransaction();
            try
            {
                //make sure the reports is empty.
                DAL_V3.Execute("DELETE FROM ValueEquation");


                foreach (ValueEquationDO ve in myVE)
                {
                    //Species TEXT NOT NULL,
                    string Species = "";
                    Species = ve.Species;

                    //PrimaryProduct TEXT NOT NULL,
                    string PrimaryProduct;
                    PrimaryProduct = ve.PrimaryProduct;

                    //ValueEquationNumber TEXT,
                    string ValueEquationNumber;
                    ValueEquationNumber = ve.ValueEquationNumber;

                    //Grade TEXT,
                    string Grade;
                    Grade = ve.Grade;

                    //Coefficient1 REAL Default 0.0,
                    float Coefficient1;
                    Coefficient1 = ve.Coefficient1;

                    //Coefficient2 REAL Default 0.0,
                    float Coefficient2;
                    Coefficient2 = ve.Coefficient2;

                    //Coefficient3 REAL Default 0.0,
                    float Coefficient3;
                    Coefficient3 = ve.Coefficient3;

                    //Coefficient4 REAL Default 0.0,
                    float Coefficient4;
                    Coefficient4 = ve.Coefficient4;

                    //Coefficient5 REAL Default 0.0,
                    float Coefficient5;
                    Coefficient5 = ve.Coefficient5;

                    //Coefficient6 REAL Default 0.0,
                    float Coefficient6;
                    Coefficient6 = ve.Coefficient6;

                    DAL_V3.Execute2(
                    @"INSERT INTO ValueEquation (
                    CruiseID,
                    Species,
                    PrimaryProduct,
                    ValueEquationNumber,
                    Grade,
                    Coefficient1,
                    Coefficient2,
                    Coefficient3,
                    Coefficient4,
                    Coefficient5,
                    Coefficient6
                
                ) VALUES (
                    @CruiseID,
                    @Species,
                    @PrimaryProduct,
                    @ValueEquationNumber,
                    @Grade,
                    @Coefficient1,
                    @Coefficient2,
                    @Coefficient3,
                    @Coefficient4,
                    @Coefficient5,
                    @Coefficient6
                );",
                    new
                    {
                        CruiseID,
                        Species,
                        PrimaryProduct,
                        ValueEquationNumber,
                        Grade,
                        Coefficient1,
                        Coefficient2,
                        Coefficient3,
                        Coefficient4,
                        Coefficient5,
                        Coefficient6

                    });


                }//end for each

                DAL_V3.CommitTransaction();
            }
            catch
            {
                DAL_V3.RollbackTransaction();
                throw;
            }
        }

        public bool saleWithNullSpecies()
        {
            bool nullSpecies = false;

            //DAL.Read<TreeDO>("SELECT * FROM Tree Where Species is null;", ).ToList();
            List<TreeDO> myList = DAL.From<TreeDO>().Where("Species is null").Read().ToList();

            if(myList.Count() > 0)
            {
                nullSpecies = true;
            }

            return nullSpecies;

        }

        #endregion

    }   //  end CPbusinessLayer
}