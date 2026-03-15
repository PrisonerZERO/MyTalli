namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_Product");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.ProductTypeId).HasColumnOrder(1);
        builder.Property(e => e.VendorId).HasColumnOrder(2);
        builder.Property(e => e.ProductName).HasMaxLength(100).IsRequired().HasColumnOrder(3);
        builder.Property(e => e.VendorPrice).HasColumnType("money").IsRequired().HasColumnOrder(4);
        builder.Property(e => e.IsActive).HasColumnOrder(5);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(6);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(7);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(8);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(9);

        builder.HasIndex(e => e.ProductTypeId).HasDatabaseName("IX_Product_ProductTypeId");
        builder.HasIndex(e => e.VendorId).HasDatabaseName("IX_Product_VendorId");

        builder.HasOne(e => e.ProductType)
            .WithMany()
            .HasForeignKey(e => e.ProductTypeId)
            .HasConstraintName("FK_Product_ProductType");

        builder.HasOne(e => e.ProductVendor)
            .WithMany()
            .HasForeignKey(e => e.VendorId)
            .HasConstraintName("FK_Product_ProductVendor");
    }

    #endregion
}
