using IIIF.Manifests.Serializer.Properties;

namespace IIIF.POC.EventSourcedManifestStore.Services;

internal static class IiifLabelFormatter
{
    public static string FirstOrDefault(
        IEnumerable<Label> labels) =>
        labels
            .Select(x => x.Value)
            .FirstOrDefault(
                x => !string.IsNullOrWhiteSpace(x))
        ?? "(untitled)";
}
