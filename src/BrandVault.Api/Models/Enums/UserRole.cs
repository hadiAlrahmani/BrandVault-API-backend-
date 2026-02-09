namespace BrandVault.Api.Models.Enums;

/// <summary>
/// In TypeScript you'd write: type UserRole = "Admin" | "Manager" | "Designer"
/// C# enums are similar but more structured — they're their own type,
/// and the compiler ensures you can only use valid values.
/// We'll configure EF Core to store these as strings in PostgreSQL.
/// </summary>
public enum UserRole
{
    Admin,
    Manager,
    Designer
}
