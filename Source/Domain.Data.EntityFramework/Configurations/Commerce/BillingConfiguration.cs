namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities.Entities;

/// <summary>Configuration</summary>
public class BillingConfiguration : IEntityTypeConfiguration<Billing>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<Billing> builder)
    {
        builder.ToTable("Billing", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_Billing");

        builder.Property(e => e.Amount).HasColumnType("money").IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.OrderId).HasDatabaseName("IX_Billing_OrderId");
        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_Billing_UserId");

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .HasConstraintName("FK_Billing_Order");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("FK_Billing_User");
    }

    #endregion
}
