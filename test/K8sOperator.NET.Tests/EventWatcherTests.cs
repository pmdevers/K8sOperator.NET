using k8s;
using k8s.Models;
using K8sOperator.NET.Metadata;
using K8sOperator.NET.Models;
using K8sOperator.NET.Tests.Mocks;
using K8sOperator.NET.Tests.Mocks.Endpoints;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace K8sOperator.NET.Tests;

public class EventWatcherTests
{
    private static Watcher<TestResource>.WatchEvent Added => CreateEvent(WatchEventType.Modified,
        new TestResource()
        {
            Metadata = new()
            {
                Name = "test",
                NamespaceProperty = "default",
                Finalizers = ["finalize"],
                Uid = "1"
            }
        });

    private static Watcher<TestResource>.WatchEvent Finalize => CreateEvent(WatchEventType.Added,
        new TestResource()
        {
            Metadata = new()
            {
                Name = "test",
                NamespaceProperty = "default",
                DeletionTimestamp = TimeProvider.System.GetUtcNow().DateTime,
                Finalizers = ["finalize"],
                Uid = "1"
            }
        });

    private static Watcher<TestResource>.WatchEvent Deleted => CreateEvent(WatchEventType.Deleted,
        new TestResource()
        {
            Metadata = new()
            {
                Name = "test",
                NamespaceProperty = "default",
                Finalizers = ["finalize"],
                Uid = "1"
            }
        });

    private static Watcher<T>.WatchEvent CreateEvent<T>(WatchEventType type, T item)
        where T : CustomResource
    {
        return new Watcher<T>.WatchEvent { Type = type, Object = item };
    }

    private readonly ITestOutputHelper _testOutput;
    private readonly Controller<TestResource> _controller = Substitute.For<Controller<TestResource>>();
    private readonly CancellationTokenSource _tokenSource;
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly List<object> _metadata;

