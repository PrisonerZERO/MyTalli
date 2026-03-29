namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class PlatformConnectionConfiguration : IEntityTypeConfiguration<PlatformConnection>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<PlatformConnection> builder)
	{
		builder.ToTable("PlatformConnection", "app");

		builder.HasKey(e => e.Id).HasName("PK_PlatformConnection");

		builder.Property(e => e.Id).HasColumnOrder(0);
		builder.Property(e => e.UserId).HasColumnOrder(1);
		builder.Property(e => e.AccessToken).IsRequired().HasColumnOrder(2);
		builder.Property(e => e.ConnectionStatus).HasMaxLength(50).IsRequired().HasColumnOrder(3);
		builder.Property(e => e.Platform).HasMaxLength(50).IsRequired().HasColumnOrder(4);
		builder.Property(e => e.PlatformAccountId).HasMaxLength(255).IsRequired().HasColumnOrder(5);
		builder.Property(e => e.RefreshToken).HasColumnOrder(6);
		builder.Property(e => e.TokenExpiryDateTime).HasColumnOrder(7);
		builder.Property(e => e.IsDeleted).HasColumnOrder(8);
		builder.Property(e => e.IsVisible).HasColumnOrder(9);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(10);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(11);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(12);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(13);

		builder.HasIndex(e => e.UserId).HasDatabaseName("IX_PlatformConnection_UserId");
		builder.HasIndex(e => new { e.UserId, e.Platform }).IsUnique().HasDatabaseName("UQ_PlatformConnection_UserId_Platform");

		builder.HasOne(e => e.User)
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.HasConstraintName("FK_PlatformConnection_User");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
