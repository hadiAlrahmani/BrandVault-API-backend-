namespace BrandVault.Api.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrandVault.Api.Models;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("workspaces");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Description).HasMaxLength(2000);

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(w => w.CreatedAt)
            .HasDefaultValueSql("NOW()");

        // Cascade: deleting a client deletes its workspaces
        builder.HasOne(w => w.Client)
            .WithMany(c => c.Workspaces)
            .HasForeignKey(w => w.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: can't delete a user who created workspaces
        builder.HasOne(w => w.CreatedBy)
            .WithMany(u => u.CreatedWorkspaces)
            .HasForeignKey(w => w.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
