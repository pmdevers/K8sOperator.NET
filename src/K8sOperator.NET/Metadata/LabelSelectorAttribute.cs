using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

[AttributeUsage(AttributeTargets.Class)]
public class LabelSelectorAttribute(string labelSelector) : Attribute
{
    public string LabelSelector { get; } = labelSelector;
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(LabelSelector), LabelSelector);
}
