using CruiseDAL.V2.Models;
using CruiseDAL;
using System.Linq;

namespace CruiseProcessing.Test
{
    public class DatabaseInitializer_V2
    {
        public string[] Units { get; set; }
        public Stratum[] Strata { get; set; }
        public (string UnitCode, string StCode)[] UnitStrata { get; set; }
        public (string SgCode, string StCode, int Freq)[] SampleGroups { get; set; }
        public TreeDefaultValue[] TreeDefaults { get; set; }
        public Stratum[] PlotStrata { get; set; }
        public Stratum[] NonPlotStrata { get; set; }

        public DatabaseInitializer_V2()
        {
            var units = Units = new string[] { "u1", "u2" };

            var plotStrata = PlotStrata = new[]
            {
                new Stratum{ Code = "st1", Method = "PNT" },
                new Stratum{ Code = "st2", Method = "PCM" },
            };

            var nonPlotStrata = NonPlotStrata = new[]
            {
                new Stratum{ Code = "st3", Method = "STR" },
                new Stratum{ Code = "st4", Method = "STR" },
            };

            var strata = Strata = plotStrata.Concat(nonPlotStrata).ToArray();

            UnitStrata = new[]
            {
                (UnitCode: units[0], StCode: plotStrata[0].Code ),
                (UnitCode: units[0], StCode: plotStrata[1].Code),
                (UnitCode: units[1], StCode: plotStrata[1].Code),

                (UnitCode: units[0], StCode: nonPlotStrata[0].Code ),
                (UnitCode: units[0], StCode: nonPlotStrata[1].Code),
                (UnitCode: units[1], StCode: nonPlotStrata[1].Code),
            };

            var sampleGroups = SampleGroups = new[]
            {
                (SgCode: "sg1", StCode: plotStrata[0].Code, Freqy: 101),
                (SgCode: "sg2", StCode: plotStrata[1].Code, Freqy: 102),

                (SgCode: "sg1", StCode: nonPlotStrata[0].Code, Freqy: 101),
                (SgCode: "sg2", StCode: nonPlotStrata[1].Code, Freqy: 102),
            };

            TreeDefaults = new[]
            {
                new TreeDefaultValue {Species = "sp1", PrimaryProduct = "01", LiveDead = "L"},
                new TreeDefaultValue {Species = "sp2", PrimaryProduct = "01", LiveDead = "L"},
                new TreeDefaultValue {Species = "sp3", PrimaryProduct = "01", LiveDead = "L"},
            };
        }

        public DAL CreateDatabase()
        {
            var units = Units;

            var strata = Strata;

            var unitStrata = UnitStrata;

            var sampleGroups = SampleGroups;

            var tdvs = TreeDefaults;

            var database = new DAL();

            InitializeDatabase(database, units, strata, unitStrata, sampleGroups, tdvs);

            return database;
        }

        public DAL CreateDatabaseFile(string path)
        {
            var units = Units;

            var strata = Strata;

            var unitStrata = UnitStrata;

            var sampleGroups = SampleGroups;

            var tdvs = TreeDefaults;

            var database = new DAL(path, true);

            InitializeDatabase(database, units, strata, unitStrata, sampleGroups, tdvs);

            return database;
        }

        public void InitializeDatabase(DAL db,
            string[] units,
            CruiseDAL.V2.Models.Stratum[] strata,
            (string UnitCode, string StCode)[] unitStrata,
            (string SgCode, string StCode, int Freq)[] sampleGroups,
            CruiseDAL.V2.Models.TreeDefaultValue[] tdvs)
        {
            //Cutting Units
            foreach (var unit in units.OrEmpty())
            {
                db.Execute(
                    "INSERT INTO CuttingUnit (" +
                    "Code, Area" +
                    ") VALUES " +
                    $"('{unit}', 101);");
            }

            //Strata
            foreach (var st in strata.OrEmpty())
            {
                db.Insert(st);
            }

            //Unit - Strata
            foreach (var i in unitStrata.OrEmpty())
            {
                db.Execute("INSERT INTO CuttingUnitStratum (CuttingUnit_CN, Stratum_CN)" +
                    "VALUES (" +
                    "(SELECT CuttingUnit_CN FROM CuttingUnit WHERE Code = @p1), " +
                    "(SELECT Stratum_CN FROM Stratum WHERE Code = @p2));", i.UnitCode, i.StCode);
            }

            //Sample Groups
            foreach (var sg in sampleGroups.OrEmpty())
            {
                db.Execute("INSERT INTO SampleGroup (" +
                    "Code, Stratum_CN, SamplingFrequency, PrimaryProduct, CutLeave, UOM" +
                    ") VALUES (" +
                    "@p1, " +
                    "(SELECT Stratum_CN FROM Stratum WHERE Code = @p2), " +
                    "@p3, " +
                    "'01', " +
                    "'C', " +
                    "'01' " +
                    ");", sg.SgCode, sg.StCode, sg.Freq);
            }

            foreach (var tdv in tdvs.OrEmpty())
            {
                db.Insert(tdv);
            }
        }
    }
}