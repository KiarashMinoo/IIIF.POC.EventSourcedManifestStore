namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record SdkChangeAuditEntryV1(
    string Path,
    string Kind,
    string? PropertyName,
    string? OriginalValueJson,
    string? CurrentValueJson,
    DateTimeOffset DetectedAtUtc);