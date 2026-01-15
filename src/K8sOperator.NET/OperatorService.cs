using K8sOperator.NET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace K8sOperator.NET;

public class OperatorService(IServiceProvider serviceProvider) : BackgroundService
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public EventWatcherDatasource EventWatcherDatasource { get; } = serviceProvider.GetRequiredService<EventWatcherDatasource>();

    private readonly List<Task> _runningTasks = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var watchers = EventWatcherDatasource.GetWatchers().ToList();
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
