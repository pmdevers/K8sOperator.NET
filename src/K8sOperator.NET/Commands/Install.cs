using DotMake.CommandLine;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Helpers;
using K8sOperator.NET.Metadata;
using System.Collections;
using System.Reflection;

namespace K8sOperator.NET.Commands;

[CliCommand(
    Name = "install",
    Description = "Install or update operator",
    Parent = typeof(Root)
    )]
internal class Install(IServiceProvider serviceProvider, IControllerDataSource dataSource)
{
    private static readonly string[] IgnoredToplevelProperties = ["metadata", "apiversion", "kind"];

    [CliOption(Description = "export")]
    public bool Export { get; set; } = true;

    public async Task RunAsync()
    {
        foreach (var item in dataSource.GetWatchers(serviceProvider))
        {
            var type = item.Controller.ResourceType;
            var meta = new MetaData(item.Metadata);

            var crd = new V1CustomResourceDefinition(new()).Initialize();

            crd.Metadata.Name = $"{meta.PluralName}.{meta.Group}";
            crd.Spec.Group = meta.Group;

            crd.Spec.Names = new()
            {
                Kind = meta.Kind,
                ListKind = meta.KindList,
                Singular = meta.SigularName,
                Plural = meta.PluralName
            };

            crd.Spec.Scope = meta.Scope;

            var version = new V1CustomResourceDefinitionVersion(meta.Version, true, true);

            if(type.GetProperty("Status") != null)
            {
                version.Subresources = new V1CustomResourceSubresources(null, new object());
            }

            version.Schema = new V1CustomResourceValidation(new V1JSONSchemaProps
            {
                Type = "object",
                Description = item.Metadata.TryGetValue<DescriptionMetadata, string>(x =>x.Description),
                Properties = type.GetProperties()
                    .Where(p => !IgnoredToplevelProperties.Contains(p.Name.ToLowerInvariant()))
                    .ToDictionary(p => p.GetPropertyName(), p => p.GetPropertySchema())
            });

            crd.Spec.Versions = [version];
            crd.Validate();

            Console.Write(KubernetesYaml.Serialize(crd));
            Console.WriteLine("---");
        }

        await Task.CompletedTask;
    }
}

public static class PropertyInfoExtensions 
{
    private const string Integer = "integer";
    private const string Number = "number";
    private const string String = "string";
    private const string Boolean = "boolean";
    private const string Object = "object";
    private const string Array = "array";

    private const string Int32 = "int32";
    private const string Int64 = "int64";
    private const string Float = "float";
    private const string Double = "double";
    private const string Decimal = "decimal";
    private const string DateTime = "date-time";

    public static string GetPropertyName(this PropertyInfo prop)
    {
        var name = prop.Name;
        return $"{name[..1].ToLowerInvariant()}{name[1..]}";
    }

    internal static V1JSONSchemaProps GetPropertySchema(this PropertyInfo prop)
    {
        var props = prop.PropertyType.GetTypeSchema();

        props.Description = "TODO";

        props.Nullable = prop.IsNullable();

        return props;
    }

    internal static V1JSONSchemaProps GetTypeSchema(this Type type)
    {
        if (type.FullName == "System.String")
        {
            return new V1JSONSchemaProps { Type = String, Nullable = false };
        }

        if (type.Name == typeof(Nullable<>).Name && type.GenericTypeArguments.Length == 1)
        {
            var props = type.GenericTypeArguments[0].GetTypeSchema();
            props.Nullable = true;
            return props;
        }

        return type.BaseType?.FullName switch
        {
            "System.Object" => type.GetObjectType(),
            //"System.ValueType" => context.MapValueType(type),
            "System.Enum" => new V1JSONSchemaProps
            {
                Type = String,
                EnumProperty = Enum.GetNames(type).Cast<object>().ToList(),
            },
            _ => throw new InvalidOperationException($"Invalid type: '{type}'."),
        };
    }

    private static V1JSONSchemaProps GetObjectType(this Type type)
    {
        switch (type.FullName)
        {
            case "k8s.Models.V1ObjectMeta":
                return new V1JSONSchemaProps { Type = Object, Nullable = false };
            case "k8s.Models.IntstrIntOrString":
                return new V1JSONSchemaProps { XKubernetesIntOrString = true, Nullable = false };
            default:
                if (typeof(IKubernetesObject).IsAssignableFrom(type) &&
                    type is { IsAbstract: false, IsInterface: false })
                {
                    return new V1JSONSchemaProps
                    {
                        Type = Object,
                        Properties = null,
                        XKubernetesPreserveUnknownFields = true,
                        XKubernetesEmbeddedResource = true,
                        Nullable = false,
                    };
                }

                return new V1JSONSchemaProps
                {
                    Type = Object,
                    Description = string.Empty,
                    Properties = type
                        .GetProperties()
                        //.Where(p => p.GetCustomAttributeData<IgnoreAttribute>() == null)
                        .Select(p => (Name: p.GetPropertyName(), Schema: p.GetPropertySchema()))
                        .ToDictionary(t => t.Name, t => t.Schema),
                    Required = type.GetProperties()
                            .Select(p => p.GetPropertyName())
                            .ToList() switch
                    {
                        { Count: > 0 } p => p,
                        _ => null,
                    },
                };
        }
    }
}


internal class MetaData(IReadOnlyList<object> metadata)
{
    public string Group => metadata.TryGetValue<GroupMetadata, string>(x => x.Group) ?? string.Empty;
    public string Version => metadata.TryGetValue<ApiVersionMetadata, string>(x => x.ApiVersion) ?? string.Empty;
    public string Kind => metadata.TryGetValue<KindMetadata, string>(x => x.Kind) ?? string.Empty;
    public string KindList => $"{Kind}List";
    public string SigularName => Kind.ToLower();
    public string PluralName => metadata.TryGetValue<PluralNameMetadata, string>(x => x.PluralName) ?? string.Empty;

    public string? Scope => metadata.TryGetValue<EntityScopeMetadata, EntityScope>(x => x.Scope) == EntityScope.Namespaced 
        ? "Namespaced" : null;
}
