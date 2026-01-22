using K8sOperator.NET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace K8sOperator.NET;

public class OperatorService(IServiceProvider serviceProvider) : BackgroundService
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public EventWatcherDatasource Datasource { get; } = serviceProvider.GetRequiredService<EventWatcherDatasource>();
    public ILeaderElectionService LeaderElectionService { get; } = serviceProvider.GetRequiredService<ILeaderElectionService>();

    private readonly List<Task> _runningTasks = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<OperatorService>>();

        // Start leader election in background
        var leaderElectionTask = Task.Run(() => LeaderElectionService.StartAsync(stoppingToken), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Wait until we become leader (event-driven)
            logger.LogInformation("Waiting to become leader...");
            await LeaderElectionService.WaitForLeadershipAsync(stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            logger.LogInformation("Became leader. Starting watchers...");

            // We are now the leader, start watchers
            using var watcherCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var watcherTask = Task.Run(() => StartWatchers(watcherCts.Token), watcherCts.Token);

            // Wait until leadership is lost (event-driven)
            await LeaderElectionService.WaitForLeadershipLostAsync(stoppingToken);

            logger.LogInformation("Lost leadership. Stopping watchers...");

            // Leadership lost or stopping, cancel watchers
            await watcherCts.CancelAsync();

            try
            {
                await watcherTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when leadership is lost
            }

            _runningTasks.Clear();
        }

        await leaderElectionTask;
    }

    public async Task StartWatchers(CancellationToken stoppingToken)
    {
        var watchers = Datasource.GetWatchers().ToList();
        var logger = ServiceProvider.GetRequiredService<ILogger<OperatorService>>();

        if (!watchers.Any())
        {
            logger.LogInformation("No event watchers registered.");
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        foreach (var watcher in watchers)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await watcher.Start(cts.Token);
                }
                catch (OperatorException)
                {
                    await cts.CancelAsync();
                    throw;
                }

            }
            , cts.Token);
            _runningTasks.Add(task);
        }

        await Task.WhenAll(_runningTasks);
    }
}
