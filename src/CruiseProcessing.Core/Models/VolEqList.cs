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
    //public record VolEqList(string vForest, string vCommonName, string vEquation, string vModelName);
    public class VolEqList
    {
        public VolEqList(string vForest, string vCommonName, string vEquation, string vModelName)
        {
            this.vForest = vForest ?? throw new ArgumentNullException(nameof(vForest));
            this.vCommonName = vCommonName ?? throw new ArgumentNullException(nameof(vCommonName));
            this.vEquation = vEquation ?? throw new ArgumentNullException(nameof(vEquation));
            this.vModelName = vModelName ?? throw new ArgumentNullException(nameof(vModelName));
        }


        //  structure for a volume equation list specific to region
        public string vForest { get; set; }
        public string vCommonName { get; set; }
        public string vEquation { get; set; }
        public string vModelName { get; set; }
    }
}
