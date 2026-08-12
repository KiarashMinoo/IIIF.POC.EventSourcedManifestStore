using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.EventSourcedManifestStore.Pages;

[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public sealed class ErrorModel : PageModel
{
    public string? RequestId { get; private set; }

    public bool ShowRequestId =>
        !string.IsNullOrWhiteSpace(RequestId);

    public void OnGet()
    {
        RequestId =
            Activity.Current?.Id
            ?? HttpContext.TraceIdentifier;
    }
}
