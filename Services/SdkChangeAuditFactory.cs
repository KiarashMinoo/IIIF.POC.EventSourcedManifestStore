using IIIF.Manifests.Serializer;
using IIIF.Manifests.Serializer.ChangeTracking;
using IIIF.POC.EventSourcedManifestStore.Domain.Events;
using Newtonsoft.Json;

namespace IIIF.POC.EventSourcedManifestStore.Services;

internal static class SdkChangeAuditFactory
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new IIIFJsonContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore
    };

    public static SdkChangeSetAuditV1 From(
        IiifChangeSet changeSet)
    {
        var entries =
            changeSet.Changes
                .Select(
                    change =>
                        new SdkChangeAuditEntryV1(
                            change.Path,
                            change.Kind.ToString(),
                            change.PropertyName,
                            SerializeValue(change.OriginalValue),
                            SerializeValue(change.CurrentValue),
                            change.ChangedAtUtc))
                .ToList();

        return new SdkChangeSetAuditV1(
            Guid.NewGuid(),
            changeSet.CreatedAtUtc,
            entries);
    }

    public static string SerializeValue(
        object? value) =>
        value is null
            ? "null"
            : JsonConvert.SerializeObject(
                value,
                JsonSettings);
}
