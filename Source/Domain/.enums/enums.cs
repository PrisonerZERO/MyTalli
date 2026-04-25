
namespace My.Talli.Domain.enums;

using Domain.attributes;

public enum EtsyLedgerTypes
{
    [StringValue("Foo")]
    Foo,

    [StringValue("Bar")]
    Bar,
}

public enum ExpenseCategory
{
    [StringValue("Listing Fee")]
    ListingFee,

    [StringValue("Ad Fee")]
    AdFee,

    [StringValue("Subscription Fee")]
    SubscriptionFee,

    [StringValue("Processing Fee")]
    ProcessingFee,

    [StringValue("Shipping Label")]
    ShippingLabel,

    [StringValue("Other")]
    Other,
}
