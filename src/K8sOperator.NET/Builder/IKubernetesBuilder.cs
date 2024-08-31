using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Builder;
public interface IKubernetesBuilder
{
    public IServiceCollection Services { get; }
}
