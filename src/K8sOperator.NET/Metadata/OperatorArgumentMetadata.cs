namespace K8sOperator.NET.Metadata;

/// <summary>
/// 
/// </summary>
internal interface ICommandArgumentMetadata
{
    /// <summary>
    /// 
    /// </summary>
    public string Argument { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 
    /// </summary>
    public int Order { get; }
}

/// <summary>
/// 
/// </summary>
/// <param name="argument"></param>
[AttributeUsage(AttributeTargets.Class)]
public class OperatorArgumentAttribute(string argument) : Attribute, ICommandArgumentMetadata
{
    /// <inheritdoc />
    public string Argument {get; set; } = argument;

    /// <inheritdoc />
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public int Order { get; set; } = 1;
}
