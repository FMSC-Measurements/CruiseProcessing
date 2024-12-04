using CruiseDAL;
using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.Processing;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CruiseProcessing.Test.Processing
{
    public class CalcualteTreeValues_20241115_Test : TestBase
    {
        public enum CompareCalculateTreeValueFlags
        {
            None = 0,
            IgnoreBiomass = 1,
        }

        public CalcualteTreeValues_20241115_Test(ITestOutputHelper output) : base(output)
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
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process", CompareCalculateTreeValueFlags.IgnoreBiomass)] // original file has no biomass equations

        [InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        [InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        [InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]
        [InlineData("Issues\\20383_Jiffy Stewardship_TS.04.30.24.process")]


        public void ProcessTrees_Compare_CalulateTreeValues3(string fileName, CompareCalculateTreeValueFlags flags = CompareCalculateTreeValueFlags.None)
        {
            //LogLevel = Microsoft.Extensions.Logging.LogLevel.Trace;
            var filePath = GetTestFile(fileName);

            var mockLogger = CreateLogger<CpDataLayer>();
            var dal = new DAL(filePath);
            var dataLayer = new CpDataLayer(dal, mockLogger, biomassOptions: null);

            //var volEqs = dataLayer.getVolumeEquations();
            //if(volEqs.Any(x => x.CalcBiomass ==1) == false)
            //{
            //    throw new Exception("Skipping test, no biomass equations found");// we are primarily interested in checking for changes in biomass calculation
            //}


            var errors = EditChecks.CheckErrors(dataLayer);
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    Output.WriteLine($"EditCheck: {error.Level} Table: {error.TableName} CN:{error.CN_Number} Column:{error.ColumnName} Message:{ErrorReport.GetErrorMessage(error.Message)}");
                }
            }
            errors.Where(e => e.Level == "E").Should().BeEmpty();



            //var ctv = new RefCalculateTreeValues(dataLayer, CreateLogger<RefCalculateTreeValues>());
            var ctv = new CalculateTreeValues3(dataLayer, new VolumeLibrary_20241101(), CreateLogger<CalculateTreeValues3>());

            var trees = dataLayer.getTrees();
            trees.All(x => x.TreeDefaultValue_CN != null && x.TreeDefaultValue_CN > 0)
                .Should().BeTrue("All Trees Should Have A Tree Default Value");
            Output.WriteLine($"Number of Trees: {trees.Count}");

            var strata = dataLayer.GetStrata();

            var ctv2 = new CalculateTreeValues3(dataLayer, new VolumeLibrary_20241115(), CreateLogger<CalculateTreeValues3>());

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
            Output.WriteLine($"Number of TreeCalculatedValues: {tcvs.Count}");
            var logStocks = dataLayer.getLogStock();


            tcvs2.Count.Should().Be(tcvs.Count);
            logStocks2.Count.Should().Be(logStocks.Count);

            int tcvErrorCount = 0;
            foreach (var pairTcv in tcvs2.Zip(tcvs, (x, y) => (x, y)))
            {
                try
                {
                    pairTcv.x.Should().BeEquivalentTo(pairTcv.y,
                        config: cfg =>
                        {
                            cfg.Excluding(x => x.rowID)
                            .Excluding(x => x.TreeCalcValues_CN)
                            .Excluding(x => x.Self)
                            .Excluding(x => x.DAL)
                            .Excluding(x => x.Tree)
                            .Using<float>(x => x.Subject.Should().BeApproximately(x.Expectation, 0.001f)).WhenTypeIs<float>();

                            cfg.Using<float>(x => x.Subject.Should().BeApproximately(x.Expectation, 0.1f)).When(x => x.SelectedMemberPath == nameof(TreeCalculatedValuesDO.BiomassMainStemPrimary));
                            cfg.Using<float>(x => x.Subject.Should().BeApproximately(x.Expectation, 1.5f)).When(x => x.SelectedMemberPath == nameof(TreeCalculatedValuesDO.BiomassTip));

                            cfg
                                .Excluding(x => x.BiomassMainStemSecondary)
                                .Excluding(x => x.Biomasstotalstem)
                                .Excluding(x => x.Biomasslivebranches)
                                .Excluding(x => x.Biomassdeadbranches)
                                .Excluding(x => x.Biomassfoliage);

                            //if (flags.HasFlag(CompareCalculateTreeValueFlags.IgnoreBiomass))
                            //{
                            //    cfg.Excluding(x => x.BiomassMainStemPrimary)
                            //    .Excluding(x => x.BiomassMainStemSecondary)
                            //    .Excluding(x => x.Biomasstotalstem)
                            //    .Excluding(x => x.Biomasslivebranches)
                            //    .Excluding(x => x.Biomassdeadbranches)
                            //    .Excluding(x => x.Biomassfoliage)
                            //    .Excluding(x => x.BiomassTip);
                            //}

                            return cfg;
                        });
                }
                catch (Exception e)
                {
                    tcvErrorCount++;
                    var tree = pairTcv.x.Tree;
                    var tdv = tree.TreeDefaultValue;
                    Output.WriteLine($"Error comparing TreeCalculatedValues for Tree_CN: {pairTcv.x.Tree_CN} Sp:{tdv.Species} Prod: {tdv.PrimaryProduct} LD:{tree.LiveDead}");
                    Output.WriteLine(e.Message);
                }
            }
            tcvErrorCount.Should().Be(0);

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
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process", CompareCalculateTreeValueFlags.IgnoreBiomass)] // original file has no biomass equations

        [InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        [InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        [InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]
        [InlineData("Issues\\20383_Jiffy Stewardship_TS.04.30.24.process")]


        public void ProcessTrees_Compare_CalulateTreeValues2(string fileName, CompareCalculateTreeValueFlags flags = CompareCalculateTreeValueFlags.None)
        {
            //LogLevel = Microsoft.Extensions.Logging.LogLevel.Trace;
            var filePath = GetTestFile(fileName);

            var mockLogger = CreateLogger<CpDataLayer>();
            var dal = new DAL(filePath);
            var dataLayer = new CpDataLayer(dal, mockLogger, biomassOptions: null);

            //var volEqs = dataLayer.getVolumeEquations();
            //if(volEqs.Any(x => x.CalcBiomass ==1) == false)
            //{
            //    throw new Exception("Skipping test, no biomass equations found");// we are primarily interested in checking for changes in biomass calculation
            //}


            var errors = EditChecks.CheckErrors(dataLayer);
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    Output.WriteLine($"EditCheck: {error.Level} Table: {error.TableName} CN:{error.CN_Number} Column:{error.ColumnName} Message:{ErrorReport.GetErrorMessage(error.Message)}");
                }
            }
            errors.Where(e => e.Level == "E").Should().BeEmpty();



            //var ctv = new RefCalculateTreeValues(dataLayer, CreateLogger<RefCalculateTreeValues>());
            var ctv = new CalculateTreeValues2(dataLayer, new VolumeLibrary_20241115(), CreateLogger<CalculateTreeValues2>());

            var trees = dataLayer.getTrees();
            trees.All(x => x.TreeDefaultValue_CN != null && x.TreeDefaultValue_CN > 0)
                .Should().BeTrue("All Trees Should Have A Tree Default Value");
            Output.WriteLine($"Number of Trees: {trees.Count}");

            var strata = dataLayer.GetStrata();

            var ctv2 = new CalculateTreeValues3(dataLayer, new VolumeLibrary_20241115(), CreateLogger<CalculateTreeValues3>());

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
            Output.WriteLine($"Number of TreeCalculatedValues: {tcvs.Count}");
            var logStocks = dataLayer.getLogStock();


            tcvs2.Count.Should().Be(tcvs.Count);
            logStocks2.Count.Should().Be(logStocks.Count);

            int tcvErrorCount = 0;
            foreach (var pairTcv in tcvs2.Zip(tcvs, (x, y) => (x, y)))
            {
                try
                {
                    pairTcv.x.Should().BeEquivalentTo(pairTcv.y,
                        config: cfg =>
                        {
                            cfg.Excluding(x => x.rowID)
                            .Excluding(x => x.TreeCalcValues_CN)
                            .Excluding(x => x.Self)
                            .Excluding(x => x.DAL)
                            .Excluding(x => x.Tree)
                            .Using<float>(x => x.Subject.Should().BeApproximately(x.Expectation, 0.001f)).WhenTypeIs<float>();

                            cfg.Using<float>(x => x.Subject.Should().BeApproximately(x.Expectation, 0.1f)).When(x => x.SelectedMemberPath == nameof(TreeCalculatedValuesDO.BiomassMainStemPrimary));
                            cfg.Using<float>(x => x.Subject.Should().BeApproximately(x.Expectation, 1.5f)).When(x => x.SelectedMemberPath == nameof(TreeCalculatedValuesDO.BiomassTip));

                            cfg
                                .Excluding(x => x.BiomassMainStemSecondary)
                                .Excluding(x => x.Biomasstotalstem)
                                .Excluding(x => x.Biomasslivebranches)
                                .Excluding(x => x.Biomassdeadbranches)
                                .Excluding(x => x.Biomassfoliage);

                            //if (flags.HasFlag(CompareCalculateTreeValueFlags.IgnoreBiomass))
                            //{
                            //    cfg.Excluding(x => x.BiomassMainStemPrimary)
                            //    .Excluding(x => x.BiomassMainStemSecondary)
                            //    .Excluding(x => x.Biomasstotalstem)
                            //    .Excluding(x => x.Biomasslivebranches)
                            //    .Excluding(x => x.Biomassdeadbranches)
                            //    .Excluding(x => x.Biomassfoliage)
                            //    .Excluding(x => x.BiomassTip);
                            //}

                            return cfg;
                        });
                }
                catch (Exception e)
                {
                    tcvErrorCount++;
                    var tree = pairTcv.x.Tree;
                    var tdv = tree.TreeDefaultValue;
                    Output.WriteLine($"Error comparing TreeCalculatedValues for Tree_CN: {pairTcv.x.Tree_CN} Sp:{tdv.Species} Prod: {tdv.PrimaryProduct} LD:{tree.LiveDead}");
                    Output.WriteLine(e.Message);
                }
            }
            tcvErrorCount.Should().Be(0);

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
