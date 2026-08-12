using System.Text;
using IIIF.Manifests.Serializer;
using IIIF.POC.EventSourcedManifestStore.Models;
using IIIF.POC.EventSourcedManifestStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.EventSourcedManifestStore.Pages.Manifests;

public sealed class DetailsModel(
    ManifestApplicationService manifests) : PageModel
{
    public ManifestDetailsView Manifest { get; private set; } = default!;

    [BindProperty]
    public string? NewLabel { get; set; }

    public ulong? RequestedRevision { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        string manifestId,
        ulong? revision,
        CancellationToken cancellationToken)
    {
        RequestedRevision = revision;

        var details =
            await manifests.GetAsync(
                manifestId,
                revision,
                cancellationToken);

        if (details is null)
            return NotFound();

        Manifest = details;

        return Page();
    }

    public async Task<IActionResult> OnPostMutateAsync(
        string manifestId,
        ulong expectedRevision,
        ManifestMutation mutation,
        CancellationToken cancellationToken)
    {
        var result =
            await manifests.MutateAsync(
                manifestId,
                expectedRevision,
                mutation,
                NewLabel,
                cancellationToken);

        if (!result.Succeeded)
        {
            TempData["StatusMessage"] =
                result.Error
                ?? "The event could not be appended.";

            return RedirectToPage(
                new
                {
                    manifestId
                });
        }

        TempData["StatusMessage"] =
            $"Event appended. Stream is now revision {result.Revision}.";

        return RedirectToPage(
            new
            {
                manifestId
            });
    }

    public async Task<IActionResult> OnGetExportAsync(
        string manifestId,
        ulong? revision,
        string target,
        CancellationToken cancellationToken)
    {
        var version =
            target.ToLowerInvariant() switch
            {
                "v2.0" => IiifPresentationVersion.V2_0,
                "v2.1" => IiifPresentationVersion.V2_1,
                _ => IiifPresentationVersion.V3_0
            };

        var json =
            await manifests.ExportAsync(
                manifestId,
                revision,
                version,
                cancellationToken);

        if (json is null)
            return NotFound();

        var suffix =
            revision.HasValue
                ? $"-r{revision.Value}"
                : "-current";

        return File(
            Encoding.UTF8.GetBytes(json),
            "application/json",
            $"iiif-manifest{suffix}-{target}.json");
    }
}
