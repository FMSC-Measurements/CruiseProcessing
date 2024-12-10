using CruiseDAL.DataObjects;
using CruiseProcessing.Output;
using CruiseProcessing.OutputModels;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CruiseProcessing.Test.Output;

public class BannerPageTest : TestBase
{
    public BannerPageTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void outputBannerPage()
    {
        var expected =
@"CRUISE REPORT HEADER & CERTIFICATION
CRUISE#: 30206      SALE#: 30206
SALENAME: Speed Goat 
RUN DATE & TIME: 3/4/2024 3:36:23 PM
 
Year: 0    Region: 05   Forest: 06   District: 58
Remarks:  
FILENAME: C:\Users\benjaminjcampbell\Documents\Support\2024.02.26_ProcessReportsIssue\30206 Speed Goat  TS 02052024 - Copy.cruise
REPORTS: A01 A02 A03 A04 A05 A07 A08 R401 R402 R501 ST1 ST3 UC1 UC2 






                            **************************** CRUISE CERTIFICATION ****************************

                                I certify that the timber for the Speed Goat timber sale
                                has been designated and cruised by the procedures and standards in
                                FSH 2409.12, Timber Cruising Handbook.  Records of checks are on file
                                at the District Ranger Office,



                                ___________________                 ___________________________________
                                RANGER DISTRICT                     (name of headquarters town)


                                ___________________                 ______________
                                DISTRICT RANGER                     DATE

                            *****************************************************************************








Developed and Maintained By:
USDA FOREST SERVICE                                                                                             VERSION: 04.15.2023
WASHINGTON OFFICE TIMBER MANAGEMENT                                                              VOLUME LIBRARY VERSION: 04.03.2023
FORT COLLINS, COLORADO (970)295-5776                                                             NATIONAL CRUISE PROCESSING PROGRAM
";

        string filename = "C:\\Users\\benjaminjcampbell\\Documents\\Support\\2024.02.26_ProcessReportsIssue\\30206 Speed Goat  TS 02052024 - Copy.cruise";
        string currentDate = "3/4/2024 3:36:23 PM";
        string currentVersion = "04.15.2023";
        string dllVersion = "04.03.2023";
        SaleDO sale = new SaleDO
        {
            SaleNumber = "30206",
            Name = "Speed Goat ",
            CalendarYear = 0,
            Region = "05",
            Forest = "06",
            District = "58",
        };
        IEnumerable<ReportsDO> reports = new[]
            {
                new ReportsDO { ReportID = "A01"},
                new ReportsDO { ReportID = "A02"},
                new ReportsDO { ReportID = "A03"},
                new ReportsDO { ReportID = "A04"},
                new ReportsDO { ReportID = "A05"},
                new ReportsDO { ReportID = "A07"},
                new ReportsDO { ReportID = "A08"},
                new ReportsDO { ReportID = "R401"},
                new ReportsDO { ReportID = "R402"},
                new ReportsDO { ReportID = "R501"},
                new ReportsDO { ReportID = "ST1"},
                new ReportsDO { ReportID = "ST3"},
                new ReportsDO { ReportID = "UC1"},
                new ReportsDO { ReportID = "UC2"},
            };
        IEnumerable<VolumeEquationDO> volumeEquations = new[] // only for BLM to see if using 32 ft vol Eqs
            {
                new VolumeEquationDO{ },
            };


        var headerData = new HeaderFieldData
        {
            SaleName = sale.Name.Trim(' '),
            CruiseName = sale.SaleNumber,
            Version = currentVersion,
            DllVersion = dllVersion,
            Date = currentDate,
        };

        var result = BannerPage.GenerateBannerPage(filename, headerData, sale, reports, volumeEquations);
        VerifyLines(result.Split(new[] { Environment.NewLine, }, StringSplitOptions.None),
            expected.Split(new[] { Environment.NewLine, }, StringSplitOptions.None));

        result.Should().Be(expected);

    }


    private void VerifyLines(IEnumerable<string> resultLines, IEnumerable<string> expectedLines)
    {
        resultLines.Should().HaveSameCount(expectedLines);

        foreach(var (r,ex) in resultLines.Zip(expectedLines, (x, y) => (x,y)))
        {
            //if (r.StartsWith("RUN DATE & TIME:")) { continue; }

            //r.Should().HaveLength(ex.Length);
            r.Should().Be(ex);
        }
    }

}
