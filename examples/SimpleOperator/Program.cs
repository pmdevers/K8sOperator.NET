using K8sOperator.NET;
using K8sOperator.NET.Extensions;
using SimpleOperator.Projects;

var builder = OperatorHost.CreateOperatorApplicationBuilder(args);

builder.Logging.AddConsole();

builder.AddController<ProjectController>()
    .WithGroup("sonarcloud.io")
    .WithVersion("v1alpha1")
    .WithKind("Project")
    .WithPluralName("projects")
    .WithFinalizer("project.local.finalizer");

var app = builder.Build();

await app.RunAsync();

//var builder = Host.CreateApplicationBuilder(args);

//builder.AddK8sOperators(o => {
//    o.WithGroup("sonarcloud.io");
//    o.WithVersion("v1alpha1");
//    });

//var app = builder.Build();

//app.MapController<ProjectController>()
//    .WithKind("Project")
//    .WithPluralName("projects")
//    .WithFinalizer("project.local.finalizer");

//await app.OperatorAsync();
