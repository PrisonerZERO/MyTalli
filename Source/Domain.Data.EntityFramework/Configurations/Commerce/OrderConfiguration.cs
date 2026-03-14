namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities;

/// <summary>Configuration</summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Order", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_Order");

        builder.Property(e => e.OrderDateTime).IsRequired();
        builder.Property(e => e.TaxCharged).HasColumnType("money").IsRequired();

        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_Order_UserId");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("FK_Order_User");
    }

    #endregion
}
