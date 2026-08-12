namespace IIIF.POC.EventSourcedManifestStore.Models;

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