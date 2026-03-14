namespace My.Talli.Domain.Data.Configurations.Commerce;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities.Entities;

/// <summary>Entity configuration for OrderItem.</summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItem", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_OrderItem");

        builder.Property(e => e.ProductPriceCharged).HasColumnType("money").IsRequired();
        builder.Property(e => e.ProductQuantity).IsRequired();

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
    }

    #endregion
}
