using IIIF.POC.EventSourcedManifestStore.Domain;
using IIIF.POC.EventSourcedManifestStore.Models;

namespace IIIF.POC.EventSourcedManifestStore.Infrastructure;

public sealed record LoadedManifestStream(
    string StreamName,
    ManifestAggregate Aggregate,
    IReadOnlyList<ManifestTimelineEventView> Timeline);