namespace My.Talli.Domain.Data.EntityFramework.Configurations.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities.Entities;

/// <summary>Entity configuration for User.</summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User", "auth");

        builder.HasKey(e => e.Id).HasName("PK_User");

        builder.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.InitialProvider).HasMaxLength(20).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.PreferredProvider).HasMaxLength(20).IsRequired();
        builder.Property(e => e.UserPreferences).IsRequired();
    }

    #endregion
}
