namespace BrandVault.Api.Features.Assets.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Multipart form for POST /api/assets/:id/versions.
/// Only needs the file — version number is auto-incremented.
/// </summary>
public class CreateVersionRequest
{
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = null!;
}
