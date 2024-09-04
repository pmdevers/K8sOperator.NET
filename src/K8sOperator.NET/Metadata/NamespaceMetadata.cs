using K8sOperator.NET.Helpers;
using System.Xml.Linq;

namespace K8sOperator.NET.Metadata;

/// <summary>
/// Interface representing metadata for watching a specific Kubernetes namespace.
/// </summary>
public interface IWatchNamespaceMetadata
{
    /// <summary>
    /// Gets the namespace to watch for Kubernetes resources.
    /// </summary>
    public string Namespace { get; }
}

internal class WatchNamespaceMetadata(string ns) : IWatchNamespaceMetadata
{
    public string Namespace => ns;
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Namespace), Namespace);
}
