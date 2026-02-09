namespace BrandVault.Api.Features.Workspaces.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Shape of the POST /api/workspaces request body.
///
/// Express/Zod equivalent:
///   const createWorkspaceSchema = z.object({
///     name: z.string().min(1).max(200),
///     description: z.string().max(2000).optional(),
///     deadline: z.string().datetime().optional(),
///     clientId: z.string().uuid(),
///   });
/// </summary>
public class CreateWorkspaceRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? Deadline { get; set; }

    [Required(ErrorMessage = "Client ID is required")]
    public Guid ClientId { get; set; }
}
