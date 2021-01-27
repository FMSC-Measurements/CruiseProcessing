using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    class ProcessCruiseTrees
    {
        public CruiseDAL.DAL DAL;
        public string fileName;

        public void ProcessCruise(string currST, string currMethod, long currST_CN)
        {
            CalculateTreeValues calcT = new CalculateTreeValues();
            calcT.bslyr.DAL = DAL;
            calcT.bslyr.fileName = fileName;
            calcT.fileName = fileName;
            calcT.ProcessTrees(currST, currMethod, currST_CN);


            return;
        }   //  end ProcessTrees

    }
}
