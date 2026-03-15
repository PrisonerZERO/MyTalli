namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Order", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_Order");

        builder.Property(e => e.Id).HasColumnOrder(0);
        builder.Property(e => e.UserId).HasColumnOrder(1);
        builder.Property(e => e.OrderDateTime).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.TaxCharged).HasColumnType("money").IsRequired().HasColumnOrder(3);
        builder.Property(e => e.IsActive).HasColumnOrder(4);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(5);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(6);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(7);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(8);

        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_Order_UserId");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("FK_Order_User");
    }

    #endregion
}
