namespace My.Talli.Domain.Data.EntityFramework.Configurations.Auth;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole", "auth");

        builder.HasKey(e => e.Id).HasName("PK_UserRole");

        builder.Property(e => e.Role).HasMaxLength(50).IsRequired();

        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_UserRole_UserId");
        builder.HasIndex(e => new { e.UserId, e.Role }).IsUnique().HasDatabaseName("UQ_UserRole_UserId_Role");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("FK_UserRole_User");
    }

    #endregion
}
