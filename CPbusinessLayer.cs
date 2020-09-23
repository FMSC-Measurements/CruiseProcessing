using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using CruiseDAL.DataObjects;

namespace CruiseProcessing
{
    public class CPbusinessLayer
    {
        #region
        public string fileName;
        //StringBuilder sb = new StringBuilder();
       // string[] parameterValues = new string[11];
       // string[] selectValues = new string[11];
      //  string[] selectParameters = new string[11];
        public CruiseDAL.DAL DAL;
       // List<SaleDO> saleList = new List<SaleDO>();
       // List<StratumDO> stList = new List<StratumDO>();
        //List<CuttingUnitDO> cutList = new List<CuttingUnitDO>();
       // List<SampleGroupDO> sgList = new List<SampleGroupDO>();
       // List<TreeDO> tList = new List<TreeDO>();
       // List<TreeDefaultValueDO> tdvList = new List<TreeDefaultValueDO>();
       // List<PlotDO> pList = new List<PlotDO>();
       // List<VolumeEquationDO> volList = new List<VolumeEquationDO>();
        #endregion

// *******************************************************************************************
        public string createNewTableQuery(string tableName, params string[] valuesList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
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
            if(tableName == "ExportValues")
            {
                valuesList = createExportArray();
                queryString = createNewTableQuery(tableName, valuesList);
            }
            else if(tableName == "LogMatrix")
            {
                valuesList = createLogMatrixArray();
                queryString = createNewTableQuery(tableName, valuesList);
            }
            else if(tableName == "StewProductCosts")
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
            //  make sure filename is complete
            fileName = checkFileName(fileName);

            //  find table in database
            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();
                StringBuilder sb = new StringBuilder();
                sb.Clear();
                sb.Append("SELECT name FROM sqlite_master WHERE type='table' AND name='");
                sb.Append(tableName);
                sb.Append("';");

                sqlcmd.CommandText = sb.ToString();
                sqlcmd.ExecuteNonQuery();
                SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
                if (sdrReader.HasRows)
                {
                    string currentState = sqlconn.State.ToString();
                    sqlconn.Close();
                    currentState = sqlconn.State.ToString();
                    return true;
                }
                else
                {
                    string currentState = sqlconn.State.ToString();
                    sqlconn.Close();
                    currentState = sqlconn.State.ToString();
                    return false;
                }   //  endif reader has rows

            }   //  end using
        }   //  end doesTableExist

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
            return DAL.Read<SaleDO>("Sale", null, null);
        }   //  end getSale

        public List<StratumDO> getStratum()
        {
            return DAL.Read<StratumDO>("Stratum", "ORDER BY Code", null);
        }   //  end getStratum

        public List<CuttingUnitDO> getCuttingUnits()
        {
            return DAL.Read<CuttingUnitDO>("CuttingUnit", "ORDER BY CODE", null);
        }   //  end getCuttingUnits

        public List<CuttingUnitDO> getPaymentUnits()
        {
            return DAL.Read<CuttingUnitDO>("CuttingUnit", "GROUP BY PaymentUnit", null);
        }   //  end getPaymentUnits

        public List<CuttingUnitDO> getLoggingMethods()
        {
            //  returns unique logging methods
            return DAL.Read<CuttingUnitDO>("CuttingUnit", "GROUP BY LoggingMethod", null);
        }   //  end getLoggingMethods

        public List<CuttingUnitStratumDO> getCuttingUnitStratum(long currStratumCN)
        {
            return DAL.Read<CuttingUnitStratumDO>("CuttingUnitStratum", "WHERE Stratum_CN = ?", currStratumCN);
        }   //  end getCuttingUnitStratum
        
        
        public List<PlotDO> getPlots()
        {
            return DAL.Read<PlotDO>("Plot", "ORDER BY PLOTNUMBER", null);
        }   //  end getPlots

        public List<SampleGroupDO> getSampleGroups()
        {
            return DAL.Read<SampleGroupDO>("SampleGroup", null, null);
        }   //  end getSampleGroups

        public List<TreeDefaultValueDO> getTreeDefaults()
        {
            return DAL.Read<TreeDefaultValueDO>("TreeDefaultValue", null, null);
        }   //  end getTreeDefaults

        public List<CountTreeDO> getCountTrees()
        {
            return DAL.Read<CountTreeDO>("CountTree", null, null);
        }   //  end getCountTrees

        public List<TreeDO> getTrees()
        {
            return DAL.Read<TreeDO>("Tree", null, null);
        }   //  end getTrees

        public List<LogDO> getLogs()
        {
            return DAL.Read<LogDO>("Log", null, null);
        }   //  end getLogs

        public List<LogStockDO> getLogStock()
        {
            return DAL.Read<LogStockDO>("LogStock", null, null);
        }   //  end getLogStock

        public List<VolumeEquationDO> getVolumeEquations()
        {
            return DAL.Read<VolumeEquationDO>("VolumeEquation", null, null);
        }   //  end getVolumeEquations

        public List<ValueEquationDO> getValueEquations()
        {
            return DAL.Read<ValueEquationDO>("ValueEquation", null, null);
        }   //  end getValueEquations


        public List<BiomassEquationDO> getBiomassEquations()
        {
            return DAL.Read<BiomassEquationDO>("BiomassEquation", null, null);
        }   //  end getBiomassEquations
        
        public List<QualityAdjEquationDO> getQualAdjEquations()
        {
            return DAL.Read<QualityAdjEquationDO>("QualityAdjEquation", null, null);
        }   //  end getQualAdjEquations


        public List<TreeEstimateDO> getTreeEstimates()
        {
            return DAL.Read<TreeEstimateDO>("TreeEstimate", null, null);
        }   //  end getTreeEstimates

