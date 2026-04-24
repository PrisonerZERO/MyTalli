namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class ExpenseManualConfiguration : IEntityTypeConfiguration<ExpenseManual>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<ExpenseManual> builder)
	{
		builder.ToTable("ExpenseManual", "app");

		builder.HasKey(e => e.Id).HasName("PK_ExpenseManual");

		builder.Property(e => e.Id).HasColumnName("ExpenseId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.Notes).HasColumnOrder(1);
		builder.Property(e => e.IsDeleted).HasColumnOrder(2);
		builder.Property(e => e.IsVisible).HasColumnOrder(3);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(4);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(5);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(6);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(7);

		builder.HasOne(e => e.Expense)
			.WithOne()
			.HasForeignKey<ExpenseManual>(e => e.Id)
			.HasConstraintName("FK_ExpenseManual_Expense");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
