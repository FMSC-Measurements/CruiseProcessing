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
        public static IEnumerable<POPDO> GetCutTrees(IEnumerable<POPDO> POPlist)
        {
            return POPlist.Where(P => P.CutLeave == "C");
        }   //  end GetCutTrees


        public static IEnumerable<POPDO> GetStratumData(IEnumerable<POPDO> POPlist, string currST, string currCL)
        {
            if (currCL != "")
            {
                return POPlist.Where(p => p.Stratum == currST && p.CutLeave == currCL);
            }
            else if (currCL == "")
            {
                return POPlist.Where(p => p.Stratum == currST);
            }   //  endif CutLeave
            return new List<POPDO>();
        }   //  end GetStratumData

        public static IEnumerable<POPDO> GetStratumData(IEnumerable<POPDO> popList, string currST, string currPP, 
                                                    string currUOM, string currSP, string currSG)
        {
            return popList.Where(p => p.Stratum == currST && p.CutLeave == "C" && p.PrimaryProduct == currPP &&
                            p.SecondaryProduct == currSP && p.UOM == currUOM && p.SampleGroup == currSG);
        }   //  end GetStratumData


        public static IEnumerable<POPDO> GetMultipleData(IEnumerable<POPDO> POPlist, string currST, string currSG, 
                                            string currPP, string currSP, string currUOM, string currCL)
        {
            if (currSP != "")           //  current secondary product
            {
                //  find records with multiple values
                return POPlist.Where(P => P.Stratum == currST && P.SampleGroup == currSG && P.PrimaryProduct == currPP && 
                                P.SecondaryProduct == currSP && P.UOM == currUOM);
            }   //  endif currSP

            if (currCL != "" && currST != "" && currPP != "" && currUOM != "")           //  current cutleave
            {
                return POPlist.Where(P => P.CutLeave == currCL && P.Stratum == currST && P.SampleGroup == currSG && P.PrimaryProduct == currPP && P.UOM == currUOM);
            }   //  endif currCL

            //  multiple values
            if (currST != "" && currUOM != "" && currCL != "")
            {
                return POPlist.Where(p => p.CutLeave == currCL && p.Stratum == currST && p.SampleGroup == currSG && p.UOM == currUOM);
            }   //  endif

            return new List<POPDO>();
        }   //  end GetMultipleData

    }
}
