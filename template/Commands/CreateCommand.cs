using k8s;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace K8sOperator.NET.ProjectTemplate.Commands;

[OperatorArgument("create", Description = "Create a custom resource definition.")]
public class CreateCommand(IOperatorApplication app) : IOperatorCommand
{
    public Task RunAsync(string[] args)
    {
        var watchers = app.DataSource.GetWatchers(app.ServiceProvider);

        if (args.Length == 1)
        {
            Console.WriteLine($"Available Resources:");
            foreach (var w in watchers)
            {
                Console.WriteLine($"  {w.Controller.ResourceType.Name}");
            }

            Console.WriteLine($"Usage: create <resource>");

            return Task.CompletedTask;
        }

        var resourceName = args[1];
        var watcher = watchers.FirstOrDefault(w => w.Controller.ResourceType.Name == resourceName);
        if (watcher == null)
        {
            Console.WriteLine($"Resource {resourceName} not found.");
            return Task.CompletedTask;
        }

        var resource = Activator.CreateInstance(watcher.Controller.ResourceType) as IKubernetesObject;

        Console.WriteLine(KubernetesYaml.Serialize(resource.Initialize()));

        return Task.CompletedTask;
    }
}
