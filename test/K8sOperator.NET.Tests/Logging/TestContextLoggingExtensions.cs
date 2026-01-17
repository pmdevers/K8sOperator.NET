namespace K8sOperator.NET.Tests.Logging;

/// <summary>
///     Extension methods for logging to TUnit Test Context.
/// </summary>
public static class TestContextLoggingExtensions
{
    extension(ILoggingBuilder logging)
    {
        /// <summary>
        ///     Log to test output.
        /// </summary>
        /// <param name="logging">
        ///     The global logging configuration.
        /// </param>
        /// <param name="testContext">
        ///     Output for the current test.
        /// </param>
        /// <param name="minLogLevel">
        ///     The minimum level to log at.
        /// </param>
        public void AddTestLogging(TestContext testContext,
            LogLevel minLogLevel = LogLevel.Information)
        {
            ArgumentNullException.ThrowIfNull(logging);
            ArgumentNullException.ThrowIfNull(testContext);

            logging.AddProvider(
                new TestContextLoggerProvider(testContext, minLogLevel));
        }
    }
}
