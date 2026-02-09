namespace BrandVault.Api.Features.Clients.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Shape of the PUT /api/clients/:id request body.
/// Same fields as CreateClientRequest — full replacement, not partial patch.
/// </summary>
public class UpdateClientRequest
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
