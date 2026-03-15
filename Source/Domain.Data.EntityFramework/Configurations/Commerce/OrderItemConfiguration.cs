namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItem", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_OrderItem");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.OrderId).HasColumnOrder(1);
        builder.Property(e => e.ProductId).HasColumnOrder(2);
        builder.Property(e => e.ProductPriceCharged).HasColumnType("money").IsRequired().HasColumnOrder(3);
        builder.Property(e => e.ProductQuantity).IsRequired().HasColumnOrder(4);
        builder.Property(e => e.IsDeleted).HasColumnOrder(5);
        builder.Property(e => e.IsVisible).HasColumnOrder(6);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(7);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(8);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(9);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(10);

        builder.HasIndex(e => e.OrderId).HasDatabaseName("IX_OrderItem_OrderId");
        builder.HasIndex(e => e.ProductId).HasDatabaseName("IX_OrderItem_ProductId");

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .HasConstraintName("FK_OrderItem_Order");

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .HasConstraintName("FK_OrderItem_Product");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }

    #endregion
}
