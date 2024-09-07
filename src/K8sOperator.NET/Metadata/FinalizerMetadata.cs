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


/// <summary>
/// Mark a Controller that it has a finalizer
/// </summary>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FinalizerMetadata(string name) : Attribute, IFinalizerMetadata
{
    /// <summary>
    /// Default value of the finalizer.
    /// </summary>
    public const string Default = "operator.default.finalizer";

    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Name), Name);
}








