namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class ShopConnectionConfiguration : IEntityTypeConfiguration<ShopConnection>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<ShopConnection> builder)
	{
		builder.ToTable("ShopConnection", "app");

		builder.HasKey(e => e.Id).HasName("PK_ShopConnection");

		builder.Property(e => e.Id).HasColumnOrder(0);
		builder.Property(e => e.PlatformConnectionId).HasColumnOrder(1);
		builder.Property(e => e.UserId).HasColumnOrder(2);
		builder.Property(e => e.AccessToken).IsRequired().HasColumnOrder(3);
		builder.Property(e => e.ConsecutiveFailures).HasDefaultValue(0).HasColumnOrder(4);
		builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnOrder(5);
		builder.Property(e => e.IsEnabled).HasDefaultValue(true).HasColumnOrder(6);
		builder.Property(e => e.LastErrorMessage).HasMaxLength(2000).HasColumnOrder(7);
		builder.Property(e => e.LastSyncDateTime).HasColumnOrder(8);
		builder.Property(e => e.NextSyncDateTime).HasColumnOrder(9);
		builder.Property(e => e.PlatformAccountId).HasMaxLength(255).IsRequired().HasColumnOrder(10);
		builder.Property(e => e.PlatformShopId).HasMaxLength(255).IsRequired().HasColumnOrder(11);
		builder.Property(e => e.RefreshToken).HasColumnOrder(12);
		builder.Property(e => e.ShopName).HasMaxLength(255).IsRequired().HasColumnOrder(13);
		builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasColumnOrder(14);
		builder.Property(e => e.TokenExpiryDateTime).HasColumnOrder(15);
		builder.Property(e => e.IsDeleted).HasColumnOrder(16);
		builder.Property(e => e.IsVisible).HasColumnOrder(17);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(18);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(19);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(20);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(21);

		builder.HasIndex(e => e.PlatformConnectionId).HasDatabaseName("IX_ShopConnection_PlatformConnectionId");
		builder.HasIndex(e => e.UserId).HasDatabaseName("IX_ShopConnection_UserId");
		builder.HasIndex(e => new { e.PlatformConnectionId, e.PlatformShopId }).IsUnique().HasDatabaseName("UQ_ShopConnection_PlatformConnectionId_PlatformShopId");
		builder.HasIndex(e => new { e.NextSyncDateTime, e.Status }).HasDatabaseName("IX_ShopConnection_NextSyncDateTime_Status");

		builder.HasOne(e => e.PlatformConnection)
			.WithMany()
			.HasForeignKey(e => e.PlatformConnectionId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("FK_ShopConnection_PlatformConnection");

		builder.HasOne(e => e.User)
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("FK_ShopConnection_User");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
