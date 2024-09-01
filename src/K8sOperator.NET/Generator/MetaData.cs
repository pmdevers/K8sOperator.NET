using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.Generator;

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
