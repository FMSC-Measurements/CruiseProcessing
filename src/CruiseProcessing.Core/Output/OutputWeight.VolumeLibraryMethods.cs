using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing
{
    public partial class OutputWeight
    {
        protected static int CROWN_FACTOR_WEIGHT_ARRAY_LENGTH = 7;

        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)] private static extern void BROWNCROWNFRACTION(ref int SPCD, ref float DBH, ref float THT, ref float CR, float[] CFWT);

        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)] private static extern void BROWNTOPWOOD(ref int SPN, ref float GCUFTS, ref float WT);

        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)] private static extern void BROWNCULLLOG(ref int SPN, ref float GCUFTS, ref float WT);

        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)] private static extern void BROWNCULLCHUNK(ref int SPN, ref float GCUFT, ref float NCUFT, ref float FLIW, ref float WT);
    }
}
