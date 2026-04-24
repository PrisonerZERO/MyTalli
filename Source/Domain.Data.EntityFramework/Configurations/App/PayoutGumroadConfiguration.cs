namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class PayoutGumroadConfiguration : IEntityTypeConfiguration<PayoutGumroad>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<PayoutGumroad> builder)
	{
		builder.ToTable("PayoutGumroad", "app");

		builder.HasKey(e => e.Id).HasName("PK_PayoutGumroad");

		builder.Property(e => e.Id).HasColumnName("PayoutId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.PayoutMethod).HasMaxLength(20).IsRequired().HasColumnOrder(1);
		builder.Property(e => e.IsDeleted).HasColumnOrder(2);
		builder.Property(e => e.IsVisible).HasColumnOrder(3);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(4);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(5);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(6);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(7);

		builder.HasOne(e => e.Payout)
			.WithOne()
			.HasForeignKey<PayoutGumroad>(e => e.Id)
			.HasConstraintName("FK_PayoutGumroad_Payout");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
