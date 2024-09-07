using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.Commands;

[OperatorArgument("operator", Description = "Starts the operator.", Order = -2)]
internal class OperatorCommand(IOperatorApplication app) : IOperatorCommand
{
    public Task RunAsync(string[] args)
    {
        return new Operator(app).RunAsync();
    }
}
