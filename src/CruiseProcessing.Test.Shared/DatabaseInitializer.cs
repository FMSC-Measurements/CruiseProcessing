using CruiseDAL;
using CruiseDAL.V3.Models;
using System;
using System.Drawing;
using System.Linq;

namespace CruiseProcessing.Test
{
    public class DatabaseInitializer
    {
        public string CruiseID { get; set; }
        public string SaleID { get; set; }
        public string SaleNumber { get; set; }
        public string Region { get; set; }
        public string[] Units { get; set; }
        public Stratum[] Strata { get; set; }
        public CuttingUnit_Stratum[] UnitStrata { get; set; }
        public string[] Species { get; set; }
        public string[][] SpProds { get; set; }
        public SampleGroup[] SampleGroups { get; set; }
        public TreeDefaultValue[] TreeDefaults { get; set; }
        public SubPopulation[] Subpops { get; set; }
        public Stratum[] PlotStrata { get; set; }
        public Stratum[] NonPlotStrata { get; set; }

        public DatabaseInitializer(bool includeDesign = true)
        {
            CruiseID = Guid.NewGuid().ToString();
            var saleID = SaleID = Guid.NewGuid().ToString();
            SaleNumber = (saleID.GetHashCode() % 10000).ToString();
            Region = "01";

            if (includeDesign)
            {

                var units = Units = new string[] { "u1", "u2" };

                var plotStrata = PlotStrata = new[]
                {
                    new Stratum{ StratumCode = "st1", Method = "PNT" },
                    new Stratum{ StratumCode = "st2", Method = "PCM" },
                };

                var nonPlotStrata = NonPlotStrata = new[]
                {
                    new Stratum{ StratumCode = "st3", Method = "STR" },
                    new Stratum{ StratumCode = "st4", Method = "STR" },
                };

                var strata = Strata = plotStrata.Concat(nonPlotStrata).ToArray();

                UnitStrata = new[]
                {
                    new CuttingUnit_Stratum {CuttingUnitCode = units[0], StratumCode = plotStrata[0].StratumCode },
                    new CuttingUnit_Stratum {CuttingUnitCode = units[0], StratumCode = plotStrata[1].StratumCode},
                    new CuttingUnit_Stratum {CuttingUnitCode = units[1], StratumCode = plotStrata[1].StratumCode},

                    new CuttingUnit_Stratum {CuttingUnitCode = units[0], StratumCode = nonPlotStrata[0].StratumCode },
                    new CuttingUnit_Stratum {CuttingUnitCode = units[0], StratumCode = nonPlotStrata[1].StratumCode},
                    new CuttingUnit_Stratum {CuttingUnitCode = units[1], StratumCode = nonPlotStrata[1].StratumCode},
                };

                var species = Species = new string[] { "sp1", "sp2", "sp3" };

                var spProds = SpProds = new string[][] {
                    new[] { "sp1", "sp1" },
                    new[] { "sp2", "sp2" },
                    new[] { "sp3", "sp3" }
                };

                var sampleGroups = SampleGroups = new[]
                {
                    new SampleGroup {SampleGroupCode = "sg1", StratumCode = plotStrata[0].StratumCode, SamplingFrequency = 101, TallyBySubPop = true},
                    new SampleGroup {SampleGroupCode = "sg2", StratumCode = plotStrata[1].StratumCode, SamplingFrequency = 102, TallyBySubPop = false},

                    new SampleGroup {SampleGroupCode = "sg1", StratumCode = nonPlotStrata[0].StratumCode, SamplingFrequency = 101, TallyBySubPop = true},
                    new SampleGroup {SampleGroupCode = "sg2", StratumCode = nonPlotStrata[1].StratumCode, SamplingFrequency = 102, TallyBySubPop = false},
                };

                TreeDefaults = new[]
                {
                    new TreeDefaultValue {SpeciesCode = species[0], PrimaryProduct = "01"},
                    new TreeDefaultValue {SpeciesCode = species[1], PrimaryProduct = "01"},
                    new TreeDefaultValue {SpeciesCode = species[2], PrimaryProduct = "01"},
                };

                Subpops = new[]
                {
                    new SubPopulation {
                        StratumCode = sampleGroups[0].StratumCode,
                        SampleGroupCode = sampleGroups[0].SampleGroupCode,
                        SpeciesCode = species[0],
                        LiveDead = "L",
                    },
                    new SubPopulation {
                        StratumCode = sampleGroups[0].StratumCode,
                        SampleGroupCode = sampleGroups[0].SampleGroupCode,
                        SpeciesCode = species[0],
                        LiveDead = "D",
                    },
                    new SubPopulation {
                        StratumCode = sampleGroups[0].StratumCode,
                        SampleGroupCode = sampleGroups[0].SampleGroupCode,
                        SpeciesCode = species[1],
                        LiveDead = "L",
                    },
                    new SubPopulation {
                        StratumCode = sampleGroups[0].StratumCode,
                        SampleGroupCode = sampleGroups[0].SampleGroupCode,
                        SpeciesCode = species[2],
                        LiveDead = "L",
                    },

                    // plot strata
                    new SubPopulation {
                        StratumCode = sampleGroups[2].StratumCode,
                        SampleGroupCode = sampleGroups[2].SampleGroupCode,
                        SpeciesCode = species[0],
                        LiveDead = "L",
                    },
                    new SubPopulation {
                        StratumCode = sampleGroups[2].StratumCode,
                        SampleGroupCode = sampleGroups[2].SampleGroupCode,
                        SpeciesCode = species[1],
                        LiveDead = "L",
                    },
                    new SubPopulation {
                        StratumCode = sampleGroups[2].StratumCode,
                        SampleGroupCode = sampleGroups[2].SampleGroupCode,
                        SpeciesCode = species[2],
                        LiveDead = "L",
                    },
                };
            }
        }

