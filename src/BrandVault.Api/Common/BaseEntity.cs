namespace BrandVault.Api.Common;

/// <summary>
/// Base class for all database entities — provides Id and CreatedAt.
/// Think of this like a TypeScript interface that every model extends:
///   interface BaseEntity { id: string; createdAt: Date; }
///
/// In .NET, we use an abstract class instead of an interface because
/// we want to provide the actual properties, not just define their shape.
/// "abstract" means you can't do "new BaseEntity()" — you must inherit from it.
/// </summary>
public abstract class BaseEntity
{
    // Guid is .NET's UUID type — a globally unique 128-bit identifier.
    // In Node, you'd use uuid.v4(). Same concept, just a built-in type in C#.
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }
}
