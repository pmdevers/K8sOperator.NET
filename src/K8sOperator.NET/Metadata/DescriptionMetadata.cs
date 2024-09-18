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


/// <summary>
/// 
/// </summary>
/// <param name="description"></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class DescriptionAttribute(string description) : Attribute, IDescriptionMetadata
{
    /// <summary>
    /// 
    /// </summary>
    public string Description { get; set; } = description;

    /// <inheritdoc />
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Description), Description);
}
