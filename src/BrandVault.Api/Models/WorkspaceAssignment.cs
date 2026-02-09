namespace BrandVault.Api.Models;

using BrandVault.Api.Common;

/// <summary>
/// Junction table that tracks which designers are assigned to which workspaces.
/// This is a many-to-many relationship: a workspace can have multiple designers,
/// and a designer can be assigned to multiple workspaces.
///
/// In Prisma, you might model this implicitly with @relation, but explicit
/// junction tables give you more control (like adding an "assigned at" timestamp later).
/// </summary>
public class WorkspaceAssignment : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
