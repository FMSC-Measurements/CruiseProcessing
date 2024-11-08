using CruiseProcessing.Interop;

namespace CruiseProcessing.Processing
{
    public interface ICalculateTreeValues
    {
        IVolumeLibrary VolLib { get; }

        string GetVersion();

        void ProcessTrees(string currST, string currMethod, long currST_CN);
    }
}