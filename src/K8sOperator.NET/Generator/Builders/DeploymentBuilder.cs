using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

public class DeploymentBuilder : KubernetesObjectBuilderWithMetaData<V1Deployment> 
{
    public override V1Deployment Build()
    {
        var deployment = base.Build();
        deployment.Initialize();
        return deployment;
    }
}
