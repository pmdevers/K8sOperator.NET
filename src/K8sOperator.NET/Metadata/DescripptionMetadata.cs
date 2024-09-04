using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

/// <summary>
/// Represents metadata that includes a description.
/// </summary>
public interface IDescriptionMetadata
{
    /// <summary>
    /// Gets the description associated with the metadata.
    /// </summary>
    string Description { get; }
}


internal class DescriptionMetadata(string description) : IDescriptionMetadata
{
    public string Description => description;
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Description), Description);
}
