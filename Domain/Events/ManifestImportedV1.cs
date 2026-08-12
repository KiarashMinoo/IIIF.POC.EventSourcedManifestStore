namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record ManifestImportedV1(
    string ManifestId,
    string CanonicalPresentation3Json,
    string SourceVersion,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;