using Microsoft.Extensions.Logging;

namespace K8sOperator.NET.Tests.Logging;

/// <summary>
///     Logger provider for logging to TUnit Test Context.
/// </summary>
internal sealed class TestContextLoggerProvider
    : ILoggerProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestContextLoggerProvider"/> class.
    ///     Create a new <see cref="TestContextLoggerProvider"/>.
    /// </summary>
    /// <param name="testContext">
    ///     The output for the current test.
    /// </param>
    /// <param name="minLogLevel">
    ///     The logger's minimum log level.
    /// </param>
    public TestContextLoggerProvider(TestContext testContext, LogLevel minLogLevel)
    {
        if (testContext == null)
        {
            throw new ArgumentNullException(nameof(testContext));
        }

        Context = testContext;
        MinLogLevel = minLogLevel;
    }

    /// <summary>
    ///     Dispose of resources being used by the logger provider.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    ///     The output for the current test.
    /// </summary>
    private TestContext Context { get; }

    /// <summary>
    ///     The logger's minimum log level.
    /// </summary>
    public LogLevel MinLogLevel { get; }

    /// <summary>
    ///     Create a new logger.
    /// </summary>
    /// <param name="categoryName">
    ///     The logger category name.
    /// </param>
    /// <returns>
    ///     The logger, as an <see cref="ILogger"/>.
    /// </returns>
    public ILogger CreateLogger(string categoryName) => new TestContextLogger(Context, categoryName, MinLogLevel);
}
