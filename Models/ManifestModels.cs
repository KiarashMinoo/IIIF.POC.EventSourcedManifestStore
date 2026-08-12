using System.ComponentModel.DataAnnotations;
using IIIF.POC.EventSourcedManifestStore.Domain.Events;

namespace IIIF.POC.EventSourcedManifestStore.Models;

public sealed class ManifestImportInput
{
    [Required]
    [Display(Name = "Manifest JSON")]
    public string Json { get; set; } = "";
}

public sealed class OpenManifestInput
{
    [Required]
    [Display(Name = "IIIF Manifest id")]
    public string ManifestId { get; set; } = "";
}

public enum ManifestMutation
{
    RenameManifest,
    ToggleRights,
    IncreaseFirstCanvasHeight,
    AddCanvas,
    RemoveLastCanvas
}

public sealed record ManifestDetailsView(
    string ManifestId,
    string Label,
    string SourceVersion,
    ulong Revision,
    bool IsDeleted,
    string StreamName,
    string Presentation3Json,
    int CanvasCount,
    int EventCount,
    bool IsHistorical);

public sealed record ManifestTimelineEventView(
    ulong Revision,
    string EventId,
    string EventType,
    DateTimeOffset OccurredAtUtc,
    string RawJson,
    SdkChangeSetAuditV1? Audit);

public sealed class OperationResult
{
    public bool Succeeded { get; init; }

    public bool ConcurrencyConflict { get; init; }

    public string? Error { get; init; }

    public string? ManifestId { get; init; }

    public ulong? Revision { get; init; }
}