//  TreeCalculatedValues
// *******************************************************************************************
        public List<TreeCalculatedValuesDO> getTreeCalculatedValues()
        {
            return DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues", null, null);
        }   //  end getTreeCalculatedValues

        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree WHERE Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("AND Tree.Stratum_CN = ?");
            return DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues", sb.ToString(), currStratumCN);
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN, int currUnitCN)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree WHERE Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("AND Tree.Stratum_CN = ? AND Tree.CuttingUnit_CN = ?");
            return DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues", sb.ToString(), currStratumCN, currUnitCN);
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(string currSP)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree WHERE Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("AND Tree.Species = ?");
            return DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues", sb.ToString(), currSP);
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN, string orderBy)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree WHERE Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("AND Tree.Stratum_CN = ? ORDER BY ");
            sb.Append(orderBy);
            return DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues", sb.ToString(), currStratumCN);
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getTreeCalculatedValues(string currCL, string orderBy, string reportType)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            //  works for stand table reports which are by stratum
            if (reportType == "STRATUM")
            {
                sb.Append("JOIN Tree ON Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
                sb.Append("JOIN SampleGroup JOIN Stratum WHERE Tree.Stratum_CN = Stratum.Stratum_CN ");
                sb.Append("AND Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
                sb.Append("SampleGroup.CutLeave = ? ORDER BY ");
                sb.Append(orderBy);
            }
            else if (reportType == "SALE")
            {
                sb.Append("JOIN Tree ON Tree.Tree_CN == TreeCalculatedValues.Tree_CN ");
                sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
                sb.Append("SampleGroup.CutLeave = ? ORDER BY ");
                sb.Append(orderBy);
            }   //  endif reportType

            return DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues", sb.ToString(), currCL);
        }   //  end getTreeCalculatedValues


        public List<TreeCalculatedValuesDO> getStewardshipTrees(string currUnit, string currSP, string currPP)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE SampleGroup.CutLeave = ? ");
            sb.Append("AND CuttingUnit.Code = ? AND Tree.Species = ? AND SampleGroup.PrimaryProduct = ?");
            return DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues", sb.ToString(), "C", currUnit, currSP, currPP);
        }   //  end getStewardshipTrees


        public List<TreeCalculatedValuesDO> getRegressTrees(string currSP, string currPR, string currLD, string currCM)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
            sb.Append("Tree.Species = ? AND SampleGroup.PrimaryProduct = ? AND ");
            sb.Append("Tree.LiveDead = ? AND Tree.CountOrMeasure = ?");
            return DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues", sb.ToString(), currSP, currPR, currLD, currCM);
        }   //  end getRegressTrees


//  Log Table and Log Stock Table
// *******************************************************************************************
        public List<LogDO> getTreeLogs(long currTreeCN)
        {
            return DAL.Read<LogDO>("Log", "WHERE Tree_CN = ?", currTreeCN);
        }   //  getTreeLogs


        public List<LogDO> getTreeLogs()
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Log ON Log.Tree_CN = Tree.Tree_CN");
            return DAL.Read<LogDO>("Log",sb.ToString());
        }   //  end getTreeLogs


        public List<LogStockDO> getLogStockSorted()
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("LEFT JOIN Plot ON Tree.Plot_CN = Plot.Plot_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN WHERE ");
            sb.Append("SampleGroup.CutLeave = 'C' ORDER BY Stratum.Code,CAST(CuttingUnit.Code AS NUMERIC),Plot.PlotNumber");
            return DAL.Read<LogStockDO>("LogStock", sb.ToString());
        }   //  end getLogStockSorted



        public List<LogStockDO> getCullLogs(long currTreeCN, string currGrade)
        {
            return DAL.Read<LogStockDO>("LogStock", "WHERE Tree_CN = ? AND Grade = ?", currTreeCN, currGrade);
        }   //  end getCullLogs


        public List<LogStockDO> getLogDIBs()
        {
            return DAL.Read<LogStockDO>("LogStock", "GROUP BY DIBClass");
        }   //  end getLogDIBS


        public List<LogStockDO> getCutOrLeaveLogs(string currCL)
        {
            //  Pulls logs for Region 10 L1 output files
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
            sb.Append("SampleGroup.CutLeave = ? ORDER BY Species");
            return DAL.Read<LogStockDO>("LogStock",sb.ToString(),currCL);
        }   //  end getCutLogs


        public List<LogStockDO> getCutLogs()
        {
            //  Pulls just cut logs
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
            sb.Append("SampleGroup.CutLeave = 'C' ORDER BY Species");
            return DAL.Read<LogStockDO>("LogStock", sb.ToString());
        }   //  end getCutLogs


        public List<LogStockDO> getCutLogs(string currCL, string currSP, string currPP, int byDIBclass)
        {
            //  Needed for R501 report
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.Species = ? AND SampleGroup.PrimaryProduct = ?");
            if (byDIBclass == 1)
                sb.Append(" GROUP BY DIBClass");
            return DAL.Read<LogStockDO>("LogStock", sb.ToString(), currCL, currSP, currPP);
        }   //  end getCutLogs


        public List<LogStockDO> getCutLogs(string currSP, string currSort, string currGrade)
        {
            //  used in EX reports
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN LogStock ON LogStock.Tree_CN = Tree.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.Species = ? ");
            sb.Append("AND LogStock.ExportGrade = ? AND LogStock.Grade = ?");
            return DAL.Read<LogStockDO>("LogStock", sb.ToString(), "C", currSP, currSort, currGrade);
        }   //  end getCutLogs


        public List<LogStockDO> getLogSorts(string currSP)
        {
            //  used in EX reports
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN LogStock ON LogStock.Tree_CN = Tree.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            if(currSP != "")
                sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.Species = ? ");
            else sb.Append("WHERE SampleGroup.CutLeave = ?");
            sb.Append("GROUP BY ExportGrade,Grade");
            if (currSP != "")
                return DAL.Read<LogStockDO>("LogStock", sb.ToString(), "C", currSP);
            else return DAL.Read<LogStockDO>("LogStock", sb.ToString(), "C");
        }   //  end getLogSorts


        public List<LogStockDO> getLogSpecies(string currSP, float minDIB, float maxDIB, 
                                              string currST, string currGrade)
        {
            //  used mostly for BLM09 and BLM10 reports
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
            sb.Append("SampleGroup.CutLeave = 'C' AND Stratum.Code = ? AND CountOrMeasure = 'M' AND ");
            sb.Append("Tree.Species = ? AND LogStock.Grade = ? AND DIBclass >= ? AND DIBclass <= ? ");
            return DAL.Read<LogStockDO>("LogStock", sb.ToString(), currST, currSP, currGrade, minDIB, maxDIB);
        }   //  end getLogSpecies


        public List<LogStockDO> getStrataLogs(string currST, string currGrade)
        {
            //  used mostly for BLM reports
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
            sb.Append("SampleGroup.CutLeave = 'C' AND Stratum.Code = ? AND CountOrMeasure = 'M' AND ");
            sb.Append("LogStock.Grade = ? ");
            return DAL.Read<LogStockDO>("LogStock", sb.ToString(), currST, currGrade);
        }   //  end getStrataLogs



        public List<LogStockDO> getStrataLogs(string currSP, string currST, string currSG, string currSTM, string currGrade)
        {
            //  used in BLM reports
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = 'C' AND CountOrMeasure = 'M' AND STM = ? ");
            sb.Append("AND Stratum.Code = ? AND Species = ? AND SampleGroup.Code = ? AND ");
            sb.Append("LogStock.Grade = ?");
            return DAL.Read<LogStockDO>("LogStock", sb.ToString(), currSTM, currST, currSP, currSG, currGrade);
        }   //  end getStrataLogs


        public List<LogStockDO> getUnitLogs(long currST_CN, long currCU_CN, string currGrade, string currSTM)
        {
            // mostly for BLM reports
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN == LogStock.Tree_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = 'C' AND Stratum.Stratum_CN = ? AND ");
            sb.Append("CuttingUnit.CuttingUnit_CN = ? AND LogStock.Grade = ? AND Tree.STM = ? ");
            return DAL.Read<LogStockDO>("LogStock", sb.ToString(), currST_CN, currCU_CN, currGrade, currSTM);
        }   //  end getUnitLogs


