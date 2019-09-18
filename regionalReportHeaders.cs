using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class regionalReportHeaders
    {
        //  Region 1 reports
        //  R101 report
        public string[] R101columns = new string[10] {"  C                      ******************************************** PRIMARY PRODUCT *********************************************",
                                                      "  O",
                                                      "  N                        AVGDEF",
                                                      "  T     S       P                       GROSS      NET",
                                                      "  R     P       R   U     %      %                         ************************** CONTRACT SPECIES  ***************************",
                                                      "        E       O                       BDFT       BDFT                                                             16'LOGS 16'LOGS",
                                                      "  S     C       D   O     B      C",
                                                      "  P     I       U   F     D      U      CUFT       CUFT       AVG       ** GROSS VOLUME **     **  NET VOLUME **     GROSS   GROSS",
                                                      "  E     E       C         F      F",
                                                      "  C     S       T   M     T      T      RATIO      RATIO      DBH         BDFT       CUFT       BDFT       CUFT       CCF     MBF"};
        //  R102/R103 reports
        //  BDFT OR CUFT
        public string[] R102R103columns = new string[7] {"  L  P",
                                                          "  O  R",
                                                          "  G  O",
                                                          "     D",
                                                          "  M  U                                                 GROSS       NET                              NET   16'LOGS/",
                                                          "  T  C          GROSS    TOTAL    NET       ESTIM      XX/         XX/   TREES/   MEAN     MEAN     XX/   GROSS    AVG",
                                                          "  H  T          XX       DEF%     XX        TREES      ACRE        ACRE  ACRE     DBH      HGT      TREE  XXX      SLOPE     ACRES"};
        //  R104 report
        public string[] R104columns = new string[2] {"           CUTTING               SAMPLE    PRIMARY     CUT/      AVG      BASAL",

                                                     " STRATA    UNIT        SPECIES   GROUP     PRODUCT     LEAVE     DBH      AREA"};
        public string[] R105sectionOne = new string[7] {"                    *************** SAWTIMBER **************   ************* NON-SAWTIMBER ****************",
                                                        "                             (PROD = 01   UM = 01, 03)            (PROD NOT = 01   UM = 01, 02, 03)",
                                                        "        A                                                     (AND ALL SECONDARY & RECOVERED PRODUCT VOLUMES)",
                                                        "  U     C      ",
                                                        "  N     R            ** GROSS VOLUME **    ** NET VOLUME **      ***   GROSS    ***   ***   NET    ***",
                                                        "  I     E    ",
                                                        "  T     S            BDFT       CUFT       BDFT       CUFT          BDFT      CUFT       BDFT      CUFT  "};
        public string[] R105sectionTwo = new string[5] {" ",
                                                        " S U B T O T A L S    **************SAWTIMBER**************     ************NON-SAWTIMBER**************",
                                                        " ",
                                                        "                    ** GROSS VOLUME **     ** NET VOLUME **        GROSS     GROSS      NET       NET",
                                                        "          SPECIES     BDFT       CUFT       BDFT       CUFT        BDFT      CUFT       BDFT      CUFT "};
        public string[] R105sectionThree = new string[5]{" ",
                                                           "T O T A L S",
                                                           "                      GROSS         NET             ************** VOLUME  ***************",
                                                           "           PRODUCT    BF/CF         BF/CF           ***** GROSS *****     *****  NET *****",
                                                           "  PRODUCT  SOURCE     RATIO         RATIO           BDFT         CUFT     BDFT        CUFT "};
        //  Region 2 reports
        //  R201 report
        public string[] R201columns = new string[3] {"                                                                 PRIMARY PRODUCT        SECONDARY PRODUCT",
                                                     "                   PRIMARY  SECONDARY    EST.       TREES           NET CUBIC              NET CUBIC              STRATUM",
                                                     " STRATUM  SPECIES  PRODUCT  PRODUCT      # TREES    PER ACRE     VOLUME   PER ACRE      VOLUME   PER ACRE         ACRES"};
        //  R203 through R205 are like stand tables so headers are created on the fly from species in the data
        //  R206 report
        public string[] R206columns = new string[3] {"                               NET        BDFT/CUFT",
                                                     "  CONTRACT                     CUFT       RATIO BY   QMD BY CONTRACT    AVG DBH BY",
                                                     "  SPECIES   SPECIES            VOLUME     CONTR SPP      SPECIES      CONTRACT SPECIES"};
        //  R207 report
        public string[] R207columns = new string[3] {"                             FIXED                                    TREES",
                                                     " CUTTING             # OF    PLOT                          TOTAL      PER       AVG",
                                                     " UNIT      STRATUM   PLOTS   SIZE    SPECIES    PRODUCT    STEMS      ACRE      DRC"};
 
        //  R208 report
        public string[] R208columns = new string[4] {"                                                  NET              % NET        LBS PER      WEIGHT",
                                                     "          UNIT         SPECIES    PRODUCT       CF VOLUME          VOLUME         CF         FRACTION",
                                                     "                                SCALING     ADJUSTED WEIGHT     LBS PER   LBS PER   TONS PER    COST PER",
                                                     "        SPECIES      PRODUCT    DEFECT %    FACTOR LBS PER CF      CF        CCF       CCF        TON"};

        //  Region 3 reports
        //  R301 report
        public string[] R301columns = new string[3] {"                                                     AVE      AVE    QUAD",
                                                     "           CONT    *********CCF********    TOTAL    GROSS    GR0SS   MEAN    RATIO    *****MBF****     CCF",
                                                     " SPEC   PD SPEC    GROSS     NET    DEF    TREES  CF/TREE  CF/ACRE    DBH   MBF:CCF   GROSS    NET     TOP"};
        public string R3specialLine1 = "                    ---------------------------------------------------------------------------------------";
        //  per Karen Jones -- drop the summary at the bottom of the R301 report -- January 2014
        //public string R3specialLine2 = "                   ----------------------------------------------------";
        //public string[] R301total = new string[5] {" ******* PP SAWTIMBER QUALITY ADJUSTMENT AND VALUE INFORMATION ********",
        //                                           "                                       $/CCF                   LUMBER",
        //                                           "         CONT               PCT     TEA#- 22004    RATIO      RECOVERY",
        //                                           " SPEC PD SPEC      QMDBH   GRADE    QUALITY-ADJ   MBF:CCF      FACTOR",
        //                                           " ----------------------------------------------------------------------"};

        //  Region 4 reports
        //  R401/R402 reports
        //  Replace X's and Z's with BDFT (BF or MBF) or CUFT (CF or CCF) terms as needed
        public string[] R401R402columns = new string[7] {"   S     P",
                                                         "   P     R",
                                                         "   E     O",
                                                         "   C     D                                                                                                                  NET LOG",
                                                         "   I     U                                                  GROSS      NET                QUAD              NET   16'LOGS/  SCALE",
                                                         "   E     C      GROSS     TOTAL        NET       ESTIM      XX/        XX/     TREES/     MEAN     MEAN     XX/   GROSS     VALUE/",
                                                         "   S     T      XX        DEF%         XX        TREES      ACRE       ACRE    ACRE       DBH      HT1      TREE  ZZZ       ZZZ"};
        //  R403/R404 reports
        //  Replace X's and Z's with BDFT (BF or MBF) or CUFT (CF or CCF) terms as needed
        public string[] R403R404columns = new string[7] {"   L    P",
                                                         "   O    R",
                                                         "   G    O",
                                                         "        D",
                                                         "   M    U                                                   GROSS      NET                QUAD            NET    16'LOGS/",
                                                         "   T    C       GROSS     TOTAL        NET       ESTIM      XX/        XX/     TREES/     MEAN     MEAN   XX/    GROSS",
                                                         "   H    T       XX        DEF%         XX        TREES      ACRE       ACRE    ACRE       DBH      HT1    TREE   ZZZ       ACRES"};
        //  Region 5 reports
        //  R501 report
        //  Replace X's, Z's and T's with appropriate data
        public string[] R501columns = new string[6] {" TABLE XX -  REPORT FOR SPECIES: ZZZZZZ",
                                                     "                       PRODUCT: TT",
                                                     " ",
                                                     "     LOG             #",
                                                     "     DIB            OF            GROSS             NET           GROSS             NET",
                                                     "   CLASS          LOGS             BDFT            BDFT            CUFT            CUFT"};
        //  Region 6 reports
        //  R602 report
        public string[] R602columns = new string[3] {"                                                    U                      ** NET  VOLUME **",
                                                     "  PAY   CUTTING  UNIT   LOGGING                     OF     %",
                                                     "  UNIT  UNIT     ACRES  METHOD   SPECIES  PRODUCT   M    DEFECT                  CCF"};
        //  R604/R605 reports
        //  Replace X's, Z's and T's with appropriate data
        public string[] R604R605columns = new string[7] {" TABLE XX",
                                                         " REPORT FOR SPECIES:  ZZZZZZ",
                                                         "        AVERAGE DBH:   TTTTTT\"",
                                                         " ",
                                                         "     LOG DIB                  SAW TIMBER                        NON-SAW TIMBER            CULL LOG        TOTAL           TOTAL",
                                                         "    SMALL END         ***** PRIMARY PRODUCT *****        ****** OTHER PRODUCT ******      (GRADE 9)   SAW+NONSAW+CULL   SAW+NONSAW",
                                                         "   1\" DIA. CLASS      GROSS       NET    % DEFECT        GROSS       NET    % DEFECT       VOLUME      GROSS VOLUME     NET VOLUME"};
        //  Region 8 reports
        //  R801 report
        public string[] R801columns = new string[5] {"SALE CUT ACRES:  XXXXX",
                                                     " ",
                                                     "       PINE (SOFTWOOD) SAWTIMBER  HARDWOOD SAWTIMBER       PINE PRODUCT 08  HARDWOOD PRODUCT 08  PINE PULPWOOD     HARDWOOD PULPWOOD",
                                                     "       EST. #                     EST. #                   EST. #           EST. #               EST. #            EST. #           ",
                                                     " DBH   OF TREES    MBF      CCF   OF TREES   MBF     CCF   OF TREES    CCF  OF TREES    CCF      OF TREES   CCF    OF TREES   CCF   "};
        public string[] R801subtotal = new string[2] {"                                                                                         TOPWOOD:       ",
                                                      "                                                                        TOTAL TOPWOOD & PULPWOOD:       "};
        public string[] R801summary = new string[3] {"                                                                        ******** PER/ACRE ********",
                                                     "                        AVG      AVG     AVG     AVG      AVG UPPER     EST. #",
                                                     "                      BF/TREE  CF/TREE   DBH    SWT HT     STEM HT      OF TREES    MBF      CCF"};
        public string[] R801lines = new string[7] {"     PINE SAWTIMBER: ",
                                                   " HARDWOOD SAWTIMBER: ",
                                                   "      PINE PRODUCTS: ",
                                                   "  HARDWOOD PRODUCTS: ",
                                                   "      PINE PULPWOOD: ",
                                                   "  HARDWOOD PULPWOOD: ",
                                                   "      TOTAL/AVERAGE: "};
        //  R802 report
        public string[] R802sawtimber = new string[4] {"                                  *********************  SAWTIMBER  *********************",
                                                       " ",
                                                       "                                  SAMPLE          EST. #                         TOPWOOD",
                                                       "                                  GROUP  SPECIES  OF TREES    MBF        CCF       CCF"};
        public string[] R802product08 = new string[4] {"                                  *********************  PRODUCT 8  *********************",
                                                       " ",
                                                       "                                  SAMPLE          EST. #                TOPWOOD",
                                                       "                                  GROUP  SPECIES  OF TREES      CCF       CCF"};
        public string[] R802pulpwood = new string[4] {"                                           ************  PULPWOOD  ************",
                                                      " ",
                                                      "                                           SAMPLE             EST. #",
                                                      "                                           GROUP    SPECIES   OF TREES   CCF"};

        //  Region 9 Report
        public string[] R902columns = new string[3] {"                                    ***TIPWOOD***",
                                                     "                        ESTIMATED       GROSS",
                                                     "     UNIT   SPECIES     # OF TREES      CUFT"};
                         
        //  Region 10 reports
        //  R001/R002 report
        //  January 2017 -- remove references to utility as they no longer sell utility volume
        public string[] R001R002columns = new string[5] {"           SALE VOLUMES    (XXX)    BY SPECIES",
                                                         "      ---------------------------------------------",
                                                         "                   GROSS         NET    ",
                                                         "                   Sawlog        Sawlog ",
                                                         "      Species      Removed       Removed"};
        //public string[] R001R002columns = new string[6] {"                            SALE VOLUMES    (XXX)    BY SPECIES",
        //                                                 "      ------------------------------------------------------------------",
        //                                                 "                   Gross         NET                   Gross      NET",
        //                                                 "                   Standing      Sawlogs               Removed    Sawlogs",
        //                                                 "                   cut trees     with        Total     without    without",
        //                                                 "      Species      with utility  utility     utility   utility    utility"};
        public string R10specialLine = "      ----------   ------------  ------------";
        public string[] R001R002parTwo = new string[12]{"                       LOGGING CHARACTERISTICS",
                                                        "     --------------------------------------------------------------------",
                                                        "      No. of 32-Ft Logs/XXX Gross removed       ",
                                                        "      Avg. XXX Gross removed per 32 ft. log     ",
                                                        "      Avg. XXX Net removed per 32 ft. log       ",
                                                        "      Avg. XXX Gross removed per acre           ",
                                                        "      Avg. XXX Net removed per acre             ",
                                                        "      Avg. Scaling Defect %                     ",
                                                        "      Quad Mean DBH (for cut trees)             ",
                                                        " ",
                                                        "      Average cut tree volume, Net xxx          ",
                                                        "      Sale xxx/zzz conversion                   "};
         
        //public string[] R001R002parTwo = new string[9] {"                       LOGGING CHARACTERISTICS",
          //                                              "     --------------------------------------------------------------------",
            //                                            "      No. of 32-Ft Logs/XXX Gross removed (without utility)      ",
              //                                          "      Avg. XXX Gross removed (without utility) per 32 ft. log    ",
                //                                        "      Avg. XXX Gross removed (without utility) per acre          ",
                  //                                      "      Avg. Scaling Defect % (without utility)                    ",
                    //                                    " ",
                      //                                  "      % Woods Defect based on Gross standing (with utility)      ",
                        //                                "      Weighted Ave DBH (for cut trees)                           "};
        //  R003/R004 report
        //  These reports build the header from species codes
        //  R005 report
        //  Replace X's with MBF or CCF depending on the program
        public string[] R005columns = new string[7] {"  L    S    P",
                                                     "  O    P    R",
                                                     "  G    E    O",
                                                     "       C    D",
                                                     "  M    I    U                TOTAL   GROSS/   AVERAGE  TOTAL        NET                                             STANDING",
                                                     "  T    E    C  SCALE  WOODS  GROSS   ACRE     LOG      NET          CCF/   ESTIM  TREES/    MEAN   MEAN    LOGS/    GROSS/",
                                                     "  H    S    T  DEF %  DEF %  REMOVED REMOVED  VOLUME   CCF          ACRE   TREES  ACRE      DBH    HGT     CCF      ACRE     ACRES"};
        //  R006/R007 reports -- flat files
        //  Replace T's with BOARD or CUBIC, Z's with MBF or CCF, X's with sale number ---  change depends on report requested
        public string[] R006R007columns = new string[5] {"NET TTTTT FOOT VOLUME (ZZZ) BY DIAMETER GROUP FOR:  XXXXXX",
                                                         " ",
                                                         "This report is by small end diameter and log grade (0, 1, 2, 3, 5, 6 and 7)",
                                                         " ",
                                                         "Sale#  Species   Diam Group  Peel/Select      #1 Saw       #2 Saw       #3 Saw       #5 Saw     Special Mill    #7"};
        //  Reports R008 and R009 are going to totally change to allow user to enter exact matrix to use
        //  Headers may not change but will stay with the report logic instead of here

    }
}
