using K8sOperator.NET.Generation;
using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

[AttributeUsage(AttributeTargets.Class)]
public class ScopeAttribute(EntityScope scope) : Attribute
{
    public static ScopeAttribute Default { get; } = new(EntityScope.Namespaced);
    public EntityScope Scope { get; } = scope;
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Scope), Scope);
}
