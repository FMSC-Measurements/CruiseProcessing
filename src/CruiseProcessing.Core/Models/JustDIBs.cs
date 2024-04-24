using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing
{
    public class JustDIBs
    {
        //  structure for DIB capture in Region 9 volume equations
        public string speciesDIB { get; set; }
        public string productDIB { get; set; }
        public float primaryDIB { get; set; }
        public float secondaryDIB { get; set; }
    }
}
