using K8sOperator.NET.Generator.Builders;
using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;
internal interface IEntityScopeMetadata
{
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
