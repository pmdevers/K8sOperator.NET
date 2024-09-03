using K8sOperator.NET.Helpers;
using System.Xml.Linq;

namespace K8sOperator.NET.Metadata;

/// <summary>
/// Interface representing metadata for a label selector used to filter Kubernetes resources.
/// </summary>
public interface ILabelSelectorMetadata
{
    /// <summary>
    /// Gets the label selector string used to filter resources based on labels.
    /// </summary>
    public string LabelSelector { get; }
}

internal class LabelSelectorMetadata(string labelSelector) : ILabelSelectorMetadata
{
    public string LabelSelector => labelSelector;
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(LabelSelector), LabelSelector);
}
