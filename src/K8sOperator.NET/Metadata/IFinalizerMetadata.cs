using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;
internal interface IFinalizerMetadata
{
    string Name { get; }
}

internal class FinalizerMetadata(string name) : IFinalizerMetadata
{
    public const string Default = "operator.default.finalizer";

    public string Name => name;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Name), Name);
}








