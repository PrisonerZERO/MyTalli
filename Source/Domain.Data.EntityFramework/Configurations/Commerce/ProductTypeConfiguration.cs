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
        builder.Property(e => e.IsDeleted).HasColumnOrder(2);
        builder.Property(e => e.IsVisible).HasColumnOrder(3);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(4);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(5);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(6);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(7);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }


    #endregion
}
