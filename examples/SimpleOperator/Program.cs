using K8sOperator.NET;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Generator;
using SimpleOperator.Projects;


var builder = OperatorHost.CreateOperatorApplicationBuilder(args);

builder.AddController<TestItemController>()
    .WithFinalizer("testitem.local.finalizer"); 

builder.AddController<ProjectController>()
    .WithFinalizer("project.local.finalizer");

var app = builder.Build();

await new Install(app).RunAsync();

await app.RunAsync();
