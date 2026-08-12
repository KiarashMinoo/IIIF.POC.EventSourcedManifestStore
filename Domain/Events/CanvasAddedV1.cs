namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record CanvasAddedV1(
    string CanvasId,
    string Label,
    int Height,
    int Width,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;