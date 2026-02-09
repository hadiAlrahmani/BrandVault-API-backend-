namespace BrandVault.Api.Models;

using BrandVault.Api.Common;
using BrandVault.Api.Models.Enums;

/// <summary>
/// Represents an agency team member (Admin, Manager, or Designer).
///
/// In Express/Prisma terms, this would be your User model:
///   model User {
///     id        String   @id @default(uuid())
///     email     String   @unique
///     password  String
///     name      String
///     role      UserRole
///     createdAt DateTime @default(now())
///     // ... relations
///   }
///
/// The "navigation properties" (ICollection fields) tell EF Core about
/// the relationships. They're like Prisma's relation fields — EF Core
/// uses them to generate JOINs when you call .Include().
/// </summary>
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    // Refresh token fields for JWT auth
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Navigation properties — EF Core uses these to build JOINs
    // (like Prisma's { include: { createdClients: true } })
    public ICollection<Client> CreatedClients { get; set; } = new List<Client>();
    public ICollection<Workspace> CreatedWorkspaces { get; set; } = new List<Workspace>();
    public ICollection<WorkspaceAssignment> WorkspaceAssignments { get; set; } = new List<WorkspaceAssignment>();
    public ICollection<Asset> UploadedAssets { get; set; } = new List<Asset>();
    public ICollection<AssetVersion> UploadedVersions { get; set; } = new List<AssetVersion>();
    public ICollection<ReviewLink> CreatedReviewLinks { get; set; } = new List<ReviewLink>();
}
