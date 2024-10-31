using CruiseDAL.DataObjects;
using CruiseProcessing.Config;
using CruiseProcessing.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CruiseProcessing.Data
{


    public interface IBiomassEquationService
    {
        BiomassValue GetPrimaryWeightFactorAndMoistureContent(string species, string product, string liveDead);

        float? GetSecondaryWeightFactor(string species, string product, string liveDead);

        float GetPrecentRemoved(string species, string product);

        IReadOnlyCollection<BiomassEquationDO> CreateBiomassEquations(IReadOnlyCollection<VolumeEquationDO> volEqs, int region, string forest, IReadOnlyCollection<PercentRemoved> percentRemovedValues);

        IReadOnlyCollection<BiomassEquationDO> CreateBiomassEquations(VolumeEquationDO volEq, IEnumerable<TreeDefaultValueDO> treeDefaults, int region, string forest, float? percentRemovedValue);
    }

    public partial class CpDataLayer : IBiomassEquationService
    {


        private static readonly IReadOnlyCollection<int> R5_Prod20_WF_FIAcodes = new int[] { 122, 116, 117, 015, 020, 202, 081, 108 };
        private const int CRZSPDFTCS_STRINGLENGTH = 256;
        public const string PRIMARY_PRODUCT_COMPONENT = "PrimaryProd";
        public const string SECONDARY_PRODUCT_COMPONENT = "SecondaryProd";
        public static readonly ReadOnlyCollection<string> BIOMASS_COMPONENTS = Array.AsReadOnly(new[]
        {
            "TotalTreeAboveGround",
            "LiveBranches",
            "DeadBranches",
            "Foliage",
            PRIMARY_PRODUCT_COMPONENT,
            SECONDARY_PRODUCT_COMPONENT,
            "StemTip"
        });

        public BiomassEquationOptions BiomassOptions { get; }

        protected Dictionary<(string species, string product), IReadOnlyCollection<BiomassEquationDO>> PrimaryProdValueCache { get; }
            = new Dictionary<(string species, string product), IReadOnlyCollection<BiomassEquationDO>>();

        protected Dictionary<(string species, string product), IReadOnlyCollection<BiomassValue>> SecondaryWeightFactorCache { get; }
            = new Dictionary<(string species, string product), IReadOnlyCollection<BiomassValue>>();

        protected Dictionary<(string species, string product), float> PrecentRemovedCache { get; }
            = new Dictionary<(string species, string product), float>();

        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CRZSPDFTCS(ref int regn, StringBuilder forst, out int spcd, float[] wf, StringBuilder agteq, StringBuilder lbreq,
            StringBuilder dbreq, StringBuilder foleq, StringBuilder tipeq, StringBuilder wf1ref, StringBuilder wf2ref, StringBuilder mcref,
            StringBuilder agtref, StringBuilder lbrref, StringBuilder dbrref, StringBuilder folref, StringBuilder tipref,
            int i1, int i2, int i3, int i4, int i5, int i6, int i7, int i8, int i9, int i10, int i11, int i12, int i13, int i14);

        // primarily used to reset cache before processing
        // cache is intend to stay live until generating output
        public void ResetBioMassEquationCache()
        {
            PrimaryProdValueCache.Clear();
            SecondaryWeightFactorCache.Clear();
            PrecentRemovedCache.Clear();
        }


        public BiomassValue GetPrimaryWeightFactorAndMoistureContent(string species, string product, string liveDead)
        {

            // try to get cached bimass value, this will include any values that we
            // needed to go to the Volume Library to lookup. 
            if (PrimaryProdValueCache.TryGetValue((species, product), out var bioEqs))
            {
                var primaryProdEq = bioEqs.FirstOrDefault(beq => beq.LiveDead == liveDead)
                    ?? bioEqs.FirstOrDefault();

                if (primaryProdEq != null)
                {
                    return new BiomassValue { WeightFactor = primaryProdEq.WeightFactorPrimary, MoistureContent = primaryProdEq.PercentMoisture };
                }
            }


            // get biomass equations from the database
            // and cache them
            bioEqs = GetBiomassEquations(species, product).Where(beq => beq.Component == PRIMARY_PRODUCT_COMPONENT).ToArray();
            if (bioEqs != null && bioEqs.Any())
            {
                var primaryProdEq = bioEqs.FirstOrDefault(beq => beq.LiveDead == liveDead)
                    ?? bioEqs.FirstOrDefault();

                if (primaryProdEq != null)
                {
                    PrimaryProdValueCache.Add((species, product), bioEqs);

                    return new BiomassValue { WeightFactor = primaryProdEq.WeightFactorPrimary, MoistureContent = primaryProdEq.PercentMoisture };
                }
            }

            // if we can't get the biomass equations from the database
            // try to get them from the volume library
            var sale = GetSale();
            var treeDefaults = GetTreeDefaultValues(species, product);
            if (sale == null || treeDefaults == null || !treeDefaults.Any())
            {
                return null;
            }

            var regionNumber = int.Parse(sale.Region);
            bioEqs = MakeBiomassEquationsInternal(species, product, treeDefaults, regionNumber, sale.Forest, BiomassOptions.DefaultPercentRemoved)
                .Where(beq => beq.Component == PRIMARY_PRODUCT_COMPONENT).ToArray();

            if (bioEqs.Any())
            {
                var primaryProdEq = bioEqs.FirstOrDefault(beq => beq.LiveDead == liveDead)
                        ?? bioEqs.FirstOrDefault();

                if (primaryProdEq != null)
                {
                    PrimaryProdValueCache.Add((species, product), bioEqs);

                    Log.LogInformation($"Created Primary Biomass Data For Sp:{species} Prod:{product} WF:{primaryProdEq.WeightFactorPrimary} PM:{primaryProdEq.PercentMoisture}");
                    LogError("-", 0, ERROR_LEVEL_INFO, $"Created Primary Biomass Data For Sp:{species} Prod:{product} WF:{primaryProdEq.WeightFactorPrimary} PM:{primaryProdEq.PercentMoisture}", Guid.NewGuid().ToString());

                    return new BiomassValue { WeightFactor = primaryProdEq.WeightFactorPrimary, MoistureContent = primaryProdEq.PercentMoisture };
                }
            }
            return null;
        }

        public float? GetSecondaryWeightFactor(string species, string product, string liveDead)
        {
            if (SecondaryWeightFactorCache.TryGetValue((species, product), out var wfValues))
            {
                var wf = wfValues.FirstOrDefault(x => x.LiveDead == liveDead)
                    ?? wfValues.FirstOrDefault();

                if ((wf != null))
                {
                    return wf.WeightFactor;
                }
            }

            var bioEqs = GetBiomassEquations(species, product)
                .Where(beq => beq.Component == SECONDARY_PRODUCT_COMPONENT);

            if (bioEqs.Any())
            {
                var wf = bioEqs.FirstOrDefault(x => x.LiveDead == liveDead)
                    ?? bioEqs.FirstOrDefault();

                if (wf != null)
                {
                    SecondaryWeightFactorCache.Add((species, product), bioEqs.Select(beq => new BiomassValue
                    {
                        Species = beq.Species,
                        Product = beq.Product,
                        LiveDead = beq.LiveDead,
                        WeightFactor = beq.WeightFactorSecondary,
                    }).ToArray());

                    return wf.WeightFactorSecondary;
                }
            }

            if (PrimaryProdValueCache.TryGetValue((species, product), out var primaryProdEqs))
            {
                var primaryProdEq = primaryProdEqs.FirstOrDefault(beq => beq.LiveDead == liveDead)
                    ?? primaryProdEqs.FirstOrDefault();

                if (primaryProdEq != null)
                {
                    SecondaryWeightFactorCache.Add((species, product), primaryProdEqs.Select(beq => new BiomassValue
                    {
                        Species = beq.Species,
                        Product = beq.Product,
                        LiveDead = beq.LiveDead,
                        WeightFactor = beq.WeightFactorPrimary,
                    }).ToArray());

                    return primaryProdEq.WeightFactorPrimary;
                }
            }

            var sale = GetSale();
            var treeDefaults = GetTreeDefaultValues(species, product);
            if (sale == null || treeDefaults == null || !treeDefaults.Any())
            {
                return null;
            }

            var regionNumber = int.Parse(sale.Region);
            bioEqs = MakeBiomassEquationsInternal(species, product, treeDefaults, regionNumber, sale.Forest, BiomassOptions.DefaultPercentRemoved)
                .Where(beq => beq.Component == SECONDARY_PRODUCT_COMPONENT).ToArray();

            if (bioEqs.Any())
            {


                var wf = bioEqs.FirstOrDefault(x => x.LiveDead == liveDead)
                    ?? bioEqs.FirstOrDefault();

                if (wf != null)
                {
                    SecondaryWeightFactorCache.Add((species, product), bioEqs.Select(beq => new BiomassValue
                    {
                        Species = beq.Species,
                        Product = beq.Product,
                        LiveDead = beq.LiveDead,
                        WeightFactor = beq.WeightFactorSecondary,
                    }).ToArray());

                    Log.LogInformation($"Created Secondary Biomass Data For Sp:{species} Prod:{product} WF:{wf.WeightFactorPrimary} PM:{wf.PercentMoisture}");
                    LogError("-", 0, ERROR_LEVEL_INFO, $"Created Secondary Biomass Data For Sp:{species} Prod:{product} WF:{wf.WeightFactorPrimary} PM:{wf.PercentMoisture}", Guid.NewGuid().ToString());

                    return wf.WeightFactorSecondary;
                }
            }

            return null;
        }

        public float GetPrecentRemoved(string species, string product)
        {
            if (PrecentRemovedCache.TryGetValue((species, product), out var pr))
            {
                return pr;
            }

            var bioEq = GetBiomassEquations(species, product)
                .FirstOrDefault(x => x.PercentRemoved > 0);
            if (bioEq != null)
            {
                PrecentRemovedCache.Add((species, product), bioEq.PercentRemoved);

                return bioEq.PercentRemoved;
            }

            pr = BiomassOptions.DefaultPercentRemoved;
            PrecentRemovedCache.Add((species, product), pr);

            Log.LogInformation($"Used Default Percent Removed For Sp:{species} Prod:{product} PR:{pr}");
            LogError("-", 0, ERROR_LEVEL_INFO, $"Used Default Percent Removed For Sp:{species} Prod:{product} PR:{pr}", Guid.NewGuid().ToString());

            return pr;
        }


        public IReadOnlyCollection<BiomassEquationDO> CreateBiomassEquations(IReadOnlyCollection<VolumeEquationDO> volEqs, int region, string forest, IReadOnlyCollection<PercentRemoved> percentRemovedValues)
        {
            var biomassEquations = new List<BiomassEquationDO>();

            //  Region 8 does things so differently -- there could be multiple volume equations for a species and product group.
            //  Because biomass equations are unique to species and product, group voleqs by species and product.
            //  February 2014
            foreach (VolumeEquationDO volEq in volEqs.GroupBy(x => x.Species + ";" + x.PrimaryProduct).Select(x => x.First()))
            {
                if (volEq.CalcBiomass != 1) { continue; }

                var percentRemoved = percentRemovedValues.FirstOrDefault(pr => pr.bioSpecies == volEq.Species && pr.bioProduct == volEq.PrimaryProduct);
                float? percentRemovedValue = (percentRemoved != null && float.TryParse(percentRemoved.bioPCremoved, out var pct))
                    ? pct : (float?)null;

                var treeDefaults = GetTreeDefaultValues(volEq.Species, volEq.PrimaryProduct);

                biomassEquations.AddRange(CreateBiomassEquations(volEq, treeDefaults, region, forest, percentRemovedValue));
            }

            return biomassEquations;
        }

        public IReadOnlyCollection<BiomassEquationDO> CreateBiomassEquations(VolumeEquationDO volEq, IEnumerable<TreeDefaultValueDO> treeDefaults, int region, string forest, float? percentRemovedValue)
        {
            percentRemovedValue ??= BiomassOptions.DefaultPercentRemoved;

            return MakeBiomassEquationsInternal(volEq, treeDefaults, region, forest, percentRemovedValue.Value);
        }




        public static IReadOnlyCollection<BiomassEquationDO> MakeBiomassEquationsInternal(VolumeEquationDO volEq, IEnumerable<TreeDefaultValueDO> treeDefaults, int region, string forest, float percentRemovedValue)
        {
            var biomassEquations = new List<BiomassEquationDO>();

            foreach (var tdv in treeDefaults)
            {
                int fiaCode = (int)tdv.FIAcode;
                string liveDead = tdv.LiveDead;

                biomassEquations.AddRange(MakeBiomassEquationsInternal(region, forest, fiaCode, volEq.PrimaryProduct, volEq.Species, liveDead, percentRemovedValue));
            }

            return biomassEquations;
        }

        public static IReadOnlyCollection<BiomassEquationDO> MakeBiomassEquationsInternal(string species, string primaryProduct, IEnumerable<TreeDefaultValueDO> treeDefaults, int region, string forest, float percentRemovedValue)
        {
            var biomassEquations = new List<BiomassEquationDO>();

            foreach (var tdv in treeDefaults)
            {
                int fiaCode = (int)tdv.FIAcode;
                string liveDead = tdv.LiveDead;

                biomassEquations.AddRange(MakeBiomassEquationsInternal(region, forest, fiaCode, primaryProduct, species, liveDead, percentRemovedValue));
            }

            return biomassEquations;
        }

        protected static IEnumerable<BiomassEquationDO> MakeBiomassEquationsInternal(int region, string forest, int fiaCode, string primaryProduct, string species, string liveDead, float percentRemovedValue)
        {
            float[] WF = new float[3];
            var FORST = new StringBuilder(CRZSPDFTCS_STRINGLENGTH).Append(forest);
            var AGTEQ = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var LBREQ = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var DBREQ = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var FOLEQ = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var TIPEQ = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var WF1REF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var WF2REF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var MCREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var AGTREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var LBRREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var DBRREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var FOLREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            var TIPREF = new StringBuilder(CRZSPDFTCS_STRINGLENGTH);
            int REGN = region;
            int SPCD = fiaCode;
            CRZSPDFTCS(ref REGN,
                       FORST,
                       out fiaCode,
                       WF,
                       AGTEQ,
                       LBREQ,
                       DBREQ,
                       FOLEQ,
                       TIPEQ,
                       WF1REF,
                       WF2REF,
                       MCREF,
                       AGTREF,
                       LBRREF,
                       DBRREF,
                       FOLREF,
                       TIPREF,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH,
                       CRZSPDFTCS_STRINGLENGTH);
            // note: fiaCode is both an input and an output variable




            foreach (var comp in BIOMASS_COMPONENTS)
            {
                BiomassEquationDO bedo = new BiomassEquationDO();
                bedo.FIAcode = fiaCode;
                bedo.LiveDead = liveDead; // note: although biomass Eqs have live dead. Since during calculating tree value BioEq's arn't selected by LiveDead it doesn't matter what the LD is
                bedo.Product = primaryProduct;
                bedo.Species = species;
                bedo.PercentMoisture = WF[2];
                bedo.PercentRemoved = percentRemovedValue;

                switch (comp)
                {
                    case "TotalTreeAboveGround":
                        {
                            bedo.Component = comp;
                            bedo.Equation = AGTEQ.ToString();
                            bedo.MetaData = AGTREF.ToString();
                            break;
                        }
                    case "LiveBranches":
                        {
                            bedo.Component = comp;
                            bedo.Equation = LBREQ.ToString();
                            bedo.MetaData = LBRREF.ToString();
                            break;
                        }
                    case "DeadBranches":
                        {
                            bedo.Component = comp;
                            bedo.Equation = DBREQ.ToString();
                            bedo.MetaData = DBRREF.ToString();
                            break;
                        }
                    case "Foliage":
                        {
                            bedo.Component = comp;
                            bedo.Equation = FOLEQ.ToString();
                            bedo.MetaData = FOLREF.ToString();
                            break;
                        }
                    case "PrimaryProd":
                        {
                            bedo.Component = comp;
                            bedo.Equation = "";
                            bedo.MetaData = WF1REF.ToString();

                            if (region == 5)
                            {

                                if (primaryProduct == "20"
                                    && R5_Prod20_WF_FIAcodes.Any(x => x == fiaCode))
                                {
                                    bedo.WeightFactorPrimary = WF[1];
                                }
                                else bedo.WeightFactorPrimary = WF[0];
                            }
                            else if (REGN == 1 && primaryProduct != "01")
                            {
                                bedo.WeightFactorPrimary = WF[1];
                            }
                            else
                            {
                                bedo.WeightFactorPrimary = WF[0];
                            }


                            break;
                        }
                    case "SecondaryProd":
                        {
                            bedo.Component = comp;
                            bedo.Equation = "";
                            bedo.WeightFactorSecondary = WF[1];
                            bedo.MetaData = WF2REF.ToString();
                            break;
                        }
                    case "StemTip":
                        {
                            bedo.Component = comp;
                            bedo.Equation = TIPEQ.ToString();
                            bedo.MetaData = TIPREF.ToString();
                            break;
                        }
                }

                yield return bedo;
            }
        }


    }
}