//  Error Log Table
// *******************************************************************************************
        public List<ErrorLogDO> getErrorMessages(string errLevel, string errProgram)
        {
            List<ErrorLogDO> errLog = new List<ErrorLogDO>();
            if (errProgram == "FScruiser")
                return DAL.Read<ErrorLogDO>("ErrorLog", "WHERE Level = ? AND (Program LIKE'%FScruiser%' OR Program LIKE '%CruiseSystemManager%' OR Program LIKE '%CruiseManage%')", errLevel);
            else if (errProgram == "CruiseProcessing")
                return DAL.Read<ErrorLogDO>("ErrorLog", "WHERE Level = ? AND Program = ?", errLevel, errProgram);

            return errLog;
        }   //  end getErrorMessages


//  LCD POP and PRO Tables
// *******************************************************************************************
        public List<LCDDO> getLCD()
        {
            return DAL.Read<LCDDO>("LCD", null, null);
        }   //  end getLCD

        public List<POPDO> getPOP()
        {
            return DAL.Read<POPDO>("POP", null, null);
        }   //  end getPOP

        public List<PRODO> getPRO()
        {
            return DAL.Read<PRODO>("PRO", null, null);
        }   //  end getPRO

//  Miscellaneous tables like log matrix, regression results
// *******************************************************************************************
        public List<LogMatrixDO> getLogMatrix(string currReport)
        {
            return DAL.Read<LogMatrixDO>("LogMatrix", "WHERE ReportNumber = ?", currReport);
        }   //  end getLogMatrix


        public List<RegressionDO> getRegressionResults()
        {
            //  pulls regression data for local volume reports
            return DAL.Read<RegressionDO>("Regression", null, null);
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
            List<SampleGroupDO> sgList = DAL.Read<SampleGroupDO>("SampleGroup", "WHERE Stratum_CN = ?", currStratumCN);
            return sgList[0].UOM;
        }   //  end getUOM

//  Sample Groups
// *******************************************************************************************
        public List<SampleGroupDO> getSampleGroups(int currStratumCN)
        {
            return DAL.Read<SampleGroupDO>("SampleGroup", "WHERE Stratum_CN = ?", currStratumCN);
        }   //  end getSampleGroups

        public List<SampleGroupDO> getSampleGroups(string qString)
        {
            //  returns just unique sample groups
            return DAL.Read<SampleGroupDO>("SampleGroup", qString, "C");
        }   //  end getSampleGroups


// *******************************************************************************************
        //  Like variable log length from global configuration
        public string getVLL()
        {
            List<GlobalsDO> globList = DAL.Read<GlobalsDO>("Globals", "WHERE Key = ? AND Block = ?", "VLL", "Global");
            if (globList.Count > 0)
                return "V";
            else return "false";
        }   //  end getVLL


//  Stratum table
// *******************************************************************************************
        public List<StratumDO> GetCurrentStratum(string currentStratum)
        {
            //  Pull current stratum from table
            return DAL.Read<StratumDO>("Stratum", "WHERE Code = ?", currentStratum);
        }   //  end GetCurrentStratum


        //  just FIXCNT methods
        public List<StratumDO> justFIXCNTstrata()
        {
            return DAL.Read<StratumDO>("Stratum", "WHERE Method = ?", "FIXCNT");
        }   //  end justFIXCNTstrata


//  Plot table
// *******************************************************************************************
        public List<PlotDO> GetStrataPlots(string currStratum)
        {
            List<StratumDO> stList = DAL.Read<StratumDO>("Stratum", "WHERE Code = ?", currStratum);
            List<PlotDO> pList = new List<PlotDO>();

            foreach (StratumDO sdo in stList)
            {
                pList =  DAL.Read<PlotDO>("Plot", "WHERE Stratum_CN = ?", sdo.Stratum_CN);
            }   //  end foreach loop
            
            return pList;
        }   //  end GetStrataPlots


        public List<PlotDO> getPlotsOrdered()
        {

            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Stratum JOIN CuttingUnit WHERE Plot.Stratum_CN = Stratum.Stratum_CN AND ");
            sb.Append("Plot.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN");
            sb.Append(" ORDER BY Stratum.Code,CuttingUnit.Code,PlotNumber");
            return DAL.Read<PlotDO>("Plot",sb.ToString());
        }   //  end getPlotsOrdered


