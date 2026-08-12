namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record ManifestLabelChangedV1(
    string Label,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;