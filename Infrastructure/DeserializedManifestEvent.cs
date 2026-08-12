using IIIF.POC.EventSourcedManifestStore.Domain.Events;

namespace IIIF.POC.EventSourcedManifestStore.Infrastructure;

public sealed record DeserializedManifestEvent(
    string EventType,
    IManifestDomainEvent Data,
    SdkChangeSetAuditV1? Audit,
    string RawJson);