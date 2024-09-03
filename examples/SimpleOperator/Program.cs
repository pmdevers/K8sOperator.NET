using K8sOperator.NET;
using K8sOperator.NET.Extensions;
using SimpleOperator.Projects;

var builder = OperatorHost.CreateOperatorApplicationBuilder(args);

builder.WithName("simple-operator");
builder.WithImage(repository: "pmdevers", name: "simple-operator", tag: "1.0.0");

builder.AddController<TestItemController>()
    .WithFinalizer("testitem.local.finalizer"); 

builder.AddController<ProjectController>()
    .WithFinalizer("project.local.finalizer");

var app = builder.Build();

await app.RunAsync();



