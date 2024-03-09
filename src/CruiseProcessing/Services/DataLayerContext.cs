using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Services
{
    public class DataLayerContext
    {
        private CPbusinessLayer _dataLayer;

        public CPbusinessLayer DataLayer
        {
            get => _dataLayer;
            set => _dataLayer = value;
        }
    }
}
