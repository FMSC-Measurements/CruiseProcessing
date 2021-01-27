using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    //  this file contains small list structures used to get volume equations
    //  Like VolEqList is used to populate lists in the equation window
    //  JustDIBs is used in Region 9 volume equations to update DIBs as equations are made
    //  JustStrata is used to get stratum, species and SG for updating the biomass report
    public class VolEqList
    {
        //  structure for a volume equation list specific to region
        public string vForest { get; set; }
        public string vCommonName { get; set; }
        public string vEquation { get; set; }
        public string vModelName { get; set; }
    }

    public class JustDIBs
    {
        //  structure for DIB capture in Region 9 volume equations
        public string speciesDIB { get; set; }
        public string productDIB { get; set; }
        public float primaryDIB { get; set; }
        public float secondaryDIB { get; set; }
    }

    public class PercentRemoved
    {
        public string bioSpecies { get; set; }
        public string bioProduct { get; set; }
        public string bioPCremoved { get; set; }
    }   //  end class PercentRemoved  
    
}
