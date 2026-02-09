namespace BrandVault.Api.Features.Reviews.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// JSON body for POST /api/review-links.
/// Creates a tokenized link for external clients to review workspace assets.
///
/// Express equivalent:
///   body: { workspaceId: z.string().uuid(), expiresInDays: z.number().default(7) }
/// </summary>
public class CreateReviewLinkRequest
{
    [Required(ErrorMessage = "Workspace ID is required")]
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// Number of days until the link expires. Defaults to 7.
    /// </summary>
    [Range(1, 90, ErrorMessage = "ExpiresInDays must be between 1 and 90")]
    public int ExpiresInDays { get; set; } = 7;
}
