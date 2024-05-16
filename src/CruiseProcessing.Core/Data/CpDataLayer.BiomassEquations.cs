using CruiseDAL.DataObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        public List<BiomassEquationDO> getBiomassEquations()
        {
            return DAL.From<BiomassEquationDO>().Read().ToList();
        }   //  end getBiomassEquations

        public void SaveBiomassEquations(List<BiomassEquationDO> bioList)
        {
            foreach (BiomassEquationDO beq in bioList)
            {
                DAL.Save(beq);
            }


            foreach (var bioEq in bioList)
            {
                if (DAL_V3 != null)
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
            DAL.Execute("DELETE FROM BiomassEquation");

            try
            {
                DAL_V3.Execute("DELETE FROM BiomassEquation WHERE CruiseID = @p1", CruiseID);
            }
            catch (Exception ex) 
            {
                Log.LogError(ex, nameof(ClearBiomassEquations));
            }

        }
    }
}
