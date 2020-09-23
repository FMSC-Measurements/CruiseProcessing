using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace CruiseProcessing
{
    public class reportHeaders
    {
        //  constant fields
        StringBuilder sb = new StringBuilder();
        public string cruiseName;
        public string saleName;
        public int pageNum;
        public string[] reportTitles = new string[3];

        //  Equation table headers
        public string[] VolEqHeaders = new string[5] {"                                                               PRIMARY                     SECONDARY",
                                                      "                                                       ******* PRODUCT ********      ****** PRODUCT ******",
                                                      "                                            TOTAL       MIN                             MIN",
                                                      "                     VOLUME       STUMP     CUBIC       TOP                             TOP",
                                                      "     SPECIES  PROD   EQUATION     HEIGHT    VOLUME      DIB   BDFT  CUFT  CORDS         DIB   BDFT  CUFT  CORDS   BIOMASS"};
        public string[] ValEqHeaders = new string[2] {"         PRODUCT         VALUE",
                                                      " SPECIES  CODE   GRADE  EQUATION   COEFFICIENT 1   COEFFICIENT 2   COEFFICIENT 3   COEFFICIENT 4   COEFFICIENT 5   COEFFICIENT 6"};
        public string[] BiomassHeaders = new string[2] {"                                                              FIA    WEIGHT FACTOR",
                                                        " SPECIES PROD  COMPONENT           EQU  % MOIST  % REMV  L/D  CODE   PRIM     SECD     METDATA"};
        public string[] QualityEqHeaders = new string[1] {"   SPECIES  EQUATION  YEAR(MMYYYY)   COEFFICIENT 1  COEFFICIENT 2  COEFFICIENT 3  COEFFICIENT 4  COEFFICIENT 5 COEFFICIENT 6"};


        //  A Reports
        public string[] A01unit = new string[3] {"                              CUTTING   CUTTING",
                                                "     CRUISE       CUTTING     UNIT      UNIT                        LOG      PAYMENT",
                                                "     NO.          UNIT NO.    ACRES     DESCRIPTION                 METHOD   UNIT NO."};
        public string[] A01stratum = new string[2] {"     CRUISE   STRATA   CRUISE   STRATA          PLOT  NO. OF STRATA                     DATE",
                                                   "     NO.      NO.      METHOD   ACRES   BAF     SIZE  PLOTS  DESCRIPTION                MMYYYY"};
        public string[] A02plot = new string[2] {"     CRUISE   PLOT     CUTTING  STRATA    SLOPE           PLOT   NULL",
                                                 "     NO.      NO.      UNIT NO. NO.         %     ASPECT  KPI    PLOT?"};
        public string[] A01payment = new string[2] {"     CRUISE     CUTTING             PAYMENT",
                                                    "     NO.        UNIT     STRATUM    UNIT"};
        public string[] A01samplegroup = new string[2] {"          SAMPLE                 BIG   SMALL",
                                                        " STRATUM  GROUP    FREQ   KZ     BAF   FPS      DESCRIPTION"};
        public string[] A03columns = new string[49] {"    STRATA","      UNIT","      PLOT","      TREE","   SPECIES",
                                                    "  PROD PRI","  PROD SEC","   SMP GRP"," CUT/LEAVE","  CNT/MEAS",
                                                    "TREE COUNT"," U OF MEAS","  ESTIMATE","P CULL&BRK",
                                                    " P HID DEF","P SEEN DEF","  P REC PD","S CULL&BRK"," S HID DEF",
                                                    "S SEEN DEF","   SLOPE %","MRKR INITL","  YLD COMP"," LIVE/DEAD",
                                                    "CONTR SPEC","TREE GRADE","  HT 1 LMB","  POLE LEN","CLEAR FACE",
                                                    " CROWN RAT","    DBH-OB","    DRC-OB","    TOT HT"," MRHT TYPE",
                                                    "MRHT LG LN","MRHT PR PD","MRHT SE PD","  REFER HT","FORM CLASS",
                                                    "Z FORM FAC"," U STM DIA"," HT U STEM","   DBH BTR","DBH DBL BT",
                                                    "TDIB PR PD","TDIB SE PD","T DEF CODE","TDIA@DF PT",
                                                    "    VOID %"};
        public string[] A04counts = new string[4] {"THE FOLLOWING INFORMATION IS PROVIDED FOR TREE-BASED METHODS AND REPRESENTS WHAT ONCE WERE COUNT RECORDS",
                                                  " ",
                                                  " CUTTING            SAMPLE                        KZ      TALLY",
                                                  "   UNIT   STRATUM   GROUP   SPECIES   FREQUENCY   VALUE   COUNT   KPI      DESCRIPTION"};
        public string[] A03remarks = new string[2] {"           CUTTING",
                                                   "   STRATUM    UNIT   PLOT   TREE   SPECIES   REMARK"};
        public string[] A08columns = new string[7] {"                     /L      %   %     /L      %   %     /L      %   %     /L      %   %     /L      %   %",
                                                   "  S                  /O      D         /O      D         /O      D         /O      D         /O      D    ",
                                                   "  T                  /G   G  E   R     /G   G  E   R     /G   G  E   R     /G   G  E   R     /G   G  E   R",
                                                   "  R  U    P   T      /    R  F   E     /    R  F   E     /    R  F   E     /    R  F   E     /    R  F   E",
                                                   "  A  N    L   R      /N   A  E   C     /N   A  E   C     /N   A  E   C     /N   A  E   C     /N   A  E   C",
                                                   "  T  I    O   E      /U   D  C   V     /U   D  C   V     /U   D  C   V     /U   D  C   V     /U   D  C   V",
                                                   "  A  T    T   E      /M   E  T   B     /M   E  T   B     /M   E  T   B     /M   E  T   B     /M   E  T   B"};
        public string[] A09columns = new string[7] {"                   L                                                            %    %",
                                                   "  S                O      S     L     L                                              D",
                                                   "  T                G      M     G     E  G                                      R    E",
                                                   "  R  U    P   T                       N  R                                      E    F",
                                                   "  A  N    L   R    N      D     D     G  A   GROSS VOLUME     NET VOLUME        C    E",
                                                   "  T  I    O   E    U      I     I     T  D                                      V    C",
                                                   "  A  T    T   E    M      A     A     H  E   BDFT    CUFT     BDFT    CUFT      B    T"};
        public string[] A05A07columns = new string[10] {"                               K   Z",
                                                        "                               K   Z",
                                                        "                               K   Z",
                                                        "                  S            K   Z",
                                                        "  S               P       D    K   Z",
                                                        "  T               E       B    K   Z",
                                                        "  R  U    P   T   C       H    K   Z   ************ PER TREE ************                           ***** PER ACRE/PER STRATA *****",
                                                        "  A  N    L   R   I       -    K   Z   *** PRIMARY ***    ** SECONDARY **                           ** PRIMARY **   ** SECONDARY **",
                                                        "  T  I    O   E   E       O    K   Z    GROSS    NET      GROSS   NET      TREE    CHAR      EXP     GROSS   NET      GROSS    NET",
                                                        "  A  T    T   E   S       B    K   Z    VVVV     VVVV     VVVV    VVVV      FAC     FAC      FAC     VVVV    VVVV     VVVV     VVVV"};
        public string[] A06columns = new string[10] {"                               K   Z",
                                                     "                               K   Z",
                                                     "                               K   Z",
                                                     "                  S            K   Z",
                                                     "  S               P        D   K   Z",
                                                     "  T               E        B   K   Z  ************ PER TREE ************                         ******* PER ACRE/PER STRATA *******",
                                                     "  R  U  P    T    C        H   K   Z",
                                                     "  A  N  L    R    I        -   K   Z          ***** VALUE *****                                          ***** VALUE *****  ",
                                                     "  T  I  O    E    E        O   K   Z      PRIMARY        SECONDARY       TREE     CHAR     EXP        PRIMARY        SECONDARY ",
                                                     "  A  T  T    E    S        B   K   Z      PRODUCT        PRODUCT         FAC      FAC      FAC        PRODUCT        PRODUCT  "};
        public string[] A10columns = new string[10] {"                                 K   Z",
                                                     "                                 K   Z",
                                                     "                                 K   Z",
                                                     "                    S            K   Z",
                                                     "  S                 P      D     K   Z",
                                                     "  T                 E      B     K   Z",
                                                     "  R  U    P    T    C      H     K   Z   ****************************** PER TREE GREEN POUNDS **********************************",
                                                     "  A  N    L    R    I      -     K   Z       **************** COMPONENT *******************************     ",
                                                     "  T  I    O    E    E      O     K   Z    PRIMARY  SECONDARY          LIVE     DEAD      STEM     TOTAL    TREE     CHAR     EXP",
                                                     "  A  T    T    E    S      B     K   Z    PRODUCT  PRODUCT   FOLIAGE  BRANCHES BRANCHES  TIP      TREE     FAC      FAC      FAC "};
        public string A10footer = "NOTE:  PERCENT MOISTURE AND PERCENT REMOVED HAVE BEEN APPLIED TO BIOMASS COMPONENT WEIGHTS.";
        public string[] A11A12columns = new string[7] {" P  S",
                                                       " R  P",
                                                       " O  E     ***********************************   T R E E    G R A D E   *********************************************************",
                                                       " D  C",
                                                       " U  I          0         1         2         3         4         5         6         7         8         9        NONE       TOTAL",
                                                       " C  E",
                                                       " T  S"};
        public string[] A13plot = new string[6] {"             S",
                                                 "             T",
                                                 "    P   U    R",
                                                 "    L   N    A",
                                                 "    O   I    T",
                                                 "    T   T    A     X-METERS     Y-METERS    Z-UNIT   METADATA"};
        public string[] A13tree = new string[6] {"  S",
                                                 "  T",
                                                 "  R    U     P      T",
                                                 "  A    N     L      R",
                                                 "  T    I     O      E",
                                                 "  A    T     T      E  SG     X-METERS     Y-METERS    Z-UNIT  METADATA"};

        //  new unit summary (A14)
        public string[] A14columns = new string[7] {"            S    S ",
                                                  "       S    M    P",
                                                  "       T    P    E            AVG    EST        NO ",
                                                  "  U    R         C     P ",
                                                  "  N    A    G    I     R      BA/    NO OF      SAMP",
                                                  "  I    T    R    E     O ",
                                                  "  T    A    P    S     D      ACRE   TREES      TREES"};
        //  new modified merch rules report (A15)
        public string[] A15columns = new string[2] {"  VOLUME                    MIN LOG LENGTH    MAX LOG LENGTH   SEGMENTATION    MIN MERCH    EVEN/ODD",
                                                    " EQUATION   PRODUCT   TRIM     PRIMARY           PRIMARY           LOGIC        LENGTH      SEGMENT     MODIFIED?"};
        //  Summary reports right side (VSM1(B1), VSM2(CP1), VSM3(CS1))
        public string[] SummaryColumns = new string[14] {"             K     Z         ",
                                                         "             K     Z         ",
                                                         "             K     Z         ",
                                                         "             K     Z         ",
                                                         "             K     Z         ",
                                                         "             K     Z         ",
                                                         "        M    K     Z     AVG",
                                                         "        E    K     Z     DEFT   GROSS NET",
                                                         "        A    K     Z     %   %               **************** STRATA LEVEL ****************",
                                                         "  QUAD  N    K     Z            BDFT  BDFT   EST.",
                                                         "             K     Z     B   C                        ********** VOLUME ************",
                                                         "  MEAN  D    K     Z     D   U  CUFT  CUFT   # OF",
                                                         "        B    K     Z     F   F                        *** GROSS ***    **** NET ****",
                                                         "  DBH   H    K     Z     T   T  RATIO RATIO  TREES    BDFT     CUFT    BDFT     CUFT  CORDS"};
        //  Summary by strata reports right side (VPA1(B2), VPA2(CP2), VPA3(CS2))
        public string[] StrataSummaryColumns = new string[11] {"  ",
                                                               "  ",
                                                               "  ",
                                                               "  ",
                                                               "  ",
                                                               " ************************************PER ACRE ************************************",
                                                               "EST.",
                                                               "  ",
                                                               "NO. OF",
                                                               "            *******GROSS*******       *******NET********",
                                                               "TREES        BDFT        CUFT           BDFT       CUFT        CORDS"};
        //  Summary value/weight reports right side (VAL1(B3), VAL2(CP3), VAL3(CS3))
        public string[] ValueWeightColumns = new string[11] {"  ",
                                                             "  ",
                                                             "  ",
                                                             "  ",
                                                             "  ",
                                                             "  ",
                                                             "  ",
                                                             "******* VALUE *******      ******* WEIGHT ******      ***** TOTAL CUFT ****",
                                                             "  ",
                                                             "STRATA                     STRATA                     STRATA",
                                                             "LEVEL       PER ACRE       LEVEL       PER ACRE       LEVEL       PER ACRE"};
        //  The following works for VSM1(B1) only
        public string[] LowLevelColumns = new string[14] {"                                           ",
                                                          "                                           ",
                                                          "                                           ",
                                                          "              P                            ",
                                                          "              R                  C         ",
                                                          "              O                  O         ",
                                                          "              D        Y       T N         ",
                                                          "      S    P           L  S    R T         ",
                                                          " S    P    R  S U      D  A    E R         ",
                                                          " T    E    O  O      L    M    E      # OF ",
                                                          " R    C    D  U O  L E C         S         ",
                                                          " A    I    U  R F  I A O  G  S G P    MEAS ",
                                                          " T    E    C  C    V V M  R  T R E         ",
                                                          " A    S    T  E M  E E P  P  M D C    TREES"};
        //  Works for VSM2(CP1) only -- left side
        public string[] VSM2columns = new string[14] {"                             ",
                                                      "                             ",
                                                      "                             ",
                                                      "       P                     ",
                                                      "       R              # OF   ",
                                                      "       O              TREES  ",
                                                      "       D              M      ",
                                                      "    P        S        E     T",
                                                      " S  R  S U   A        A     A",
                                                      " T  O  O     M        S     L",
                                                      " R  D  U O            U     L",
                                                      " A  U  R F   G  S     R     I",
                                                      " T  C  C     R  T     E     E",
                                                      " A  T  E M   P  M     D     D"};
        public string[] VPA1VAL1columns = new string[11] {"              P                            ",
                                                          "              R                  C         ",
                                                          "              O                  O         ",
                                                          "              D        Y       T N         ",
                                                          "      S    P           L  S    R T         ",
                                                          " S    P    R  S U      D  A    E R         ",
                                                          " T    E    O  O      L    M    E           ",
                                                          " R    C    D  U O  L E C         S         ",
                                                          " A    I    U  R F  I A O  G  S G P         ",
                                                          " T    E    C  C    V V M  R  T R E         ",
                                                          " A    S    T  E M  E E P  P  M D C         "};
        //  Works for VPA2(CP2)/VAL2(CP3)--left side
        public string[] VPA2VAL2columns = new string[11] {"            P                             ",
                                                          "            R                             ",
                                                          "            O                             ",
                                                          "            D                             ",
                                                          "       P             S                    ",
                                                          "   S   R    S   U    M                    ",
                                                          "   T   O    O        P                    ",
                                                          "   R   D    U   O                         ",
                                                          "   A   U    R   F    G    S               ",
                                                          "   T   C    C        R    T               ",
                                                          "   A   T    E   M    P    M               "};
        //  Works for VSM4(CP4) only
        public string[] VSM4columns = new string[2] {"          SAMPLE   CUTTING                              GROSS    NET      EXPANSION   MEAS   TOTAL   TREE              MARKER'S",
                                                     " STRATUM  GROUP    UNIT    PLOT   TREE  SPECIES   DBH   VOLUME   VOLUME   FACTOR      KPI    KPI**   COUNT    RATIO    INITIALS"};


        //  Works for VSM5 only
        public string[] VSM5columns = new string[2] {"             CUTTING                       NUMBER     GROSS    NET      SECONDARY            BDFT/CUFT",
                                                     "             UNIT      SPECIES   PRODUCT   OF TREES   CUFT     CUFT     NET CUFT     QMD     RATIO"};
        public string[] VSM6leftSide = new string[14] {"                                         ",
                                                          "                                      ",
                                                          "                                      ",
                                                          "              P                       ",
                                                          "              R                  C    ",
                                                          "              O                  O    ",
                                                          "              D        Y       T N    ",
                                                          "      S    P           L  S    R T    ",
                                                          " S    P    R  S U      D  A    E R    ",
                                                          " T    E    O  O      L    M    E      ",
                                                          " R    C    D  U O  L E C         S    ",
                                                          " A    I    U  R F  I A O  G  S G P    ",
                                                          " T    E    C  C    V V M  R  T R E    ",
                                                          " A    S    T  E M  E E P  P  M D C    "};        
        //  Works for VSM6 only
        public string[] VSM6columns = new string[14] {" ",
                                                      " ",
                                                      " ",
                                                      " ",                                               
                                                      " ",
                                                      " ",
                                                      " ",
                                                      " GROSS   NET                                                 ",                                                       
                                                      "               **************** STRATA LEVEL ****************",
                                                      " CUFT    CUFT                                                ",
                                                      "               ********** VOLUME ************                ",
                                                      " TON     TON                                                 ",
                                                      "                      GROSS          NET                     ",
                                                      " RAIO    RATIO        CUFT           CUFT       TONS         "};

        //  Works for VSM3(CS1) only
        public string[] VSM3columns = new string[14] {"                           ",
                                                      "                           ",
                                                      "                           ",
                                                      "       P                   ",
                                                      "       R         # OF      ",
                                                      "       O         TREES     ",
                                                      "       D         M         ",
                                                      "    P            E    T    ",
                                                      " S  R  S  U      A    A    ",
                                                      " T  O  O         S    L    ",
                                                      " R  D  U  O      U    L    ",
                                                      " A  U  R  F      R    I    ",
                                                      " T  C  C         E    E    ",
                                                      " A  T  E  M      D    D    "};
        //  Works for VPA3(CS2)/VAL3(CS3)
        public string[] VPA3VAL3columns = new string[11] {"              P             ",
                                                          "              R             ",
                                                          "              O             ",
                                                          "              D             ",
                                                          "         P                  ",
                                                          "   S     R    S    U        ",
                                                          "   T     O    O             ",
                                                          "   R     D    U    O        ",
                                                          "   A     U    R    F        ",
                                                          "   T     C    C             ",
                                                          "   A     T    E    M        "};
        //  LD1(CS4) through LD8(CS11) --  broken into right and left sides
        public string[] LiveDeadRight = new string[8] {" ",
                                                       " ",
                                                       " ",
                                                       " ",
                                                       "********   LIVE   ********    ********   DEAD   ********    *******   OTHER   ********     *******   TOTAL   ********",
                                                       "EST.                          EST.                          EST.                           EST.",
                                                       "NO OF     GROSS     NET       NO OF     GROSS     NET       NO OF     GROSS     NET        NO OF     GROSS     NET",
                                                       "TREES     XXXX      XXXX      TREES     XXXX      XXXX      TREES     XXXX      XXXX       TREES     XXXX      XXXX"};
        // LD1(CS4) and LD2(CS5) left side
        public string[] LD1LD2left = new string[8] {" ",
                                                    " P   S      ",
                                                    " R   P      ",
                                                    " O   E      ",
                                                    " D   C      ",
                                                    " U   I      ",
                                                    " C   E      ",
                                                    " T   S      "};
        public string[] LD3LD4left = new string[8] {"C             ",
                                                    "U  P   S      ",
                                                    "T  R   P      ",
                                                    "   O   E      ",
                                                    "U  D   C      ",
                                                    "N  U   I      ",
                                                    "I  C   E      ",
                                                    "T  T   S      "};
        public string[] LD5LD6left = new string[8] {" P             ",
                                                    " A  P   S      ",
                                                    " Y  R   P      ",
                                                    "    O   E      ",
                                                    " U  D   C      ",
                                                    " N  U   I      ",
                                                    " I  C   E      ",
                                                    " T  T   S      "};
        public string[] LD7LD8left = new string[8] {" L             ",
                                                    " O  P   S      ",
                                                    " G  R   P      ",
                                                    "    O   E      ",
                                                    " M  D   C      ",
                                                    " E  U   I      ",
                                                    " T  C   E      ",
                                                    " H  T   S      "};
        public string[] LiveDeadFooter = new string[3] {"NOTE:  Volumes include primary, secondary and recovered for each product number.",
                                                        "       Net cubic or board includes recovered volume if present.",
                                                        "       Number of trees includes only primary values even though no volume may appear."};
        //  ST(DP) reports
        public string[] ST1ST2columns = new string[9] {"                  S",
                                                   "                  T",
                                                   "    P      S      G   *************************************ZZZZZZZZZZ PRODUCT XXXXX VOLUME *****************************************",
                                                   " S  R  U   A",
                                                   " T  O      M      S   NO                                     SUM          SUM                     COEFF                     COMBINED",
                                                   " R  D  O          A",
                                                   " A  U  F   G   S  M   SAM   BIG  SMALL   t      MEAN         OF           OF          STANDARD     OF     STANDARD SAMPLING SAMPLING",
                                                   " T  C      R   T",
                                                   " A  T  M   P   M 1/2 TREES   N     N*  VALUE     X            X         (X * X)       DEVIATION VARIATION  ERROR    ERROR    ERROR"};
        //  ST(DS) reports
        public string[] ST3columns = new string[7] {"     P         ******** ZZZZZZZZZZZZZZZZZ GROSS VOLUME **********       ******* ZZZZZZZZZZZZZZZZZ NET VOLUME ********",
                                                    "  S  R   U",
                                                    "  T  O                            ****** 95% CONFIDENCE *****                        ****** 95% CONFIDENCE ******",
                                                    "  R  D   O",
                                                    "  A  U   F                                    INTERVAL                                           INTERVAL",
                                                    "  T  C",
                                                    "  A  T   M      VOLUME*     ERROR       FROM            TO                VOLUME*   ERROR      FROM            TO"};
        public string[] ST4columns = new string[7] {"                                      P",
                                                   "                                  S   R    U",
                                                   "                                  T   O",
                                                   "                                  R   D    O                            **** 95% CONFIDENCE ****",
                                                   "                                  A   U    F                                    INTERVAL",
                                                   "                                  T   C",
                                                   "                                  A   T    M       VALUE       ERROR           FROM              TO"};
        public string[] STfooters = new string[3] {"WARNING:  PRODUCTS WITH MORE THAN ONE UNIT OF MEASURE ARE NOT CALCULATED.",
                                                   "* FOR SECONDARY PRODUCT, IF PRIMARY PRODUCT UNIT OF MEASURE IS BOARD FOOT, THEN CUBIC FOOT VOLUME IS USED FOR SECONDARY PRODUCT.",
                                                   "* UOM DETERMINES VALUES USED IN CALCULATION UNDER VOLUME"};
        public string[] STsubtotals = new string[3] {"                                       ------------  AGGREGATED BY PRODUCT  ------------",
                                                     "                                       --------  AGGREGATED BY UNIT OF MEASURE  --------",
                                                     "                                       ------------  AGGREGATED BY STRATA   ------------"};
        //  Export grade reports
        public string[] EX1EX2left = new string[2] {"                           LOG            LOG          LOG",
                                                    " STRATA  UNIT  PLOT  TREE  NUM  SPECIES   SORT  GRADE  LENGTH  % DEFECT"};
        public string[] EX2right = new string[2] {"                                         LOG     LOG     BDFT",
                                                  "                       SORT  GRADE       LENGTH  DIAM    VOL   DEFECT"};
        public string[] EX3columns = new string[2] {"                                                       MINIMUM    MINIMUM  MINIMUM   MAXIMUM",
                                                    "          SORT    GRADE   CODE        NAME             DIAMETER   LENGTH    BD FT    DEFECT"};
        public string[] EX4columns = new string[3] {"    LOG DIB          ****************************************  SORT & GRADE COMBINATIONS  ****************************************",
                                                    "    SMALL END",
                                                    "  1\" DIA CLASS            "};     //  sort and grade combinations are added to this line from the data
        public string[] EX6EX7right = new string[3] {"                                     EST.    ***  BDFT  ***    ***  CUFT  ***",
                                                     "                        LOG          # of        VOLUME            VOLUME                   AVE",
                                                     "  SPECIES  SORT  GRADE  LOGS    GROSS     NET      GROSS   NET     % DEFECT   LENGTH"};
        //  the last line will have "STRATA" or "STRATA  UNIT" added at the beginning depending on the report
        public string EXfooter = "    ** LOG SORT AND/OR LOG GRADE MISSING OR INCORRECT";
        //  Log level reports
        //  L1 report
        public string[] L1columns = new string[9] {"                             U",
                                                   "                                                                %",
                                                   "                     S    P  O   L     S      L                      %",
                                                   "   S                 P    R  F   O     M      G      L          D",
                                                   "   T                 E    O      G                   E      G   E    R",
                                                   "   R   U   P    T    C    D  M         D      D      N      R   F    E",
                                                   "   A   N   L    R    I    U  E   N     I      I      G      A   E    C",
                                                   "   T   I   O    E    E    C  A   U     A      A      T      D   C    V    GROSS  BDFT   NET     GROSS  CUBIC  NET    DIB  TOTAL",
                                                   "   A   T   T    E    S    T  S   M     M      M      H      E   T    B    BDFT   REMVD  BDFT    CUBIC  REMVD  CUBIC  CLS  EXPANS"};
        //  L2 report
        public string[] L2columns = new string[3] {" LOG    ********************************* NET VOLUMES **********************************    TOTAL                        TOTAL NET",
                                                   " DIB                                                                                        NET      CULL LOG   DEFECT   + CULL LOG",
                                                   " CLASS   GRADE 0   GRADE 1   GRADE 2   GRADE 3   GRADE 4   GRADE 5   GRADE 6   GRADE 7      VOLUME   GRD=8,9    GRD=1-7  + DEFECT"};
        //  L8 report
        public string[] L8columns = new string[3] {"    LOG     ****************** PRIMARY PRODUCT *****************     **** SECONDARY PRODUCT *****     ********** TOTAL ***********",
                                                   "    DIB     # OF       GROSS         NET       GROSS         NET     # OF       GROSS         NET     # OF       GROSS         NET",
                                                   "  CLASS     LOGS        BDFT        BDFT        CUFT        CUFT     LOGS        CUFT        CUFT     LOGS        CUFT        CUFT"};
        //  L10 report
        public string[] L10columns = {" LENGTH|    8'  |   10'  |   12'  |   14'  |   16'  |   18'  |   20'  |   22'  |   24'  |   26'  |   28'  |   30'  |   32'  | TOTALS"};
        //  Stem count reports
        //  Column headings are by species and are created on the fly
        //  Stand tables -- column headings are also by a combination of values and are created on the fly
        
        //  UC reports (1 through 6 and 25/26) -- rest are stand table format
        public string[] UC1columns = new string[9] {"                               ",
                                                    "                               ",
                                                    "         S                     ",
                                                    "  S      P         U           ",
                                                    "  T      E                     ",
                                                    "  R   U  C      P  O           ",
                                                    "  A   N  I      R  F           ",
                                                    "  T   I  E      O              ",
                                                    "  A   T  S      D  M           "};
        public string[] UC2left = new string[9] {"                               ",
                                                 "                               ",
                                                 "         S                     ",
                                                 "  S      M         U           ",
                                                 "  T      P                     ",
                                                 "  R   U         P  O           ",
                                                 "  A   N  G      R  F           ",
                                                 "  T   I  R      O              ",
                                                 "  A   T  P      D  M           "};
        public string[] UC1UC2right = new string[9] {"***************************************** PRIMARY PRODUCT ******************************************",
                                                     "  AVGDEF",
                                                     "                 GROSS      NET",
                                                     " %       %                             ************************ STRATA LEVEL ***********************",
                                                     "                 BDFT       BDFT       EST.",
                                                     " B       C",
                                                     " D       U       CUFT       CUFT       NO OF      ** GROSS VOLUME **     ** NET VOLUME **",
                                                     " F       F",
                                                     " T       T       RATIO      RATIO      TREES       BDFT        CUFT       BDFT      CUFT      CORDS"};
        public string[] UC3left = new string[7] {"          S         ",
                                                 "  S       P         ",
                                                 "  T       E         ",
                                                 "  R   U   C         ",
                                                 "  A   N   I         ",
                                                 "  T   I   E         ",
                                                 "  A   T   S         "};
        public string[] UC4left = new string[7] {"          S         ",
                                                 "  S       M         ",
                                                 "  T       P         ",
                                                 "  R   U             ",
                                                 "  A   N   G         ",
                                                 "  T   I   R         ",
                                                 "  A   T   P         "};
        public string[] UC5left = new string[7] {"      S             ",
                                                 "      P             ",
                                                 "      E             ",
                                                 "  U   C             ",
                                                 "  N   I             ",
                                                 "  I   E             ",
                                                 "  T   S             "};
        public string[] UC6left = new string[7] {"      S             ",
                                                 "      M             ",
                                                 "      P             ",
                                                 "  U                 ",
                                                 "  N   G             ",
                                                 "  I   R             ",
                                                 "  T   P             "};
        public string[] UC3TO6right = new string[7] {"TOTAL    ******************* SAWTIMBER *******************     ***************** NON-SAWTIMBER ****************",
                                                     "                     (PROD = 01   UM = 01, 03)                         (PROD NOT = 01   UM = 01, 02, 03)",
                                                     "EST.     EST.                                                   (AND ALL SECONDARY & RECOVERED PRODUCT VOLUMES)",
                                                     "",
                                                     "# OF     # OF    ** GROSS VOLUME **      ** NET VOLUME **      ***   GROSS    ***   ***   NET    ***",
                                                     "",
                                                     "TREES    TREES      BDFT       CUFT       BDFT       CUFT        BDFT      CUFT       BDFT      CUFT      CORDS"};
        public string[] UC25columns = new string[2] {"                                PAYMENT            CONTRACT    SAWTIMBER     NON-SAWTIMBER",
                                                     "                                 UNIT     ACRES    SPECIES        CCF              CCF"};
        public string[] UC26columns = new string[2] {"                                CUTTING            CONTRACT    SAWTIMBER     NON-SAWTIMBER",
                                                     "                                 UNIT     ACRES    SPECIES        CCF              CCF"};
        public string[] UCfooter = new string[5] {"* FOR TREE-BASED SAMPLES, THE CUTTING UNIT VOLUME IS BASED ON THE # OF TREES TALLIED OR THE SUM OF KPIS THAT FALL WITHIN",
                                                  "THE CUTTING UNIT.  FOR AREA-BASED SAMPLES, THE CUTTING UNIT VOLUME IS BASED ON AN AVERAGE VOLUME PER ACRE AT THE STRATA",
                                                  "LEVEL TIMES THE NUMBER OF ACRES IN THE CUTTING UNIT.",
                                                  "*R = RECOVERED VOLUME ADDED TO NET SECONDARY VOLUME.",
                                                  "*FOR CRUISE METHOD 3P ONLY, THE COLUMN Est. # of Trees for sawtimber WILL SHOW ZERO INSTEAD OF ACTUAL TALLY."};
        //  Weight reports
        //  WT1 report
        public string[] WT1columns = new string[3] {"                                                         (1)             (2)       (3)            (4)            (5)",
                                                    "           CRUISE    CONTRACT                   GROSS     WEIGHT         POUNDS   PERCENT         POUNDS          TONS",
                                                    "          SPECIES     SPECIES    PRODUCT         CUFT     FACTOR       STANDING   REMOVED        REMOVED       REMOVED"};
        public string[] WT1footer = new string[5] {"         (1)  WEIGHT FACTOR = POUNDS PER GROSS CUFT",
                                                   "         (2)  POUNDS STANDING = GROSS CUFT x WEIGHT FACTOR",
                                                   "         (3)  PERCENT REMOVED = % MATERIAL HAULED OUT ON TRUCKS",
                                                   "         (4)  POUNDS REMOVED =  POUNDS STANDING x (PERCENT REMOVED/100)",
                                                   "         (5)  TONS REMOVED = POUNDS REMOVED / 2,000"};
        public string[] WT1total = new string[3] {"                                 SUMMARY",
                                                  "                 CONTRACT                         TONS",
                                                  "                  SPECIES        PRODUCT       REMOVED"};
        //  WT2 and WT3 reports
        //  WT2columns includes line for WT3 header -- rest is the same for WT3
        public string[] WT2columns = new string[3] {"STRATUM:  XXXX",
                                                    "SLASH LOAD            SPECIES",
                                                    "CUTTING UNIT:  XXXX"};
        public string[] WT2crown = new string[5] {"   NEEDLES |", "  0 - 1/4\" |", "  1/4 - 1\" |", "    1 - 3\" |", "       3\"+ |"};
        public string[] WT2threeplus = new string[4] {"    TOPWOOD |", "CULL VOLUME |", "     CHUNKS |", "       FLIW |"};
        //  WT4 report
        public string[] WT4columns = new string[3] {"                                           SAWTIMBER         NON-SAWTIMBER        NON-SAWTIMBER",
                                                    "  CUTTING                                  PRIM PROD = 01    OTHER PRIM PROD      SECOND PROD ONLY",
                                                    "   UNIT         ACRES       SPECIES        GREEN TONS        GREEN TONS           GREEN TONS"};
        //  WT5 report
        public string[] WT5columns = new string[4] {"    STRATUM:  XX                      GREEN TONS",
                                                    "__________________________________________________________________________________________________",
                                                    "                   PRIMARY    SECONDARY   --------BIOMASS COMPONENTS------- |------STEM WGT-------",
                                                    " SPECIES       |   PRODUCT    PRODUCT        TIP     BRANCHES     FOLIAGE   |    MERCH*    TOTAL**"};
        public string[] WT5footer = new string[2] {" * WHOLE TREE (ABOVE GROUND) BIOMASS AS CALCULATED USING THE TOT TREE EQN SPECIFIED IN DEFAULTS (REGIONAL).",
                                                   " ** TOTAL IS THE ADDITION OF PRIMARY PRODUCT, SECONDARY PRODUCT, TIP, BRANCHES AND FOLIAGE."};

        //  Functions
        public void outputReportHeader(StreamWriter strWriteOut, ArrayList mainHeaderData, string titleOne,
                                        string titleTwo, string titleThree)
        {
            //  write report title and page number
            sb.Clear();
            sb.Append(titleOne.PadRight(124,' '));
            sb.Append("PAGE ");
            sb.Append(pageNum);
            strWriteOut.WriteLine(sb.ToString());

            //  if subtitles present, output those
            if ((reportTitles[1] != "" && reportTitles[1] != null) || titleTwo != "")
                strWriteOut.WriteLine(titleTwo);
            if ((reportTitles[2] != "" && reportTitles[2] != null) || titleThree != "")
                strWriteOut.WriteLine(titleThree);

            //  write remaining info
            sb.Clear();
            sb.Append("CRUISE#: ");
            sb.Append(mainHeaderData[3].ToString().PadRight(5,' '));
            sb.Append("      SALE#: ");
            sb.Append(mainHeaderData[3].ToString().PadRight(5, ' '));
            strWriteOut.WriteLine(sb.ToString());

            sb.Clear();
            sb.Append("SALENAME: ");
            sb.Append(mainHeaderData[4].ToString().PadRight(103, ' '));
            sb.Append("VERSION: ");
            sb.Append(mainHeaderData[1]);
            strWriteOut.WriteLine(sb.ToString());

            sb.Clear();
            sb.Append("RUN DATE & TIME: ");
            sb.Append(mainHeaderData[0]);
            sb.Append("                                                             VOLUME LIBRARY VERSION: ");
            sb.Append(mainHeaderData[2]);
            strWriteOut.WriteLine(sb.ToString());

            strWriteOut.WriteLine();
            return;
        }   //  end outputReportHeader


        public void createReportTitle(string currentTitle, int doWhat, int whereSplit1, int whereSplit2,
                                       string whichReportConstant, string secondReportConstant)
        {
            reportTitles[0] = "";
            reportTitles[1] = "";
            reportTitles[2] = "";

            //  what's done depends on doWhat
            switch(doWhat)
            {
                case 1:     //  just upper case the title and put in report title array
                    reportTitles[0] = currentTitle.ToUpper();
                    break;
                case 2:     //  upper case and split once
                    reportTitles[0] = currentTitle.Substring(0, whereSplit1).ToUpper();
                    reportTitles[1] = currentTitle.Substring(whereSplit1, currentTitle.Length-whereSplit1).ToUpper();
                    break;
                case 3:     //  upper case and split twice
                    reportTitles[0] = currentTitle.Substring(0, whereSplit1).ToUpper();
                    reportTitles[1] = currentTitle.Substring(whereSplit1, whereSplit2-whereSplit1).ToUpper();
                    reportTitles[2] = currentTitle.Substring(whereSplit2, currentTitle.Length-whereSplit2).ToUpper();
                    break;
                case 4:     //  two line split with report constant
                    reportTitles[0] = currentTitle.Substring(0, whereSplit1).ToUpper();
                    reportTitles[1] = currentTitle.Substring(whereSplit1, currentTitle.Length - whereSplit1).ToUpper();
                    reportTitles[2] = whichReportConstant;
                    break;
                case 5:     //  one line with report constant
                    reportTitles[0] = currentTitle.ToUpper();
                    reportTitles[1] = whichReportConstant;
                    break;
                case 6:     //  one line with two report constants
                    reportTitles[0] = currentTitle.ToUpper();
                    reportTitles[1] = whichReportConstant;
                    reportTitles[2] = secondReportConstant;
                    break;
                case 7:     //  upper case and split one with report constant
                    reportTitles[0] = currentTitle.Substring(0, whereSplit1).ToUpper();
                    reportTitles[1] = currentTitle.Substring(whereSplit1, currentTitle.Length - whereSplit1).ToUpper();
                    reportTitles[2] = whichReportConstant;
                    break;

            }   //  end switch on doWhat

            return;
        }   //  end createReportTitle


        public void outputColumnHeadings(StreamWriter strWriteOut, string[] columnHeaders)
        {
            //  output column headers and dividing line
            for (int m = 0; m < columnHeaders.Count(); m++)
            {
                strWriteOut.WriteLine(columnHeaders[m]);
            }   //  end for m loop

            strWriteOut.WriteLine(reportConstants.longLine);

            return;
        }   //  end outputColumnHeadings
    }
}
