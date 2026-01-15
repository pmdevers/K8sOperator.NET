namespace K8sOperator.NET.Generation;


/// <summary>
/// 
/// </summary>
public enum EntityScope
{
    /// <summary>
    /// The entity is scoped to a specific namespace.
    /// </summary>
    Namespaced = 0,

    /// <summary>
    /// The entity is scoped to the entire cluster.
    /// </summary>
    Cluster = 1
}
