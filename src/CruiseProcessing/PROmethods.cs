using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{

    public static class PROmethods
    {
        //  methods pertaining to the PRO table
        public static List<PRODO> GetCutTrees(List<PRODO> PROlist, string currST, string currSG, 
                                            string currCU, int justCuts)
        {
            List<PRODO> rtrnList = new List<PRODO>();
            if (currST != "" && currSG == "" && currCU == "" && justCuts == 1)
            {
                rtrnList = PROlist.FindAll(
                    delegate(PRODO P)
                    {
                        return P.Stratum == currST && P.CutLeave == "C";
                    });
                if (rtrnList != null)
                    return rtrnList;
            }
            else if (currST != "" && justCuts == 0)
            {
                rtrnList = PROlist.FindAll(
                    delegate(PRODO P)
                    {
                        return P.Stratum == currST;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }
            else if (currST != "" && currSG != "" && currCU == "")
            {
                rtrnList = PROlist.FindAll(
                    delegate(PRODO P)
                    {
                        return P.CutLeave == "C" && P.Stratum == currST && P.SampleGroup == currSG;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }
            else
            {
                rtrnList = PROlist.FindAll(
                    delegate(PRODO P)
                    {
                        return P.CutLeave == "C" && P.Stratum == currST && P.CuttingUnit == currCU && P.SampleGroup == currSG;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }   //  endif

            return rtrnList;
        }   //  end GetCutTrees


        public static List<PRODO> GetCutTrees(List<PRODO> PROlist, string currST, string currSG, string currUOM, 
                                                string currSTM)
        {
            List<PRODO> rtrnList = new List<PRODO>();
            rtrnList = PROlist.FindAll(
                delegate(PRODO pdo)
                {
                    return pdo.CutLeave == "C" && pdo.Stratum == currST && pdo.SampleGroup == currSG &&
                                pdo.UOM == currUOM && pdo.STM == currSTM;
                });
            return rtrnList;
        }   //  end GetCutTrees


        public static List<PRODO> GetMultipleData(List<PRODO> PROlist, string currCL, string currST, 
                                                    string currCU, string currSG, string currPP, 
                                                    string currUOM, string currSTM, int whichGroup)
        {
            //  find current values based on which group needed
            List<PRODO> rtrnList = new List<PRODO>();
            switch (whichGroup)
            {
                case 1:         //  cutleave, stratum, cutting unit and sample group and STM
                    rtrnList = PROlist.FindAll(
                        delegate(PRODO P)
                        {
                            return P.CutLeave == currCL && P.Stratum == currST && P.CuttingUnit == currCU && 
                                    P.SampleGroup == currSG && P.STM == currSTM;
                        });
                    if (rtrnList != null)
                        return rtrnList;
                    break;
                case 2:         //  cutleave, stratum, cutting unit, sample group and primary product
                    rtrnList = PROlist.FindAll(
                        delegate(PRODO P)
                        {
                            return P.CuttingUnit == currCL && P.Stratum == currST && P.CuttingUnit == currCU && P.SampleGroup == currSG && P.PrimaryProduct == currPP;
                        });
                    if (rtrnList != null)
                        return rtrnList;
                    break;
                case 3:         //  all of the above plus uom
                    rtrnList = PROlist.FindAll(
                        delegate(PRODO P)
                        {
                            return P.CuttingUnit == currCL && P.Stratum == currST && P.CuttingUnit == currCU && P.SampleGroup == currSG && P.PrimaryProduct == currPP && P.UOM == currUOM;
                        });
                    if (rtrnList != null)
                        return rtrnList;
                    break;
                case 4:         //  stratum and cutting unit
                    rtrnList = PROlist.FindAll(
                        delegate(PRODO P)
                        {
                            return P.Stratum == currST && P.CuttingUnit == currCU;
                        });
                    if (rtrnList != null)
                        return rtrnList;
                    break;
                case 5:         //  stratum, cutting unit and sample group
                    rtrnList = PROlist.FindAll(
                        delegate(PRODO P)
                        {
                            return P.Stratum == currST && P.CuttingUnit == currCU && P.SampleGroup == currSG;
                        });
                    if (rtrnList != null)
                        return rtrnList;
                    break;
                case 6:         //  cutting unit and sample group
                    rtrnList = PROlist.FindAll(
                        delegate(PRODO P)
                        {
                            return P.CuttingUnit == currCU && P.SampleGroup == currSG;
                        });
                    if (rtrnList != null)
                        return rtrnList;
                    break;
            }   //  end switch

            return rtrnList;
        }   //  end GetMultipleData
    }
}
