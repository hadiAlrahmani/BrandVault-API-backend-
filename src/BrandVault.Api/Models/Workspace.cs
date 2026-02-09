namespace BrandVault.Api.Models;

using BrandVault.Api.Common;
using BrandVault.Api.Models.Enums;

/// <summary>
/// A workspace is a project within a client — like "Q1 Rebrand" or "Social Media Kit".
/// Contains assets, has team assignments, and can have review links for client feedback.
/// </summary>
public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public WorkspaceStatus Status { get; set; } = WorkspaceStatus.Active;

    // Belongs to a client
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    // Created by an agency user
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    // Reverse navigation
    public ICollection<WorkspaceAssignment> Assignments { get; set; } = new List<WorkspaceAssignment>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<ReviewLink> ReviewLinks { get; set; } = new List<ReviewLink>();
}
