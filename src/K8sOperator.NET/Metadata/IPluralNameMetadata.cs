using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

internal interface IPluralNameMetadata
{
    string PluralName { get; }
}
internal class PluralNameMetadata(string pluralName) : IPluralNameMetadata
{
    public string PluralName => pluralName;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(PluralName), PluralName);
}
