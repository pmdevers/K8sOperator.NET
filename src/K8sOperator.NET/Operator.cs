using K8sOperator.NET.Extensions;

namespace K8sOperator.NET;

internal class Operator(IOperatorApplication app)
{
    private readonly CancellationTokenSource _tokenSource = new();
    public string ArgumentName { get; } = "operator";

    public async Task RunAsync()
    {
        var watchers = app.DataSource.GetWatchers(app.ServiceProvider) ?? [];
        var logger = app.Logger.CreateLogger("operator");

        logger.StartOperator();

        if (!watchers.Any())
        {
            logger.NoWatchers();
            return;
        }

        var tasks = new List<Task>();

        foreach (var watcher in watchers)
        {
            tasks.Add(watcher.Start(_tokenSource.Token));
        }

        await Task.WhenAll(tasks);

        await _tokenSource.CancelAsync();
        _tokenSource.Dispose();
    }
}
