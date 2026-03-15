namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.ToTable("ProductType", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_ProductType");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.ProductTypeName).HasMaxLength(50).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.IsActive).HasColumnOrder(2);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(3);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(4);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(5);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(6);
    }

    #endregion
}
