using CruiseDAL;
using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.Processing;
using CruiseProcessing.ReferenceImplmentation;
using CruiseProcessing.Services;
using DiffPlex.DiffBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CruiseProcessing.Test.Processing
{
    public class CruiseProcessor_Test : TestBase
    {
        public CruiseProcessor_Test(ITestOutputHelper output) : base(output)
        {
        }


        [Theory]
        //[InlineData("OgTest\\BLM\\Hammer Away.cruise")]
        //[InlineData("OgTest\\BLM\\Long Nine.cruise")]
        [InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test_V3.process")]
        //[InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        //[InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise")]
        [InlineData("OgTest\\Region4\\R4_McDougal.cruise")]
        [InlineData("OgTest\\Region5\\R5.cruise")]
        //[InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise")]
        [InlineData("OgTest\\Region9\\R9.cruise")]
        [InlineData("OgTest\\Region10\\R10.cruise")]

        [InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        [InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise")]
        //[InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        //[InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        [InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise")]

        [InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        //[InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        //[InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process")]

        //[InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        [InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        [InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]
        public void ProcessCruise(string testFileName)
        {
            using var dataLayer = ProcessCruiseHelper(testFileName);
        }





        private CpDataLayer ProcessCruiseHelper(string testFileName)
        {
            string filePath = null;

            var extention = Path.GetExtension(testFileName);
            if (extention == ".crz3")
            {
                var v3Path = GetTestFile(testFileName);
                using var v3db = new CruiseDatastore_V3(v3Path);
                var cruiseID = v3db.From<CruiseDAL.V3.Models.Cruise>().Query().Single().CruiseID;

                var v2Path = GetTempFilePath(".process", Path.GetFileNameWithoutExtension(testFileName) + ".ProcessAndVerityOutput_V3");
                using var v2Db = new DAL(v2Path, true);

                var migrator = new DownMigrator();
                migrator.MigrateFromV3ToV2(cruiseID, v3db, v2Db);

                filePath = v2Path;
            }
            else
            {
                filePath = GetTestFile(testFileName);
            }


            var dal = new DAL(filePath);

            var mockDlLogger = Substitute.For<ILogger<CpDataLayer>>();
            var dataLayer = new CpDataLayer(dal, mockDlLogger, biomassOptions: null);

            List<ErrorLogDO> fscList = dataLayer.getErrorMessages("E", "FScruiser");
            var errors = EditChecks.CheckErrors(dataLayer);

            if (fscList.Any())
            { throw new Exception("Skip - Cruise FSC errors"); }
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    Output.WriteLine($"Table: {error.TableName} Msg:{ErrorReport.GetErrorMessage(error.Message)}");
                }

                throw new Exception("Skip - Cruise errors");
            }

            var mockDialogService = new Mock<IDialogService>();
            var ctv = new CalculateTreeValues2(dataLayer, VolumeLibraryInterop.Default, CreateLogger<CalculateTreeValues2>());
            var cruiseProcessor = new CruiseProcessor(dataLayer, mockDialogService.Object, ctv, CreateLogger<CruiseProcessor>());



            dal.TransactionDepth.Should().Be(0, "Before Process");
            var mockProgress = new Mock<IProgress<string>>();
            cruiseProcessor.ProcessCruise(mockProgress.Object);

            dal.TransactionDepth.Should().Be(0, "After Process");

            return dataLayer;
        }

        [Flags]

        public enum CompareTreeCalculatedValuesFlags
        {
            None = 0,
            IgnoreBiomass = 1,
        }

        [Theory]
        //[InlineData("OgTest\\BLM\\Hammer Away.cruise")]
        //[InlineData("OgTest\\BLM\\Long Nine.cruise")]
        [InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test.cruise")]
        [InlineData("OgTest\\Region2\\R2_Test_V3.process")]
        //[InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        //[InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise")]
        [InlineData("OgTest\\Region4\\R4_McDougal.cruise")]
        [InlineData("OgTest\\Region5\\R5.cruise")]
        //[InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise")]
        [InlineData("OgTest\\Region9\\R9.cruise")]
        [InlineData("OgTest\\Region10\\R10.cruise")]

        [InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        [InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process")]

        [InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise")]
        //[InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        //[InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        [InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise")]

        [InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        //[InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        //[InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process", Skip = "Has Blank Tree Grades")]

        //[InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        [InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        [InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]

        [InlineData("Temp\\21853\\21853 Swanson Creek TS Data Checked.cruise")]
        [InlineData("Temp\\21853\\21853 Swanson Creek TS Data Checked_withTallySetup.cruise")]
        public void ProcessCruise_CompareReferenceImplementation(string testFileName)
        {
            //
            // initialize datalayer for test file
            //
            string filePath = null;

            var extention = Path.GetExtension(testFileName);
            if (extention == ".crz3")
            {
                var v3Path = GetTestFile(testFileName);
                using var v3db = new CruiseDatastore_V3(v3Path);
                var cruiseID = v3db.From<CruiseDAL.V3.Models.Cruise>().Query().Single().CruiseID;

                var v2Path = GetTempFilePath(".process", Path.GetFileNameWithoutExtension(testFileName) + ".ProcessAndVerityOutput_V3");
                using var v2Db = new DAL(v2Path, true);

                var migrator = new DownMigrator();
                migrator.MigrateFromV3ToV2(cruiseID, v3db, v2Db);

                filePath = v2Path;
            }
            else
            {
                filePath = GetTestFile(testFileName);
            }

            var dal = new DAL(filePath);

            var mockDlLogger = CreateLogger<CpDataLayer>();
            var dataLayer = new CpDataLayer(dal, mockDlLogger, biomassOptions: null);

            //
            // check cruise for errors
            //

            List<ErrorLogDO> fscList = dataLayer.getErrorMessages("E", "FScruiser");
            var errors = EditChecks.CheckErrors(dataLayer);

            if (fscList.Any())
            { throw new Exception("Skip - Cruise FSC errors"); }
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    Output.WriteLine($"Table: {error.TableName} CN:{error.CN_Number} Column:{error.ColumnName} Message:{ErrorReport.GetErrorMessage(error.Message)}");
                }

                throw new Exception("Skip - Cruise errors");
            }

            //
            // initialize processor and process cruise
            //

            var mockDialogService = new Mock<IDialogService>();
            var mockLogger = CreateLogger<CruiseProcessor>();

            var ctv = new CalculateTreeValues2(dataLayer, VolumeLibraryInterop.Default, CreateLogger<CalculateTreeValues2>());
            var cruiseProcessor = new CruiseProcessor(dataLayer, mockDialogService.Object, ctv, CreateLogger<CruiseProcessor>());



            dal.TransactionDepth.Should().Be(0, "Before Process");
            var mockProgress = new Mock<IProgress<string>>();
            cruiseProcessor.ProcessCruise(mockProgress.Object);

            dal.TransactionDepth.Should().Be(0, "After Process");

            var tcvs = dataLayer.getTreeCalculatedValues();
            var lcds = dataLayer.getLCD();
            var pops = dataLayer.getPOP();
            var pros = dataLayer.getPRO();

            //
            // initialize reference processor and process cruise
            //

            //var mockServiceProvider = Substitute.For<IServiceProvider>();
            //mockServiceProvider.GetService(Arg.Is(typeof(ICalculateTreeValues))).Returns(new CalculateTreeValues2(dataLayer, Substitute.For<ILogger<CalculateTreeValues2>>()));
            //mockServiceProvider.GetService(Arg.Is(typeof(ICalculateTreeValues))).Returns(new RefCalculateTreeValues(dataLayer));

            var refProcessor = new RefCruiseProcessor(dataLayer, Substitute.For<ILogger<RefCruiseProcessor>>(), CreateLogger<RefCalculateTreeValues>());
            refProcessor.ProcessCruise(Substitute.For<IProgress<string>>());

            var tcvAgain = dataLayer.getTreeCalculatedValues();
            var lcdsAgain = dataLayer.getLCD();
            var popsAgain = dataLayer.getPOP();
            var prosAgain = dataLayer.getPRO();

            //
            // compare results
            //

            tcvs.Should().HaveSameCount(tcvAgain);
            foreach (var tcv in tcvs)
            {
                var match = tcvAgain.SingleOrDefault(x => x.Tree_CN == tcv.Tree_CN);
                match.Should().NotBeNull();
                tcv.Should().BeEquivalentTo(match, config: (cfg) =>
                {
                    return cfg.Excluding(x => x.TreeCalcValues_CN)
                    .Excluding(x => x.rowID)
                    .Excluding(x => x.Self)
                    .Using<double>(x => x.Subject.Should().BeApproximately(x.Expectation, 0.001)).WhenTypeIs<double>()
                    .Using<float>(x => x.Subject.Should().BeApproximately(x.Expectation, 0.001f)).WhenTypeIs<float>();
                });
            }

            lcdsAgain.Should().HaveSameCount(lcds);
            foreach (var lcd in lcds)
            {
                var match = lcdsAgain.SingleOrDefault(x => x.Stratum == lcd.Stratum && x.SampleGroup == lcd.SampleGroup && x.Species == lcd.Species && x.STM == lcd.STM && x.LiveDead == lcd.LiveDead && x.TreeGrade == lcd.TreeGrade);
                match.Should().NotBeNull();

                Output.WriteLine($"lcd st:{lcd.Stratum} sg:{lcd.SampleGroup} sp:{lcd.Species} {lcd.STM} {lcd.LiveDead} {lcd.TreeGrade}");

                lcd.Should().BeEquivalentTo(match, config: (cfg) =>
                {
                    return cfg.Excluding(x => x.LCD_CN)
                    .Excluding(x => x.rowID)
                    .Excluding(x => x.Self)
                    .Using<double>(x => x.Subject.Should().BeApproximately(x.Expectation, 0.001)).WhenTypeIs<double>();
                });
            }

            foreach (var pop in pops)
            {
                var match = popsAgain.SingleOrDefault(x => x.Stratum == pop.Stratum && x.SampleGroup == pop.SampleGroup && x.STM == pop.STM);
                match.Should().NotBeNull();

                Output.WriteLine($"pop st:{pop.Stratum} sg:{pop.SampleGroup}");

                pop.Should().BeEquivalentTo(match, because: $"{pop.Stratum} {pop.SampleGroup}", config: (cfg) =>
                {
                    return cfg.Excluding(x => x.POP_CN).Excluding(x => x.rowID).Excluding(x => x.Self)
                    .Using<double>(x => x.Subject.Should().BeApproximately(x.Expectation, 0.001)).WhenTypeIs<double>();
                });
            }

            foreach (var pro in pros)
            {
                var match = prosAgain.SingleOrDefault(x => x.Stratum == pro.Stratum && x.SampleGroup == pro.SampleGroup && x.CuttingUnit == pro.CuttingUnit && x.STM == pro.STM);
                match.Should().NotBeNull();
                pro.Should().BeEquivalentTo(match, config: (cfg) =>
                {
                    return cfg.Excluding(x => x.PRO_CN).Excluding(x => x.rowID).Excluding(x => x.Self)
                    .Using<double>(x => x.Subject.Should().BeApproximately(x.Expectation, 0.001)).WhenTypeIs<double>();
                });
            }



        }

        [Theory]
        //[InlineData("OgTest\\BLM\\Hammer Away.cruise")]
        //[InlineData("OgTest\\BLM\\Long Nine.cruise")]
        //[InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise")]
        //[InlineData("OgTest\\Region2\\R2_Test.cruise")]
        //[InlineData("OgTest\\Region2\\R2_Test_V3.process")]
        //[InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        //[InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise")]
        //[InlineData("OgTest\\Region4\\R4_McDougal.cruise")]
        [InlineData("OgTest\\Region5\\R5.cruise")]
        //[InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise")]
        //[InlineData("OgTest\\Region9\\R9.cruise")]
        //[InlineData("OgTest\\Region10\\R10.cruise")]

        //[InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        //[InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process")]

        //[InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise")]
        //[InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        //[InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        //[InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise")]

        //[InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        //[InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        //[InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        //[InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process", Skip = "Has Blank Tree Grades")]

        //[InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        //[InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")]
        //[InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")]

        //[InlineData("Temp\\21853\\21853 Swanson Creek TS Data Checked.cruise")]
        //[InlineData("Temp\\21853\\21853 Swanson Creek TS Data Checked_withTallySetup.cruise")]
        public void ProcessCruise_CompareImplimentations_TreeCalculatedValues(string testFileName)
        {
            //
            // initialize datalayer for test file
            //
            string filePath = null;

            var extention = Path.GetExtension(testFileName);
            if (extention == ".crz3")
            {
                var v3Path = GetTestFile(testFileName);
                using var v3db = new CruiseDatastore_V3(v3Path);
                var cruiseID = v3db.From<CruiseDAL.V3.Models.Cruise>().Query().Single().CruiseID;

                var v2Path = GetTempFilePath(".process", Path.GetFileNameWithoutExtension(testFileName) + ".ProcessAndVerityOutput_V3");
                using var v2Db = new DAL(v2Path, true);

                var migrator = new DownMigrator();
                migrator.MigrateFromV3ToV2(cruiseID, v3db, v2Db);

                filePath = v2Path;
            }
            else
            {
                filePath = GetTestFile(testFileName);
            }

            var dal = new DAL(filePath);

            var mockDlLogger = CreateLogger<CpDataLayer>();
            var dataLayer = new CpDataLayer(dal, mockDlLogger, biomassOptions: null);

            //
            // check cruise for errors
            //

            List<ErrorLogDO> fscList = dataLayer.getErrorMessages("E", "FScruiser");
            var errors = EditChecks.CheckErrors(dataLayer);

            if (fscList.Any())
            { throw new Exception("Skip - Cruise FSC errors"); }
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    Output.WriteLine($"Table: {error.TableName} CN:{error.CN_Number} Column:{error.ColumnName} Message:{ErrorReport.GetErrorMessage(error.Message)}");
                }

                throw new Exception("Skip - Cruise errors");
            }


            var defaultProcessor = new CruiseProcessorItem
            {
                Name = "OldProcess_20240626",
                Processor = new CruiseProcessor(dataLayer, Substitute.For<IDialogService>(), new CalculateTreeValues2(dataLayer, VolumeLibraryInterop.Default, CreateLogger<CalculateTreeValues2>()), CreateLogger<CruiseProcessor>())
            };

            var processors = new[]
            {
                defaultProcessor,
                //new CruiseProcessorItem
                //{
                //    Name = "Reference",
                //    Processor = new RefCruiseProcessor(dataLayer, Substitute.For<ILogger<RefCruiseProcessor>>(), CreateLogger<RefCalculateTreeValues>())
                //},

                new CruiseProcessorItem
                {
                    Name = "OldProcess_20241101",
                    Processor = new CruiseProcessor(dataLayer, Substitute.For<IDialogService>(), new CalculateTreeValues2(dataLayer, new VolumeLibrary_20241101(), CreateLogger<CalculateTreeValues2>()), CreateLogger<CruiseProcessor>())
                },
                new CruiseProcessorItem
                {
                    Name = "NewProcess_20241101",
                    Processor = new CruiseProcessor_20241101_Preview(dataLayer, Substitute.For<IDialogService>(), new CalculateTreeValues3(dataLayer, new VolumeLibrary_20241101(), CreateLogger<CalculateTreeValues3>()), CreateLogger<CruiseProcessor_20241101_Preview>())
                },
                new CruiseProcessorItem
                {
                    Name = "OldProcess_20241118",
                    Processor = new CruiseProcessor(dataLayer, Substitute.For<IDialogService>(), new CalculateTreeValues2(dataLayer, new VolumeLibrary_20241118(), CreateLogger<CalculateTreeValues2>()), CreateLogger<CruiseProcessor>())
                },
                new CruiseProcessorItem
                {
                    Name = "NewProcess_20241118",
                    Processor = new CruiseProcessor_20241101_Preview(dataLayer, Substitute.For<IDialogService>(), new CalculateTreeValues3(dataLayer, new VolumeLibrary_20241118(), CreateLogger<CalculateTreeValues3>()), CreateLogger<CruiseProcessor_20241101_Preview>())
                },
            };

            BiomassHelpers.UpdateBiomass(dataLayer, dataLayer.getVolumeEquations());

            foreach (var processor in processors)
            {
                processor.Processor.ProcessCruise(Substitute.For<IProgress<string>>());

                processor.TreeCalculatedValueResults = dataLayer.getTreeCalculatedValues();
                processor.LCDResults = dataLayer.getLCD();
                processor.POPResults = dataLayer.getPOP();
            }

            var headerPad = 24;
            var dataColPad = processors.Max(x => x.Name.Length) + 2;

            foreach (var tcv in defaultProcessor.TreeCalculatedValueResults)
            {
                var tree = dataLayer.GetTree(tcv.Tree_CN);

                var treeCnStr = tcv.Tree_CN.ToString().PadLeft(4);
                var treeNumStr = tree.TreeNumber.ToString().PadLeft(4);
                var dbhStr = tree.DBH.ToString().PadLeft(6);
                var totHtStr = tree.TotalHeight.ToString().PadLeft(6);
                var species = tree.Species.PadLeft(4);
                var fiaCode = tree.TreeDefaultValue.FIAcode.ToString().PadLeft(4);
                var liveDead = tree.LiveDead.PadLeft(1);
                Output.WriteLine($"Tree_CN {treeCnStr} TreeNum:{treeNumStr} Sp:{species} FIA:{fiaCode} LD:{liveDead} DBH:{dbhStr} TotHt:{totHtStr}");

                var tcvResults = processors
                    //.Where(p => !object.ReferenceEquals(p, defaultProcessor))
                    .Select(x => x.GetTcvResult(tcv.Tree_CN)).ToArray();


                // write header for each tree
                Output.WriteLine($"{string.Empty.PadRight(headerPad)}  {string.Join(",", processors.Select(x => x.Name.PadLeft(dataColPad).Substring(0, dataColPad)))}");

                WriteProperty(Output, tcv, tcvResults, headerPad, dataColPad, x => x.Biomasstotalstem);

                WriteProperty(Output, tcv, tcvResults, headerPad, dataColPad, x => x.BiomassMainStemPrimary);
                WriteProperty(Output, tcv, tcvResults, headerPad, dataColPad, x => x.BiomassMainStemSecondary);
                WriteProperty(Output, tcv, tcvResults, headerPad, dataColPad, x => x.BiomassTip);
                WriteProperty(Output, tcv, tcvResults, headerPad, dataColPad, x => x.Biomasslivebranches);
                WriteProperty(Output, tcv, tcvResults, headerPad, dataColPad, x => x.Biomassfoliage);

                //WriteProperty(Output, tcv, tcvResults, headerPad, dataColPad, x => x.BiomassProd);
                //WriteProperty(Output, tcv, tcvResults, headerPad, dataColPad, x => x.Biomassdeadbranches);





                Output.WriteLine("");
            }

            void WriteProperty<A, T>(ITestOutputHelper output, A firstData, IEnumerable<A> moreData, int headerPad, int dataPad, Expression<Func<A, T>> accessorExpr)
            {

                // Get the property name
                if (accessorExpr.Body is MemberExpression memberExpression)
                {
                    var propertyName = memberExpression.Member.Name;

                    // Compile the expression to get the property value
                    var func = accessorExpr.Compile();
                    var firstValue = func.Invoke(firstData);


                    var moreValues = moreData.Select(x => (func.Invoke(x)?.ToString() ?? "").PadLeft(dataPad).Substring(0, dataPad)).ToArray();
                    //var firstValueStr = (firstValue?.ToString() ?? "").PadLeft(dataPad).Substring(0, dataPad);
                    // Write the property name and value to the output stream
                    output.WriteLine($"{propertyName.ToString().PadRight(headerPad)}: {string.Join(",", moreValues)}");
                }
                else
                {
                    throw new ArgumentException("The expression must be a member expression", nameof(accessorExpr));
                }
            }
        }

        private class CruiseProcessorItem
        {
            public string Name { get; set; }
            public ICruiseProcessor Processor { get; set; }

            public IEnumerable<TreeCalculatedValuesDO> TreeCalculatedValueResults { get; set; }
            public IEnumerable<LCDDO> LCDResults { get; set; }
            public IEnumerable<POPDO> POPResults { get; set; }
            public IEnumerable<PRODO> PROResults { get; set; }

            public TreeCalculatedValuesDO GetTcvResult(long? tree_CN)
            {
                return TreeCalculatedValueResults.Single(x => x.Tree_CN == tree_CN);
            }

            public LCDDO GetLCDResult(LCDDO lcd)
            {
                return LCDResults.SingleOrDefault(x => x.Stratum == lcd.Stratum
                && x.SampleGroup == lcd.SampleGroup
                && x.Species == lcd.Species
                && x.STM == lcd.STM
                && x.LiveDead == lcd.LiveDead
                && x.TreeGrade == lcd.TreeGrade);
            }

            public POPDO GetPOPResult(POPDO pop)
            {
                return POPResults.SingleOrDefault(x => x.Stratum == pop.Stratum
                && x.SampleGroup == pop.SampleGroup
                && x.STM == pop.STM);
            }
        }


        [Theory]
        //[InlineData("OgTest\\BLM\\Hammer Away.cruise")]
        //[InlineData("OgTest\\BLM\\Long Nine.cruise")]
        [InlineData("OgTest\\Region1\\R1_FrenchGulch.cruise", "OgTest\\Region1\\R1_FrenchGulch.out")]
        [InlineData("OgTest\\Region2\\R2_Test.cruise", "OgTest\\Region2\\R2_Test.out")]
        [InlineData("OgTest\\Region2\\R2_Test_V3.process", "OgTest\\Region2\\R2_Test_V3.out")]
        //[InlineData("OgTest\\Region3\\R3_FCM_100.cruise")]
        //[InlineData("OgTest\\Region3\\R3_PCM_FIXCNT.cruise")]
        [InlineData("OgTest\\Region3\\R3_PNT_FIXCNT.cruise", "OgTest\\Region3\\R3_PNT_FIXCNT.out")]
        [InlineData("OgTest\\Region4\\R4_McDougal.cruise", "OgTest\\Region4\\R4_McDougal.out")]
        [InlineData("OgTest\\Region5\\R5.cruise", "OgTest\\Region5\\R5.out")]
        //[InlineData("OgTest\\Region6\\R6.cruise")]
        [InlineData("OgTest\\Region8\\R8.cruise", "OgTest\\Region8\\R8.out")]
        [InlineData("OgTest\\Region9\\R9.cruise", "OgTest\\Region9\\R9.out")]
        [InlineData("OgTest\\Region10\\R10.cruise", "OgTest\\Region10\\R10.out", "R008,R009")]

        [InlineData("Version3Testing\\3P\\87654 test 3P TS.cruise", "Version3Testing\\3P\\87654 test 3P TS.out")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021.process")]
        //[InlineData("Version3Testing\\3P\\87654_test 3P_Timber_Sale_26082021_fixedTallyBySp.process")]
        [InlineData("Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.process", "Version3Testing\\3P\\87654_Test 3P_Timber_Sale_30092021.out")]

        [InlineData("Version3Testing\\FIX\\20301 Cold Springs Recon.cruise", "Version3Testing\\FIX\\20301 Cold Springs Recon.out")]
        //[InlineData("Version3Testing\\FIX\\20301_Cold Springs_Timber_Sale_29092021.process")]

        //[InlineData("Version3Testing\\FIX and PNT\\99996_TestMeth_Timber_Sale_08072021.process")]

        [InlineData("Version3Testing\\PCM\\27504_Spruce East_TS.cruise", "Version3Testing\\PCM\\27504_Spruce East_TS.out")]

        [InlineData("Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise", "Version3Testing\\PNT\\Exercise3_Dead_LP_Recon.cruise")]

        //[InlineData("Version3Testing\\STR\\98765 test STR TS.cruise")]
        //[InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_26082021.process")]
        [InlineData("Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.process", "Version3Testing\\STR\\98765_test STR_Timber_Sale_30092021.out")]

        //[InlineData("Version3Testing\\TestMeth\\99996_TestMeth_TS_202310040107_KC'sTabActive3-R9Q8.process")]

        //[InlineData("Version3Testing\\27504PCM_Spruce East_Timber_Sale.cruise")] no out file
        //[InlineData("Version3Testing\\99996FIX_PNT_Timber_Sale_08242021.cruise")] no out file


        public void ProcessAndVerityOutput(string testFileName, string expectedOutputFileName, string expectedFailingReports = "")
        {
            using var dataLayer = ProcessCruiseHelper(testFileName);

            var ctf = new CreateTextFile(dataLayer, VolumeLibraryInterop.Default, Substitute.For<ILogger<CreateTextFile>>());

            var stringWriter = new StringWriter();

            var reports = dataLayer.GetSelectedReports();
            var headerData = dataLayer.GetReportHeaderData();
            _ = ctf.CreateOutFile(reports, headerData, stringWriter, out var failedReports, out var hasWarnings);

            if (expectedFailingReports.Any() || expectedFailingReports != string.Empty)
            {
                var expectedFailReportsArray = expectedFailingReports.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                failedReports.Should().Contain(expectedFailReportsArray);
            }
            //hasWarnings.Should().BeFalse();


            var expectedOutputPath = GetTestFile(expectedOutputFileName);
            var expectedOutput = File.OpenText(expectedOutputPath).ReadToEnd();

            var diff = InlineDiffBuilder.Diff(expectedOutput, stringWriter.ToString());

            var changedLines = diff.Lines
                .Where((x) =>
                {
                    return x.Type != DiffPlex.DiffBuilder.Model.ChangeType.Unchanged
                        && !x.Text.StartsWith("SALENAME:")
                        && !x.Text.StartsWith("RUN DATE")
                        && !x.Text.StartsWith("USDA FOREST SERVICE")
                        && !x.Text.StartsWith("WASHINGTON")
                        && !x.Text.StartsWith("FILENAME:");
                })
                .ToArray();

            foreach (var line in changedLines)
            {
                Output.WriteLine(line.Position.ToString().PadLeft(5) + ":" + line.Text);
            }

            changedLines.Should().HaveCount(0);



        }

        [Theory]
        [InlineData("Issues\\20383_Jiffy Stewardship_TS.crz3", "Issues\\20383_Jiffy Stewardship_TS.04.15.2023.out")]
        public void ProcessAndVerityOutput_V3(string testFileName, string expectedOutputFileName, string expectedFailingReports = "")
        {
            using var dataLayer = ProcessCruiseHelper(testFileName);

            var ctf = new CreateTextFile(dataLayer, VolumeLibraryInterop.Default, Substitute.For<ILogger<CreateTextFile>>());

            var stringWriter = new StringWriter();

            var reports = dataLayer.GetSelectedReports();
            var headerData = dataLayer.GetReportHeaderData();
            _ = ctf.CreateOutFile(reports, headerData, stringWriter, out var failedReports, out var hasWarnings);

            if (expectedFailingReports.Any() || expectedFailingReports != string.Empty)
            {
                var expectedFailReportsArray = expectedFailingReports.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                failedReports.Should().Contain(expectedFailReportsArray);
            }
            //hasWarnings.Should().BeFalse();


            var expectedOutputPath = GetTestFile(expectedOutputFileName);
            var expectedOutput = File.OpenText(expectedOutputPath).ReadToEnd();

            var resultOutput = stringWriter.ToString();
            var diff = InlineDiffBuilder.Diff(expectedOutput, resultOutput);

            var changedLines = diff.Lines
                .Where((x) =>
                {
                    return x.Type != DiffPlex.DiffBuilder.Model.ChangeType.Unchanged
                        && !x.Text.StartsWith("SALENAME:")
                        && !x.Text.StartsWith("RUN DATE")
                        && !x.Text.StartsWith("USDA FOREST SERVICE")
                        && !x.Text.StartsWith("WASHINGTON")
                        && !x.Text.StartsWith("FILENAME:");
                })
                .ToArray();

            foreach (var line in changedLines)
            {
                Output.WriteLine(line.Position.ToString().PadLeft(5) + ":" + line.Text);
            }

            changedLines.Should().HaveCount(0);



        }
    }
}
