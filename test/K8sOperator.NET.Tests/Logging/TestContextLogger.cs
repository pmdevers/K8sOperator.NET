namespace K8sOperator.NET.Tests.Logging;

/// <summary>
///     An implementation of <see cref="ILogger"/> that writes to the output of the current TUnit Test Context.
/// </summary>
internal sealed class TestContextLogger
    : ILogger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestContextLogger"/> class.
    ///     Create a new <see cref="TestContextLogger"/>.
    /// </summary>
    /// <param name="context">
    ///     The output for the current test.
    /// </param>
    /// <param name="loggerCategory">
    ///     The logger's category name.
    /// </param>
    /// <param name="minLogLevel">
    ///     The logger's minimum log level.
    /// </param>
    public TestContextLogger(TestContext context, string loggerCategory, LogLevel minLogLevel)
    {
        if (string.IsNullOrWhiteSpace(loggerCategory))
        {
            throw new ArgumentException(
                "Argument cannot be null, empty, or entirely composed of whitespace: 'loggerCategory'.",
                nameof(loggerCategory));
        }

        Context = context ?? throw new ArgumentNullException(nameof(context));
        LoggerCategory = loggerCategory;
        MinLogLevel = minLogLevel;
    }

    /// <summary>
    ///     The output for the current test.
    /// </summary>
    public TestContext Context { get; }

    /// <summary>
    ///     The logger's category name.
    /// </summary>
    public string LoggerCategory { get; }

    /// <summary>
    ///     The logger's minimum log level.
    /// </summary>
    public LogLevel MinLogLevel { get; }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => NullScope.Instance;

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();
        public void Dispose() { }
    }

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
            Context.Output.WriteLine(string.Format(
                "[{0}] {1}: {2}",
                logLevel,
                LoggerCategory,
                formatter(state, exception)));

            if (exception != null)
            {
                Context.Output.WriteLine(
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

public sealed class TestContextLoggerFactory(TestContext testContext)
        : ILoggerFactory
{
    private readonly TestContext _testContext = testContext ?? throw new ArgumentNullException(nameof(testContext));

    public void Dispose()
    {
    }
    public ILogger CreateLogger(string categoryName)
        => new TestContextLogger(_testContext, categoryName, LogLevel.Debug);

    public void AddProvider(ILoggerProvider provider)
    {
        throw new NotImplementedException();
    }
}


