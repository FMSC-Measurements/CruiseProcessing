using CruiseProcessing.Data;
using CruiseProcessing.Services;
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
        public NextVerCruiseProcessor(CpDataLayer dataLayer, IDialogService dialogService, ILogger<NextVerCruiseProcessor> logger, ILogger<CalculateTreeValues_20241101> ctvLogger)
        {
            DataLayer = dataLayer;
            DialogService = dialogService;
            Logger = logger;
            TreeValCalculator = new CalculateTreeValues_20241101(dataLayer, ctvLogger);
        }
    }
}
