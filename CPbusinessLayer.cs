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
        private string fileName;
        private CruiseDAL.DAL DAL;

       // string[] parameterValues = new string[11];
       // string[] selectValues = new string[11];
       // string[] selectParameters = new string[11];
       // IEnumerable<SaleDO> saleIEnumerable = new IEnumerable<SaleDO>().Read();
       // IEnumerable<StratumDO> stIEnumerable = new IEnumerable<StratumDO>().Read();
       // IEnumerable<CuttingUnitDO> cutIEnumerable = new IEnumerable<CuttingUnitDO>().Read();
       // IEnumerable<SampleGroupDO> sgIEnumerable = new IEnumerable<SampleGroupDO>().Read();
       // IEnumerable<TreeDO> tIEnumerable = new IEnumerable<TreeDO>().Read();
       // IEnumerable<TreeDefaultValueDO> tdvIEnumerable = new IEnumerable<TreeDefaultValueDO>().Read();
       // IEnumerable<PlotDO> pIEnumerable = new IEnumerable<PlotDO>().Read();
       // IEnumerable<VolumeEquationDO> volIEnumerable = new IEnumerable<VolumeEquationDO>().Read();
        #endregion

        public CPbusinessLayer(CruiseDAL.DAL dal, string fileName)
        {
            DAL = dal;
            this.fileName = fileName;
        }



// *******************************************************************************************
        public string createNewTableQuery(string tableName, params string[] values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(tableName);
            sb.Append(" (");

            //  add values to command
            foreach (string s in values)
                sb.Append(s);

            //  add close parens and semi-colon
            sb.Append(");");

            return sb.ToString();
        }   //  end createNewTableQuery



        public string[] createExportArray()
        {
            //  create valuesIEnumerable for export grade sorts and grade
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
            //  create valuesIEnumerable for log matrix output files for Region 10
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
            //  create valuesIEnumerable for stewardship product costs IEnumerable for Region 2
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
            fileName = checkFileName(fileName);

            string[] values;
            string queryString = "";
            //  build query
            if(tableName == "ExportValues")
            {
                values = createExportArray();
                queryString = createNewTableQuery(tableName, values);
            }
            else if(tableName == "LogMatrix")
            {
                values = createLogMatrixArray();
                queryString = createNewTableQuery(tableName, values);
            }
            else if(tableName == "StewProductCosts")
            {
                values = createStewCostsArray();
                queryString = createNewTableQuery("StewProductCosts", values);
            
            }   //  endif 
            
            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();
                sqlcmd.CommandText = queryString;
                sqlcmd.ExecuteNonQuery();

                sqlconn.Close();
            }   //  end using

            return 1;
        }   //  end createNewTable


        public bool doesTableExist(string tableName)
        {
            //  make sure filename is complete
            fileName = checkFileName(fileName);
            StringBuilder sb = new StringBuilder();

            //  find table in database
            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();
                sb.Remove(0, sb.Length);
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
                }   //  endif Reader has rows

            }   //  end using
        }   //  end doesTableExist

