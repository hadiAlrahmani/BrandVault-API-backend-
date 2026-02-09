namespace BrandVault.Api.Features.Reviews.DTOs;

/// <summary>
/// JSON body for PUT /api/review-links/:id.
/// Toggle active state or change expiration.
/// </summary>
public class UpdateReviewLinkRequest
{
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
