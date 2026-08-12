namespace IIIF.POC.EventSourcedManifestStore.Models;

public sealed class OperationResult
{
    public bool Succeeded { get; init; }

    public bool ConcurrencyConflict { get; init; }

    public string? Error { get; init; }

    public string? ManifestId { get; init; }

    public ulong? Revision { get; init; }
}