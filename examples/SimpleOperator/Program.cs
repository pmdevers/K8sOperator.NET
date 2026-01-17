using K8sOperator.NET;
using SimpleOperator.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOperator();

var app = builder.Build();

app.MapController<TodoController>();

await app.RunOperatorAsync();
