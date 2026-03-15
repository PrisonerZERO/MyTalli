namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class BillingConfiguration : IEntityTypeConfiguration<Billing>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Billing> builder)
    {
        builder.ToTable("Billing", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_Billing");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.OrderId).HasColumnOrder(1);
        builder.Property(e => e.UserId).HasColumnOrder(2);
        builder.Property(e => e.Amount).HasColumnType("money").IsRequired().HasColumnOrder(3);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasColumnOrder(4);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasColumnOrder(5);
        builder.Property(e => e.IsDeleted).HasColumnOrder(6);
        builder.Property(e => e.IsVisible).HasColumnOrder(7);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(8);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(9);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(10);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(11);

        builder.HasIndex(e => e.OrderId).HasDatabaseName("IX_Billing_OrderId");
        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_Billing_UserId");

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .HasConstraintName("FK_Billing_Order");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("FK_Billing_User")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }

    #endregion
}
