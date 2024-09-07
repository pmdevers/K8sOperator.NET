using Microsoft.Extensions.Logging;
using System.Reactive.Disposables;
using Xunit.Abstractions;

namespace K8sOperator.NET.Tests.Logging;
/// <summary>
///     An implementation of <see cref="ILogger"/> that writes to the output of the current Xunit test.
/// </summary>
internal sealed class TestOutputLogger
    : ILogger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestOutputLogger"/> class.
    ///     Create a new <see cref="TestOutputLogger"/>.
    /// </summary>
    /// <param name="testOutput">
    ///     The output for the current test.
    /// </param>
    /// <param name="loggerCategory">
    ///     The logger's category name.
    /// </param>
    /// <param name="minLogLevel">
    ///     The logger's minimum log level.
    /// </param>
    public TestOutputLogger(ITestOutputHelper testOutput, string loggerCategory, LogLevel minLogLevel)
    {
        if (string.IsNullOrWhiteSpace(loggerCategory))
        {
            throw new ArgumentException(
                "Argument cannot be null, empty, or entirely composed of whitespace: 'loggerCategory'.",
                nameof(loggerCategory));
        }

        TestOutput = testOutput ?? throw new ArgumentNullException(nameof(testOutput));
        LoggerCategory = loggerCategory;
        MinLogLevel = minLogLevel;
    }

    /// <summary>
    ///     The output for the current test.
    /// </summary>
    public ITestOutputHelper TestOutput { get; }

    /// <summary>
    ///     The logger's category name.
    /// </summary>
    public string LoggerCategory { get; }

    /// <summary>
    ///     The logger's minimum log level.
    /// </summary>
    public LogLevel MinLogLevel { get; }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => Disposable.Empty;



    /// <summary>
    ///     Check if the given <paramref name="logLevel"/> is enabled.
    /// </summary>
    /// <param name="logLevel">
    ///     The level to be checked.
    /// </param>
    /// <returns>
    ///     <c>true</c> if enabled; otherwise, <c>false</c>.
    /// </returns>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= MinLogLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        try
        {
            TestOutput.WriteLine(string.Format(
                "[{0}] {1}: {2}",
                logLevel,
                LoggerCategory,
                formatter(state, exception)));

            if (exception != null)
            {
                TestOutput.WriteLine(
                    exception.ToString());
            }
        }
        catch (Exception e)
        {
            // ignore 'There is no currently active test.'
            if (e.ToString().Contains("There is no currently active test"))
            {
                return;
            }

            throw;
        }

    }
}
