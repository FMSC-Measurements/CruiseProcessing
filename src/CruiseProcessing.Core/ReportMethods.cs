using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public static class ReportMethods
    {
        public static List<ReportsDO> fillReportsList()
        {
            //  this would be used to fill the list with reports when there are none in the database
            List<ReportsDO> rList = new List<ReportsDO>();

            //  need the reports array to loop through
            allReportsArray ara = new allReportsArray();
            for (int k = 0; k < ara.reportsArray.GetLength(0); k++)
            {
                ReportsDO rl = new ReportsDO();
                rl.ReportID = ara.reportsArray[k, 0];
                //  since this is an initial list where none exists, selected will always be zero or false
                rl.Selected = false;
                rl.Title = ara.reportsArray[k, 1];
                rList.Add(rl);
            }   //  end for k loop

            return rList;
        }   //  end fillReportsList


        public static List<ReportsDO> updateReportsList(List<ReportsDO> rList,allReportsArray ara)
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
                        for (int j = 0; j < ara.reportsArray.GetLength(0); j++)
                        {
                            if (r.ReportID == ara.reportsArray[j, 0])
                                r.Title = ara.reportsArray[j, 1];
                        }   //  end for j loop
                        break;
                }   //  end switch on report number
            }   //  end foreach loop
            //  add any reports not selected
            if (rList.Count < ara.reportsArray.GetLength(0))
                rList = addReports(rList, ara);

            return rList;
        }   //  end updateReportsList


        public static List<ReportsDO> addReports(List<ReportsDO> rList, allReportsArray ara)
        {
            //  check current reports list for each report and add if not there
            //  hoping this catches new reports as they are added to the reports 
            //  March 2014
            for (int k = 0; k < ara.reportsArray.GetLength(0); k++)
            {
                int nthRow = rList.FindIndex(
                    delegate(ReportsDO r)
                    {
                        return r.ReportID == ara.reportsArray[k, 0];
                    });
                if (nthRow < 0)
                {
                    //  add report to list
                    ReportsDO rr = new ReportsDO();
                    rr.ReportID = ara.reportsArray[k, 0];
                    rr.Title = ara.reportsArray[k, 1];
                    rr.Selected = false;
                    rList.Add(rr);
                }   //  endif nthRow
            }   //  end for k loop
            return rList;
        }   //  end addReports


        public static List<ReportsDO> deleteReports(List<ReportsDO> rList, CPbusinessLayer bslyr)
        {
            //  probably used infrequently
            //  currently deletes CSV reports from the reports list
            //  however it does not delete in the database
            //  so do that first
            string[] reportsToDelete = new string[6] { "CSV1", "CSV2", "CSV3", "CSV4", "CSV5", "CSV6" };
            for (int j = 0; j < 6; j++)
            {
                bslyr.deleteReport(reportsToDelete[j]);

                foreach (ReportsDO rr in rList)
                {
                    if (rr.ReportID == reportsToDelete[j])
                        rr.Delete();
                }   //  end foreach loop
            }   //  end for j loop
            return rList;
        }   //  end deleteReports

    }
}
