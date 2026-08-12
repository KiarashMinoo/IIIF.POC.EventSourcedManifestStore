namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record CanvasRemovedV1(
    string CanvasId,
    string RemovedCanvasJson,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;