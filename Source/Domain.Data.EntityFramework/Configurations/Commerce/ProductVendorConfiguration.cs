namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class ProductVendorConfiguration : IEntityTypeConfiguration<ProductVendor>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<ProductVendor> builder)
    {
        builder.ToTable("ProductVendor", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_ProductVendor");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.VendorName).HasMaxLength(100).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.IsActive).HasColumnOrder(2);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(3);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(4);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(5);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(6);
    }

    #endregion
}
