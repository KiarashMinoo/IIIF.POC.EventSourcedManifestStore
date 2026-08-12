using IIIF.POC.EventSourcedManifestStore.Models;
using IIIF.POC.EventSourcedManifestStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.EventSourcedManifestStore.Pages.Manifests;

public sealed class CreateModel(
    ManifestApplicationService manifests) : PageModel
{
    [BindProperty]
    public ManifestImportInput Input { get; set; } = new();

    public void OnGet()
    {
        Input.Json = SampleManifest;
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        var result =
            await manifests.ImportAsync(
                Input.Json,
                cancellationToken);

        if (!result.Succeeded ||
            string.IsNullOrWhiteSpace(result.ManifestId))
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error
                ?? "The Manifest could not be imported.");

            return Page();
        }

        TempData["StatusMessage"] =
            "Manifest imported as stream revision 0.";

        return RedirectToPage(
            "Details",
            new
            {
                manifestId = result.ManifestId
            });
    }

    private const string SampleManifest = """
{
  "@context": "http://iiif.io/api/presentation/3/context.json",
  "id": "https://example.org/iiif/event-sourced-demo/manifest",
  "type": "Manifest",
  "label": {
    "en": [
      "Event-Sourced IIIF Demo"
    ]
  },
  "rights": "http://creativecommons.org/licenses/by/4.0/",
  "summary": {
    "en": [
      "A Manifest reconstructed entirely from a KurrentDB event stream."
    ]
  },
  "items": [
    {
      "id": "https://example.org/iiif/event-sourced-demo/canvas/1",
      "type": "Canvas",
      "label": {
        "en": [
          "Page 1"
        ]
      },
      "height": 1200,
      "width": 900,
      "items": []
    },
    {
      "id": "https://example.org/iiif/event-sourced-demo/canvas/2",
      "type": "Canvas",
      "label": {
        "en": [
          "Page 2"
        ]
      },
      "height": 1200,
      "width": 900,
      "items": []
    }
  ]
}
""";
}
