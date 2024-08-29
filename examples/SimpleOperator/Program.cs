using K8sOperator.NET.Extensions;
using SimpleOperator.Projects;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddK8sOperators(o => { });

var app = builder.Build();

app.MapController<ProjectController>()
    .WithFinalizer("project.local.finalizer");

await app.RunAsync();
