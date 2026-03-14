namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using My.Talli.Domain.Entities.Entities;

/// <summary>Entity configuration for SubscriptionStripe.</summary>
public class SubscriptionStripeConfiguration : IEntityTypeConfiguration<SubscriptionStripe>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<SubscriptionStripe> builder)
    {
        builder.ToTable("SubscriptionStripe", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_SubscriptionStripe");

        builder.Property(e => e.StripeCustomerId).HasMaxLength(256).IsRequired();
        builder.Property(e => e.StripePriceId).HasMaxLength(256).IsRequired();
        builder.Property(e => e.StripeSubscriptionId).HasMaxLength(256).IsRequired();

        builder.HasIndex(e => e.SubscriptionId).IsUnique().HasDatabaseName("UQ_SubscriptionStripe_SubscriptionId");

        builder.HasOne(e => e.Subscription)
            .WithOne()
            .HasForeignKey<SubscriptionStripe>(e => e.SubscriptionId)
            .HasConstraintName("FK_SubscriptionStripe_Subscription");
    }

    #endregion
}
