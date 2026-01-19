using K8sOperator.NET;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Generation;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace K8sOperator.NET.Commands;

[OperatorArgument("version", Description = "Shows the version", Order = 998)]
internal class VersionCommand(IHost app) : IOperatorCommand
{
    public Task RunAsync(string[] args)
    {
        var watcher = app.Services.GetRequiredService<EventWatcherDatasource>();
        var name = watcher.Metadata.OfType<OperatorNameAttribute>().FirstOrDefault()
            ?? OperatorNameAttribute.Default;
        var version = watcher.Metadata.OfType<DockerImageAttribute>().FirstOrDefault()
            ?? DockerImageAttribute.Default;

        if (string.IsNullOrWhiteSpace(name.OperatorName) || string.IsNullOrWhiteSpace(version.Tag))
        {
            Console.WriteLine("Operator name or version metadata is missing.");
            return Task.CompletedTask;
        }

        Console.WriteLine($"{name} version {version}.");
        Console.WriteLine($"Docker Info: {version.GetImage()}.");
        return Task.CompletedTask;
    }
}
