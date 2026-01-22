using k8s.Models;

namespace K8sOperator.NET.Generation;

public static class LeaseBuilderExtensions
{
    extension(IObjectBuilder<V1Lease> builder)
    {
        /// <summary>
        /// Sets the holder identity of the lease.
        /// </summary>
        /// <param name="holderIdentity">The holder identity to assign to the lease.</param>
        /// <returns>The configured builder.</returns>
        public IObjectBuilder<V1LeaseSpec> WithSpecs()
        {
            var specBuilder = new ObjectBuilder<V1LeaseSpec>();
            builder.Add(x => x.Spec = specBuilder.Build());
            return specBuilder;
        }
    }

    extension(IObjectBuilder<V1LeaseSpec> builder)
    {
        /// <summary>
        /// Sets the holder identity of the lease.
        /// </summary>
        /// <param name="holderIdentity">The holder identity to assign to the lease.</param>
        /// <returns>The configured builder.</returns>
        public IObjectBuilder<V1LeaseSpec> WithHolderIdentity(string holderIdentity)
        {
            builder.Add(x => x.HolderIdentity = holderIdentity);
            return builder;
        }

        public IObjectBuilder<V1LeaseSpec> WithLeaseDuration(int seconds)
        {
            builder.Add(x => x.LeaseDurationSeconds = seconds);
            return builder;
        }

        public IObjectBuilder<V1LeaseSpec> WithAcquireTime(DateTime acquireTime)
        {
            builder.Add(x => x.AcquireTime = acquireTime);
            return builder;
        }

        public IObjectBuilder<V1LeaseSpec> WithRenewTime(DateTime renewTime)
        {
            builder.Add(x => x.RenewTime = renewTime);
            return builder;
        }
    }
}
