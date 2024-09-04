using K8sOperator.NET;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Generators;
using SimpleOperator.Projects;


var builder = OperatorHost.CreateOperatorApplicationBuilder(args);

builder.AddController<TestItemController>()
    .WithFinalizer("testitem.local.finalizer"); 

builder.AddController<ProjectController>()
    .WithFinalizer("project.local.finalizer");

var app = builder.Build();

var writer = new StringWriter();

await new Install(app, writer).RunAsync();

Console.WriteLine(writer.ToString());

await app.RunAsync();
