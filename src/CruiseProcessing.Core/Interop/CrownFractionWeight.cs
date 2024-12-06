using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Interop
{
    public class CrownFractionWeight
    {
        public CrownFractionWeight() { }

        protected CrownFractionWeight(float[] volLibArray)
        {
            Needles = volLibArray[0];
            QuarterInch = volLibArray[1];
            OneInch = volLibArray[2];
            ThreeInch = volLibArray[3];
            ThreePlus = volLibArray[4];
        }

        public static CrownFractionWeight FromArray(float[] volLibArray)
        {
            return new CrownFractionWeight(volLibArray);
        }

        public float Needles { get; set; }
        public float QuarterInch { get; set; }
        public float OneInch { get; set; }
        public float ThreeInch { get; set; }
        public float ThreePlus { get; set; }
    }
}
