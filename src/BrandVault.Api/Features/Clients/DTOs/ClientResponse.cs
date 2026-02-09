namespace BrandVault.Api.Features.Clients.DTOs;

/// <summary>
/// JSON response for client endpoints.
///
/// Express equivalent — the shape you'd return:
///   res.json({
///     id: "uuid",
///     name: "John Doe",
///     company: "Acme Corp",
///     email: "john@acme.com",
///     phone: "+1234567890",
///     industry: "Technology",
///     createdById: "uuid",
///     createdByName: "Admin User",
///     createdAt: "2026-02-09T..."
///   });
///
/// We include createdByName so the frontend can display who added the client
/// without a separate API call to fetch the user.
/// </summary>
public class ClientResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Industry { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
