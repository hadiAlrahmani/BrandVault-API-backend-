namespace BrandVault.Api.Features.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BrandVault.Api.Common;
using BrandVault.Api.Data;
using BrandVault.Api.Features.Auth.DTOs;
using BrandVault.Api.Models;
using BrandVault.Api.Models.Enums;

/// <summary>
/// All authentication business logic: register, login, refresh tokens.
///
/// Express equivalent — this is your authService.ts:
///   class AuthService {
///     constructor(private prisma: PrismaClient) {}
///     async register(dto: RegisterDto) { ... }
///     async login(dto: LoginDto) { ... }
///   }
///
/// The big difference: in Express you'd import prisma and jwt directly.
/// In .NET, they're injected via the constructor by the DI container.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    // Constructor injection — .NET's DI container provides these automatically.
    // Express equivalent: constructor(private prisma: PrismaClient, private config: Config)
    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Register a new user. First user becomes Admin; all others default to Designer.
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        // Express: const existing = await prisma.user.findUnique({ where: { email } });
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email.ToLower());

        if (emailExists)
        {
            throw new ApiException("Email already registered", 409);
        }

        // First user is Admin (bootstraps the system), rest are Designers
        // Express: const userCount = await prisma.user.count();
        var isFirstUser = !await _context.Users.AnyAsync();

        // Hash the password using BCrypt
        // Express: const hash = await bcrypt.hash(password, 12);
        // BCrypt.Net uses 11 work factor by default (similar to saltRounds in Node)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = passwordHash,
            Name = request.Name,
            Role = isFirstUser ? UserRole.Admin : UserRole.Designer,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays"))
        };

        // Save to database
        // Express: await prisma.user.create({ data: { ... } });
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var (accessToken, expiresAt) = GenerateAccessToken(user);
        return BuildAuthResponse(user, accessToken, expiresAt);
    }

    /// <summary>
    /// Authenticate with email and password. Returns JWT + refresh token.
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        // Express: const user = await prisma.user.findUnique({ where: { email } });
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (user is null)
        {
            // Intentionally vague — don't reveal whether email exists
            throw new ApiException("Invalid email or password", 401);
        }

        // Verify password
        // Express: const valid = await bcrypt.compare(password, user.passwordHash);
        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            throw new ApiException("Invalid email or password", 401);
        }

        // Rotate refresh token on each login (security best practice)
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
            _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays"));

        await _context.SaveChangesAsync();

        var (accessToken, expiresAt) = GenerateAccessToken(user);
        return BuildAuthResponse(user, accessToken, expiresAt);
    }

    /// <summary>
    /// Issue a new access token using a valid refresh token.
    /// Also rotates the refresh token itself.
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if (user is null)
        {
            throw new ApiException("Invalid refresh token", 401);
        }

        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            // Token exists but expired — clear it and force re-login
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            await _context.SaveChangesAsync();

            throw new ApiException("Refresh token has expired, please log in again", 401);
        }

        // Rotate: issue a new refresh token, invalidate the old one
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
            _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays"));

        await _context.SaveChangesAsync();

        var (accessToken, expiresAt) = GenerateAccessToken(user);
        return BuildAuthResponse(user, accessToken, expiresAt);
    }

    /// <summary>
    /// Get the current user's info by their ID (extracted from JWT claims).
    /// Express equivalent: const user = await prisma.user.findUnique({ where: { id }, select: { id, email, name, role } });
    /// </summary>
    public async Task<UserInfo> GetCurrentUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
        {
            throw new ApiException("User not found", 404);
        }

        return new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString()
        };
    }

    /// <summary>
    /// Logout — clear the refresh token so it can't be reused.
    /// The access token still works until it expires (stateless JWT), but
    /// the client should discard it. Server-side, we just kill the refresh token.
    /// </summary>
    public async Task LogoutAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
        {
            throw new ApiException("User not found", 404);
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await _context.SaveChangesAsync();
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    /// <summary>
    /// Generate a JWT access token with user claims.
    ///
    /// Express equivalent using jsonwebtoken:
    ///   const token = jwt.sign(
    ///     { userId: user.id, email: user.email, role: user.role },
    ///     process.env.JWT_SECRET,
    ///     { expiresIn: '60m', issuer: 'BrandVault' }
    ///   );
    ///
    /// More verbose in .NET, but same concepts: claims (payload), signing key, expiration.
    /// </summary>
    private (string token, DateTime expiresAt) GenerateAccessToken(User user)
    {
        // Claims = JWT payload data. Same as what you'd put in jwt.sign({...}).
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),  // "sub" claim
            new(ClaimTypes.Email, user.Email),                    // "email" claim
            new(ClaimTypes.Name, user.Name),                      // "name" claim
            new(ClaimTypes.Role, user.Role.ToString())            // "role" claim — [Authorize(Roles)] checks this
        };

        // Read secret key from config (like process.env.JWT_SECRET)
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes");
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresAt);
    }

    /// <summary>
    /// Generate a cryptographically random refresh token.
    ///
    /// Express equivalent:
    ///   const refreshToken = crypto.randomBytes(64).toString('base64url');
    ///
    /// NOT a JWT — just a random string stored in the database.
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Map User entity to AuthResponse DTO — ensures PasswordHash never leaks.
    /// </summary>
    private static AuthResponse BuildAuthResponse(User user, string accessToken, DateTime expiresAt)
    {
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = user.RefreshToken!,
            ExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString()
            }
        };
    }
}
