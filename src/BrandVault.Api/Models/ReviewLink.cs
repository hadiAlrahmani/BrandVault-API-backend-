namespace BrandVault.Api.Models;

using BrandVault.Api.Common;

/// <summary>
/// A secure, tokenized link that lets external clients view a workspace
/// and leave feedback — without needing an account.
///
/// Token is a cryptographically random string used in the URL.
/// ExpiresAt controls when the link stops working.
/// IsActive lets the agency manually deactivate a link.
///
/// When regenerating an expired link, we create a new ReviewLink
/// (with a new token) but keep the old one's comments intact.
/// </summary>
public class ReviewLink : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Belongs to a workspace
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    // Created by an agency user
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    // Comments and approvals made through this link
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<ApprovalAction> ApprovalActions { get; set; } = new List<ApprovalAction>();
}
