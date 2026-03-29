namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<Goal> builder)
	{
		builder.ToTable("Goal", "app");

		builder.HasKey(e => e.Id).HasName("PK_Goal");

		builder.Property(e => e.Id).HasColumnOrder(0);
		builder.Property(e => e.GoalTypeId).HasColumnOrder(1);
		builder.Property(e => e.UserId).HasColumnOrder(2);
		builder.Property(e => e.EndDate).HasColumnOrder(3);
		builder.Property(e => e.Platform).HasMaxLength(50).HasColumnOrder(4);
		builder.Property(e => e.StartDate).HasColumnOrder(5);
		builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasColumnOrder(6);
		builder.Property(e => e.TargetAmount).HasColumnType("decimal(18,2)").HasColumnOrder(7);
		builder.Property(e => e.IsDeleted).HasColumnOrder(8);
		builder.Property(e => e.IsVisible).HasColumnOrder(9);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(10);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(11);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(12);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(13);

		builder.HasIndex(e => e.GoalTypeId).HasDatabaseName("IX_Goal_GoalTypeId");
		builder.HasIndex(e => e.UserId).HasDatabaseName("IX_Goal_UserId");

		builder.HasOne(e => e.GoalType)
			.WithMany()
			.HasForeignKey(e => e.GoalTypeId)
			.HasConstraintName("FK_Goal_GoalType");

		builder.HasOne(e => e.User)
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.HasConstraintName("FK_Goal_User");

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
