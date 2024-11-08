﻿using Bogus;
using Castle.Core.Logging;
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
    protected LogLevel LogLevel { get; set; } = LogLevel.Information;

    private List<string> FilesToBeDeleted { get; } = new List<string>();

    public TestBase(ITestOutputHelper output)
    {
        Output = output;
        Output.WriteLine($"CodeBase: {System.Reflection.Assembly.GetExecutingAssembly().CodeBase}");
        Rand = new Randomizer(this.GetType().Name.GetHashCode()); // make the randomizer fixed based on the test class


    }

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

    public string GetTestFile(string fileName) => InitializeTestFile(fileName);

    protected string InitializeTestFile(string fileName)
    {
        var sourcePath = Path.Combine(TestFilesDirectory, fileName);
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

    public void RegesterFileForCleanUp(string path)
    {
        FilesToBeDeleted.Add(path);
    }

    public ILogger<T> CreateLogger<T>()
    {
        var typeName = typeof(T).Name;

        var logger = Substitute.For<ILogger<T>>();
        logger.When(x => x.Log<object>(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception, string>>()))
            .Do(x =>
            {
                if (x.Arg<LogLevel>() >= LogLevel)
                { Output.WriteLine("Logger:" + typeName + "::::" + x.ArgAt<object>(2).ToString()); }
            });
        return logger;
    }
}