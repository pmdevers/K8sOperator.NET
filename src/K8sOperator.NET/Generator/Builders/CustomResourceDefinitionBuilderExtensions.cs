using k8s;
using k8s.Models;
using K8sOperator.NET.Helpers;
using K8sOperator.NET.Models;
using System.Reflection;

namespace K8sOperator.NET.Generator.Builders;

public static class CustomResourceDefinitionBuilderExtensions {

    public static IKubernetesObjectBuilder<V1CustomResourceDefinitionSpec> WithSpec<TBuilder>(this TBuilder builder)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinition>
    {
        var specBuilder = new KubernetesObjectBuilder<V1CustomResourceDefinitionSpec>();
        builder.Add(x => x.Spec = specBuilder.Build());
        return specBuilder;
    }
    public static TBuilder WithGroup<TBuilder>(this TBuilder builder, string group)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionSpec>
    {
        builder.Add(x => x.Group = group);
        return builder;
    }

    public static TBuilder WithNames<TBuilder>(this TBuilder builder, 
        string kind,
        string kindList,
        string plural,
        string singular,
        string[]? shortnames = null,
        string[]? categories = null)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionSpec>
    {
        builder.Add(x => x.Names = new()
        {
            Kind = kind,
            ListKind = kindList,
            Plural = plural,
            Singular = singular,
            Categories = categories,
            ShortNames = shortnames,
        });
        return builder;
    }

    public static TBuilder WithScope<TBuilder>(this TBuilder builder, Scope scope)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionSpec>
    {
        builder.Add(x => x.Scope = scope.ToString());
        return builder;
    }
    public static TBuilder WithVersion<TBuilder>(this TBuilder builder, string name, Action<IKubernetesObjectBuilder<V1CustomResourceDefinitionVersion>> schema)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionSpec>
    {
        var b = new KubernetesObjectBuilder<V1CustomResourceDefinitionVersion>();
        b.Add(x => x.Name = name);
        schema(b);

        builder.Add(x => {

            x.Versions ??= [];
            if(!x.Versions.Any(x=>x.Name == name))
            {
                x.Versions.Add(b.Build());
            }
            
        });
        return builder;
    }
    public static TBuilder WithSchemaForType<TBuilder>(this TBuilder builder, Type resourceType)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionVersion>
    {
        var b = new KubernetesObjectBuilder<V1CustomResourceValidation>();
        var s = new KubernetesObjectBuilder<V1JSONSchemaProps>();

        s.OfType("object");

        var status = resourceType.GetProperty("Status");
        var spec = resourceType.GetProperty("Spec");

        s.WithProperty("status", sub => {
            sub.ObjectType(status.PropertyType);
        });
        s.WithProperty("spec", sub => sub.ObjectType(spec.PropertyType));

        builder.Add(x => {
            b.Add(x => x.OpenAPIV3Schema = s.Build());
            x.Schema = b.Build();
        });
        return builder;
    }

    public static TBuilder WithSchema<TBuilder>(this TBuilder builder, Action<IKubernetesObjectBuilder<V1JSONSchemaProps>> schema)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionVersion>
    {
        var b = new KubernetesObjectBuilder<V1CustomResourceValidation>();
        var s = new KubernetesObjectBuilder<V1JSONSchemaProps>();
        schema(s);
        
        builder.Add(x =>{
            b.Add(x=>x.OpenAPIV3Schema = s.Build());
            x.Schema = b.Build();
        });
        return builder;
    }
    
    public static TBuilder OfType<TBuilder>(this TBuilder builder, string type)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        builder.Add(x => x.Type = type);
        return builder;
    }

    private static TBuilder ObjectType<TBuilder>(this TBuilder builder, Type type)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        switch (type.FullName)
        {
            case "k8s.Models.V1ObjectMeta":
                builder.Add(x =>
                {
                    x.Type = "object";
                    x.Nullable = false;
                });
                return builder;
            case "k8s.Models.IntstrIntOrString":
                builder.Add(x =>
                {
                    x.XKubernetesIntOrString = true;
                    x.Nullable = false;
                });
                return builder;

            default:
                if (typeof(IKubernetesObject).IsAssignableFrom(type) &&
                    type is { IsAbstract: false, IsInterface: false })
                {
                    builder.Add(x =>
                    {
                        x.Type = "object";
                        x.Properties = null;
                        x.XKubernetesPreserveUnknownFields = true;
                        x.XKubernetesEmbeddedResource = true;
                        x.Nullable = false;
                    });
                    return builder;
                }

                builder.OfType("object");
                builder.IsNullable(false);
                foreach(var prop in type.GetProperties())
                {
                    builder.WithProperty(prop.Name, s => s.OfType(prop.PropertyType));
                }

                builder.WithRequired(type.GetProperties()
                    .Where(x => !x.IsNullable())
                    .Select(x => x.Name).ToList() switch
                    {
                        { Count: > 0 } p => p,
                        _ => null,
                    });

                return builder;
        }
    }
    private static TBuilder EnumType<TBuilder>(this TBuilder builder, Type type)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        builder.Add(x =>
        {
            x.Type = "string";
            x.EnumProperty = Enum.GetNames(type).Cast<object>().ToList();
        });
        return builder;
    }

    public static TBuilder OfType<TBuilder>(this TBuilder builder, Type type)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        if (type.FullName == "System.String")
        {
            builder.Add(x=>
            {
                x.Type = "string";
                x.Nullable = false;
            });
            return builder;
        }

        if (type.Name == typeof(Nullable<>).Name && type.GenericTypeArguments.Length == 1)
        {
            return builder.OfType(type.GenericTypeArguments[0]); 
        }
        
        return type.BaseType?.FullName switch
        {
            "System.Object" => builder.ObjectType(type),
            //"System.ValueType" => context.MapValueType(type),
            "System.Enum" => builder.EnumType(type),
            _ => throw new InvalidOperationException($"Invalid type: '{type}'."),
        };
    }

    public static TBuilder IsNullable<TBuilder>(this TBuilder builder, bool nullable)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        builder.Add(x => x.Nullable = nullable);
        return builder;
    }

    public static TBuilder WithProperty<TBuilder>(this TBuilder builder, string name, Action<IKubernetesObjectBuilder<V1JSONSchemaProps>> schema)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        var p = new KubernetesObjectBuilder<V1JSONSchemaProps>();
        schema(p);

        builder.Add(x => {
            x.Properties ??= new Dictionary<string, V1JSONSchemaProps>();
            x.Properties.Add($"{name[..1].ToLowerInvariant()}{name[1..]}", p.Build());
        });
        return builder;
    }

    public static TBuilder WithRequired<TBuilder>(this TBuilder builder, IEnumerable<string>? names)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        builder.Add(x=> x.Required = names?.Select(name => $"{name[..1].ToLowerInvariant()}{name[1..]}").ToList());
        return builder;
    }
}

