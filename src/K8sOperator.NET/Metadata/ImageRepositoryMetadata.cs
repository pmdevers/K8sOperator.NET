using K8sOperator.NET.Helpers;

namespace K8sOperator.NET.Metadata;

/// <summary>
/// Interface representing metadata for a Docker image.
/// </summary>
public interface IImageMetadata
{
    /// <summary>
    /// Gets the Docker registry where the image is stored.
    /// </summary>
    string Registery { get; }

    /// <summary>
    /// Gets the Docker repository containing the image.
    /// </summary>
    string Repository { get; }

    /// <summary>
    /// Gets the tag of the Docker image.
    /// </summary>
    string Tag { get; }

    /// <summary>
    /// Constructs and returns the full image name in the format "registry/repository/name:tag".
    /// </summary>
    /// <returns>The full Docker image name.</returns>
    string GetImage();
}

/// <summary>
/// Annotates the assemnbly with the docker image information.
/// </summary>
/// <param name="registery">The registry of the image.</param>
/// <param name="repository">The image repository name.</param>
/// <param name="tag">The tag of the image.</param>
[AttributeUsage(AttributeTargets.Assembly)]
public class DockerImageAttribute(string registery, string repository, string tag) : Attribute, IImageMetadata
{
    /// <summary>
    /// Default docker image
    /// </summary>
    public static DockerImageAttribute Default => new("ghcr.io", "operator/operator", "latest");

    /// <inheritdoc/>
    public string Registery {get;set;} =  registery;
    /// <inheritdoc/>
    public string Repository { get; set; } = repository;
    /// <inheritdoc/>
    public string Tag { get; set; } = tag;
    /// <inheritdoc/>
    public string GetImage() => $"{Registery}/{Repository}:{Tag}";


    /// <inheritdoc />
    public override string ToString()
        => DebuggerHelpers.GetDebugText("DockerImage", GetImage());
}
