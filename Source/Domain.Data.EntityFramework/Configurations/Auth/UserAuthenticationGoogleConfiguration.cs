namespace My.Talli.Domain.Data.EntityFramework.Configurations.Auth;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class UserAuthenticationGoogleConfiguration : IEntityTypeConfiguration<UserAuthenticationGoogle>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<UserAuthenticationGoogle> builder)
    {
        builder.ToTable("UserAuthenticationGoogle", "auth");

        builder.HasKey(e => e.Id).HasName("PK_UserAuthGoogle");

        builder.Property(e => e.Id).HasColumnName("UserId").ValueGeneratedNever().HasColumnOrder(0);
        builder.Property(e => e.AvatarUrl).HasMaxLength(500).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.DisplayName).HasMaxLength(100).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired().HasColumnOrder(3);
        builder.Property(e => e.EmailVerified).HasColumnOrder(4);
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired().HasColumnOrder(5);
        builder.Property(e => e.GoogleId).HasMaxLength(256).IsRequired().HasColumnOrder(6);
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired().HasColumnOrder(7);
        builder.Property(e => e.Locale).HasMaxLength(10).IsRequired().HasColumnOrder(8);
        builder.Property(e => e.IsDeleted).HasColumnOrder(9);
        builder.Property(e => e.IsVisible).HasColumnOrder(10);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(11);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(12);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(13);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(14);

        builder.HasIndex(e => e.GoogleId).IsUnique().HasDatabaseName("UQ_UserAuthGoogle_GoogleId");

        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<UserAuthenticationGoogle>(e => e.Id)
            .HasConstraintName("FK_UserAuthGoogle_User");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }


    #endregion
}
