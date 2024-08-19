using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public static class ReportsDataservice
    {
        public static List<ReportsDO> GetDefaultReports()
        {
            //  this would be used to fill the list with reports when there are none in the database
            List<ReportsDO> rList = new List<ReportsDO>();

            //  need the reports array to loop through
            for (int k = 0; k < reportsArray.GetLength(0); k++)
            {
                ReportsDO rl = new ReportsDO();
                rl.ReportID = reportsArray[k, 0];
                //  since this is an initial list where none exists, selected will always be zero or false
                rl.Selected = false;
                rl.Title = reportsArray[k, 1];
                rList.Add(rl);
            }   //  end for k loop

            return rList;
        }   //  end fillReportsList

        public static List<ReportsDO> AddMissingReports(List<ReportsDO> rList)
        {
            //  check current reports list for each report and add if not there
            //  hoping this catches new reports as they are added to the reports
            //  March 2014
            for (int k = 0; k < reportsArray.GetLength(0); k++)
            {
                var reportID = reportsArray[k, 0];
                var existingReport = rList.FirstOrDefault(x => x.ReportID == reportID);
                if (existingReport == null)
                {
                    var newReport = new ReportsDO
                    {
                        ReportID = reportID,
                        Title = reportsArray[k, 1],
                        Selected = false,
                    };
                    rList.Add(newReport);
                }
            }
            return rList;
        }

        public static List<ReportsDO> UpdateReportIDAndTitles(List<ReportsDO> rList)
        {
            //  convert old reports ID to new reportsID
            foreach (ReportsDO r in rList)
            {
                switch (r.ReportID)
                {
                    case "A11":
                        r.ReportID = "TIM";
                        r.Title = "Input file for Timber Information Manager (A11)";
                        break;

                    case "A1":
                        r.ReportID = "A01";
                        r.Title = "Strata, Unit, Payment Unit and Sample Group Report (A1)";
                        break;

                    case "A2":
                        r.ReportID = "A03";
                        r.Title = "Listing of Tree Measurements and Characteristics (A2)";
                        break;

                    case "A3":
                        r.ReportID = "A08";
                        r.Title = "Log Grade (A3)";
                        break;

                    case "A4":
                        r.ReportID = "A09";
                        r.Title = "Fall, Buck and Scale (A4)";
                        break;

                    case "A5":
                        r.ReportID = "A05";
                        r.Title = "Cubic Foot Primary and Secondary Product Volume Information (A5)";
                        break;

                    case "A6":
                        r.ReportID = "A06";
                        r.Title = "$ Value Information Primary and Secondary Product (A6)";
                        break;

                    case "A7":
                        r.ReportID = "A07";
                        r.Title = "Board Foot Primary and Secondary Product Volume Information (A7)";
                        break;

                    case "A9":
                        r.ReportID = "A10";
                        r.Title = "Biomass Weight Information (A9)";
                        break;

                    case "A12":
                        r.ReportID = "A11";
                        r.Title = "Tree Grade Report By Sale - BDFT Net Volume (A12)";
                        break;

                    case "A13":
                        r.ReportID = "A12";
                        r.Title = "Tree Grade Report By Sale - CUFT Net Volume (A13)";
                        break;

                    case "A14":
                        r.ReportID = "A13";
                        r.Title = "Listing of Geospatial Information (A14)";
                        break;

                    case "B1":
                        r.ReportID = "VSM1";
                        r.Title = "Low Level Volume Summary (B1)";
                        break;

                    case "B2":
                        r.ReportID = "VPA1";
                        r.Title = "Low Level Volume per Acre Summary (B2)";
                        break;

                    case "B3":
                        r.ReportID = "VAL1";
                        r.Title = "Low Level $ Value Summary (B3)";
                        break;

                    case "CP1":
                        r.ReportID = "VSM2";
                        r.Title = "Volume Summary for Sample Group (CP1)";
                        break;

                    case "CP2":
                        r.ReportID = "VPA2";
                        r.Title = "Volume per Acre Summary by Sample Group (CP2)";
                        break;

                    case "CP3":
                        r.ReportID = "VAL2";
                        r.Title = "Sample Group $ Value Summary (CP3)";
                        break;

                    case "CP4":
                        r.ReportID = "VSM4";
                        r.Title = "3P Tree Report (CP4)";
                        break;

                    case "CS1":
                        r.ReportID = "VSM3";
                        r.Title = "Volume Summary for Strata (CS1)";
                        break;

                    case "CS2":
                        r.ReportID = "VPA3";
                        r.Title = "Volume per Acre Summary by Strata (CS2)";
                        break;

                    case "CS3":
                        r.ReportID = "VAL3";
                        r.Title = "Strata $ Value Summary (CS3)";
                        break;

                    case "CS4":
                        r.ReportID = "LD1";
                        r.Title = "Live/Dead Volume By Product and Species - CUFT (CS4)";
                        break;

                    case "CS5":
                        r.ReportID = "LD2";
                        r.Title = "Live/Dead Volume By Product and Species - BDFT (CS5)";
                        break;

                    case "CS6":
                        r.ReportID = "LD3";
                        r.Title = "Live/Dead Volume By Cutting Unit, Product and Species - CUFT (CS6)";
                        break;

                    case "CS7":
                        r.ReportID = "LD4";
                        r.Title = "Live/Dead Volume By Cutting Unit, Product and Species - BDFT (CS7)";
                        break;

                    case "CS8":
                        r.ReportID = "LD5";
                        r.Title = "Live/Dead Volume By Payment Unit, Product and Species - CUFT (CS8)";
                        break;

                    case "CS9":
                        r.ReportID = "LD6";
                        r.Title = "Live/Dead Volume By Payment Unit, Product and Species - BDFT (CS9)";
                        break;

                    case "CS10":
                        r.ReportID = "LD7";
                        r.Title = "Live/Dead Volume By Logging Method, Product and Species - CUFT (CS10)";
                        break;

                    case "CS11":
                        r.ReportID = "LD8";
                        r.Title = "Live/Dead Volume By Logging Method, Product and Species - BDFT (CS11)";
                        break;

                    case "DP1":
                        r.ReportID = "ST1";
                        r.Title = "Net Volume Statistics for Sample Group (DP1)";
                        break;

                    case "DP2":
                        r.ReportID = "ST2";
                        r.Title = "Gross Volume Statistics for Sample Group (DP2)";
                        break;

                    case "DS1":
                        r.ReportID = "ST3";
                        r.Title = "Error Term, Volume and Confidence Interval (DS1)";
                        break;

                    case "DS2":
                        r.ReportID = "ST4";
                        r.Title = "Error Term, Value and Confidence Interval (DS2)";
                        break;

                    case "L3":
                        r.ReportID = "CSV6";
                        r.Title = "Comma-delimited text file for L2 Log Stock Table report";
                        break;

                    case "R701":
                        r.ReportID = "BLM01";
                        r.Title = "Board Foot Strata Summary (R701)";
                        break;

                    case "R702":
                        r.ReportID = "BLM02";
                        r.Title = "Cubic Foot Strata Summary (R702)";
                        break;

                    case "R703":
                        r.ReportID = "BLM03";
                        r.Title = "Board Foot Unit Summary (R703)";
                        break;

                    case "R704":
                        r.ReportID = "BLM04";
                        r.Title = "Cubic Foot Unit Summary (R704)";
                        break;

                    case "R705":
                        r.ReportID = "BLM05";
                        r.Title = "Board Foot Species Summary (R705)";
                        break;

                    case "R706":
                        r.ReportID = "BLM06";
                        r.Title = "Cubic Foot Species Summary (R706)";
                        break;

                    case "R707":
                        r.ReportID = "BLM07";
                        r.Title = "Board Foot Unit and Species Summary (R707)";
                        break;

                    case "R708":
                        r.ReportID = "BLM08";
                        r.Title = "Cubic Foot Unit and Species Summary (R708)";
                        break;

                    case "R709":
                        r.ReportID = "BLM09";
                        r.Title = "Board Foot Log Grade -- Diameter Class by Species (R709)";
                        break;

                    case "R710":
                        r.ReportID = "BLM10";
                        r.Title = "Cubic Foot Log Grade -- Diameter Class by Species (R710)";
                        break;

                    case "R105":
                        r.ReportID = "R105";
                        r.Title = "Summary by Unit, Species and Total";
                        break;

                    default:
                        //  means report doesn't need an update just a title
                        //  look it up in the reports array
                        for (int j = 0; j < reportsArray.GetLength(0); j++)
                        {
                            if (r.ReportID == reportsArray[j, 0])
                                r.Title = reportsArray[j, 1];
                        }   //  end for j loop
                        break;
                }   //  end switch on report number
            }   //  end foreach loop

            return rList;
        }

        //  an all reports array
        public static readonly string[,] reportsArray = new string[,] {
                                                 {"BLM01","Board Foot Strata Summary (R701)"},
                                                 {"BLM02","Cubic Foot Strata Summary (R702)"},
                                                 {"BLM03","Board Foot Unit Summary (R703)"},
                                                 {"BLM04","Cubic Foot Unit Summary (R704)"},
                                                 {"BLM05","Board Foot Species Summary (R705)"},
                                                 {"BLM06","Cubic Foot Species Summary (R706)"},
                                                 {"BLM07","Board Foot Unit and Species Summary (R707)"},
                                                 {"BLM08","Cubic Foot Unit and Species Summary (R708)"},
                                                 {"BLM09","Board Foot Log Grade -- Diameter Class by Species (R709)"},
                                                 {"BLM10","Cubic Foot Log Grade -- Diameter Class by Species (R710)"},
                                                 {"A01","Strata, Unit, Payment Unit and Sample Group Report (A1)"},
                                                 {"A02","Plot Information Report (A1)"},
                                                 {"A03","Listing of Tree Measurements and Characteristics (A2)"},
                                                 {"A04","Count Table Information (A2)"},
                                                 {"A05","Cubic Foot Primary and Secondary Product Volume Information (A5)"},
                                                 {"A06","$ Value Information Primary and Secondary Product (A6)"},
                                                 {"A07","Board Foot Primary and Secondary Product Volume Information (A7)"},
                                                 {"A08","Log Grade (A3)"},
                                                 {"A09","Fall, Buck and Scale (A4)"},
                                                 {"A10","Biomass Weight Information (A9)"},
                                                 {"A11","Tree Grade Report By Sale - BDFT Net Volume (A12)"},
                                                 {"A12","Tree Grade Report By Sale - CUFT Net Volume (A13)"},
                                                 {"A13","Listing of Geospatial Information (A14)"},
                                                 {"A14","Summary of Species Based on Unit Level Data"},
                                                 {"A15","Merchandizing Rules for Current Sale"},
                                                 {"EX1","Variable Log Length"},
                                                 {"EX2","Export Sort and Grade Edit Checks"},
                                                 {"EX3","Export Sort and Grade Specifications Report"},
                                                 {"EX4","Sort/Grade Log Stock Table -- Net MBF"},
                                                 {"EX5","Sort/Grade Log Stock Table -- Net CCF"},
                                                 {"EX6","Volume by Species/Sort/Grade within Unit for each Stratum"},
                                                 {"EX7","Volume by Species/Sort/Grade for each Stratum"},
                                                 {"GR01","Scatter plot of DBH and Total Height by Species"},
                                                 {"GR02","Species Distribution for the Sale (pie chart)"},
                                                 {"GR03","Volume by Species - Sawtimber Only (pie chart)"},
                                                 {"GR04","Volume by Product(pie chart)"},
                                                 {"GR05","Number of 16-foot Logs by DIB Class by Species (bar chart)"},
                                                 {"GR06","Diameter Distribtuion for the Sale (bar chart)"},
                                                 {"GR07","Diameter Distribution by Species (bar chart)"},
                                                 {"GR08","Diameter Distribution by Stratum (bar chart)"},
                                                 {"GR09","KPI Distribution by Species (bar chart)"},
                                                 {"GR10","BA/Acrre bySpecies (bar chart)"},
                                                 {"GR11","BA/Acre by 2-inch DBH class by Sample Group (bar chart)"},
                                                 {"L1","Log Grade File"},
                                                 {"L2","Log Stock Table - MBF"},
                                                 {"L8","Log Stock Table - Board and Cubic"},
                                                 {"L10","Log Counts and Volume by Length and Species"},
                                                 {"LD1","Live/Dead Volume By Product and Species - CUFT (CS4)"},
                                                 {"LD2","Live/Dead Volume By Product and Species - BDFT (CS5)"},
                                                 {"LD3","Live/Dead Volume By Cutting Unit, Product and Species - CUFT (CS6)"},
                                                 {"LD4","Live/Dead Volume By Cutting Unit, Product and Species - BDFT (CS7)"},
                                                 {"LD5","Live/Dead Volume By Payment Unit, Product and Species - CUFT (CS8)"},
                                                 {"LD6","Live/Dead Volume By Payment Unit, Product and Species - BDFT (CS9)"},
                                                 {"LD7","Live/Dead Volume By Logging Method, Product and Species - CUFT (CS10)"},
                                                 {"LD8","Live/Dead Volume By Logging Method, Product and Species - BDFT (CS11)"},
                                                 {"LV01","Volume Summary for Sample Group (VSM2)"},
                                                 {"LV02","Volume Summary for Strata (VSM3)"},
                                                 {"LV03","Net Volume Statistics for Sample Group (ST1)"},
                                                 {"LV04","Gross Volume Statistics for Sample Group (ST2)"},
                                                 {"LV05","Volume by Species within Cutting Unit Across All Stratum (UC5)"},
                                                 {"R101","Volume Summary by Primary Product and Contract Species"},
                                                 {"R102","Summary by Logging Method-BDFT"},
                                                 {"R103","Summary by Logging Method-CUFT"},
                                                 {"R104","Basal Area Per Unit"},
                                                 {"R105","Summary by Unit, Species and Total"},
                                                 {"R201","Trees per Acre & Net Cubic Volume"},
                                                 {"R202","Percent Defect by Species-BDFT Volume 1-inch classes"},
                                                 {"R203","Percent Defect by Species-CUFT Volume 1-inch classes"},
                                                 {"R204","Percent Defect by Species-BDFT Volume 2-inch classes"},
                                                 {"R205","Percent Defect by Species-CUFT Volume 2-inch classes"},
                                                 {"R206","QMD & Average DBH by Species & Contract Species"},
                                                 {"R207","Stewardship Product Summary -- Stems Per Acre"},
                                                 {"R208","Stewardship Average Product Cost"},
                                                 {"R301","Region-3 Synopsis"},
                                                 {"R302","RAW output file"},
                                                 {"R401","Sale Appraisal Summary by Species-BDFT"},
                                                 {"R402","Sale Appraisal Summary by Species-CUFT"},
                                                 {"R403","Sale Appraisal Summary by Logging Method-BDFT"},
                                                 {"R404","Sale Appraisal Summary by Logging Method-CUFT"},
                                                 {"R501","Log Stock Table by Species and Product"},
                                                 {"R602","Volume by Species & Product within Payment and Cutting Unit Across Strata"},
                                                 {"R604","Product Volume Summary - CCF"},
                                                 {"R605","Product Volume Summary - MBF"},
                                                 {"R801","Sale Summary"},
                                                 {"R802","Sale Summary -- Estimated Sale Volume and Number of Trees"},
                                                 {"R901","PUVOL Output File"},
                                                 {"R902","TIPWOOD SUMMARY BY UNIT AND SPECIES "},
                                                 {"R001","Appraisal Summary Report-BDFT Volume"},
                                                 {"R002","Appraisal Summary Report-CUFT Volume"},
                                                 {"R003","Log Grade Summary-BDFT Volume"},
                                                 {"R004","Log Grade Summary-CUFT Volume"},
                                                 {"R005","Appraisal Summary by Logging Method"},
                                                 {"R006","Net MBF by Diameter Group (file only)"},
                                                 {"R007","Net CCF by Diameter Group (file only)"},
                                                 {"R008","Log Matrix Output File"},
                                                 {"R009","Updated Log Matrix Output File"},
                                                 {"SC1","Stem Count per Acre by Unit by Species (FIXCNT Method Only)"},
                                                 {"SC2","Stem Count (Tally) by Unit by Stratum by Species (FIXCNT Method Only)"},
                                                 {"SC3","Stem Count by Stratum by Species -- Stems Per Acre (FIXCNT Method Only)"},
                                                 {"ST1","Net Volume Statistics for Sample Group (DP1)"},
                                                 {"ST2","Gross Volume Statistics for Sample Group (DP2)"},
                                                 {"ST3","Error Term, Volume and Confidence Interval (DS1)"},
                                                 {"ST4","Error Term, Value and Confidence Interval (DS2)"},
                                                 {"TC1","Stand Table for Strata X -- Gross BDFT Volume"},
                                                 {"TC2","Stand Table for Strata X -- Net BDFT Volume"},
                                                 {"TC3","Stand Table for Strata X -- Gross CUFT Volume"},
                                                 {"TC4","Stand Table for Strata X -- Net CUFT Volume"},
                                                 {"TC6","Stand Table for Strata X -- Estimated Number of Trees"},
                                                 {"TC8","Stand Table for Sale X -- Net BDFT Volume"},
                                                 {"TC10","Stand Table for Sale X -- Net CUFT Volume"},
                                                 {"TC12","Stand Table for Sale X -- Estimated Number of Trees"},
                                                 {"TC19","Stand Table for Sale X -- Gross BDFT Volume"},
                                                 {"TC20","Stand Table for Sale X -- Net BDFT Volume"},
                                                 {"TC21","Stand Table for Sale X -- Gross CUFT Volume"},
                                                 {"TC22","Stand Table for Sale X -- Net CUFT Volume"},
                                                 {"TC24","Stand Table for Sale X -- Estimated Number of Trees"},
                                                 {"TC51","Stand Table for Strata X -- Gross BDFT Volume"},
                                                 {"TC52","Stand Table for Strata X -- Net BDFT Volume"},
                                                 {"TC53","Stand Table for Strata X -- Gross CUFT Volume"},
                                                 {"TC54","Stand Table for Strata X -- Net CUFT Volume"},
                                                 {"TC56","Stand Table for Strata X -- Estimated Number of Trees"},
                                                 {"TC57","Stand Table for Sale X -- Gross BDFT Volume"},
                                                 {"TC58","Stand Table for Sale X -- Net BDFT Volume"},
                                                 {"TC59","Stand Table for Sale X -- Gross CUFT Volume"},
                                                 {"TC60","Stand Table for Sale X -- Net CUFT Volume"},
                                                 {"TC62","Stand Table for Sale X -- Estimated Number of Trees"},
                                                 {"TC65","Stand Table for Strata X - Gross CUFT Volume"},
                                                 {"TC71","Stand Table for Sale X -- Gross CUFT Volume"},
                                                 {"TC72","Stand Table for Sale X -- Net CUFT Volume"},
                                                 {"TC74","Stand Table for Sale X -- Estimated Number of Trees"},
                                                 {"TL2","Stand Table for Strata X -- Gross BDFT Volume"},
                                                 {"TL6","Stand Table for Strata X -- Estimated Number of Trees"},
                                                 {"TL7","Stand Table for Strata X -- Gross BDFT Volume"},
                                                 {"TL8","Stand Table for Strata X -- Net BDFT Volume"},
                                                 {"TL9","Stand Table for Strata X -- Gross CUFT Volume"},
                                                 {"TL10","Stand Table for Strata X -- Net CUFT Volume"},
                                                 {"TL12","Stand Table for Sale X -- Estimated Number of Trees"},
                                                 {"TL52","Stand Table for Strata X -- Gross BDFT Volume"},
                                                 {"TL54","Stand Table for Strata X -- Net CUFT Volume"},
                                                 {"TL56","Stand Table for Strata X -- Estimated Number of Trees"},
                                                 {"TL58","Stand Table for Strata X -- Net BDFT Volume"},
                                                 {"TL59","Stand Table for Strata X -- Gross CUFT Volume"},
                                                 {"TL60","Stand Table for Strata X -- Net CUFT Volume"},
                                                 {"TL62","Stand Table for Sale X -- Estimated Number of Trees"},
                                                 {"UC1","Volume by Species within Cutting Unit for Each Stratum-Sawtimber"},
                                                 {"UC2","Volume by Sample Group within Cutting Unit for Each Stratum-Sawtimber"},
                                                 {"UC3","Volume by Species within Cutting Unit for Each Stratum-Sawtimber/Pulpwood"},
                                                 {"UC4","Volume by Sample Group within Cutting Unit for Each Stratum-Sawtimber/Pulpwood"},
                                                 {"UC5","Volume by Species within Cutting Unit Across All Strata"},
                                                 {"UC6","Volume by Sample Group within Cutting Unit Across All Strata"},
                                                 {"UC7","Gross BDFT Volume by Species within Cutting Unit Across All Strata-Sawtimber"},
                                                 {"UC8","Net BDFT Volume by Species within Cutting Unit Across All Strata-Sawtimber"},
                                                 {"UC9","Gross CUFT Volume by Species within Cutting Unit Across All Strata-Sawtimber"},
                                                 {"UC10","Net CUFT Volume by Species within Cutting Unit Across All Strata-Sawtimber"},
                                                 {"UC11","Est. number of trees by Species within Cutting Unit Across All Strata-Sawtimber"},
                                                 {"UC12","Gross CUFT Volume by Species within Cutting Unit Across All Strata-Non-sawtimber"},
                                                 {"UC13","Net CUFT Volume by Species within Cutting Unit Across All Strata-Non-sawtimber"},
                                                 {"UC14","Cord Wood Volume by Species within Cutting Unit Across All Strata-Non-sawtimber"},
                                                 {"UC15","Total Estimated Number of Trees by Species within Cutting Unit Across All Strata"},
                                                 {"UC16","Gross BDFT Volume by Sample Group within Cutting Unit Across All Strata-Sawtimber"},
                                                 {"UC17","Net BDFT Volume by Sample Group within Cutting Unit Across All Strata-Sawtimber"},
                                                 {"UC18","Gross CUFT Volume by Sample Group within Cutting Unit Across All Strata-Sawtimber"},
                                                 {"UC19","Net CUFT Volume by Sample Group within Cutting Unit Across All Strata-Sawtimber"},
                                                 {"UC20","Est. Number of Trees by Sample Group within Cutting Unit Across All Strata-Sawtimber"},
                                                 {"UC21","Gross CUFT Volume by Sample Group within Cutting Unit Across All Strata-Non-sawtimber"},
                                                 {"UC22","Net CUFT Volume by Sample Group within Cutting Unit Across All Strata-Non-sawtimber"},
                                                 {"UC23","Cord Wood Volume by Sample Group within Cutting Unit Across All Strata-Non-sawtimber"},
                                                 {"UC24","Total Estimated Number of Trees by Sample Group within Cutting Unit Across All Strata-Non-sawtimber"},
                                                 {"UC25","Volume by Contract Species and Product within Payment Units Across Strata"},
                                                 {"UC26","Volume Summary by Contract Species and Product by Cutting Unit for the Sale"},
                                                 {"VSM1","Low Level Volume Summary (B1)"},
                                                 {"VSM2","Volume Summary for Sample Group (CP1)"},
                                                 {"VSM3","Volume Summary for Strata (CS1)"},
                                                 {"VSM4","3P Tree Report (CP4)"},
                                                 {"VSM5","Volume Summary by Cutting Unit"},
                                                 {"VSM6","Low Level Volume Summary -- CUFT And Tons Only"},
                                                 {"VPA1","Low Level Volume per Acre Summary (B2)"},
                                                 {"VPA2","Volume per Acre Summary by Sample Group (CP2)"},
                                                 {"VPA3","Volume per Acre Summary by Strata (CS2)"},
                                                 {"VAL1","Low Level $ Value Summary (B3)"},
                                                 {"VAL2","Sample Group $ Value Summary (CP3)"},
                                                 {"VAL3","Strata $ Value Summary (CS3)"},
                                                 {"TIM","Input file for Timber Information Manager (A11)"},
                                                 {"WT1","Weight by Species Across Strata"},
                                                 {"WT2","Slash Loading Summary by Strata"},
                                                 {"WT3","Slash Loading Summary by Unit"},
                                                 {"WT4","Weight by Species and Product by Unit"},
                                                 {"WT5","Biomass Sale Summary"},
                                                 {"IDL1","Idaho Dept of Lands Summary of Cruise Data"}};

        public static readonly string[][] GRAPH_REPORTS = new string[11][]
        {
            new []{"GR01","Scatter plot of DBH and Total Height by Species"},
            new []{"GR02","Species Distribution for the Sale (pie chart)"},
            new [] {"GR03","Volume by Species - Sawtimber Only (pie chart)"},
            new [] {"GR04","Volume by Product(pie chart)"},
            new [] {"GR05","Number of 16-foot Logs by DIB Class by Species (bar chart)"},
            new [] {"GR06","Number of Trees by DBH (bar chart)"},
            new [] {"GR07","Number of Trees by Species (bar chart)"},
            new [] {"GR08","Number of Trees by DBH by Stratum (bar chart)"},
            new [] {"GR09","Number of Trees by KPI by Species (bar chart)"},
            new [] {"GR10","BA/Acre by Species (pie chart)"},
            new [] {"GR11","BA/Acre by 2-inch DBH Class by Sample Group (bar chart)"}
        };

        public static bool IsGraphReport(string reportID)
        {
            return GRAPH_REPORTS.Any(x => reportID == x[0]);
        }

        

    //public static readonly string[] GRAPH_REPORTS = new[] { "GR01", "GR02", "GR03", "GR04", "GR05", "GR06", "GR07", "GR08", "GR09", "GR10", "GR11"  };

    //  Functions
    public static string findReportTitle(string currentReport)
        {
            var arrayLength = reportsArray.GetLength(0);

            for (int j = 0; j < arrayLength; j++)
            {
                if (currentReport == reportsArray[j, 0])
                    return reportsArray[j, 1];
            }

            return "";
        }   //  end findReportTitle
    }
}