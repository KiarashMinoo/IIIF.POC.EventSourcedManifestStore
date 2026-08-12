using IIIF.POC.EventSourcedManifestStore.Models;
using IIIF.POC.EventSourcedManifestStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.EventSourcedManifestStore.Pages.Manifests;

public sealed class TimelineModel(
    ManifestApplicationService manifests) : PageModel
{
    public string ManifestId { get; private set; } = "";

    public IReadOnlyList<ManifestTimelineEventView> Events { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(
        string manifestId,
        CancellationToken cancellationToken)
    {
        var events =
            await manifests.TimelineAsync(
                manifestId,
                cancellationToken);

        if (events is null)
            return NotFound();

        ManifestId = manifestId;
        Events = events;

        return Page();
    }
}
