namespace BrandVault.Api.Common;

/// <summary>
/// Reusable exception for all API features — carries an HTTP status code.
///
/// Express equivalent:
///   class AppError extends Error {
///     constructor(public statusCode: number, message: string) { super(message); }
///   }
///   throw new AppError(404, "Client not found");
///
/// Thrown in any service, caught by ExceptionHandlingMiddleware, which returns
/// the correct HTTP status with a { "error": "message" } JSON body.
/// </summary>
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
