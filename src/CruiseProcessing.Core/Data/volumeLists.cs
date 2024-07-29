using CruiseProcessing;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing
{
    public class volumeLists
    {
        public static List<VolEqList> GetVolumeEquationsByRegion(string currentRegion)
        {
            switch (currentRegion)
            {
                case "10":
                    return Region10Equations.ToList();

                case "06":
                    return Region6Equations.ToList();

                case "05":
                    return Region5Equations.ToList();

                case "04":
                    return Region4Eauations.ToList();

                case "03":
                    return Region3Equations.ToList();

                case "02":
                    return Region2Equations.ToList();

                case "01":
                    return Region1Equations.ToList();

                case "11":
                    return DODVolEquations.ToList();
                default:
                    return new List<VolEqList>();
            } 

        }

        //  Region 1 equations
        public static readonly VolEqList[] Region1Equations = new[] {
            new VolEqList("ALL","Balsam fir/Whitebark pine","I00FW2W012","Flewelling Profile Model"),
            new VolEqList("ALL","Cottonwood",               "102DVEW740","Kemp Equation"),
            new VolEqList("ALL","Douglas fir",              "I00FW2W202","Flewelling Profile Model"),
            new VolEqList("ALL","Engelmann's spruce",       "I00FW2W093","Flewelling Profile Model"),
            new VolEqList("ALL","Grand fir",                "I00FW2W017","Flewelling Profile Model"),
            new VolEqList("ALL","Larch/Limber pine",        "I00FW2W073","Flewelling Profile Model"),
            new VolEqList("ALL","Lodgepole pine",           "I00FW2W108","Flewelling Profile Model"),
            new VolEqList("ALL","Western/Mountain hemlock", "I00FW2W260","Flewelling Profile Model"),
            new VolEqList("ALL","Ponderosa pine",           "I00FW2W122","Flewelling Profile Model"),
            new VolEqList("ALL","Quaking aspen",            "102DVEW746","Kemp Equation"),
            new VolEqList("ALL","Subalpine fir",            "I00FW2W019","Flewelling Profile Model"),
            new VolEqList("ALL","Western redcedar",         "I00FW2W242","Flewelling Profile Model"),
            new VolEqList("ALL","Western white pine",       "I00FW2W119","Flewelling Profile Model"),
            new VolEqList("8",  "Ponderosa pine",           "203FW2W122","Flewelling Profile Model")
        };

        //  Region 2 equations
        public static readonly VolEqList[] Region2Equations = new[] {
            new VolEqList("ALL","Bur oak",               "200DVEW823","Chojnacky Equation"),
            new VolEqList("ALL","Douglas fir",           "200FW2W202","Flewelling Profile Model"),
            new VolEqList("ALL","Engelmann's spruce",    "407FW2W093","Flewelling Profile Model"),
            new VolEqList("ALL","Gambel oak",            "200DVEW814","Chojnacky Equation"),
            new VolEqList("ALL","Lodgepole pine",        "200FW2W108","Flewelling Profile Model"),
            new VolEqList("ALL","Mountain mahogany",     "200DVEW475","Chojnacky Equation"),
            new VolEqList("ALL","Oneseed juniper",       "200DVEW069","Chojnacky Equation"),
            new VolEqList("ALL","Other hardwoods",       "200DVEW998","Chojnacky Equation"),
            new VolEqList("ALL","Pinyon pine",           "200DVEW106","Chojnacky Equation"),
            new VolEqList("ALL","Ponderosa pine",        "200FW2W122","Flewelling Profile Model"),
            new VolEqList("ALL","Quaking aspen",         "200FW2W746","Flewelling Profile Model"),
            new VolEqList("ALL","Rocky Mountain juniper","200DVEW066","Chojnacky Equation"),
            new VolEqList("ALL","Subalpine fir",         "I00FW2W019","Flewelling Profile Model"),
            new VolEqList("ALL","White fir",             "200FW2W015","Flewelling Profile Model"),
            new VolEqList("ALL","Utah juniper",          "200DVEW065","Chojnacky Equation"),
            new VolEqList("2",  "Lodgepole pine",        "202FW2W108","Flewelling Profile Model"),
            new VolEqList("3",  "Ponderosa pine",        "203FW2W122","Flewelling Profile Model"),
            new VolEqList("3",  "Ponderosa pine",        "223DVEW122","BlackHills PP Non-sawtimber"),
            new VolEqList("13", "Ponderosa pine",        "213FW2W122","Flewelling Profile Model"),
            new VolEqList("14", "Lodgepole pine",        "202FW2W108","Flewelling Profile Model")
        };

        //  Region 3 equations
        public static readonly VolEqList[] Region3Equations = new[] {
            new VolEqList("ALL", "Juniper",              "300DVEW060", "Chojnacky Equation"),
            new VolEqList("ALL", "Unknown",              "300DVEW999", "Chojnacky Equation"),
            new VolEqList("ALL", "White fir",            "NVB0000015", "National-Scale Volume and Biomass Equation"),
            new VolEqList("ALL", "Douglas fir",          "300FW2W202", "Flewelling Profile Model"),
            new VolEqList("ALL", "Pinyon pine",          "300DVEW106", "Chojnacky Equation"),
            new VolEqList("ALL", "Oak",                  "300DVEW800", "Chojnacky Equation"),
            new VolEqList("ALL", "Ponderosa pine",       "300FW2W122", "Flewelling Profile Model"),
            new VolEqList("ALL", "White Pine",           "NVBM240119", "National-Scale Volume and Biomass Equation"),
            new VolEqList("ALL", "Engelmann's Spruce",   "NVBM330093", "National-Scale Volume and Biomass Equation"),
            new VolEqList("ALL", "Quaking aspen",        "NVB0000746", "National-Scale Volume and Biomass Equation"),
            new VolEqList("ALL", "Maple",                "300DVEW310", "Chojnacky Equation"),
            new VolEqList("ALL", "Black maple",          "300DVEW314", "Chojnacky Equation"),
            new VolEqList("ALL", "Limber pine",          "300DVEW113", "Hann and Bare Equation"),
        };

        //  Region 4 equations
        public static readonly VolEqList[] Region4Eauations = new[] {
			new VolEqList("ALL", "Douglas fir"                    , "400MATW202", "Rustagi Profile Model"),
			new VolEqList("ALL", "Engelmann's/Blue spruce"        , "400MATW093", "Rustagi Profile Model"),
			new VolEqList("ALL", "Grand/White fir"                , "400MATW015", "Rustagi Profile Model"),
			new VolEqList("ALL", "Lodgepole/Limber/Whitebark pine", "400MATW108", "Rustagi Profile Model"),
			new VolEqList("ALL", "Mountain mahogany"              , "400DVEW475", "Chojnacky Equation"),
			new VolEqList("ALL", "Other hardwoods"                , "400DVEW998", "Chojnacky Equation"),
			new VolEqList("ALL", "Pinyon pine"                    , "400DVEW106", "Chojnacky Equation"),
			new VolEqList("ALL", "Ponderosa pine"                 , "400MATW122", "Rustagi Profile Model"),
			new VolEqList("ALL", "Quaking aspen"                  , "400MATW746", "Rustagi Profile Model"),
			new VolEqList("ALL", "Rocky Mountain juniper"         , "400DVEW066", "Chojnacky Equation"),
			new VolEqList("ALL", "Single leaf pinyon pine"        , "400DVEW133", "Chojnacky Equation"),
			new VolEqList("ALL", "Subalpine fir"                  , "400MATW019", "Rustagi Profile Model"),
			new VolEqList("ALL", "Utah juniper-Great Basin"       , "400DVEW065", "Chojnacky Equation"),
			new VolEqList("ALL", "Utah juniper-Colorado Plateau"  , "401DVEW065", "Chojnacky Equation"),
			new VolEqList("ALL", "Western juniper"                , "400DVEW064", "Chojnacky Equation"),
			new VolEqList("1"  , "Ponderosa pine"                 , "401MATW122", "Rustagi Profile Model"),
			new VolEqList("2"  , "Western larch"                  , "400MATW073", "Rustagi Profile Model"),
			new VolEqList("2"  , "Engelmann's/Blue spruce"        , "I15FW2W093", "Flewelling Profile Model"),
			new VolEqList("2"  , "Ponderosa pine"                 , "I15FW2W122", "Flewelling Profile Model"),
			new VolEqList("2"  , "Douglas fir"                    , "I15FW2W202", "Flewelling Profile Model"),
			new VolEqList("2"  , "Grand/White fir"                , "I15FW2W017", "Flewelling Profile Model"),
			new VolEqList("5"  , "Subalpine fir"                  , "405MATW019", "Rustagi Profile Model"),
			new VolEqList("5"  , "Douglas fir"                    , "405MATW202", "Rustagi Profile Model"),
			new VolEqList("6"  , "Western larch"                  , "400MATW073", "Rustagi Profile Model"),
			new VolEqList("7"  , "Ponderosa pine"                 , "402MATW122", "Rustagi Profile Model"),
			new VolEqList("7"  , "Engelmann's/Blue spruce"        , "407FW2W093", "Flewelling Profile Model"),
			new VolEqList("8"  , "Ponderosa pine"                 , "402MATW122", "Rustagi Profile Model"),
			new VolEqList("8"  , "Engelmann's/Blue spruce"        , "407MATW093", "Rustagi Profile Model"),
			new VolEqList("9"  , "Incense cedar"                  , "400MATW081", "Rustagi Profile Model"),
			new VolEqList("9"  , "White fir/Mountain hemlock"     , "401MATW015", "Rustagi Profile Model"),
			new VolEqList("9"  , "Ponderosa pine"                 , "403MATW122", "Rustagi Profile Model"),
			new VolEqList("9"  , "Lodgepole/Limber/Whitebark pine", "401MATW108", "Rustagi Profile Model"),
			new VolEqList("9"  , "California red fir"             , "400MATW020", "Rustagi Profile Model"),
			new VolEqList("9"  , "Sugar/Western white pine"       , "400MATW117", "Rustagi Profile Model"),
			new VolEqList("10" , "Ponderosa pine"                 , "402MATW122", "Rustagi Profile Model"),
			new VolEqList("12" , "Western larch"                  , "400MATW073", "Rustagi Profile Model"),
			new VolEqList("12" , "Engelmann's/Blue spruce"        , "I15FW2W093", "Flewelling Profile Model"),
			new VolEqList("12" , "Douglas fir"                    , "I15FW2W202", "Flewelling Profile Model"),
			new VolEqList("12" , "Grand/White fir"                , "I15FW2W017", "Flewelling Profile Model"),
			new VolEqList("12" , "Ponderosa pine"                 , "I15FW2W122", "Flewelling Profile Model"),
			new VolEqList("13" , "Engelmann's/Blue spruce"        , "I15FW2W093", "Flewelling Profile Model"),
			new VolEqList("13" , "Western larch"                  , "400MATW073", "Rustagi Profile Model"),
			new VolEqList("13" , "Douglas fir"                    , "I15FW2W202", "Flewelling Profile Model"),
			new VolEqList("13" , "Grand/White fir"                , "I15FW2W017", "Flewelling Profile Model"),
			new VolEqList("13" , "Ponderosa Pine"                 , "I15FW2W122", "Flewelling Profile Model"),
			new VolEqList("14" , "Western larch"                  , "400MATW073", "Rustagi Profile Model"),
			new VolEqList("17" , "Incense cedar"                  , "400MATW081", "Rustagi Profile Model"),
			new VolEqList("17" , "Lodgepole/Limber/Whitebark pine", "401MATW108", "Rustagi Profile Model"),
			new VolEqList("17" , "White fir/Mountain hemlock"     , "401MATW015", "Rustagi Profile Model"),
			new VolEqList("17" , "Sugar/Western white pine"       , "400MATW117", "Rustagi Profile Model"),
			new VolEqList("17" , "Ponderosa pine"                 , "403MATW122", "Rustagi Profile Model"),
			new VolEqList("17" , "California red fir"             , "400MATW020", "Rustagi Profile Model"),
			new VolEqList("18" , "Ponderosa pine"                 , "402MATW122", "Rustagi Profile Model"),
			new VolEqList("19" , "Ponderosa pine"                 , "402MATW122", "Rustagi Profile Model")};

        //  Region 5 equations
        public static readonly VolEqList[] Region5Equations = new [] {
            new VolEqList("ALL", "Koa"                             , "H00SN2W301", "Sharpnack Profile Model"),
            new VolEqList("ALL", "Ohia"                            , "H00SN2W671", "Sharpnack Profile Model"),
            new VolEqList("ALL", "Eucalyptus"                      , "H00SN2W510", "Sharpnack Profile Model"),
            new VolEqList("ALL", "Eucalyptus"                      , "H01SN2W510", "Sharpnack Profile Model"),
            new VolEqList("ALL", "E. spruce/Mtn. hemlock/White fir", "500WO2W015", "Wensel and Olsen Profile Model"),
            new VolEqList("ALL", "Western white/Sugar pine"        , "500WO2W117", "Wensel and Olsen Profile Model"),
            new VolEqList("ALL", "Engelmann's oak"                 , "500DVEW811", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Tanoak"                          , "500DVEW631", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Lodgepole pine"                  , "500WO2W108", "Wensel and Olsen Profile Model"),
            new VolEqList("ALL", "Golden chinkapin"                , "500DVEW431", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Giant sequoia"                   , "500DVEW212", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Blue oak"                        , "500DVEW807", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "California black oak"            , "500DVEW818", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Bigleaf maple"                   , "500DVEW312", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Red alder"                       , "500DVEW351", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Interior live oak"               , "500DVEW839", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Redwood"                         , "500WO2W211", "Wensel and Olsen Profile Model"),
            new VolEqList("ALL", "California live oak"             , "500DVEW801", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Jeffrey pine"                    , "500WO2W116", "Wensel and Olsen Profile Model"),
            new VolEqList("ALL", "Incense cedar"                   , "500WO2W081", "Wensel and Olsen Profile Model"),
            new VolEqList("ALL", "California red fir"              , "500WO2W020", "Wensel and Olsen Profile Model"),
            new VolEqList("ALL", "Ponderosa pine"                  , "500WO2W122", "Wensel and Olsen Profile Model"),
            new VolEqList("ALL", "Douglas fir"                     , "500WO2W202", "Wensel and Olsen Profile Model"),
            new VolEqList("ALL", "Juniper"                         , "500DVEW060", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "California white oak"            , "500DVEW821", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Oregon white oak"                , "500DVEW815", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Pacific madrone"                 , "500DVEW361", "Pillsbury and Krikley Equation"),
            new VolEqList("ALL", "California laurel"               , "500DVEW981", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Canyon live oak"                 , "500DVEW805", "Pillsbury and Kirkley Equation"),
            new VolEqList("ALL", "Grand fir"                       , "I15FW2W017", "Flewelling-Central Idaho Model")
        };

        //  per C. Bodenhausen, April 2017, remove all equations related to 532
        /*                                                         {"5",  "Ponderosa pine",                  "532WO2W122","Wensel and Olsen Profile Model"},
                                                                   {"5",  "Jeffrey pine",                    "532WO2W116","Wensel and Olsen Profile Model"},
                                                                   {"5",  "Sugar pine",                      "532WO2W117","Wensel and Olsen Profile Model"},
                                                                   {"5",  "White fir",                       "532WO2W015","Wensel and Olsen Profile Model"},
                                                                   {"5",  "California red fir",              "532WO2W020","Wensel and Olsen Profile Model"},
                                                                   {"5",  "Douglas fir",                     "532WO2W202","Wensel and Olsen Profile Model"},
                                                                   {"5",  "Incense cedar",                   "532WO2W081","Wensel and Olsen Profile Model"},
                                                                   {"5",  "Lodgepole pine",                  "532WO2W108","Wensel and Olsen Profile Model"},
                                                                   {"5",  "Redwood",                         "532WO2W211","Wensel and Olsen Profile Model"},
                                                                   {"6",  "Ponderosa pine",                  "532WO2W122","Wensel and Olsen Profile Model"},
                                                                   {"6",  "Jeffrey pine",                    "532WO2W116","Wensel and Olsen Profile Model"},
                                                                   {"6",  "Sugar pine",                      "532WO2W117","Wensel and Olsen Profile Model"},
                                                                   {"6",  "White fir",                       "532WO2W015","Wensel and Olsen Profile Model"},
                                                                   {"6",  "California red fir",              "532WO2W020","Wensel and Olsen Profile Model"},
                                                                   {"6",  "Douglas fir",                     "532WO2W202","Wensel and Olsen Profile Model"},
                                                                   {"6",  "Incense cedar",                   "532WO2W081","Wensel and Olsen Profile Model"},
                                                                   {"6",  "Lodgepole pine",                  "532WO2W108","Wensel and Olsen Profile Model"},
                                                                   {"6",  "Redwood",                         "532WO2W211","Wensel and Olsen Profile Model"},
                                                                   {"8",  "Ponderosa pine",                  "532WO2W122","Wensel and Olsen Profile Model"},
                                                                   {"8",  "Jeffrey pine",                    "532WO2W116","Wensel and Olsen Profile Model"},
                                                                   {"8",  "Sugar pine",                      "532WO2W117","Wensel and Olsen Profile Model"},
                                                                   {"8",  "White fir",                       "532WO2W015","Wensel and Olsen Profile Model"},
                                                                   {"8",  "California red fir",              "532WO2W020","Wensel and Olsen Profile Model"},
                                                                   {"8",  "Douglas fir",                     "532WO2W202","Wensel and Olsen Profile Model"},
                                                                   {"8",  "Incense cedar",                   "532WO2W081","Wensel and Olsen Profile Model"},
                                                                   {"8",  "Lodgepole pine",                  "532WO2W108","Wensel and Olsen Profile Model"},
                                                                   {"8",  "Redwood",                         "532WO2W211","Wensel and Olsen Profile Model"},
                                                                   {"9",  "Ponderosa pine",                  "532WO2W122","Wensel and Olsen Profile Model"},
                                                                   {"9",  "Jeffrey pine",                    "532WO2W116","Wensel and Olsen Profile Model"},
                                                                   {"9",  "Sugar pine",                      "532WO2W117","Wensel and Olsen Profile Model"},
                                                                   {"9",  "White fir",                       "532WO2W015","Wensel and Olsen Profile Model"},
                                                                   {"9",  "California red fir",              "532WO2W020","Wensel and Olsen Profile Model"},
                                                                   {"9",  "Douglas fir",                     "532WO2W202","Wensel and Olsen Profile Model"},
                                                                   {"9",  "Incense cedar",                   "532WO2W081","Wensel and Olsen Profile Model"},
                                                                   {"9",  "Lodgepole pine",                  "532WO2W108","Wensel and Olsen Profile Model"},
                                                                   {"9",  "Redwood",                         "532WO2W211","Wensel and Olsen Profile Model"},
                                                                   {"10", "Ponderosa pine",                  "532WO2W122","Wensel and Olsen Profile Model"},
                                                                   {"10", "Jeffrey pine",                    "532WO2W116","Wensel and Olsen Profile Model"},
                                                                   {"10", "Sugar pine",                      "532WO2W117","Wensel and Olsen Profile Model"},
                                                                   {"10", "White fir",                       "532WO2W015","Wensel and Olsen Profile Model"},
                                                                   {"10", "California red fir",              "532WO2W020","Wensel and Olsen Profile Model"},
                                                                   {"10", "Douglas fir",                     "532WO2W202","Wensel and Olsen Profile Model"},
                                                                   {"10", "Incense cedar",                   "532WO2W081","Wensel and Olsen Profile Model"},
                                                                   {"10", "Lodgepole pine",                  "532WO2W108","Wensel and Olsen Profile Model"},
                                                                   {"10", "Redwood",                         "532WO2W211","Wensel and Olsen Profile Model"},
                                                                   {"14", "Ponderosa pine",                  "532WO2W122","Wensel and Olsen Profile Model"},
                                                                   {"14", "Jeffrey pine",                    "532WO2W116","Wensel and Olsen Profile Model"},
                                                                   {"14", "Sugar pine",                      "532WO2W117","Wensel and Olsen Profile Model"},
                                                                   {"14", "White fir",                       "532WO2W015","Wensel and Olsen Profile Model"},
                                                                   {"14", "California red fir",              "532WO2W020","Wensel and Olsen Profile Model"},
                                                                   {"14", "Douglas fir",                     "532WO2W202","Wensel and Olsen Profile Model"},
                                                                   {"14", "Incense cedar",                   "532WO2W081","Wensel and Olsen Profile Model"},
                                                                   {"14", "Lodgepole pine",                  "532WO2W108","Wensel and Olsen Profile Model"},
                                                                   {"14", "Redwood",                         "532WO2W211","Wensel and Olsen Profile Model"}};

         */

        //  Region 6 equations
        public static readonly VolEqList[] Region6Equations = new[] {
            new VolEqList("ALL",    "All coniferous",           "616BEHW000", "Behr's Hyperbola"),
            new VolEqList("1",      "Douglas fir",              "I11FW2W202", "INGY 2 Point"),
            new VolEqList("1",      "White fir/Shasta fir",     "I00FW2W017", "INGY 2 Point"),
            new VolEqList("1",      "Lodgepole pine",           "I11FW2W108", "INGY 2 Point"),
            new VolEqList("1",      "Ponderosa pine",           "I11FW2W122", "INGY 2 Point"),
            new VolEqList("1",      "Western larch",            "I11FW2W073", "INGY 2 Point"),
            new VolEqList("1",      "Incense cedar",            "I11FW2W242", "INGY 2 Point"),
            new VolEqList("1",      "Sugar pine",               "I13FW2W122", "INGY 2 Point"),
            new VolEqList("2",      "Incense cedar",            "I00FW2W202", "INGY 2 Point"),
            new VolEqList("2",      "Douglas fir",              "I00FW2W017", "INGY 2 Point"),
            new VolEqList("2",      "Grand fir/White fir",      "I11FW2W017", "INGY 2 Point"),
            new VolEqList("2",      "Lodgepole pine",           "I00FW2W108", "INGY 2 Point"),
            new VolEqList("2",      "Ponderosa pine",           "I00FW2W122", "INGY 2 Point"),
            new VolEqList("3",      "Pacific silver fir",       "I12FW2W017", "INGY 2 Point"),
            new VolEqList("3",      "Douglas fir",              "F03FW2W202", "Flewelling Westside"),
            new VolEqList("3",      "Douglas fir",              "F07FW2W202", "Flewelling Westside"),
            new VolEqList("3",      "Western hemlock",          "F00FW2W263", "Flewelling Westside"),
            new VolEqList("3",      "Western hemlock",          "F04FW2W263", "Flewelling Westside"),
            new VolEqList("3",      "Western hemlock",          "F06FW2W263", "Flewelling Westside"),
            new VolEqList("3",      "Subalpine fir",            "I00FW2W108", "INGY Area Wide"),
            new VolEqList("3",      "Red Alder",                "A16CURW351", "Curtis Model"),
            new VolEqList("4",      "Grand fir/White fir",      "I12FW2W017", "INGY 2 Point"),
            new VolEqList("4",      "Lodgepole pine",           "I12FW2W108", "INGY 2 Point"),
            new VolEqList("4",      "Ponderosa pine",           "I12FW2W122", "INGY 2 Point"),
            new VolEqList("4",      "Douglas fir",              "I12FW2W202", "INGY 2 Point"),
            new VolEqList("5",      "Douglas fir",              "F08FW2W202", "Flewelling Westside"),
            new VolEqList("5",      "Western hemlock",          "F03FW2W263", "Flewelling Westside"),
            new VolEqList("6",      "Western hemlock",          "I11FW2W260", "INGY 2 Point"),
            new VolEqList("6",      "Pacific silver fir",       "I12FW2W017", "INGY 2 Point"),
            new VolEqList("6",      "Noble fir/Grand fir",      "I13FW2W017", "INGY 2 Point"),
            new VolEqList("6",      "Douglas fir",              "F05FW2W202", "Flewelling Westside"),
            new VolEqList("6",      "Douglas fir",              "F03FW2W202", "Flewelling Westside"),
            new VolEqList("6",      "Englemann spruce",         "I11FW2W093", "INGY 2 Point"),
            new VolEqList("6",      "Lodgepole pine",           "I11FW2W108", "INGY 2 Point"),
            new VolEqList("6",      "Ponderosa pine",           "I12FW2W122", "INGY 2 Point"),
            new VolEqList("7",      "Grand fir/White fir",      "I12FW2W017", "INGY 2 Point"),
            new VolEqList("7",      "Western larch",            "I12FW2W073", "INGY 2 Point"),
            new VolEqList("7",      "Lodgepole pine",           "I12FW2W108", "INGY 2 Point"),
            new VolEqList("7",      "Ponderosa pine",           "I12FW2W122", "INGY 2 Point"),
            new VolEqList("7",      "Douglas fir",              "I12FW2W202", "INGY 2 Point"),
            new VolEqList("9",      "Western hemlock",          "F00FW2W263", "Flewelling Westside"),
            new VolEqList("9",      "Douglas fir",              "F03FW2W202", "Flewelling Westside"),
            new VolEqList("9",      "Sitka spruce",             "F03FW2W263", "Flewelling Westside"),
            new VolEqList("10",     "Douglas fir",              "F06FW2W202", "Flewelling 2 Point"),
            new VolEqList("10",     "Ponderosa pine",           "I00FW2W073", "INGY 2 Point"),
            new VolEqList("10",     "White fir",                "I00FW2W093", "INGY 2 Point"),
            new VolEqList("10",     "Western hemlock",          "F06FW2W263", "fLEWELLING 2 Point"),
            new VolEqList("12",     "Douglas fir",              "F00FW2W202", "Flewelling 2 Point"),
            new VolEqList("12",     "Western hemlock",          "F03FW2W263", "Flewelling 2 Point"),
            new VolEqList("12",     "Red Alder",                "NVBM240351", "National-Scale Volume and Biomass Equation"),
            new VolEqList("14",     "Subalpine fir",            "I00FW2W019", "INGY 2 Point"),
            new VolEqList("14",     "Engelmann's spruce",       "I00FW2W093", "INGY 2 Point"),
            new VolEqList("14",     "Lodgepole pine",           "I00FW2W108", "INGY 2 Point"),
            new VolEqList("14",     "Douglas/White fir/Grand fir", "I13FW2W017", "INGY 2 Point"),
            new VolEqList("14",     "Ponderosa pine",           "I13FW2W122", "INGY 2 Point"),
            new VolEqList("14",     "Western larch",            "I13FW2W202", "INGY 2 Point"),
            new VolEqList("15",     "Douglas fir",              "F00FW2W202", "Flewelling 2 Point"),
            new VolEqList("15",     "Western hemlock",          "I11FW2W260", "INGY 2 Point"),
            new VolEqList("15",     "White fir",                "I00FW2W017", "INGY 2 Point"),
            new VolEqList("15",     "Lodgepole pine",           "I00FW2W108", "INGY 2 Point"),
            new VolEqList("15",     "Ponderosa pine/incense cedar", "I00FW2W073", "INGY 2 POINT"),
            new VolEqList("15",     "Mountain hemlock",         "I00FW2W242", "INGY 2 Point"),
            new VolEqList("15",     "Engelmann's spruce",       "I00FW2W093", "INGY 2 Point"),
            new VolEqList("15",     "Shasta fir/Red cedar",     "I00FW2W012", "INGY 2 Point"),
            new VolEqList("15",     "Knob Cone pine",           "I00FW2W260", "Flewelling 2 Point"),
            new VolEqList("15",     "Pacific Silver fir",       "I13FW2W017", "INGY 2 Point"),
            new VolEqList("16",     "Grand/White fir",          "I11FW2W017", "INGY 2 Point"),
            new VolEqList("16",     "Ponderosa pine",           "I11FW2W122", "INGY 2 Point"),
            new VolEqList("16",     "Douglas fir",              "I11FW2W202", "INGY 2 Point"),
            new VolEqList("16",     "Western larch",            "I13FW2W073", "INGY 2 Point"),
            new VolEqList("16",     "Lodgepole pine",           "I00FW2W108", "INGY 2 Point"),
            new VolEqList("16",     "Engelmann's spruce",       "I11FW2W093", "INGY 2 Point"),
            new VolEqList("17",     "Ponderosa pine/Western larch", "I11FW2W122", "INGY 2 Point"),
            new VolEqList("17",     "Douglas fir",              "I12FW2W202", "INGY 2 Point"),
            new VolEqList("17",     "Douglas fir",              "I00FW2W202", "INGY 2 Point"),
            new VolEqList("17",     "Ponderosa pine",           "I12FW2W122", "INGY 2 Point"),
            new VolEqList("17",     "Lodgepole pine",           "I12FW2W108", "INGY 2 Point"),
            new VolEqList("17",     "Engelmann's spruce",       "I11FW2W093", "INGY 2 Point"),
            new VolEqList("17",     "Grand fir",                "I11FW2W017", "INGY 2 Point"),
            new VolEqList("18",     "Douglas fir",              "F05FW2W202", "Flewelling 2 Point"),
            new VolEqList("18",     "Noble fir",                "I00FW2W108", "Flewelling 2 Point"),
            new VolEqList("18",     "Western hemlock",          "F03FW2W263", "Flewelling 2 Point"),
            new VolEqList("18",     "Grand fir",                "I00FW2W017", "INGY 2 Point"),
            new VolEqList("21",     "Douglas/Alpine fir",       "I11FW2W202", "INGY 2 Point"),
            new VolEqList("21",     "Lodgepole pine",           "I11FW2W108", "INGY 2 Point"),
            new VolEqList("21",     "Western larch",            "I11FW2W073", "INGY 2 Point"),
            new VolEqList("21",     "Western red cedar",        "I11FW2W242", "INGY 2 Point"),
            new VolEqList("21",     "Engelmann's spruce",       "I13FW2W093", "INGY 2 Point"),
            new VolEqList("21",     "Grand fir/Hemlock",        "I11FW2W017", "INGY 2 Point"),
            new VolEqList("21",     "Ponderosa pine",           "I12FW2W122", "INGY 2 Point"),
            new VolEqList("21",     "White pine",               "I00FW2W119", "INGY 2 Point")
        };

        //  Region 10 equations
        public static readonly VolEqList[] Region10Equations = new[] {
            new VolEqList("ALL","Red alder"              , "A32CURW351","Curtis Profile Model"),
            new VolEqList("ALL","Sitka spruce"           , "A00F32W098","Flewelling Profile-32ft Log Rule"),
            new VolEqList("ALL","Western hemlock"        , "A00F32W263","Flewelling Profile-32ft Log Rule"),
            new VolEqList("4"  , "White spruce"          , "A00DVEW094","Larson Volume Equation"),
            new VolEqList("4"  , "Black spruce"          , "A00DVEW094","Larson Volume Equation"),
            new VolEqList("4"  , "Cottongwood"           , "A00DVEW747","Larson Volume Equation"),
            new VolEqList("4"  , "Mountain hemlock"      , "A01DEMW263","Demars Profile Model"),
            new VolEqList("4"  , "Paper birch"           , "A00DVEW375","Larson Volume Equation"),
            new VolEqList("4"  , "Quaking aspen"         , "A00DVEW351","Larson Volume Equation"),
            new VolEqList("5"  , "Alaska yellow cedar"   , "A00F32W042","Flewelling Profile -32ft Log Rule"),
            new VolEqList("5"  , "Western redcedar"      , "A00F32W242","Flewelling Profile -32ft Log Rule"),
            new VolEqList("5"  , "Hemlock -second growth", "A02F32W263","Flewelling Profile-32ft Log Rule"),
            new VolEqList("5"  , "Spruce -second growth" , "A02F32W098","Flewelling Profile-32ft Log Rule"),
        };

        //  DOD equations ("Region 11")
        public static readonly VolEqList[] DODVolEquations = new[] {
            new VolEqList("ALL", "Douglas fir"     , "F00FW2W202", "Flewelling 2 Point"),
            new VolEqList("ALL", "Douglas fir"     , "F02FW2W202", "Flewelling 2 Point"),
            new VolEqList("ALL", "Douglas fir"     , "F07FW2W202", "Flewelling 2 Point"),
            new VolEqList("ALL", "Douglas fir"     , "F00F32W202", "Flewelling Profile-32ft Log Rule"),
            new VolEqList("ALL", "Douglas fir"     , "F02F32W202", "Flewelling Profile-32ft Log Rule"),
            new VolEqList("ALL", "Douglas fir"     , "F07F32W202", "Flewelling Profile-32ft Log Rule"),
            new VolEqList("ALL", "Western hemlock" , "F00FW2W263", "Flewelling 2 Point"),
            new VolEqList("ALL", "Western hemlock" , "F02FW2W263", "Flewelling 2 Point"),
            new VolEqList("ALL", "Western hemlock" , "F07FW2W263", "Flewelling 2 Point"),
            new VolEqList("ALL", "Western hemlock" , "F00F32W263", "Flewelling Profile-32ft Log Rule"),
            new VolEqList("ALL", "Western hemlock" , "F02F32W263", "Flewelling Profile-32ft Log Rule"),
            new VolEqList("ALL", "Western hemlock" , "F07F32W263", "Flewelling Profile-32ft Log Rule"),
            new VolEqList("ALL", "Western redcedar", "F00FW2W242", "Flewelling 2 Point"),
            new VolEqList("ALL", "Western redcedar", "F02FW2W242", "Flewelling 2 Point"),
            new VolEqList("ALL", "Western redcedar", "F07FW2W242", "Flewelling 2 Point"),
            new VolEqList("ALL", "Western redcedar", "F00F32W242", "Flewelling Profile-32ft Log Rule"),
            new VolEqList("ALL", "Western redcedar", "F02F32W242", "Flewelling Profile-32ft Log Rule"),
            new VolEqList("ALL", "Western redcedar", "F07F32W242", "Flewelling Profile-32ft Log Rule"),
            new VolEqList("ALL", "Other species"   , "616BEHW000", "Behre's Hyperbola"),
            new VolEqList("ALL", "Other species"   , "628BEHW000", "Behre's Hyperbola")
        };
    }
}