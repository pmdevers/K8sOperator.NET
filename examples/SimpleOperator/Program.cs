using K8sOperator.NET;
using K8sOperator.NET.Generation;
using SimpleOperator.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOperator(x =>
{
    x.WithLeaderElection();

});

var app = builder.Build();

app.MapController<TodoController>()
    .WithNamespaceScope();
//.WithFinalizer("todo.example.com/finalizer");

await app.RunOperatorAsync();
