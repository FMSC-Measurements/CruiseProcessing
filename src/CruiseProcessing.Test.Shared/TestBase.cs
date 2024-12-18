using Bogus;
using CruiseDAL;
using CruiseProcessing.Data;
using CruiseProcessing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Collections;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using Xunit.Abstractions;

namespace CruiseProcessing.Test;

public class TestBase
{
    protected ITestOutputHelper Output { get; }
    protected DbProviderFactory DbProvider { get; private set; }
    protected Randomizer Rand { get; }
    protected Stopwatch _stopwatch;
    private string _testTempPath;
    private IHost _implicitHost;

    protected LogLevel LogLevel { get; set; } = LogLevel.Information;

    private List<string> FilesToBeDeleted { get; } = new List<string>();

    public TestBase(ITestOutputHelper output)
    {
        Output = output;
        Output.WriteLine($"CodeBase: {System.Reflection.Assembly.GetExecutingAssembly().CodeBase}");
        Rand = new Randomizer(this.GetType().Name.GetHashCode()); // make the randomizer fixed based on the test class
        //LoggerFactory = new TestLoggerFactory
        //{
        //    Output = output,
        //    MinLogLevel = minLogLevel
        //};
        //CruiseProcessing.Services.Logging.LoggerProvider.DefaultLoggerFactory = LoggerFactory;

    }

    public IHost ImplicitHost => _implicitHost ??= CreateTestHost();

    ~TestBase()
    {
        foreach (var file in FilesToBeDeleted)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // do nothing
            }
        }
    }

    public string TestExecutionDirectory
    {
        get
        {
            var codeBase = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            return Path.GetDirectoryName(codeBase);
        }
    }

    public string TestTempPath => _testTempPath ??= Path.Combine(Path.GetTempPath(), "TestTemp", Assembly.GetExecutingAssembly().GetName().Name, this.GetType().FullName);

    protected IHost CreateTestHost(Action<IServiceCollection> configureServices = null)
    {
        var builder = new HostBuilder()
            .ConfigureServices(configureServices)
            .ConfigureLogging(ConfigureLogging);

        if(configureServices != null)
        {
            builder.ConfigureServices(configureServices);
        }

        var host = builder.Build();
        CruiseProcessing.Services.Logging.LoggerProvider.Initialize(host.Services);

        return builder.Build();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDialogService>((x) => Substitute.For<IDialogService>());
    }

    protected virtual void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
        var testLoggerProvider = new TestLoggingProvider(Output, LogLevel);
        loggingBuilder.AddProvider(testLoggerProvider);
    }

    protected string GetTestTempPath()
    {
        var testType = this.GetType();
        var assName = testType.Assembly.FullName;
        return Path.Combine(Path.GetTempPath(), "TestTemp", this.GetType().FullName);
    }

    public string TestFilesDirectory => Path.Combine(TestExecutionDirectory, "TestFiles");

    public void StartTimer()
    {
        _stopwatch = new Stopwatch();
        Output.WriteLine("Stopwatch Started");
        _stopwatch.Start();
    }

    public void EndTimer()
    {
        _stopwatch.Stop();
        Output.WriteLine("Stopwatch Ended:" + _stopwatch.ElapsedMilliseconds.ToString() + "ms");
    }

    public string GetTempFilePath(string extention, string fileName = null)
    {
        var testTempPath = TestTempPath;
        if (Directory.Exists(testTempPath) is false)
        {
            Directory.CreateDirectory(testTempPath);
        }

        // note since Rand is using a fixed see the guid generated will
        var tempFilePath = Path.Combine(testTempPath, (fileName ?? Rand.Guid().ToString()) + extention);
        Output.WriteLine($"Temp File Path Generated: {tempFilePath}");
        return tempFilePath;
    }

    public string GetTestFile(string fileName)
    {
        var sourcePath = Path.Combine(TestFilesDirectory, fileName);
        return GetTestFileFromFullPath(sourcePath);
    }

    protected string GetTestFileFromFullPath(string sourcePath)
    {
        var fileName = Path.GetFileName(sourcePath);
        if (File.Exists(sourcePath) == false) { throw new FileNotFoundException(sourcePath); }

        var targetPath = Path.Combine(TestTempPath, fileName);
        var targetDir = Path.GetDirectoryName(targetPath);
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        RegesterFileForCleanUp(targetPath);
        File.Copy(sourcePath, targetPath, true);
        return targetPath;
    }

    protected CpDataLayer GetCpDataLayer(string filePath)
    {
        var fileExtention = System.IO.Path.GetExtension(filePath);
        if (fileExtention == ".crz3")
        {
            var migrator = new DownMigrator();
            var v3Db = new CruiseDatastore_V3(filePath);
            var v2Db = new DAL();
            var cruiseID = v3Db.QueryScalar<string>("SELECT CruiseID FROM Cruise").First();
            migrator.MigrateFromV3ToV2(cruiseID, v3Db, v2Db);
            return new CpDataLayer(v2Db, Substitute.For<ILogger<CpDataLayer>>(), biomassOptions: null);
        }
        else
        {
            var db = new DAL(filePath);
            return new CpDataLayer(db, Substitute.For<ILogger<CpDataLayer>>(), biomassOptions: null);
        }
    }

    public void RegesterFileForCleanUp(string path)
    {
        FilesToBeDeleted.Add(path);
    }

    /// <summary>
    /// creates a logger that uses the log level set on the test base
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ILogger<T> CreateLogger<T>()
    {
        return ImplicitHost.Services.GetRequiredService<ILogger<T>>();
    }
}