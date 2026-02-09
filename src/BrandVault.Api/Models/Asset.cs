namespace BrandVault.Api.Models;

using BrandVault.Api.Common;
using BrandVault.Api.Models.Enums;

/// <summary>
/// An asset is a brand file (logo, guideline, mockup, font, etc.) inside a workspace.
/// Each asset tracks its current version number and approval status.
/// The actual files are stored in AssetVersion records — when someone uploads
/// a new version, CurrentVersion increments and a new AssetVersion is created,
/// but the old versions remain accessible.
/// </summary>
public class Asset : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int CurrentVersion { get; set; } = 1;
    public AssetStatus Status { get; set; } = AssetStatus.Draft;

    // Belongs to a workspace
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    // Who initially uploaded this asset
    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;

    // Reverse navigation
    public ICollection<AssetVersion> Versions { get; set; } = new List<AssetVersion>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<ApprovalAction> ApprovalActions { get; set; } = new List<ApprovalAction>();
}
