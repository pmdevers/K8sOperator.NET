using k8s.Models;
using K8sOperator.NET.Configuration;
using K8sOperator.NET.Metadata;
using K8sOperator.NET.Tests.Fixtures;
using K8sOperator.NET.Tests.Logging;
using K8sOperator.NET.Tests.Mocks;
using NSubstitute;
using System.Reflection;

namespace K8sOperator.NET.Tests;

public class EventWatcher_Tests()
{
    private readonly CancellationTokenSource _tokenSource = new(TimeSpan.FromSeconds(2));
    private readonly OperatorController<TestResource> _controller = Substitute.For<OperatorController<TestResource>>();
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly List<object> _metadata = [
            typeof(TestResource).GetCustomAttribute<KubernetesEntityAttribute>()!,
            new NamespaceAttribute("default"),
            new LabelSelectorAttribute("label"),
            new FinalizerAttribute("finalize")
        ];

    [Test]
    public async Task Start_Should_StartWatchAndLogStart()
    {
        var cancellationToken = _tokenSource.Token;

        using var server = new MockKubeApiServer(endpoints =>
        {
            endpoints.CustomObjects.WatchListClusterCustomObjectAsync(WatchEvents<TestResource>.Added);
        });

        var watcher = new EventWatcher<TestResource>(new OperatorConfiguration(), server.Client, _controller, _metadata, _loggerFactory);

        await watcher.Start(cancellationToken);

        _loggerFactory.Received(1).CreateLogger(Arg.Any<string>());
    }

    [Test]
    public async Task OnEvent_Should_HandleAddedEventAndCallAddOrModifyAsync()
    {
        var cancellationToken = _tokenSource.Token;
        using var server = new MockKubeApiServer(endpoints =>
        {
            endpoints.CustomObjects.WatchListClusterCustomObjectAsync(WatchEvents<TestResource>.Added);
            endpoints.CustomObjects.ReplaceNamespacedCustomObjectAsync<TestResource>();
        });

        var watcher = new EventWatcher<TestResource>(new OperatorConfiguration(), server.Client, _controller, _metadata, _loggerFactory);

        await watcher.Start(cancellationToken);

        await _controller.Received(1).AddOrModifyAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task OnEvent_Should_HandleDeletedEventAndCallDeleteAsync()
    {
        var cancellationToken = _tokenSource.Token;

        using var server = new MockKubeApiServer(endpoints =>
        {
            endpoints.CustomObjects.WatchListClusterCustomObjectAsync(WatchEvents<TestResource>.Deleted);
            endpoints.CustomObjects.ReplaceNamespacedCustomObjectAsync<TestResource>();
        });

        var watcher = new EventWatcher<TestResource>(new OperatorConfiguration(), server.Client, _controller, _metadata, _loggerFactory);

        await watcher.Start(cancellationToken);

        await _controller.Received(1).DeleteAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleFinalizeAsync_Should_CallFinalizeAndRemoveFinalizer()
    {
        var cancellationToken = _tokenSource.Token;

        using var server = new MockKubeApiServer(endpoints =>
        {
            endpoints.CustomObjects.WatchListClusterCustomObjectAsync(WatchEvents<TestResource>.Finalize);
            endpoints.CustomObjects.ReplaceNamespacedCustomObjectAsync<TestResource>();
        });

        var watcher = new EventWatcher<TestResource>(new OperatorConfiguration(), server.Client, _controller, _metadata, _loggerFactory);

        await watcher.Start(cancellationToken);

        await _controller.Received(1).FinalizeAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleAddOrModifyAsync_Should_AddFinalizer_IfNotPresent()
    {
        var cancellationToken = _tokenSource.Token;
        using var server = new MockKubeApiServer(endpoints =>
        {
            endpoints.CustomObjects.WatchListClusterCustomObjectAsync(WatchEvents<TestResource>.Added);
            endpoints.CustomObjects.ReplaceNamespacedCustomObjectAsync<TestResource>(resource: async x =>
            {
                await Assert.That(x?.Metadata.Finalizers).Contains("finalize");
            });
        });

        var watcher = new EventWatcher<TestResource>(new OperatorConfiguration(), server.Client, _controller, _metadata, _loggerFactory);

        await watcher.Start(cancellationToken);
    }
}
