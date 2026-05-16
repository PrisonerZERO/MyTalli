namespace My.Talli.Domain.Data.EntityFramework.Configurations.Components;

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class DataProtectionKeyConfiguration : IEntityTypeConfiguration<DataProtectionKey>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
    {
        builder.ToTable("DataProtectionKey", "components");

        builder.HasKey(e => e.Id).HasName("PK_DataProtectionKey");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.FriendlyName).HasColumnOrder(1);
        builder.Property(e => e.Xml).HasColumnOrder(2);
    }


    #endregion
}
