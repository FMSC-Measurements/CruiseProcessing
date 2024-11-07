using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Processing
{
    public class NextVerCruiseProcessor : CruiseProcessor
    {
        //public NextVerCruiseProcessor(CpDataLayer dataLayer,
        //                              IDialogService dialogService,
        //                              [FromKeyedServices(nameof(VolumeLibrary_20241101))] IVolumeLibrary volumeLibrary,
        //                              ILogger<NextVerCruiseProcessor> logger,
        //                              ILogger<CalculateTreeValues_20241101> ctvLogger)
        //{
        //    DataLayer = dataLayer;
        //    DialogService = dialogService;
        //    Logger = logger;
        //    TreeValCalculator = new CalculateTreeValues_20241101(dataLayer, volumeLibrary, ctvLogger);
        //}

        public NextVerCruiseProcessor(CpDataLayer dataLayer,
                                      IDialogService dialogService,
                                      [FromKeyedServices(nameof(CalculateTreeValues_20241101))] ICalculateTreeValues calculateTreeValues,
                                      ILogger<NextVerCruiseProcessor> logger)
            : base(dataLayer, dialogService, calculateTreeValues, logger)
        {
            DataLayer = dataLayer;
            DialogService = dialogService;
            Logger = logger;
            TreeValCalculator = calculateTreeValues;
        }
    }
}
