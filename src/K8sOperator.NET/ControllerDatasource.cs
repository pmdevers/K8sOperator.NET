using K8sOperator.NET.Builder;
using System.Collections;

namespace K8sOperator.NET;

/// <summary>
/// Describes a Controller Datasource
/// </summary>
public interface IControllerDataSource
{
    /// <summary>
    /// Gets a readonly list of metadata
    /// </summary>
    IReadOnlyList<object> Metadata { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    IEnumerable<IEventWatcher> GetWatchers(IServiceProvider serviceProvider);
}

internal class ControllerDatasource(List<object> metadata) : IControllerDataSource
{
    private readonly List<ControllerEntry> _entries = [];
    public IReadOnlyList<object> Metadata => metadata;

    internal IControllerConventionBuilder AddController(Type controllerType)
    {
        var conventions = new AddAfterProcessBuildConventionCollection();
        var finallyConventions = new AddAfterProcessBuildConventionCollection();

        _entries.Add(new()
        {
            ControllerType = controllerType,
            Conventions = conventions,
            FinallyConventions = finallyConventions
        });

        return new ControllerConventionBuilder(conventions, finallyConventions);
    }

    public IEnumerable<IEventWatcher> GetWatchers(IServiceProvider serviceProvider)
    {
        foreach (var entry in _entries)
        {
            var builder = new ControllerBuilder(serviceProvider, entry.ControllerType);

            foreach (var convention in entry.Conventions)
            {
                convention(builder);
            }

            var controller = builder.Build();

            foreach (var convention in entry.FinallyConventions)
            {
                convention(builder);
            }

            var eventWatcher = new EventWatcherBuilder(serviceProvider, controller, builder.Metadata)
                .Build();

            yield return eventWatcher;
        }
    }

    private sealed class ControllerEntry
    {
        public required Type ControllerType { get; init; }
        public required AddAfterProcessBuildConventionCollection Conventions { get; init; }
        public required AddAfterProcessBuildConventionCollection FinallyConventions { get; init; }

    }
    internal sealed class AddAfterProcessBuildConventionCollection : ICollection<Action<IControllerBuilder>>
    {
        private readonly List<Action<IControllerBuilder>> _actions = [];
        public bool IsReadOnly { get; set; }

        public int Count => ((ICollection<Action<IControllerBuilder>>)_actions).Count;

        public void Add(Action<IControllerBuilder> convention)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException($"{nameof(ControllerDatasource)} can not be modified after build.");
            }

            _actions.Add(convention);
        }

        public void Clear()
        {
            ((ICollection<Action<IControllerBuilder>>)_actions).Clear();
        }

        public bool Contains(Action<IControllerBuilder> item)
        {
            return ((ICollection<Action<IControllerBuilder>>)_actions).Contains(item);
        }

        public void CopyTo(Action<IControllerBuilder>[] array, int arrayIndex)
        {
            ((ICollection<Action<IControllerBuilder>>)_actions).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Action<IControllerBuilder>> GetEnumerator()
        {
            return ((IEnumerable<Action<IControllerBuilder>>)_actions).GetEnumerator();
        }

        public bool Remove(Action<IControllerBuilder> item)
        {
            return ((ICollection<Action<IControllerBuilder>>)_actions).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_actions).GetEnumerator();
        }
    }
}
