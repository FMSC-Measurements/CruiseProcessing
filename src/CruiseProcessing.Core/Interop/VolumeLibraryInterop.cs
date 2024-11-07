using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Interop
{
    public abstract class VolumeLibraryInterop
    {
        public static IVolumeLibrary Default => new VolumeLibrary_20240626();

        public static int CROWN_FACTOR_WEIGHT_ARRAY_LENGTH = 7;

        public const int CRZSPDFTCS_STRINGLENGTH = 256;
        public const int STRING_BUFFER_SIZE = 256;
        public const int CHARLEN = 1;

        public const int DRYBIO_ARRAY_SIZE = 15;
        public const int GRNBIO_ARRAY_SIZE = 15;

        public const int I3 = 3;
        public const int I7 = 7;
        public const int I15 = 15;
        public const int I20 = 20;
        public const int I21 = 21;

        public const int CRZBIOMASSCS_BMS_SIZE = 8;
    }
}
