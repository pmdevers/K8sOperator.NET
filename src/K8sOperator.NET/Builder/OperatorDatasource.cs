namespace K8sOperator.NET.Builder;
internal class OperatorDatasource(IServiceProvider serviceProvider) : IOperatorDataSource
{
    private readonly List<OperatorEntry> _entries = [];

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public IOperatorConventionBuilder AddController(Type controllerType)
    {
        var conventions = new AddAfterProcessBuildConventionCollection();
        var finallyConventions = new AddAfterProcessBuildConventionCollection();

        _entries.Add(new()
        {
            ControllerType = controllerType,
            Conventions = conventions,
            FinallyConventions = finallyConventions
        });

        return new OperatorConventionBuilder(conventions, finallyConventions);
    }

    public IEnumerable<IEventWatcher> GetWatchers()
    {
        foreach (var controller in _entries) 
        {
            var builder = new OperatorBuilder(ServiceProvider);

            foreach (var convention in controller.Conventions)
            {
                convention(builder);
            }

            var o = EventWatcherFactory.Create(controller.ControllerType, ServiceProvider, builder.MetaData);

            foreach (var convention in controller.FinallyConventions)
            {
                convention(builder);
            }

            yield return o;
        }
    }

    private sealed class OperatorEntry
    {
        public required Type ControllerType { get; init; }
        public required AddAfterProcessBuildConventionCollection Conventions { get; init; }
        public required AddAfterProcessBuildConventionCollection FinallyConventions { get; init; }
        
    }
    internal sealed class AddAfterProcessBuildConventionCollection :
            List<Action<IOperatorBuilder>>,
            ICollection<Action<IOperatorBuilder>>
    {
        public bool IsReadOnly { get; set; }

        void ICollection<Action<IOperatorBuilder>>.Add(Action<IOperatorBuilder> convention)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException($"{nameof(OperatorDatasource)} can not be modified after build.");
            }

            Add(convention);
        }
    }
}
