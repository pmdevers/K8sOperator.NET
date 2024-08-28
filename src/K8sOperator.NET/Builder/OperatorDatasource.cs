
namespace K8sOperator.NET.Builder;
internal class OperatorDatasource(IServiceProvider serviceProvider) : IOperatorDataSource
{
    private readonly List<OperatorEntry> _entries = [];

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public OperatorConventionBuilder AddController(string group, string version, string entity, Delegate watch)
    {
        var conventions = new AddAfterProcessBuildConventionCollection();
        var finallyConventions = new AddAfterProcessBuildConventionCollection();

        _entries.Add(new()
        {
            Group = group,
            Version = version,
            Entity = entity,
            Delegate = watch,
            Conventions = conventions,
            FinallyConventions = finallyConventions
        });

        return new OperatorConventionBuilder(conventions, finallyConventions);
    }

    public IEnumerable<IOperatorProcess> GetProcesses()
    {
        foreach (var controller in _entries) 
        {
            var builder = new OperatorBuilder(serviceProvider);

            foreach (var convention in controller.Conventions)
            {
                convention(builder);
            }

            var o = builder.Build();
        }
    }
    private struct OperatorEntry()
    {
        public string Group { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;

        public Delegate Delegate { get; set; } = () => Task.CompletedTask;

        public AddAfterProcessBuildConventionCollection Conventions { get; init; }
        public AddAfterProcessBuildConventionCollection FinallyConventions { get; init; }
        
    }
    private sealed class AddAfterProcessBuildConventionCollection :
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