/*        public void ContainColumn(string ColumnName, string TableName)
        {
            IEnumerable<TreeCalculatedValuesDO> tcvIEnumerable = DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues",null,null);

            if (tcvIEnumerable.Contains(TreeCalculatedValuesDO tipWood))
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
        public IEnumerable<SaleDO> getSale()
        {
            return DAL.From<SaleDO>().Read("Sale", null, null);
        }   //  end getSale

        public IEnumerable<StratumDO> getStratum()
        {
            return DAL.From<StratumDO>().Read("Stratum", "ORDER BY Code", null);
        }   //  end getStratum

        public IEnumerable<CuttingUnitDO> getCuttingUnits()
        {
            return DAL.From<CuttingUnitDO>().Read("CuttingUnit", "ORDER BY CODE", null);
        }   //  end getCuttingUnits

        public IEnumerable<CuttingUnitDO> getPaymentUnits()
        {
            return DAL.From<CuttingUnitDO>().Read("CuttingUnit", "GROUP BY PaymentUnit", null);
        }   //  end getPaymentUnits

        public IEnumerable<CuttingUnitDO> getLoggingMethods()
        {
            //  returns unique logging methods
            return DAL.From<CuttingUnitDO>().Read("CuttingUnit", "GROUP BY LoggingMethod", null);
        }   //  end getLoggingMethods

        public IEnumerable<CuttingUnitStratumDO> getCuttingUnitStratum(long currStratumCN)
        {
            return DAL.From<CuttingUnitStratumDO>().Read("CuttingUnitStratum", "WHERE Stratum_CN = ?", currStratumCN);
        }   //  end getCuttingUnitStratum
        
        
        public IEnumerable<PlotDO> getPlots()
        {
            return DAL.From<PlotDO>().Read("Plot", "ORDER BY PLOTNUMBER", null);
        }   //  end getPlots

        public IEnumerable<SampleGroupDO> getSampleGroups()
        {
            return DAL.From<SampleGroupDO>().Read("SampleGroup", null, null);
        }   //  end getSampleGroups

        public IEnumerable<TreeDefaultValueDO> getTreeDefaults()
        {
            return DAL.From<TreeDefaultValueDO>().Read("TreeDefaultValue", null, null);
        }   //  end getTreeDefaults

        public IEnumerable<CountTreeDO> getCountTrees()
        {
            return DAL.From<CountTreeDO>().Read("CountTree", null, null);
        }   //  end getCountTrees

        public IEnumerable<TreeDO> getTrees()
        {
            return DAL.From<TreeDO>().Read("Tree", null, null);
        }   //  end getTrees

        public IEnumerable<LogDO> getLogs()
        {
            return DAL.From<LogDO>().Read("Log", null, null);
        }   //  end getLogs

        public IEnumerable<LogStockDO> getLogStock()
        {
            return DAL.From<LogStockDO>().Read("LogStock", null, null);
        }   //  end getLogStock

        public IEnumerable<VolumeEquationDO> getVolumeEquations()
        {
            return DAL.From<VolumeEquationDO>().Read("VolumeEquation", null, null);
        }   //  end getVolumeEquations

        public IEnumerable<ValueEquationDO> getValueEquations()
        {
            return DAL.From<ValueEquationDO>().Read("ValueEquation", null, null);
        }   //  end getValueEquations


        public IEnumerable<BiomassEquationDO> getBiomassEquations()
        {
            return DAL.From<BiomassEquationDO>().Read("BiomassEquation", null, null);
        }   //  end getBiomassEquations
        
        public IEnumerable<QualityAdjEquationDO> getQualAdjEquations()
        {
            return DAL.From<QualityAdjEquationDO>().Read("QualityAdjEquation", null, null);
        }   //  end getQualAdjEquations


        public IEnumerable<TreeEstimateDO> getTreeEstimates()
        {
            return DAL.From<TreeEstimateDO>().Read("TreeEstimate", null, null);
        }   //  end getTreeEstimates

//  TreeCalculatedValues
// *******************************************************************************************
        public IEnumerable<TreeCalculatedValuesDO> getTreeCalculatedValues()
        {
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", null, null);
        }   //  end getTreeCalculatedValues

        public IEnumerable<TreeCalculatedValuesDO> getTreeCalculatedValuesTree(long tree_CN)
        {
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", "Where Tree_CN = ?", tree_CN);
        }

        public IEnumerable<TreeCalculatedValuesDO> getTreeCalculatedValues(long currStratumCN)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree WHERE Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("AND Tree.Stratum_CN = ?");
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", sb.ToString(), currStratumCN);
        }   //  end getTreeCalculatedValues


        public IEnumerable<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN, int currUnitCN)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree WHERE Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("AND Tree.Stratum_CN = ? AND Tree.CuttingUnit_CN = ?");
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", sb.ToString(), currStratumCN, currUnitCN);
        }   //  end getTreeCalculatedValues


        public IEnumerable<TreeCalculatedValuesDO> getTreeCalculatedValues(string currSP)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree WHERE Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("AND Tree.Species = ?");
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", sb.ToString(), currSP);
        }   //  end getTreeCalculatedValues


        public IEnumerable<TreeCalculatedValuesDO> getTreeCalculatedValues(int currStratumCN, string orderBy)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree WHERE Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("AND Tree.Stratum_CN = ? ORDER BY ");
            sb.Append(orderBy);
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", sb.ToString(), currStratumCN);
        }   //  end getTreeCalculatedValues


        public IEnumerable<TreeCalculatedValuesDO> getTreeCalculatedValues(string currCL, string orderBy, string reportType)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
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

            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", sb.ToString(), currCL);
        }   //  end getTreeCalculatedValues


        public IEnumerable<TreeCalculatedValuesDO> getStewardshipTrees(string currUnit, string currSP, string currPP)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE SampleGroup.CutLeave = ? ");
            sb.Append("AND CuttingUnit.Code = ? AND Tree.Species = ? AND SampleGroup.PrimaryProduct = ?");
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", sb.ToString(), "C", currUnit, currSP, currPP);
        }   //  end getStewardshipTrees


        public IEnumerable<TreeCalculatedValuesDO> getRegressTrees(string currSP, string currPR, string currLD, string currCM)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
            sb.Append("Tree.Species = ? AND SampleGroup.PrimaryProduct = ? AND ");
            sb.Append("Tree.LiveDead = ? AND Tree.CountOrMeasure = ?");
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", sb.ToString(), currSP, currPR, currLD, currCM);
        }   //  end getRegressTrees


//  Log Table and Log Stock Table
// *******************************************************************************************
        public IEnumerable<LogDO> getTreeLogs(long currTreeCN)
        {
            return DAL.From<LogDO>().Read("Log", "WHERE Tree_CN = ?", currTreeCN);
        }   //  getTreeLogs


        public IEnumerable<LogDO> getTreeLogs()
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0,sb.Length);
            sb.Append("JOIN Log ON Log.Tree_CN = Tree.Tree_CN");
            return DAL.From<LogDO>().Read("Log",sb.ToString());
        }   //  end getTreeLogs


        public IEnumerable<LogStockDO> getLogStockSorted()
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("LEFT JOIN Plot ON Tree.Plot_CN = Plot.Plot_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN WHERE ");
            sb.Append("SampleGroup.CutLeave = 'C' ORDER BY Stratum.Code,CAST(CuttingUnit.Code AS NUMERIC),Plot.PlotNumber");
            return DAL.From<LogStockDO>().Read("LogStock", sb.ToString());
        }   //  end getLogStockSorted



        public IEnumerable<LogStockDO> getCullLogs(long currTreeCN, string currGrade)
        {
            return DAL.From<LogStockDO>().Read("LogStock", "WHERE Tree_CN = ? AND Grade = ?", currTreeCN, currGrade);
        }   //  end getCullLogs


        public IEnumerable<LogStockDO> getLogDIBs()
        {
            return DAL.From<LogStockDO>().Read("LogStock", "GROUP BY DIBClass");
        }   //  end getLogDIBS


        public IEnumerable<LogStockDO> getCutOrLeaveLogs(string currCL)
        {
            //  Pulls logs for Region 10 L1 output files
            StringBuilder sb = new StringBuilder();
            sb.Remove(0,sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
            sb.Append("SampleGroup.CutLeave = ? ORDER BY Species");
            return DAL.From<LogStockDO>().Read("LogStock",sb.ToString(),currCL);
        }   //  end getCutLogs


        public IEnumerable<LogStockDO> getCutLogs()
        {
            //  Pulls just cut logs
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
            sb.Append("SampleGroup.CutLeave = 'C' ORDER BY Species");
            return DAL.From<LogStockDO>().Read("LogStock", sb.ToString());
        }   //  end getCutLogs


        public IEnumerable<LogStockDO> getCutLogs(string currCL, string currSP, string currPP, int byDIBclass)
        {
            //  Needed for R501 report
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.Species = ? AND SampleGroup.PrimaryProduct = ?");
            if (byDIBclass == 1)
                sb.Append(" GROUP BY DIBClass");
            return DAL.From<LogStockDO>().Read("LogStock", sb.ToString(), currCL, currSP, currPP);
        }   //  end getCutLogs


        public IEnumerable<LogStockDO> getCutLogs(string currSP, string currSort, string currGrade)
        {
            //  used in EX reports
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN LogStock ON LogStock.Tree_CN = Tree.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.Species = ? ");
            sb.Append("AND LogStock.ExportGrade = ? AND LogStock.Grade = ?");
            return DAL.From<LogStockDO>().Read("LogStock", sb.ToString(), "C", currSP, currSort, currGrade);
        }   //  end getCutLogs


        public IEnumerable<LogStockDO> getLogSorts(string currSP)
        {
            //  used in EX reports
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN LogStock ON LogStock.Tree_CN = Tree.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            if(currSP != "")
                sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.Species = ? ");
            else sb.Append("WHERE SampleGroup.CutLeave = ?");
            sb.Append("GROUP BY ExportGrade,Grade");
            if (currSP != "")
                return DAL.From<LogStockDO>().Read("LogStock", sb.ToString(), "C", currSP);
            else return DAL.From<LogStockDO>().Read("LogStock", sb.ToString(), "C");
        }   //  end getLogSorts


        public IEnumerable<LogStockDO> getLogSpecies(string currSP, float minDIB, float maxDIB, 
                                              string currST, string currGrade)
        {
            //  used mostly for BLM09 and BLM10 reports
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
            sb.Append("SampleGroup.CutLeave = 'C' AND Stratum.Code = ? AND CountOrMeasure = 'M' AND ");
            sb.Append("Tree.Species = ? AND LogStock.Grade = ? AND DIBclass >= ? AND DIBclass <= ? ");
            return DAL.From<LogStockDO>().Read("LogStock", sb.ToString(), currST, currSP, currGrade, minDIB, maxDIB);
        }   //  end getLogSpecies


        public IEnumerable<LogStockDO> getStrataLogs(string currST, string currGrade)
        {
            //  used mostly for BLM reports
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN WHERE ");
            sb.Append("SampleGroup.CutLeave = 'C' AND Stratum.Code = ? AND CountOrMeasure = 'M' AND ");
            sb.Append("LogStock.Grade = ? ");
            return DAL.From<LogStockDO>().Read("LogStock", sb.ToString(), currST, currGrade);
        }   //  end getStrataLogs



        public IEnumerable<LogStockDO> getStrataLogs(string currSP, string currST, string currSG, string currSTM, string currGrade)
        {
            //  used in BLM reports
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN = LogStock.Tree_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = 'C' AND CountOrMeasure = 'M' AND STM = ? ");
            sb.Append("AND Stratum.Code = ? AND Species = ? AND SampleGroup.Code = ? AND ");
            sb.Append("LogStock.Grade = ?");
            return DAL.From<LogStockDO>().Read("LogStock", sb.ToString(), currSTM, currST, currSP, currSG, currGrade);
        }   //  end getStrataLogs


        public IEnumerable<LogStockDO> getUnitLogs(long currST_CN, long currCU_CN, string currGrade, string currSTM)
        {
            // mostly for BLM reports
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN == LogStock.Tree_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = 'C' AND Stratum.Stratum_CN = ? AND ");
            sb.Append("CuttingUnit.CuttingUnit_CN = ? AND LogStock.Grade = ? AND Tree.STM = ? ");
            return DAL.From<LogStockDO>().Read("LogStock", sb.ToString(), currST_CN, currCU_CN, currGrade, currSTM);
        }   //  end getUnitLogs


//  Error Log Table
// *******************************************************************************************
        public IEnumerable<ErrorLogDO> getErrorMessages(string errLevel, string errProgram)
        {
            if (errProgram == "FScruiser")
                return DAL.From<ErrorLogDO>().Read("ErrorLog", "WHERE Level = ? AND (Program LIKE'%FScruiser%' OR Program LIKE '%CruiseSystemManager%' OR Program LIKE '%CruiseManage%')", errLevel);
            else if (errProgram == "CruiseProcessing")
                return DAL.From<ErrorLogDO>().Read("ErrorLog", "WHERE Level = ? AND Program = ?", errLevel, errProgram);

            return new List<ErrorLogDO>();
        }   //  end getErrorMessages


//  LCD POP and PRO Tables
// *******************************************************************************************
        public IEnumerable<LCDDO> getLCD()
        {
            return DAL.From<LCDDO>().Read("LCD", null, null);
        }   //  end getLCD

        public IEnumerable<POPDO> getPOP()
        {
            return DAL.From<POPDO>().Read("POP", null, null);
        }   //  end getPOP

        //public IEnumerable<PRODO> getPRO()
        //{
        //    return DAL.From<PRODO>().Read("PRO", null, null);
        //}   //  end getPRO
        public IEnumerable<PRODO> getPRO(string code = null)
        {
            return DAL.From<PRODO>().Read("PRO", code != null ? "Where Stratum = ?" : null,code);
        }

        //  Miscellaneous tables like log matrix, regression results
        // *******************************************************************************************
        public IEnumerable<LogMatrixDO> getLogMatrix(string currReport)
        {
            return DAL.From<LogMatrixDO>().Read("LogMatrix", "WHERE ReportNumber = ?", currReport);
        }   //  end getLogMatrix


        public IEnumerable<RegressionDO> getRegressionResults()
        {
            //  pulls regression data for local volume reports
            return DAL.From<RegressionDO>().Read("Regression", null, null);
        }   //  end getRegressionResults


//  functions returning single elements
// *******************************************************************************************
        //  like region
        public string getRegion()
        {
            //  retrieve sale record
            return getSale().First().Region;

            //IEnumerable<SaleDO> saleIEnumerable = getSale();
            //return saleIEnumerable[0].Region;
        }   //  end getRegion

        //  forest
        public string getForest()
        {
            return getSale().First().Forest;

            //IEnumerable<SaleDO> saleIEnumerable = getSale();
            //return saleIEnumerable[0].Forest;
        }   //  end getForest

        //  district
        public string getDistrict()
        {
            return getSale().First().District;

            //IEnumerable<SaleDO> saleIEnumerable = getSale();
            //return saleIEnumerable[0].District;
        }   //  end getDistrict

        public string getCruiseNumber()
        {
            return getSale().First().SaleNumber;

            //IEnumerable<SaleDO> saleIEnumerable = getSale();
            //return saleIEnumerable[0].SaleNumber;
        }   //  end getCruiseNumber

        public string getUOM(int currStratumCN)
        {
            SampleGroupDO sgIEnumerable = DAL.From<SampleGroupDO>().Read("SampleGroup", "WHERE Stratum_CN = ?", currStratumCN).First();
            return sgIEnumerable.UOM;
        }   //  end getUOM

//  Sample Groups
// *******************************************************************************************
        public IEnumerable<SampleGroupDO> getSampleGroups(int currStratumCN)
        {
            return DAL.From<SampleGroupDO>().Read("SampleGroup", "WHERE Stratum_CN = ?", currStratumCN);
        }   //  end getSampleGroups

        public IEnumerable<SampleGroupDO> getSampleGroups(string qString)
        {
            //  returns just unique sample groups
            return DAL.From<SampleGroupDO>().Read("SampleGroup", qString, "C");
        }   //  end getSampleGroups


// *******************************************************************************************
        //  Like variable log length from global configuration
        public string getVLL()
        {
            if (DAL.From<GlobalsDO>().Read("Globals", "WHERE Key = ? AND Block = ?", "VLL", "Global").Count() > 0)
                return "V";
            else return "false";
        }   //  end getVLL


//  Stratum table
// *******************************************************************************************
        public IEnumerable<StratumDO> GetCurrentStratum(string currentStratum)
        {
            //  Pull current stratum from table
            return DAL.From<StratumDO>().Read("Stratum", "WHERE Code = ?", currentStratum);
        }   //  end GetCurrentStratum


        //  just FIXCNT methods
        public IEnumerable<StratumDO> justFIXCNTstrata()
        {
            return DAL.From<StratumDO>().Read("Stratum", "WHERE Method = ?", "FIXCNT");
        }   //  end justFIXCNTstrata


//  Plot table
// *******************************************************************************************
        public IEnumerable<PlotDO> GetStrataPlots(string currStratum)
        {
            return DAL.From<PlotDO>().Read("Plot", "WHERE Stratum_CN = ?",
                (DAL.From<StratumDO>().Read("Stratum", "WHERE Code = ?", currStratum).First()).Stratum_CN);
        }   //  end GetStrataPlots


        public IEnumerable<PlotDO> getPlotsOrdered()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("JOIN Stratum JOIN CuttingUnit WHERE Plot.Stratum_CN = Stratum.Stratum_CN AND ");
            sb.Append("Plot.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN");
            sb.Append(" ORDER BY Stratum.Code,CuttingUnit.Code,PlotNumber");
            return DAL.From<PlotDO>().Read("Plot",sb.ToString());
        }   //  end getPlotsOrdered


//  LCD table
// *******************************************************************************************
        public IEnumerable<LCDDO> getLCDOrdered(string searchString, string orderBy, string currCutLeave, 
                                            string currST, string currPP)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append(searchString);
            sb.Append(orderBy);
            if (currST == "")
                return DAL.From<LCDDO>().Read("LCD", sb.ToString(), currCutLeave);
            else if (currCutLeave == "")
                return DAL.From<LCDDO>().Read("LCD", sb.ToString(), currST);
            else if (currCutLeave != "" && currST != "" && currPP != "")
                return DAL.From<LCDDO>().Read("LCD", sb.ToString(), currCutLeave, currST, currPP);
            else return DAL.From<LCDDO>().Read("LCD", sb.ToString(), currCutLeave, currST);
        }   //  end getLCDOrdered


        public IEnumerable<LCDDO> getLCDOrdered(string searchString, string orderBy, string currCutLeave, 
                                            string currPP)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append(searchString);
            sb.Append(orderBy);
            if (currPP != "")
                return DAL.From<LCDDO>().Read("LCD", sb.ToString(), currCutLeave, currPP);
            else return  DAL.From<LCDDO>().Read("LCD", sb.ToString(), currCutLeave);
        }   //  end getLCDOrdered


        public IEnumerable<LCDDO> GetLCDgroup(string currST, int currRPT, string currCL)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
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
                        yield return ld;
                    }   //  end while From
                }   //  endif
                sqlconn.Close();
            }   //  end using

            fileName = prevFileName;
        }   //  end GetLCDgroup

        public IEnumerable<LCDDO> GetLCDgroup(string fileName, string groupedBy)
        {
            //  overloaded for stat report  ST3
            StringBuilder sb = new StringBuilder();
            string tempFileName = checkFileName(fileName);
            using (SQLiteConnection sqlconn = new SQLiteConnection(tempFileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();
                sb.Remove(0, sb.Length);
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
                        yield return ld;
                    }   //  end while Reader
                }   //  endif Reader has rows
                sqlconn.Close();
            }   //  end using
        }   //  end GetLCDgroup


        public IEnumerable<LCDDO> GetLCDdata(string whereClause, LCDDO lcd, int reportNum, string currCL)
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
                    return DAL.From<LCDDO>().Read("LCD", whereClause, lcd.Stratum, lcd.Species, lcd.PrimaryProduct, lcd.UOM,
                                            lcd.LiveDead, lcd.CutLeave, lcd.Yield, lcd.SampleGroup, lcd.TreeGrade, lcd.STM);
                                            //  September 2016 -- dropping contract species from LCD identifier
                                            //lcd.ContractSpecies, lcd.STM);
                case 2:     
                    return DAL.From<LCDDO>().Read("LCD", whereClause, lcd.Stratum, lcd.PrimaryProduct, lcd.UOM, currCL, 
                                                lcd.SampleGroup, lcd.STM);
                case 3:
                    return DAL.From<LCDDO>().Read("LCD", whereClause, lcd.Stratum, lcd.PrimaryProduct, lcd.UOM, currCL, lcd.STM);
                case 4:
                    return DAL.From<LCDDO>().Read("LCD", whereClause, lcd.Species, lcd.PrimaryProduct, lcd.SecondaryProduct, 
                                                lcd.LiveDead, lcd.ContractSpecies, "C");
                case 5:
                    return DAL.From<LCDDO>().Read("LCD", whereClause, "C", lcd.Species, lcd.PrimaryProduct, lcd.ContractSpecies);
            }   //  end switch
            return new List<LCDDO>();
        }   //  end GetLCDdata


        public IEnumerable<LCDDO> GetLCDdata(string currST, string whereClause, string orderBy)
        {
            //  works for UC1/UC2 reports -- maybe all UC reports?
            return DAL.From<LCDDO>().Read("LCD", whereClause, currST, "C", orderBy);
        }   //  end GetLCDdata


        public IEnumerable<LCDDO> GetLCDdata(string whereClause, string currST)
        {
            //  works for regional report R104
            return DAL.From<LCDDO>().Read("LCD", whereClause, currST);
        }   //  end GetLCDdata

        public LCDDO getLCDrow(PRODO prdo)
        {
            //          return ldo.CutLeave == prdo.CutLeave && ldo.Stratum == prdo.Stratum &&
            //     ldo.SampleGroup == prdo.SampleGroup && ldo.PrimaryProduct == prdo.PrimaryProduct &&
            //     ldo.SecondaryProduct == prdo.SecondaryProduct && ldo.UOM == prdo.UOM && ldo.STM == prdo.STM;
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("Where CutLeave = ? AND Stratum = ? AND SampleGroup = ? AND PrimaryProduct = ? AND SecondaryProduct = ? AND UOM = ? AND STM = ?");
            return DAL.From<LCDDO>().Read("LCD", sb.ToString(), prdo.CutLeave, prdo.Stratum, prdo.SampleGroup, prdo.PrimaryProduct, prdo.SecondaryProduct, prdo.UOM, prdo.STM).First();
        }

       public POPDO getPOProw(PRODO prdo)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("Where CutLeave = ? AND Stratum = ? AND SampleGroup = ? AND PrimaryProduct = ? AND SecondaryProduct = ? AND UOM = ? AND STM = ?");

            return DAL.From<POPDO>().Read("POP", sb.ToString(), prdo.CutLeave, prdo.Stratum, prdo.SampleGroup, prdo.PrimaryProduct, prdo.SecondaryProduct, prdo.UOM, prdo.STM).First();
        }
        public IEnumerable<TreeCalculatedValuesDO> GetLCDtrees(string currST, LCDDO ldo, string currCM)
        {
            //  captures tree calculated values for LCD summation in SumAll
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
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
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", sb.ToString(), currST, ldo.CutLeave, 
                                        ldo.SampleGroup, ldo.Species, ldo.PrimaryProduct, ldo.SecondaryProduct, 
                                        //  see comment above
                                        //ldo.UOM, ldo.LiveDead, ldo.TreeGrade, ldo.Yield, ldo.ContractSpecies, 
                                        ldo.UOM, ldo.LiveDead, ldo.TreeGrade, ldo.Yield,  
                                        ldo.STM, currCM);
        }   //  end GetLCDtrees

// POP table
// *******************************************************************************************
        public IEnumerable<TreeCalculatedValuesDO> GetPOPtrees(POPDO pdo, string currST, string currCM)
        {
            //  captures tree calculated values for POP summation in SumAll
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Tree ON Tree.Tree_CN = TreeCalculatedValues.Tree_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("WHERE Stratum.Code = ? AND SampleGroup.CutLeave = ? AND SampleGroup.Code = ? ");
            sb.Append("AND SampleGroup.PrimaryProduct = ? AND SampleGroup.SecondaryProduct = ? AND ");
            sb.Append("Tree.STM = ? AND Tree.CountOrMeasure = ?");
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues", sb.ToString(), currST,
                                pdo.CutLeave, pdo.SampleGroup, pdo.PrimaryProduct, 
                                pdo.SecondaryProduct, pdo.STM, currCM);
        }   //  end GetPOPtrees


        public IEnumerable<POPDO> GetUOMfromPOP()
        {
            return DAL.From<POPDO>().Read("POP", "GROUP BY UOM", null);
        }   //  end GetUOMfromPOP


//  PRO table
// *******************************************************************************************
        public IEnumerable<PRODO> getPROunit(string currCU)
        {
            return DAL.From<PRODO>().Read("PRO", "WHERE CuttingUnit = ?", currCU);
        }   //  end getPRO


//  Count table
// *******************************************************************************************
        public IEnumerable<CountTreeDO> getCountTrees(long currSG_CN)
        {
            return DAL.From<CountTreeDO>().Read("CountTree", " WHERE SampleGroup_CN = ?", currSG_CN);
        }   //  end overload of getCountTrees


        public IEnumerable<CountTreeDO> getCountsOrdered()
        {
            return DAL.From<CountTreeDO>().Read("CountTree", "ORDER BY CuttingUnit_CN");
        }   //  end getCountsOrdered


        public IEnumerable<CountTreeDO> getCountsOrdered(string searchString, string orderBy, string[] searchValues)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append(searchString);
            sb.Append(orderBy);
            return DAL.From<CountTreeDO>().Read("CountTree", sb.ToString(), searchValues);
        }   //  end getCountsOrdered

//  Tree table
// *******************************************************************************************
        public IEnumerable<TreeDO> getTreesOrdered(string searchString, string orderBy, string[] searchValues)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append(searchString);
            sb.Append(orderBy);
            return DAL.From<TreeDO>().Read("Tree", sb.ToString(), searchValues);
        }   //  end getTreesOrdered


        public IEnumerable<TreeDO> getTreesSorted()
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN CuttingUnit ON Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("LEFT OUTER JOIN Plot ON Tree.Plot_CN = Plot.Plot_CN ");
            sb.Append("ORDER BY Stratum.Code, CuttingUnit.Code, ifnull(Plot.PlotNumber, 0), Tree.TreeNumber;");

            return DAL.From<TreeDO>().Read("Tree", sb.ToString(), null);
        }   //  end getTreesSorted


        public IEnumerable<TreeDO> getUnitTrees(long currST_CN, long currCU_CN)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("JOIN CuttingUnit on Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = 'C' AND Tree.CountOrMeasure = 'M' ");
            sb.Append("AND Tree.Stratum_CN = ? AND Tree.CuttingUnit_CN = ?");
            return DAL.From<TreeDO>().Read("Tree", sb.ToString(), currST_CN, currCU_CN);
        }   //  end getUnitTrees


        public IEnumerable<TreeDO> getUniqueStewardGroups()
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = ? ");
            sb.Append("GROUP BY CuttingUnit_CN,Species,Tree.SampleGroup_CN");

            return DAL.From<TreeDO>().Read("Tree", sb.ToString(), "C");
        }   //  end getUniqueStewardGroups


        public IEnumerable<TreeDO> getTreeDBH(string currCL)
        {
            //  works for stand tables DIB classes for the sale
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = ? AND Tree.CountOrMeasure = 'M' ");
            sb.Append("GROUP BY DBH");
            return DAL.From<TreeDO>().Read("Tree", sb.ToString(), currCL);
        }   //  end getTreeDBH


        public IEnumerable<TreeDO> getInsureTrees(string Str, string SG, string species, string STM)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN SampleGroup, Stratum Where Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Stratum.Code = ? AND SampleGroup.Code = ? AND Tree.Species = ? AND Tree.STM = ?");

            return DAL.From<TreeDO>().Read("Tree", sb.ToString(),Str,SG,species,STM);
            //    return td.Stratum.Code == l.Stratum && td.SampleGroup.Code == l.SampleGroup && 
            //            td.Species == l.Species && td.STM == l.STM;     

        }
        //    return td.Stratum.Code == l.Stratum && td.SampleGroup.Code == l.SampleGroup && 
        //            td.Species == l.Species && td.STM == l.STM;     

        public IEnumerable<TreeDO> getTreeDBH(string currCL, string currST, string currCM)
        {
            //  works for stand tables DIB classes for current stratum
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN ");
            sb.Append("JOIN Stratum ON Tree.Stratum_CN = Stratum.Stratum_CN ");
            sb.Append("WHERE SampleGroup.CutLeave = ? AND Stratum.Code = ? AND Tree.CountOrMeasure = ? ");
            sb.Append("GROUP BY DBH");
            return DAL.From<TreeDO>().Read("Tree", sb.ToString(), currCL, currST, currCM);
        }   //  end getTreeDBH


        public IEnumerable<TreeDO> JustMeasuredTrees()
        {
            return DAL.From<TreeDO>().Read("Tree", "WHERE CountOrMeasure = ?", "M");
        }   //  end JustMeasuredTrees


        public IEnumerable<TreeDO> JustMeasuredTrees(long currST_CN)
        {
            return DAL.From<TreeDO>().Read("Tree", "WHERE CountOrMeasure = ? AND Stratum_CN = ?", "M", currST_CN);
        }   //  end overloaded JustMeasuredTrees


        public IEnumerable<TreeDO> JustUnitTrees(long currST_CN, long currCU_CN)
        {
            return DAL.From<TreeDO>().Read("Tree", "WHERE Stratum_CN = ? AND CuttingUnit_CN = ?", currST_CN, currCU_CN);
        }   //  end JustUnitTrees

        public IEnumerable<TreeDO> JustFIXCNTtrees(long currST_CN)
        {
            return DAL.From<TreeDO>().Read("Tree", "WHERE CountOrMeasure = ? AND Stratum_CN = ?", "C", currST_CN);
        }   //  end JustFIXCNTtrees
        public IEnumerable<TreeDO> getTreeStratum(long currST_CN)
        {
            return DAL.From<TreeDO>().Read("Tree", "WHERE Stratum_CN = ?", currST_CN);
        }
        public IEnumerable<CountTreeDO> getJustCounts(long cuCN, string code)
        {
            return DAL.From<CountTreeDO>().Read("CountTree", "WHERE CuttingUnit_CN = ? AND SampleGroup.Stratum.Code = ?", cuCN, code);
        }
        public IEnumerable<TreeDO> getTreeJustSTM(long sgd_CN, string STM)
        {
            return DAL.From<TreeDO>().Read("Tree", "WHERE SampleGroup_CN = ? AND STM = ?", sgd_CN, STM);
            //IEnumerable<TreeDO> justSTM = tIEnumerable.FindAll(
            //    delegate(TreeDO td)
            //    {
            //        return sgd.CutLeave == td.SampleGroup.CutLeave && sgd.Stratum.Code == td.Stratum.Code &&
            //                    sgd.Code == td.SampleGroup.Code && sgd.PrimaryProduct == td.SampleGroup.PrimaryProduct &&
            //                    sgd.SecondaryProduct == td.SampleGroup.SecondaryProduct && sgd.UOM == td.SampleGroup.UOM &&
            //                    td.STM == "Y";
        }
        public IEnumerable<TreeDO> getTreeJustSTM(long sgd_CN, long cu_CN, string STM)
        {
            return DAL.From<TreeDO>().Read("Tree", "WHERE SampleGroup_CN = ? AND CuttingUnit_CN = ? AND STM = ?", sgd_CN, cu_CN, STM);
        }
            //  Save data
            // *******************************************************************************************
            public void SaveTreeCalculatedValues(IEnumerable<TreeCalculatedValuesDO> tcvList)
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

        public IEnumerable<VolumeEquationDO> GetAllEquationsToCalc(string currSP, string currPP)
        {
            //  This returns a IEnumerable of equations for the current species and product to calculate volumes
            //IEnumerable<VolumeEquationDO> returnIEnumerable = volIEnumerable.FindAll(
            //    delegate(VolumeEquationDO ved)
            //    {
            //        return ved.Species == currSP && ved.PrimaryProduct == currPP;
            //    });
            //return returnIEnumerable;
            return DAL.From<VolumeEquationDO>().Read("VolumeEquation", "Where Species = ? AND PrimaryProduct = ?", currSP, currPP);

        }   //  end GetAllEquationsToCalc
        public IEnumerable<TreeDO> getTreeMatch()
        {
            return DAL.From<TreeDO>().Read("Tree", "JOIN SampleGroup ON Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN GROUP BY Tree.Species,SampleGroup.PrimaryProduct", null);

        }

        public TreeCalculatedValuesDO getTreeCalcRow(long currTreeCN)
        {
            return DAL.From<TreeCalculatedValuesDO>().Read("TreeCalculatedValues","Where Tree_CN = ?",currTreeCN).FirstOrDefault();
        }

        public void SaveTrees(IEnumerable<TreeDO> tIEnumerable)
        {
            foreach (TreeDO tdo in tIEnumerable)
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



        public void SaveLogMatrix(IEnumerable<LogMatrixDO> lmList, string currReport)
        {
                IEnumerable<LogMatrixDO> currLogMatrix = getLogMatrix(currReport);
            //  same problem as with volume equations below.
            // need to delete current report form log matrix before saving
            if (currReport != "")
            {
                if (currLogMatrix.Count() > 0)
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


        public void SaveVolumeEquations(IEnumerable<VolumeEquationDO> volumeEquationList)
        {

            //  need to delete equations in order to update the database
            //  not sure why this has to happen but the way the DAL save works
            //  is if the user deleted an equation, the Save does not consider that
            //  when the equation IEnumerable is updated ???????
            foreach (VolumeEquationDO vdo in getVolumeEquations())
            {
                VolumeEquationDO vdof = volumeEquationList.FirstOrDefault(ve => ve.VolumeEquationNumber == vdo.VolumeEquationNumber && ve.Species == vdo.Species &&
                                ve.PrimaryProduct == vdo.PrimaryProduct);
                if(vdof != null)
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


        public void SaveValueEquations(IEnumerable<ValueEquationDO> values)
        {
            //  need to delete equations in order to update the database
            //  not sure why this has to happen but the way the DAL save works
            //  is if the user deleted an equation, the Save does not consider that
            //  when the equation IEnumerable is updated ???????
            //  it also doesn't recognize deleting a row and then adding it back in
            //  says it's a constraint violation.  need an undo method for deletes
            //  December 2013
            IEnumerable<ValueEquationDO> vIEnumerable = getValueEquations();
            foreach (ValueEquationDO v in vIEnumerable)
            {
                ValueEquationDO vdo = values.FirstOrDefault(ved => ved.ValueEquationNumber == v.ValueEquationNumber && ved.Species == v.Species);
                if (vdo != null)
                    v.Delete();
            }   //  end foreach loop
            foreach (ValueEquationDO veq in values)
            {
                if (veq.DAL == null)
                {
                    veq.DAL = DAL;
                }
                veq.Save();
            }   //  end foreach loop

            return;
        }   //  end SaveValueEquations


        public void SaveQualityAdjEquations(IEnumerable<QualityAdjEquationDO> qaEquationList)
        {
            //  need to delete equations in order to update the database
            //  not sure why this has to happen but the way the DAL save works
            //  is if the user deleted an equation, the Save does not consider that
            //  when the equation IEnumerable is updated ???????
            foreach (QualityAdjEquationDO qdo in getQualAdjEquations())
            {
                QualityAdjEquationDO q = qaEquationList.FirstOrDefault(qed => qed.QualityAdjEq == qdo.QualityAdjEq && qed.Species == qdo.Species);
                if (q != null)
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


        public void SaveBiomassEquations(IEnumerable<BiomassEquationDO> bioList)
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


        public void SaveSampleGroups(IEnumerable<SampleGroupDO> sgList)
        {
            foreach (SampleGroupDO sgd in sgList)
            {
                if (sgd.DAL == null)
                    sgd.DAL = DAL;
                sgd.Save();
            }   //  end foreach loop
            return;
        }   //  end SaveSampleGroups


        public void SaveErrorMessages(IEnumerable<ErrorLogDO> errList)
        {
            DAL = new CruiseDAL.DAL(fileName);
            //  February 2015 -- due to constraining levels in the error log table
            //  duplicates are not allowed.  for example, if a Region 8 volume equation
            //  has two errors on the same tree (BDFT and CUFT), only one error is allowed
            //  to be logged in the table
            //  so need to check for that warning/error alFromy existing in the table and skip if it is
            IEnumerable<ErrorLogDO> currentIEnumerable = DAL.From<ErrorLogDO>().Read("ErrorLog", null, null);
            foreach (ErrorLogDO eld in errList)
            {
                ErrorLogDO erl = currentIEnumerable.FirstOrDefault(el => el.TableName == eld.TableName && el.CN_Number == eld.CN_Number &&
                            el.ColumnName == eld.ColumnName && el.Level == eld.Level);
                if (erl != null)
                {
                    if (eld.DAL == null)
                        eld.DAL = DAL;
                    eld.Save();
                }   //  endif
            }   //  end foreach loop

            return;
        }   //  end SaveErrorMessages


        public void SaveLCD(IEnumerable<LCDDO> LCDList)
        {
            foreach (LCDDO DOlcd in LCDList)
            {
                if (DOlcd.DAL == null)
                {
                    DOlcd.DAL = DAL;
                }
                DOlcd.Save(Backpack.SqlBuilder.OnConflictOption.Ignore);
            }   //  end foreach loop
        }   //  end SaveLCD


        public void SavePOP(IEnumerable<POPDO> POPList)
        {
            foreach (POPDO DOpop in POPList)
            {
                if (DOpop.DAL == null)
                {
                    DOpop.DAL = DAL;
                }
                DOpop.Save(Backpack.SqlBuilder.OnConflictOption.Ignore);
            }   //  end foreach loop
            return;
        }   //  end SavePOP


        public void SavePRO(IEnumerable<PRODO> PROList)
        {
            foreach (PRODO DOpro in PROList)
            {
                if (DOpro.DAL == null)
                {
                    DOpro.DAL = DAL;
                }
                DOpro.Save(Backpack.SqlBuilder.OnConflictOption.Ignore);
            }   //  end foreach loop
        }   //  end SavePRO


//  Get select data
// *******************************************************************************************
        public ArrayList GetJustSpecies(string whichTable)
        {
            ArrayList justSpecies = new ArrayList();
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
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
                    }   //  end while From
                }   //  endif
                sqlconn.Close();
            }   //  end using
            return justSpecies;
        }   //  end GetJustSpecies


        public ArrayList GetJustPrimaryProduct()
        {
            ArrayList justProduct = new ArrayList();
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
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
                    }   //  end while From
                }   //  endif
                sqlconn.Close();
            }   //  end using
            return justProduct; ;
        }   //  end GetJustPrimaryProduct


        public ArrayList GetJustSampleGroups()
        {
            ArrayList justSampleGroups = new ArrayList();
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("SELECT Code FROM SampleGroup GROUP BY Code");
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
                        justSampleGroups.Add(temp);
                    }   //  end while From
                }   //  endif
                sqlconn.Close();
            }   //  end using
            return justSampleGroups;
        }   //  end GetJustSampleGroups

// *  July 2017 -- changes to Region 8 volume equations -- this code is no longer used
// * July 2017 --  decision made to hold off on releasing the changes to R8 volume equations
// * so this code is replaced.
        public ArrayList GetProduct08Species()
        {
            ArrayList justSpecies = new ArrayList();

            foreach (TreeDefaultValueDO tdv in DAL.From<TreeDefaultValueDO>().Read("TreeDefaultValue", "WHERE PrimaryProduct = ? GROUP BY Species", "08"))
                justSpecies.Add(tdv.Species);

            return justSpecies;
        }   //  end GetProduct08Species

/*  December 2013 -- need to change where species/product comes from.  Template files will have all volume equations
 *  but it was found if only one or more species are used, folks don't want to see all those equations
 *  so changed this to pull those combinations from Tree instead of the default
        public string[,] GetUniqueSpeciesProduct()
        {
            DAL = new CruiseDAL.DAL(fileName);

            IEnumerable<TreeDefaultValueDO> tdvIEnumerable = DAL.From<TreeDefaultValueDO>().Read("TreeDefaultValue", "GROUP BY Species, PrimaryProduct", null);
            int numSpecies = tdvIEnumerable.Count();
            string[,] speciesProduct = new string[numSpecies, 2];
            int nthRow = 0;
            foreach (TreeDefaultValueDO tdv in tdvIEnumerable)
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
            IEnumerable<TreeDO> tIEnumerable = DAL.From<TreeDO>().Read("Tree", "GROUP BY Species,SampleGroup_CN", null);
            int numSpecies = tIEnumerable.Count();
            string[,] speciesProduct = new string[numSpecies, 2];
            int nthRow = -1;
            foreach (TreeDO t in tIEnumerable)
            {
                //  is the combination alFromy in the IEnumerable?
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


        public IEnumerable<TreeDO> GetUniqueSpecies()
        {
            return DAL.From<TreeDO>().Read("Tree", "GROUP BY Species", null);
        }   //  end GetUniqueSpecies


        public string[,] GetStrataUnits(long currST_CN)
        {
            //  Get all units for the current stratum
            int nthRow = 0;
            string[,] unitAcres = new string[25, 2];

            foreach (StratumDO sd in DAL.From<StratumDO>().Read("Stratum", "WHERE Stratum_CN = ?", currST_CN))
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

            IEnumerable<CuttingUnitDO> cIEnumerable = DAL.From<CuttingUnitDO>().Read("CuttingUnit", "WHERE Code = ?", currUnit);
            foreach (CuttingUnitDO cud in cIEnumerable)
            {
                cud.Strata.Populate();
                foreach (StratumDO sd in cud.Strata)
                    unitStratum.Add(sd.Code);
            }   //  end for each unit
            return unitStratum;
        }   //  end GetUnitStrata


        public IEnumerable<JustDIBs> GetJustDIBs()
        {
            //  retrieve species, product and both DIBs from volume equation table

            foreach (VolumeEquationDO veq in DAL.From<VolumeEquationDO>().Read("VolumeEquation", "GROUP BY Species,PrimaryProduct", null))
            {
                JustDIBs js = new JustDIBs();
                js.speciesDIB = veq.Species;
                js.productDIB = veq.PrimaryProduct;
                js.primaryDIB = veq.TopDIBPrimary;
                js.secondaryDIB = veq.TopDIBSecondary;
                yield return js;
            }   //  end foreach loop
            
        }   //  end GetJustDIBs


        public IEnumerable<TreeDefaultValueDO> GetUniqueSpeciesProductLiveDead()
        {
            //  make sure filename is complete
            fileName = checkFileName(fileName);
            //  build query string
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("SELECT DISTINCT Species,LiveDead,PrimaryProduct FROM TreeDefaultValue");

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
                        yield return td;
                    }   //  end while From
                }   //  endif
                sqlconn.Close();
            }   //  end using
        }   //  end GetUniqueSpeciesProductLiveDead


        //never returns anything?
        public IEnumerable<RegressGroups> GetUniqueSpeciesGroups()
        {
            //  used for Local Volume table

            List<RegressGroups> rgList = new List<RegressGroups>();
            foreach (TreeDO t in DAL.From<TreeDO>().Read("Tree", "WHERE CountOrMeasure = ? GROUP BY Species,SampleGroup_CN,LiveDead", "M"))
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


        public IEnumerable<TreeDO> GetDistinctSpecies(long SG_CN)
        {
            return DAL.From<TreeDO>().Read("Tree", "WHERE SampleGroup_CN = ? GROUP BY Species,LiveDead,Grade,STM", SG_CN);
        }   //  end GetDistinctSpecies



        public IEnumerable<TreeDO> getLCDtrees(LCDDO currLCD, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Stratum JOIN SampleGroup WHERE Tree.LiveDead = ? AND Tree.Species = ? AND Tree.Grade = ? AND ");
            sb.Append("Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND SampleGroup.Code = ? AND ");
            sb.Append("Stratum.Code = ? AND Tree.CountOrMeasure = ? AND Tree.STM = ?");
            return DAL.From<TreeDO>().Read("Tree", sb.ToString(), currLCD.LiveDead, currLCD.Species, currLCD.TreeGrade, 
                                            currLCD.SampleGroup, currLCD.Stratum, cntMeas, currLCD.STM);
        }   //  end getLCDtrees


        public IEnumerable<TreeDO> getPOPtrees(POPDO currPOP, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Stratum JOIN SampleGroup WHERE Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("SampleGroup.Code = ? AND Stratum.Code = ? AND Tree.CountOrMeasure = ? AND Tree.STM = ?");
            return DAL.From<TreeDO>().Read("Tree", sb.ToString(), currPOP.SampleGroup, currPOP.Stratum, cntMeas, currPOP.STM);
        }   //  end getPOPtrees


        public IEnumerable<TreeDO> getPROtrees(PRODO currPRO, string cntMeas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Stratum JOIN SampleGroup JOIN CuttingUnit WHERE Stratum.Stratum_CN = Tree.Stratum_CN AND ");
            sb.Append("Tree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("Tree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN AND ");
            sb.Append("SampleGroup.Code = ? AND Stratum.Code = ? AND CuttingUnit.Code = ? AND Tree.CountOrMeasure = ? ");
            sb.Append("AND Tree.STM = ?");
            return DAL.From<TreeDO>().Read("Tree", sb.ToString(), currPRO.SampleGroup, currPRO.Stratum, currPRO.CuttingUnit, cntMeas, currPRO.STM);
        }   //  end getPROtrees

        public IEnumerable<CountTreeDO> getPROCounts(PRODO currPRO)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Stratum JOIN SampleGroup JOIN CuttingUnit WHERE Stratum.Stratum_CN = SampleGroup.Stratum_CN AND ");
            sb.Append("CountTree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("CountTree.CuttingUnit_CN = CuttingUnit.CuttingUnit_CN AND ");
            sb.Append("SampleGroup.Code = ? AND Stratum.Code = ? AND CuttingUnit.Code = ? AND SampleGroup.CutLeave = ? AND ");
            sb.Append("SampleGroup.PrimaryProduct = ? AND SampleGroup.SecondaryProduct = ? AND SampleGroup.UOM = ?");
            return DAL.From<CountTreeDO>().Read("CountTree", sb.ToString(), currPRO.SampleGroup, currPRO.Stratum, currPRO.CuttingUnit, currPRO.CutLeave, currPRO.PrimaryProduct, currPRO.SecondaryProduct, currPRO.UOM);
        }   //  end getPROtrees

        public IEnumerable<CountTreeDO> getPOPCounts(POPDO currPOP)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Stratum, SampleGroup WHERE Stratum.Stratum_CN = SampleGroup.Stratum_CN AND ");
            sb.Append("CountTree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("SampleGroup.Code = ? AND Stratum.Code = ? AND SampleGroup.CutLeave = ? AND ");
            sb.Append("SampleGroup.PrimaryProduct = ? AND SampleGroup.SecondaryProduct = ? AND SampleGroup.UOM = ?");
            return DAL.From<CountTreeDO>().Read("CountTree", sb.ToString(), currPOP.SampleGroup, currPOP.Stratum, currPOP.CutLeave, currPOP.PrimaryProduct, currPOP.SecondaryProduct, currPOP.UOM);
        }   //  end getPOPCounts

        public IEnumerable<CountTreeDO> getLCDCounts(LCDDO currLCD)
        {
            StringBuilder sb = new StringBuilder();
            sb.Remove(0, sb.Length);
            sb.Append("JOIN Stratum JOIN SampleGroup JOIN TreeDefaultValue WHERE Stratum.Stratum_CN = SampleGroup.Stratum_CN AND ");
            sb.Append("CountTree.SampleGroup_CN = SampleGroup.SampleGroup_CN AND ");
            sb.Append("CountTree.TreeDefaultValue_CN = TreeDefaultValue.TreeDefaultValue_CN AND ");
            sb.Append("SampleGroup.Code = ? AND Stratum.Code = ? AND SampleGroup.CutLeave = ? AND SampleGroup.UOM = ? AND ");
            sb.Append("SampleGroup.PrimaryProduct = ? AND SampleGroup.SecondaryProduct = ? AND TreeDefaultValue.Species = ? AND ");
            sb.Append("TreeDefaultValue.LiveDead = ? AND Stratum.YieldComponent = ? AND TreeDefaultValue.TreeGrade = ?");
            return DAL.From<CountTreeDO>().Read("CountTree", sb.ToString(), currLCD.SampleGroup, currLCD.Stratum, currLCD.CutLeave, currLCD.UOM, currLCD.PrimaryProduct, currLCD.SecondaryProduct, currLCD.Species, currLCD.LiveDead, currLCD.Yield, currLCD.TreeGrade);

        }   //  end getLCDCounts

        //  Reports table
        // *******************************************************************************************
        public void SaveReports(IEnumerable<ReportsDO> rIEnumerable)
        {
            //  This saves the updated reports IEnumerable
            foreach (ReportsDO rd in rIEnumerable)
            {
                if (rd.DAL == null)
                {
                    rd.DAL = DAL;
                }
                rd.Save();
            }   //  end foreach loop
            return;
        }   //  end InsertReports


        public IEnumerable<ReportsDO> GetReports()
        {
            //  Retrieve reports
            return DAL.From<ReportsDO>().Read("Reports", "ORDER BY ReportID", null);
        }   //  end GetReports


        public IEnumerable<ReportsDO> GetSelectedReports()
        {
            return DAL.From<ReportsDO>().Read("Reports", "WHERE Selected = ? OR Selected = ? ORDER BY ReportID", "True", "1");
        }   //  end GetSelectedReports



        public void updateReports(IEnumerable<ReportsDO> reportList)
        {
            //  this updates the reports IEnumerable after user has selected reports
            //  make sure filename is complete
           // fileName = checkFileName(fileName);

            //  open connection and update records
          //  using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
          //  {
          //      sqlconn.Open();
          //      SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                StringBuilder sb = new StringBuilder();
                foreach (ReportsDO rdo in reportList)
                {
                    sb.Remove(0, sb.Length);
                    sb.Append("UPDATE Reports SET Selected='");
                    sb.Append(rdo.Selected);
                    sb.Append("' WHERE ReportID='");
                    sb.Append(rdo.ReportID);
                    sb.Append("';");
                    DAL.Execute(rdo.ToString());
//                sqlcmd.CommandText = sb.ToString();
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

            ////  open connection and delete record
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                StringBuilder sb = new StringBuilder();
                sb.Remove(0, sb.Length);
                sb.Append("DELETE FROM Reports WHERE ReportID='");
                sb.Append(reptToDelete);
                sb.Append("';");
                DAL.Execute(sb.ToString());
            //    sqlcmd.CommandText = sb.ToString();
            //    sqlcmd.ExecuteNonQuery();

            //    sqlconn.Close();
            //}   //  end using
            return;
        }   //  end deleteReport


//  Specific to EX reports
// *******************************************************************************************
        public IEnumerable<exportGrades> GetExportGrade()
        {
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

                //  Open Reader and get data
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

                        yield return eg;
                    }   //  end while
                }   //  endif Reader has rows
                sqlconn.Close();
            }   //  end using
            
        }   //  end GetExportGrade


        public void SaveExportGrade(IEnumerable<exportGrades> sortList, 
                                    IEnumerable<exportGrades> gradeList, bool tableExists)
        {
            //  make sure filename is complete
//            fileName = checkFileName(fileName);

            StringBuilder sb = new StringBuilder();
            //  open connection and save data
//            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
//            {
                //  open connection
 //               sqlconn.Open();
 //               SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                //  if table exists, delete all rows
                if (tableExists == true)
                {
                    DAL.Execute("DELETE FROM ExportValues");
//                    sqlcmd.CommandText = "DELETE FROM ExportValues";
//                    sqlcmd.ExecuteNonQuery();
                }   //  endif

                //  create query for each IEnumerable
                foreach (exportGrades eg in sortList)
                {
                    sb.Remove(0, sb.Length);
                    sb = eg.createInsertQuery(eg, "sort");
                    DAL.Execute(sb.ToString());

                //    sqlcmd.CommandText = sb.ToString();
                //    sqlcmd.ExecuteNonQuery();
            }   //  end foreach on sortIEnumerable

            foreach (exportGrades eg in gradeList)
                {
                    sb.Remove(0, sb.Length);
                    sb = eg.createInsertQuery(eg, "grade");
                    DAL.Execute(sb.ToString());

 //               sqlcmd.CommandText = sb.ToString();
 //                   sqlcmd.ExecuteNonQuery();
                }   //  end foreach on gradeIEnumerable
  //              sqlconn.Close();
   //         }   //  end using

            return;
        }   //  end SaveExportGrade


//  specific to stewardship reports (R208)
// *******************************************************************************************
        public IEnumerable<StewProductCosts> getStewCosts()
        {
            //  make sure filename is complete
            fileName = checkFileName(fileName);

            //  open connection and get data
            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            {
                sqlconn.Open();
                SQLiteCommand sqlcmd = sqlconn.CreateCommand();
                sqlcmd.CommandText = "SELECT * FROM StewProductCosts";
                sqlcmd.ExecuteNonQuery();

                //  open Reader
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
                        yield return st;
                    }   //  end while From
                }   //  endif has rows
                sqlconn.Close();
            }   //  end using
        }   //  end getStewCosts


        public void SaveStewCosts(IEnumerable<StewProductCosts> stList)
        {
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            StringBuilder sb = new StringBuilder();
            ////  open connection and save data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();
                foreach (StewProductCosts st in stList)
                {
                    sb.Remove(0, sb.Length);
                    sb = st.createInsertQuery(st);
                    DAL.Execute(sb.ToString());
            //        sqlcmd.CommandText = sb.ToString();
            //        sqlcmd.ExecuteNonQuery();
                }   //  end foreach loop                
            //    sqlconn.Close();
            //}   //  end using
            return;
        }   //  end SaveStewCosts



//  Delete all records from tables containing calculated values
// *******************************************************************************************
        public void deleteTreeCalculatedValues()
        {
            //  the following code is what Ben Campbell suggested but it runs REALLY SLOW
            //  Switched back to my method which runs faster
            //IEnumerable<TreeCalculatedValuesDO> tcvIEnumerable = getTreeCalculatedValues();
            //foreach (TreeCalculatedValuesDO tcvdo in tcvIEnumerable)
            //    tcvdo.Delete();

            //  make sure filename is complete
 //           fileName = checkFileName(fileName);

            //   open connection and delete data
 //           using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
 //           {
                //  open connection
//                sqlconn.Open();
//                SQLiteCommand sqlcmd = sqlconn.CreateCommand();

                //  delete all rows
 //               sqlcmd.CommandText = "DELETE FROM TreeCalculatedValues WHERE TreeCalcValues_CN>0";
                DAL.Execute("DELETE FROM TreeCalculatedValues WHERE TreeCalcValues_CN>0");
//                sqlcmd.ExecuteNonQuery();
//                sqlconn.Close();
//            }   //  end using
            return;
        }   //  end deleteTreeCalculatedValues


        public void DeleteLogStock()
        {
            //  see note above concerning this code

            //  make sure filename is complete
            //            fileName = checkFileName(fileName);

            //   open connection and delete data
            //            using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //            {
            //  open connection
            //                sqlconn.Open();
            //                SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //  delete all rows
            //               sqlcmd.CommandText = "DELETE FROM LogStock WHERE LogStock_CN>0";
            //               sqlcmd.ExecuteNonQuery();
            //               sqlconn.Close();
            //           }   //  end using
            DAL.Execute("DELETE FROM LogStock WHERE LogStock_CN>0");
            return;
        }   //  end deleteLogStock


        public void DeleteLCD()
        {
            //  see note above concerning this code
            //IEnumerable<LCDDO> lcdIEnumerable = getLCD();
            //foreach (LCDDO lcdo in lcdIEnumerable)
            //    lcdo.Delete();
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            ////   open connection and delete data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //    //  open connection
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //    //  delete all rows
            //    sqlcmd.CommandText = "DELETE FROM LCD WHERE LCD_CN>0";
            //    sqlcmd.ExecuteNonQuery();
            //    sqlconn.Close();
            //}   //  end using
            DAL.Execute("DELETE FROM LCD WHERE LCD_CN>0");
            return;
        }   //  end deleteLCD


        public void DeletePOP()
        {
            //  see note above concerning this code
            //IEnumerable<POPDO> popIEnumerable = getPOP();
            //foreach (POPDO ppdo in popIEnumerable)
            //    ppdo.Delete();

            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            ////   open connection and delete data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //    //  open connection
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //    //  delete all rows
            //    sqlcmd.CommandText = "DELETE FROM POP WHERE POP_CN>0";
            //    sqlcmd.ExecuteNonQuery();
            //    sqlconn.Close();
            //}   //  end using
            DAL.Execute("DELETE FROM POP WHERE POP_CN>0");
            return;
        }   //  end deletePOP


        public void DeletePRO()
        {
            //  see note above concerning this code
            //IEnumerable<PRODO> proIEnumerable = getPRO();
            //foreach (PRODO prdo in proIEnumerable)
            //    prdo.Delete();
            //  make sure filename is complete
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
            DAL.Execute("DELETE FROM PRO WHERE PRO_CN>0");
            return;
        }   //  end deletePRO


        public void DeleteErrorMessages()
        {
            //  deletes warnings and errors messages just for CruiseProcessing
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            ////   open connection and delete data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //    //  open connection
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //    //  delete all rows
            //    sqlcmd.CommandText = "DELETE FROM ErrorLog WHERE Program = 'CruiseProcessing'";
            //    sqlcmd.ExecuteNonQuery();
            //    sqlconn.Close();
            //}   //  end using
            DAL.Execute("DELETE FROM ErrorLog WHERE Program = 'CruiseProcessing'");
            return;
        }   //  end deleteErrorMessages


        public void deleteVolumeEquations()
        {
            //  used mostly in region 8 volume equations
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            ////  open connection and delete data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();
            //    //  delete all rows
            //    sqlcmd.CommandText = "DELETE FROM VolumeEquation";
            //    sqlcmd.ExecuteNonQuery();
            //    sqlconn.Close();
            //}   //  end using
            DAL.Execute("DELETE FROM VolumeEquation");
            return;
        }   //  end deleteVolumeEquations


        //  Regression table
        public void DeleteRegressions()
        {
            //  fix filename
            //string tempFileName = checkFileName(fileName);

            ////  open connection and delete data
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


        public void SaveRegress(IEnumerable<RegressionDO> resultsList)
        {
            //  Then saves the updated regression results IEnumerable
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

            ////  open connection and delete data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //    sqlconn.Open();
            //    SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //    //  delete all rows
            //    sqlcmd.CommandText = "DELETE FROM BiomassEquation WHERE rowid>=0";
            //    sqlcmd.ExecuteNonQuery();
            //    sqlconn.Close();
            //}   //  end using
            DAL.Execute("DELETE FROM BiomassEquation WHERE rowid>=0");
            return;
        }   //  end clearBiomassEquations

        // To make filename complete
        private string checkFileName(string fileName)
        {
            if (!fileName.StartsWith("Data Source"))
                return fileName.Insert(0, "Data Source = ");

            return fileName;

        }   //  end checkFileName


        public void SaveTreeDefaults(IEnumerable<TreeDefaultValueDO> treeDefaults)
        {
            //  Then saves the updated regression results IEnumerable
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