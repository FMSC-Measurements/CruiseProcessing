using CruiseProcessing.Data;
using CruiseProcessing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CruiseProcessing.Processing
{
    public class CruiseProcessor_20241101_Preview : CruiseProcessor
    {
        public CruiseProcessor_20241101_Preview(CpDataLayer dataLayer,
                                      IDialogService dialogService,
                                      [FromKeyedServices(nameof(CalculateTreeValues3))] ICalculateTreeValues calculateTreeValues,
                                      ILogger<CruiseProcessor_20241101_Preview> logger)
            : base(dataLayer, dialogService, calculateTreeValues, logger)
        {
            DataLayer = dataLayer;
            DialogService = dialogService;
            Logger = logger;
            TreeValCalculator = calculateTreeValues;
        }
    }
}