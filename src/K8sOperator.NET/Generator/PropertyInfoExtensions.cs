using k8s;
using k8s.Models;
using K8sOperator.NET.Helpers;
using System.Reflection;

namespace K8sOperator.NET.Generator;

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
