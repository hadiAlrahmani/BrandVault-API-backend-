namespace BrandVault.Api.Features.Assets.DTOs;

/// <summary>
/// Standard response for asset list endpoints.
/// Includes denormalized names to avoid extra API calls on the frontend.
/// </summary>
public class AssetResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int CurrentVersion { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public Guid UploadedById { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
