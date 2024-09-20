using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        public List<PlotDO> getPlots()
        {
            return DAL.From<PlotDO>().OrderBy("PlotNumber").Read().ToList();
        }   //  end getPlots


        public List<PlotDO> GetPlotsByStratum(string currStratum)
        {
            return DAL.From<PlotDO>()
                .Join("Stratum AS st", "USING (Stratum_CN)")
                .Where("st.Code = @p1")
                .Read(currStratum)
                .ToList();
        }   //  end GetStrataPlots


        public List<PlotDO> getPlotsOrdered()
        {
            return DAL.From<PlotDO>()
                .Join("Stratum AS st", "USING (Stratum_CN)")
                .Join("CuttingUnit AS cu", "USING (CuttingUnit_CN)")
                .OrderBy("st.Code", "cu.Code", "PlotNumber")
                .Read().ToList();
        }   //  end getPlotsOrdered
    }
}
