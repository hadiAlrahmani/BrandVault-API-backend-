namespace BrandVault.Api.Features.Reviews.DTOs;

/// <summary>
/// Response for review link management endpoints.
/// Includes the token which forms the shareable URL.
/// </summary>
public class ReviewLinkResponse
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
