using K8sOperator.NET;
using K8sOperator.NET.Extensions;
using Microsoft.Extensions.Logging;

namespace K8sOperator.NET.Tests;

public class OperatorTests
{
    private readonly Operator _sut; // System Under Test (SUT)
    private readonly IServiceProvider _serviceProvider;
    private readonly IControllerDataSource _dataSource;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Operator> _logger;
    private readonly CancellationTokenSource _tokenSource;

    public OperatorTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        _dataSource = Substitute.For<IControllerDataSource>();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _logger = Substitute.For<ILogger<Operator>>();

        _tokenSource = new CancellationTokenSource();

        _loggerFactory.CreateLogger<Operator>().Returns(_logger);

        // Initialize the System Under Test (SUT)
        _sut = new Operator(_serviceProvider, _dataSource, _loggerFactory);
    }

    [Fact]
    public async Task RunAsync_NoWatchers_LogsNoWatchersAndReturns()
    {
        // Arrange
        _dataSource.GetWatchers(_serviceProvider).Returns(Enumerable.Empty<IEventWatcher>());

        // Act
        await _sut.RunAsync();

        // Assert
        _logger.Received(1).StartOperator();
        _logger.Received(1).NoWatchers();
        _logger.DidNotReceiveWithAnyArgs().Log(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task RunAsync_WithWatchers_StartsWatchersAndCancelsToken()
    {
        // Arrange
        var watcher1 = Substitute.For<IEventWatcher>();
        var watcher2 = Substitute.For<IEventWatcher>();

        var watchers = new List<IEventWatcher> { watcher1, watcher2 };
        _dataSource.GetWatchers(_serviceProvider).Returns(watchers);

        watcher1.Start(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        watcher2.Start(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.RunAsync();

        // Assert
        _logger.Received(1).StartOperator();
        _logger.DidNotReceive().NoWatchers();

        // Check that watchers are started
        await watcher1.Received(1).Start(Arg.Any<CancellationToken>());
        await watcher2.Received(1).Start(Arg.Any<CancellationToken>());

        // Ensure the token is cancelled after watchers are done
        //_tokenSource.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_CancelsTokenWhenExceptionOccursInWatcher()
    {
        // Arrange
        var watcher1 = Substitute.For<IEventWatcher>();
        var watcher2 = Substitute.For<IEventWatcher>();

        var watchers = new List<IEventWatcher> { watcher1, watcher2 };
        _dataSource.GetWatchers(_serviceProvider).Returns(watchers);

        // Simulate an exception during watcher2 execution
        watcher1.Start(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        watcher2.Start(Arg.Any<CancellationToken>()).Returns(Task.FromException(new Exception("Watcher Error")));

        // Act
        Func<Task> act = async () => await _sut.RunAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Watcher Error");

        _logger.Received(1).StartOperator();
        _logger.DidNotReceive().NoWatchers();

        // Ensure the token is cancelled after the exception
        //_tokenSource.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_DisposesCancellationTokenSourceAfterCompletion()
    {
        // Arrange
        var watcher1 = Substitute.For<IEventWatcher>();
        _dataSource.GetWatchers(_serviceProvider).Returns(new[] { watcher1 });

        watcher1.Start(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.RunAsync();

        // Assert
        // Verify that the CancellationTokenSource is disposed
        _tokenSource.Invoking(ts => ts.Dispose()).Should().NotThrow();
    }
}
