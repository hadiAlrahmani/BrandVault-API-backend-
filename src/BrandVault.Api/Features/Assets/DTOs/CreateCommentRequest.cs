namespace BrandVault.Api.Features.Assets.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// JSON body for POST /api/assets/:id/comments (agency users).
/// AuthorName is auto-set from the JWT user — not in the request body.
/// </summary>
public class CreateCommentRequest
{
    [Required(ErrorMessage = "Content is required")]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}
