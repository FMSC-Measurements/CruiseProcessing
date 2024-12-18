using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CruiseProcessing.Interop
{
    public static class VolumeLibraryExtentions
    {
        public const string PRIMARY_PRODUCT_COMPONENT = "PrimaryProd";
        public const string SECONDARY_PRODUCT_COMPONENT = "SecondaryProd";

        public static readonly ReadOnlyCollection<string> BIOMASS_COMPONENTS = Array.AsReadOnly(new[]
        {
            "TotalTreeAboveGround",
            "LiveBranches",
            "DeadBranches",
            "Foliage",
            PRIMARY_PRODUCT_COMPONENT,
            SECONDARY_PRODUCT_COMPONENT,
            "StemTip"
        });

        public static IEnumerable<BiomassEquationDO> MakeBiomassEquationsInternal(this IVolumeLibrary volumeLibrary, int region, string forest, int fiaCode, string primaryProduct, string species, string liveDead, float percentRemovedValue)
        {
            //Log?.LogDebug("Making Biomass Equations For Sp:{Species} Prod:{PrimaryProduct} LD:{LiveDead} FIA:{FIAcode} Region:{Region} Forest:{Forest} PR:{PercentRemoved}"
            //    , species, primaryProduct, liveDead, fiaCode, region, forest, percentRemovedValue);

            float[] WF = volumeLibrary.LookupWeightFactorsCRZSPDFT(region, forest, primaryProduct, fiaCode);

            foreach (var comp in BIOMASS_COMPONENTS)
            {
                BiomassEquationDO bedo = new BiomassEquationDO();
                bedo.FIAcode = fiaCode;
                bedo.LiveDead = liveDead; // note: although biomass Eqs have live dead. Since during calculating tree value BioEq's arn't selected by LiveDead it doesn't matter what the LD is
                bedo.Product = primaryProduct;
                bedo.Species = species;
                bedo.PercentMoisture = WF[2];
                bedo.PercentRemoved = percentRemovedValue;
                bedo.Component = comp;

                switch (comp)
                {
                    //case "TotalTreeAboveGround":
                    //    {
                    //        bedo.Equation = AGTEQ.ToString();
                    //        bedo.MetaData = AGTREF.ToString();
                    //        break;
                    //    }
                    //case "LiveBranches":
                    //    {
                    //        bedo.Equation = LBREQ.ToString();
                    //        bedo.MetaData = LBRREF.ToString();
                    //        break;
                    //    }
                    //case "DeadBranches":
                    //    {
                    //        bedo.Equation = DBREQ.ToString();
                    //        bedo.MetaData = DBRREF.ToString();
                    //        break;
                    //    }
                    //case "Foliage":
                    //    {
                    //        bedo.Equation = FOLEQ.ToString();
                    //        bedo.MetaData = FOLREF.ToString();
                    //        break;
                    //    }
                    case "PrimaryProd":
                        {
                            bedo.Equation = "";
                            //bedo.MetaData = WF1REF.ToString();
                            bedo.WeightFactorPrimary = WF[0];

                            break;
                        }
                    case "SecondaryProd":
                        {
                            bedo.Equation = "";
                            bedo.WeightFactorSecondary = WF[1];
                            //bedo.MetaData = WF2REF.ToString();
                            break;
                        }
                        //case "StemTip":
                        //    {
                        //        bedo.Equation = TIPEQ.ToString();
                        //        bedo.MetaData = TIPREF.ToString();
                        //        break;
                        //    }
                }

                yield return bedo;
            }
        }
    }
}