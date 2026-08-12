namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record SdkChangeAuditEntryV1(
    string Path,
    string Kind,
    string? PropertyName,
    string? OriginalValueJson,
    string? CurrentValueJson,
    DateTimeOffset DetectedAtUtc);

public sealed record SdkChangeSetAuditV1(
    Guid ChangeSetId,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<SdkChangeAuditEntryV1> Changes);
