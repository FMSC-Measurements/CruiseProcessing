using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class StewProductCosts
    {
        public string costUnit { get; set; }
        public string costSpecies { get; set; }
        public string costProduct { get; set; }
        public float costPounds { get; set; }
        public float costCost { get; set; }
        public float scalePC { get; set; }
        public string includeInReport { get; set; }


        public StringBuilder createInsertQuery(StewProductCosts st)
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
    }
}
