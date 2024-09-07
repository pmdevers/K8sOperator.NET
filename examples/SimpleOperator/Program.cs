using K8sOperator.NET;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Generators;
using SimpleOperator.Projects;


var builder = OperatorHost.CreateOperatorApplicationBuilder(args)
    .WithName("simple-operator");

builder.AddController<TestItemController>()
    .WithFinalizer("testitem.local.finalizer"); 

builder.AddController<ProjectController>()
    .WithFinalizer("project.local.finalizer");

var app = builder.Build();

app.AddInstall();

await app.RunAsync();
