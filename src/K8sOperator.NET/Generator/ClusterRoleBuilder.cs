using k8s;
using k8s.Models;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.Generator;

public interface IClusterRoleBuilder
{
    string Name { get; }
    List<V1PolicyRule> Rules { get; }
    V1ClusterRole Build();
}

internal class ClusterRoleBuilder : IClusterRoleBuilder
{
    private readonly List<V1PolicyRule> _rules = [];
    public string Name { get; set; }
    public List<V1PolicyRule> Rules => _rules;

    private ClusterRoleBuilder(string name)
    {
        Name = name;
    }

    public static IClusterRoleBuilder Create(string name)
    {
        return new ClusterRoleBuilder(name);
    }

    public V1ClusterRole Build()
    {
        var role = new V1ClusterRole(
            metadata: new() 
            {  
                Name = Name 
            }, 
            rules: Rules
        ).Initialize();
        
        role.Validate();

        return role;
    }
}


public static class ClusterRoleBuilderExtensions
{
    public static IClusterRoleBuilder AddDefaultRules(this IClusterRoleBuilder builder)
    {
        builder.Rules.Add(new V1PolicyRule() {
            ApiGroups = [ "" ],
            Resources = [ "events" ],
            Verbs = ["get", "list", "create", "update" ]
        });

        builder.Rules.Add(new V1PolicyRule()
        {
            ApiGroups = [""],
            Resources = ["events"],
            Verbs = ["get", "list", "create", "update"]
        });

        return builder;
    }

    public static IClusterRoleBuilder AddRuleFor(this IClusterRoleBuilder builder, Type resourceType, IReadOnlyList<object> metadata)
    {
        builder.Rules.Add(new V1PolicyRule()
        {
            ApiGroups = [metadata.TryGetValue<GroupMetadata, string>(x => x.Group)],
            Resources = [metadata.TryGetValue<PluralNameMetadata, string>(x => x.PluralName)],
            Verbs = ["*"],
        });
        builder.Rules.Add(new V1PolicyRule()
        {
            ApiGroups = [metadata.TryGetValue<GroupMetadata, string>(x => x.Group)],
            Resources = [$"{metadata.TryGetValue<PluralNameMetadata, string>(x => x.PluralName)}/status"],
            Verbs = ["get", "update", "patch"],
        });

        return builder;
    }
}
