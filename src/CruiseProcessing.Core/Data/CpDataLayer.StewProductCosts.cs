using CruiseDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        public List<StewProductCosts> getStewCosts()
        {
            // rewritten Dec 2020 - Ben

            return DAL.Query<StewProductCosts>("SELECT * FROM StewProductCosts;").ToList();
        }   //  end getStewCosts


        public void SaveStewCosts(List<StewProductCosts> stList)
        {
            foreach (StewProductCosts st in stList)
            {
                var sb = createInsertQuery(st);
                DAL.Execute(sb.ToString());

            }
                

            StringBuilder createInsertQuery(StewProductCosts st)
            {
                StringBuilder queryString = new StringBuilder();
                queryString.Append("INSERT INTO StewProductCosts ");
                queryString.Append("(costUnit,costSpecies,costProduct,costPounds,costCost,scalePC,includeInReport)");

                //  enter the actual values for the record
                queryString.Append(" VALUES ('");
                queryString.Append(st.costUnit);
                queryString.Append("','");
                queryString.Append(st.costSpecies);
                queryString.Append("','");
                queryString.Append(st.costProduct);
                queryString.Append("','");
                queryString.Append(st.costPounds);
                queryString.Append("','");
                queryString.Append(st.costCost);
                queryString.Append("','");
                queryString.Append(st.scalePC);
                queryString.Append("','");
                queryString.Append(st.includeInReport);
                queryString.Append("');");

                return queryString;
            }   //  end createInsertQuery
        }   //  end SaveStewCosts
    }
}
