namespace BrandVault.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Shape of the POST /api/auth/register request body.
///
/// Express/Zod equivalent:
///   const registerSchema = z.object({
///     email: z.string().email(),
///     password: z.string().min(8),
///     name: z.string().min(1).max(200),
///   });
///
/// In .NET, the [Required], [EmailAddress], [MinLength] attributes are "data annotations."
/// ASP.NET validates these BEFORE your controller code runs. If validation fails,
/// it auto-returns a 400 with error details — you don't write that logic yourself.
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
