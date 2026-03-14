namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities;

/// <summary>Configuration</summary>
public class BillingStripeConfiguration : IEntityTypeConfiguration<BillingStripe>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<BillingStripe> builder)
    {
        builder.ToTable("BillingStripe", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_BillingStripe");

        builder.Property(e => e.CardBrand).HasMaxLength(20).IsRequired();
        builder.Property(e => e.CardLastFour).HasMaxLength(4).IsRequired();
        builder.Property(e => e.PaymentMethod).HasMaxLength(50).IsRequired();
        builder.Property(e => e.StripePaymentIntentId).HasMaxLength(256).IsRequired();

        builder.HasIndex(e => e.BillingId).IsUnique().HasDatabaseName("UQ_BillingStripe_BillingId");

        builder.HasOne(e => e.Billing)
            .WithOne()
            .HasForeignKey<BillingStripe>(e => e.BillingId)
            .HasConstraintName("FK_BillingStripe_Billing");
    }

    #endregion
}