        public CruiseDatastore_V3 CreateDatabase(string cruiseID = null, string saleID = null, string saleNumber = null)
        {
            var database = new CruiseDatastore_V3();

            PopulateDatabase(database, cruiseID, saleID, saleNumber);

            return database;
        }

        public CruiseDatastore_V3 CreateDatabaseWithAllTables(string cruiseID = null, string saleID = null, string saleNumber = null)
        {
            var database = new CruiseDatastore_V3();

            PopulateDatabaseWithAllTables(database, cruiseID, saleID, saleNumber);

            return database;
        }

        public CruiseDatastore_V3 CreateDatabaseFileWithAllTables(string path, string cruiseID = null, string saleID = null, string saleNumber = null)
        {
            var database = new CruiseDatastore_V3(path, true);

            PopulateDatabaseWithAllTables(database, cruiseID, saleID, saleNumber);

            return database;
        }

        public CruiseDatastore_V3 CreateDatabaseFile(string path, string cruiseID = null, string saleID = null, string saleNumber = null)
        {
            var database = new CruiseDatastore_V3(path, true);

            PopulateDatabase(database, cruiseID, saleID, saleNumber);

            return database;
        }

        public void PopulateDatabase(CruiseDatastore database, string cruiseID = null, string saleID = null, string saleNumber = null)
        {
            PopulateCruiseInfo(database, cruiseID, saleID, saleNumber);
            PopulateDesign(database, cruiseID: cruiseID);
        }

        public void PopulateDatabaseWithAllTables(CruiseDatastore database, string cruiseID = null, string saleID = null, string saleNumber = null)
        {
            PopulateCruiseInfo(database, cruiseID, saleID, saleNumber);
            PopulateDesign(database, cruiseID: cruiseID);
            PopulateTemplateInfo(database, cruiseID: cruiseID);

            PopulatePlotFieldData(database, cruiseID: cruiseID);
            PopulateStrFieldData(database, cruiseID: cruiseID);
        }

        public void PopulateCruiseInfo(CruiseDatastore db, string cruiseID = null, string saleID = null, string saleNumber = null)
        {
            cruiseID = cruiseID ?? CruiseID;
            saleID = saleID ?? SaleID;
            saleNumber = saleNumber ?? SaleNumber;

            db.Insert(new Sale()
            {
                SaleID = SaleID,
                SaleNumber = saleNumber,
                Region = Region,
                Forest = "01",
                District = "01",
            });

            db.Insert(new Cruise()
            {
                CruiseID = cruiseID,
                SaleID = saleID,
                CruiseNumber = saleNumber,
                SaleNumber = saleNumber,
                DefaultUOM = "01",
            });
        }

