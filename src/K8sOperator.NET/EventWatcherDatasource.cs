using K8sOperator.NET.Builder;
using K8sOperator.NET.Configuration;

namespace K8sOperator.NET;

public interface IEventWatcher
{
    Task Start(CancellationToken cancellationToken);
    IReadOnlyList<object> Metadata { get; }

    IOperatorController Controller { get; }
}


public class EventWatcherDatasource(IServiceProvider serviceProvider, OperatorConfiguration configuration)
{
    private readonly List<ControllerEntry> _controllers = [];
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public OperatorConfiguration Configuration { get; } = configuration;

    public ConventionBuilder<ControllerBuilder> Add<TController>()
        where TController : IOperatorController
    {
        var conventions = new List<Action<ControllerBuilder>>();
        _controllers.Add(new ControllerEntry
        {
            ControllerType = typeof(TController),
            Conventions = conventions,
        });

        return new ConventionBuilder<ControllerBuilder>(conventions);
    }

    public IEnumerable<IEventWatcher> GetWatchers()
    {
        foreach (var controller in _controllers)
        {
            var builder = ControllerBuilder.Create(ServiceProvider, controller.ControllerType, Configuration);

            foreach (var convention in controller.Conventions)
            {
                convention(builder);
            }

            var result = builder.Build();

            var eventWatcher = EventWatcherBuilder.Create(ServiceProvider, Configuration, result, builder.Metadata)
                .Build();

            yield return eventWatcher;
        }
    }

    private sealed record ControllerEntry
    {
        public required Type ControllerType { get; init; }
        public required List<Action<ControllerBuilder>> Conventions { get; init; }
    }
}



public record ControllerInfo(
    IOperatorController Controller,
    IEnumerable<object> Metadata
);

public interface IOperatorController
{
    Type ResourceType { get; }
}
