using System;
using System.Text;

namespace CruiseProcessing.Interop
{
    public interface IVolumeLibrary
    {
        int GetVersionNumber();

        VolLibNVBoutput CalculateVolumeNVB(
            int regn, string forst, string voleq, float mtopp, float mtops,
            float stump, float dbhob, float drcob, string httype, float httot,
            int htlog, float ht1prd, float ht2prd, float upsht1, float upsht2,
            float upsd1, float upsd2, int htref, float avgz1, float avgz2,
            int fclass, float dbtbh, float btr, int cutflg, int bfpflg, int cupflg, int cdpflg,
            int spflg, string conspec, string prod, int httfll, string live,
            int ba, int si, string ctype, int pmtflg,
            MRules mRules, int idist,
            float brkht, float brkhtd, int fiaspcd,
            float cr, float cull, int decaycd);

        void CalculateVolumeNVB(
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

        CrzBiomassResult CalculateBiomass(
            int regn, string forst, int spcd, float dbhob, float drcob,
            float httot, int fclass, float[] vol, float[] wf,
            out int errflg, string prod);

        float[] LookupWeightFactorsCRZSPDFT(int region, string forest, string product, int fiaCode);

        float[] LookupWeightFactorsCRZSPDFTRaw(int region, string forest, int fiaCode);

        void LookupWeightFactorsNVB(int regin, string forest, int fiaCode, string prod, out float greenWf, out float deadWf);

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