using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

internal interface IKindMetadata
{
    string Kind { get; }
}


internal class KindMetadata(string kind) : IKindMetadata
{
    public string Kind => kind;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Kind), Kind);
}