//  LCD table
// *******************************************************************************************
        public List<LCDDO> getLCDOrdered(string searchString, string orderBy, string currCutLeave, 
                                            string currST, string currPP)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append(searchString);
            sb.Append(orderBy);
            if (currST == "")
                return DAL.Read<LCDDO>("LCD", sb.ToString(), currCutLeave);
            else if (currCutLeave == "")
                return DAL.Read<LCDDO>("LCD", sb.ToString(), currST);
            else if (currCutLeave != "" && currST != "" && currPP != "")
                return DAL.Read<LCDDO>("LCD", sb.ToString(), currCutLeave, currST, currPP);
            else return DAL.Read<LCDDO>("LCD", sb.ToString(), currCutLeave, currST);
        }   //  end getLCDOrdered


        public List<LCDDO> getLCDOrdered(string searchString, string orderBy, string currCutLeave, 
                                            string currPP)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append(searchString);
            sb.Append(orderBy);
            if (currPP != "")
                return DAL.Read<LCDDO>("LCD", sb.ToString(), currCutLeave, currPP);
            else return  DAL.Read<LCDDO>("LCD", sb.ToString(), currCutLeave);
        }   //  end getLCDOrdered


        public List<LCDDO> GetLCDgroup(string currST, int currRPT, string currCL)
        {
            List<LCDDO> justGroup = new List<LCDDO>();
            StringBuilder sb = new StringBuilder();
            sb.Clear();
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
            string prevFileName = fileName;
            fileName = checkFileName(fileName);

            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                sqlcmd.CommandText = sb.ToString();
                sqlcmd.ExecuteNonQuery();

                SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
                if (sdrReader.HasRows)
                {
                    while (sdrReader.Read())
                    {
                        LCDDO ld = new LCDDO();
                        switch (currRPT)
                        {
                            case 1:
                                ld.Stratum = sdrReader.GetString(0);
                                ld.Species = sdrReader.GetString(1);
                                ld.PrimaryProduct = sdrReader.GetString(2);
                                ld.SecondaryProduct = sdrReader.GetString(3);
                                ld.UOM = sdrReader.GetString(4);
                                ld.LiveDead = sdrReader.GetString(5);
                                ld.CutLeave = sdrReader.GetString(6);
                                ld.Yield = sdrReader.GetString(7);
                                ld.SampleGroup = sdrReader.GetString(8);
                                ld.TreeGrade = sdrReader.GetString(9);
                                ld.ContractSpecies = sdrReader.GetString(10);
                                ld.STM = sdrReader.GetString(11);
                                break;
                            case 2:
                                ld.Stratum = sdrReader.GetString(0);
                                ld.PrimaryProduct = sdrReader.GetString(1);
                                ld.SecondaryProduct = sdrReader.GetString(2);
                                ld.UOM = sdrReader.GetString(3);
                                ld.SampleGroup = sdrReader.GetString(4);
                                ld.CutLeave = sdrReader.GetString(5);
                                ld.STM = sdrReader.GetString(6);
                                break;
                            case 3:
                                ld.Stratum = sdrReader.GetString(0);
                                ld.PrimaryProduct = sdrReader.GetString(1);
                                ld.SecondaryProduct = sdrReader.GetString(2);
                                ld.UOM = sdrReader.GetString(3);
                                break;
                            case 4:     //  WT1
                                ld.Species = sdrReader.GetString(0);
                                ld.PrimaryProduct = sdrReader.GetString(1);
                                ld.SecondaryProduct = sdrReader.GetString(2);
                                ld.LiveDead = sdrReader.GetString(3);
                                ld.ContractSpecies = sdrReader.GetString(4);
                                break;
                            case 5:     //  WT4
                                ld.Species = sdrReader.GetString(0);
                                break;
                        }   //  end switch
                        justGroup.Add(ld);
                    }   //  end while read
                }   //  endif
                sqlconn.Close();
            }   //  end using

            fileName = prevFileName;
            return justGroup;
        }   //  end GetLCDgroup

        public List<LCDDO> GetLCDgroup(string fileName, string groupedBy)
        {
            //  overloaded for stat report  ST3
            List<LCDDO> justGroup = new List<LCDDO>();
            string tempFileName = checkFileName(fileName);
            using (SQLiteConnection sqlconn = new SQLiteConnection(tempFileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();
                StringBuilder sb = new StringBuilder();
                sb.Clear();
                sb.Append("SELECT Stratum,PrimaryProduct,SecondaryProduct,UOM FROM LCD WHERE CutLeave = 'C' GROUP BY ");
                sb.Append(groupedBy);
                sqlcmd.CommandText = sb.ToString();
                sqlcmd.ExecuteNonQuery();

                SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
                if (sdrReader.HasRows)
                {
                    while (sdrReader.Read())
                    {
                        LCDDO ld = new LCDDO();
                        ld.Stratum = sdrReader.GetString(0);
                        ld.PrimaryProduct = sdrReader.GetString(1);
                        ld.SecondaryProduct = sdrReader.GetString(2);
                        ld.UOM = sdrReader.GetString(3);
                        justGroup.Add(ld);
                    }   //  end while reader
                }   //  endif reader has rows
                sqlconn.Close();
            }   //  end using
            return justGroup;
        }   //  end GetLCDgroup


        public List<LCDDO> GetLCDdata(string whereClause, LCDDO lcd, int reportNum, string currCL)
        {
            //  report number key
            //  1 = VSM1
            //  2 = VSM2
            //  3 = VSM3
            //  4 = WT1
            //  5 = R301
            
            switch (reportNum)
            {
                case 1:
                    return DAL.Read<LCDDO>("LCD", whereClause, lcd.Stratum, lcd.Species, lcd.PrimaryProduct, lcd.UOM,
                                            lcd.LiveDead, lcd.CutLeave, lcd.Yield, lcd.SampleGroup, lcd.TreeGrade, lcd.STM);
                                            //  September 2016 -- dropping contract species from LCD identifier
                                            //lcd.ContractSpecies, lcd.STM);
                case 2:     
                    return DAL.Read<LCDDO>("LCD", whereClause, lcd.Stratum, lcd.PrimaryProduct, lcd.UOM, currCL, 
                                                lcd.SampleGroup, lcd.STM);
                case 3:
                    return DAL.Read<LCDDO>("LCD", whereClause, lcd.Stratum, lcd.PrimaryProduct, lcd.UOM, currCL, lcd.STM);
                case 4:
                    return DAL.Read<LCDDO>("LCD", whereClause, lcd.Species, lcd.PrimaryProduct, lcd.SecondaryProduct, 
                                                lcd.LiveDead, lcd.ContractSpecies, "C");
                case 5:
                    return DAL.Read<LCDDO>("LCD", whereClause, "C", lcd.Species, lcd.PrimaryProduct, lcd.ContractSpecies);
            }   //  end switch
            List<LCDDO> emptyList = new List<LCDDO>();
            return emptyList;
        }   //  end GetLCDdata


        public List<LCDDO> GetLCDdata(string currST, string whereClause, string orderBy)
        {
            //  works for UC1/UC2 reports -- maybe all UC reports?
            return DAL.Read<LCDDO>("LCD", whereClause, currST, "C", orderBy);
        }   //  end GetLCDdata


        public List<LCDDO> GetLCDdata(string whereClause, string currST)
        {
            //  works for regional report R104
            return DAL.Read<LCDDO>("LCD", whereClause, currST);
        }   //  end GetLCDdata


        public List<TreeCalculatedValuesDO> GetLCDtrees(string currST, LCDDO ldo, string currCM)
        {
            //  captures tree calculated values for LCD summation in SumAll
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("JOIN TreeDefaultValue ON Tree.TreeDefaultValue_CN = TreeDefaultValue.TreeDefaultValue_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("WHERE Stratum.Code = ? AND SampleGroup.CutLeave = ? AND SampleGroup.Code = ? ");
            sb.Append("AND Tree.Species = ? AND SampleGroup.PrimaryProduct = ? AND ");
            sb.Append("SampleGroup.SecondaryProduct = ? AND SampleGroup.UOM = ? AND ");
            sb.Append("Tree.LiveDead = ? AND Tree.Grade = ? AND Stratum.YieldComponent = ? AND ");
            //sb.Append("Tree.LiveDead = ? AND Tree.Grade = ? AND TreeDefaultValue.Chargeable = ? AND ");
            //  September 2016 -- per K.Cormier -- dropping contract species from LCD identifier
            //sb.Append("TreeDefaultValue.ContractSpecies = ? AND Tree.STM = ? and Tree.CountOrMeasure = ?");
            sb.Append("Tree.STM = ? and Tree.CountOrMeasure = ?");
            return DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues", sb.ToString(), currST, ldo.CutLeave, 
                                        ldo.SampleGroup, ldo.Species, ldo.PrimaryProduct, ldo.SecondaryProduct, 
                                        //  see comment above
                                        //ldo.UOM, ldo.LiveDead, ldo.TreeGrade, ldo.Yield, ldo.ContractSpecies, 
                                        ldo.UOM, ldo.LiveDead, ldo.TreeGrade, ldo.Yield,  
                                        ldo.STM, currCM);
        }   //  end GetLCDtrees

// POP table
// *******************************************************************************************
        public List<TreeCalculatedValuesDO> GetPOPtrees(POPDO pdo, string currST, string currCM)
        {
            //  captures tree calculated values for POP summation in SumAll
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Tree ON Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("WHERE Stratum.Code = ? AND SampleGroup.CutLeave = ? AND SampleGroup.Code = ? ");
            sb.Append("AND SampleGroup.PrimaryProduct = ? AND SampleGroup.SecondaryProduct = ? AND ");
            sb.Append("Tree.STM = ? AND Tree.CountOrMeasure = ?");
            return DAL.Read<TreeCalculatedValuesDO>("TreeCalculatedValues", sb.ToString(), currST,
                                pdo.CutLeave, pdo.SampleGroup, pdo.PrimaryProduct, 
                                pdo.SecondaryProduct, pdo.STM, currCM);
        }   //  end GetPOPtrees


        public List<POPDO> GetUOMfromPOP()
        {
            return DAL.Read<POPDO>("POP", "GROUP BY UOM", null);
        }   //  end GetUOMfromPOP


//  PRO table
// *******************************************************************************************
        public List<PRODO> getPROunit(string currCU)
        {
            return DAL.Read<PRODO>("PRO", "WHERE CuttingUnit = ?", currCU);
        }   //  end getPRO


//  Count table
// *******************************************************************************************
        public List<CountTreeDO> getCountTrees(long currSG_CN)
        {
            return DAL.Read<CountTreeDO>("CountTree", " WHERE SampleGroup_CN = ?", currSG_CN);
        }   //  end overload of getCountTrees


        public List<CountTreeDO> getCountsOrdered()
        {
            return DAL.Read<CountTreeDO>("CountTree", "ORDER BY CuttingUnit_CN");
        }   //  end getCountsOrdered


        public List<CountTreeDO> getCountsOrdered(string searchString, string orderBy, string[] searchValues)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append(searchString);
            sb.Append(orderBy);
            return DAL.Read<CountTreeDO>("CountTree", sb.ToString(), searchValues);
        }   //  end getCountsOrdered

