namespace BrandVault.Api.Features.Assets.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Multipart form data for POST /api/assets.
/// Accepts a file upload plus metadata fields.
///
/// Express/Multer equivalent:
///   upload.single('file'),
///   body: { name: z.string().max(500), workspaceId: z.string().uuid() }
/// </summary>
public class CreateAssetRequest
{
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = null!;

    [Required(ErrorMessage = "Name is required")]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workspace ID is required")]
    public Guid WorkspaceId { get; set; }
}
