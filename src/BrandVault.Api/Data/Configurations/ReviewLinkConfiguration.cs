namespace BrandVault.Api.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrandVault.Api.Models;

public class ReviewLinkConfiguration : IEntityTypeConfiguration<ReviewLink>
{
    public void Configure(EntityTypeBuilder<ReviewLink> builder)
    {
        builder.ToTable("review_links");

        builder.HasKey(rl => rl.Id);

        builder.Property(rl => rl.Token)
            .IsRequired()
            .HasMaxLength(500);

        // Unique index on token for fast lookups from the review URL
        builder.HasIndex(rl => rl.Token)
            .IsUnique();

        builder.Property(rl => rl.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(rl => rl.Workspace)
            .WithMany(w => w.ReviewLinks)
            .HasForeignKey(rl => rl.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rl => rl.CreatedBy)
            .WithMany(u => u.CreatedReviewLinks)
            .HasForeignKey(rl => rl.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
