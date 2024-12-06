using CruiseProcessing.OutputModels;
using Json.Schema;
using Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CruiseProcessing.Test.OutputModels
{
    public class TeaReport_Test : TestBase
    {
        public TeaReport_Test(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GenerateSchema()
        {
            var schemabulider = new JsonSchemaBuilder();
            var schema = schemabulider.FromType<TeaReport>().Build();

            var schemaString = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true, IndentSize = 4});
            Output.WriteLine(schemaString);
        }
    }
}