//  Tree table
// *******************************************************************************************
        public List<TreeDO> getTreesOrdered(string searchString, string orderBy, string[] searchValues)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append(searchString);
            sb.Append(orderBy);
            return DAL.Read<TreeDO>("Tree", sb.ToString(), searchValues);
        }   //  end getTreesOrdered


        public List<TreeDO> getTreesSorted()
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("LEFT OUTER JOIN Plot ON Tree.Plot_CN = Plot.Plot_CN ");
            sb.Append("ORDER BY Stratum.Code, CuttingUnit.Code, ifnull(Plot.PlotNumber, 0), Tree.TreeNumber;");

            return DAL.Read<TreeDO>("Tree", sb.ToString(), null);
        }   //  end getTreesSorted


        public List<TreeDO> getUnitTrees(long currST_CN, long currCU_CN)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("JOIN CuttingUnit on Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = 'C' AND Tree.CountOrMeasure = 'M' ");
            sb.Append("AND Tree.Stratum_CN = ? AND Tree.CuttingUnit_CN = ?");
            return DAL.Read<TreeDO>("Tree", sb.ToString(), currST_CN, currCU_CN);
        }   //  end getUnitTrees


        public List<TreeDO> getUniqueStewardGroups()
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = ? ");
            sb.Append("GROUP BY CuttingUnit_CN,Species,Tree.SampleGroup_CN");

            return DAL.Read<TreeDO>("Tree", sb.ToString(), "C");
        }   //  end getUniqueStewardGroups


        public List<TreeDO> getTreeDBH(string currCL)
        {
            //  works for stand tables DIB classes for the sale
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.CountOrMeasure = 'M' ");
            sb.Append("GROUP BY DBH");
            return DAL.Read<TreeDO>("Tree", sb.ToString(), currCL);
        }   //  end getTreeDBH


        public List<TreeDO> getTreeDBH(string currCL, string currST, string currCM)
        {
            //  works for stand tables DIB classes for current stratum
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = ? AND Stratum.Code = ? AND Tree.CountOrMeasure = ? ");
            sb.Append("GROUP BY DBH");
            return DAL.Read<TreeDO>("Tree", sb.ToString(), currCL, currST, currCM);
        }   //  end getTreeDBH


        public List<TreeDO> JustMeasuredTrees()
        {
            return DAL.Read<TreeDO>("Tree", "WHERE CountOrMeasure = ?", "M");
        }   //  end JustMeasuredTrees


        public List<TreeDO> JustMeasuredTrees(long currST_CN)
        {
            return DAL.Read<TreeDO>("Tree", "WHERE CountOrMeasure = ? AND Stratum_CN = ?", "M", currST_CN);
        }   //  end overloaded JustMeasuredTrees


        public List<TreeDO> JustUnitTrees(long currST_CN, long currCU_CN)
        {
            return DAL.Read<TreeDO>("Tree", "WHERE Stratum_CN = ? AND CuttingUnit_CN = ?", currST_CN, currCU_CN);
        }   //  end JustUnitTrees

        public List<TreeDO> JustFIXCNTtrees(long currST_CN)
        {
            return DAL.Read<TreeDO>("Tree", "WHERE CountOrMeasure = ? AND Stratum_CN = ?", "C", currST_CN);
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
            foreach(LogMatrixDO lmx in lmList)
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
                    delegate(VolumeEquationDO ve)
                    {
                        return ve.VolumeEquationNumber == vdo.VolumeEquationNumber && ve.Species == vdo.Species &&
                                ve.PrimaryProduct == vdo.PrimaryProduct;
                    });
                if(nthRow < 0)
                    vdo.Delete();
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
                    delegate(ValueEquationDO ved)
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
                    delegate(QualityAdjEquationDO qed)
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
            DAL = new CruiseDAL.DAL(fileName);
            //  February 2015 -- due to constraining levels in the error log table
            //  duplicates are not allowed.  for example, if a Region 8 volume equation
            //  has two errors on the same tree (BDFT and CUFT), only one error is allowed
            //  to be logged in the table
            //  so need to check for that warning/error already existing in the table and skip if it is
            List<ErrorLogDO> currentList = DAL.Read<ErrorLogDO>("ErrorLog", null, null);

            foreach (ErrorLogDO eld in errList)
            {
                if (currentList.FindIndex(el => el.TableName == eld.TableName && el.CN_Number == eld.CN_Number &&
                            el.ColumnName == eld.ColumnName && el.Level == eld.Level) != null)
                {
                    if (eld.DAL == null)
                        eld.DAL = DAL;
                    eld.Save();
                }   //  endif
            }   //  end foreach loop

            return;
        }   //  end SaveErrorMessages


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
            ArrayList justSpecies = new ArrayList();
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("SELECT DISTINCT Species FROM ");
            if (whichTable == "Tree")
            {
                sb.Append(whichTable);
                sb.Append(" WHERE CountOrMeasure='M'");
            }
            else sb.Append(whichTable);
            fileName = checkFileName(fileName);
            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                sqlcmd.CommandText = sb.ToString();
                sqlcmd.ExecuteNonQuery();

                SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
                if (sdrReader.HasRows)
                {
                    while (sdrReader.Read())
                    {
                        string temp = sdrReader.GetString(0);
                        if (temp != "")
                            justSpecies.Add(temp);
                    }   //  end while read
                }   //  endif
                sqlconn.Close();
            }   //  end using
            return justSpecies;
        }   //  end GetJustSpecies


        public ArrayList GetJustPrimaryProduct()
        {
            ArrayList justProduct = new ArrayList();
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("SELECT DISTINCT PrimaryProduct FROM TreeDefaultValue");
            fileName = checkFileName(fileName);
            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                sqlcmd.CommandText = sb.ToString();
                sqlcmd.ExecuteNonQuery();

                SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
                if (sdrReader.HasRows)
                {
                    while (sdrReader.Read())
                    {
                        string temp = sdrReader.GetString(0);
                        if (temp != "")
                            justProduct.Add(temp);
                    }   //  end while read
                }   //  endif
                sqlconn.Close();
            }   //  end using
            return justProduct; ;
        }   //  end GetJustPrimaryProduct


        public ArrayList GetJustSampleGroups()
        {
            ArrayList justSampleGroups = new ArrayList();
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("SELECT Code FROM SampleGroup GROUP BY Code");
            List<SampleGroupDO> SgList = DAL.Read<SampleGroupDO>("SampleGroup", "GROUP BY Code", null);
           foreach(SampleGroupDO sdr in SgList)
            {
                string temp = sdr.Code.ToString();
                justSampleGroups.Add(temp);
            }
            //            fileName = checkFileName(fileName);
            //            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //            {
            //                sqlconn.Open();
            //                SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //                sqlcmd.CommandText = sb.ToString();
            //                sqlcmd.ExecuteNonQuery();

            //                SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
            //                if (sdrReader.HasRows)
            //                {
    
           // while (sdrReader.Read())
             //       {
               //         string temp = sdrReader.GetString(0);
                 //       justSampleGroups.Add(temp);
                   // }   //  end while read
//                }   //  endif
//                sqlconn.Close();
//            }   //  end using
            return justSampleGroups;
        }   //  end GetJustSampleGroups

