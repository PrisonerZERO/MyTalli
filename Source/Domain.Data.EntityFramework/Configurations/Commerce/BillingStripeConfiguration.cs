namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class BillingStripeConfiguration : IEntityTypeConfiguration<BillingStripe>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<BillingStripe> builder)
    {
        builder.ToTable("BillingStripe", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_BillingStripe");

        builder.Property(e => e.Id).ValueGeneratedNever().HasColumnOrder(0);
        builder.Property(e => e.CardBrand).HasMaxLength(20).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.CardLastFour).HasMaxLength(4).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.PaymentMethod).HasMaxLength(50).IsRequired().HasColumnOrder(3);
        builder.Property(e => e.StripePaymentIntentId).HasMaxLength(256).IsRequired().HasColumnOrder(4);
        builder.Property(e => e.IsActive).HasColumnOrder(5);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(6);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(7);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(8);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(9);

        builder.HasOne(e => e.Billing)
            .WithOne()
            .HasForeignKey<BillingStripe>(e => e.Id)
            .HasConstraintName("FK_BillingStripe_Billing");
    }

    #endregion
}
