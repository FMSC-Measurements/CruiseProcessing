using CruiseProcessing.Data;
using Microsoft.Toolkit.Mvvm.ComponentModel;

#nullable enable

namespace CruiseProcessing.Services
{
    public class DataLayerContext : ObservableObject
    {
        private CpDataLayer? _dataLayer;

        public CpDataLayer? DataLayer
        {
            get => _dataLayer;
            set => SetProperty(ref _dataLayer, value);
        }
    }
}