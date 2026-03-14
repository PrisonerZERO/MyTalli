namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities;

/// <summary>Configuration</summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_Product");

        builder.Property(e => e.ProductName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.VendorPrice).HasColumnType("money").IsRequired();

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
