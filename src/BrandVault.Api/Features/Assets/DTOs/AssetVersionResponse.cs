namespace BrandVault.Api.Features.Assets.DTOs;

/// <summary>
/// Response for each version of an asset.
/// UploadedAt maps to the version's CreatedAt (from BaseEntity).
/// </summary>
public class AssetVersionResponse
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public long FileSize { get; set; }
    public Guid UploadedById { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
