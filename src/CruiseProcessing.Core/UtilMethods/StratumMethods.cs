using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;


namespace CruiseProcessing
{
    public static class StratumMethods
    {
        public static double GetBafOrFps(StratumDO st)
        {
            var method = st.Method;
            if (method == "F3P"
                || method == "FIX"
                || method == "FIXCNT"
                || method == "FCM")
            { return Convert.ToDouble(st.FixedPlotSize); }
            else if (method == "P3P"
                || method == "PNT"
                || method == "PCMTRE"
                || method == "PCM"
                || method == "3PPNT")
            { return Convert.ToDouble(st.BasalAreaFactor); }

            return 1;

        }


        public static int GetStratumCN(string currST, List<StratumDO> sList)
        {
            int nthRow = sList.FindIndex(
                delegate(StratumDO s)
                {
                    return s.Code == currST;
                });
            if (nthRow >= 0)
                return (int)sList[nthRow].Stratum_CN;
            return -1;
        }   //  end GetStratumCN





        public static List<string> buildPrintArray(TreeCalculatedValuesDO tcv, string currUOM, ref int firstLine)
        {
            //  currently for VSM4 (CP4) report only
            var prtArray = new List<string>();

            prtArray.Add("");
            if (firstLine == 0)
            {
                prtArray.Add(tcv.Tree.Stratum.Code.PadLeft(2, ' '));
                prtArray.Add(tcv.Tree.SampleGroup.Code.PadLeft(2,' '));
                firstLine = 1;
            }
            else
            {
                prtArray.Add("  ");
                prtArray.Add("  ");
            }   //  endif firstLine

            prtArray.Add(tcv.Tree.CuttingUnit.Code.PadLeft(3, ' '));
            if(tcv.Tree != null && tcv.Tree.Plot != null)
                prtArray.Add(tcv.Tree.Plot.PlotNumber.ToString().PadLeft(4, ' '));
            else prtArray.Add("    ");
            prtArray.Add(tcv.Tree.TreeNumber.ToString().PadLeft(4, ' '));
            prtArray.Add(tcv.Tree.Species.PadRight(6, ' '));
            prtArray.Add(String.Format("{0,5:F1}", tcv.Tree.DBH).PadLeft(5, ' '));
            
            //  volumes used depend on current UOM
            double currVolume = 0;
            switch (currUOM)
            {
                case "03":
                    prtArray.Add(String.Format("{0,7:F1}", tcv.GrossCUFTPP));
                    prtArray.Add(String.Format("{0,7:F1}", tcv.NetCUFTPP));
                    currVolume = tcv.NetCUFTPP;
                    break;
                case "01":
                    prtArray.Add(String.Format("{0,7:F1}", tcv.GrossBDFTPP));
                    prtArray.Add(String.Format("{0,7:F1}", tcv.NetBDFTPP));
                    currVolume = tcv.NetBDFTPP;
                    break;
                case "05":
                    prtArray.Add(String.Format("{0,7:F1}", tcv.BiomassMainStemPrimary));
                    prtArray.Add(String.Format("{0,7:F1}", tcv.BiomassMainStemPrimary));
                    currVolume = tcv.BiomassMainStemPrimary;
                    break;
                case "02":
                    prtArray.Add(String.Format("{0,7:F1}", tcv.CordsPP));
                    prtArray.Add(String.Format("{0,7:F1}", tcv.CordsPP));
                    currVolume = tcv.CordsPP;
                    break;
            }   //  end switch on UOM

            prtArray.Add(String.Format("{0,8:F2}", tcv.Tree.ExpansionFactor));
            prtArray.Add(tcv.Tree.KPI.ToString().PadLeft(5, ' '));
            //  April 2017 --  separate KPI from count table since it includes measured KPI
            prtArray.Add("     ");
            prtArray.Add(tcv.Tree.TreeCount.ToString().PadLeft(6, ' '));
            
            //  calculate ratio
            if (tcv.Tree.KPI > 0)
                prtArray.Add(String.Format("{0,7:F3}", (currVolume / tcv.Tree.KPI)));
            else prtArray.Add("       ");
            //  and finally marker's initials
            if (tcv.Tree != null && tcv.Tree.Initials != null)
                prtArray.Add(tcv.Tree.Initials.PadRight(3, ' '));
            else prtArray.Add("   ");

            return prtArray;
        }   //  end buildPrintArray
    }
}
