namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class ShopConnectionEtsyConfiguration : IEntityTypeConfiguration<ShopConnectionEtsy>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<ShopConnectionEtsy> builder)
	{
		builder.ToTable("ShopConnectionEtsy", "app");

		builder.HasKey(e => e.Id).HasName("PK_ShopConnectionEtsy");

		builder.Property(e => e.Id).HasColumnName("ShopConnectionId").ValueGeneratedNever().HasColumnOrder(0);
		builder.Property(e => e.CountryCode).HasMaxLength(2).IsRequired().IsFixedLength().HasColumnOrder(1);
		builder.Property(e => e.IsVacationMode).HasDefaultValue(false).HasColumnOrder(2);
		builder.Property(e => e.ShopCurrency).HasMaxLength(3).IsRequired().IsFixedLength().HasColumnOrder(3);
		builder.Property(e => e.ShopUrl).HasMaxLength(500).IsRequired().HasColumnOrder(4);
		builder.Property(e => e.IsDeleted).HasColumnOrder(5);
		builder.Property(e => e.IsVisible).HasColumnOrder(6);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(7);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(8);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(9);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(10);

		builder.HasOne(e => e.ShopConnection)
			.WithOne()
			.HasForeignKey<ShopConnectionEtsy>(e => e.Id)
			.HasConstraintName("FK_ShopConnectionEtsy_ShopConnection");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
