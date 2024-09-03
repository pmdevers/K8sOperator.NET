﻿using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

public class ClusterRoleBuilder : KubernetesObjectBuilderWithMetaData<V1ClusterRole> {
    public override V1ClusterRole Build()
    {
        var role = base.Build();
        role.Initialize();
        return role;
    }
}
