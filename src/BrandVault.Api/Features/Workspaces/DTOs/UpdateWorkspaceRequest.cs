namespace BrandVault.Api.Features.Workspaces.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Shape of the PUT /api/workspaces/:id request body.
/// Status is a string that must match a valid WorkspaceStatus enum value
/// (Active, InReview, Completed, Archived).
/// </summary>
public class UpdateWorkspaceRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? Deadline { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;
}
