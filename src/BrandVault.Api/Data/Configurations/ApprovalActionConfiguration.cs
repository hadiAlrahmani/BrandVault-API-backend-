namespace BrandVault.Api.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrandVault.Api.Models;

public class ApprovalActionConfiguration : IEntityTypeConfiguration<ApprovalAction>
{
    public void Configure(EntityTypeBuilder<ApprovalAction> builder)
    {
        builder.ToTable("approval_actions");

        builder.HasKey(aa => aa.Id);

        builder.Property(aa => aa.Comment).HasMaxLength(5000);
        builder.Property(aa => aa.DoneByName).IsRequired().HasMaxLength(200);

        builder.Property(aa => aa.ActionType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(aa => aa.DoneByType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(aa => aa.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(aa => aa.Asset)
            .WithMany(a => a.ApprovalActions)
            .HasForeignKey(aa => aa.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(aa => aa.ReviewLink)
            .WithMany(rl => rl.ApprovalActions)
            .HasForeignKey(aa => aa.ReviewLinkId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
