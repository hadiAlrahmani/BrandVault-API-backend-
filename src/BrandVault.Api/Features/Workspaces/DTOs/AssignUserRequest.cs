namespace BrandVault.Api.Features.Workspaces.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Shape of the POST /api/workspaces/:id/assignments request body.
/// Assigns a user (by ID) to the workspace.
/// </summary>
public class AssignUserRequest
{
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }
}
