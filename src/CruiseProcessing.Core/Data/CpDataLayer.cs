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

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer : ObservableObject, IErrorLogDataService, IDisposable
    {
        private bool _isProcessed;
        private bool disposedValue;

        public string CruiseID { get; } // for v3 files
        public string FilePath { get; }
        public DAL DAL { get; }
        public CruiseDatastore_V3 DAL_V3 { get; }
        public string CPVersion { get; }
        public string VolLibVersion { get; }

        public bool IsTemplateFile { get; }

        protected ILogger Log { get; }

        public bool IsProcessed
        {
            get => _isProcessed;
            set => SetProperty(ref _isProcessed, value);
        }

        public CpDataLayer(DAL dal, ILogger<CpDataLayer> logger, bool isTemplateFile = false)
        {
            DAL = dal;
            FilePath = DAL.Path;
            IsTemplateFile = isTemplateFile;
            Log = logger;

            var verson = Assembly.GetExecutingAssembly().GetName().Version.ToString(3); // only get the major.minor.build components of the version
            CPVersion = DateTime.Parse(verson).ToString("MM.dd.yyyy");
            VolLibVersion = Utilities.CurrentDLLversion();
            IsTemplateFile = isTemplateFile;
        }

        public CpDataLayer(DAL dal, CruiseDatastore_V3 dal_V3, string cruiseID, ILogger<CpDataLayer> logger, bool isTemplateFile = false)
            : this(dal, logger, isTemplateFile)
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

        // *******************************************************************************************
        //  variable log length from global configuration
        public string getVLL()
        {
            List<GlobalsDO> globList = DAL.From<GlobalsDO>().Where("Key = @p1 AND Block = @p2").Read("VLL", "Global").ToList();
            if (globList.Count > 0)
                return "V";
            else return "false";
        }   //  end getVLL

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