namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class ExpenseEtsyConfiguration : IEntityTypeConfiguration<ExpenseEtsy>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<ExpenseEtsy> builder)
	{
		builder.ToTable("ExpenseEtsy", "app");

		builder.HasKey(e => e.Id).HasName("PK_ExpenseEtsy");

		builder.Property(e => e.Id).HasColumnName("ExpenseId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.AdCampaignId).HasColumnOrder(1);
		builder.Property(e => e.LedgerEntryId).HasColumnOrder(2);
		builder.Property(e => e.ListingId).HasColumnOrder(3);
		builder.Property(e => e.IsDeleted).HasColumnOrder(4);
		builder.Property(e => e.IsVisible).HasColumnOrder(5);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(6);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(7);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(8);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(9);

		builder.HasOne(e => e.Expense)
			.WithOne()
			.HasForeignKey<ExpenseEtsy>(e => e.Id)
			.HasConstraintName("FK_ExpenseEtsy_Expense");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
