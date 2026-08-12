using System.Text.Json;
using IIIF.POC.EventSourcedManifestStore.Domain.Events;
using KurrentDB.Client;

namespace IIIF.POC.EventSourcedManifestStore.Infrastructure;

public sealed class ManifestEventSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public EventData Serialize(
        IManifestDomainEvent @event,
        SdkChangeSetAuditV1? audit)
    {
        var eventType =
            ManifestEventTypes.For(@event);

        var payload =
            @event switch
            {
                ManifestImportedV1 typed =>
                    SerializeEnvelope(typed, audit),

                ManifestLabelChangedV1 typed =>
                    SerializeEnvelope(typed, audit),

                ManifestRightsChangedV1 typed =>
                    SerializeEnvelope(typed, audit),

                CanvasHeightChangedV1 typed =>
                    SerializeEnvelope(typed, audit),

                CanvasAddedV1 typed =>
                    SerializeEnvelope(typed, audit),

                CanvasRemovedV1 typed =>
                    SerializeEnvelope(typed, audit),

                ManifestDeletedV1 typed =>
                    SerializeEnvelope(typed, audit),

                _ => throw new NotSupportedException(
                    $"Unsupported event type '{@event.GetType().Name}'.")
            };

        return new EventData(
            Uuid.NewUuid(),
            eventType,
            payload);
    }

    public DeserializedManifestEvent Deserialize(
        string eventType,
        ReadOnlyMemory<byte> payload)
    {
        var bytes = payload.ToArray();

        return eventType switch
        {
            ManifestEventTypes.Imported =>
                Read<ManifestImportedV1>(
                    eventType,
                    bytes),

            ManifestEventTypes.LabelChanged =>
                Read<ManifestLabelChangedV1>(
                    eventType,
                    bytes),

            ManifestEventTypes.RightsChanged =>
                Read<ManifestRightsChangedV1>(
                    eventType,
                    bytes),

            ManifestEventTypes.CanvasHeightChanged =>
                Read<CanvasHeightChangedV1>(
                    eventType,
                    bytes),

            ManifestEventTypes.CanvasAdded =>
                Read<CanvasAddedV1>(
                    eventType,
                    bytes),

            ManifestEventTypes.CanvasRemoved =>
                Read<CanvasRemovedV1>(
                    eventType,
                    bytes),

            ManifestEventTypes.Deleted =>
                Read<ManifestDeletedV1>(
                    eventType,
                    bytes),

            _ => throw new NotSupportedException(
                $"Unknown event schema '{eventType}'.")
        };
    }

    private static byte[] SerializeEnvelope<T>(
        T @event,
        SdkChangeSetAuditV1? audit)
        where T : IManifestDomainEvent =>
        JsonSerializer.SerializeToUtf8Bytes(
            new StoredEventEnvelope<T>(
                @event,
                audit),
            JsonOptions);

    private static DeserializedManifestEvent Read<T>(
        string eventType,
        byte[] bytes)
        where T : IManifestDomainEvent
    {
        var envelope =
            JsonSerializer.Deserialize<StoredEventEnvelope<T>>(
                bytes,
                JsonOptions)
            ?? throw new JsonException(
                $"Could not deserialize event '{eventType}'.");

        var rawJson =
            JsonSerializer.Serialize(
                envelope,
                PrettyJsonOptions);

        return new DeserializedManifestEvent(
            eventType,
            envelope.Data,
            envelope.Audit,
            rawJson);
    }
}

public sealed record DeserializedManifestEvent(
    string EventType,
    IManifestDomainEvent Data,
    SdkChangeSetAuditV1? Audit,
    string RawJson);
