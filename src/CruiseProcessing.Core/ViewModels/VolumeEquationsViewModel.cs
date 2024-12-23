using CruiseDAL.DataObjects;
using CruiseProcessing.Config;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace CruiseProcessing.ViewModels
{
    public class VolumeEquationsViewModel : ObservableObject
    {
        private ObservableCollection<VolumeEquationDO> _volumeEquations;
        private IReadOnlyCollection<string> _speciesCodes;
        private IReadOnlyCollection<string> _productCodes;
        private string _region;
        private IReadOnlyCollection<string> _forestOptions;
        private string _forest;
        private IReadOnlyCollection<string> _volumeEquationNumbers;
        private bool _updatingVolumeEquation;
        private string _selectedSpecies;
        private string _selectedProduct;
        private string _selectedVolumeEquationNumber;
        private RelayCommand _insertVolumeEquationCommand;
        private RelayCommand _deleteVolumeEquationCommand;
        private VolumeEquationDO _selectedVolumeEquation;
        private RelayCommand _saveVolumeEquationsCommand;
        private RelayCommand _deleteUnusedVolumeEquations;

        public event EventHandler VolumeEquationsSaved;

        protected ILogger Log { get; }
        public CpDataLayer DataLayer { get; }
        public IDialogService DialogService { get; }
        public BiomassEquationOptions BiomassOptions { get; }
        public IVolumeLibrary VolumeLibrary { get; }

        public RelayCommand InsertVolumeEquationCommand => _insertVolumeEquationCommand ??= new RelayCommand(InsertVolumeEquation, InsertVolumeEquationCanExecute);
        public RelayCommand DeleteVolumeEquationCommand => _deleteVolumeEquationCommand ??= new RelayCommand(DeleteVolumeEquation, DeleteVolumeEquationCanExecute);
        public RelayCommand DeleteUnusedVolumeEquationsCommand => _deleteUnusedVolumeEquations ??= new RelayCommand(DeleteUnusedVolumeEquations);

        public RelayCommand SaveVolumeEquationsCommand => _saveVolumeEquationsCommand ??= new RelayCommand(() => Save());

        public bool IsCruise => DataLayer.IsTemplateFile == false;

        public string Region
        {
            get => _region;
            set
            {
                if (_region == value) { return; }

                SetProperty(ref _region, value);

                Forest = null;
                if (!string.IsNullOrEmpty(value))
                {
                    var volEqList = volumeLists.GetVolumeEquationsByRegion(value);
                    ForestOptions = volEqList.Select(x => x.vForest).Distinct().ToArray();
                }
            }
        }
        public string Forest
        {
            get => _forest;
            set
            {
                if (_forest == value) { return; }
                SetProperty(ref _forest, value);
                RefreshVolumeEquationOptions();

                InsertVolumeEquationCommand.NotifyCanExecuteChanged();
            }
        }

        public string SelectedVolumeEquationNumber
        {
            get => _selectedVolumeEquationNumber;
            set
            {
                SetProperty(ref _selectedVolumeEquationNumber, value);
                InsertVolumeEquationCommand.NotifyCanExecuteChanged();
            }
        }

        public string SelectedSpecies
        {
            get => _selectedSpecies;
            set
            {
                SetProperty(ref _selectedSpecies, value);
                InsertVolumeEquationCommand.NotifyCanExecuteChanged();
            }
        }

        public string SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                SetProperty(ref _selectedProduct, value);
                InsertVolumeEquationCommand.NotifyCanExecuteChanged();
            }
        }

        public IReadOnlyCollection<string> ForestOptions
        {
            get => _forestOptions;
            protected set => SetProperty(ref _forestOptions, value);
        }

        public IReadOnlyCollection<string> RegionOptions { get; protected set; }

        public IReadOnlyCollection<string> VolumeEquationNumbers
        {
            get => _volumeEquationNumbers;
            protected set => SetProperty(ref _volumeEquationNumbers, value);
        }

        public VolumeEquationDO SelectedVolumeEquation
        {
            get => _selectedVolumeEquation;
            set
            {
                SetProperty(ref _selectedVolumeEquation, value);
                DeleteVolumeEquationCommand.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<VolumeEquationDO> VolumeEquations
        {
            get => _volumeEquations;
            protected set
            {
                if (_volumeEquations != null)
                {
                    foreach (var ve in _volumeEquations)
                    {
                        ve.PropertyChanged -= VolumeEquation_PropertyChanged;
                    }
                }
                SetProperty(ref _volumeEquations, value);
                if (value != null)
                {
                    foreach (var ve in value)
                    {
                        ve.PropertyChanged += VolumeEquation_PropertyChanged;
                    }
                }

            }
        }
        private void VolumeEquation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_updatingVolumeEquation)
            {
                _updatingVolumeEquation = true;
                try
                {
                    UpdateVolumeEquationAutoValues((VolumeEquationDO)sender);
                }
                finally
                {
                    _updatingVolumeEquation = false;
                }
            }
        }

        public IReadOnlyCollection<string> SpeciesCodes
        {
            get => _speciesCodes;
            protected set => SetProperty(ref _speciesCodes, value);
        }
        public IReadOnlyCollection<string> ProductCodes
        {
            get => _productCodes;
            protected set => SetProperty(ref _productCodes, value);
        }

        public VolumeEquationsViewModel(CpDataLayer dataLayer, IDialogService dialogService, IVolumeLibrary volumeLibrary, IOptions<BiomassEquationOptions> biomassOptions, ILogger<VolumeEquationsViewModel> logger)
        {
            Log = logger;
            DataLayer = dataLayer;
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            BiomassOptions = biomassOptions?.Value ?? new BiomassEquationOptions();
            VolumeLibrary = volumeLibrary;

            RegionOptions = new[] { "01", "02", "03", "04", "05", "06", "10", "DOD" };
        }

        public void Initialize(string region = null, string forest = null)
        {
            Log?.LogDebug("Initializing VolumeEquationsViewModel");
            var isTemplate = DataLayer.IsTemplateFile;

            region ??= Region;
            forest ??= Forest;

            if (region == null || forest == null)
            {
                if (isTemplate)
                {
                    var result = DialogService.AskTemplateRegionForest();
                    region = result.Region;
                    forest = result.Forest;
                }
                else
                {
                    region = DataLayer.getRegion();
                    forest = DataLayer.getForest();
                }
            }


            Region = region ?? throw new ArgumentNullException(nameof(region));
            Forest = forest ?? throw new ArgumentNullException(nameof(forest));

            var volumeEquations = DataLayer.getVolumeEquations();
            VolumeEqMethods.SetSpeciesAndModelValues(volumeEquations, region);

            if (!isTemplate)
            {
                var speciesProduct = DataLayer.GetUniqueSpeciesProductFromTrees();

                foreach (var spProd in speciesProduct)
                {

                    if (!volumeEquations.Any(x => x.Species == spProd.SpeciesCode && x.PrimaryProduct == spProd.ProductCode))
                    {
                        var ved = new VolumeEquationDO
                        {
                            Species = spProd.SpeciesCode,
                            PrimaryProduct = spProd.ProductCode,
                        };
                        volumeEquations.Add(ved);
                    }
                }
            }

            VolumeEquations = new ObservableCollection<VolumeEquationDO>(volumeEquations);

            SpeciesCodes = DataLayer.GetAllSpeciesCodes().ToArray();
            ProductCodes = DataLayer.GetDistinctPrimaryProductCodes().ToArray();

            Log?.LogDebug("Done Initializing VolumeEquationsViewModel");
        }




        protected bool InsertVolumeEquationCanExecute()
        {
            return !string.IsNullOrEmpty(SelectedSpecies) && !string.IsNullOrEmpty(SelectedProduct) && !string.IsNullOrEmpty(SelectedVolumeEquationNumber);
        }

        public void InsertVolumeEquation()
        {
            InsertVolumeEquation(SelectedSpecies, SelectedProduct, SelectedVolumeEquationNumber);
        }

        public void InsertVolumeEquation(string species, string product, string volumeEquationNumber)
        {
            if (string.IsNullOrEmpty(species))
            {
                throw new ArgumentException($"'{nameof(species)}' cannot be null or empty.", nameof(species));
            }

            if (string.IsNullOrEmpty(product))
            {
                throw new ArgumentException($"'{nameof(product)}' cannot be null or empty.", nameof(product));
            }

            if (string.IsNullOrEmpty(volumeEquationNumber))
            {
                throw new ArgumentException($"'{nameof(volumeEquationNumber)}' cannot be null or empty.", nameof(volumeEquationNumber));
            }

            var ve = new VolumeEquationDO
            {
                Species = species,
                PrimaryProduct = product,
                VolumeEquationNumber = volumeEquationNumber,
            };

            UpdateVolumeEquationAutoValues(ve);

            VolumeEquations.Add(ve);

            SelectedVolumeEquationNumber = null;
            SelectedSpecies = null;
            SelectedProduct = null;
        }

        private bool DeleteVolumeEquationCanExecute()
        {
            return SelectedVolumeEquation != null;
        }

        public void DeleteVolumeEquation()
        {
            DeleteVolumeEquation(SelectedVolumeEquation);
        }

        public void DeleteVolumeEquation(VolumeEquationDO volumeEquation)
        {
            if (volumeEquation is null)
            {
                throw new ArgumentNullException(nameof(volumeEquation));
            }

            VolumeEquations.Remove(volumeEquation);
        }

        public void DeleteUnusedVolumeEquations()
        {
            if(DataLayer.IsTemplateFile == false)
            {
                var speciesProducts = DataLayer.GetUniqueSpeciesProductFromTrees().Select(x => (x.SpeciesCode, x.ProductCode)).ToArray();
                var volumeEquationsToRemove = VolumeEquations.Where(x => !speciesProducts.Contains((x.Species, x.PrimaryProduct))).ToArray();
                foreach (var ve in volumeEquationsToRemove)
                {
                    VolumeEquations.Remove(ve);
                }
            }
        }

        protected void UpdateVolumeEquationAutoValues(VolumeEquationDO veq)
        {
            var region = Region;
            var volEqList = volumeLists.GetVolumeEquationsByRegion(region);

            var volEqDefaults = volEqList.Where(x => x.vEquation == veq.VolumeEquationNumber).FirstOrDefault();

            if (volEqDefaults != null)
            {
                veq.CommonSpeciesName = volEqDefaults.vCommonName;
                veq.Model = volEqDefaults.vModelName;
            }

        }

        protected void RefreshVolumeEquationOptions()
        {
            var region = Region;
            var forest = Forest;
            if (!string.IsNullOrEmpty(region) && !string.IsNullOrEmpty(forest))
            {
                var volEqList = volumeLists.GetVolumeEquationsByRegion(region);
                var forestEqList = volEqList.Where(x => x.vForest == forest).ToArray();

                VolumeEquationNumbers = forestEqList.Select(x => x.vEquation).ToArray();
            }
            else
            {
                VolumeEquationNumbers = Array.Empty<string>();
            }
        }

        public void Save()
        {
            var errors = new List<string>();
            foreach (var volEq in VolumeEquations)
            {
                var itemErrors = ValidateVolumeEquation(volEq);
                if (itemErrors.Any())
                {
                    errors.Add($"Volume Equation {volEq.VolumeEquationNumber} {volEq.Species} {volEq.PrimaryProduct} has errors:\r\n" +
                        string.Join("\r\n", itemErrors.Select(x => "\t" + x).ToArray()));
                }
            }

            if (errors.Any())
            {
                DialogService.ShowMessage(string.Concat(errors), "Volume Equation Errors");
                return;
            }

            DataLayer.SaveVolumeEquations(VolumeEquations);

            var calcBioVolEqs = VolumeEquations.Where(x => x.CalcBiomass == 1).ToArray();

            if (calcBioVolEqs.Any())
            {
                UpdateBiomass(calcBioVolEqs);
            }
            else
            {
                //  remove all biomass equations
                DataLayer.ClearBiomassEquations();
            }

            VolumeEquationsSaved?.Invoke(this, EventArgs.Empty);
        }

        protected IReadOnlyList<string> ValidateVolumeEquation(VolumeEquationDO veq)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(veq.Species))
            {
                errors.Add("Species is required");
            }
            if (string.IsNullOrEmpty(veq.PrimaryProduct))
            {
                errors.Add("Primary Product is required");
            }
            if (string.IsNullOrEmpty(veq.VolumeEquationNumber))
            {
                errors.Add("Volume Equation Number is required");
            }
            else if (veq.VolumeEquationNumber.Length > 10)
            {
                if (veq.VolumeEquationNumber.Length > 10)
                {
                    errors.Add("Volume Equation Number must be 10 characters or less");
                }
                if (veq.VolumeEquationNumber.Contains("DVE"))
                {
                    if (veq.CalcTopwood == 1
                                && (veq.CalcBoard == 0 && veq.CalcCubic == 0 && veq.CalcCord == 0))
                    {
                        errors.Add("On DVE Volume Equation Topwood calculation requires at least one volume calculation (Calc Board, Calc Cubic, Calc Cord)");
                    }
                }
            }

            if (veq.CalcBoard == 0 && veq.CalcCubic == 0 && veq.CalcCord == 0)
            {
                errors.Add("At least one volume calculation must be selected (Calc Board, Calc Cubic, Calc Cord)");
            }
            if (veq.TopDIBSecondary > veq.TopDIBPrimary)
            {
                errors.Add("Top DIB Secondary must be less than or equal to Top DIB Primary");
            }

            return errors;
        }


        public void UpdateBiomass(IReadOnlyCollection<VolumeEquationDO> equationList, string region = null, string forest = null)
        {
            var prList = DialogService.ShowPercentRemovedDialog(equationList);


            if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(forest))
            {
                var sale = DataLayer.GetSale();
                if (sale != null)
                {
                    region = sale.Region;
                    forest = sale.Forest;
                }
            }

            if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(forest))
            {
                var trf = DialogService.AskTemplateRegionForest();

                region = trf.Region;
                forest = trf.Forest;
            }

            UpdateBiomass(region, forest, equationList, prList);
        }

        //public void UpdateBiomass(CpDataLayer dataLayer, IReadOnlyCollection<VolumeEquationDO> equationList, IReadOnlyCollection<PercentRemoved> prList, string region, string forest)
        //{
        //    if(string.IsNullOrEmpty(region)) { throw new ArgumentNullException(nameof(region)); }
        //    if(string.IsNullOrEmpty(forest)) { throw new ArgumentNullException(nameof(forest)); }

        //    UpdateBiomass(region, forest, dataLayer, equationList, prList);
        //}

        public void UpdateBiomass(string region, string forest, IReadOnlyCollection<VolumeEquationDO> equationList, IReadOnlyCollection<PercentRemoved> prList)
        {
            int REGN = Convert.ToInt32(region);

            var biomassEquations = CreateBiomassEquations(equationList, REGN, forest, prList);

            DataLayer.ClearBiomassEquations();
            DataLayer.SaveBiomassEquations(biomassEquations);
        }

        public IReadOnlyCollection<BiomassEquationDO> CreateBiomassEquations(IReadOnlyCollection<VolumeEquationDO> volEqs, int region, string forest, IReadOnlyCollection<PercentRemoved> percentRemovedValues)
        {
            var biomassEquations = new List<BiomassEquationDO>();

            //  Region 8 does things so differently -- there could be multiple volume equations for a species and product group.
            //  Because biomass equations are unique to species and product, group voleqs by species and product.
            //  February 2014
            foreach (VolumeEquationDO volEq in volEqs.Where(x => x.CalcBiomass == 1)
                .GroupBy(x => x.Species + ";" + x.PrimaryProduct).Select(x => x.First()))
            {
                var percentRemoved = percentRemovedValues.FirstOrDefault(pr => pr.bioSpecies == volEq.Species && pr.bioProduct == volEq.PrimaryProduct);
                float? percentRemovedValue = (percentRemoved != null && float.TryParse(percentRemoved.bioPCremoved, out var pct))
                    ? pct : (float?)null;



                biomassEquations.AddRange(CreateBiomassEquations(volEq, region, forest, percentRemovedValue));
            }

            return biomassEquations;
        }

        public IReadOnlyCollection<BiomassEquationDO> CreateBiomassEquations(VolumeEquationDO volEq, int region, string forest, float? percentRemovedValue)
        {
            percentRemovedValue ??= BiomassOptions.DefaultPercentRemoved;

            var treeDefaults = DataLayer.GetTreeDefaultValues(volEq.Species, volEq.PrimaryProduct);
            return MakeBiomassEquationsInternal(volEq, treeDefaults, region, forest, percentRemovedValue.Value);
        }

        public IReadOnlyCollection<BiomassEquationDO> MakeBiomassEquationsInternal(VolumeEquationDO volEq, IEnumerable<TreeDefaultValueDO> treeDefaults, int region, string forest, float percentRemovedValue)
        {
            if (!treeDefaults.Any())
            {
                Log?.LogWarning("No Tree Defaults Found For Biomass Equation VolEq:{VolumeEquationNumber} Sp:{Species} Prod:{PrimaryProduct}"
                    , volEq.VolumeEquationNumber, volEq.Species, volEq.PrimaryProduct);
            }

            var biomassEquations = new List<BiomassEquationDO>();
            // there might be multiple tree defaults for a species, product, liveDead
            // we're mostly concerned with making sure we have a biomass eq for each liveDead 
            var liveTdv = treeDefaults.FirstOrDefault(x => x.LiveDead == "L");
            if (liveTdv != null)
            {
                biomassEquations.AddRange(MakeBiomassEquationsInternal(volEq, liveTdv, region, forest, percentRemovedValue));
            }

            var deadTdv = treeDefaults.FirstOrDefault(x => x.LiveDead == "D");
            if (deadTdv != null)
            {
                biomassEquations.AddRange(MakeBiomassEquationsInternal(volEq, deadTdv, region, forest, percentRemovedValue));
            }

            return biomassEquations;
        }

        private IEnumerable<BiomassEquationDO> MakeBiomassEquationsInternal(VolumeEquationDO volEq, TreeDefaultValueDO treeDefault, int region, string forest, float percentRemovedValue)
        {
            int fiaCode = (int)treeDefault.FIAcode;
            string liveDead = treeDefault.LiveDead;

            return VolumeLibrary.MakeBiomassEquationsInternal(region, forest, fiaCode, volEq.PrimaryProduct, volEq.Species, liveDead, percentRemovedValue);
        }

        //public IReadOnlyCollection<BiomassEquationDO> MakeBiomassEquationsInternal(string species, string primaryProduct, IEnumerable<TreeDefaultValueDO> treeDefaults, int region, string forest, float percentRemovedValue)
        //{
        //    var biomassEquations = new List<BiomassEquationDO>();

        //    foreach (var tdv in treeDefaults)
        //    {
        //        int fiaCode = (int)tdv.FIAcode;
        //        string liveDead = tdv.LiveDead;

        //        biomassEquations.AddRange(MakeBiomassEquationsInternal(region, forest, fiaCode, primaryProduct, species, liveDead, percentRemovedValue));
        //    }

        //    return biomassEquations;
        //}
    }
}
