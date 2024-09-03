using K8sOperator.NET.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K8sOperator.NET.Metadata;
internal interface IEntityScopeMetadata
{
    EntityScope Scope { get; }
}

public enum EntityScope
{
    Namespaced = 0,
    Cluster = 1
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class EntityScopeMetadata(EntityScope scope) : Attribute, IEntityScopeMetadata
{
    public const EntityScope Default = EntityScope.Namespaced;

    public EntityScope Scope => scope;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Scope), Scope);
}
