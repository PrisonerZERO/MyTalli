namespace My.Talli.Domain.Data.Configurations.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities.Entities;

/// <summary>Entity configuration for UserAuthenticationMicrosoft.</summary>
public class UserAuthenticationMicrosoftConfiguration : IEntityTypeConfiguration<UserAuthenticationMicrosoft>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<UserAuthenticationMicrosoft> builder)
    {
        builder.ToTable("UserAuthenticationMicrosoft", "auth");

        builder.HasKey(e => e.Id).HasName("PK_UserAuthMicrosoft");

        builder.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.MicrosoftId).HasMaxLength(256).IsRequired();

        builder.HasIndex(e => e.MicrosoftId).IsUnique().HasDatabaseName("UQ_UserAuthMicrosoft_MicrosoftId");
        builder.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("UQ_UserAuthMicrosoft_UserId");

        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<UserAuthenticationMicrosoft>(e => e.UserId)
            .HasConstraintName("FK_UserAuthMicrosoft_User");
    }

    #endregion
}
