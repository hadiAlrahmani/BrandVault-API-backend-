namespace BrandVault.Api.Features.Auth;

using BrandVault.Api.Features.Auth.DTOs;

/// <summary>
/// Contract for the auth service — defines what operations are available.
///
/// TypeScript equivalent:
///   interface IAuthService {
///     register(dto: RegisterRequest): Promise&lt;AuthResponse&gt;;
///     login(dto: LoginRequest): Promise&lt;AuthResponse&gt;;
///     refreshToken(dto: RefreshTokenRequest): Promise&lt;AuthResponse&gt;;
///   }
///
/// Why an interface? .NET's DI container needs it. You tell it:
///   "When someone asks for IAuthService, give them AuthService."
/// This decouples the controller from the implementation — the controller
/// only knows about the interface, not the concrete class. Makes testing
/// easy (swap in a mock) and follows the Dependency Inversion principle.
///
/// Task&lt;T&gt; is .NET's Promise&lt;T&gt;. The Async suffix is a naming convention.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<UserInfo> GetCurrentUserAsync(Guid userId);
    Task LogoutAsync(Guid userId);
}
