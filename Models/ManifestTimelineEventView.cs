using IIIF.POC.EventSourcedManifestStore.Domain.Events;

namespace IIIF.POC.EventSourcedManifestStore.Models;

public sealed record ManifestTimelineEventView(
    ulong Revision,
    string EventId,
    string EventType,
    DateTimeOffset OccurredAtUtc,
    string RawJson,
    SdkChangeSetAuditV1? Audit);