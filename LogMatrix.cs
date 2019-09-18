using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class LogMatrix
    {
        public string nameSale { get; set; }
        public string cruiseNumb { get; set; }
        public string logSortDescrip { get; set; }
        public string speciesCode { get; set; }
        public string logGradeCode { get; set; }
        public string logSED { get; set; }
        public string standingGross { get; set; }
        public string grossRemoved { get; set; }
        public string netRemoved { get; set; }
        public string netUtility { get; set; }


        public string[] sortDescrip008 = new string[] {"AYC #PS sawlog MBF net       ","AYC #1sawlog MBF net         ",
                                                    "AYC SM sawlog MBF net        ","AYC #2 sawlog MBF net        ",
                                                    "AYC #3 sawlog MBF net        ","H 1saw 24+ SED               ",
                                                    "H 2saw 15_0 thru 17_9 SED    ","H 2saw 18_0 thru 23_9 SED    ",
                                                    "H 2saw 24+ SED               ","H 3saw greater than 14_9 SED ",
                                                    "H Between 12_0 and 14_9 SED  ","H Between 8_0 and 11_9 SED   ",
                                                    "H less than 8 SED            ","H PSsaw 24+ SED              ",
                                                    "H SMsaw 16_0 thru 23_9 SED   ","H SMsaw 24+ SED              ",
                                                    "SS 1saw 24+ SED              ","SS 2saw 15_0 thru 17_9 SED   ",
                                                    "SS 2saw 18_0 thru 23_9 SED   ","SS 2saw 24+ SED              ",
                                                    "SS 3saw greater than 14_9 SED","SS less than 15 SED          ",
                                                    "SS PSsaw 30+ SED             ","SS SMsaw 16_0 thru 23_9 SED  ",
                                                    "SS SMsaw 24+ SED             ","WRC all diam & grades        "};
        public string[] species008 = new string[] {"042   ","042   ","042   ","042   ","042   ","263   ","263   ",
                                                "263   ","263   ","263   ","263   ","263   ","263   ","263   ",
                                                "263   ","263   ","098   ","098   ","098   ","098   ","098   ",
                                                "098   ","098   ","098   ","098   ","242   "};
        public string[] grade008 = new string[] {"0            ","1            ","6            ","2            ",
                                              "3            ","1            ","2            ","2            ",
                                              "2            ","3            ","2 or 3       ","3            ",
                                              "3            ","0            ","6            ","6            ",
                                              "1            ","2            ","2            ","2            ",
                                              "3            ","2 or 3       ","0            ","6            ",
                                              "6            ","5 (camprun)  "};
        public string[] diameter008 = new string[] {"                       ","                       ",
                                                 "                       ","                       ",
                                                 "                       ","24.0+                ",
                                                 "15.0 thru 17.9       ","18.0 thru 23.9       ",
                                                 "24.0+                ","greater than 14.9    ",
                                                 "between 12.0 and 14.9","between 8.0 and 11.9 ",
                                                 "less than 8.0        ","24.0+                ",
                                                 "16.0 thru 23.9       ","24.0+                ",
                                                 "24.0+                ","15.0 thru 17.9       ",
                                                 "18.0 thru 23.9       ","24.0+                ",
                                                 "greater than 14.9    ","less than 15.0       ",
                                                 "30.0+                ","16.0 thru 23.9       ",
                                                 "24+                  ","greater than 5.9     "};
        public string[] sortDescrip009 = new string[] {"AYC #3 sawlog                ","AYC #2 sawlog                ",
                                                    "AYC SM sawlog                ","AYC #1 sawlog                ",
                                                    "AYC #Peeler sawlog           ","H 3saw 6+ SED                ",
                                                    "H 2saw 12_0 thru 19_9 SED    ","H 2saw 20+ SED               ",
                                                    "H SM 1saw Peeler 20+ SED     ","Hem YG 3saw 6+ SED           ",
                                                    "Hem YG 2saw & Btr            ","SS YG 3saw 6+ SED            ",
                                                    "SS YG 2saw & Btr             ","SS 3saw 6+ SED               ",
                                                    "SS 2saw 12_0 thru 17_9 SED   ","SS 2saw 18+ SED              ",
                                                    "SS SM 1saw Select 18+ SED    ","WRC less than 24_0 sawlog    ",
                                                    "WRC 24+ sawlog               ","Red Alder all diam & grades  ",
                                                    "Hem YG 3saw 8+ SED           ","Hem YG 2saw & Btr            ",
                                                    "SS YG 3saw 8+ SED            ","SS YG 2saw & Btr             "};
        public string[] species009 = new string[] {"042   ","042   ","042   ","042   ","042   ","263   ",
                                                "263   ","263   ","263   ","263Y  ","263Y  ","098Y  ",
                                                "098Y  ","098   ","098   ","098   ","098   ","242   ",
                                                "242   ","351   ","263Y  ","263Y  ","098Y  ","098Y  "};
        public string[] grade009 = new string[] {"3            ","2            ","6            ",
                                              "1            ","0            ","3            ",
                                              "2 & 6        ","2            ","6 & 1 & 0    ",
                                              "3            ","2 & 6 & 1 & 0","3            ",
                                              "2 & 6 & 1 & 0","3            ","2 & 6        ",
                                              "2            ","6 & 1 & 0    ","5 (camprun)  ",
                                              "5 (camprun)  ","1 or 2 or 3  ","3            ",
                                              "2 & 6 & 1 & 0","3            ","2 & 6 & 1 & 0"};
        public string[] diameter009 = new string[] {"                       ","                       ",
                                                 "                       ","                       ",
                                                 "                       ","6.0+                 ",
                                                 "12.0 thru 19.9999    ","20.0+                ",
                                                 "20.0+                ","6.0+                 ",
                                                 "12.0+                ","6.0+                 ",
                                                 "12.0+                ","6.0+                 ",
                                                 "12.0 thru 17.9999    ","18.0+                ",
                                                 "18.0+                ","less than 24.0       ",
                                                 "24.0+                ","6.0+                 ",
                                                 "8.0+                 ","12.0+                ",
                                                 "8.0+                 ","12.0+                "};
    }   //  end LogMatrix
}