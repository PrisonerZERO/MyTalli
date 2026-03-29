namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class RevenueEtsyConfiguration : IEntityTypeConfiguration<RevenueEtsy>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<RevenueEtsy> builder)
	{
		builder.ToTable("RevenueEtsy", "app");

		builder.HasKey(e => e.Id).HasName("PK_RevenueEtsy");

		builder.Property(e => e.Id).HasColumnName("RevenueId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.AdjustedFees).HasColumnType("decimal(18,2)").HasColumnOrder(1);
		builder.Property(e => e.AdjustedGross).HasColumnType("decimal(18,2)").HasColumnOrder(2);
		builder.Property(e => e.AdjustedNet).HasColumnType("decimal(18,2)").HasColumnOrder(3);
		builder.Property(e => e.ListingId).HasColumnOrder(4);
		builder.Property(e => e.ReceiptId).HasColumnOrder(5);
		builder.Property(e => e.ShopCurrency).HasMaxLength(3).IsRequired().HasColumnOrder(6);
		builder.Property(e => e.IsDeleted).HasColumnOrder(7);
		builder.Property(e => e.IsVisible).HasColumnOrder(8);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(9);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(10);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(11);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(12);

		builder.HasOne(e => e.Revenue)
			.WithOne()
			.HasForeignKey<RevenueEtsy>(e => e.Id)
			.HasConstraintName("FK_RevenueEtsy_Revenue");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
