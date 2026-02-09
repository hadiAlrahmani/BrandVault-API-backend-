namespace BrandVault.Api.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrandVault.Api.Models;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.AuthorName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Content).IsRequired().HasMaxLength(5000);

        builder.Property(c => c.AuthorType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(c => c.Asset)
            .WithMany(a => a.Comments)
            .HasForeignKey(c => c.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        // SetNull: if a review link is deleted, keep the comments but clear the link reference
        builder.HasOne(c => c.ReviewLink)
            .WithMany(rl => rl.Comments)
            .HasForeignKey(c => c.ReviewLinkId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
