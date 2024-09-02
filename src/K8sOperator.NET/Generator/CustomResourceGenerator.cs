using k8s;
using k8s.Models;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.Generator;
internal static class CustomResourceGenerator
{
    private static readonly string[] IgnoredToplevelProperties = ["metadata", "apiversion", "kind"];

    public static IKubernetesObject Generated(Type resourceType, IReadOnlyList<object> metadata)
    {
        var meta = metadata.OfType<KubernetesEntityAttribute>().First();
        var scope = metadata.OfType<EntityScopeMetadata>().FirstOrDefault()?.Scope ?? EntityScope.Namespaced;

        var crd = new V1CustomResourceDefinition(new()).Initialize();

        crd.Metadata.Name = $"{meta.PluralName}.{meta.Group}";
        crd.Spec.Group = meta.Group;

        crd.Spec.Names = new()
        {
            Kind = meta.Kind,
            ListKind = $"{meta.Kind}List",
            Singular = meta.Kind.ToLower(),
            Plural = meta.PluralName
        };

        crd.Spec.Scope = scope.ToString();

        var version = new V1CustomResourceDefinitionVersion(meta.ApiVersion, true, true);

        if (resourceType.GetProperty("Status") != null)
        {
            version.Subresources = new V1CustomResourceSubresources(null, new object());
        }

        version.Schema = new V1CustomResourceValidation(new V1JSONSchemaProps
        {
            Type = "object",
            Description = metadata.TryGetValue<DescriptionMetadata, string>(x => x.Description),
            Properties = resourceType.GetProperties()
                .Where(p => !IgnoredToplevelProperties.Contains(p.Name.ToLowerInvariant()))
                .ToDictionary(p => p.GetPropertyName(), p => p.GetPropertySchema())
        });

        crd.Spec.Versions = [version];
        crd.Validate();

        return crd;
    }
}
