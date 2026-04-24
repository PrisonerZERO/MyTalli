namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class ExpenseStripeConfiguration : IEntityTypeConfiguration<ExpenseStripe>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<ExpenseStripe> builder)
	{
		builder.ToTable("ExpenseStripe", "app");

		builder.HasKey(e => e.Id).HasName("PK_ExpenseStripe");

		builder.Property(e => e.Id).HasColumnName("ExpenseId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.BalanceTransactionId).HasMaxLength(255).IsRequired().HasColumnOrder(1);
		builder.Property(e => e.FeeType).HasMaxLength(50).IsRequired().HasColumnOrder(2);
		builder.Property(e => e.IsDeleted).HasColumnOrder(3);
		builder.Property(e => e.IsVisible).HasColumnOrder(4);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(5);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(6);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(7);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(8);

		builder.HasOne(e => e.Expense)
			.WithOne()
			.HasForeignKey<ExpenseStripe>(e => e.Id)
			.HasConstraintName("FK_ExpenseStripe_Expense");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
