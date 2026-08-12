using IIIF.Manifests.Serializer;
using IIIF.Manifests.Serializer.Nodes;
using IIIF.Manifests.Serializer.Properties;
using IIIF.POC.EventSourcedManifestStore.Domain.Events;

namespace IIIF.POC.EventSourcedManifestStore.Domain;

public sealed class ManifestAggregate
{
    public Manifest? Manifest { get; private set; }

    public ulong Revision { get; private set; }

    public bool Exists { get; private set; }

    public bool IsDeleted { get; private set; }

    public string SourceVersion { get; private set; } = "";

    public void Apply(
        IManifestDomainEvent @event,
        ulong revision)
    {
        switch (@event)
        {
            case ManifestImportedV1 imported:
                Manifest =
                    IiifSerializer.DeserializeManifest(
                        imported.CanonicalPresentation3Json);

                Manifest.AcceptChanges();
                SourceVersion = imported.SourceVersion;
                Exists = true;
                IsDeleted = false;
                break;

            case ManifestLabelChangedV1 labelChanged:
                RequireManifest();
                Manifest!.SetLabel(
                    [new Label(labelChanged.Label)]);
                Manifest.AcceptChanges();
                break;

            case ManifestRightsChangedV1 rightsChanged:
                RequireManifest();
                Manifest!.SetRights(
                    new Rights(rightsChanged.Rights));
                Manifest.AcceptChanges();
                break;

            case CanvasHeightChangedV1 heightChanged:
            {
                RequireManifest();

                var canvas =
                    Manifest!.Items
                        .OfType<Canvas>()
                        .SingleOrDefault(
                            x => x.Id == heightChanged.CanvasId)
                    ?? throw new InvalidOperationException(
                        $"Canvas '{heightChanged.CanvasId}' is missing while replaying the stream.");

                canvas.SetHeight(heightChanged.Height);
                Manifest.AcceptChanges();
                break;
            }

            case CanvasAddedV1 canvasAdded:
                RequireManifest();

                Manifest!.AddItem(
                    new Canvas(
                        canvasAdded.CanvasId,
                        new Label(canvasAdded.Label),
                        canvasAdded.Height,
                        canvasAdded.Width));

                Manifest.AcceptChanges();
                break;

            case CanvasRemovedV1 canvasRemoved:
            {
                RequireManifest();

                var canvas =
                    Manifest!.Items
                        .OfType<Canvas>()
                        .SingleOrDefault(
                            x => x.Id == canvasRemoved.CanvasId)
                    ?? throw new InvalidOperationException(
                        $"Canvas '{canvasRemoved.CanvasId}' is missing while replaying the stream.");

                Manifest.RemoveItem(canvas);
                Manifest.AcceptChanges();
                break;
            }

            case ManifestDeletedV1:
                RequireManifest();
                IsDeleted = true;
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported event type '{@event.GetType().Name}'.");
        }

        Revision = revision;
    }

    private void RequireManifest()
    {
        if (!Exists || Manifest is null)
            throw new InvalidOperationException(
                "The event stream does not contain a Manifest import event.");

        if (IsDeleted)
            throw new InvalidOperationException(
                "The Manifest was deleted and cannot accept further state-changing events.");
    }
}
