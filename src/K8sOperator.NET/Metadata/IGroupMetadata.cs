using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

internal interface IGroupMetadata
{
    string Group { get; }
}

internal class GroupMetadata(string group) : IGroupMetadata
{
    public string Group => group;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Group), Group);
}
