using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K8sOperator.NET.Metadata;
public interface IImageMetadata
{
    string Registery { get; }
    string Repository { get; }
    string Name { get; }
    string Tag { get; }
    string GetImage();
}

internal class ImageMetadata(string registery, string repository, string imageName, string tag) : IImageMetadata
{
    public string Registery => registery;
    public string Repository => repository;
    public string Name => imageName;
    public string Tag => tag;

    public string GetImage() => $"{Registery}/{Repository}/{Name}:{Tag}";
}
