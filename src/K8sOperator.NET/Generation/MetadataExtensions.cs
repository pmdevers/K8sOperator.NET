using k8s.Models;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Generation;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.Generation;

/// <summary>
/// Provides extension methods for metadata manipulation and configuration of Kubernetes controllers.
/// </summary>
public static class MetadataExtensions
{
    extension(ConventionBuilder<ControllerBuilder> builder)
    {
        public ConventionBuilder<ControllerBuilder> WithClusterScope()
            => builder.WithSingle(new ScopeAttribute(EntityScope.Cluster));

        public ConventionBuilder<ControllerBuilder> WithNamespaceScope()
            => builder.WithSingle(new ScopeAttribute(EntityScope.Namespaced));

        /// <summary>
        /// Configures the builder with a Kubernetes entity group, version, kind, and plural name.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="builder">The builder to configure.</param>
        /// <param name="group">The Kubernetes API group.</param>
        /// <param name="version">The API version. Defaults to "v1".</param>
        /// <param name="kind">The kind of the Kubernetes entity.</param>
        /// <param name="pluralName">The plural name of the Kubernetes entity.</param>
        /// <returns>The configured builder.</returns>
        public ConventionBuilder<ControllerBuilder> WithGroup(
            string group = "",
            string version = "v1",
            string kind = "",
            string pluralName = ""
            )
        {
            return builder.WithSingle(new KubernetesEntityAttribute()
            {
                Group = group,
                ApiVersion = version,
                Kind = kind,
                PluralName = pluralName
            });
        }

        /// <summary>
        /// Configures the builder with a label selector for filtering Kubernetes resources.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="builder">The builder to configure.</param>
        /// <param name="labelselector">The label selector string.</param>
        /// <returns>The configured builder.</returns>
        public ConventionBuilder<ControllerBuilder> WithLabel(string labelselector)
            => builder.WithSingle(new LabelSelectorAttribute(labelselector));

        /// <summary>
        /// Configures the builder with a finalizer for the Kubernetes resource.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="builder">The builder to configure.</param>
        /// <param name="finalizer">The finalizer string.</param>
        /// <returns>The configured builder.</returns>
        public ConventionBuilder<ControllerBuilder> WithFinalizer(string finalizer)
            => builder.WithSingle(new FinalizerAttribute(finalizer));


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public ConventionBuilder<ControllerBuilder> RemoveMetadata(object item)
            => builder.Add(b =>
            {
                b.Metadata.RemoveAll(x => x.GetType() == item.GetType());
            });

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public ConventionBuilder<ControllerBuilder> WithMetadata(params object[] items)
            => builder.Add(b =>
            {
                foreach (var item in items)
                {
                    b.Metadata.Add(item);
                }
            });

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public ConventionBuilder<ControllerBuilder> WithSingle(object metadata)
        {
            builder.RemoveMetadata(metadata);
            builder.WithMetadata([metadata]);
            return builder;
        }
    }
}
