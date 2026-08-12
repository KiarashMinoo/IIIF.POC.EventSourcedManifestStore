using IIIF.POC.EventSourcedManifestStore.Domain;
using IIIF.POC.EventSourcedManifestStore.Domain.Events;
using IIIF.POC.EventSourcedManifestStore.Models;
using KurrentDB.Client;

namespace IIIF.POC.EventSourcedManifestStore.Infrastructure;

public sealed class KurrentManifestEventStore(
    KurrentDBClient client,
    ManifestEventSerializer serializer)
{
    public async Task<LoadedManifestStream?> LoadAsync(
        string manifestId,
        ulong? maxRevision,
        CancellationToken cancellationToken)
    {
        var streamName =
            ManifestStreamName.For(manifestId);

        var read =
            client.ReadStreamAsync(
                Direction.Forwards,
                streamName,
                StreamPosition.Start,
                cancellationToken: cancellationToken);

        if (await read.ReadState == ReadState.StreamNotFound)
            return null;

        var aggregate = new ManifestAggregate();
        var timeline = new List<ManifestTimelineEventView>();

        await foreach (var resolved in read
                           .WithCancellation(cancellationToken))
        {
            var revision =
                resolved.OriginalEventNumber.ToUInt64();

            if (maxRevision.HasValue &&
                revision > maxRevision.Value)
            {
                break;
            }

            var stored =
                serializer.Deserialize(
                    resolved.OriginalEvent.EventType,
                    resolved.OriginalEvent.Data);

            aggregate.Apply(
                stored.Data,
                revision);

            timeline.Add(
                new ManifestTimelineEventView(
                    revision,
                    resolved.OriginalEvent.EventId.ToString(),
                    stored.EventType,
                    stored.Data.OccurredAtUtc,
                    stored.RawJson,
                    stored.Audit));
        }

        if (!aggregate.Exists)
            return null;

        return new LoadedManifestStream(
            streamName,
            aggregate,
            timeline);
    }

    public async Task AppendNewAsync(
        string manifestId,
        IManifestDomainEvent @event,
        SdkChangeSetAuditV1? audit,
        CancellationToken cancellationToken)
    {
        var streamName =
            ManifestStreamName.For(manifestId);

        var eventData =
            serializer.Serialize(
                @event,
                audit);

        await client.AppendToStreamAsync(
            streamName,
            StreamState.NoStream,
            [eventData],
            cancellationToken: cancellationToken);
    }

    public async Task AppendAsync(
        string manifestId,
        ulong expectedRevision,
        IManifestDomainEvent @event,
        SdkChangeSetAuditV1? audit,
        CancellationToken cancellationToken)
    {
        var streamName =
            ManifestStreamName.For(manifestId);

        var eventData =
            serializer.Serialize(
                @event,
                audit);

        await client.AppendToStreamAsync(
            streamName,
            expectedRevision,
            [eventData],
            cancellationToken: cancellationToken);
    }
}

public sealed record LoadedManifestStream(
    string StreamName,
    ManifestAggregate Aggregate,
    IReadOnlyList<ManifestTimelineEventView> Timeline);
