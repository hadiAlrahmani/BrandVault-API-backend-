namespace BrandVault.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Shape of the POST /api/auth/login request body.
///
/// Express/Zod equivalent:
///   const loginSchema = z.object({
///     email: z.string().email(),
///     password: z.string().min(1),
///   });
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
