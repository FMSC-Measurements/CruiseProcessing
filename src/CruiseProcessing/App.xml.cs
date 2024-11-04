using CruiseProcessing.Async;
using CruiseProcessing.Data;
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
            //example
            //services.AddTransient<IMyService, MyService>();
            //services.AddTransient<MyForm>();

            services.AddSingleton(appInstance);

            //var config = context.Configuration;
            //if (config.GetValue("UesReferenceProcessor", false))
            //{
            //    services.AddTransient<ICruiseProcessor, RefCruiseProcessor>();
            //}
            //else
            //{
            //    services.AddTransient<ICruiseProcessor, CruiseProcessor>();
            //}

            services.AddTransient<ICruiseProcessor, CruiseProcessor>();
            services.RegisterReferenceImplimentations();

            //services.AddTransient<ICalculateTreeValues, CalculateTreeValues2>();


            // register all forms
            //services.AddSingleton<MainMenu>();
            services.AddSingleton<MainWindow>();
            services.AddTransient<ProcessCruiseDialog>();

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