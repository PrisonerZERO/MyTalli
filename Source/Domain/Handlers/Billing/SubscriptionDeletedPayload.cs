namespace My.Talli.Domain.Handlers.Billing;

/// <summary>Payload</summary>
public class SubscriptionDeletedPayload
{
	#region <Properties>

	public string StripeSubscriptionId { get; set; } = string.Empty;


	#endregion
}
