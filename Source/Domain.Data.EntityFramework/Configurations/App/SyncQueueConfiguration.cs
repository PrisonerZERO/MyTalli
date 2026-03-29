namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class SyncQueueConfiguration : IEntityTypeConfiguration<SyncQueue>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<SyncQueue> builder)
	{
		builder.ToTable("SyncQueue", "app");

		builder.HasKey(e => e.Id).HasName("PK_SyncQueue");

		builder.Property(e => e.Id).HasColumnOrder(0);
		builder.Property(e => e.UserId).HasColumnOrder(1);
		builder.Property(e => e.ConsecutiveFailures).HasDefaultValue(0).HasColumnOrder(2);
		builder.Property(e => e.IsEnabled).HasDefaultValue(true).HasColumnOrder(3);
		builder.Property(e => e.LastErrorMessage).HasMaxLength(2000).HasColumnOrder(4);
		builder.Property(e => e.LastSyncDateTime).HasColumnOrder(5);
		builder.Property(e => e.NextSyncDateTime).HasColumnOrder(6);
		builder.Property(e => e.Platform).HasMaxLength(50).IsRequired().HasColumnOrder(7);
		builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasColumnOrder(8);
		builder.Property(e => e.IsDeleted).HasColumnOrder(9);
		builder.Property(e => e.IsVisible).HasColumnOrder(10);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(11);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(12);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(13);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(14);

		builder.HasIndex(e => e.UserId).HasDatabaseName("IX_SyncQueue_UserId");
		builder.HasIndex(e => new { e.UserId, e.Platform }).IsUnique().HasDatabaseName("UQ_SyncQueue_UserId_Platform");
		builder.HasIndex(e => new { e.NextSyncDateTime, e.Status }).HasDatabaseName("IX_SyncQueue_NextSyncDateTime_Status");

		builder.HasOne(e => e.User)
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.HasConstraintName("FK_SyncQueue_User");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
