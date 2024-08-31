using K8sOperator.NET;
using K8sOperator.NET.Extensions;
using SimpleOperator.Projects;

var builder = OperatorHost.CreateOperatorApplicationBuilder(args);

builder.AddController<ProjectController>()
    .WithGroup("sonarcloud.io")
    .WithVersion("v1alpha1")
    .WithKind("Project")
    .WithPluralName("projects")
    .WithFinalizer("project.local.finalizer");

var app = builder.Build();

await app.RunAsync();
