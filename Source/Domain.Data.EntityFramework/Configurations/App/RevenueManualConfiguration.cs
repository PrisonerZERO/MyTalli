namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class RevenueManualConfiguration : IEntityTypeConfiguration<RevenueManual>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<RevenueManual> builder)
    {
        builder.ToTable("RevenueManual", "app");

        builder.HasKey(e => e.Id).HasName("PK_RevenueManual");

        builder.Property(e => e.Id).HasColumnName("RevenueId").ValueGeneratedNever().HasColumnOrder(0);
        builder.Property(e => e.Category).HasMaxLength(50).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.Notes).HasMaxLength(1000).IsRequired(false).HasColumnOrder(2);
        builder.Property(e => e.Quantity).HasDefaultValue(1).HasColumnOrder(3);
        builder.Property(e => e.IsDeleted).HasColumnOrder(4);
        builder.Property(e => e.IsVisible).HasColumnOrder(5);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(6);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(7);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(8);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(9);

        builder.HasOne(e => e.Revenue)
            .WithOne()
            .HasForeignKey<RevenueManual>(e => e.Id)
            .HasConstraintName("FK_RevenueManual_Revenue");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }

    #endregion
}
