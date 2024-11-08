using CruiseDAL.DataObjects;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        public const string GLOBAL_KEY_VOLUMELIBRARY_VERSION = "VolumeLibraryVersion";
        public const string GLOBAL_KEY_VOLUMELIBRARY_TYPE = "VolumeLibraryType";
        public const string GLOBAL_KEY_TREEVALUECALCULATOR_TYPE = "TreeValueCalculatorType";


        // *******************************************************************************************
        //  variable log length from global configuration
        public string getVLL()
        {
            List<GlobalsDO> globList = DAL.From<GlobalsDO>().Where("Key = @p1 AND Block = @p2").Read("VLL", "Global").ToList();
            if (globList.Count > 0)
                return "V";
            else return "false";
        }   //  end getVLL

        public void WriteGlobalValue(string gKey, string gValue)
        {
            DAL.WriteGlobalValue("CruiseProcessing", gKey, gValue);
        }

        public string ReadGlobalValue(string gKey)
        {
            return DAL.ReadGlobalValue("CruiseProcessing", gKey);
        }
    }
}