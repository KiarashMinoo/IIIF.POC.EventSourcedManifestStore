using IIIF.POC.EventSourcedManifestStore.Models;
using IIIF.POC.EventSourcedManifestStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.EventSourcedManifestStore.Pages;

public sealed class IndexModel(
    ManifestApplicationService manifests) : PageModel
{
    [BindProperty]
    public OpenManifestInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        var details =
            await manifests.GetAsync(
                Input.ManifestId.Trim(),
                revision: null,
                cancellationToken);

        if (details is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "No event stream exists for this IIIF Manifest id.");

            return Page();
        }

        return RedirectToPage(
            "/Manifests/Details",
            new
            {
                manifestId = details.ManifestId
            });
    }
}
