using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class BiomassList
    {
        //  Biomass equations
        public string bioSpecies { get; set; }
        public string bioProduct { get; set; }
        public string bioComponent { get; set; }
        public string bioEquation { get; set; }
        public double bioPercentMoisture { get; set; }
        public double bioPercentRemoved { get; set; }
        public string bioMetadata { get; set; }


        public StringBuilder createInsertQuery(BiomassList bio)
        {
            StringBuilder queryString = new StringBuilder();
            queryString.Append("INSERT INTO BiomassEquation ");
            queryString.Append("(Species,Product,Component,Equation,PercentMoisture,PercentRemoved,MetaData)");

            //  enter the actual values for the record
            queryString.Append(" VALUES ('");
            queryString.Append(bio.bioSpecies);
            queryString.Append("','");
            queryString.Append(bio.bioProduct);
            queryString.Append("','");
            queryString.Append(bio.bioComponent);
            queryString.Append("','");
            queryString.Append(bio.bioEquation);
            queryString.Append("','");
            queryString.Append(bio.bioPercentMoisture.ToString());
            queryString.Append("','");
            queryString.Append(bio.bioPercentRemoved.ToString());
            queryString.Append("','");
            queryString.Append(bio.bioMetadata);
            queryString.Append("');");
            
            return queryString;
        }   //  end createInsertQuery


        public string[] buildPrintArray(BiomassList bel)
        {
            string[] bioArray = new string[10];
            return bioArray;
        }   //  end buildPrintArray
    }
}
