using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Output;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CruiseProcessing.Test.Output
{
    public class ErrorReport_Test : TestBase
    {
        public ErrorReport_Test(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void PrintErrorReport_ErrorsAndWarnings()
        {
            var errList = new List<ErrorLogDO>();


            foreach(var i in Enumerable.Range(0,ErrorReport.ERROR_MESSAGE_LOOKUP.GetLength(0)))
            {
                errList.Add(new ErrorLogDO()
                {
                    Level = "E",
                    Message = ErrorReport.ERROR_MESSAGE_LOOKUP[i, 0],
                    TableName = "Sale",
                    ColumnName = "ColumnName",
                    CN_Number = 1,
                    Program = "CruiseProcessing"
                });
            }

            //errList.Add(new ErrorLogDO()
            //{
            //    Level = "E",
            //    Message = null,
            //    TableName = "Sale",
            //    CN_Number = 1,
            //    Program = "CruiseProcessing"
            //});

            errList.Add(new ErrorLogDO()
            {
                Level = "E",
                Message = "someErrorMessage",
                TableName = "Sale",
                CN_Number = 1,
                Program = "CruiseProcessing"
            });

            errList.Add(new ErrorLogDO()
            {
                Level = "E",
                Message = null,
                TableName = "Sale",
                CN_Number = 1,
                Program = "NotCruiseProcessing"
            });

            errList.Add(new ErrorLogDO()
            {
                Level = "E",
                Message = "someErrorMessage",
                TableName = "Sale",
                CN_Number = 1,
                Program = "NotCruiseProcessing"
            });

            errList.Add(new ErrorLogDO()
            {
                Level = "E",
                Message = "someErrorMessage",
                TableName = "-",
                CN_Number = 0,
                Program = "NotCruiseProcessing"
            });


            errList.Add(new ErrorLogDO()
            {
                Level = "E",
                Message = $"Sg: 01 Missing Primary Product",
                TableName = "SampleGroup",
                CN_Number = 1,
                Program = "CruiseProcessing"
            });

            foreach (var s in ErrorReport.WARNING_MESSAGE_LOOKUP)
            {
                errList.Add(new ErrorLogDO { Level = "W", Message = s, TableName = "Sale", CN_Number = 1 });
            }

            errList.Add(new ErrorLogDO()
            {
                Level = "I",
                Message = $"Some Informational Message",
                TableName = "-",
                CN_Number = 1,
                Program = "CruiseProcessing"
            });

            var init = new DatabaseInitializer_V2();
            var db = init.CreateDatabase();

            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();

            var dataLayer = new CpDataLayer(db, mockDlLogger, biomassOptions: null);

            var headerFieldData = new HeaderFieldData { CruiseName = "Name", Date = DateTime.Now.ToString(), DllVersion = "1.1.1.1", SaleName = "saleName", Version= "1.1.1.2"  };

            var errorReport = new ErrorReport(dataLayer, headerFieldData);


            using var stringWriter = new StringWriter();
            errorReport.WriteErrorReport(stringWriter, errList, "someProgram", "someFile.out");

            Output.WriteLine(stringWriter.ToString());
        }
    }
}
