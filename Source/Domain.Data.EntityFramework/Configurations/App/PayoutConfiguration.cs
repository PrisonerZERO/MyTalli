namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class PayoutConfiguration : IEntityTypeConfiguration<Payout>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<Payout> builder)
	{
		builder.ToTable("Payout", "app");

		builder.HasKey(e => e.Id).HasName("PK_Payout");

		builder.Property(e => e.Id).HasColumnOrder(0);
		builder.Property(e => e.ShopConnectionId).HasColumnOrder(1);
		builder.Property(e => e.UserId).HasColumnOrder(2);
		builder.Property(e => e.Amount).HasColumnType("decimal(18,2)").HasColumnOrder(3);
		builder.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasColumnOrder(4);
		builder.Property(e => e.ExpectedArrivalDate).HasColumnOrder(5);
		builder.Property(e => e.PayoutDate).HasColumnOrder(6);
		builder.Property(e => e.Platform).HasMaxLength(50).IsRequired().HasColumnOrder(7);
		builder.Property(e => e.PlatformPayoutId).HasMaxLength(255).IsRequired().HasColumnOrder(8);
		builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasColumnOrder(9);
		builder.Property(e => e.IsDeleted).HasColumnOrder(10);
		builder.Property(e => e.IsVisible).HasColumnOrder(11);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(12);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(13);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(14);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(15);

		builder.HasIndex(e => e.ShopConnectionId).HasDatabaseName("IX_Payout_ShopConnectionId");
		builder.HasIndex(e => e.UserId).HasDatabaseName("IX_Payout_UserId");
		builder.HasIndex(e => new { e.Platform, e.PayoutDate }).HasDatabaseName("IX_Payout_Platform_PayoutDate");
		builder.HasIndex(e => e.PlatformPayoutId).IsUnique().HasDatabaseName("UQ_Payout_PlatformPayoutId");

		builder.HasOne(e => e.ShopConnection)
			.WithMany()
			.HasForeignKey(e => e.ShopConnectionId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("FK_Payout_ShopConnection");

		builder.HasOne(e => e.User)
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.HasConstraintName("FK_Payout_User");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
