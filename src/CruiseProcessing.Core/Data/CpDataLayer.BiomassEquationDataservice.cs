using CruiseProcessing.Config;
using CruiseProcessing.Interop;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        //private const int CRZSPDFTCS_STRINGLENGTH = 256;

        public BiomassEquationOptions BiomassOptions { get; }

        protected Dictionary<(string species, string product, string liveDead), float[]> CRZSPDFTWeightFactorCache
            = new Dictionary<(string species, string product, string liveDead), float[]>();

        public Dictionary<(string species, string product, string liveDead), float> NVBWeightFactorCache
            = new Dictionary<(string species, string product, string liveDead), float>();

        protected Dictionary<(string species, string product), float> PercentRemovedCache { get; }
            = new Dictionary<(string species, string product), float>();

        // primarily used to reset cache before processing
        // cache is intend to stay live until generating output
        public void ResetBioMassEquationCache()
        {
            CRZSPDFTWeightFactorCache.Clear();
            NVBWeightFactorCache.Clear();
            PercentRemovedCache.Clear();
        }

        public float[] GetWeightFactorArray(string species, string product, string liveDead, IVolumeLibrary volumeLibrary)
        {
            var key = (species, product, liveDead);
            if (CRZSPDFTWeightFactorCache.TryGetValue(key, out var cacheWf))
            {
                return cacheWf;
            }

            var bioValues = GetBiomassWfValues(species, product, liveDead);
            if (bioValues == null && liveDead == "D")
            {
                bioValues = GetBiomassWfValues(species, product, "L");
            }

            if (bioValues != null)
            {
                var newwf = new float[]
                {
                    bioValues.FirstWeightFactor,
                    bioValues.SecondWeightFactor,
                    bioValues.PercentMoisture,
                };
                CRZSPDFTWeightFactorCache.Add(key, newwf);

                return newwf;
            }

            if (volumeLibrary != null)
            {
                var fiaCode = GetFIACode(species);
                var region = int.Parse(getRegion());
                var forest = getForest();
                var wf = volumeLibrary.LookupWeightFactorsCRZSPDFT(region, forest, product, fiaCode);
                CRZSPDFTWeightFactorCache.Add((species, product, liveDead), wf);
                return wf;
            }
            throw new InvalidOperationException("Cannot find weight factor for species, product, and liveDead");
        }

        public float GetWeightFactor(string species, string product, string liveDead, IVolumeLibrary volumeLibrary)
        {
            var key = (species, product, liveDead);
            if (CRZSPDFTWeightFactorCache.TryGetValue(key, out var cacheWf))
            {
                return cacheWf[0];
            }

            if (NVBWeightFactorCache.TryGetValue(key, out var singleWf))
            {
                return singleWf;
            }

            // get weight factor from biomass equations
            // cahece it
            // and return it
            var fiaCode = GetFIACode(species);
            var region = int.Parse(getRegion());
            var forest = getForest();
            volumeLibrary.LookupWeightFactorsNVB(region, forest, fiaCode, product, out var liveWf, out var deadWf);
            singleWf = (liveDead == "D") ? deadWf : liveWf;

            NVBWeightFactorCache.Add(key, singleWf);

            return singleWf;
        }

        public float GetPercentRemoved(string species, string product)
        {
            if (PercentRemovedCache.TryGetValue((species, product), out var pr))
            {
                return pr;
            }

            var bioEq = GetBiomassEquations(species, product)
                .FirstOrDefault(x => x.PercentRemoved > 0);
            if (bioEq != null)
            {
                PercentRemovedCache.Add((species, product), bioEq.PercentRemoved);
                Log.LogDebug($"Using BiomassEquation Percent Removed For Sp:{species} Prod:{product} PR:{pr}");
                return bioEq.PercentRemoved;
            }

            return BiomassOptions.DefaultPercentRemoved;
        }
    }
}