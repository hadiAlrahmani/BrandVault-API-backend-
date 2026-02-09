using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BrandVault.Api.Data;
using BrandVault.Api.Features.Auth;
using BrandVault.Api.Features.Clients;
using BrandVault.Api.Features.Workspaces;
using BrandVault.Api.Features.Assets;
using BrandVault.Api.Features.Reviews;
using BrandVault.Api.Features.Dashboard;
using BrandVault.Api.Services.FileStorage;
using BrandVault.Api.Hubs;
using BrandVault.Api.Middleware;

// Tell Npgsql to accept DateTime with any Kind (Local, Unspecified, Utc)
// instead of throwing when Kind != Utc for 'timestamp with time zone' columns.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// SERVICE REGISTRATION
// =============================================================================
// In Express, you'd import and configure middleware/modules at the top of app.ts.
// In .NET, you register "services" into a dependency injection (DI) container.
// The DI container then automatically provides these services to any class
// that asks for them in its constructor — no manual wiring needed.
// =============================================================================

// Register the DbContext with PostgreSQL.
// This is like: const prisma = new PrismaClient({ datasources: { db: { url: process.env.DATABASE_URL } } })
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register controllers — tells ASP.NET to scan the assembly for classes
// decorated with [ApiController] and wire them up as HTTP endpoints.
builder.Services.AddControllers();

// Swagger/OpenAPI — auto-generates interactive API documentation.
builder.Services.AddOpenApi();

// CORS — same concept as the `cors` npm package.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Needed for SignalR WebSocket connections
    });
});

// =============================================================================
// JWT AUTHENTICATION
// =============================================================================
// Express equivalent (using passport-jwt):
//   passport.use(new JwtStrategy({
//     secretOrKey: process.env.JWT_SECRET,
//     jwtFromRequest: ExtractJwt.fromAuthHeaderAsBearerToken(),
//     issuer: 'BrandVault',
//   }, (payload, done) => done(null, payload)));
//
// In .NET, AddAuthentication + AddJwtBearer configures all of that.
// It tells ASP.NET: "When a request has an Authorization: Bearer <token> header,
// validate the token with this secret key, issuer, and audience."
// =============================================================================
builder.Services.AddAuthentication(options =>
{
    // Set JWT as the default auth scheme (like Passport's default strategy)
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        ValidateLifetime = true,

        // No clock skew — tokens expire exactly when they should
        ClockSkew = TimeSpan.Zero
    };

    // SignalR sends the JWT via query string (?access_token=...) instead of
    // the Authorization header, because WebSockets can't set custom headers.
    // This is like Socket.IO's auth: { token } option in the handshake.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// =============================================================================
// FEATURE SERVICES (Dependency Injection)
// =============================================================================
// AddScoped = one instance per HTTP request.
// Express equivalent: container.register<IAuthService>(AuthService)
// =============================================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();

// FileStorage settings + service
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorage"));
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Assets feature
builder.Services.AddScoped<IAssetService, AssetService>();

// Reviews feature
builder.Services.AddScoped<IReviewService, ReviewService>();

// Dashboard feature
builder.Services.AddScoped<IDashboardService, DashboardService>();

// SignalR — real-time WebSocket communication, like Socket.IO.
// AddSignalR() registers the hub infrastructure into the DI container.
builder.Services.AddSignalR();

var app = builder.Build();

// =============================================================================
// MIDDLEWARE PIPELINE
// =============================================================================
// Just like Express, ORDER MATTERS. Each middleware runs in sequence.
// Requests flow top-to-bottom, responses flow bottom-to-top.
// =============================================================================

// Global exception handler — FIRST, so it catches errors from all middleware.
// Express equivalent: app.use((err, req, res, next) => { ... }) but placed first.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

// Authentication BEFORE Authorization — order matters.
// UseAuthentication reads JWT from Authorization header and sets HttpContext.User.
// UseAuthorization checks [Authorize] attributes on controllers/actions.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR hub endpoint — like: io.listen(server) + io.of('/hubs/reviews')
app.MapHub<ReviewHub>("/hubs/reviews");

app.Run();
