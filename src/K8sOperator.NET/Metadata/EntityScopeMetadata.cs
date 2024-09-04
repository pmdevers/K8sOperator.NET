using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

/// <summary>
/// Specifies the scope of a Kubernetes entity.
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

/// <summary>
/// Represents metadata that includes the scope of an entity in Kubernetes.
/// </summary>
public interface IEntityScopeMetadata
{
    /// <summary>
    /// Gets the scope of the entity, indicating whether it is namespaced or cluster-wide.
    /// </summary>
    EntityScope Scope { get; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class EntityScopeMetadata(EntityScope scope) : Attribute, IEntityScopeMetadata
{
    public const EntityScope Default = EntityScope.Namespaced;

    public EntityScope Scope => scope;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Scope), Scope);
}
