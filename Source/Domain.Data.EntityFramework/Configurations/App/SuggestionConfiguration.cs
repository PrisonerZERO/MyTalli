namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class SuggestionConfiguration : IEntityTypeConfiguration<Suggestion>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Suggestion> builder)
    {
        builder.ToTable("Suggestion", "app");

        builder.HasKey(e => e.Id).HasName("PK_Suggestion");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.UserId).HasColumnOrder(1);
        builder.Property(e => e.AdminNote).HasMaxLength(500).HasColumnOrder(2);
        builder.Property(e => e.Category).HasMaxLength(50).IsRequired().HasColumnOrder(3);
        builder.Property(e => e.Description).HasMaxLength(2000).IsRequired().HasColumnOrder(4);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasColumnOrder(5);
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired().HasColumnOrder(6);
        builder.Property(e => e.IsDeleted).HasColumnOrder(7);
        builder.Property(e => e.IsVisible).HasColumnOrder(8);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(9);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(10);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(11);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(12);

        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_Suggestion_UserId");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("FK_Suggestion_User");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }


    #endregion
}
