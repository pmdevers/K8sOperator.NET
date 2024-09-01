using K8sOperator.NET;
using K8sOperator.NET.Extensions;
using SimpleOperator.Projects;

var builder = OperatorHost.CreateOperatorApplicationBuilder(args);

builder.WithName("sonarcube-operator");
builder.WithImage(
    repository: "pmdevers", 
    name: "sonarcube-operator", 
    tag: "fc5d6122d6ff1057062e368214ddf4cfe34f5d6d"
);

builder.AddController<ProjectController>()
    .WithGroup("sonarcloud.io")
    .WithVersion("v1alpha1")
    .WithKind("Project")
    .WithPluralName("projects")
    .WithFinalizer("project.local.finalizer");

var app = builder.Build();

await app.RunAsync();
