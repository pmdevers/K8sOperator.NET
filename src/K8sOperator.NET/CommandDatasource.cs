using K8sOperator.NET.Builder;
using System.Runtime.InteropServices;

namespace K8sOperator.NET;
/// <summary>
/// 
/// </summary>
public interface ICommandDatasource
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="operatorCommandType"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public IOperatorCommandConventionBuilder AddCommand(Type operatorCommandType, int? order = null);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<CommandInfo> GetCommands();
}
internal class CommandDatasource(IServiceProvider serviceProvider) : ICommandDatasource
{
    private sealed class CommandEntry
    {
        public required Type CommandType { get; init; }
        public required List<Action<IOperatorCommandBuilder>> Conventions { get; init; }
        public required List<Action<IOperatorCommandBuilder>> FinallyConventions { get; init; }
        public required int Order { get; init; }
    }

    private readonly List<CommandEntry> _commands = [];

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public IOperatorCommandConventionBuilder AddCommand(Type operatorCommandType, int? order = null)
    {
        var conventions = new List<Action<IOperatorCommandBuilder>>();
        var finallyConventions = new List<Action<IOperatorCommandBuilder>>();

        _commands.Add(new()
        {
            CommandType = operatorCommandType,
            Conventions = conventions,
            FinallyConventions = finallyConventions,
            Order = order ?? (_commands.Count + 1)
        });

        return new OperatorCommandConventionBuilder(conventions, finallyConventions);
    }

    public IEnumerable<CommandInfo> GetCommands()
    {
        foreach (var command in _commands.OrderBy(x => x.Order))
        {
            var builder = new OperatorCommandBuilder(ServiceProvider, command.CommandType);

            foreach (var convention in command.Conventions)
            {
                convention(builder);
            }

            var result = builder.Build();

            foreach (var convention in command.FinallyConventions)
            {
                convention(builder);
            }

            yield return new()
            {
               Command = result,
               Metadata = builder.Metadata,
            };
        }
    }
}


/// <summary>
/// 
/// </summary>
public class CommandInfo
{
    /// <summary>
    /// 
    /// </summary>
    public required IOperatorCommand Command { get; init; }
    /// <summary>
    /// 
    /// </summary>
    public required IList<object> Metadata { get; init; }
}


