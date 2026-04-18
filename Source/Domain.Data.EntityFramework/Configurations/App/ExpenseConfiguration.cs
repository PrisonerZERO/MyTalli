namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<Expense> builder)
	{
		builder.ToTable("Expense", "app");

		builder.HasKey(e => e.Id).HasName("PK_Expense");

		builder.Property(e => e.Id).HasColumnOrder(0);
		builder.Property(e => e.ShopConnectionId).HasColumnOrder(1);
		builder.Property(e => e.UserId).HasColumnOrder(2);
		builder.Property(e => e.Amount).HasColumnType("decimal(18,2)").HasColumnOrder(3);
		builder.Property(e => e.Category).HasMaxLength(50).IsRequired().HasColumnOrder(4);
		builder.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasColumnOrder(5);
		builder.Property(e => e.Description).HasMaxLength(500).IsRequired().HasColumnOrder(6);
		builder.Property(e => e.ExpenseDate).HasColumnOrder(7);
		builder.Property(e => e.Platform).HasMaxLength(50).IsRequired().HasColumnOrder(8);
		builder.Property(e => e.PlatformTransactionId).HasMaxLength(255).HasColumnOrder(9);
		builder.Property(e => e.IsDeleted).HasColumnOrder(10);
		builder.Property(e => e.IsVisible).HasColumnOrder(11);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(12);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(13);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(14);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(15);

		builder.HasIndex(e => e.ShopConnectionId).HasDatabaseName("IX_Expense_ShopConnectionId");
		builder.HasIndex(e => e.UserId).HasDatabaseName("IX_Expense_UserId");
		builder.HasIndex(e => new { e.Platform, e.ExpenseDate }).HasDatabaseName("IX_Expense_Platform_ExpenseDate");

		builder.HasOne(e => e.ShopConnection)
			.WithMany()
			.HasForeignKey(e => e.ShopConnectionId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("FK_Expense_ShopConnection");

		builder.HasOne(e => e.User)
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.HasConstraintName("FK_Expense_User");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
