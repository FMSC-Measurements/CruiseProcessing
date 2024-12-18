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
        public List<VolumeEquationDO> getVolumeEquations()
        {
            return DAL.From<VolumeEquationDO>().Read().ToList();
        }   //  end getVolumeEquations

        public List<JustDIBs> GetJustDIBs()
        {
            // TODO can be improved. Just use JustDIB as the data object instead of using VolEq and an intermediate

            //  retrieve species, product and both DIBs from volume equation table
            List<JustDIBs> jstDIBs = new List<JustDIBs>();

            List<VolumeEquationDO> volList = DAL.From<VolumeEquationDO>().GroupBy("Species", "PrimaryProduct").Read().ToList();
            foreach (VolumeEquationDO veq in volList)
            {
                JustDIBs js = new JustDIBs();
                js.speciesDIB = veq.Species;
                js.productDIB = veq.PrimaryProduct;
                js.primaryDIB = veq.TopDIBPrimary;
                js.secondaryDIB = veq.TopDIBSecondary;
                jstDIBs.Add(js);
            }   //  end foreach loop

            return jstDIBs;
        }   //  end GetJustDIBs

        public void SaveVolumeEquations(IReadOnlyCollection<VolumeEquationDO> volumeEquationList)
        {

            //  need to delete equations in order to update the database
            //  not sure why this has to happen but the way the DAL save works
            //  is if the user deleted an equation, the Save does not consider that
            //  when the equation list is updated ???????

            List<VolumeEquationDO> volList = getVolumeEquations();
            foreach (VolumeEquationDO vdo in volList)
            {
                // if volumeEquationList doesn't contain vdo then delete it from the db
                if (!volumeEquationList.Any(ve => ve.VolumeEquationNumber == vdo.VolumeEquationNumber && ve.Species == vdo.Species &&
                                ve.PrimaryProduct == vdo.PrimaryProduct))
                {
                    DAL.Delete(vdo);

                    try
                    {
                        DAL_V3?.Execute("DELETE FROM VolumeEquation WHERE CruiseID = @p1 AND VolumeEquationNumber = @p2 AND Species = @p3 AND PrimaryProduct = @p4;",
                            CruiseID, vdo.VolumeEquationNumber, vdo.Species, vdo.PrimaryProduct);
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ex, nameof(getVolumeEquations));
                    }
                }
            }

            foreach (VolumeEquationDO veq in volumeEquationList)
            {
                DAL.Save(veq, option: Backpack.SqlBuilder.OnConflictOption.Replace);

                try
                {
                    DAL_V3?.Execute2(
    @"INSERT OR REPLACE INTO VolumeEquation (
    CruiseID,
    Species,
    PrimaryProduct,
    VolumeEquationNumber,
    StumpHeight,
    TopDIBPrimary,
    TopDIBSecondary,
    CalcTotal,
    CalcBoard,
    CalcCubic,
    CalcCord,
    CalcTopwood,
    CalcBiomass,
    Trim,
    SegmentationLogic,
    MinLogLengthPrimary,
    MaxLogLengthPrimary,
    MinMerchLength,
    Model,
    CommonSpeciesName,
    MerchModFlag,
    EvenOddSegment
) VALUES (
    @CruiseID,
    @Species,
    @PrimaryProduct,
    @VolumeEquationNumber,
    @StumpHeight,
    @TopDIBPrimary,
    @TopDIBSecondary,
    @CalcTotal,
    @CalcBoard,
    @CalcCubic,
    @CalcCord,
    @CalcTopwood,
    @CalcBiomass,
    @Trim,
    @SegmentationLogic,
    @MinLogLengthPrimary,
    @MaxLogLengthPrimary,
    @MinMerchLength,
    @Model,
    @CommonSpeciesName,
    @MerchModFlag,
    @EvenOddSegment
);",
                        new
                        {
                            CruiseID,
                            veq.Species,
                            veq.PrimaryProduct,
                            veq.VolumeEquationNumber,
                            veq.StumpHeight,
                            veq.TopDIBPrimary,
                            veq.TopDIBSecondary,
                            veq.CalcTotal,
                            veq.CalcBoard,
                            veq.CalcCubic,
                            veq.CalcCord,
                            veq.CalcTopwood,
                            veq.CalcBiomass,
                            veq.Trim,
                            veq.SegmentationLogic,
                            veq.MinLogLengthPrimary,
                            veq.MaxLogLengthPrimary,
                            veq.MinMerchLength,
                            veq.Model,
                            veq.CommonSpeciesName,
                            veq.MerchModFlag,
                            veq.EvenOddSegment
                        });
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, nameof(SaveVolumeEquations));
                }
            }
        }

        public void deleteVolumeEquations()
        {
            //  used only in region 8 volume equations to reset volEqs

            DAL.Execute("DELETE FROM VolumeEquation");

            try
            {
                DAL_V3?.Execute("DELETE FROM VolumeEquation WHERE CruiseID = @p1;", CruiseID);
            }
            catch (Exception ex)
            { Log.LogError(ex, nameof(deleteVolumeEquations)); }
        }
    }
}
