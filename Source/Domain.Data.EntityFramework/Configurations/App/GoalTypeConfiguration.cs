namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class GoalTypeConfiguration : IEntityTypeConfiguration<GoalType>
{
	#region <Methods>

	public void Configure(EntityTypeBuilder<GoalType> builder)
	{
		builder.ToTable("GoalType", "app");

		builder.HasKey(e => e.Id).HasName("PK_GoalType");

		builder.Property(e => e.Id).HasColumnOrder(0);
		builder.Property(e => e.Name).HasMaxLength(100).IsRequired().HasColumnOrder(1);
		builder.Property(e => e.IsDeleted).HasColumnOrder(2);
		builder.Property(e => e.IsVisible).HasColumnOrder(3);
		builder.Property(e => e.CreateByUserId).HasColumnOrder(4);
		builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(5);
		builder.Property(e => e.UpdatedByUserId).HasColumnOrder(6);
		builder.Property(e => e.UpdatedOnDate).HasColumnOrder(7);

		builder.HasQueryFilter(e => !e.IsDeleted);
	}

	#endregion
}
