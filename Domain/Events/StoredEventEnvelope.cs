namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public sealed record StoredEventEnvelope<T>(
    T Data,
    SdkChangeSetAuditV1? Audit)
    where T : IManifestDomainEvent;
