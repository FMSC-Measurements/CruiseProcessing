using Bogus;
using CruiseDAL.V3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseDAL.TestCommon.V3
{
    public class TemplateDatabaseInitializer
    {
        public string CruiseID { get; set; }
        public string SaleID { get; set; }
        public string CreatedBy { get; set; }
        public string[] Species { get; set; }

        public (string field, string heading)[] TreeFieldHeadings { get; set; }

        public (string field, string heading)[] LogFieldHeadings { get; set; }

        public string[] StratumTemplates { get; set; }

        public TreeDefaultValue[] TreeDefaults { get; set; }

        public TreeAuditRule[] TreeAuditRules { get; set; }

        public LogGradeAuditRule[] LogGradeAuditRules { get; set; }

        public Reports[] Reports { get; set; }

        public VolumeEquation[] VolumeEquations { get; set; }

        public ValueEquation[] ValueEquations { get; set; }

        public BiomassEquation[] BiomassEquations { get; set; }

        public TemplateDatabaseInitializer()
        {
            CruiseID = Guid.NewGuid().ToString();
            SaleID = Guid.NewGuid().ToString();

            CreatedBy = nameof(TemplateDatabaseInitializer);

            Species = new[] { "sp1", "sp2", "sp3" };

            TreeFieldHeadings = new[] 
            {
                (nameof(TreeMeasurment.DBH), "MyDBH"),
                //(nameof(TreeMeasurment.Aspect), null),
            };

            LogFieldHeadings = new[]
            {
                (nameof(Log.DIBClass), "MyDibClass"),
                //(nameof(Log.Grade), null),
            };

            StratumTemplates = new[] { "StTemplate1", "StTemplate2", };

            TreeDefaults = new TreeDefaultValue[] 
            { 
                new TreeDefaultValue {SpeciesCode = "sp1", PrimaryProduct = "01", },
                new TreeDefaultValue {SpeciesCode = null, PrimaryProduct = "01", },
                new TreeDefaultValue {SpeciesCode = "sp1", PrimaryProduct = null, },
                new TreeDefaultValue {SpeciesCode = null, PrimaryProduct = null, },
            };

            TreeAuditRules = new[]
            {
                new TreeAuditRule { Field = nameof(TreeMeasurment.DBH), Min = 0, Max = 100, },
                new TreeAuditRule { Field = nameof(TreeMeasurment.TotalHeight), Min = 0, Max = 100, },
            };

            LogGradeAuditRules = new[]
            {
                new LogGradeAuditRule {Grade = "1", DefectMax = 101.0 },
                new LogGradeAuditRule {Grade = "2", DefectMax = 102.0 },
            };

            Reports = new[]
            {
                new Reports {ReportID = "A1", Selected =  true},
                new Reports {ReportID = "A2", Selected =  true},
            };

            VolumeEquations = new[]
            {
                new VolumeEquation {Species = "sp1", PrimaryProduct = "01", VolumeEquationNumber = "1234", },
                new VolumeEquation {Species = "sp2", PrimaryProduct = "01", VolumeEquationNumber = "1234", },
            };

            ValueEquations = new[]
            {
                new ValueEquation {Species = "sp1", PrimaryProduct = "01", },
                new ValueEquation {Species = "sp2", PrimaryProduct = "01", },
            };

            BiomassEquations = new[]
            {
                new BiomassEquation {Species = "sp1", Product = "01", Component = "something", LiveDead = "L", },
                new BiomassEquation {Species = "sp2", Product = "01", Component = "something", LiveDead = "L", },
            };

        }

        public CruiseDatastore_V3 CreateDatabase()
        {
            var database = new CruiseDatastore_V3();
            InitializeDatabase(database, this);
            return database;
        }

        public CruiseDatastore_V3 CreateDatabaseFile(string path)
        {
            var database = new CruiseDatastore_V3(path, true);
            InitializeDatabase(database, this);
            return database;
        }

        public static void InitializeDatabase(CruiseDatastore db, TemplateDatabaseInitializer init)
        {
            var cruiseID = init.CruiseID;

            var insertOverrides = new Dictionary<string, object>()
            {
                { "CreatedBy", init.CreatedBy },
            };

            var sale = new Sale()
            {
                SaleID = init.SaleID,
                SaleNumber = "00000",
                CreatedBy = init.CreatedBy,
            };
            db.Insert(sale);

            var cruise = new Cruise()
            {
                CruiseID = cruiseID,
                SaleID = init.SaleID,
                CruiseNumber = "00000",
                SaleNumber = "00000",
                CreatedBy = init.CreatedBy,
            };
            db.Insert(cruise);

            foreach (var sp in init.Species)
            {
                db.Insert(new Species { CruiseID = cruiseID, SpeciesCode = sp, });

                db.Insert(new Species_Product { CruiseID = cruiseID, SpeciesCode = sp, PrimaryProduct = null, ContractSpecies = "123" });
                db.Insert(new Species_Product { CruiseID = cruiseID, SpeciesCode = sp, PrimaryProduct = "01", ContractSpecies = "234" });
            }

            foreach (var (field, heading) in init.TreeFieldHeadings)
            {
                db.Insert(new TreeFieldHeading { CruiseID = cruiseID, Field = field, Heading = heading, CreatedBy = init.CreatedBy, });
            }

            foreach (var (field, heading) in init.LogFieldHeadings)
            {
                db.Insert(new LogFieldHeading { CruiseID = cruiseID, Field = field, Heading = heading, CreatedBy = init.CreatedBy, });
            }

            foreach (var stTemp in init.StratumTemplates)
            {
                db.Insert(new StratumTemplate { CruiseID = cruiseID, StratumTemplateName = stTemp, CreatedBy = init.CreatedBy, });
            }

            foreach (var tdv in init.TreeDefaults)
            {
                tdv.CruiseID = cruiseID;
                tdv.CreatedBy = init.CreatedBy;
                db.Insert(tdv);
            }

            foreach (var tar in init.TreeAuditRules)
            {
                tar.CruiseID = cruiseID;
                tar.TreeAuditRuleID = Guid.NewGuid().ToString();
                db.Insert(tar);

                db.Insert(new TreeAuditRuleSelector { CruiseID = cruiseID, TreeAuditRuleID = tar.TreeAuditRuleID, SpeciesCode = null, PrimaryProduct = null });
                db.Insert(new TreeAuditRuleSelector { CruiseID = cruiseID, TreeAuditRuleID = tar.TreeAuditRuleID, SpeciesCode = null, PrimaryProduct = "01" });
                db.Insert(new TreeAuditRuleSelector { CruiseID = cruiseID, TreeAuditRuleID = tar.TreeAuditRuleID, SpeciesCode = init.Species[0], PrimaryProduct = null });
            }

            foreach (var lgar in init.LogGradeAuditRules)
            {
                lgar.CruiseID = cruiseID;
                db.Insert(lgar);
            }

            foreach (var report in init.Reports)
            {
                report.CruiseID = cruiseID;
                report.CreatedBy = init.CreatedBy;
                db.Insert(report);
            }

            foreach (var volEq in init.VolumeEquations)
            {
                volEq.CruiseID = cruiseID;
                volEq.CreatedBy = init.CreatedBy;
                db.Insert(volEq);
            }

            foreach (var valEq in init.ValueEquations)
            {
                valEq.CruiseID = cruiseID;
                valEq.CreatedBy = init.CreatedBy;
                db.Insert(valEq);
            }

            foreach (var bioEq in init.BiomassEquations)
            {
                bioEq.CruiseID = cruiseID;
                bioEq.CreatedBy = init.CreatedBy;
                db.Insert(bioEq);
            }
        }
    }
}