// *  July 2017 -- changes to Region 8 volume equations -- this code is no longer used
// * July 2017 --  decision made to hold off on releasing the changes to R8 volume equations
// * so this code is replaced.
        public ArrayList GetProduct08Species()
        {
            ArrayList justSpecies = new ArrayList();

            List<TreeDefaultValueDO> tdvList = DAL.Read<TreeDefaultValueDO>("TreeDefaultValue", "WHERE PrimaryProduct = ? GROUP BY Species", "08");

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
            List<TreeDO> tList = DAL.Read<TreeDO>("Tree", "GROUP BY Species,SampleGroup_CN", null);
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
            return DAL.Read<TreeDO>("Tree", "GROUP BY Species", null);
        }   //  end GetUniqueSpecies


        public string[,] GetStrataUnits(long currST_CN)
        {
            //  Get all units for the current stratum
            int nthRow = 0;
            string[,] unitAcres = new string[25, 2];

            List<StratumDO> stList = DAL.Read<StratumDO>("Stratum", "WHERE Stratum_CN = ?", currST_CN);
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
            ArrayList unitStratum = new ArrayList();

            List<CuttingUnitDO> cList = DAL.Read<CuttingUnitDO>("CuttingUnit", "WHERE Code = ?", currUnit);
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
            //  retrieve species, product and both DIBs from volume equation table
            List<JustDIBs> jstDIBs = new List<JustDIBs>();

            List<VolumeEquationDO> volList = DAL.Read<VolumeEquationDO>("VolumeEquation", "GROUP BY Species,PrimaryProduct", null);
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
            //  make sure filename is complete
            fileName = checkFileName(fileName);

            //  build query string
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("SELECT DISTINCT Species,LiveDead,PrimaryProduct FROM TreeDefaultValue");
            List<TreeDefaultValueDO> tdvList = new List<TreeDefaultValueDO>();

            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                sqlcmd.CommandText = sb.ToString();
                sqlcmd.ExecuteNonQuery();
                SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
                if (sdrReader.HasRows)
                {
                    while (sdrReader.Read())
                    {
                        TreeDefaultValueDO td = new TreeDefaultValueDO();
                        td.Species = sdrReader.GetString(0);
                        td.LiveDead = sdrReader.GetString(1);
                        td.PrimaryProduct = sdrReader.GetString(2);
                        tdvList.Add(td);
                    }   //  end while read
                }   //  endif
                sqlconn.Close();
            }   //  end using
            return tdvList;
        }   //  end GetUniqueSpeciesProductLiveDead


        public List<RegressGroups> GetUniqueSpeciesGroups()
        {
            //  used for Local Volume table

            List<TreeDO> tList = DAL.Read<TreeDO>("Tree", "WHERE CountOrMeasure = ? GROUP BY Species,SampleGroup_CN,LiveDead", "M");

            List<RegressGroups> rgList = new List<RegressGroups>();
            foreach (TreeDO t in tList)
            {
                if(!rgList.Exists(r=>r.rgSpecies==t.Species && r.rgLiveDead == t.LiveDead && r.rgProduct==t.SampleGroup.PrimaryProduct))
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
            return DAL.Read<TreeDO>("Tree", "WHERE SampleGroup_CN = ? GROUP BY Species,LiveDead,Grade,STM", SG_CN);
        }   //  end GetDistinctSpecies



        public List<TreeDO> getLCDtrees(LCDDO currLCD, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Stratum JOIN SampleGroup WHERE Tree.LiveDead = ? AND Tree.Species = ? AND Tree.Grade = ? AND ");
            sb.Append("Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND SampleGroup.Code = ? AND ");
            sb.Append("Stratum.Code = ? AND Tree.CountOrMeasure = ? AND Tree.STM = ?");
            return DAL.Read<TreeDO>("Tree", sb.ToString(), currLCD.LiveDead, currLCD.Species, currLCD.TreeGrade, 
                                            currLCD.SampleGroup, currLCD.Stratum, cntMeas, currLCD.STM);
        }   //  end getLCDtrees


        public List<TreeDO> getPOPtrees(POPDO currPOP, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Stratum JOIN SampleGroup WHERE Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("SampleGroup.Code = ? AND Stratum.Code = ? AND Tree.CountOrMeasure = ? AND Tree.STM = ?");
            return DAL.Read<TreeDO>("Tree", sb.ToString(), currPOP.SampleGroup, currPOP.Stratum, cntMeas, currPOP.STM);
        }   //  end getPOPtrees


        public List<TreeDO> getPROtrees(PRODO currPRO, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("JOIN Stratum JOIN SampleGroup JOIN CuttingUnit WHERE Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN AND ");
            sb.Append("SampleGroup.Code = ? AND Stratum.Code = ? AND CuttingUnit.Code = ? AND Tree.CountOrMeasure = ? ");
            sb.Append("AND Tree.STM = ?");
            return DAL.Read<TreeDO>("Tree", sb.ToString(), currPRO.SampleGroup, currPRO.Stratum, currPRO.CuttingUnit, cntMeas, currPRO.STM);
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
            return;
        }   //  end InsertReports


        public List<ReportsDO> GetReports()
        {
            //  Retrieve reports
            return DAL.Read<ReportsDO>("Reports", "ORDER BY ReportID", null);
        }   //  end GetReports


        public List<ReportsDO> GetSelectedReports()
        {
            return DAL.Read<ReportsDO>("Reports", "WHERE Selected = ? OR Selected = ? ORDER BY ReportID", "True", "1");
        }   //  end GetSelectedReports



        public void updateReports(List<ReportsDO> reportList)
        {
            //  this updates the reports list after user has selected reports
            //  make sure filename is complete
 //           fileName = checkFileName(fileName);

            //  open connection and update records
//            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
//            {
//                sqlconn.Open();
//                SQLiteCommand sqlcmd = sqlconn.CreateCommand();
                StringBuilder sb = new StringBuilder();

                foreach (ReportsDO rdo in reportList)
                {
                    sb.Clear();
                    sb.Append("UPDATE Reports SET Selected='");
                    sb.Append(rdo.Selected);
                    sb.Append("' WHERE ReportID='");
                    sb.Append(rdo.ReportID);
                    sb.Append("';");
                    DAL.Execute(sb.ToString());
//                    sqlcmd.CommandText = sb.ToString();
//                    sqlcmd.ExecuteNonQuery();
                }   //  end foreach loop
//                sqlconn.Close();
//            }   //  end using            
            return;
        }   //  end updateReports


        public void deleteReport(string reptToDelete)
        {
            //  deletes the report from the reports table
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            //  open connection and delete record
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
           // {
             //   sqlconn.Open();
              //  SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                StringBuilder sb = new StringBuilder();
                sb.Clear();
                sb.Append("DELETE FROM Reports WHERE ReportID='");
                sb.Append(reptToDelete);
                sb.Append("';");
                DAL.Execute(sb.ToString());

     //       sqlcmd.CommandText = sb.ToString();
       //         sqlcmd.ExecuteNonQuery();

         //       sqlconn.Close();
           // }   //  end using
            return;
        }   //  end deleteReport


//  Specific to EX reports
// *******************************************************************************************
        public List<exportGrades> GetExportGrade()
        {
            List<exportGrades> egList = new List<exportGrades>();
            // make sure filename is complete
            fileName = checkFileName(fileName);

            //  open connection and get data
            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                // open connection
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();
                sqlcmd.CommandText = "SELECT * FROM ExportValues";
                sqlcmd.ExecuteNonQuery();

                //  Open reader and get data
                SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
                if (sdrReader.HasRows)
                {
                    while (sdrReader.Read())
                    {
                        exportGrades eg = new exportGrades();
                        eg.exportSort = sdrReader.GetString(0);
                        eg.exportGrade = sdrReader.GetString(1);
                        eg.exportCode = sdrReader.GetString(2);
                        eg.exportName = sdrReader.GetString(3);
                        eg.minDiam = sdrReader.GetDouble(4);
                        eg.minLength = sdrReader.GetDouble(5);
                        eg.minBDFT = sdrReader.GetDouble(6);
                        eg.maxDefect = sdrReader.GetDouble(7);
                        egList.Add(eg);
                    }   //  end while
                }   //  endif reader has rows
                sqlconn.Close();
            }   //  end using

            return egList;
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
                    sb.Clear();
                    sb = eg.createInsertQuery(eg, "sort");
                    DAL.Execute(sb.ToString());
                   // sqlcmd.CommandText = sb.ToString();
                   // sqlcmd.ExecuteNonQuery();
                }   //  end foreach on sortList

                foreach (exportGrades eg in gradeList)
                {
                    sb.Clear();
                    sb = eg.createInsertQuery(eg, "grade");
                    DAL.Execute(sb.ToString());

                //sqlcmd.CommandText = sb.ToString();
                 //   sqlcmd.ExecuteNonQuery();
                }   //  end foreach on gradeList
                //sqlconn.Close();
            //}   //  end using

            return;
        }   //  end SaveExportGrade


