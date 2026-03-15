namespace My.Talli.Domain.Data.EntityFramework.Configurations.Commerce;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Configuration</summary>
public class SubscriptionStripeConfiguration : IEntityTypeConfiguration<SubscriptionStripe>
{
    #region <Methods>

    public void Configure(EntityTypeBuilder<SubscriptionStripe> builder)
    {
        builder.ToTable("SubscriptionStripe", "commerce");

        builder.HasKey(e => e.Id).HasName("PK_SubscriptionStripe");

        builder.Property(e => e.Id).HasColumnName("SubscriptionId").ValueGeneratedNever().HasColumnOrder(0);
        builder.Property(e => e.StripeCustomerId).HasMaxLength(256).IsRequired().HasColumnOrder(1);
        builder.Property(e => e.StripePriceId).HasMaxLength(256).IsRequired().HasColumnOrder(2);
        builder.Property(e => e.StripeSubscriptionId).HasMaxLength(256).IsRequired().HasColumnOrder(3);
        builder.Property(e => e.IsActive).HasColumnOrder(4);
        builder.Property(e => e.CreateByUserId).HasColumnOrder(5);
        builder.Property(e => e.CreatedOnDateTime).HasColumnOrder(6);
        builder.Property(e => e.UpdatedByUserId).HasColumnOrder(7);
        builder.Property(e => e.UpdatedOnDate).HasColumnOrder(8);

        builder.HasOne(e => e.Subscription)
            .WithOne()
            .HasForeignKey<SubscriptionStripe>(e => e.Id)
            .HasConstraintName("FK_SubscriptionStripe_Subscription");
    }

    #endregion
}
