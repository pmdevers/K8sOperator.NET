using K8sOperator.NET;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Generators;
using K8sOperator.NET.ProjectTemplate.Commands;
using K8sOperator.NET.ProjectTemplate.Todos;

var builder = OperatorHost.CreateOperatorApplicationBuilder(args)
    .WithNamespace("todo-system");

builder.AddController<TodoController>()
    .WithFinalizer("todo-item.finalizer");

var app = builder.Build();

app.AddInstall();
app.AddCommand<CreateCommand>();

await app.RunAsync();
