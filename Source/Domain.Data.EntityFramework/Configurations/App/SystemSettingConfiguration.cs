namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSetting", "app");

        builder.HasKey(e => e.Id).HasName("PK_SystemSetting");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.SettingKey).HasMaxLength(100).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.SettingValue).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.IsDeleted).HasColumnOrder(3);
        builder.Property(e => e.IsVisible).HasColumnOrder(4);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(5);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(6);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(7);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(8);

        builder.HasIndex(e => e.SettingKey).IsUnique().HasDatabaseName("UQ_SystemSetting_SettingKey");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }


    #endregion
}
