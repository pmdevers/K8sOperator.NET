using K8sOperator.NET.Builder;
using Microsoft.Extensions.Hosting;

namespace K8sOperator.NET.Commands;

[OperatorArgument("operator", Description = "Starts the operator.", Order = -2)]
public class OperatorCommand(IHost app) : IOperatorCommand
{
    public Task RunAsync(string[] args)
    {
        return app.RunAsync();
    }
}
