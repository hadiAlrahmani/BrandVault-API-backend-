namespace BrandVault.Api.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrandVault.Api.Models;

public class AssetVersionConfiguration : IEntityTypeConfiguration<AssetVersion>
{
    public void Configure(EntityTypeBuilder<AssetVersion> builder)
    {
        builder.ToTable("asset_versions");

        builder.HasKey(av => av.Id);

        builder.Property(av => av.FilePath).IsRequired().HasMaxLength(1000);
        builder.Property(av => av.ThumbnailPath).HasMaxLength(1000);

        builder.Property(av => av.CreatedAt)
            .HasDefaultValueSql("NOW()");

        // Only one version 1, one version 2, etc. per asset
        // In Prisma: @@unique([assetId, versionNumber])
        builder.HasIndex(av => new { av.AssetId, av.VersionNumber })
            .IsUnique();

        builder.HasOne(av => av.Asset)
            .WithMany(a => a.Versions)
            .HasForeignKey(av => av.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(av => av.UploadedBy)
            .WithMany(u => u.UploadedVersions)
            .HasForeignKey(av => av.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
