using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.Services;
using CruiseProcessing.ViewModels;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Test
{
    // TODO idealy these biomass equation methods would be the biomass equation method in the VolumeEquations class
    // but that class is not currently not very testable and that code is in CruiseProcessing project, so not accessible right now
    // in the long run I think I will need to convert the VolumeEquation logic to a ViewModel and make the update biomass methods accessible
    // from CruiseProcessing.Core because it is very useful for testing to make sure the biomass equations are updated exactly how they are in the application
    public class BiomassHelpers
    {
        public static void UpdateBiomass(CpDataLayer dataLayer, IReadOnlyCollection<VolumeEquationDO> equationList = null, string region = null, string forest = null, IVolumeLibrary volumeLibrary = null)
        {
            if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(forest))
            {
                var sale = dataLayer.GetSale();
                if (sale != null)
                {
                    region = sale.Region;
                    forest = sale.Forest;
                }
            }

            equationList ??= dataLayer.getVolumeEquations();

            var prList = equationList.Where(x => x.CalcBiomass == 1).Select(x => new PercentRemoved
            {
                bioSpecies = x.Species,
                bioProduct = x.PrimaryProduct,
                bioPCremoved = "95.0"
            }).ToList();

            var mockDialogService = Substitute.For<IDialogService>();
            mockDialogService.ShowPercentRemovedDialog(Arg.Any<IEnumerable<VolumeEquationDO>>())
                .Returns(prList);

            volumeLibrary ??= new VolumeLibrary_20241118();
            var volEqViewModel = new VolumeEquationsViewModel(dataLayer, mockDialogService, volumeLibrary, biomassOptions: null, logger: null);
            volEqViewModel.UpdateBiomass(equationList.ToList(), region, forest);
        }
    }
}