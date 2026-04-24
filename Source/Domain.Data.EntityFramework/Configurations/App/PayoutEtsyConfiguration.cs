namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class PayoutEtsyConfiguration : IEntityTypeConfiguration<PayoutEtsy>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<PayoutEtsy> builder)
	{
		builder.ToTable("PayoutEtsy", "app");

		builder.HasKey(e => e.Id).HasName("PK_PayoutEtsy");

		builder.Property(e => e.Id).HasColumnName("PayoutId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.LedgerEntryId).HasColumnOrder(1);
		builder.Property(e => e.ShopCurrency).HasMaxLength(3).IsRequired().IsFixedLength().HasColumnOrder(2);
		builder.Property(e => e.IsDeleted).HasColumnOrder(3);
		builder.Property(e => e.IsVisible).HasColumnOrder(4);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(5);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(6);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(7);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(8);

		builder.HasOne(e => e.Payout)
			.WithOne()
			.HasForeignKey<PayoutEtsy>(e => e.Id)
			.HasConstraintName("FK_PayoutEtsy_Payout");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