        public void PopulateDesign(CruiseDatastore db, string cruiseID = null)
        {
            cruiseID ??= CruiseID;

            //Cutting Units
            foreach (var unit in Units.OrEmpty())
            {
                var unitID = Guid.NewGuid().ToString();
                db.Execute(
                    "INSERT INTO CuttingUnit (" +
                    "CruiseID, CuttingUnitID, CuttingUnitCode" +
                    ") VALUES " +
                    $"('{cruiseID}', '{unitID}', '{unit}');");
            }

            //Strata
            foreach (var st in Strata.OrEmpty())
            {
                st.CruiseID = cruiseID;
                st.StratumID = Guid.NewGuid().ToString();
                db.Insert(st);
            }

            //Unit - Strata
            foreach (var cust in UnitStrata.OrEmpty())
            {
                cust.CruiseID = cruiseID;
                db.Insert(cust);
            }

            //Sample Groups
            foreach (var sg in SampleGroups.OrEmpty())
            {
                sg.SampleGroupID = Guid.NewGuid().ToString();
                sg.CruiseID = cruiseID;
                sg.PrimaryProduct ??= "01";
                db.Insert(sg);
            }

            foreach (var sp in Species.OrEmpty())
            {
                db.Execute($"INSERT INTO Species (CruiseID, SpeciesCode) VALUES ('{cruiseID}', '{sp}');");
            }

            foreach (var spProd in SpProds.OrEmpty())
            {
                db.Insert(new Species_Product { CruiseID = cruiseID, SpeciesCode = spProd[0], ContractSpecies = spProd[1] });
            }

            foreach (var tdv in TreeDefaults.OrEmpty())
            {
                tdv.CruiseID = cruiseID;
                db.Insert(tdv);
            }

            foreach (var sub in Subpops.OrEmpty())
            {
                sub.SubPopulationID = Guid.NewGuid().ToString();
                sub.CruiseID = cruiseID;
                db.Insert(sub);
            }
        }

        public void PopulateTemplateInfo(CruiseDatastore db, string cruiseID = null, bool fieldSetup = true, bool auditRules = true)
        {
            cruiseID ??= CruiseID;

            foreach (var st in Strata)
            {
                var treeFieldSetup = new TreeFieldSetup()
                {
                    CruiseID = cruiseID,
                    StratumCode = st.StratumCode,
                    Field = "DBH"
                };
                db.Insert(treeFieldSetup);

                var logFileSetup = new LogFieldSetup()
                {
                    CruiseID = cruiseID,
                    StratumCode = st.StratumCode,
                    Field = "Grade",
                };
                db.Insert(logFileSetup);
            }

            var tar = new TreeAuditRule()
            {
                CruiseID = cruiseID,
                TreeAuditRuleID = Guid.NewGuid().ToString(),
                Description = "something",
                Field = "DBH",
                Max = 100.0,
            };
            db.Insert(tar);

            var tars = new TreeAuditRuleSelector()
            {
                CruiseID = cruiseID,
                TreeAuditRuleID = tar.TreeAuditRuleID,

            };
            db.Insert(tars);

            var lgar = new LogGradeAuditRule()
            {
                CruiseID = cruiseID,
                Grade = "01",
                DefectMax = 1,
                SpeciesCode = Species.First(),
            };
            db.Insert(lgar);

            var volumeEquation = new VolumeEquation()
            {
                CruiseID = cruiseID,
                VolumeEquationNumber = "something",
                Species = Species.First(),
                PrimaryProduct = "01",
            };
            db.Insert(volumeEquation);

            var report = new Reports()
            {
                CruiseID = cruiseID,
                ReportID = Guid.NewGuid().ToString(),
            };
            db.Insert(report);

            var stratumTemplate = new StratumTemplate()
            {
                CruiseID = cruiseID,
                StratumTemplateName = "something",
            };
            db.Insert(stratumTemplate);

            var sttfs = new StratumTemplateTreeFieldSetup()
            {
                CruiseID = cruiseID,
                StratumTemplateName = stratumTemplate.StratumTemplateName,
                Field = "DBH",
            };
            db.Insert(sttfs);

            var stlfs = new StratumTemplateLogFieldSetup()
            {
                CruiseID = cruiseID,
                StratumTemplateName = stratumTemplate.StratumTemplateName,
                Field = "Grade",
            };
            db.Insert(stlfs);
        }

