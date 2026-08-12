namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record CanvasHeightChangedV1(
    string CanvasId,
    int? PreviousHeight,
    int Height,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;