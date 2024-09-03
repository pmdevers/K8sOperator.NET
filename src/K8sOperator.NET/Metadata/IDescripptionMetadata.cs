using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

internal interface IDescriptionMetadata
{
    string Description { get; }
}


internal class DescriptionMetadata(string description) : IDescriptionMetadata
{
    public string Description => description;
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Description), Description);
}
