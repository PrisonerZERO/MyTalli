namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class PayoutStripeConfiguration : IEntityTypeConfiguration<PayoutStripe>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<PayoutStripe> builder)
	{
		builder.ToTable("PayoutStripe", "app");

		builder.HasKey(e => e.Id).HasName("PK_PayoutStripe");

		builder.Property(e => e.Id).HasColumnName("PayoutId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.PayoutMethod).HasMaxLength(20).IsRequired().HasColumnOrder(1);
		builder.Property(e => e.StatementDescriptor).HasMaxLength(500).HasColumnOrder(2);
		builder.Property(e => e.StripePayoutId).HasMaxLength(255).IsRequired().HasColumnOrder(3);
		builder.Property(e => e.IsDeleted).HasColumnOrder(4);
		builder.Property(e => e.IsVisible).HasColumnOrder(5);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(6);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(7);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(8);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(9);

		builder.HasOne(e => e.Payout)
			.WithOne()
			.HasForeignKey<PayoutStripe>(e => e.Id)
			.HasConstraintName("FK_PayoutStripe_Payout");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
