using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruiseProcessing
{
    public class RegressGroups
    {
        public string rgSpecies { get; set; }
        public string rgProduct { get; set; }
        public string rgLiveDead { get; set; }
        public int rgSelected { get; set; }
    }   //  end RegressGroups


    public class RegressionResults
    {
        public string rVolume { get; set; }
        public string rVolType { get; set; }
        public string rSpecies { get; set; }
        public string rProduct { get; set; }
        public string rLiveDead { get; set; }
        public double CoefficientA { get; set; }
        public double CoefficientB { get; set; }
        public double CoefficientC { get; set; }
        public float TotalTrees { get; set; }
        public float MeanSE { get; set; }
        public float RSquared { get; set; }
        public string RegressModel { get; set; }
        public float rMinDBH { get; set; }
        public float rMaxDBH { get; set; }


        public StringBuilder createInsertQuery(RegressionResults rr)
        {
            StringBuilder qString = new StringBuilder();
            qString.Append("INSERT INTO RegressionResults ");
            qString.Append("(rVolume,rVolType,rSpecies,rProduct,rLiveDead,CoefficientA,CoefficientB,CoefficientC,TotalTrees,MeanSE,RSquared,RegressModel,rMinDBH,rMaxDBH)");
            qString.Append(" VALUES ('");
            qString.Append(rr.rVolume);
            qString.Append("','");
            qString.Append(rr.rVolType);
            qString.Append("','");
            qString.Append(rr.rSpecies);
            qString.Append("','");
            qString.Append(rr.rProduct);
            qString.Append("','");
            qString.Append(rr.rLiveDead);
            qString.Append("','");
            qString.Append(rr.CoefficientA.ToString());
            qString.Append("','");
            qString.Append(rr.CoefficientB.ToString());
            qString.Append("','");
            qString.Append(rr.CoefficientC.ToString());
            qString.Append("','");
            qString.Append(rr.TotalTrees.ToString());
            qString.Append("','");
            qString.Append(rr.MeanSE.ToString());
            qString.Append("','");
            qString.Append(rr.RSquared.ToString());
            qString.Append("','");
            qString.Append(rr.RegressModel);
            qString.Append("','");
            qString.Append(rr.rMinDBH.ToString());
            qString.Append("','");
            qString.Append(rr.rMaxDBH.ToString());
            qString.Append("');");

            return qString;
        }   //  end createInsertQuery
    }   //  end RegressionResults
}
