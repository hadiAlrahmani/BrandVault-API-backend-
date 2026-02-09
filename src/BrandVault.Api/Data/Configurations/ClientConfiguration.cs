namespace BrandVault.Api.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrandVault.Api.Models;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Company).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(255);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Industry).HasMaxLength(100);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        // Relationship: each client was created by one user.
        // Restrict delete = if you try to delete a user who created clients, EF will block it.
        // This prevents orphaned records. In Prisma: onDelete: Restrict
        builder.HasOne(c => c.CreatedBy)
            .WithMany(u => u.CreatedClients)
            .HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
