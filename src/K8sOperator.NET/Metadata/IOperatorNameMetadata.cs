namespace K8sOperator.NET.Metadata;
public interface IOperatorNameMetadata
{
    string Name { get; }
}

[AttributeUsage(AttributeTargets.Assembly)]
public class OperatorNameAttribute(string name) : Attribute, IOperatorNameMetadata
{
    public string Name => name;
}
