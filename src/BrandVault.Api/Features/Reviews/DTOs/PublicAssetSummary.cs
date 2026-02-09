namespace BrandVault.Api.Features.Reviews.DTOs;

/// <summary>
/// Lightweight asset info returned in the public review response.
/// Doesn't include version details — use the asset detail endpoint for that.
/// </summary>
public class PublicAssetSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int CurrentVersion { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
