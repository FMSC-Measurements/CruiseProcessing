using CruiseProcessing.Async;
using CruiseProcessing.Config;
using CruiseProcessing.Data;
using CruiseProcessing.Interop;
using CruiseProcessing.Processing;
using CruiseProcessing.ReferenceImplmentation;
using CruiseProcessing.Services;
using CruiseProcessing.Services.Logging;
using CruiseProcessing.ViewModel;
using CruiseProcessing.Views;
using Microsoft.AppCenter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace CruiseProcessing
{
    public partial class App : Application
    {
        private IServiceProvider Services { get; set; }

        private IHost _host;

        private DataLayerContext DataLayerContext { get; set; }

        public Version AppVersion { get; }

        public App()
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version;
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
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

            _host = Host.CreateDefaultBuilder(e.Args)
                 .ConfigureServices((context, services) => ConfigureServices(context, services, this))
                 .ConfigureLogging(ConfigureLogging).Build();

            Services = _host.Services;
            await _host.StartAsync();

            var loggerProvider = Services.GetRequiredService<ILoggerProvider>();
            TaskExtentions.Logger = loggerProvider.CreateLogger(nameof(TaskExtentions));

            DataLayerContext = _host.Services.GetRequiredService<DataLayerContext>();

            var mainWindow = Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.AddAppCenterLogger();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services, App appInstance)
        {
            // app instance used by dialog service to get the main window
            // dialog service can't directly request the main window because
            // that would create a circular dependency, with the main window
            // needing the dialog service
            services.AddSingleton(appInstance);

            // add service collection so that we can programmatic detect what processors are registered
            services.AddSingleton<IServiceCollection>(services);


            services.AddTransient<ICruiseProcessor, CruiseProcessor>();
            //services.RegisterReferenceImplimentations();
            //services.AddKeyedTransient<ICruiseProcessor, NextVerCruiseProcessor>(nameof(NextVerCruiseProcessor));


            // register tree value calculators
            services.AddTransient<ICalculateTreeValues, CalculateTreeValues2>();
            //services.AddKeyedTransient<ICalculateTreeValues, CalculateTreeValues2>(nameof(CalculateTreeValues2));
            //services.AddKeyedTransient<ICalculateTreeValues, RefCalculateTreeValues>(nameof(RefCalculateTreeValues));


            // register volume libraries
            services.AddSingleton<IVolumeLibrary>(VolumeLibraryInterop.Default);
            //services.AddKeyedSingleton<IVolumeLibrary, VolumeLibrary_20240626>(nameof(VolumeLibrary_20240626));
            //services.AddKeyedSingleton<IVolumeLibrary, VolumeLibrary_20241101>(nameof(VolumeLibrary_20241101));


            // register WPF views
            services.AddSingleton<MainWindow>();
            services.AddTransient<ProcessCruiseDialog>();

            // register all forms
            //services.AddSingleton<MainMenu>();


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

            // register business logic services
            services.AddTransient<OutputGraphs>();
            services.AddTransient<CreateTextFile>();

            // register View Models
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<ProcessCruiseViewModel>();

            // register other services
            services.AddSingleton<DialogService>();
            services.AddTransient<IDialogService>(x => x.GetRequiredService<DialogService>());
            services.AddSingleton<DataLayerContext>();
            services.AddTransient<CpDataLayer>(x => x.GetRequiredService<DataLayerContext>().DataLayer);
        }
    }
}