using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.Commands;

[OperatorArgument("version", Description = "Shows the version", Order = 998)]
internal class VersionCommand(IOperatorApplication app) : IOperatorCommand
{
    public Task RunAsync(string[] args)
    {
        Console.WriteLine($"{app.Name} version {app.Version}.");

        return Task.CompletedTask;
    }
}
