using IIIF.Manifests.Serializer;
using IIIF.Manifests.Serializer.Nodes;
using IIIF.Manifests.Serializer.Properties;
using IIIF.Manifests.Serializer.Validation;
using IIIF.POC.EventSourcedManifestStore.Domain.Events;
using IIIF.POC.EventSourcedManifestStore.Infrastructure;
using IIIF.POC.EventSourcedManifestStore.Models;
using KurrentDB.Client;
using Newtonsoft.Json;

namespace IIIF.POC.EventSourcedManifestStore.Services;

public sealed class ManifestApplicationService(
    KurrentManifestEventStore eventStore)
{
    private static readonly JsonSerializerSettings IiifJsonSettings = new()
    {
        ContractResolver = new IIIFJsonContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore
    };

    public async Task<OperationResult> ImportAsync(
        string json,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Failure("Manifest JSON is required.");

        try
        {
            var validation =
                IiifValidator.ValidateJson(json);

            if (!validation.IsValid)
            {
                var first =
                    validation.Errors.FirstOrDefault();

                return Failure(
                    first is null
                        ? "The Manifest failed validation."
                        : $"Validation failed: {first.RuleId} at {first.Path}: {first.Message}");
            }

            var sourceVersion =
                IiifPresentationVersionDetector
                    .Detect(json)
                    .ToString();

            var manifest =
                IiifSerializer.DeserializeManifest(json);

            var canonical =
                IiifSerializer.Serialize(
                    manifest,
                    new IiifSerializerOptions(
                        IiifPresentationVersion.V3_0));

            var imported =
                new ManifestImportedV1(
                    manifest.Id,
                    canonical,
                    sourceVersion,
                    DateTimeOffset.UtcNow);

            try
            {
                await eventStore.AppendNewAsync(
                    manifest.Id,
                    imported,
                    audit: null,
                    cancellationToken);
            }
            catch (Exception ex) when (
                IsConcurrencyException(ex))
            {
                return Failure(
                    $"A stream for Manifest '{manifest.Id}' already exists.");
            }

            return Success(
                manifest.Id,
                revision: 0);
        }
        catch (Exception ex) when (
            ex is System.Text.Json.JsonException ||
            ex is JsonException ||
            ex is ArgumentException ||
            ex is NotSupportedException)
        {
            return Failure(
                $"The Manifest could not be imported: {ex.Message}");
        }
    }

    public async Task<ManifestDetailsView?> GetAsync(
        string manifestId,
        ulong? revision,
        CancellationToken cancellationToken)
    {
        var loaded =
            await eventStore.LoadAsync(
                manifestId,
                revision,
                cancellationToken);

        if (loaded is null ||
            loaded.Aggregate.Manifest is null)
        {
            return null;
        }

        var aggregate =
            loaded.Aggregate;

        var manifest =
            aggregate.Manifest;

        var json =
            IiifSerializer.Serialize(
                manifest,
                new IiifSerializerOptions(
                    IiifPresentationVersion.V3_0));

        return new ManifestDetailsView(
            manifest.Id,
            IiifLabelFormatter.FirstOrDefault(
                manifest.Label),
            aggregate.SourceVersion,
            aggregate.Revision,
            aggregate.IsDeleted,
            loaded.StreamName,
            PrettyPrint(json),
            manifest.Items.OfType<Canvas>().Count(),
            loaded.Timeline.Count,
            revision.HasValue);
    }

    public async Task<IReadOnlyList<ManifestTimelineEventView>?> TimelineAsync(
        string manifestId,
        CancellationToken cancellationToken)
    {
        var loaded =
            await eventStore.LoadAsync(
                manifestId,
                maxRevision: null,
                cancellationToken);

        return loaded?.Timeline;
    }

    public async Task<string?> ExportAsync(
        string manifestId,
        ulong? revision,
        IiifPresentationVersion targetVersion,
        CancellationToken cancellationToken)
    {
        var loaded =
            await eventStore.LoadAsync(
                manifestId,
                revision,
                cancellationToken);

        if (loaded?.Aggregate.Manifest is null)
            return null;

        if (loaded.Aggregate.IsDeleted &&
            !revision.HasValue)
        {
            return null;
        }

        return IiifSerializer.Serialize(
            loaded.Aggregate.Manifest,
            new IiifSerializerOptions(
                targetVersion));
    }

    public async Task<OperationResult> MutateAsync(
        string manifestId,
        ulong expectedRevision,
        ManifestMutation mutation,
        string? value,
        CancellationToken cancellationToken)
    {
        var loaded =
            await eventStore.LoadAsync(
                manifestId,
                maxRevision: null,
                cancellationToken);

        if (loaded?.Aggregate.Manifest is null)
            return Failure("The Manifest stream does not exist.");

        var aggregate =
            loaded.Aggregate;

        if (aggregate.IsDeleted)
            return Failure("The Manifest has been deleted.");

        if (aggregate.Revision != expectedRevision)
        {
            return Conflict(
                $"The stream is now at revision {aggregate.Revision}; the page was based on revision {expectedRevision}.");
        }

        var manifest =
            aggregate.Manifest;

        IManifestDomainEvent domainEvent;

        switch (mutation)
        {
            case ManifestMutation.RenameManifest:
            {
                var newLabel =
                    string.IsNullOrWhiteSpace(value)
                        ? $"Event-sourced Manifest {DateTimeOffset.UtcNow:HH:mm:ss}"
                        : value.Trim();

                manifest.SetLabel(
                    [new Label(newLabel)]);

                domainEvent =
                    new ManifestLabelChangedV1(
                        newLabel,
                        DateTimeOffset.UtcNow);

                break;
            }

            case ManifestMutation.ToggleRights:
            {
                var current =
                    manifest.Rights?.Value;

                var next =
                    string.Equals(
                        current,
                        Rights.CcBy.Value,
                        StringComparison.OrdinalIgnoreCase)
                        ? Rights.CcBySa
                        : Rights.CcBy;

                manifest.SetRights(next);

                domainEvent =
                    new ManifestRightsChangedV1(
                        next.Value,
                        DateTimeOffset.UtcNow);

                break;
            }

            case ManifestMutation.IncreaseFirstCanvasHeight:
            {
                var canvas =
                    manifest.Items
                        .OfType<Canvas>()
                        .FirstOrDefault();

                if (canvas is null)
                    return Failure(
                        "The Manifest has no Canvas.");

                var previous =
                    canvas.Height;

                var next =
                    (previous ?? 0) + 100;

                canvas.SetHeight(next);

                domainEvent =
                    new CanvasHeightChangedV1(
                        canvas.Id,
                        previous,
                        next,
                        DateTimeOffset.UtcNow);

                break;
            }

            case ManifestMutation.AddCanvas:
            {
                var number =
                    manifest.Items
                        .OfType<Canvas>()
                        .Count() + 1;

                var canvasId =
                    $"{manifest.Id.TrimEnd('/')}/canvas/event-{Guid.NewGuid():N}";

                var label =
                    $"Event Canvas {number}";

                const int height = 1200;
                const int width = 900;

                manifest.AddItem(
                    new Canvas(
                        canvasId,
                        new Label(label),
                        height,
                        width));

                domainEvent =
                    new CanvasAddedV1(
                        canvasId,
                        label,
                        height,
                        width,
                        DateTimeOffset.UtcNow);

                break;
            }

            case ManifestMutation.RemoveLastCanvas:
            {
                var canvas =
                    manifest.Items
                        .OfType<Canvas>()
                        .LastOrDefault();

                if (canvas is null)
                    return Failure(
                        "The Manifest has no Canvas.");

                var removedJson =
                    JsonConvert.SerializeObject(
                        canvas,
                        IiifJsonSettings);

                manifest.RemoveItem(canvas);

                domainEvent =
                    new CanvasRemovedV1(
                        canvas.Id,
                        removedJson,
                        DateTimeOffset.UtcNow);

                break;
            }

            default:
                return Failure(
                    "Unsupported mutation.");
        }

        if (!manifest.HasChanges)
        {
            return Failure(
                "The command did not produce a tracked SDK change.");
        }

        var changeSet =
            manifest.GetChangeSet();

        var audit =
            SdkChangeAuditFactory.From(
                changeSet);

        try
        {
            await eventStore.AppendAsync(
                manifestId,
                expectedRevision,
                domainEvent,
                audit,
                cancellationToken);
        }
        catch (Exception ex) when (
            IsConcurrencyException(ex))
        {
            return Conflict(
                "Another request appended to this Manifest stream first. Reload and retry.");
        }

        return Success(
            manifestId,
            expectedRevision + 1);
    }

    public async Task<OperationResult> DeleteAsync(
        string manifestId,
        ulong expectedRevision,
        CancellationToken cancellationToken)
    {
        var loaded =
            await eventStore.LoadAsync(
                manifestId,
                maxRevision: null,
                cancellationToken);

        if (loaded is null)
            return Failure(
                "The Manifest stream does not exist.");

        if (loaded.Aggregate.IsDeleted)
            return Failure(
                "The Manifest is already deleted.");

        if (loaded.Aggregate.Revision != expectedRevision)
        {
            return Conflict(
                $"The stream is now at revision {loaded.Aggregate.Revision}.");
        }

        var @event =
            new ManifestDeletedV1(
                DateTimeOffset.UtcNow);

        try
        {
            await eventStore.AppendAsync(
                manifestId,
                expectedRevision,
                @event,
                audit: null,
                cancellationToken);
        }
        catch (Exception ex) when (
            IsConcurrencyException(ex))
        {
            return Conflict(
                "Another request appended to this Manifest stream first.");
        }

        return Success(
            manifestId,
            expectedRevision + 1);
    }

    private static bool IsConcurrencyException(
        Exception exception) =>
        exception is WrongExpectedVersionException;

    private static string PrettyPrint(
        string json)
    {
        using var document =
            System.Text.Json.JsonDocument.Parse(json);

        return System.Text.Json.JsonSerializer.Serialize(
            document.RootElement,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
    }

    private static OperationResult Success(
        string manifestId,
        ulong revision) =>
        new()
        {
            Succeeded = true,
            ManifestId = manifestId,
            Revision = revision
        };

    private static OperationResult Failure(
        string error) =>
        new()
        {
            Error = error
        };

    private static OperationResult Conflict(
        string error) =>
        new()
        {
            Error = error,
            ConcurrencyConflict = true
        };
}
