using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public static class reportConstants
    {
        //  This contains constant like phrases or lines used in most reports

        //  Constant fields
        public const string FCTO = "FOR CUT TREES ONLY";
        public const string FLTO = "FOR LEAVE TREES ONLY";
        public const string BCUFS = "BY CUTTING UNIT FOR THE SALE";
        public const string WPUAS = "WITHIN PAYMENT UNITS ACROSS STRATA";
        public const string FPPO = "FOR PRIMARY PRODUCT ONLY";
        public const string B2DC = "BY 2\" DIAMETER CLASS";
        public const string B1DC = "BY 1\" DIAMETER CLASS";
        public const string twoInchDC = " (12-inch class = 11.0 - 12.9 inches; 14-inch class = 13.0 - 14.9 inches, and so on.)";
        public const string oneInchDC = " (5-inch class = 4.6 - 5.5 inches; 9-inch class = 8.6 - 9.5 inches, and so on.)";
        public const string FTS = "FOR THE SALE";
        public const string FCTO_PPO = "FOR CUT TREES ONLY -- PRIMARY PRODUCT ONLY";
        public const string ENOT = "ESTIMATED NUMBER OF TREES";
        public const string PASP = "PRIMARY AND SECONDARY PRODUCTS";
        public const string FSTO = "(FOR SAWTIMBER ONLY : PROD = 01)";
        public const string FNSTO = "(FOR NON-SAWTIMBER ONLY : PROD NOT = 01)";
        public const string WCUAAS = "BY XXX WITHIN CUTTING UNIT ACROSS ALL STRATA";
        public const string SVSG = "SALE VOLUME (XXX) BY SPECIES BY GRADE";       //  R003/R004 reports (Region 10)
        public const string volumeType = "-- XXXXX FOOT --";                      //  R102/103 (Region 1)and R401/R402/R403/R404 (region 4)
        public const string FCLT = "FOR CUT AND LEAVE TREES";                     //  R104 (Region 1)
        public const string NSPO = "SAWTIMBER AND NON-SAWTIMBER PRODUCTS";                   //  A15 merch rules report
        //  Constant lines
        public const string longLine = "____________________________________________________________________________________________________________________________________";
        public const string subtotalLine1 = "__________________________________________________________________________________________________________________";
        public const string shortLine = "__________________________________________________";
        public const string subtotalLine2 = "_____________________________________________________________________________________";

    }
}
