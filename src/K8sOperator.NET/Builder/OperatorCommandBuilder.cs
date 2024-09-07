using K8sOperator.NET.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace K8sOperator.NET.Builder;

/// <summary>
/// 
/// </summary>
public interface IOperatorCommandBuilder
{
    /// <summary>
    /// 
    /// </summary>
    IServiceProvider ServiceProvider { get; }
    /// <summary>
    /// 
    /// </summary>
    List<object> Metadata { get; }
}

/// <summary>
/// 
/// </summary>
public interface IOperatorCommand
{    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task RunAsync(string[] args);
}

internal class OperatorCommandBuilder(IServiceProvider serviceProvider, Type commandType) : IOperatorCommandBuilder
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public Type CommandType { get; } = commandType;
    public List<object> Metadata { get; } = [];

    public IOperatorCommand Build()
    {
        var meta = CommandType.GetCustomAttribute<OperatorArgumentAttribute>(false) ?? new(CommandType.Name.Replace("Command", string.Empty).ToLower());
        Metadata.Add(meta);

        return (IOperatorCommand)ActivatorUtilities.CreateInstance(ServiceProvider, CommandType);
    }
}

/// <summary>
/// 
/// </summary>
public static class OperatorCommandsBuilderExtensions 
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IOperatorCommandConventionBuilder AddCommand<T>(this IOperatorApplication app, int order = 1)
        where T : class, IOperatorCommand
    {
        return app.Commands.AddCommand(typeof(T), order);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="argument"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static TBuilder WithArgument<TBuilder>(this TBuilder builder, string argument, string description)
        where TBuilder : IOperatorCommandConventionBuilder
    {
        FinallyReplace(builder, new OperatorArgumentAttribute(argument) { Description = description});
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static TBuilder FinallyReplace<TBuilder>(this TBuilder builder, object item)
        where TBuilder : IOperatorCommandConventionBuilder
    {
        builder.Finally(b => {
            b.Metadata.RemoveAll(x => x.GetType() == item.GetType());
            b.Metadata.Add(item);
        });

        return builder;
    }
}
