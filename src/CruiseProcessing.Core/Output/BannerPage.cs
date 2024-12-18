using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CruiseDAL.DataObjects;
using CruiseProcessing.Output;
using CruiseProcessing.Data;
using CruiseProcessing.OutputModels;


namespace CruiseProcessing
{
    public class BannerPage
    {
        #region
            StringBuilder sb = new StringBuilder();
            public string saleName = "";
            public string cruiseName = "";
        #endregion

        static readonly string[]  bannerTitle = new string[2] {"CRUISE REPORT HEADER & CERTIFICATION",
                                                  "CRUISE REPORT HEADER"};
        static readonly string[] bannerYFD = new string[2] {"Year: yyyy    Region: rr   Forest: ff   District: dd",
                                                "Year: yyyy    Agency: BLM   Forest: ff   District: dd"};
        static readonly string[] cruiseCertification = new string[17] {"                            **************************** CRUISE CERTIFICATION ****************************",
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
        static readonly string[] BLMcertification = new string[23] {"                            ***************  B L M  C R U I S E  C E R T I F I C A T I O N  ***************",
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
        static readonly string[] bannerFooter = new string[4] {"Developed and Maintained By:",
                                                   "USDA FOREST SERVICE                                                                                             VERSION: ",
                                                   "WASHINGTON OFFICE TIMBER MANAGEMENT                                                              VOLUME LIBRARY VERSION: ",
                                                   "FORT COLLINS, COLORADO (970)295-5776                                                             NATIONAL CRUISE PROCESSING PROGRAM"};

        public void outputBannerPage(string fileName, TextWriter strWriteOut, string currentDate,
                                        string currentVersion, string DLLversion, CpDataLayer bslyr)
        {
            //  Data arrays for the banner page
            var sale = bslyr.GetSale();
            List<ReportsDO> rList = bslyr.GetSelectedReports();

            //  Is this a BLM file?
            bool isBLM = sale.Region == "7" || sale.Region == "07";

            //  Output header portion
            if (isBLM == true)
                strWriteOut.WriteLine(bannerTitle[1]);
            else strWriteOut.WriteLine(bannerTitle[0]);
            //sb.Clear();
            //sb.Append("CRUISE#: ");
            //sb.Append(sale.SaleNumber);
            //sb.Append("      SALE#: ");
            //sb.Append(sale.SaleNumber);
            //strWriteOut.WriteLine(sb.ToString());
            strWriteOut.WriteLine($"CRUISE#: {sale.SaleNumber}      SALE#: {sale.SaleNumber}");
            

            //sb.Clear();
            //sb.Append("SALENAME: ");
            //sb.Append(sale.Name);
            //strWriteOut.WriteLine(sb.ToString());
            strWriteOut.WriteLine("SALENAME: " + sale.Name);
            

            //sb.Clear();
            //sb.Append("RUN DATE & TIME: ");
            //sb.Append(currentDate);
            //strWriteOut.WriteLine(sb.ToString());
            strWriteOut.WriteLine("RUN DATE & TIME: " + currentDate);
            strWriteOut.WriteLine(" ");

            //  Output remaining portion of upper left corner
            // replace fields as needed
            if (isBLM == true)
            {
                var yfd = bannerYFD[1].Replace("yyyy", sale.CalendarYear.ToString())
                    .Replace("ff", sale.Forest.PadLeft(2, ' '))
                    .Replace("dd", sale.District.PadLeft(2, ' '));

                strWriteOut.WriteLine(yfd);
            }
            else
            {
                var yfd = bannerYFD[0].Replace("yyyy", sale.CalendarYear.ToString())
                    .Replace("rr", sale.Region.PadLeft(2, '0'))
                    .Replace("ff", sale.Forest.PadLeft(2, ' '));

                var district = sale.District;
                if (district == "" || district == " " || district == null)
                { yfd = yfd.Replace("dd", "  "); }
                else
                { yfd = yfd.Replace("dd", district.PadLeft(2, ' ')); }

                strWriteOut.WriteLine(yfd);
            }   //  endif

            //sb.Clear();
            //sb.Append("Remarks: ");
            //sb.Append(sale.Remarks ?? (" "));
            //strWriteOut.WriteLine(sb.ToString());
            strWriteOut.WriteLine("Remarks: " + (sale.Remarks ?? " "));

            //sb.Clear();
            //sb.Append("FILENAME: ");
            //sb.Append(fileName);
            //strWriteOut.WriteLine(sb.ToString());
            strWriteOut.WriteLine("FILENAME: " + fileName);

            //  Reports list
            var reportsSb = new StringBuilder();
            reportsSb.Append("REPORTS: ");
            int numReports = 0;
            foreach (ReportsDO rd in rList)
            {
                numReports++;
                if (numReports <= 20)
                {
                    reportsSb.Append(rd.ReportID);
                    reportsSb.Append(" ");
                }
                else if (numReports == 21)
                {
                    numReports = 0;
                    reportsSb.AppendLine();
                    reportsSb.Append("         ");
                    reportsSb.Append(rd.ReportID);
                    reportsSb.Append(" ");
                    numReports++;
                }   //  endif
            }   //  end foreach loop

            strWriteOut.WriteLine(sb.ToString());

            //  output BLM line
            if (isBLM == true)
            {
                List<VolumeEquationDO> vList = bslyr.getVolumeEquations();
                //  write reports line

                sb.Clear();
                sb.Append("VOLUME BASED ON ");
                //  need volume equation type for BLM here
                
                if (vList.Any(x => x.VolumeEquationNumber.Substring(3, 4) == "B32W"))
                    sb.Append("32 ");
                else sb.Append("16");
                sb.Append(" FOOT EQUATIONS");
                strWriteOut.WriteLine(sb.ToString());
            }

            string currRegion = sale.Region;
            string currForest = sale.Forest;

            //  Output note for Region 9 as to whether International or Scribner for board foot volume
            if (currRegion == "9" || currRegion == "09")
            {
                switch (currForest)
                {
                    case "03":      case "3":       //  Chippewa
                    case "09":      case "9":       //  Superior
                    case "13":      case "10":      //  Chequamegon-Nicolet and Hiawatha
                    case "07":      case "7":       //  Ottawa
                        strWriteOut.WriteLine("Scribner rule was used for board foot volumes shown in the selected reports.");
                        break;
                    case "04":      case "4":       //  Huron-Manistee
                    case "05":      case "5":       //  Mark Twain
                    case "08":      case "8":       //  Shawnee
                    case "12":      case "14":      //  Hoosier and Wayne
                    case "19":      case "21":      //  Allegheny and Monongahela
                    case "20":      case "22":      //  Green Mountain/Finger Lakes and White Mountain
                        strWriteOut.WriteLine("International 1/4 inch rule was used for board foot volumes in the selected reports.");
                        break;
                }       //  end switch
            }   //  endif region 9

            //  Several blank lines now
            for (int k = 0; k < 6; k++)
                strWriteOut.WriteLine();

            //  Certification part
            if (isBLM == true)
            {
                for (int k = 0; k < 23; k++)
                {
                    if (k == 3)
                    { strWriteOut.WriteLine(BLMcertification[k].Replace("XXXXXXXX", saleName)); }
                    else
                    { strWriteOut.WriteLine(BLMcertification[k]); }
                }   //  end for k loop
            }
            else
            {
                for (int k = 0; k < 17; k++)
                {
                    if (k == 2)
                    { strWriteOut.WriteLine(cruiseCertification[k].Replace("XXXXXXXX", saleName)); }
                    else
                    { strWriteOut.WriteLine(cruiseCertification[k]); }
                }   //  end for loop for certification
            }   //  endif

            //  last portion
            for (int k = 0; k < 8; k++)
                strWriteOut.WriteLine();

            strWriteOut.WriteLine(bannerFooter[0]);
            strWriteOut.WriteLine(bannerFooter[1] + currentVersion);
            strWriteOut.WriteLine(bannerFooter[2] + DLLversion);
            strWriteOut.WriteLine(bannerFooter[3]);

            //  save cruise "name" for later
            cruiseName = sale.SaleNumber;

            //  save salename for later
            saleName = sale.Name.TrimEnd(' ');

        }   //  end outputBannerPage

        public static string GenerateBannerPage(string fileName, HeaderFieldData headerData, SaleDO sale, IEnumerable<ReportsDO> reports, CpDataLayer bslyr)
        {
            var strWriter = new StringWriter();

            var currentDate = headerData.Date;
            var currentVersion = headerData.Version;
            var dllVersion = headerData.DllVersion;

            //  Is this a BLM file?
            bool isBLM = sale.Region == "7" || sale.Region == "07";

            //  Output header portion
            if (isBLM == true)
                strWriter.WriteLine(bannerTitle[1]);
            else strWriter.WriteLine(bannerTitle[0]);

            strWriter.WriteLine($"CRUISE#: {sale.SaleNumber}      SALE#: {sale.SaleNumber}");
            strWriter.WriteLine("SALENAME: " + sale.Name);
            strWriter.WriteLine("RUN DATE & TIME: " + currentDate);
            strWriter.WriteLine(" ");

            //  Output remaining portion of upper left corner
            // replace fields as needed
            if (isBLM == true)
            {
                var yfd = bannerYFD[1].Replace("yyyy", sale.CalendarYear.ToString())
                    .Replace("ff", sale.Forest.PadLeft(2, ' '))
                    .Replace("dd", sale.District.PadLeft(2, ' '));

                strWriter.WriteLine(yfd);
            }
            else
            {
                var yfd = bannerYFD[0].Replace("yyyy", sale.CalendarYear.ToString())
                    .Replace("rr", sale.Region.PadLeft(2, '0'))
                    .Replace("ff", sale.Forest.PadLeft(2, ' '));

                var district = sale.District;
                if (district == "" || district == " " || district == null)
                { yfd = yfd.Replace("dd", "  "); }
                else
                { yfd = yfd.Replace("dd", district.PadLeft(2, ' ')); }

                strWriter.WriteLine(yfd);
            }

            strWriter.WriteLine("Remarks: " + (sale.Remarks ?? " "));
            strWriter.WriteLine("FILENAME: " + fileName);

            //  Reports list
            strWriter.Write("REPORTS: ");
            int numReports = 0;
            foreach (ReportsDO rd in reports)
            {
                numReports++;
                if (numReports <= 20)
                {
                    strWriter.Write(rd.ReportID + " ");
                }
                else if (numReports == 21)
                {

                    strWriter.WriteLine();
                    strWriter.Write("         " + rd.ReportID + " ");
                    numReports = 1;
                }
            }

            strWriter.WriteLine();

            //  output BLM line
            if (isBLM == true)
            {
                List<VolumeEquationDO> volumeEquations = bslyr.getVolumeEquations();
                //  write reports line

                var logLength = volumeEquations.Any(x => x.VolumeEquationNumber.Substring(3, 4) == "B32W") 
                    ? "32" : "16";

                //  need volume equation type for BLM here
                strWriter.WriteLine($"VOLUME BASED ON {logLength} FOOT EQUATIONS");
                
            }

            string currRegion = sale.Region;
            string currForest = sale.Forest;

            //  Output note for Region 9 as to whether International or Scribner for board foot volume
            if (currRegion == "9" || currRegion == "09")
            {
                switch (currForest)
                {
                    case "03":
                    case "3":       //  Chippewa
                    case "09":
                    case "9":       //  Superior
                    case "13":
                    case "10":      //  Chequamegon-Nicolet and Hiawatha
                    case "07":
                    case "7":       //  Ottawa
                        strWriter.WriteLine("Scribner rule was used for board foot volumes shown in the selected reports.");
                        break;
                    case "04":
                    case "4":       //  Huron-Manistee
                    case "05":
                    case "5":       //  Mark Twain
                    case "08":
                    case "8":       //  Shawnee
                    case "12":
                    case "14":      //  Hoosier and Wayne
                    case "19":
                    case "21":      //  Allegheny and Monongahela
                    case "20":
                    case "22":      //  Green Mountain/Finger Lakes and White Mountain
                        strWriter.WriteLine("International 1/4 inch rule was used for board foot volumes in the selected reports.");
                        break;
                }       //  end switch
            }   //  endif region 9

            //  Several blank lines now
            for (int k = 0; k < 6; k++)
                strWriter.WriteLine();

            //  Certification part
            if (isBLM == true)
            {
                for (int k = 0; k < 23; k++)
                {
                    if (k == 3)
                    { strWriter.WriteLine(BLMcertification[k].Replace("XXXXXXXX", headerData.SaleName)); }
                    else
                    { strWriter.WriteLine(BLMcertification[k]); }
                }   //  end for k loop
            }
            else
            {
                for (int k = 0; k < 17; k++)
                {
                    if (k == 2)
                    { strWriter.WriteLine(cruiseCertification[k].Replace("XXXXXXXX", headerData.SaleName)); }
                    else
                    { strWriter.WriteLine(cruiseCertification[k]); }
                }   //  end for loop for certification
            }   //  endif

            //  last portion
            for (int k = 0; k < 8; k++)
                strWriter.WriteLine();

            strWriter.WriteLine(bannerFooter[0]);
            strWriter.WriteLine(bannerFooter[1] + currentVersion);
            strWriter.WriteLine(bannerFooter[2] + dllVersion);
            strWriter.WriteLine(bannerFooter[3]);

            return strWriter.ToString();
        }


    }
}
