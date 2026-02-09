namespace BrandVault.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Shape of the POST /api/auth/refresh-token request body.
/// Client sends the refresh token they received from login/register.
/// </summary>
public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}
