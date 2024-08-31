namespace K8sOperator.NET.Builder;

/// <summary>
/// 
/// </summary>
public interface IControllerDataSource
{
    /// <summary>
    /// 
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
