using K8sOperator.NET.Builder;
using Microsoft.Extensions.Hosting;

namespace K8sOperator.NET;

public class CommandDatasource(IServiceProvider serviceProvider)
{
    private sealed record CommandEntry
    {
        public required Type CommandType { get; init; }
        public required List<Action<CommandBuilder>> Conventions { get; init; }
        public required int Order { get; init; }
    }

    private readonly List<CommandEntry> _commands = [];

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public ConventionBuilder<CommandBuilder> Add<TCommand>()
        where TCommand : IOperatorCommand
    {
        var conventions = new List<Action<CommandBuilder>>();
        _commands.Add(new CommandEntry
        {
            CommandType = typeof(TCommand),
            Conventions = conventions,
            Order = 1
        });
        return new ConventionBuilder<CommandBuilder>(conventions);
    }

    public IEnumerable<CommandInfo> GetCommands(IHost app)
    {
        foreach (var command in _commands.OrderBy(x => x.Order))
        {
            var builder = new CommandBuilder(ServiceProvider, command.CommandType);

            foreach (var convention in command.Conventions)
            {
                convention(builder);
            }

            var result = builder.Build(app);

            yield return new(result, builder.Metadata);
        }
    }
}

public record CommandInfo(
    IOperatorCommand Command,
    IEnumerable<object> Metadata
);