//  specific to stewardship reports (R208)
// *******************************************************************************************
        public List<StewProductCosts> getStewCosts()
        {
            List<StewProductCosts> stList = new List<StewProductCosts>();
            //  make sure filename is complete
            fileName = checkFileName(fileName);

            //  open connection and get data
            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();
                sqlcmd.CommandText = "SELECT * FROM StewProductCosts";
                sqlcmd.ExecuteNonQuery();

                //  open reader
                SQLiteDataReader sdrReader = sqlcmd.ExecuteReader();
                if (sdrReader.HasRows)
                {
                    while (sdrReader.Read())
                    {
                        StewProductCosts st = new StewProductCosts();
                        st.costUnit = sdrReader.GetString(0);
                        st.costSpecies = sdrReader.GetString(1);
                        st.costProduct = sdrReader.GetString(2);
                        st.costPounds = sdrReader.GetFloat(3);
                        st.costCost = sdrReader.GetFloat(4);
                        st.scalePC = sdrReader.GetFloat(5);
                        st.includeInReport = sdrReader.GetString(6);
                        stList.Add(st);
                    }   //  end while read
                }   //  endif has rows
                sqlconn.Close();
            }   //  end using
            return stList;
        }   //  end getStewCosts


        public void SaveStewCosts(List<StewProductCosts> stList)
        {
            //  make sure filename is complete
            // fileName = checkFileName(fileName);

            //  open connection and save data
            // using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            // {
            // sqlconn.Open();
            // SQLiteCommand sqlcmd = sqlconn.CreateCommand();
            StringBuilder sb = new StringBuilder();
            foreach (StewProductCosts st in stList)
            {
                    sb.Clear();
                    sb = st.createInsertQuery(st);
                    DAL.Execute(sb.ToString());
                    //sqlcmd.CommandText = sb.ToString();
                    //sqlcmd.ExecuteNonQuery();
             }   //  end foreach loop                
                //sqlconn.Close();
            //}   //  end using
            return;
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
            //  see note above concerning this code
            //List<PRODO> proList = getPRO();
            //foreach (PRODO prdo in proList)
            //    prdo.Delete();
            //  make sure filename is complete
            fileName = checkFileName(fileName);

            //   open connection and delete data
            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                //  open connection
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                //  delete all rows
                sqlcmd.CommandText = "DELETE FROM PRO WHERE PRO_CN>0";
                sqlcmd.ExecuteNonQuery();
                sqlconn.Close();
            }   //  end using
            return;
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

        // To make filename complete
        private string checkFileName(string fileName)
        {
            if (!fileName.StartsWith("Data Source"))
                return fileName.Insert(0, "Data Source = ");

            return fileName;

        }   //  end checkFileName


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

    }   //  end CPbusinessLayer
}