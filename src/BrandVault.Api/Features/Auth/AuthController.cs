namespace BrandVault.Api.Features.Auth;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrandVault.Api.Features.Auth.DTOs;

/// <summary>
/// HTTP endpoints for authentication.
///
/// Express equivalent — this is your auth router:
///   const router = express.Router();
///   router.post('/register', authController.register);
///   router.post('/login', authController.login);
///   router.post('/refresh-token', authController.refreshToken);
///   app.use('/api/auth', router);
///
/// Key .NET concepts:
/// - [ApiController] = enables automatic request body validation (like Zod middleware)
/// - [Route("api/auth")] = base path for all endpoints in this controller
/// - [HttpPost("register")] = POST /api/auth/register
/// - ControllerBase provides Ok() (200), Created() (201), etc. — like res.status(200).json()
/// - No try/catch here — ExceptionHandlingMiddleware catches AuthExceptions globally
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // Constructor injection — DI container provides IAuthService automatically.
    // Express equivalent: you'd import authService in your route file.
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// POST /api/auth/register
    /// Creates a new user account. First user becomes Admin, rest are Designers.
    ///
    /// [FromBody] = parse JSON request body into RegisterRequest object.
    /// In Express with express.json(), req.body is auto-parsed. In .NET, [FromBody] does the same.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        return Created("", response);  // 201 Created
    }

    /// <summary>
    /// POST /api/auth/login
    /// Authenticates with email/password, returns JWT + refresh token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);  // 200 OK
    }

    /// <summary>
    /// POST /api/auth/refresh-token
    /// Exchanges a valid refresh token for a new JWT + new refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        return Ok(response);  // 200 OK
    }

    /// <summary>
    /// GET /api/auth/me
    /// Returns the current user's info from their JWT token.
    /// Protected — requires a valid Bearer token.
    ///
    /// Express equivalent:
    ///   router.get('/me', requireAuth, (req, res) => res.json(req.user));
    ///
    /// [Authorize] = .NET's requireAuth middleware. If no valid JWT, returns 401 automatically.
    /// ClaimTypes.NameIdentifier = the "sub" claim we put in the JWT during login.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userInfo = await _authService.GetCurrentUserAsync(userId);
        return Ok(userInfo);
    }

    /// <summary>
    /// POST /api/auth/logout
    /// Clears the refresh token server-side. The access token (JWT) is stateless
    /// and will still be valid until it expires — client should discard it.
    ///
    /// Express equivalent:
    ///   router.post('/logout', requireAuth, async (req, res) => {
    ///     await prisma.user.update({ where: { id: req.user.id }, data: { refreshToken: null } });
    ///     res.sendStatus(204);
    ///   });
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.LogoutAsync(userId);
        return NoContent();  // 204 No Content
    }
}
