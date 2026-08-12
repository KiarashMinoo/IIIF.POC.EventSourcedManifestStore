namespace IIIF.POC.EventSourcedManifestStore.Models;

public enum ManifestMutation
{
    RenameManifest,
    ToggleRights,
    IncreaseFirstCanvasHeight,
    AddCanvas,
    RemoveLastCanvas
}