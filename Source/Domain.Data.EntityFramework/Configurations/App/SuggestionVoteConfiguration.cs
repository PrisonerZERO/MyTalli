namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class SuggestionVoteConfiguration : IEntityTypeConfiguration<SuggestionVote>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<SuggestionVote> builder)
    {
        builder.ToTable("SuggestionVote", "app");

        builder.HasKey(e => e.Id).HasName("PK_SuggestionVote");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.SuggestionId).HasColumnOrder(1);
        builder.Property(e => e.UserId).HasColumnOrder(2);
        builder.Property(e => e.IsDeleted).HasColumnOrder(3);
        builder.Property(e => e.IsVisible).HasColumnOrder(4);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(5);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(6);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(7);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(8);

        builder.HasIndex(e => e.SuggestionId).HasDatabaseName("IX_SuggestionVote_SuggestionId");
        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_SuggestionVote_UserId");
        builder.HasIndex(e => new { e.SuggestionId, e.UserId }).IsUnique().HasDatabaseName("UQ_SuggestionVote_SuggestionId_UserId");

        builder.HasOne(e => e.Suggestion)
            .WithMany()
            .HasForeignKey(e => e.SuggestionId)
            .HasConstraintName("FK_SuggestionVote_Suggestion");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("FK_SuggestionVote_User")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }


    #endregion
}
