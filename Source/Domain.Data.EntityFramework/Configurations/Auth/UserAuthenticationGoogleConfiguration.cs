namespace My.Talli.Domain.Data.EntityFramework.Configurations.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities.Entities;

/// <summary>Entity configuration for UserAuthenticationGoogle.</summary>
public class UserAuthenticationGoogleConfiguration : IEntityTypeConfiguration<UserAuthenticationGoogle>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<UserAuthenticationGoogle> builder)
    {
        builder.ToTable("UserAuthenticationGoogle", "auth");

        builder.HasKey(e => e.Id).HasName("PK_UserAuthGoogle");

        builder.Property(e => e.AvatarUrl).HasMaxLength(500).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.GoogleId).HasMaxLength(256).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Locale).HasMaxLength(10).IsRequired();

        builder.HasIndex(e => e.GoogleId).IsUnique().HasDatabaseName("UQ_UserAuthGoogle_GoogleId");
        builder.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("UQ_UserAuthGoogle_UserId");

        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<UserAuthenticationGoogle>(e => e.UserId)
            .HasConstraintName("FK_UserAuthGoogle_User");
    }

    #endregion
}
