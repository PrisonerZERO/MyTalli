namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class PayoutManualConfiguration : IEntityTypeConfiguration<PayoutManual>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<PayoutManual> builder)
	{
		builder.ToTable("PayoutManual", "app");

		builder.HasKey(e => e.Id).HasName("PK_PayoutManual");

		builder.Property(e => e.Id).HasColumnName("PayoutId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.Notes).HasColumnOrder(1);
		builder.Property(e => e.IsDeleted).HasColumnOrder(2);
		builder.Property(e => e.IsVisible).HasColumnOrder(3);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(4);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(5);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(6);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(7);

		builder.HasOne(e => e.Payout)
			.WithOne()
			.HasForeignKey<PayoutManual>(e => e.Id)
			.HasConstraintName("FK_PayoutManual_Payout");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
