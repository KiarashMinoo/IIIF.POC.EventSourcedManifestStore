namespace IIIF.POC.EventSourcedManifestStore.Domain.Events;

public static class ManifestEventTypes
{
    public const string Imported = "iiif.manifest.imported.v1";
    public const string LabelChanged = "iiif.manifest.label-changed.v1";
    public const string RightsChanged = "iiif.manifest.rights-changed.v1";
    public const string CanvasHeightChanged = "iiif.canvas.height-changed.v1";
    public const string CanvasAdded = "iiif.canvas.added.v1";
    public const string CanvasRemoved = "iiif.canvas.removed.v1";
    public const string Deleted = "iiif.manifest.deleted.v1";

    public static string For(IManifestDomainEvent @event) =>
        @event switch
        {
            ManifestImportedV1 => Imported,
            ManifestLabelChangedV1 => LabelChanged,
            ManifestRightsChangedV1 => RightsChanged,
            CanvasHeightChangedV1 => CanvasHeightChanged,
            CanvasAddedV1 => CanvasAdded,
            CanvasRemovedV1 => CanvasRemoved,
            ManifestDeletedV1 => Deleted,
            _ => throw new NotSupportedException(
                $"Unsupported event type '{@event.GetType().Name}'.")
        };
}
