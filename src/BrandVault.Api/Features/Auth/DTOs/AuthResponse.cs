namespace BrandVault.Api.Features.Auth.DTOs;

/// <summary>
/// JSON response returned from login, register, and refresh-token endpoints.
///
/// Express equivalent — the shape you'd return:
///   res.json({
///     accessToken: "eyJ...",
///     refreshToken: "a1b2c3...",
///     expiresAt: "2026-02-08T18:00:00Z",
///     user: { id: "...", email: "...", name: "...", role: "Admin" }
///   });
///
/// We define a class so the serializer knows exactly what to output.
/// Property names get camelCased automatically by ASP.NET's JSON serializer.
/// </summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = null!;
}

/// <summary>
/// Safe subset of User data for auth responses.
/// We NEVER send PasswordHash or RefreshToken to the client.
/// This is why we use DTOs instead of returning the User entity directly.
/// </summary>
public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
