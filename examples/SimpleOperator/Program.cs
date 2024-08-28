var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.AddK8sOperators();

var app = builder.Build();



app.Run();
