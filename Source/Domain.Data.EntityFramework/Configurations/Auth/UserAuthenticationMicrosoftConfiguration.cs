namespace My.Talli.Domain.Data.EntityFramework.Configurations.Auth;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class UserAuthenticationMicrosoftConfiguration : IEntityTypeConfiguration<UserAuthenticationMicrosoft>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<UserAuthenticationMicrosoft> builder)
    {
        builder.ToTable("UserAuthenticationMicrosoft", "auth");

        builder.HasKey(e => e.Id).HasName("PK_UserAuthMicrosoft");

        builder.Property(e => e.Id).ValueGeneratedNever().HasColumnOrder(0);
        builder.Property(e => e.DisplayName).HasMaxLength(100).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired().HasColumnOrder(3);
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired().HasColumnOrder(4);
        builder.Property(e => e.MicrosoftId).HasMaxLength(256).IsRequired().HasColumnOrder(5);
        builder.Property(e => e.IsActive).HasColumnOrder(6);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(7);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(8);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(9);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(10);

        builder.HasIndex(e => e.MicrosoftId).IsUnique().HasDatabaseName("UQ_UserAuthMicrosoft_MicrosoftId");

        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<UserAuthenticationMicrosoft>(e => e.Id)
            .HasConstraintName("FK_UserAuthMicrosoft_User");
    }

    #endregion
}
