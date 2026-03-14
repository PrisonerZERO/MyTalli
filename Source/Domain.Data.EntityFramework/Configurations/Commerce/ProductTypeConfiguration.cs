namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities;

/// <summary>Configuration</summary>
public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.ToTable("ProductType", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_ProductType");

        builder.Property(e => e.ProductTypeName).HasMaxLength(50).IsRequired();
    }

    #endregion
}
