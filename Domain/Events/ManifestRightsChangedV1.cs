namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record ManifestRightsChangedV1(
    string Rights,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;