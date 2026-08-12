namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public interface IManifestDomainEvent
{
    DateTimeOffset OccurredAtUtc { get; }
}
