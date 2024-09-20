using CruiseDAL;
using CruiseProcessing.Data;
using CruiseProcessing.ReferenceImplmentation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CruiseProcessing.Test
{
    public class CalculateTreeValues_Test : TestBase
    {
        public CalculateTreeValues_Test(ITestOutputHelper output) : base(output)
        {

        }

        [Theory]
        [InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test_V3.process")]
        [InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise")]
        [InlineData("OgTest\\Region4\\R4_McDougal.cruise")]
        [InlineData("OgTest\\Region5\\R5.cruise")]
        [InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise")]
        [InlineData("OgTest\\Region9\\R9.cruise")]
        [InlineData("OgTest\\Region10\\R10.cruise")]

        [InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise")]
        [InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        [InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise")]
        [InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        [InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        [InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise")]

        [InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        [InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        [InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        [InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]
        [InlineData("Issues\\20383_Jiffy Stewardship_TS.04.30.24.process")]


        public void ProcessTrees_V2(string fileName)
        {
            var filePath = GetTestFile(fileName);

            var mockLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dal = new DAL(filePath);
            var dataLayer = new CpDataLayer(dal, mockLogger);
            var ctv = new RefCalculateTreeValues(dataLayer);

            var trees = dataLayer.getTrees();
            trees.All(x => x.TreeDefaultValue_CN != null && x.TreeDefaultValue_CN > 0)
                .Should().BeTrue();

            var strata = dataLayer.GetStrata();

            dataLayer.DeleteLogStock();
            dataLayer.deleteTreeCalculatedValues();


            dal.BeginTransaction();
            foreach (var st in strata)
            {
                ctv.ProcessTrees(st.Code, st.Method, st.Stratum_CN.Value);
            }
            dal.CommitTransaction();

            var tcvLookup = dataLayer.getTreeCalculatedValues().ToLookup(x => x.Tree_CN.Value);


            foreach (var t in trees.Where(x => x.CountOrMeasure == "M"))
            {
                tcvLookup.Contains(t.Tree_CN.Value).Should().BeTrue();
            }

            //var logStockLookup = dataLayer.getLogStock().ToLookup(x => x.Tree_CN.Value);

            //foreach (var t in trees.Where(x => x.CountOrMeasure == "M"))
            //{
            //    logStockLookup.Contains(t.Tree_CN.Value).Should().BeTrue();
            //}


        }



        [Theory]
        [InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test_V3.process")]
        [InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        [InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise")]
        [InlineData("OgTest\\Region4\\R4_McDougal.cruise")]
        [InlineData("OgTest\\Region5\\R5.cruise")]
        [InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise")]
        [InlineData("OgTest\\Region9\\R9.cruise")]
        [InlineData("OgTest\\Region10\\R10.cruise")]

        [InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise")]
        [InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        [InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise")]
        [InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        [InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        [InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise")]

        [InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        [InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        [InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        [InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]
        [InlineData("Issues\\20383_Jiffy Stewardship_TS.04.30.24.process")]


        public void ProcessTrees_Compare_CalulateTreeValues2(string fileName)
        {
            var filePath = GetTestFile(fileName);

            var mockLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dal = new DAL(filePath);
            var dataLayer = new CpDataLayer(dal, mockLogger);
            var ctv = new RefCalculateTreeValues(dataLayer);

            var trees = dataLayer.getTrees();
            trees.All(x => x.TreeDefaultValue_CN != null && x.TreeDefaultValue_CN > 0)
                .Should().BeTrue();

            var strata = dataLayer.GetStrata();

            var ctv2 = new CalculateTreeValues2(dataLayer, Substitute.For<ILogger<CalculateTreeValues2>>());

            dataLayer.DeleteLogStock();
            dataLayer.deleteTreeCalculatedValues();
            dal.BeginTransaction();
            foreach (var st in strata)
            {
                ctv2.ProcessTrees(st.Code, st.Method, st.Stratum_CN.Value);
            }
            dal.CommitTransaction();

            var tcvs2 = dataLayer.getTreeCalculatedValues();
            var logStocks2 = dataLayer.getLogStock();


            dataLayer.DeleteLogStock();
            dataLayer.deleteTreeCalculatedValues();

           
            dal.BeginTransaction();
            foreach (var st in strata)
            {
                ctv.ProcessTrees(st.Code, st.Method, st.Stratum_CN.Value);
            }
            dal.CommitTransaction();

            var tcvs = dataLayer.getTreeCalculatedValues();
            var logStocks = dataLayer.getLogStock();


            


            tcvs2.Count.Should().Be(tcvs.Count);
            logStocks2.Count.Should().Be(logStocks.Count);

            foreach (var pairTcv in tcvs2.Zip(tcvs, (x, y) => (x, y)))
            {
                pairTcv.x.Should().BeEquivalentTo(pairTcv.y,
                    config: cfg => cfg
                    .Excluding(x => x.rowID)
                    .Excluding(x => x.TreeCalcValues_CN)
                    .Excluding(x => x.Self)
                    .Excluding(x => x.DAL)
                    .Excluding(x => x.Tree));
            }

            var logStockTrees = logStocks.GroupBy(x => x.Tree_CN).ToDictionary(x => x.Key, y => y.ToArray());
            var logStockTrees2 = logStocks2.GroupBy(x => x.Tree_CN).ToDictionary(x => x.Key, y => y.ToArray());

            foreach (var tcn in logStockTrees.Keys)
            {
                logStockTrees2.ContainsKey(tcn).Should().BeTrue();

                var logs = logStockTrees[tcn];
                var logs2 = logStockTrees2[tcn];

                logs.Length.Should().Be(logs2.Length);

                foreach (var item in logs2.Select((log, i) => (log, i)))
                {
                    item.log.Should().BeEquivalentTo(logs[item.i],
                        config: cfg => cfg
                        .Excluding(x => x.rowID)
                        .Excluding(x => x.LogStock_CN)
                        .Excluding(x => x.Self)
                        .Excluding(x => x.DAL)
                        .Excluding(x => x.Tree));
                }
            }


        }

        [Theory]
        [InlineData("Issues\\20383_Jiffy Stewardship_TS.crz3")]
        public void ProcessTrees_Compare_CalulateTreeValues2_V3(string fileName)
        {
            var filePath = GetTestFile(fileName);

            var mockLogger = Substitute.For<ILogger<CpDataLayer>>();

            var db = new CruiseDatastore_V3(filePath);
            var cruiseID = db.From<CruiseDAL.V3.Models.Cruise>().Query().Single().CruiseID;

            var v2Path = GetTempFilePath(".cruise", Path.GetFileNameWithoutExtension(filePath) + ".v2");
            var dal = new DAL(v2Path, true);

            var migrator = new DownMigrator();
            migrator.MigrateFromV3ToV2(cruiseID, db, dal);

            var dataLayer = new CpDataLayer(dal, mockLogger);
            var ctv = new RefCalculateTreeValues(dataLayer);

            var trees = dataLayer.getTrees();
            trees.All(x => x.TreeDefaultValue_CN != null && x.TreeDefaultValue_CN > 0)
                .Should().BeTrue();

            dataLayer.DeleteLogStock();
            dataLayer.deleteTreeCalculatedValues();

            var strata = dataLayer.GetStrata();
            dal.BeginTransaction();
            foreach (var st in strata)
            {
                ctv.ProcessTrees(st.Code, st.Method, st.Stratum_CN.Value);
            }
            dal.CommitTransaction();

            var tcvs = dataLayer.getTreeCalculatedValues();
            var logStocks = dataLayer.getLogStock();


            var ctv2 = new CalculateTreeValues2(dataLayer, Substitute.For<ILogger<CalculateTreeValues2>>());

            dataLayer.DeleteLogStock();
            dataLayer.deleteTreeCalculatedValues();
            dal.BeginTransaction();
            foreach (var st in strata)
            {
                ctv2.ProcessTrees(st.Code, st.Method, st.Stratum_CN.Value);
            }
            dal.CommitTransaction();

            var tcvs2 = dataLayer.getTreeCalculatedValues();
            var logStocks2 = dataLayer.getLogStock();


            tcvs2.Count.Should().Be(tcvs.Count);
            logStocks2.Count.Should().Be(logStocks.Count);

            foreach (var pairTcv in tcvs2.Zip(tcvs, (x, y) => (x, y)))
            {
                pairTcv.x.Should().BeEquivalentTo(pairTcv.y,
                    config: cfg => cfg
                    .Excluding(x => x.rowID)
                    .Excluding(x => x.TreeCalcValues_CN)
                    .Excluding(x => x.Self)
                    .Excluding(x => x.DAL)
                    .Excluding(x => x.Tree));
            }

            var logStockTrees = logStocks.GroupBy(x => x.Tree_CN).ToDictionary(x => x.Key, y => y.ToArray());
            var logStockTrees2 = logStocks2.GroupBy(x => x.Tree_CN).ToDictionary(x => x.Key, y => y.ToArray());

            foreach (var tcn in logStockTrees.Keys)
            {
                logStockTrees2.ContainsKey(tcn).Should().BeTrue();

                var logs = logStockTrees[tcn];
                var logs2 = logStockTrees2[tcn];

                logs.Length.Should().Be(logs2.Length);

                foreach (var item in logs2.Select((log, i) => (log, i)))
                {
                    item.log.Should().BeEquivalentTo(logs[item.i],
                        config: cfg => cfg
                        .Excluding(x => x.rowID)
                        .Excluding(x => x.LogStock_CN)
                        .Excluding(x => x.Self)
                        .Excluding(x => x.DAL)
                        .Excluding(x => x.Tree));
                }
            }


        }
    }
}
