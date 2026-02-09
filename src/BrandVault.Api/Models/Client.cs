namespace BrandVault.Api.Models;

using BrandVault.Api.Common;

/// <summary>
/// A client is a brand/company the agency works with.
/// Each client can have multiple workspaces (projects).
///
/// "CreatedBy" tracks which agency user created this client record.
/// The "?" on Phone and Industry means they're nullable (optional),
/// just like TypeScript's "phone?: string".
/// </summary>
public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Industry { get; set; }

    // Foreign key + navigation property
    // In Prisma: createdBy User @relation(fields: [createdById], references: [id])
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    // One client has many workspaces
    public ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
}