        public void PopulatePlotFieldData(CruiseDatastore db, string cruiseID = null, int numTreesPerPlot = 1)
        {
            var plotStrataCodes = PlotStrata.Select(x => x.StratumCode).ToArray();

            var unitStrata = UnitStrata.Where(x => plotStrataCodes.Contains(x.StratumCode))
                .OrderBy(x => x.CuttingUnitCode)
                .ToArray();

            foreach (var uSt in unitStrata)
            {
                var stratumCode = uSt.StratumCode;
                var unit = uSt.CuttingUnitCode;

                var plotNumber = 1;
                var plot = new Plot()
                {
                    CruiseID = cruiseID,
                    CuttingUnitCode = unit,
                    PlotID = Guid.NewGuid().ToString(),
                    PlotNumber = plotNumber,
                };
                db.Insert(plot);

                var plotLocation = new PlotLocation()
                {
                    PlotID = plot.PlotID,
                    Latitude = 1.1,
                    Longitude = 2.2,
                };
                db.Insert(plotLocation);


                var plotStratum = new Plot_Stratum()
                {
                    CruiseID = cruiseID,
                    CuttingUnitCode = unit,
                    PlotNumber = plotNumber,
                    StratumCode = stratumCode,
                };
                db.Insert(plotStratum);

                var speciesCode = Species.First();
                var sampleGroupCode = SampleGroups.First().SampleGroupCode;

                foreach (var treeNum in Enumerable.Range(1, numTreesPerPlot))
                {
                    var plotTreeNumber = 1;
                    var plotTree = new Tree()
                    {
                        CruiseID = cruiseID,
                        CuttingUnitCode = unit,
                        PlotNumber = plotNumber,
                        TreeID = Guid.NewGuid().ToString(),
                        TreeNumber = treeNum,
                        StratumCode = stratumCode,
                        SampleGroupCode = sampleGroupCode,
                        SpeciesCode = speciesCode,
                    };
                    db.Insert(plotTree);
                }

            }
        }

        public void PopulateStrFieldData(CruiseDatastore db, string cruiseID, int numTreesPerUnit = 1)
        {
            var strStratumCodes = Strata.Where(x => x.Method == "STR").Select(x => x.StratumCode).ToArray();

            var unitStrata = UnitStrata.Where(x => strStratumCodes.Contains(x.StratumCode))
                .OrderBy(x => x.CuttingUnitCode)
                .ToArray();

            var tar = db.From<TreeAuditRule>().Where("CruiseID = @p1").Query(cruiseID).FirstOrDefault();

            foreach (var uSt in unitStrata)
            {
                var statumCode = uSt.StratumCode;
                var unit = uSt.CuttingUnitCode;

                foreach(var treeNumber in Enumerable.Range(0, numTreesPerUnit))
                {
                    var tree = new Tree()
                    {
                        CuttingUnitCode = unit,
                        TreeNumber = treeNumber,
                        StratumCode = statumCode,
                        SampleGroupCode = "sg1",
                        SpeciesCode = "sp1",
                    };

                    AddTreeRecord(db, tree);

                    if (tar != null)
                    {
                        var tares = new TreeAuditResolution()
                        {
                            CruiseID = cruiseID,
                            TreeAuditRuleID = tar.TreeAuditRuleID,
                            TreeID = tree.TreeID,
                            Initials = "so",
                        };
                        db.Insert(tares);
                    }
                }
            }

            
        }

        public void AddTreeRecord(CruiseDatastore db, Tree tree)
        {
            var cruiseID = CruiseID;

            tree.CruiseID = cruiseID;
            if(tree.TreeID == null)
            {
                tree.TreeID = Guid.NewGuid().ToString();
            }
            db.Insert(tree);

            var tallyLedger = new TallyLedger()
            {
                CruiseID = cruiseID,
                TallyLedgerID = Guid.NewGuid().ToString(),
                CuttingUnitCode = tree.CuttingUnitCode,
                StratumCode = tree.StratumCode,
                SampleGroupCode = tree.SampleGroupCode,
                SpeciesCode = tree.SpeciesCode,
                TreeID = tree.TreeID,
                TreeCount = 1,
            };
            db.Insert(tallyLedger);

            var treeLocation = new TreeLocation()
            {
                TreeID = tree.TreeID,
                Latitude = 1.1,
                Longitude = 2.2,
            };
            db.Insert(treeLocation);

            var treeMeasurment = new TreeMeasurment()
            {
                TreeID = tree.TreeID,
            };
            db.Insert(treeMeasurment);

            var logNumber = "1";
            var log = new Log()
            {
                CruiseID = cruiseID,
                TreeID = tree.TreeID,
                LogID = Guid.NewGuid().ToString(),
                LogNumber = logNumber,
            };
            db.Insert(log);

            var stem = new Stem
            {
                CruiseID = cruiseID,
                TreeID = tree.TreeID,
                StemID = Guid.NewGuid().ToString(),
                Diameter = 1.1,
            };
            db.Insert(stem);

            
        }

    }
}