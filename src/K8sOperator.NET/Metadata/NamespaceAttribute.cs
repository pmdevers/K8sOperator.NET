using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
public sealed class NamespaceAttribute(string ns) : Attribute
{
    public static NamespaceAttribute Default => new("default");

    public string Namespace { get; set; } = ns;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Namespace), Namespace);
}

