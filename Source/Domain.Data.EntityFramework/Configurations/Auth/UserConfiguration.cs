namespace My.Talli.Domain.Data.EntityFramework.Configurations.Auth;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User", "auth");

        builder.HasKey(e => e.Id).HasName("PK_User");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.DisplayName).HasMaxLength(100).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.InitialProvider).HasMaxLength(20).IsRequired().HasColumnOrder(3);
        builder.Property(e => e.LastLoginAt).IsRequired().HasColumnOrder(4);
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired().HasColumnOrder(5);
        builder.Property(e => e.PreferredProvider).HasMaxLength(20).IsRequired().HasColumnOrder(6);
        builder.Property(e => e.UserPreferences).IsRequired().HasColumnOrder(7);
        builder.Property(e => e.IsActive).HasColumnOrder(8);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(9);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(10);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(11);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(12);
    }

    #endregion
}
