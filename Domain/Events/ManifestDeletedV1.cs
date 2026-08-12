namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record ManifestDeletedV1(
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;