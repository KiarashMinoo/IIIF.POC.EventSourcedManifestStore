using System.ComponentModel.DataAnnotations;

namespace IIIF.POC.EventSourcedManifestStore.Models;

public sealed class ManifestImportInput
{
    [Required]
    [Display(Name = "Manifest JSON")]
    public string Json { get; set; } = "";
}