using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

/// <summary>
/// 
/// </summary>
public interface IWatchNamespaceMetadata
{
    /// <summary>
    /// The namespace to watch.
    /// </summary>
    public string Namespace { get; }
}

internal class WatchNamespaceMetadata(string ns) : IWatchNamespaceMetadata
{
    public string Namespace => ns;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Namespace), Namespace);
}
