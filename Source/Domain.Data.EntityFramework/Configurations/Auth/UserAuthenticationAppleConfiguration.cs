namespace My.Talli.Domain.Data.EntityFramework.Configurations.Auth;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class UserAuthenticationAppleConfiguration : IEntityTypeConfiguration<UserAuthenticationApple>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<UserAuthenticationApple> builder)
    {
        builder.ToTable("UserAuthenticationApple", "auth");

        builder.HasKey(e => e.Id).HasName("PK_UserAuthApple");

        builder.Property(e => e.Id).ValueGeneratedNever().HasColumnOrder(0);
        builder.Property(e => e.AppleId).HasMaxLength(256).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.DisplayName).HasMaxLength(100).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired().HasColumnOrder(3);
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired().HasColumnOrder(4);
        builder.Property(e => e.IsPrivateRelay).HasColumnOrder(5);
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired().HasColumnOrder(6);
        builder.Property(e => e.IsActive).HasColumnOrder(7);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(8);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(9);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(10);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(11);

        builder.HasIndex(e => e.AppleId).IsUnique().HasDatabaseName("UQ_UserAuthApple_AppleId");

        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<UserAuthenticationApple>(e => e.Id)
            .HasConstraintName("FK_UserAuthApple_User");
    }

    #endregion
}
