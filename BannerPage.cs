using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public class BannerPage
    {
        #region
            reportHeaders rh = new reportHeaders();
            StringBuilder sb = new StringBuilder();
            public string saleName = "";
            public string cruiseName = "";
        #endregion
        
        public void outputBannerPage(string fileName, StreamWriter strWriteOut, string currentDate,
                                        string currentVersion, string DLLversion, CPbusinessLayer bslyr)
        {
            int flagBLM = 0;
            //  Data arrays for the banner page
            string[] bannerTitle = new string[2] {"CRUISE REPORT HEADER & CERTIFICATION",
                                                  "CRUISE REPORT HEADER"};
            string[] bannerYFD = new string[2] {"Year: yyyy    Region: rr   Forest: ff   District: dd",
                                                "Year: yyyy    Agency: BLM   Forest: ff   District: dd"};
            string[] cruiseCertification = new string[17] {"                            **************************** CRUISE CERTIFICATION ****************************",
                                                           "",
                                                           "                                I certify that the timber for the XXXXXXXX timber sale",
                                                           "                                has been designated and cruised by the procedures and standards in",
                                                           "                                FSH 2409.12, Timber Cruising Handbook.  Records of checks are on file",
                                                           "                                at the District Ranger Office,",
                                                           "",
                                                           "",
                                                           "",
                                                           "                                ___________________                 ___________________________________",
                                                           "                                RANGER DISTRICT                     (name of headquarters town)",
                                                           "",
                                                           "",
                                                           "                                ___________________                 ______________",
                                                           "                                DISTRICT RANGER                     DATE",
                                                           "",
                                                           "                            *****************************************************************************"};
            string[] BLMcertification = new string[23] {"                            ***************  B L M  C R U I S E  C E R T I F I C A T I O N  ***************",
                                                        "                            *******************************************************************************",
                                                        "",
                                                        "                                I certify that the timber for the XXXXXXXX timber sale",
                                                        "                                has been cruised according to the procedures and standards set",
                                                        "                                forth in the BLM Timber Cruising Handbook 5310-1.",
                                                        "",
                                                        "",
                                                        "",
                                                        "",
                                                        "                                ___________________________                 ___________________",
                                                        "                                Principal Area Cruiser                             Date",
                                                        "",
                                                        "",
                                                        "                                ___________________________                 ___________________",
                                                        "                                District Cruiser Appraiser                         Date",
                                                        "",
                                                        "",
                                                        "                                ___________________________                 ___________________",
                                                        "                                Field Manager                                      Date",
                                                        "",
                                                        "                            *****************************************************************************",
                                                        "                            *****************************************************************************"};
            string[] bannerFooter = new string[4] {"Developed and Maintained By:",
                                                   "USDA FOREST SERVICE                                                                                             VERSION: ",
                                                   "WASHINGTON OFFICE TIMBER MANAGEMENT                                                              VOLUME LIBRARY VERSION: ",
                                                   "FORT COLLINS, COLORADO (970)295-5776                                                             NATIONAL CRUISE PROCESSING PROGRAM"};

            //  open sale table
            List<SaleDO> saleList = bslyr.getSale();
            //  need to capture region and forest 
            string currRegion = "";
            string currForest = "";
            foreach (SaleDO sl in saleList)
            {
                //  Is this a BLM file?
                if (sl.Region == "7" || sl.Region == "07")
                    flagBLM = 1;

                //  Output header portion
                if (flagBLM == 1)
                    strWriteOut.WriteLine(bannerTitle[1]);
                else strWriteOut.WriteLine(bannerTitle[0]);
                sb.Clear();
                sb.Append("CRUISE#: ");
                sb.Append(sl.SaleNumber);
                sb.Append("      SALE#: ");
                sb.Append(sl.SaleNumber);
                strWriteOut.WriteLine(sb.ToString());
                //  save cruise "name" for later
                cruiseName = sl.SaleNumber;

                sb.Clear();
                sb.Append("SALENAME: ");
                sb.Append(sl.Name);
                strWriteOut.WriteLine(sb.ToString());
                //  save salename for later
                saleName = sl.Name.TrimEnd(' ');

                sb.Clear();
                sb.Append("RUN DATE & TIME: ");
                sb.Append(currentDate);
                strWriteOut.WriteLine(sb.ToString());
                strWriteOut.WriteLine(" ");

                //  Output remaining portion of upper left corner
                sb.Clear();
                // replace fields as needed
                if (flagBLM == 1)
                {
                    bannerYFD[flagBLM] = bannerYFD[flagBLM].Replace("yyyy", sl.CalendarYear.ToString());
                    bannerYFD[flagBLM] = bannerYFD[flagBLM].Replace("ff", sl.Forest.PadLeft(2, ' '));
                    bannerYFD[flagBLM] = bannerYFD[flagBLM].Replace("dd", sl.District.PadLeft(2, ' '));
                    strWriteOut.WriteLine(bannerYFD[flagBLM]);
                }
                else
                {
                    bannerYFD[0] = bannerYFD[0].Replace("yyyy", sl.CalendarYear.ToString());
                    bannerYFD[0] = bannerYFD[0].Replace("rr", sl.Region.PadLeft(2, '0'));
                    bannerYFD[0] = bannerYFD[0].Replace("ff", sl.Forest.PadLeft(2, ' '));
                    if(sl.District == "" || sl.District == " " || sl.District == null)
                        bannerYFD[0] = bannerYFD[0].Replace("dd", "  ");
                    else bannerYFD[0] = bannerYFD[0].Replace("dd", sl.District.PadLeft(2, ' '));
                    strWriteOut.WriteLine(bannerYFD[0]);
                }   //  endif

                sb.Clear();
                sb.Append("Remarks: ");
                sb.Append(sl.Remarks??(" "));
                strWriteOut.WriteLine(sb.ToString());

                currRegion = sl.Region;
                currForest = sl.Forest;
            }   //  end foreach loop

            sb.Clear();
            sb.Append("FILENAME: ");
            sb.Append(fileName);
            strWriteOut.WriteLine(sb.ToString());

            //  Reports list
            List<ReportsDO> rList = bslyr.GetSelectedReports();
            sb.Clear();
            sb.Append("REPORTS: ");
            int numReports = 0;
            foreach (ReportsDO rd in rList)
            {
                numReports++;
                if (numReports <= 20)
                {
                    sb.Append(rd.ReportID);
                    sb.Append(" ");
                }
                else if (numReports == 21)
                {
                    numReports = 0;
                    strWriteOut.WriteLine(sb.ToString());
                    sb.Clear();
                    sb.Append("         ");
                    sb.Append(rd.ReportID);
                    sb.Append(" ");
                    numReports++;
                }   //  endif
            }   //  end foreach loop

            //  output BLM line
            if (flagBLM == 1)
            {
                //  write reports line
                strWriteOut.WriteLine(sb.ToString());
                sb.Clear();
                sb.Append("VOLUME BASED ON ");
                //  need volume equation type for BLM here
                List<VolumeEquationDO> vList = bslyr.getVolumeEquations();
                int nthRow = vList.FindIndex(
                    delegate(VolumeEquationDO ved)
                    {
                        return ved.VolumeEquationNumber.Substring(3, 4) == "B32W";
                    });
                if (nthRow >= 0)
                    sb.Append("32 ");
                else sb.Append("16");
                sb.Append(" FOOT EQUATIONS");
                strWriteOut.WriteLine(sb.ToString());
            }
            else strWriteOut.WriteLine(sb.ToString());
            
            //  Output note for Region 9 as to whether International or Scribner for board foot volume
            if (currRegion == "9" || currRegion == "09")
            {
                switch (currForest)
                {
                    case "03":      case "3":       //  Chippewa
                    case "09":      case "9":       //  Superior
                    case "13":      case "10":      //  Chequamegon-Nicolet and Hiawatha
                    case "07":      case "7":       //  Ottawa
                        sb.Clear();
                        sb.Append("Scribner rule was used for board foot volumes shown in the selected reports.");
                        break;
                    case "04":      case "4":       //  Huron-Manistee
                    case "05":      case "5":       //  Mark Twain
                    case "08":      case "8":       //  Shawnee
                    case "12":      case "14":      //  Hoosier and Wayne
                    case "19":      case "21":      //  Allegheny and Monongahela
                    case "20":      case "22":      //  Green Mountain/Finger Lakes and White Mountain
                        sb.Clear();
                        sb.Append("International 1/4 inch rule was used for board foot volumes in the selected reports.");
                        break;
                }       //  end switch
                strWriteOut.WriteLine(sb.ToString());
            }   //  endif region 9

            //  Several blank lines now
            for (int k = 0; k < 6; k++)
                strWriteOut.WriteLine();

            //  Certification part
            if (flagBLM == 1)
            {
                for (int k = 0; k < 23; k++)
                {
                    if (k == 3)
                        BLMcertification[k] = BLMcertification[k].Replace("XXXXXXXX", saleName);

                    
                    strWriteOut.WriteLine(BLMcertification[k]);
                }   //  end for k loop
            }
            else
            {
                for (int k = 0; k < 17; k++)
                {
                    if (k == 2)
                        cruiseCertification[k] = cruiseCertification[k].Replace("XXXXXXXX", saleName);
                    
                    strWriteOut.WriteLine(cruiseCertification[k]);
                }   //  end for loop for certification
            }   //  endif

            //  last portion
            for (int k = 0; k < 8; k++)
                strWriteOut.WriteLine();

            strWriteOut.WriteLine(bannerFooter[0]);

            sb.Clear();
            sb.Append(bannerFooter[1]);
            sb.Append(currentVersion);
            strWriteOut.WriteLine(sb.ToString());
            
            sb.Clear();
            sb.Append(bannerFooter[2]);
            sb.Append(DLLversion);
            strWriteOut.WriteLine(sb.ToString());

            strWriteOut.WriteLine(bannerFooter[3]);

        }   //  end outputBannerPage


    }
}
