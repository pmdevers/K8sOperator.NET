using k8s;
using K8sOperator.NET.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace K8sOperator.NET.Commands;

[OperatorArgument("create", Description = "Creates a resource definition.", Order = 4)]
public class CreateCommand(IHost app) : IOperatorCommand
{
    public Task RunAsync(string[] args)
    {
        if(args.Length < 2)
        {
            Console.WriteLine($"Please provide a resourcename.");
            return Task.CompletedTask; 
        }

        var datasource = app.Services.GetRequiredService<EventWatcherDatasource>();
        var watchers = datasource.GetWatchers().ToList();
        var watcher = watchers.FirstOrDefault(x => x.Controller.ResourceType.Name.Equals(args[1], StringComparison.CurrentCultureIgnoreCase));

        if(watcher == null)
        {
            Console.WriteLine($"Unknown resource: {args[1]}");
            return Task.CompletedTask;
        }

        var activator = Activator.CreateInstance(watcher.Controller.ResourceType) as CustomResource;
        activator.Initialize();
        Console.WriteLine(KubernetesYaml.Serialize(activator));
        return Task.CompletedTask;
    }
}
