﻿using K8sOperator.NET.Builder;
using K8sOperator.NET.Extensions;
using Microsoft.Extensions.Logging;

namespace K8sOperator.NET.Commands;

internal class Operator(IServiceProvider serviceProvider, IControllerDataSource dataSource, ILoggerFactory loggerFactory)
{
    private readonly CancellationTokenSource _tokenSource = new();

    public ILogger<Operator> Logger { get; } = loggerFactory.CreateLogger<Operator>();
    public IEnumerable<IEventWatcher> Watchers
        => dataSource.GetWatchers(serviceProvider) ?? [];

    public async Task RunAsync()
    {
        Logger.StartOperator();

        if (!Watchers.Any())
        {
            Logger.NoWatchers();
            return;
        }

        var tasks = new List<Task>();

        foreach (var watcher in Watchers)
        {
            tasks.Add(watcher.Start(_tokenSource.Token));
        }

        await Task.WhenAll(tasks);

        await _tokenSource.CancelAsync();
        _tokenSource.Dispose();
    }
}
