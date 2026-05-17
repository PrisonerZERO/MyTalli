namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using My.Talli.Web.Services.Billing;

/// <summary>Stub</summary>
public class StripeBillingApiClientStub : IStripeBillingApiClient
{
	#region <Variables>

	private readonly Dictionary<string, StripeSubscriptionInfo?> _responses = new(StringComparer.Ordinal);
	private readonly Dictionary<string, Exception> _throws = new(StringComparer.Ordinal);

	#endregion

	#region <Properties>

	public List<string> Calls { get; } = new();

	#endregion

	#region <Methods>

	public Task<StripeSubscriptionInfo?> GetSubscriptionAsync(string stripeSubscriptionId, CancellationToken cancellationToken)
	{
		Calls.Add(stripeSubscriptionId);

		if (_throws.TryGetValue(stripeSubscriptionId, out var ex))
			throw ex;

		return Task.FromResult(_responses.TryGetValue(stripeSubscriptionId, out var info) ? info : null);
	}

	public void SetNotFound(string stripeSubscriptionId)
	{
		_responses[stripeSubscriptionId] = null;
	}

	public void SetResponse(string stripeSubscriptionId, StripeSubscriptionInfo info)
	{
		_responses[stripeSubscriptionId] = info;
	}

	public void SetThrows(string stripeSubscriptionId, Exception ex)
	{
		_throws[stripeSubscriptionId] = ex;
	}

	#endregion
}
