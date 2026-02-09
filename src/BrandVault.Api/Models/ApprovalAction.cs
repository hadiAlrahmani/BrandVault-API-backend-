namespace BrandVault.Api.Models;

using BrandVault.Api.Common;
using BrandVault.Api.Models.Enums;

/// <summary>
/// Records when someone approves an asset or requests a revision.
/// Similar to Comment — can come from agency users or external clients.
/// The Comment field here is optional and holds the reason for a revision request.
/// </summary>
public class ApprovalAction : BaseEntity
{
    public ApprovalActionType ActionType { get; set; }
    public string? Comment { get; set; }
    public string DoneByName { get; set; } = string.Empty;
    public AuthorType DoneByType { get; set; }

    // Which asset this action is on
    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    // Nullable: only set for client actions made via a review link
    public Guid? ReviewLinkId { get; set; }
    public ReviewLink? ReviewLink { get; set; }
}
