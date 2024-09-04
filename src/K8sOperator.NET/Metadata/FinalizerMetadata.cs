using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

/// <summary>
/// Represents metadata for a Kubernetes finalizer.
/// </summary>
public interface IFinalizerMetadata
{
    /// <summary>
    /// Gets the name of the finalizer.
    /// </summary>
    string Name { get; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class FinalizerMetadata(string name) : Attribute, IFinalizerMetadata
{
    public const string Default = "operator.default.finalizer";

    public string Name => name;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Name), Name);
}








