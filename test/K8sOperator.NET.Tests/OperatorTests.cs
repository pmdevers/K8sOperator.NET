using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace K8sOperator.NET.Tests;

public class OperatorTests
{
    private readonly ILogger _logger;
    public OperatorTests(ITestOutputHelper testOutputHelper)
    {
        var builder = OperatorHost.CreateOperatorApplicationBuilder();
        var logProvider = Substitute.For<ILoggerProvider>();
        _logger = Substitute.For<ILogger>();

        logProvider.CreateLogger(Arg.Any<string>()).Returns(_logger);
        
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(logProvider);

        App = builder.Build();
    }

    public IOperatorApplication App { get; }
}
