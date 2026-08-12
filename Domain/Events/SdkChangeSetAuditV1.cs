namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record SdkChangeSetAuditV1(
    Guid ChangeSetId,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<SdkChangeAuditEntryV1> Changes);