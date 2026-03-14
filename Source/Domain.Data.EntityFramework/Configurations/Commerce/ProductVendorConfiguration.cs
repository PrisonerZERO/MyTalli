namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities.Entities;

/// <summary>Configuration</summary>
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
