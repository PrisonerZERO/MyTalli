namespace My.Talli.Domain.Data.Configurations.Commerce;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities.Entities;

/// <summary>Entity configuration for ProductVendor.</summary>
public class ProductVendorConfiguration : IEntityTypeConfiguration<ProductVendor>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<ProductVendor> builder)
    {
        builder.ToTable("ProductVendor", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_ProductVendor");

        builder.Property(e => e.VendorName).HasMaxLength(100).IsRequired();
    }

    #endregion
}
