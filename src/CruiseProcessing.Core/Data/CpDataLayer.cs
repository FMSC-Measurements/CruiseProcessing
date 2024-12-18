using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL;
using System;
using CruiseProcessing.Output;
using System.Reflection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using CruiseProcessing.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CruiseProcessing.Config;
using CruiseProcessing.Interop;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer : ObservableObject, IErrorLogDataService, IDisposable
    {
        private bool _isProcessed;
        private bool disposedValue;
        private string _volumeLibraryVersion;

        public string CruiseID { get; } // for v3 files
        public string FilePath { get; }
        public DAL DAL { get; }
        public CruiseDatastore_V3 DAL_V3 { get; }
        public string CPVersion { get; }
        public string VolLibVersion
        {
            get => _volumeLibraryVersion;
            set => _volumeLibraryVersion = value;
        }

        public bool IsTemplateFile { get; }

        protected ILogger Log { get; }

        public bool IsProcessed
        {
            get => _isProcessed;
            set => SetProperty(ref _isProcessed, value);
        }

        // for unit test mocking
        protected CpDataLayer()
        {
            BiomassOptions = new BiomassEquationOptions();
        }

        public CpDataLayer(DAL dal, ILogger<CpDataLayer> logger, IOptions<BiomassEquationOptions> biomassOptions, bool isTemplateFile = false)
        {
            DAL = dal;
            FilePath = DAL.Path;
            IsTemplateFile = isTemplateFile;
            Log = logger;
            BiomassOptions = biomassOptions?.Value ?? new BiomassEquationOptions();

            var verson = Assembly.GetExecutingAssembly().GetName().Version.ToString(3); // only get the major.minor.build components of the version
            CPVersion = DateTime.Parse(verson).ToString("MM.dd.yyyy");
            //VolLibVersion = Utilities.CurrentDLLversion();
            IsTemplateFile = isTemplateFile;
        }

        public CpDataLayer(DAL dal, CruiseDatastore_V3 dal_V3, string cruiseID, ILogger<CpDataLayer> logger, IOptions<BiomassEquationOptions> biomassOptions, bool isTemplateFile = false)
            : this(dal, logger, biomassOptions, isTemplateFile)
        {
            if (DAL_V3 != null && string.IsNullOrEmpty(cruiseID)) { throw new InvalidOperationException("v3 DAL was set, expected CruiseID"); }
            
            DAL_V3 = dal_V3;
            CruiseID = cruiseID;
        }

        public string GetGraphsFolderPath()
        {
            var fileDirectory = System.IO.Path.GetDirectoryName(FilePath);
            var sale = GetSale();
            var graphsFolderPath = System.IO.Path.Combine(fileDirectory, "Graphs", sale.Name);
            return graphsFolderPath;
        }

        public List<QualityAdjEquationDO> getQualAdjEquations()
        {
            return DAL.From<QualityAdjEquationDO>().Read().ToList();
        }   //  end getQualAdjEquations

        public List<TreeEstimateDO> getTreeEstimates()
        {
            return DAL.From<TreeEstimateDO>().Read().ToList();
        }   //  end getTreeEstimates



        public void SaveQualityAdjEquations(List<QualityAdjEquationDO> qaEquationList)
        {
            //  need to delete equations in order to update the database
            //  not sure why this has to happen but the way the DAL save works
            //  is if the user deleted an equation, the Save does not consider that
            //  when the equation list is updated ???????
            List<QualityAdjEquationDO> qaList = getQualAdjEquations();
            foreach (QualityAdjEquationDO qdo in qaList)
            {
                int nthRow = qaEquationList.FindIndex(
                    delegate (QualityAdjEquationDO qed)
                    {
                        return qed.QualityAdjEq == qdo.QualityAdjEq && qed.Species == qdo.Species;
                    });
                if (nthRow < 0)
                    qdo.Delete();
            }   //  end foreach loop
            foreach (QualityAdjEquationDO qae in qaEquationList)
            {
                if (qae.DAL == null)
                {
                    qae.DAL = DAL;
                }
                qae.Save();
            }   //  end foreach loop

            return;
        }   //  end SaveQualityAdjEquations

        public List<RegressGroups> GetUniqueSpeciesGroups()
        {
            // TODO optimize

            //  used for Local Volume table

            List<TreeDO> tList = DAL.From<TreeDO>().Where("CountOrMeasure = 'M'").GroupBy("Species", "SampleGroup_CN", "LiveDead").Read().ToList();

            List<RegressGroups> rgList = new List<RegressGroups>();
            foreach (TreeDO t in tList)
            {
                if (!rgList.Exists(r => r.rgSpecies == t.Species && r.rgLiveDead == t.LiveDead && r.rgProduct == t.SampleGroup.PrimaryProduct))
                {
                    RegressGroups r = new RegressGroups();
                    r.rgSpecies = t.Species;
                    r.rgProduct = t.SampleGroup.PrimaryProduct;
                    r.rgLiveDead = t.LiveDead;
                    r.rgSelected = 0;
                    rgList.Add(r);
                }
            }   //  end foreach loop

            return rgList;
        }   //  end GetUniqueSpeciesGroups

        public IReadOnlyCollection<SpeciesProductLiveDead> GetDesignedAndActualSpeciesProductGroups(string species, string product)
        {
            return DAL.Query<SpeciesProductLiveDead>(
@"SELECT
    sg.PrimaryProduct AS Product,
    tdv.Species,
    tdv.FIAcode,
    tdv.LiveDead
FROM SampleGroupTreeDefaultValue AS sgtdv
JOIN SampleGroup AS sg USING (SampleGroup_CN)
JOIN TreeDefaultValue AS tdv USING (TreeDefaultValue_CN)
JOIN Stratum AS st USING (Stratum_CN)
WHERE tdv.Species = @p1 AND sg.PrimaryProduct = @p2

UNION

SELECT
    sg.PrimaryProduct AS Product,
    tdv.Species,
    tdv.FIAcode,
    t.LiveDead
FROM Tree AS t
JOIN SampleGroup AS sg USING (SampleGroup_CN)
JOIN Stratum AS st USING (Stratum_CN)
JOIN TreeDefaultValue AS tdv USING (TreeDefaultValue_CN)
WHERE tdv.Species = @p1 AND sg.PrimaryProduct = @p2
", species, product).ToArray();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state
                    DAL.Dispose();
                    DAL_V3?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}