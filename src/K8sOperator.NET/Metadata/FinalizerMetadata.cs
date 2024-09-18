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
    string Finalizer { get; }
}


/// <summary>
/// Mark a Controller that it has a finalizer
/// </summary>
/// <param name="finalizer"></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FinalizerAttribute(string finalizer) : Attribute, IFinalizerMetadata
{
    /// <summary>
    /// Default value of the finalizer.
    /// </summary>
    public const string Default = "operator.default.finalizer";

    /// <inheritdoc/>
    public string Finalizer { get; set; } = finalizer;

    /// <inheritdoc/>
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Finalizer), Finalizer);
}








