
namespace My.Talli.Domain.enums;

using Domain.attributes;

public enum EtsyLedgerTypes
{
    [StringValue("Sale")]
    Sale,

    [StringValue("Refund")]
    Refund,

    [StringValue("Listing Fee")]
    ListingFee,

    [StringValue("Transaction Fee")]
    TransactionFee,

    [StringValue("Processing Fee")]
    ProcessingFee,

    [StringValue("Promoted Listing Fee")]
    PromotedListingFee,

    [StringValue("Marketing Fee")]
    MarketingFee,

    [StringValue("Subscription Fee")]
    SubscriptionFee,

    [StringValue("Postage Label")]
    PostageLabel,

    [StringValue("Shipping Label")]
    ShippingLabel,

    [StringValue("Tax")]
    Tax,

    [StringValue("VAT")]
    Vat,

    [StringValue("Payment")]
    Payment,

    [StringValue("Disbursement")]
    Disbursement,

    [StringValue("Withdrawal")]
    Withdrawal,
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
