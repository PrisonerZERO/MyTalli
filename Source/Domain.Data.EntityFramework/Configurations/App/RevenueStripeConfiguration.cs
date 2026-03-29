namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class RevenueStripeConfiguration : IEntityTypeConfiguration<RevenueStripe>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<RevenueStripe> builder)
	{
		builder.ToTable("RevenueStripe", "app");

		builder.HasKey(e => e.Id).HasName("PK_RevenueStripe");

		builder.Property(e => e.Id).HasColumnName("RevenueId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.BalanceTransactionId).HasMaxLength(255).IsRequired().HasColumnOrder(1);
		builder.Property(e => e.ExchangeRate).HasColumnType("decimal(18,6)").HasColumnOrder(2);
		builder.Property(e => e.PaymentMethod).HasMaxLength(50).IsRequired().HasColumnOrder(3);
		builder.Property(e => e.RiskScore).HasColumnOrder(4);
		builder.Property(e => e.IsDeleted).HasColumnOrder(5);
		builder.Property(e => e.IsVisible).HasColumnOrder(6);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(7);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(8);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(9);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(10);

		builder.HasOne(e => e.Revenue)
			.WithOne()
			.HasForeignKey<RevenueStripe>(e => e.Id)
			.HasConstraintName("FK_RevenueStripe_Revenue");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
