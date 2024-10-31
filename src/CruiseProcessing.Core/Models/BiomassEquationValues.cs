using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Models
{
    public class BiomassValue
    {
        public string Species { get; set; }
        public string Product { get; set; }
        public string LiveDead { get; set; }
        public float WeightFactor { get; set; }
        public float MoistureContent { get; set; }
    }
}
