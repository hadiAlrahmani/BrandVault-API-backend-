namespace BrandVault.Api.Features.Assets.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// JSON body for PUT /api/assets/:id.
/// Updates metadata only — use the version endpoint for new files.
/// Status is a string validated against AssetStatus enum in the service.
/// </summary>
public class UpdateAssetRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;
}
