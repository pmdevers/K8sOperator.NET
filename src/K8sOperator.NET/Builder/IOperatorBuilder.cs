namespace K8sOperator.NET.Builder;

/// <summary>
/// Describes a OperatorBuilder.
/// </summary>
public interface IOperatorBuilder
{
    /// <summary>
    /// ServiceProvider
    /// </summary>
    IServiceProvider ServiceProvider { get; }
    /// <summary>
    /// Operator Datasource
    /// </summary>
    IOperatorDataSource DataSource { get; set; }

    /// <summary>
    /// MetaData for this datasource
    /// </summary>
    List<object> MetaData { get; }
}


public interface IOperatorDataSource
{
    /// <summary>
    /// Service Provider.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    OperatorConventionBuilder AddController(string group, string version, string entity, Delegate watch);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerable<IController> GetProcesses();
}
