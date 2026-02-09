namespace BrandVault.Api.Features.Workspaces.DTOs;

/// <summary>
/// JSON response for workspace assignment endpoints.
/// Shows who is assigned and their role in the agency.
/// </summary>
public class AssignmentResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
