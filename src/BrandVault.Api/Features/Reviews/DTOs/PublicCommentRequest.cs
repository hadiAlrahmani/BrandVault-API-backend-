namespace BrandVault.Api.Features.Reviews.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// JSON body for POST /api/reviews/:token/assets/:assetId/comments.
/// External clients provide their name (no account required).
/// </summary>
public class PublicCommentRequest
{
    [Required(ErrorMessage = "Author name is required")]
    [MaxLength(200)]
    public string AuthorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}
