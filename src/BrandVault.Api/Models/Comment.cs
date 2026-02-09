namespace BrandVault.Api.Models;

using BrandVault.Api.Common;
using BrandVault.Api.Models.Enums;

/// <summary>
/// A comment on a specific asset. Can come from either:
/// - An agency user (AuthorType.Agency) — ReviewLinkId will be null
/// - An external client (AuthorType.Client) — ReviewLinkId links to the review link they used
///
/// AuthorName is stored directly (not a foreign key) because client reviewers
/// don't have user accounts — they just type their name when commenting.
/// </summary>
public class Comment : BaseEntity
{
    public string AuthorName { get; set; } = string.Empty;
    public AuthorType AuthorType { get; set; }
    public string Content { get; set; } = string.Empty;

    // Which asset this comment is on
    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    // Nullable: only set for client comments made via a review link
    public Guid? ReviewLinkId { get; set; }
    public ReviewLink? ReviewLink { get; set; }
}
