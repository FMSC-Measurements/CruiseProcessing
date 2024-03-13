using CruiseProcessing.Services;
using Microsoft.AppCenter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using CruiseProcessing.Services.Logging;

namespace CruiseProcessing
{
    static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static DataLayerContext DataLayerContext { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if !DEBUG
            Microsoft.AppCenter.AppCenter.Start(Secrets.CRUISEPROCESSING_APPCENTER_KEY_WINDOWS,
                               typeof(Microsoft.AppCenter.Analytics.Analytics), typeof(Microsoft.AppCenter.Crashes.Crashes));
#else
            ApplicationLifecycleHelper.Instance.UnhandledExceptionOccurred += Instance_UnhandledExceptionOccurred;

            static void Instance_UnhandledExceptionOccurred(object sender, Microsoft.AppCenter.Utils.UnhandledExceptionOccurredEventArgs e)
            {
                Debug.WriteLine("UNHANDLED APPLICATION EXCEPTION::::\r\n" + e.Exception);
            }
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => ConfigureServices(context, services))
                .ConfigureLogging(ConfigureLogging).Build();

            DataLayerContext = host.Services.GetRequiredService<DataLayerContext>();
            ServiceProvider = host.Services;

            Application.Run(ServiceProvider.GetRequiredService<MainMenu>());
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.AddAppCenterLogger();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            //example
            //services.AddTransient<IMyService, MyService>();
            //services.AddTransient<MyForm>();

            // register all forms
            services.AddSingleton<MainMenu>();

            services.RegisterForm<CapturePercentRemoved>();
            services.RegisterForm<GraphForm>();

            services.RegisterForm<graphOutputDialog>();
            services.RegisterForm<GraphReportsDialog>();
            services.RegisterForm<LocalVolume>();
            services.RegisterForm<LogMatrixUpdate>();
            
            services.RegisterForm<ModifyMerchRules>();
            services.RegisterForm<ModifyWeightFactors>();
            services.RegisterForm<PasswordProtect>();
            services.RegisterForm<PDFfileOutput>();
            services.RegisterForm<PDFwatermarkDlg>();
            services.RegisterForm<PrintPreview>();
            services.RegisterForm<ProcessStatus>();
            services.RegisterForm<R8PulpwoodMeasurement>();
            services.RegisterForm<R8Topwood>();
            services.RegisterForm<R8VolEquation>();
            services.RegisterForm<R9TopDIB>();
            services.RegisterForm<R9Topwood>();
            services.RegisterForm<R9VolEquation>();
            services.RegisterForm<ReportsDialog>();
            services.RegisterForm<SecurityQuestion>();
            services.RegisterForm<SelectCSV>();
            services.RegisterForm<selectLength>();
            services.RegisterForm<StewardshipProductCosts>();
            services.RegisterForm<TemplateRegionForest>();
            services.RegisterForm<TextFileOutput>();
            services.RegisterForm<ValueEquations>();
            services.RegisterForm<VolumeEquations>();
            services.RegisterForm<CapturePercentRemoved>();



            // register other services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<DataLayerContext>();
            services.AddTransient<CPbusinessLayer>(x => x.GetRequiredService<DataLayerContext>().DataLayer);

        }
    }
}
