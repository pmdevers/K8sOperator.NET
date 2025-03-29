using K8sOperator.NET;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Generators;
using K8sOperator.NET.ProjectTemplate.Todos;

var builder = OperatorHost.CreateOperatorApplicationBuilder(args)
    .WithNamespace("todo-system");

builder.AddController<TodoController>()
    .WithFinalizer("todo-item.finalizer");

var app = builder.Build();

app.AddInstall();

await app.RunAsync();
