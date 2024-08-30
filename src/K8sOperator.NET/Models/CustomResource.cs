using k8s;
using k8s.Models;
using System.Text.Json.Serialization;

namespace K8sOperator.NET.Models;

/// <summary>
/// Custom Resource Definition
/// </summary>
public abstract class CustomResource : KubernetesObject, IKubernetesObject<V1ObjectMeta>
{
    /// <inheritdoc />
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; } = new V1ObjectMeta();
}

/// <summary>
/// Custom Resource Definition with Specifications
/// </summary>
/// <typeparam name="TSpec"></typeparam>
public abstract class CustomResource<TSpec> : CustomResource, ISpec<TSpec>
    where TSpec : new()
{
    /// <summary>
    /// Specifications
    /// </summary>
    [JsonPropertyName("spec")]
    public TSpec Spec { get; set; } = new();
}

/// <summary>
/// Custom Resource Definition with Specifications and Status
/// </summary>
/// <typeparam name="TSpec"></typeparam>
/// <typeparam name="TStatus"></typeparam>
public abstract class CustomResource<TSpec, TStatus> : CustomResource<TSpec>, IStatus<TStatus>
    where TSpec : new()
    where TStatus : new()
{
    /// <summary>
    /// Specifications
    /// </summary>
    [JsonPropertyName("status")]
    public TStatus Status { get; set; } = new();
}
