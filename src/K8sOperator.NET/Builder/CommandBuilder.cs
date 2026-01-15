using K8sOperator.NET.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace K8sOperator.NET.Builder;

public class CommandBuilder(IServiceProvider serviceProvider, Type commandType)
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public Type CommandType { get; } = commandType;
    public List<object> Metadata { get; } = [];

    public IOperatorCommand Build(IHost app)
    {
        var meta = CommandType.GetCustomAttribute<OperatorArgumentAttribute>(false) ??
            new(CommandType.Name.Replace("Command", string.Empty).ToLower());

        Metadata.Add(meta);

        return (IOperatorCommand)ActivatorUtilities.CreateInstance(ServiceProvider, CommandType, app);
    }
}

public interface IOperatorCommand
{
    Task RunAsync(string[] args);
}

[AttributeUsage(AttributeTargets.Class)]
public class OperatorArgumentAttribute(string argument) : Attribute
{
    /// <inheritdoc />
    public string Argument { get; set; } = argument;

    /// <inheritdoc />
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public int Order { get; set; } = 1;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Argument), Argument);
}
