namespace BrandVault.Api.Features.Workspaces.DTOs;

/// <summary>
/// JSON response for workspace endpoints.
/// Includes client name and creator name so the frontend doesn't need extra API calls.
/// AssignmentCount tells you how many team members are assigned.
/// </summary>
public class WorkspaceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AssignmentCount { get; set; }
}
