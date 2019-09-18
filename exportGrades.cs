using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class exportGrades
    {
        public string exportSort { get; set; }
        public string exportGrade { get; set; }
        public string exportCode { get; set; }
        public string exportName { get; set; }
        public double minDiam { get; set; }
        public double minLength { get; set; }
        public double minBDFT { get; set; }
        public double maxDefect { get; set; }

        public string[,] defaultLogSort = new string[22,7] {{"0","CU","CULL     ","0","0","0","100"},
                                                            {"2","HI","H EX 16  ","16","26","0","10"},
                                                            {"3","JA","JA EX 12 ","12","26","0","10"},
                                                            {"4","JP","PEEWEE JA","8","34","0","15"},
                                                            {"5","CH","CH EX 12 ","12","26","0","20"},
                                                            {"6","KO","KO EX 8  ","8","26","0","15"},
                                                            {"7","K6","KO 6 PLUS","6","12","0","15"},
                                                            {"8","DO","DOMESTIC ","4","12","0","66"},
                                                            {"9","PU","PULP     ","4","12","0","66"},
                                                            {"A","OG","OG EX 24 ","24","17","0","66"},
                                                            {"B","JR","JA 8 RP1 ","12","26","0","10"},
                                                            {"C","CH","CHINA    ","12","26","0","20"},
                                                            {"D"," D","DOM SAW  ","4","12","0","66"},
                                                            {"E","EX","EXPORT   ","12","20","0","66"},
                                                            {"G","PI","PILING   ","8","30","0","66"},
                                                            {"H","JH","JAPAN 14 ","14","26","0","10"},
                                                            {"J","JA","JAPAN    ","12","26","0","10"},
                                                            {"K"," K","KOREA    ","8","26","0","15"},
                                                            {"M","JC","JA/CH 12 ","12","34","0","15"},
                                                            {"P","PO","POLE     ","5","25","0","66"},
                                                            {"W","PW","PEEWEE   ","8","34","0","15"},
                                                            {"X","HD","OG HD    ","24","17","0","50"}};

        public string[,] defaultLogGrade = new string[18,7]  {{"0","CU","CULL     ","0","0","0","100"},
                                                              {"1","1S","1 SAW    ","30","16","0","66"},
                                                              {"2","2S","2 SAWMILL","12","12","0","66"},
                                                              {"3","3S","3 SAWMILL","6","12","50","66"},
                                                              {"4","4S","4 SAWMILL","5","12","10","66"},
                                                              {"5","SM","SP MILL  ","16","17","0","66"},
                                                              {"6","1P","1 PEELER ","30","17","0","66"},
                                                              {"7","2P","2 PEELER ","30","17","0","66"},
                                                              {"8","3P","3 PEELER ","24","17","0","66"},
                                                              {"9","UT","UTILITY  ","4","16","0","66"},
                                                              {"A","SP","SP CULL  ","12","17","0","66"},
                                                              {"B","PC","PEEL CULL","12","17","0","66"},
                                                              {"C","1S","PP 1 SAW ","30","16","0","66"},
                                                              {"D","2S","PP 2 SAW ","24","12","0","66"},
                                                              {"E","3S","PP 3 SAW ","24","12","0","66"},
                                                              {"F","4S","PP 4 SAW ","12","12","0","66"},
                                                              {"G","5S","PP 5 SAW ","6","12","0","66"},
                                                              {"H","6S","PP 6 SAW ","5","12","0","66"}};

        public string[,] R10defaultLogSort = new string[21,7] {{"0","CU","CULL     ","0","0","0","100"},
                                                                {"1","11","HIGRADE  ","16","26","0","10"},
                                                                {"3","13","MEDIUM   ","12","26","0","10"},
                                                                {"4","33","MEDIUM   ","8","34","0","15"},
                                                                {"5","15","DOM15    ","12","26","0","20"},
                                                                {"6","16","DOM16    ","8","26","0","15"},
                                                                {"7","17","DOM17    ","6","12","0","15"},
                                                                {"8","38","KSORT    ","4","12","0","66"},
                                                                {"9","PU","PULP     ","4","12","0","66"},
                                                                {"B","34","SHOP     ","24","17","0","66"},
                                                                {"C","26","DOM      ","12","26","0","10"},
                                                                {"D","37","DOM      ","12","26","0","20"},
                                                                {"E","31","HIGRADE  ","4","12","0","66"},
                                                                {"J","32","JSORT    ","12","20","0","66"},
                                                                {"L","20","EXPORT   ","8","30","0","66"},
                                                                {"N","10","J-C HEM  ","14","26","0","10"},
                                                                {"P","23","KPLY     ","12","26","0","10"},
                                                                {"S","35","REG      ","8","26","0","15"},
                                                                {"T","14","SHOP     ","12","34","0","15"},
                                                                {"X","29","UTILITY  ","5","25","0","66"},
                                                                {"Y","70","YCDR     ","8","34","0","15"}};

        public string[,] R10defaultLogGrade = new string[9,7] {{"1","1S","1SAW     ","0","0","0","100"},
                                                                {"2","2S","2SAW     ","16","26","0","10"},
                                                                {"3","3S","3SAW     ","12","26","0","10"},
                                                                {"4","4S","4SAW     ","8","34","0","15"},
                                                                {"5","SM","SM       ","12","26","0","20"},
                                                                {"7","SELECT","SELECT   ","8","26","0","15"},
                                                                {"P","PEELER","PEELER   ","6","12","0","15"},
                                                                {"U","UTILITY","UTILITY  ","4","12","0","66"},
                                                                {"0","CU","CULL     ","4","12","0","66"}};

        public List<exportGrades> createDefaultList(string currentRegion,string sortOrGrades)
        {
            List<exportGrades> egList = new List<exportGrades>();
            exportGrades eg = new exportGrades();

            //  if this is region 10, use those arrays to build the list
            if (currentRegion == "10")
            {
                if (sortOrGrades == "sort")
                {
                    for (int n = 0; n < 21; n++)
                    {
                        eg.exportSort = R10defaultLogSort[n, 0];
                        eg.exportCode = R10defaultLogSort[n, 1];
                        eg.exportName = R10defaultLogSort[n, 2];
                        eg.minDiam = Convert.ToDouble(R10defaultLogSort[n, 3]);
                        eg.minLength = Convert.ToDouble(R10defaultLogSort[n, 4]);
                        eg.minBDFT = Convert.ToDouble(R10defaultLogSort[n, 5]);
                        eg.maxDefect = Convert.ToDouble(R10defaultLogSort[n, 6]);
                        egList.Add(eg);
                    }   //  end for n loop
                }
                else if(sortOrGrades == "grade")
                {
                    for (int n = 0; n < 9; n++)
                    {
                        eg.exportGrade = R10defaultLogGrade[n, 0];
                        eg.exportCode = R10defaultLogGrade[n, 1];
                        eg.exportName = R10defaultLogGrade[n, 2];
                        eg.minDiam = Convert.ToDouble(R10defaultLogGrade[n, 3]);
                        eg.minLength = Convert.ToDouble(R10defaultLogGrade[n, 4]);
                        eg.minBDFT = Convert.ToDouble(R10defaultLogGrade[n, 5]);
                        eg.maxDefect = Convert.ToDouble(R10defaultLogGrade[n, 6]);
                        egList.Add(eg);                        
                    }   //  end for n loop
                }   //  endif sortOrGrades
            }
            else
            {
                if (sortOrGrades == "sort")
                {
                    for (int n = 0; n < 21; n++)
                    {
                        eg.exportSort = defaultLogSort[n, 0];
                        eg.exportCode = defaultLogSort[n, 1];
                        eg.exportName = defaultLogSort[n, 2];
                        eg.minDiam = Convert.ToDouble(defaultLogSort[n, 3]);
                        eg.minLength = Convert.ToDouble(defaultLogSort[n, 4]);
                        eg.minBDFT = Convert.ToDouble(defaultLogSort[n, 5]);
                        eg.maxDefect = Convert.ToDouble(defaultLogSort[n, 6]);
                        egList.Add(eg);
                    }   //  end for n loop
                }
                else if(sortOrGrades == "grade")
                {
                    for (int n = 0; n < 18; n++)
                    {
                        eg.exportGrade = defaultLogGrade[n, 0];
                        eg.exportCode = defaultLogGrade[n, 1];
                        eg.exportName = defaultLogGrade[n, 2];
                        eg.minDiam = Convert.ToDouble(defaultLogGrade[n, 3]);
                        eg.minLength = Convert.ToDouble(defaultLogGrade[n, 4]);
                        eg.minBDFT = Convert.ToDouble(defaultLogGrade[n, 5]);
                        eg.maxDefect = Convert.ToDouble(defaultLogGrade[n, 6]);
                        egList.Add(eg);
                    }
                }   //  endif sortOrGrades
            }   //  endif currentRegion
            return egList;
        }   //  end createDefaultList


        public StringBuilder createInsertQuery(exportGrades eg,string sortOrGrade)
        {
            StringBuilder queryString = new StringBuilder();
            queryString.Append("INSERT INTO ExportValues ");
            queryString.Append("(exportSort,exportGrade,exportCode,exportName,minDiam,minLength,minBDFT,maxDefect)");

            //  enter the actual values for the record
            queryString.Append(" VALUES ('");
            if (sortOrGrade == "sort")
            {
                queryString.Append(eg.exportSort);
                queryString.Append("','','");          //  blank exportGrade
            }
            else if (sortOrGrade == "grade")
            {
                queryString.Append("','");        //  blank exportSort
                queryString.Append(eg.exportGrade);
                queryString.Append("','");
            }   //  endif sortOrGrade
            
            queryString.Append(eg.exportCode);
            queryString.Append("','");
            queryString.Append(eg.exportName);
            queryString.Append("','");
            queryString.Append(eg.minDiam);
            queryString.Append("','");
            queryString.Append(eg.minLength);
            queryString.Append("','");
            queryString.Append(eg.minBDFT);
            queryString.Append("','");
            queryString.Append(eg.maxDefect);
            queryString.Append("');");

            return queryString;
        }   //  end createInsertQuery

    }
}
