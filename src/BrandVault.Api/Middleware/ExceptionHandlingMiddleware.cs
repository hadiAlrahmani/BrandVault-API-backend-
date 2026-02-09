namespace BrandVault.Api.Middleware;

using System.Text.Json;
using BrandVault.Api.Common;

/// <summary>
/// Global exception handler — catches all unhandled exceptions and returns
/// consistent JSON error responses.
///
/// Express equivalent — the 4-argument error handler:
///   app.use((err, req, res, next) => {
///     if (err instanceof AppError) {
///       return res.status(err.statusCode).json({ error: err.message });
///     }
///     console.error(err);
///     res.status(500).json({ error: 'Internal server error' });
///   });
///
/// In .NET, middleware is a class with InvokeAsync(). The try/catch around
/// _next(context) intercepts errors from all downstream middleware and controllers.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // RequestDelegate _next = "call the next middleware in the pipeline."
    // Same concept as calling next() in Express middleware.
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pass the request through the rest of the pipeline.
            // If anything throws, we catch it below.
            await _next(context);
        }
        catch (ApiException ex)
        {
            // Known API error — return the specific status code
            _logger.LogWarning("API error: {Message}", ex.Message);
            await WriteErrorResponse(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            // Unknown error — log full details, return generic 500
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponse(context, 500, "An unexpected error occurred");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new { error = message };
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
