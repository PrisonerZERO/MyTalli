namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.ToTable("Milestone", "app");

        builder.HasKey(e => e.Id).HasName("PK_Milestone");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.MilestoneGroup).HasMaxLength(50).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.SortOrder).HasColumnOrder(3);
        builder.Property(e => e.Status).HasMaxLength(50).IsRequired().HasColumnOrder(4);
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired().HasColumnOrder(5);
        builder.Property(e => e.IsDeleted).HasColumnOrder(6);
        builder.Property(e => e.IsVisible).HasColumnOrder(7);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(8);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(9);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(10);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(11);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }


    #endregion
}
