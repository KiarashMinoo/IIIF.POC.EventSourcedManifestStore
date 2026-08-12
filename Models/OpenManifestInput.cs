using System.ComponentModel.DataAnnotations;

namespace IIIF.POC.EventSourcedManifestStore.Models;

public sealed class OpenManifestInput
{
    [Required]
    [Display(Name = "IIIF Manifest id")]
    public string ManifestId { get; set; } = "";
}