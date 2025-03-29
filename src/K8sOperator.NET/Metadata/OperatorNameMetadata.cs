using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

/// <summary>
/// Interface representing metadata for the name of an operator.
/// </summary>
public interface IOperatorNameMetadata
{
    /// <summary>
    /// Gets the name of the operator.
    /// </summary>
    string OperatorName { get; }
}


/// <summary>
/// Sets the name of the operator at assembly level.
/// </summary>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Assembly)]
public class OperatorNameAttribute(string? name) : Attribute, IOperatorNameMetadata
{
    /// <summary>
    /// Default value of the attribute
    /// </summary>
    public static OperatorNameAttribute Default => new("operator");

    /// <inheritdoc/>
    public string OperatorName => name ?? Default.OperatorName;

    /// <inheritdoc />
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(OperatorName), OperatorName);
}
