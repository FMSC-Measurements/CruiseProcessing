using CruiseProcessing.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing.Output
{
    public abstract class ReportGeneratorBase
    {
        protected int numOlines { get; set; } = 0;

        protected string currentReport { get; private set; }

        protected ReportGeneratorBase(CpDataLayer dataLayer, string reportID = "")
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));

            currentReport = reportID;
        }

        protected CpDataLayer DataLayer { get; }

        protected string FilePath => DataLayer.FilePath;

        // public because of TreeListMethods
        public enum HeightFieldType
        { Unknown = 0, Total = 1, MerchPrimary = 2, MerchSecondary = 3, UpperStem = 4 }

        // needed in common base class because used by OutputCSV and OutputBLM
        protected class CSVlist
        {
            public string field1 { get; set; }
            public string field2 { get; set; }
            public string field3 { get; set; }
            public string field4 { get; set; }
            public string field5 { get; set; }
            public string field6 { get; set; }
            public string field7 { get; set; }
            public string field8 { get; set; }
            public string field9 { get; set; }
            public string field10 { get; set; }
            public string field11 { get; set; }
            public string field12 { get; set; }
            public string field13 { get; set; }
            public string field14 { get; set; }
            public string field15 { get; set; }
            public string field16 { get; set; }
            public string field17 { get; set; }
            public string field18 { get; set; }
            public string field19 { get; set; }
            public string field20 { get; set; }
            public string field21 { get; set; }
            public string field22 { get; set; }
            public string field23 { get; set; }
            public string field24 { get; set; }
            public string field25 { get; set; }
            public string field26 { get; set; }
            public string field27 { get; set; }
            public string field28 { get; set; }
            public string field29 { get; set; }
            public string field30 { get; set; }
            public string field31 { get; set; }
            public string field32 { get; set; }
            public string field33 { get; set; }
            public string field34 { get; set; }
            public string field35 { get; set; }
            public string field36 { get; set; }
            public string field37 { get; set; }
            public string field38 { get; set; }
            public string field39 { get; set; }
            public string field40 { get; set; }
            public string field41 { get; set; }
            public string field42 { get; set; }
            public string field43 { get; set; }
            public string field44 { get; set; }
            public string field45 { get; set; }
            public string field46 { get; set; }
        }
    }


}