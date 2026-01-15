namespace K8sOperator.NET;

public class ChangeTracker
{
    private readonly Dictionary<string, long> _lastResourceGenerationProcessed = [];

    public bool IsResourceGenerationAlreadyHandled(CustomResource resource)
    {
        bool processedInPast = _lastResourceGenerationProcessed.TryGetValue(resource.Metadata.Uid, out long resourceGeneration);

        return processedInPast
            && resource.Metadata.Generation != null
            && resourceGeneration >= resource.Metadata.Generation.Value;
    }

    public void TrackResourceGenerationAsHandled(CustomResource resource)
    {
        if (resource.Metadata.Generation != null)
        {
            _lastResourceGenerationProcessed[resource.Metadata.Uid] = resource.Metadata.Generation.Value;
        }
    }

    public void TrackResourceGenerationAsDeleted(CustomResource resource)
    {
        _lastResourceGenerationProcessed.Remove(resource.Metadata.Uid);
    }
}
