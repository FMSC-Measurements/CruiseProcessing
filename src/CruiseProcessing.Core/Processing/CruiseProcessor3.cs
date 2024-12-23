using CruiseProcessing.Data;
using CruiseProcessing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CruiseProcessing.Processing
{
    public class CruiseProcessor3 : CruiseProcessor
    {
        public CruiseProcessor3(CpDataLayer dataLayer,
                                      IDialogService dialogService,
                                      ILogger<CruiseProcessor3> logger)
            : base(dataLayer, dialogService, new CalculateTreeValues3(dataLayer), logger)
        {
        }

        protected CruiseProcessor3(CpDataLayer dataLayer,
                                      IDialogService dialogService,
                                      [FromKeyedServices(nameof(CalculateTreeValues3))] ICalculateTreeValues calculateTreeValues,
                                      ILogger<CruiseProcessor3> logger)
            : base(dataLayer, dialogService, calculateTreeValues, logger)
        {
        }
    }
}