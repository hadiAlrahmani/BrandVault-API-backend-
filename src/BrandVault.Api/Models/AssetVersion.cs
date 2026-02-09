namespace BrandVault.Api.Models;

using BrandVault.Api.Common;

/// <summary>
/// Stores each version of an asset file. When a designer uploads a new version,
/// the previous versions stay in the database — nothing gets overwritten.
///
/// FilePath points to the stored file on disk (or cloud storage later).
/// ThumbnailPath points to the auto-generated thumbnail (for images).
/// FileSize is in bytes.
///
/// Note: "UploadedAt" uses BaseEntity.CreatedAt — no need for a separate field.
/// </summary>
public class AssetVersion : BaseEntity
{
    public int VersionNumber { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public long FileSize { get; set; }

    // Belongs to an asset
    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    // Who uploaded this version
    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;
}
