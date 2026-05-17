namespace My.Talli.Web.Services.Billing;

/// <summary>Interface</summary>
public interface IStripeBillingApiClient
{
	#region <Methods>

	/// <summary>Returns subscription info from Stripe, or null if Stripe reports the subscription does not exist (404).</summary>
	Task<StripeSubscriptionInfo?> GetSubscriptionAsync(string stripeSubscriptionId, CancellationToken cancellationToken);

	#endregion
}
