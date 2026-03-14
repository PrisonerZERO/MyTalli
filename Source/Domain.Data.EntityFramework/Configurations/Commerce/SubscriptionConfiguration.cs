namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

/// <summary>Configuration</summary>
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscription", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_Subscription");

        builder.Property(e => e.CancelledDate).IsRequired();
        builder.Property(e => e.EndDate).IsRequired();
        builder.Property(e => e.RenewalDate).IsRequired();
        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

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
    }

    #endregion
}
