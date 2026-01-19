using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

[AttributeUsage(AttributeTargets.Class)]
public class FinalizerAttribute(string finalizer) : Attribute
{
    public static FinalizerAttribute Default => new("default");
    public string Finalizer { get; } = finalizer;
    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(Finalizer), Finalizer);
}
