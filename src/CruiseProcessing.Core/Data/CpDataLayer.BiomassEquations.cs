using CruiseDAL.DataObjects;
using CruiseProcessing.Interop;
using CruiseProcessing.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
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

        public List<BiomassEquationDO> getBiomassEquations()
        {
            return DAL.From<BiomassEquationDO>().Read().ToList();
        }   //  end getBiomassEquations

        public List<BiomassEquationDO> GetBiomassEquations(string species)
        {
            return DAL.From<BiomassEquationDO>()
                .Where("Species = @p1")
                .Read(species).ToList();
        }

        // percent removed values can species product specific so this can be used to get those values
        public List<BiomassEquationDO> GetBiomassEquations(string species, string product)
        {
            return DAL.From<BiomassEquationDO>()
                .Where("Species = @p1 AND Product = @p2")
                .Read(species, product).ToList();
        }


        public CrzBiomassWfValues GetBiomassWfValues(string species, string product, string liveDead)
        {
            var biomassValues = GetBiomassWfValuesInternal(species, product, liveDead);
            if (biomassValues == null && liveDead == "D")
            {
                return GetBiomassWfValuesInternal(species, product, "L");
            }
            return biomassValues;
        }


        protected CrzBiomassWfValues GetBiomassWfValuesInternal(string species, string product, string liveDead)
        {
            return DAL.Query<CrzBiomassWfValues>(
$@"With bioEq as (
    SELECT
        *
    FROM
        BiomassEquation
    WHERE
        Species = @p1
        AND Product = @p2
        AND LiveDead = @p3
)

SELECT
    (SELECT WeightFactorPrimary FROM bioEq WHERE Component = '{PRIMARY_PRODUCT_COMPONENT}' LIMIT 1) AS FirstWeightFactor,
    (SELECT PercentMoisture FROM bioEq WHERE Component = '{PRIMARY_PRODUCT_COMPONENT}' LIMIT 1) AS PercentMoisture,
    (SELECT WeightFactorSecondary FROM bioEq WHERE Component = '{SECONDARY_PRODUCT_COMPONENT}'  LIMIT 1) AS SecondWeightFactor;",
                species, product, liveDead).FirstOrDefault();
        }

        public void SaveBiomassEquations(IEnumerable<BiomassEquationDO> bioList)
        {
            Log?.LogDebug("Saving BiomassEquations");
            foreach (BiomassEquationDO beq in bioList)
            {
                DAL.Save(beq);
            }

            if(DAL_V3 != null)
            {
                foreach (var bioEq in bioList)
                {
                    DAL_V3.Execute2(
        @"INSERT INTO BiomassEquation (
    CruiseID,
    Species,
    Product,
    Component,
    LiveDead,
    FIAcode,
    Equation,
    PercentMoisture,
    PercentRemoved,
    MetaData,
    WeightFactorPrimary,
    WeightFactorSecondary
) 
VALUES ( 
    @CruiseID,
    @Species,
    @Product,
    @Component,
    @LiveDead,
    @FIAcode,
    @Equation,
    @PercentMoisture,
    @PercentRemoved,
    @MetaData,
    @WeightFactorPrimary,
    @WeightFactorSecondary
) 
ON CONFLICT (CruiseID, Species, Product, Component, LiveDead) DO
UPDATE SET
    FIAcode = @FIAcode,
    Equation = @Equation,
    PercentMoisture = @PercentMoisture,
    PercentRemoved = @PercentRemoved,
    MetaData = @MetaData,
    WeightFactorPrimary = @WeightFactorPrimary,
    WeightFactorSecondary = @WeightFactorSecondary
;",
                            new
                            {
                                CruiseID,
                                bioEq.Species,
                                bioEq.Product,
                                bioEq.Component,
                                bioEq.LiveDead,
                                bioEq.FIAcode,
                                bioEq.Equation,
                                bioEq.PercentMoisture,
                                bioEq.PercentRemoved,
                                bioEq.MetaData,
                                bioEq.WeightFactorPrimary,
                                bioEq.WeightFactorSecondary
                            });
                }
            }
            
        }   //  end SaveBiomassEquations

        public void ClearBiomassEquations()
        {
            Log?.LogDebug("Clearing BiomassEquations");
            DAL.Execute("DELETE FROM BiomassEquation");

            if (DAL_V3 != null)
            {
                try
                {
                    DAL_V3?.Execute("DELETE FROM BiomassEquation WHERE CruiseID = @p1", CruiseID);
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, nameof(ClearBiomassEquations));
                }
            }
            
            
        }
    }
}
