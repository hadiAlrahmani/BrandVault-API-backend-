namespace BrandVault.Api.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrandVault.Api.Models;

public class WorkspaceAssignmentConfiguration : IEntityTypeConfiguration<WorkspaceAssignment>
{
    public void Configure(EntityTypeBuilder<WorkspaceAssignment> builder)
    {
        builder.ToTable("workspace_assignments");

        builder.HasKey(wa => wa.Id);

        // Composite unique index: a user can only be assigned once per workspace.
        // In Prisma: @@unique([workspaceId, userId])
        builder.HasIndex(wa => new { wa.WorkspaceId, wa.UserId })
            .IsUnique();

        builder.Property(wa => wa.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(wa => wa.Workspace)
            .WithMany(w => w.Assignments)
            .HasForeignKey(wa => wa.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wa => wa.User)
            .WithMany(u => u.WorkspaceAssignments)
            .HasForeignKey(wa => wa.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
