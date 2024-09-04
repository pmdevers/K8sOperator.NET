namespace K8sOperator.NET.Metadata;

/// <summary>
/// Interface representing metadata for the name of an operator.
/// </summary>
public interface IOperatorNameMetadata
{
    /// <summary>
    /// Gets the name of the operator.
    /// </summary>
    string Name { get; }
}

[AttributeUsage(AttributeTargets.Assembly)]
internal class OperatorNameAttribute(string name) : Attribute, IOperatorNameMetadata
{
    public static OperatorNameAttribute Default => new("operator");

    public string Name => name;
}
