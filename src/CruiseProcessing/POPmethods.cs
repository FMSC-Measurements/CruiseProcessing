using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public static class POPmethods
    {
        //  methods pertaining to the POP table
        public static List<POPDO> GetCutTrees(List<POPDO> POPlist)
        {
            List<POPDO> rtrnList = POPlist.FindAll(
                delegate(POPDO P)
                {
                    return P.CutLeave == "C";
                });
            if(rtrnList != null)
                return rtrnList;

            return rtrnList;
        }   //  end GetCutTrees


        public static List<POPDO> GetStratumData(List<POPDO> POPlist, string currST, string currCL)
        {
            List<POPDO> rtrnList = new List<POPDO>();
            if (currCL != "")
            {
                rtrnList = POPlist.FindAll(
                    delegate(POPDO P)
                    {
                        return P.Stratum == currST && P.CutLeave == currCL;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }
            else if (currCL == "")
            {
                rtrnList = POPlist.FindAll(
                    delegate(POPDO P)
                    {
                        return P.Stratum == currST;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }   //  endif CutLeave
            return rtrnList;
        }   //  end GetStratumData

        public static List<POPDO> GetStratumData(List<POPDO> popList, string currST, string currPP, 
                                                    string currUOM, string currSP, string currSG)
        {
            List<POPDO> lGroup = popList.FindAll(
                delegate(POPDO p)
                {
                    return p.Stratum == currST && p.CutLeave == "C" && p.PrimaryProduct == currPP &&
                            p.SecondaryProduct == currSP && p.UOM == currUOM && p.SampleGroup == currSG;
                });
            return lGroup;
        }   //  end GetStratumData


        public static List<POPDO> GetMultipleData(List<POPDO> POPlist, string currST, string currSG, 
                                            string currPP, string currSP, string currUOM, string currCL)
        {
            List<POPDO> rtrnList = new List<POPDO>();
            if (currSP != "")           //  current secondary product
            {
                //  find records with multiple values
                rtrnList = POPlist.FindAll(
                    delegate(POPDO P)
                    {
                        return P.Stratum == currST && P.SampleGroup == currSG && P.PrimaryProduct == currPP && 
                                P.SecondaryProduct == currSP && P.UOM == currUOM;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }   //  endif currSP

            if (currCL != "" && currST != "" && currPP != "" && currUOM != "")           //  current cutleave
            {
                rtrnList = POPlist.FindAll(
                    delegate(POPDO P)
                    {
                        return P.CutLeave == currCL && P.Stratum == currST && P.SampleGroup == currSG && P.PrimaryProduct == currPP && P.UOM == currUOM;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }   //  endif currCL

            //  multiple values
            if (currST != "" && currUOM != "" && currCL != "")
            {
                rtrnList = POPlist.FindAll(
                    delegate(POPDO p)
                    {
                        return p.CutLeave == currCL && p.Stratum == currST && p.SampleGroup == currSG && p.UOM == currUOM;
                    });
                if (rtrnList != null)
                    return rtrnList;
            }   //  endif
            return rtrnList;
        }   //  end GetMultipleData

    }
}
