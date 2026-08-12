namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record ManifestImportedV1(
    string ManifestId,
    string CanonicalPresentation3Json,
    string SourceVersion,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;

public sealed record ManifestLabelChangedV1(
    string Label,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;

public sealed record ManifestRightsChangedV1(
    string Rights,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;

public sealed record CanvasHeightChangedV1(
    string CanvasId,
    int? PreviousHeight,
    int Height,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;

public sealed record CanvasAddedV1(
    string CanvasId,
    string Label,
    int Height,
    int Width,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;

public sealed record CanvasRemovedV1(
    string CanvasId,
    string RemovedCanvasJson,
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;

public sealed record ManifestDeletedV1(
    DateTimeOffset OccurredAtUtc)
    : IManifestDomainEvent;
