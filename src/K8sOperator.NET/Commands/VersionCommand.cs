using K8sOperator.NET;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Configuration;
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
        var config = app.Services.GetRequiredService<OperatorConfiguration>();

        if (string.IsNullOrWhiteSpace(config.OperatorName) || string.IsNullOrWhiteSpace(config.ContainerTag))
        {
            Console.WriteLine("Operator name or version metadata is missing.");
            return Task.CompletedTask;
        }

        Console.WriteLine($"{config.OperatorName} version {config.ContainerTag}.");
        Console.WriteLine($"Docker Info: {config.ContainerImage}.");
        return Task.CompletedTask;
    }
}