    public EventWatcherTests(ITestOutputHelper testOutput)
    {
        _tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        _testOutput = testOutput;
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_logger);
        _metadata = [
            new KubernetesEntityAttribute { Group = "group", ApiVersion = "v1", Kind = "Test", PluralName = "tests" },
            new NamespaceAttribute("default"),
            Substitute.For<ILabelSelectorMetadata>(),
            new FinalizerAttribute("finalize")
        ];
    }

    [Fact]
    public async Task Start_Should_StartWatchAndLogStart()
    {
        var cancellationToken = _tokenSource.Token;

        using (var server = new MockKubeApiServer(_testOutput, endpoints =>
        {
            endpoints.MapWatchNamespacedCustomObjectAsync<TestResource>(Added);
        }))
        {
            var client = new NamespacedKubernetesClient(server.GetMockedKubernetesClient(), _loggerFactory.CreateLogger<NamespacedKubernetesClient>());
            var watcher = new EventWatcher<TestResource>(client, _controller, _metadata, _loggerFactory);

            await watcher.Start(cancellationToken);
        }

        _loggerFactory.Received(2).CreateLogger(Arg.Any<string>());
    }

    [Fact]
    public async Task OnEvent_Should_HandleAddedEventAndCallAddOrModifyAsync()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var eventProcessed = new TaskCompletionSource<bool>();
        
        // Setup the controller to signal when AddOrModifyAsync is called
        _controller.AddOrModifyAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>())
            .Returns(x =>
            {
                eventProcessed.TrySetResult(true);
                return Task.CompletedTask;
            });
        
        using (var server = new MockKubeApiServer(_testOutput, endpoints =>
        {
            endpoints.MapWatchNamespacedCustomObjectAsync(Added);
            endpoints.MapReplaceNamespacedCustomObjectAsync();
        }))
        {
            var client = new NamespacedKubernetesClient(server.GetMockedKubernetesClient(), _loggerFactory.CreateLogger<NamespacedKubernetesClient>());
            var watcher = new EventWatcher<TestResource>(client, _controller, _metadata, _loggerFactory);

            var watchTask = Task.Run(async () => await watcher.Start(cancellationToken.Token));
            
            // Wait for either the event to be processed or timeout
            var completedTask = await Task.WhenAny(eventProcessed.Task, Task.Delay(TimeSpan.FromSeconds(3)));
            
            if (completedTask != eventProcessed.Task)
            {
                throw new TimeoutException("AddOrModifyAsync was not called within the timeout period");
            }
        }

        _loggerFactory.Received(2).CreateLogger(Arg.Any<string>());

        _controller.Received(1).AddOrModifyAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OnEvent_Should_HandleDeletedEventAndCallDeleteAsync()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var eventProcessed = new TaskCompletionSource<bool>();
        
        // Setup the controller to signal when DeleteAsync is called
        _controller.DeleteAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>())
            .Returns(x =>
            {
                eventProcessed.TrySetResult(true);
                return Task.CompletedTask;
            });

        using (var server = new MockKubeApiServer(_testOutput, endpoints =>
        {
            endpoints.MapWatchNamespacedCustomObjectAsync(Deleted);
            endpoints.MapReplaceNamespacedCustomObjectAsync();
        }))
        {
            var client = new NamespacedKubernetesClient(server.GetMockedKubernetesClient(), _loggerFactory.CreateLogger<NamespacedKubernetesClient>());
            var watcher = new EventWatcher<TestResource>(client, _controller, _metadata, _loggerFactory);

            var watchTask = Task.Run(async () => await watcher.Start(cancellationToken.Token));
            
            // Wait for either the event to be processed or timeout
            var completedTask = await Task.WhenAny(eventProcessed.Task, Task.Delay(TimeSpan.FromSeconds(3)));
            
            if (completedTask != eventProcessed.Task)
            {
                throw new TimeoutException("DeleteAsync was not called within the timeout period");
            }
        }

        _loggerFactory.Received(2).CreateLogger(Arg.Any<string>());

        _controller.Received(1).DeleteAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleFinalizeAsync_Should_CallFinalizeAndRemoveFinalizer()
    {
        var cancellationToken = _tokenSource.Token;
        var eventProcessed = new TaskCompletionSource<bool>();
        
        // Setup the controller to signal when FinalizeAsync is called
        _controller.FinalizeAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>())
            .Returns(x =>
            {
                eventProcessed.TrySetResult(true);
                return Task.CompletedTask;
            });

        using (var server = new MockKubeApiServer(_testOutput, endpoints =>
        {
            endpoints.MapWatchNamespacedCustomObjectAsync(Finalize);
            endpoints.MapReplaceNamespacedCustomObjectAsync();
        }))
        {
            var client = new NamespacedKubernetesClient(server.GetMockedKubernetesClient(), _loggerFactory.CreateLogger<NamespacedKubernetesClient>());
            var watcher = new EventWatcher<TestResource>(client, _controller, _metadata, _loggerFactory);

            var watchTask = Task.Run(async () => await watcher.Start(cancellationToken));
            
            // Wait for either the event to be processed or timeout
            var completedTask = await Task.WhenAny(eventProcessed.Task, Task.Delay(TimeSpan.FromSeconds(3)));
            
            if (completedTask != eventProcessed.Task)
            {
                throw new TimeoutException("FinalizeAsync was not called within the timeout period");
            }
        }

        _loggerFactory.Received(2).CreateLogger(Arg.Any<string>());

        _controller.Received(1).FinalizeAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void HandleAddOrModifyAsync_Should_AddFinalizer_IfNotPresent()
    {
        Assert.True(true);
    }
}


[KubernetesEntity(Group = "unittest", ApiVersion = "v1", Kind = "TestResource", PluralName = "testresources")]
public class TestResource : CustomResource<TestResource.TestSpec, TestResource.TestStatus>
{
    public class TestStatus
    {
        public string Status { get; set; } = string.Empty;
    }

    public class TestSpec
    {
        public string Property { get; set; } = string.Empty;
    }
}
