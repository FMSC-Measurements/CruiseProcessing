using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Services
{
    public class DataLayerContext : ObservableObject
    {
        private CPbusinessLayer _dataLayer;

        public CPbusinessLayer DataLayer
        {
            get => _dataLayer;
            set => SetProperty(ref _dataLayer, value);
        }
    }
}
