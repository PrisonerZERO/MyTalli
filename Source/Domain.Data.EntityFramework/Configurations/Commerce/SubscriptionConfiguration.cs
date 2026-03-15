namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscription", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_Subscription");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.OrderItemId).HasColumnOrder(1);
        builder.Property(e => e.ProductId).HasColumnOrder(2);
        builder.Property(e => e.UserId).HasColumnOrder(3);
        builder.Property(e => e.CancelledDate).IsRequired().HasColumnOrder(4);
        builder.Property(e => e.EndDate).IsRequired().HasColumnOrder(5);
        builder.Property(e => e.RenewalDate).IsRequired().HasColumnOrder(6);
        builder.Property(e => e.StartDate).IsRequired().HasColumnOrder(7);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasColumnOrder(8);
        builder.Property(e => e.IsDeleted).HasColumnOrder(9);
        builder.Property(e => e.IsVisible).HasColumnOrder(10);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(11);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(12);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(13);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(14);

        builder.HasIndex(e => e.OrderItemId).HasDatabaseName("IX_Subscription_OrderItemId");
        builder.HasIndex(e => e.ProductId).HasDatabaseName("IX_Subscription_ProductId");
        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_Subscription_UserId");

        builder.HasOne(e => e.OrderItem)
            .WithMany()
            .HasForeignKey(e => e.OrderItemId)
            .HasConstraintName("FK_Subscription_OrderItem");

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .HasConstraintName("FK_Subscription_Product")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("FK_Subscription_User")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }

    #endregion
}
