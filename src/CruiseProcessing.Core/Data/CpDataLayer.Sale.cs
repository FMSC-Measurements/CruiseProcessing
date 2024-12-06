using CruiseDAL.DataObjects;
using CruiseProcessing.Output;
using CruiseProcessing.OutputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {

        public HeaderFieldData GetReportHeaderData()
        {
            var sale = GetSale();


            return new HeaderFieldData()
            {
                Date = DateTime.Now.ToString(),
                Version = CPVersion,
                DllVersion = VolLibVersion,
                CruiseName = sale.SaleNumber,
                SaleName = sale.Name.Trim(' '),
            };
        }

        public List<SaleDO> GetAllSaleRecords()
        {
            return DAL.From<SaleDO>().Read().ToList();
        }   //  end getSale

        public SaleDO GetSale()
        {
            return DAL.From<SaleDO>().Query().FirstOrDefault();
        }

        //  functions returning single elements
        // *******************************************************************************************
        //  like region
        public string getRegion()
        {
            //  retrieve sale record
            var sale = GetSale();
            return sale.Region;
        }

        //  forest
        public string getForest()
        {
            var sale = GetSale();
            return sale.Forest;
        }

        //  district
        public string getDistrict()
        {
            var sale = GetSale();
            return sale.District;
        }

        public string getCruiseNumber()
        {
            var sale = GetSale();
            return sale.SaleNumber;
        }

        public string getUOM(int currStratumCN)
        {
            var sg = DAL.From<SampleGroupDO>()
                .Where("Stratum_CN = @p1")
                .Read(currStratumCN)
                .FirstOrDefault();

            return sg.UOM;
        }

        public bool saleWithNullSpecies()
        {
            return DAL.From<TreeDO>().Where("Species is null").Count() > 0;
        }
    }
}
