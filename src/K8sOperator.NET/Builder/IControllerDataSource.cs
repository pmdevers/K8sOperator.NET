namespace K8sOperator.NET.Builder;

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
    /// Adds a controller to the datasource.
    /// </summary>
    /// <returns></returns>
    IControllerConventionBuilder AddController(Type controllerType);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    IEnumerable<IEventWatcher> GetWatchers(IServiceProvider serviceProvider);
}
