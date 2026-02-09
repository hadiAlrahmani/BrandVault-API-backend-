namespace BrandVault.Api.Features.Reviews.DTOs;

/// <summary>
/// Response for GET /api/reviews/:token.
/// Shows the workspace info and all its assets for the external client reviewer.
/// </summary>
public class PublicReviewResponse
{
    public string WorkspaceName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<PublicAssetSummary> Assets { get; set; } = new();
}
