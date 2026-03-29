namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class RevenueGumroadConfiguration : IEntityTypeConfiguration<RevenueGumroad>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<RevenueGumroad> builder)
	{
		builder.ToTable("RevenueGumroad", "app");

		builder.HasKey(e => e.Id).HasName("PK_RevenueGumroad");

		builder.Property(e => e.Id).HasColumnName("RevenueId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.DiscoverFee).HasColumnType("decimal(18,2)").HasColumnOrder(1);
		builder.Property(e => e.LicenseKey).HasMaxLength(500).HasColumnOrder(2);
		builder.Property(e => e.SaleId).HasMaxLength(255).IsRequired().HasColumnOrder(3);
		builder.Property(e => e.IsDeleted).HasColumnOrder(4);
		builder.Property(e => e.IsVisible).HasColumnOrder(5);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(6);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(7);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(8);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(9);

		builder.HasOne(e => e.Revenue)
			.WithOne()
			.HasForeignKey<RevenueGumroad>(e => e.Id)
			.HasConstraintName("FK_RevenueGumroad_Revenue");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
