using K8sOperator.NET.Helpers;
using System.Xml.Linq;

namespace K8sOperator.NET.Metadata;

/// <summary>
/// Interface representing metadata for a specific Kubernetes namespace.
/// </summary>
public interface INamespaceMetadata
{
    /// <summary>
    /// Gets the namespace to for Kubernetes resources.
    /// </summary>
    public string Namespace { get; }
}

/// <summary>
/// Sets the Namespace
/// </summary>
/// <param name="ns"></param>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
public sealed class NamespaceAttribute(string ns) : Attribute, INamespaceMetadata
{
    /// <summary>
    /// Default namespace
    /// </summary>
    public static NamespaceAttribute Default => new("default");

    /// <summary>
    /// The namespace
    /// </summary>
    public string Namespace { get; set;} = ns;

    /// <inheritdoc />
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Namespace), Namespace);
}
