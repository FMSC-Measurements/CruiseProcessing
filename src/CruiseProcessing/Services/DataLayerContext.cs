using Microsoft.Toolkit.Mvvm.ComponentModel;

#nullable enable

namespace CruiseProcessing.Services
{
    public class DataLayerContext : ObservableObject
    {
        private CPbusinessLayer? _dataLayer;

        public CPbusinessLayer? DataLayer
        {
            get => _dataLayer;
            set => SetProperty(ref _dataLayer, value);
        }
    }
}