using K8sOperator.NET.Extensions;
using SimpleOperator.Projects;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddK8sOperators(o => {
    o.WithGroup("sonarcloud.io");
    o.WithVersion("v1alpha1");
    });

var app = builder.Build();

app.MapController<ProjectController>()
    .WithKind("Project")
    .WithPluralName("projects")
    .WithFinalizer("project.local.finalizer");

await app.RunAsync();
