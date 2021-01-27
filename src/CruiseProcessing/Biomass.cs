using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class Biomass
    {
        //  Data for biomass report
        public string userStratum { get; set; }
        public string userSG { get; set; }
        public string userSpecies { get; set; }
        public string bioSpecies { get; set; }
        public string needles {get;set;}
        public string quarterInch { get; set; }
        public string oneInch { get; set; }
        public string threeInch { get; set; }
        public string threePlus { get; set; }
        public string topwoodDryWeight { get; set; }
        public string cullLogWgt { get; set; }
        public string cullChunkWgt { get; set; }
        public string FLIW { get; set; }
        public string DSTneedles { get; set; }
        public string DSTquarterInch { get; set; }
        public string DSToneInch { get; set; }
        public string DSTthreeInch { get; set; }
        public string DSTthreePlus { get; set; }
        public string DSTincluded { get; set; }


        public StringBuilder createInsertQuery(Biomass bms)
        {
            //  used for current CP report (Brown's equations) --  subject to change in the future
            StringBuilder queryString = new StringBuilder();
            queryString.Append("INSERT INTO Biomass ");
            queryString.Append("(userStratum,userSG,userSpecies,bioSpecies,needles,quarterInch,oneInch,");
            queryString.Append("threeInch,threePlus,topwoodDryWeight,cullLogWgt,cullChunkWgt,FLIW,");
            queryString.Append("DSTneedles,DSTquarterInch,DSToneInch,DSTthreeInch,DSTthreePlus,DSTincluded)");

            //  enter the actual values for the record
            queryString.Append(" VALUES ('");
            queryString.Append(bms.userStratum);
            queryString.Append("','");
            queryString.Append(bms.userSG);
            queryString.Append("','");
            queryString.Append(bms.userSpecies);
            queryString.Append("','");
            queryString.Append(bms.bioSpecies);
            queryString.Append("','");
            queryString.Append(bms.needles);
            queryString.Append("','");
            queryString.Append(bms.quarterInch);
            queryString.Append("','");
            queryString.Append(bms.oneInch);
            queryString.Append("','");
            queryString.Append(bms.threeInch);
            queryString.Append("','");
            queryString.Append(bms.threePlus);
            queryString.Append("','");
            queryString.Append(bms.topwoodDryWeight);
            queryString.Append("','");
            queryString.Append(bms.cullLogWgt);
            queryString.Append("','");
            queryString.Append(bms.cullChunkWgt);
            queryString.Append("','");
            queryString.Append(bms.FLIW);
            queryString.Append("','");
            queryString.Append(bms.DSTneedles);
            queryString.Append("','");
            queryString.Append(bms.DSTquarterInch);
            queryString.Append("','");
            queryString.Append(bms.DSToneInch);
            queryString.Append("','");
            queryString.Append(bms.DSTthreeInch);
            queryString.Append("','");
            queryString.Append(bms.DSTthreePlus);
            queryString.Append("','");
            queryString.Append(bms.DSTincluded);
            queryString.Append("');");

            return queryString;
        }   // end createInsertQuery
    }   //  end Biomass
}
