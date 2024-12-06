using System;
using System.Text;

namespace CruiseProcessing.Interop
{
    public interface IVolumeLibrary
    {
        int GetVersionNumber();

        //void CalculateVolumeAndBiomassNSVB(
        //    int region,
        //    string forest,
        //    string volumeEquation,
        //    float topDIBPrimary,
        //    float topDIBSecondary,

        //    float stumpHeight,
        //    float dbh,
        //    float drc,
        //    string merchHeightType,
        //    float totalHeight,

        //    int merchHeightLogLength,
        //    float merchHeightPrimary,
        //    float merchHeightSecondary,
        //    float upperStemHeight,
        //    float upperStemHeight2,

        //    float upperStemDiameter,
        //    float upperStemDiameter2,
        //    int referenceHeightPercent,
        //    float averageZ,
        //    float averageZ2,

        //    int formClass,
        //    float dbhDoubleBarkThickness,
        //    float barkThicknessRatio,
        //    float[] volumes, //output
        //    float[] logVolumes, //output

        //    float[] logDiameters, //output
        //    float[] logLengths, //output
        //    float[] bottomOfLogHeights, //output
        //    int totalLogs, //output
        //    float noLogsPrimary, //output

        //    float noLogsSecondary, //output
        //    bool calcTotal,
        //    bool calcBoard,
        //    bool calcCubic,
        //    bool calcCords,

        //    bool calcTopwood,
        //    string contractSpecies,
        //    string product,
        //    int heightToFirstLiveLimb,
        //    string liveDead,

        //    int ba, //unused - basal area of stand
        //    int si, //unused - site index
        //    string ctype, // cruise type: 'C', 'F', 'V', 'I'
        //    int errorCode,
        //    int pmFlag, //unused, unknown purpose

        //    MRules mchRules,
        //    int iDist, // district number

        //    float brkht, // unused, unknown purpose
        //    float brkhtd, // unused, unknown purpose
        //    int fiaSpcd, // fiacode from volume equation
        //    float[] dryBiomass,
        //    float[] greenBiomass,

        //    float cr, // unused, The percent of the tree bole supporting live, healthy foliage
        //    float cull, // unused, The percent of the tree bole that is cull
        //    int decayCode // unused, Code indicating stage of decay
        //    );

        void CalculateVolumeNVB(
            int regn, string forst, string voleq, float mtopp, float mtops,
            float stump, float dbhob, float drcob, string httype, float httot,
            int htlog, float ht1prd, float ht2prd, float upsht1, float upsht2,
            float upsd1, float upsd2, int htref, float avgz1, float avgz2,
            int fclass, float dbtbh, float btr, float[] vol, float[,] logvol,
            float[,] logdia, float[] loglen, float[] bohlt, ref int tlogs, out float nologp,
            out float nologs, int cutflg, int bfpflg, int cupflg, int cdpflg,
            int spflg, string conspec, string prod, int httfll, string live,
            out int ba, out int si, string ctype, out int errflg, int pmtflg,
            ref MRules mRules, int idist,
            float brkht, float brkhtd, int fiaspcd, float[] drybio, float[] grnbio,
            float cr, float cull, int decaycd);

        void CalculateBiomass(
            int regn, string forst, int spcd, float dbhob, float drcob,
            float httot, int fclass, float[] vol, float[] wf, float[] bms,
            out int errflg, string prod);

        float[] LookupWeightFactors(int region, string forest, ref int fiaCode);

        void LookupWeightFactors2(int regin, string forest, int fiaCode, string prod, out float greenWf, out float deadWf);

        [Obsolete]
        void BrownCrownFraction(int fiaCode, float DBH, float THT, float CR, float[] crownFractionWGT);

        CrownFractionWeight BrownCrownFraction(int fiaCode, float DBH, float THT, float CR);

        void BrownTopwood(int fiaCode, float grsVol, out float topwoodWGT);

        void BrownCullLog(int fiaCode, float GCUFTS, out float cullLogWGT);

        void BrownCullChunk(int fiaCode, float GCUFT, float NCUFT, float FLIW, out float cullChunkWGT);
    }

    public static class VolumeLibraryExtensions
    {
        public static string GetVersionNumberString(this IVolumeLibrary volumeLibrary)
        {
            return VolLibVersionNumberToString(volumeLibrary.GetVersionNumber());
        }

        public static string VolLibVersionNumberToString(int versionNumber)
        {
            try
            {
                //  Convert to a string to reformat date
                string sTemp = versionNumber.ToString();
                StringBuilder sDate = new StringBuilder();
                sDate.Append(sTemp.Substring(4, 2));
                sDate.Append(".");
                sDate.Append(sTemp.Substring(6, 2));
                sDate.Append(".");
                sDate.Append(sTemp.Substring(0, 4));

                return sDate.ToString();
            }
            catch
            {
                return "0.0.0.0";
            }
        }
    }
}