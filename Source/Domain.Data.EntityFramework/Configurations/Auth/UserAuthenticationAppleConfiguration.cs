namespace My.Talli.Domain.Data.EntityFramework.Configurations.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities.Entities;

/// <summary>Entity configuration for UserAuthenticationApple.</summary>
public class UserAuthenticationAppleConfiguration : IEntityTypeConfiguration<UserAuthenticationApple>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<UserAuthenticationApple> builder)
    {
        builder.ToTable("UserAuthenticationApple", "auth");

        builder.HasKey(e => e.Id).HasName("PK_UserAuthApple");

        builder.Property(e => e.AppleId).HasMaxLength(256).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired();

        builder.HasIndex(e => e.AppleId).IsUnique().HasDatabaseName("UQ_UserAuthApple_AppleId");
        builder.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("UQ_UserAuthApple_UserId");

        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<UserAuthenticationApple>(e => e.UserId)
            .HasConstraintName("FK_UserAuthApple_User");
    }

    #endregion
}
