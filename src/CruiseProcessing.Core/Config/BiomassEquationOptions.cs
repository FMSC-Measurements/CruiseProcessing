using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Config
{
    public class BiomassEquationOptions
    {
        public const string Biomass = "Biomass";

        public float DefaultPercentRemoved { get; set; } = 95.0f;

        public bool UseWightFactorsFromVolumeLibrary { get; set; } = true;

        public bool UseWeightFactorsFromBiomassEquations { get; set; } = true;
    }
}
