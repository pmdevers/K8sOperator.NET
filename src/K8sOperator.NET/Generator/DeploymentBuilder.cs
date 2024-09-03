using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generator;

public interface IDeploymentBuilder
{
    V1Deployment Build();
}

internal class DeploymentBuilder : IDeploymentBuilder
{
    public string Name { get; }
    public string Image { get; }

    private DeploymentBuilder(string name, string image) 
    { 
        Name = name;
        Image = image;
    }

    public static IDeploymentBuilder Create(string name, string image)
    {
        return new DeploymentBuilder(name, image);
    }

    public V1Deployment Build()
    {
        var labels = new Dictionary<string,string>()
        {
            { "operator-deployment", $"{Name}" }
        };

        var envar = new V1EnvVar()
        {
            Name = "POD_NAMESPACE",
            ValueFrom = new V1EnvVarSource()
            {
                FieldRef = new() { FieldPath = "metadata.namespace" }
            }
        };

        var container = new V1Container()
        {
            Name = Name,
            Image = Image,
            Env = [envar],
            Resources = new()
            {
                Limits = new Dictionary<string, ResourceQuantity>()
                {
                    { "cpu", new ResourceQuantity("100m") },
                    { "memory", new ResourceQuantity("128Mi") },
                },
                Requests = new Dictionary<string, ResourceQuantity>()
                {
                    { "cpu", new ResourceQuantity("100m") },
                    { "memory", new ResourceQuantity("64Mi") },
                }
            }
        };

        var deployment = new V1Deployment(metadata: new()
        {
            Name = Name,
            Labels = labels
        }, spec: new()
        {
            Replicas = 1,
            RevisionHistoryLimit = 0,
            Selector = new() {  MatchLabels = labels },
            Template = new()
            {
                Metadata = new() {  Labels =  labels },
                Spec = new()
                {
                    Containers = [container]
                }
            }
        })
        .Initialize();  
        return deployment;
    }
}


public static class IDeploymentBuilderExtensions
{
    //public static TBuilder WithName<TBuilder>(this TBuilder builder, string name)
    //    where TBuilder : IDeploymentBuilder
    //{
    //    builder.Name = name;
    //    return builder;
    //}
}
