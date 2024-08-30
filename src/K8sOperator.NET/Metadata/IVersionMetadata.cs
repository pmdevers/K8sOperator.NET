using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

internal interface IApiVersionMetadata
{
    string ApiVersion { get; }
}

internal class ApiVersionMetadata(string apiVersion) : IApiVersionMetadata
{
    public string ApiVersion => apiVersion;

    public override string ToString()
        => DebuggerHelpers.GetDebugText(nameof(ApiVersion), ApiVersion);
}
