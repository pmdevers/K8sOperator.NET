using K8sOperator.NET.Helpers;
using System.Xml.Linq;

namespace K8sOperator.NET.Metadata;

/// <summary>
/// 
/// </summary>
public interface ILabelSelectorMetadata
{
    /// <summary>
    /// The namespace to watch.
    /// </summary>
    public string LabelSelector { get; }
}

internal class LabelSelectorMetadata(string labelSelector) : ILabelSelectorMetadata
{
    public string LabelSelector => labelSelector;
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(LabelSelector), LabelSelector);
}
