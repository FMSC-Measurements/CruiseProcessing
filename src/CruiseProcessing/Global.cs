using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public static class Global
    {
        public static CPbusinessLayer BL
        {
            get;
            private set;
        }


        public static void Init(CPbusinessLayer cPbusinessLayer)
        {
            BL = cPbusinessLayer;
        }
    }
}
