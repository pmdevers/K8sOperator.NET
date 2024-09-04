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
    /// Gets the name of the Docker image.
    /// </summary>
    string Name { get; }

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

[AttributeUsage(AttributeTargets.Assembly)]
internal class DockerImageAttribute(string registery, string repository, string imageName, string tag) : Attribute, IImageMetadata
{
    public static DockerImageAttribute Default => new("ghcr.io", "operator", "operator", "latest");

    public string Registery => registery;
    public string Repository => repository;
    public string Name => imageName;
    public string Tag => tag;

    public string GetImage() => $"{Registery}/{Repository}/{Name}:{Tag}";
}
