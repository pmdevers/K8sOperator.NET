using k8s;
using k8s.Models;
using K8sOperator.NET.Helpers;
using K8sOperator.NET.Models;
using System.Reflection;

namespace K8sOperator.NET.Generator.Builders;

/// <summary>
/// Provides extension methods for building Kubernetes CustomResourceDefinitions.
/// </summary>
public static class CustomResourceDefinitionBuilderExtensions {

    /// <summary>
    /// Configures the spec section of the CustomResourceDefinition.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>A builder for configuring the CustomResourceDefinition spec.</returns>
    public static IKubernetesObjectBuilder<V1CustomResourceDefinitionSpec> WithSpec<TBuilder>(this TBuilder builder)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinition>
    {
        var specBuilder = new KubernetesObjectBuilder<V1CustomResourceDefinitionSpec>();
        builder.Add(x => x.Spec = specBuilder.Build());
        return specBuilder;
    }

    /// <summary>
    /// Sets the group for the CustomResourceDefinition.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="group">The API group of the CustomResourceDefinition.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithGroup<TBuilder>(this TBuilder builder, string group)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionSpec>
    {
        builder.Add(x => x.Group = group);
        return builder;
    }

    /// <summary>
    /// Sets the names section of the CustomResourceDefinition, including kind, list kind, plural, singular, shortnames, and categories.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="kind">The kind of the resource.</param>
    /// <param name="kindList">The list kind of the resource.</param>
    /// <param name="plural">The plural form of the resource name.</param>
    /// <param name="singular">The singular form of the resource name.</param>
    /// <param name="shortnames">Optional short names for the resource.</param>
    /// <param name="categories">Optional categories for the resource.</param>
    /// <returns>The configured builder.</returns>
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

    /// <summary>
    /// Sets the scope (Namespaced or Cluster) for the CustomResourceDefinition.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="scope">The scope of the CustomResourceDefinition.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithScope<TBuilder>(this TBuilder builder, EntityScope scope)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionSpec>
    {
        builder.Add(x => x.Scope = scope.ToString());
        return builder;
    }

    /// <summary>
    /// Adds a version to the CustomResourceDefinition, including a schema configuration.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name of the version.</param>
    /// <param name="schema">An action to configure the schema for the version.</param>
    /// <returns>The configured builder.</returns>
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

    /// <summary>
    /// Sets whether the version of the CustomResourceDefinition is served.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="served">A value indicating whether the version is served.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithServed<TBuilder>(this TBuilder builder, bool served)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionVersion>
    {
        builder.Add(x => x.Served = served);
        return builder;
    }

    /// <summary>
    /// Sets whether the version of the CustomResourceDefinition is stored.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="storage">A value indicating whether the version is stored.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithStorage<TBuilder>(this TBuilder builder, bool storage)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionVersion>
    {
        builder.Add(x => x.Storage = storage);
        return builder;
    }

    /// <summary>
    /// Configures the schema for the CustomResourceDefinition version based on the specified resource type.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="resourceType">The type of the resource.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithSchemaForType<TBuilder>(this TBuilder builder, Type resourceType)
        where TBuilder : IKubernetesObjectBuilder<V1CustomResourceDefinitionVersion>
    {
        var b = new KubernetesObjectBuilder<V1CustomResourceValidation>();
        var s = new KubernetesObjectBuilder<V1JSONSchemaProps>();

        s.OfType("object");

        var status = resourceType.GetProperty("Status")!;
        var spec = resourceType.GetProperty("Spec")!;

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

    /// <summary>
    /// Configures the schema for the CustomResourceDefinition version.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="schema">An action to configure the schema.</param>
    /// <returns>The configured builder.</returns>
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

    /// <summary>
    /// Sets the type of the schema property for the CustomResourceDefinition.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="type">The type of the schema property.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder OfType<TBuilder>(this TBuilder builder, string type)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        builder.Add(x => x.Type = type);
        return builder;
    }

    /// <summary>
    /// Configures the schema property for the CustomResourceDefinition based on the provided type.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="type">The type of the schema property.</param>
    /// <returns>The configured builder.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provided type is not valid.</exception>
    public static TBuilder OfType<TBuilder>(this TBuilder builder, Type type)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        if (type.FullName == "System.String")
        {
            builder.Add(x =>
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

    /// <summary>
    /// Sets whether the schema property is nullable.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="nullable">A value indicating whether the property is nullable.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder IsNullable<TBuilder>(this TBuilder builder, bool nullable)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        builder.Add(x => x.Nullable = nullable);
        return builder;
    }

    /// <summary>
    /// Adds a property to the schema with the specified name and configuration.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="schema">An action to configure the schema for the property.</param>
    /// <returns>The configured builder.</returns>
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

    /// <summary>
    /// Sets the required properties for the schema.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="names">The names of the required properties.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithRequired<TBuilder>(this TBuilder builder, IEnumerable<string>? names)
        where TBuilder : IKubernetesObjectBuilder<V1JSONSchemaProps>
    {
        builder.Add(x => x.Required = names?.Select(name => $"{name[..1].ToLowerInvariant()}{name[1..]}").ToList());
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

    

    
}

