using IIIF.POC.EventSourcedManifestStore.Models;
using IIIF.POC.EventSourcedManifestStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.EventSourcedManifestStore.Pages.Manifests;

public sealed class DeleteModel(
    ManifestApplicationService manifests) : PageModel
{
    public ManifestDetailsView Manifest { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(
        string manifestId,
        CancellationToken cancellationToken)
    {
        var details =
            await manifests.GetAsync(
                manifestId,
                revision: null,
                cancellationToken);

        if (details is null)
            return NotFound();

        Manifest = details;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string manifestId,
        ulong expectedRevision,
        CancellationToken cancellationToken)
    {
        var result =
            await manifests.DeleteAsync(
                manifestId,
                expectedRevision,
                cancellationToken);

        TempData["StatusMessage"] =
            result.Succeeded
                ? $"Deletion event appended at revision {result.Revision}. The stream history remains intact."
                : result.Error ?? "The deletion event could not be appended.";

        return RedirectToPage(
            "Details",
            new
            {
                manifestId
            });
    }
}
