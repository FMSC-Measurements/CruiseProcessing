using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Test
{
    public class BiomassHelpers
    {
        public static void UpdateBiomass(CpDataLayer dataLayer, List<VolumeEquationDO> equationList, string region = null, string forest = null)
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

            UpdateBiomass(dataLayer, region, forest, equationList);
        }

        //public void UpdateBiomass(CpDataLayer dataLayer, IReadOnlyCollection<VolumeEquationDO> equationList, IReadOnlyCollection<PercentRemoved> prList, string region, string forest)
        //{
        //    if(string.IsNullOrEmpty(region)) { throw new ArgumentNullException(nameof(region)); }
        //    if(string.IsNullOrEmpty(forest)) { throw new ArgumentNullException(nameof(forest)); }

        //    UpdateBiomass(region, forest, dataLayer, equationList, prList);
        //}

        public static void UpdateBiomass(CpDataLayer dataLayer, string region, string forest, IReadOnlyCollection<VolumeEquationDO> equationList)
        {
            int REGN = Convert.ToInt32(region);

            var biomassEquations = new List<BiomassEquationDO>();

            var prList = equationList.Where(x => x.CalcBiomass == 1).Select(x => new PercentRemoved
            {
                bioSpecies = x.Species,
                bioProduct = x.PrimaryProduct,
                bioPCremoved = "95.0"
            }).ToList();

            dataLayer.CreateBiomassEquations(equationList, REGN, forest, prList);

            dataLayer.ClearBiomassEquations();
            dataLayer.SaveBiomassEquations(biomassEquations);
        }
    }
}
