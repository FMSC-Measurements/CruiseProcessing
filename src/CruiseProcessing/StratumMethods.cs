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
        //  edit check functions
        public static int IsEmpty(List<StratumDO> strList)
        {
            //  checks for empty table
            if (strList.Count == 0)
                return 25;
            else return 0;
        }   //  end IsEmpty



        //  methods pertaining to stratum table
        public static double CheckMethod(List<StratumDO> sList, string currST)
        {
            //  returns fixed plot size or basal area factor for the current stratum
            List<StratumDO> rtrnList = sList.FindAll(
                delegate(StratumDO st)
                {
                    return st.Code == currST;
                });
            if (rtrnList != null)
            {
                if (rtrnList[0].Method == "F3P" || rtrnList[0].Method == "FIX" || 
                    rtrnList[0].Method == "FIXCNT" || rtrnList[0].Method == "FCM")
                    return Convert.ToDouble(rtrnList[0].FixedPlotSize);
                else if (rtrnList[0].Method == "P3P" || rtrnList[0].Method == "PNT" ||
                         rtrnList[0].Method == "PCMTRE" || rtrnList[0].Method == "PCM" ||
                          rtrnList[0].Method == "3PPNT")
                    return Convert.ToDouble(rtrnList[0].BasalAreaFactor);
                else return 1;
            }   //  endif rtrnList not null
            return 0;
        }   //  end CheckMethod


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


        public static ArrayList buildPrintArray(StratumDO stl, string cruiseName, double totalAcres,
                                                    double numPlots)
        {
            //  parameter list will have two other fields -- strata acres and number of plots
            ArrayList stratumArray = new ArrayList();
            stratumArray.Add("   ");
            stratumArray.Add(cruiseName.PadRight(5, ' '));
            stratumArray.Add(stl.Code.PadLeft(2, ' '));

            stratumArray.Add(stl.Method.PadRight(6, ' '));
            stratumArray.Add(Utilities.Format("{0,6:F2}", totalAcres).ToString().PadLeft(6, ' '));
            stratumArray.Add(Utilities.Format("{0,7:F2}", stl.BasalAreaFactor).ToString().PadLeft(7, ' '));
            stratumArray.Add(Utilities.Format("{0,4:F0}", stl.FixedPlotSize).ToString().PadLeft(3, ' '));
            stratumArray.Add(Utilities.Format("{0,3:F0}", numPlots).ToString().PadLeft(3, ' '));
            stratumArray.Add(stl.Description ?? (" ").PadRight(25, ' '));
            stratumArray.Add(Utilities.Format("{0,2:F0}", stl.Month).ToString());
            stratumArray.Add(Utilities.Format("{0,4:F0}", stl.Year).ToString());

            return stratumArray;
        }   //  end buildPrintArray


        public static ArrayList buildPrintArray(TreeCalculatedValuesDO tcv, string currUOM, ref int firstLine)
        {
            //  currently for VSM4 (CP4) report only
            ArrayList prtArray = new ArrayList();

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
            prtArray.Add(Utilities.Format("{0,5:F1}", tcv.Tree.DBH).ToString().PadLeft(5, ' '));
            
            //  volumes used depend on current UOM
            double currVolume = 0;
            switch (currUOM)
            {
                case "03":
                    prtArray.Add(Utilities.Format("{0,7:F1}", tcv.GrossCUFTPP));
                    prtArray.Add(Utilities.Format("{0,7:F1}", tcv.NetCUFTPP));
                    currVolume = tcv.NetCUFTPP;
                    break;
                case "01":
                    prtArray.Add(Utilities.Format("{0,7:F1}", tcv.GrossBDFTPP));
                    prtArray.Add(Utilities.Format("{0,7:F1}", tcv.NetBDFTPP));
                    currVolume = tcv.NetBDFTPP;
                    break;
                case "05":
                    prtArray.Add(Utilities.Format("{0,7:F1}", tcv.BiomassMainStemPrimary));
                    prtArray.Add(Utilities.Format("{0,7:F1}", tcv.BiomassMainStemPrimary));
                    currVolume = tcv.BiomassMainStemPrimary;
                    break;
                case "02":
                    prtArray.Add(Utilities.Format("{0,7:F1}", tcv.CordsPP));
                    prtArray.Add(Utilities.Format("{0,7:F1}", tcv.CordsPP));
                    currVolume = tcv.CordsPP;
                    break;
            }   //  end switch on UOM

            prtArray.Add(Utilities.Format("{0,8:F2}", tcv.Tree.ExpansionFactor));
            prtArray.Add(tcv.Tree.KPI.ToString().PadLeft(5, ' '));
            //  April 2017 --  separate KPI from count table since it includes measured KPI
            prtArray.Add("     ");
            prtArray.Add(tcv.Tree.TreeCount.ToString().PadLeft(6, ' '));
            
            //  calculate ratio
            if (tcv.Tree.KPI > 0)
                prtArray.Add(Utilities.Format("{0,7:F3}", (currVolume / tcv.Tree.KPI)));
            else prtArray.Add("       ");
            //  and finally marker's initials
            if (tcv.Tree != null && tcv.Tree.Initials != null)
                prtArray.Add(tcv.Tree.Initials.PadRight(3, ' '));
            else prtArray.Add("   ");

            return prtArray;
        }   //  end buildPrintArray
    }
}
