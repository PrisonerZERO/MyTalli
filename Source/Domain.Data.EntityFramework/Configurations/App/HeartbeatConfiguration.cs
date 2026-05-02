namespace My.Talli.Domain.Data.EntityFramework.Configurations.App;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class HeartbeatConfiguration : IEntityTypeConfiguration<Heartbeat>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Heartbeat> builder)
    {
        builder.ToTable("Heartbeat", "app");

        builder.HasKey(e => e.Id).HasName("PK_Heartbeat");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.ExpectedIntervalSeconds).HasColumnOrder(1);
        builder.Property(e => e.HeartbeatSource).HasMaxLength(100).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.LastTickAt).HasColumnOrder(3);
        builder.Property(e => e.Metadata).HasColumnOrder(4);
        builder.Property(e => e.IsDeleted).HasColumnOrder(5);
        builder.Property(e => e.IsVisible).HasColumnOrder(6);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(7);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(8);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(9);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(10);

        builder.HasIndex(e => e.HeartbeatSource).IsUnique().HasDatabaseName("UQ_Heartbeat_HeartbeatSource");
        builder.HasIndex(e => e.LastTickAt).HasDatabaseName("IX_Heartbeat_LastTickAt");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }


    #endregion
}
