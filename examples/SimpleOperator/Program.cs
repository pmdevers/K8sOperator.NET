using k8s;
using K8sOperator.NET;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Generator.Builders;
using SimpleOperator.Projects;

var deployment = new DeploymentBuilder(); //DeploymentBuilder.Create($"{name}-operator", image);

deployment.WithName("Test");
deployment.WithLabel("operator-deployment", "sonarcube-operator")
        .WithSpec()
        .WithReplicas(1)
        .WitRevisionHistory(0)
        .WithTemplate()
        .WithPod()
        .AddContainer()
        .Add(x => x.Image = "ghcr.io/pmdevers/simple-operator:1.0.0");

Console.WriteLine(KubernetesYaml.Serialize(deployment.Build()));

var builder = OperatorHost.CreateOperatorApplicationBuilder(args);

builder.WithName("sonarcube-operator");
builder.WithImage(
    repository: "pmdevers", 
    name: "sonarcube-operator", 
    tag: "fc5d6122d6ff1057062e368214ddf4cfe34f5d6d"
);

builder.AddController<TestItemController>()
    .WithFinalizer("testitem.local.finalizer"); 

builder.AddController<ProjectController>()
    .WithFinalizer("project.local.finalizer");

var app = builder.Build();

await app.RunAsync();



