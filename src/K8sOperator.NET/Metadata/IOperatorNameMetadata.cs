namespace K8sOperator.NET.Metadata;
public interface IOperatorNameMetadata
{
    string Name { get; }
}

internal class OperatorNameMetadata(string name) : IOperatorNameMetadata
{
    public string Name => name;
}
