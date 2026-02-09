namespace BrandVault.Api.Features.Clients.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Shape of the POST /api/clients request body.
///
/// Express/Zod equivalent:
///   const createClientSchema = z.object({
///     name: z.string().min(1).max(200),
///     company: z.string().min(1).max(200),
///     email: z.string().email(),
///     phone: z.string().max(50).optional(),
///     industry: z.string().max(100).optional(),
///   });
/// </summary>
public class CreateClientRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Company is required")]
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }
}
