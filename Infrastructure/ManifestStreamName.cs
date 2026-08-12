using System.Security.Cryptography;
using System.Text;

namespace IIIF.POC.EventSourcedManifestStore.Infrastructure;

public static class ManifestStreamName
{
    public static string For(string manifestId)
    {
        var bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(manifestId));

        var hash =
            Convert.ToHexString(bytes)
                .ToLowerInvariant();

        return $"iiif-manifest-{hash}";
    }
}
