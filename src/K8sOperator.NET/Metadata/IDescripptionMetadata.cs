using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

internal interface IDescripptionMetadata
{
    string Description { get; }
}


internal class DescriptionMetadata(string description) : IDescripptionMetadata
{
    public string Description => description;
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Description), Description);
}
