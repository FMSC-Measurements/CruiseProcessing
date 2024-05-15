using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class StewProductCosts
    {
        public string costUnit { get; set; }
        public string costSpecies { get; set; }
        public string costProduct { get; set; }
        public float costPounds { get; set; }
        public float costCost { get; set; }
        public float scalePC { get; set; }
        public string includeInReport { get; set; }
    }
}
