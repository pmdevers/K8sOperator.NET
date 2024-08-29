using K8sOperator.NET.Builder;
using K8sOperator.NET.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace K8sOperator.NET;

internal sealed class Operator(
    IOperatorBuilder builder,
    ILogger<Operator> logger) : IOperator
{
    private readonly IOperatorBuilder _builder = builder;
    private readonly ILogger<Operator> _logger = logger;

    public IEnumerable<IEventWatcher> Watchers
        => _builder.DataSource?.GetWatchers() ?? [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.StartOperator();

        if(!Watchers.Any())
        {
            _logger.NoWatchers();
            await StopAsync(cancellationToken);
            return;
        }

        var tasks = new List<Task>();

        foreach (var watcher in Watchers)
        {
            tasks.Add(watcher.Start(cancellationToken));
        }

        await Task.WhenAll(tasks);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.StopOperator();
        return Task.CompletedTask;
    }
    
}

/// <summary>
/// Describes a Kubernetes Operator
/// </summary>
public interface IOperator : IHostedService
{
}
