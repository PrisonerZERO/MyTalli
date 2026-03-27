namespace My.Talli.Domain.Data.EntityFramework.Configurations.Auth;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class AuthenticatedUserConfiguration : IEntityTypeConfiguration<AuthenticatedUser>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<AuthenticatedUser> builder)
    {
        builder.HasNoKey();
        builder.ToView("vAuthenticatedUser", "auth");

        builder.Property(e => e.DisplayName);
        builder.Property(e => e.EmailAddress);
        builder.Property(e => e.FirstName);
        builder.Property(e => e.Id);
        builder.Property(e => e.IsDeleted);
        builder.Property(e => e.LastName);
        builder.Property(e => e.PreferredProvider);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }


    #endregion
}
