using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing.Interop
{
    public abstract class VolumeLibraryInterop : IVolumeLibrary
    {
        public static IVolumeLibrary Default => new VolumeLibrary_20240626();

        private static readonly IReadOnlyCollection<int> R5_Prod20_WF_FIAcodes = new int[] { 122, 116, 117, 015, 020, 202, 081, 108 };
        public static int CROWN_FACTOR_WEIGHT_ARRAY_LENGTH = 7;

        public const int CRZSPDFTCS_STRINGLENGTH = 256;
        public const int STRING_BUFFER_SIZE = 256;
        public const int CHARLEN = 1;

        public const int VOLLIBNVB_VOL_SIZE = 15;
        public const int VOLLIBNVB_LOGLEN_SIZE = 20;
        public const int VOLLIBNVB_BOLHT_SIZE = 21;
        public const int VOLLIBNVB_LOGVOL_SIZE_X = 20;
        public const int VOLLIBNVB_LOGVOL_SIZE_Y = 7;
        public const int VOLLIBNVB_LOGDIA_SIZE_X = 3;
        public const int VOLLIBNVB_LOGDIA_SIZE_Y = 21;

        public const int VOLLIBNVB_BIO_SIZE = 15;
        public const int DRYBIO_ARRAY_SIZE = VOLLIBNVB_BIO_SIZE;
        public const int GRNBIO_ARRAY_SIZE = VOLLIBNVB_BIO_SIZE;

        public const int I3 = 3;
        public const int I7 = 7;
        public const int I15 = 15;
        public const int I20 = 20;
        public const int I21 = 21;

        public const int CRZBIOMASSCS_BMS_SIZE = 8;

        protected ILogger Log { get; }

        public VolumeLibraryInterop(ILogger logger)
        {
            Log = logger;
        }

        public VolLibNVBoutput CalculateVolumeNVB(
    int regn, string forst, string voleq, float mtopp, float mtops,
    float stump, float dbhob, float drcob, string httype, float httot,
    int htlog, float ht1prd, float ht2prd, float upsht1, float upsht2,
    float upsd1, float upsd2, int htref, float avgz1, float avgz2,
    int fclass, float dbtbh, float btr, int cutflg, int bfpflg, int cupflg, int cdpflg,
    int spflg, string conspec, string prod, int httfll, string live,
    int ba, int si, string ctype, int pmtflg,
    MRules mRules, int idist,
    float brkht, float brkhtd, int fiaspcd,
    float cr, float cull, int decaycd)
        {
            CalculateVolumeNVB(regn, forst, voleq, mtopp, mtops,
                stump, dbhob, drcob, httype, httot,
                htlog, ht1prd, ht2prd, upsht1, upsht2,
                upsd1, upsd2, htref, avgz1, avgz2,
                fclass, dbtbh, btr, out var vol, out var logvol,
                out var logdia, out var loglen, out var bolht, out var tlogs, out var nologp,
                out var nologs, cutflg, bfpflg, cupflg, cdpflg,
                spflg, conspec, prod, httfll, live,
                ba, si, ctype, out var errflg, pmtflg,
                mRules, idist,
                brkht, brkhtd, fiaspcd, out var drybio, out var grnbio,
                cr, cull, decaycd);

            var volumes = new Volumes(vol);
            var logVolumes = new LogVolume[VOLLIBNVB_LOGVOL_SIZE_X];
            for (var i = 0; i < VOLLIBNVB_LOGVOL_SIZE_X; i++)
            {
                logVolumes[i] = new LogVolume().FromArray(logvol, i);
            }

            var logDiameters = new LogDiameter[VOLLIBNVB_LOGDIA_SIZE_Y];
            for (var i = 0; i < VOLLIBNVB_LOGDIA_SIZE_Y; i++)
            {
                logDiameters[i] = new LogDiameter().FromArray(logdia, i);
            }

            var greenBio = new VolLibNVBCalculatedBiomass().FromArray(grnbio);
            var dryBio = new VolLibNVBCalculatedBiomass().FromArray(drybio);

            var output = new VolLibNVBoutput
            {
                Volumes = volumes,
                LogVolumes = logVolumes,
                LogDiameters = logDiameters,
                LogLengths = loglen,
                BottomOfLogHeights = bolht,
                TotalLogs = tlogs,
                NoLogsPrimary = nologp,
                NoLogsSecondary = nologs,
                GreenBio = greenBio,
                DryBio = dryBio,
                ErrorCode = errflg,
            };

            return output;
        }

        public float[] LookupWeightFactorsCRZSPDFT(int region, string forest, string product, int fiaCode)
        {
            var wf = LookupWeightFactorsCRZSPDFTRaw(region, forest, fiaCode);

            if (wf[1] > 0)
            {
                if ((region == 5 && product == "20" && R5_Prod20_WF_FIAcodes.Any(x => x == fiaCode))
                   || (region == 1 && product != "01"))
                {
                    wf[0] = wf[1];
                }
            }

            return wf;
        }

        public abstract int GetVersionNumber();

        public abstract void CalculateVolumeNVB(
            int regn, string forst, string voleq, float mtopp, float mtops,
            float stump, float dbhob, float drcob, string httype, float httot,
            int htlog, float ht1prd, float ht2prd, float upsht1, float upsht2,
            float upsd1, float upsd2, int htref, float avgz1, float avgz2,
            int fclass, float dbtbh, float btr, out float[] vol, out float[,] logvol,
            out float[,] logdia, out float[] loglen, out float[] bolht, out int tlogs, out float nologp,
            out float nologs, int cutflg, int bfpflg, int cupflg, int cdpflg,
            int spflg, string conspec, string prod, int httfll, string live,
            int ba, int si, string ctype, out int errflg, int pmtflg,
            MRules mRules, int idist,
            float brkht, float brkhtd, int fiaspcd, out float[] drybio, out float[] grnbio,
            float cr, float cull, int decaycd);

        public abstract CrzBiomassResult CalculateBiomass(int regn, string forst, int spcd, float dbhob, float drcob, float httot, int fclass, float[] vol, float[] wf, out int errflg, string prod);

        public abstract float[] LookupWeightFactorsCRZSPDFTRaw(int region, string forest, int fiaCode);

        public abstract void LookupWeightFactorsNVB(int regin, string forest, int fiaCode, string prod, out float greenWf, out float deadWf);

        public abstract void BrownCrownFraction(int fiaCode, float DBH, float THT, float CR, float[] crownFractionWGT);

        public abstract CrownFractionWeight BrownCrownFraction(int fiaCode, float DBH, float THT, float CR);

        public abstract void BrownTopwood(int fiaCode, float grsVol, out float topwoodWGT);

        public abstract void BrownCullLog(int fiaCode, float GCUFTS, out float cullLogWGT);

        public abstract void BrownCullChunk(int fiaCode, float GCUFT, float NCUFT, float FLIW, out float cullChunkWGT);
    }
}