using System.Runtime.InteropServices;
using System.Text;

namespace CruiseProcessing
{
    public partial class CalculateTreeValues2
    {
        private const int STRING_BUFFER_SIZE = 256;
        private const int CHARLEN = 1;

        private const int DRYBIO_ARRAY_SIZE = 15;
        private const int GRNBIO_ARRAY_SIZE = 15;

        private const int I3 = 3;
        private const int I7 = 7;
        private const int I15 = 15;
        private const int I20 = 20;
        private const int I21 = 21;

        private const int CRZBIOMASSCS_BMS_SIZE = 8;

        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)]//EntryPoint = "VERNUM2",
        public static extern void VERNUM2(out int a);

        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CRZBIOMASSCS(ref int regn,
                            StringBuilder forst,
                            ref int spcd,
                            ref float dbhob,
                            ref float drcob,
                            ref float httot,
                            ref int fclass,
                            float[] vol,
                            float[] wf,
                            float[] bms,
                            ref int errflg,
                            int i1);

        [DllImport("vollib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void VOLLIBCSNVB(ref int regn,
                            StringBuilder forst,
                            StringBuilder voleq,
                            ref float mtopp,
                            ref float mtops,

                            ref float stump,
                            ref float dbhob,
                            ref float drcob,
                            StringBuilder httype,
                            ref float httot,

                            ref int htlog,
                            ref float ht1prd,
                            ref float ht2prd,
                            ref float upsht1,
                            ref float upsht2,

                            ref float upsd1,
                            ref float upsd2,
                            ref int htref,
                            ref float avgz1,
                            ref float avgz2,

                            ref int fclass,
                            ref float dbtbh,
                            ref float btr,
                            float[] vol,
                            float[,] logvol,

                            float[,] logdia,
                            float[] loglen,
                            float[] bohlt,
                            ref int tlogs,
                            ref float nologp,

                            ref float nologs,
                            ref int cutflg,
                            ref int bfpflg,
                            ref int cupflg,
                            ref int cdpflg,

                            ref int spflg,
                            StringBuilder conspec,
                            StringBuilder prod,
                            ref int httfll,
                            StringBuilder live,

                            ref int ba,
                            ref int si,
                            StringBuilder ctype,
                            ref int errflg,
                            ref int pmtflg,

                            ref MRules mRules,
                            ref int dist,

                            ref float brkht,
                            ref float brkhtd,
                            ref int fiaspcd,
                            float[] drybio,
                            float[] grnbio,

                            ref float cr,
                            ref float cull,
                            ref int decaycd,

                            int ll1,
                            int ll2,
                            int ll3,
                            int ll4,
                            int ll5,
                            int ll6,
                            int ll7,
                            int charLen);
    }
}