namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class RevenueConfiguration : IEntityTypeConfiguration<Revenue>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Revenue> builder)
    {
        builder.ToTable("Revenue", "app");

        builder.HasKey(e => e.Id).HasName("PK_Revenue");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.UserId).HasColumnOrder(1);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired().HasColumnOrder(3);
        builder.Property(e => e.FeeAmount).HasPrecision(18, 2).HasColumnOrder(4);
        builder.Property(e => e.GrossAmount).HasPrecision(18, 2).HasColumnOrder(5);
        builder.Property(e => e.IsDisputed).HasColumnOrder(6);
        builder.Property(e => e.IsRefunded).HasColumnOrder(7);
        builder.Property(e => e.NetAmount).HasPrecision(18, 2).HasColumnOrder(8);
        builder.Property(e => e.Platform).HasMaxLength(50).IsRequired().HasColumnOrder(9);
        builder.Property(e => e.PlatformTransactionId).HasMaxLength(255).IsRequired().HasColumnOrder(10);
        builder.Property(e => e.TransactionDate).IsRequired().HasColumnOrder(11);
        builder.Property(e => e.IsDeleted).HasColumnOrder(12);
        builder.Property(e => e.IsVisible).HasColumnOrder(13);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(14);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(15);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(16);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(17);

        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_Revenue_UserId");
        builder.HasIndex(e => new { e.Platform, e.TransactionDate }).HasDatabaseName("IX_Revenue_Platform_TransactionDate");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("FK_Revenue_User");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }

    #endregion
}
