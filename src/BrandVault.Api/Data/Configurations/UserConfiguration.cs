namespace BrandVault.Api.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrandVault.Api.Models;

/// <summary>
/// Configures how the User entity maps to the "users" PostgreSQL table.
///
/// This is the EF Core equivalent of a Prisma schema definition.
/// Instead of writing:
///   model User {
///     email String @unique
///     ...
///   }
///
/// We write fluent C# that does the same thing — sets constraints,
/// indexes, max lengths, and default values.
///
/// IEntityTypeConfiguration&lt;User&gt; is an interface that says:
/// "This class knows how to configure the User entity."
/// AppDbContext auto-discovers these via ApplyConfigurationsFromAssembly.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Store the enum as a string in the database (e.g., "Admin", "Designer")
        // instead of an integer. Makes the database more readable.
        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("NOW()");
    }
}
